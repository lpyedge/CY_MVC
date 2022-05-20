using System;
using CY_MVC.HttpHandlers;

namespace MyControllers
{
    internal class user : CommandHandler
    {
        protected override void Page_Load()
        {
            if (IsPostBack)
            {
                if (!string.IsNullOrEmpty(CY_MVC.Utility.AjaxMethod.Method))
                {
                    ResultBytes = CY_MVC.Utility.AjaxMethod.Invoke<TespLib.User>();
                }
            }
        }
    }
}