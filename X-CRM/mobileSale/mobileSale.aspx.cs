using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.mobileSale
{
    public partial class mobileSale1 : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblMobileSaleNew";
        string frm = "frmMaster";
        string IDFIeld = "mbsl_MobileSaleID".ToLower();
        string Tab = "tblMobileSale";
        string cTab = "tblMobileSale";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            Init();

            try
            {
                if (db.connect())
                {

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(SaveRecord());
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
                            Response.Write(LoadScreen(Request.Form["eid"].ToString(), cls.Mode));
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
                            LoadScreen(eid, cls.Mode);

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

        void Init()
        {
            url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

            vals.Clear();

            foreach (var st in Request.Form.AllKeys)
            {
                vals.Add(st.ToLower(), Request.Form[st].ToString());
            }

        }

        string LoadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
            }
            else
            {
                if (cls.Mode == sapi.sapi.recordMode.View)
                {
                    DataTable tbl = db.readData("Select * from tblMobileSale Where mbsl_Deleted is null and mbsl_MobileSaleID = " + eid);
                    foreach (DataRow row in tbl.Rows)
                    {
                        if (string.IsNullOrEmpty(row["mbsl_CheckIn"].ToString()))
                        {   
                            sapi.Buttons.add("Invoice", "plus", "success", "window.location = '../invoice/invoice.aspx?mbsl_mobilesaleid=" + eid + "'","I","tblInvoice");
                            sapi.Buttons.add("Clear ", "cancel", "warning", "clearStock(" + eid + ")","E","tblMobileSale");
                        }
                        else
                        {
                            cls.hideEdit = true;
                            cls.hideDelete = true;
                        }

                        dvSalesman.InnerHtml = cls.findRecord(db, "tblMobileSaleItemFind", "tblMobileSaleItemList", "frmSalesman", null, "user_UserName", "DESC", filter: " and msit_MobileSaleID=" + eid, cPage: -1);
                        tblData = null;

                    }
                    
                }
            }

            string re = "";
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);

            //var tmp = sapi.loadTab(db, "sys_table", "sys_table", 0, eid);
            //Response.Write("<script></script>");


            return re;
        }

        string SaveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            return re;
        }
    }
}