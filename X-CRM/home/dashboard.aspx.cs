using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.home
{
    public partial class dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!new sapi.sapi().getLic())
            {
                Response.Write("<center><h3>Invalid License !</h3></center>");
                Response.End();
            }
            string re = "";
            sapi.sapi cls = new sapi.sapi();
            sapi.db db = new sapi.db();
            try
            {
                DataTable tblDashboard = db.readData("Select * from sys_dashboard where dash_Deleted is null  Order by dash_Order ");
                foreach (DataRow rowDashboard in tblDashboard.Rows)
                {

                    DataTable tblDashboardItem = db.readData("Select * from sys_dashboardItem Where dbit_Deleted is null and dbit_DashboardID = " + rowDashboard["dash_Dashboardid"].ToString());
                    re = re + "<div class=\"row cells" + tblDashboardItem.Rows.Count + "\">";
                    foreach (DataRow rowDashboardItem in tblDashboardItem.Rows)
                    {
                        if (rowDashboardItem["dbit_Type"].ToString() == "1")
                        {

                            string list = db.readData("list_Name", "Select list_Name From sys_list Where list_ListID = " + rowDashboardItem["dbit_ListID"].ToString());
                            string filter = rowDashboardItem["dbit_Condition"].ToString().Replace("#u", Session["userid"].ToString()).Replace("#p", Session["profiles"].ToString());
                            if (filter.Length > 0)
                                filter = " and " + filter;
                            re = re +
                                "<div class='cell'>" +
                                    "<div class='panel '>" +
                                        "<div class='padding10 bg-white fg-black padding10' style='border: 1px solid #eee'>" +
                                            "<span class='text-enlarged text-bold'>" + rowDashboardItem["dbit_Name"].ToString() +
                                                "</span>" +
                                                    "<a href='dashboardDetail.aspx?dbit=" + rowDashboardItem["dbit_DashboardItemID"] + "' class='place-right text-enlarged'>" +
                                                    "View All</a>" +
                                            "<hr class='thin' /> " +
                                            "<div id='dvInfo" + rowDashboardItem["dbit_DashboardItemID"] + "' runat='server' style='height: 485px; overflow: auto;'> " +
                                            cls.findRecord(db, "", list, "frm" + rowDashboardItem["dbit_DashboardItemID"], null, "","DESC",
                                            filter: filter,
                                            hidePage: true) +
                                            "</div> " +
                                        "</div>" +
                                    "</div>" +
                                "</div>";

                        }
                        if (rowDashboardItem["dbit_Type"].ToString() == "2")
                        {
                            string badgeData = "";
                            DataTable tblBadge = db.readData(rowDashboardItem["dbit_Condition"].ToString().Replace("#u", Session["userid"].ToString()).Replace("#p", Session["profiles"].ToString()));
                            if (tblBadge != null)
                            {
                                foreach (DataRow rowBadge in tblBadge.Rows)
                                {
                                    foreach (DataColumn _col in tblBadge.Columns)
                                    {
                                        badgeData = badgeData + "<h4 class='align-right'>" +
                                            (_col.ColumnName.Substring(0, 1) == "_" ? "" : _col.ColumnName) + " " +
                                            (string.IsNullOrEmpty(rowBadge[_col.ColumnName].ToString()) ? "<br/>" : rowBadge[_col.ColumnName]) + "</h3> ";
                                    }

                                }
                                re = re +
                                    "<div class='cell'>" +
                                        "<div style='background-color:#" + rowDashboardItem["dbit_backcolor"].ToString() +
                                        ";color:#" + rowDashboardItem["dbit_forecolor"].ToString() + "' class='padding10 text-shadow'> " +
                                        "    <span class='mif-" + rowDashboardItem["dbit_Icon"].ToString() + " mif-2x'></span> " +
                                        "    <span class='title text-enlarged padding10'>" + rowDashboardItem["dbit_Name"].ToString() + "</span> " +
                                        "    <div id='dvInfo" + rowDashboardItem["dbit_DashboardItemID"] + "' runat='server'> " +
                                            badgeData +
                                        "    </div> " +
                                        "</div> " +
                                    "</div>";
                            }

                        }
                        if (rowDashboardItem["dbit_Type"].ToString() == "3")
                        {
                            string cats = "";
                            string data = "";
                            DataTable tblData = db.readData(rowDashboardItem["dbit_Condition"].ToString().Replace("#u", Session["userid"].ToString()).Replace("#p", Session["profiles"].ToString()));
                            if (tblData != null)
                            {
                                foreach (DataRow rowData in tblData.Rows)
                                {
                                    cats = cats + "'" + rowData[0].ToString() + "',";
                                    data = data +  rowData[1].ToString() + ",";
                                }
                                if (cats.Length > 0)
                                {
                                    cats = "[" + cats.Substring(0, cats.Length - 1) + "]";
                                    data = "[" + data.Substring(0, data.Length - 1) + "]";
                                }
                            }
                            re = re +
                                "<div class='cell'>" +
                                    "<div id='dvGraph" + rowDashboardItem["dbit_DashboardItemID"] + "' runat='server' class='bg-grayLighter  fg-white padding10' style='height: 485px'>" +
                                    "</div> " +
                                "</div>";
                            
                            re = re + "<script>loadBarGraph2(\"dvGraph" + rowDashboardItem["dbit_DashboardItemID"] + 
                                "\",\"" + rowDashboardItem["dbit_Name"] + 
                                "\",\"" + cats + "\",\"" + data + "\")</script>";
                        }
                    }
                    re = re + "</div>";



                    /*
                    if (rowDashboard["dash_Type"].ToString() == "2")
                    {
                        
                        DataTable tblDashboardItem = db.readData("Select * from sys_dashboardItem Where dbit_DashboardID = " + rowDashboard["dash_Dashboardid"].ToString());
                        re = re + "<div class=\"row cells" + tblDashboardItem.Rows.Count + "\">";
                        foreach (DataRow rowDashboardItem in tblDashboardItem.Rows)
                        {
                            string badgeData = "";
                            DataTable tblBadge = db.readData(rowDashboardItem["dbit_Condition"].ToString().Replace("#u", Session["userid"].ToString()).Replace("#p", Session["profiles"].ToString()));
                            if (tblBadge != null)
                            {
                                foreach (DataRow rowBadge in tblBadge.Rows)
                                {
                                    foreach(DataColumn _col in tblBadge.Columns)
                                    {
                                    badgeData = badgeData + "<h4 class='align-right'>" + 
                                        (_col.ColumnName.Substring(0,1) == "_" ? "" : _col.ColumnName) + " " +
                                        (string.IsNullOrEmpty(rowBadge[_col.ColumnName].ToString()) ? "<br/>" : rowBadge[_col.ColumnName]) + "</h3> ";
                                    }

                                }
                                re = re +
                                    "<div class='cell'>" +
                                        "<div style='background-color:#" + rowDashboardItem["dbit_backcolor"].ToString() +
                                        ";color:#" + rowDashboardItem["dbit_forecolor"].ToString() + "' class='padding10 text-shadow'> " +
                                        "    <span class='mif-" + rowDashboardItem["dbit_Icon"].ToString() + " mif-2x'></span> " +
                                        "    <span class='title text-enlarged padding10'>" + rowDashboardItem["dbit_Name"].ToString() + "</span> " +
                                        "    <div id='dvInfo" + rowDashboardItem["dbit_DashboardItemID"] + "' runat='server'> " +
                                            badgeData +
                                        "    </div> " +
                                        "</div> " +
                                    "</div>";
                            }
                        }

                        re = re + "</div>";
                    }

                    if (rowDashboard["dash_Type"].ToString() == "3")
                    {

                        DataTable tblDashboardItem = db.readData("Select * from sys_dashboardItem Where dbit_DashboardID = " + rowDashboard["dash_Dashboardid"].ToString());
                        re = re + "<div class=\"row cells" + tblDashboardItem.Rows.Count + "\">";
                        foreach (DataRow rowDashboardItem in tblDashboardItem.Rows)
                        {

                            re = re +
                                "<div class='cell'>" +
                                    "<div id='dvGraph" + rowDashboardItem["dbit_DashboardItemID"] + "' runat='server' class='g-grayLighter  fg-white padding10' style='height: 485px'>" +
                                    "</div> " +
                                "</div>";
                        }

                        re = re + "</div>";
                    }*/
                }
                Response.Write(re);
            }
            catch (Exception ex) { Response.Write(ex.Message); }
            finally { db.close(); }


        }
    }
}