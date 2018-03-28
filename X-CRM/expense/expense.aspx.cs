using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.expense
{
    public partial class expense : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblExpenseNewForOppo";
        string frm = "frmMaster";
        string IDFIeld = "exps_ExpenseID".ToLower();
        string Tab = "tblExpense";
        string cTab = "tblExpense";

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

                            var oppoID = GetOppoID(Request.Form["eid"].ToString());
                            Response.Write(cls.delRecord(screen, Request.Form["eid"].ToString(), db) + oppoID);
                            updateExpense(Request.Form["eid"].ToString());
                        }

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
                        if (!String.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                            sapi.defaultValue.add("exps_OpportunityID", url.Get("oppo_opportunityid"));

                        sapi.defaultValue.add("exps_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
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

            if (mode == sapi.sapi.recordMode.View)
            {
                if (!string.IsNullOrEmpty(url.Get("exps_expenseid")))
                {
                    string id = db.readData("exps_OpportunityID", "SELECT exps_OpportunityID FROM tblExpense WHERE exps_Deleted IS NULL AND exps_ExpenseID=" + url.Get("exps_expenseid"));
                    sapi.Buttons.add("Back", "arrow-left", "info",
                        "Back(" + id + ")");
                }
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

            try
            {
                re = cls.saveRecord(screen, vals, db, aVals: aVal);
                string id = "";
                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        id = (string)str.tbl[0].msg;
                        updateExpense(id);

                    }

                }
                db.commit();
                db.close();
                
            }

            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.rollback();
                db.close();
                cls.endRequest();
            }

            return re;

        }

        private void updateExpense(string expenseID)
        {
            var oppoID = db.readData("exps_OpportunityID", "SELECT exps_OpportunityID FROM tblExpense WHERE /* exps_Deleted IS NULL AND */ exps_ExpenseID =" + expenseID);
            var sql = "UPDATE tblOpportunity SET oppo_Expense =" +
                         " ( SELECT SUM(ISNULL(exps_Total, 0)) AS Total FROM tblExpense WHERE exps_Deleted IS NULL AND exps_OpportunityID =" + oppoID + ")" +
                         " WHERE oppo_Deleted IS NULL AND oppo_OpportunityID =" + oppoID;

            db.execData(sql);

        }

        private string GetOppoID(string expenseID)
        {
            var oppoID = db.readData("exps_OpportunityID", "SELECT exps_OpportunityID FROM tblExpense WHERE /* exps_Deleted IS NULL AND */ exps_ExpenseID =" + expenseID);
            return oppoID;
        }
    }
}