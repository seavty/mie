<%@ Page Title="" Language="C#" MasterPageFile="~/main.Master" AutoEventWireup="true" CodeBehind="itemGroup.aspx.cs" Inherits="X_CRM.itemGroup.itemGroup" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="headContent">
    <script>
        $(document).ready(function (e) {

            initComp("");
			$("#frmMaster").append($("#dvList"));
			
			//Hide detail in line item
			if ($("#CaughtMode").val() == 2)
				$(".table th:last-child, .table td:last-child").remove();

			//hide showing from 1-2 of 2
			$("table > tfoot").hide();
		});

		//function sortClick(frm, screen, col, v) {

		//	$(v).closest("div").find("#orderFieldBy").val(col);
		//	var orderBy = "asc";
		//	if ($(v).hasClass("sort-asc")) {
		//		orderBy = "desc";
		//	} else {
		//		orderBy = "asc";
		//	}
		//	$(v).closest("div").find("#orderBy").val(orderBy);
		//	_findRecord(frm, screen, "", 1, $(v).closest("div").attr("id"));
		//}

        function saveRecord(frm, screen) {
            var re = _saveRecord(frm, screen, "");
            if (re != "error") {
                if (re[0].status == "ok") {
                    window.location = "itemGroup.aspx?itmg_itemgroupid=" + re[0].msg;

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
                    window.location = "ItemGroupList.aspx";
                } else {
                    alert("Error Deleting Record !\n" + re);
                    loadScreen(screen, 2, eid)
                }
            }
        }

		function savePreLine() {
			var re = _savePreLine("../include.aspx", "tblItemNew", "frm", "dvList");
			var ind = $("#dvList>table>tbody>tr").length;
			$("#dvList>table>tbody>tr:eq(" + (ind - 1) + ")").before(re);
			
		}

		function delLine(pos, v) {
			_delLine(pos, v);
		}

	</script>

</asp:Content>

<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="mainContent" ClientIDMode="Static">
    <div id="dvList" runat="server"></div>
</asp:Content>
