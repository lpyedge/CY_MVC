using System.Collections;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace CY_MVC.HttpHandlers
{
    /// <summary>
    ///     后台类基类
    /// </summary>
    public abstract class BaseHandler : IHttpHandler, IRequiresSessionState
    {
        #region protected函数

        /// <summary>
        ///     是否是POST请求
        /// </summary>
        protected bool IsPostBack
        {
            get { return string.CompareOrdinal(Request.HttpMethod, "POST") == 0; }
        }

        #endregion protected函数

        /// <summary>
        ///     执行请求
        /// </summary>
        /// <param name="p_Context"></param>
        public abstract void ProcessRequest(HttpContext p_Context);

        /// <summary>
        ///     能否重用
        /// </summary>
        public bool IsReusable
        {
            get { return false; }
        }

        /// <summary>
        ///     页面加载方法
        /// </summary>
        protected abstract void Page_Load();


        #region private函数

        /// <summary>
        ///     后台类初始化
        /// </summary>
        /// <param name="p_Context"></param>
        protected void Init(HttpContext p_Context)
        {
            Items = p_Context.Items;
            Request = p_Context.Request;
            Response = p_Context.Response;
            Session = p_Context.Session;
            Server = p_Context.Server;

            //默认下发类型为html页面
            Response.ClearContent();
            Response.ClearHeaders();
            Response.ContentEncoding = Encoding.UTF8;
            Response.HeaderEncoding = Encoding.UTF8;
            Response.Charset = "utf-8";
        }

        #endregion private函数

        #region 变量

        /// <summary>
        ///     Items
        /// </summary>
        protected IDictionary Items;

        /// <summary>
        ///     Request
        /// </summary>
        protected HttpRequest Request;

        /// <summary>
        ///     Response
        /// </summary>
        protected HttpResponse Response;

        /// <summary>
        ///     Session
        /// </summary>
        protected HttpSessionState Session;

        /// <summary>
        ///     Server
        /// </summary>
        protected HttpServerUtility Server;

        /// <summary>
        ///     后台类名称
        /// </summary>
        public string Name { get; protected set; }

        #endregion 变量
    }
}