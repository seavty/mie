<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="opportunity.aspx.cs" Inherits="X_CRM.opportunity.opportunity" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
		var url = "<%  Response.Write(new sapi.sapi().baseUrl);%>opportunity/ajax.aspx";
		var quotationAjax = "<%  Response.Write(new sapi.sapi().baseUrl);%>quotation/ajax.aspx"; //Call function from another folder(quotation)

        $(document).ready(function (e) {

            initComp("");
            $("#frmMaster").append($("#dvList"));

			$("#oppo_CustomerID").change(function () {
				customerChange();
			});
			$("table > tfoot").hide();
        });

		function ViewFromQuotation(data) {
			window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>quotation/quotation.aspx?quot_quotationid=" + data;
		}

		function customerChange() {
			if ($("#oppo_CustomerID").val() == null || $("#oppo_CustomerID").val() == "") {
                $("#lbloppo_Code").val("");
                $("#lbloppo_VATTIN").val("");
                $("#oppo_Phone").val("");
				$("#oppo_Email").val("");
				$("#oppo_Address").val("");
				
			}
			else {
				$.ajax({
					url: quotationAjax,
					data: {
						"app": "getQuotInfo",
						"compID": $("#oppo_CustomerID").val()
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
							//alert(data);
                            data = $.parseJSON(data);
                            $("#lbloppo_Code").text(data[0].cust_Code);
                            $("#lbloppo_VATTIN").text(data[0].cust_VATTIN);
							$("#oppo_Phone").val(data[0].cust_Phone);
							$("#oppo_Email").val(data[0].cust_Email);
							$("#oppo_Address").val(data[0].cust_Address);

                            
						}
					}
				});
			}
		}

        function saveRecord(frm, screen) {
            /*
            if (frm == "frmtblOpportunityNew") {
                convertToInvoice(frm, screen);
                return;
            }
            */

            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "opportunity.aspx?oppo_opportunityid=" + re[0].msg;
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
			if (mode == 3) {
				$("#oppo_CustomerID").change(function () {
					customerChange();
				});
			}
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
				var re = _delRecord(frm, screen, eid, "", "");
				//salert(re);
                if (re == "ok") {
                    window.location = "opportunityList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

        function savePreLine() {
            var re = _savePreLine("../include.aspx", "tblOpportunityItemNew", "frm", "dvList");
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

        function loadFormInput(oppoID) {
            $.ajax({
                url: url,
                data: {
                    "app": "loadFormInput",
                    "oppoID": oppoID
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
                    $("#oppo_InputType").find("option").eq(0).remove();
                    $("#btCancel").hide();
                    $("#oppoID").val(oppoID);
                }
            });
        }

        function validation() {
            var type = $("#oppo_InputType").val();
            var convertValue = $("#oppo_ConvertValue").val();
            if (isNaN(convertValue)) {
                alert(type + " must be numeric!");
                return;
            }
            if (convertValue == "") {
                alert(type + " can not be empty!");
                return;
            }
        }

        
        function convertToInvoice(oppoID) {
            //validation();
            if (!confirm("Do you want to convert this Project to Invoice"))
                return;

            if ($("#oppo_InvoiceType").val() == "") {
                alert("Please select invoice type first before convert!");
                return;
            }

            $.ajax({
                url: url,
                data: {
                    "app": "convertToInvoice",
                    "oppoID": $("#oppoID").val()
                },
                data: "app=convertToInvoice" + "&oppoID=" + oppoID,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    //alert(data);
                    if (data.substring(0, 2) == "ok") {
                        window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>invoice/invoice.aspx?invo_invoiceid=" + data.replace("ok", "");
                        }
                    }
            });
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

