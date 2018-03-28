using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
using System.Text;

namespace X_CRM.opportunity
{
    public partial class ajax : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (db.connect())
                {
                    
                    if (Request.Form["app"].ToString() == "convertToInvoice")
                        ConvertToInvoice();

                    if (Request.Form["app"].ToString() == "loadFormInput")
                        LoadFormInput();

                    if (Request.Form["app"].ToString() == "getQuotInfo")
                        Opportunity();
                }

            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.close();
                cls.endRequest();
            }
        }
       
        private void Opportunity()
        {
            try
            {
                string sql = "SELECT  cust_Phone,cust_Email,cust_Address,cust_VATTIN FROM tblCustomer WHERe cust_Deleted IS NULL AND cust_CustomerID = " + db.sqlStr(Request.Form["compID"]);
                Response.Write(db.tblToJson(db.readData(sql)));
                db.close();
            }
            catch (Exception ex)
            {

                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void LoadFormInput()
        {
            try
            {
                cls.Mode = global::sapi.sapi.recordMode.New;
                string re = cls.loadScreen(db, "tblOpportunityInput", "frmtblOpportunityNew", ref tblData, "0");
                Response.Write(re);
                db.close();
                cls.endRequest();
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }

        }

        private void ConvertToInvoice()
        {
            try
            {
                string re = "";
                string invoiceID = "";
                Dictionary <string, string> aVal = new Dictionary<string, string>();
                db.beginTran();
                string oppoSQL = "SELECT *  FROM tblOpportunity WHERE oppo_Deleted IS NULL AND oppo_OpportunityID = " + db.sqlStr(Request.Form["oppoID"]);
                DataTable tblOppo = db.readData(oppoSQL);
                string oppoID = tblOppo.Rows[0]["oppo_OpportunityID"].ToString();
                string quoteID = tblOppo.Rows[0]["oppo_QuotationID"].ToString();
                string oppoRunningID = tblOppo.Rows[0]["oppo_name"].ToString();
                string invoiceType  = tblOppo.Rows[0]["oppo_InvoiceType"].ToString();
                string quoteSQL = "SELECT * FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID = " + quoteID;
                DataTable tblQuote = db.readData(quoteSQL);
                foreach (DataRow row in tblQuote.Rows)
                {
                    aVal["invo_OpportunityID".ToLower()] = oppoID;
                    aVal["invo_Name".ToLower()] = "";
                    aVal["invo_Date".ToLower()] = DateTime.Now.ToString("yyyy-MM-dd");
                    aVal["invo_Remark".ToLower()] = "Invoice converted from Project ID: " + oppoRunningID;
                    aVal["invo_CustomerID".ToLower()] = row["quot_CustomerID"].ToString();

                    aVal["invo_Code".ToLower()] = row["quot_Code"].ToString();
                    aVal["invo_VATTIN".ToLower()] = row["quot_VATTIN"].ToString();
                    aVal["invo_Phone".ToLower()] = row["quot_Phone"].ToString();
                    aVal["invo_Email".ToLower()] = row["quot_Email"].ToString();
                    aVal["invo_Address".ToLower()] = row["quot_Address"].ToString();

                    aVal["invo_VAT".ToLower()] = row["quot_VAT"].ToString();
                    aVal["invo_VATAmount".ToLower()] = row["quot_VATAmount"].ToString();
                    aVal["invo_Disc".ToLower()] = row["quot_Disc"].ToString();
                    aVal["invo_Discount".ToLower()] = row["quot_Discount"].ToString();
                    aVal["invo_DiscountAmount".ToLower()] = row["quot_DiscountAmount"].ToString();
                    aVal["invo_SubTotal".ToLower()] = row["quot_SubTotal"].ToString();
                    aVal["invo_Total".ToLower()] = row["quot_Total"].ToString();
                    aVal["invo_GTotal".ToLower()] = row["quot_TotalVAT"].ToString();

                    aVal["invo_Type".ToLower()] = invoiceType;
                    aVal["invo_QuotationID".ToLower()] = row["quot_QuotationID"].ToString();


                    if (!string.IsNullOrEmpty(invoiceType))
                    {
                        string note = "";
                        if (invoiceType == "Invoice")
                        {
                            note = "1. វិក័យប័ត្រនេះ មិនរួមបញ្ចូលតម្លៃពន្ធអាករទាំងអស់។      This invoice is exclusive of all taxes.   2. វិក័យប័ត្រនេះ មិនអាចប្រើសម្រាប់ក្នុងគោលបំណងបង់ពន្ធបានទេ។     This invoice can not use for tax return purpose.  3. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are not responsible for any bank service charges.  4. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is not refundable.  5. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.     វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.    2. បង់ប្រាក់រឺ​ផ្ទេរប្រាក់តាមធនាគារ/Bank Deposit or Bank Transfer :    Bank name: ACLEDA HQ   Name: Sop Phat   Account number: 00010057135912  Swift:  ACLBKHPP    Bank name: ABA Bank   Name: Sop Phat  Account number: 000157424  Swift: ABAAKHPP    ";
                        }
                        else if (invoiceType == "CommercialInvoice")
                        {
                            
                            note = "  សម្គាល់/Note:    1. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are NOT responsible for any bank service charges.  2. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is NOT refundable.  3. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.       វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.  2. បង់ប្រាក់ រឺផ្ទេរប្រាក់តាមធនាគារ      Bank Deposit or Bank Transfer :                           Bank name: ABA Bank                                      Name: MAKING IT EASY CO LTD                                     Account number: 000297675                                     SWIFT: ABAAKHPP";
                        }
                        else if (invoiceType == "TaxInvoice")
                        {
                            note = "សម្គាល់/Note:    1. យើងមិនទទួលខុសត្រូវចំពោះ ថ្លៃសេវាកម្មផ្ទេរប្រាក់របស់ធនាគារទេ។      We are NOT responsible for any bank service charges.  2. ការទូទាត់នេះ មិនអាចប្តូរយកប្រាក់មកវិញទេ។       Payment is NOT refundable.  3. សូមធ្វើ​ការ​ទូទាត់វិក័យប័ត្រ ក្នុងរយៈពេលមួយ​ស​ប្តា​ហ៍។      Please settle the invoice within a week after the issued date.       វិធីទូទាត់សាច់ប្រាក់/Payment Procedures:  1. ធ្វើការបង់ប្រាក់ដោយផ្ទាល់ជាមួយ គណនេយ្យកររបស់យើងនៅការិយាល័យ។      Pay directly to our accountant at the office.  2. បង់ប្រាក់ រឺផ្ទេរប្រាក់តាមធនាគារ      Bank Deposit or Bank Transfer :                           Bank name: ABA Bank                                      Name: MAKING IT EASY CO LTD                                     Account number: 000297675                                     SWIFT: ABAAKHPP  ";
                        }
                        
                        aVal["invo_Note".ToLower()] = note;
                    }




                    string invoiceScreen = "tblInvoiceNew;tblInvoiceCustomerInfo;";
                    re = cls.saveRecord(invoiceScreen, vals, db, aVals: aVal, ignoreROF: true);

                    var str = JsonConvert.DeserializeObject<dynamic>(re);
                    
                    if (str.tbl != null)
                    {
                        if (str.tbl[0].status == "ok")
                        {
                            invoiceID = (string)str.tbl[0].msg;
                            SaveLineItem(invoiceID, quoteID);
                        }
                        else
                        {
                            throw new Exception("Error occur when saving record to db");
                        }
                    }
                    
                }
                string sqlUpdate = "UPDATE tblOpportunity SET oppo_Converted = 1 WHERE oppo_Deleted IS NULL AND oppo_OpportunityID =" + oppoID;
                db.execData(sqlUpdate);
                db.commit();
                Response.Write("ok" + invoiceID);
            }
            catch (Exception ex)
            {
                db.rollback();
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void SaveLineItem(string invoiceID, string quoteID)
        {
            string quoteItemSQL = "SELECT * FROM tblQuotationItem WHERE quit_Deleted IS NULL AND quit_QuotationID =" + quoteID;
            DataTable tlbQuoteItem = db.readData(quoteItemSQL);
            foreach(DataRow quoteItemRow in tlbQuoteItem.Rows)
            {
                Dictionary<string, string> aVal = new Dictionary<string, string>();

                aVal["init_InvoiceID".ToLower()] = invoiceID;
                aVal["init_Description".ToLower()] = quoteItemRow["quit_Description"].ToString();
                aVal["init_Qty".ToLower()] = quoteItemRow["quit_Qty"].ToString();
                aVal["init_Price".ToLower()] = quoteItemRow["quit_Price"].ToString();
                aVal["init_Total".ToLower()] = quoteItemRow["quit_Total"].ToString();

                aVal["init_QuotationItemID".ToLower()] = quoteItemRow["quit_QuotationItemID"].ToString();

                string re = cls.saveRecord("tblInvoiceItemNew", vals, db, aVals: aVal, ignoreROF: true);
                var str = JsonConvert.DeserializeObject<dynamic>(re);

                if (str.tbl[0].status == "ok")
                {
                    string invoiceItemID = (string)str.tbl[0].msg;
                    clsGlobal global = new clsGlobal();
                    global.invoiceTotal(invoiceID, db);

                    //UpdateOpportunity(oppoRow);
                }
                else
                {
                    throw new Exception("Error occur when saving record to db");
                }
            }

        }

        private void UpdateOpportunity(DataRow oppoRow)
        {
            string oppoID = oppoRow["oppo_OpportunityID"].ToString();
            string sql = "SELECT  ISNULL(SUM(ISNULL(invo_Total,0)),0) AS Total  FROM tblInvoice WHERE invo_Deleted IS NULL AND invo_OpportunityID = " + oppoID;
            DataTable tblOppo = db.readData(sql);

            double oppoAmount = db.cNum(oppoRow["oppo_Amount"].ToString());
            double totalInvoiceAmount = db.cNum(tblOppo.Rows[0]["Total"].ToString());

            double oppoPercetage = totalInvoiceAmount / oppoAmount * 100;

            sql = "UPDATE tblOpportunity SET oppo_ConvertToInvoiceAmount = " + totalInvoiceAmount + "," + 
                    " oppo_ConvertToInvoicePercentage = "  + oppoPercetage + 
                    " WHERE oppo_Deleted IS NULL AND  oppo_OpportunityID = " + oppoID;

            db.execData(sql);

        }
    }

    
}