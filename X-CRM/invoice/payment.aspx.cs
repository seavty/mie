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
    public partial class payment : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblInvoicePaymentNew";
        string frm = "frmPayment";
        string IDFIeld = "ivpm_invoicepaymentid";
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
                            sapi.defaultValue.add("ivpm_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                            if (Request.Form["invo_invoiceid"] != null)
                            {
                                if (Request.Form["invo_invoiceid"].ToString() == "0")
                                {
                                    DataTable tbl = db.readData("Select SUM(invo_Balance) invo_Balance from tblInvoice Where invo_Deleted is null and invo_CustomerID = " +
                                        Request.Form["cust_customerid"].ToString());
                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        sapi.defaultValue.add("ivpm_Amount", db.cNum(row["invo_Balance"].ToString()).ToString());
                                    }
                                    sapi.readOnlyField.add("ivpm_InvoiceID");
                                }
                                else
                                {
                                    DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID = " +
                                        Request.Form["invo_invoiceid"].ToString());
                                    foreach (DataRow row in tbl.Rows)
                                    {
                                        sapi.defaultValue.add("ivpm_Amount", db.cNum(row["invo_Balance"].ToString()).ToString());
                                    }

                                    if (!string.IsNullOrEmpty(Request.Form["invo_invoiceid"].ToString()))
                                    {
                                        sapi.defaultValue.add("ivpm_InvoiceID", Request.Form["invo_invoiceid"].ToString());
                                        sapi.readOnlyField.add("ivpm_InvoiceID");
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
                sapi.defaultValue.add("ivpm_Payment", "Payment");
            }
            string re = "";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid);


            return re;
        }

        string saveRecord()
        {
            string re = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();

            if (vals.ContainsKey("ivpm_invoiceid"))
            {
                if (!string.IsNullOrEmpty(vals["ivpm_invoiceid"]))
                {
                    if (vals["ivpm_invoiceid"] == "0")
                    {
                        DataTable tbl = db.readData("Select SUM(invo_Balance) invo_Balance from tblInvoice Where invo_Deleted is null and invo_CustomerID=" + vals["cust_customerid"]);
                        foreach (DataRow row in tbl.Rows)
                        {
                            
                            DataTable tblErr = new DataTable();
                            tblErr.Columns.Add("colName");
                            tblErr.Columns.Add("msg");
                            tblErr.Columns.Add("errType");
                            tblErr.Rows.Add();
                            if (db.cNum(vals["ivpm_amount"]) <= 0)
                            {
                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount must be greater than 0 !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }

                            if (db.cNum(vals["ivpm_amount"]) > db.cNum(row["invo_Balance"].ToString()))
                            {

                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount cannot greater than " +
                                    (db.cNum(row["invo_Balance"].ToString())).ToString(cls.numFormat) + " !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }
                        }
                    }
                    else
                    {
                        DataTable tbl = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_InvoiceID=" + vals["ivpm_invoiceid"]);
                        foreach (DataRow row in tbl.Rows)
                        {
                            aVal.Add("ivpm_WarehouseID", row["invo_WarehouseID"].ToString());
                            DataTable tblErr = new DataTable();
                            tblErr.Columns.Add("colName");
                            tblErr.Columns.Add("msg");
                            tblErr.Columns.Add("errType");
                            tblErr.Rows.Add();
                            if (db.cNum(vals["ivpm_amount"]) <= 0)
                            {
                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
                                tblErr.Rows[tblErr.Rows.Count - 1]["msg"] = "Payment amount must be greater than 0 !";
                                tblErr.Rows[tblErr.Rows.Count - 1]["errType"] = "repeat";

                                re = ("{\"tbl\":" + db.tblToJson(tblErr) + "}");
                                return re;
                            }

                            if (db.cNum(db.cNum(vals["ivpm_amount"]).ToString(cls.numFormat)) >
                                db.cNum(db.cNum(row["invo_Balance"].ToString()).ToString(cls.numFormat)))
                            {

                                tblErr.Rows[tblErr.Rows.Count - 1]["colName"] = "ivpm_Amount";
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

            DataTable tblInv = db.readData("Select * from tblInvoice Where invo_Deleted is null and invo_CustomerID=" + vals["cust_customerid"]);
            
            if (tblInv.Rows.Count == 0)
            {
                
                re = cls.saveRecord(screen, vals, db, aVals: aVal);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    if (str.tbl[0].status == "ok")
                    {
                        
                        var tmp = db.execData("Update tblInvoice Set " +
                                " invo_PaidAmount = isNull(invo_PaidAmount,0) + " + db.cNum(vals["ivpm_amount"]) +
                                ",invo_Balance = isnull(invo_GTotal,0) - isnull(invo_CreditNote,0) - isnull(invo_Deposit,0) - isnull(invo_PaidAmount,0) - " + db.cNum(vals["ivpm_amount"]) +
                                " Where invo_InvoiceID = " + vals["ivpm_invoiceid"]);
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
                        clsGlobal clsglobal = new clsGlobal();
                        clsglobal.validInvoice(vals["ivpm_invoiceid"], db);
                    }
                }
            }
            else
            {
                double ivpm_amount = db.cNum(vals["ivpm_amount"]);
                double rivpm_amount = 0;
                foreach (DataRow row in tblInv.Rows)
                {
                    if (ivpm_amount <= 0) break;
                    aVal.Clear();
                    aVal.Add("ivpm_WarehouseID", row["invo_WarehouseID"].ToString());
                    rivpm_amount = db.cNum(row["invo_Balance"].ToString());
                    if (ivpm_amount < db.cNum(row["invo_Balance"].ToString()))
                    {
                        rivpm_amount = ivpm_amount;
                    }
                    else
                    {
                        ivpm_amount = ivpm_amount - rivpm_amount;
                    }
                    vals["ivpm_invoiceid"] = row["invo_InvoiceID"].ToString();
                    rivpm_amount = db.cNum(rivpm_amount.ToString(cls.numFormat));
                    vals["ivpm_amount"] = rivpm_amount.ToString();
                    re = cls.saveRecord(screen, vals, db, aVals: aVal);

                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                    if (str.tbl != null)
                    {
                        if (str.tbl[0].status == "ok")
                        {
                            var tmp = db.execData("Update tblInvoice Set " +
                                    " invo_PaidAmount = isNull(invo_PaidAmount,0) + " + rivpm_amount +
                                    ",invo_Balance = isnull(invo_GTotal,0) - isnull(invo_Deposit,0) - isnull(invo_PaidAmount,0) - " + rivpm_amount +
                                    " Where invo_InvoiceID = " + row["invo_InvoiceID"]);
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
                            new clsGlobal().validInvoice(row["invo_InvoiceID"].ToString(), db);
                        }
                    }
                }
            }
            db.commit();
            return re;
        }
    }
}