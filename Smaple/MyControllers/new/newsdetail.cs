using CY_MVC.HttpHandlers;
using System.Web;

namespace MyControllers.news
{
    internal class newsdetail : PageHandler
    {
        protected override void Page_Load()
        {
            SearchEngineOptimization();

            if (HttpContext.Current.Request.HttpMethod.ToLowerInvariant() == "get")
            {
                ViewData["id"] = HttpContext.Current.Request.QueryString["id"];
                ViewData["newslist"] = TespLib.Dao.GetList();
            }
        }
    }
}