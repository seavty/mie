using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.apInvoice
{
    public partial class payment : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblAPInvoicePaymentNew";
        string frm = "frmPayment";
        string IDFIeld = "avpm_apinvoicepaymentid";
        string Tab = "";
        string cTab = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            init();
            if (Request.Form["app"] != null)
            {
                try
                {
                    if (db.connect())
                    {

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            sapi.defaultValue.add("avpm_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                            if (Request.Form["apiv_apinvoiceid"] != null)
                            {
                                if (Request.Form["apiv_apinvoiceid"].ToString() == "0")
                                {
                                    DataTable tbl = db.readData("Select SUM(apiv_Balance) apiv_Balance from tblAPInvoice Where apiv_Deleted is null and apiv_SupplierID = " +
                                        Request.Form["supp_supplierid"].ToString());
                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        sapi.defaultValue.add("avpm_Amount", db.cNum(row["apiv_Balance"].ToString()).ToString());
                                    }
                                    sapi.readOnlyField.add("avpm_APInvoiceID");
                                }
                                else
                                {
                                    DataTable tbl = db.readData("Select * from tblAPInvoice Where apiv_Deleted is null and apiv_APInvoiceID = " +
                                    Request.Form["apiv_apinvoiceid"].ToString());
                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        sapi.defaultValue.add("avpm_Amount", db.cNum(row["apiv_Balance"].ToString()).ToString());
                                    }

                                    if (!string.IsNullOrEmpty(Request.Form["apiv_apinvoiceid"].ToString()))
                                    {
                                        sapi.defaultValue.add("avpm_APInvoiceID", Request.Form["apiv_apinvoiceid"].ToString());
                                        sapi.readOnlyField.add("avpm_APInvoiceID");
                                    }
                                }
                            }
                            Response.Write(loadScreen("0", global::sapi.sapi.recordMode.New));
                        }
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(saveRecord());
                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally { db.close(); }
            }
        }

        void init()
        {
            //url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

            vals.Clear();

            foreach (var st in Request.Form.AllKeys)
            {
                vals.Add(st.ToLower(), Request.Form[st].ToString());
            }
        }

        string loadScreen(string eid, global::sapi.sapi.recordMode mode)
        {
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
            }
            string re = "";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid);


            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();

            if (vals.ContainsKey("avpm_apinvoiceid"))
            {
                if (!string.IsNullOrEmpty(vals["avpm_apinvoiceid"]))
                {
                    if (vals["avpm_apinvoiceid"] == "0")
                    {
                        DataTable tbl = db.readData("Select SUM(apiv_Balance) apiv_Balance from tblInvoice Where apiv_Deleted is null and invo_CustomerID=" + vals["supp_supplierid"]);
                        foreach (DataRow row in tbl.Rows)
                        {

                            DataTable tblErr = new DataTable();
                            tblErr.Columns.Add("colName");
                            tblErr.Columns.Add("msg");
                            tblErr.Columns.Add("errType");
                            tblErr.Rows.Add();
                            if (db.cNum(vals["ivpm_amount"]) <= 0)
                            {
                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "apiv_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount must be greater than 0 !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }

                            if (db.cNum(vals["apiv_amount"]) > db.cNum(row["apiv_Balance"].ToString()))
                            {

                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "apiv_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount cannot greater than " +
                                    (db.cNum(row["apiv_Balance"].ToString())).ToString(cls.numFormat) + " !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }
                        }
                    }
                    else
                    {
                        DataTable tbl = db.readData("Select * from tblAPInvoice Where apiv_Deleted is null and apiv_APInvoiceID=" + vals["avpm_apinvoiceid"]);
                        foreach (DataRow row in tbl.Rows)
                        {
                            DataTable tblErr = new DataTable();
                            tblErr.Columns.Add("colName");
                            tblErr.Columns.Add("msg");
                            tblErr.Columns.Add("errType");
                            tblErr.Rows.Add();
                            if (db.cNum(vals["avpm_amount"]) <= 0)
                            {
                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "avpm_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount must be greater than 0 !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }

                            if (db.cNum(vals["avpm_amount"]) > db.cNum(row["apiv_Balance"].ToString()))
                            {

                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "avpm_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount cannot greater than " +
                                    (db.cNum(row["invo_Balance"].ToString())).ToString(cls.numFormat) + " !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }
                        }
                    }
                }
            }
            db.beginTran();
            DataTable tblInv = db.readData("Select * from tblAPInvoice Where apiv_Deleted is null and apiv_SupplierID=" + vals["supp_supplierid"]);

            if (tblInv.Rows.Count == 0)
            {
                re = cls.saveRecord(screen, vals, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        var tmp = db.execData("Update tblAPInvoice Set " +
                                " apiv_PaidAmount = isNull(apiv_PaidAmount,0) + " + db.cNum(vals["avpm_amount"]) +
                                ",apiv_Balance = isnull(apiv_GTotal,0) - isnull(apiv_Deposit,0) - isnull(apiv_PaidAmount,0) - " + db.cNum(vals["avpm_amount"]) +
                                " Where apiv_APInvoiceID = " + vals["avpm_apinvoiceid"]);
                        if (tmp != "ok")
                        {
                            db.rollback();
                            DataTable tblResult = new DataTable();
                            tblResult.Rows.Add();
                            tblResult.Columns.Add("status");
                            tblResult.Columns.Add("msg");
                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = tmp;
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            return re;
                        }
                        new clsGlobal().validAPInvoice(vals["avpm_apinvoiceid"], db);
                    }
                }
            }
            else
            {
                double ivpm_amount = db.cNum(vals["avpm_amount"]);
                double rivpm_amount = 0;
                foreach (DataRow row in tblInv.Rows)
                {
                    if (ivpm_amount <= 0) break;
                    aVal.Clear();
                    
                    rivpm_amount = db.cNum(row["apiv_Balance"].ToString());
                    if (ivpm_amount < db.cNum(row["apiv_Balance"].ToString()))
                    {
                        rivpm_amount = ivpm_amount;
                    }
                    else
                    {
                        ivpm_amount = ivpm_amount - rivpm_amount;
                    }
                    vals["avpm_APInvoiceID".ToLower()] = row["apiv_APInvoiceID"].ToString();
                    vals["avpm_Amount".ToLower()] = rivpm_amount.ToString();
                    re = cls.saveRecord(screen, vals, db, aVals: aVal);

                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                    if (str.tbl != null)
                    {
                        if (str.tbl[0].status == "ok")
                        {
                            var tmp = db.execData("Update tblAPInvoice Set " +
                                    " apiv_PaidAmount = isNull(apiv_PaidAmount,0) + " + rivpm_amount +
                                    ",apiv_Balance = isnull(apiv_GTotal,0) - isnull(apiv_Deposit,0) - isnull(apiv_PaidAmount,0) - " + rivpm_amount +
                                    " Where apiv_APInvoiceID = " + row["apiv_APInvoiceID"]);
                            if (tmp != "ok")
                            {
                                db.rollback();
                                DataTable tblResult = new DataTable();
                                tblResult.Rows.Add();
                                tblResult.Columns.Add("status");
                                tblResult.Columns.Add("msg");
                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = tmp;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                            new clsGlobal().validAPInvoice(row["apiv_APInvoiceID"].ToString(), db);
                        }
                    }
                }
            }
            db.commit();
            return re;
        }
    }
}