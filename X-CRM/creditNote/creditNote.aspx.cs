using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.creditNote
{
    public partial class creditNote : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblCreditNoteNew";
        string screenItem = "tblCreditNoteItemNew";
        string screenItemList = "tblCreditNoteItemList";
        string frm = "frmMaster";
        string IDFIeld = "crdn_creditnoteid";
        string Tab = "";
        string cTab = "";

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
                            db.beginTran();
                            DataTable tbl = db.readData("Select * from tblCreditNote " +
                                " INNER JOIN tblInvoiceCN on ivcn_CreditNoteID = crdn_CreditNoteID " +
                                " Where crdn_CreditNoteID = " + Request.Form["eid"].ToString());
                            foreach (DataRow row in tbl.Rows)
                            {
                                //if (!string.IsNullOrEmpty(row["crdn_InvoiceID"].ToString()))
                                {
                                    var re = db.execData("Update tblInvoice set invo_CreditNote = 0 " +
                                        " Where invo_InvoiceID = " + row["ivcn_InvoiceID"].ToString());
                                    re = db.execData("Update tblInvoiceCN set ivcn_Deleted = 'Y' " +
                                        " Where ivcn_InvoiceID = " + row["ivcn_InvoiceID"].ToString() +
                                        " and ivcn_CreditNoteID = " + Request.Form["eid"].ToString());
                                    clsGlobal clsglobal = new clsGlobal();
                                    clsglobal.invoiceTotal(row["ivcn_InvoiceID"].ToString(), db);
                                    clsglobal.validInvoice(row["ivcn_InvoiceID"].ToString(), db);

                                    DataTable tblItem = db.readData("Select * from tblCreditNoteItem " +
                                        " INNER JOIN tblInvoiceItem on init_InvoiceItemID = cnit_InvoiceID and init_Deleted is null " +
                                        " Where cnit_Deleted is null and cnit_InvoiceID is not null and cnit_CreditNoteID = " + Request.Form["eid"].ToString());
                                    foreach (DataRow rowItem in tblItem.Rows)
                                    {
                                        clsglobal.stockDeduction(db, rowItem["cnit_ItemID"].ToString(), rowItem["init_WarehouseID"].ToString(),
                                            db.cNum(rowItem["cnit_Qty"].ToString()));
                                        db.execData("Update tblInvoiceItem Set init_BQty = isNULL(init_BQty,0) + " +
                                            db.cNum(rowItem["cnit_Qty"].ToString()) +
                                            ",init_RQty = isNULL(init_RQty,0) - " + db.cNum(rowItem["cnit_Qty"].ToString()) +
                                            " Where init_InvoiceItemID = " + rowItem["init_InvoiceItemID"].ToString());
                                    }
                                }
                                
                            }
                            Response.Write(cls.delRecord(screen, Request.Form["eid"].ToString(), db));
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
                            cls.Mode = sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                            {
                                sapi.defaultValue.add("crdn_CustomerID", url.Get("cust_customerid"));
                            }
                            //sapi.defaultValue.add("quot_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
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

        string loadScreen(string eid, sapi.sapi.recordMode mode)
        {
            string re = "";
            string list = "";
            string topTitle = "";
            DataTable tblTop = null;
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and cnit_CreditNoteID = " + eid, cPage: -1) +
                       "</div>";
            }



            if (mode == sapi.sapi.recordMode.View)
            {
                cls.hideEdit = true;
                //cls.hideDelete = true;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and cnit_CreditNoteID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                sapi.Buttons.add("Print", "print", "success", "printCN(" + eid + ")");
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and cnit_CreditNoteID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //topTitle = "Quotation";
                //tblTop = db.readData("Select quot_Name,quot_Date,quot_Total,quot_QuotationID From tblQuotation Where quot_Deleted is null and quot_QuotationID=" + eid);
            }

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list;
            return re;
        }

        string saveRecord()
        {
            string re = "";
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            hid = (string)str.tbl[0].msg;
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("cnit_CreditNoteID", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["cnit_CreditNoteItemID".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("cnit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["cnit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("cnit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["cnit_Price".ToLower()]);
                                }

                                if (v.ContainsKey("cnit_Total".ToLower()))
                                {
                                    v["cnit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                }
                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
                                    }
                                }

                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }
                    }
                    new clsGlobal().creditNoteTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }
    }

}