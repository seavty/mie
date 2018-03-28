<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="APInvoice.aspx.cs" Inherits="X_CRM.apInvoice.APInvoice" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblAPInvoiceItemNew", "frm", "dvList");
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
                    window.location = "apinvoice.aspx?apiv_apinvoiceid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    $("#returnBtn").hide();
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
                    window.location = "apinvoiceList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            $("#invo_Discount,#invo_Disc").unbind("change");
            $(".init_Qty,.init_Price").unbind("change");

            $(".databind").on("change", function (e) {
                if ($(e.target).attr("id") == "invo_OpportunityID") {
                    if ($("#invo_OpportunityID").val() != null) {
                        $.ajax({
                            url: "../opportunity/opportunity.aspx",
                            data: "app=getOppo&eid=" + $("#invo_OpportunityID").val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#invo_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                                initComp("APInvoice.aspx");
                            }
                        });
                    }
                }

                if ($(e.target).hasClass("apit_ItemID")) {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#apit_ItemID" + n).val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getItem&eid=" + $("#apit_ItemID" + n).val() +
                                "&cust_customerid=0",
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#apit_Description" + n).val(data[0].item_Name);
                                $("#apit_Qty" + n).val(1);
                                $("#apit_Price" + n).val(data[0].item_Price);
                                $("#apit_Price" + n).change();
                                calTotal();
                            }
                        });
                    }
                }
            });

            initComp("APInvoice.aspx");

            $(".apit_Qty,.apit_Price").change(function (e) {
                var n = "";

                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");

                }

                var qty = $("#apit_Qty" + n).val();
                var price = $("#apit_Price" + n).val();
                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
                }
                var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
                $("#lblapit_Total" + n).text(total);
                $("#apit_Total" + n).val(total);
                calTotal();
            });


            $("#apiv_Discount,#apiv_Disc,#apiv_isTax").change(function (e) {
                calTotal();
            });

        }

        function calTotal() {
            var quot_SubTotal = 0;
            var quot_Disc = $("#apiv_Disc").val();
            var quot_DiscountAmount = 0;
            var quot_Total = 0;
            var tax = 0;
            var isTax = 0;
            var gTotal = 0;

            if (isNaN(quot_Disc) || quot_Disc == "") {
                quot_Disc = 0;
            }
            $.each($(".lblapit_Total"), function (i, v) {
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
            if ($("#apiv_Discount").val() == "P") {
                quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
            } else {
                quot_DiscountAmount = parseFloat(quot_Disc);
            }

            quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

            isTax = $("#apiv_isTax").val();
            if (isNaN(isTax) || isTax == "") {
                isTax = 0;
            }
            tax = parseFloat(quot_Total) * parseFloat(isTax) / 100;
            gTotal = tax + quot_Total;


            $("#lblapiv_SubTotal").text(quot_SubTotal.toFixed(numFormat));
            $("#lblapiv_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
            $("#lblapiv_Total").text(quot_Total.toFixed(numFormat));

            $("#lblapiv_Tax").text(tax.toFixed(numFormat));
            $("#lblapiv_GTotal").text(gTotal.toFixed(numFormat));

            $("#lblapiv_Balance").text((gTotal - parseFloat($("#lblapiv_PaidAmount").text())).toFixed(numFormat));
        }

        function payment(id) {
            $.ajax({
                url: "payment.aspx",
                data: "app=loadScreen&apiv_apinvoiceid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvPopContent").html(data);
                    $("#frmPayment").find("#btSave").removeAttr("onclick");
                    $("#frmPayment").find("#btCancel").removeAttr("onclick");

                    $("#frmPayment").find("#btCancel").click(function (e) {
                        metroDialog.close('#dialog');
                    });

                    $("#frmPayment").find("#btSave").click(function (e) {
                        savePayment(id);
                    });
                    $("#returnBtn").hide();
                    metroDialog.open('#dialog');
                }
            });
        }

        function savePayment(id) {
            $.ajax({
                url: "payment.aspx",
                data: "app=saveRecord&invo_invoiceid=" + id +
                    "&" + $("#frmPayment").serialize() + "&supp_supplierid=0",
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    var re = (setError(data, "frmPayment"));
                    if (re == "error") {
                    } else {
                        if (re[0].status == "ok") {
                            window.location = window.location.href;
                        } else {
                            alert(re[0].msg);
                        }
                    }
                }
            });
        }


        

        function printInv(id) {
            window.open("../report/report.aspx?report=rptInvoicePrint&pid=" + id, "_blank");
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

        <div class="align-right padding20" style="padding-top: 0px" id="returnBtn">
            <button type="button" id="btShip" onclick="saveReturn();" class="button success rounded">
                <span class="mif-dollar2 icon"></span>
                <span class="title">Return</span>
            </button>
            <button type="button" id="btCancel" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Cancel</span>
            </button>
        </div>
    </div>

    <!-- Add Item -->
    <div data-role="dialog" class="panel dialog" id="dvAddItem"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="heading bg-dark">
            <span class="title">Add Item</span>
        </div>

        <div class="">

            <div style="max-height: 500px; min-height: 500px; overflow: auto; padding: 20px;">
                <div style="max-height: 460px; min-height: 460px; width: 200px; float: left; border: 1px solid #ddd; background-color: #fff; overflow: auto; padding: 10px;" id="dvItemGroup">b</div>
                <div class="grid" style="margin-top:0px;max-height: 460px; min-height: 460px; width: calc(100% - 200px); float: right; border: 1px solid #ddd; background-color: #fff; overflow: auto; padding: 10px">
                    <div class="input-control text full-size ">
                        <label class="input-control checkbox small-check">
                            <input type="checkbox" id="chkMutlipleQty"/>
                            <span class="check"></span>
                            <span class="caption"></span>
                        </label>
                    </div>
                    <div style="border: 1px solid #ddd;display:none" class="padding10" id="dvQtyForm">
                        <div id="lblItem" style="font-size:18px;font-weight:bold">Item</div>
                        <div class="row cells2">
                            <div class="cell">
                                <div class="input-control text full-size ">
                                    Quantity : 
                                <input id="txtQty" value="1" />
                                </div>
                            </div>
                            <div class="cell">
                                <div class="input-control text full-size ">
                                    Price : 
                                <input id="txtPrice" value="0.00" />
                                </div>
                            </div>
                        </div>
                        <hr class="thin bg-lightGray" />
                        <div class="place-right">
                            <button class="button rounded success" onclick="selectItemSave()">Save</button>
                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <div id="dvItem"></div>
                </div>
            </div>

        </div>

    </div>

</asp:Content>