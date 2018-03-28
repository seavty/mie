using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.quotation
{
    public partial class quotation : System.Web.UI.Page
    {
        sapi.sapi cls = new sapi.sapi();
        sapi.db db = new sapi.db();
        DataTable tblData = null;

        Dictionary<string, string> vals = new Dictionary<string, string>();

        string screen = "tblQuotationNew;tblQuotationTotal";
        //string screen = "tblQuotationNew;";

        string screenItem = "tblQuotationItemNew";
        string screenItemList = "tblQuotationItemList";
        string frm = "frmMaster";
        string IDFIeld = "quot_quotationid";
        string Tab = "tblQuotation";
        string cTab = "tblQuotation";

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
                                if (!string.IsNullOrEmpty(Request.Form["quot_OpportunityID"].ToString()))
                                {
                                    DataTable tmp = db.readData("Select oppo_CustomerID From tblOpportunity Where oppo_OpportunityID=" + Request.Form["quot_OpportunityID"].ToString());
                                    foreach (DataRow tmpRow in tmp.Rows)
                                    {
                                        filter = " cust_CustomerID = " + tmpRow["oppo_CustomerID"].ToString();
                                    }
                                }
                            }
                            
                            if (url.Get("colName") == "quot_OpportunityID")
                            {
                                if (!string.IsNullOrEmpty(url.Get("quot_CustomerID")))
                                {
                                    filter = " oppo_CustomerID = " + Request.Form["quot_CustomerID"].ToString();
                                }
                                
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
                            Response.Write(cls.delRecord("tblQuotationNew", Request.Form["eid"].ToString(), db));
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
                            if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                            {
                                sapi.defaultValue.add("quot_CustomerID", url.Get("cust_customerid"));
                            }
                            sapi.defaultValue.add("quot_Date", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                            //sapi.defaultValue.add("quot_ValidatedDate", DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"));
                            //sapi.defaultValue.add("quot_WarehouseID", db.readData("sett_WarehouseID", "Select sett_WarehouseID From sys_Setting"));
                            //sapi.defaultValue.add("quot_TermAndCondition",
                            //                "Terms & Conditions: \n" +
                            //                "1.Term of payment: The service provider starts working after receiving the full deposit.Payment is not refunable.\n" +
                            //                "2.Type of payment: Cash on hand, bank deposit, or bank transfer\n" +
                            //                "3.International transfer: Any international bank transfer fees is the obligation of client\n" +
                            //                "4.Currency: USD\n" +
                            //                "5.Exclusive of VAT: Please kindly inform us in advance if you like VAT to be included in this quote.");
                            //sapi.defaultValue.add("quot_Notice",
                            //                "Notice:\n" +
                            //                "1.The client is responsible for providing all required information / documents and making his / her presence available for signing, sealing, finger scanning, photo taking, cooperate bank account opening, and residency permit application on the requested dates.\n" +
                            //                "2.The payment is exclusive of penalty if the client fails to submit the required documents before the deadline required by related offices.\n" +
                            //                "3.The service provider is not liable for the result if client fails to provide true information / supporting documents.\n" +
                            //                "4.The service provider is responsible for the completion of the project according to the agreed scope of work and timeframe.\n" +
                            //                "5.Full or part of the payment is not refundable unless the service provider fails to complete the project.");

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
                vals.Add(st.ToLower(), Request.Form[st].ToString());
            }
        }

        string loadScreen(string eid, sapi.sapi.recordMode mode)
        {
            string re = "";
            string list = "";
            string topTitle = "";
            DataTable tblTop = null;
            cls.Mode = mode;
            string countRevise = "";
            string QuotIDConverted = "";

            if (eid == "0")
            {
                cls.Mode = sapi.sapi.recordMode.New;
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and quit_QuotationID = " + eid, cPage: -1) +
                       "</div>";

                sapi.defaultValue.add("quot_PreparedBy", Session["userid"].ToString());
                DateTime dt = DateTime.Now;
                dt = dt.AddDays(7); 
                string validateDate = dt.ToString("yyyy-MM-dd");
                sapi.defaultValue.add("quot_ValidatedDate", validateDate);

                if (!string.IsNullOrEmpty(url.Get("oppo_opportunityid")))
                {
                    DataTable tbl = db.readData("SELECT * FROM tblOpportunity WHERE oppo_Deleted IS NULL AND oppo_OpportunityID=" + url.Get("oppo_opportunityid"));
                    foreach (DataRow row in tbl.Rows)
                    {
                        sapi.defaultValue.add("quot_OpportunityID", url.Get("oppo_opportunityid"));
                        sapi.defaultValue.add("quot_CustomerID", row["oppo_CustomerID"].ToString());
                    }
                }
                if (!string.IsNullOrEmpty(url.Get("cust_customerid")))
                {
                    DataTable tblCus = db.readData("SELECT * FROM tblCustomer WHERE cust_CustomerID=" + url.Get("cust_customerid"));
                    string id = tblCus.Rows[0]["cust_InvoiceType"].ToString();
                    sapi.defaultValue.add("quot_Phone", tblCus.Rows[0]["cust_Phone"].ToString());
                    sapi.defaultValue.add("quot_Email", tblCus.Rows[0]["cust_Email"].ToString());
                    sapi.defaultValue.add("quot_Address", tblCus.Rows[0]["cust_Address"].ToString());
                    sapi.defaultValue.add("quot_Code", tblCus.Rows[0]["cust_Code"].ToString());
                    sapi.defaultValue.add("quot_VATTIN", tblCus.Rows[0]["cust_VATTIN"].ToString());
                    sapi.defaultValue.add("quot_InvoiceType", tblCus.Rows[0]["cust_InvoiceType"].ToString());

                    //DataTable tblUser = db.readData("SELECT user_userName FROM sys_user WHERE user_userID=" + Session["userid"]);
                    //sapi.defaultValue.add("quot_Remark",tblUser.Rows[0]["user_userName"].ToString());
                }
            }

            if (mode == sapi.sapi.recordMode.View)
            {
                string compID = db.readData("quot_CustomerID", "SELECT quot_CustomerID FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID =" + db.sqlStr(url.Get("quot_quotationid")));
                sapi.Buttons.add("Company", "arrow-left", "info", "ViewCustomer(" + compID + ")");
                sapi.Buttons.add("Print", "print", "success", "printInv(" + eid + ")");
                DataTable dtQuotID= new DataTable();
                if (!string.IsNullOrEmpty(url.Get("quot_quotationid")))
                {
                    dtQuotID = db.readData("SELECT * FROM tblQuotationItem WHERE quit_Deleted IS NULL AND quit_IsConvert = 'Yes' AND quit_QuotationID = " + url.Get("quot_quotationid"));
                    foreach (DataRow dr in dtQuotID.Rows)
                    {
                        QuotIDConverted += dr["quit_QuotationItemID"] + ",";
                    }
                }

                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                    mode: 0, filter: " and quit_QuotationID = " + eid, cPage: -1, assc: 2) +
                       "</div>";
                sapi.Buttons.add("Revise", "loop2", "warning", "Revise()");

                DataTable dtConverted = db.readData("SELECT * FROM tblQuotationItem WHERE quit_Deleted IS NULL AND quit_QuotationID = " + url.Get("quot_quotationid"));
                bool b = true;
                foreach (DataRow dr in dtConverted.Rows)
                {
                    if (dr["quit_IsConvert"].ToString() != "Yes")
                    {
                        b = false;
                        break;
                    }
                }
                if(!b)
                    sapi.Buttons.add("Convert to Invoice", "dollars", "bg-pink bg-active-darkPink fg-white", "ConvertToInvoice()");
                
                //view revise quot
                DataTable datatb = new DataTable();
                datatb = db.readData("SELECT quot_ReviseFromQuotID FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID=" + eid);
                if (datatb.Rows.Count > 0)
                {
                    if(!string.IsNullOrEmpty(datatb.Rows[0]["quot_ReviseFromQuotID"].ToString()))
                        sapi.Buttons.add("View Revise Quotation", "tab", "warning", "ViewReviseQuotation("+ datatb.Rows[0]["quot_ReviseFromQuotID"].ToString() + ")");//Button for view original
                }
                //sapi.Buttons.add("Convert to Project", "medal", "", "convertToOpportunity()");

                string reviseCount = "SELECT quot_CountRevise,quot_CustomerID FROM tblQuotation WHERE quot_Deleted IS NULL AND quot_QuotationID=" + url.Get("quot_quotationid");
                DataTable dt = new DataTable();
                dt = db.readData(reviseCount);
                if (string.IsNullOrEmpty(dt.Rows[0]["quot_CountRevise"].ToString()))
                    countRevise = "0";
                else
                    countRevise = "1";

                //sapi.Buttons.add("Back","loop2","info","ViewCustomer(" + dt.Rows[0]["quot_CustomerID"].ToString() + ")");
                //countQuotUsed
                int QuotUsed = int.Parse(db.readData("C","SELECT COUNT(*) AS [C] FROM tblInvoice WHERE invo_QuotationID="+ db.sqlStr(url.Get("quot_quotationid"))));
                if (QuotUsed > 0)
                {
                    cls.hideDelete = true;
                    cls.hideEdit = true;
                }
                
            }
            if (mode == sapi.sapi.recordMode.Edit)
            {
                list = "<div id='dvList'>" + cls.findRecord(db, screenItem, screenItemList, "frmList", null, "",
                                mode: 1, filter: " and quit_QuotationID = " + eid, cPage: -1) +
                       "</div>";
            }

            if (mode == sapi.sapi.recordMode.View || mode == sapi.sapi.recordMode.Edit)
            {
                //sapi.Buttons.add("Convert to SO", "arrow-right", "info", "convertSO(" + eid + ")");
                //sapi.Buttons.add("Convert to Invoice", "arrow-right", "info", "convertInv(" + eid + ")");
                topTitle = "Quotation";
                tblTop = db.readData("SELECT quot_Name,cust_Name FROM tblQuotation INNER JOIN tblCustomer ON quot_CustomerID=cust_CustomerID WHERE quot_Deleted IS NULL AND quot_QuotationID=" + eid);
            }
            
            re = cls.loadScreen(db, screen, frm, ref tblData, eid, tblTop, topTitle);

            string res = "<input id='QuotIDConverted' type='hidden' value=" + QuotIDConverted + ">";
            
            //string res = "<input id = 'hidButtonEditDelete' type = 'hidden' value =" + countRevise+">";

            re = re + list + res;
            return re;
        }

        string saveRecord()
        {
            EventDateValidation();

            string re = "";
            string hid = "";
            Dictionary<string, string> aVal = new Dictionary<string, string>();

            db.beginTran();

            if (vals.ContainsKey("quot_customerid".ToLower()))
            {
                string sql = "SELECT * FROM tblCustomer WHERE cust_CustomerID=" + db.cNum(vals["quot_customerid"].ToString());
                DataTable dt = db.readData(sql);
                foreach (DataRow dr in dt.Rows) {
                    if (vals.ContainsKey("quot_Code".ToLower()))
                        vals["quot_Code".ToLower()] = dr["cust_Code"].ToString();
                    if (vals.ContainsKey("quot_VATTIN".ToLower()))
                        vals["quot_VATTIN".ToLower()] = dr["cust_VATTIN"].ToString();
                }
            }

            re = cls.saveRecord(screen, vals, db, aVals: aVal);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            var strTest = JsonConvert.DeserializeObject<dynamic>(re);

            if (str.tbl != null)
            {
                hid = (string)strTest.tbl[0].msg; 

                if (str.tbl[0].status == "ok")
                {
                    if (!vals.ContainsKey("quot_quotationid"))
                    {
                        if (vals.ContainsKey("quot_customerid"))
                        {
                            if (!string.IsNullOrEmpty(vals["quot_customerid"]))
                            {
                                var tmp = db.execData("Update tblCustomer Set cust_Type='Customer' Where cust_Type='Lead' and cust_CustomerID=" +
                                    vals["quot_customerid"].ToString());
                                hid = (string)str.tbl[0].msg;
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
                            }
                        }
                    }

                    if (Request.Form.GetValues("N") != null)
                    {
                        //hid = (string)str.tbl[0].msg;
                        aVal.Clear();
                        aVal.Add("quit_QuotationID", hid);

                        foreach (var st in Request.Form.GetValues("N"))
                        {
                            if (vals["txtdel" + st] != "")
                            {
                                cls.delRecord(screenItem, vals["quit_QuotationItemID".ToLower() + st], db);
                            }else
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double quit_Qty = 0;
                                double quit_Price = 0;
                                string quit_DiscType = "";
                                double quit_Discount = 0;
                                double quit_DiscountAmount = 0;

                                if (v.ContainsKey("quit_Qty".ToLower()))
                                {
                                    quit_Qty = db.cNum(v["quit_Qty".ToLower()]);
                                }
                                if (v.ContainsKey("quit_Price".ToLower()))
                                {
                                    quit_Price = db.cNum(v["quit_Price".ToLower()]);
                                }
                                if (v.ContainsKey("quit_Total".ToLower()))
                                {
                                    v["quit_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                    v["quit_SubTotal".ToLower()] = v["quit_Total".ToLower()];
                                }
                                if (v.ContainsKey("quit_Disc".ToLower()))
                                {
                                    quit_Discount = db.cNum(v["quit_Disc".ToLower()]);
                                }
                                if (v.ContainsKey("quit_Discount".ToLower()))
                                {
                                    quit_DiscType = v["quit_Discount".ToLower()];
                                    if (quit_DiscType == "P")
                                    {
                                        quit_DiscountAmount = (double.Parse(v["quit_Total".ToLower()]) * quit_Discount) / 100;
                                        //v["quit_Discount".ToLower()] = "%";
                                    }
                                    else if (quit_DiscType == "A")
                                    {
                                        quit_DiscountAmount = quit_Discount;
                                        //v["quit_Discount".ToLower()] = "$";
                                    }
                                    else
                                    {
                                        quit_DiscountAmount=0;
                                    }
                                }

                                v["quit_Total".ToLower()] = (double.Parse(v["quit_Total".ToLower()]) - quit_DiscountAmount).ToString();

                                string re2 = cls.saveRecord(screenItem, v, db, aVal, st);
                                str = JsonConvert.DeserializeObject<dynamic>(re2);
                                if (str.tbl != null)
                                {
                                    if (str.tbl[0].status != "ok")
                                    {
                                        db.rollback();
                                        return re2;
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
                    quotationTotal(hid,db);
                    db.commit();
                }
            }
            return re;
        }

        private void EventDateValidation()
        {
            if (vals.ContainsKey("quot_date".ToLower()) && vals.ContainsKey("quot_validateddate".ToLower()))
            {
                if (vals["quot_date".ToLower()] != "" && vals["quot_validateddate".ToLower()] != "")
                {
                    if (DateTime.Parse(db.getDate(vals["quot_date".ToLower()], 0)) >
                        DateTime.Parse(db.getDate(vals["quot_validateddate".ToLower()], 0)))
                    {
                        DataTable tblResult = new DataTable();
                        tblResult.Rows.Add();
                        tblResult.Columns.Add("colName");
                        tblResult.Columns.Add("msg");
                        tblResult.Rows[0]["colName"] = "quot_date";
                        tblResult.Rows[0]["msg"] = "Issued Date cannot greater than Validated Date";
                        string jsonError = ("{\"error\":" + db.tblToJson(tblResult) + "}");

                        Response.Clear();
                        Response.Write(jsonError);
                        db.close();
                        //Response.End();
                        cls.endRequest();
                    }

                }
            }
        }

        void quotationTotal(string eid,sapi.db db)
        {
            string quot_Discount = "";
            double quot_Disc = 0;
            double quot_SubTotal = 0;
            double quot_DiscountAmount = 0;
            double quot_Total = 0;
            Double quot_VAT = 0;
            double quot_Deposit = 0;

            if (string.IsNullOrEmpty(eid.Trim()))
                eid = "0";

          DataTable tbl = db.readData("Select * From tblQuotation " +
                " Where quot_Deleted is null and quot_QuotationID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                quot_Discount = row["quot_Discount"].ToString();
                quot_Disc = db.cNum(row["quot_Disc"].ToString());
                quot_VAT = db.cNum(row["quot_VAT"].ToString());
                quot_Deposit = db.cNum(row["quot_Deposit"].ToString());
            }
            tbl = db.readData("Select * From tblQuotationItem Where quit_Deleted is null and quit_QuotationID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                quot_SubTotal += db.cNum(row["quit_Total"].ToString());
            }
            if (quot_Discount == "P")
            {
                quot_DiscountAmount = (quot_SubTotal * quot_Disc / 100);
            }
            else
            {
                quot_DiscountAmount = quot_Disc;
            }
            double quot_VATAmount = (quot_VAT / 100) * (quot_SubTotal);
            quot_Total = quot_SubTotal - quot_DiscountAmount;
            double quot_GrandTotal = quot_Total + quot_VATAmount;
            
            double Balance = 0;
            Balance = quot_GrandTotal - quot_Deposit;

            db.execData("Update tblQuotation Set " +
                " quot_SubTotal = " + quot_SubTotal +
                ",quot_DiscountAmount = " + quot_DiscountAmount +
                ",quot_Total = " + quot_Total +
                ",quot_TotalVAT = " + quot_GrandTotal +
                ",quot_VATAmount = " + quot_VATAmount +
                ",quot_Balance = " + Balance +
                " Where quot_QuotationID = " + eid
                );
            //if put db.close() error show Message = "This SqlTransaction has completed; it is no longer usable."
            //db.close();
            //cls.endRequest();
        }
    }
}