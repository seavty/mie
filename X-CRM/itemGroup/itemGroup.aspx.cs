using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.itemGroup
{
    public partial class itemGroup : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemGroupNew";
        string screenItem = "tblItemNew";
        string screenItemList = "tblItemList";
        string frm = "frmMaster";
        string IDFIeld = "itmg_itemgroupid";
        string Tab = "";
        string cTab = "";
        //string Tab = "tblItemGroup";
        //string cTab = "tblItemGroup";

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

                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = true;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = global::sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;

                        }
                        else
                        {

                        }

                        sapi.defaultValue.add("cont_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
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
            string list = "";
            int caughtMode = 0;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                         mode: 1, filter: " AND item_ItemGroupID = " + eid, cPage: -1) +
                       "</div>";
            }
            if(mode == sapi.sapi.recordMode.View)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                         mode: 0, filter: " AND item_ItemGroupID = " + eid, cPage: -1) +
                       "</div>";
                caughtMode = 2;

                int QuotUsed = int.Parse(db.readData("C", "SELECT COUNT(*) AS [C] FROM tblInvoice WHERE invo_ServiceType=" + db.sqlStr(url.Get("itmg_itemgroupid"))));
                int InvoUsed = int.Parse(db.readData("C", "SELECT COUNT(*) AS [C] FROM tblQuotation WHERE quot_Type2=" + db.sqlStr(url.Get("itmg_itemgroupid"))));
                if (QuotUsed > 0 || InvoUsed > 0)
                    cls.hideDelete = true;
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                         mode: 1, filter: " AND item_ItemGroupID = " + eid, cPage: -1) +
                       "</div>";
            }

            string re = "";

            string res = "<input id='CaughtMode' type='hidden' value=" + caughtMode + ">";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid,null,"Service Type");
            re = re + list + res;
            //var tmp = sapi.loadTab(db, "sys_table", "sys_table", 0, eid);
            //Response.Write("<script></script>");

            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);

            if(str.tbl != null)
            {
                string hid = (string)str.tbl[0].msg;

                if (Request.Form.GetValues("N") != null)
                {
                    aVal.Clear();
                    aVal.Add("item_ItemGroupID".ToLower(),hid);
                    string re2="";
                    var n = Request.Form.GetValues("N");
                    foreach (var st in Request.Form.GetValues("N"))
                    {
                        if (vals["txtdel" + st] != "")
                        {
                            cls.delRecord(screenItem, vals["item_ItemID".ToLower() + st], db);
                        }
                        else
                        {
                            Dictionary<string, string> valItem = cls.getItemVals(screenItem, vals, db, st);

                            if (valItem.ContainsKey("item_code"))
                                if (!string.IsNullOrEmpty(valItem["item_code"]))
                                    valItem["item_code"] = valItem["item_code"].Replace(",","");
                            if (valItem.ContainsKey("item_price"))
                                if (!string.IsNullOrEmpty(valItem["item_price"]))
                                    valItem["item_price"] = valItem["item_price"].Replace(",","");
                            re2 = cls.saveRecord(screenItem, valItem, db, aVal, st);
                        }
                    }
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
                db.commit();
            }
            return re;
        }
    }
}