<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="itemLayout.aspx.cs" Inherits="X_CRM.item.itemLayout" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("itemLayout.aspx");

        });

        function findRecord(frm, screen, cPage) {
            $.ajax({
                url: "",
                data: "app=getData&warehouse=" + $("#itwh_WarehouseID").val() +
                    "&layout1=" + $("#itwh_ItemLayout1").val(),
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
                    $("#dvList").html(data);
                }
            });
        }

        function itemDetail(id) {
            $.ajax({
                url: "",
                data: "app=getItem&id=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    metroDialog.open('#dvLoadingOut');
                },
                success: function (data) {
                    $("#returnBtn").show();
                    metroDialog.close('#dvLoadingOut');
                    $("#dvPopContent").html(data);
                }
            });
            
            metroDialog.open('#dialog');
        }

        function addItem(id) {
            $.ajax({
                url: "",
                data: "app=addtItem&id=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    metroDialog.open('#dvLoadingOut');
                },
                success: function (data) {
                    $("#returnBtn").hide();
                    metroDialog.close('#dvLoadingOut');
                    $("#dvPopContent").html(data);
                    initComp("itemLayout.aspx");

                    $("#frmAddItem").find("#btSave").removeAttr("onclick");
                    $("#frmAddItem").find("#btCancel").removeAttr("onclick");

                    $("#frmAddItem").find("#btCancel").click(function (e) {
                        $(".notify-container").html("");
                        metroDialog.close('#dialog');
                    });

                    $("#frmAddItem").find("#btSave").click(function (e) {
                        saveItem();
                        
                    });

                }
            });

            metroDialog.open('#dialog');
        }

        function saveItem() {
            
            $.ajax({
                url: "",
                data: "app=saveItem&" + $("#frmAddItem").serialize(),
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    //metroDialog.open('#dvLoadingOut');
                },
                success: function (data) {
                    var re = data
                    setError(data, "frmAddItem");
                    if (re != "error") {
                        re = $.parseJSON(data);
                        re = re["tbl"];
                        if (re[0].status == "ok") {
                            metroDialog.close('#dialog');

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
            <div id="dvPopContent" style="max-height: 500px;min-height:500px; overflow: auto; padding: 20px;">
            </div>
        </div>

        <div class="align-right padding20" style="padding-top: 0px" id="returnBtn">
            
            <button type="button" id="btCancel" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Close</span>
            </button>
        </div>
    </div>

</asp:Content>
