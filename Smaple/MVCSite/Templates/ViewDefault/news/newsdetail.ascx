<!DOCTYPE>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="CY_MVC.ViewTemplate.Default.TemplateBody" %>
<html>
<head>
</head>
<body>
新闻<%= ViewData["id"] %>
<br/>
<table>
    <% foreach (TespLib.Users item in (List<TespLib.Users>) ViewData["newslist"])
       {
    %>
        <tr>
            <td>
                <%= item.name %>
            </td>
            <td>
                <%= item.sex %>
            </td>
            <td>
                <%= item.regdate.ToLongDateString() %>
            </td>
            <td>
                <%= item.money %>
            </td>
        </tr>
    <%
       }
    %>
</table>
</body>
</html>