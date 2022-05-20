using CY_MVC.HttpHandlers;
using CY_MVC.Utility;
using Fasterflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Web;

namespace CY_MVC.UrlRewrite
{
    /// <summary>
    /// UrlRewrite工具类
    /// </summary>
    public static class UrlRewriteHelper
    {
        private static readonly string UrlRewritKey = typeof(UrlRewriteHelper).FullName;

        private static Dictionary<string, UrlRewritePreRule> m_UrlRewritePreRules;

        private static List<UrlRewriteRule> m_UrlRewriteRules;

        private static readonly object locker = new object();

        public static void ClearCache()
        {
            m_UrlRewriteRules = null;
            m_UrlRewritePreRules = null;
            MemoryCacher.RemoveByKeyStartWith(UrlRewritKey);
        }

        public static List<UrlRewriteRule> UrlRewriteRules
        {
            get
            {
                if (m_UrlRewriteRules == null)
                {
                    lock (locker)
                    {
                        if (m_UrlRewriteRules == null)
                        {
                            m_UrlRewriteRules = XmlDataHelper.LoadFile<List<UrlRewriteRule>>(StaticConfig.UrlRewriteRulesPath) ??
                                      new List<UrlRewriteRule>();
                        }
                    }
                }
                return m_UrlRewriteRules;
            }
            set
            {
                if (null != value)
                {
                    m_UrlRewriteRules = value;
                    MemoryCacher.RemoveByKeyStartWith(UrlRewritKey);
                }
            }
        }

