<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="routePlanning.aspx.cs" Inherits="X_CRM.routePlanning.routePlanning" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            initComp("");
            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblRoutePlanningItemNew", "frm", "dvList");
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
                    window.location = "routePlanning.aspx?rtpn_routeplanningid=" + re[0].msg;
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
                    window.location = "routePlanningList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {
            $(".databind").on("change", function (e) {
                if ($(e.target).attr("id") == "quot_OpportunityID") {
                    if ($("#quot_OpportunityID").val() != null) {

                        $.ajax({
                            url: "../opportunity/opportunity.aspx",
                            data: "app=getOppo&eid=" + $("#quot_OpportunityID").val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#quot_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                                $("#quot_CustomerID").val(data[0].cust_CustomerID).trigger("change");


                            }
                        });
                    }
                }
                if ($(e.target).hasClass("quit_ItemID")) {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }
                    if ($("#quit_ItemID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#quit_ItemID" + n).val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#quit_Description" + n).val(data[0].item_Name);
                                $("#quit_Qty" + n).val(1);
                                $("#quit_Price" + n).val(data[0].item_Price);
                                $("#quit_Price" + n).change();
                                calTotal();
                            }
                        });
                    }
                }

            });

            

        }

        

        

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
