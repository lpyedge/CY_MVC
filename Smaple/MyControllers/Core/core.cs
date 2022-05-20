using System;
using System.Web;

namespace MyControllers
{
    public class core
    {
        public static void BeginRequest(HttpApplication p_Sender, EventArgs p_E)
        {
            string a = p_E.ToString();
        }

        public static void BeginRequest1(HttpApplication p_Sender, EventArgs p_E)
        {
            string a = p_E.ToString();
        }
    }
}