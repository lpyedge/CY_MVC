using CY_MVC.Utility;
using CY_MVC.ViewTemplate;
using Fasterflect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CY_MVC.HttpHandlers
{
    public static class HandlerHelper
    {
        internal const string Iiem_HandlerName = "CY_MVC.HttpHandlers.HandlerHelper._HandlerName_";
        internal const string Iiem_RequestUrl = "CY_MVC.HttpHandlers.HandlerHelper._RequestUrl_";

        internal const string MvcHandler = "/CYMVC.axd";

        internal static SortedList<string, Type> m_Handlers;

        internal static SortedList<string, Type> m_UrlHandlers = new SortedList<string, Type>();

        private static readonly object locker = new object();

        public static SortedList<string, Type> Handlers
        {
            get
            {
                if (m_Handlers == null)
                {
                    lock (locker)
                    {
                        if (m_Handlers == null)
                        {
                            var AssemblyLibrary = SimpleTypeHelper.AssemblyLoad(StaticConfig.LibraryName);

                            if (AssemblyLibrary != null)
                            {
                                //var pageHandlerName = typeof(PageHandler).FullName;
                                m_Handlers = new SortedList<string, Type>();
                                foreach (var item in AssemblyLibrary.TypesImplementing<BaseHandler>())
                                {
                                    if (item.FullName.StartsWith(StaticConfig.LibraryName + ".",
                                        StringComparison.OrdinalIgnoreCase))
                                        m_Handlers[item.FullName.ToLowerInvariant()
                                            .Replace(StaticConfig.LibraryName.ToLowerInvariant() + ".", string.Empty)
                                            ] = item;
                                    else
                                        m_Handlers[item.Name.ToLowerInvariant()] = item;
                                    ////预热模版
                                    //if (item.BaseType.FullName == pageHandlerName)
                                    //{
                                    //    ViewTemplateHelper.GetView(item);
                                    //}
                                }
                            }
                            else
                            {
                                Trace.Write("未能加载后台程序类库！");
                            }

                            //动态加载App_Code目录内的页面后台代码
                            var asm =
                                AppDomain.CurrentDomain.GetAssemblies()
                                    .FirstOrDefault(
                                        p => p.FullName.IndexOf("App_Code", StringComparison.OrdinalIgnoreCase) == 0);
                            if (asm != null)
                                foreach (var item in asm.TypesImplementing<BaseHandler>())
                                {
                                    if (item.FullName.StartsWith(StaticConfig.LibraryName + ".",
                                        StringComparison.OrdinalIgnoreCase))
                                        m_Handlers[item.FullName.ToLowerInvariant()
                                            .Replace(StaticConfig.LibraryName.ToLowerInvariant() + ".", string.Empty)
                                            ] = item;
                                    else
                                        m_Handlers[item.Name.ToLowerInvariant()] = item;
                                }
                        }
                    }
                }
                return m_Handlers;
            }
        }

        public static bool AddHandler(Type t)
        {
            if (t.BaseType == typeof(CommandHandler) || t.BaseType == typeof(PageHandler))
            {
                if (!Handlers.ContainsKey(t.Name.ToLowerInvariant()))
                {
                    Handlers.Add(t.Name.ToLowerInvariant(), t);
                    return true;
                }
            }
            return false;
        }

        public static bool AddHandler<T>()
        {
            var t = typeof(T);
            return AddHandler(t);
        }

        public static bool AddHandler(Type t, string url)
        {
            if (t.GetInterface("IHttpHandler") != null)
            {
                if (!m_UrlHandlers.ContainsKey(url.ToLowerInvariant()))
                {
                    m_UrlHandlers.Add(url.ToLowerInvariant(), t);
                    return true;
                }
            }
            return false;
        }

        public static bool AddHandler<T>(string url)
        {
            var t = typeof(T);
            return AddHandler(t, url);
        }

        internal static BaseHandler GetHandler(string p_HandlerName)
        {
            if (Handlers.ContainsKey(p_HandlerName))
            {
                return Activator.CreateInstance(Handlers[p_HandlerName]) as BaseHandler;
            }
            return null;
        }


        #region RemapUrl

        internal static readonly Regex RegexUrl =
            new Regex(
                @"^/([\w\W]+?).(aspx|axd|ashx|asmx|aspq|rem|rules|svc|soap|xamlx|xoml|cshtm|cshtml|vbhtm|vbhtml|htm|html)$",
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        internal static string Url2HandlerName(HttpContext p_Context)
        {
            var AbsolutePath = p_Context.Request.ApplicationPath == "/"
                ? p_Context.Request.Url.AbsolutePath
                : p_Context.Request.Url.AbsolutePath.Replace(p_Context.Request.ApplicationPath, string.Empty);
            var match = RegexUrl.Match(AbsolutePath);
            var handlerName = string.Empty;
            if (match.Success && match.Groups[1].Success)
            {
                handlerName = match.Groups[1].Value.Replace("/", ".").ToLowerInvariant();
            }
            return handlerName;
        }

        internal static void RemapHandler(HttpContext p_Context)
        {
            var HandlerName = Url2HandlerName(p_Context);
            if (m_UrlHandlers.ContainsKey(p_Context.Request.Path.ToLowerInvariant()))
            {
                p_Context.RemapHandler(
                    Activator.CreateInstance(m_UrlHandlers[p_Context.Request.Path.ToLowerInvariant()]) as IHttpHandler);
            }
            else if (Handlers.ContainsKey(HandlerName))
            {
                p_Context.RemapHandler(GetHandler(HandlerName));
                //Context.Handler = GetHandler(HandlerName);
            }
            else if (StaticConfig.IgnoreHandler)
            {
                //如果后缀名在url重写忽略列表里面则不处理
                if (StaticConfig.UrlRewriteRulesSuffixFilters.Any(
                        filter => p_Context.Request.Url.LocalPath.EndsWith("." + filter, StringComparison.OrdinalIgnoreCase)))
                    return;
                //如果网址类型在自定义handlers列表里则不处理
                if (StaticConfig.CustomHandlers.Any(
                        path =>
                        path.StartsWith("*") ?
                        p_Context.Request.Url.LocalPath.EndsWith(path.Replace("*", string.Empty), StringComparison.OrdinalIgnoreCase) :
                        p_Context.Request.Url.LocalPath.EndsWith(path, StringComparison.OrdinalIgnoreCase)
                        ))
                    return;
                //开启忽略后台代码的情况下只有存在对应前台模版才会动态调用空白后台方法
                if (ViewTemplateHelper.GetView(StaticConfig.TemplatePath + HandlerName) != null)
                    p_Context.RemapHandler(Activator.CreateInstance(typeof(DefaultHandler), HandlerName) as BaseHandler);
            }
        }

        #endregion RemapUrl
    }
}