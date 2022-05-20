using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace MyControllers
{
    public class BeginRequestTest
    {
        public static void BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current != null)
            {
                string Url = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority +
                             HttpContext.Current.Request.Url.AbsolutePath;


                HttpContext.Current.Response.Write(Url+"<br/>"+DateTime.Now.ToLongTimeString());


                WriteLog(HttpContext.Current, true);

                HttpContext.Current.Response.End();
            }
        }


        private static void WriteLog(HttpContext p_Context, bool p_NotifyRes)
        {
            try
            {
                string Name = p_Context.Request.Url.AbsolutePath + DateTime.Now.ToString("yyMMddHHmmssfff");
                string LogContent = p_Context.Request.HttpMethod + " : " + p_Context.Request.RawUrl + Environment.NewLine;

                LogContent += "Oauth:" + p_NotifyRes.ToString() + Environment.NewLine;

                LogContent += Environment.NewLine + "QueryString:" + Environment.NewLine;
                LogContent = p_Context.Request.QueryString.AllKeys.Aggregate(LogContent, (current, item) => current + (item + " = " + p_Context.Request.QueryString[item] + Environment.NewLine));

                LogContent += Environment.NewLine + "Form:" + Environment.NewLine;
                LogContent = p_Context.Request.Form.AllKeys.Aggregate(LogContent, (current, item) => current + (item + " = " + p_Context.Request.Form[item] + Environment.NewLine));

                LogContent += Environment.NewLine + "Params:" + Environment.NewLine;
                LogContent = p_Context.Request.Params.AllKeys.Aggregate(LogContent, (current, item) => current + (item + " = " + p_Context.Request.Params[item] + Environment.NewLine));

                LogContent += Environment.NewLine + "Cookies:" + Environment.NewLine;
                LogContent = p_Context.Request.Cookies.AllKeys.Aggregate(LogContent, (current, item) => current + (item + " = " + p_Context.Request.Cookies[item].Value + Environment.NewLine));

                if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "App_Data/"))
                {
                    Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "App_Data/");
                }
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "App_Data/" + Name + ".log", LogContent);
            }
            catch
            { }
        }
    }
}
