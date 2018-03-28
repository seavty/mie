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
    public partial class newOpportunityItem : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblOpportunityItemNew";
        string frm = "frmMaster";
        string IDFIeld = "opit_opportunityitemid";
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
                            cls.Mode = global::sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;

                        }
                        if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                        {
                            sapi.readOnlyField.add("opit_OpportunityID");
                            sapi.defaultValue.add("opit_OpportunityID", url.Get("oppo_opportunityid"));
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
            if (cls.Mode == sapi.sapi.recordMode.View)
            {
                if(eid != "0")
                {
                    try
                    {
                        DataTable tbl = db.readData("Select * from tblOpportunityItem where opit_OpportunityItemID=" + eid);
                        foreach (DataRow row in tbl.Rows)
                        {
                            if (!string.IsNullOrEmpty(row["opit_OpportunityID"].ToString()))
                            sapi.Buttons.add("Opportunity", "arrow-left", "info",
                                "window.location = 'opportunityItem.aspx?oppo_opportunityid=" + row["opit_OpportunityID"].ToString() + "';");
                        }

                    }
                    catch (Exception ex) { }
                    finally { db.close(); }
                    
                }
            }
            string re = "";
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            return re;
        }
    }
}