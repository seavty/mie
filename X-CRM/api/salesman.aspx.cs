using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.api
{
    public partial class salesman : System.Web.UI.Page
    {
        int recPage = 50;
        string getdate = "DATEADD(HOUR,7, GETUTCDATE())";
        string cdb = "";
        string numFormat = "##0.00";

        protected void Page_Load(object sender, EventArgs e)
        {
            Session["userid"] = "1";
            
            /*sapi.db ndb = new sapi.db();
            ndb.connect();
            Response.Write("{" + "\"tbl\" :" + ndb.tblToJson(ndb.readData("exec sales_analyst " +
                        "3" +
                        "," + ndb.sqlStrN("2017-08-01") +
                        "," + ndb.sqlStrN("2017-08-31") +
                        ",1"
                        )) + "}");

            new sapi.sapi().endRequest();
            Response.End();
            */
            
            if (Request.Form["app"] != null)
            {
                if (Request.Form["app"].ToString() == "test")
                {
                    Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"ok\"}]}");
                    new sapi.sapi().endRequest();
                    Response.End();
                }

                if (Request.Form["app"].ToString() == "customerLog")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            sapi.sapi cls = new sapi.sapi();
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            Dictionary<string, string> rVals = new Dictionary<string, string>();
                            Dictionary<string, string> aVals = new Dictionary<string, string>();

                            foreach (var st in Request.Form.AllKeys)
                            {
                                rVals.Add(st.ToLower(), Request.Form[st].ToString());
                            }

                            vals.Add("culg_SalesmanID".ToLower(), rVals["culg_SalesmanID".ToLower()]);
                            vals.Add("culg_CustomerID".ToLower(), rVals["culg_CustomerID".ToLower()]);
                            vals.Add("culg_Type".ToLower(), rVals["culg_Type".ToLower()]);
                            vals.Add("culg_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                            //vals.Add("culg_Status".ToLower(), rVals["culg_Status".ToLower()]);
                            
                            var re = cls.saveRecord("tblCustomerLogNew", vals, db, aVals);

                            var str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"" +
                                    (string)str.tbl[0].msg + "\"}]}");

                            }
                            else
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "login")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            DataTable tbl = db.readData("Select * from tblSalesman " +
                                " Where salm_Deleted is null and salm_Name=" + db.sqlStr(Request.Form["user"].ToString()));
                            if (tbl.Rows.Count > 0)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"" + tbl.Rows[0]["salm_SalesmanID"].ToString() + "\"}]}");
                            }
                            else
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Invalid User Name or Password !\"}]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }
                    new sapi.sapi().endRequest();
                    Response.End();
                }

                if (Request.Form["app"].ToString() == "getRoute")
                {
                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["salm_salesmanid"] != null)
                    {
                        search = " and rtpn_salesmanid = " +
                            db.sqlStr(Request.Form["salm_salesmanid"].ToString()) +
                            " and cast(GETDATE() as date) >= cast(rtpn_Date as date) " +
                            " and cast(GETDATE() as date) <= cast(rtpn_ToDate as date)";
                    }

                    if (Request.Form["sql"] != null)
                    {
                        if (Request.Form["sql"].ToString().Length > 0)
                        search = " and " + Request.Form["sql"].ToString();

                    }

                    Response.Write(getJSONtbl(" tblRoutePlanning where rtpn_Deleted is null ",
                        search, "rtpn_Name", recPage, cdb));
                    
                    new sapi.sapi().endRequest();
                    Response.End();
                }

                if (Request.Form["app"].ToString() == "getSaleStatistic")
                {
                    sapi.db db = new sapi.db();
                    string search = "";
                    Response.Write("{" + "\"tbl\" :" + db.tblToJson(db.readData("exec sales_analyst " + 
                        Request.Form["salm_salesmanid"].ToString() +
                        "," + db.sqlStrN(Request.Form["frdate"].ToString()) +
                        "," + db.sqlStrN(Request.Form["todate"].ToString()) +
                        ",1"
                        )) + "}");


                    new sapi.sapi().endRequest();
                    Response.End();
                }

                if (Request.Form["app"].ToString() == "checkIn")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            sapi.sapi cls = new sapi.sapi();
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            Dictionary<string, string> rVals = new Dictionary<string, string>();
                            Dictionary<string, string> aVals = new Dictionary<string, string>();

                            foreach (var st in Request.Form.AllKeys)
                            {
                                rVals.Add(st.ToLower(), Request.Form[st].ToString());
                            }


                            if (db.readData("Select 1 from  tblRoutePlanningItem " +
                                " where (rpit_CheckIn is not NULL and rpit_CheckOut is null) " +
                                " and rpit_Deleted is null and rpit_RoutePlanningID = " +
                                rVals["rtpn_RoutePlanningID".ToLower()]
                                //+ "and rpit_Type != " + db.sqlStr(rVals["rpit_Type".ToLower()])
                                ).Rows.Count > 0)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" +
                                    "You already Checked In(" + rVals["rpit_Type".ToLower()].ToUpper() + ") !" + "\"}]}");
                                db.close();
                                Response.End();
                                cls.endRequest();
                            }
     
                           
                            //vals.Add("rpit_CheckIn".ToLower(), db.getDate(rVals["rpit_CheckIn".ToLower()], 1));
                            vals.Add("rpit_CheckIn".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                            vals.Add("rpit_Type".ToLower(), rVals["rpit_Type".ToLower()]);
                            aVals.Add("rpit_RoutePlanningID", rVals["rtpn_RoutePlanningID".ToLower()]);
                            
                            var re = cls.saveRecord("tblRoutePlanningItemNew", vals, db, aVals);
                           
                            var str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"" +
                                    (string)str.tbl[0].msg + "\"}]}");

                            }
                            else
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "checkOut")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            sapi.sapi cls = new sapi.sapi();
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            Dictionary<string, string> rVals = new Dictionary<string, string>();
                            Dictionary<string, string> aVals = new Dictionary<string, string>();

                            foreach (var st in Request.Form.AllKeys)
                            {
                                rVals.Add(st.ToLower(), Request.Form[st].ToString());
                            }



                            vals.Add("rpit_CheckOut".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
                            vals.Add("rpit_RoutePlanningItemID".ToLower(), rVals["rpit_RoutePlanningItemID".ToLower()]);

                            var re = cls.saveRecord("tblRoutePlanningItemNew", vals, db, aVals);

                            var str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"" +
                                    (string)str.tbl[0].msg + "\"}]}");
                            }
                            else
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "getActiveRouteItem")
                {

                    sapi.db db = new sapi.db();
                    string search = "";

                    if (Request.Form["rtpn_RoutePlanningID".ToLower()] != null)
                    {
                        search = " and rpit_RoutePlanningID = " +
                            db.sqlStr(Request.Form["rtpn_RoutePlanningID".ToLower()].ToString());
                    }


                    if (Request.Form["sql"] != null)
                    {
                        search = " and " + Request.Form["sql"].ToString();

                    }
                    Response.Write(getJSONtbl(" tblRoutePlanningItem " +
                        " where (rpit_CheckIn is not NULL and rpit_CheckOut is NULL) " +
                        " and rpit_Deleted is null ",
                        search, " rpit_RoutePlanningItemID ", recPage, cdb));
                    Response.End();
                    new sapi.sapi().endRequest();
                }




                if (Request.Form["app"].ToString() == "saveRouteItem")
                {
                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            sapi.sapi cls = new sapi.sapi();
                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            Dictionary<string, string> rVals = new Dictionary<string, string>();
                            Dictionary<string, string> aVals = new Dictionary<string, string>();

                            foreach (var st in Request.Form.AllKeys)
                            {
                                rVals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            if (rVals.ContainsKey("cust_customerid"))
                                if (!string.IsNullOrEmpty(rVals["cust_customerid".ToLower()]))
                                {
                                    vals.Add("cust_customerid".ToLower(), rVals["cust_customerid".ToLower()]);
                                }
                            vals.Add("cust_Type".ToLower(), rVals["cust_Type".ToLower()]);
                            vals.Add("cust_Name".ToLower(), rVals["cust_Name".ToLower()]);
                            vals.Add("cust_Phone".ToLower(), rVals["cust_Phone".ToLower()]);
                            vals.Add("cust_Email".ToLower(), rVals["cust_Email".ToLower()]);
                            vals.Add("cust_Address".ToLower(), rVals["cust_Address".ToLower()]);

                            db.beginTran();
                            var re = cls.saveRecord("tblCustomerNew", vals, db, aVals);

                            var str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                                if (str.tbl[0].status == "ok")
                                {
                                    string id = (string)str.tbl[0].msg;
                                    vals.Clear();
                                    aVals.Clear();

                                    vals.Add("rpit_RoutePlanningItemID".ToLower(), rVals["rpit_RoutePlanningItemID".ToLower()]);
                                    vals.Add("rpit_CustomerID".ToLower(), id);

                                    if(rVals.ContainsKey("rpit_Note".ToLower()))
                                        vals.Add("rpit_Note".ToLower(), rVals["rpit_Note".ToLower()]);

                                    //aVals.Add("rpit_RoutePlanningID", rVals["rtpn_RoutePlanningID".ToLower()]);
                                    re = cls.saveRecord("tblRoutePlanningItemNew", vals, db, aVals);
                                    str = JsonConvert.DeserializeObject<dynamic>(re);
                                    if (str.tbl != null)
                                    {
                                        calcActual(rVals["rtpn_RoutePlanningID".ToLower()], db);
                                        Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"" +
                                            (string)str.tbl[0].msg + "\"}]}");
                                        db.commit();
                                    }
                                    else
                                    {
                                        db.rollback();
                                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                                    }
                                }
                                else
                                {
                                    db.rollback();
                                    Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                                }
                            }
                            else
                            {
                                db.rollback();
                                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + re + "\"}]}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "getCustomerByRoute")
                {
                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["rtpn_routeplanningid"] != null)
                    {
                        search = " and rpit_RoutePlanningID = " +
                            db.sqlStr(Request.Form["rtpn_routeplanningid"].ToString());
                    }

                    if (Request.Form["sql"] != null)
                    {
                        if (Request.Form["sql"].ToString().Length > 0)
                        search = " and " + Request.Form["sql"].ToString();

                    }

                    Response.Write(getJSONtbl(" tblRoutePlanningItem " +
                        " left join tblCustomer on cust_CustomerID = rpit_CustomerID and cust_Deleted is null" +
                        " where rpit_Deleted is null ",
                        search, "rpit_RoutePlanningItemID", recPage, cdb));
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "getCustomer")
                {
                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        if (!string.IsNullOrEmpty(search))
                            search = " and (" +
                                    " lower(cust_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                    " OR lower(cust_Phone) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                    " OR lower(cust_Email) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";
                    }
                    if (Request.Form["sql"] != null)
                    {
                        if(Request.Form["sql"].ToString().Length>0)
                        search = search + " and " + Request.Form["sql"].ToString();

                    }
                    Response.Write(getJSONtbl(" tblCustomer " +
                        " where cust_Deleted is null ",
                        search, "cust_Name", recPage, cdb));
                    Response.End();
                    new sapi.sapi().endRequest();
                }

                if (Request.Form["app"].ToString() == "getData")
                {
                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        
                    }

                    Response.Write(getJSONtbl(" sys_dataItem where dati_DataID in (select data_DataID from sys_data Where data_Name = " + db.sqlStr(search) + ") " +
                        "",
                        "", "dati_Name", recPage, cdb));
                    Response.End();
                    new sapi.sapi().endRequest();
                }



            }
            
        }


        string getJSONtbl(String sql, string searchStr, string ord, int recPage, string cdb)
        {
            string re = "";
            double cPage = 0;
            //recPage = 1000;
            sapi.db db = new sapi.db();
            try
            {

                cPage = db.cNum(Request.Form["cPage"].ToString());
                if (db.connect())
                {
                    string search = searchStr;

                    /*
                    if (Request.Form["search"] != null)
                    {

                        if (!string.IsNullOrEmpty(search))
                            search = searchStr;
                        //" and (lower(item_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                        //" )";
                    }

                    if (Request.Form["sql"] != null)
                    {
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }*/

                    DataTable tbl = db.readData(
                        " Declare @cPage int = " + cPage + "; " +
                        " Declare @RecPage int = " + recPage + "; " +
                        "Select * from " +
                        "( Select Row_number() over (order by " + ord + ") ROWID,* from " +
                        //" vItem where item_Deleted is null " + search + " ) tblItem " +
                        sql + search + " ) tbl" +
                        (cPage == -1 ? "" : " Where ROWID> ((@cPage * @RecPage) - @RecPage) " +
                        " and ROWID<=(@cPage * @RecPage) ") +
                        " order by " + ord
                        );

                    DataTable tblNext = db.readData(
                        " Declare @cPage int = " + (cPage + 1) + "; " +
                        " Declare @RecPage int = " + recPage + "; " +
                        "Select * from " +
                        "( Select Row_number() over (order by " + ord + ") ROWID,* from " +
                        //"vItem where item_Deleted is null " + search + " ) tblItem " +
                        sql + search + " ) tbl" +
                        (cPage == -1 ? "" : " Where ROWID> ((@cPage * @RecPage) - @RecPage) " +
                        " and ROWID<=(@cPage * @RecPage) ") +
                        " order by " + ord
                        );
                    tbl.Columns.Add("status");
                    if (tbl.Rows.Count > 0)
                    {
                        if (tblNext.Rows.Count <= 0)
                        {
                            tbl.Rows[0]["status"] = "oef";
                        }
                    }
                    string tmp = db.tblToJson(tbl);
                    Response.Write("{" + "\"tbl\" :" + tmp +
                        "}");

                }
            }
            catch (Exception ex)
            {
                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
            }
            finally
            {
                db.close();
            }

            return re;
        }


        void calcActual(string hid, sapi.db db)
        {
            DataTable tbl = db.readData("Select sum(case when isnull(rpit_Type,'new') = 'new' then 1 else 0 end) c, " +
                " sum(case when isnull(rpit_Type,'new') = 'revisit' then 1 else 0 end) c2 " +
                " From tblRoutePlanningItem " +
                " Where rpit_Deleted is null and rpit_RoutePlanningID = " + hid);
            foreach (DataRow row in tbl.Rows)
            {
                db.execData("Update tblRoutePlanning set " +
                    " rtpn_Actual = " + db.cNum(row["c"].ToString()) +
                    ",rtpn_RActual = " + db.cNum(row["c2"].ToString()) +
                    " Where rtpn_RoutePlanningID = " + db.cNum(hid));
            }
        }

    }
}