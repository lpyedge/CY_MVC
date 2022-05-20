using CY_MVC.Seo;
using CY_MVC.UrlRewrite;
using CY_MVC.Utility;
using CY_MVC.ViewTemplate;
using CY_MVC.WordFilter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Text;
using System.Web;

namespace CY_MVC.HttpHandlers
{
    public abstract class PageHandler : BaseHandler
    {

        private static readonly string PageHandlerKey = typeof(PageHandler).FullName;

        #region private函数

        private string ResponsePage()
        {
            Func<string> PageLoadRender = () =>
            {
                string renderHtml;

                Stopwatch sw = null;

                if (StaticConfig.Debug)
                {
                    sw = new Stopwatch();
                    sw.Start();
                }

                Page_Load();

                if (StaticConfig.Debug)
                {
                    sw.Stop();
                    Response.Headers["Debug_ProcessPageLoad"] = sw.ElapsedMilliseconds.ToString();
                    sw.Restart();
                }

                using (ViewTemplate = string.IsNullOrWhiteSpace(TemplatePath)
                    ? ViewTemplateHelper.GetView(this)
                    : ViewTemplateHelper.GetView(TemplatePath))
                {
                    TemplatePath = ViewTemplate.TemplateFile;

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_ViewTemplateGenerate"] = sw.ElapsedMilliseconds.ToString();
                        sw.Restart();
                    }

                    for (var i = 0; i < Request.Form.Count; i++)
                    {
                        if (Request.Form.Keys[i] == null || Request.Form[i] == null) continue;
                        if (!ViewData.ContainsKey(Request.Form.Keys[i]))
                        {
                            ViewData[Request.Form.Keys[i]] = Request.Form[i];
                        }
                    }

                    renderHtml = ViewTemplate.BuildString(ViewData);

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_ViewTemplateBuild"] = sw.ElapsedMilliseconds.ToString();
                        sw.Restart();
                    }

                    //销毁View对象,释放内存
                    ViewData = null;


                    if (StaticConfig.WhiteSpaceClear)
                    {
                        renderHtml = StaticConfig.WhiteSpaceHtmlRegex.Replace(renderHtml, "><");
                        renderHtml = StaticConfig.WhiteSpaceAllRegex.Replace(renderHtml, " ");
                    }

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_WhiteSpaceClear"] = sw.ElapsedMilliseconds.ToString();
                        sw.Restart();
                    }

                    renderHtml = SeoHelper.Execute(renderHtml);

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_Seo"] = sw.ElapsedMilliseconds.ToString();
                        sw.Restart();
                    }

