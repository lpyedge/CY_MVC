using CY_MVC.Utility;
using System;
using System.Linq;
using System.Web;

namespace CY_MVC.OnlineClient
{
    public static class OnlineClientHelper
    {
        private static readonly string OnlineClientKey = typeof(OnlineClientHelper).FullName;

        internal static void Analyse(HttpContext context)
        {
            string userIpAddress = null;
            //ipAddress = this.getRequest().getRemoteAddr();
            userIpAddress = context.Request.Headers["x-forwarded-for"];
            if (string.IsNullOrEmpty(userIpAddress) || string.Equals("unknown", userIpAddress, StringComparison.OrdinalIgnoreCase))
            {
                userIpAddress = context.Request.Headers["Proxy-Client-IP"];
            }
            if (string.IsNullOrEmpty(userIpAddress) || string.Equals("unknown", userIpAddress, StringComparison.OrdinalIgnoreCase))
            {
                userIpAddress = context.Request.Headers["WL-Proxy-Client-IP"];
            }
            if (string.IsNullOrEmpty(userIpAddress) || string.Equals("unknown", userIpAddress, StringComparison.OrdinalIgnoreCase))
            {
                userIpAddress = context.Request.ServerVariables["REMOTE_ADDR"];
                if (string.Equals("127.0.0.1", userIpAddress, StringComparison.OrdinalIgnoreCase))
                {
                    userIpAddress = context.Request.UserHostAddress;
                }
            }
            //对于通过多个代理的情况，第一个IP为客户端真实IP,多个IP按照','分割
            if (userIpAddress != null && userIpAddress.Length > 15)
            { //"***.***.***.***".length() = 15
                if (userIpAddress.IndexOf(",") > 0)
                {
                    userIpAddress = userIpAddress.Substring(0, userIpAddress.IndexOf(","));
                }
            }

            var userAgent = context.Request.UserAgent;
            var userLanguages = (context.Request.UserLanguages != null && context.Request.UserLanguages.Length > 0) ? context.Request.UserLanguages.Aggregate(string.Empty, (x, y) => x += y) : "";

            if (!MemoryCacher.Contains(OnlineClientKey + userIpAddress + userAgent + userLanguages))
            {
                MemoryCacher.Set(OnlineClientKey + userIpAddress + userAgent + userLanguages, "ip:" + userIpAddress + " agent:" + userAgent + " languages:" + userLanguages, System.Runtime.Caching.CacheItemPriority.Default, null, TimeSpan.FromMinutes(30));
            }
        }

        public static int Count
        {
            get
            {
                return MemoryCacher.GetByKeyStartWith(OnlineClientKey).Count;
            }
        }
    }
}
