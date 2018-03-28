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
    public partial class productionOutput : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblProductionNew";
        string screenItem = "tblProductionOutputNew";
        string screenItemList = "tblProductionOutputList";
        string frm = "frmMaster";
        string IDFIeld = "prdt_productionid".ToLower();
        string Tab = "tblProduction";
        string cTab = "tblProductionOutput";

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
                                mode: 1, filter: " and ptop_ProductionID = " + eid, cPage: -1) +
                       "</div>";


            }

            if (mode == sapi.sapi.recordMode.View)
            {

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and ptop_ProductionID = " + eid, cPage: -1, assc: 0) +
                       "</div>";

                cls.hideDelete = true;
                DataTable tbl = db.readData("Select * from tblProduction Where prdt_Deleted is null and prdt_ProductionID = " + eid);
                foreach (DataRow row in tbl.Rows)
                {
                    if (
                        row["prdt_Status"].ToString().ToLower() == "completed")
                    {
                        cls.hideEdit = true;
                    }
                }
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and ptop_ProductionID = " + eid, cPage: -1) +
                       "</div>";
                DataTable tmpScreen = db.readData("select * from vSys_screen where scrn_Name = " + db.sqlStr(screen));
                foreach (DataRow row in tmpScreen.Rows)
                {
                    sapi.readOnlyField.add(row["cols_Name"].ToString());
                }

            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                topTitle = "Production Output";
                //tblTop = db.readData("Select prdt_Name,prdt_Date,prdt_EndDate From tblProduction Where prdt_Deleted is null and prdt_ProductionID = " + url.Get("prdt_productionid"));


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


            if (Request.Form.GetValues("N") != null)
            {
                hid = url.Get("prdt_productionid");
                aVal.Clear();
                aVal.Add("ptop_ProductionID".ToLower(), hid);

                foreach (var st in Request.Form.GetValues("N"))
                {
                    if (vals["txtdel" + st] != "")
                    {
                        cls.delRecord(screenItem, vals["ptop_ProductionInputID".ToLower() + st], db);
                    }
                    else
                    {
                        Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                        if(v.ContainsKey("ptop_Variant".ToLower()))
                        {
                            v["ptop_Variant".ToLower()] = (db.cNum(v["ptop_EQty".ToLower()]) - db.cNum(v["ptop_Qty".ToLower()])).ToString();
                        }
                        string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                        var str = JsonConvert.DeserializeObject<dynamic>(re2);
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

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = url.Get("prdt_productionid");
            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");

            return re;
        }
    }
}