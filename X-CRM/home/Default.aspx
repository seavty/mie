<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="X_CRM.home.Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <script src="<%=url%>Scripts/hc/highcharts.js"></script>
    <script src="<%=url%>Scripts/hc/highcharts-more.js"></script>
    <script src="<%=url%>Scripts/hc/exporting.js"></script>
    <script>

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", cPage, "");
        }

        $(document).ready(function (e) {

            $.ajax({
                url: "dashboard.aspx",
                data: "app=dashboard",
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    $("#dvDashboard").html("<center><h3>Loading ...</h3></center>");
                },
                success: function (data) {
                    $("#dvDashboard").html(data);
                }
            });

            $("#dvLeft").hide();
            $("#dvContent").hide();

        });

    

        function loadBarGraph1(/*dv,title,cats,yText*/) {
            var chart1 = Highcharts.chart('dvGraph8', {

                chart: {
                    type: 'column'
                },

                title: {
                    text: 'Invoice(Last 30 days)'
                },
                legend: {
                    align: 'right',
                    verticalAlign: 'bottom',
                    layout: 'horizontal'
                },

                xAxis: {
                    categories: eval("['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']"),
                    labels: {
                        x: -10
                    }
                },

                yAxis: {
                    allowDecimals: true,
                    title: {
                        text: 'Amount/Quantity'
                    }
                },

                series: eval("[{" +
                    "name: 'Amount'," +
                        "data: [29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4]" +
                    "}, {" +
                    "name: 'No of Invoice'," +
                        "data: [29.9, 71.5, 106.4, 129.2, 144.0, 176.0, 135.6, 148.5, 216.4, 194.1, 95.6, 54.4]" +
                    "}]")

            });
        }

        function loadBarGraph2(dv, title, cats, data) {
            var chart2 = Highcharts.chart(dv, {

                title: {
                    text: title
                },

                xAxis: {
                    categories: eval(cats)
                },

                series: [{
                    type: 'column',
                    colorByPoint: true,
                    data: eval(data),
                    showInLegend: false
                }]

            });
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div class="clearfix"></div>

    <div id="dvDashboard" class="grid padding20"></div>
    <%
        string skin = System.Configuration.ConfigurationManager.AppSettings["theme"];
    %>
    
</asp:Content>
