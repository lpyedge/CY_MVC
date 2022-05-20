<%@ Application Language="C#" %>

<script RunAt="server">

    private void Application_Start(object sender, EventArgs e)
    {
        // 在应用程序启动时运行的代码

        //CY_MVC.HttpModules.MvcModule.BeginRequestEvent += MyControllers.BeginRequestTest.BeginRequest;
        CY_MVC.HttpHandlers.HandlerHelper.AddHandler<CY_Common.Web.VerifyCodePage>("/verifycodepage.axd");
        CY_MVC.HttpHandlers.HandlerHelper.AddHandler<MyControllers.UrlHandlerTest>("/UrlHandlerTest");

        CY_MVC.HttpModules.MvcModule.BeginRequestEvent += MyControllers.core.BeginRequest;
        CY_MVC.HttpModules.MvcModule.BeginRequestEvent += MyControllers.core.BeginRequest1;
    }

    private void Application_End(object sender, EventArgs e)
    {
        //  在应用程序关闭时运行的代码
    }

    private void Application_Error(object sender, EventArgs e)
    {
        // 在出现未处理的错误时运行的代码
    }

    private void Session_Start(object sender, EventArgs e)
    {
        // 在新会话启动时运行的代码
    }

    private void Session_End(object sender, EventArgs e)
    {
        // 在会话结束时运行的代码。 
        // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
        // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
        // 或 SQLServer，则不引发该事件。
    }

</script>