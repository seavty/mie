<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="documentList.aspx.cs" Inherits="X_CRM.quotation.documentList" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>
    <script src="<%=url%>Scripts/jquery.fileupload.js"></script>
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
            initComp("");
            $("#btFind").click();
			$("table > tfoot").hide();
            $('#fileupload').fileupload({
                //dataType: 'json',
                done: function (e, data) {
					try {
						//alert(data);
						var str = $.parseJSON(data.result);
						
                        if (str[0].status == "ok")
                            window.location = window.location.href;
                        else if (str[0].status == "fs") {
                            alert("File too big to upload !\n Max size is 2MB !");
                        } else {
                            alert("Error Uploading File");
                        }
                    } catch (e) { }

                },
                fail: function (e, data) {
                    alert(data.textStatus);
                }
            });

            $("#dvList").before($("#dvUpload"));
        });

        function findRecord(frm, screen, cPage) {
            _findRecord(frm, screen, "", -1, "");

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
			$("table > tfoot").remove();
        }



	</script>

</asp:Content>

<asp:content id="BodyContent" runat="server" contentplaceholderid="mainContent" clientidmode="Static">
    <div class="clearfix"></div>
    <div id="dvList" name="dvList" runat="server"></div>
    <div id="dvUpload" runat="server"></div>

</asp:content>

