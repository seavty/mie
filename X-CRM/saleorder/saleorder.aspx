<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="saleorder.aspx.cs" Inherits="X_CRM.saleorder.saleorder" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            $("#frmMaster").append($("#dvList"));
            event();
        });

        function print(id) {
            //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
            window.open("../report/report.aspx?report=rptSaleOrderPrint&pid=" + id, "_blank");
        }

        function savePreLine(t) {
            if ($(t).closest("form").attr("id") == "frmCreateSet") {
                var re = _savePreLine("../include.aspx", "tblSaleOrderItemDetailNew", "frm", "dvSODetailList");
                var ind = $("#dvSODetailList>table>tbody>tr").length;
                $("#dvSODetailList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
                initComp("saleorder.aspx");
            } else {
                var re = _savePreLine("../include.aspx", "tblSaleOrderItemNew", "frm", "dvList");
                var ind = $("#dvList>table>tbody>tr").length;
                $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
                event();
                calTotal()
            }

            $(".soit_Qty").change();
        }

        function delLine(pos, v) {
            if ($(v).closest("form").attr("id") == "frmEditSet") {

            } else {
                $.ajax({
                    url: "",
                    data: "app=preDelLine&id=" +
                        $(v).closest("tr").find("input[name='soit_SaleOrderItemID" + pos + "']").val(),
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
            if (frm == "frmCreateSet") {
                var re = _saveRecord(frm, screen, "createSet.aspx?sord_saleorderid=" + getUrlVars(window.location.href)["sord_saleorderid"]);
                if (re != "error") {

                    if (re[0].status == "ok") {
                        window.location = "saleorder.aspx?sord_saleorderid=" + re[0].msg;
                    }
                }
            } else {
                var re = _saveRecord(frm, screen, "");
                if (re != "error") {

                    if (re[0].status == "ok") {
                        window.location = "saleorder.aspx?sord_saleorderid=" + re[0].msg;
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
                    window.location = "saleorderList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            if ($('#soit_WarehouseID').length > 0) {
                $('#soit_WarehouseID').html('<option value="' + $("#sord_WarehouseID").val() + '">' + $("#sord_WarehouseID option:selected").text() + '</option>');
                $('#soit_WarehouseID').val($("#sord_WarehouseID").val()).trigger("change");
            }

            $("#sord_Discount,#sord_Disc").unbind("change");
            $(".soit_Qty,.soit_Price").unbind("change");

            $(".databind").on("change", function (e) {

                if ($(e.target).attr("id") == "sord_CustomerID") {
                    if ($("#sord_CustomerID").val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getCustomer&cust_customerid=" + $("#sord_CustomerID").val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                if ($("#sord_PriceListID").length > 0) {
                                    if (data[0].prls_PriceListID + "" != "null" || data[0].prls_PriceListID != null) {
                                        $("#sord_PriceListID").append("<option value='" + data[0].prls_PriceListID + "'>" + data[0].prls_Name + "</option>");
                                        $("#sord_PriceListID").trigger("change");
                                    } else {
                                        $("#sord_PriceListID").html("<option value=''></option>");
                                        $("#sord_PriceListID").trigger("change");
                                    }
                                }
                            }
                        });
                    }
                }

                if ($(e.target).hasClass("soit_ItemID")) {
                    var str = "";
                    if ($("#sord_PriceListID").length > 0) {
                        if ($("#sord_PriceListID").val() != null)
                            str = "&prls_pricelistid=" + $("#sord_PriceListID").val();
                    }
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#soit_ItemID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#soit_ItemID" + n).val() +
                                "&cust_customerid=" + $("#sord_CustomerID").val() +
                                str,
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
                                    $("#soit_Unit").val(data[0].item_Unit).trigger("change");
                                }
                                $("#soit_Description" + n).val(data[0].item_Name);
                                $("#soit_Qty" + n).val(1);
                                $("#soit_Price" + n).val(data[0].item_Price);
                                $("#soit_Price" + n).change();
                                calTotal();
                            }
                        });
                    }
                }
            });

            initComp("saleorder.aspx");

            $(".soit_Qty,.soit_Price").change(function (e) {
                var n = "";

                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");

                }

                var qty = $("#soit_Qty" + n).val();
                var price = $("#soit_Price" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblsoit_Total" + n).text(total);
                $("#soit_Total" + n).val(total);
                calTotal();
            });


            $("#sord_Discount,#sord_Disc,#sord_isTax").change(function (e) {
                calTotal();
            });

        }

        function calTotal() {
            var quot_SubTotal = 0;
            var quot_Disc = $("#sord_Disc").val();
            var quot_DiscountAmount = 0;
            var quot_Total = 0;
            var tax = 0;
            var isTax = 0;
            var gTotal = 0;

            if (isNaN(quot_Disc) || quot_Disc == "") {
                quot_Disc = 0;
            }
            $.each($(".lblsoit_Total"), function (i, v) {
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
            if ($("#sord_Discount").val() == "P") {
                quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
            } else {
                quot_DiscountAmount = parseFloat(quot_Disc);
            }

            quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

            isTax = $("#sord_isTax").val();
            if (isNaN(isTax) || isTax == "") {
                isTax = 0;
            }
            tax = parseFloat(quot_Total) * parseFloat(isTax) / 100;
            gTotal = tax + quot_Total;


            $("#lblsord_SubTotal").text(quot_SubTotal.toFixed(numFormat));
            $("#lblsord_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
            $("#lblsord_Total").text(quot_Total.toFixed(numFormat));

            $("#lblsord_Tax").text(tax.toFixed(numFormat));
            $("#lblsord_GTotal").text(gTotal.toFixed(numFormat));

        }

        function editSet(id) {
            $.ajax({
                url: "editSet.aspx",
                data: "app=getItemset&sord_saleorderid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#itemSet").html(data);
                    $("#itemSet").select2();
                    $("#dvEditSetContent").append("<input type='hidden' id='txtsord_saleorderid' name='txtsord_saleorderid' value='" + id + "'/>");
                    metroDialog.open('#dvEditSet');

                    $("#itemSet").unbind("change");
                    $("#itemSet").change(function (e) {
                        if ($(this).val() == "")
                            $("#dvItemSetDetail").html("");
                        else {
                            $.ajax({
                                url: "editSet.aspx",
                                data: "app=getItemsetDetail&itemSet=" + $(this).val(),
                                type: "POST",
                                async: false,
                                error: function () {
                                    return "error";
                                },
                                beforeSend: function () {
                                },
                                success: function (data) {
                                    $("#dvItemSetDetail").html(data);
                                    $("#dvItemSetDetail>table>tbody>tr:last").remove();

                                }
                            });


                        }
                    });
                }
            });
        }

        function completeSO(id) {
            if (confirm("Complete Current Sale Order ?")) {
                $.ajax({
                    url: "shipment.aspx",
                    data: "app=completeSO&sord_saleorderid=" + id,
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

        function shipSO(id) {
            $.ajax({
                url: "shipment.aspx",
                data: "app=loadScreen&sord_saleorderid=" + id,
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
                    $("#dvPopContent").append("<input type='hidden' id='txtsord_saleorderid' name='txtsord_saleorderid' value='" + id + "'/>");
                    metroDialog.open('#dialog');
                }
            });
        }

        function saveSet() {
            //alert($("#dvItemSetDetail>table>tbody>tr").length);
            $.ajax({
                url: "editSet.aspx",
                data: "app=saveRecord&" + $("#frmEditSet").serialize() + "&N=" +
                    $("#dvItemSetDetail>table>tbody>tr").length,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    alert(data);
                }
            });
        }

        function saveShipment() {
            var isOk = 1;
            $.each($("#frmShip>div>table>tbody>tr"), function (i, v) {
                $.each($(v), function (ia, va) {
                    var soit_ShipQty = ($(va).find(".soit_ShipQty").val());
                    var soit_RemainQty = ($(va).find(".soit_RemainQty").val());
                    var soit_Qty = ($(va).find(".soit_Qty").val());

                    if (isNaN(soit_ShipQty) || soit_ShipQty == "") {
                        soit_ShipQty = 0;
                    }
                    if (isNaN(soit_RemainQty) || soit_RemainQty == "") {
                        soit_RemainQty = 0;
                    }
                    if (isNaN(soit_Qty) || soit_Qty == "") {
                        soit_Qty = 0;
                    }

                    if (parseFloat(soit_Qty) - parseFloat(soit_ShipQty) < parseFloat(soit_RemainQty) /*|| parseFloat(soit_RemainQty) == 0*/) {
                        isOk = 0;
                    }
                });
            });

            if (isOk == 0) {
                alert("Invalid Ship Quantity !");
                return;
            }

            if (confirm("Ship Current Sale Order ?")) {
                var id = $("#txtsord_saleorderid").val();
                $.ajax({
                    url: "shipment.aspx",
                    data: "app=saveRecord&sord_saleorderid=" + id +
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

        function createSet(id) {

            $.ajax({
                url: "createSet.aspx",
                data: "app=loadScreen&sord_saleorderid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvCreateSetContent").html(data);
                    event();
                    $("#frmCreateSet").find("#btCancel").removeAttr("onclick");
                    $("#frmCreateSet").find("#btCancel").click(function (e) {
                        metroDialog.close('#dvCreateSet');
                    });
                    metroDialog.open("#dvCreateSet");
                    initComp("saleorder.aspx");
                }
            });


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
                <span class="title">Ship</span>
            </button>
            <button type="button" id="btCancel" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Cancel</span>
            </button>
        </div>
    </div>

    

    <!-- Create set -->
    <div data-role="dialog" class="panel dialog" id="dvCreateSet"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="heading bg-dark">
            <span class="title">Create Set</span>
        </div>

        <div class="">
            <form id="frmCreateSet" class="frmCreateSet">
                <div id="dvCreateSetContent" style="max-height: 500px; overflow: auto; padding: 20px;">
                </div>
            </form>
        </div>

    </div>

    <!-- edit set -->
    <div data-role="dialog" class="panel dialog" id="dvEditSet"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="heading bg-dark">
            <span class="title">Edit Set</span>
        </div>

        <div class="">
            <form id="frmEditSet">
                <div id="dvEditSetContent" style="max-height: 500px; overflow: auto; padding: 20px;">
                    <div class="input-control text full-size">
                        <select class="select" id="itemSet"></select>
                    </div>
                    <div class="panel" id="dvItemSetDetail" style="border: 1px solid #eee; background-color: #fff; height: 420px; overflow: auto">
                    </div>
                </div>
            </form>
        </div>
        <div class="align-right padding20" style="padding-top: 0px">
            <button type="button" onclick="saveSet();" class="button success rounded">
                <span class="mif-dollar2 icon"></span>
                <span class="title">OK</span>
            </button>
            <button type="button" onclick="metroDialog.close('#dvEditSet');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Cancel</span>
            </button>
        </div>
    </div>

</asp:Content>
