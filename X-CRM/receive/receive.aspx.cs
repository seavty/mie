using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.receive
{
    public partial class receive : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblReceiveNew";
        string screenItem = "tblReceiveItemNew";
        string screenItemList = "tblReceiveItemList";
        string frm = "frmMaster";
        string IDFIeld = "rece_receiveid";
        string Tab = "";
        string cTab = "";

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
                        else
                        {

                            sapi.defaultValue.add("rece_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
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
                                mode: 1, filter: " and reit_ReceiveID = " + eid, cPage: -1) +
                       "</div>";
            }
            if (mode == sapi.sapi.recordMode.View)
            {
                cls.hideDelete = true;
                cls.hideEdit = true;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and reit_ReceiveID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
                sapi.Buttons.add("Print", "print", "success", "printRec(" + eid + ")");
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and reit_ReceiveID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //topTitle = "Quotation";
                //tblTop = db.readData("Select quot_Name,quot_Date,quot_Total,quot_QuotationID From tblQuotation Where quot_Deleted is null and quot_QuotationID=" + eid);
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
                    if (!vals.ContainsKey("rece_receiveid"))
                    {

                    }

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("reit_receiveid", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["reit_receiveitemid".ToLower() + st], db);
                            }
                            else
                            {
                                string re2 = "";
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                DataTable tblItemSet = null;

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
                                DataTable tblItem = db.readData("Select * from tblItem " +
                                                " Where item_Deleted is null and item_ItemID = " + v["reit_itemid"]);

                                re2 = cls.saveRecord(screenItem, v, db, aVal, st);
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
                                        string tmp = "";
                                        DataTable tblResult = new DataTable();
                                        tblResult.Rows.Add();
                                        tblResult.Columns.Add("status");
                                        tblResult.Columns.Add("msg");
                                        if (string.IsNullOrEmpty(tblItem.Rows[0]["item_isSet"].ToString()))
                                        {
                                            tmp = db.execData("Update tblItemWarehouse Set " +
                                                " itwh_Qty = isNULL(itwh_Qty,0) + " + db.cNum(v["reit_qty"].ToString()) +
                                                " where itwh_WarehouseID = " + db.sqlStr(vals["rece_warehouseid"].ToString()) +
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
                                        }


                                        double cost = 0;
                                        foreach (DataRow rowItem in tblItem.Rows)
                                        {

                                            cost = (db.cNum(rowItem["item_Cost"].ToString()) * db.cNum(rowItem["item_Qty"].ToString()) + (quit_Price * quit_Qty)) /
                                                (db.cNum(rowItem["item_Qty"].ToString()) + quit_Qty);
                                        }

                                        tmp = db.execData("Declare @tot decimal(18,6) " +
                                            " Select @tot = SUM(itwh_Qty) " +
                                            " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(v["reit_itemid"].ToString()) +
                                            " update tblItem set item_Qty = @tot " +
                                            ",item_LastCost = " + quit_Price +
                                            ",item_Cost = " + cost +
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
                                            tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemUsedID = " + v["reit_itemid"]);

                                            foreach (DataRow rowItemSet in tblItemSet.Rows)
                                            {
                                                tmp = db.execData("Declare @tot decimal(18,6) " +
                                                    " Select @tot = SUM(item_Cost) " +
                                                    " from tblItem inner join  tblSubItem on item_ItemID = sitm_ItemUsedID " +
                                                    " where sitm_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemID"].ToString()) +
                                                    " update tblItem set " +
                                                    " item_LastCost = " + quit_Price +
                                                    ",item_Cost = @tot" +
                                                    " where item_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemID"].ToString())
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

                                tblItemSet = null;

                                if (db.readData("item_isSet", "Select item_isSet From tblItem Where item_Deleted is null and item_ItemID = " + v["reit_itemid"]) == "Y")
                                {
                                    tblItemSet = db.readData("Select * from tblSubItem Where sitm_Deleted is null and sitm_ItemID = " + v["reit_itemid"]);
                                }
                                if (tblItemSet != null)
                                {
                                    foreach (DataRow rowItemSet in tblItemSet.Rows)
                                    {
                                        quit_Qty = 0;
                                        quit_Price = 0;
                                        if (v.ContainsKey("reit_Qty".ToLower()))
                                        {
                                            quit_Qty = db.cNum(v["reit_Qty".ToLower()]) * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                        }
                                        if (v.ContainsKey("reit_Price".ToLower()))
                                        {
                                            quit_Price = db.cNum(v["reit_Price".ToLower()]);
                                        }

                                        if (v.ContainsKey("reit_Total".ToLower()))
                                        {
                                            v["reit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                        }

                                        DataTable tblResult = new DataTable();
                                        tblResult.Rows.Add();
                                        tblResult.Columns.Add("status");
                                        tblResult.Columns.Add("msg");

                                        /*string ss = "Update tblItemWarehouse Set " +
                                            " itwh_Qty = isNULL(itwh_Qty,0) + " + quit_Qty +
                                            " where itwh_WarehouseID = " + db.sqlStr(vals["rece_warehouseid"].ToString()) +
                                            " and itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString());*/
                                        var tmp = db.execData("Update tblItemWarehouse Set " +
                                            " itwh_Qty = isNULL(itwh_Qty,0) + " + quit_Qty +
                                            " where itwh_WarehouseID = " + db.sqlStr(vals["rece_warehouseid"].ToString()) +
                                            " and itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString())
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

                                            double cost = 0;

                                            foreach (DataRow rowItem in tblItem.Rows)
                                            {

                                                cost = (db.cNum(rowItem["item_Cost"].ToString()) * db.cNum(rowItem["item_Qty"].ToString()) +
                                                    (quit_Price * quit_Qty / db.cNum(rowItemSet["sitm_Qty"].ToString()))) /
                                                    (db.cNum(rowItem["item_Qty"].ToString()) + (quit_Qty * db.cNum(rowItemSet["sitm_Qty"].ToString())));
                                            }

                                            tmp = db.execData("Declare @tot decimal(18,6) " +
                                                " Select @tot = SUM(itwh_Qty) " +
                                                " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString()) +
                                                " update tblItem set item_Qty = @tot " +
                                                ",item_LastCost = " + quit_Price +
                                                ",item_Cost = " + cost +
                                                " where item_ItemID = " + db.sqlStr(rowItemSet["sitm_ItemUsedID"].ToString())
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


                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }
                    }
                    calTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        void calTotal(string eid, sapi.db db)
        {

            double reit_Total = 0;


            DataTable tbl = db.readData("Select SUM(isNull(reit_Total,0)) reit_Total From tblReceiveItem " +
                " Where reit_Deleted is null and reit_ReceiveID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                reit_Total = db.cNum(row["reit_Total"].ToString());


                db.execData("Update tblReceive Set " +
                    " rece_Total = " + reit_Total +
                    " Where rece_ReceiveID = " + eid
                    );
            }
        }
    }
}