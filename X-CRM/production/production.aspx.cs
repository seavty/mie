using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.production
{
    public partial class productionItem : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblProductionNew";
        string screenItem = "tblProductionItemNew";
        string screenItemList = "tblProductionItemList";
        string frm = "frmMaster";
        string IDFIeld = "prdt_productionid";
        string Tab = "tblProduction";
        string cTab = "tblProduction";

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

                        if (Request.Form["app"].ToString() == "start")
                        {
                            Response.Write(startProduction(db));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "complete")
                        {
                            Response.Write(completeProduction(db, Request.Form["id"].ToString()));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "pcomplete")
                        {
                            Response.Write(completeProduction(db, Request.Form["id"].ToString(),true));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }


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
                        sapi.defaultValue.add("prdt_Status", "New");
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
            if (!cls.getLic("PR1"))
            {
                Response.Write(cls.getString("accessdeny",db));
                Response.End();
            }
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
                                mode: 1, filter: " and ptit_ProductionID = " + eid, cPage: -1) +
                       "</div>";


            }

            if (mode == sapi.sapi.recordMode.View)
            {

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and ptit_ProductionID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                DataTable tbl = db.readData("Select * from tblProduction Where prdt_ProductionID = " + eid);
                foreach (DataRow row in tbl.Rows)
                {
                    if (row["prdt_Status"].ToString() == "New")
                    {
                        sapi.Buttons.add("Start", "bicycle", "primary", "startProduction(" + eid + ")");
                    }
                    else
                    {
                        cls.hideDelete = true;
                    }
                    if (row["prdt_Status"].ToString() == "Started")
                    {
                        sapi.Buttons.add("Complete", "checkmark", "success", "completeProduction(" + eid + ")");
                        //sapi.Buttons.add("Partial Complete", "checkmark", "success", "pCompleteProduction(" + eid + ")");
                    }
                    if (row["prdt_Status"].ToString() == "Completed")
                    {
                        cls.hideEdit = true;
                    }
                }
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and ptit_ProductionID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                topTitle = "Production";
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

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    if (!vals.ContainsKey("item_itemid"))
                    {
                        string id = (string)str.tbl[0].msg;


                    }

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("ptit_ProductionID".ToLower(), hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["ptit_ProductionItemID".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
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
                    db.commit();
                }
            }
            return re;
        }

        string completeProduction(sapi.db db, string id,bool isPart = false)
        {
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            db.beginTran();
            string re = "";

            if(!isPart)
            re = (db.execData("Update tblProduction Set prdt_Status='Completed' Where prdt_ProductionID = " + id));

            Dictionary<string, string> aVal = new Dictionary<string, string>();
            Dictionary<string, string> v = new Dictionary<string, string>();

            DataTable tblProduction = db.readData("Select * from tblProduction Where prdt_Deleted is null and prdt_ProductionID = " + id);
            foreach (DataRow rowProduction in tblProduction.Rows)
            {
                v.Add("rece_WarehouseID".ToLower(), rowProduction["prdt_RWarehouseID"].ToString());
                v.Add("rece_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"),1));
                v.Add("rece_ReceivedBy".ToLower(), Session["userid"].ToString());
                v.Add("rece_ProductionID".ToLower(), id);
                //v.Add("".ToLower(), rowProduction[""].ToString());

                re = cls.saveRecord("tblReceiveNew", v, db, aVals: aVal);
                string hid = "";
                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("reit_receiveid", hid);
                        
                        
                        DataTable tblProInp = db.readData("Select * from tblProductionOutput Where ptop_Deleted is null and ptop_ProductionID = " + id);
                        foreach (DataRow rowProInp in tblProInp.Rows)
                        {
                            {
                                v.Clear();
                                v.Add("reit_ItemID".ToLower(), rowProInp["ptop_ItemID"].ToString());
                                v.Add("reit_Qty".ToLower(), rowProInp["ptop_Qty"].ToString());
                                v.Add("reit_Price".ToLower(), rowProInp["ptop_Cost"].ToString());
                                v.Add("reit_Total".ToLower(), "0");

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
                                string re2 = cls.saveRecord("tblReceiveItemNew", v, db, aVal);
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


                                        var tmp = db.execData("Update tblItemWarehouse Set " +
                                            " itwh_Qty = isNULL(itwh_Qty,0) + " + db.cNum(v["reit_qty"].ToString()) +
                                            " where itwh_WarehouseID = " + db.sqlStr(rowProduction["prdt_RWarehouseID"].ToString()) +
                                            " and itwh_ItemID = " + db.sqlStr(v["reit_itemid"].ToString())
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
                                                " Select @tot = SUM(itwh_Qty) " +
                                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(v["reit_itemid"].ToString()) +
                                                " update tblItem set item_Qty = @tot " +
                                                " where item_ItemID = " + db.sqlStr(v["reit_itemid"].ToString())
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
                                                tmp = db.execData("update tblProductionOutput Set " +
                                                    " ptop_CQty = isNULL(ptop_CQty,0) + " + db.cNum(v["reit_qty"].ToString()) +
                                                    ",ptop_Variant = isNULL(ptop_EQty,0) - isNULL(ptop_CQty,0) - " + db.cNum(v["reit_qty"].ToString()) +
                                                    " Where ptop_ProductionOutputID = " + rowProInp["ptop_ProductionOutputID"].ToString());
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

                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }

                        if (!isPart)
                        {
                            tblProInp = db.readData("Select * from tblProductionInput Where ptip_Deleted is null " +
                                " and isnull(ptip_RQty,0) >0 and ptip_ProductionID = " + id);
                            foreach (DataRow rowProInp in tblProInp.Rows)
                            {
                                var tmp = db.execData("Update tblItemWarehouse Set " +
                                            " itwh_Qty = isNULL(itwh_Qty,0) + " + db.cNum(rowProInp["ptip_RQty"].ToString()) +
                                            " where itwh_WarehouseID = " + db.sqlStr(rowProduction["prdt_WarehouseID"].ToString()) +
                                            " and itwh_ItemID = " + db.sqlStr(rowProInp["ptip_ItemID"].ToString())
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
                                        " Select @tot = SUM(itwh_Qty) " +
                                        " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(rowProInp["ptip_ItemID"].ToString()) +
                                        " update tblItem set item_Qty = @tot " +
                                        " where item_ItemID = " + db.sqlStr(rowProInp["ptip_ItemID"].ToString())
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
                    new clsGlobal().ReceiveTotal(hid, db);
                    //calTotal(hid, db);
                }
            }
            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = "";
            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
            db.commit();
            return re;
        }
        string startProduction(sapi.db db)
        {

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            db.beginTran();
            var re = (db.execData("Update tblProduction Set prdt_Status='Started' Where prdt_ProductionID = " + Request.Form["id"].ToString()));
            DataTable tblProductionInp = db.readData("Select * from tblProductionInput inner join tblProduction On prdt_ProductionID = ptip_ProductionID Where ptip_Deleted is null and ptip_ProductionID = " + Request.Form["id"].ToString());
            foreach (DataRow row in tblProductionInp.Rows)
            {
                //foreach (var st in Request.Form.GetValues("N"))
                {

                    string strErr = "";
                    DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                        " Where itwh_ItemID = " + db.sqlStr(row["ptip_ItemID"].ToString()) +
                        " and itwh_WarehouseID = " + db.sqlStr(row["prdt_WarehouseID"].ToString())); // wh
                    foreach (DataRow rowItem in tblItem.Rows)
                    {

                        double qty = db.cNum(db.readData("ptip_Qty", "Select SUM(ptip_Qty) ptip_Qty from tblProductionInput Where ptip_Deleted is null and ptip_ItemID = " + db.cNum(row["ptip_ItemID"].ToString())));

                        if (rowItem["item_isSet"].ToString() == "Y")
                        {
                            DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                " Where sitm_Deleted is null and sitm_ItemID=" + row["ptip_ItemID"].ToString());
                            string prestrErr = "";
                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                            {
                                double initQty = 0;
                                if (rowItemSet["item_IsStock"].ToString() == "Y")
                                {
                                    double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                        " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                        " and itwh_WarehouseID = " + db.sqlStr(vals["prdt_WarehouseID".ToLower()]))); // wh

                                    initQty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                    if (initQty > itemwhQty2)
                                    {
                                        prestrErr = prestrErr +
                                            "&nbsp;&nbsp;+ " + rowItemSet["item_Name"] + "(" +
                                            initQty + " / " +
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

                    // stock Deduction

                    foreach (DataRow rowItem in tblItem.Rows)
                    {
                        if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                        {
                            DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                " Where sitm_Deleted is null and sitm_ItemID=" + row["ptip_ItemID"].ToString());
                            if (tblItemSet.Rows.Count <= 0)
                                tblItemSet.Rows.Add();
                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                            {

                                double qty = db.cNum(row["ptip_Qty"].ToString());
                                string itemid = row["ptip_ItemID"].ToString();
                                if (rowItem["item_isSet"].ToString() == "Y")
                                {
                                    qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                    itemid = rowItemSet["sitm_ItemUsedID"].ToString();

                                }

                                string tmp = db.execData("Update tblItemWarehouse Set " +
                                        " itwh_Qty = isNULL(itwh_Qty,0) - " + qty +
                                        " where itwh_WarehouseID = " + db.sqlStr(row["prdt_WarehouseID"].ToString()) +
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
                                        " @tot2 = SUM(isNull(itwh_Qty,0)) " +
                                        " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                        " update tblItem set  " +
                                        " item_Qty = @tot2 " +
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

                }
            }
            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = "";
            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
            db.commit();
            return re;
        }
    }
}