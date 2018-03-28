using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.invoicePayment
{
    public partial class invoicePayment : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoicePaymentNew";
        string frm = "frmMaster";
        string IDFIeld = "ivpm_invoicepaymentid";
        string Tab = "InvoicePayment";
        string cTab = "InvoicePayment";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();
            if (url.Get("ivpm_invoicepaymentid") == "")
            {
                Response.Redirect(cls.baseUrl + "invoice/invoice.aspx");
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
                           
                            string invo_invoiceid = db.readData("ivpm_InvoiceID", "Select ivpm_InvoiceID From tblInvoicePayment Where ivpm_InvoicePaymentID = " + Request.Form["eid"].ToString());
                            db.beginTran();
                            Response.Write(cls.delRecord(screen, Request.Form["eid"].ToString(), db) + invo_invoiceid);
                            double paidAmount = db.cNum(db.readData("ivpm_Amount", "Select SUM(isNULL(ivpm_Amount,0)) From tblInvoicePayment Where ivpm_Deleted is null and ivpm_InvoiceID = " + invo_invoiceid));
                            db.execData("Update tblInvoice Set " +
                                " invo_PaidAmount = " + paidAmount +
                                ",invo_Balance = invo_GTotal - isNULL(invo_CreditNote,0) - isNull(invo_Deposit,0) - " + paidAmount +
                                ",invo_isPaid = NULL " +
                                " where invo_InvoiceID = " + invo_invoiceid);
                            clsGlobal clsglobal = new clsGlobal();
                            clsglobal.validInvoice(invo_invoiceid, db);
                            
                            db.commit();
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
            //cls.hideEdit = true;
            //cls.hideDelete = true;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
            }
            if (cls.Mode == sapi.sapi.recordMode.Edit)
            {
                sapi.readOnlyField.add("ivpm_InvoiceID");
                sapi.readOnlyField.add("ivpm_Amount");
                sapi.readOnlyField.add("ivpm_PaymentType");
                
            }

            if (url.Get("ivpm_invoicepaymentid") != "")
            {
                tblData = db.readData("Select * from tblInvoicePayment Where ivpm_Deleted is null and ivpm_InvoicePaymentID=" + url.Get("ivpm_invoicepaymentid"));
                foreach (DataRow rowInvoice in tblData.Rows)
                {
                    sapi.Buttons.add("Invoice", "arrow-left", "info",
                        "window.location = '" + cls.baseUrl + "invoice/invoice.aspx?invo_invoiceid=" + rowInvoice["ivpm_InvoiceID"].ToString() + "'");
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