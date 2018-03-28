using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.api
{
    public partial class Default : System.Web.UI.Page
    {
        int recPage = 50;
        string getdate = "DATEADD(HOUR,7, GETUTCDATE())";
        string cdb = "";
        string numFormat = "##0.00";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["app"] != null)
            {
                if (Request.Form["app"].ToString() == "getItemCat")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = " and (lower(itmc_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }

                    if (Request.Form["sql"] != null)
                    {
                        search = " and " + Request.Form["sql"].ToString();

                    }

                    Response.Write(getJSONtbl(" tblItemCat where itmc_Deleted is null ",
                        search, "itmc_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItemGroup")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = " and (lower(itmg_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        search = " and " + Request.Form["sql"].ToString();

                    }
                    Response.Write(getJSONtbl(" tblItemGroup where itmg_Deleted is null ",
                        search, "itmg_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItem")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = " and (lower(item_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        search = " and " + Request.Form["sql"].ToString();

                    }
                    Response.Write(getJSONtbl(" tblItem where item_Deleted is null ",
                        search, "item_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getCustomer")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = " and (lower(cust_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        search = " and " + Request.Form["sql"].ToString();

                    }
                    Response.Write(getJSONtbl(" tblCustomer where Cust_Deleted is null ",
                        search, "cust_Name", recPage, cdb));
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
                    string search = "";
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
                    }
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
    }
}