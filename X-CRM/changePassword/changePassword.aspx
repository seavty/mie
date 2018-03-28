<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="changePassword.aspx.cs" Inherits="X_CRM.changePassword.changePassword" %>


<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <script>
        $(document).ready(function (e) {

            $("#btChangePwd").click(function (e) {
                if ($("#txtNewPwd").val() != $("#txtConPwd").val()) {
                    alert("Confirm Password does not match !");
                    return false;
                }

                $.ajax({
                    url: "",
                    data: "app=changePassword&OldPwd=" + $("#txtOldPwd").val() + 
                        "&NewPwd=" + $("#txtNewPwd").val(),
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
                        if (data == "wrongpwd") {
                            alert("Old password does not match !");
                        }
                        else if (data == "ok") {
                            window.location = "<%=url%>Home/Default.aspx";
                        } else {
                            alert(data);
                        }
                    }
                });
            });
        });

        



    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div style="width:500px;margin:0 auto">
        <div class="input-control text full-size rounded">
            <input id="txtOldPwd" placeholder="Old Password" class="align-center" type="password" />
        </div>
        <div class="input-control text full-size rounded">
            <input id="txtNewPwd" placeholder="New Password" class="align-center" type="password" />
        </div>
        <div class="input-control text full-size rounded    ">
            <input id="txtConPwd" placeholder="Confirm Password" class="align-center" type="password" />
        </div>

        <button class="button rounded success full-size" type="button" id="btChangePwd">
            Change Password
        </button>
    </div>
</asp:Content>
