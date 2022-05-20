using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace CY_MVC.Utility
{
    internal static class SimpleTypeHelper
    {
        public static bool IsSimpleType(Type p_Type)
        {
            if (IsNumbericType(p_Type))
            {
                return true;
            }

            if (p_Type == typeof(char))
            {
                return true;
            }

            if (p_Type == typeof(string))
            {
                return true;
            }

            if (p_Type == typeof(bool))
            {
                return true;
            }

            if (p_Type == typeof(DateTime))
            {
                return true;
            }

            if (p_Type == typeof(Type))
            {
                return true;
            }

            if (p_Type.IsEnum)
            {
                return true;
            }

            if (IsNullableType(p_Type))
            {
                return true;
            }

            return false;
        }

        public static bool IsNumbericType(Type p_DestDataType)
        {
            return (p_DestDataType == typeof(int)) || (p_DestDataType == typeof(uint)) ||
                   (p_DestDataType == typeof(double))
                   || (p_DestDataType == typeof(short)) || (p_DestDataType == typeof(ushort)) ||
                   (p_DestDataType == typeof(decimal))
                   || (p_DestDataType == typeof(long)) || (p_DestDataType == typeof(ulong)) ||
                   (p_DestDataType == typeof(float))
                   || (p_DestDataType == typeof(byte)) || (p_DestDataType == typeof(sbyte));
        }

        public static bool IsNullableType(Type p_DestDataType)
        {
            return p_DestDataType.IsGenericType && p_DestDataType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>
        /// ChangeType 对System.Convert.ChangeType进行了增强，支持(0,1)到bool的转换，字符串->枚举、int->枚举、字符串->Type
        /// </summary>
        public static dynamic ConvertSimpleType(this object p_Inputvalue, Type p_DestinationType)
        {
            #region null

            if (p_Inputvalue == null)
            {
                return null;
            }

            #endregion null

            #region Same Type

            if (p_DestinationType == p_Inputvalue.GetType())
            {
                return p_Inputvalue;
            }

            #endregion Same Type

            #region bool 1,0

            if (p_DestinationType == typeof(bool))
            {
                if (p_Inputvalue.ToString() == "0")
                {
                    return false;
                }

                if (p_Inputvalue.ToString() == "1")
                {
                    return true;
                }
            }

            #endregion bool 1,0

            #region Enum

            if (p_DestinationType.IsEnum)
            {
                int intVal;
                var suc = int.TryParse(p_Inputvalue.ToString(), out intVal);
                return !suc ? Enum.Parse(p_DestinationType, p_Inputvalue.ToString()) : p_Inputvalue;
            }

            #endregion Enum

            #region Type

            if (p_DestinationType == typeof(Type))
            {
                return GetType(p_Inputvalue.ToString());
            }

            #endregion Type

            if (IsNullableType(p_DestinationType))
            {
                var ncConverter = new NullableConverter(p_DestinationType);
                return ncConverter.ConvertFrom(p_Inputvalue);
            }

            //将double赋值给数值型的DataRow的字段是可以的，但是通过反射赋值给object的非double的其它数值类型的属性，却不行
            return Convert.ChangeType(p_Inputvalue, p_DestinationType);
        }

        public static Assembly AssemblyLoadFile(string p_AssemblyFilePath)
        {
            Assembly resAssembly = null;
            if (File.Exists(p_AssemblyFilePath))
            {
                var data = File.ReadAllBytes(p_AssemblyFilePath);
                resAssembly = Assembly.Load(data);
            }
            return resAssembly;
        }

        public static Assembly AssemblyLoad(string p_AssemblyName)
        {
            //return Assembly.Load(p_AssemblyName);
            return Assembly.LoadFrom(StaticConfig.RootPath + "Bin/" + p_AssemblyName + ".dll");
            //return Assembly.LoadFile(StaticConfig.RootPath + "Bin/" + p_AssemblyName + ".dll");
            //return AssemblyLoadFile(StaticConfig.RootPath + "Bin/" + p_AssemblyName + ".dll");

            //return AppDomain.CurrentDomain.GetAssemblies().Where(p => p.GlobalAssemblyCache == false).SingleOrDefault(assembly => string.Equals(assembly.ManifestModule.Name.Replace(".dll", ""), p_AssemblyName, StringComparison.OrdinalIgnoreCase));
            //return AppDomain.CurrentDomain.GetAssemblies().Where(p => !p.GlobalAssemblyCache).FirstOrDefault(p => p.FullName.IndexOf(p_AssemblyName + ",", StringComparison.Ordinal) == 0);
        }

        #region 私有方法

        private static Type GetType(string p_TypeAndAssName)
        {
            var names = p_TypeAndAssName.Split(',');
            if (names.Length < 2)
            {
                return Type.GetType(p_TypeAndAssName);
            }

            return GetType(names[0].Trim(), names[1].Trim());
        }

        private static Type GetType(string p_TypeFullName, string p_AssemblyName)
        {
            if (p_AssemblyName == null)
            {
                return Type.GetType(p_TypeFullName);
            }

            //搜索当前域中已加载的程序集
            var asses = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var ass in asses)
            {
                var names = ass.FullName.Split(',');
                if (names[0].Trim() == p_AssemblyName.Trim())
                {
                    return ass.GetType(p_TypeFullName);
                }
            }

            //加载目标程序集
            var tarAssem = Assembly.Load(p_AssemblyName);
            if (tarAssem != null)
            {
                return tarAssem.GetType(p_TypeFullName);
            }

            return null;
        }

        #endregion 私有方法
    }
}