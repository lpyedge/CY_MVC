<!DOCTYPE>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="CY_MVC.ViewTemplate.Default.TemplateBody" %>
<html>
<head>
    <!-- #include file="_header.htm" -->
</head>
<body>
    <div id="wrapper">
        <div id="header">
            <div class="site">
                <h2>
                    <a href="#">模版位置</a>
                </h2>
                <p>
                    <%= ViewData["TemplatePath"].ToString() %>
                </p>
            </div>
        </div>
        <div id="container">
            <div id="content">
                <form action="/news.html" method="post">
                    <input type="text" name="pageindex" value="1" />
                    <input type="submit" name="news" value="提交到新闻页" />
                </form>
                <form action="/index222.html" method="post">
                    <input type="hidden" name="password" value="nonehahaha" />
                    <input type="text" name="word" value="aaaaa" />
                    <input type="submit" name="index" value="提交到Remap首页" />
                </form>
                <br />
                <br />
                <input type="button" value="测试Command" onclick=" GetCommand() " />
                <input type="button" value="测试Command Class" onclick=" GetCommandClass() " />
                <input type="button" value="测试Command User Class" onclick=" GetCommandUserClass() " />
                <script type="text/javascript">
                    function GetCommand() {
                        Ajax("/cmd.ashx", "Test", {}, function (data) {
                            alert(data);
                        });
                    }
                    function GetCommandClass() {
                        Ajax("/cmd.ashx", "User.Login", {}, function (data) {
                            alert(data);
                        });
                    }
                    function GetCommandUserClass() {
                        Ajax("/user.ashx", "Login", {}, function (data) {
                            alert(data);
                        });
                    }


                </script>
            </div>
        </div>
        <!-- #include file="_footer.htm" -->
    </div>
</body>
</html>
