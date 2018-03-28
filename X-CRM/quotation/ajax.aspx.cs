using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
using System.Globalization;

namespace X_CRM.quotation
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
                    if (Request.Form["app"].ToString() == "getQuotInfo")
                        GetCompInfo();
                    if (Request.Form["app"].ToString() == "getServiceType")
                        GetServiceType();
                    if (Request.Form["app"].ToString() == "Revise")
                        Revise();
                    if (Request.Form["app"].ToString() == "ViewReviseQuotation")
                        ViewReviseQuotation();
                    if (Request.Form["app"].ToString() == "convertToOpportunity")
                        Opportunity();
                    if (Request.Form["app"].ToString() == "ConvertToInvoice")
                        ConvertToInvoice();
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.close();
                cls.endRequest();
            }
        }

        private void ConvertToInvoice()
        {
            try
            {
                string[] quotItemID = Request.Form["ids[]"].ToString().Split(',');
                string quotID = Request.Form["quotID"].ToString();
                DataTable dt = new DataTable();

                Dictionary<string, string> vals = new Dictionary<string, string>();
                Dictionary<string, string> aval = new Dictionary<string, string>();

                string screen = "";

                string sql = "SELECT * FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID = " + quotID;
                int countIsHaveSomeItemConvert = int.Parse(db.readData("C","SELECT COUNT(*) AS C FROM tblQuotationItem WHERE quit_Deleted IS NULL AND quit_IsConvert='Yes' AND quit_QuotationID = " + quotID));
                dt = db.readData(sql);                
                string iDate = DateTime.Now.ToString("dd/MM/yyyy");
                string modeInvoice = "";

                string quot_Discount = "";
                double quot_Disc = 0;
                double quot_SubTotal = 0;
                double quot_DiscountAmount = 0;
                double quot_Total = 0;
                Double quot_VAT = 0;
                double quot_Deposit = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    aval.Add("invo_QuotationID".ToLower(), quotID);

                    vals.Add("invo_Date".ToLower(), iDate);
                    vals.Add("invo_Type".ToLower(), dr["quot_InvoiceType"].ToString());
                    vals.Add("invo_ServiceType".ToLower(), dr["quot_Type2"].ToString());
                    vals.Add("invo_CustomerID".ToLower(), dr["quot_CustomerID"].ToString());
                    vals.Add("invo_Attendance".ToLower(), dr["quot_Attendance"].ToString());
                    vals.Add("invo_Remark".ToLower(), dr["quot_Remark"].ToString());

                    vals.Add("invo_Discount".ToLower(), dr["quot_Discount"].ToString());
                    vals.Add("invo_Disc".ToLower(), dr["quot_Disc"].ToString());
                    vals.Add("invo_DiscountAmount".ToLower(), dr["quot_DiscountAmount"].ToString());
                    if (countIsHaveSomeItemConvert > 0)
                    {
                        vals.Add("invo_Deposit".ToLower(), "0");
                        quot_Deposit = 0;
                    }
                    else
                    {
                        vals.Add("invo_Deposit".ToLower(), dr["quot_Deposit"].ToString());
                        quot_Deposit = db.cNum(dr["quot_Deposit"].ToString());
                    }
                    vals.Add("invo_SubTotal".ToLower(), dr["quot_SubTotal"].ToString());

                    quot_Discount = dr["quot_Discount"].ToString();
                    quot_Disc = db.cNum(dr["quot_Disc"].ToString());
                    quot_VAT = db.cNum(dr["quot_VAT"].ToString());
                    //quot_Deposit = db.cNum(dr["quot_Deposit"].ToString());

                    if (dr["quot_InvoiceType"].ToString() == "Tax Invoice")
                    {
                        vals.Add("invo_VAT".ToLower(), dr["quot_VAT"].ToString());
                        vals.Add("invo_VATAmount".ToLower(), dr["quot_VATAmount"].ToString());
                        vals.Add("invo_Total".ToLower(), dr["quot_Total"].ToString());
                        vals.Add("invo_GTotal".ToLower(), dr["quot_TotalVAT"].ToString());
                        vals.Add("invo_Balance".ToLower(), dr["quot_Balance"].ToString());
                        screen = "tblInvoiceNew;tblInvoiceTotal";
                        modeInvoice = dr["quot_InvoiceType"].ToString();
                    }
                    else
                    {
                        double total = double.Parse(dr["quot_SubTotal"].ToString()) - double.Parse(dr["quot_DiscountAmount"].ToString());
                        double deposit = 0;
                        if (!string.IsNullOrEmpty(dr["quot_Deposit"].ToString().Trim()))
                            deposit = double.Parse(dr["quot_Deposit"].ToString());
                        double balance = total - deposit;
                        vals.Add("invo_Total".ToLower(), total.ToString());
                        vals.Add("invo_GTotal".ToLower(), total.ToString());
                        vals.Add("invo_Balance".ToLower(), balance.ToString());
                        screen = "tblInvoiceNew;tblInvoiceTotalNoTax";
                    }
                }

                string re = cls.saveRecord(screen, vals, db, aVals: aval, ignoreROF: true);
                var str = JsonConvert.DeserializeObject<dynamic>(re);
                string invoiceID = (string)str.tbl[0].msg;
                string dd = "";
              
                foreach (string b in quotItemID) {
                    sql = "SELECT * FROM tblQuotationItem WHERE quit_Deleted IS NULL AND quit_QuotationItemID = " + b;
                    dt = db.readData(sql);

                    foreach (DataRow dr in dt.Rows)
                    {
                        string quotItemId = dr["quit_IsConvert"].ToString();
                        if (quotItemId != "Yes")
                        {
                            vals.Clear();
                            aval.Clear();

                            aval.Add("init_InvoiceID".ToLower(), invoiceID);

                            vals.Add("init_ItemID".ToLower(), dr["quit_ItemID"].ToString());
                            vals.Add("init_Description".ToLower(), dr["quit_Description"].ToString());
                            vals.Add("init_PeriodProcessing".ToLower(), dr["quit_PeriodProcessing"].ToString());
                            vals.Add("init_Qty".ToLower(), dr["quit_Qty"].ToString());
                            vals.Add("init_Price".ToLower(), dr["quit_Price"].ToString());
                            vals.Add("init_Discount".ToLower(),dr["quit_Discount"].ToString());
                            vals.Add("init_Disc".ToLower(), dr["quit_Disc"].ToString());
                            vals.Add("init_DiscountAmount".ToLower(), dr["quit_DiscountAmount"].ToString());
                            vals.Add("init_SubTotal".ToLower(), dr["quit_SubTotal"].ToString());
                            vals.Add("init_Total".ToLower(), dr["quit_Total"].ToString());
                            vals.Add("init_Remark".ToLower(), dr["quit_Remark"].ToString());

                            quot_SubTotal += float.Parse(dr["quit_Total"].ToString());
                            
                            re = cls.saveRecord("tblInvoiceItemNew", vals, db, aVals: aval, ignoreROF: true);
                            str = JsonConvert.DeserializeObject<dynamic>(re);
                            if (str.tbl != null)
                            {
                               dd= db.execData("UPDATE tblQuotationItem SET quit_IsConvert = 'Yes' WHERE quit_QuotationItemID = " + b);
                            }
                        }
                    }
                }

                double quot_VATAmount = 0;
                double quot_GrandTotal = 0;
                double Balance = 0;
                if (modeInvoice == "Tax Invoice")
                {
                    if (quot_Discount == "P")
                    {
                        quot_DiscountAmount = (quot_SubTotal * quot_Disc / 100);
                    }
                    else
                    {
                        quot_DiscountAmount = quot_Disc;
                    }
                    quot_VATAmount = (quot_VAT / 100) * (quot_SubTotal);
                    quot_Total = quot_SubTotal - quot_DiscountAmount;
                    quot_GrandTotal = quot_Total + quot_VATAmount;

                    Balance = quot_GrandTotal - quot_Deposit;

                }
                else
                {
                    if (quot_Discount == "P")
                    {
                        quot_DiscountAmount = (quot_SubTotal * quot_Disc / 100);
                    }
                    else
                    {
                        quot_DiscountAmount = quot_Disc;
                    }
                    quot_Total = quot_SubTotal - quot_DiscountAmount;
                    quot_GrandTotal = quot_Total;

                    Balance = quot_GrandTotal - quot_Deposit;
                }

                db.execData("Update tblInvoice Set " +
                                " invo_SubTotal = " + quot_SubTotal +
                                ",invo_DiscountAmount = " + quot_DiscountAmount +
                                ",invo_Total = " + quot_Total +
                                ",invo_GTotal = " + quot_GrandTotal +
                                ",invo_VATAmount = " + quot_VATAmount +
                                ",invo_Balance = " + Balance +
                                " Where invo_InvoiceID = " + invoiceID
                                );

                if (str.tbl[0].status == "ok")
                    Response.Write("ok" + invoiceID);
                db.close();
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void Opportunity()
        {
            try
            {
                Dictionary<string, string> vals = new Dictionary<string, string>();
                Dictionary<string, string> aval = new Dictionary<string, string>();
                string sql = "SELECT * FROM tblQuotation WHERE quot_QuotationID = " + Request.Form["quotID"].ToString();
                string a = Request.Form["quotID"].ToString();
                DataTable dt = db.readData(sql);
                var dd = DateTime.Now.ToShortDateString();
                
                string iDate = DateTime.Now.ToString("dd/MM/yyyy");
                //DateTime oDate = Convert.ToDateTime(iDate);
                foreach (DataRow row in dt.Rows)
                {
                    vals.Add("oppo_CustomerID".ToLower(),row["quot_CustomerID"].ToString());
                    vals.Add("oppo_Phone".ToLower(),row["quot_Phone"].ToString());
                    vals.Add("oppo_Email".ToLower(),row["quot_Email"].ToString());
                    vals.Add("oppo_Address".ToLower(),row["quot_Address"].ToString());
                    //vals.Add("oppo_Description".ToLower(),row["quot_Description"].ToString());
                    //vals.Add("oppo_Remarks".ToLower(),row["quot_Remark"].ToString());
                    vals.Add("oppo_Amount".ToLower(),row["quot_Total"].ToString());
                    
                    string am = DateTime.Parse(row["quot_Date"].ToString()).ToString(); 
                    //oDate = Convert.ToDateTime(am);
                    vals.Add("oppo_Date".ToLower(), iDate);
                    vals.Add("oppo_StartDate".ToLower(), iDate);
                    //vals.Add("oppo_Date".ToLower(),row["DATE"].ToString());
                    //vals.Add("oppo_Date".ToLower(), DateTime.Parse(row["DATE"].ToString()).ToShortDateString());
                    vals.Add("oppo_Detail".ToLower(), "Testing Detail");
                    vals.Add("oppo_Type".ToLower(),row["quot_Type2"].ToString());

                    vals.Add("oppo_Description".ToLower(), row["quot_Type2"].ToString());
                    vals.Add("oppo_Remarks".ToLower(), "Converted from Quotation No: " +  row["quot_Name"].ToString() + ". " + row["quot_Remark"].ToString());
                    
                    vals.Add("oppo_Status".ToLower(), "New");

                    string b = Request.Form["quotID"].ToString();
                    aval.Add("oppo_QuotationID".ToLower(),b);
                }
                string screen = "tblOpportunityNew;tblOpportunityCompanyInfo;tblOpportunityTotal";
                string re= cls.saveRecord(screen, vals, db, aVals: aval, ignoreROF:true);

                var str = JsonConvert.DeserializeObject<dynamic>(re);
                if (str.tbl != null)
                {
                    string oppoID = (string)str.tbl[0].msg;
                    Response.Write("ok" + oppoID);
                }
                db.close();
            }
            catch(Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void Revise()
        {
            try
            {
                string hid="";
                string re="";

                Dictionary<string, string> vals = new Dictionary<string, string>();
                Dictionary<string, string> valsID = new Dictionary<string, string>();
                Dictionary<string, string> valsItem = new Dictionary<string, string>();

                string sql = "SELECT * FROM tblQuotation WHERE quot_QuotationID = " + db.sqlStr(Request.Form["quotID"].ToString());

                
                valsID.Add("quot_ReviseFromQuotID".ToLower(),Request.Form["quotID"].ToString());
                //string reviseFromQuoteID = Request.Form["quotID"].ToString();

                DataTable dt = new DataTable();
                dt = db.readData(sql);

                foreach (DataRow row in dt.Rows)
                {
                    //screen quotation
                    vals.Add("quot_Name".ToLower(),"");
                    DataTable d = new DataTable();

                    if(string.IsNullOrEmpty(row["quot_Reference"].ToString()))
                        valsID.Add("quot_Reference".ToLower(), row["quot_name"].ToString());
                    else
                        valsID.Add("quot_Reference".ToLower(), row["quot_Reference"].ToString());


                    string Date = (DateTime.Parse(row["quot_Date"].ToString()).ToString("dd/MM/yyyy"));
                    vals.Add("quot_Date".ToLower(),Date);

                    string ValidatedDate = DateTime.Parse(row["quot_ValidatedDate"].ToString()).ToString("dd/MM/yyyy");
                    vals.Add("quot_ValidatedDate".ToLower(), ValidatedDate);
                    vals.Add("quot_InvoiceType".ToLower(), row["quot_InvoiceType"].ToString());
                    vals.Add("quot_Type2".ToLower(), row["quot_Type2"].ToString());
                    vals.Add("quot_Attendance".ToLower(), row["quot_Attendance"].ToString());
                    vals.Add("quot_PreparedBy".ToLower(), row["quot_PreparedBy"].ToString());
                    vals.Add("quot_TermAndCondition".ToLower(), row["quot_TermAndCondition"].ToString());
                    vals.Add("quot_Notice".ToLower(), row["quot_Notice"].ToString());
                    vals.Add("quot_Remark".ToLower(), row["quot_Remark"].ToString());

                    //screen comapny info
                    vals.Add("quot_CustomerID".ToLower(),row["quot_CustomerID"].ToString());
                    //vals.Add("quot_Phone".ToLower(),row["quot_Phone"].ToString());
                    //vals.Add("quot_Email".ToLower(),row["quot_Email"].ToString());
                    //vals.Add("quot_Address".ToLower(),row["quot_Address"].ToString());
                    //vals.Add("quot_Code".ToLower(),row["quot_Code"].ToString());
                    //vals.Add("quot_VATTIN".ToLower(),row["quot_VATTIN"].ToString());
                    
                    //screen total
                    vals.Add("quot_SubTotal".ToLower(),row["quot_SubTotal"].ToString());
                    vals.Add("quot_Discount".ToLower(),row["quot_Discount"].ToString());
                    vals.Add("quot_Disc".ToLower(),row["quot_Disc"].ToString());
                    vals.Add("quot_DiscountAmount".ToLower(),row["quot_DiscountAmount"].ToString());
                    vals.Add("quot_Total".ToLower(),row["quot_Total"].ToString());

                    vals.Add("quot_VAT".ToLower(), row["quot_VAT"].ToString());
                    vals.Add("quot_VATAmount".ToLower(), row["quot_VATAmount"].ToString());
                    vals.Add("quot_TotalVAT".ToLower(), row["quot_TotalVAT"].ToString());
                    vals.Add("quot_Balance".ToLower(), row["quot_Balance"].ToString());
                }

                string screen = "tblQuotationNew;tblQuotationTotal";
                //cls.saveRecord(screen, vals, db, aVals: null);
                
                re = cls.saveRecord(screen, vals, db, aVals: valsID);
                var strTest = JsonConvert.DeserializeObject<dynamic>(re);
                hid = (string)strTest.tbl[0].msg;

                string sqlItem = "SELECT * FROM tblQuotationItem WHERE quit_QuotationID=" + Request.Form["quotID"].ToString();
                dt = db.readData(sqlItem);
                
                foreach (DataRow dr in dt.Rows)
                {
                    Dictionary<string, string> aVal = new Dictionary<string, string>();
                    valsItem.Clear();

                    valsItem.Add("quit_ItemID".ToLower(), dr["quit_ItemID"].ToString());
                    valsItem.Add("quit_Description".ToLower(), dr["quit_Description"].ToString());
                    valsItem.Add("quit_PeriodProcessing".ToLower(), dr["quit_PeriodProcessing"].ToString());
                    valsItem.Add("quit_Qty".ToLower(), dr["quit_Qty"].ToString());
                    valsItem.Add("quit_Price".ToLower(), dr["quit_Price"].ToString());
                    valsItem.Add("quit_Discount".ToLower(), dr["quit_Discount"].ToString()); // discount type
                    valsItem.Add("quit_Disc".ToLower(), dr["quit_Disc"].ToString());
                    valsItem.Add("quit_DiscountAmount".ToLower(), dr["quit_DiscountAmount"].ToString());
                    valsItem.Add("quit_SubTotal".ToLower(), dr["quit_SubTotal"].ToString());
                    valsItem.Add("quit_Total".ToLower(), dr["quit_Total"].ToString());
                    valsItem.Add("quit_Remark".ToLower(), dr["quit_Remark"].ToString());
                    

                    string screenItem = "tblQuotationItemNew";
                    aVal.Add("quit_QuotationID", hid);
                    aVal.Add("quit_ReviseFromQuotItemID",dr["quit_QuotationItemID"].ToString());

                    cls.saveRecord(screenItem, valsItem, db, aVals: aVal);
                }

                string updateCountRevise = "UPDATE tblQuotation SET quot_CountRevise=1 WHERE quot_QuotationID="+ Request.Form["quotID"].ToString();
                db.execData(updateCountRevise);

                Response.Write(hid);
                db.close();
            }
            catch(Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
            
        }

        private void ViewReviseQuotation()
        {
            try
            {
                DataTable dt = new DataTable();
                dt = db.readData("SELECT quot_ReviseFromQuotID FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID=" + Request.Form["quotID"].ToString());
                if (dt.Rows.Count > 0)
                {
                    Response.Write(dt.Rows[0]["quot_ReviseFromQuotID"].ToString());
                }
            }
            catch (Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void GetServiceType()
        {
            try
            {
                string test = Request.Form["serviceType"].ToString();
                Response.Write(test);
                db.close();
            }
            catch(Exception ex)
            {
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
            
            
        }

        private void GetCompInfo()
        {
            try
            {
                string sql = "SELECT  cust_Phone,cust_Email,cust_Address,cust_VATTIN,cust_Code FROM tblCustomer WHERE cust_Deleted IS NULL AND cust_CustomerID = " + db.sqlStr(Request.Form["compID"]);
                Response.Write(db.tblToJson(db.readData(sql)));
                db.close();
                //cust_Phone,cust_Email,cust_Address
            }
            catch (Exception ex)
            {

                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }

            //try
            //{
            //    string sql = "SELECT  cust_Phone, cust_Email, cust_Address FROM tblCustomer WHERe cust_Deleted IS NULL AND cust_CustomerID = " + db.sqlStr(Request.Form["compID"]);
            //    DataTable dt = new DataTable();
            //    dt = db.readData(sql);
            //    string Info = "";
            //    if (dt.Rows.Count > 0)
            //    {
            //        Info = dt.Rows[0][0].ToString() + "^";
            //        Info = Info + dt.Rows[0][1].ToString() + "^";
            //        Info = Info + dt.Rows[0][2].ToString();
            //    }
            //    Response.Write(Info);
            //    db.close();
            //}
            //catch (Exception ex)
            //{
            //    db.close();
            //    Response.Write(ex.Message);
            //    cls.endRequest();
            //}

        }
    }
}