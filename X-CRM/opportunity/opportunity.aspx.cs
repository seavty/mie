using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.opportunity
{
    public partial class opportunity : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        //string screen = "tblOpportunityNew;tblOpportunityCompanyInfo;tblOpportunityTotal";
        string screen = "tblOpportunityNew;";
        string screenItem = "tblOpportunityItemNew";
        string screenItemList = "tblOpportunityItemList2";
        string frm = "frmMaster";
        string IDFIeld = "oppo_opportunityid";
        string Tab = "tblOpportunity";
        string cTab = "tblOpportunity";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            try
            {
                if (db.connect())
                {

                    if (Request.QueryString["app"] != null)
                    {
                        if (Request.QueryString["app"].ToString() == "SSA")
                        {
                            string filter = "";

                            if (url.Get("colName") == "empl_LeaveSchemeID")
                            {
                                if (!string.IsNullOrEmpty(url.Get("empl_DepartmentID")))
                                {
                                    filter = " lvsc_LeaveSchemeID = " + url.Get("empl_DepartmentID");
                                }

                            }

                            Response.Write(cls.loadSSA(db,
                                Request.QueryString["colid"].ToString(),
                                Request.QueryString["q"].ToString(), filter: filter));
                            cls.endRequest();
                            Response.End();
                        }
                    }

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(saveRecord());
                        }

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            if (Request.Form["mode"].ToString() == "3")
                            {
                                cls.Mode = global::sapi.sapi.recordMode.Edit;
                            }
                            else
                            {
                                cls.Mode = global::sapi.sapi.recordMode.View;
                            }
                            Response.Write(loadScreen(Request.Form["eid"].ToString(), cls.Mode));
                        }
                        if (Request.Form["app"].ToString() == "delRecord")
                        {
                            var tmp = Request.Form["eid"].ToString();
                            string re = cls.delRecord(screen,Request.Form["eid"].ToString(),db);
                            Response.Write(cls.delRecord("tblOpportunityNew", Request.Form["eid"].ToString(), db));

                            try
                            {
                                //db.beginTran();
                                DataTable dt = db.readData("SELECT * FROM tblProjectInvoiceItem WHERE ipit_Deleted IS NULL AND ipit_ProjectID=" + url.Get("oppo_opportunityid"));
                                if (dt.Rows.Count > 0)
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        db.execData("UPDATE tblInvoiceItem SET init_IsConvert = NULL WHERE init_InvoiceItemID = " + dr["ipit_InvoiceItemID"]);
                                    }
                                }
                                //db.commit();
                            }
                            catch (Exception ex)
                            {
                                db.rollback();
                                Response.Write(ex.Message);
                            }
                        }
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = true;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = global::sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;

                        }

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen(eid, cls.Mode);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, Tab, cTab, eid: eid, showCTab: showCTab);

                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                //throw;
            }
            finally { db.close(); }
        }

        void init()
        {
            url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

            vals.Clear();

            foreach (var st in Request.Form.AllKeys)
            {
                vals.Add(st.ToLower(), Request.Form[st].ToString());
            }

        }

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            //cls.Mode = mode;
            //if (eid == "0")
            //{
            //    cls.Mode = global::sapi.sapi.recordMode.New;
            //}
            //string re = "";
            //re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            //return re;
            string re = "";
            string list = "";
            string topTitle = "";
            DataTable tblTop = null;
            cls.Mode = mode;

            if (!string.IsNullOrEmpty(url.Get("quot_quotationid")))
            {
                DataTable dt = db.readData("SELECT oppo_CustomerID FROM tblOpportunity WHERE oppo_Deleted IS NULL AND oppo_OpportunityID=" + eid);
                DataTable dtCus = db.readData("SELECT * FROM tblCustomer WHERE cust_CustomerID=" + dt.Rows[0]["oppo_CustomerID"].ToString());
                foreach (DataRow row in dtCus.Rows)
                {
                    sapi.defaultValue.add("oppo_Phone", row["cust_Phone"].ToString());
                    sapi.defaultValue.add("oppo_Email", row["cust_Email"].ToString());
                    sapi.defaultValue.add("oppo_Address", row["cust_Address"].ToString());
                }
                
            }

            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and opit_OpportunityID = " + eid, cPage: -1) +
                       "</div>";
                sapi.defaultValue.add("oppo_Date",DateTime.Now.ToShortDateString());
                sapi.defaultValue.add("oppo_StartDate", DateTime.Now.ToShortDateString());
                sapi.defaultValue.add("oppo_Status","New");
                string invoID = url.Get("invo_invoiceid");
                string invoNo = db.readData("IN", "SELECT invo_Name AS [IN] FROM tblInvoice WHERE invo_InvoiceID=" + invoID);
                sapi.defaultValue.add("oppo_InvoiceNo", invoNo);
            }

            if (mode == sapi.sapi.recordMode.View)
            {
                string div = cls.findRecord(db, "tblProjectInvoiceItemNew", "tblProjectInvoiceItemList", "frmList", null, "",
                    mode: 0, filter: " and ipit_ProjectID = " + eid, cPage: -1, assc: 0);

                list = "<div id='dvList'>" + div +  cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and opit_OpportunityID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                    

                DataTable dt = db.readData("SELECT * FROM tblOpportunity WHERE oppo_OpportunityID=" + url.Get("oppo_opportunityid"));
                if (dt.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dt.Rows[0]["oppo_QuotationID"].ToString()))
                    {
                        sapi.Buttons.add("View Quotation", "history", "warning", "ViewFromQuotation(" + dt.Rows[0]["oppo_QuotationID"] + ")"); //View From Quotation

                        /*
                        sapi.Buttons.add("Convert To Invoice", "dollar2", "",
                            "loadFormInput(" + eid + ")");
                        */
                        if (string.IsNullOrEmpty(dt.Rows[0]["oppo_Converted"].ToString()))
                        {
                            sapi.Buttons.add("Convert To Invoice", "dollar2", "",
                                "convertToInvoice(" + eid + ")");
                        }
                    }
                    if (!string.IsNullOrEmpty(dt.Rows[0]["oppo_InvoiceID"].ToString()))
                    {
                        sapi.Buttons.add("View Invoice", "arrow-left", "info", "window.location = '../invoice/invoice.aspx?invo_invoiceid=" + dt.Rows[0]["oppo_InvoiceID"] + "';");
                    }
                }
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and opit_OpportunityID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                topTitle = "Project";
                tblTop = db.readData("SELECT oppo_Name, cust_Name FROM vOpportunity WHERE oppo_Deleted IS NULL AND oppo_OpportunityID=" + eid);
            }

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list;
            return re;
        }

        string saveRecord()
        {
            //string re = "";
            //Dictionary<string, string> aVal = new Dictionary<string, string>();
            //re = cls.saveRecord(screen, vals, db, aVals: aVal);
            //return re;

            DateValidation();

            string re = "";
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();

            if (vals.ContainsKey("oppo_CustomerID".ToLower()))
            {
                string sql = "SELECT * FROM tblCustomer WHERE cust_CustomerID=" + vals["oppo_CustomerID".ToLower()].ToString();
                DataTable dt = db.readData(sql);
                foreach (DataRow dr in dt.Rows)
                {
                    vals["oppo_Code".ToLower()] = dr["cust_Code"].ToString();
                    vals["oppo_VATTIN".ToLower()] = dr["cust_VATTIN"].ToString();
                }
            }
            if (!string.IsNullOrEmpty(url.Get("invo_invoiceid")))
            {
                aVal.Add("oppo_InvoiceID".ToLower(), url.Get("invo_invoiceid"));
            }
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("opit_OpportunityID", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["opit_OpportunityItemID".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
                                    }
                                }

                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }
                    }
                    db.commit();
                }
            }
            return re;
        }


        private void DateValidation()
        {
            if (vals.ContainsKey("oppo_StartDate".ToLower()) && vals.ContainsKey("oppo_EndDate".ToLower()))
            {
                if (vals["oppo_StartDate".ToLower()] != "" && vals["oppo_EndDate".ToLower()] != "")
                {
                    if (DateTime.Parse(db.getDate(vals["oppo_StartDate".ToLower()], 0)) >
                        DateTime.Parse(db.getDate(vals["oppo_EndDate".ToLower()], 0)))
                    {
                        DataTable tblResult = new DataTable();
                        tblResult.Rows.Add();
                        tblResult.Columns.Add("colName");
                        tblResult.Columns.Add("msg");
                        tblResult.Rows[0]["colName"] = "oppo_StartDate";
                        tblResult.Rows[0]["msg"] = "Start Date cannot greater than End Date";
                        string jsonError = ("{\"error\":" + db.tblToJson(tblResult) + "}");

                        Response.Clear();
                        Response.Write(jsonError);
                        db.close();
                        //Response.End();
                        cls.endRequest();

                        
                    }

                }
            }
        }
    }

        
}