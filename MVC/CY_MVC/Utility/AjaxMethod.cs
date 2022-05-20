using Fasterflect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;

namespace CY_MVC.Utility
{
    public static class AjaxMethod
    {
        public class MethodModel
        {
            public MethodModel(MethodInfo mi)
            {
                MethodInvoker = mi.DelegateForCallMethod(); //缓存方法增加执行效率
                ParameterInfoList = mi.Parameters(); //此方法不消耗资源
                if (!mi.DeclaringType.IsAbstract)
                    DeclaringObject = Activator.CreateInstance(mi.DeclaringType); //缓存对象增加执行效率(实例化对象最为消耗资源)
            }

            public MethodInvoker MethodInvoker { get; set; }
            public IList<ParameterInfo> ParameterInfoList { get; set; }
            public object DeclaringObject { get; set; }
        }

        private const string AjaxMethodKey = "CY_MVC.Utility.AjaxMethod._AjaxMethodKey_";

        static AjaxMethod()
        {
            //json序列化全局设置
            Newtonsoft.Json.JsonSerializerSettings setting = new Newtonsoft.Json.JsonSerializerSettings();
            JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
            {
                //日期类型默认格式化处理
                setting.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
                setting.DateFormatString = "yyyy-MM-dd HH:mm:ss";

                //空值处理,如果某个值为空或者null,可以忽略并设置为默认值
                setting.NullValueHandling = NullValueHandling.Ignore;

                //高级用法九中的Bool类型转换 设置
                //setting.Converters.Add(new BoolConvert());

                return setting;
            });

            ParamPrefix = "p_";
            MethodKey = "action";
        }

        public static string ParamPrefix { get; set; }

        public static string MethodKey { get; set; }

