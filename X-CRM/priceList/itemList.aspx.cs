using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.priceList
{
    public partial class itemList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblPriceListItemFind";
        string grid = "tblPriceListItemList";
        string frm = "frmMaster";
        string tabl_columnid = "prls_pricelistid";
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (db.connect())
                {
                    string eid = "";
                    var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "newItem")
                        {

                            if (Request.Form["eid"] != null)
                            {
                                if (Request.Form["eid"].ToString() != "0")
                                {
                                    sapi.readOnlyField.add("plit_pricelistid");
                                    cls.Mode = sapi.sapi.recordMode.Edit;
                                    eid = Request.Form["eid"].ToString();
                                    sapi.Buttons.add("Delete", "bin", "danger", "delItem(" + eid + ")");
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(url.Get("prls_pricelistid")))
                                    {
                                        sapi.defaultValue.add("plit_pricelistid", url.Get("prls_pricelistid"));
                                        sapi.readOnlyField.add("plit_pricelistid");
                                    }

                                    if (Request.Form["item_itemid"] != null)
                                    {
                                        if (!string.IsNullOrEmpty(Request.Form["item_itemid"].ToString()))
                                        {
                                            sapi.defaultValue.add("plit_itemid", Request.Form["item_itemid"].ToString());
                                            sapi.readOnlyField.add("plit_itemid");
                                            sapi.requiredField.add("plit_PriceListID");
                                        }
                                    }
                                }

                            }
                           
                            DataTable tbl = null;

                            Response.Write(cls.loadScreen(db, "tblPriceListItemNew", "frmAddItem", ref tbl, eid));

                        }

                        if (Request.Form["app"].ToString() == "saveItem")
                        {

                            Dictionary<string, string> vals = new Dictionary<string, string>();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string tmpStr = "";
                            if (vals.ContainsKey("plit_PriceListItemID".ToLower()))
                            {
                                tmpStr = " and plit_PriceListItemID <> " + vals["plit_PriceListItemID".ToLower()];
                            }
                            var plid = vals["txt_eid".ToLower()];

                            if (Request.Form["plit_PriceListID"] != null)
                                plid = Request.Form["plit_PriceListID"].ToString();

                            if (Request.Form["plit_PriceListID"] == null)
                            {

                                Response.Write("{\"error\":[{\"colName\":\"plit_PriceListID\",\"msg\":\"Item is already in Price list !\",\"errType\":\"once\"}]}");
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }

                            if (db.readData("Select 1 from tblPriceListItem where plit_Deleted is null and plit_ItemID = " +
                                    vals["plit_ItemID".ToLower()] + " and plit_PriceListID = " + 
                                    plid + tmpStr
                               ).Rows.Count > 0)
                            {

                                Response.Write("{\"error\":[{\"colName\":\"plit_ItemID\",\"msg\":\"Item is already in Price list !\",\"errType\":\"repate\"}]}");
                                db.close();
                                cls.endRequest();
                                Response.End();
                            }

                            Dictionary<string, string> aVals = new Dictionary<string, string>();
                            if (!vals.ContainsKey("plit_PriceListID".ToLower()))
                            {
                                if(Request.Form["plit_PriceListID"] == null)
                                    aVals.Add("plit_PriceListID", vals["txt_eid".ToLower()]);
                                else
                                    aVals.Add("plit_PriceListID", Request.Form["plit_PriceListID"].ToString());	
                            }
                            else
                            {
                                if (Request.Form["plit_PriceListID"] == null)
                                    vals["plit_PriceListID".ToLower()] = vals["txt_eid".ToLower()];
                                else
                                    vals["plit_PriceListID".ToLower()] = Request.Form["plit_PriceListID"].ToString();

                                }
                            var re = cls.saveRecord("tblPriceListItemNew", vals, db, aVals);
                            Response.Write(re);
                        }

                        if (Request.Form["app"].ToString() == "delItem")
                        {

                            Response.Write(db.execData("Update tblPriceListItem set plit_Deleted = 'Y' " +
                                " Where  plit_PriceListItemID = " + Request.Form["eid"].ToString()));


                        }

                        ////////////////////////////////////////

                        if (Request.Form["app"].ToString() == "findRecord")
                        {
                            string filter = " 1= 2 ";
                            vals.Clear();
                            foreach (var st in Request.Form.AllKeys)
                            {
                                if (st != null)
                                    vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                            {
                                filter = " and plit_PriceListID =" + url.Get(tabl_columnid);
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
                        if (!string.IsNullOrEmpty(url.Get(tabl_columnid)))
                        {
                            eid = url.Get(tabl_columnid);
                        }

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblPriceList", "tblPriceListItem", 1,eid);
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
            cls.scrnType = global::sapi.sapi.screenType.SearchScreen;

            if (eid == "0")
                cls.Mode = global::sapi.sapi.recordMode.New;

            string re = "";
            sapi.Buttons.add("New Item", "plus", "success", "addItem(" + eid + ")");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            return re;
        }
    }
}