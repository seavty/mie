<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="invoiceList.aspx.cs" Inherits="X_CRM.reportList.invoiceList" %>

<asp:content id="HeaderContent" runat="server" contentplaceholderid="headContent">
	<script>
		$(document).ready(function (e) {
			$("#frmMaster").append($("#dvList"));
			initComp("");

			var rid = getUrlVars("<%=HttpUtility.UrlDecode(HttpContext.Current.Request.Url.AbsoluteUri.ToString())%>")["invoice"];
			//alert(rid);
			if ($("h1").hasClass("text-light"))
				$("h1").text("Report");
			$("#btFind").hide();
        });

		function printInv(id) {
             //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
             window.open("../report/report.aspx?report=rptInvoiceReport&pid=" + id, "_blank");
        }

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
	</script>

</asp:content>

<asp:content id="BodyContent" runat="server" contentplaceholderid="mainContent" clientidmode="Static">
	<div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>
</asp:content>
