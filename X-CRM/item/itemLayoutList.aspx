<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="itemLayoutList.aspx.cs" Inherits="X_CRM.item.itemLayoutList" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("");
            $("#btFind").click();
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", -1, "");
            $("#dvList>table>tbody>tr").each(function (i, v) {
                var tmp = $(v).find(">td:last>a");
                tmp.attr("href", "javascript:addItem(" + tmp.attr("eid") + ")");
            });
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

        }

        ///////////////////////////////////

        function delItem(id) {
            if (confirm("Delete Current Record?")) {
                $.ajax({
                    url: "",
                    data: "app=delItem&eid=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {


                    },
                    success: function (data) {
                        if (data == "ok") {
                            metroDialog.close("#dialog");
                            $("#btFind").click();
                        } else {
                            alert(data);
                        }
                    }

                });
            }
        }


        function addItem(id) {

            $.ajax({
                url: "",
                data: "app=newItem&id=" + "&eid=" + id + "&item_itemid=" + getUrlVars(window.location.href)["item_itemid"],
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvPopContent").html(data);
                    initComp("");
                    $("#frmAddItem").find("#btCancel").removeAttr("onclick");
                    $("#frmAddItem").find("#btSave").removeAttr("onclick");

                    $("#frmAddItem").find("#btCancel").click(function (e) {
                        metroDialog.close("#dialog");
                        $(".notify-container").html("");
                    });
                    var _eid = $("#plit_PriceListID").val();
                    $("#frmAddItem").append("<input type='hidden' id='txt_eid' name='txt_eid' value='" + _eid + "'/>");

                    $("#frmAddItem").find("#btSave").click(function (e) {

                        var re = "";
                        $(".notify-container").html("");
                        $.ajax({
                            url: "",
                            data: "app=saveItem&screen=tblPriceListItemNew&" + $(".frmAddItem").serialize(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {


                            },
                            success: function (data) {
                                re = setError(data, "frmAddItem");

                                if (re != "error") {
                                    if (re[0].status == "ok") {
                                        metroDialog.close("#dialog");
                                        $(".notify-container").html("");
                                        $("#btFind").click();
                                    } else {
                                        alert(re[0].msg);
                                    }
                                }
                            }
                        });

                    });

                    metroDialog.open("#dialog");

                }
            });
        }
    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>

    <div data-role="dialog" class="panel dialog" id="dialog"
        data-close-button="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <!--<div class="heading bg-dark">
            <span class="title">Add Expience</span>
        </div>-->
        <div class="">
            <div id="dvPopContent" style="max-height: 600px; overflow: auto; padding: 20px; width: 800px;">

            </div>
        </div>

    </div>
</asp:Content>