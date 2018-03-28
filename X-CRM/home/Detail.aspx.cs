using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.home
{
    public partial class Detail : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoiceFind";
        string grid = "tblInvoiceList";
        string frm = "frmMaster";
        string filter = "";
        string topContentHeader = "Outstanding Invoices";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["app"] != null)
            {
                if (Request.QueryString["app"].ToString() == "dv5")
                {
                    screen = "tblInvoiceFind";
                    grid = "tblInvoiceList";
                    frm = "frmMaster";
                    filter = " and invo_Deleted is null and isNull(invo_Balance,0) > 0 ";
                    topContentHeader = "Outstanding Invoices";
                }
                if (Request.QueryString["app"].ToString() == "dv6")
                {
                    screen = "tblSaleOrderFind";
                    grid = "tblSaleOrderList";
                    frm = "frmMaster";
                    filter = " and sord_Deleted is null and isnull(sord_isComplete,'N') = 'N' ";
                    topContentHeader = "Pending Sale Order";
                }
                if (Request.QueryString["app"].ToString() == "dv7")
                {
                    screen = "tblContractFind";
                    grid = "tblContractList";
                    frm = "frmMaster";
                    filter = " and 1=1 ";
                    topContentHeader = "Expired Contract";
                }
            }
            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "findRecord")
                        {
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "";
                            string orderBy = "";
                           
                            
                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString()),filter:filter));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                       
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally { db.close(); }
        }

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            cls.scrnType = sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = sapi.sapi.recordMode.New;

            string re = "";
            
            re = cls.loadScreen(db, screen, frm, ref tblData, eid,topContentHeader:topContentHeader);
            return re;
        }
    }
}