<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="production.aspx.cs" Inherits="X_CRM.production.productionItem" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {


            $("#frmMaster").append($("#dvList"));
            event();
        });

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblProductionItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            event();
        }

        function delLine(pos, v) {
            _delLine(pos, v);

        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {

                if (re[0].status == "ok") {
                    window.location = "production.aspx?prdt_productionid=" + re[0].msg;
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
            event();
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
                var re = _delRecord(frm, screen, eid, "", "");
                if (re == "ok") {
                    window.location = "itemList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function event() {
            initComp("");
        }

        function startProduction(id)
        {
            if(confirm("Start Production ?"))
            {
                $.ajax({
                    url: "",
                    data: "app=start&id=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        metroDialog.open('#dvLoadingOut');
                    },
                    success: function (data) {
                        metroDialog.close('#dvLoadingOut');
                        
                        var re = (setError(data, "frmShip"));
                        if (re == "error") {
                        } else {
                            if (re[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                $("#dvPopContent").html(re[0].msg);
                                metroDialog.open('#dialog');
                            }
                        }

                    }
                });
            }
        }

        function completeProduction(id) {
            if (confirm("Complete Production ?")) {
                $.ajax({
                    url: "",
                    data: "app=complete&id=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        metroDialog.open('#dvLoadingOut');
                    },
                    success: function (data) {
                        metroDialog.close('#dvLoadingOut');
                        var re = (setError(data, "frmShip"));
                        if (re == "error") {
                        } else {
                            if (re[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                $("#dvPopContent").html(re[0].msg);
                                metroDialog.open('#dialog');
                            }
                        }
                    }
                });
            }
        }

        function pcompleteProduction(id) {
            if (confirm("Partial Complete Production ?")) {
                $.ajax({
                    url: "",
                    data: "app=pcomplete&id=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        metroDialog.open('#dvLoadingOut');
                    },
                    success: function (data) {
                        metroDialog.close('#dvLoadingOut');
                        var re = (setError(data, "frmShip"));
                        if (re == "error") {
                        } else {
                            if (re[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                $("#dvPopContent").html(re[0].msg);
                                metroDialog.open('#dialog');
                            }
                        }
                    }
                });
            }
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

        <div class="align-right padding20" style="padding-top: 0px" >
            <button type="button" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">OK</span>
            </button>
        </div>
    </div>

</asp:Content>
