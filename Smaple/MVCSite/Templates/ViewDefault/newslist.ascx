<!DOCTYPE>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="CY_MVC.ViewTemplate.Default.TemplateBody" %>
<html>
<head>
</head>
<body>
<table>
    <% foreach (var item in (List<MyControllers.Models.News>) ViewData["list"])
       {
    %>
        <tr>
            <td>
                <a href="<%= item.URL %>">
                    <%= item.Title %>
                </a>
            </td>
            <td>
                <%= item.AddDate %>
            </td>
            <td>
                <% if (item.Hot)
                   { %>
                    热门
                <% }
                   else
                   {
                %>
                    冷门
                <% } %>
            </td>
            <td>
                <%= item.Key %>
            </td>
        </tr>
    <%
       }
    %>
</table>
<br/>
<!-- #include file = "_page.htm" -->
<br/>
</body>
</html>