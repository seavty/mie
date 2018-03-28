using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
namespace X_CRM.saleorder
{
    public partial class createSet : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblSaleOrderItemNew";
        string screenItem = "tblSaleOrderItemDetailNew";
        string screenItemList = "tblSaleOrderItemDetailList";
        string frm = "frmCreateSet";
        string IDFIeld = "soit_SaleOrderItemID".ToLower();
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
                            
                            Response.Write(loadScreen("0", cls.Mode));
                        }
                        if (Request.Form["app"].ToString() == "delRecord")
                        {
                            Response.Write(cls.delRecord(screen, Request.Form["eid"].ToString(), db));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
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
            sapi.readOnlyField.add("soid_SaleOrderItemID");
            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvSODetailList'>" + cls.findRecord(db, screenItem, screenItemList, "frmSODetailList", null, "",
                                mode: 1, filter: " and soid_SaleOrderItemID = " + eid, cPage: -1) +
                       "</div>";


            }

            if (mode == sapi.sapi.recordMode.View)
            {

                list = "<div id='dvSODetailList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and soid_SaleOrderItemID = " + eid, cPage: -1, assc: 0) +
                       "</div>";


            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvSODetailList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and soid_SaleOrderItemID = " + eid, cPage: -1) +
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
            string soid = Request.QueryString["sord_saleorderid"].ToString();
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            db.beginTran();
            aVal.Add("soit_SaleOrderID", db.cNum(soid).ToString());
            double quit_Qty = 0;
            double quit_Price = 0;
            if (vals.ContainsKey("soit_Qty".ToLower()))
            {
                quit_Qty = db.cNum(vals["soit_Qty".ToLower()]);
            }
            if (vals.ContainsKey("soit_Price".ToLower()))
            {
                quit_Price = db.cNum(vals["soit_Price".ToLower()]);
            }

            if (vals.ContainsKey("soit_Total".ToLower()))
            {
                vals["soit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
            }
            aVal.Add("soit_RemainQty", quit_Qty.ToString());

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            if (vals.ContainsKey("soid_SaleOrderItemID".ToLower()))
                            {
                                vals["soid_SaleOrderItemID".ToLower() + st] = hid;
                            }
                            else
                            {
                                aVal.Add("soid_SaleOrderItemID", hid);
                            }

                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["soid_SaleOrderItemDetailID".ToLower() + st], db);
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                if (v.ContainsKey("soid_qty"))
                                    v["soid_qty"] = (quit_Qty * db.cNum(v["soid_qty"])).ToString();

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
                    new clsGlobal().SOTotal(soid, db);
                    db.commit();
                }
            }

            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            tblResult.Rows[0]["status"] = "ok";
            tblResult.Rows[0]["msg"] = soid;
            return ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
        }
    }
}