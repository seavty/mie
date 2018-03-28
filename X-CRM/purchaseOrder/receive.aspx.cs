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
    public partial class receive : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblPurchaseOrderItemReceive";
        string frm = "frmPayment";
        string IDFIeld = "purc_purchaseorderid";
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

                        if (Request.Form["app"].ToString() == "completePO")
                        {
                            Response.Write(db.execData("Update tblPurchaseOrder Set purc_isComplete = 'Y' Where purc_PurchaseOrderID = " + Request.Form["purc_purchaseorderid"].ToString()));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            Response.Write(cls.findRecord(db, screen, "tblPurchaseOrderItemList", frm, vals, "", "ASC", -1, 1,
                                " and poit_PurchaseOrderID = " + Request.Form["purc_purchaseorderid"].ToString()));
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
            string hid = "";
            string screenItem = "tblReceiveItemNew";
            string screen = "tblReceiveNew";
            string purc_purchaseorderid = Request.Form["purc_purchaseorderid"].ToString();
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblPO = db.readData("Select * from tblPurchaseOrder Where purc_Deleted is null and purc_PurchaseOrderID = " + purc_purchaseorderid);

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            foreach (DataRow rowPO in tblPO.Rows)
            {
                if (rowPO["purc_isComplete"].ToString() == "Y")
                {
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = "Purchase Order has already completed !";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    return re;
                }

                db.beginTran();

                if (!createInvoice(rowPO["purc_purchaseorderid"].ToString()))
                {
                    db.rollback();
                    db.close();
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = "Error Creating AP Invoice";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    return re;
                    
                }
                Dictionary<string, string> rVals = new Dictionary<string, string>();
                rVals.Add("rece_WarehouseID".ToLower(), rowPO["purc_WarehouseID"].ToString());
                rVals.Add("rece_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                rVals.Add("rece_ReceivedBy".ToLower(), Session["userid"].ToString());
                rVals.Add("rece_Remark".ToLower(), "");
                rVals.Add("rece_PurchaseOrderID".ToLower(), rowPO["purc_PurchaseOrderID"].ToString());

                re = cls.saveRecord(screen, rVals, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {

                    if (str.tbl[0].status == "ok")
                    {
                        clsGlobal clsglobal = new clsGlobal();


                        hid = (string)str.tbl[0].msg;


                        Dictionary<string, string> v = new Dictionary<string, string>();
                        //DataTable tblPOItem = db.readData("Select * from tblPurchaseOrderItem Where poit_Deleted is null and poit_PurchaseOrderID = " + purc_purchaseorderid);
                        //foreach (DataRow rowPOItem in tblPOItem.Rows)
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            aVal.Add("reit_receiveid", hid);
                            v.Clear();
                            v.Add("reit_ItemID".ToLower(), vals["poit_ItemID".ToLower() + st].ToString());
                            v.Add("reit_Qty".ToLower(), vals["poit_RemainQty".ToLower() + st].ToString());
                            v.Add("reit_Price".ToLower(), vals["poit_Price".ToLower() + st].ToString());
                            v.Add("reit_Total".ToLower(), vals["poit_Total".ToLower() + st].ToString());


                            double quit_Qty = 0;
                            double quit_Price = 0;
                            if (v.ContainsKey("reit_Qty".ToLower()))
                            {
                                quit_Qty = db.cNum(v["reit_Qty".ToLower()]);
                            }
                            if (v.ContainsKey("reit_Price".ToLower()))
                            {
                                quit_Price = db.cNum(v["reit_Price".ToLower()]);
                            }

                            if (v.ContainsKey("reit_Total".ToLower()))
                            {
                                v["reit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                            }

                            

                            string re2 = db.execData("Update tblPurchaseOrderItem Set " +
                                            " poit_ReceivedQty = isNull(poit_ReceivedQty,0) + " + quit_Qty +
                                            ",poit_RemainQty = isNull(poit_Qty,0) - isNull(poit_ReceivedQty,0) - " + quit_Qty +
                                            " Where poit_PurchaseOrderItemID = " + vals["poit_PurchaseOrderItemID".ToLower() + st].ToString());
                            if (re2 != "ok")
                            {
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = re2;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re2;
                            }

                            re2 = cls.saveRecord(screenItem, v, db, aVal);
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
                                    clsglobal.receiveItem(db, v, rowPO["purc_WarehouseID"].ToString());
                                }

                            }

                            if (str.error != null)
                            {
                                db.rollback();
                                return re2;
                            }

                        }

                        clsglobal.ReceiveTotal(hid, db);
                        db.commit();
                    }
                }
            }
            return re;
        }

        bool createInvoice(string poid)
        {
            bool res = true;
            string re = "";
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            Dictionary<string, string> aVal = new Dictionary<string, string>();
            Dictionary<string, string> nVal = new Dictionary<string, string>();

            DataTable tbl = db.readData("Select * from tblPurchaseOrder Where purc_Deleted is null and purc_PurchaseOrderID = " +
                    poid);
            foreach (DataRow row in tbl.Rows)
            {
                nVal.Add("apiv_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1)); //db.getDate(DateTime.Parse(row["sord_Date"].ToString()).ToString("yyyy-MM-dd"), 1));
                nVal.Add("apiv_SupplierID".ToLower(), row["purc_SupplierID"].ToString());
                nVal.Add("apiv_WarehouseID".ToLower(), row["purc_WarehouseID"].ToString());
                nVal.Add("apiv_Status".ToLower(), "Completed");
                nVal.Add("apiv_Discount".ToLower(), "");
                nVal.Add("apiv_Disc".ToLower(), "0");
                nVal.Add("apiv_DiscountAmount".ToLower(), "0");
                nVal.Add("apiv_isTax".ToLower(), row["purc_isTax"].ToString());

                aVal.Add("apiv_PurchaseOrderID", poid);
                aVal.Add("apiv_WorkflowID", "6");
                aVal.Add("apiv_WorkflowItemID", "13");

                re = cls.saveRecord("tblAPInvoiceNew", nVal, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        string hid = (string)str.tbl[0].msg;

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (db.cNum(vals["poit_RemainQty".ToLower() + st]) > 0)
                            {

                                nVal.Clear();
                                aVal.Clear();
                                aVal.Add("apit_APInvoiceID", hid);
                                nVal.Add("apit_ItemID".ToLower(), vals["poit_ItemID".ToLower() + st]);
                                nVal.Add("apit_Description".ToLower(), vals["poit_Description".ToLower() + st]);
                                nVal.Add("apit_Qty".ToLower(), vals["poit_RemainQty".ToLower() + st]);
                                nVal.Add("apit_Price".ToLower(), vals["poit_Price".ToLower() + st]);

                                /*aVal.Add("apit_RPrice", vals["poit_Price".ToLower() + st]);
                                aVal.Add("apit_RQty", "0");
                                aVal.Add("apit_BQty", vals["poit_RemainQty".ToLower() + st]);*/


                                double quit_Qty = 0;
                                double quit_Price = 0;

                                quit_Qty = db.cNum(nVal["apit_Qty".ToLower()]);
                                double rmqty = 0;
                                double soit_Qty = 0;
                                DataTable tblTmp = db.readData("Select poit_RemainQty,poit_Qty From tblPurchaseOrderItem Where poit_Deleted is null and poit_PurchaseOrderItemID=" +
                                     vals["poit_purchaseorderitemid".ToLower() + st]);
                                foreach (DataRow rowTmp in tblTmp.Rows)
                                {
                                    rmqty = db.cNum(rowTmp["poit_RemainQty"].ToString());
                                    soit_Qty = db.cNum(rowTmp["poit_Qty"].ToString());
                                }

                                if (quit_Qty > rmqty)
                                {
                                    quit_Qty = rmqty;
                                }
                                if (quit_Qty <= 0)
                                    continue;


                                quit_Price = db.cNum(nVal["apit_Price".ToLower()]);
                                nVal["apit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();

                                re = cls.saveRecord("tblAPInvoiceItemNew", nVal, db, aVals: aVal);
                                str = JsonConvert.DeserializeObject<dynamic>(re);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();

                                        tblResult.Rows[0]["status"] = "error";
                                        tblResult.Rows[0]["msg"] = str.tbl[0].msg;
                                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                        return false;
                                    }

                                }

                            }
                        }

                        new clsGlobal().APInvoiceTotal(hid, db);
                        new clsGlobal().validPO(poid, db);

                    }else{
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return res;
        }
    }
}