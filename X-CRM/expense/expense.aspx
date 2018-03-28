<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="expense.aspx.cs" Inherits="X_CRM.expense.expense" %>

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
                    window.location = "expense.aspx?exps_expenseid=" + re[0].msg;

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
                var re = _delRecord(frm, screen, eid, "", "");
                if (re.substring(0, 2) == "ok") {
                    var myRe = re.replace("ok", "");
                    //window.location = "expenseList.aspx";
                    window.location = "../opportunity/expenseList.aspx?oppo_opportunityid=" + myRe;
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function Back(id) {
            window.location = "../opportunity/expenseList.aspx?oppo_opportunityid=" + id;
        }



    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