        public static Dictionary<string, UrlRewritePreRule> UrlRewritePreRules
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(StaticConfig.UrlRewritePreRuleLibraryName))
                {
                    if (m_UrlRewritePreRules == null)
                    {
                        lock (locker)
                        {
                            if (m_UrlRewritePreRules == null)
                            {
                                var AssemblyUrlRewritePreRuleLibrary =
                                    SimpleTypeHelper.AssemblyLoad(StaticConfig.UrlRewritePreRuleLibraryName);

                                if (AssemblyUrlRewritePreRuleLibrary != null)
                                {
                                    m_UrlRewritePreRules =
                                        AssemblyUrlRewritePreRuleLibrary.TypesImplementing<UrlRewritePreRule>()
                                            .Select(type => (UrlRewritePreRule)Activator.CreateInstance(type))
                                            .ToDictionary(p => p.Name.ToLowerInvariant(), p => p);
                                }
                                else
                                {
                                    throw new Exception("未能加载URL预生成规则类库！");
                                }
                            }
                        }
                    }
                    return m_UrlRewritePreRules;
                }
                return new Dictionary<string, UrlRewritePreRule>();
            }
        }

        public static void Save()
        {
            Save(UrlRewriteRules);
        }

        public static void Save(List<UrlRewriteRule> p_urlRewriteRule)
        {
            if (p_urlRewriteRule == null)
            {
                throw new Exception("网址重写列表为空!");
            }
            UrlRewriteRules = p_urlRewriteRule;
            XmlDataHelper.SaveFile(p_urlRewriteRule, StaticConfig.UrlRewriteRulesPath);
        }

        public static void GenerateUrlRewriteRules()
        {
            if (UrlRewritePreRules.Count > 0)
            {
                var urlRewriteRules = new List<UrlRewriteRule>(UrlRewritePreRules.Count);
                urlRewriteRules.AddRange(
                    UrlRewritePreRules.Select(
                        urlRewritePreRule =>
                            new UrlRewriteRule
                            {
                                RealUrl = urlRewritePreRule.Value.RealUrl,
                                ShowUrl = urlRewritePreRule.Value.RewriteShowUrl,
                                ReDirect = urlRewritePreRule.Value.ReDirect,
                                Stop = urlRewritePreRule.Value.Stop
                            }));

                Save(urlRewriteRules);
            }
        }

        public static string Url<T>(params dynamic[] p_Objs) where T : UrlRewritePreRule
        {
            return Url(typeof(T).Name, p_Objs);
        }

        public static string Url(string name, params dynamic[] p_Objs)
        {
            if (UrlRewritePreRules.ContainsKey(name.ToLowerInvariant()))
            {
                var urpr = UrlRewritePreRules[name.ToLowerInvariant()];
                return ModelProperty.RegexReplace(urpr.ShowUrl, urpr.UrlMatchs(), p_Objs);
            }
            return string.Empty;
        }
        public static string GetRealUrl(Uri ShowUrl, out bool ReDirect)
        {
            ReDirect = false;
            var RealUrl = string.Empty;

            if (!MemoryCacher.TryGet(UrlRewritKey + ShowUrl.AbsolutePath.ToLowerInvariant(), out RealUrl))
            {
                RealUrl = string.Empty;
                //url的后缀名不在过滤范围内则判断是否符合url重写规则,否则直接跳过
                if (
                    !StaticConfig.UrlRewriteRulesSuffixFilters.Any(
                        filter => ShowUrl.AbsolutePath.EndsWith("." + filter, StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var Item in UrlRewriteRules)
                    {
                        Regex regexShowUrl;
                        Match match;
                        //如果要重写的网址不是以http开头的，说明是当前域名下的重写地址
                        if (Item.ShowUrl.IndexOf("http", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            //当前域名下的重写地址
                            regexShowUrl = Item.RegexShowUrl(HttpContext.Current.Request.ApplicationPath);

                            //Context.Request.Path 获取不到?后面的内容 修正为 Context.Request.RawUrl
                            //后改为Request.Url.PathAndQuery(统一调用uri的属性)
                            var path = ShowUrl.AbsolutePath.Length == 1 ? "/" : ShowUrl.AbsolutePath.TrimEnd('/');
                            match = regexShowUrl.Match(path);
                            if (!match.Success)
                            {
                                //如果Context.Request.RawUrl匹配不成功有可能是自定义额外参数需要忽略,自动判断并实现重写
                                //后改为Request.Url.LocalPath(统一调用uri的属性)
                                match = regexShowUrl.Match(ShowUrl.AbsolutePath.TrimEnd('/'));
                                if (!match.Success)
                                    continue;
                            }
                        }
                        else
                        {
                            //其他域名下的重写地址
                            regexShowUrl = Item.RegexShowUrl();

                            //Context.Request.Path 获取不到?后面的内容 修正为 Context.Request.RawUrl
                            //后改为Request.Url.PathAndQuery(统一调用uri的属性)
                            var path = ShowUrl.AbsolutePath.Length == 1 ? "/" : ShowUrl.AbsolutePath.TrimEnd('/');
                            match = regexShowUrl.Match(ShowUrl.Scheme + "://" + ShowUrl.Authority + path);
                            if (!match.Success)
                            {
                                //如果Context.Request.RawUrl匹配不成功有可能是自定义额外参数需要忽略,自动判断并实现重写
                                //后改为Request.Url.LocalPath(统一调用uri的属性)
                                match = regexShowUrl.Match(ShowUrl.Scheme + "://" + ShowUrl.Authority + ShowUrl.AbsolutePath.TrimEnd('/'));
                                if (!match.Success)
                                    continue;
                            }
                        }

                        RealUrl = ResolveUrl(HttpContext.Current.Request.ApplicationPath, Item.RealUrl);

                        for (var i = 0; i < match.Groups.Count; i++)
                        {
                            if (match.Groups[i].Success)
                            {
                                RealUrl = RealUrl.Replace("$" + i, HttpUtility.UrlEncode(match.Groups[i].Value));
                            }
                        }
                        foreach (var groupname in regexShowUrl.GetGroupNames())
                        {
                            if (match.Groups[groupname].Success)
                            {
                                RealUrl = RealUrl.Replace("$[" + groupname + "]",
                                    HttpUtility.UrlEncode(match.Groups[groupname].Value));
                            }
                        }

                        RealUrl = RealUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                            ? RealUrl
                            : ShowUrl.Scheme + "://" + ShowUrl.Authority + RealUrl;

                        ReDirect = Item.ReDirect;

                        if (!string.IsNullOrWhiteSpace(RealUrl) && (Item.Stop || Item.ReDirect))
                        {
                            break;
                        }
                    }
                }

                //如果是302跳转的话不做缓存,否则就没法返回正确的ReDirect值了
                if (!ReDirect)
                    MemoryCacher.Set(UrlRewritKey + ShowUrl.AbsoluteUri.ToLowerInvariant(), RealUrl, CacheItemPriority.Default, null,
                        TimeSpan.FromHours(1),
                        new List<ChangeMonitor> { MemoryCacher.DependencyOnFiles(StaticConfig.UrlRewriteRulesPath) });
            }

            //重写地址RealUrl存在时,补上正常传递过来的Url参数
            if (!string.IsNullOrWhiteSpace(RealUrl) && ShowUrl.Query.Length > 0)
            {
                if (RealUrl.IndexOf('?') != -1)
                {
                    RealUrl += "&" + ShowUrl.Query.TrimStart('?');
                }
                else
                {
                    RealUrl += ShowUrl.Query;
                }
            }

            return RealUrl;
        }

        //public static string GetRealUrl(Uri ShowUrl, out bool ReDirect)
        //{
        //    ReDirect = false;
        //    var RealUrl = string.Empty;

        //    if (!MemoryCacher.TryGet(UrlRewritKey + ShowUrl.AbsoluteUri.ToLowerInvariant(), out RealUrl))
        //    {
        //        RealUrl = string.Empty;
        //        //url的后缀名不在过滤范围内则判断是否符合url重写规则,否则直接跳过
        //        if (
        //            !StaticConfig.UrlRewriteRulesSuffixFilters.Any(
        //                filter => ShowUrl.LocalPath.EndsWith("." + filter, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            foreach (var Item in UrlRewriteRules)
        //            {
        //                Regex regexShowUrl;
        //                Match match;
        //                //如果要重写的网址不是以http开头的，说明是当前域名下的重写地址
        //                if (Item.ShowUrl.IndexOf("http", StringComparison.OrdinalIgnoreCase) != 0)
        //                {
        //                    //当前域名下的重写地址
        //                    regexShowUrl = Item.RegexShowUrl(HttpContext.Current.Request.ApplicationPath);

        //                    //Context.Request.Path 获取不到?后面的内容 修正为 Context.Request.RawUrl
        //                    //后改为Request.Url.PathAndQuery(统一调用uri的属性)
        //                    var path = ShowUrl.PathAndQuery.Length == 1 ? "/" : ShowUrl.PathAndQuery.TrimEnd('/');
        //                    match = regexShowUrl.Match(path);
        //                    if (!match.Success)
        //                    {
        //                        //如果Context.Request.RawUrl匹配不成功有可能是自定义额外参数需要忽略,自动判断并实现重写
        //                        //后改为Request.Url.LocalPath(统一调用uri的属性)
        //                        match = regexShowUrl.Match(ShowUrl.LocalPath.TrimEnd('/'));
        //                        if (!match.Success)
        //                            continue;
        //                    }
        //                }
        //                else
        //                {
        //                    //其他域名下的重写地址
        //                    regexShowUrl = Item.RegexShowUrl();

        //                    //Context.Request.Path 获取不到?后面的内容 修正为 Context.Request.RawUrl
        //                    //后改为Request.Url.PathAndQuery(统一调用uri的属性)
        //                    var path = ShowUrl.PathAndQuery.Length == 1 ? "/" : ShowUrl.PathAndQuery.TrimEnd('/');
        //                    match = regexShowUrl.Match(ShowUrl.Scheme + "://" + ShowUrl.Authority + path);
        //                    if (!match.Success)
        //                    {
        //                        //如果Context.Request.RawUrl匹配不成功有可能是自定义额外参数需要忽略,自动判断并实现重写
        //                        //后改为Request.Url.LocalPath(统一调用uri的属性)
        //                        match = regexShowUrl.Match(ShowUrl.Scheme + "://" + ShowUrl.Authority + ShowUrl.LocalPath.TrimEnd('/'));
        //                        if (!match.Success)
        //                            continue;
        //                    }
        //                }

        //                RealUrl = ResolveUrl(HttpContext.Current.Request.ApplicationPath, Item.RealUrl);

        //                for (var i = 0; i < match.Groups.Count; i++)
        //                {
        //                    if (match.Groups[i].Success)
        //                    {
        //                        RealUrl = RealUrl.Replace("$" + i, HttpUtility.UrlEncode(match.Groups[i].Value));
        //                    }
        //                }
        //                foreach (var groupname in regexShowUrl.GetGroupNames())
        //                {
        //                    if (match.Groups[groupname].Success)
        //                    {
        //                        RealUrl = RealUrl.Replace("$[" + groupname + "]",
        //                            HttpUtility.UrlEncode(match.Groups[groupname].Value));
        //                    }
        //                }
        //                if (ShowUrl.Query.Length > 0)
        //                {
        //                    if (RealUrl.IndexOf('?') != -1)
        //                    {
        //                        RealUrl += "&" + ShowUrl.Query.TrimStart('?');
        //                    }
        //                    else
        //                    {
        //                        RealUrl += ShowUrl.Query;
        //                    }
        //                }

        //                RealUrl = RealUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
        //                    ? RealUrl
        //                    : ShowUrl.Scheme + "://" + ShowUrl.Authority + RealUrl;

        //                ReDirect = Item.ReDirect;

        //                if (!string.IsNullOrWhiteSpace(RealUrl) && (Item.Stop || Item.ReDirect))
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        //如果是302跳转的话不做缓存,否则就没法返回正确的ReDirect值了
        //        if (!ReDirect)
        //            MemoryCacher.Set(UrlRewritKey + ShowUrl.AbsoluteUri.ToLowerInvariant(), RealUrl, CacheItemPriority.Default, null,
        //                TimeSpan.FromHours(1),
        //                new List<ChangeMonitor> { MemoryCacher.DependencyOnFiles(StaticConfig.UrlRewriteRulesPath) });
        //    }

        //    return RealUrl;
        //}

        /// <summary>
        ///     RemapUrl's a URL using <b>HttpContext.RemapUrl()</b>.
        /// </summary>
        /// <param name="p_Context">The HttpContext object to remap the URL to.</param>
        /// <param name="p_RemapUrl">The URL to remap to.</param>
        internal static void RewriteUrl(HttpContext p_Context, string p_RemapUrl)
        {
            var uri = new Uri(p_RemapUrl);
            p_Context.RewritePath(uri.AbsolutePath, string.Empty,
                string.IsNullOrEmpty(uri.Query) ? string.Empty : uri.Query.Substring(1, uri.Query.Length - 1));
            HandlerHelper.RemapHandler(p_Context);
        }

        /// <summary>
        ///     Converts a URL into one that is usable on the requesting client.
        /// </summary>
        /// <remarks>
        ///     Converts ~ to the requesting application path.  Mimics the behavior of the
        ///     <b>Control.ResolveUrl()</b> method, which is often used by control developers.
        /// </remarks>
        /// <param name="p_AppPath">The application path.</param>
        /// <param name="p_Url">The URL, which might contain ~.</param>
        /// <returns>
        ///     A resolved URL.  If the input parameter <b>url</b> contains ~, it is replaced with the
        ///     value of the <b>appPath</b> parameter.
        /// </returns>
        internal static string ResolveUrl(string p_AppPath, string p_Url)
        {
            if (p_Url.Length == 0 || p_Url[0] != '~')
                return p_Url; // there is no ~ in the first character position, just return the url
            if (p_Url.Length == 1)
                return p_AppPath; // there is just the ~ in the URL, return the appPath
            if (p_Url[1] == '/' || p_Url[1] == '\\')
            {
                // url looks like ~/ or ~\
                if (p_AppPath.Length > 1)
                    return p_AppPath + "/" + p_Url.Substring(2);
                return "/" + p_Url.Substring(2);
            }
            // url looks like ~something
            if (p_AppPath.Length > 1)
                return p_AppPath + "/" + p_Url.Substring(1);
            return p_AppPath + p_Url.Substring(1);
        }
    }
}