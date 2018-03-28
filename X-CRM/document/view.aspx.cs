using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.document
{
    public partial class view : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["docs_docid"] == null)
            {
                Response.Write("Document Not Found !");
                Response.End();
            }
            string docs_docid = Request.QueryString["docs_docid"].ToString();
            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {
                    DataTable tbl = db.readData("Select * from sm_doc Where docs_docID=" + docs_docid);
                    foreach (DataRow row in tbl.Rows)
                    {
                        string FileName = (Server.MapPath("~/" + row["docs_FilePath"].ToString()));
                        Response.Write(FileName);
                        
                        Response.Clear();
                        Response.ContentType = @"application\octet-stream";
                        System.IO.FileInfo file = new System.IO.FileInfo(FileName);
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + file.Name);
                        Response.AddHeader("Content-Length", file.Length.ToString());
                        Response.ContentType = "application/octet-stream";
                        Response.WriteFile(file.FullName);
                        Response.Flush();
                         
                    }
                }
            }
            catch (Exception ex) { }
            finally { db.close(); }
            
        }
    }
}