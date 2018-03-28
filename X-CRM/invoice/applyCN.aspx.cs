using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.invoice
{
    public partial class applyCN : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();

        Dictionary<string, string> vals = new Dictionary<string, string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            init();
            if (Request.Form["app"] != null)
            {
                try
                {
                    if (db.connect())
                    {

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            string filter = " and 1=2";
                            string id = Request.Form["invo_invoiceid"].ToString();
                            filter = " and isNULL(crdn_Balance,0) >0 and crdn_CustomerID = " + 
                                db.cNum(db.readData("invo_CustomerID", "select invo_CustomerID From tblInvoice Where invo_Deleted is null and invo_InvoiceID = " +id));
                            Response.Write(cls.findRecord(db,"","tblCreditNoteList","frmApplyCN",null,"",cPage:-1,assc:3,filter:filter));
                            
                        }
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            DataTable tblInvoice = db.readData("Select * from tblInvoice " +
                                " Where invo_Deleted is null and invo_InvoiceID = " + Request.Form["invo_invoiceid"].ToString());
                            string hid = Request.Form["crdn_creditnoteid"].ToString();

                            db.beginTran();
                            foreach (DataRow rowInvoice in tblInvoice.Rows)
                            {
                                DataTable tblResult = new DataTable();
                                tblResult.Rows.Add();
                                tblResult.Columns.Add("status");
                                tblResult.Columns.Add("msg");

                                if (db.cNum(rowInvoice["invo_Balance"].ToString()) > 0)
                                {
                                    double totalCN = db.cNum(db.readData("crdn_Total", "select crdn_Total From tblCreditNote Where crdn_CreditNoteID = " + hid));
                                    double cnAmount = totalCN;
                                    clsGlobal clsglobal = new clsGlobal();
                                    if (db.cNum(rowInvoice["invo_Balance"].ToString()) >= totalCN)
                                    {

                                    }
                                    else
                                    {
                                        cnAmount = db.cNum(rowInvoice["invo_Balance"].ToString());
                                    }
                                    var tmp = db.execData("Update tblInvoice Set invo_CreditNote = isNull(invo_CreditNote,0) + " +
                                           cnAmount +
                                            " Where invo_InvoiceID = " + rowInvoice["invo_InvoiceID"].ToString());
                                    clsglobal.invoiceTotal(rowInvoice["invo_InvoiceID"].ToString(), db);
                                    clsglobal.validInvoice(rowInvoice["invo_InvoiceID"].ToString(), db);
                                    tmp = db.execData("Update tblCreditNote Set crdn_UsedAmount = isNULL(crdn_UsedAmount,0) + " + cnAmount +
                                        ",crdn_Balance =​ isNULL(crdn_Total,0) - isNULL(crdn_UsedAmount,0) - " + cnAmount +
                                        " Where crdn_CreditNoteID = " + hid);
                                    tmp = db.execData("Insert into tblInvoiceCN(ivcn_InvoiceID,ivcn_CreditNoteID,ivcn_Amount,ivcn_Date) VALUES(" +
                                            rowInvoice["invo_InvoiceID"].ToString() + "," + hid + "," + cnAmount + ",GETDATE()" +
                                        ")");
                                    if (tmp != "ok")
                                    {
                                        db.rollback();
                                        Response.Write("Error while processing data !");
                                    }
                                    else
                                    {
                                        db.commit();
                                        Response.Write("ok");
                                    }
                                    
                                   
                                }
                            }
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { db.close(); }
            }
        }

        void init()
        {
            //url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

            vals.Clear();

            foreach (var st in Request.Form.AllKeys)
            {
                if (!string.IsNullOrEmpty(st))
                    vals.Add(st.ToLower(), Request.Form[st].ToString());
            }
        }
    }
}