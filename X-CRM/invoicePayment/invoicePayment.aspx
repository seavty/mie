<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="invoicePayment.aspx.cs" Inherits="X_CRM.invoicePayment.invoicePayment" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            initComp("");
            $("#frmMaster").append($("#dvList"));
        });


        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    
                    window.location = "../invoice/invoicePaymentList.aspx?invo_invoiceid=" + $("#ivpm_InvoiceID").val();
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
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
                var tmp = $("#ivpm_InvoiceID").val();
                var re = _delRecord(frm, screen, eid, "", "");
                if (re.substring(0,2) == "ok") {
                    window.location = "../invoice/invoicePaymentList.aspx?invo_invoiceid=" +
                         tmp;
                } else {
                    alert("Error Deleting Record !\n" + re);
                }
            }
		}

		function printInv(id) {
             //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
             window.open("../report/report.aspx?report=rptOfficialReceipt&pid=" + id, "_blank");
		}



	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
