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
    public partial class shipment : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblSaleOrderItemShip";
        string frm = "frmPayment";
        string IDFIeld = "sord_saleorderid";
        string Tab = "";
        string cTab = "";

        protected void Page_Load(object sender, EventArgs e)
        {

            init();
            if (Request.Form["app"] != null)
            {
                try
                {
                    if (db.connect())
                    {
                        if (Request.Form["app"].ToString() == "completeSO")
                        {
                            Response.Write(db.execData("Update tblSaleOrder Set sord_isComplete = 'Y' Where sord_SaleOrderID = " + Request.Form["sord_saleorderid"].ToString()));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }


                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            /*sapi.readOnlyField.add("soit_ItemID");
                            sapi.readOnlyField.add("soit_Description");
                            sapi.readOnlyField.add("soit_Qty");
                            sapi.readOnlyField.add("soit_Price");
                            */
                            Response.Write(cls.findRecord(db, screen, "tblSaleOrderItemList", frm, vals, "", "ASC", -1, 1,
                                " and soit_SaleOrderID = " + Request.Form["sord_saleorderid"].ToString()));
                        }
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(saveRecord());
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

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
            }
            string re = "";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid);


            return re;
        }

        string saveRecord()
        {
            string re = "";
            if (Request.Form.GetValues("N") != null)
            {
                DataTable tblResult = new DataTable();
                tblResult.Rows.Add();
                tblResult.Columns.Add("status");
                tblResult.Columns.Add("msg");

                //tblResult.Rows[0]["status"] = "error";
                //tblResult.Rows[0]["msg"] = tmp;
                //re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                int i = 0;

                DataTable tbl = db.readData("Select * from tblSaleOrder Where sord_Deleted is null and sord_SaleOrderID = " +
                    Request.Form["sord_saleorderid"].ToString());

                // verify stock
                if (tbl.Rows.Count > 0)
                {
                    if (tbl.Rows[0]["sord_isComplete"].ToString() == "Y")
                    {
                        tblResult.Rows[0]["status"] = "error";
                        tblResult.Rows[0]["msg"] = "Sale Order has already completed !";
                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        return re;
                    }

                    foreach (var st in Request.Form.GetValues("N"))
                    {
                        Dictionary<string, string> v = cls.getItemVals("tblSaleOrderItemShip", vals, db, st);
                        if (db.cNum(v["soit_RemainQty".ToLower()]) <= 0)
                            continue;

                        double exQty = 0;

                        if (v.ContainsKey("init_invoiceitemid"))
                        {
                            if (!string.IsNullOrEmpty(v["init_invoiceitemid"]))
                                exQty = db.cNum(db.readData("init_Qty",
                                    "Select init_Qty From tblInvoiceItem Where init_InvoiceItemID=" + v["init_invoiceitemid"]));
                        }

                        string strErr = "";
                        DataTable tblSOIT = db.readData("select * from tblSaleOrderitem Where soit_Deleted is null and soit_SaleOrderitemID = " + v["soit_saleorderitemid".ToLower()]);
                        foreach (DataRow rowSOIT in tblSOIT.Rows)
                        {
                            string wh = tbl.Rows[0]["sord_WarehouseID"].ToString();
                            if (!string.IsNullOrEmpty(rowSOIT["soit_warehouseid"].ToString()))
                                wh = rowSOIT["soit_warehouseid"].ToString();

                            DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                " Where itwh_ItemID = " + db.sqlStr(v["soit_ItemID".ToLower()]) +
                                " and itwh_WarehouseID = " + db.sqlStr(wh));
                            foreach (DataRow rowItem in tblItem.Rows)
                            {
                                double qty = 0;
                                int n = Request.Form.GetValues("N").Length;
                                for (int st1 = 0; st1 < n; st1++)
                                {
                                    bool isWH = false;
                                    if (vals.ContainsKey("soit_WarehouseID".ToLower() + st))
                                    {
                                        if (vals["soit_WarehouseID".ToLower() + st] == vals["soit_WarehouseID".ToLower() + st1])
                                            isWH = true;
                                    }
                                    if (vals["soit_ItemID".ToLower() + st] == vals["soit_ItemID".ToLower() + st1] && isWH
                                        )
                                    {
                                        qty = qty + db.cNum(vals["soit_Qty".ToLower() + st1]);
                                    }
                                    /*
                                    if (vals["soit_ItemID".ToLower() + st] == vals["soit_ItemID".ToLower() + st1])
                                    {
                                        qty = qty + db.cNum(vals["soit_RemainQty".ToLower() + st1]);
                                    }*/
                                }
                                qty = qty - exQty;
                                if (rowItem["item_isSet"].ToString() == "Y")
                                {
                                    DataTable tblItemSet = db.readData("Select * from tblSaleOrderItemDetail " +
                                        " left JOIN tblItem on soid_ItemID = item_ItemID " +
                                        " Where soid_Deleted is null and soid_SaleOrderItemID=" + v["soit_SaleorderItemID".ToLower()]);
                                    string prestrErr = "";
                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                    {
                                        if (rowItemSet["item_IsStock"].ToString() == "Y")
                                        {
                                            double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                                " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                                " and itwh_WarehouseID = " + db.sqlStr(wh)));

                                            //qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                                            qty = db.cNum(rowItemSet["soid_Qty"].ToString());
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
                        }
                        if (strErr.Length > 0)
                        {

                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = "<h3>Items out of stock : </h3><hr class='thin bg-grayLighter'/><br/>" + strErr;
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            return re;
                        }
                    }
                }

                db.beginTran();

                foreach (DataRow row in tbl.Rows)
                {
                    Dictionary<string, string> aVal = new Dictionary<string, string>();
                    Dictionary<string, string> nVal = new Dictionary<string, string>();

                    nVal.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1)); //db.getDate(DateTime.Parse(row["sord_Date"].ToString()).ToString("yyyy-MM-dd"), 1));
                    nVal.Add("invo_CustomerID".ToLower(), row["sord_CustomerID"].ToString());
                    nVal.Add("invo_WarehouseID".ToLower(), row["sord_WarehouseID"].ToString());
                    nVal.Add("invo_Status".ToLower(), "Completed");

                    nVal.Add("invo_Discount".ToLower(), row["sord_Discount"].ToString());
                    nVal.Add("invo_Disc".ToLower(), row["sord_Disc"].ToString());

                    nVal.Add("invo_DiscountAmount".ToLower(), "0");
                    nVal.Add("invo_isTax".ToLower(), row["sord_isTax"].ToString());

                    if (tbl.Columns.Contains("sord_Province"))
                        nVal.Add("invo_Province".ToLower(), row["sord_Province"].ToString());
                    if (tbl.Columns.Contains("sord_Company"))
                        nVal.Add("invo_Company".ToLower(), row["sord_Company"].ToString());
                    if (tbl.Columns.Contains("sord_PriceListID"))
                        nVal.Add("invo_PriceListID".ToLower(), row["sord_PriceListID"].ToString());
                    if (tbl.Columns.Contains("sord_SalesmanID"))
                        nVal.Add("invo_SalesmanID".ToLower(), row["sord_SalesmanID"].ToString());

                    aVal.Add("invo_SaleOrderID", Request.Form["sord_saleorderid"].ToString());
                    aVal.Add("invo_WorkflowID", "6");
                    aVal.Add("invo_WorkflowItemID", "13");

                    re = cls.saveRecord("tblInvoiceNew", nVal, db, aVals: aVal, ignoreROF: true);

                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                    if (str.tbl != null)
                    {
                        if (str.tbl[0].status == "ok")
                        {
                            string hid = (string)str.tbl[0].msg;

                            foreach (var st in Request.Form.GetValues("N"))
                            {
                                if (db.cNum(vals["soit_RemainQty".ToLower() + st]) > 0)
                                {

                                    nVal.Clear();
                                    aVal.Clear();
                                    aVal.Add("init_InvoiceID", hid);
                                    nVal.Add("init_ItemID".ToLower(), vals["soit_ItemID".ToLower() + st]);
                                    nVal.Add("init_Description".ToLower(), vals["soit_Description".ToLower() + st]);
                                    nVal.Add("init_Qty".ToLower(), vals["soit_RemainQty".ToLower() + st]);
                                    nVal.Add("init_Price".ToLower(), vals["soit_Price".ToLower() + st]);
                                    if (vals.ContainsKey("soit_Unit".ToLower() + st))
                                        nVal.Add("init_Unit", vals["soit_Unit".ToLower() + st]);

                                    aVal.Add("init_RPrice", vals["soit_Price".ToLower() + st]);
                                    aVal.Add("init_RQty", "0");
                                    aVal.Add("init_BQty", vals["soit_RemainQty".ToLower() + st]);

                                    double init_Cost = db.cNum(db.readData("item_Cost", "Select item_Cost From tblItem " +
                                    " Where item_ItemID = " + vals["soit_ItemID".ToLower() + st]));
                                    aVal.Add("init_Cost", init_Cost.ToString());

                                    

                                    string wh = row["sord_WarehouseID"].ToString();

                                    double quit_Qty = 0;
                                    double quit_Price = 0;

                                    quit_Qty = db.cNum(nVal["init_Qty".ToLower()]);
                                    double rmqty = 0;
                                    double soit_Qty = 0;
                                    DataTable tblTmp = db.readData("Select soit_RemainQty,soit_Qty,soit_WarehouseID From tblSaleOrderItem Where soit_Deleted is null and soit_SaleOrderItemID=" +
                                         vals["soit_saleorderitemid".ToLower() + st]);
                                    foreach (DataRow rowTmp in tblTmp.Rows)
                                    {
                                        rmqty = db.cNum(rowTmp["soit_RemainQty"].ToString());
                                        soit_Qty = db.cNum(rowTmp["soit_Qty"].ToString());
                                        if (string.IsNullOrEmpty(rowTmp["soit_warehouseID"].ToString()))
                                        {
                                            nVal.Add("init_WarehouseID".ToLower(), row["sord_WarehouseID"].ToString());

                                        }
                                        else
                                        {
                                            nVal.Add("init_WarehouseID".ToLower(), rowTmp["soit_WarehouseID"].ToString());
                                            wh = rowTmp["soit_WarehouseID"].ToString();
                                        }
                                    }

                                    if (quit_Qty > rmqty)
                                    {
                                        quit_Qty = rmqty;
                                    }
                                    if (quit_Qty <= 0)
                                        continue;


                                    quit_Price = db.cNum(nVal["init_Price".ToLower()]);
                                    nVal["init_Total".ToLower()] = (quit_Qty * quit_Price).ToString();

                                    re = db.execData("Update tblSaleOrderItem Set " +
                                        " soit_ShipQty = isNull(soit_ShipQty,0) + " + quit_Qty +
                                        ",soit_RemainQty = isNull(soit_Qty,0) - isNull(soit_ShipQty,0) - " + quit_Qty +
                                        " Where soit_SaleOrderItemID = " + vals["soit_saleorderitemid".ToLower() + st]);
                                    if (re != "ok")
                                    {
                                        tblResult.Rows[0]["status"] = "error";
                                        tblResult.Rows[0]["msg"] = re;
                                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                        return re;
                                    }
                                    re = cls.saveRecord("tblInvoiceItemNew", nVal, db, aVals: aVal);
                                    str = JsonConvert.DeserializeObject<dynamic>(re);
                                    if (str.tbl != null)
                                    {
                                        if (str.tbl[0].status != "ok")
                                        {
                                            db.rollback();

                                            tblResult.Rows[0]["status"] = "error";
                                            tblResult.Rows[0]["msg"] = str.tbl[0].msg;
                                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                            return re;
                                        }
                                        else
                                        {
                                            DataTable tblSOItemDtl = db.readData("Select * from tblSaleOrderItemDetail where soid_Deleted is null and soid_SaleOrderItemID = " + vals["soit_saleorderitemid".ToLower() + st]);
                                            foreach (DataRow rowSOItemDtl in tblSOItemDtl.Rows)
                                            {
                                                db.execData("Insert into tblInvoiceItemDetail(inid_InvoiceItemID,inid_ItemID,inid_Qty) VALUES(" +
                                                                db.sqlStr((string)str.tbl[0].msg) + "," + db.sqlStr(vals["soit_ItemID".ToLower() + st]) + "," +
                                                                (quit_Qty * db.cNum(rowSOItemDtl["soid_Qty"].ToString()) / soit_Qty) +
                                                                ")");
                                            }
                                        }
                                    }

                                    // stock Deduction

                                    DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                        " Where itwh_ItemID = " + db.sqlStr(vals["soit_ItemID".ToLower() + st]) +
                                        " and itwh_WarehouseID = " + db.sqlStr(wh));
                                    foreach (DataRow rowItem in tblItem.Rows)
                                    {
                                        if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                        {
                                            //DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                            //    vals["soit_ItemID".ToLower() + st]);
                                            DataTable tblItemSet = db.readData("Select * from tblSaleOrderItemDetail " +
                                                " Where soid_Deleted is null and soid_SaleOrderItemID=" +
                                                vals["soit_SaleOrderItemID".ToLower() + st]);
                                            if (tblItemSet.Rows.Count <= 0)
                                            {
                                                tblItemSet.Rows.Add();
                                            }
                                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                                            {

                                                double qty = quit_Qty;
                                                string itemid = vals["soit_ItemID".ToLower() + st];
                                                if (rowItem["item_isSet"].ToString() == "Y")
                                                {
                                                    //qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                    //itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                                                    qty = db.cNum(rowItemSet["soid_Qty"].ToString());
                                                    itemid = rowItemSet["soid_ItemID"].ToString();
                                                }
                                                string tmp = db.execData("Update tblItemWarehouse Set " +
                                                    //" itwh_SOAllocation = isNULL(itwh_SOAllocation,0) - " + qty +
                                                        " itwh_Qty = isNULL(itwh_Qty,0) - " + qty +
                                                        " where itwh_WarehouseID = " + db.sqlStr(wh) +
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
                                                        " Declare @tot2 decimal(18,6) " +
                                                        " Select " +
                                                        " @tot = SUM(isNull(itwh_SOAllocation,0)) " +
                                                        ",@tot2 = SUM(isNull(itwh_Qty,0)) " +
                                                        " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                        " update tblItem set item_SOAllocation = @tot " +
                                                        ",item_Qty = @tot2 " +
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
                                                }
                                            }
                                        }
                                    }
                                    // End of stock Deduction
                                    i++;
                                }
                            }
                            double deposit = db.cNum(row["sord_Deposit"].ToString()) - db.cNum(row["sord_DepositUsed"].ToString());
                            //invoiceTotal(hid, db, deposit, row["sord_SaleOrderID"].ToString());
                            new clsGlobal().invoiceTotal(hid, db, deposit, row["sord_SaleOrderID"].ToString());
                            new clsGlobal().validSO(Request.Form["sord_saleorderid"].ToString(), db);


                        }
                    }
                }
                if (i > 0)
                {
                    db.commit();
                }
                else
                {
                    db.rollback();
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = "No Item To Ship !";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

                }
            }

            return re;
        }

        void invoiceTotal(string eid, sapi.db db, double deposit, string soid)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblInvoice " +
                " Where invo_Deleted is null and invo_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["invo_Discount"].ToString();
                invo_Disc = db.cNum(row["invo_Disc"].ToString());
                invo_IsTax = db.cNum(row["invo_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(init_Total,0)) init_Total,SUM(isNull(init_Cost,0) * isNull(init_Qty,0)) totalCost From tblInvoiceItem " +
                " Where init_Deleted is null and init_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["init_Total"].ToString());
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


                double rDeposit = deposit;
                if (deposit > invo_GTotal)
                {
                    rDeposit = invo_GTotal;
                }
                if (rDeposit > 0)
                {
                    /*
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    Dictionary<string, string> aVals = new Dictionary<string, string>();
                    vals.Add("ivpm_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                    vals.Add("ivpm_InvoiceID".ToLower(), eid);
                    vals.Add("ivpm_Amount".ToLower(), rDeposit.ToString());
                    vals.Add("ivpm_Remark".ToLower(), "");

                    cls.saveRecord("tblInvoicePaymentNew", vals, db, aVals);
                    
                    */
                    db.execData("Update tblSaleOrder set sord_DepositUsed = IsNULL(sord_DepositUsed,0) + " + rDeposit +
                        " Where sord_SaleOrderID = " + soid);

                }

                db.execData("Update tblInvoice Set " +
                    " invo_SubTotal = " + invo_SubTotal +
                    ",invo_DiscountAmount = " + invo_DiscountAmount +
                    ",invo_Total = " + invo_Total +
                    ",invo_GTotal = " + invo_GTotal +
                    ",invo_Tax = " + invo_Tax +
                    ",invo_PaidAmount = 0 " +
                    ",invo_Deposit = " + rDeposit +
                    //",invo_Balance = " + invo_GTotal + " - isNull(invo_PaidAmount,0) " +
                    ",invo_Balance = " + (invo_GTotal - rDeposit) +
                    ",invo_Cost = " + db.cNum(row["totalCost"].ToString()) +
                    " Where invo_InvoiceID = " + eid
                    );
            }
        }
    }

}