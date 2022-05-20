using CY_MVC.HttpHandlers;
using CY_MVC.Utility;
using MyControllers.Models;
using System;
using System.Collections.Generic;

namespace MyControllers
{
    internal class newslist : PageHandler
    {
        protected override void Page_Load()
        {
            ViewData["IncludePage"] = CY_MVC.StaticConfig.RootPath + CY_MVC.StaticConfig.TemplatePath + "_page.html";


            ulong pageindex = 1;
            if (!string.IsNullOrEmpty(Request.QueryString["page"]))
            {
                if (ulong.TryParse(Request.QueryString["page"], out pageindex))
                {
                    Title = "GET方法获取页数" + pageindex.ToString();
                }
            }
            if (IsPostBackBy("news"))
            {
                if (!string.IsNullOrEmpty(Request.Form["pageindex"]))
                {
                    if (ulong.TryParse(Request.Form["pageindex"], out pageindex))
                    {
                        Title = "POST方法获取页数" + pageindex.ToString();
                    }
                }
            }

            List<News> list = new List<News>();
            for (int i = 0; i < 1000; i++)
            {
                News temp = new News();
                temp.Title = "新闻" + i.ToString();
                temp.ID = i;
                temp.AddDate = DateTime.Now.AddDays(i);
                temp.Hot = i < 5;
                temp.Key = Guid.NewGuid();
                list.Add(temp);
            }
            ViewData["list"] = list;

            ViewData["datapage"] = DataPage.Generate(pageindex, 50, 1000, 15, "/news.{0}.html");
        }
    }
}