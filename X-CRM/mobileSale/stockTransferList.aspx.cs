using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.mobileSale
{
    public partial class stockTransferList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblStockTransferFind";
        string grid = "tblStockTransferList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
                {
                    Response.Redirect(cls.baseUrl + "mobileSale/mobileSale.aspx");
                }
                if (db.connect())
                {

                    string eid = "";

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
                            string orderFieldBy = "sttf_Name";
                            string orderBy = "";

                            if (!string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
                            {
                                filter = " and sttf_MobileSaleID =" + url.Get("mbsl_mobilesaleid");
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
                        if (!string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
                        {
                            eid = url.Get("mbsl_mobilesaleid");
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblMobileSale", "tblStockTransfer", 1, eid);
                    }

                }
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
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

            if (!string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
            {
                tblTop = db.readData("Select mbsl_Name,mbsl_CheckOut,mbsl_CheckIn From tblMobileSale Where mbsl_Deleted is null and mbsl_mobilesaleid=" + url.Get("mbsl_mobilesaleid"));
                if (tblTop.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(tblTop.Rows[0]["mbsl_CheckIn"].ToString()))
                    {
                        sapi.Buttons.add("New Stock Transefer", "plus", "success",
                            "window.location = '../stockTransfer/stockTransfer.aspx?mbsl_mobilesaleid=" + url.Get("mbsl_mobilesaleid") + "'","I","tblStockTransfer");
                    }
                }
                topTitle = "Mobile Sale";
                            }
            string re = "";




            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}