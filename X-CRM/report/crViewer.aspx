<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="crViewer.aspx.cs" Inherits="X_CRM.report.crViewer" %>

<%@ Register Assembly="CrystalDecisions.Web, Version=13.0.2000.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" Namespace="CrystalDecisions.Web" TagPrefix="CR" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <CR:CrystalReportViewer ID="CR" runat="server" AutoDataBind="true" />
    
</body>
</html>
