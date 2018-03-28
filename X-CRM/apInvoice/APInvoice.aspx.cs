using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.apInvoice
{
    public partial class APInvoice : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblAPInvoiceNew";
        string screenItem = "tblAPInvoiceItemNew";
        string screenItemList = "tblAPInvoiceItemList";
        string frm = "frmMaster";
        string IDFIeld = "apiv_apinvoiceid";
        string Tab = "tblAPInvoice";
        string cTab = "tblAPInvoice";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "";
                            if (Request.Form["colName"].ToString() == "quot_CustomerID")
                            {
                                if (url.Get("invo_OpportunityID") != "")
                                {
                                    DataTable tmp = db.readData("Select oppo_CustomerID From tblOpportunity Where oppo_OpportunityID=" + Request.Form["quot_OpportunityID"].ToString());
                                    foreach (DataRow tmpRow in tmp.Rows)
                                    {
                                        filter = " cust_CustomerID = " + tmpRow["oppo_CustomerID"].ToString();
                                    }
                                }

                            }
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
                            Response.Write(cls.delRecord(screen, Request.Form["eid"].ToString(), db));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = true;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;
                        }
                        else
                        {
                            
                            {

                                if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                                {
                                    sapi.defaultValue.add("apiv_CustomerID", url.Get("cust_customerid").ToString());
                                }
                                sapi.defaultValue.add("apiv_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                                
                            }
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
                throw;
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

        string loadScreen(string eid, sapi.sapi.recordMode mode)
        {
            string re = "";
            string list = "";
            string topTitle = "";
            DataTable tblTop = null;
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and apit_APInvoiceID = " + eid, cPage: -1) +
                       "</div>";
                sapi.defaultValue.add("apiv_Status", "New");
                


            }

            if (mode == sapi.sapi.recordMode.View)
            {

                //cls.showWorkflow = true;

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and apit_APInvoiceID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                if (eid != "0")
                {
                    tblData = db.readData("Select * from tblAPInvoice WHere apiv_Deleted is null and apiv_APInvoiceID=" + eid);
                    foreach (DataRow row in tblData.Rows)
                    {
                        if (row["apiv_Status"].ToString().ToLower() == "completed")
                        {
                            cls.hideDelete = true;
                            cls.hideEdit = true;


                        }
                        
                        if (db.cNum(row["apiv_Balance"].ToString().ToLower()) > 0)
                        {
                            sapi.Buttons.add("Payment", "dolBlar2", "warning",
                                            "payment(" + eid + ");","I","tblAPInvoicePayment");
                        }
                        else
                        {
                            //sapi.Buttons.add("Return", "undo", "info", "returnInv(" + eid + ")");
                        }

                        sapi.Buttons.add("Print", "print", "success", "printInv(" + eid + ")");

                    }
                }

            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and apit_APInvoiceID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //topTitle = "Invoice";
                //tblTop = db.readData("Select invo_Name,invo_Date,invo_Total From tblInvoice Where invo_Deleted is null and invo_InvoiceID = " + eid);
            }

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list;
            return re;
        }

        string saveRecord()
        {
            string re = "";
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            db.beginTran();
            if (!vals.ContainsKey("apiv_invoiceid"))
            {
                aVal.Add("apiv_WorkflowID", "6");
                aVal.Add("apiv_WorkflowItemID", "12");
            }
            if (vals.ContainsKey("apiv_status"))
                vals["apiv_status"] = "completed";
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;

                    /*
                    if (!vals.ContainsKey("invo_invoiceid"))
                    {
                        if (vals.ContainsKey("invo_customerid"))
                        {
                            if (!string.IsNullOrEmpty(vals["invo_customerid"]))
                            {
                                var tmp = db.execData("Update tblCustomer Set cust_Type='Customer' Where cust_Type='Lead' and cust_CustomerID=" +
                                    vals["invo_customerid"].ToString());
                                if (tmp != "ok")
                                {
                                    db.rollback();

                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = tmp;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                    return re;
                                }
                            }
                        }
                    }*/

                    double qty = 0;
                    if (Request.Form.GetValues("N") != null)
                    {


                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            aVal.Add("apit_APInvoiceID", hid);

                            if (vals["txtdel" + st] != "")
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                cls.delRecord(screenItem, vals["apit_APInvoiceItemID".ToLower() + st], db);
                                if (!string.IsNullOrEmpty(v["apit_APInvoiceItemID".ToLower()]))
                                {

                                }
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double exQty = 0;

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("apit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["apit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("apit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["apit_Price".ToLower()]);
                                }

                                if (v.ContainsKey("apit_Total".ToLower()))
                                {
                                    v["apit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                }
                                /*
                                if (!v.ContainsKey("apit_APInvoiceItemID".ToLower() + st))
                                {
                                    aVal.Add("apit_BQty", quit_Qty.ToString());
                                }
                                aVal.Add("apit_RPrice", quit_Price.ToString());
                                */
                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
                                    }
                                    else
                                    {

                                        
                                        string id = (string)str.tbl[0].msg;
                                        
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
                    new clsGlobal().APInvoiceTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        
    }
}