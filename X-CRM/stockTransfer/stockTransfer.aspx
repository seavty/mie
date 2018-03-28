<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="stockTransfer.aspx.cs" Inherits="X_CRM.stockTransfer.stockTransfer" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblstockTransferItemNew", "frm", "dvList");
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
                    window.location = "stockTransfer.aspx?sttf_stocktransferid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    $("#dvPopContent").html(re[0].msg);
                    metroDialog.open('#dialog'); 
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
                    window.location = "stockTransferList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            $(".cnit_Qty,.cnit_Price").unbind("change");

            initComp("");


            $(".cnit_Qty,.cnit_Price").change(function (e) {
                var n = "";
                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");
                }

                var qty = $("#cnit_Qty" + n).val();
                var price = $("#cnit_Price" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblcnit_Total" + n).text(total);
                $("#cnit_Total" + n).val(total);
                calTotal();
            });

        }

        function calTotal() {

            var quot_Total = 0;

            $.each($(".lblquit_Total"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
                    var quit_Total = $(v).text();
                    if (isNaN(quit_Total) || quit_Total == "") {
                        quit_Total = 0;
                    }
                    quot_Total = parseFloat(quot_Total) + parseFloat(quit_Total);
                }
            });

            $("#lblcrdn_Total").text(quot_Total.toFixed(numFormat));
        }


    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>

    <div data-role="dialog" class="dialog" id="dialog"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">

        <div class="">
            <div id="dvPopContent" style="max-height: 500px; overflow: auto; padding: 20px;">
            </div>
        </div>
    </div>
</asp:Content>

