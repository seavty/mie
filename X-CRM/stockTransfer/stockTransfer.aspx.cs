using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.stockTransfer
{
    public partial class stockTransfer : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblStockTransferNew";
        string screenItem = "tblStockTransferItemNew";
        string screenItemList = "tblStockTransferItemList";
        string frm = "frmMaster";
        string IDFIeld = "sttf_stocktransferid";
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
                            
                            if (!string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
                            {
                                sapi.defaultValue.add("sttf_MobileSaleID", url.Get("mbsl_mobilesaleid"));

                                sapi.defaultValue.add("sttf_ToWarehouse", 
                                    db.readData("mbsl_WarehouseID","Select mbsl_WarehouseID From tblMobileSale " +
                                    " Where mbsl_Deleted is null and mbsl_MobileSaleID = " + url.Get("mbsl_mobilesaleid")));
                                sapi.readOnlyField.add("sttf_ToWarehouse");
                            }
                            sapi.defaultValue.add("sttf_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                            
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
                                mode: 1, filter: " and stit_stockTransferID = " + eid, cPage: -1) +
                       "</div>";
            }



            if (mode == sapi.sapi.recordMode.View)
            {
                cls.hideEdit = true;
                cls.hideDelete = true;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and stit_stockTransferID = " + eid, cPage: -1, assc: 0) +
                       "</div>";
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and stit_stockTransferID = " + eid, cPage: -1) +
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
            double qty = 0;
            string strErr = "";
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            clsGlobal c = new clsGlobal();

            int n = Request.Form.GetValues("N").Length;
            foreach (var st in Request.Form.GetValues("N"))
            {
                if (vals["txtdel" + st] != "")
                {

                }
                else
                {
                    qty = 0;
                    for (int st1 = 0; st1 < n; st1++)
                    {
                        if (vals["txtdel" + st1] == "")
                        {

                            if (vals["stit_ItemID".ToLower() + st] == vals["stit_ItemID".ToLower() + st1])
                            {
                                qty = qty + db.cNum(vals["stit_Qty".ToLower() + st1]);
                            }
                        }
                    }
                    Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                    strErr = strErr + c.stockVerification(db, cls,
                        v["stit_ItemID".ToLower()], qty,
                        vals["sttf_FrWarehouse".ToLower()]);
                }
            }

            if (strErr.Length > 0)
            {

                tblResult.Rows[0]["status"] = "error";
                tblResult.Rows[0]["msg"] = "<h3>Items out of stock : </h3><hr class='thin bg-grayLighter'/><br/>" + strErr;
                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                return re;
            }

            if (vals.ContainsKey("sttf_MobileSaleID".ToLower()))
            {
                re = db.readData("mbsl_CheckIn", "Select mbsl_CheckIn From tblMobileSale " +
                    " Where mbsl_Deleted is null and mbsl_MobileSaleID = " + vals["sttf_MobileSaleID".ToLower()]);
                if (!string.IsNullOrEmpty(re))
                {
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = "<h3>Mobile Sale is already Completed !</h3>";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    return re;
                }
            }

            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("stit_stockTransferID", hid);
                        clsGlobal clsg = new clsGlobal();
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["stit_stockTransferItemID".ToLower() + st], db);
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
                                    else
                                    {
                                        clsg.stockDeduction(db, v["stit_itemid".ToLower()], vals["sttf_FrWarehouse".ToLower()], db.cNum(v["stit_Qty".ToLower()]), add: false);
                                        clsg.stockDeduction(db, v["stit_itemid".ToLower()], vals["sttf_ToWarehouse".ToLower()], db.cNum(v["stit_Qty".ToLower()]), add: true);
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
    }
}