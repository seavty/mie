<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="payment.aspx.cs" Inherits="X_CRM.apInvoicePayment.payment" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("");
            $("#btFind").click();
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", cPage, "");
        }

        function sortClick(frm, screen, col, v) {

            $(v).closest("div").find("#orderFieldBy").val(col);
            var orderBy = "asc";
            if ($(v).hasClass("sort-asc")) {
                orderBy = "desc";
            } else {
                orderBy = "asc";
            }
            $(v).closest("div").find("#orderBy").val(orderBy);
            _findRecord(frm, screen, "", 1, $(v).closest("div").attr("id"));
        }

        function apiv_APInvoiceID_Select(t, id) {
            payment(id);
        }


        function payment(id) {
            if (id == 0 && ($("#apiv_SupplierID").val() == "" || $("#apiv_SupplierID").val() == null)) {
                alert("Please select Supplier !");
                return;
            }
            $.ajax({
                url: "../apinvoice/payment.aspx",
                data: "app=loadScreen&apiv_apinvoiceid=" + id + "&supp_supplierid=" + $("#apiv_SupplierID").val(),
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
                url: "../apinvoice/payment.aspx",
                data: "app=saveRecord&apiv_apinvoiceid=" + id + "&supp_supplierid=" + $("#apiv_SupplierID").val() +
                    "&" + $("#frmPayment").serialize(),
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
    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>



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
