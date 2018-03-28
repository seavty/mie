using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.shared
{
    public partial class shared : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "refresh")
                        {
                            Session["user"] = Session["user"];
                        }

                        if (Request.Form["app"].ToString() == "getInvoice")
                        {
                            Response.Write(db.tblToJson( db.readData("Select * from vInvoice " +
                                " LEFT JOIN tblWarehouse on ware_WarehouseID = invo_WarehouseID and ware_Deleted is null " +
                                " Where invo_Deleted is null and invo_InvoiceID = " + Request.Form["eid"].ToString())));
                        }


                        if (Request.Form["app"].ToString() == "getItem")
                        {
                            if (Request.Form["cust_customerid"] != null &&
                                db.readData("sett_CustomerlastPrice", "Select sett_CustomerlastPrice from sys_setting ") == "Y")
                            {
                                bool isOK = true;
                                DataTable tbl = null;
                                        
                                if (db.cNum(Request.Form["cust_customerid"].ToString()) > 0)
                                {
                                    tbl = db.readData(" Select item_ItemID,item_Code,item_Name,case  cusp_Price when null then item_Price else cusp_Price end item_Price from tblItem " +
                                        " left join tblCustomerPrice on cusp_ItemID = item_ItemID and cusp_Deleted is null and cusp_CustomerID = " + Request.Form["cust_customerid"].ToString() +
                                        " Where item_Deleted is null and item_ItemID=" + Request.Form["eid"].ToString());
                                    if (tbl.Rows.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(tbl.Rows[0]["item_Price"].ToString()))
                                        {
                                            isOK = false;
                                        }
                                    }

                                }
                                else
                                {
                                    isOK = false;
                                }
                                if (!isOK)
                                {
                                    //Response.Write(db.tblToJson(db.readData("Select * from tblItem Where item_Deleted is null and item_ItemID=" + Request.Form["eid"].ToString())));
                                    Response.Write(db.tblToJson(db.readData("Select * from tblItem Where 1=2 and item_Deleted is null and item_ItemID=" + Request.Form["eid"].ToString())));
                                }
                                else
                                {
                                    Response.Write(db.tblToJson(tbl));
                                }

                            }
                            else
                            {
                                if (Request.Form["prls_pricelistid"] == null || Request.Form["prls_pricelistid"].ToString() == "")
                                    Response.Write(db.tblToJson(db.readData("Select * from tblItem Where item_Deleted is null and item_ItemID=" + Request.Form["eid"].ToString())));
                                else
                                {
                                    Response.Write(db.tblToJson(db.readData(" select item_ItemID,item_Name, " +
                                        " isnull(plit_Price,item_Price) item_Price,item_Unit from tblItem " +
                                        " left join tblPriceListItem on plit_ItemID = item_ItemID and plit_Deleted is null " +
                                        " and plit_PriceListID = " + Request.Form["prls_pricelistid"].ToString() +
                                        //" inner join tblPriceList on prls_PriceListID = plit_PriceListID and prls_Deleted is null " +
                                        " Where item_Deleted is null and item_ItemID=" + Request.Form["eid"].ToString())));
                                }
                            }

                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getCustomer")
                        {
                            DataTable tbl = db.readData("Select * from tblCustomer " +
                                " left join tblPriceList on prls_PriceListID = cust_PriceListID and prls_Deleted is null " +
                                " WHere cust_Deleted is null and cust_CustomerID = " + Request.Form["cust_customerid"].ToString());
                            
                            Response.Write(db.tblToJson(tbl));
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getItemGroupGrid")
                        {
                            DataTable tbl = db.readData("Select * from tblItemGroup");
                            string re = "";
                            foreach (DataRow row in tbl.Rows)
                            {
                                re = re + "<div onclick='getItemGrid(" + row["itmg_ItemGroupID"].ToString() + ")' style='padding:10px;height:50px;border:1px solid #ddd' eid='" + row["itmg_ItemGroupID"].ToString() + "'>" + row["itmg_Name"].ToString() + "</div>";
                            }
                            Response.Write(re);
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getItemGrid")
                        {
                            DataTable tbl = db.readData("Select * from tblItem Where item_Deleted is null and item_ItemGroupID = " + Request.Form["gid"].ToString());
                            string re = "";
                            foreach (DataRow row in tbl.Rows)
                            {
                                re = re + "<div onclick='selectItem(" + row["item_ItemID"].ToString() +
                                    ",\"" + row["item_Name"].ToString() +
                                    "\",\"" + db.cNum(row["item_Price"].ToString()).ToString("#,##0.00") +
                                    "\")' style='padding:10px;height:50px;border:1px solid #ddd' eid='" +
                                    row["item_ItemID"].ToString() + "'>" +
                                        row["item_Name"].ToString() +
                                        "<br/><label>" + db.cNum(row["item_Price"].ToString()).ToString("#,##0.00") + "<label>" +
                                    "</div>";
                            }
                            Response.Write(re);
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally { db.close(); }
        }
    }
}