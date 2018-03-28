<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="incomeStatementList.aspx.cs" Inherits="X_CRM.accounting.incomeStatementList" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("");
            $("#btFind").removeAttr("onclick");
            event();
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", cPage, "");
        }

        function sortClick(frm, screen, col, v) {

            $(v).closest("div").find("#orderFieldBy").val(col);
            var orderBy = "asc";
            if ($(v).hasClass("sort-asc")) {
                orderBy = "desc";
            }
            else {
                orderBy = "asc";
            }
            $(v).closest("div").find("#orderBy").val(orderBy);
            _findRecord(frm, screen, "", 1, $(v).closest("div").attr("id"));
        }

        function event() {
            $("#btFind").click(function () {
                findCustomerRecord();
            });
        }

        function findCustomerRecord(id) {

            //var JournalNo = $("#jour_JournalNo").val();
            var JournalDate = "";
            var JournalDate_To = "";
            //var JournalDesc = $("#jour_Desc").val();

            if ($("#jour_Date").val() != "") {
                JournalDate = $("#jour_Date").val();
                if ($("#jour_Date_hh").val() != null)
                    JournalDate = JournalDate + ' ' + $("#jour_Date_hh").val() + ':' + $("#jour_Date_mm").val();
            }
            else {
                alert('Date is required !');
                return false;
            }

            if ($("#jour_Date_To").val() != "") {
                JournalDate_To = $("#jour_Date_To").val();
                if ($("#jour_Date_To_hh").val() != null)
                    JournalDate_To = JournalDate_To + ' ' + $("#jour_Date_To_hh").val() + ':' + $("#jour_Date_To_mm").val();
            }

            $.ajax({
                url: "../querryData/querryData.aspx",
                data: "app=getISList&jour_date=" + JournalDate + "&jour_date_to=" + JournalDate_To,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    //try {
                    //    data = $.parseJSON(data);
                    //    if (data != "") {
                    //        //$("#quot_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                    //        //$("#quot_CustomerID").val(data[0].cust_CustomerID).trigger("change");
                    //        $("#lbljoud_CAName" + n).text(data[0].chac_Name);
                    //        $("#joud_CAName" + n).val(data[0].chac_Name);
                    //    }
                    //    else {
                    //        $("#lbljoud_CAName" + n).text("");
                    //        $("#joud_CAName" + n).val("");
                    //    }
                    //}
                    //catch (e) { }

                    $("#dvList").html(data.replace("ok", ""));
                }
                
            });

        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>
</asp:Content>
