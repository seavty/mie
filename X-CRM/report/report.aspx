<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="report.aspx.cs" Inherits="X_CRM.report.report" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <link href="Scripts/css/metro.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-icons.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-colors.min.css" rel="stylesheet" />


    <link href="Scripts/css/jquery.gridly.css" rel="stylesheet" />

    <script src="<%=url%>Scripts/jquery_min.js"></script>
    <script src="<%=url%>Scripts/js/metro.js"></script>
    <script src="<%=url%>Scripts/js/select2.min.js"></script>

    <script>
        


    </script>

</head>
<body>

    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server">
            </asp:ScriptManager>
        </div>
        
        <rsweb:ReportViewer  Width="100%" Height="100%" ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt" WaitMessageFont-Names="Verdana" WaitMessageFont-Size="14pt"></rsweb:ReportViewer>
    </form>
</body>
</html>

