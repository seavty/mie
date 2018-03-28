<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="technicianList.aspx.cs" Inherits="X_CRM.serviceOrder.technicianList" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("", "");
            $("#btFind").click();
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", cPage, "");
            customizeButton();
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

        function addTechician(serviceOrderID){
            $.ajax({
                url: "ajax.aspx",
                data: {
                    "app": "loadFormUser",
                    "serviceOrderID": serviceOrderID
                },
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#popupContent").html(data);
                    metroDialog.open('#diaPopUp');
                    $("#serviceOrderID").val(serviceOrderID);
                }
            });
        }


        function submitAdd() {
            var ids = [];
            $.each($(".userID"), function (i, v) {
                if ($(v).is(":checked")) {
                    var tmpID = $(v).attr("useridvalue");
                    ids.push(tmpID);
                }
            });

            if (ids.length == 0) {
                alert("Please select at least one technician to proceed!");
                return;
            }

            $.ajax({
                url: "ajax.aspx",
                data: {
                    "app": "submitTechnician",
                    "serviceOrderID": $("#serviceOrderID").val(),
                    "ids": ids
                },
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () { },
                success: function (data) {
                    if (data = "ok") {
                        $("#btFind").click();
                        metroDialog.close('#diaPopUp');
                    }
                }
            });
        }

        function customizeButton() {
            $("table > tbody> tr > td > a").each(function (i, v) {
                var technicianID = $(this).attr("eid");
                var myHtml = "";
                
                myHtml =
                        "<a href='javascript:removeTechnician(" + technicianID + ")' >" +
                            "<span class='mif-bin mif-lg fg-red'></span>" +
                        "</a>"
                //$(this).html(myHtml);
                $(this).closest("td").html(myHtml);

            });
        }

        function removeTechnician(technicainID) {
            if (confirm("Do you want to remove this technician?")) {
                $.ajax({
                    url: "ajax.aspx",
                    data: {
                        "app": "removeTechnician",
                        "technicianID": technicainID
                    },
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        if (data == "ok")
                            $("#btFind").click();
                    }
                });
            }
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>


    <!-- Pop Up -->
    <div data-role="dialog" class="dialog" id="diaPopUp"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false"
        data-close-button="true">
        <div class="padding20 grid" style="height: 460px; width: 700px; overflow: auto;">
            <div id="popupContent">
            </div>
            <div id="dvListPopUp" name="dvListPopUp" runat="server"></div>
        </div>

        <hr class="thin bg-lightGray" />
        <div class="align-right" style="margin-right: 10px">
            <input type="hidden" name="serviceOrderID" id="serviceOrderID" value=" " />
        </div>
        <br />
    </div>
</asp:Content>
