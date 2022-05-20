using CY_MVC.HttpHandlers;
using MyControllers.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using CY_Common.Core;

namespace MyControllers
{
    public class @default : PageHandler
    {
        public @default()
        {
            //CacheSeconds = 0;

            Name = "网站首页";

            Info = "[name]使用;[test]保留测试";

            //TemplatePath = AppDomain.CurrentDomain.BaseDirectory + "Templates/ViewDEfault/defaulta.ascx";// CY_MVC.StaticConfig.TemplatePath+ "Test.ascx"; //可以不填写模版页面的后缀名
        }

        protected override void Page_Load()
        {
            SearchEngineOptimization(new { name = "我的测试内容" + Request.RawUrl });

            ViewData["TemplatePath"] = CY_MVC.StaticConfig.TemplatePath;
            ViewData["ExtendPage"] = CY_MVC.StaticConfig.RootPath + CY_MVC.StaticConfig.TemplatePath + "_main.html";

            //var t = CY_MVC.ViewTemplate.ViewTemplateHelper.GetView(CY_MVC.StaticConfig.TemplatePath + "default.ascx");
            //RegisterScript("alert('" + t.TemplateText + "');");

            //if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            //{
            //    RegisterScript("alert('Request.QueryStrin：" + Request.QueryString["id"] + "');");
            //    Response.Cookies.Add(new HttpCookie("id", Request.QueryString["id"]));
            //}
            //if (Request.Cookies.Get("id") != null)
            //{
            //    RegisterScript("alert('Request.Cookies：" + Request.Cookies.Get("id") + "');");
            //}
            //var ss = CY_MVC.ViewTemplate.ViewTemplateHelper.TemplateRender(AppDomain.CurrentDomain.BaseDirectory + CY_MVC.StaticConfig.TemplatePath + "_main.html", new Dictionary<string,dynamic>());

            //SearchEngineOptimization();
            Title = "标题1";
            Description = "Description%%%%Description";
            Keywords = "Keywords####Keywords";

            ViewData["processtime"] = "";
        }

        public string CheckLogin(string username, string password)
        {
            return "帐号：" + username + "|密码：" + password + "|是否登录：" +
                   (Session["islogon"] != null ? Session["islogon"].ToString() : "空值") + "|时间：" +
                   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void SetLogin(bool res)
        {
            Session["islogon"] = res;
        }

        public News GetNews(int id, bool hot)
        {
            News temp = new News();
            temp.Title = "新闻" + id;
            temp.ID = id;
            temp.AddDate = DateTime.Now.AddDays(5);
            temp.Hot = hot;
            temp.Key = Guid.NewGuid();
            return temp;
        }

        public TespLib.AAA GetAAA(int id)
        {
            TespLib.AAA aaa = new TespLib.AAA();
            aaa.name = "haha";
            aaa.sex = true;
            aaa.users = TespLib.Dao.GetList();
            return aaa;
        }

        public string ShowUser(Users usermodel)
        {
            Response.ContentType = "text/*";
            return "用户：" + usermodel.name.ToString() + "|注册日期：" + usermodel.regdate.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public string ShowUsers(List<Users> usermodels)
        {
            return "用户：" + usermodels[1].name.ToString() + "|注册日期：" +
                   usermodels[1].regdate.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }


    public enum names
    {
        曹操,
        吕布
    }

    [Serializable()]
    public class Users
    {
        private names m_name;

        public names name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        private bool m_sex;

        public bool sex
        {
            get { return m_sex; }
            set { m_sex = value; }
        }

        private DateTime m_regdate;

        public DateTime regdate
        {
            get { return m_regdate; }
            set { m_regdate = value; }
        }

        private double m_money;

        public double money
        {
            get { return m_money; }
            set { m_money = value; }
        }
    }
}