using CY_Common.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace MyControllers
{

    public class UrlHandlerTest : IHttpHandler
    {
        public static string VerifyCodeImage
        {
            get
            {
                return "<img id=\"VerifyCodeImage\" src=\"/VerifyCodePage.axd\" alt=\"验证码\" onclick=\"RefreshVerifyCodeImage()\" /><script type=\"text/javascript\">function RefreshVerifyCodeImage(){document.getElementById(\"VerifyCodeImage\").src = \"/VerifyCodePage.axd?\" + Math.random();}</script>";
            }
        }
        public static string VerifyCodeImageCustom(int length, params string[] VerifyChars)
        {
            var vstr = "";
            if (VerifyChars.Length > 0)
            {
                vstr = VerifyChars.Aggregate(vstr, (current, t) => current + Uri.EscapeDataString(t.ToString()));
            }
            return $"<img id=\"VerifyCodeImage\" src=\"/VerifyCodePage.axd?vstr={vstr}&length={length}\" alt=\"验证码\" onclick=\"RefreshVerifyCodeImage()\" /><script type=\"text/javascript\">function RefreshVerifyCodeImage(){{document.getElementById(\"VerifyCodeImage\").src = \"/VerifyCodePage.axd?vstr={vstr}&length={length}&\" + Math.random();}}</script>";
        }

        void IHttpHandler.ProcessRequest(HttpContext p_Context)
        {
            if (p_Context.Response.IsClientConnected)
            {
                var vstr = !string.IsNullOrWhiteSpace(p_Context.Request.QueryString["vstr"])
                    ? p_Context.Request.QueryString["vstr"]
                    : "0123456789";

                var length = !string.IsNullOrWhiteSpace(p_Context.Request.QueryString["length"])
                    ? p_Context.Request.QueryString["length"].TryParse<int>(5)
                    : 5;

                p_Context.Response.Write(vstr + "<br/>" + length);

                p_Context.Response.End();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
