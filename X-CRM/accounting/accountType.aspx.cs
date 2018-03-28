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
    public partial class accountType : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblAccountTypeNew";
        string frm = "frmMaster";
        string IDFIeld = "acct_accounttypeid";
        string Tab = "tblAccountType";
        string cTab = "tblAccountType";

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


                        //if (Request.QueryString["app"].ToString() == "SSA")
                        //{
                        //    string filter = "";
                        //    if (url.Get("colName") == "acct_ChartAccountTypeID")
                        //    {
                        //        string account = Request.Form["acct_Account"];
                        //        if (!string.IsNullOrEmpty(account))
                        //        {
                        //            //DataTable tmp = db.readData("Select cact_ChartAccountTypeID From tblChartAccountType Where cact_Account = " + account);
                        //            //foreach (DataRow tmpRow in tmp.Rows)
                        //            //{
                        //                //filter = " cact_Account = " + tmpRow["cact_Account"].ToString();
                        //            //}
                        //            filter = " cact_Account = " + account;
                        //        }
                        //    }

                        //    Response.Write(cls.loadSSA(db,
                        //                                    Request.QueryString["colid"].ToString(),
                        //                                    Request.QueryString["q"].ToString(), filter: filter));
                        //    db.close();
                        //    cls.endRequest();
                        //    Response.End();
                        //}



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
                            cls.Mode = global::sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;

                        }
                        else
                        {

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

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
            }

            string re = "";
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);

            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    if (!vals.ContainsKey("acct_accounttypeid"))
                    {
                        string id = (string)str.tbl[0].msg;
                        //DataTable tblAccountType = db.readData("Select * from tblAccountType Where acct_Deleted is null");
                        //foreach (DataRow rowItem in tblAccountType.Rows)
                        //{
                        //    var tmp = db.execData("Insert into tblItemWarehouse(itwh_ItemID,itwh_WarehouseID,itwh_Qty) " +
                        //        "VALUES(" + db.sqlStr(rowItem["item_ItemID"].ToString()) +
                        //        "," + id + ",0)");
                        //    if (tmp != "ok")
                        //    {
                        //        db.rollback();
                        //        DataTable tblResult = new DataTable();
                        //        tblResult.Rows.Add();
                        //        tblResult.Columns.Add("status");
                        //        tblResult.Columns.Add("msg");
                        //        tblResult.Rows[0]["status"] = "error";
                        //        tblResult.Rows[0]["msg"] = tmp;
                        //        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        //        return re;
                        //    }
                        //}

                    }
                }
            }
            db.commit();
            return re;
        }

    }
}