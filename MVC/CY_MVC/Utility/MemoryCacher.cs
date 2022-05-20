using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using CacheItemPriority = System.Runtime.Caching.CacheItemPriority;
namespace CY_MVC.Utility
{
    public static class MemoryCacher
    {
        private static MemoryCache m_MemoryCache = MemoryCache.Default;

        /// <summary>
        /// 缓存键列表
        /// </summary>
        public static List<string> Keys
        {
            get
            {
                return m_MemoryCache.Select(p => p.Key).ToList();
            }
        }

        /// <summary>
        /// 缓存总项数
        /// </summary>
        public static long Count
        {
            get { return m_MemoryCache.GetCount(); }
        }

        /// <summary>
        /// 获取计算机上缓存可使用的内存量（以字节为单位）。
        /// </summary>
        public static long CacheMemoryLimit
        {
            get { return m_MemoryCache.CacheMemoryLimit; }
        }

        /// <summary>
        /// 获取缓存可使用的物理内存的百分比。
        /// </summary>
        public static long PhysicalMemoryLimit
        {
            get { return m_MemoryCache.PhysicalMemoryLimit; }
        }

        /// <summary>
        /// 获取在缓存更新其内存统计信息之前需等待的最大时间量。
        /// </summary>
        public static TimeSpan PollingInterval
        {
            get { return m_MemoryCache.PollingInterval; }
        }

