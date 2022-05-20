<%@ Page Language="C#" %>

<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.StatusCode = 404;
        Response.Status = "404 Not Found";
    }

</script>
<html>
<head>
    <title>你所输入的网址不存在!</title>
</head>
<body>
你所输入的网址不存在!<br/>
点击返回<a href="/">首页</a>
</body>
</html>