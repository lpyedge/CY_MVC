using System;
using CY_MVC.HttpHandlers;

namespace MyControllers
{
    internal class cmd : CommandHandler
    {
        protected override void Page_Load()
        {
            if (IsPostBack)
            {
                if (!string.IsNullOrEmpty(CY_MVC.Utility.AjaxMethod.Method))
                {
                    Invoke(200);
                    //ResultBytes = CY_MVC.Utility.AjaxMethod.Invoke<cmd>();
                }
            }
        }

        private dynamic Test()
        {

            var list = CY_MVC.UrlRewrite.UrlRewriteHelper.UrlRewriteRules;
            list.Clear();
            list.Add(new CY_MVC.UrlRewrite.UrlRewriteRule() { ShowUrl = "/haha", RealUrl = "/default.aspx" });

            CY_MVC.UrlRewrite.UrlRewriteHelper.UrlRewriteRules = list;

            CY_MVC.UrlRewrite.UrlRewriteHelper.Save();

            return DateTime.Now.ToLongTimeString();
        }

        private static class User
        {
            private static int a = 0;
            private static string Login()
            {
                a ++;
                return a +" User.Login." + DateTime.Now.ToLongTimeString();
            }
        }
    }
}