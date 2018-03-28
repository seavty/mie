<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="journal.aspx.cs" Inherits="X_CRM.accounting.journal" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            $("#frmMaster").append($("#dvList"));
            event();
            //$("#joud_Transaction").change();
            //disableInput();
        });

        function savePreLine() {

            var n = "";
            if ($(this).closest("tr").attr("pos") != null) {
                n = $(this).closest("tr").attr("pos");
            }

            if ($("#joud_ChartAccountID" + n).val() == null) {

                alert('Account No. is required !');
                return false;
            }

            var debit = $("#joud_Debit" + n).val();
            var credit = $("#joud_Credit" + n).val();
            if (isNaN(debit) || debit == "") debit = 0;
            if (isNaN(credit) || credit == "") credit = 0;
            
            if (parseFloat(debit) + parseFloat(credit) <= 0) {
                alert('Debit / Credit is required !');
                return false;
            }

            var re = _savePreLine("../include.aspx", "tblJournalDetailNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            event();
            //calTotal();
            //$("#joud_Transaction").change();
            //disableInput();
        }

        function delLine(pos, v) {
            _delLine(pos, v);
            //calTotal();
            //$("#joud_Transaction").change();
            //disableInput();
        }

        function saveRecord(frm, screen) {

            if (calIsBalanced() == false) {

                alert('Transaction is unbalance ! Please recheck !');
                return false;
            }

            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "journal.aspx?jour_journalid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                }
                else {
                    alert(re[0].msg);
                }
            }
        }

        function loadScreen(screen, mode, eid) { // 3 edit // 2 View
            _loadScreen(screen, mode, eid, "dvContent", "");
            $("#frmMaster").append($("#dvList"));
            event();
            //$("#joud_Transaction").change();
            //disableInput();
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
                var re = _delRecord(frm, screen, eid, "", "");
                if (re == "ok") {
                    window.location = "journalList.aspx";
                }
                else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {

            initComp("journal.aspx");

            $(".joud_ChartAccountID").change(function (e) {

                var n = "";
                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");
                }

                $.ajax({
                    url: "../querryData/querryData.aspx",
                    data: "app=getChartAccount&chac_chartaccountid=" + $(this).val(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        try {
                            data = $.parseJSON(data);
                            if (data != "") {
                                //$("#quot_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                                //$("#quot_CustomerID").val(data[0].cust_CustomerID).trigger("change");
                                $("#lbljoud_CAName" + n).text(data[0].chac_Name);
                                $("#joud_CAName" + n).val(data[0].chac_Name);
                            }
                            else {
                                $("#lbljoud_CAName" + n).text("");
                                $("#joud_CAName" + n).val("");
                            }
                        }
                        catch (e) { }
                    }
                });

            });

            $(".joud_Credit").keydown(function (e) {

                var n = "";
                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");
                }

                if ($("#joud_Debit" + n).val() != "")
                    $("#joud_Debit" + n).val("");


                var dc = calCountDC();
                var debit = dc[0];
                var credit = dc[1];

                if (debit > 1)    //Many Debits
                {
                    if (credit > 0)   //Credit already had
                    {
                        if ($("#joud_Credit" + n).val() != "") {
                            alert('Credit already putted !');
                            $("#joud_Credit" + n).val("");
                        }
                            return false;
                        
                    }
                }


                //alert(calCountDC()[0] + ', ' + calCountDC()[1]);

            });

            $(".joud_Debit").keydown(function (e) {

                var n = "";
                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");
                }

                if ($("#joud_Credit" + n).val() != "")
                    $("#joud_Credit" + n).val("");

                var dc = calCountDC();
                var debit = dc[0];
                var credit = dc[1];

                if (credit > 1)    //Many Credit
                {
                    if (debit > 0)   //Debit already had
                    {
                        if ($("#joud_Debit" + n).val() != "") {
                            alert('Debit already putted !');
                            $("#joud_Debit" + n).val("");
                        }
                        return false;

                    }
                }

            });


            //$(".joud_Credit").change(function (e) {

            //    var n = "";
            //    if ($(this).closest("tr").attr("pos") != null) {
            //        n = $(this).closest("tr").attr("pos");
            //    }

            //    var credit = $("#joud_Credit" + n).val().replace(/,/g, '');
            //    if (isNaN(credit)) credit = 0;
            //    if(credit>0)
            //        $("#joud_Debit" + n).val("");

            //});

            //$(".joud_Debit").change(function (e) {

            //    var n = "";
            //    if ($(this).closest("tr").attr("pos") != null) {
            //        n = $(this).closest("tr").attr("pos");
            //    }

            //    var debit = $("#joud_Debit" + n).val().replace(/,/g, '');
            //    if (isNaN(debit)) debit = 0;
            //    if (debit > 0)
            //        $("#joud_Credit" + n).val("");

            //});


            //$(".databind").on("change", function (e) {
            //    if ($(e.target).attr("id") == "quot_OpportunityID") {
            //        if ($("#quot_OpportunityID").val() != null) {

            //            $.ajax({
            //                url: "../opportunity/opportunity.aspx",
            //                data: "app=getOppo&eid=" + $("#quot_OpportunityID").val(),
            //                type: "POST",
            //                async: false,
            //                error: function () {
            //                    return "error";
            //                },
            //                beforeSend: function () {
            //                },
            //                success: function (data) {
            //                    data = $.parseJSON(data);
            //                    $("#quot_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
            //                    $("#quot_CustomerID").val(data[0].cust_CustomerID).trigger("change");


            //                }
            //            });
            //        }
            //    }
            //    if ($(e.target).hasClass("quit_ItemID")) {
            //        var n = "";
            //        var t = e.target;
            //        if ($(t).closest("tr").attr("pos") != null) {
            //            n = $(t).closest("tr").attr("pos");
            //        }
            //        if ($("#quit_ItemID" + n).val() != null) {
            //            $.ajax({
            //                url: "../shared/shared.aspx",
            //                data: "app=getItem&eid=" + $("#quit_ItemID" + n).val(),
            //                type: "POST",
            //                async: false,
            //                error: function () {
            //                    return "error";
            //                },
            //                beforeSend: function () {
            //                },
            //                success: function (data) {
            //                    data = $.parseJSON(data);
            //                    $("#quit_Description" + n).val(data[0].item_Name);
            //                    $("#quit_Qty" + n).val(1);
            //                    $("#quit_Price" + n).val(data[0].item_Price);
            //                    $("#quit_Price" + n).change();
            //                    calTotal();
            //                }
            //            });
            //        }
            //    }

            //});


            //$("#quot_Discount,#quot_Disc").unbind("change");
            //$(".quit_Qty,.quit_Price").unbind("change");
            ////$(".quit_ItemID").unbind("change");



            //$(".quit_Qty,.quit_Price").change(function (e) {
            //    var n = "";
            //    if ($(this).closest("tr").attr("pos") != null) {
            //        n = $(this).closest("tr").attr("pos");
            //    }

            //    var qty = $("#quit_Qty" + n).val();
            //    var price = $("#quit_Price" + n).val();
            //    if (isNaN(qty) || qty == "") {
            //        qty = 0;
            //    }
            //    if (isNaN(price) || price == "") {
            //        price = 0;
            //    }
            //    var total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);
            //    $("#lblquit_Total" + n).text(total);
            //    $("#quit_Total" + n).val(total);
            //    calTotal();
            //});


            //$("#quot_Discount,#quot_Disc").change(function (e) {
            //    calTotal();
            //});

        }

        //function disableInput() {

        //    $.each($(".lbljoud_CAName"), function (i, v) {
        //        var n = "";
        //        if ($(v).closest("tr").attr("pos") != null) {
        //            n = $(v).closest("tr").attr("pos");
        //        }
        //        //if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "")
        //        {
        //            var credit = $("#joud_Credit" + n).val().replace(/,/g, '');
        //            var debit = $("#joud_Debit" + n).val().replace(/,/g, '');
        //            if (isNaN(credit)) credit = 0;
        //            if (isNaN(debit)) debit = 0;

        //            if (credit > 0 && debit <= 0) {
        //                $("#joud_Credit" + n).prop("disabled", false);
        //                $("#joud_Debit" + n).prop("disabled", true);
        //            }
        //            else if (debit > 0 && credit <= 0) {
        //                $("#joud_Credit" + n).prop("disabled", true);
        //                $("#joud_Debit" + n).prop("disabled", false);
        //            }
        //            else {
        //                $("#joud_Credit" + n).prop("disabled", false);
        //                $("#joud_Debit" + n).prop("disabled", false);
        //            }
        //        }

        //    });

        //}

        //function calTotal() {
        //    var quot_SubTotal = 0;
        //    var quot_Disc = $("#quot_Disc").val();
        //    var quot_DiscountAmount = 0;
        //    var quot_Total = 0;
        //    if (isNaN(quot_Disc) || quot_Disc == "") {
        //        quot_Disc = 0;
        //    }
        //    $.each($(".lblquit_Total"), function (i, v) {
        //        var n = "";
        //        if ($(v).closest("tr").attr("pos") != null) {
        //            n = $(v).closest("tr").attr("pos");
        //        }
        //        if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
        //            var quit_Total = $(v).text().replace(/,/g, '');
        //            if (isNaN(quit_Total) || quit_Total == "") {
        //                quit_Total = 0;
        //            }
        //            quot_SubTotal = parseFloat(quot_SubTotal) + parseFloat(quit_Total);
        //        }
        //    });

        //    if ($("#quot_Discount").val() == "P") {
        //        quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
        //    } else {
        //        quot_DiscountAmount = parseFloat(quot_Disc);
        //    }

        //    quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

        //    $("#lblquot_SubTotal").text(quot_SubTotal.toFixed(numFormat));
        //    $("#lblquot_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
        //    $("#lblquot_Total").text(quot_Total.toFixed(numFormat));
        //}

        //function convertSO(id) {
        //    window.location = "../saleorder/saleorder.aspx?quot_quotationid=" + id;
        //}

        //function convertInv(id) {
        //    window.location = "../invoice/invoice.aspx?quot_quotationid=" + id;
        //}


        function calCountDC() {
            var isBalanced = true;
            var debit = 0;
            var credit = 0;

            $.each($(".joud_Debit"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {

                    var tmpDebit = $(v).val().replace(/,/g, '');
                    if (isNaN(tmpDebit) || tmpDebit == "") tmpDebit = 0;
                    if (parseInt(tmpDebit) > 0)
                        debit += 1;
                }
            });

            $.each($(".joud_Credit"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {

                    var tmpCredit = $(v).val().replace(/,/g, '');
                    if (isNaN(tmpCredit) || tmpCredit == "") tmpCredit = 0;
                    if (parseInt(tmpCredit) > 0)
                        credit += 1;
                }
            });

            if (parseInt(debit) > 1 && parseInt(credit) > 1)
                isBalanced = false;

            //alert('debit=' + debit + ', credit=' + credit);
            var dc = new Array(debit, credit);
            return dc;
        }

        function calIsBalanced() {
            var isBalanced = false;
            var debit = 0;
            var credit = 0;

            $.each($(".joud_Debit"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {

                    var tmpDebit = $(v).val().replace(/,/g, '');
                    if (isNaN(tmpDebit) || tmpDebit == "") tmpDebit = 0;
                    debit = parseFloat(debit) + parseFloat(tmpDebit);
                }
            });

            $.each($(".joud_Credit"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
                if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {

                    var tmpCredit = $(v).val().replace(/,/g, '');
                    if (isNaN(tmpCredit) || tmpCredit == "") tmpCredit = 0;
                    credit = parseFloat(credit) + parseFloat(tmpCredit);
                }
            });

            if (parseFloat(debit) == parseFloat(credit))
                isBalanced = true;

            return isBalanced;
        }


    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>

