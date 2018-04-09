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
    public partial class invoice : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        //string screen = "tblInvoiceNew;tblInvoiceCustomerInfo;";
        string screen = "tblInvoiceNew";
        string screenItem = "tblInvoiceItemNew";
        string screenItemList = "tblInvoiceItemList";
        string frm = "frmMaster";
        string IDFIeld = "invo_invoiceid";
        string Tab = "tblInvoice";
        string cTab = "tblInvoice";

        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            init();

            try
            {
                if (db.connect())
                {
                    if (Request.Form["app"] != null)
                    {
                        if (Request.Form["app"].ToString() == "SSA")
                        {
                            string filter = "";
                            if (Request.Form["colName"].ToString() == "quot_CustomerID")
                            {
                                if (Request.Form["invo_OpportunityID"].ToString() != "")
                                {
                                    DataTable tmp = db.readData("Select oppo_CustomerID From tblOpportunity Where oppo_OpportunityID=" + Request.Form["quot_OpportunityID"].ToString());
                                    foreach (DataRow tmpRow in tmp.Rows)
                                    {
                                        filter = " cust_CustomerID = " + tmpRow["oppo_CustomerID"].ToString();
                                    }
                                }

                            }
                            if (Request.Form["colName"].ToString() == "init_ItemID")
                            {
                                string b = Request.Form["invo_ServiceType"].ToString();
                                if(!string.IsNullOrEmpty(b))
                                    filter = " item_ItemGroupID = " + b;
                                //DataTable tmp = db.readData("SELECT item_Code FROM tblItem WHERE item_Deleted IS NULL AND item_ItemGroupID =" + b);
                                //foreach (DataRow tmpRow in tmp.Rows)
                                //{
                                //    filter = " item_ItemGroupID = " + Request.Form["item_Code"].ToString();
                                //}
                            }

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
                        if (Request.Form["app"].ToString() == "getPreSaveLine")
                        {

                            sapi.readOnlyField.clear();
                            Response.Write(new clsGlobal().savePreLine(db, Request.Form["screen"].ToString(), vals, Request.Form["rowNum"].ToString()));
                            //sapi.readOnlyField.add("invo_WarehouseID");

                            db.close();
                            cls.endRequest();
                            Response.End();
                        }
                        if (Request.Form["app"].ToString() == "saveRecord")
                        {
                            Response.Write(saveRecord());
                        }

                        if (Request.Form["app"].ToString() == "loadScreen")
                        {
                            if (Request.Form["mode"].ToString() == "3")
                            {
                                cls.Mode = global::sapi.sapi.recordMode.Edit;
                            }
                            else
                            {
                                cls.Mode = global::sapi.sapi.recordMode.View;
                            }
                            Response.Write(loadScreen(Request.Form["eid"].ToString(), cls.Mode));
                        }
                        if (Request.Form["app"].ToString() == "delRecord")
                        {
                            string s = url.Get("invo_invoiceid").ToString();
                            DataTable tb = db.readData("SELECT * FROM tblInvoice WHERE invo_InvoiceID = " + s);

                            Response.Write(cls.delRecord("tblInvoiceNew", Request.Form["eid"].ToString(), db)+ ";" + tb.Rows[0]["invo_Type"]);
                        }
                        db.close();
                        cls.endRequest();
                        Response.End();
                    }
                    else
                    {
                        string eid = "0";
                        bool showCTab = true;
                        if (!String.IsNullOrEmpty(url.Get(IDFIeld)))
                        {
                            cls.Mode = sapi.sapi.recordMode.View;
                            eid = url.Get(IDFIeld);
                            showCTab = false;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(url.Get("quot_quotationid")))
                            {
                                DataTable tbl = db.readData("Select * from vQuotation Where quot_QuotationID = " + url.Get("quot_quotationid"));
                                foreach (DataRow row in tbl.Rows)
                                {
                                    sapi.defaultValue.add("invo_CustomerID", row["quot_CustomerID"].ToString());
                                    sapi.defaultValue.add("invo_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"));
                                    //sapi.defaultValue.add("invo_WarehouseID", row["quot_WarehouseID"].ToString());
                                    sapi.defaultValue.add("invo_Discount", row["quot_Discount"].ToString());
                                    sapi.defaultValue.add("invo_Disc", row["quot_Disc"].ToString());
                                    break;
                                }
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                                {
                                    sapi.defaultValue.add("invo_CustomerID", url.Get("cust_customerid").ToString());
                                }

                                sapi.defaultValue.add("invo_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"));
                                if (!string.IsNullOrEmpty(url.Get("mbsl_mobilesaleid")))
                                {
                                    sapi.defaultValue.add("invo_MobileSaleID", url.Get("mbsl_mobilesaleid").ToString());

                                    sapi.defaultValue.add("invo_WarehouseID",
                                        db.readData("mbsl_WarehouseID", "Select mbsl_WarehouseID From tblMobileSale Where mbsl_Deleted is null and mbsl_MobileSaleID = " +
                                        url.Get("mbsl_mobilesaleid").ToString()));
                                    sapi.readOnlyField.add("invo_WarehouseID");
                                }
                                else
                                    sapi.defaultValue.add("invo_WarehouseID", db.readData("sett_WarehouseID", "Select sett_WarehouseID From sys_Setting"));
                            }


                        }

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvContent")).InnerHtml =
                            loadScreen(eid, cls.Mode);

                        ((System.Web.UI.HtmlControls.HtmlGenericControl)Master.FindControl("dvLeft")).InnerHtml =
                            cls.loadTab(db, Tab, cTab, eid: eid, showCTab: showCTab);

                    }

                }
            }
            catch (Exception ex)
            {
                //throw;
                Response.Write(ex.Message);
            }
            finally { db.close(); }
        }

        void init()
        {
            url = HttpUtility.ParseQueryString((new Uri(Request.Url.Scheme + "://" + Request.Url.Authority + Uri.UnescapeDataString(Request.RawUrl))).Query);

            vals.Clear();

            foreach (var st in Request.Form.AllKeys)
            {
                if (!string.IsNullOrEmpty(st))
                    vals.Add(st.ToLower(), Request.Form[st].ToString());
            }

            string getInvoice = url.Get("invoice");
            string getUrl = url.Get("invo_invoiceid");

            if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
            {
                DataTable tbl = db.readData("SELECT * FROM tblCustomer WHERE cust_Deleted IS NULL AND cust_CustomerID = " + url.Get("cust_customerid"));
                foreach (DataRow row in tbl.Rows)
                {
                    if (row["cust_InvoiceType"].ToString() == "Tax Invoice")
                    {
                        screen = "tblInvoiceNew;tblInvoiceTotal;tblInvoiceComission";
                    }
                    else
                    {
                        screen = "tblInvoiceNew;tblInvoiceTotalNoTax;tblInvoiceComission";
                    }
                }
            }

            if (!string.IsNullOrEmpty(getInvoice))
            {
                if (getInvoice == "Tax Invoice")
                {
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotal";
                    screen = "tblInvoiceNew;tblInvoiceTotal;tblInvoiceComission";
                }
                else
                {
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotalNoTax";
                    screen = "tblInvoiceNew;tblInvoiceTotalNoTax;tblInvoiceComission";
                }
            }
            else if (!string.IsNullOrEmpty(getUrl))
            {
                DataTable dt = db.readData("SELECT invo_Type FROM tblInvoice WHERE invo_InvoiceID=" + getUrl);
                if (dt.Rows.Count > 0)
                {
                    string tax = dt.Rows[0]["invo_Type"].ToString();

                    if (tax == "invTaxInvoice")
                    {
                        //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotal";
                        screen = "tblInvoiceNew;tblInvoiceTotal;tblInvoiceComission";
                    }
                    else
                    {
                        //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotalNoTax";
                        screen = "tblInvoiceNew;tblInvoiceTotalNoTax;tblInvoiceComission";
                    }
                }
            }

            //if (getInvoice == "invTaxInvoice")
            //{
            //    screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotal";
            //}
            //else
            //    screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotalNoTax";
        }

        string loadScreen(string eid, sapi.sapi.recordMode mode)
        {

            string re = "";
            string list = "";
            string topTitle = "";
            string InvoiceIDConverted = "";
            DataTable tblTop = null;
            cls.Mode = mode;
            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and init_InvoiceID = " + eid, cPage: -1) +
                       "</div>";
                sapi.defaultValue.add("invo_Status", "New");
                if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                {
                    DataTable tbl = db.readData("Select * from tblOpportunity Where oppo_Deleted is null and oppo_OpportunityID = " + url.Get("oppo_opportunityid"));
                    foreach (DataRow row in tbl.Rows)
                    {
                        sapi.defaultValue.add("invo_OpportunityID", url.Get("oppo_opportunityid"));
                        sapi.defaultValue.add("invo_CustomerID", row["oppo_CustomerID"].ToString());
                    }
                }
                if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                {
                    DataTable tbl = db.readData("SELECT * FROM tblCustomer WHERE cust_Deleted IS NULL AND cust_CustomerID = " + url.Get("cust_customerid"));
                    foreach (DataRow row in tbl.Rows)
                    {
                        sapi.defaultValue.add("invo_Type", row["cust_InvoiceType"].ToString());
                    }
                }
                //--tmp add type ;; -need to optimize code 
                string invoiceType = url.Get("invoice");
                if (!string.IsNullOrEmpty(invoiceType))
                {
                    string note = "";
                    if (invoiceType == "Invoice")
                    {
                        sapi.defaultValue.add("invo_Type".ToLower(), "Invoice");
                        note = "1. វិក័យប័ត្រនេះ មិនរួមបញ្ចូលតម្លៃពន្ធអាករទាំងអស់។      This invoice is exclusive of all taxes.   2. វិក័យប័ត្រនេះ មិនអាចប្រើសម្រាប់ក្នុងគោលបំណងបង់ពន្ធបានទេ។     This invoice can not use for tax return purpose.  3. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are not responsible for any bank service charges.  4. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is not refundable.  5. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.     វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.    2. បង់ប្រាក់រឺ​ផ្ទេរប្រាក់តាមធនាគារ/Bank Deposit or Bank Transfer :    Bank name: ACLEDA HQ   Name: Sop Phat   Account number: 00010057135912  Swift:  ACLBKHPP    Bank name: ABA Bank   Name: Sop Phat  Account number: 000157424  Swift: ABAAKHPP    ";
                    }
                    else if (invoiceType == "Commercial Invoice")
                    {
                        sapi.defaultValue.add("invo_Type".ToLower(), "Commercial Invoice");
                        note = "  សម្គាល់/Note:    1. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are NOT responsible for any bank service charges.  2. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is NOT refundable.  3. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.       វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.  2. បង់ប្រាក់ រឺផ្ទេរប្រាក់តាមធនាគារ      Bank Deposit or Bank Transfer :                           Bank name: ABA Bank                                      Name: MAKING IT EASY CO LTD                                     Account number: 000297675                                     SWIFT: ABAAKHPP";
                    }
                    else if (invoiceType == "Tax Invoice")
                    {
                        sapi.defaultValue.add("invo_Type".ToLower(), "Tax Invoice");
                        note = "សម្គាល់/Note:    1. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are NOT responsible for any bank service charges.  2. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is NOT refundable.  3. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.       វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.  2. បង់ប្រាក់ រឺផ្ទេរប្រាក់តាមធនាគារ      Bank Deposit or Bank Transfer :                           Bank name: ABA Bank                                      Name: MAKING IT EASY CO LTD                                     Account number: 000297675                                     SWIFT: ABAAKHPP  ";
                    }
                    sapi.defaultValue.add("invo_Note".ToLower(), note);
                }

                //sapi.defaultValue.add("invo_Remark".ToLower(), "test");

                //sapi.Buttons.add("Add Item", "plus", "warning",
                //                            "addItem(" + eid + ");");

            }

            if (mode == sapi.sapi.recordMode.View)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and init_InvoiceID = " + eid, cPage: -1, assc: 2) +
                       "</div>";
                //Hide button delete because Invoice can't delete
                cls.hideDelete = true;

                if (!string.IsNullOrEmpty(url.Get("invo_invoiceid")))
                {
                    DataTable dt = db.readData("SELECT * FROM tblInvoiceItem WHERE init_Deleted IS NULL AND init_IsConvert = 'Yes' AND init_InvoiceID = " + url.Get("invo_invoiceid"));
                    foreach (DataRow dr in dt.Rows)
                    {
                        InvoiceIDConverted += dr["init_InvoiceItemID"] + ",";
                    }
                }

                string compID = db.readData("invo_CustomerID", "SELECT invo_CustomerID FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_InvoiceID =" + db.sqlStr(url.Get("invo_invoiceid")));
                sapi.Buttons.add("Company", "arrow-left", "info", "ViewCustomer(" + compID + ")");

                //put button find our self 
                string id= url.Get("invo_invoiceid").ToString();
                DataTable tb = db.readData("SELECT * FROM tblInvoice WHERE invo_InvoiceID = " + id);

                sapi.Buttons.add("New Project", "plus", "warning", "window.location='../opportunity/opportunity.aspx?invo_invoiceid=" + eid + "'", "I", "tblOpportunity");
                sapi.Buttons.add("Find", "search", "success", "window.location = '../invoice/invoiceList.aspx?invoice=" + tb.Rows[0]["invo_Type"] + "';");
                if (!string.IsNullOrEmpty(tb.Rows[0]["invo_QuotationID"].ToString().Trim()))
                    sapi.Buttons.add("View Quotation", "loop2", "info", "window.location = '../quotation/quotation.aspx?quot_quotationid=" + tb.Rows[0]["invo_QuotationID"] + "';");

                if (eid != "0")
                {
                    tblData = db.readData("Select * from tblInvoice WHere invo_Deleted is null and invo_InvoiceID=" + eid);
                    foreach (DataRow row in tblData.Rows)
                    {

                        if (row["invo_Status"].ToString().ToLower() == "completed" ||
                            row["invo_Status"].ToString().ToLower() == "void")
                        {
                            cls.hideDelete = true;
                            if (row["invo_Status"].ToString().ToLower() == "void")
                                cls.hideEdit = true;


                        }
                        if (!string.IsNullOrEmpty(row["invo_OpportunityID"].ToString()))
                        {
                            sapi.Buttons.add("Opportunity", "arrow-left", "info",
                                        "window.location = '../opportunity/invoiceList.aspx?oppo_opportunityid=" + row["invo_OpportunityID"].ToString() + "';");
                        }
                        if (row["invo_Status"].ToString().ToLower() != "void")
                        {
                            if (db.cNum(row["invo_Balance"].ToString().ToLower()) > 0)
                            {
                                sapi.Buttons.add("Payment", "dollar2", "warning",
                                                "payment(" + eid + ");", "I", "tblInvoicePayment");

                                sapi.Buttons.add("Apply CN", "dollar2", "warning",
                                                "applyCN(" + eid + ");", "I", "tblInvoicePayment");
                                //cls.hideEdit = true;
                            }

                            //sapi.Buttons.add("Return", "undo", "info", "returnInv(" + eid + ")", "I", "tblCreditNote");


                            //sapi.Buttons.add("Payment", "dollar2", "warning",
                            //                "payment(" + eid + ");", "I", "tblInvoicePayment");


                            if (db.cNum(row["invo_PaidAmount"].ToString()) > 0 ||
                                db.cNum(row["invo_CreditNote"].ToString()) > 0)
                            {
                                cls.hideEdit = true;
                            }

                            //if (db.cNum(row["invo_PaidAmount"].ToString()) == 0 &&
                            //    db.cNum(row["invo_CreditNote"].ToString()) == 0)
                            //{
                            //    sapi.Buttons.add("Void", "cancel", "danger", "voidInv(" + eid + ")", "D", "tblInvoice");
                            //}

                            sapi.Buttons.add("Print", "print", "success", "printInv(" + eid + ")");
                            DataTable dtConverted = db.readData("SELECT * FROM tblInvoiceItem WHERE init_Deleted IS NULL AND init_InvoiceID = " + url.Get("invo_invoiceid"));
                            bool b = true;
                            foreach (DataRow dr in dtConverted.Rows)
                            {
                                if (dr["init_IsConvert"].ToString() != "Yes")
                                {
                                    b = false;
                                    break;
                                }
                            }
                            if (!b)
                                sapi.Buttons.add("Convert to Project", "medal", "", "convertToOpportunity()");
                        }

                    }
                }
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                sapi.readOnlyField.add("invo_WarehouseID");
                sapi.readOnlyField.add("init_WarehouseID");
                //sapi.readOnlyField2.add("init_WarehouseID");
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and init_InvoiceID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                DataTable dt = db.readData("SELECT invo_Type FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_InvoiceID=" + eid);
                string invoiceType = null;
                if (dt.Rows.Count > 0)
                    invoiceType = dt.Rows[0]["invo_Type"].ToString();


                //--some place need to change and optimize becos no time need to rush
                if (invoiceType == "Invoice")
                {
                    topTitle = "Invoice";
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotalNoTax";
                    screen = "tblInvoiceNew;tblInvoiceTotalNoTax;tblInvoiceComission";
                    //tblTop = db.readData("SELECT invo_Name,cust_Name FROM tblInvoice INNER JOIN tblCustomer ON cust_CustomerID = invo_CustomerID WHERE invo_InvoiceID = " + eid);
                }
                else if (invoiceType == "Commercial Invoice")
                {
                    topTitle = "Commercial Invoice";
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotalNoTax";
                    screen = "tblInvoiceNew;tblInvoiceTotalNoTax;tblInvoiceComission";
                    //tblTop = db.readData("SELECT invo_Name,cust_Name FROM tblInvoice INNER JOIN tblCustomer ON cust_CustomerID = invo_CustomerID WHERE invo_InvoiceID = " + eid);
                }
                else if (invoiceType == "Tax Invoice")
                {
                    topTitle = "Tax Invoice";
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotal";
                    screen = "tblInvoiceNew;tblInvoiceTotal;tblInvoiceComission";
                    //screen += "tblInvoiceTotal";
                    //tblTop = db.readData("SELECT invo_Name,cust_Name FROM tblInvoice INNER JOIN tblCustomer ON cust_CustomerID = invo_CustomerID WHERE invo_InvoiceID = " + eid);
                }
                tblTop = db.readData("SELECT invo_Name,cust_Name FROM tblInvoice INNER JOIN tblCustomer ON cust_CustomerID = invo_CustomerID WHERE invo_InvoiceID = " + eid);
            }
            string res = "<input id='InvoiceIDConverted' type='hidden' value=" + InvoiceIDConverted + ">";

            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);
            re = re + list + res;
            return re;
        }

        string saveRecord()
        {
            string re = "";
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");
            clsGlobal c = new clsGlobal();

            db.beginTran();
            if (!vals.ContainsKey("invo_invoiceid"))
            {
                aVal.Add("invo_WorkflowID", "6");
                aVal.Add("invo_WorkflowItemID", "12");
            }
            if (vals.ContainsKey("invo_status"))
                vals["invo_status"] = "completed";

            if (vals.ContainsKey("invo_MobileSaleID".ToLower()))
            {
                re = db.readData("mbsl_CheckIn", "Select mbsl_CheckIn From tblMobileSale " +
                    " Where mbsl_Deleted is null and mbsl_MobileSaleID = " + vals["invo_MobileSaleID".ToLower()]);
                if (!string.IsNullOrEmpty(re))
                {
                    tblResult.Rows[0]["status"] = "error";
                    tblResult.Rows[0]["msg"] = "<h3>Mobile Sale is already Completed !</h3>";
                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                    return re;
                }
            }

            if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                aVal["invo_OpportunityID".ToLower()] = url.Get("oppo_opportunityid");

            if (vals.ContainsKey("invo_customerid")) //dictionary use ContainsKey for check null or not
            {
                DataTable dt = db.readData("SELECT * FROM tblCustomer WHERE cust_CustomerID=" + vals["invo_customerid"]);
                if (dt.Rows.Count > 0)
                {
                    vals["invo_vattin"] = dt.Rows[0]["cust_VATTIN"].ToString();
                    vals["invo_Code".ToLower()] = dt.Rows[0]["cust_Code"].ToString();
                }
            }

            var type = url.Get("invoice");
            if (!string.IsNullOrEmpty(type))
                aVal.Add("invo_type", type);


            //if (vals.ContainsKey("invo_VAT")) 
            //{
            //    double vatAmount = db.cNum(vals.ContainsKey("invo_VAT").ToString());
            //    vals["invo_VATAmount"] = dt.Rows[0]["cust_VATTIN"].ToString();

            //}

            //--tmp no time ; just need to simplify the thing and rush
            if (!string.IsNullOrEmpty(url.Get("invo_invoiceid")))
            {
                var invoiceType = db.readData("invo_Type", "SELECT invo_Type FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_InvoiceID = " + db.sqlStr(url.Get("invo_invoiceid")));
                if (invoiceType == "Tax Invoice")
                {
                    //screen = "tblInvoiceNew;tblInvoiceCustomerInfo;tblInvoiceTotal";
                    screen = "tblInvoiceNew;tblInvoiceTotal;tblInvoiceComission";
                }
            }

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {
                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;

                    if (!vals.ContainsKey("invo_invoiceid"))
                    {
                        if (vals.ContainsKey("invo_customerid"))
                        {
                            if (!string.IsNullOrEmpty(vals["invo_customerid"]))
                            {
                                var tmp = db.execData("Update tblCustomer Set cust_Type='Customer',cust_LastTransDate=GETDATE() Where /*cust_Type='Lead' and*/ cust_CustomerID=" +
                                    vals["invo_customerid"].ToString());
                                if (tmp != "ok")
                                {
                                    db.rollback();

                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = tmp;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                    return re;
                                }
                            }
                        }
                    }

                    double qty = 0;
                    if (Request.Form.GetValues("N") != null)
                    {
                        string strErr = "";
                        int n = Request.Form.GetValues("N").Length;
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {

                            }
                            else
                            {
                                qty = 0;
                                for (int st1 = 0; st1 < n; st1++)
                                {
                                    if (vals["txtdel" + st1] == "")
                                    {
                                        //bool isWH = false;
                                        //if (vals.ContainsKey("init_WarehouseID".ToLower() + st))
                                        //{
                                        //    if (vals["init_WarehouseID".ToLower() + st] == vals["init_WarehouseID".ToLower() + st1])
                                        //        isWH = true;
                                        //}
                                        //if (vals["init_ItemID".ToLower() + st] == vals["init_ItemID".ToLower() + st1] && isWH)
                                        //{
                                        //    qty = qty + db.cNum(vals["init_Qty".ToLower() + st1]);
                                        //}
                                    }
                                }

                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                //string wh = vals["invo_WarehouseID".ToLower()];
                                //if (v.ContainsKey("init_WarehouseID".ToLower()))
                                //    if (!string.IsNullOrEmpty(v["init_WarehouseID".ToLower()]))
                                //    {
                                //        wh = v["init_WarehouseID".ToLower()];
                                //    }

                                //strErr = strErr + c.stockVerification(db, cls,
                                //    v["init_ItemID".ToLower()], qty,
                                //    //vals["invo_WarehouseID".ToLower()],
                                //    wh,
                                //    (v.ContainsKey("init_InvoiceItemID".ToLower()) ? v["init_InvoiceItemID".ToLower()] : ""));
                            }
                        }

                        if (strErr.Length > 0)
                        {
                            db.rollback();

                            tblResult.Rows[0]["status"] = "error";
                            tblResult.Rows[0]["msg"] = "<h3>Items out of stock : </h3><hr class='thin bg-grayLighter'/><br/>" + strErr;
                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                            return re;
                        }

                        /*
                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {

                            }else
                            {

                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                double exQty = 0;

                                if (v.ContainsKey("init_invoiceitemid"))
                                {
                                    if (!string.IsNullOrEmpty(v["init_invoiceitemid"]))
                                        exQty = db.cNum(db.readData("init_Qty",
                                            "Select init_Qty From tblInvoiceItem Where init_InvoiceItemID=" + v["init_invoiceitemid"]));
                                }

                                string strErr = "";
                                DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                    " Where itwh_ItemID = " + db.sqlStr(v["init_ItemID".ToLower()]) +
                                    " and itwh_WarehouseID = " + db.sqlStr(vals["invo_WarehouseID".ToLower()]));
                                foreach (DataRow rowItem in tblItem.Rows)
                                {

                                    qty = 0;
                                    int n = Request.Form.GetValues("N").Length;
                                    for (int st1 = 0; st1 < n; st1++)
                                    {
                                        if (vals["txtdel" + st1] == "")
                                        {

                                            if (vals["init_ItemID".ToLower() + st] == vals["init_ItemID".ToLower() + st1])
                                            {
                                                qty = qty + db.cNum(vals["init_Qty".ToLower() + st1]);
                                            }
                                        }
                                    }
                                    qty = qty - exQty;
                                    if (rowItem["item_isSet"].ToString() == "Y")
                                    {
                                        DataTable tblItemSet = db.readData("Select * from vSubItem " +
                                            " Where sitm_Deleted is null and sitm_ItemID=" + v["init_ItemID".ToLower()]);
                                        string prestrErr = "";
                                        foreach (DataRow rowItemSet in tblItemSet.Rows)
                                        {
                                            double initQty = 0;
                                            if (rowItemSet["item_IsStock"].ToString() == "Y")
                                            {
                                                double itemwhQty2 = db.cNum(db.readData("itwh_Qty", "Select itwh_Qty from vItemWarehouse " +
                                                    " Where itwh_ItemID = " + db.sqlStr(rowItemSet["item_itemID"].ToString()) +
                                                    " and itwh_WarehouseID = " + db.sqlStr(vals["invo_WarehouseID".ToLower()])));

                                                initQty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                if (initQty > itemwhQty2)
                                                {
                                                    prestrErr = prestrErr +
                                                        "&nbsp;&nbsp;+ " + rowItemSet["item_Name"] + "(" +
                                                        initQty + " / " +
                                                        itemwhQty2 + ")" +
                                                        "<br/>";
                                                }
                                            }
                                        }
                                        if (prestrErr.Length > 0)
                                        {
                                            strErr = strErr + " <h4>- " + rowItem["item_Name"] + " : </h4><br/>" + prestrErr +
                                                "<hr class='thin bg-grayLighter'/>";
                                        }
                                    }
                                    else
                                    {
                                        if (rowItem["item_IsStock"].ToString() == "Y")
                                        {
                                            if (qty > db.cNum(rowItem["itwh_Qty"].ToString()))
                                            {
                                                strErr = strErr + " <h4>- " + rowItem["item_Name"] + "(" +
                                                    qty.ToString() + " / " +
                                                    db.cNum(rowItem["itwh_Qty"].ToString()) + ")</h3>" + "<hr class='thin bg-grayLighter'/>";
                                            }
                                        }
                                    }
                                }

                                if (strErr.Length > 0)
                                {
                                    db.rollback();

                                    tblResult.Rows[0]["status"] = "error";
                                    tblResult.Rows[0]["msg"] = "<h3>Items out of stock : </h3><hr class='thin bg-grayLighter'/><br/>" + strErr;
                                    re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                    return re;
                                }
                            }
                        }
                        */


                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            aVal.Clear();
                            aVal.Add("init_InvoiceID", hid);

                            if (vals["txtdel" + st] != "")
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                cls.delRecord(screenItem, vals["init_InvoiceItemID".ToLower() + st], db);
                                if (!string.IsNullOrEmpty(v["init_InvoiceItemID".ToLower()]))
                                {
                                    string wh;// = vals["invo_WarehouseID".ToLower()];
                                    if (!vals.ContainsKey("invo_WarehouseID".ToLower()))
                                        wh = "";
                                    else
                                        wh = vals["invo_WarehouseID".ToLower()];
                                    if (v.ContainsKey("init_WarehouseID".ToLower()))
                                        if (!string.IsNullOrEmpty(v["init_WarehouseID".ToLower()]))
                                        {
                                            wh = v["init_WarehouseID".ToLower()];
                                        }
                                    c.stockDeduction(db,
                                            vals["init_ItemID".ToLower() + st],
                                            wh,
                                            db.cNum(db.readData("init_Qty", "Select init_Qty From tblInvoiceItem Where init_InvoiceItemID = " + vals["init_InvoiceItemID".ToLower() + st])),
                                            vals["init_InvoiceItemID".ToLower() + st],
                                            v.ContainsKey("init_InvoiceItemID".ToLower()) ? "" : v["init_InvoiceItemID".ToLower()], true);
                                }
                            }
                            else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double exQty = 0;
                                double init_RQty = 0;
                                string init_DiscType = "";
                                double init_Discount = 0;
                                double init_DiscountAmount = 0;

                                if (v.ContainsKey("init_InvoiceItemID".ToLower()))
                                {
                                    if (!string.IsNullOrEmpty(v["init_InvoiceItemID".ToLower()]))
                                    {
                                        DataTable tblInvItem = db.readData("Select init_Qty,init_RQty From tblInvoiceItem Where init_InvoiceItemID=" + v["init_InvoiceItemID".ToLower()]);
                                        foreach (DataRow rowInvItem in tblInvItem.Rows)
                                        {
                                            exQty = db.cNum(rowInvItem["init_Qty"].ToString());
                                            init_RQty = db.cNum(rowInvItem["init_RQty"].ToString());
                                        }
                                    }
                                }

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                if (v.ContainsKey("init_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["init_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("init_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["init_Price".ToLower()]);
                                }
                                quit_Qty = quit_Qty - exQty;
                                if (v.ContainsKey("init_Total".ToLower()))
                                {
                                    v["init_Total".ToLower()] = (quit_Price * db.cNum(v["init_Qty".ToLower()])).ToString();
                                }
                                if (v.ContainsKey("init_Disc".ToLower()))
                                {
                                    init_Discount = db.cNum(v["init_Disc".ToLower()]);
                                }
                                if (v.ContainsKey("init_Discount".ToLower()))
                                {
                                    init_DiscType = v["init_Discount".ToLower()];
                                    if (init_DiscType == "P")
                                    {
                                        init_DiscountAmount = (double.Parse(v["init_Total".ToLower()]) * init_Discount) / 100;
                                        //v["init_Discount".ToLower()] = "%";
                                    }
                                    else if (init_DiscType == "A")
                                    {
                                        init_DiscountAmount = init_Discount;
                                        //v["init_Discount".ToLower()] = "$";
                                    }
                                    else
                                    {
                                        init_DiscountAmount = 0;
                                    }
                                }

                                v["init_Total".ToLower()] = (double.Parse(v["init_Total".ToLower()]) - init_DiscountAmount).ToString();

                                if (!v.ContainsKey("init_invoiceitemid".ToLower() + st))
                                {
                                    aVal.Add("init_BQty", (quit_Qty + exQty - init_RQty).ToString());
                                }
                                aVal.Add("init_RPrice", quit_Price.ToString());

                                //double init_Cost = db.cNum(db.readData("item_Cost", "Select item_Cost From tblItem " +
                                //    " Where item_ItemID = " + v["init_ItemID".ToLower()]));
                                //aVal.Add("init_Cost", init_Cost.ToString());
                                
                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
                                    }
                                    else
                                    {
                                        if (db.readData("sett_CustomerlastPrice", "Select sett_CustomerlastPrice from sys_setting ") == "Y")
                                        {
                                            DataTable tblCustPrice = db.readData("Select * from tblCustomerPrice Where cusp_CustomerID = " +
                                                "" + vals["invo_customerid".ToLower()] + " and cusp_ItemID = " + vals["init_ItemID".ToLower() + st]);
                                            if (tblCustPrice.Rows.Count > 0)
                                            {
                                                db.execData("Update tblCustomerPrice Set cusp_Price = " + db.cNum(vals["init_Price".ToLower() + st]) +
                                                    " Where cusp_CustomerID = " + vals["invo_customerid".ToLower()] +
                                                    " and cusp_ItemID = " + vals["init_ItemID".ToLower() + st]);
                                            }
                                            else
                                            {
                                                db.execData("Insert into tblCustomerPrice(cusp_Price,cusp_CustomerID,cusp_ItemID) VALUES(" +
                                                    db.cNum(vals["init_Price".ToLower() + st]) +
                                                    "," + vals["invo_customerid".ToLower()] +
                                                    "," + vals["init_ItemID".ToLower() + st] + ")");
                                            }
                                        }

                                        string id = (string)str.tbl[0].msg;

                                        //string wh = vals["invo_WarehouseID".ToLower()];
                                        
                                        //if (v.ContainsKey("init_WarehouseID".ToLower()))
                                        //    if (!string.IsNullOrEmpty(v["init_WarehouseID".ToLower()]))
                                        //    {
                                        //        wh = v["init_WarehouseID".ToLower()];
                                        //    }
                                        //string tmp = c.stockDeduction(db,
                                        //    vals["init_ItemID".ToLower() + st],
                                        //    wh,
                                        //    quit_Qty, id,
                                        //    v.ContainsKey("init_InvoiceItemID".ToLower()) ? "" : v["init_InvoiceItemID".ToLower()]);
                                        //if (!string.IsNullOrEmpty(tmp))
                                        //    return tmp;
                                        /*
                                        // stock Deduction

                                        DataTable tblItem = db.readData("Select * from vItemWarehouse " +
                                            " Where itwh_ItemID = " + db.sqlStr(vals["init_ItemID".ToLower() + st]) +
                                            " and itwh_WarehouseID = " + db.sqlStr(vals["invo_WarehouseID".ToLower()].ToString()));
                                        foreach (DataRow rowItem in tblItem.Rows)
                                        {
                                            if (rowItem["item_IsStock"].ToString() == "Y" || rowItem["item_isSet"].ToString() == "Y")
                                            {
                                                DataTable tblItemSet = db.readData("Select sitm_Qty inid_Qty,sitm_ItemUsedID inid_ItemID from tblSubItem Where sitm_Deleted is null and sitm_ItemID=" +
                                                    vals["init_ItemID".ToLower() + st]);
                                                if (vals.ContainsKey("init_InvoiceItemID".ToLower() + st))
                                                {
                                                    if (!string.IsNullOrEmpty(vals["init_InvoiceItemID".ToLower() + st]))
                                                        tblItemSet = db.readData("Select * from tblInvoiceItemDetail " +
                                                            " Where inid_Deleted is null and inid_InvoiceItemID=" +
                                                            vals["init_InvoiceItemID".ToLower() + st]);
                                                }
                                                if (tblItemSet.Rows.Count <= 0)
                                                {
                                                    tblItemSet.Rows.Add();
                                                }


                                                foreach (DataRow rowItemSet in tblItemSet.Rows)
                                                {

                                                    qty = quit_Qty;
                                                    string itemid = vals["init_ItemID".ToLower() + st];
                                                    if (rowItem["item_isSet"].ToString() == "Y")
                                                    {
                                                        //qty = qty * db.cNum(rowItemSet["sitm_Qty"].ToString());
                                                        //itemid = rowItemSet["sitm_ItemUsedID"].ToString();
                                                        qty = qty * db.cNum(rowItemSet["inid_Qty"].ToString());
                                                        itemid = rowItemSet["inid_ItemID"].ToString();
                                                    }

                                                    if (rowItem["item_isSet"].ToString() == "Y")
                                                    {
                                                        if (v.ContainsKey("init_InvoiceItemID".ToLower()))
                                                            if (string.IsNullOrEmpty(v["init_InvoiceItemID".ToLower()]))
                                                            {
                                                                db.execData("Insert into tblInvoiceItemDetail(inid_InvoiceItemID,inid_ItemID,inid_Qty) VALUES(" +
                                                                    db.sqlStr(id) + "," + db.sqlStr(itemid) + "," +
                                                                    //(db.cNum(v["init_Qty".ToLower()]) - exQty) +
                                                                    qty +
                                                                    ")");
                                                            }
                                                    }

                                                    string tmp = db.execData("Update tblItemWarehouse Set " +
                                                            " itwh_Qty = isNULL(itwh_Qty,0) - " + qty +
                                                            " where itwh_WarehouseID = " + db.sqlStr(vals["invo_WarehouseID".ToLower()].ToString()) +
                                                            " and itwh_ItemID = " + db.sqlStr(itemid)
                                                            );
                                                    if (tmp != "ok")
                                                    {
                                                        db.rollback();

                                                        tblResult.Rows[0]["status"] = "error";
                                                        tblResult.Rows[0]["msg"] = tmp;
                                                        re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                                        return re;
                                                    }
                                                    else
                                                    {

                                                        tmp = db.execData("Declare @tot decimal(18,6) " +
                                                            " Declare @tot2 decimal(18,6) " +
                                                            " Select " +
                                                            " @tot2 = SUM(isNull(itwh_Qty,0)) " +
                                                            " from tblItemWarehouse where itwh_ItemID = " + db.sqlStr(itemid) +
                                                            " update tblItem set  " +
                                                            " item_Qty = @tot2 " +
                                                            " where item_ItemID = " + db.sqlStr(itemid)
                                                        );
                                                        if (tmp != "ok")
                                                        {
                                                            db.rollback();

                                                            tblResult.Rows[0]["status"] = "error";
                                                            tblResult.Rows[0]["msg"] = tmp;
                                                            re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                                            return re;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        // End of stock Deduction
                                        */
                                    }
                                }

                                if (str.error != null)
                                {
                                    db.rollback();
                                    return re2;
                                }
                            }
                        }
                    }
                    //c.invoiceTotal(hid, db);
                    invoiceTotal(hid, db);
                    db.commit();
                }
            }
            return re;
        }

        void invoiceTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            string invo_ComissionType = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;
            double invo_VAT = 0;
            double invo_VATAmount = 0;
            double invo_ComissionAmount = 0;

            double invo_Deposit = 0;

            DataTable tbl = db.readData("Select * From tblInvoice " +
                " Where invo_Deleted is null and invo_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["invo_Discount"].ToString();
                invo_Disc = db.cNum(row["invo_Disc"].ToString());
                invo_IsTax = db.cNum(row["invo_IsTax"].ToString());
                invo_Deposit = db.cNum(row["invo_Deposit"].ToString());
                invo_VAT = db.cNum(row["invo_VAT"].ToString());
                invo_ComissionType = row["invo_ComissionType"].ToString();
            }

            tbl = db.readData("Select * From tblInvoiceItem Where init_Deleted is null and init_InvoiceID = " + eid);

            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal += db.cNum(row["init_Total"].ToString());
            }
            if (invo_Discount == "P")
            {
                invo_DiscountAmount = (invo_SubTotal * invo_Disc / 100);
            }
            else
            {
                invo_DiscountAmount = invo_Disc;
            }
            //Tax Calculation
            invo_VATAmount = (invo_SubTotal * invo_VAT) / 100;
            invo_Total = invo_SubTotal - invo_DiscountAmount;
            invo_Tax = (invo_SubTotal * invo_IsTax) / 100;
            invo_GTotal = invo_Total + invo_VATAmount;
            double balance = invo_GTotal - invo_Deposit;

            if (string.IsNullOrEmpty(invo_ComissionType))
                invo_ComissionAmount = 0;
            else if (invo_ComissionType == "Employee (3%)")
                invo_ComissionAmount = invo_GTotal * 0.03;
            else if(invo_ComissionType == "Other (5%)")
                invo_ComissionAmount = invo_GTotal * 0.05;


            db.execData("Update tblInvoice Set " +
                " invo_SubTotal = " + invo_SubTotal +
                ",invo_DiscountAmount = " + invo_DiscountAmount +
                ",invo_Total = " + invo_Total +
                ",invo_GTotal = " + invo_GTotal +
                ",invo_Tax = " + invo_Tax +
                ",invo_VATAmount = " + invo_VATAmount +
                ",invo_Balance = " + balance +
                ",invo_comissionAmount = " + invo_ComissionAmount +
                " Where invo_InvoiceID = " + eid
                );
        }
    }
}