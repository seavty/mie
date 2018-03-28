using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.accounting
{
    public partial class journal : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblJournalNew";
        string screenItem = "tblJournalDetailNew";
        string screenItemList = "tblJournalDetailList";
        string frm = "frmMaster";
        string IDFIeld = "jour_journalid";
        string Tab = "";
        string cTab = "";

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
                            //if (url.Get("colName") == "quot_CustomerID")
                            //{
                            //    if (!string.IsNullOrEmpty(url.Get("quot_OpportunityID")))
                            //    {
                            //        DataTable tmp = db.readData("Select oppo_CustomerID From tblOpportunity Where oppo_OpportunityID=" + url.Get("quot_OpportunityID"));
                            //        foreach (DataRow tmpRow in tmp.Rows)
                            //        {
                            //            filter = " cust_CustomerID = " + tmpRow["oppo_CustomerID"].ToString();
                            //        }
                            //    }

                            //}

                            //if (url.Get("colName") == "quot_OpportunityID")
                            //{
                            //    if (!string.IsNullOrEmpty(url.Get("quot_CustomerID")))
                            //    {
                            //        filter = " oppo_CustomerID = " + url.Get("quot_CustomerID");
                            //    }

                            //}

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
                            //if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                            //{
                            //    sapi.defaultValue.add("quot_CustomerID", url.Get("cust_customerid"));
                            //}

                            sapi.defaultValue.add("jour_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"));
                            //sapi.defaultValue.add("quot_WarehouseID", db.readData("sett_WarehouseID", "Select sett_WarehouseID From sys_Setting"));

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
                                mode: 1, filter: " and joud_JournalID = " + eid, cPage: -1) +
                       "</div>";

                //if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                //{
                //    DataTable tbl = db.readData("Select * from tblOpportunity Where oppo_Deleted is null and oppo_OpportunityID=" + url.Get("oppo_opportunityid"));
                //    foreach (DataRow row in tbl.Rows)
                //    {
                //        sapi.defaultValue.add("quot_OpportunityID", url.Get("oppo_opportunityid"));
                //        sapi.defaultValue.add("quot_CustomerID", row["oppo_CustomerID"].ToString());
                //    }

                //}
            }

            if (mode == sapi.sapi.recordMode.View)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and joud_JournalID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and joud_JournalID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //sapi.Buttons.add("Convert to SO", "arrow-right", "info", "convertSO(" + eid + ")");
                //sapi.Buttons.add("Convert to Invoice", "arrow-right", "info", "convertInv(" + eid + ")");
                //topTitle = "Quotation";
                //tblTop = db.readData("Select quot_Name,quot_Date,quot_Total,quot_QuotationID From tblQuotation Where quot_Deleted is null and quot_QuotationID=" + eid);
            }

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list;
            return re;
        }

        string saveRecord()
        {
            string re = "";
            string hid = "";

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    //if (!vals.ContainsKey("quot_quotationid"))
                    //{
                    //    if (vals.ContainsKey("quot_customerid"))
                    //    {
                    //        if (!string.IsNullOrEmpty(vals["quot_customerid"]))
                    //        {
                    //            var tmp = db.execData("Update tblCustomer Set cust_Type='Customer' Where cust_Type='Lead' and cust_CustomerID=" +
                    //                vals["quot_customerid"].ToString());
                    //            if (tmp != "ok")
                    //            {
                    //                db.rollback();
                    //                DataTable tblResult = new DataTable();
                    //                tblResult.Rows.Add();
                    //                tblResult.Columns.Add("status");
                    //                tblResult.Columns.Add("msg");
                    //                tblResult.Rows[0]["status"] = "error";
                    //                tblResult.Rows[0]["msg"] = tmp;
                    //                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    //                return re;
                    //            }
                    //        }
                    //    }
                    //}

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("joud_JournalID", hid);

                        string re2 = "";
                        double debit = 0;
                        double credit = 0;
                        Int16 countDebit = 0;
                        Int16 countCredit = 0;

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["joud_JournalDetailID".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                if (db.cNum(v["joud_debit"].ToString()) > 0) countDebit += 1;
                                if (db.cNum(v["joud_credit"].ToString()) > 0) countCredit += 1;

                                debit += db.cNum(v["joud_debit"].ToString());
                                credit += db.cNum(v["joud_credit"].ToString());

                                //double quit_Qty = 0;
                                //double quit_Price = 0;

                                //if (v.ContainsKey("quit_Qty".ToLower()))
                                //{
                                //    quit_Qty = db.cNum(v["quit_Qty".ToLower()]);
                                //}
                                //if (v.ContainsKey("quit_Price".ToLower()))
                                //{
                                //    quit_Price = db.cNum(v["quit_Price".ToLower()]);
                                //}

                                //if (v.ContainsKey("quit_Total".ToLower()))
                                //{
                                //    v["quit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                //}

                                re2 = cls.saveRecord(screenItem, v, db, aVal, st);
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

                        if (countDebit > 1 && countCredit > 1)
                        {
                            db.rollback();

                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = "Many debit lines within many credit lines are not allowed !";
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            return re;
                        }
                        else
                        {
                            if (debit != credit)
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "Debit and Credit is unbalance !";
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }
                    }
                    //quotationTotal(hid,db);
                    db.commit();
                }
            }
            return re;
        }

        //void quotationTotal(string eid,sapi.db db)
        //{
        //    string quot_Discount = "";
        //    double quot_Disc = 0;
        //    double quot_SubTotal = 0;
        //    double quot_DiscountAmount = 0;
        //    double quot_Total = 0;

        //    DataTable tbl = db.readData("Select * From tblQuotation " +
        //        " Where quot_Deleted is null and quot_QuotationID = " + eid);
        //    foreach (DataRow row in tbl.Rows)
        //    {
        //        quot_Discount = row["quot_Discount"].ToString();
        //        quot_Disc = db.cNum(row["quot_Disc"].ToString());
        //    }

        //    tbl = db.readData("Select SUM(isNull(quit_Total,0)) quit_Total From tblQuotationItem " +
        //        " Where quit_Deleted is null and quit_QuotationID = " + eid);
        //    foreach (DataRow row in tbl.Rows)
        //    {
        //        quot_SubTotal = db.cNum(row["quit_Total"].ToString());
        //        if (quot_Discount == "P")
        //        {
        //            quot_DiscountAmount = (quot_SubTotal * quot_Disc / 100);
        //        }
        //        else
        //        {
        //            quot_DiscountAmount = quot_Disc;
        //        }
        //        quot_Total = quot_SubTotal - quot_DiscountAmount;

        //        db.execData("Update tblQuotation Set " +
        //            " quot_SubTotal = " + quot_SubTotal +
        //            ",quot_DiscountAmount = " + quot_DiscountAmount +
        //            ",quot_Total = " + quot_Total +
        //            " Where quot_QuotationID = " + eid
        //            );
        //    }
        //}
    }
}