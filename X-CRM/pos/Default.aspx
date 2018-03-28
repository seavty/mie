<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="POS.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>X-POS</title>

    <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
    <meta http-equiv="Pragma" content="no-cache" />
    <meta http-equiv="Expires" content="0" />

    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <link href="../Scripts/css/metro.min.css" rel="stylesheet" />
    <link href="../Scripts/css/metro-icons.min.css" rel="stylesheet" />
    <link href="../Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="../Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="../Scripts/css/metro-colors.min.css" rel="stylesheet" />
    <script src="../Scripts/js/jquery.num.js"></script>

    <script src="<%=url%>Scripts/jquery_min.js"></script>
    <script src="<%=url%>Scripts/js/metro.js"></script>
    <script src="<%=url%>Scripts/js/select2.min.js"></script>
    <script src="<%=url%>Scripts/js/jquery.num.js"></script>
    <!-- #include file ="../includejs.aspx" -->

    <style>
        .itemGroup {
            height: 60px;
        }

        .item {
            height: 100px;
        }
    </style>

    <script>

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", cPage, "dvDiaList");
        }

        function cust_CustomerID_Select(t, id) {

            $("#invo_CustomerID").val(id);
            metroDialog.close('#diaList');
            $("#lblinvo_Customer").val("");
            $("#lblCustomer").text($(t).closest("tr").find("td:first").text());
        }

        $(document).ready(function (e) {

            $("#lblinvo_Customer").keypress(function (event) {
                console.log(event.keyCode + "; " + event.which);
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    $("input[name='q']").val($("#lblinvo_Customer").val());
                    $("input[name='q']").attr("app", "customer");
                    metroDialog.open('#diaList');

                    $.ajax({
                        url: "",
                        data: "app=getCustomer&q=" + $("#lblinvo_Customer").val(),
                        type: "POST",
                        async: false,
                        error: function () {
                            return "error";
                        },
                        beforeSend: function () {
                            $("#dvDiaList").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                        },
                        success: function (data) {
                            $("#dvDiaList").html(data);
                        }
                    });
                }


            });

            $("input[name='q']").keypress(function (event) {
                
                var keycode = (event.keyCode ? event.keyCode : event.which);
                if (keycode == '13') {
                    event.preventDefault();
                    if ($(this).attr("app") == "item") {
                        findRecord("frmList", "tblItemFind", "1")
                    }
                    if ($(this).attr("app") == "customer") {
                        findRecord("frmList", "tblCustomerFind", "1")
                    }
                }
            });

            $("#lblitem_ItemID").keypress(function (event) {
                itemKeyPress(event);
            });
            //$("#tblInvoiceItem>tbody").height(300);
            var wh = ($(window).height());
            //debugger;
            _newInvoice();
            $("#tblInvoiceItem>tbody").css("height", (wh - 400) + "px");

            $(".itemGroup").click(function (e) {
                //debugger;
                $(".itemGroup").removeClass('element-selected');
                $(this).addClass('element-selected');
                //alert($(this).attr("eid"));
                $.ajax({
                    url: "",
                    data: "app=getItem&itmg_ItemGroupID=" + $(this).attr("eid"),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        $("#<%=dvItem.ClientID%>").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                    },
                    success: function (data) {

                        $("#<%=dvItem.ClientID%>").html(data);

                        $(".item").unbind("click");
                        $(".item").click(function (e) {
                            var item_Name = ($(this).find("span.tile-label").text());
                            var item_Price = ($(this).find("span#lblPrice").text());
                            var item_ItemID = $(this).attr("eid");

                            addItemToRow(item_ItemID, item_Name, item_Price);


                            $(".tblInvoiceItemRow").unbind("click");
                            $(".tblInvoiceItemRow").click(function (e) {

                                //$.each(e, function(i, v){
                                //    alert(i + "; " + v);

                                //});
                                var n = ($(this).closest("tr").attr("eid"));
                                //alert(n);
                                $("#init_EDescription").text($("input[name='init_Description" + n + "']").val());
                                $("#init_EQty").val($("input[name='init_Qty" + n + "']").val());
                                $("#init_EPrice").val($("input[name='init_Price" + n + "']").val());
                                $("#init_ETotal").text($("input[name='init_Total" + n + "']").val());

                                $("#hidItemID").val(n);
                                metroDialog.open("#diaEdit");
                            });

                        });


                    }
                });
            });

            $("#btDiscOK").click(function (e) {
                $("#invo_Disc").val($("#txtInvo_Disc").val());
                $("#invo_Discount").val($("input[name='rdDiscount']:checked").val());
                calTotal();
                metroDialog.close('#diaDiscount');
            });

            $("#btDiscCancel").click(function (e) {
                metroDialog.close('#diaDiscount');
            });

            $("#btBillCancel").click(function (e) {
                metroDialog.close('#dialog');
                metroDialog.close('#diaMsg');
            });

            $("#btBill").click(function (e) {

                $.ajax({
                    url: "",
                    data: "app=bill&n=" + $("#tblInvoiceItem>tbody>tr").length +
                        "&invo_cashin=" + $("#txtCashIn").val() +
                        "&invo_cashin2=" + $("#txtCashIn2").val() +
                        "&" + $("#frm").serialize(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        //$("#<%=dvItem.ClientID%>").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                    },
                    success: function (data) {

                        try {
                            data = $.parseJSON(data);
                            data = data["tbl"];
                            if (data[0].status == "ok") {
                                _newInvoice();
                                metroDialog.close('#dialog');
                                metroDialog.close('#diaMsg');
                            } else {
                                $("#dvDiaMsg").html(data[0].msg);
                                metroDialog.open('#diaMsg');
                            }
                        } catch (e) { alert(data); }
                    }
                })
            });
        });

        //-------- function ---------/
        function item_ItemID_Select(t, id) {
            var item_Name = t.closest("tr").find("td:first").text();
            var item_Price = t.closest("tr").find("td:eq(1)").text();
            var item_ItemID = id;
            addItemToRow(item_ItemID, item_Name, item_Price);

        }

        function addItemToRow(item_ItemID, item_Name, item_Price) {
            item_Price = parseFloat(item_Price).toFixed(numFormat);
            var n = $("#tblInvoiceItem>tbody>tr").length;
            $("#tblInvoiceItem>tbody").append('<tr class="" eid="' + n + '">' +
                '<td style="width: 50px" class="tblInvoiceItemRow lblinit_Qty">' +
                    '<span  id="lbl_init_Qty' + n + '" name="lbl_init_Qty' + n + '">1 </span>' +
                    '<input type="hidden" id="init_Qty' + n + '" name="init_Qty' + n + '" value="1"/>' +
                    '<input type="hidden" name="init_ItemID' + n + '" value="' + item_ItemID + '"/>' +
                    '<input type="hidden" class="txtDel" name="txtDel' + n + '" value=""/>' +
                '</td>' +
                '<td style="width: 210px" class="lblinit_Description tblInvoiceItemRow">' +
                    item_Name +
                    '<input type="hidden" id="init_Description' + n + '" name="init_Description' + n + '" value="' + item_Name + '"/>' +
                '</td>' +
                '<td style="width: 80px" class="align-right lblinit_Price tblInvoiceItemRow">' +
                    '<span  id="lbl_init_Price' + n + '" name="lbl_init_Price' + n + '">' + item_Price + '</span>' +
                    '<input type="hidden" id="init_Price' + n + '" name="init_Price' + n + '" value="' + item_Price + '"/>' +
                '</td>' +
                '<td style="width: 90px" class="align-right lblinit_Total tblInvoiceItemRow">' +
                     '<span  id="lbl_init_Total' + n + '" name="lbl_init_Total' + n + '">' + item_Price + '</span>' +
                    '<input type="hidden" id="init_Total' + n + '" name="init_Total' + n + '" value="' + item_Price + '"/>' +
                '</td>' +
                '<td style="width: 50px;padding:0px;" class="align-center">' +
                    '<a href="javascript:void(0);" onclick="delLine($(this))" class="fg-red">' +
                        '<span class="mif-cancel"></span>' +
                    '</a>' +
                '</td>' +
            '</tr>');
            calTotal();

            metroDialog.close('#diaList');
        }

        function itemKeyPress(event) {

            var keycode = (event.keyCode ? event.keyCode : event.which);
            if (keycode == '13') {
                $("input[name='q']").attr("app", "item");

                $.ajax({
                    url: "",
                    data: "app=scanItem&q=" + $("#lblitem_ItemID").val(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        
                    },
                    success: function (data) {
                        if (data == "[]") {
                            $("input[name='q']").val($("#lblitem_ItemID").val());
                            metroDialog.open('#diaList');
                            $.ajax({
                                url: "",
                                data: "app=getItemToList&q=" + $("#lblitem_ItemID").val(),
                                type: "POST",
                                async: false,
                                error: function () {
                                    return "error";
                                },
                                beforeSend: function () {
                                    $("#dvDiaList").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                                },
                                success: function (data) {
                                    $("#dvDiaList").html(data);
                                }
                            });
                        } else {
                            var re = $.parseJSON(data);
                            //alert(re[0].item_ItemID);
                            var item_Name = re[0].item_Name;
                            var item_Price = re[0].item_Price
                            var item_ItemID = re[0].item_ItemID;
                            $("#lblitem_ItemID").val("");
                            addItemToRow(item_ItemID, item_Name, item_Price);
                            
                        }
                    }
                });

                
            }

        }

        function calTotal() {
            var invo_SubTotal = 0;
            var invo_DisType = "";
            var invo_Disc = 0;
            var invo_DiscAmount = 0;
            var invo_Total = 0;
            var invo_isTax = $("#invo_isTax").val();
            var invo_Tax = 0;
            var invo_GTotal = 0;

            invo_DisType = $("#invo_Discount").val();
            invo_Disc = $("#invo_Disc").val();

            if (!isNaN(invo_isTax) && invo_isTax == "") {
                invo_isTax = 0;
            }
            if (!isNaN(invo_Disc) && invo_Disc == "") {
                invo_Disc = 0;
            }
            $.each($(".lblinit_Total"), function (i, v) {
                if ($(v).closest("tr").find(".txtDel").val() == "") {
                    var init_Total = $(v).text().replace('$', '').replace(/,/g, '');
                    if (!isNaN(init_Total) && init_Total == "") {
                        init_Total = 0;
                    }
                    invo_SubTotal = parseFloat(invo_SubTotal) + parseFloat(init_Total);
                }
            });

            if (invo_DisType == "P") {
                invo_DiscAmount = parseFloat(invo_SubTotal) / 100 * parseFloat(invo_Disc);
            } else {
                invo_DiscAmount = parseFloat(invo_Disc);
            }

            invo_Total = invo_SubTotal - invo_DiscAmount;

            invo_Tax = parseFloat(invo_Total) * parseFloat(invo_isTax) / 100;
            invo_GTotal = (parseFloat(invo_Tax) + parseFloat(invo_Total))

            $("#lblinvo_SubTotal").text("$" + parseFloat(invo_SubTotal).toFixed(numFormat));
            $("#lblinvo_DiscountAmount").text("$" + parseFloat(invo_DiscAmount).toFixed(numFormat));
            $("#lblinvo_Total").text("$" + parseFloat(invo_Total).toFixed(numFormat));
            $("#lblinvo_Tax").text("$" + parseFloat(invo_Tax).toFixed(numFormat));
            $("#lblinvo_GTotal").text("$" + parseFloat(invo_GTotal).toFixed(numFormat));
            $("#lblitem_ItemID").focus();
        }

        function delLine(v) {
            if (confirm("Delete Current Record ?")) {
                $(v).closest("tr").find(".txtDel").val(1);
                $(v).closest("tr").hide();
                calTotal();
                eOKClick();
            }

            //$(v).preventDefault();
        }

        function newInvoice() {

            if (confirm("Cancel current transaction ?")) {
                _newInvoice();
            }
        }

        function _newInvoice() {
            $("#invo_CustomerID").val($("#defCust").val());
            $("#lblCustomer").text($("#defCustName").val());
            $("#invo_Discount").val("");
            $("#invo_Disc").val("0");
            $("#tblInvoiceItem>tbody").html("");
            calTotal();
            $("#lblitem_ItemID").focus();
        }

        function discount() {
            if ($("#invo_Discount").val() == "P") {
                $("#rdDiscountP").prop("checked", true)
            } else {
                $("#rdDiscountA").prop("checked", true)
            }
            $("#txtInvo_Disc").val($("#invo_Disc").val());
            metroDialog.open("#diaDiscount");
        }

        function bill() {
            $("#lblTotal").text($("#lblinvo_Total").text());
            $("#lblTax").text($("#lblinvo_Tax").text());
            $("#lblGrandTotal").text($("#lblinvo_GTotal").text());
            $("#txtCashIn").val($("#lblinvo_GTotal").text().replace('$', '').replace(/,/g, ''));
            $("#txtCashIn2").val("0.00");
            metroDialog.open('#dialog');
            $("#txtCashIn").focus();
            $("#txtCashIn").select();
            $("#lblChange").text("0.00");

            $("#txtCashIn").unbind("change");
            $("#txtCashIn").change(function (e) {
                var lblGrandTotal = $("#lblGrandTotal").text().replace('$', '').replace(/,/g, '');
                var txtCashIn = $("#txtCashIn").val().replace(/,/g, '');
                var exRate = $("#<%=txtExRate.ClientID%>").val();
                var txtCashIn2 = 0;
                var change = 0;

                if (!isNaN(txtCashIn) && txtCashIn == "") {
                    txtCashIn = 0;
                }
                if (parseFloat(lblGrandTotal) > parseFloat(txtCashIn))
                    txtCashIn2 = ((parseFloat(lblGrandTotal) - parseFloat(txtCashIn)) * parseFloat(exRate));
                

                $("#txtCashIn2").val(txtCashIn2);

                change = (parseFloat(txtCashIn) + (parseFloat(txtCashIn2) / parseFloat(exRate)) - lblGrandTotal);
                $("#lblChange").text(change.toFixed(numFormat));
            });

            $("#txtCashIn2").unbind("change");
            $("#txtCashIn2").change(function (e) {
                var lblGrandTotal = $("#lblGrandTotal").text().replace('$', '').replace(/,/g, '');
                var txtCashIn = $("#txtCashIn").val().replace(/,/g, '');
                var exRate = $("#<%=txtExRate.ClientID%>").val();
                var change = 0;

                if (!isNaN(txtCashIn) && txtCashIn == "") {
                    txtCashIn = 0;
                }
                var txtCashIn2 = $("#txtCashIn2").val().replace(/,/g, '');
                if (!isNaN(txtCashIn2) && txtCashIn2 == "") {
                    txtCashIn2 = 0;
                }
                change = (parseFloat(txtCashIn) + (parseFloat(txtCashIn2) / parseFloat(exRate)) - lblGrandTotal);
                $("#lblChange").text(change.toFixed(numFormat));
            });

        }

        //-- my script -----/
        //$("#init_EQty").change(function () {
        //    alert("Hello World");
        //});
        //function itemTotal() {
        //    alert();
        //}

    </script>

