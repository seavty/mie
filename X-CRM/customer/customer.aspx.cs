using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.customer
{
    public partial class customer : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblCustomerNew";
        string frm = "frmMaster";
        string IDFIeld = "cust_customerid";
        string Tab = "tblCustomer";
        string cTab = "tblCustomer";

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
            string topTitle="";
            DataTable tblTop=null;
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
                sapi.defaultValue.add("cust_Country", "Cambodia");
            }

            if (mode == global::sapi.sapi.recordMode.View)
            {   
                sapi.Buttons.add(cls.getString("Quotation",db), "plus", "warning", "window.location='../quotation/quotation.aspx?cust_customerid=" + eid + "'","I","tblQuotation");
                //sapi.Buttons.add(cls.getString("Sale Order", db), "plus", "success", "window.location='../saleorder/saleorder.aspx?cust_customerid=" + eid + "'","I","tblSaleOrder");
                //sapi.Buttons.add(cls.getString("Invoice", db), "plus", "info", "window.location='../invoice/invoice.aspx?cust_customerid=" + eid + "'","I","tblInvoice");
                sapi.Buttons.add("Invoice", "plus", "info", "convertToInvoice()");
                int countQuotation = int.Parse(db.readData("C", "SELECT COUNT(*) AS C FROM tblQuotation WHERE quot_CustomerID = " + url.Get("cust_customerid")));
                int countInvoice = int.Parse(db.readData("C", "SELECT COUNT(*) AS C FROM tblInvoice WHERE invo_CustomerID = " + url.Get("cust_customerid")));
                if (countQuotation > 0 || countInvoice > 0)
                    cls.hideDelete = true;

            }
            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //sapi.Buttons.add("Convert to SO", "arrow-right", "info", "convertSO(" + eid + ")");
                //sapi.Buttons.add("Convert to Invoice", "arrow-right", "info", "convertInv(" + eid + ")");
                topTitle = "Company / Customer";
                tblTop = db.readData("SELECT cust_Code,cust_Name FROM tblCustomer WHERE cust_CustomerID=" + eid);
                //re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            }
            string re = "";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);


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