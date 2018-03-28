<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="Permission.aspx.cs" Inherits="X_CRM.user.Permission" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#cboProfile").change(function (e) {
                $.ajax({
                    url: "",
                    data: "app=getPermission&prof_profileid=" + $(this).val(),
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
                        $("#tblPermission>tbody").html(data);
                        event();
                    }
                });

                $.ajax({
                    url: "",
                    data: "app=getReports&prof_profileid=" + $(this).val(),
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
                        $("#tblReport>tbody").html(data);
                        event();
                    }
                });
            });
            initComp("");
            $("#cboProfile").change();
        });

        function event() {
            $(".pfpm_V").unbind("change");
            $(".pfpm_I").unbind("change");
            $(".pfpm_E").unbind("change");
            $(".pfpm_D").unbind("change");
            $(".pfpm_A").unbind("change");

            $(".pfpm_V").change(function (e) {
            });
            $(".pfpm_I").change(function (e) {

                if ($(this).is(":checked")) {
                    var n = $(this).closest("tr").find("input[type='hidden']").val();
                    $("#pfpm_V" + n).prop("checked", "checked");
                }
            });
            $(".pfpm_E").change(function (e) {
                if ($(this).is(":checked")) {
                    var n = $(this).closest("tr").find("input[type='hidden']").val();
                    $("#pfpm_V" + n).prop("checked", "checked");
                }
            });
            $(".pfpm_D").change(function (e) {
                if ($(this).is(":checked")) {
                    var n = $(this).closest("tr").find("input[type='hidden']").val();
                    $("#pfpm_V" + n).prop("checked", "checked");
                }
            });

            $(".pfpm_A").change(function (e) {
                if ($(this).is(":checked")) {
                    var n = $(this).closest("tr").find("input[type='hidden']").val();

                    $("#pfpm_V" + n).prop("checked", "checked");
                    $("#pfpm_I" + n).prop("checked", "checked");
                    $("#pfpm_E" + n).prop("checked", "checked");
                    $("#pfpm_D" + n).prop("checked", "checked");
                } else {
                    var n = $(this).closest("tr").find("input[type='hidden']").val();

                    $("#pfpm_V" + n).removeAttr("checked");
                    $("#pfpm_I" + n).removeAttr("checked");
                    $("#pfpm_E" + n).removeAttr("checked");
                    $("#pfpm_D" + n).removeAttr("checked");
                }
            });

            $("#chkpfpm_V").unbind("change");
            $("#chkpfpm_I").unbind("change");
            $("#chkpfpm_E").unbind("change");
            $("#chkpfpm_D").unbind("change");
            $("#chkpfpm_A").unbind("change");
            $("#chkrppm_V").unbind("change");

            $("#chkpfpm_V").removeAttr("checked");
            $("#chkpfpm_I").removeAttr("checked");
            $("#chkpfpm_E").removeAttr("checked");
            $("#chkpfpm_D").removeAttr("checked");
            $("#chkpfpm_A").removeAttr("checked");
            $("#chkrppm_V").removeAttr("checked");

            $("#chkpfpm_V").change(function (e) {
                if ($(this).is(":checked"))
                    $(".pfpm_V").prop("checked", "checked");
                else
                    $(".pfpm_V").removeAttr("checked");

            });
            $("#chkpfpm_I").change(function (e) {
                if ($(this).is(":checked"))
                    $(".pfpm_I").prop("checked", "checked");
                else
                    $(".pfpm_I").removeAttr("checked");
            });
            $("#chkpfpm_E").change(function (e) {
                if ($(this).is(":checked"))
                    $(".pfpm_E").prop("checked", "checked");
                else
                    $(".pfpm_E").removeAttr("checked");
            });
            $("#chkpfpm_D").change(function (e) {
                if ($(this).is(":checked"))
                    $(".pfpm_D").prop("checked", "checked");
                else
                    $(".pfpm_D").removeAttr("checked");
            });

            $("#chkpfpm_A").change(function (e) {
                if ($(this).is(":checked")) {
                    $(".pfpm_V").prop("checked", "checked");
                    $(".pfpm_I").prop("checked", "checked");
                    $(".pfpm_E").prop("checked", "checked");
                    $(".pfpm_D").prop("checked", "checked");
                    $(".pfpm_A").prop("checked", "checked");
                }
                else {
                    $(".pfpm_V").removeAttr("checked");
                    $(".pfpm_I").removeAttr("checked");
                    $(".pfpm_E").removeAttr("checked");
                    $(".pfpm_D").removeAttr("checked");
                    $(".pfpm_A").removeAttr("checked");
                }
            });

            $("#chkrppm_V").change(function (e) {
                if ($(this).is(":checked")) {
                    $(".rppm_V").prop("checked", "checked");
                } else {
                    $(".rppm_V").removeAttr("checked");
                }
            });

            $("#chkrppm_VO").change(function (e) {
                if ($(this).is(":checked")) {
                    $(".rppm_VO").prop("checked", "checked");
                } else {
                    $(".rppm_VO").removeAttr("checked");
                }
            });

        }
        function save() {
            $.ajax({
                url: "",
                data: "app=savePermission&prof_profileid=" + $("#cboProfile").val() + "&" + $("#frmPermission").serialize(),
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
                        alert("Record saved !");
                    } else {
                        alert(data);
                    }
                }
            });
        }
    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div>
        <div class="padding10" style="width: 650px; margin: 0 auto">
            <div class="input-control text full-size">
                <select class="frm select" id="cboProfile" name="cboProfile" runat="server" style="width: 200px;"></select>
                <button class="button rounded" onclick="save()">
                    <span class="mif-floppy-disk"></span>
                    Save
                </button>
            </div>
            <hr class="thin bg-grayLighter" />
            <div id="dvPermission">
                <form id="frmPermission">
                    <h3>Modules</h3>
                    <table id="tblPermission" class="table border bordered striped hovered">
                        <thead>
                            <tr>
                                <th>Module</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkpfpm_V" />
                                    View</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkpfpm_I" />
                                    Insert</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkpfpm_E" />
                                    Edit</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkpfpm_D" />
                                    Delete</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkpfpm_A" />
                                    ALL</th>
                            </tr>
                        </thead>
                        <tbody></tbody>
                    </table>

                    <h3>Report</h3>
                    <table id="tblReport" class="table border bordered striped hovered">
                        <thead>
                            <tr>
                                <th>Report</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkrppm_VO" />View Other</th>
                                <th style="width: 90px">
                                    <input type="checkbox" id="chkrppm_V" />View</th>
                            </tr>
                        </thead>
                        <tbody>
                        </tbody>
                    </table>

                </form>
            </div>
        </div>
    </div>
</asp:Content>