        public static string Method
        {
            get
            {
                var Key = HttpContext.Current.Request.Params.AllKeys.Where(
                        p => string.Equals(p, MethodKey, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (Key != null)
                {
                    return HttpContext.Current.Request.Params[Key];
                }
                return null;
            }
        }

        public static Dictionary<string, string> Params
        {
            get
            {
                return HttpContext.Current.Request.HttpMethod == "POST"
                    ? HttpContext.Current.Request.Form.AllKeys.Where(
                        p => !string.Equals(p, MethodKey, StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(Key => Key.ToLowerInvariant(), Key => HttpContext.Current.Request.Form[Key])
                    : HttpContext.Current.Request.QueryString.AllKeys.Where(
                        p => !string.Equals(p, MethodKey, StringComparison.OrdinalIgnoreCase))
                        .ToDictionary(Key => Key.ToLowerInvariant(), Key => HttpContext.Current.Request.QueryString[Key]);
            }
        }

        public static byte[] InvokeJsonP<T>()
        {
            return InvokeJsonP(typeof(T));
        }

        public static byte[] InvokeJsonP(Type p_ObjectType)
        {
            return InvokeJsonP(p_ObjectType, Method, Params);
        }

        public static byte[] InvokeJsonP<T>(string p_Methodname, IDictionary<string, string> p_PDictionary)
        {
            return InvokeJsonP(typeof(T), p_Methodname, p_PDictionary);
        }

        public static byte[] InvokeJsonP(Type p_ObjectType, string p_Methodname,
            IDictionary<string, string> p_PDictionary)
        {
            HttpContext.Current.Response.ContentType = "text/plain";
            if (Params.ContainsKey("callback"))
            {
                var callback = p_PDictionary["callback"];
                p_PDictionary.Remove("callback");
                p_PDictionary.Remove("_");
                var jsoncontent = Encoding.UTF8.GetString(Invoke(p_ObjectType, p_Methodname, p_PDictionary));
                return Encoding.UTF8.GetBytes(callback + "(" + jsoncontent + ")");
            }
            return Encoding.UTF8.GetBytes("JsonP请求没有callback方法");
        }

        public static byte[] Invoke<T>()
        {
            return Invoke(typeof(T));
        }

        public static byte[] Invoke(Type p_ObjectType)
        {
            return Invoke(p_ObjectType, Method, Params);
        }

        public static byte[] Invoke<T>(string p_Methodname, IDictionary<string, string> p_PDictionary)
        {
            return Invoke(typeof(T), p_Methodname, p_PDictionary);
        }

        public static byte[] Invoke(Type p_ObjectType, string p_Methodname,
            IDictionary<string, string> p_PDictionary)
        {
            if (!string.IsNullOrWhiteSpace(p_Methodname))
            {
                var Key = TypeMethodName(p_ObjectType, p_Methodname);
                MethodModel mm;
                if (!MemoryCacher.TryGet(AjaxMethodKey + Key + "M", out mm))
                {
                    GetMethod(p_ObjectType, p_Methodname, out mm);
                    MemoryCacher.Set(AjaxMethodKey + Key + "M", mm, CacheItemPriority.Default, null,
                        TimeSpan.FromHours(1));
                }

                if (mm.ParameterInfoList.Count == p_PDictionary.Count)
                {
                    var parameters = new List<object>();
                    foreach (var parameter in mm.ParameterInfoList)
                    {
                        if (p_PDictionary.ContainsKey(parameter.Name.ToLowerInvariant().NameNoPrefix()))
                        {
                            if (SimpleTypeHelper.IsSimpleType(parameter.ParameterType))
                            {
                                try
                                {
                                    parameters.Add(
                                        p_PDictionary[parameter.Name.ToLowerInvariant().NameNoPrefix()]
                                            .ConvertSimpleType(parameter.ParameterType));
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    throw ex;
#else
                                    throw new Exception("简单类型:" + parameter.ParameterType.Name + "转换失败!转换对象:" +
                                                        p_PDictionary[parameter.Name.ToLowerInvariant().NameNoPrefix()]);
#endif
                                }
                            }
                            else
                            {
                                try
                                {
                                    parameters.Add(
                                        JsonConvert.DeserializeObject(
                                            p_PDictionary[parameter.Name.ToLowerInvariant().NameNoPrefix()],
                                            parameter.ParameterType));
                                }
                                catch (Exception ex)
                                {
#if DEBUG
                                    throw ex;
#else
                                    throw new Exception("复杂类型:" + parameter.ParameterType.Name + "转换失败!转换对象:" +
                                                        p_PDictionary[parameter.Name.ToLowerInvariant().NameNoPrefix()]);
#endif
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("请求方法" + Key + "时参数" + parameter.Name.ToLowerInvariant().NameNoPrefix() +
                                                "名称错误!");
                        }
                    }
                    string res;
                    try
                    {
                        Stopwatch sw = null;

                        if (StaticConfig.Debug)
                        {
                            sw = new Stopwatch();
                            sw.Start();
                        }

                        var data = mm.MethodInvoker.Invoke(mm.DeclaringObject, parameters.ToArray());

                        if (StaticConfig.Debug)
                        {
                            sw.Stop();
                            HttpContext.Current.Response.Headers["MethodInvoke"] = sw.ElapsedMilliseconds.ToString();
                            sw.Restart();
                        }

                        res = JsonConvert.SerializeObject(data);

                        if (StaticConfig.Debug)
                        {
                            sw.Stop();
                            HttpContext.Current.Response.Headers["JsonConvert"] = sw.ElapsedMilliseconds.ToString();
                            //sw.Restart();
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        throw ex;
#else
                        throw new Exception("请求方法" + Key + "执行失败!" + ex.Message);
#endif
                    }
                    return Encoding.UTF8.GetBytes(res);
                }
                return Encoding.UTF8.GetBytes("请求方法" + Key + "的参数个数不对应");
            }
            return Encoding.UTF8.GetBytes("请求方法为空");
        }

        public static void GetMethod(Type p_ObjectType, string p_Methodname, out MethodModel mm)
        {
            List<MethodInfo> methodlist;

            methodlist = new List<MethodInfo>();
            methodlist.AddRange(
                p_ObjectType.GetMethods(
                    BindingFlags.NonPublic | BindingFlags.Public |
                    BindingFlags.Instance | BindingFlags.Static)
                                        );

            //获取属性类里面的方法
            foreach (var mc in GetMemberClass(p_ObjectType))
            {
                methodlist.AddRange(
                    mc.GetMethods(
                        BindingFlags.NonPublic | BindingFlags.Public |
                        BindingFlags.Instance | BindingFlags.Static)
                        );
            }

            var methodinfo = methodlist
                .FirstOrDefault(p => p.DeclaringType == p_ObjectType
                    ? string.Equals(p.Name, p_Methodname, StringComparison.OrdinalIgnoreCase)
                    : string.Equals(p.DeclaringType.Name + "." + p.Name, p_Methodname, StringComparison.OrdinalIgnoreCase)
                );

            if (methodinfo != null)
            {
                mm = new MethodModel(methodinfo);
            }
            else
            {
                throw new Exception("对象" + p_ObjectType.Name + "没有" + p_Methodname + "这个方法！");
            }
        }

        public static List<Type> GetMemberClass(Type p_ObjectType)
        {
            return
                p_ObjectType.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(member => member.MemberType == MemberTypes.NestedType)
                    .Cast<Type>()
                    .ToList();
        }

        public static string NameNoPrefix(this string p_ParameterName)
        {
            return p_ParameterName.StartsWith(ParamPrefix, StringComparison.OrdinalIgnoreCase)
                ? p_ParameterName.Remove(0, ParamPrefix.Length)
                : p_ParameterName;
        }

        public static string TypeMethodName(Type p_Type, string p_MethodName)
        {
            return p_Type.FullName.ToLowerInvariant() + "_" + p_MethodName.ToLowerInvariant();
        }
    }
}