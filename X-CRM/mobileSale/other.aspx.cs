using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.mobileSale
{
    public partial class other : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["app"] != null)
            {
                if (Request.Form["app"].ToString() == "delSalesman")
                {
                    sapi.sapi cls = new sapi.sapi();
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Response.Write(db.execData("Update tblMobileSaleItem Set msit_Deleted = 'Y' Where msit_MobileSaleItemID = " + Request.Form["eid"].ToString()));
                        }

                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "loadSalesman")
                {
                    sapi.sapi cls = new sapi.sapi();
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Response.Write(cls.findRecord(db, "tblMobileSaleItemFind", "tblMobileSaleItemList", "frmSalesman", null, "user_UserName", "DESC",
                                filter: " and msit_MobileSaleID = " + Request.Form["mbsl_mobilesaleid"].ToString(), cPage: -1));
                        }

                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "newSalesman")
                {
                    sapi.sapi cls = new sapi.sapi();
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            string eid = "";
                            if (Request.Form["eid"] != null)
                            {
                                if (!string.IsNullOrEmpty(Request.Form["eid"].ToString()))
                                {
                                    cls.Mode = sapi.sapi.recordMode.Edit;
                                    eid = Request.Form["eid"].ToString();
                                    sapi.Buttons.add("Delete", "bin", "danger", "delSalesman(" + eid + ")");
                                }
                            }
                            DataTable tbl = null;

                            Response.Write(cls.loadScreen(db, "tblMobileSaleItemNew", "frmNewSalesman", ref tbl, eid));
                        }
                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "saveSalesman")
                {
                    sapi.sapi cls = new sapi.sapi();
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            Dictionary<string, string> aVals = new Dictionary<string, string>();

                            if (db.cNum( db.readData("c",
                                "Select count(1) c From tblMobileSale " +
                                " inner join tblMobileSaleItem on mbsl_MobileSaleID = msit_MobileSaleID " +
                                " and msit_Deleted is null " +
                                " Where mbsl_Deleted is null and mbsl_CheckIn is null and msit_UserID = " + 
                                vals["msit_UserID".ToLower()] +
                                (!vals.ContainsKey("msit_MobileSaleItemID".ToLower()) ? "" :
                                " and msit_MobileSaleItemID <> " + vals["msit_MobileSaleItemID".ToLower()])
                                ))>0)
                            {

                                db.close();
                                
                                DataTable tblResult = new DataTable();
                                tblResult.Rows.Add();
                                tblResult.Columns.Add("status");
                                tblResult.Columns.Add("msg");
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "Salesman is already assign to other Team";
                                Response.Write("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                               
                                cls.endRequest();
                                Response.End();
                            }
                            
                            aVals.Add("msit_MobileSaleID", vals["txtmbsl_mobilesaleid".ToLower()]);


                            var re = cls.saveRecord("tblMobileSaleItemNew", vals, db, aVals);
                            Response.Write(re);


                        }
                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "clearStock")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            string id = (Request.Form["id"].ToString());
                            DataTable tbl = db.readData("Select * from tblMobileSale where mbsl_Deleted is null and mbsl_MobileSaleID = " + id);
                            sapi.sapi cls = new sapi.sapi();
                            string re = "";
                            db.beginTran();
                            foreach (DataRow row in tbl.Rows)
                            {
                                if (!string.IsNullOrEmpty(row["mbsl_CheckIn"].ToString()))
                                {
                                    Response.Write("This record is already checked in !");
                                    db.close();
                                    cls.endRequest();
                                    Response.End();
                                }

                                string hid = "";
                                DataTable tblItem = db.readData("Select * from tblItemWarehouse " +
                                    " Where  itwh_Qty >0 and itwh_WarehouseID = " + row["mbsl_WarehouseID"].ToString());
                                if (tblItem.Rows.Count > 0)
                                {
                                    Dictionary<string, string> vals = new Dictionary<string, string>();
                                    vals.Add("sttf_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
                                    vals.Add("sttf_UserID".ToLower(), Session["userid"].ToString());
                                    vals.Add("sttf_FrWarehouse".ToLower(), row["mbsl_WarehouseID"].ToString());
                                    vals.Add("sttf_ToWarehouse".ToLower(), row["mbsl_returnWarehouseID"].ToString());
                                    vals.Add("sttf_MobileSaleID".ToLower(), id);
                                    vals.Add("sttf_Remark".ToLower(), "Clear Stock ");
                                    re = cls.saveRecord("tblStockTransferNew", vals, db, aVals: null);

                                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                                    if (str.tbl != null)
                                    {
                                        if (str.tbl[0].status == "ok")
                                        {
                                            hid = (string)str.tbl[0].msg;
                                        }
                                    }
                                }
                                foreach (DataRow rowItem in tblItem.Rows)
                                {
                                    Dictionary<string, string> vals = new Dictionary<string, string>();
                                    Dictionary<string, string> aVal = new Dictionary<string, string>();
                                    vals.Add("stit_ItemID".ToLower(), rowItem["itwh_ItemID"].ToString());
                                    vals.Add("stit_Qty".ToLower(), rowItem["itwh_Qty"].ToString());
                                    aVal.Add("stit_stockTransferID", hid);
                                    re = cls.saveRecord("tblStockTransferItemNew", vals, db, aVals: aVal);

                                    re = db.execData("Update tblItemWarehouse Set " +
                                        " itwh_Qty = 0 " +
                                        " Where itwh_WarehouseID = " + row["mbsl_WarehouseID"].ToString() +
                                        " and itwh_ItemID = " + rowItem["itwh_ItemID"]);
                                    if (re != "ok")
                                    {
                                        Response.Write(re);
                                        db.close();
                                        cls.endRequest();
                                        Response.End();
                                    }
                                    re = db.execData("Update tblItemWarehouse Set " +
                                        " itwh_Qty = isNull(itwh_Qty,0) + " + rowItem["itwh_Qty"].ToString() +
                                        " Where itwh_WarehouseID = " + row["mbsl_returnWarehouseID"].ToString() +
                                        " and itwh_ItemID = " + rowItem["itwh_ItemID"]);
                                    if (re != "ok")
                                    {
                                        Response.Write(re);
                                        db.close();
                                        cls.endRequest();
                                        Response.End();
                                    }

                                }
                            }

                            re = db.execData("Update tblMobileSale Set mbsl_CheckIn = GETDATE() Where mbsl_mobileSaleID = " + id);
                            if (re != "ok")
                            {
                                Response.Write(re);
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }
                            db.commit();
                            Response.Write("ok");
                        }
                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                }
            }
        }
    }
}