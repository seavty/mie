using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.report
{
    public partial class _default : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemFind";
        string grid = "tblItemList";
        string frm = "frmMaster";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                if (db.connect())
                {
                    url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);


                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "";

                            Response.Write(cls.loadSSA(db,
                                Request.Form["colid"].ToString(),
                                Request.Form["q"].ToString(), filter: filter));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                    }

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "findRecord")
                        {
                            string sql = "";
                            Dictionary<string, string> param = null;
                            foreach (var st in Request.Form.AllKeys)
                            {
                                if (st != null)
                                    if (st.ToLower() != "app")
                                    {
                                        if (url.Get("report").ToLower() == "rptreceive".ToLower() ||
                                            url.Get("report").ToLower() == "rptissue".ToLower() ||
                                            url.Get("report").ToLower() == "rptdailysale".ToLower() ||
                                            url.Get("report").ToLower() == "rptoutstandinginvoice".ToLower() ||
                                            url.Get("report").ToLower() == "rptsaleorder".ToLower() ||
                                            url.Get("report").ToLower() == "rptquotation".ToLower() ||
                                            url.Get("report").ToLower() == "rptcreditnote".ToLower() ||
                                            url.Get("report").ToLower() == "rptitemsold".ToLower() ||
                                            url.Get("report").ToLower() == "rptPayment".ToLower() ||
                                            url.Get("report").ToLower() == "rptIncomeStatement".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByCustomer".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByCustomerDetail".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByItem".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByItemDetail".ToLower() ||
                                            url.Get("report").ToLower() == "rptStockTransfer".ToLower() ||
                                            url.Get("report").ToLower() == "rptExpense".ToLower() ||
                                            url.Get("report").ToLower() == "rptIncome".ToLower() ||
                                            url.Get("report").ToLower() == "rptInvoiceBySalesman".ToLower() ||
                                            url.Get("report").ToLower() == "rptInvoiceReport".ToLower()
                                            )
                                            if (param == null)
                                                param = new Dictionary<string, string>();
                                        vals.Add(st.ToLower(), Request.Form[st].ToString());
                                    }
                            }

                            foreach (var st in Request.Form.AllKeys)
                            {
                                if (st != null)
                                    if (st.ToLower() != "app")
                                    {
                                        /*if (url.Get("report").ToLower() == "rptreceive".ToLower() ||
                                            url.Get("report").ToLower() == "rptissue".ToLower() ||
                                            url.Get("report").ToLower() == "rptdailysale".ToLower() ||
                                            url.Get("report").ToLower() == "rptoutstandinginvoice".ToLower() ||
                                            url.Get("report").ToLower() == "rptsaleorder".ToLower() ||
                                            url.Get("report").ToLower() == "rptquotation".ToLower() ||
                                            url.Get("report").ToLower() == "rptcreditnote".ToLower() ||
                                            url.Get("report").ToLower() == "rptitemsold".ToLower() ||
                                            url.Get("report").ToLower() == "rptPayment".ToLower() ||
                                            url.Get("report").ToLower() == "rptIncomeStatement".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByCustomer".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByCustomerDetail".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByItem".ToLower() ||
                                            url.Get("report").ToLower() == "rptSalesByItemDetail".ToLower() ||
                                            url.Get("report").ToLower() == "rptStockTransfer".ToLower()
                                            )
                                            if (param == null)
                                                param = new Dictionary<string, string>();
                                        vals.Add(st.ToLower(), Request.Form[st].ToString());*/
                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                        { 
                                            if (url.Get("report").ToLower() == "rptInvoiceReport".ToLower())
                                            {
                                                if (url.Get("report").ToLower() == "rptInvoiceReport".ToLower())
                                                {
                                                    param = new Dictionary<string, string>();
                                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                                        " To " + vals["invo_Date_To".ToLower()]);
                                                    Session["rptParam"] = param;
                                                }

                                                if (st.ToLower() == "invo_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "invo_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptitem".ToLower())
                                            {

                                                if (st.ToLower() == "item_ItemGroupID".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    sql = sql + " and lower(" + st + ") like " + db.sqlStrLike(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptitemwarehouse".ToLower())
                                            {
                                                sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                            }

                                            if (url.Get("report").ToLower() == "rptIncome".ToLower())
                                            {
                                                if (st.ToLower() == "inco_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "inco_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (inco_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptExpense".ToLower())
                                            {
                                                if (st.ToLower() == "exps_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "exps_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (exps_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            

                                            if (url.Get("report").ToLower() == "rptreceive".ToLower())
                                            {
                                                if (st.ToLower() == "rece_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "rece_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (rece_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptissue".ToLower())
                                            {
                                                if (st.ToLower() == "issu_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "issu_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (issu_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptstocktransfer".ToLower())
                                            {

                                                if (st.ToLower() == "sttf_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "sttf_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (sttf_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (st.Contains("_mm") && st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptdailysale".ToLower())
                                            {
                                                if (st.ToLower() != "invo_viewdetail")
                                                {
                                                    if (st.ToLower() == "invo_Date".ToLower())
                                                    {
                                                        string fr = "";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr );
                                                    }
                                                    else if (st.ToLower() == "invo_Date_To".ToLower())
                                                    {
                                                        string to = " 23:59:59";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }

                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                    }
                                                    else
                                                    {
                                                        if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                    }
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptsalesbycustomer".ToLower())
                                            {
                                                if (st.ToLower() != "invo_viewdetail")
                                                {
                                                    if (st.ToLower() == "invo_Date".ToLower())
                                                    {
                                                        string fr = "";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                    }
                                                    else if (st.ToLower() == "invo_Date_To".ToLower())
                                                    {
                                                        string to = " 23:59:59";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                    }
                                                    else
                                                    {
                                                        if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                    }
                                                }
                                                
                                            }

                                            if (url.Get("report").ToLower() == "rptsalesbyitem".ToLower())
                                            {
                                                if (st.ToLower() != "invo_viewdetail")
                                                {
                                                    if (st.ToLower() == "invo_Date".ToLower())
                                                    {
                                                        string fr = "";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                    }
                                                    else if (st.ToLower() == "invo_Date_To".ToLower())
                                                    {
                                                        string to = " 23:59:59";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                    }
                                                    else
                                                    {
                                                        if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                    }
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptoutstandinginvoice".ToLower())
                                            {
                                                if (st.ToLower() == "invo_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "invo_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                //if (sql.Length > 0)
                                                sql = sql + " and isNULL(invo_Balance,0)>0 ";
                                            }

                                            if (url.Get("report").ToLower() == "rptCustomer".ToLower())
                                            {

                                                if (st.ToLower() == "cust_Type".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    sql = sql + " and lower(" + st + ") like " + db.sqlStrLike(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptInactiveCustomer".ToLower())
                                            {

                                                if (st.ToLower() == "cust_Type".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    sql = sql + " and lower(" + st + ") like " + db.sqlStrLike(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptsaleorder".ToLower())
                                            {
                                                if (st.ToLower() == "sord_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "sord_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (sord_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else if (st.ToLower() == "sord_isComplete".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptCreditNote".ToLower())
                                            {
                                                if (st.ToLower() == "crdn_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "crdn_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (crdn_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else if (st.ToLower() == "crdn_CustomerID".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") like " + db.sqlStrLike(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptquotation".ToLower())
                                            {
                                                if (st.ToLower() == "quot_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "quot_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (quot_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else if (st.ToLower() == "quot_CustomerID".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptpayment".ToLower())
                                            {
                                                if (st.ToLower() == "ivpm_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "ivpm_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (ivpm_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else if (st.ToLower() == "ivpm_InvoiceID".ToLower())
                                                {
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                    sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptItemSold".ToLower())
                                            {
                                                if (st.ToLower() != "invo_viewdetail")
                                                {
                                                    if (st.ToLower() == "invo_Date".ToLower())
                                                    {
                                                        string fr = "";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                    }
                                                    else if (st.ToLower() == "invo_Date_To".ToLower())
                                                    {
                                                        string to = " 23:59:59";
                                                        if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                        {
                                                            to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                        }
                                                        if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                            sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                    }
                                                    else
                                                    {
                                                        if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                    }
                                                }
                                            }

                                            if (url.Get("report").ToLower() == "rptInvoiceBySalesman".ToLower())
                                            {
                                                
                                                if (st.ToLower() == "invo_Date".ToLower())
                                                {
                                                    string fr = "";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        fr = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (" + st + ") >= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + fr);
                                                }
                                                else if (st.ToLower() == "invo_Date_To".ToLower())
                                                {
                                                    string to = " 23:59:59";
                                                    if (vals.ContainsKey(st.ToLower() + "_hh"))
                                                    {
                                                        to = " " + vals[st.ToLower() + "_hh"] + ":" + vals[st.ToLower() + "_mm"];
                                                    }
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + " and (invo_Date) <= " + db.sqlStr(db.getDate(vals[st.ToLower()]) + to);
                                                }
                                                else
                                                {
                                                    if (!st.Contains("_mm") && !st.Contains("_hh"))
                                                        sql = sql + " and lower(" + st + ") = " + db.sqlStr(vals[st.ToLower()]);
                                                }
                                                
                                            }


                                            if (url.Get("report").ToLower() == "rptsaleperformance".ToLower())
                                            {

                                                if (st.ToLower() == "culg_Date".ToLower())
                                                {

                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + db.sqlStrN(db.getDate(vals[st.ToLower()])) + ",";
                                                }
                                                else if (st.ToLower() == "culg_Date_To".ToLower())
                                                {
                                                    if (!string.IsNullOrEmpty(vals[st.ToLower()]))
                                                        sql = sql + db.sqlStrN(db.getDate(vals[st.ToLower()]) + " 23:59:59") + ",";
                                                }
                                                else if (st.ToLower() == "culg_SalesmanID".ToLower())
                                                {
                                                    sql = sql + db.cNum(vals[st.ToLower()]) + ",";
                                                }
                                            }

                                            
                                        }
                                    }
                            }
                            if (url.Get("report").ToLower() == "rptsaleperformance".ToLower())
                            {
                                if (!vals.ContainsKey("culg_SalesmanID".ToLower()))
                                    sql = "0," + sql;
                                sql = sql + "1";
                            }
                            else
                            {
                                if (sql.Length > 0)
                                    sql = " 1=1 " + sql;
                            }
                            if (param != null)
                            {
                                if (url.Get("report").ToLower() == "rptreceive".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["rece_Date".ToLower()] +
                                        " To " + vals["rece_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }
                                if (url.Get("report").ToLower() == "rptissue".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["issu_Date".ToLower()] +
                                        " To " + vals["issu_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptdailysale".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptInvoiceBySalesman".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptincomestatement".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptoutstandinginvoice".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptSaleOrder".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["sord_Date".ToLower()] +
                                        " To " + vals["sord_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptQuotation".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["quot_Date".ToLower()] +
                                        " To " + vals["quot_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptCreditNote".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["crdn_Date".ToLower()] +
                                        " To " + vals["crdn_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptPayment".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["ivpm_Date".ToLower()] +
                                        " To " + vals["ivpm_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptItemSold".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptSalesByCustomer".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptSalesByItem".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["invo_Date".ToLower()] +
                                        " To " + vals["invo_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptstocktransfer".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["sttf_Date".ToLower()] +
                                        " To " + vals["sttf_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptExpense".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["exps_Date".ToLower()] +
                                        " To " + vals["exps_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                                if (url.Get("report").ToLower() == "rptIncome".ToLower())
                                {
                                    param = new Dictionary<string, string>();
                                    param.Add("Title", "From " + vals["inco_Date".ToLower()] +
                                        " To " + vals["inco_Date_To".ToLower()]);
                                    Session["rptParam"] = param;
                                }

                            }
                            Session["rptSQL"] = sql;

                            Response.Write("ok" + url.Get("report"));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "", "", 1, showCTab: true);
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally { db.close(); }
        }

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            screen = url.Get("report");
            if (url.Get("report").ToLower() == "rptoutstandinginvoice".ToLower())
            {
                screen = "rptDailySale";
            }
            if (url.Get("report").ToLower() == "rptInactiveCustomer".ToLower())
            {
                screen = "rptCustomer";
            }
            if (url.Get("report").ToLower() == "rptsaleperformance".ToLower())
            {
                screen = "rptCustomerLog";
            }
            
           

            bool isOk = true;
            string re = "";
            string topContentHeader = "";
            sapi.sapi cls = new sapi.sapi();
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {
                    
                    topContentHeader = cls.getString(url.Get("report").ToLower(), db);
                    DataTable tblPermission = db.readData(
                    "Select * from ( " +
                    " select mmnu_Name smnu_Name, mmnu_MainMenuID smnu_SubMenuID,mmnu_Text smnu_Text,'1' _type from sys_mainMenu " +
                    " where mmnu_GroupID = 4 and mmnu_Url is not null " +
                    " UNION all " +
                    " select smnu_Name,smnu_SubMenuID,smnu_Text,'2' _type from sys_subMenu " +
                    " where smnu_Url is not null and smnu_MainMenuID is not null " +
                    " ) A " +
                    " inner join [dbo].[sys_ReportPermission] on rppm_ReportID = smnu_SubMenuID " +
                    " and rppm_ProfileID in (" + Session["profiles"].ToString() + ")" +
                    " Where rppm_V = 'Y' and smnu_Name = " + db.sqlStr(url.Get("report")));
                    if (tblPermission.Rows.Count <= 0)
                    {
                        re = ("<div class=\"padding10 bg-red fg-white text-accent align-center\">You do not have permission to access this module !</div>");
                        isOk = false;
                    }
                }
            }
            catch (Exception ex) { }
            finally { db.close(); }

            if (isOk)
            {
                cls.Mode = mode;
                cls.scrnType = global::sapi.sapi.screenType.SearchScreen;
                sapi.Buttons.add("Export", " file-excel", "warning", "exp();");
                if (eid == "0")
                    cls.Mode = global::sapi.sapi.recordMode.New;


                re = cls.loadScreen(db, screen, frm, ref tblData, eid, topContentHeader: topContentHeader);
            }
            return re;
        }
    }
}