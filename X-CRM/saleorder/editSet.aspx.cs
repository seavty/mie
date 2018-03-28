using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.saleorder
{
    public partial class editSet : System.Web.UI.Page
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

                        if (Request.Form["app"].ToString() == "getItemset")
                        {
                            string id = (Request.Form["sord_saleorderid"].ToString());
                            string str = "<option value=''>--- Select One ---</option>";
                            DataTable tbl = db.readData("Select * from vSaleOrderItem Where soit_Deleted is null and item_isSet='Y' and soit_SaleOrderID=" + id);
                            foreach (DataRow row in tbl.Rows)
                            {
                                str = str + "<option value='" + row["soit_SaleOrderItemID"].ToString() + "'>" + row["soit_Description"].ToString() + "</option>";
                            }
                            Response.Write(str);
                        }
                        if (Request.Form["app"].ToString() == "getItemsetDetail")
                        {
                            string id = (Request.Form["itemSet"].ToString());
                            string str = "";
                            sapi.sapi cls = new sapi.sapi();
                            sapi.readOnlyField.add("soid_ItemID");
                            sapi.readOnlyField.add("soid_SaleOrderItemID");


                            str = cls.findRecord(db, "tblSaleOrderItemDetailNew", "tblSaleOrderItemDetailList", "frmList2", null, "",
                                mode: 1, filter: " and soid_SaleOrderItemID = " + id, cPage: -1);
                            Response.Write(str);
                        }

                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            DataTable tblResult = new DataTable();
                            tblResult.Rows.Add();
                            tblResult.Columns.Add("status");
                            tblResult.Columns.Add("msg");

                            string screenItem = "tblSaleOrderItemDetailNew";
                            db.beginTran();
                            
                            foreach (var st in Request.Form.GetValues("N"))
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                double exQty = 0;
                                if (v.ContainsKey("soid_saleorderitemid"))
                                {
                                    string wh = db.readData("sord_WarehouseID", "Select sord_WarehouseID From vSaleOrderItem " +
                                        " Where soit_SaleOrderItemID = " + v["soid_saleorderitemid"]);

                                    string id = v["soid_saleorderitemid"];
                                    exQty = db.cNum(db.readData("soid_Qty",
                                        "Select soid_Qty From tblSaleOrderItemDetail Where soid_SaleOrderItemDetailID=" + v["soid_saleorderitemdetailid"]));


                                    db.execData("Update tblSaleOrderItemDetail Set soid_Qty = " + v["soid_Qty".ToLower()] +
                                        " Where soid_SaleOrderItemDetailID = " + v["soid_saleorderitemdetailid".ToLower()]);

                                    DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                        " Where itwh_ItemID = " + db.sqlStr(v["soid_ItemID".ToLower()]) +
                                        " and itwh_WarehouseID = " + db.sqlStr(wh));
                                    foreach (DataRow rowItem in tblItem.Rows)
                                    {
                                        if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                        {
                                            DataTable tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                v["soid_ItemID".ToLower()]);
                                            if (tblItemSet.Rows.Count <= 0)
                                            {
                                                tblItemSet.Rows.Add();
                                            }
                                            else
                                            {
                                                if (v.ContainsKey("soid_SaleOrderItemID".ToLower()))
                                                {
                                                    if (!string.IsNullOrEmpty(v["soid_SaleOrderItemID".ToLower()]))
                                                    {
                                                        tblItemSet = db.readData("Select soid_ItemID sitm_ItemUsedID,soid_Qty sitm_Qty from tblSaleOrderItemDetail Where soid_Deleted is null and soid_SaleOrderItemID=" +
                                                        v["soid_SaleOrderItemID".ToLower()]);
                                                    }
                                                }
                                            }

                                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                                            {

                                                double qty = db.cNum(v["soid_Qty".ToLower()]) - exQty;
                                                string itemid = v["soid_ItemID".ToLower()];

                                                if (rowItem["item_isSet"].ToString() == "Y")
                                                {
                                                    qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                    itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                                                }

                                                if (rowItem["item_isSet"].ToString() == "Y")
                                                {
                                                    if (v.ContainsKey("soit_SaleOrderItemID".ToLower()))
                                                        if (string.IsNullOrEmpty(v["soit_saleorderitemid".ToLower()]))
                                                        {
                                                            db.execData("Insert into tblSaleOrderItemDetail(soid_SaleOrderItemID,soid_ItemID,soid_Qty) VALUES(" +
                                                                db.sqlStr(id) + "," + db.sqlStr(itemid) + "," +
                                                                (db.cNum(v["soid_Qty".ToLower()]) - exQty) +
                                                                ")");
                                                        }
                                                }

                                                string tmp = db.execData("Update tblItemWarehouse Set " +
                                                        " itwh_SOAllocation = isNULL(itwh_SOAllocation,0) + " + qty +
                                                        " where itwh_WarehouseID = " + db.sqlStr(wh) +
                                                        " and itwh_ItemID = " + db.sqlStr(itemid)
                                                        );
                                                if (tmp != "ok")
                                                {
                                                    db.rollback();

                                                    tblResult.Rows[0]["status"] = "error";
                                                    tblResult.Rows[0]["msg"] = tmp;
                                                    Response.Write("{\"tbl\":" + db.tblToJson(tblResult) + "}");

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
                                                        Response.Write("{\"tbl\":" + db.tblToJson(tblResult) + "}");

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            db.commit();
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