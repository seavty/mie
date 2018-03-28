using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.serviceOrder
{
    public partial class serviceOrder : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblServiceOrderNew";
        string screenItem = "tblServiceOrderItemNew";
        string screenItemList = "tblServiceOrderItemList";
        string frm = "frmMaster";
        string IDFIeld = "seor_serviceorderid";
        string Tab = "tblServiceOrder";
        string cTab = "tblServiceOrder";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            try
            {
                if (db.connect())
                {

                    if (Request.QueryString["app"] != null)
                    {
                        if (Request.QueryString["app"].ToString() == "SSA")
                        {
                            string filter = "";

                            if (url.Get("colName") == "empl_LeaveSchemeID")
                            {
                                if (!string.IsNullOrEmpty(url.Get("empl_DepartmentID")))
                                {
                                    filter = " lvsc_LeaveSchemeID = " + url.Get("empl_DepartmentID");
                                }

                            }

                            Response.Write(cls.loadSSA(db,
                                Request.QueryString["colid"].ToString(),
                                Request.QueryString["q"].ToString(), filter: filter));
                            cls.endRequest();
                            Response.End();
                        }
                    }

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
                        cls.endRequest();
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
                        if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                        {
                            sapi.defaultValue.add("seor_CustomerID", url.Get("cust_customerid"));
                            
                            string sqlRead = "SELECT * FROM tblCustomer WHERE cust_Deleted IS NULL AND cust_CustomerID = " + db.sqlStr(url.Get("cust_customerid"));

                            sapi.defaultValue.add("seor_Phone", db.readData(sqlRead).Rows[0]["cust_Phone"].ToString());
                        }

                       

                        sapi.defaultValue.add("seor_ServiceDate", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));

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
            //cls.Mode = mode;
            //if (eid == "0")
            //{
            //    cls.Mode = global::sapi.sapi.recordMode.New;
            //}
            //string re = "";
            //re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            //return re;
            string re = "";
            string list = "";
            string topTitle = "";
            DataTable tblTop = null;
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and seoi_ServiceOrderID = " + eid, cPage: -1) +
                       "</div>";

            }



            if (mode == sapi.sapi.recordMode.View)
            {
                //sapi.Buttons.add("Convert To Invoice", "opencart", "success",
                //    "loadFormInput(" + eid + ")");

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and seoi_ServiceOrderID = " + eid, cPage: -1, assc: 0) +
                       "</div>";

            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and seoi_ServiceOrderID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                topTitle = "Service Order";
                tblTop = db.readData("SELECT cust_Name, seor_Name FROM vServiceOrder where seor_Deleted IS NULL AND seor_ServiceOrderID =" + eid);
            }

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list;
            return re;
        }

        string saveRecord()
        {
            //string re = "";
            //Dictionary<string, string> aVal = new Dictionary<string, string>();
            //re = cls.saveRecord(screen, vals, db, aVals: aVal);
            //return re;

            //DateValidation();

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

                    if (Request.Form.GetValues("N") != null)
                    {
                        hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("seoi_ServiceOrderID", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["seoi_ServiceOrderItemID".ToLower() + st], db);
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


        private void DateValidation()
        {
            if (vals.ContainsKey("oppo_StartDate".ToLower()) && vals.ContainsKey("oppo_EndDate".ToLower()))
            {
                if (vals["oppo_StartDate".ToLower()] != "" && vals["oppo_EndDate".ToLower()] != "")
                {
                    if (DateTime.Parse(db.getDate(vals["oppo_StartDate".ToLower()], 0)) >
                        DateTime.Parse(db.getDate(vals["oppo_EndDate".ToLower()], 0)))
                    {
                        DataTable tblResult = new DataTable();
                        tblResult.Rows.Add();
                        tblResult.Columns.Add("colName");
                        tblResult.Columns.Add("msg");
                        tblResult.Rows[0]["colName"] = "oppo_StartDate";
                        tblResult.Rows[0]["msg"] = "Start Date cannot greater than End Date";
                        string jsonError = ("{\"error\":" + db.tblToJson(tblResult) + "}");

                        Response.Clear();
                        Response.Write(jsonError);
                        db.close();
                        Response.End();
                        cls.endRequest();
                    }

                }
            }
        }
    }


}