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
    public partial class workflow : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;
        Dictionary<string, string> vals = new Dictionary<string, string>();
        string screen = "tblOpportunityWF";
        string frm = "frmMaster";
        string IDFIeld = "eid";
        string Tab = "";
        string cTab = "";
        string tabl_Name = "tblOpportunity";
        string tabl_Prefix = "oppo";
        string tabl_ColumnID = "oppo_OpportunityID";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            if (string.IsNullOrEmpty(url.Get("wfdtid")) ||
                string.IsNullOrEmpty(url.Get("eid")))
            {
                Response.Redirect(cls.baseUrl + "opportunity/opportunity.aspx");
            }

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
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = false;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = global::sapi.sapi.recordMode.Edit;
                            eid = url.Get(IDFIeld);
                        }

                        // preset the workflow things
                        setVal(url.Get("wfdtid"));

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
                DataTable tblResult = new DataTable();
                tblResult.Rows.Add();
                tblResult.Columns.Add("status");
                tblResult.Columns.Add("msg");

                if (str.tbl[0].status == "ok")
                {
                    if (!string.IsNullOrEmpty(url.Get("wfdtid")) && !string.IsNullOrEmpty(url.Get("eid")))
                    {
                        DataTable tbl = db.readData("Select * from vSys_Workflow Where wkid_WorkflowItemDetailID = " + url.Get("wfdtid"));
                        foreach (DataRow row in tbl.Rows)
                        {
                            var tmp = db.execData("Update " + tabl_Name + " Set " +
                                tabl_Prefix + "_WorkflowItemID=" + db.cNum(row["wkid_TargetWorkflowItemID"].ToString()) +
                                " Where " + tabl_ColumnID + "=" + db.cNum(url.Get("eid")) +
                                " and " + tabl_Prefix + "_WorkflowItemID = " + db.cNum(row["wfit_WorkflowItemID"].ToString()));
                            if (tmp == "ok")
                            {
                                // do the workflow things
                            }else
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }
                        
                    }
                    else
                    {
                        db.rollback();
                        tblResult.Rows[0]["status"] = "error";
                        tblResult.Rows[0]["msg"] = "Error processing workflow";
                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        return re;
                    }
                }
            }
            db.commit();
            return re;
        }

        void setVal(string val)
        {
            if (val == "1")
            {
                sapi.setValue.add("oppo_Status", "Candidate");
            }
            if (val == "2")
            {
                sapi.setValue.add("oppo_Status", "Placement");
            }
            if (val == "3" || val == "6")
            {
                sapi.setValue.add("oppo_Status", "Invoice");
            }
            if (val == "4" || val == "7")
            {
                sapi.setValue.add("oppo_Status", "Closed");
            }
            if (val == "5")
            {
                sapi.setValue.add("oppo_Status", "Work Process");
            }
            
        }
    }
}