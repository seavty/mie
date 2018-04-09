<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="X_CRM.report._default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            $("#invo_Date_hh").val("00");
            $("#invo_Date_mm").val("00");
            $("#invo_Date_To_hh").val("23");
            $("#invo_Date_To_mm").val("59");
            //alert("<%=DateTime.UtcNow.AddHours(7).ToString("dd") + "/" + DateTime.UtcNow.AddHours(7).ToString("MM") + "/" + DateTime.UtcNow.AddHours(7).ToString("yyyy")%>");
            initComp("default.aspx");
            //$(".datepicker").attr("data-preset", "<%=DateTime.UtcNow.AddHours(7).ToString("dd") + "/" + DateTime.UtcNow.AddHours(7).ToString("MM") + "/" + DateTime.UtcNow.AddHours(7).ToString("yyyy")%>");
            $(".datepicker>input").val("<%=DateTime.UtcNow.AddHours(7).ToString("dd") + "/" + DateTime.UtcNow.AddHours(7).ToString("MM") + "/" + DateTime.UtcNow.AddHours(7).ToString("yyyy")%>");
            $(".datepicker").datepicker();

			$("#isExp").val("");

			if ($("h1").hasClass("text-light"))
					$("h1").text("Invoice Report");

        });

        function exp()
        {
            $("#isExp").val("Y");
            $("#btFind").click();
        }

        function findRecord(frm, screen, cPage) {
            $.ajax({
                url: "",
                data: "app=findRecord&" + $("#frmMaster").serialize(),
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    if (data.substring(0, 2) == "ok") {
                        var exp = "";
                        if ($("#isExp").val() == "Y") {
                            exp = "&isExp=" + $("#isExp").val();
                        }
                        window.open("report.aspx?report=" + data.replace('ok','') +
                            exp + "&" + $("#frmMaster").serialize(), "_blank");
                        $("#isExp").val("")
                    }
                }
            });

        }


	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
        <input type="hidden" id="isExp" name="isExp" />
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>
</asp:Content>
