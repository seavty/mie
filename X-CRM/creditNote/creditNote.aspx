<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="creditNote.aspx.cs" Inherits="X_CRM.creditNote.creditNote" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblCreditNoteItemNew", "frm", "dvList");
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
                    window.location = "creditNote.aspx?crdn_creditnoteid=" + re[0].msg;
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
                    window.location = "creditNoteList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {
            
            $(".cnit_Qty,.cnit_Price").unbind("change");
            
            $(".databind").on("change", function (e) {

                if ($(e.target).attr("id") == "crdn_InvoiceID") {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#crdn_InvoiceID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getInvoice&eid=" + $("#crdn_InvoiceID" + n).val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#crdn_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                                $("#crdn_WarehouseID").append("<option value='" + data[0].ware_WarehouseID + "'>" + data[0].ware_Name + "</option>");
                                initComp("");
                            }
                        });
                    }

                }

                if ($(e.target).attr("id") == "cnit_ItemID") {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#cnit_ItemID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#cnit_ItemID" + n).val() ,
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#cnit_Description" + n).val(data[0].item_Name);
                                $("#cnit_Qty" + n).val(1);
                                $("#cnit_Price" + n).val(data[0].item_Price);
                                $("#cnit_Price" + n).change();
                                calTotal();
                            }
                        });
                    }
                }
            });
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
            
            $.each($(".lblcnit_Total"), function (i, v) {
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

        function printCN(id) {
            window.open("../report/report.aspx?report=rptCreditNotePrintUpdate&eid=" + id, "_blank");
        }
	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