</head>
<body>

    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
        sapi.db db = new sapi.db();
        Dictionary<string, List<string>> strings = new Dictionary<string, List<string>>();
        try
        {
            if (db.connect())
            {
                System.Data.DataTable tblString = db.readData("Select * from sys_string");
                foreach (System.Data.DataRow rowString in tblString.Rows)
                {
                    List<string> st = new List<string>();
                    st.Add(rowString["strn_Text"].ToString());
                    st.Add(rowString["strn_Text2"].ToString());
                    if (!strings.ContainsKey(rowString["strn_Name"].ToString().ToLower()))
                        strings.Add(rowString["strn_Name"].ToString().ToLower(),st);
                }
            }
        }
        catch (Exception ex) { 
        
        }
        finally { db.close(); }
    %>
    <div class="app-bar darcula">
        <a class="app-bar-element" href="../home/Default.aspx">
            <img src="../imgs/logo.png" style="height:50px;" />
        </a>
        <span class="app-bar-divider"></span>
        <div class="app-bar-element">
            <span class="title">X-POS</span>
        </div>

        <ul class="app-bar-menu place-right">
            <li>
                <span class="app-bar-element">
                    <span class="mif-user icon"></span>
                    <span class="title" id="lblUserName" runat="server"></span>
                </span>
                <span class="app-bar-divider"></span>
            </li>
            <li>
                <a class="app-bar-element" href="<%=url%>Default.aspx?app=logout">
                    <span class="mif-enter icon"></span>
                    <span class="title">Log out
                    </span>
                </a>
            </li>
        </ul>
        
    </div>

    <div class="place-left padding10" style="width: 400px; border: 1px solid #eee;" id="dvLeft">
        <div class="grid">
            <div class="row cells2">
                <div class="cell input-control text rounded">
                    <span class="mif-search prepend-icon"></span>
                    <input type="text" placeholder="Customer" id="lblinvo_Customer" />
                </div>
                <div class="cell input-control text rounded">
                    <span class="mif-search prepend-icon"></span>
                    <input type="text" placeholder="Product" id="lblitem_ItemID" />
                </div>
            </div>
            <div style="height: 30px;">
                <h1 class="fg-indigo" id="lblCustomer"></h1>
            </div>
        </div>

        <div class="" style="">
            <form method="post" id="frm">
                <input type="hidden" id="invo_Discount" name="invo_Discount" />
                <input type="hidden" id="invo_Disc" name="invo_Disc" />
                <input type="hidden" id="invo_CustomerID" name="invo_CustomerID" />
                <table class="table striped  hovered" id="tblInvoiceItem">
                    <thead style="display: block">
                        <tr>
                            <td style="width: 50px">Qty</td>
                            <td style="width: 210px">Description</td>
                            <td style="width: 80px">Unit</td>
                            <td style="width: 90px">Total</td>
                            <td style="width: 50px"></td>
                        </tr>
                    </thead>
                    <tbody style="display: block; overflow: auto">
                    </tbody>
                </table>
            </form>
        </div>

        <hr class="bg-lightGray" />

        <input type="hidden" id="invo_isTax" runat="server" />
        <input type="hidden" id="txtExRate" runat="server" />
        <input type="hidden" id="defCust" runat="server" />
        <input type="hidden" id="defCustName" runat="server" />
        <div class="grid" style="height: 140px;">
            <div class="row cells2">
                <div class="cell">
                    <span class="text-enlarged"><%=strings["sub total"][1] %></span>
                </div>
                <div class="cell align-right">
                    <span id="lblinvo_SubTotal" class="text-enlarged text-bold">$0.00</span>
                </div>
            </div>
            <div class="row cells2">
                <div class="cell">
                    <span class="text-enlarged"><%=strings["disc"][1] %></span>
                </div>
                <div class="cell align-right">
                    <span id="lblinvo_DiscountAmount" class="text-enlarged text-bold">$0.00</span>
                </div>
            </div>
            <hr class="thin bg-lightGray" />

            <div class="row cells2">
                <div class="cell fg-blue">
                    <span class="text-enlarged "><%=strings["total"][1] %></span>
                </div>
                <div class="cell align-right">
                    <span id="lblinvo_Total" class="text-enlarged fg-blue text-bold">$0.00</span>
                </div>
            </div>
            <hr class="thin bg-lightGray" />
            <div class="row cells2">
                <div class="cell">
                    <span class="text-enlarged"><%=strings["tax"][1] %></span>
                </div>
                <div class="cell align-right">

                    <span id="lblinvo_Tax" class="text-enlarged  text-bold">$0.00</span>
                </div>
            </div>
            <div class="row cells2">
                <div class="cell fg-green">
                    <h3><%=strings["grand total"][1] %></h3>
                </div>
                <div class="cell align-right">
                    <h3 id="lblinvo_GTotal" class="fg-green text-bold">$0.00</h3>
                </div>
            </div>
        </div>
    </div>

    <!-- -->

    <div class="place-left" style="width: calc(100% - 530px); border: 1px solid #eee; height: calc(100% - 50px)">

        <div id="dvItemGroup" runat="server" class="tile-container bg-white full-size"
            style="margin: 0px; height: 160px; overflow: auto; border: 1px solid #eee">
        </div>

        <div id="dvItem" runat="server" class="frames grid full-size"
            style="margin: 0px; height: calc(100% - 160px); border: 0px solid #eee; overflow: auto;">
        </div>
    </div>

    <div class="place-right" style="width: 130px; border: 1px solid #eee; height: calc(100% - 50px)">
        <ul class="sidebar2 dark">
            <!--<li class="title">Menu</li>-->
            <li class="stick bg-lightGray">
                <a href="javascript:void(0)" onclick="newInvoice()">
                    <span class="mif-plus icon"></span><%=strings["new"][1] %>
                </a>
            </li>
            <li class="stick bg-green">
                <a href="javascript:void(0)" onclick="bill()">
                    <span class="mif-dollar2 icon"></span><%=strings["bill"][1] %>
                </a>
            </li>
            <li class="stick bg-red">
                <a href="javascript:void(0)" onclick="discount()">
                    <span class="mif-tag icon"></span><%=strings["disc"][1] %>
                </a>
            </li>
            <!--
            <li class="stick bg-blue">
                <a href="javascript:void(0)" onclick="newInvoice()">
                    <span class="mif-floppy-disk icon"></span>SAVE
                </a>
            </li>-->
        </ul>
    </div>
    <div></div>



    <div data-role="dialog" class="panel dialog" id="dialog"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="heading bg-dark">
            <span class="title"><%=strings["payment"][1] %></span>
        </div>
        <div class="padding10">
            <div class="grid" id="dvPopContent" style="height: 500px; width: 800px; overflow: auto; padding: 20px;">
                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right fg-blue text-bold"><%=strings["total"][1] %> : </h1>
                    </div>
                    <div class="cell">
                        <h1 class="align-right fg-blue text-bold" id="lblTotal" style="padding-right: 100px">100.00</h1>
                    </div>
                </div>
                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right text-bold"><%=strings["tax"][1] %> : </h1>
                    </div>
                    <div class="cell">
                        <h1 class="align-right text-bold" id="lblTax" style="padding-right: 100px">100.00</h1>
                    </div>
                </div>
                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right fg-green text-bold"><%=strings["grand total"][1] %> : </h1>
                    </div>
                    <div class="cell">
                        <h1 class="align-right fg-green text-bold" id="lblGrandTotal" style="padding-right: 100px">100.00</h1>
                    </div>
                </div>
                <hr class="bg-lightGray thin" />

                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right"><%=strings["cash in"][1] %> ($) : </h1>
                    </div>
                    <div class="cell input-control text rounded">
                        <input type="text" id="txtCashIn" name="invo_CashIn" style="font-size: 30px; margin: 0.625rem 0; height: 45px" class="align-center text-bold" />
                    </div>
                </div>

                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right"><%=strings["cash in"][1] %> (R) : </h1>
                    </div>
                    <div class="cell input-control text rounded">
                        <input type="text" id="txtCashIn2" name="invo_CashIn2" style="font-size: 30px; margin: 0.625rem 0; height: 45px" class="align-center text-bold" />
                    </div>
                </div>


                <div class="row cells2">
                    <div class="cell">
                        <h1 class="align-right fg-red"><%=strings["change"][1] %> : </h1>
                    </div>
                    <div class="cell">
                        <h1 id="lblChange" class="align-right fg-red" style="padding-right: 100px">0.00</h1>
                    </div>
                </div>
            </div>
            <div class="align-right">
                <button type="button" id="btBill" class="button success rounded">
                    <span class="mif-dollar2 icon"></span>
                    <span class="title"><%=strings["ok"][1] %></span>
                </button>
                <button type="button" id="btBillCancel" class="button danger rounded">
                    <span class="mif-cancel icon"></span>
                    <span class="title"><%=strings["cancel"][1] %></span>
                </button>
            </div>
        </div>

    </div>

    <div data-role="dialog" class="panel dialog" id="diaMsg"
        data-windows-style="true"
        data-close-button="true">
        <div class="heading bg-dark">
            <span class="title"><%=strings["information"][1] %></span>
        </div>
        <div class="padding10" id="dvDiaMsg">
        </div>

    </div>

    <div data-role="dialog" class="panel dialog" id="diaDiscount"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false"
        data-close-button="true">
        <div class="heading bg-dark">
            <span class="title"><%=strings["discount"][1] %></span>
        </div>
        <div class="padding20 grid" style="height: 200px; width: 500px; overflow: auto;">
            <div class="row cells2">
                <div class="cell">
                    <span class="title"><%=strings["discount type"][1] %> :</span>
                </div>
                <div class="cell grid">

                    <div class="row cells2">
                        <div class="cell">
                            <label class="input-control radio">
                                <input type="radio" name="rdDiscount" id="rdDiscountP" value="P" checked="checked" />
                                <span class="check"></span>
                                <span class="caption">%</span>
                            </label>
                        </div>
                        <div class="cell">
                            <label class="input-control radio">
                                <input type="radio" name="rdDiscount" id="rdDiscountA" value="" />
                                <span class="check"></span>
                                <span class="caption">$</span>
                            </label>
                        </div>
                    </div>

                </div>
            </div>
            <div class="row cells2">
                <div class="cell">
                    <span class="title"><%=strings["disc"][1] %> :</span>
                </div>
                <div class="cell input-control text rounded">
                    <input id="txtInvo_Disc" />
                </div>
            </div>

            <hr class="thin bg-lightGray" />

            <div class="place-right">
                <button class="button rounded success" id="btDiscOK">
                    <span class="mif-floppy-disk icon"></span>
                    <span class="title"><%=strings["ok"][1] %></span>
                </button>
                <button class="button rounded danger" id="btDiscCancel">
                    <span class="mif-cancel icon"></span>
                    <span class="title"><%=strings["cancel"][1] %></span>
                </button>
            </div>
        </div>

    </div>

    <div data-role="dialog" class="panel dialog" id="diaList"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false"
        data-close-button="true">
        <div class="heading bg-dark">
            <span class="title"><%=strings["search"][1] %></span>
        </div>
        <div class="padding20 grid" style="height: 500px; width: 800px; overflow: auto;">
            <div class="input-control text rounded full-size">
                <form class="frmList">
                    <input type="text" name="q" />
                </form>
            </div>
            <div id="dvDiaList">
            </div>
        </div>

    </div>

    <div data-role="dialog" class="panel dialog" id="diaEdit"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false"
        data-close-button="true">
        <div class="heading bg-dark">
            <span class="title"><%=strings["edit item"][1] %></span>
        </div>
        <div class=" padding20 grid" style="max-height: 500px; width: 500px; overflow: auto;">


            <h1 id="init_EDescription"><%=strings["item"][1] %></h1>
            <hr class="thin bg-lightGray" />
            <div class="grid">
                <div class="row cells2">
                    <div class="cell input-control text rounded">
                        <span class="title text-enlarged"><%=strings["qty"][1] %></span>
                    </div>
                    <div class="cell input-control text rounded">
                        <input type="text" placeholder="<%=strings["qty"][1] %>" id="init_EQty" class="numeric align-right" />
                    </div>
                </div>
                <div class="row cells2">
                    <div class="cell input-control text rounded">
                        <span class="title text-enlarged"><%=strings["price"][1] %></span>
                    </div>
                    <div class="cell input-control text rounded">
                        <input type="text" placeholder="Price" id="init_EPrice" class="align-right" />
                    </div>
                </div>
            </div>
            <div class="align-right">
                <h3 id="init_ETotal">100</h3>
            </div>
            <hr class="thin bg-lightGray" />
            <div class="align-right">
                <button class="button rounded success" id="btEOK">
                    <span class="mif-floppy-disk icon"></span>
                    <span class="title"><%=strings["ok"][1] %></span>
                </button>
                <button class="button rounded danger" id="btECancel" onclick='metroDialog.close("#diaEdit");'>
                    <span class="mif-cancel icon"></span>
                    <span class="title"><%=strings["cancel"][1] %></span>
                </button>
            </div>
        </div>

        <input type="hidden" name="hidItemID" id="hidItemID" value=" " />

    </div>


    <script>
        $(document).ready(function () {
            $("#init_EQty").numeric();
            $("#init_EPrice").numeric();
            $("#init_EQty,#init_EPrice").change(function () {
                itemTotal();
            });

            $("#btEOK").click(function () {
                eOKClick();
            });
        });

        function eOKClick() {
            //alert();
            var itemID = $("#hidItemID").val();

            var qty = $("#init_EQty").val();
            var price = $("#init_EPrice").val();

            var total = qty * price;

            $("#init_Qty" + itemID).val(qty);
            $("#lbl_init_Qty" + itemID).text(qty);

            $("#init_Price" + itemID).val(price);
            $("#lbl_init_Price" + itemID).text(price);


            $("#init_Total" + itemID).val(parseFloat(total).toFixed(numFormat));
            $("#lbl_init_Total" + itemID).text(parseFloat(total).toFixed(numFormat));

            calTotal();

            metroDialog.close('#diaEdit');
        }


        function itemTotal() {
            var qty = $("#init_EQty").val();
            var price = $("#init_EPrice").val();
            var total = 0;
            if (!isNaN(qty) && qty == "")
                qty = 0;

            if (!isNaN(price) && price == "")
                price = 0;

            qty = parseInt(qty);
            price = parseFloat(price).toFixed(numFormat);

            $("#init_EQty").val(qty);
            $("#init_EPrice").val(price);

            $("#init_ETotal").text(parseFloat(qty * price).toFixed(numFormat));
        }

    </script>

</body>
</html>