                    renderHtml = WordFilterHelper.Execute(renderHtml);

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_WordFilter"] = sw.ElapsedMilliseconds.ToString();
                        sw.Restart();
                    }

                    renderHtml = Page_Render(renderHtml);

                    if (StaticConfig.Debug)
                    {
                        sw.Stop();
                        Response.Headers["Debug_ProcessPageRender"] = sw.ElapsedMilliseconds.ToString();
                    }

                }

                return renderHtml;
            };

            string htmlstr;
            if (ServerCacheSeconds > 0)
            {
                if (!MemoryCacher.TryGet(PageHandlerKey + Request.RawUrl.ToLowerInvariant(), out htmlstr))
                {
                    htmlstr = PageLoadRender();

                    MemoryCacher.Set(
                            PageHandlerKey + Request.RawUrl.ToLowerInvariant(), htmlstr,
                             CacheItemPriority.Default, DateTime.Now.AddSeconds(ServerCacheSeconds), null,
                        new List<ChangeMonitor>() { MemoryCacher.DependencyOnFiles(StaticConfig.RootPath.TrimEnd('/') + TemplatePath) });
                }
            }            
            else
            {
                htmlstr = PageLoadRender();
            }

            return htmlstr;
        }

        #endregion private函数

        public override void ProcessRequest(HttpContext p_Context)
        {
            Stopwatch sw = null;

            if (StaticConfig.Debug)
            {
                sw = new Stopwatch();
                sw.Start();
            }

            Init(p_Context);

            Response.ContentType = "text/html;charset=utf-8";

            ViewData = new Dictionary<string, dynamic>();

           
            byte[] PageBytes;
            if (ClientCacheSeconds > 0)
            {
                var DateLastModified = DateTime.MinValue;
                if (!string.IsNullOrEmpty(p_Context.Request.Headers["If-Modified-Since"]))
                {
                    DateTime.TryParse(p_Context.Request.Headers["If-Modified-Since"], out DateLastModified);
                }
                if (DateLastModified.AddSeconds(ClientCacheSeconds) < DateTime.Now)
                {
                    //Response.Cache.SetExpires(DateTime.Now.AddSeconds(CacheSeconds));
                    //Response.Cache.SetMaxAge(TimeSpan.FromSeconds(CacheSeconds));
                    Response.Cache.SetLastModified(DateTime.Now);
                    Response.Cache.SetCacheability(HttpCacheability.Public);
                    
                    PageBytes = Encoding.UTF8.GetBytes(ResponsePage());
                    if (StaticConfig.GzipLength != 0 && HttpGzip.CanGZip() &&
                        PageBytes.Length > StaticConfig.GzipLength)
                    {
                        Response.AppendHeader("Content-Encoding", "gzip");
                        PageBytes = HttpGzip.GzipStr(PageBytes);
                    }
                    Response.BinaryWrite(PageBytes);
                }
                else
                {
                    Response.ClearHeaders();
                    Response.Cache.SetLastModified(DateLastModified);
                    Response.Status = "304 Not Modified";
                    Response.StatusCode = 304;
                    Response.AppendHeader("Content-Length", "0");
                }
            }
            else
            {
                PageBytes = Encoding.UTF8.GetBytes(ResponsePage());
                if (StaticConfig.GzipLength != 0 && HttpGzip.CanGZip() && PageBytes.Length > StaticConfig.GzipLength)
                {
                    Response.AppendHeader("Content-Encoding", "gzip");
                    PageBytes = HttpGzip.GzipStr(PageBytes);
                }
                Response.BinaryWrite(PageBytes);
            }

            if (StaticConfig.Debug)
            {
                sw.Stop();
                Response.Headers["Debug_ProcessTotal"] = sw.ElapsedMilliseconds.ToString();
            }

            if (!HttpRuntime.UsingIntegratedPipeline)
                return;
            //判断是否处于集成管道模式下，否则的话无法执行下列方法
            foreach (var Key in Response.Headers.AllKeys)
            {
                switch (Key.ToLowerInvariant())
                {
                    case "x-powered-by":
                        Response.Headers.Remove(Key);
                        break;

                    case "x-aspnet-version":
                        Response.Headers.Remove(Key);
                        break;

                    case "server":
                        Response.Headers.Remove(Key);
                        break;

                    case "etag":
                        Response.Headers.Remove(Key);
                        break;
                }
            }
        }

        #region 变量


        /// <summary>
        /// 服务器端缓存时间(秒) 默认为0 不缓存
        /// </summary>
        protected int ServerCacheSeconds = 0;

        /// <summary>
        /// 客户端缓存时间(秒) 默认为0 不缓存
        /// </summary>
        protected int ClientCacheSeconds = 0;

        /// <summary>
        /// 模板对象
        /// </summary>
        protected BaseViewTemplate ViewTemplate;

        /// <summary>
        ///     Seo标题
        /// </summary>
        public string Title
        {
            set { Items[SeoHelper.TitleKey] = value; }
            get { return Items[SeoHelper.TitleKey].ToString(); }
        }

        /// <summary>
        ///     Seo关键词
        /// </summary>
        public string Keywords
        {
            set { Items[SeoHelper.KeywordsKey] = value; }
            get { return Items[SeoHelper.KeywordsKey].ToString(); }
        }

        /// <summary>
        ///     Seo说明
        /// </summary>
        public string Description
        {
            set { Items[SeoHelper.DescriptionKey] = value; }
            get { return Items[SeoHelper.DescriptionKey].ToString(); }
        }

        /// <summary>
        ///     模版路径，基于网站根目录
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        ///     模版视图
        /// </summary>
        public Dictionary<string, dynamic> ViewData
        {
            set { Items["_ViewData_"] = value; }
            get { return (Dictionary<string, dynamic>)Items["_ViewData_"]; }
        }

        public string Info { get; set; }

        #endregion 变量

        #region public函数

        public void SearchEngineOptimization(params dynamic[] objs)
        {
            if (!string.IsNullOrWhiteSpace(StaticConfig.SeoRulesPath))
            {
                SeoHelper.Seo(this, GetType().Name, objs);
            }
        }

        public void SearchEngineOptimization<T>(params dynamic[] objs) where T : UrlRewritePreRule
        {
            if (!string.IsNullOrWhiteSpace(StaticConfig.SeoRulesPath))
            {
                SeoHelper.Seo(this, typeof(T).Name, objs);
            }
        }

        #endregion public函数

        #region protected函数

        protected bool IsPostBackBy(string p_BtnName)
        {
            return IsPostBack && (Request.Form[p_BtnName] != null);
        }

        /// <summary>
        ///     页面呈现方法
        /// </summary>
        protected virtual string Page_Render(string renderHtml) {
            return renderHtml;
        }

        #endregion protected函数
    }
}