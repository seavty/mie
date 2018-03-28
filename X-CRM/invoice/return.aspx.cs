using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.invoice
{
    public partial class _return : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoiceItemReturn";
        string frm = "frmPayment";
        string IDFIeld = "invo_invoiceid";
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

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            /*sapi.readOnlyField.add("soit_ItemID");
                            sapi.readOnlyField.add("soit_Description");
                            sapi.readOnlyField.add("soit_Qty");*/
                            DataTable tbl = db.readData("Select * from tblInvoice Where invo_InvoiceID = " + Request.Form["invo_invoiceid"].ToString());
                            foreach (DataRow row in tbl.Rows)
                            {
                                //if (db.cNum(row["invo_PaidAmount"].ToString()) == 0)
                                    //sapi.readOnlyField.add("init_RPrice");
                            }

                            Response.Write(cls.findRecord(db, screen, "tblInvoiceItemReturn", frm, vals, "", "ASC", -1, 1,
                                " and init_InvoiceID = " + Request.Form["invo_invoiceid"].ToString()));
                        }
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(saveRecord());
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "void")
                        {
                            Response.Write(voidInv());
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
                db.beginTran();
                DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID = " +
                    Request.Form["invo_invoiceid"].ToString());
                foreach (DataRow row in tbl.Rows)
                {
                    Dictionary<string, string> aVal = new Dictionary<string, string>();
                    Dictionary<string, string> nVal = new Dictionary<string, string>();

                    //nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.Parse(row["invo_Date"].ToString()).ToString("yyyy-MM-dd"), 1));
                    nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
                    nVal.Add("crdn_CustomerID".ToLower(), row["invo_CustomerID"].ToString());
                    nVal.Add("crdn_WarehouseID".ToLower(), row["invo_WarehouseID"].ToString());
                    nVal.Add("crdn_InvoiceID".ToLower(), row["invo_InvoiceID"].ToString());
                    double invo_PaidAmount = db.cNum(row["invo_PaidAmount"].ToString()); // db.cNum(db.readData("invo_PaidAmount", "Select invo_PaidAmount From tblInvoice Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString()));
                    re = cls.saveRecord("tblCreditNoteNew", nVal, db, aVals: aVal);

                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                    if (str.tbl != null)
                    {
                        if (str.tbl[0].status == "ok")
                        {
                            string hid = (string)str.tbl[0].msg;

                            foreach (var st in Request.Form.GetValues("N"))
                            {
                                if (db.cNum(vals["init_BQty".ToLower() + st]) > 0)
                                {

                                    nVal.Clear();
                                    aVal.Clear();
                                    aVal.Add("cnit_CreditNoteID", hid);
                                    aVal.Add("cnit_InvoiceID".ToLower(), vals["init_invoiceitemid".ToLower() + st]);
                                    nVal.Add("cnit_ItemID".ToLower(), vals["init_ItemID".ToLower() + st]);
                                    nVal.Add("cnit_Description".ToLower(), vals["init_Description".ToLower() + st]);
                                    nVal.Add("cnit_Qty".ToLower(), vals["init_BQty".ToLower() + st]);

                                    /*if(invo_PaidAmount == 0)
                                        nVal.Add("cnit_Price".ToLower(), "0");
                                    else*/
                                    nVal.Add("cnit_Price".ToLower(), vals["init_RPrice".ToLower() + st]);

                                    double quit_Qty = 0;
                                    double quit_Price = 0;

                                    quit_Qty = db.cNum(nVal["cnit_Qty".ToLower()]);

                                    //double rmqty = db.cNum(db.readData("init_BQty", "Select init_BQty From tblInvoiceItem Where init_Deleted is null and init_InvoiceItemID=" +
                                    //    vals["init_invoiceitemid".ToLower() + st]));
                                    double rmqty = 0;
                                    double init_Qty = 0;
                                    DataTable tblTmp = db.readData("Select init_BQty,init_Qty From tblInvoiceItem Where init_Deleted is null and init_InvoiceItemID = " +
                                         vals["init_invoiceitemid".ToLower() + st]);
                                    foreach (DataRow rowTmp in tblTmp.Rows)
                                    {
                                        rmqty = db.cNum(rowTmp["init_BQty"].ToString());
                                        init_Qty = db.cNum(rowTmp["init_Qty"].ToString());
                                    }
                                    if (quit_Qty > rmqty)
                                    {
                                        quit_Qty = rmqty;
                                    }
                                    if (quit_Qty <= 0)
                                        continue;

                                    quit_Price = db.cNum(nVal["cnit_Price".ToLower()]);
                                    nVal["cnit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();

                                    re = db.execData("Update tblInvoiceItem Set " +
                                        " init_RQty = isNull(init_RQty,0) + " + quit_Qty +
                                        ",init_BQty = isNull(init_Qty,0) - isNull(init_RQty,0) - " + quit_Qty +
                                        " Where init_InvoiceitemID = " + vals["init_invoiceitemid".ToLower() + st]);
                                    if (re != "ok")
                                    {
                                        tblResult.Rows[0]["status"] = "error";
                                        tblResult.Rows[0]["msg"] = re;
                                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                        return re;
                                    }
                                    re = cls.saveRecord("tblCreditNoteItemNew", nVal, db, aVals: aVal);
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

                                            //DataTable tblInvItem = db.readData("Select * from vInvoiceDetail Where invo_InvoiceID=" + url.Get("eid"));
                                            //foreach (DataRow rowInvItem in tblInvItem.Rows)
                                            {
                                                DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                                    " Where itwh_ItemID = " + db.sqlStr(vals["init_ItemID".ToLower() + st]) +
                                                    " and itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()));
                                                foreach (DataRow rowItem in tblItem.Rows)
                                                {
                                                    if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                                    {
                                                        //DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                        //db.sqlStr(vals["init_ItemID".ToLower() + st]));
                                                        DataTable tblItemSet = db.readData("Select * from tblInvoiceItemDetail Where inid_Deleted is null and inid_InvoiceItemID=" +
                                                            db.sqlStr(vals["init_InvoiceItemID".ToLower() + st]));
                                                        if (tblItemSet.Rows.Count <= 0)
                                                            tblItemSet.Rows.Add();
                                                        foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                        {

                                                            double qty = quit_Qty;
                                                            string itemid = vals["init_ItemID".ToLower() + st];
                                                            if (rowItem["item_isSet"].ToString() == "Y")
                                                            {
                                                                //qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                                                                qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString()) / init_Qty;
                                                                itemid = rowItemSet["inid_ItemID"].ToString();
                                                            }
                                                            if (db.readData("item_isStock", "Select item_isStock From tblItem Where item_ItemID = " + itemid) == "Y")
                                                            {
                                                                var tmp = db.execData("Update tblItemWarehouse Set " +
                                                                        " itwh_Qty = isNULL(itwh_Qty,0) + " + qty +
                                                                        " where itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()) +
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
                                                                        " Select @tot = SUM(isNull(itwh_Qty,0)) " +
                                                                        " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                                        " update tblItem set item_Qty = @tot " +
                                                                        " where item_ItemID = " + db.sqlStr(itemid)
                                                                    );

                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            // End of Stock Deduction
                                        }
                                    }
                                    i++;
                                }
                            }
                            clsGlobal clsglobal = new clsGlobal();
                            clsglobal.creditNoteTotal(hid, db);
                            if (db.cNum(row["invo_Balance"].ToString()) > 0)
                            {
                                double totalCN = db.cNum(db.readData("crdn_Total", "select crdn_Total From tblCreditNote Where crdn_CreditNoteID = " + hid));
                                double cnAmount = totalCN;

                                if (db.cNum(row["invo_Balance"].ToString()) >= totalCN)
                                {

                                }
                                else
                                {
                                    cnAmount = db.cNum(row["invo_Balance"].ToString());
                                }
                                var tmp = db.execData("Update tblInvoice Set invo_CreditNote = isNull(invo_CreditNote,0) + " +
                                       cnAmount +
                                        " Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString());
                                clsglobal.invoiceTotal(row["invo_InvoiceID"].ToString(), db);
                                clsglobal.validInvoice(row["invo_InvoiceID"].ToString(), db);
                                tmp = db.execData("Update tblCreditNote Set crdn_UsedAmount = isNULL(crdn_UsedAmount,0) + " + cnAmount +
                                    ",crdn_Balance =​ isNULL(crdn_Total,0) - isNULL(crdn_UsedAmount,0) - " + cnAmount +
                                    " Where crdn_CreditNoteID = " + hid);
                                tmp = db.execData("Insert into tblInvoiceCN(ivcn_InvoiceID,ivcn_CreditNoteID,ivcn_Amount,ivcn_Date) VALUES(" +
                                        row["invo_InvoiceID"].ToString() + "," + hid + "," + cnAmount + ",GETDATE()" +
                                    ")");
                                if (tmp != "ok")
                                {
                                    db.rollback();
                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = "No Item To Return !";
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                }
                            }
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
                    tblResult.Rows[0]["msg"] = "No Item To Return !";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

                }
            }

            return re;
        }


        void creditNoteTotal(string eid, sapi.db db)
        {
            double crnt_Total = 0;



            DataTable tbl = db.readData("Select SUM(isNull(cnit_Total,0)) cnit_Total From tblCreditNoteItem " +
                " Where cnit_Deleted is null and cnit_CreditNoteID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                crnt_Total = db.cNum(row["cnit_Total"].ToString());
                db.execData("Update tblCreditNote Set " +
                    " crdn_Total = " + crnt_Total +
                    " Where crdn_CreditNoteID = " + eid
                    );
            }

        }

        string voidInv()
        {
            string re = "";

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            int i = 0;
            db.beginTran();
            DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID = " +
                Request.Form["invo_invoiceid"].ToString());
            DataTable tblItems = db.readData("Select * from tblInvoiceItem Where init_Deleted is null and init_InvoiceID = " +
                Request.Form["invo_invoiceid"].ToString());
            foreach (DataRow row in tbl.Rows)
            {
                
                Dictionary<string, string> aVal = new Dictionary<string, string>();
                Dictionary<string, string> nVal = new Dictionary<string, string>();

                //nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.Parse(row["invo_Date"].ToString()).ToString("yyyy-MM-dd"), 1));
                nVal.Add("crdn_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
                nVal.Add("crdn_CustomerID".ToLower(), row["invo_CustomerID"].ToString());
                nVal.Add("crdn_WarehouseID".ToLower(), row["invo_WarehouseID"].ToString());
                nVal.Add("crdn_InvoiceID".ToLower(), row["invo_InvoiceID"].ToString());
                double invo_PaidAmount = db.cNum(row["invo_PaidAmount"].ToString()); // db.cNum(db.readData("invo_PaidAmount", "Select invo_PaidAmount From tblInvoice Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString()));
                re = cls.saveRecord("tblCreditNoteNew", nVal, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        string hid = (string)str.tbl[0].msg;

                        foreach (DataRow rowItems in tblItems.Rows)
                        {
                            if (db.cNum(rowItems["init_BQty"].ToString()) > 0)
                            {

                                nVal.Clear();
                                aVal.Clear();
                                aVal.Add("cnit_CreditNoteID", hid);
                                nVal.Add("cnit_ItemID".ToLower(), rowItems["init_ItemID"].ToString());
                                nVal.Add("cnit_Description".ToLower(), rowItems["init_Description"].ToString());
                                nVal.Add("cnit_Qty".ToLower(), rowItems["init_BQty"].ToString());

                                nVal.Add("cnit_Price".ToLower(), rowItems["init_RPrice"].ToString());

                                double quit_Qty = 0;
                                double quit_Price = 0;

                                quit_Qty = db.cNum(nVal["cnit_Qty".ToLower()]);

                                double rmqty = 0;
                                double init_Qty = 0;
                                DataTable tblTmp = db.readData("Select init_BQty,init_Qty From tblInvoiceItem Where init_Deleted is null and init_InvoiceItemID = " +
                                     rowItems["init_invoiceitemid"].ToString());
                                foreach (DataRow rowTmp in tblTmp.Rows)
                                {
                                    rmqty = db.cNum(rowTmp["init_BQty"].ToString());
                                    init_Qty = db.cNum(rowTmp["init_Qty"].ToString());
                                }
                                if (quit_Qty > rmqty)
                                {
                                    quit_Qty = rmqty;
                                }
                                if (quit_Qty <= 0)
                                    continue;

                                quit_Price = db.cNum(nVal["cnit_Price".ToLower()]);
                                nVal["cnit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();

                                re = db.execData("Update tblInvoiceItem Set " +
                                    " init_RQty = isNull(init_RQty,0) + " + quit_Qty +
                                    ",init_BQty = isNull(init_Qty,0) - isNull(init_RQty,0) - " + quit_Qty +
                                    " Where init_InvoiceitemID = " + rowItems["init_invoiceitemid"].ToString());
                                if (re != "ok")
                                {
                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = re;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                    return re;
                                }
                                re = cls.saveRecord("tblCreditNoteItemNew", nVal, db, aVals: aVal);
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

                                        DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(rowItems["init_ItemID"].ToString()) +
                                            " and itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()));
                                        foreach (DataRow rowItem in tblItem.Rows)
                                        {
                                            if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                            {
                                                //DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                //db.sqlStr(vals["init_ItemID".ToLower() + st]));
                                                DataTable tblItemSet = db.readData("Select * from tblInvoiceItemDetail Where inid_Deleted is null and inid_InvoiceItemID=" +
                                                    db.sqlStr(rowItems["init_InvoiceItemID"].ToString()));
                                                if (tblItemSet.Rows.Count <= 0)
                                                    tblItemSet.Rows.Add();
                                                foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                {

                                                    double qty = quit_Qty;
                                                    string itemid = rowItems["init_ItemID"].ToString();
                                                    if (rowItem["item_isSet"].ToString() == "Y")
                                                    {
                                                        //qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                                                        qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString()) / init_Qty;
                                                        itemid = rowItemSet["inid_ItemID"].ToString();
                                                    }
                                                    if (db.readData("item_isStock", "Select item_isStock From tblItem Where item_ItemID = " + itemid) == "Y")
                                                    {
                                                        var tmp = db.execData("Update tblItemWarehouse Set " +
                                                                " itwh_Qty = isNULL(itwh_Qty,0) + " + qty +
                                                                " where itwh_WarehouseID = " + db.sqlStr(row["invo_WarehouseID"].ToString()) +
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
                                                                " Select @tot = SUM(isNull(itwh_Qty,0)) " +
                                                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                                " update tblItem set item_Qty = @tot " +
                                                                " where item_ItemID = " + db.sqlStr(itemid)
                                                            );

                                                        }
                                                    }
                                                }
                                            }

                                        }

                                        // End of Stock Deduction
                                    }
                                }
                                i++;
                            }
                        }
                        clsGlobal clsglobal = new clsGlobal();
                        clsglobal.creditNoteTotal(hid, db);
                        if (db.cNum(row["invo_Balance"].ToString()) > 0)
                        {
                            double totalCN = db.cNum(db.readData("crdn_Total", "select crdn_Total From tblCreditNote Where crdn_CreditNoteID = " + hid));
                            double cnAmount = totalCN;

                            if (db.cNum(row["invo_Balance"].ToString()) >= totalCN)
                            {

                            }
                            else
                            {
                                cnAmount = db.cNum(row["invo_Balance"].ToString());
                            }
                            var tmp = db.execData("Update tblInvoice Set invo_CreditNote = isNull(invo_CreditNote,0) + " +
                                   cnAmount +
                                    " Where invo_InvoiceID = " + row["invo_InvoiceID"].ToString());
                            clsglobal.invoiceTotal(row["invo_InvoiceID"].ToString(), db);
                            clsglobal.validInvoice(row["invo_InvoiceID"].ToString(), db);
                            tmp = db.execData("Update tblCreditNote Set crdn_UsedAmount = isNULL(crdn_UsedAmount,0) + " + cnAmount +
                                ",crdn_Balance =​ isNULL(crdn_Total,0) - isNULL(crdn_UsedAmount,0) - " + cnAmount +
                                " Where crdn_CreditNoteID = " + hid);
                            tmp = db.execData("Insert into tblInvoiceCN(ivcn_InvoiceID,ivcn_CreditNoteID,ivcn_Amount,ivcn_Date) VALUES(" +
                                    row["invo_InvoiceID"].ToString() + "," + hid + "," + cnAmount + ",GETDATE()" +
                                ")");
                            if (tmp != "ok")
                            {
                                db.rollback();
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "No Item To Return !";
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            }
                        }
                    }
                }

            }
            if (i > 0)
            {
                db.execData("Update tblInvoice set invo_Status='Void' Where invo_InvoiceID = " + Request.Form["invo_invoiceid"].ToString());
                db.commit();
            }
            else
            {
                db.rollback();
                tblResult.Rows[0]["status"] = "error";
                tblResult.Rows[0]["msg"] = "No Item To Void !";
                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

            }

            return re;
        }

    }
}