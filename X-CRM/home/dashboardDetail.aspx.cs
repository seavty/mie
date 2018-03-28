using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.home
{
    public partial class dashboardDetail : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string grid = "tblInvoiceList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["dbit"] != null)
            {
                try
                {
                    if (db.connect())
                    {
                        DataTable tbl = db.readData("Select * " +
                            "from sys_dashboarditem inner join sys_list on list_listid = dbit_ListID where dbit_Deleted is null and dbit_Dashboarditemid = " + Request.QueryString["dbit"]);
                        foreach (DataRow row in tbl.Rows)
                        {

                            grid = row["list_Name"].ToString();
                            string filter = row["dbit_Condition"].ToString().Replace("#u", Session["userid"].ToString()).Replace("#p", Session["profiles"].ToString());
                            if (filter.Length > 0)
                                filter = " and " + filter;
                            string re = "<h2>" + row["dbit_Name"].ToString() + "</h2>" +
                                "<hr class=thin bg-grayLighter'/>";
                            if (Request.Form["app"] != null)
                            {
                                if (Request.Form["app"].ToString() == "findRecord")
                                {
                                    vals.Clear();
                                    foreach (var st in Request.Form.AllKeys)
                                    {
                                        if (st != null)
                                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                                    }
                                    string orderFieldBy = "";
                                    string orderBy = "";

                                    if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                                    if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                                    re = re + cls.findRecord(db, "", grid, frm, vals, orderFieldBy, orderBy,
                                        cPage: (int)db.cNum(Request.Form["cPage"].ToString()), filter: filter);

                                    Response.Write(re);
                                }
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }
                            else
                            {
                                re = re + (cls.findRecord(db, "", grid, frm, vals, "", "",
                                    cPage: 1, filter: filter));
                                dvList.InnerHtml = re;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }
                finally { db.close(); }
            }
        }
    }
}