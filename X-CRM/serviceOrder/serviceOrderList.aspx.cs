﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.serviceOrder
{
    public partial class serviceOrderList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblServiceOrderFind";
        string grid = "tblServiceOrderList2";
        string frm = "frmMaster";

        protected void Page_Load(object sender, EventArgs e)
        {
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
                            string orderFieldBy = "seor_Name";
                            string orderBy = "";

                            if (Request.Form["orderFieldBy"] != null) orderFieldBy = Request.Form["orderFieldBy"].ToString();
                            if (Request.Form["orderBy"] != null) orderBy = Request.Form["orderBy"].ToString();

                            Response.Write(cls.findRecord(db, screen, grid, frm, vals, orderFieldBy, orderBy,
                                cPage: (int)db.cNum(Request.Form["cPage"].ToString())));
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", sapi.sapi.recordMode.New);

                        //((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                        //    cls.loadTab(db, "tblCustomer", "tblCustomer", 1, showCTab: true);
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
            sapi.Buttons.add("New Service Order", "plus", "success", "window.location = 'serviceOrder.aspx'");
            re = cls.loadScreen(db, screen, frm, ref tblData, eid);
            return re;
        }
    }
}