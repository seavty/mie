﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
namespace X_CRM.item
{
    public partial class item : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemGroupNew";
        string screenItem = "tblItemNew";
        string screenItemList = "tblItemList";
        string frm = "frmMaster";
        string IDFIeld = "item_itemid";
        string Tab = "tblItem";
        string cTab = "tblItem";

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
                            //showCTab = false;
                        }

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen(eid, cls.Mode);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, Tab, cTab, eid: eid);

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
        {/*
            if (!cls.getLic("I1"))
            {
                Response.Write(cls.getString("accessdeny", db));
                Response.End();
            }*/
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
                                mode: 1, filter: " and item_ItemID = " + eid, cPage: -1) +
                       "</div>";

                //sapi.defaultValue.add("item_ServiceType",url.Get("itmg_itemgroupid"));
            }

            if (mode == sapi.sapi.recordMode.View)
            {

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 0, filter: " and item_ItemID = " + eid, cPage: -1, assc: 0) +
                       "</div>";


            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and item_ItemID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //topTitle = "Invoice";
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
                        DataTable tblItem = db.readData("Select * from tblWarehouse Where ware_Deleted is null");
                        foreach (DataRow rowItem in tblItem.Rows)
                        {
                            var tmp = db.execData("Insert into tblItemWarehouse(itwh_WarehouseID,itwh_ItemID,itwh_Qty) " +
                                "VALUES(" + db.sqlStr(rowItem["ware_WarehouseID"].ToString()) +
                                "," + id + ",0)");
                            if (tmp != "ok")
                            {
                                db.rollback();
                                DataTable tblResult = new DataTable();
                                tblResult.Rows.Add();
                                tblResult.Columns.Add("status");
                                tblResult.Columns.Add("msg");
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }

                    }

                    //if (Request.Form.GetValues("N") != null)
                    //{
                    //    hid = (string)str.tbl[0].msg;
                    //    aVal.Clear();
                    //    //aVal.Add("sitm_ItemID", hid);

                    //    foreach (var st in Request.Form.GetValues("N"))
                    //    {
                    //        if (vals["txtdel" + st] != "")
                    //        {
                    //            cls.delRecord(screenItem, vals["sitm_SubItemID".ToLower() + st], db);
                    //        }
                    //        else
                    //        {
                    //            Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                    //            string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                    //            str = JsonConvert.DeserializeObject<dynamic>(re2);
                    //            if (str.tbl != null)
                    //            {
                    //                if (str.tbl[0].status != "ok")
                    //                {
                    //                    db.rollback();
                    //                    return re2;
                    //                }
                    //            }

                    //            if (str.error != null)
                    //            {
                    //                db.rollback();
                    //                return re2;
                    //            }
                    //        }
                    //    }
                    //}
                    db.commit();
                }
            }
            return re;
        }
    }
}