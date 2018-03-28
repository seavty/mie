using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Configuration;
using System.Data;
using Microsoft.Reporting.WebForms;

namespace X_CRM.report
{
    public partial class report : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                sapi.db db = new sapi.db();
                try
                {
                    if (db.connect())
                    {
                        if (Request.QueryString["report"] != null)
                        {

                            DataTable tblPermission = db.readData(
                            "Select * from ( " +
                            " select mmnu_Name smnu_Name, mmnu_MainMenuID smnu_SubMenuID,mmnu_Text smnu_Text,'1' _type,mmnu_GroupID from sys_mainMenu " +
                            " where (mmnu_GroupID = 4 or mmnu_GroupID = 0) and mmnu_Url is not null " +
                            " UNION all " +
                            " select smnu_Name,smnu_SubMenuID,smnu_Text,'2' _type,0 mmnu_GroupID from sys_subMenu " +
                            " where smnu_Url is not null and smnu_MainMenuID is not null " +
                            " ) A " +
                            " inner join [dbo].[sys_ReportPermission] on rppm_ReportID = smnu_SubMenuID " +
                            " and rppm_ProfileID in (" + Session["profiles"].ToString() + ")" +
                            " Where (rppm_V = 'Y' and smnu_Name = " + db.sqlStr(Request.QueryString["report"]) +
                            ") or (mmnu_GroupID = 0)");
                            if (tblPermission.Rows.Count <= 0)
                            {
                                
                                    Response.Write ("<div class=\"padding10 bg-red fg-white text-accent align-center\">You do not have permission to access this module !</div>");
                                    db.close();
                                    new sapi.sapi().endRequest();
                                    Response.End();
                                
                                
                            }

                            ReportViewer1.ProcessingMode = ProcessingMode.Local;
                            ReportDataSource datasource = null;

                            string sql = "";
                            if (Session["rptSQL"] != null)
                                sql = Session["rptSQL"].ToString();

                            if (sql.Length > 0)
                                if (Request.QueryString["report"] != "rptSalePerformance")
                                    sql = " where " + sql;

                            //if (sql.Length > 0)
                            //    if (Request.QueryString["report"] != "rptCommercialInvoice")
                            //        sql = " where " + sql;

                            if (Request.QueryString["report"] == "rptItem")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptItem.rdlc");
                                //ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptInventoryBalance.rdlc");
                                if (sql == "") sql = " Where 1=1 ";
                                datasource = new ReportDataSource("vItem", db.readData("Select * from vItem " + sql +
                                    " and item_isSet is null "));
                            }
                            if (Request.QueryString["report"] == "rptItemWarehouse")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptItemWarehouse.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vItemWarehouse " + sql));
                            }

                            if (Request.QueryString["report"] == "rptReceivePrint")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptReceiveitemPrint.rdlc");
                                datasource = new ReportDataSource("DataSet1", 
                                    db.readData("Select * from vReceive where rece_ReceiveID = " + Request.QueryString["eid"]));


                            }

