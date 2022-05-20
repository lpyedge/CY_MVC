namespace CY_MVC.HttpHandlers
{
    internal class DefaultHandler : PageHandler
    {
        public DefaultHandler(string templatename)
        {
            TemplatePath = StaticConfig.TemplatePath + templatename; //可以不填写模版页面的后缀名
        }

        protected override void Page_Load()
        {
        }
    }
}