using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.invoice
{
    public partial class ajax : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"].ToString() == "ConvertToProject")
                        ConvertToProject();
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.close();
                cls.endRequest();
            }
        }

        public void ConvertToProject()
        {
            try
            {
                string[] InvoiceItemID = Request.Form["ids[]"].ToString().Split(',');
                string InvoID = Request.Form["InvoID"].ToString();
                string screen = "";
                DataTable dt = new DataTable();

                Dictionary<string, string> vals = new Dictionary<string, string>();
                Dictionary<string, string> aval = new Dictionary<string, string>();

                dt = db.readData("SELECT * FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_InvoiceID = " + InvoID);
                foreach (DataRow dr in dt.Rows)
                {
                    aval.Add("oppo_InvoiceID".ToLower(), InvoID);
                    vals.Add("oppo_InvoiceNo".ToLower(), dr["invo_Name"].ToString());
                    vals.Add("oppo_Status".ToLower(), "New");

                    vals.Add("oppo_StartDate".ToLower(), DateTime.Now.ToString("dd/MM/yyyy"));

                    screen = "tblOpportunityNew";
                }

                string re = cls.saveRecord(screen, vals, db, aVals: aval, ignoreROF: true);
                var str = JsonConvert.DeserializeObject<dynamic>(re);
                string oppoID = (string)str.tbl[0].msg;

                foreach (string b in InvoiceItemID)
                {
                    dt = db.readData("SELECT * FROM tblInvoiceItem WHERE init_Deleted IS NULL AND init_InvoiceItemID = " + b);

                    foreach (DataRow dr in dt.Rows)
                    {
                        string quotItemId = dr["init_IsConvert"].ToString();
                        if (quotItemId != "Yes")
                        {
                            vals.Clear();
                            aval.Clear();

                            vals.Add("ipit_ProjectID".ToLower(), oppoID);
                            vals.Add("ipit_InvoiceID".ToLower(), InvoID);
                            vals.Add("ipit_InvoiceItemID".ToLower(), b);

                            //aval.Add("opit_OpportunityID".ToLower(), oppoID);
                            //vals.Add("opit_InvoiceItemCode".ToLower(), dr["init_ItemID"].ToString());
                            //vals.Add("opit_InvoiceItemDescription".ToLower(), dr["init_Description"].ToString());
                            //vals.Add("opit_InvoiceItemProcessingPeriod".ToLower(), dr["init_PeriodProcessing"].ToString());
                            //vals.Add("opit_InvoiceItemRemark".ToLower(), dr["init_Remark"].ToString());

                            re = cls.saveRecord("tblProjectInvoiceItemNew", vals, db, aVals: aval, ignoreROF: true);
                            str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                                db.execData("UPDATE tblInvoiceItem SET init_IsConvert = 'Yes' WHERE init_InvoiceItemID = " + b);
                            }
                        }
                    }
                }
                if (str.tbl[0].status == "ok")
                    Response.Write("ok" + oppoID);
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }
    }
}