                            if (Request.QueryString["report"] == "rptCreditNotePrint")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCreditNotePrint.rdlc");
                                datasource = new ReportDataSource("DataSet1",
                                    db.readData("Select * from vCreditNoteDetail where crdn_CreditNoteID = " + Request.QueryString["eid"]));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }

                            if (Request.QueryString["report"] == "rptReceive")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptReceive.rdlc");
                                datasource = new ReportDataSource("vReceive", db.readData("Select * from vReceive " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptIssue")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptIssue.rdlc");
                                datasource = new ReportDataSource("vIssue", db.readData("Select * from vIssue " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }

                            if (Request.QueryString["report"] == "rptStockTransfer")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptStockTransfer.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from rptStockTransfer " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }

                            if (Request.QueryString["report"] == "rptDailySale")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptDailySale.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoice " + sql));
                                if (Request.QueryString["invo_ViewDetail"] != null)
                                {
                                    if (Request.QueryString["invo_ViewDetail"].ToString().ToLower() == "y")
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptDailySaleDetail.rdlc");
                                        datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoiceDetail " + sql));
                                    }
                                }
                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    ReportParameter p = null;
                                    foreach (var pram in param)
                                    {
                                        p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                    string tSql = "";
                                    var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date")))
                                    {
                                        tSql = tSql + " and ivpm_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date_To")))
                                    {
                                        tSql = tSql + " and ivpm_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_WarehouseID")))
                                    {
                                        tSql = tSql + " and ivpm_WarehouseID =" + db.sqlStr(url.Get("invo_WarehouseID"));
                                    }

                                    p = new ReportParameter("Payment", db.cNum(db.readData("ivpm_Amount",
                                        "Select SUM(isNULL(ivpm_Amount,0)) ivpm_Amount From tblInvoicePayment " +
                                        " Where ivpm_Deleted is null " + tSql)).ToString("#,#00.00"));
                                    tSql = "";
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date")))
                                    {
                                        tSql = tSql + " and crdn_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date_To")))
                                    {
                                        tSql = tSql + " and crdn_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_WarehouseID")))
                                    {
                                        tSql = tSql + " and crdn_WarehouseID =" + db.sqlStr((url.Get("invo_WarehouseID")));
                                    }

                                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    p = new ReportParameter("CreditNote", db.cNum(db.readData("crdn_Total",
                                        "Select SUM(isNULL(crdn_Total,0)) crdn_Total From tblCreditNote " +
                                        " Where crdn_Deleted is null " + tSql)).ToString("#,##0.00"));
                                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });

                                    tSql = "";
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date")))
                                    {
                                        tSql = tSql + " and exps_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_Date_To")))
                                    {
                                        tSql = tSql + " and exps_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                    }
                                    if (!string.IsNullOrEmpty(url.Get("invo_WarehouseID")))
                                    {
                                        tSql = tSql + " and exps_WarehouseID =" + db.sqlStr(url.Get("invo_WarehouseID"));
                                    }

                                    p = new ReportParameter("Expense", db.cNum(db.readData("exps_Total",
                                        "Select SUM(isNULL(exps_Total,0)) exps_Total From tblExpense " +
                                        " Where exps_Deleted is null " + tSql)).ToString("#,##0.00"));
                                    this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });

                                }
                            }

                            if (Request.QueryString["report"] == "rptInvoiceBySalesman")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptInvoiceBySalesman.rdlc");
                                datasource = new ReportDataSource("DataSet1",
                                    db.readData("select SUM(invo_GTotal) invo_GTotal,count(1) C,salm_Name from vInvoice " + sql +
                                    " group by salm_Name "));
                                
                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    ReportParameter p = null;
                                    foreach (var pram in param)
                                    {
                                        p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                    

                                }
                            }

                            if (Request.QueryString["report"] == "rptOutstandingInvoice")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptOutstandingInvoice.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoice " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptSalesByCustomer")
                            {
                                if (Request.QueryString["invo_ViewDetail"] != null)
                                {
                                    if (Request.QueryString["invo_ViewDetail"].ToString().ToLower() == "y")
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSalesByCustomerDetail.rdlc");
                                        datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoiceDetail " + sql));
                                    }
                                    else
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSalesByCustomer.rdlc");
                                        datasource = new ReportDataSource("DataSet1", db.readData("select SUM(invo_GTotal) invo_GTotal,cust_Name from vInvoice " + sql +
                                            "Group by cust_Name order by Cust_Name "));
                                    }
                                }

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptSalesByItem")
                            {
                                if (Request.QueryString["invo_ViewDetail"] != null)
                                {
                                    if (Request.QueryString["invo_ViewDetail"].ToString().ToLower() == "y")
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSalesByItemrDetail.rdlc");
                                        datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoiceDetail " + sql));
                                    }
                                    else
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSalesByItem.rdlc");
                                        datasource = new ReportDataSource("DataSet1", db.readData("select item_Name,itmg_Name,SUM(init_Quantity) init_Quantity,SUM(init_Total) init_Total from vInvoiceDetail " + sql +
                                            "Group by item_Name,itmg_Name order by item_Name "));
                                    }
                                }

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptCustomer")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCustomer.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vCustomer " + sql));
                            }

                            if (Request.QueryString["report"] == "rptInactiveCustomer")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptInactiveCustomer.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInactiveCustomer " + sql));
                            }

                            if (Request.QueryString["report"] == "rptSaleOrder")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSaleOrder.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vSaleOrder " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptQuotation")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptQuotation.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vQuotation " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptCreditNote")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCreditNote.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vCreditNoteDetail " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptPayment")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptInvoicePayment.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoicePayment " + sql));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptItemSold")
                            {
                                if (Request.QueryString["invo_ViewDetail"] != null)
                                {
                                    if (Request.QueryString["invo_ViewDetail"].ToString().ToLower() == "y")
                                    {
                                        var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                                        string[] tSql = new string[2];
                                        if (!string.IsNullOrEmpty(url.Get("invo_Date")))
                                        {
                                            tSql[0] = tSql[0] + " and invo_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                            tSql[1] = tSql[1] + " and crdn_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                            
                                        }
                                        if (!string.IsNullOrEmpty(url.Get("invo_Date_To")))
                                        {
                                            tSql[0] = tSql[0] + " and invo_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                            tSql[1] = tSql[1] + " and crdn_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                           
                                        }

                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptItemSoldDetail.rdlc");
                                        datasource = new ReportDataSource("DataSet1",
                                            db.readData("select invo_Date,invo_Name, itmg_Name,item_Name,cust_Name,init_Qty,init_Price,init_Total,invo_GTotal from vInvoiceDetail Where 1=1 " + tSql[0] +
                                            " UNION ALL " + 
                                            " Select crdn_Date invo_Date, crdn_Name invo_Date, itmg_Name,item_Name,cust_Name,-1 * cnit_Qty init_Qty,cnit_Price init_Price,-1 * cnit_Total init_Total,crdn_Total invo_GTotal from vCreditNoteDetail WHere 1=1 " + tSql[1]));
                                    }
                                    else
                                    {
                                        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptItemSold.rdlc");
                                        datasource = new ReportDataSource("DataSet1",
                                            db.readData("select SUM(init_Qty) init_Qty,itmg_Name,item_Name,SUM(cnit_Qty) cnit_Qty,SUM(init_Total) init_Total,SUM(cnit_Total) cnit_Total From (Select * from vItemSold " + sql +
                                            ") A Group by itmg_Name,item_Name"));
                                    }
                                }
                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptIncomeStatement")
                            {
                                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                                string[] tSql = new string[3];
                                if (!string.IsNullOrEmpty(url.Get("invo_Date")))
                                {
                                    tSql[0] = tSql[0] + " and invo_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                    tSql[1] = tSql[1] + " and inco_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                    tSql[2] = tSql[2] + " and exps_Date >=" + db.sqlStr(db.getDate(url.Get("invo_Date")));
                                }
                                if (!string.IsNullOrEmpty(url.Get("invo_Date_To")))
                                {
                                    tSql[0] = tSql[0] + " and invo_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                    tSql[1] = tSql[1] + " and inco_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                    tSql[2] = tSql[2] + " and exps_Date <" + db.sqlStr(DateTime.Parse(db.getDate(url.Get("invo_Date_To"))).ToString("yyyy-MM-dd 23:59:59"));
                                }

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptIncomeStatement.rdlc");
                                datasource = new ReportDataSource("DataSet1",
                                    db.readData(
                                        "Select cDate,SUM(COGS) COGS, SUM(Invoice) Invoice,SUM(Income) Income,SUM(OtherIncome) OtherIncome,SUM(Expense) Expense From (" +
                                        " select cast(invo_Date as date) cDate, " +
                                        " SUM((invo_GTotal - invo_Cost)) Income,  " +
                                        " 0.0 OtherIncome,  " +
                                        " 0.0 Expense,SUM(invo_GTotal) Invoice,SUM(invo_Cost) COGS  " +
                                        " from tblInvoice " +
                                        " where 1=1 " +
                                        tSql[0] +
                                        " group by cast(invo_Date as date),invo_GTotal , invo_Cost " +
                                        " UNION ALL  " +
                                        " select cast(inco_Date as date) cDate, " +
                                        " 0.0 Income,  " +
                                        " SUM(inco_Total) OtherIncome,  " +
                                        " 0.0 Expense,0.0 Invoice,0.0 COGS " +
                                        " from tblIncome " +
                                        " where 1=1 " +
                                        tSql[1] +
                                        " group by cast(inco_Date as date) " +
                                        " UNION ALL  " +
                                        " select cast(exps_Date as date) cDate, " +
                                        " 0.0 Income,  " +
                                        " 0.0 OtherIncome,  " +
                                        " SUM(exps_Total) Expense,0.0 Invoice,0.0 COGS " +
                                        " from tblExpense " +
                                        " where 1=1 " +
                                        tSql[2] +
                                        " group by cast(exps_Date as date) " +
                                        ") A Group by cDate"
                                    ));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptInvoicePrint")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/Invoice/rptInvoice.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vInvoicePrint Where invo_InvoiceID = " + Request.QueryString["pid"]));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }

                            }

                            if (Request.QueryString["report"] == "rptSaleOrderPrint")
                            {

                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/SaleOrder/rptSaleOrder.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vSaleOrderPrint Where sord_SaleOrderID = " + Request.QueryString["pid"]));

                            }

                            if (Request.QueryString["report"] == "rptSalePerformance")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptSalePerformance.rdlc");
                                DataTable tbl = db.readData("exec sales_analyst " + sql);

                                datasource = new ReportDataSource("DataSet1", tbl);
                            }

                            if (Request.QueryString["report"] == "rptIncome")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptIncome.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vIncome " + sql));
                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }

                            if (Request.QueryString["report"] == "rptExpense")
                            {
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptExpense.rdlc");
                                datasource = new ReportDataSource("DataSet1", db.readData("Select * from vExpense " + sql));
                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }

                            if (Request.QueryString["report"] == "rptCommercialInvoice")
                            {
                                DataTable dt= db.readData("Select * from vInvoicePrint WHERE invo_InvoiceID=" + Request.QueryString["pid"]);
                                string invoiceType = dt.Rows[0]["invo_Type"].ToString();
                                if (invoiceType == "Commercial Invoice")
                                {
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCommercialInvoice.rdlc");
                                    datasource = new ReportDataSource("commercialInvoice", dt);
                                }
                                else if (invoiceType == "Tax Invoice")
                                {
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptTaxInvoice.rdlc");
                                    datasource = new ReportDataSource("taxInvoice", dt);
                                }
                                else if (invoiceType == "Invoice")
                                {
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptInvoice.rdlc");
                                    datasource = new ReportDataSource("invoice", dt);
                                }
                                //DataTable dt = db.readData("Select * from vInvoicePrint WHERE invo_InvoiceID=" + Request.QueryString["pid"]);
                                //string ids = Request.QueryString["pid"];
                                

                                //if (Session["rptParam"] != null)
                                //{
                                //    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                //    foreach (var pram in param)
                                //    {
                                //        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                //        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                //    }
                                //}

                            }

                            if (Request.QueryString["report"] == "rptRelocationQuotation")
                            {
                                DataTable dt = db.readData("Select * from vQuotationPrint WHERE quot_QuotationID=" + Request.QueryString["pid"]);
                                string serviceType = dt.Rows[0]["itmg_Name"].ToString().Trim().ToLower();
                                if(serviceType == "Relocation Services".ToLower())
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptRelocationQuotation.rdlc");
                                else if (serviceType == "Khmer Language Services".ToLower())
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptKhmerLanguageQuotation.rdlc");
                                else if(serviceType == "Corporate Services (formation & registrations)".ToLower() || serviceType == "Corporate Services (maintenance, renewal, change & closure)")
                                    ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCoporateSereviceQuotation.rdlc");
                                datasource = new ReportDataSource("relocationQuotaion", dt);
                                
                            }

                            if (Request.QueryString["report"] == "rptCreditNotePrintUpdate")
                            {
                                string id = Request.QueryString["eid"];
                                ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/report/rptCreditNotePrintUpdate.rdlc");
                                datasource = new ReportDataSource("creditNote",
                                    db.readData("Select * from vCreditNoteDetailPrint where crdn_CreditNoteID = " + Request.QueryString["eid"]));

                                if (Session["rptParam"] != null)
                                {
                                    Dictionary<string, string> param = (Dictionary<string, string>)Session["rptParam"];
                                    foreach (var pram in param)
                                    {
                                        ReportParameter p = new ReportParameter(pram.Key, pram.Value);
                                        this.ReportViewer1.LocalReport.SetParameters(new ReportParameter[] { p });
                                    }
                                }
                            }


                            ReportViewer1.LocalReport.DataSources.Clear();
                            /*var setup = ReportViewer1.GetPageSettings();
                            setup.Margins = new System.Drawing.Printing.Margins(0, 0, 0, 0);
                            ReportViewer1.SetPageSettings(setup);*/
                            ReportViewer1.LocalReport.DataSources.Add(datasource);


                            Warning[] warn = null;
                            String[] streamids = null;
                            String mimeType = "application/pdf";
                            String encoding = String.Empty;
                            String extension = String.Empty;
                            Byte[] byteViewer;

                            if (Request.QueryString["isExp"] == null)
                            {
                                // Pdf - Default Export
                                byteViewer = ReportViewer1.LocalReport.Render("pdf", null, out mimeType, out encoding, out extension, out streamids, out warn);
                                Response.ContentType = "application/pdf";
                                Response.AddHeader("content-disposition", "inline;filename=" + Request.QueryString["report"] + ".pdf");
                            }
                            else
                            {
                                // Excel
                                
                                mimeType = "application/Excel";
                                byteViewer = ReportViewer1.LocalReport.Render("Excel", null, out mimeType, out encoding, out extension, out streamids, out warn);
                                Response.ContentType = "application/Excel";
                                Response.AddHeader("content-disposition", "inline;filename=" + Request.QueryString["report"] + ".xls");
                                
                            }
                            Response.Buffer = true;
                            Response.Clear();
                            Response.BinaryWrite(byteViewer);
                            Response.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }
                finally
                {
                    db.close();
                }
            }
        }

    }
}