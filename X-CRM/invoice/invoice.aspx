<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="invoice.aspx.cs" Inherits="X_CRM.invoice.invoice" %>


<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
	 <%
        string url = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + HttpContext.Current.Request.ApplicationPath + (HttpContext.Current.Request.ApplicationPath != "/" ? "/" : "");
    %>

    <script src="<%=url%>Scripts/jquery.signalR-2.2.2.min.js"></script>
    <script src="<%=url%>signalr/hubs"></script>

    <script>
		var chat = $.connection.chatHub;
		//
		var quotationAjax = "<%  Response.Write(new sapi.sapi().baseUrl);%>quotation/ajax.aspx";

		$(document).ready(function (e) {

			chat.client.addNewMessageToPage = function (name, message, connectionid) {
				var chat = $.connection.chatHub;
				if (connectionid == "<%=Session["SID"].ToString()%>") {
					window.open("../tmp/" + message, "_blank");
				};
			};

			//$('#displayname').val(prompt('Enter your name:', ''));
			//$('#connection').val(prompt('Dest:', ''));

			$.connection.hub.start().done(function () {
				$('#sendmessage').click(function () {
					chat.server.send($('#displayname').val(), $('#message').val(), $('#connection').val());
					$('#message').val('').focus();
				});
			});

			$("#frmMaster").append($("#dvList"));
			event();

			//alert(invoice);
			//$(".text-light").text("Tax Invoice");

			//hide showing from 1-2 of 2
			$("table > tfoot").hide();

			var rid = $("#lblinvo_Type").text();
			if (rid == "Tax Invoice") {
				if ($("h1").hasClass("text-light"))
					$("h1").text("Tax Invoice");
				//$(".text-light").text("Tax Invoice");
			}
			else if (rid == "Invoice") {
				if ($("h1").hasClass("text-light"))
					$("h1").text("Invoice");
				//$(".text-light").text("Invoice");
			}
			else if (rid == "Commercial Invoice") {
				if ($("h1").hasClass("text-light"))
					$("h1").text("Commercial Invoice");
					//$(".text-light").text("Commercial Invoice");
			}

			var ids = [];
			ids = $("#InvoiceIDConverted").val().split(',');

			if (ids.length > 1) {
				var a = 0;
				a = $(".init_InvoiceItemID").length;

				for (b = 0; b < ids.length - 1; b++) {
					$.each($(".init_InvoiceItemID"), function (i, v) {
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

		function convertToOpportunity() {
			var ids = [];
			var guardID = "";

			$.each($(".init_InvoiceItemID"), function (i, v) {
				if ($(v).is(":checked")) {
					ids.push($(v).val());
				}
			});
			if (ids.length == 0) {
				alert("Please select at least one item in order to convert to project!");
				return;
			}

			if (confirm("Do you want convert this invoice to project?")) {
				$.ajax({
					url: "ajax.aspx",
					data: {
						"app": "ConvertToProject",
						"InvoID": $("#invo_InvoiceID").val(),
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
							window.location = "<%  Response.Write(new sapi.sapi().baseUrl);%>opportunity/opportunity.aspx?oppo_opportunityid=" + data;

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

		function customerChange() {
			if ($("#invo_CustomerID").val() == null) {
				//alert(null);
				$("#invo_Phone").val(null);
				$("#invo_Email").val(null);
				$("#invo_Address").val(null);
				$("#lblinvo_VATTIN").text("");
				//alert($("#lblinvo_VATTIN").text());
			}
			else {
				$.ajax({
					url: quotationAjax,
					data: {
						"app": "getQuotInfo",
						"compID": $("#invo_CustomerID").val()
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
						data = $.parseJSON(data);
						$("#invo_Phone").val(data[0].cust_Phone);
						$("#invo_Email").val(data[0].cust_Email);
						$("#invo_Address").val(data[0].cust_Address);
						//alert(data[0].cust_VATTIN);
						//$("#invo_VATTIN").text(data[0].cust_VATTIN); 
						$("#lblinvo_VATTIN").text(data[0].cust_VATTIN);
                        $("#lblinvo_Code").text(data[0].cust_Code);
                    }
				});
			}
		}

        <%--function printInv(id) {
             //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
             window.open("../report/report.aspx?report=rptInvoicePrint&pid=" + id, "_blank");
        }--%>

		function printInv(id) {
             //chat.server.send("<%=Session["SID"].ToString()%>", id,"printserver");
             window.open("../report/report.aspx?report=rptCommercialInvoice&pid=" + id, "_blank");
        }

		function savePreLine() {
			
            var re = _savePreLine("invoice.aspx?wh=" + $("#invo_WarehouseID").val(), "tblInvoiceItemNew", "frm", "dvList");
            var ind = $("#dvList>table>tbody>tr").length;
            $("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
			event();
			
            calTotal();
			$(".init_Qty").change();
        }

        function delLine(pos, v) {
            _delLine(pos, v);
            calTotal();
        }

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {

                if (re[0].status == "ok") {
                    window.location = "invoice.aspx?invo_invoiceid=" + re[0].msg;
                    //_loadScreen(screen, "2", re[0].msg, "dvContent", "");
                    //history.pushState(null, null, "customer.aspx?cust_customerid=" + re[0].msg);
                } else {
                    $("#returnBtn").hide();
                    $("#dvPopContent").html(re[0].msg);
                    metroDialog.open('#dialog');
                }
            }
        }

        function loadScreen(screen, mode, eid) { // 3 edit // 2 View
            _loadScreen(screen, mode, eid, "dvContent", "");
            $("#frmMaster").append($("#dvList"));
            event();
        }

        function delRecord(frm, screen, eid) {
            if (confirm("Delete Current Record ?")) {
				var re = _delRecord(frm, screen, eid, "", "");
				var invoiceType = re.substring(3, re.length);
				
				if (re.substring(0, 2) == "ok") {
					window.location = "invoiceList.aspx?invoice=" + invoiceType;
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

		function event() {
			var rid = getUrlVars("<%=HttpUtility.UrlDecode(HttpContext.Current.Request.Url.AbsoluteUri.ToString())%>")["invoice"];
			if (rid == "invTaxInvoice")
				$(".text-light").text("Tax Invoice");
			else if (rid == "invNonTaxInvoice")
				$(".text-light").text("Non Tax Invoice");
			else if (rid == "invCommercial")
				$(".text-light").text("Commercial");
			
            if ($('#init_WarehouseID').length > 0) {
                $('#init_WarehouseID').html('<option value="' + $("#invo_WarehouseID").val() + '">' + $("#invo_WarehouseID option:selected").text() + '</option>');
                $('#init_WarehouseID').val($("#invo_WarehouseID").val()).trigger("change");
            }

            $("#invo_Discount,#invo_Disc").unbind("change");
            $(".init_Qty,.init_Price").unbind("change");

            $(".databind").on("change", function (e) {
                
                if ($(e.target).attr("id") == "invo_CustomerID") {
                    if ($("#invo_CustomerID").val() != null) {
                        $.ajax({
                            url: "../shared/shared.aspx",
                            data: "app=getCustomer&cust_customerid=" + $("#invo_CustomerID").val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                if ($("#invo_PriceListID").length > 0) {
                                    if (data[0].prls_PriceListID +"" != "null" || data[0].prls_PriceListID != null) {
                                        

                                        //$("#invo_PriceListID").addItem({ text: data[0].prls_Name, value: data[0].prls_PriceListID });
                                        //$("#invo_PriceListID").setValue(data[0].prls_PriceListID);

                                        $("#invo_PriceListID").html("<option value='" + data[0].prls_PriceListID + "'>" + data[0].prls_Name + "</option>");

                                        $("#invo_PriceListID").val(data[0].prls_PriceListID).trigger("change");
                                    } else {
                                        $("#invo_PriceListID").html("<option value=''></option>");
                                        $("#invo_PriceListID").val('').trigger("change");
                                    }
                                }
                            }
                        });
                    }
                }

                if ($(e.target).attr("id") == "invo_OpportunityID") {
                    if ($("#invo_OpportunityID").val() != null) {
                        $.ajax({
                            url: "../opportunity/opportunity.aspx",
                            data: "app=getOppo&eid=" + $("#invo_OpportunityID").val(),
                            type: "POST",
                            async: false,
                            error: function () {
                                return "error";
                            },
                            beforeSend: function () {
                            },
                            success: function (data) {
                                data = $.parseJSON(data);
                                $("#invo_CustomerID").append("<option value='" + data[0].cust_CustomerID + "'>" + data[0].cust_Name + "</option>");
                                initComp("invoice.aspx");
                            }
                        });
                    }
                }

                if ($(e.target).hasClass("init_ItemID")) {
                    var n = "";
                    var t = e.target;
                    if ($(t).closest("tr").attr("pos") != null) {
                        n = $(t).closest("tr").attr("pos");
                    }

                    if ($("#init_ItemID" + n).val() != null) {
                        var str = "";
                        if ($("#invo_PriceListID").length > 0) {
                            if($("#invo_PriceListID").val() != null)
                            str = "&prls_pricelistid=" + $("#invo_PriceListID").val();
                        }
						$.ajax({
							url: "../shared/shared.aspx",
							data: "app=getItem&eid=" + $("#init_ItemID" + n).val() +
							"&cust_customerid=" + $("#invo_CustomerID").val() + str,
							type: "POST",
							async: false,
							error: function () {
								return "error";
							},
							beforeSend: function () {
							},
							success: function (data) {
								data = $.parseJSON(data);
								if (data[0].item_Unit != null || data[0].item_Name + "" != "null") {
									$("#init_Unit").val(data[0].item_Unit).trigger("change");
								}
								$("#init_Description" + n).val(data[0].item_Name);

								//add
								$("#init_PeriodProcessing" + n).val(data[0].item_Period);
								$("#init_Remark" + n).val(data[0].item_Remark);
								//

								$("#init_Qty" + n).val(data[0].item_Qty);
								$("#init_Total" + n).val(data[0].item_Total);
                                
                                $("#init_Price" + n).val(data[0].item_Price);
                                $("#init_Price" + n).change();
                                if ($("#lblinit_Price" + n).length > 0) {
                                    $("#lblinit_Price" + n).text(data[0].item_Price);
                                }
                                calTotal();
                            }
                        });
                    }
                }
            });

            initComp("invoice.aspx");

            $(".init_Qty,.init_Price,.init_Discount,.init_Disc").change(function (e) {
                var n = "";

                if ($(this).closest("tr").attr("pos") != null) {
                    n = $(this).closest("tr").attr("pos");

                }

				var qty = $("#init_Qty" + n).val().replace(/,/g, '');
                var price = $("#init_Price" + n).val().replace(/,/g, '');
                /*if ($("#lblinit_Price" + n).length > 0) {
                    price = $("#lblinit_Price" + n).text();
                }*/

                if (isNaN(qty) || qty == "") {
                    qty = 0;
                }
                if (isNaN(price) || price == "") {
                    price = 0;
				}
				var discType = $("#init_Discount" + n).val().replace(/,/g, ""); //P=%; A=$
				var total = 0;
				var discountAmount = parseFloat($("#init_Disc" + n).val().replace(/,/g, ""));
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
					$("#init_Disc" + n).val(discountAmount.toFixed(numFormat));
				}
				$("#lblinit_SubTotal" + n).text(total);
				$("#init_SubTotal" + n).val(total);

				total = total - discountAmount;
				$("#lblinit_DiscountAmount" + n).text(discountAmount.toFixed(numFormat));
				$("#init_DiscountAmount" + n).val(discountAmount);
                $("#lblinit_Total" + n).text(total);
                $("#init_Total" + n).val(total);
				calTotal();
            });


            $("#invo_Discount,#invo_Disc,#invo_VAT").change(function (e) {
                calTotal();
			});

			$("#invo_CustomerID").change(function () {
				customerChange();
			});

			$("#invo_Deposit").change(function () {
				calTotal();
			});

			$("#invo_ComissionType").change(function () {
				calTotal();
			});

			$("#init_ItemID").change(function () {
				if ($("#init_ItemID").val() != null) {
					//alert(true);
					$("#init_Qty").val(1);
					$("#init_Total").val($("#init_Price").val());
				}
				else {
					//alert(false);
					$("#init_Qty").val(null);
					$("#init_Price").val(null);
					$("#init_Total").val(null);
					$("#init_Remark").val(null);
					$("#init_Description").val(null);
				}
			});
			customizeButton();
        }

        function calTotal() {
            var quot_SubTotal = 0;
            var quot_Disc = $("#invo_Disc").val();
            var quot_DiscountAmount = 0;
            var quot_Total = 0;
            var tax = 0;
            var isTax = 0;
            var gTotal = 0;

            if (isNaN(quot_Disc) || quot_Disc == "") {
                quot_Disc = 0;
            }
			$.each($(".init_Total"), function (i, v) {
                var n = "";
                if ($(v).closest("tr").attr("pos") != null) {
                    n = $(v).closest("tr").attr("pos");
                }
				if (n != "" && $(v).closest("tr").find("input[name='txtDel" + n + "']").val() == "") {
					//original code
                    var quit_Total = $(v).text().replace(/,/g, '');
                    if (isNaN(quit_Total) || quit_Total == "") {
                        quit_Total = 0;
                    }
					quot_SubTotal = parseFloat(quot_SubTotal) + parseFloat($("#init_Total" + n).val().replace(/,/g, ''));
                }
			});

            if ($("#invo_Discount").val() == "P") {
                quot_DiscountAmount = parseFloat(quot_SubTotal) / 100 * parseFloat(quot_Disc);
            } else {
                quot_DiscountAmount = parseFloat(quot_Disc);
            }

            quot_Total = parseFloat(quot_SubTotal) - parseFloat(quot_DiscountAmount);

            isTax = $("#invo_isTax").val();
            if (isNaN(isTax) || isTax == "") {
                isTax = 0;
            }

            //--my VAT Caulation --//
            var vatAmount = 0;
            //alert($("#invo_VAT").val())

            if ($("#invo_VAT").length > 0) {
				if ($("#invo_VAT").val() == "")
					vatAmount = 0;
				else {
					//vatAmount = (parseFloat($("#invo_VAT").val()) / 100) * quot_Total;
					vatAmount = quot_SubTotal * (parseFloat($("#invo_VAT").val()) / 100);
				}
            }

            //alert("vat selection value = " + $("#invo_VAT").val());
            //alert("quot_Total=" + quot_Total);

            var totalwithVat = vatAmount + quot_Total;

            $("#lblinvo_VATAmount").text(vatAmount.toFixed(numFormat));
            //$("#lblquot_TotalVAT").text(totalwithVat.toFixed(numFormat));
            
            //--end my Calculation --//

            tax = parseFloat(quot_Total) * parseFloat(isTax) / 100;
            //gTotal = tax + quot_Total;// ty comment
			gTotal = vatAmount + quot_Total; // ty add
			var comission = gTotal;
			var comissionType = $("#invo_ComissionType").val();
			if (comissionType == "Employee (3%)") {
				comission = gTotal * 0.03;
				//alert(comission);
				$("#lblinvo_comissionAmount").text((comission).toFixed(numFormat));
			}
			else if (comissionType == "Other (5%)")
				$("#lblinvo_comissionAmount").text((gTotal * 0.05).toFixed(numFormat));
			else
				$("#lblinvo_comissionAmount").text((0).toFixed(numFormat));

            $("#lblinvo_SubTotal").text(quot_SubTotal.toFixed(numFormat));
            $("#lblinvo_DiscountAmount").text(quot_DiscountAmount.toFixed(numFormat));
            $("#lblinvo_Total").text(quot_Total.toFixed(numFormat));

            $("#lblinvo_Tax").text(tax.toFixed(numFormat));
            $("#lblinvo_GTotal").text(gTotal.toFixed(numFormat));

			$("#lblinvo_Balance").text((gTotal - parseFloat($("#lblinvo_PaidAmount").text()) - parseFloat($("#invo_Deposit").val().replace(/,/g, ''))).toFixed(numFormat));
        }

        ///////////////////////////////

        function payment(id) {
            $.ajax({
                url: "payment.aspx",
                data: "app=loadScreen&invo_invoiceid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvPopContent").html(data);
                    $("#frmPayment").find("#btSave").removeAttr("onclick");
                    $("#frmPayment").find("#btCancel").removeAttr("onclick");

                    $("#frmPayment").find("#btCancel").click(function (e) {
                        metroDialog.close('#dialog');
                    });

                    $("#frmPayment").find("#btSave").click(function (e) {
                        savePayment(id);
                    });
                    $("#returnBtn").hide();
                    metroDialog.open('#dialog');
                }
            });
        }

        function savePayment(id) {
            $.ajax({
                url: "payment.aspx",
                data: "app=saveRecord&invo_invoiceid=" + id +
                    "&" + $("#frmPayment").serialize() + "&cust_customerid=0",
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    var re = (setError(data, "frmPayment"));
                    if (re == "error") {
                    } else {
                        if (re[0].status == "ok") {
                            window.location = window.location.href;
                        } else {
                            alert(re[0].msg);
                        }
                    }
                }
            });
        }

        //////////////////////////

        function applyCN(id) {
            $.ajax({
                url: "applyCN.aspx",
                data: "app=loadScreen&invo_invoiceid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvPopContent").html(data);
                    $("#returnBtn").hide();
                    metroDialog.open('#dialog');
                }
            });
        }

        function crdn_CreditNoteID_Select(t, id) {
            
            $.ajax({
                url: "applyCN.aspx",
                data: "app=saveRecord&crdn_creditnoteid=" + id +
                    "&invo_invoiceid=" + $("#invo_InvoiceID").val(),
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    if (data == "ok") {
                        window.location = window.location.href;
                    } else {
                        alert(data);
                    }
                }
            });
        }

        /////////////////////////

        function voidInv(id) {
            if (confirm("Void Current Invoice ?")) {
                $.ajax({
                    url: "return.aspx",
                    data: "app=void&invo_invoiceid=" + id,
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        data = $.parseJSON(data);
                        if (data == "error") {
                        } else {
                            data = data["tbl"];
                            if (data[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                alert(data[0].msg);
                            }
                        }
                    }
                });
            }
        }

        function returnInv(id) {
            $.ajax({
                url: "return.aspx",
                data: "app=loadScreen&invo_invoiceid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                },
                success: function (data) {
                    $("#dvPopContent").html("<form id='frmShip'>" + data + "</form>");
                    $("#dvPopContent table tbody>tr:last").remove();
                    $("#dvPopContent").append("<input type='hidden' id='txtinvo_invoiceid' name='txtinvo_invoiceid' value='" + id + "'/>");
                    $("#returnBtn").show();
                    metroDialog.open('#dialog');

                    $(".init_BQty").val("0");
                }
            });
        }

        function saveReturn() {
            var isOk = 1;
            $.each($("#frmShip>table>tbody>tr"), function (i, v) {
                
                $.each($(v), function (ia, va) {
                    var soit_ShipQty = ($(va).find(".init_RQty").val());
                    var soit_RemainQty = ($(va).find(".init_BQty").val());
                    var soit_Qty = ($(va).find(".init_Qty").val());

                    if (isNaN(soit_ShipQty) || soit_ShipQty == "") {
                        soit_ShipQty = 0;
                    }
                    if (isNaN(soit_RemainQty) || soit_RemainQty == "") {
                        soit_RemainQty = 0;
                    }
                    if (isNaN(soit_Qty) || soit_Qty == "") {
                        soit_Qty = 0;
                    }

                    if (parseFloat(soit_Qty) - parseFloat(soit_ShipQty) < parseFloat(soit_RemainQty)) {
                        isOk = 0;
                    }
                    
                });
            });

            if (isOk == 0) {
                alert("Invalid Return Quantity !");
                return;
            }
            
            if (confirm("Return Current Invoice ?")) {
                var id = $("#txtinvo_invoiceid").val();
                $.ajax({
                    url: "return.aspx",
                    data: "app=saveRecord&invo_invoiceid=" + id +
                        "&" + $("#frmShip").serialize(),
                    type: "POST",
                    async: false,
                    error: function () {
                        return "error";
                    },
                    beforeSend: function () {
                    },
                    success: function (data) {
                        var re = (setError(data, "frmShip"));
                        if (re == "error") {
                        } else {
                            if (re[0].status == "ok") {
                                window.location = window.location.href;
                            } else {
                                alert(re[0].msg);
                            }
                        }


                    }
                });
            }
        }

        ///////////////////////////////////////

        function addItem() {
            metroDialog.open("#dvAddItem");
            $.ajax({
                url: "../shared/shared.aspx",
                data: "app=getItemGroupGrid",
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    $("#dvItemGroup").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                },
                success: function (data) {
                    $("#dvItemGroup").html(data);
                    $("#dvItem").html("");
                    $("#chkMutlipleQty").unbind("change");
                    $("#chkMutlipleQty").change(function () {
                        if ($("#chkMutlipleQty").is(":checked")) {
                            $("#dvQtyForm").show();
                        } else {
                            $("#dvQtyForm").hide();
                        }
                    });

                    $("#chkMutlipleQty").change();
                }
            });
        }

        function getItemGrid(id) {
            $.ajax({
                url: "../shared/shared.aspx",
                data: "app=getItemGrid&gid=" + id,
                type: "POST",
                async: false,
                error: function () {
                    return "error";
                },
                beforeSend: function () {
                    $("#dvItem").html('<br/><br/><br/><div class="align-center"><span class="fg-cyan mif-spinner2 mif-ani-spin mif-4x"></span></div>');
                },
                success: function (data) {
                    $("#dvItem").html(data);
                }
            });
        }

        function selectItem(id, name, price) {
            if ($("#chkMutlipleQty").is(":checked")) {
                $("#lblItem").html(name);
                $("#lblItem").attr("eid", id);
                $("#txtPrice").val(price);

            } else {
                $("#init_ItemID").html('<option value="' + id + '">' + name + '</option>');
                $("#init_ItemID").select2("val", id);
                $("#init_Qty").val($("#txtQty").val());
				//$("#init_Qty").val(1);
                $("#init_Qty").change();
                $("#btLSave").click();
            }
        }

        function selectItemSave() {
            $("#init_ItemID").html('<option value="' + $("#lblItem").attr("eid") + '">' + $("#lblItem").text() + '</option>');
            $("#init_ItemID").select2("val", $("#lblItem").attr("eid"));
            $("init_Price").val($("#txtPrice").val());
            $("#init_Qty").val($("#txtQty").val());
            $("#init_Qty").change();
            $("#btLSave").click();
		}

		function customizeButton() {
			$("#dvList").find("table > thead > tr > th")
				.eq(11)
				.next("td")
				.append("<input type='checkbox' id='chkAll' name='chkAll' style='margin-left: 3px;' onchange='myCheckAll()'/>")
		}

		function myCheckAll() {
			if ($("#chkAll").is(":checked"))
				$('input[name=init_InvoiceItemID]').prop('checked', true);
			else
				$('input[name=init_InvoiceItemID]').prop('checked', false);
		}



	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
	<div id="dvList" runat="server"></div>

    <div data-role="dialog" class="dialog" id="dialog"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">

        <div class="">
            <div id="dvPopContent" style="max-height: 500px; overflow: auto; padding: 20px;">
            </div>
        </div>

        <div class="align-right padding20" style="padding-top: 0px" id="returnBtn">
            <button type="button" id="btShip" onclick="saveReturn();" class="button success rounded">
                <span class="mif-dollar2 icon"></span>
                <span class="title">Return</span>
            </button>
            <button type="button" id="btCancel" onclick="metroDialog.close('#dialog');" class="button danger rounded">
                <span class="mif-cancel icon"></span>
                <span class="title">Cancel</span>
            </button>
        </div>
    </div>

    <!-- Add Item -->
    <div data-role="dialog" class="panel dialog" id="dvAddItem"
        data-close-button="true"
        data-windows-style="true"
        data-overlay="true"
        data-overlay-color="op-dark"
        data-overlay-click-close="false">
        <div class="heading bg-dark">
            <span class="title">Add Item</span>
        </div>

        <div class="">

            <div style="max-height: 700px; min-height: 700px; overflow: auto; padding: 20px;">
                <div style="max-height: 660px; min-height: 660px; width: 200px; float: left; border: 1px solid #ddd; background-color: #fff; overflow: auto; padding: 10px;" id="dvItemGroup">b</div>
                <div class="grid" style="margin-top: 0px; max-height: 460px; min-height: 460px; width: calc(100% - 200px); float: right; border: 1px solid #ddd; background-color: #fff; overflow: auto; padding: 10px">
                    <div class="input-control text full-size ">
                        <label class="input-control checkbox small-check">
                            <input type="checkbox" id="chkMutlipleQty" />
                            <span class="check"></span>
                            <span class="caption"></span>
                        </label>
                    </div>
                    <div style="border: 1px solid #ddd; display: none" class="padding10" id="dvQtyForm">
                        <div id="lblItem" style="font-size: 18px; font-weight: bold">Item</div>
                        <div class="row cells2">
                            <div class="cell">
                                <div class="input-control text full-size ">
                                    Quantity : 
                                <input id="txtQty" value="1" />
                                </div>
                            </div>
                            <div class="cell">
                                <div class="input-control text full-size ">
                                    Price : 
                                <input id="txtPrice" value="0.00" />
                                </div>
                            </div>
                        </div>
                        <hr class="thin bg-lightGray" />
                        <div class="place-right">
                            <button class="button rounded success" onclick="selectItemSave()">Save</button>
                        </div>
                        <div class="clearfix"></div>
                    </div>

                    <div id="dvItem"></div>
                </div>
            </div>

        </div>

    </div>

</asp:Content>
