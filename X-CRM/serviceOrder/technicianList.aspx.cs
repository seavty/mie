using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.serviceOrder
{
    public partial class technicianList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblTechnicianFind";
        string grid = "tblTechnicianList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get("seor_serviceorderid")))
                {
                    Response.Redirect(cls.baseUrl + "serviceOrder/serviceOrder.aspx");
                }
                if (db.connect())
                {

                    string eid = "";

                    if (Request.QueryString["app"] != null)
                    {

                        if (Request.QueryString["app"].ToString() == "SSA")
                        {
                            string filter = "1=2";
                            if (!string.IsNullOrEmpty(url.Get("empl_employeeid")))
                            {
                                filter = " lang_EmployeeID=" + url.Get("empl_employeeid");
                            }

                            Response.Write(cls.loadSSA(db,
                                Request.QueryString["colid"].ToString(),
                                Request.QueryString["q"].ToString()));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
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
                            string orderFieldBy = "teci_TechnicianID";
                            string orderBy = " ASC ";

                            if (!string.IsNullOrEmpty(url.Get("seor_serviceorderid")))
                            {
                                filter = " and teci_ServiceOrderID=" + url.Get("seor_serviceorderid");
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
                        if (!string.IsNullOrEmpty(url.Get("seor_serviceorderid")))
                        {
                            eid = url.Get("seor_serviceorderid");
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblServiceOrder", "tblTechnician", 1, eid);
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.close();
                Response.End();
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

            if (!string.IsNullOrEmpty(url.Get("seor_serviceorderid")))
            {
                sapi.Buttons.add("Add Technician", "plus", "success",
                    "addTechician(" + url.Get("seor_serviceorderid") + ")");
                
                topTitle = "Service Order";
                tblTop = db.readData("SELECT  cust_Name, seor_Name FROM vServiceOrder WHERE seor_ServiceOrderID=" + url.Get("seor_serviceorderid"));
            }
            string re = "";




            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}