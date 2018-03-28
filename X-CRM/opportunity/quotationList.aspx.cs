using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.opportunity
{
    public partial class quotationList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblQuotationFind";
        string grid = "tblQuotationList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                {
                    Response.Redirect(cls.baseUrl + "opportunity/opportunity.aspx");
                }
                if (db.connect())
                {
                    
                    string eid = "";

                    if (Request.Form["app"] != null)
                    {

                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "1=2";
                            if (!string.IsNullOrEmpty(Request.Form["oppo_opportunityid"].ToString()))
                            {
                                filter = " quot_opportunityid=" + Request.Form["oppo_opportunityid"].ToString();
                            }

                            Response.Write(cls.loadSSA(db,
                                Request.Form["colid"].ToString(),
                                Request.Form["q"].ToString()));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                        
                    }

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "findRecord")
                        {

                            string filter = " 1= 2 ";
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "quot_Name";
                            string orderBy = "";

                            if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                            {
                                filter = " and quot_opportunityid=" + url.Get("oppo_opportunityid");
                            }

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString()), filter: filter));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                        {
                            eid = url.Get("oppo_opportunityid");
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblOpportunity", "tblQuotation", 1, eid);
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
            cls.Mode = mode;
            cls.scrnType = global::sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = global::sapi.sapi.recordMode.New;

            var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
            string topTitle = "";
            DataTable tblTop = null;

            if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
            {
                sapi.Buttons.add("New Quotation", "plus", "success",
                    "window.location = '../quotation/quotation.aspx?oppo_opportunityid=" + url.Get("oppo_opportunityid") + "'");

                topTitle = "Opportunity";
                tblTop = db.readData("Select oppo_Name,cust_Name From vOpportunity Where oppo_Deleted is null and oppo_OpportunityID=" + url.Get("oppo_opportunityid"));
            }
            string re = "";

            


            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}