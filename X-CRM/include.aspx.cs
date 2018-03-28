using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM
{
    public partial class include : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["app"] != null)
            {
                sapi.db db = new sapi.db();
                try
                {
                    if (db.connect())
                    {
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            if (Request.Form["colName"].ToString() == "quit_ItemID")
                            {

                                sapi.sapi cls = new sapi.sapi();
                                string filter = "";
                                string ID = Request.Form["quot_Type2"].ToString();
                                filter = " item_ItemGroupID = " + Request.Form["quot_Type2"].ToString();
                                Response.Write(cls.loadSSA(db,
                                Request.Form["colid"].ToString(),
                                Request.Form["q"].ToString(), filter: filter));
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }
                            else
                            {

                                sapi.sapi cls = new sapi.sapi();
                                Response.Write(cls.loadSSA(db,
                                    Request.Form["colid"].ToString(),
                                    Request.Form["q"].ToString()));
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }
                        }
                    }
                }
                catch (Exception ex) { }
                finally { db.close(); }
            }

            if (Request.Form["app"] != null)
            {
                if (Request.Form["app"].ToString() == "setSSA")
                {
                    if (Session["SSA"] != null)
                    {
                        string re = "";
                        List<string> SSA = new List<string>();
                        SSA = (List<string>)HttpContext.Current.Session["SSA"];
                        foreach (var st in SSA)
                        {
                            //setSSA('<%=st%>', '');
                            re = re + st + ",";
                        }
                        if (re.Length > 0)
                        {
                            re = "{\"tbl\":[" + re.Substring(0, re.Length - 1) + "]}";
                        }
                        Response.Write(re);
                    }
                    
                    Response.End();
                }
                sapi.db db = new sapi.db();
                try
                {
                    if (Request.Form["app"].ToString() == "getPreSaveLine")
                    {
                        Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                if (!string.IsNullOrEmpty(st))
                                {
                                    if (Request.Form[st] != null)
                                    {
                                        vals.Add(st.ToLower(), Request.Form[st].ToString());
                                    }
                                }
                            }
                            Response.Write(new clsGlobal().savePreLine(db, Request.Form["screen"].ToString(), vals, Request.Form["rowNum"].ToString()));
                        /*
                        sapi.sapi cls = new sapi.sapi();
                        string screen = Request.Form["screen"].ToString();
                        string tabl_Name = "";
                        DataTable tblScreen = db.readData("Select * from vSys_Screen Where scrn_Name=" + 
                            db.sqlStr(screen));
                        DataTable tblData = null;

                        string col = "";
                        foreach (DataRow rowScreen in tblScreen.Rows)
                        {
                            tabl_Name = rowScreen["tabl_Name"].ToString();
                            col = col + "[" + rowScreen["cols_Name"].ToString() + "],";
                        }
                        if (col.Length > 0)
                        {
                            col = col.Substring(0, col.Length - 1);
                            tblData = db.readData("Select " + col + " From " + tabl_Name + " Where 1=2");
                            tblData.Rows.Add();

                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                if (!string.IsNullOrEmpty(st))
                                {
                                    if (Request.Form[st] != null)
                                    {
                                        vals.Add(st.ToLower(), Request.Form[st].ToString());
                                    }
                                }
                            }
                            foreach (DataColumn c in tblData.Columns)
                            {
                                try
                                {
                                    if (tblScreen.Select("cols_Name = " + db.sqlStrN(c.ColumnName))[0]["cols_Type"].ToString() == "4")
                                    {
                                        tblData.Rows[0][c] = db.getDate(vals[c.ToString().ToLower()]);
                                    }else
                                        if (tblScreen.Select("cols_Name = " + db.sqlStrN(c.ColumnName))[0]["cols_Type"].ToString() == "5")
                                    {
                                        tblData.Rows[0][c] = db.getDate(vals[c.ToString().ToLower()] + " " + vals[c.ToString().ToLower() + "_hh"] + ":" + vals[c.ToString().ToLower() + "_mm"]);
                                    }
                                    else
                                    {
                                        tblData.Rows[0][c] = vals[c.ToString().ToLower()];

                                    }
                                }
                                catch (Exception ex) { }
                            }
                        }
                        cls.Mode = global::sapi.sapi.recordMode.Edit;
                        Response.Write(cls.initCol1(tblScreen, db, tblData.Rows[0],
                            Request.Form["rowNum"].ToString(), ""));
                        */ 
                    }
                }
                catch (Exception ex) { }
                finally { db.close(); }
            }
        }
    }
}