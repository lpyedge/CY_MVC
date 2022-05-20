using Fasterflect;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace CY_MVC.Utility
{
    public static class ModelProperty
    {
        private static readonly string ModelPropertyKey = typeof(ModelProperty).FullName;

        public static readonly Regex RegexPropertyName = new Regex("\\[(?<PropertyName>\\w+)\\]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        public static string RegexReplace(string p_Str, MatchCollection p_Mc, params dynamic[] p_Objs)
        {
            if (p_Str != null && p_Mc != null)
            {
                foreach (Match match in p_Mc)
                {
                    if (match.Success && match.Groups["PropertyName"].Success)
                    {
                        if (p_Objs != null && p_Objs.Length > 0)
                        {
                            foreach (var obj in p_Objs)
                            {
                                if (obj != null)
                                {
                                    Type objType = obj.GetType();
                                    MemberGetter propertyGetter;
                                    if (
                                        !MemoryCacher.TryGet(ModelPropertyKey + objType.FullName + match.Groups["PropertyName"],
                                            out propertyGetter))
                                    {
                                        if (objType.Properties()
                                            .Any(
                                                p =>
                                                    string.Equals(p.Name, match.Groups["PropertyName"].ToString(),
                                                        StringComparison.OrdinalIgnoreCase)))
                                        {
                                            propertyGetter =
                                                objType.DelegateForGetPropertyValue(
                                                    match.Groups["PropertyName"].ToString());

                                            MemoryCacher.Set(
                                                ModelPropertyKey + objType.FullName + match.Groups["PropertyName"],
                                                propertyGetter, CacheItemPriority.Default, null, TimeSpan.FromHours(1));
                                        }
                                    }
                                    if (propertyGetter != null)
                                    {
                                        var val = propertyGetter.Invoke(obj);
                                        p_Str = p_Str.Replace("[" + match.Groups["PropertyName"] + "]",
                                            val != null ? val.ToString() : string.Empty);
                                    }
                                }
                                ////加载程序集有时候会出现重复加载结果反射的时候有冲突的情况，就只能用移除缓存的办法每次都用反射来获取属性
                                //dynamic o = obj != null ? Fasterflect.PropertyExtensions.TryGetPropertyValue(obj, match.Groups["key"].ToString()) : string.Empty;
                                //if (o != null)
                                //    p_Str = p_Str.Replace("[" + match.Groups["key"].ToString() + "]", o.ToString());
                            }
                        }
                        p_Str = p_Str.Replace("[" + match.Groups["PropertyName"] + "]", string.Empty);
                    }
                }
            }
            return p_Str;
        }
    }
}