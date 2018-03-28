using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using Newtonsoft.Json;

namespace X_CRM.purchaseOrder
{
    public partial class purchaseOrder : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblPurchaseOrderNew";
        string screenItem = "tblPurchaseOrderItemNew";
        string screenItemList = "tblPurchaseOrderItemList";
        string frm = "frmMaster";
        string IDFIeld = "purc_purchaseorderid";
        string Tab = "tblPurchaseOrder";
        string cTab = "tblPurchaseOrder";

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
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "";
                            
                            Response.Write(cls.loadSSA(db,
                                Request.Form["colid"].ToString(),
                                Request.Form["q"].ToString(), filter: filter));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                    }

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

                        if (Request.Form["app"].ToString() == "preDelLine")
                        {
                            double val = db.cNum(db.readData("soit_ShipQty",
                                "Select isNull(soit_ReceivedQty,0) soit_ShipQty From tblPurchaseOrderItem Where poit_Deleted is null and poit_PurchaseOrderItemID  = " + Request.Form["id"].ToString()));
                            if (val <= 0)
                            {
                                Response.Write("ok");
                            }
                            else
                            {
                                Response.Write("Purchase Order is already Received !");
                            }

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
                                sapi.defaultValue.add("purc_SupplierID", url.Get("cust_customerid").ToString());
                            }
                            sapi.defaultValue.add("purc_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));

                            sapi.defaultValue.add("purc_WarehouseID", db.readData("sett_WarehouseID", "Select sett_WarehouseID From sys_Setting"));
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
                                mode: 1, filter: " and poit_PurchaseOrderID = " + eid, cPage: -1) +
                       "</div>";

            }

            if (mode == sapi.sapi.recordMode.View)
            {



                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and poit_PurchaseOrderID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                if (eid != "0")
                {
                    tblData = db.readData("Select * from tblPurchaseOrder WHere purc_Deleted is null and purc_PurchaseOrderID=" + eid);
                    foreach (DataRow row in tblData.Rows)
                    {
                        

                        if (db.readData("Select 1 From tblPurchaseOrderItem " +
                            " Where poit_Deleted is null and isNull(poit_ReceivedQty,0)>0 and poit_PurchaseOrderID = " + eid).Rows.Count > 0)
                        {
                            cls.hideDelete = true;
                            cls.hideEdit = true;
                        }
                        if (row["purc_isComplete"].ToString() != "Y")
                        {
                            sapi.Buttons.add("Receive", "motorcycle", "info", "receivePO(" + eid + ")","I","tblAPInvoice");
                            sapi.Buttons.add("Complete", "checkmark", "success", "completePO(" + eid + ")","E","tblPurchaseOrder");
                        }
                    }
                }

            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                //sapi.readOnlyField.add("soit_ItemID");
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and poit_PurchaseOrderID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //topTitle = "Invoice";
                //tblTop = db.readData("Select invo_Name,invo_Date,invo_Total From tblInvoice Where invo_Deleted is null and invo_InvoiceID = " + eid);

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
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;
                    if (Request.Form.GetValues("N") != null)
                    {
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            aVal.Add("poit_PurchaseOrderID", hid);

                            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                            double exQty = db.cNum(db.readData("poit_Qty",
                                    "Select poit_Qty From tblPurchaseOrderItem Where poit_PurchaseOrderIemID=" + v["poit_purchaseorderitemid"]));
                            
                            if (vals["txtdel" + st] != "")
                            {

                                cls.delRecord(screenItem, vals["poit_PurchaseOrderItemID".ToLower() + st], db);

                                if (!string.IsNullOrEmpty(v["poit_PurchaseOrderItemID".ToLower()]))
                                {

                                }
                            }
                            else
                            {

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("poit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["poit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("poit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["poit_Price".ToLower()]);
                                }

                                if (v.ContainsKey("poit_Total".ToLower()))
                                {
                                    v["poit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
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
                                    else
                                    {
                                        string id = (string)str.tbl[0].msg;
                                        db.execData("Update tblPurchaseOrderItem " +
                                            " Set poit_RemainQty = isNull(poit_Qty,0) - isNull(poit_ReceivedQty,0) " +
                                            " Where poit_PurchaseOrderItemID=" + id);

                                        
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
                    new clsGlobal().POTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        
    }
}