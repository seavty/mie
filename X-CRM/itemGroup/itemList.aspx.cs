using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.itemGroup
{
    public partial class itemList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemFind";
        string grid = "tblItemList";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get("itmg_itemgroupid")))
                {
                    Response.Redirect(cls.baseUrl + "itemGroup/itemGroup.aspx");
                }
                if (db.connect())
                {

                    string eid = "";

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "findRecord")
                        {

                            string filter = " 1= 2 ";
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "item_Name";
                            string orderBy = "ASC";

                            if (!string.IsNullOrEmpty(url.Get("itmg_itemgroupid")))
                            {
                                filter = " and item_ItemGroupID=" + url.Get("itmg_itemgroupid");
                            }

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString()), filter: filter));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(url.Get("itmg_itemgroupid")))
                        {
                            eid = url.Get("itmg_itemgroupid");
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblItemGroup", "tblItemList", 1, eid);
                    }

                }
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                Response.End();
            }
            finally { db.close(); }
        }

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            cls.scrnType = global::sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = global::sapi.sapi.recordMode.New;

            var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
            string topTitle = "";
            DataTable tblTop = null;

            if (!string.IsNullOrEmpty(url.Get("itmg_itemgroupid")))
            {
                sapi.Buttons.add("New Item", "plus", "success",
                    "window.location = '../item/item.aspx?itmg_itemgroupid=" + url.Get("itmg_itemgroupid") + "'","I","tblItem");

                topTitle = "Item Group";
                tblTop = db.readData("Select itmg_Name From tblItemGroup Where itmg_Deleted is null and itmg_ItemGroupID=" + url.Get("itmg_itemgroupid"));
            }
            string re = "";




            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}