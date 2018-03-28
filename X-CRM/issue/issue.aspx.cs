using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.issue
{
    public partial class issue : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblIssueNew";
        string screenItem = "tblIssueItemNew";
        string screenItemList = "tblIssueItemList";
        string frm = "frmMaster";
        string IDFIeld = "issu_issueid";
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

                            sapi.defaultValue.add("issu_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
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
                                mode: 1, filter: " and isit_IssueID = " + eid, cPage: -1) +
                       "</div>";
            }
            if (mode == sapi.sapi.recordMode.View)
            {
                cls.hideDelete = true;
                cls.hideEdit = true;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and isit_IssueID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and isit_IssueID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
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
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    if (!vals.ContainsKey("issu_issueid"))
                    {

                    }

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("isit_issueid", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["isit_issueitemid".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("isit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["isit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("isit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["isit_Price".ToLower()]);
                                }

                                if (v.ContainsKey("isit_Total".ToLower()))
                                {
                                    v["isit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                }
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
                                        DataTable tblResult = new DataTable();
                                        tblResult.Rows.Add();
                                        tblResult.Columns.Add("status");
                                        tblResult.Columns.Add("msg");

                                        var tmp = db.execData("Update tblItemWarehouse Set " +
                                            " itwh_Qty = isNULL(itwh_Qty,0) - " + db.cNum(v["isit_qty"].ToString()) +
                                            " where itwh_WarehouseID = " + db.sqlStr(vals["issu_warehouseid"].ToString()) +
                                            " and itwh_ItemID = " + db.sqlStr(v["isit_itemid"].ToString())
                                            );
                                        if (tmp != "ok")
                                        {
                                            db.rollback();

                                            tblResult.Rows[0]["status"] = "error";
                                            tblResult.Rows[0]["msg"] = tmp;
                                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                            return re;
                                        }
                                        else
                                        {

                                            tmp = db.execData("Declare @tot decimal(18,6) " +
                                                " Select @tot = SUM(itwh_Qty) " +
                                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(v["isit_itemid"].ToString()) +
                                                " update tblItem set item_Qty = @tot " +
                                                " where item_ItemID = " + db.sqlStr(v["isit_itemid"].ToString())
                                            );
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

                                }

                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }
                    }
                    calTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        void calTotal(string eid, sapi.db db)
        {

            double reit_Total = 0;


            DataTable tbl = db.readData("Select SUM(isNull(isit_Total,0)) isit_Total From tblIssueItem " +
                " Where isit_Deleted is null and isit_IssueID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                reit_Total = db.cNum(row["isit_Total"].ToString());


                db.execData("Update tblIssue Set " +
                    " issu_Total = " + reit_Total +
                    " Where issu_IssueID = " + eid
                    );
            }
        }
    }
}