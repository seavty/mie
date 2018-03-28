using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.customer
{
    public partial class documentList : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "sm_docFind";
        string grid = "sm_docList";
        string frm = "frmMaster";
        string tabl_TableID = "1";
        string tabl_ColumnID = "cust_customerid";

        protected void Page_Load(object sender, EventArgs e)
        {
            var url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);
            if (Request.Form["app"] == null && Request.QueryString["app"] == null)
            {
                dvUpload.InnerHtml =
                    "<hr class='thin bg-grayLighter'/>" +
                    "<div class='input-control file full-size' data-role='input'>" +
                        "<input placeholder='File to upload ...' id='fileupload' type='file' name='files[]' data-url='" +
                        cls.baseUrl + "customer/documentList.aspx?app=upload&" + tabl_ColumnID + "=" + url.Get(tabl_ColumnID) +
                        "'>" + "<button class='button'><span class='mif-folder'></span></button>" +
                    "</div>" +
                    "<br/><label class='tag alert'>(max 2MB)</label> <br/><br/>";
            }

            if (Request.QueryString["app"] != null)
            {
                //string b = Request.QueryString["app"].ToString();
                if (Request.QueryString["app"] == "upload")
                {
                    upload();
                }
                db.close();
                cls.endRequest();
                Response.End();
            }

            ////
            try
            {

                if (string.IsNullOrEmpty(url.Get(tabl_ColumnID)))
                {
                    Response.Redirect(cls.baseUrl + "customer/customer.aspx");
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
                            string orderFieldBy = "docs_CreatedDate";
                            string orderBy = " DESC ";

                            if (!string.IsNullOrEmpty(url.Get(tabl_ColumnID)))
                            {
                                filter = " and docs_Value=" + db.sqlStr(url.Get(tabl_ColumnID)) +
                                    " and docs_TableID = " + tabl_TableID;
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
                        if (!string.IsNullOrEmpty(url.Get(tabl_ColumnID)))
                        {
                            eid = url.Get(tabl_ColumnID);
                        }
                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen("0", global::sapi.sapi.recordMode.New);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, "tblCustomer", "sm_doc", 1, eid);
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

            if (!string.IsNullOrEmpty(url.Get(tabl_ColumnID)))
            {
                topTitle = "Company / Customer";
                tblTop = db.readData("Select cust_Code,cust_Name From tblCustomer Where cust_Deleted is null and cust_CustomerID=" + url.Get("cust_customerid"));
            }
            string re = "";
            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            return re;
        }

        void upload()
        {
            var files = Request.Files;
            if (files != null)
            {
                double fs = files[0].ContentLength / 1024;
                if (fs > 2048)
                {
                    Response.Write("[{\"status\":\"fs\"}]");
                    db.close();
                    cls.endRequest();
                    Response.End();
                }
                string Y = @"uploads/" + DateTime.Now.ToString("yyyy");
                string M = DateTime.Now.ToString("MM");
                string filePath = Y + @"/" + M;
                string filename = "";

                try
                {

                    if (db.connect())
                    {
                        if (!System.IO.Directory.Exists(Server.MapPath("~/" + Y)))
                        {
                            System.IO.Directory.CreateDirectory(Server.MapPath("~/" + Y));
                        }
                        if (M != "")
                        {
                            M = "/" + M;
                            if (!System.IO.Directory.Exists(Server.MapPath("~/" + Y + M)))
                            {
                                System.IO.Directory.CreateDirectory(Server.MapPath("~/" + Y + M));
                            }
                            M = M + "/";
                        }

                        string sFilename = Server.MapPath("~/" + Y + M + files[0].FileName);
                        filePath = Y + M + files[0].FileName;
                        filename = System.IO.Path.GetFileName(files[0].FileName);

                        if (System.IO.File.Exists(sFilename))
                        {
                            filename = System.IO.Path.GetFileNameWithoutExtension(files[0].FileName) + "_" +
                                DateTime.Now.ToString("yyyyMMddhhmmss") + System.IO.Path.GetExtension(files[0].FileName);

                            sFilename = Server.MapPath("~/" + Y + M + filename);
                            filePath = Y + M + filename;

                        }
                        files[0].SaveAs(sFilename);

                        vals.Clear();

                        vals.Add("docs_TableID".ToLower(), tabl_TableID);
                        vals.Add("docs_Name".ToLower(), filename);
                        Dictionary<string, string> aVal = new Dictionary<string, string>();
                        aVal.Add("docs_FilePath".ToLower(), filePath);
                        aVal.Add("docs_Value".ToLower(), Request.QueryString[tabl_ColumnID]);
                        string re = cls.saveRecord("sm_docNew", vals, db, aVals: aVal);
                        var str = JsonConvert.DeserializeObject<dynamic>(re);
                        if (str.tbl != null)
                        {
                            if (str.tbl[0].status == "ok")
                            {

                                Response.Write("[{\"status\":\"ok\"}]");
                            }
                            else
                            {
                                Response.Write("[{\"status\":\"error\",\"msg\":\"" + str.tabl[0].msg + "\"}]");
                            }

                        }
                        else
                        {
                            Response.Write("[{\"status\":\"error\",\"msg\":\"Error Uploading Data \"}]");
                        }

                    }
                }
                catch (Exception ex)
                {
                    Response.Write("[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]");
                }
                finally { db.close(); }
            }
        }
    }
}