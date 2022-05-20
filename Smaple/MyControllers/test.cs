using CY_MVC.HttpHandlers;

namespace MyControllers
{
    internal class test : CommandHandler
    {
        protected override void Page_Load()
        {
            Response.Write("111111");
        }
    }
}