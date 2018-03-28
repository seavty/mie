<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="mobileSale.aspx.cs" Inherits="X_CRM.mobileSale.mobileSale1" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            initComp("");
            $("#frmMaster").append($("#dvList"));

            $("#dvSalesman>table>tbody>tr").each(function (i, v) {
                var tmp = $(v).find(">td:last>a");
                tmp.attr("href", "javascript:newSalesman(" + tmp.attr("eid") + ")");
            });

        });

        function delSalesman(id) {
            if (confirm("Remove Current Salesman?")) {
                $.ajax({
                    url: "other.aspx",
                    data: "app=delSalesman&eid=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        metroDialog.open('#dvLoadingOut');

                    },
                    success: function (data) {
                        $("#dvLoadingOut").hide();
                        if (data == "ok") {
                            metroDialog.close("#dialog");
                            loadSalesman($("#mbsl_MobileSaleID").val());
                            metroDialog.close('#dvLoadingOut');
                        } else {
                            alert(re[0].msg);
                        }

                    }
                });
            }
        }

        function loadSalesman(id) {
            $.ajax({
                url: "other.aspx",
                data: "app=loadSalesman&mbsl_mobilesaleid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {

                    $("#dvSalesman").html(data);
                    $("#dvSalesman>table>tbody>tr").each(function (i, v) {
                        var tmp = $(v).find(">td:last>a");
                        tmp.attr("href", "javascript:newSalesman(" + tmp.attr("eid") + ")");
                    });
                }
            });
        }

        function newSalesman(id) {
            var mbsl_mobilesaleid = $("#mbsl_MobileSaleID").val();
            $.ajax({
                url: "other.aspx",
                data: "app=newSalesman&id=" + mbsl_mobilesaleid + "&eid=" + id,
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
                    $("#frmNewSalesman").find("#btCancel").removeAttr("onclick");
                    $("#frmNewSalesman").find("#btSave").removeAttr("onclick");

                    $("#frmNewSalesman").find("#btCancel").click(function (e) {
                        metroDialog.close("#dialog");
                    });


                    $("#frmNewSalesman").append("<input type='hidden' id='txtmbsl_mobilesaleid' name='txtmbsl_mobilesaleid' value='" + $("#mbsl_MobileSaleID").val() + "'/>");

                    $("#frmNewSalesman").find("#btSave").click(function (e) {

                        var re = "";
                        $(".notify-container").html("");
                        $.ajax({
                            url: "other.aspx",
                            data: "app=saveSalesman&screen=tblMobileSaleItemNew&" + $(".frmNewSalesman").serialize(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                                metroDialog.open('#dvLoadingOut');

                            },
                            success: function (data) {
                                re = setError(data, "frmNewSalesman");
                                //metroDialog.close('#dvLoadingOut');
                                $("#dvLoadingOut").hide();
                                if (re != "error") {
                                    if (re[0].status == "ok") {
                                        metroDialog.close("#dialog");
                                        loadSalesman($("#mbsl_MobileSaleID").val());
                                        metroDialog.close('#dvLoadingOut');
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

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "mobileSale.aspx?mbsl_mobilesaleid=" + re[0].msg;

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
                if (re == "ok") {
                    window.location = "mobileSaleList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function clearStock(id) {
            if (confirm("Procced ?")) {
                $.ajax({
                    url: "other.aspx",
                    data: "app=clearStock&id=" + id,
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
                        if (data == "ok") {
                            window.location = window.location.href;
                        } else {
                            alert(data);
                        }
                    }
                });
            }
        }


    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server">
        <br />
        <div class="place-right">
            <button type="button" class="button success" onclick="newSalesman('')">Add Salesman</button>

        </div>
        <h3>Salesman</h3>
        <div id="dvSalesman" runat="server"></div>
    </div>

    <div data-role="dialog" class="panel dialog" id="dialog"
        data-close-button="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="">
            <div id="dvPopContent" style="max-height: 600px; overflow: auto; padding: 20px; width: 800px;">
            </div>
        </div>
    </div>
</asp:Content>
