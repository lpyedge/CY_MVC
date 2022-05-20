using System;

namespace TespLib
{
    public class User
    {
        private static int a = 0;
        public static string Login()
        {
            a++;
            return a + " User.Login." + DateTime.Now.ToLongTimeString();
        }
    }
}
