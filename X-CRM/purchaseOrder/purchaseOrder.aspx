<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="purchaseOrder.aspx.cs" Inherits="X_CRM.purchaseOrder.purchaseOrder" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine(t) {

            var re = _savePreLine("../include.aspx", "tblPurchaseOrderItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            event();
            calTotal()
            $(".poit_Qty").change();
        }

        function delLine(pos, v) {
            if ($(v).closest("form").attr("id") == "frmEditSet") {

            } else {
                $.ajax({
                    url: "",
                    data: "app=preDelLine&id=" +
                        $(v).closest("tr").find("input[name='poit_PurchaseOrderItemID" + pos + "']").val(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {

                    },
                    success: function (data) {
                        if (data == "ok") {
                            _delLine(pos, v);
                            calTotal();
                        } else {
                            alert(data);
                        }
                    }
                });
            }

        }

        function saveRecord(frm, screen) {

            var re = _saveRecord(frm, screen, "");
            if (re != "error") {

                if (re[0].status == "ok") {
                    window.location = "purchaseOrder.aspx?purc_purchaseorderid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    $("#dvButtons").hide();
                    $("#dvPopContent").html(re[0].msg);
                    metroDialog.open('#dialog');
                    //alert(re[0].msg);
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
                    window.location = "purchaseOrderList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            if ($('#poit_WarehouseID').length > 0) {
                $('#poit_WarehouseID').html('<option value="' + $("#purc_WarehouseID").val() + '">' + $("#purc_WarehouseID option:selected").text() + '</option>');
                $('#poit_WarehouseID').val($("#purc_WarehouseID").val()).trigger("change");
            }

            $("#purc_Discount,#purc_Disc").unbind("change");
            $(".poit_Qty,.poit_Price").unbind("change");

            $(".databind").on("change", function (e) {


                if ($(e.target).hasClass("poit_ItemID")) {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#poit_ItemID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#poit_ItemID" + n).val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#poit_Description" + n).val(data[0].item_Name);
                                $("#poit_Qty" + n).val(1);
                                $("#poit_Price" + n).val(parseFloat(data[0].item_Cost).toFixed(2));
                                $("#poit_Price" + n).change();
                                calTotal();
                            }
                        });
                    }
                }
            });

            initComp("purchaseOrder.aspx");

            $(".poit_Qty,.poit_Price").change(function (e) {
                var n = "";

                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");

                }

                var qty = $("#poit_Qty" + n).val();
                var price = $("#poit_Price" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblpoit_Total" + n).text(total);
                $("#poit_Total" + n).val(total);
                calTotal();
            });


            $("#purc_Discount,#purc_Disc,#purc_isTax").change(function (e) {
                calTotal();
            });

        }

        function calTotal() {
            var quot_SubTotal = 0;
            var quot_Disc = $("#purc_Disc").val();
            var quot_DiscountAmount = 0;
            var quot_Total = 0;
            var tax = 0;
            var isTax = 0;
            var gTotal = 0;

            if (isNaN(quot_Disc) || quot_Disc == "") {
                quot_Disc = 0;
            }
            $.each($(".lblpoit_Total"), function (i, v) {
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
            if ($("#purc_Discount").val() == "P") {
                quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
            } else {
                quot_DiscountAmount = parseFloat(quot_Disc);
            }

            quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

            isTax = $("#purc_isTax").val();
            if (isNaN(isTax) || isTax == "") {
                isTax = 0;
            }
            tax = parseFloat(quot_Total) * parseFloat(isTax) / 100;
            gTotal = tax + quot_Total;


            $("#lblpurc_SubTotal").text(quot_SubTotal.toFixed(numFormat));
            $("#lblpurc_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
            $("#lblpurc_Total").text(quot_Total.toFixed(numFormat));

            $("#lblpurc_Tax").text(tax.toFixed(numFormat));
            $("#lblpurc_GTotal").text(gTotal.toFixed(numFormat));

        }

        function completePO(id) {
            if (confirm("Complete Current Purchase Order ?")) {
                $.ajax({
                    url: "receive.aspx",
                    data: "app=completePO&purc_purchaseorderid=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        if (data == "ok") {
                            window.location = window.location.href;
                        } else {
                            alert(data);
                        }
                    }
                });
            }
        }

        function receivePO(id) {
            $.ajax({
                url: "receive.aspx",
                data: "app=loadScreen&purc_purchaseorderid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvButtons").show();
                    $("#dvPopContent").html(data);
                    $("#dvPopContent table tbody>tr:last").remove();
                    $("#dvPopContent").append("<input type='hidden' id='txtpurc_purchaseorderid' name='txtpurc_purchaseorderid' value='" + id + "'/>");
                    metroDialog.open('#dialog');
                }
            });
        }

        function saveShipment() {
            var isOk = 1;
            $.each($("#frmShip>div>table>tbody>tr"), function (i, v) {
                $.each($(v), function (ia, va) {
                    var soit_ShipQty = ($(va).find(".poit_ReceivedQty").val());
                    var soit_RemainQty = ($(va).find(".poit_RemainQty").val());
                    var soit_Qty = ($(va).find(".poit_Qty").val());

                    if (isNaN(soit_ShipQty) || soit_ShipQty == "") {
                        soit_ShipQty = 0;
                    }
                    if (isNaN(soit_RemainQty) || soit_RemainQty == "") {
                        soit_RemainQty = 0;
                    }
                    if (isNaN(soit_Qty) || soit_Qty == "") {
                        soit_Qty = 0;
                    }
                    if ((parseFloat(soit_Qty) - parseFloat(soit_ShipQty) < parseFloat(soit_RemainQty))/* || parseFloat(soit_RemainQty) == 0*/) {
                        isOk = 0;
                    }
                });
            });

            if (isOk == 0) {
                alert("Invalid Receive Quantity !");
                return;
            }

            if (confirm("Receive Current Purchase Order ?")) {
                var id = $("#txtpurc_purchaseorderid").val();
                $.ajax({
                    url: "receive.aspx",
                    data: "app=saveRecord&purc_purchaseorderid=" + id +
                        "&" + $("#frmShip").serialize(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        var re = (setError(data, "frmShip"));
                        if (re == "error") {
                        } else {
                            if (re[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                $("#dvButtons").hide();
                                $("#dvPopContent").html(re[0].msg);
                                metroDialog.open('#dialog');
                            }
                        }


                    }
                });
            }
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
            <form id="frmShip">
                <div id="dvPopContent" style="max-height: 500px; overflow: auto; padding: 20px;">
                </div>
            </form>
        </div>
        <div class="align-right padding20" style="padding-top: 0px" id="dvButtons">
            <button type="button" id="btShip" onclick="saveShipment();" class="button success rounded">
                <span class="mif-dollar2 icon"></span>
                <span class="title">Receive</span>
            </button>
            <button type="button" id="btCancel" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Cancel</span>
            </button>
        </div>
    </div>



</asp:Content>
