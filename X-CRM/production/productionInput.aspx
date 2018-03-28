<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="productionInput.aspx.cs" Inherits="X_CRM.production.productionInput" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblProductionInputNew", "frm", "dvList");
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
                    window.location = "productionInput.aspx?prdt_productionid=" + re[0].msg;
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
            $(".ptip_Qty,.ptip_Cost").change(function (e) {
                var n = "";

                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");

                }

                var qty = $("#ptip_Qty" + n).val();
                var price = $("#ptip_Cost" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblptip_Total" + n).text(total);
                $("#ptip_Total" + n).val(total);
                calTotal();
            });
        }

        function calTotal() {
            var quot_SubTotal = 0;
            $.each($(".lblptip_Total"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
                    var quit_Total = $(v).text().replace(/,/g, '');
                    if (isNaN(quit_Total) || quit_Total == "") {
                        quit_Total = 0;
                    }
                    quot_SubTotal = parseFloat(quot_SubTotal) + parseFloat(quit_Total);
                }
            });

            $("#lblprdt_Total").text(quot_SubTotal.toFixed(numFormat));
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    
    

</asp:Content>
