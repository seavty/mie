using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.customer
{
    public partial class customerLogList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblCustomerLogFind";
        string grid = "tblCustomerLogList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get("cust_customerid")))
                {
                    Response.Redirect(cls.baseUrl + "contract/contract.aspx");
                }
                if (db.connect())
                {

                    string eid = "";

                    if (Request.Form["app"] != null)
                    {

                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "1=2";
                            if (!string.IsNullOrEmpty(Request.Form["cust_customerid"].ToString()))
                            {
                                filter = " culg_customerid=" + url.Get("cust_customerid");
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
                            string orderFieldBy = "";
                            string orderBy = "";

                            if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                            {
                                filter = " and culg_customerid=" + url.Get("cust_customerid");
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
                        if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                        {
                            eid = url.Get("cust_customerid");
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblCustomer", "tblContract", 1, eid);
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

            if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
            {
                sapi.Buttons.add("New Customer Log", "plus", "success",
                    "window.location = '../customerLog/customerLog.aspx?cust_customerid=" + url.Get("cust_customerid") + "'");

                topTitle = "Customer";
                tblTop = db.readData("Select cust_Name,cust_Type From tblCustomer Where cust_Deleted is null and cust_CustomerID=" + url.Get("cust_customerid"));
            }
            string re = "";




            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}