using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.saleorder
{
    public partial class saleorder : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblSaleOrderNew";
        string screenItem = "tblSaleOrderItemNew";
        string screenItemList = "tblSaleOrderItemList";
        string frm = "frmMaster";
        string IDFIeld = "sord_saleorderid";
        string Tab = "tblSaleOrder";
        string cTab = "tblSaleOrder";

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
                                "Select isNull(soit_ShipQty,0) soit_ShipQty From tblSaleOrderItem Where soit_Deleted is null and soit_SaleOrderItemID  = " + Request.Form["id"].ToString()));
                            if (val <= 0)
                            {
                                Response.Write("ok");
                            }
                            else
                            {
                                Response.Write("Sales Order is already shipped !");
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
                                sapi.defaultValue.add("sord_CustomerID", url.Get("cust_customerid").ToString());
                            }
                            sapi.defaultValue.add("sord_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));

                            sapi.defaultValue.add("sord_WarehouseID", db.readData("sett_WarehouseID", "Select sett_WarehouseID From sys_Setting"));
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
                                mode: 1, filter: " and soit_SaleOrderID = " + eid, cPage: -1) +
                       "</div>";
                /*
                if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                {
                    DataTable tbl = db.readData("Select * from tblOpportunity Where oppo_Deleted is null and oppo_OpportunityID=" + url.Get("oppo_opportunityid"));
                    foreach (DataRow row in tbl.Rows)
                    {
                        sapi.defaultValue.add("invo_OpportunityID", url.Get("oppo_opportunityid"));
                        sapi.defaultValue.add("invo_CustomerID", row["oppo_CustomerID"].ToString());
                    }
                }
                */
            }

            if (mode == sapi.sapi.recordMode.View)
            {



                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and soit_SaleOrderID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                if (eid != "0")
                {
                    tblData = db.readData("Select * from tblSaleOrder WHere sord_Deleted is null and sord_SaleOrderID=" + eid);
                    foreach (DataRow row in tblData.Rows)
                    {
                        /*
                        if (row["invo_Status"].ToString().ToLower() == "completed")
                        {
                            cls.hideDelete = true;
                            cls.hideEdit = true;
                        }
                        if (!string.IsNullOrEmpty(row["invo_OpportunityID"].ToString()))
                        {
                            sapi.Buttons.add("Opportunity", "arrow-left", "info",
                                        "window.location = '../opportunity/invoiceList.aspx?oppo_opportunityid=" + row["invo_OpportunityID"].ToString() + "';");
                        }
                        if (db.cNum(row["invo_Balance"].ToString().ToLower()) > 0)
                        {
                            sapi.Buttons.add("Payment", "dollar2", "warning",
                                            "payment(" + eid + ");");
                        }*/

                        
                        sapi.Buttons.add("Print", "print", "success",
                                        "print(" + eid + ");");
                        

                        if (db.readData("Select 1 From tblSaleOrderItem " +
                            " Where soit_Deleted is null and isNull(soit_ShipQty,0)>0 and soit_SaleOrderID = " + eid).Rows.Count > 0)
                        {
                            cls.hideDelete = true;
                            cls.hideEdit = true;
                        }
                        else
                        {
                            //sapi.Buttons.add("Edit Set", "pencil", "warning", "editSet(" + eid + ")");
                            //sapi.Buttons.add("Add Set", "plus", "warning", "createSet(" + eid + ")");
                            //sapi.Buttons.add("Add Item", "plus", "warning", "createItem(" + eid + ")");
                        }
                        if (row["sord_isComplete"].ToString() != "Y")
                        {
                            sapi.Buttons.add("Ship", "motorcycle", "info", "shipSO(" + eid + ")","I","tblInvoice");
                            sapi.Buttons.add("Complete", "checkmark", "success", "completeSO(" + eid + ")","E","tblSaleOrder");
                        }
                    }
                }

            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                //sapi.readOnlyField.add("soit_ItemID");
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and soit_SaleOrderID = " + eid, cPage: -1) +
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
                    
                    double rDeposit = db.cNum(vals["sord_Deposit".ToLower()]);
                    if (rDeposit > 0)
                    {

                        Dictionary<string, string> pVals = new Dictionary<string, string>();
                        Dictionary<string, string> aVals = new Dictionary<string, string>();
                        pVals.Add("ivpm_InvoicePaymentID".ToLower(), 
                            db.readData("ivpm_InvoicePaymentID", "Select ivpm_InvoicePaymentID From tblInvoicePayment Where ivpm_Deleted is null and ivpm_SaleOrderID = " + hid));
                        pVals.Add("ivpm_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                        pVals.Add("ivpm_SaleOrderID".ToLower(), hid);
                        pVals.Add("ivpm_InvoiceID".ToLower(), "0");
                        pVals.Add("ivpm_Amount".ToLower(), rDeposit.ToString());
                        pVals.Add("ivpm_Remark".ToLower(), "");
                        pVals.Add("ivpm_PaymentType".ToLower(), "Cash");
                        aVal.Add("ivpm_WarehouseID", vals["sord_warehouseid"].ToString());

                        var tmp = cls.saveRecord("tblInvoicePaymentNew", pVals, db, aVals);

                        str = JsonConvert.DeserializeObject<dynamic>(tmp);
                        if (str.error != null)
                        {
                            db.rollback();
                            
                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = "Error Saving Payment !";
                            return("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        }
                        
                    }
                    
                    if (Request.Form.GetValues("N") != null)
                    {
                        /*
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                            double exQty = 0;

                            if (v.ContainsKey("soit_saleorderitemid"))
                            {
                                exQty = db.cNum(db.readData("soit_Qty",
                                    "Select soit_Qty From tblSaleOrderItem Where soit_SaleOrderItemID=" + v["soit_saleorderitemid"]));
                            }

                            string strErr = "";
                            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                " Where itwh_ItemID = " + db.sqlStr(v["soit_ItemID".ToLower()]) +
                                " and itwh_WarehouseID = " + db.sqlStr(vals["sord_WarehouseID".ToLower()]));
                            foreach (DataRow rowItem in tblItem.Rows)
                            {
                                double qty = 0;
                                int n = Request.Form.GetValues("N").Length;
                                for (int st1 = 0; st1 < n; st1++)
                                {
                                    if (vals["soit_ItemID".ToLower() + st] == vals["soit_ItemID".ToLower() + st1])
                                    {
                                        qty = qty + db.cNum(vals["soit_Qty".ToLower() + st1]);
                                    }
                                }
                                qty = qty - exQty;
                                if (rowItem["item_isSet"].ToString() == "Y")
                                {

                                    DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                        " Where sitm_Deleted is null and sitm_ItemID=" + v["soit_ItemID".ToLower()]);
                                    string prestrErr = "";
                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                    {
                                        if (rowItemSet["item_IsStock"].ToString() == "Y")
                                        {
                                            double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                                " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                                " and itwh_WarehouseID = " + db.sqlStr(vals["sord_WarehouseID".ToLower()])));

                                            qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                            if (qty > itemwhQty2)
                                            {
                                                prestrErr = prestrErr +
                                                    "&nbsp;&nbsp;+ " + rowItemSet["item_Name"] + "(" +
                                                    qty + " / " +
                                                    itemwhQty2 + ")" +
                                                    "<br/>";
                                            }
                                        }
                                    }

                                    if (prestrErr.Length > 0)
                                    {
                                        strErr = strErr + " <h4>- " + rowItem["item_Name"] + " : </h4><br/>" + prestrErr +
                                            "<hr class='thin bg-grayLighter'/>";
                                    }

                                }
                                else
                                {
                                    if (rowItem["item_IsStock"].ToString() == "Y")
                                    {
                                        if (qty > db.cNum(rowItem["itwh_Qty"].ToString()))
                                        {
                                            strErr = strErr + " <h4>- " + rowItem["item_Name"] + "(" +
                                                qty.ToString() + " / " +
                                                db.cNum(rowItem["itwh_Qty"].ToString()) + ")</h3>" + "<hr class='thin bg-grayLighter'/>";
                                        }
                                    }
                                }
                            }

                            if (strErr.Length > 0)
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "<h3>Items out of stock : </h3><hr class='thin bg-grayLighter'/><br/>" + strErr;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }
                        */
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            aVal.Add("soit_SaleOrderID", hid);

                            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                            double exQty = db.cNum(db.readData("soit_Qty",
                                    "Select soit_Qty From tblSaleOrderItem Where soit_SaleOrderIemID=" + v["soit_saleorderitemid"]));
                            /*
                            if (exQty != db.cNum(v["soit_Qty".ToLower()]))
                            {
                                // SO Deallocation

                                if (!string.IsNullOrEmpty(v["soit_SaleOrderItemID".ToLower()]))
                                {
                                    DataTable tblItemSet = db.readData("Select * from tblSaleOrderItemDetail " +
                                        " Where soid_Deleted is null and soid_SaleOrderItemID=" +
                                        v["soit_SaleOrderItemID".ToLower()]);
                                    if (tblItemSet.Rows.Count <= 0)
                                    {
                                        tblItemSet.Rows.Add();
                                    }
                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                    {

                                        double qty = db.cNum(rowItemSet["soid_Qty"].ToString());
                                        string itemid = rowItemSet["soid_ItemID"].ToString();

                                        string tmp = db.execData("Update tblItemWarehouse Set " +
                                                " itwh_SOAllocation = isNULL(itwh_SOAllocation,0) - " + qty +
                                                " where itwh_WarehouseID = " + db.sqlStr(vals["sord_WarehouseID".ToLower()]) +
                                                " and itwh_ItemID = " + db.sqlStr(itemid)
                                                );
                                        if (tmp != "ok")
                                        {
                                            db.rollback();

                                            tblResult.Rows[0]["status"] = "error";
                                            tblResult.Rows[0]["msg"] = tmp;
                                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                            return re;
                                        }
                                    }
                                }
                                // End of SO Deallocation
                            }*/
                            if (vals["txtdel" + st] != "")
                            {

                                cls.delRecord(screenItem, vals["soit_SaleOrderItemID".ToLower() + st], db);

                                if (!string.IsNullOrEmpty(v["soit_SaleOrderItemID".ToLower()]))
                                {

                                }
                            }
                            else
                            {

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("soit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["soit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("soit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["soit_Price".ToLower()]);
                                }

                                if (v.ContainsKey("soit_Total".ToLower()))
                                {
                                    v["soit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
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

                                        if (!vals.ContainsKey("sord_saleorderid"))
                                        {
                                            if (db.readData("sett_CustomerlastPrice", "Select sett_CustomerlastPrice from sys_setting ") == "Y")
                                            {
                                                DataTable tblCustPrice = db.readData("Select * from tblCustomerPrice Where cusp_CustomerID = " +
                                                    "" + vals["sord_CustomerID".ToLower()] + " and cusp_ItemID = " + vals["soit_ItemID".ToLower() + st]);
                                                if (tblCustPrice.Rows.Count > 0)
                                                {
                                                    db.execData("Update tblCustomerPrice Set cusp_Price = " + db.cNum(vals["soit_Price".ToLower() + st]) +
                                                        " Where cusp_CustomerID = " + vals["sord_customerid".ToLower()] +
                                                        " and cusp_ItemID = " + vals["soit_ItemID".ToLower() + st]);
                                                }
                                                else
                                                {
                                                    db.execData("Insert into tblCustomerPrice(cusp_Price,cusp_CustomerID,cusp_ItemID) VALUES(" +
                                                        db.cNum(vals["soit_Price".ToLower() + st]) +
                                                        "," + vals["sord_customerid".ToLower()] +
                                                        "," + vals["soit_ItemID".ToLower() + st] + ")");
                                                }
                                            }
                                        }

                                        db.execData("Update tblSaleOrderItem " +
                                            " Set soit_RemainQty = isNull(soit_Qty,0) - isNull(soit_ShipQty,0) " +
                                            " Where soit_SaleOrderItemID=" + id);
                                        
                                        // SO allocation
                                        if (exQty != db.cNum(v["soit_Qty".ToLower()]))
                                        {
                                            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(v["soit_ItemID".ToLower()]) +
                                            " and itwh_WarehouseID = " + db.sqlStr(vals["sord_WarehouseID".ToLower()]));
                                            foreach (DataRow rowItem in tblItem.Rows)
                                            {
                                                if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                                {
                                                    DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                        v["soit_ItemID".ToLower()]);
                                                   
                                                    db.execData("Delete tblSaleOrderItemDetail Where soid_SaleOrderItemID = " + v["soit_SaleOrderItemID".ToLower()]);
                                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                    {

                                                        double qty = db.cNum(v["soit_Qty".ToLower()]);
                                                        string itemid = v["soit_ItemID".ToLower()];

                                                        if (rowItem["item_isSet"].ToString() == "Y")
                                                        {
                                                            qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                            itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                                                        }

                                                        if (rowItem["item_isSet"].ToString() == "Y")
                                                        {
                                                            db.execData("Insert into tblSaleOrderItemDetail(soid_SaleOrderItemID,soid_ItemID,soid_Qty) VALUES(" +
                                                                db.sqlStr(id) + "," + db.sqlStr(itemid) + "," +
                                                                (db.cNum(v["soit_Qty".ToLower()])) +
                                                                ")");

                                                        }
                                                        /*
                                                        string tmp = db.execData("Update tblItemWarehouse Set " +
                                                                " itwh_SOAllocation = isNULL(itwh_SOAllocation,0) + " +
                                                                    (qty) +
                                                                " where itwh_WarehouseID = " + db.sqlStr(vals["sord_WarehouseID".ToLower()]) +
                                                                " and itwh_ItemID = " + db.sqlStr(itemid)
                                                                );
                                                        if (tmp != "ok")
                                                        {
                                                            db.rollback();

                                                            tblResult.Rows[0]["status"] = "error";
                                                            tblResult.Rows[0]["msg"] = tmp;
                                                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                                            return re;
                                                        }
                                                        else
                                                        {

                                                            tmp = db.execData("Declare @tot decimal(18,6) " +
                                                                " Select @tot = SUM(isNull(itwh_SOAllocation,0)) " +
                                                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                                " update tblItem set item_SOAllocation = @tot " +
                                                                " where item_ItemID = " + db.sqlStr(itemid)
                                                            );
                                                            if (tmp != "ok")
                                                            {
                                                                db.rollback();

                                                                tblResult.Rows[0]["status"] = "error";
                                                                tblResult.Rows[0]["msg"] = tmp;
                                                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                                                return re;
                                                            }
                                                        }*/
                                                    }
                                                }
                                            }
                                            // End of SO allocation
                                        }
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
                    //invoiceTotal(hid, db);
                    new clsGlobal().SOTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        void invoiceTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblSaleOrder " +
                " Where sord_Deleted is null and sord_SaleOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["sord_Discount"].ToString();
                invo_Disc = db.cNum(row["sord_Disc"].ToString());
                invo_IsTax = db.cNum(row["sord_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(soit_Total,0)) soit_Total From tblSaleOrderItem " +
                " Where soit_Deleted is null and soit_SaleOrderID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["soit_Total"].ToString());
                if (invo_Discount == "P")
                {
                    invo_DiscountAmount = (invo_SubTotal * invo_Disc / 100);
                }
                else
                {
                    invo_DiscountAmount = invo_Disc;
                }
                invo_Total = invo_SubTotal - invo_DiscountAmount;

                //Tax Calculation
                invo_Tax = invo_Total * invo_IsTax / 100;
                invo_GTotal = invo_Total + invo_Tax;

                db.execData("Update tblSaleOrder Set " +
                    " sord_SubTotal = " + invo_SubTotal +
                    ",sord_DiscountAmount = " + invo_DiscountAmount +
                    ",sord_Total = " + invo_Total +
                    ",sord_GTotal = " + invo_GTotal +
                    ",sord_Tax = " + invo_Tax +
                    " Where sord_SaleOrderID = " + eid
                    );
            }
        }
    }
}