using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.item
{
    public partial class itemLayout : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblItemLayout";
        string grid = "";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "";
                            if (Request.Form["colName"].ToString() == "itwh_ItemLayout1")
                            {
                                filter = " 1 = 2";
                                if (Request.Form["itwh_WarehouseID"] != null)
                                {
                                    filter = " itl1_WarehouseID = " + Request.Form["itwh_WarehouseID"].ToString();
                                }
                            }
                            sapi.sapi cls = new sapi.sapi();
                            Response.Write(cls.loadSSA(db,
                                Request.Form["colid"].ToString(),
                                Request.Form["q"].ToString(), filter: filter));
                            db.close();
                            cls.endRequest();
                            Response.End();

                        }
                    }

                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "saveItem")
                        {
                            vals.Clear();

                            foreach (var st in Request.Form.AllKeys)
                            {
                                vals.Add(st.ToLower(), Request.Form[st].ToString());
                            }
                            string tmpStr = "";
                            if (vals.ContainsKey("itel_ItemLayoutID".ToLower()))
                            {
                                tmpStr = " and itel_ItemLayoutID <> " + vals["itel_ItemLayoutID".ToLower()];
                            }

                            if (vals.ContainsKey("itel_ItemID".ToLower()))
                                if (db.readData("Select 1 from tblItemLayout where itel_Deleted is null and itel_ItemID = " +
                                    vals["itel_ItemID".ToLower()] + " and itel_ItemLayout3 = " + vals["itel_ItemLayout3".ToLower()] +
                                    tmpStr).Rows.Count > 0)
                                {

                                    Response.Write("{\"error\":[{\"colName\":\"itel_ItemID\",\"msg\":\"Item is already in layout !\",\"errType\":\"repate\"}]}");
                                    db.close();
                                    cls.endRequest();
                                    Response.End();
                                }
                            Response.Write(cls.saveRecord("tblItemLayoutNew", vals, db, aVals: null));
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getItem")
                        {

                            Response.Write(cls.findRecord(db, "", "tblItemList", "", null, "", "", -1,
                                filter: " and item_ItemID in (select itel_ItemID from tblItemLayout where itel_ItemLayout3 = " + db.cNum(Request.Form["id"].ToString()) +
                                ") "));
                        }

                        if (Request.Form["app"].ToString() == "addtItem")
                        {
                            sapi.defaultValue.add("itel_ItemLayout3", Request.Form["id"].ToString());
                            Response.Write(cls.loadScreen(db, "tblItemLayoutNew", "frmAddItem", ref tblData));

                        }


                        if (Request.Form["app"].ToString() == "getData")
                        {
                            string tbl = "<table class='table border bordered striped hovered'>";
                            DataTable tbl2 = db.readData("Select * from tblItemLayout2 Where itl2_Deleted is null and itl2_ItemLayout1 = " +
                                db.cNum(Request.Form["layout1"].ToString()));
                            foreach (DataRow row2 in tbl2.Rows)
                            {
                                tbl = tbl + "<tr>" +
                                    "<td>" + row2["itl2_Name"].ToString() + "</td>";
                                DataTable tbl3 = db.readData("Select * from tblItemLayout3 Where itl3_Deleted is null and itl3_ItemLayout2 = " + row2["itl2_ItemLayout2"].ToString());
                                foreach (DataRow row3 in tbl3.Rows)
                                {
                                    tbl = tbl + "<td >" +
                                            "<a href='javascript:itemDetail(" + row3["itl2_ItemLayout3"].ToString() + ")'>" +
                                            row3["itl3_Name"].ToString() + "</a>" +
                                            "<div class='place-right'>" +
                                                "<button onclick=\"addItem(" + row3["itl2_ItemLayout3"].ToString() + ")\" class=\"button rounded success\" type=\"button\"><span class=\"mif-plus\">" +
                                                "</span></button></div>" +
                                        "</td>";
                                }
                                tbl = tbl + "</tr>";
                            }
                            tbl = tbl + "</table>";
                            Response.Write(tbl);
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "", "", 1, showCTab: true);
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
            //sapi.Buttons.add("New Item Group", "plus", "success", "window.location = 'itemGroup.aspx'");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid, topContentHeader: "Item Layout");
            return re;
        }
    }
}