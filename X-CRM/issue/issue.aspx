﻿<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="issue.aspx.cs" Inherits="X_CRM.issue.issue" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblIssueItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            event();
            calTotal()
        }

        function delLine(pos, v) {
            _delLine(pos, v);
            calTotal();
        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "issue.aspx?issu_issueid=" + re[0].msg;
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
                    window.location = "issueList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            $(".isit_Qty,.isit_Price").unbind("change");

            initComp();


            $(".isit_Qty,.isit_Price").change(function (e) {
                var n = "";
                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");
                }

                var qty = $("#isit_Qty" + n).val();
                var price = $("#isit_Price" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblisit_Total" + n).text(total);
                $("#isit_Total" + n).val(total);
                calTotal();
            });

            $(".databind").on("change", function (e) {

                if ($(e.target).hasClass("isit_ItemID")) {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#isit_ItemID" + n).val() != null) {
                        var str = "";

                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#isit_ItemID" + n).val() +
                                "&cust_customerid=",
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                if (data[0].item_Unit != null || data[0].item_Name + "" != "null") {
                                    $("#isit_Unit").val(data[0].item_Unit).trigger("change");
                                }

                            }
                        });
                    }
                }
            });

        }

        function calTotal() {

            var rece_Total = 0;

            $.each($(".lblisit_Total"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
                    var reit_Total = $(v).text();
                    if (isNaN(reit_Total) || reit_Total == "") {
                        reit_Total = 0;
                    }
                    rece_Total = parseFloat(rece_Total) + parseFloat(reit_Total);
                }
            });


            $("#lblissu_Total").text(rece_Total.toFixed(numFormat));
        }


    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
