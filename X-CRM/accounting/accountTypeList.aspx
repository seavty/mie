﻿<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="accountTypeList.aspx.cs" Inherits="X_CRM.accounting.accountTypeList" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e)
        {
            $("#frmMaster").append($("#dvList"));
            initComp("");
        });

        function findRecord(frm, screen, cPage)
        {
            _findRecord(frm, screen, "", cPage, "");
        }

        function sortClick(frm, screen, col, v)
        {
            $(v).closest("div").find("#orderFieldBy").val(col);
            var orderBy = "asc";
            if ($(v).hasClass("sort-asc")) {
                orderBy = "desc";
            } else {
                orderBy = "asc";
            }
            $(v).closest("div").find("#orderBy").val(orderBy);
            _findRecord(frm, screen, "", 1, $(v).closest("div").attr("id"));
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>
</asp:Content>