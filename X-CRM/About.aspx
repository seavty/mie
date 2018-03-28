<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="X_CRM.About" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="panel align-center" style="width: 500px; margin: 0 auto; z-index: 0">
        <img src="imgs/logo.png" style="width: 800px;" />
        <hr class="thin bg-lighterGray" />
        <div style="text-align:left">
            <h2>X-CRM 2.0</h2>
            <hr  class="thin bg-lighterGray" />
            Licensed To : <label id="lblInfo" runat="server"></label>
            <hr  class="thin bg-lighterGray" />
            <h4>Modules</h4>
            <ul id="ulModules" runat="server">
            </ul>
        </div>
        <hr  class="thin bg-lighterGray" />
        <label class="">X-ware powered</label>
    </div>
</asp:Content>
