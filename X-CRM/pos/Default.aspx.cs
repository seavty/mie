using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using Newtonsoft.Json;

namespace POS
{
    public partial class Default : System.Web.UI.Page
    {
        string WH = "3";
        double Rate = 4000;
        string tax = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["user"] != null)
            {
                lblUserName.InnerHtml = Session["user"].ToString();
            }
            else
            {
                Response.Redirect("../Default.aspx");
            }
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {

                    dvItemGroup.InnerHtml = getItemGroup(db);
                    DataTable tblSetting = db.readData("Select * from sys_Setting " +
                        " LEFT JOIN tblCustomer on cust_CustomerID = sett_CustomerID");
                    foreach (DataRow row in tblSetting.Rows)
                    {
                        tax = db.cNum(row["sett_isTax"].ToString()).ToString();
                        Rate = db.cNum(row["sett_ExRate"].ToString());
                        WH = row["sett_WarehouseID"].ToString();
                        invo_isTax.Value = tax;
                        txtExRate.Value = Rate.ToString();
                        defCust.Value = row["cust_CustomerID"].ToString();
                        defCustName.Value = row["cust_Name"].ToString();
                    }
                    //dvItem.InnerHtml = getItem(db);
                    if (Request.Form["app"] != null)
                    {
                        sapi.sapi cls = new sapi.sapi();
                        if (Request.Form["app"].ToString() == "getItem")
                        {
                            Response.Write(getItem(db, Request.Form["itmg_ItemGroupID"].ToString()));
                        }

                        if (Request.Form["app"].ToString() == "bill")
                        {
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            Response.Write(bill(vals, db));
                        }

                        if (Request.Form["app"].ToString() == "getCustomer" || (Request.Form["app"].ToString() == "findRecord" && Request.Form["screen"].ToString() == "tblCustomerFind"))
                        {
                            string q = Request.Form["q"].ToString();
                            Response.Write(cls.findRecord(db, "tblCustomerFind", "tblCustomerListPOS", "frmList", null, "",
                                filter: " and cust_Name like " + db.sqlStrLike(q), assc: 3));
                        }

                        if (Request.Form["app"].ToString() == "getItemToList" || (Request.Form["app"].ToString() == "findRecord" && Request.Form["screen"].ToString() == "tblItemFind"))
                        {
                            string q = Request.Form["q"].ToString();
                            Response.Write(cls.findRecord(db, "tblItemFind", "tblItemList", "frmList", null, "",
                                filter: 
                                    " and (" +
                                        " item_Name like " + db.sqlStrLike(q) +
                                        " OR item_Code like " + db.sqlStrLike(q) +
                                    ")"
                                    , assc: 3));
                        }

                        if (Request.Form["app"].ToString() == "scanItem")
                        {
                            string q = Request.Form["q"].ToString();
                            Response.Write(db.tblToJson(db.readData("Select * from vItemWarehouse " +
                                " Where ware_WarehouseID = " + WH +
                                " and item_code = " + db.sqlStr(q))));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();

                    }
                }
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                Response.End();
            }
            finally { db.close(); }
        }

        string getItemGroup(sapi.db db)
        {
            StringBuilder str = new StringBuilder();
            DataTable tbl = db.readData("Select * from tblItemGroup Where itmg_Deleted is null");
            int i = 0;
            string[] color = { "red", "blue", "orange", "green" };
            foreach (DataRow row in tbl.Rows)
            {
                if (i == 4) i = 0;
                str.Append("<div eid='" + row["itmg_ItemGroupID"].ToString() + "' class='itemGroup tile bg-" + color[i] + " fg-white itemGroup' data-role='tile'>" +
                    "<div class='tile-content iconic'>" +
                    //"<span class='icon mif-cogs'></span>" +
                    //"<span class='tile-badge bg-white fg-black'>" +
                    //"6</span>" +
                        "<span class='tile-label'>" +
                            row["itmg_Name"].ToString() +
                        "</span>" +
                    "</div>" +
                "</div>");
                i++;
            }

            return str.ToString();
        }

        string getItem(sapi.db db, string itemGroupID)
        {
            StringBuilder str = new StringBuilder();
            DataTable tbl = db.readData("Select * from tblItem Where item_Deleted is null " +
                (string.IsNullOrEmpty(itemGroupID) ? "" : "and item_ItemGroupID=" + db.cNum(itemGroupID)));
            int i = 0;

            foreach (DataRow row in tbl.Rows)
            {
                str.Append("<div eid='" + row["item_ItemID"].ToString() + "' class='item tile-wide bg-blue fg-white' data-role='tile'>" +
                    "<div class='tile-content  slide-up'>" +
                        "<div class='slide'>" +
                            (System.IO.File.Exists("~/imgs/" + row["item_ItemID"].ToString() + ".jpg") ? 
                            "<div class='image-container image-format-square' style='width: 100%;'>" +
                                "<div class='frame'>" +
                                    "<div style='width: 100%; height: 150px; background-image: url(\"imgs/" + row["item_ItemID"].ToString() + ".jpg\"); background-size: cover; background-repeat: no-repeat; border-radius: 0px;'></div>" +
                                "</div>" +
                            "</div>"  : "") +
                        "</div>" +
                    //"<span class='icon mif-cogs'></span>" +
                        "<span class='tile-badge bg-red fg-white text-shadow ' id='lblPrice'>" +
                        db.cNum(row["item_Price"].ToString()).ToString("#,##0.00") + "</span>" +
                        "<span class='tile-label text-shadow text-enlarged fg-black'>" +
                            row["item_Name"].ToString() +
                        "</span>" +
                        "<div class='slide-over op-gray padding10'>" +
                            row["item_Name"].ToString() +
                        "</div>" +
                    "</div>" +
                "</div>");
                i++;
            }
            str.Append("</div>");
            return str.ToString();
        }
        /*
         string getItem(sapi.db db)
        {
            StringBuilder str = new StringBuilder();
            DataTable tbl = db.readData("Select * from tblItem Where item_Deleted is null");
            int i = 0;
            foreach (DataRow row in tbl.Rows)
            {
                if (i % 4 == 0 && i > 0)
                {
                    str.Append("</div>");
                    i = 0;
                }
                if (i == 0)
                {
                    str.Append("<div class='row cells4'>");
                }

                str.Append("<div class='cell item rounded button'>" +
                    row["item_Name"].ToString() +
                "</div>");
                i++;
            }
            str.Append("</div>");
            return str.ToString();
        }
         */

        string bill(Dictionary<string, string> rVals, sapi.db db)
        {
            sapi.sapi cls = new sapi.sapi();
            string re = "";
            Dictionary<string, string> vals = new Dictionary<string, string>();
            Dictionary<string, string> aVals = new Dictionary<string, string>();

            int n = int.Parse(Request.Form["n"].ToString());
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");


            // Stock Deduction
            string strErr = "";
            double qty = 0;
            for (int st = 0; st < n; st++)
            {
                DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                    " Where itwh_ItemID = " + db.sqlStr(rVals["init_ItemID".ToLower() + st]) +
                    " and itwh_WarehouseID = " + db.sqlStr(WH));
                foreach (DataRow rowItem in tblItem.Rows)
                {
                    if (rVals["txtDel".ToLower() + st] == "")
                    {
                        if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                        {
                            qty = 0;
                            for (int st1 = 0; st1 < n; st1++)
                            {
                                if (rVals["init_ItemID".ToLower() + st] == rVals["init_ItemID".ToLower() + st1])
                                {
                                    qty = qty + db.cNum(rVals["init_Qty".ToLower() + st1]);
                                }
                            }

                            if (rowItem["item_isSet"].ToString() == "Y")
                            {
                                DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                    " Where sitm_Deleted is null and sitm_ItemID=" + rVals["init_ItemID".ToLower() + st]);
                                string prestrErr = "";
                                foreach (DataRow rowItemSet in tblItemSet.Rows)
                                {
                                    if (rowItemSet["item_IsStock"].ToString() == "Y")
                                    {
                                        double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString()) +
                                            " and itwh_WarehouseID = " + db.sqlStr(WH)));

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
                                if (qty > db.cNum(rowItem["itwh_Qty"].ToString()))
                                {
                                    strErr = strErr + " <h4>- " + rowItem["item_Name"] + "(" +
                                        db.cNum(rVals["init_Qty".ToLower() + st]) + " / " +
                                        db.cNum(rowItem["itwh_Qty"].ToString()) + ")<h4>" + "<hr class='thin bg-grayLighter'/>";
                                }
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

            db.beginTran();
            for (int st = 0; st < n; st++)
            {
                if (rVals["txtDel".ToLower() + st] == "")
                {
                    DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                        " Where itwh_ItemID = " + db.sqlStr(rVals["init_ItemID".ToLower() + st]) +
                        " and itwh_WarehouseID = " + db.sqlStr(WH));
                    foreach (DataRow rowItem in tblItem.Rows)
                    {
                        if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                        {
                            DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                rVals["init_ItemID".ToLower() + st]);
                            if (tblItemSet.Rows.Count <= 0)
                            {
                                tblItemSet.Rows.Add();
                            }
                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                            {

                                qty = db.cNum(rVals["init_Qty".ToLower() + st]);
                                string itemid = rVals["init_ItemID".ToLower() + st];
                                if (rowItem["item_isSet"].ToString() == "Y")
                                {
                                    qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                    itemid = rowItemSet["sitm_ItemUsedID"].ToString();

                                }

                                string tmp = db.execData("Update tblItemWarehouse Set " +
                                        " itwh_Qty = isNULL(itwh_Qty,0) - " + qty +
                                        " where itwh_WarehouseID = " + db.sqlStr(WH) +
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
                }
            }

            // End of Stock Deduction

            vals.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
            vals.Add("invo_CustomerID".ToLower(), rVals["invo_CustomerID".ToLower()]);
            vals.Add("invo_WarehouseID".ToLower(), WH);
            vals.Add("invo_Status".ToLower(), "Completed");
            vals.Add("invo_Discount".ToLower(), rVals["invo_Discount".ToLower()]);
            vals.Add("invo_Disc".ToLower(), rVals["invo_Disc".ToLower()]);
            vals.Add("invo_isTax".ToLower(), tax);
            
            aVals.Add("invo_WorkflowID", "6");
            aVals.Add("invo_WorkflowItemID", "13");
            aVals.Add("invo_ExRate", Rate.ToString());
            aVals.Add("invo_CashIn", rVals["invo_cashin".ToLower()]);
            aVals.Add("invo_CashIn2", rVals["invo_cashin2".ToLower()]);
            //vals.Add("invo_invoiceid".ToLower(), "");
            
            string re2 = "";

            re = cls.saveRecord("tblInvoiceNew", vals, db, aVals: aVals);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            string hid = "0";
            if (str.tbl != null)
            {   
                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;

                    for (int st = 0; st < n; st++)
                    {
                        if (rVals["txtDel".ToLower() + st] == "")
                        {
                            vals.Clear();
                            aVals.Clear();

                            aVals.Add("init_InvoiceID", hid);

                            vals.Add("init_ItemID".ToLower(), rVals["init_ItemID".ToLower() + st]);
                            vals.Add("init_Description".ToLower(), rVals["init_Description".ToLower() + st]);
                            vals.Add("init_Qty".ToLower(), rVals["init_Qty".ToLower() + st]);
                            vals.Add("init_Price".ToLower(), rVals["init_Price".ToLower() + st]);
                            vals.Add("init_Total".ToLower(), "0");

                            aVals.Add("init_RPrice", rVals["init_Price".ToLower() + st]);
                            aVals.Add("init_RQty", "0");
                            aVals.Add("init_BQty", rVals["init_Qty".ToLower() + st]);

                            double quit_Qty = 0;
                            double quit_Price = 0;
                            if (vals.ContainsKey("init_Qty".ToLower()))
                            {
                                quit_Qty = db.cNum(vals["init_Qty".ToLower()]);
                            }
                            if (vals.ContainsKey("init_Price".ToLower()))
                            {
                                quit_Price = db.cNum(vals["init_Price".ToLower()]);
                            }

                            if (vals.ContainsKey("init_Total".ToLower()))
                            {
                                vals["init_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                            }
                            re2 = cls.saveRecord("tblInvoiceItemNew", vals, db, aVals, st.ToString());
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
                                    if (db.readData("item_isSet", "Select item_isSet from tblItem Where item_ItemID = " + rVals["init_ItemID".ToLower() + st]) == "Y")
                                    {
                                        //if (rVals.ContainsKey("init_InvoiceItemID".ToLower() + st))
                                            //if (string.IsNullOrEmpty(rVals["init_InvoiceItemID".ToLower() + st]))
                                        DataTable tblItemSet = db.readData("Select sitm_Qty inid_Qty,sitm_ItemUsedID inid_ItemID from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                    rVals["init_ItemID".ToLower() + st]);
                                        foreach (DataRow rowItemSet in tblItemSet.Rows)
                                            {
                                               
                                                db.execData("Insert into tblInvoiceItemDetail(inid_InvoiceItemID,inid_ItemID,inid_Qty) VALUES(" +
                                                    db.sqlStr((string)str.tbl[0].msg) + "," + db.sqlStr(rowItemSet["inid_ItemID"].ToString()) + "," +
                                                    //(db.cNum(rVals["init_Qty".ToLower() + st])) + 
                                                    qty + 
                                                    ")");
                                            }
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

                    invoiceTotal(hid, db);

                    vals.Clear();
                    aVals.Clear();

                    double invo_Total = db.cNum(db.readData("invo_GTotal",
                        "Select invo_GTotal From tblInvoice Where invo_Deleted is null and invo_InvoiceID=" + hid));

                    vals.Add("ivpm_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
                    vals.Add("ivpm_InvoiceID".ToLower(), hid);
                    vals.Add("ivpm_Amount".ToLower(), invo_Total.ToString());



                    re2 = cls.saveRecord("tblInvoicePaymentNew", vals, db, aVals);
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
                            var tmp = db.execData("Update tblInvoice Set " +
                            " invo_PaidAmount = isNull(invo_PaidAmount,0) + " + invo_Total +
                            ",invo_Balance = isnull(invo_GTotal,0) - isnull(invo_PaidAmount,0) - " + invo_Total +
                            ",invo_isPaid = 'Y' " + 
                            " Where invo_InvoiceID = " + hid);
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
            db.execData("Insert into tblNotification([table],[id],[date]) " + 
                "VALUES('invoice'," + hid + ",GETDATE())" );
            db.commit();

            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = "";
            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

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
            DataTable tbl = db.readData("Select * From tblInvoice " +
                " Where invo_Deleted is null and invo_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["invo_Discount"].ToString();
                invo_Disc = db.cNum(row["invo_Disc"].ToString());
                invo_IsTax = db.cNum(row["invo_IsTax"].ToString());
            }

            tbl = db.readData("Select SUM(isNull(init_Total,0)) init_Total From tblInvoiceItem " +
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

                db.execData("Update tblInvoice Set " +
                    " invo_SubTotal = " + invo_SubTotal +
                    ",invo_DiscountAmount = " + invo_DiscountAmount +
                    ",invo_Total = " + invo_Total +
                    ",invo_GTotal = " + invo_GTotal +
                    ",invo_Tax = " + invo_Tax +
                    ",invo_Balance = " + invo_GTotal + " - isNull(invo_PaidAmount,0) " +
                    " Where invo_InvoiceID = " + eid
                    );
            }
        }
    }
}