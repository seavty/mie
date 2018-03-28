<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="workflow.aspx.cs" Inherits="X_CRM.invoice.workflow" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            initComp("", "");
            $("#frmMaster").append($("#dvList"));
            event();
        });

        function saveRecord(frm, screen) {
            $("#<%=dvList.ClientID%>").show();
            $("#<%=dvList.ClientID%>").html("Processing ... ");
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "invoice.aspx?invo_invoiceid=" + re[0].msg;
                } else {
                    
                    $("#<%=dvList.ClientID%>").html(re[0].msg);
                }
            }
        }

        function loadScreen(screen, mode, eid) { // 3 edit // 2 View
            _loadScreen(screen, mode, eid, "dvContent", "");
            $("#frmMaster").append($("#dvList"));
            event();
        }

        function event() {

            $("#btCancel").removeAttr("onclick");
            $("#btCancel").click(function (e) {
                window.location = "invoice.aspx?invo_invoiceid=" + getUrlVars(window.location.href)["eid"];

            });
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    
    <div id="dvList" class="padding10" runat="server" style="border:2px solid #e8f1f4; background-color: #fff;display:none"></div>
</asp:Content>
