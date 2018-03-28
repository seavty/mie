﻿<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="productionOutput.aspx.cs" Inherits="X_CRM.production.productionOutput" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblProductionOutputNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            event();
        }

        function delLine(pos, v) {
            _delLine(pos, v);

        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {

                if (re[0].status == "ok") {
                    window.location = "productionOutput.aspx?prdt_productionid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    alert(re[0].msg);
                }
            }
        }

        function loadScreen(screen, mode, eid) { // 3 edit // 2 View
            _loadScreen(screen, mode, eid, "dvContent", "");
            $("#frmMaster").append($("#dvList"));
            event();
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
                var re = _delRecord(frm, screen, eid, "", "");
                if (re == "ok") {
                    window.location = "itemList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {
            initComp("");
        }


    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    
    

</asp:Content>
