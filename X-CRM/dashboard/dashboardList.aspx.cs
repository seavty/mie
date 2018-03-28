using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.dashboard
{
    public partial class dashboardList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "sys_DashboardFind";
        string grid = "sys_DashboardList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "findRecord")
                        {
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "dash_Name";
                            string orderBy = "";

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString())));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "sys_Dashboard", "sys_Dashboard", 1, showCTab: true);
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
            cls.scrnType = sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = sapi.sapi.recordMode.New;

            string re = "";
            sapi.Buttons.add("New Dashboard", "plus", "success", "window.location = 'dashboard.aspx'");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            return re;
        }
    }
}