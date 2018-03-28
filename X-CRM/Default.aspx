<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="X_CRM.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <link href="Scripts/css/metro.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-icons.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-schemes.min.css" rel="stylesheet" />
    <link href="Scripts/css/metro-colors.min.css" rel="stylesheet" />
    
    <script src="<%=url%>Scripts/jquery_min.js"></script>
    <script src="<%=url%>Scripts/js/metro.js"></script>
    <script src="<%=url%>Scripts/js/select2.min.js"></script>

    <script src="<%=url%>Scripts/jquery.signalR-2.2.2.min.js"></script>
    <script src="signalr/hubs"></script>
    <script>

        $(document).keypress(function (e) {
            if (e.which == 13) {
                $("#btLogin").click();
            }

        });

        $(document).ready(function (e) {
            
            var chat = $.connection.chatHub;
            chat.client.addNewMessageToPage = function (name, message, connectionid) {

                if (connectionid == $('#displayname').val()) {
                    alert(message)
                    //   Do What You want Here...
                };
            };

			$('#app-bar.darcula').html();


            //$('#displayname').val(prompt('Enter your name:', ''));
            //$('#connection').val(prompt('Dest:', ''));

            $.connection.hub.start().done(function () {
                $('#sendmessage').click(function () {
                    chat.server.send($('#displayname').val(), $('#message').val(), $('#connection').val());
                    $('#message').val('').focus();
                });
            });
            
            $("#btLogin").click(function (e) {
                $.ajax({
                    url: "Default.aspx",
                    data: "app=login&user=" + $("#txtUser").val() + "&password=" + $("#txtPassword").val() + "&chk=" +
                        ($("#chk").is(":checked") ? "Y" : ""),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                        $("#<%=dvInfo.ClientID%>").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                    },
                    success: function (data) {
                        if (data == "ok") {
                            $("#<%=dvInfo.ClientID%>").html("");
                            window.location = "Home/Default.aspx";
                        } else {
                            $("#<%=dvInfo.ClientID%>").html(data);
                        }
                    }
                });
            });
        });



	</script>

</head>
<body>




    <asp:Panel ID="pnlPopup" runat="server" CssClass="modalPopup" Style="display: none">
        <div class="header">
            Session Expiring!
        </div>
        <div class="body">
            Your Session will expire in&nbsp;<span id="seconds"></span>&nbsp;seconds.<br />
            Do you want to reset?
        </div>
        <div class="footer" align="right">
        </div>
    </asp:Panel>

    <div class="panel align-center" style="width: 500px; margin: 0 auto; z-index: 0; margin-top: 30px;">
        <img src="imgs/Logo_with Name and Tagline.jpg" style="height: 200px;" />
    </div>

    <div class="" style="width: 400px; margin: 0 auto; z-index: 0">
        <hr class="thin bg-lightGray" />
        <div class="content" style="padding-top: 0px;">
            <div class="input-control text full-size rounded ">
                <input type="text" class="align-center" placeholder="User Name" id="txtUser" />
            </div>
            <div class="input-control text full-size rounded">
                <input type="password" class="align-center" placeholder="Password" id="txtPassword" />
            </div>

            <label class="input-control checkbox">
                <input type="checkbox" class="align-center" id="chk" />
                <span class="check"></span>
                <span class="caption">Remember me</span>
            </label>

            <button class="button loading-pulse lighten success full-size rounded" id="btLogin">Login</button>


            <h4 class="fg-red align-center" id="dvInfo" runat="server"></h4>
        </div>
        <hr class="bg-lighterGray" />
        <div style="text-align:right">
            Licensed To :
            <label id="lblInfo" runat="server"></label>
        </div>

        <input type="hidden" id="message" />
        <input type="button" id="sendmessage" value="Send" style="display:none" />
        <input type="hidden" id="displayname" />
        <input type="hidden" id="connection" />

    </div>

</body>
</html>
