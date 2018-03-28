using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.invoice
{
    public partial class invoiceList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;
        
        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoiceFind";
        string grid = "tblInvoiceList";
        string frm = "frmMaster";

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
                        if (Request.Form["app"].ToString() == "findRecord")
                        {
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "invo_Name";
                            string orderBy = "";
                            string filter = " AND invo_Type = '" + url.Get("invoice") + "'";
                            //string filter = " AND 1 = 2"; //TEST FILTER TRUE OR FALSE. IF TRUE SHOW ALL ELSE NOT SHOW 
                            //string filter = " AND 1 = 1";
                            int mode = 0;

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            if (url.Get("invoice") == "Tax Invoice")
                                grid = "tblInvoiceTaxList";
                            else
                                grid = "tblInvoiceList";
                            //findRecord(db db, string screen, string grid, string frm, Dictionary < string, string > vals, string orderFieldBy, string orderBy = "ASC", int cPage = 1, int mode = 0, string filter = "", int assc = 1, bool hidePage = false, bool hideDel = false, bool hideNewRow = false);
                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString()),mode:0,filter:filter));
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
                            cls.loadTab(db, "", "", 1, showCTab: true);
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally { db.close(); }
        }

        void init()
        {
            url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
        }
        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            cls.scrnType = sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = sapi.sapi.recordMode.New;

            string re = "";

            string invoiceType = url.Get("invoice");
            string invoiceName = "Invoice";
            if (invoiceType == "Commercial Invoice")
                invoiceName = "Commercial Invoice";
            else if (invoiceType == "Tax Invoice")
                invoiceName = "Tax Invoice";

            //sapi.Buttons.add("New " + invoiceName, "plus", "success", "window.location = 'invoice.aspx?invoice=" + invoiceType + "'", "I", "tblInvoice");

            //sapi.Buttons.add("New Invoice", "plus", "success", "window.location = 'invoice.aspx?invoice=invTaxInvoice'", "I","tblInvoice");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            return re;
        }
    }
}