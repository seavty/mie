using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.invoice
{
    public partial class invoicePaymentList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoicePaymentFind";
        string grid = "tblInvoicePaymentList";
        string frm = "frmMaster";
        string tabl_columnid = "invo_invoiceid";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get(tabl_columnid)))
                {
                    Response.Redirect(cls.baseUrl + "invoice/invoice.aspx");
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
                            string orderFieldBy = "ivpm_Name";
                            string orderBy = "";

                            if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                            {
                                filter = " and ivpm_invoiceid=" + url.Get(tabl_columnid);
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
                        if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                        {
                            eid = url.Get(tabl_columnid);
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblInvoice", "tblInvoicePayment", 1, eid);
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

            if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
            {
                string invoID = url.Get("invo_invoiceid");
                topTitle = "Invoice";
                if (!string.IsNullOrEmpty(invoID))
                {
                    topTitle = db.readData("invo_Type","SELECT invo_Type FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_InvoiceID=" + invoID);
                }
                tblTop = db.readData("Select invo_Name,invo_Date,invo_Total From vInvoice Where invo_Deleted is null and invo_InvoiceID=" + url.Get(tabl_columnid));
            }
            string re = "";
            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}