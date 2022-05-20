using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CY_MVC.Utility
{
    public static class XmlDataHelper
    {
        private static ConcurrentDictionary<Type, XmlSerializer> m_cache;

        static XmlDataHelper()
        {
            m_cache = new ConcurrentDictionary<Type, XmlSerializer>();
        }


        private static XmlSerializer GetSerializer<T>()
        {
            var type = typeof(T);
            return m_cache.GetOrAdd(type, XmlSerializer.FromTypes(new[] { type }).FirstOrDefault());
        }



        /// <summary>
        /// 读取被序列化保存的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="p_FilePath">数据文件路径</param>
        /// <returns></returns>
        public static T LoadFile<T>(string p_FilePath) where T : new()
        {
            var obj = default(T);
            if (File.Exists(p_FilePath))
            {
                var XmlContent = File.ReadAllText(p_FilePath);
                obj = Load<T>(XmlContent);
            }
            return obj;
        }

        /// <summary>
        /// 序列化保存XML数据方法
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="p_Obj">数据对象</param>
        /// <param name="p_FilePath">数据文件</param>
        public static void SaveFile<T>(T p_Obj, string p_FilePath) where T : new()
        {
            var XmlContent = Save(p_Obj);
            if (!string.IsNullOrWhiteSpace(XmlContent))
            {
                File.WriteAllText(p_FilePath, XmlContent);
            }
        }


        /// <summary>
        /// 读取被序列化保存的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="p_XmlContent">数据文件内容</param>
        /// <returns></returns>
        public static T Load<T>(string p_XmlContent) where T : new()
        {
            T obj = default(T);
            if (!string.IsNullOrEmpty(p_XmlContent))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(p_XmlContent)))
                {
                    obj = (T)GetSerializer<T>().Deserialize(ms);
                }

            }
            return obj;
        }

        /// <summary>
        /// 序列化保存XML数据方法
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="p_Obj">数据对象</param>
        public static string Save<T>(T p_Obj) where T : new()
        {
            var XmlContent = string.Empty;
            if (null != p_Obj)
            {
                using (var ms = new MemoryStream())
                {
                    GetSerializer<T>().Serialize(ms, p_Obj);
                    XmlContent = Encoding.UTF8.GetString(ms.ToArray());
                }
                XmlContent = XmlContent.Replace("<?xml version=\"1.0\"?>" + Environment.NewLine, string.Empty);
                XmlContent = XmlContent.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", string.Empty);
                XmlContent = XmlContent.Replace(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", string.Empty);
            }
            return XmlContent;
        }
    }
}