<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="serviceOrder.aspx.cs" Inherits="X_CRM.serviceOrder.serviceOrder" %>
<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        var url = "<%  Response.Write(new sapi.sapi().baseUrl);%>opportunity/ajax.aspx";
        $(document).ready(function (e) {

            initComp("");
            $("#frmMaster").append($("#dvList"));


            $("#seor_CustomerID").change(function () {
                customerChange();
            });

        });

        function customerChange() {
            if ($("#seor_CustomerID").val() == null) {
                $("#seor_Phone").val("");
            }
            else {
                $.ajax({
                    url: "ajax.aspx",
                    data: {
                        "app": "getCompInfo",
                        "compID": $("#seor_CustomerID").val()
                    },
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        //alert(data);
                        if (data != "") {
							data = $.parseJSON(data);
							
							$("#quot_Phone").val(data[0].cust_Phone);
							$("#quot_Email").val(data[0].cust_Email);
							$("#quot_Address").val(data[0].cust_Address);
                        }
                        

                    }
                });
            }
        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "serviceOrder.aspx?seor_serviceorderid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
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
                    window.location = "serviceOrderList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblServiceOrderItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
            //alert(re);
            //event();
            //calTotal()
            //initComp("site.aspx");
            initComp("");
        }

        function delLine(pos, v) {
            _delLine(pos, v);
            //calTotal();
        }

    </script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>


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
            <input type="hidden" name="oppoID" id="oppoID" value=" " />
        </div>
        <br />
    </div>
</asp:Content>
