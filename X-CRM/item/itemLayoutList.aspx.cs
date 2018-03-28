using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.item
{
    public partial class itemLayoutList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemLayout";
        string grid = "tblItemLayoutList";
        string frm = "frmMaster";
        string tabl_columnid = "item_itemid";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
                if (string.IsNullOrEmpty(url.Get(tabl_columnid)))
                {
                    Response.Redirect(cls.baseUrl + "item/item.aspx");
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
                                if (st != null)
                                    vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string orderFieldBy = "";
                            string orderBy = "";

                            if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                            {
                                filter = " and itel_ItemID=" + url.Get(tabl_columnid);
                            }

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString()), filter: filter));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                       

                        if (Request.Form["app"].ToString() == "newItem")
                        {
                            sapi.readOnlyField.add("itel_ItemID");
                            if (Request.Form["eid"] != null)
                            {
                                if (Request.Form["eid"].ToString() != "0")
                                {
                                    
                                    cls.Mode = sapi.sapi.recordMode.Edit;
                                    eid = Request.Form["eid"].ToString();
                                    sapi.Buttons.add("Delete", "bin", "danger", "delItem(" + eid + ")");
                                }
                                else
                                {
                                    

                                    if (Request.Form["item_itemid"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(Request.Form["item_itemid"].ToString()))
                                        {
                                            sapi.defaultValue.add("itel_ItemID", Request.Form["item_itemid"].ToString());
                                            
                                        }
                                    }
                                }

                            }

                            DataTable tbl = null;

                            Response.Write(cls.loadScreen(db, "tblItemLayoutNew", "frmAddItem", ref tbl, eid));

                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "saveItem")
                        {

                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            Dictionary<string, string> aVals = new Dictionary<string, string>();
                            
                            var re = cls.saveRecord("tblItemLayoutNew", vals, db, aVals);
                            Response.Write(re);
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "delItem")
                        {

                            Response.Write(db.execData("Update tblItemLayout set itel_Deleted = 'Y' " +
                                " Where  itel_ItemLayoutID = " + Request.Form["eid"].ToString()));
                            db.close();
                            cls.endRequest();
                            Response.End();

                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                        {
                            eid = url.Get(tabl_columnid);
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblItem", "tblItemLayout", 1, eid);
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

            if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
            {
                topTitle = "Item";
                tblTop = db.readData("Select item_Name From tblItem Where item_Deleted is null and item_ItemID=" + url.Get(tabl_columnid));
            }
            string re = "";



            sapi.Buttons.add("New Item", "plus", "success", "addItem(" + eid + ")");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }
    }
}