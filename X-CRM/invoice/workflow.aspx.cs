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
    public partial class workflow : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;
        Dictionary<string, string> vals = new Dictionary<string, string>();
        string screen = "tblInvoiceWF";
        string frm = "frmMaster";
        string IDFIeld = "eid";
        string Tab = "";
        string cTab = "";
        string tabl_Name = "tblInvoice";
        string tabl_Prefix = "invo";
        string tabl_ColumnID = "invo_InvoiceID";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            if (string.IsNullOrEmpty(url.Get("wfdtid")) ||
                string.IsNullOrEmpty(url.Get("eid")))
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
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = false;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = global::sapi.sapi.recordMode.Edit;
                            eid = url.Get(IDFIeld);
                        }

                        // preset the workflow things
                        setVal(url.Get("wfdtid"));

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
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);
            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                DataTable tblResult = new DataTable();
                tblResult.Rows.Add();
                tblResult.Columns.Add("status");
                tblResult.Columns.Add("msg");

                if (str.tbl[0].status == "ok")
                {
                    if (!string.IsNullOrEmpty(url.Get("wfdtid")) && !string.IsNullOrEmpty(url.Get("eid")))
                    {
                        DataTable tbl = db.readData("Select * from vSys_Workflow Where wkid_WorkflowItemDetailID = " + url.Get("wfdtid"));
                        foreach (DataRow row in tbl.Rows)
                        {
                            var tmp = db.execData("Update " + tabl_Name + " Set " +
                                tabl_Prefix + "_WorkflowItemID=" + db.cNum(row["wkid_TargetWorkflowItemID"].ToString()) +
                                " Where " + tabl_ColumnID + "=" + db.cNum(url.Get("eid")) +
                                " and " + tabl_Prefix + "_WorkflowItemID = " + db.cNum(row["wfit_WorkflowItemID"].ToString()));
                            if (tmp == "ok")
                            {
                                // do the workflow things
                                if (url.Get("wfdtid") == "9")
                                {
                                    // Stock Deduction
                                    string strErr = "";
                                    DataTable tblInvItem = db.readData("Select * from vInvoiceDetail Where invo_InvoiceID=" + url.Get("eid"));
                                    foreach (DataRow rowInvItem in tblInvItem.Rows)
                                    {

                                        DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(rowInvItem["init_itemid"].ToString()) +
                                            " and itwh_WarehouseID = " + db.sqlStr(rowInvItem["invo_warehouseid"].ToString()));
                                        foreach (DataRow rowItem in tblItem.Rows)
                                        {
                                            double qty = db.cNum(db.readData("c",
                                                        "Select SUM(isnull(init_Qty,0)) c from tblInvoiceItem " +
                                                        " Where init_Deleted is null and init_itemid=" + db.sqlStr(rowInvItem["init_itemid"].ToString()) +
                                                        " and init_InvoiceID=" + db.sqlStr(rowInvItem["invo_InvoiceID"].ToString())));

                                            if (rowItem["item_isSet"].ToString() == "Y")
                                            {
                                                DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                                    " Where sitm_Deleted is null and sitm_ItemID=" + rowInvItem["init_itemid"].ToString());
                                                string prestrErr = "";
                                                foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                {
                                                    if (rowItemSet["item_IsStock"].ToString() == "Y")
                                                    {
                                                        double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select (itwh_Qty - itwh_SOAllocation) as itwh_Qty from vItemWarehouse " +
                                                            " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                                            " and itwh_WarehouseID = " + db.sqlStr(rowInvItem["invo_warehouseid"].ToString())));

                                                        qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                        //if (qty > db.cNum(rowItem["itwh_Qty"].ToString()))
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
                                                    if (qty > db.cNum(rowItem["itwh_Qty"].ToString()) - db.cNum(rowItem["itwh_SOAllocation"].ToString()))
                                                    {
                                                        strErr = strErr + " <h4>- " + rowItem["item_Name"] + "(" +
                                                            db.cNum(rowInvItem["init_Qty"].ToString()) + " / " +
                                                            (db.cNum(rowItem["itwh_Qty"].ToString()) - db.cNum(rowItem["itwh_SOAllocation"].ToString()))
                                                            + ")</h3>" + "<hr class='thin bg-grayLighter'/>";
                                                    }
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

                                    foreach (DataRow rowInvItem in tblInvItem.Rows)
                                    {
                                        DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(rowInvItem["init_itemid"].ToString()) +
                                            " and itwh_WarehouseID = " + db.sqlStr(rowInvItem["invo_warehouseid"].ToString()));
                                        foreach (DataRow rowItem in tblItem.Rows)
                                        {
                                            if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                            {
                                                DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" + rowInvItem["init_itemid"].ToString());
                                                if (tblItemSet.Rows.Count <= 0)
                                                {
                                                    tblItemSet.Rows.Add();
                                                }
                                                foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                {

                                                    double qty = db.cNum(rowInvItem["init_qty"].ToString());
                                                    string itemid = rowInvItem["init_itemid"].ToString();
                                                    if (rowItem["item_isSet"].ToString() == "Y")
                                                    {
                                                        qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                        itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                                                    }

                                                    tmp = db.execData("Update tblItemWarehouse Set " +
                                                            " itwh_Qty = isNULL(itwh_Qty,0) - " + qty +
                                                            " where itwh_WarehouseID = " + db.sqlStr(rowInvItem["invo_warehouseid"].ToString()) +
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

                                    // End of Stock Deduction
                                }
                            }
                            else
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }

                    }
                    else
                    {
                        db.rollback();
                        tblResult.Rows[0]["status"] = "error";
                        tblResult.Rows[0]["msg"] = "Error processing workflow";
                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                        return re;
                    }
                }
            }
            db.commit();
            return re;
        }

        void setVal(string val)
        {

            if (val == "9")
            {
                sapi.setValue.add("invo_Status", "Completed");
            }

        }
    }
}