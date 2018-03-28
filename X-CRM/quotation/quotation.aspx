<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="quotation.aspx.cs" Inherits="X_CRM.quotation.quotation" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {
            $("#frmMaster").append($("#dvList"));
			event();
			//add new initComp("");
			initComp("");

			//Get in into function event(); Work with New and Edit
			//$("#quot_CustomerID").change(function () {
			//	customerChange();
			//});
			//$("#quot_Type2").change(function () {
			//	serviceTypeChange();
			//});
			
			//alert($("#hidButtonEditDelete").val());
			var num = $("#hidButtonEditDelete").val();
			if (num == 1) {
				$("#btEdit").hide();
				$("#btDelete").hide();
			}

			//hide showing from 1-2 of 2
			$("table > tfoot").hide();

			//$(".table th:eq(5), .table td:eq(6)").remove();
			//$(".table th:eq(5), .table td:eq(6)").remove();

			//$("table > thead > tr ").append("<th>&nbsp;<input id='chkAll' name='chkAll' type='checkbox' onchange='myCheckAll()'></th>");
			//$("table > tbody > tr ").append("<td><input name='lineItem'"+$("#quit_ItemID").val()+" type='checkbox'></td>");


			//alert($("#QuotIDConverted").val());

			var ids = [];
			ids = $("#QuotIDConverted").val().split(',');

			if (ids.length > 1) {
				var a = 0;
				a = $(".quit_QuotationItemID").length;

				for (b = 0; b < ids.length - 1; b++) {
					$.each($(".quit_QuotationItemID"), function (i, v) {
						if ($(v).val() == ids[b]) {
							$(v).hide();
						}
					});
				}

				if (ids.length - 1 == a) {
					$(".table th:last-child, .table td:last-child").remove();

				}
			}
		});

		function printInv(id) {
             //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
             window.open("../report/report.aspx?report=rptRelocationQuotation&pid=" + id, "_blank");
		}

		function ConvertToInvoice() {
			var ids = [];
			var guardID = "";

			$.each($(".quit_QuotationItemID"), function (i, v) {
				if ($(v).is(":checked")) {
					ids.push($(v).val());
				}
			});
			
			if (ids.length == 0) {
				alert("Please select at least one item in order to convert to invoice!");
				return;
			}

			if (confirm("Do you want convert this quotation to invoce?")) {
				$.ajax({
					url: "ajax.aspx",
					data: {
						"app": "ConvertToInvoice",
						"quotID": $("#quot_QuotationID").val(),
						"ids": ids,
					},
					type: "POST",
					async: false,
					error: function () {
						return "error";
					},
					beforeSend: function () {
					},
					success: function (data) {
						//alert(data+"\n, You are the best!");
						var check = data.substring(0, 2);
						if (check == "ok") {
							data = data.replace("ok", "");
							window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>invoice/invoice.aspx?invo_invoiceid=" + data;

						}
						else {
							alert("erroor: " + data);
						}
					}
				});
			}
		}

		function ViewCustomer(data) {
			window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>customer/customer.aspx?cust_customerid=" + data;
		}

        function convertToOpportunity() {
            if (!confirm("Do you want to convert this Quotation to Project? "))
                return;

            if ($("#quot_TotalVAT").val() <= 0) {
                alert("Grand Total must greater than 0 in order to convert to Project!");
                return;
            }

                $.ajax({
				url: "ajax.aspx",
				data: {
					"app": "convertToOpportunity",
					"quotID": $("#quot_QuotationID").val()
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
                    /*
                    //alert(data);
					var s=data.substring(data.search("o"), data.search("k") + 1);
					//alert(s);
					if (s == "ok")
						alert("success");
                    */

                    var check = data.substring(0, 2);
                    if (check == "ok") {
                        data = data.replace("ok", "");
                        window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>opportunity/opportunity.aspx?oppo_opportunityid=" + data;
                    }
                    else {
                        alert("erroor: " + data);
                    }
				}
				
			});
		}

		function ViewReviseQuotation(data) {
			//alert(data);
			window.location = "quotation.aspx?quot_quotationid=" + data;
			//$.ajax({
			//	url: "ajax.aspx",
			//	data: {
			//		"app": "ViewReviseQuotation",
			//		"quotID": $("#quot_QuotationID").val()
			//	},
			//	type: "POST",
			//	async: false,
			//	error: function () {
			//		return "error";
			//	},
			//	beforeSend: function () {
			//	},
			//	success: function (data) {
			//		alert(data);
			//		window.location = "quotation.aspx?quot_quotationid=" + data;
			//	}
			//});
		}

		function Revise() {
			if (confirm("Do you want to revise this quotation?")) {
				$.ajax({
					url: "ajax.aspx",
					data: {
						"app": "Revise",
						"quotID": $("#quot_QuotationID").val()
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
						window.location = "quotation.aspx?quot_quotationid=" + data;
					}
				});
			}
		}


		function serviceTypeChange() {
			if ($("#quot_Type2").val() == null) {
				$("#quot_TermAndCondition").val("");
				$("#quot_Notice").val("");
			}
			else {
				$.ajax({
					url: "ajax.aspx",
					data: {
						"app": "getServiceType",
						"serviceType": $("#quot_Type2").val()
					},
					type: "POST",
					async: false,
					error: function () {
						return "error";
					},
					beforeSend: function () {

					},
					success: function (data) {
						alert(data);
						if (data == "Corporate") {
							$("#quot_TermAndCondition").val("" +
								"Terms & Conditions:"+
								"\n1. Term of payment: The service provider starts working after receiving the full deposit. Payment is not refundable." +
								"\n2. Type of payment: Cash on hand, bank deposit, or bank transfer."+ 
								"\n3. International transfer: Any international bank transfer fees is the obligation of client. " +
								"\n4. Currency: USD. " +
								"\n5. Exclusive of VAT: Please kindly inform us in advance if you like VAT to be included in this quote.");
							$("#quot_Notice").val("" +
								"Notice: "+
								"\n1. The client is responsible for providing all required information/documents and making his/her presence available for signing, sealing, finger scanning, photo taking, cooperate bank account opening, and residency permit application on the requested dates."+ 
								"\n2. The payment is exclusive of penalty if the client fails to submit the required documents before the deadline required by related offices."+
								"\n3. The service provider is not liable for the result if client fails to provide true information/supporting documents." +
								"\n4. The service provider is responsible for the completion of the project according to the agreed scope of work and timeframe. " +
								"\n5. Full or part of the payment is not refundable unless the service provider fails to complete the project.");
						}
						else if (data == "Relocation") {
							$("#quot_TermAndCondition").val("" +
								"Terms & Conditions: " +
								"\n1. Term of payment: The service provider starts working after receiving the full payment. Payment is not refundable. " +
								"\n2. Type of payment: Cash on hand, bank deposit, or bank transfer. " +
								"\n3. International transfer: Any international bank transfer fees is the obligation of client." +
								"\n4. Currency: USD. " +
								"\n5. Exclusive of VAT: Please kindly inform us in advance if you like VAT to be included in this quote. ");
							$("#quot_Notice").val("" +
								"Notice: " +
								"\n1. The client is responsible for providing all required information/documents on the requested dates." +
								"\n2. The payment is exclusive of penalty if the client fails to submit the required documents before the deadline required by related offices. " +
								"\n3. The service provider is not liable for the result if client fails to provide true information/ supporting documents. " +
								"\n4. The service provider is responsible for the completion of the project according to the agreed scope of work and timeframe." +
								"\n5. Full or part of the payment is not refundable unless the service provider fails to complete the project.");
						}
						else if (data == "Translation") {
							$("#quot_TermAndCondition").val("" +
								"Terms & Conditions:" +
								"\n1. Term of payment: The service provider starts working after receiving the full deposit. The remaining sum of payment will be made upon the completion of the project.Payment is not refundable. " +
								"\n2. Type of payment: Cash on hand, bank deposit, or bank transfer. " +
								"\n3. International transfer: Any international bank transfer fees is the obligation of client. " +
								"\n4.Currency: USD. " +
								"\n5. Exclusive of VAT: Please kindly inform us in advance if you like VAT to be included in this quote. ");
							$("#quot_Notice").val("" +
								"Notice: " +
								"\n1. The number of pages for proofreading on Khmer document is estimated.The fee will be charged based on actual number of pages." +
								"\n2. The service provider is responsible for the completion of the project according to the agreed scope of work and timeframe." +
								"\n3. The service provider is responsible for correction free of charge on error made by the team if client reported it within 30 days." +
								"\n4. Full or part of the payment is not refundable unless the service provider fails to complete the project.");
						}
						else {
							$("#quot_TermAndCondition").val("");
							$("#quot_Notice").val("");
						}
					}
				});
			}
		}

		function customerChange() {
			if ($("#quot_customerid").val() == ""){ // || ($("#oppo_CustomerID").val() == null)) {
				$("#quot_phone").val("");
				$("#quot_email").val("");
				$("#quot_address").val("");
				$("#lblquot_Code").text("");
				$("#lblquot_VATTIN").text("");
				
			}
			else {
				$.ajax({
					url: "ajax.aspx",
					data: {
						"app": "getQuotInfo",
						"compID": $("#quot_CustomerID").val()
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
							//alert(data[0].cust_Phone);
							//alert(data[0].cust_Email);
							//alert(data[0].cust_Address);
							$("#quot_Phone").val(data[0].cust_Phone);
							$("#quot_Email").val(data[0].cust_Email);
							$("#quot_Address").val(data[0].cust_Address);
							$("#lblquot_Code").text(data[0].cust_Code);
							//$("#quot_Code").val(data[0].cust_Code);
							if (data[0].cust_VATTIN == null) {
								$("#lblquot_VATTIN").text("");
								$("#quot_VATTIN").val("");
							}
							else {
								$("#lblquot_VATTIN").text(data[0].cust_VATTIN);
								//$("#quot_VATTIN").val(data[0].cust_VATTIN);
							}
						}	
					}
				});
			}
		}

		function savePreLine() {

            var re = _savePreLine("../include.aspx", "tblQuotationItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
			calTotal();
			event();
			initComp("");
        }

        function delLine(pos, v) {
            _delLine(pos, v);
			calTotal();

        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "quotation.aspx?quot_quotationid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    alert(re[0].msg);
                }
            }
            initComp("");
        }

		function loadScreen(screen, mode, eid) { // 3 edit // 2 View
            _loadScreen(screen, mode, eid, "dvContent", "");
            $("#frmMaster").append($("#dvList"));
			event();
			initComp("");
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
                var re = _delRecord(frm, screen, eid, "", "");
				if (re == "ok") {
                    window.location = "quotationList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
					loadScreen(screen, 2, eid);
                }
            }
        }

		function event() {
			$(".databind").on("change", function (e) {
				if ($(e.target).attr("id") == "quot_OpportunityID") {
					if ($("#quot_OpportunityID").val() != null) {

						$.ajax({
							url: "../opportunity/opportunity.aspx",
							data: "app=getOppo&eid=" + $("#quot_OpportunityID").val(),
							type: "POST",
							async: false,
							error: function () {
								return "error";
							},
							beforeSend: function () {
							},
							success: function (data) {
								data = $.parseJSON(data);
								$("#quot_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
								$("#quot_CustomerID").val(data[0].cust_CustomerID).trigger("change");


							}
						});
					}
				}
				if ($(e.target).hasClass("quit_ItemID")) {
					var n = "";
					var t = e.target;
					if ($(t).closest("tr").attr("pos") != null) {
						n = $(t).closest("tr").attr("pos");
					}
					if ($("#quit_ItemID" + n).val() != null) {
						$.ajax({
							url: "../shared/shared.aspx",
							data: "app=getItem&eid=" + $("#quit_ItemID" + n).val(),
							type: "POST",
							async: false,
							error: function () {
								return "error";
							},
							beforeSend: function () {
							},
							success: function (data) {
								data = $.parseJSON(data);
								$("#quit_Description" + n).val(data[0].item_Name);
								$("#quit_Qty" + n).val(1);
								$("#quit_Price" + n).val(data[0].item_Price);
								$("#quit_Price" + n).change();
								$("#quit_PeriodProcessing" + n).val(data[0].item_Period);
								$("#quit_Remark" + n).val(data[0].item_Remark);

								calTotal();

							}
						});
					}
				}

			});

			$("#quot_Discount,#quot_Disc").unbind("change");
			$(".quit_Qty, .quit_Price, .quit_Discount, .quit_Disc").unbind("change");
			//$(".quit_ItemID").unbind("change");
			initComp("quotation.aspx");


			$(".quit_Qty, .quit_Price, .quit_Discount, .quit_Disc").change(function (e) {
				var n = "";
				if ($(this).closest("tr").attr("pos") != null) {
					n = $(this).closest("tr").attr("pos");
				}

				var qty = $("#quit_Qty" + n).val().replace(/,/g, "");
				//var qty = 1; // I am update.
				var price = $("#quit_Price" + n).val().replace(/,/g, "");
				if (isNaN(qty) || qty == "") {
					qty = 1;
				}
				if (isNaN(price) || price == "") {
					price = 0;
				}
				var discType = $("#quit_Discount" + n).val().replace(/,/g, ""); //P=%; A=$
				var total = 0;
				var discountAmount = parseFloat($("#quit_Disc" + n).val().replace(/,/g, ""));
				if (isNaN(discountAmount) || discountAmount == "")
					discountAmount = 0;

				total = (parseFloat(qty) * parseFloat(price)).toFixed(numFormat);

				if (discType == "P") {
					discountAmount = (total * discountAmount) / 100;
					//alert(discountAmount);
				}
				else if (discType == "A") {
					discountAmount = discountAmount;
					//alert(discountAmount);
				}
				else {
					discountAmount = 0;
					$("#quit_Disc" + n).val(discountAmount.toFixed(numFormat));
				}
				$("#lblquit_SubTotal" + n).text(total);
				$("#quit_SubTotal" + n).val(total);

				total = total - discountAmount;
				
				//var total = parseFloat(price).toFixed(numFormat);
				$("#lblquit_DiscountAmount" + n).text(discountAmount.toFixed(numFormat));
				$("#quit_DiscountAmount" + n).val(discountAmount);
				$("#lblquit_Total" + n).text(total.toFixed(numFormat));
				$("#quit_Total" + n).val(total);
				calTotal();
		
            });
            
            
            $("#quot_Discount,#quot_Disc,#quot_VAT").change(function (e) {
                calTotal();
			});

			//$("#quot_CustomerID").change(function () {
			//	customerChange();
			//});

			//$("#quot_Type2").change(function () {
			//	serviceTypeChange();
			//});

			//$("#quot_VAT").change(function () {
			//	calVal();
			//});

			//$("#quot_Type2").change(function () {
			//	alert($("#quot_Type2").val());
			//});

			$("#quot_Deposit").change(function () {
				calTotal();
			});

			customizeButton(); 
		}

		//update after come from mie
		//function calTotal() {
		//	//alert($("#quit_Price").val());

		//	var quot_SubTotal = 0;
		//	var quot_Disc = $("#quot_Disc").val();
		//	var quot_DiscountAmount = 0;
		//	var quot_Total = 0;
		//	if (isNaN(quot_Disc) || quot_Disc == "") {
		//		quot_Disc = 0;
		//	}

		//	$.each($(".lblquit_Total"), function (i, v) {
		//		var n = "";
		//		if ($(v).closest("tr").attr("pos") != null) {
		//			n = $(v).closest("tr").attr("pos");
		//		}
		//		if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
		//			var quit_Total = $(v).text().replace(/,/g, '');
		//			if (isNaN(quit_Total) || quit_Total == "") {
		//				quit_Total = 0;
		//			}
		//			quot_SubTotal = parseFloat(quot_SubTotal) + parseFloat(quit_Total);
		//		}
		//	});

		//	if ($("#quot_Discount").val() == "P") {
		//		quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
		//	} else {
		//		quot_DiscountAmount = parseFloat(quot_Disc);
		//	}

		//	quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

		//	//disType = $("#lblquot_Total" + n).val().replace(/,/g, "");

		//	var vatAmount = 0;
		//	if ($("#quot_VAT").val() == "")
		//		vatAmount = 0;
		//	else
		//		vatAmount = (parseFloat($("#quot_VAT").val()) / 100) * quot_Total;

		//	var totalwithVat = vatAmount + quot_Total;

		//	$("#lblquot_TotalVAT").text(totalwithVat.toFixed(numFormat));
		//	$("#lblquot_VATAmount").text(vatAmount.toFixed(numFormat));

		//	$("#lblquot_SubTotal").text(quot_SubTotal.toFixed(numFormat));
		//	$("#lblquot_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
		//	$("#lblquot_Total").text(quot_Total.toFixed(numFormat));
		//}

		//Before go to mie.
		function calTotal() {
            var quot_SubTotal = 0;
            var quot_Disc = $("#quot_Disc").val();
            var quot_DiscountAmount = 0;
            var quot_Total = 0;
            if (isNaN(quot_Disc) || quot_Disc == "") {
                quot_Disc = 0;
			}

			//alert($("#quit_Qty").val());
			$.each($(".quit_Total"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
				if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
					//Original Code.
                    var quit_Total = $(v).text().replace(/,/g, '');
                    if (isNaN(quit_Total) || quit_Total == "") {
                        quit_Total = 0;
                    }
					//quot_SubTotal = parseFloat(quot_SubTotal) + parseFloat(quit_Total);

					quot_SubTotal = quot_SubTotal + parseFloat($("#quit_Total" + n).val().replace(/,/g, ''));
				}
			});
			
            if ($("#quot_Discount").val() == "P") {
                quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
            } else {
                quot_DiscountAmount = parseFloat(quot_Disc);
            }

			quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount) ;

			//disType = $("#lblquot_Total" + n).val().replace(/,/g, "");
			
			var vatAmount = 0;
			if ($("#quot_VAT").val() == "")
				vatAmount = 0;
			else {
				//vatAmount = (parseFloat($("#quot_VAT").val()) / 100) * quot_Total;
				vatAmount = quot_SubTotal * (parseFloat($("#quot_VAT").val()) / 100);
			}
			//quot_Total += vatAmount;

			var grandTotal = quot_Total + vatAmount;

			var quot_Balance = grandTotal - parseFloat($("#quot_Deposit").val().replace(/,/g, ''));

			$("#lblquot_TotalVAT").text(grandTotal.toFixed(numFormat));
			$("#lblquot_VATAmount").text(vatAmount.toFixed(numFormat));

            $("#lblquot_SubTotal").text(quot_SubTotal.toFixed(numFormat));
            $("#lblquot_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
			$("#lblquot_Total").text(quot_Total.toFixed(numFormat));
			$("#lblquot_Balance").text(quot_Balance.toFixed(numFormat));
        }

        function convertSO(id) {
            window.location = "../saleorder/saleorder.aspx?quot_quotationid=" + id;
        }

        function convertInv(id) {
            window.location = "../invoice/invoice.aspx?quot_quotationid=" + id;
		}

		function calVal() {
			$("#lblquot_TotalVAT").text(234);
		}

		function customizeButton() {
			//How to compare string in jquery
			//if ($("#btFind").text().localeCompare("Find"))
			//	alert("true");
			//alert("Haha");

			if ($("#btFind").length > 0) { 
				$("#dvList").find("table > thead > tr > th")
					.eq(11)
					.next("td")
					.append("<input type='checkbox' id='chkAll' name='chkAll' style='margin-left: 3px;' onchange='myCheckAll()'/>")

				//$("table > tbody> tr > td > a").each(function (i, v) {
				//	var guardID = $(this).attr("eid");
					
				//	var myHtml = "";
				//	//$(this).closest("td").html("");
				//	myHtml =
				//		"<a href='javascript:indivualAttendance(" + guardID + ")' >" +
				//		"<span class='mif-pencil mif-lg fg-green'></span>" +
				//		"</a>" + "&nbsp;&nbsp;&nbsp" +
				//		"<a href='javascript:removeIndivualAttendance(" + guardID + ")' >" +
				//		"<span class='mif-bin mif-lg fg-red'></span>" +
				//		"</a>"
				//	//$(this).html(myHtml);
				//	$(this).closest("td").html(myHtml);
				//});
			}
		}

		function myCheckAll() {
			if ($("#chkAll").is(":checked"))
				$('input[name=quit_QuotationItemID]').prop('checked', true);
			else
				$('input[name=quit_QuotationItemID]').prop('checked', false);
		}

	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>

