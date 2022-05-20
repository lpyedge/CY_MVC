using CY_MVC.HttpHandlers;
using CY_MVC.UrlRewrite;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;

[assembly: PreApplicationStartMethod(typeof(CY_MVC.HttpModules.MvcModule), "PreStart")]
namespace CY_MVC.HttpModules
{
    #region MvcModule

    public class MvcModule : IHttpModule
    {
        public static Action<HttpApplication, EventArgs> BeginRequestEvent;

        private static bool m_PreStarted;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void PreStart()
        {
            if (!m_PreStarted)
            {
                m_PreStarted = true;
                //注意这里的动态注册，此静态方法在Microsoft.Web.Infrastructure.DynamicModuleHelper
                Microsoft.Web.Infrastructure.DynamicModuleHelper.DynamicModuleUtility.RegisterModule(
                    typeof(MvcModule));
            }
        }

        /// <summary>初始化模块，并使其为处理请求做好准备。</summary>
        /// <param name="p_App">一个 <see cref="System.Web.HttpApplication" />，它提供对 ASP.NET 应用程序内所有应用程序对象的公用的方法、属性和事件的访问</param>
        public void Init(HttpApplication p_App)
        {
            p_App.BeginRequest += DomainCheck;


            p_App.BeginRequest += ForceHttps;

            if (BeginRequestEvent != null)
            {
                p_App.BeginRequest += (sender, args) => BeginRequestEvent(p_App, new EventArgs());
            }

            //UrlRewrite
            if (!string.IsNullOrEmpty(StaticConfig.UrlRewriteRulesPath))
            {
                p_App.BeginRequest += Rewrite;
            }

            //设置Handler
            p_App.ResolveRequestCache += SetController;

            //统计在线客户数量
            p_App.EndRequest += OnlineClient;
        }


        /// <summary>处置由实现 <see cref="System.Web.IHttpModule" /> 的模块使用的资源（内存除外）。</summary>
        public void Dispose()
        {
        }

        internal static void SetController(object p_Sender, EventArgs p_E)
        {
            var context = (p_Sender as HttpApplication).Context;
            if (context != null)
            {
                HandlerHelper.RemapHandler(context);
            }
        }
        internal static void Rewrite(object p_Sender, EventArgs p_E)
        {
            var context = ((HttpApplication)p_Sender).Context;
            if (context != null)
            {
                var ReDirect = false;
                var RealUrl = UrlRewriteHelper.GetRealUrl(context.Request.Url, out ReDirect);

                if (!string.IsNullOrEmpty(RealUrl))
                {
                    if (ReDirect)
                        context.Response.RedirectPermanent(RealUrl);
                    else
                        UrlRewriteHelper.RewriteUrl(context, RealUrl);
                }
            }

        }
        internal static void ForceHttps(object p_Sender, EventArgs p_E)
        {
            var context = (p_Sender as HttpApplication).Context;
            if (context != null &&
                StaticConfig.ForceHttps &&
                string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(context.Request.Url.Scheme, "http", StringComparison.OrdinalIgnoreCase))
            {
                var url = context.Request.Url.ToString();
                context.Response.Redirect("https" + url.Substring(4, url.Length - 4), true);
            }
        }
        internal static void DomainCheck(object p_Sender, EventArgs p_E)
        {
            var context = (p_Sender as HttpApplication).Context;
            if (context != null && !context.Request.IsLocal)
            {
                string SecretKey;
                IntPtr bstr = Marshal.SecureStringToBSTR(StaticConfig.SecretKey);
                try
                {
                    SecretKey = Marshal.PtrToStringBSTR(bstr);
                }
                finally
                {
                    Marshal.FreeBSTR(bstr);
                }
                if (!string.Equals(StaticConfig.Key, SecretKey, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("程序密钥错误！");
                }

                if (!StaticConfig.Domains.Contains(context.Request.Url.Authority.ToLowerInvariant()))
                {
                    context.Response.ClearContent();
                    context.Response.ClearHeaders();
                    context.Response.ContentType = "text/html;charset=utf-8";
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.HeaderEncoding = Encoding.UTF8;
                    context.Response.Charset = "utf-8";
                    context.Response.Write(context.Request.Url.Authority.ToLowerInvariant() + "域名受限，请检查网站域名设置！");
                    context.Response.End();
                }
                else if (!string.IsNullOrWhiteSpace(StaticConfig.MainDomain))
                {
                    //本来是准备在检查域名的时候就做判断如果有主域名存在的话就自动跳转到主域名下
                    //和url重写的时候会有冲突。需要二级域名url重写的话就不能设置主域名
                    if (
                        !string.Equals(context.Request.Url.Authority, StaticConfig.MainDomain,
                            StringComparison.OrdinalIgnoreCase))
                        context.Response.RedirectPermanent(
                            context.Request.Url.AbsoluteUri.Replace(
                                context.Request.Url.Authority, StaticConfig.MainDomain), true);
                }
            }
        }

        internal static void OnlineClient(object p_Sender, EventArgs p_E)
        {
            var context = (p_Sender as HttpApplication).Context;
            if (context != null && context.Handler != null && string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase) && StaticConfig.OnlineClient)
            {
                CY_MVC.OnlineClient.OnlineClientHelper.Analyse(context);
            }
        }
    }

    #endregion MvcModule
}