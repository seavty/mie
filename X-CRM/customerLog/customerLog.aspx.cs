using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.customerLog
{
    public partial class customerLog : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblCustomerLogNew";
        string frm = "frmMaster";
        string IDFIeld = "culg_CustomerLogID".ToLower();
        string Tab = "tblCustomerLog";
        string cTab = "tblCustomerLog";

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
                            cls.Mode = global::sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;

                        }
                        else
                        {

                        }

                        if (url.Get("cust_customerid") != "")
                        {
                            sapi.defaultValue.add("culg_CustomerID", url.Get("cust_customerid"));
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

            //var tmp = sapi.loadTab(db, "sys_table", "sys_table", 0, eid);
            //Response.Write("<script></script>");


            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            /*
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    if (!vals.ContainsKey("cont_contractid"))
                    {
                        if (vals.ContainsKey("cont_customerid"))
                        {
                            if (!string.IsNullOrEmpty(vals["cont_customerid"]))
                            {
                                var tmp = db.execData("Update tblCustomer Set cust_Type='Customer' Where cust_Type='Lead' and cust_CustomerID=" +
                                    vals["cont_customerid"].ToString());
                                if (tmp != "ok")
                                {
                                    db.rollback();
                                    DataTable tblResult = new DataTable();
                                    tblResult.Rows.Add();
                                    tblResult.Columns.Add("status");
                                    tblResult.Columns.Add("msg");
                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = tmp;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

                                    return re;
                                }
                            }
                        }
                    }
                }
            }*/
            db.commit();
            return re;
        }
    }
}