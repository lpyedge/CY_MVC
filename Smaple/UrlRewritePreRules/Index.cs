using CY_MVC.UrlRewrite;

namespace UrlRewritePreRules
{
    public class Index : UrlRewritePreRule
    {
        public Index()
        {
            RealUrl = "~/index.html";
            ShowUrl = "/";
            Text = "首页";
        }
    }
}