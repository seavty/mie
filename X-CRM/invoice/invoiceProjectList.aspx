<%@ Page Title="" Language="C#" AutoEventWireup="true" MasterPageFile="~/main.Master" CodeBehind="invoiceProjectList.aspx.cs" Inherits="X_CRM.invoice.projectList" %>

<asp:content id="HeaderContent" runat="server" contentplaceholderid="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("");
			$("#btFind").click();
			$("table > tfoot").hide();
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", -1, "");

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
            _findRecord(frm, screen, "", -1, $(v).closest("div").attr("id"));
			$("table > tfoot").remove();
        }



	</script>

</asp:content>

<asp:content id="BodyContent" runat="server" contentplaceholderid="mainContent" clientidmode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>


</asp:content>