        /// <summary>
        /// 从缓存中移除指定百分比的缓存项
        /// </summary>
        /// <param name="p_Percent">百分比 不得大于100</param>
        /// <returns>返回被移除的项数</returns>
        public static long Trim(double p_Percent)
        {
            return m_MemoryCache.Trim((int)(p_Percent * 100));
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <typeparam name="T">缓存内容的类型</typeparam>
        /// <param name="p_Key">缓存ID</param>
        /// <returns>缓存内容</returns>
        public static T Get<T>(string p_Key)
        {
            return m_MemoryCache.Contains(p_Key) ? (T)m_MemoryCache[p_Key] : default(T);
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <param name="p_Key">缓存ID</param>
        /// <returns>缓存内容</returns>
        public static object Get(string p_Key)
        {
            return m_MemoryCache.Contains(p_Key) ? m_MemoryCache[p_Key] : null;
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <param name="p_Keys">缓存ID</param>
        /// <returns>缓存内容</returns>
        public static IDictionary<string, object> Get(params string[] p_Keys)
        {
            return m_MemoryCache.GetValues(p_Keys);
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <param name="p_Keys">缓存ID</param>
        /// <returns>缓存内容</returns>
        public static IDictionary<string, object> Get(List<string> p_Keys)
        {
            return m_MemoryCache.GetValues(p_Keys);
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <typeparam name="T">缓存内容的类型</typeparam>
        /// <param name="p_Key">缓存ID</param>
        /// <param name="p_Value">缓存内容</param>
        /// <returns>是否存在</returns>
        public static bool TryGet<T>(string p_Key, out T p_Value)
        {
            if (m_MemoryCache.Contains(p_Key))
            {
                p_Value = Get<T>(p_Key);
                return true;
            }
            else
            {
                p_Value = default(T);
                return false;
            }
        }

        /// <summary>
        /// 获取对应缓存ID的缓存内容
        /// </summary>
        /// <param name="p_Key">缓存ID</param>
        /// <param name="p_Value">缓存内容</param>
        /// <returns>是否存在</returns>
        public static bool TryGet(string p_Key, out object p_Value)
        {
            if (m_MemoryCache.Contains(p_Key))
            {
                p_Value = Get(p_Key);
                return true;
            }
            else
            {
                p_Value = null;
                return false;
            }
        }

        /// <summary>
        /// 对应缓存ID的缓存是否存在
        /// </summary>
        /// <param name="p_Key"></param>
        /// <returns></returns>
        public static bool Contains(string p_Key)
        {
            return m_MemoryCache.Contains(p_Key);
        }

        /// <summary>
        /// 获取某个标识开头的缓存
        /// </summary>
        /// <param name="p_KeyStartsWith"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetByKeyStartWith(string p_KeyStartsWith)
        {
            return GetKeysByKeyStartWith(p_KeyStartsWith).ToDictionary(p => p, p => m_MemoryCache[p]);
        }

        private static List<string> GetKeysByKeyStartWith(string p_KeyStartsWith)
        {
            return Keys.Where(p => p.StartsWith(p_KeyStartsWith, StringComparison.Ordinal)).ToList();
        }

        /// <summary>
        /// 获取包含某个标识的缓存
        /// </summary>
        /// <param name="p_KeyContains"></param>
        /// <returns></returns>
        public static IDictionary<string, object> GetByKeyContains(string p_KeyContains)
        {
            return GetKeysByKeyContains(p_KeyContains).ToDictionary(p => p, p => m_MemoryCache[p]);
        }
        private static List<string> GetKeysByKeyContains(string p_KeyContains)
        {
            return Keys.Where(p => p.IndexOf(p_KeyContains, StringComparison.Ordinal) > -1).ToList();
        }

        /// <summary>
        /// 获取所有缓存
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, object> GetAll()
        {
            return m_MemoryCache.ToDictionary(p => p.Key, p => p.Value);
        }


        /// <summary>
        /// 删除key缓存
        /// </summary>
        /// <param name="p_Key">缓存ID</param>
        public static dynamic Remove(string p_Key)
        {
            return m_MemoryCache.Remove(p_Key);
        }

        /// <summary>
        /// 清除某个标识开头的缓存
        /// </summary>
        /// <param name="p_KeyStartsWith">缓存ID开头标识</param>
        public static void RemoveByKeyStartWith(string p_KeyStartsWith)
        {
            foreach (var cachekey in GetKeysByKeyStartWith(p_KeyStartsWith))
            {
                m_MemoryCache.Remove(cachekey);
            }
        }

        /// <summary>
        /// 清除包含某个标识的缓存
        /// </summary>
        /// <param name="p_KeyContains">缓存ID标识</param>
        public static void RemoveByKeyContains(string p_KeyContains)
        {
            foreach (var cachekey in GetKeysByKeyContains(p_KeyContains))
            {
                m_MemoryCache.Remove(cachekey);
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public static void Clear()
        {
            m_MemoryCache.Dispose();
            m_MemoryCache = MemoryCache.Default;
        }

        public static ChangeMonitor DependencyOnKeys(params string[] keys)
        {
            if (null != keys && keys.Any())
            {
                var exiskeys = keys.Where(p => Keys.Contains(p));
                if (exiskeys.Any())
                    return m_MemoryCache.CreateCacheEntryChangeMonitor(exiskeys);
            }
            return null;
        }

        public static ChangeMonitor DependencyOnFiles(params string[] files)
        {
            if (null != files && files.Any())
            {
                var existfiles = new List<string>();
                foreach (var item in files)
                {
                    if (File.Exists(item))
                    {
                        existfiles.Add(item);
                    }
                    if (Directory.Exists(item))
                    {
                        existfiles.Add(item);
                    }
                }
                if (existfiles.Any())
                    return new HostFileChangeMonitor(existfiles);
            }
            return null;
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="p_Key">缓存ID</param>
        /// <param name="p_Value">缓存内容</param>
        /// <param name="p_CacheItemPriority">缓存的相对优先级</param>
        /// <param name="p_AbsoluteExpiration">绝对到期时间(不可与相对到期时间同时设置,不设置的话设置为null)</param>
        /// <param name="p_SlidingExpiration">相对到期时间(不可与绝对到期时间同时设置,不设置的话设置为null)</param>
        /// <param name="p_ChangeMonitors">缓存依赖项</param>
        /// <param name="p_OnRemovedCallback">缓存移除事件</param>
        public static void Set(string p_Key, object p_Value, CacheItemPriority p_CacheItemPriority, DateTimeOffset? p_AbsoluteExpiration = null, TimeSpan? p_SlidingExpiration = null, IEnumerable<ChangeMonitor> p_ChangeMonitors = null, Action<CacheEntryRemovedArguments> p_OnRemovedCallback = null)
        {
            if (!string.IsNullOrEmpty(p_Key))
            {
                if (null != p_Value)
                {
                    var cip = new CacheItemPolicy();
                    cip.Priority = p_CacheItemPriority;

                    if (p_AbsoluteExpiration != null && p_SlidingExpiration == null)
                    {
                        cip.AbsoluteExpiration = (DateTimeOffset)p_AbsoluteExpiration;
                    }
                    else if (p_SlidingExpiration != null && p_AbsoluteExpiration == null)
                    {
                        cip.SlidingExpiration = (TimeSpan)p_SlidingExpiration;
                    }
                    else if (p_SlidingExpiration == null && p_SlidingExpiration == null)
                    {
                        cip.SlidingExpiration = TimeSpan.FromMinutes(30);
                    }
                    else if (p_SlidingExpiration != null && p_SlidingExpiration != null)
                    {
                        cip.AbsoluteExpiration = (DateTimeOffset)p_AbsoluteExpiration;
                        cip.SlidingExpiration = (TimeSpan)p_SlidingExpiration;
                    }
                    if (p_OnRemovedCallback != null)
                        cip.RemovedCallback = arguments => p_OnRemovedCallback(arguments);
                    //if (p_OnUpdateCallback != null)
                    //    cip.UpdateCallback = arguments => p_OnUpdateCallback(arguments);
                    if (p_ChangeMonitors != null && p_ChangeMonitors.Any())
                        foreach (var pChangeMonitor in p_ChangeMonitors)
                        {
                            cip.ChangeMonitors.Add(pChangeMonitor);
                        }
                    m_MemoryCache.Set(p_Key, p_Value, cip);
                }
            }
            else
            {
                throw new ArgumentNullException("p_Key");
            }
        }
    }
}