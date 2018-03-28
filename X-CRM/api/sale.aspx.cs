using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.api
{
    public partial class sale : System.Web.UI.Page
    {
        string _userid = "";
        string _user = "";
        string prof = "";

        protected void Page_Load(object sender, EventArgs e)
        {

            int recPage = 50;
            string getdate = "DATEADD(HOUR,7, GETUTCDATE())";
            string cdb = "";
            string numFormat = "##0.00";

            if (Request.Form["app"] != null)
            {

                if (Request.Form["app"].ToString() == "login")
                {
                    CryptLib _crypt = new CryptLib();
                    string plainText = Request.Form["password"].ToString();
                    string iv = "Xsoft";// CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
                    string key = CryptLib.getHashSha256("@XSoft201701", 31); //32 bytes = 256 bits

                    string password = _crypt.encrypt(plainText, key, iv);
                    string user = Request.Form["user"].ToString();
                    sapi.db db = new sapi.db();

                    try
                    {
                        if (db.connect())
                        {
                            DataTable tbl = db.readData("Select * from sys_user Where user_userName=" + db.sqlStr(user) +
                                " and user_Password = " + db.sqlStr(password));
                            if (tbl.Rows.Count > 0)
                            {
                                iv = CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
                                plainText = tbl.Rows[0]["user_userName"].ToString() +
                                    tbl.Rows[0]["user_userID"].ToString() +
                                    DateTime.UtcNow.AddHours(7).ToString("mmyyyyhhssMMmmdd");

                                string re = db.execData("Update sys_user Set user_SID=" +
                                    db.sqlStr(_crypt.encrypt(plainText, key, iv)) +
                                    " Where user_UserID=" + tbl.Rows[0]["user_userID"].ToString());
                                if (re == "ok")
                                {
                                    string wh = "";
                                    string mbid = "";
                                    DataTable tblMB = db.readData("select * from tblMobileSale " +
                                        " inner join tblMobileSaleItem on msit_MobileSaleID = mbsl_MobileSaleID and msit_Deleted is null " +
                                        " where mbsl_CheckIn is null and msit_UserID = " + tbl.Rows[0]["user_userID"].ToString());
                                    foreach (DataRow rowMB in tblMB.Rows)
                                    {
                                        wh = rowMB["mbsl_warehouseID"].ToString();
                                        mbid = rowMB["mbsl_MobileSaleID"].ToString();
                                    }
                                    Response.Write("{\"tbl\":[{\"status\":\"ok\",\"mobilesaleid\":\"" + mbid + "\",\"warehouse\":\"" + wh + "\",\"SID\":\"" + _crypt.encrypt(plainText, key, iv) + "\"}]}");

                                }
                                else
                                {
                                    Response.Write(re);
                                }
                            }
                            else
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"Invalid User Name or Password !\"}]}");

                            }

                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"ok\",\"msg\":\"Unable To Connect To Server !\"}]}");

                        }
                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }
                    new sapi.sapi().endRequest();
                    Response.End();

                }
                if (Request.Form["SID"] == null)
                {
                    Response.Write("{\"tbl\":[{\"status\":\"nosession\",\"msg\":\"You must login to get access !\"}]}");
                    new sapi.sapi().endRequest();
                    Response.End();
                }
                else
                {

                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            DataTable tbl = db.readData("Select * from sys_user Where user_SID = " +
                                db.sqlStr(Request.Form["SID"].ToString()));
                            if (tbl.Rows.Count <= 0)
                            {
                                Response.Write("{\"tbl\":[{\"status\":\"nosession\",\"msg\":\"Session Expired !\"}]}");
                                new sapi.sapi().endRequest();
                                Response.End();
                            }
                            else
                            {
                                _userid = tbl.Rows[0]["user_userid"].ToString();
                                _user = tbl.Rows[0]["user_userName"].ToString();

                                DataTable tblProf = db.readData("Select * from sys_UserProfile Where uspr_UserID = " + tbl.Rows[0]["user_userID"].ToString());
                                foreach (DataRow rowProf in tblProf.Rows)
                                {
                                    prof = prof + rowProf["uspr_ProfileID"].ToString() + ",";
                                }
                                if (prof.Length > 0)
                                {
                                    prof = prof.Substring(0, prof.Length - 1);
                                }
                                else
                                {
                                    prof = "0";
                                }

                            }
                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Unable To Connect To Server !\"}]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally
                    {
                        db.close();
                    }

                }

                if (Request.Form["app"].ToString() == "Setting")
                {

                    sapi.db db = new sapi.db();
                    string search = "";


                    Response.Write(getJSONtbl(" sys_Setting where 1=1 ",
                        search, "sett_SettingID", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItemCat")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(itmc_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }

                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }

                    Response.Write(getJSONtbl(" tblItemCat where itmc_Deleted is null ",
                        search, "itmc_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItemGroup")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(itmg_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" tblItemGroup where itmg_Deleted is null ",
                        search, "itmg_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItem")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(item_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" tblItem where item_Deleted is null ",
                        search, "item_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getItemWarehouse")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(item_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" vItemWarehouse where item_Deleted is null ",
                        search, "item_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getCustomer")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        //search = " and (lower(cust_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                        //        " )"; comment by ty;
                        search = Request.Form["search"].ToString();
                        search = " and (lower(cust_Name) like " + db.sqlStrLike(Request.Form["search"].ToString().Trim().ToLower()) +
                                " )"; //-- add by ty

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" tblCustomer where Cust_Deleted is null ",
                        search, "cust_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getWarehouse")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(ware_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" tblWarehouse where ware_Deleted is null ",
                        search, "ware_Name", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "getInvoice")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(invo_InvoiceID) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" vInvoicePrint where 1=1 ",
                        search, "invo_InvoiceID", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "Reprint")
                {

                    sapi.db db = new sapi.db();
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        search = Request.Form["search"].ToString();
                        search = " and (lower(invo_InvoiceID) like " + db.sqlStrLike(search.Trim().ToLower()) +
                                " )";

                    }
                    if (Request.Form["sql"] != null)
                    {
                        //search = " and " + Request.Form["sql"].ToString();
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    Response.Write(getJSONtbl(" vInvoice where 1=1 ",
                        search, "invo_InvoiceID", recPage, cdb));
                }

                if (Request.Form["app"].ToString() == "uploadInvoice")
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    foreach (var st in Request.Form.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(st))
                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                    }


                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Response.Write(uploadInvoice(db, vals));
                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Unable to connect to server !\"}]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "uploadSO")
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    foreach (var st in Request.Form.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(st))
                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                    }


                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Response.Write(uploadSO(db, vals));
                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Unable to connect to server !\"}]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "createCN")
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    foreach (var st in Request.Form.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(st))
                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                    }


                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Session["userid"] = _userid;
                            Session["user"] = _user;
                            Response.Write(new clsGlobal().createCN(Request.Form["invo_invoiceid"].ToString(), vals,db));
                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Unable to connect to server !\"}]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally { db.close(); }
                }

                if (Request.Form["app"].ToString() == "invoicePayment")
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    foreach (var st in Request.Form.AllKeys)
                    {
                        if (!string.IsNullOrEmpty(st))
                            vals.Add(st.ToLower(), Request.Form[st].ToString());
                    }


                    sapi.db db = new sapi.db();
                    try
                    {
                        if (db.connect())
                        {
                            Session["userid"] = _userid;
                            Session["user"] = _user;
                            Response.Write(new clsGlobal().invoicePayment(vals, db));
                        }
                        else
                        {
                            Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"Unable to connect to server !\"}]}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
                    }
                    finally { db.close(); }
                }
            }
        }

        string uploadSO(sapi.db db, Dictionary<string, string> vals)
        {
            Session["userid"] = _userid;
            Session["user"] = _user;
            string re = "";
            string re2 = "";
            string hid = "";
            sapi.sapi cls = new sapi.sapi();
            string screenItem = "tblSaleOrderItemNew";
            string screen = "tblSaleOrderNew";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            db.beginTran();
            re = cls.saveRecord(screen, vals, db, aVals: aVal,ignoreROF:true);

            var str = JsonConvert.DeserializeObject<dynamic>(re);
            if (str.tbl != null)
            {


                if (str.tbl[0].status == "ok")
                {
                    hid = (string)str.tbl[0].msg;
                    foreach (var st in Request.Form["N"].ToString().Split(','))
                    {
                        aVal.Clear();
                        aVal.Add("soit_SaleOrderID", hid);

                        Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                        v["soit_total"] = (db.cNum(v["soit_Qty".ToLower()].ToString()) * db.cNum(v["soit_Price".ToLower()].ToString())).ToString();
                        aVal.Add("soit_ShipQty", "0");
                        aVal.Add("soit_RemainQty", v["soit_Qty".ToLower()]);

                        re2 = cls.saveRecord(screenItem, v, db, aVal, st,ignoreROF:true);
                        str = JsonConvert.DeserializeObject<dynamic>(re2);
                        if (str.tbl != null)
                        {
                            if (str.tbl[0].status != "ok")
                            {
                                db.rollback();
                                return re2;
                            }
                        }
                    }
                    new clsGlobal().SOTotal(hid, db);

                }
                else
                {
                    db.rollback();
                    return re;
                }
            }
            db.commit();
            return re;
        }

        string uploadInvoice(sapi.db db, Dictionary<string, string> vals)
        {
            Session["userid"] = _userid;
            Session["user"] = _user;
            string re = "";
            string re2 = "";
            string hid = "";
            sapi.sapi cls = new sapi.sapi();
            string screenItem = "tblInvoiceItemNew";
            string screen = "tblInvoiceNew";
            Dictionary<string, string> aVal = new Dictionary<string, string>();
            DataTable tblResult = new DataTable();
            tblResult.Rows.Add();
            tblResult.Columns.Add("status");
            tblResult.Columns.Add("msg");

            bool isCredit = false;
            if (vals.ContainsKey("isCredit".ToLower()))
            {
                isCredit = true;
            }

            if (db.readData("select 1 from sys_ProfilePermission " +
                " where pfpm_TableID = 14 and pfpm_I = 'Y' and pfpm_ProfileID in (" + prof + ")").Rows.Count <= 0 ||
                db.readData("select 1 from sys_ProfilePermission " +
                " where pfpm_TableID = 17 and pfpm_I = 'Y' and pfpm_ProfileID in (" + prof + ")").Rows.Count <= 0 ||
                db.readData("select 1 from sys_ProfilePermission " +
                " where pfpm_TableID = 21 and pfpm_I = 'Y' and pfpm_ProfileID in (" + prof + ")").Rows.Count <= 0 ||
                db.readData("select 1 from sys_ProfilePermission " +
                " where pfpm_TableID = 4035 and pfpm_I = 'Y' and pfpm_ProfileID in (" + prof + ")").Rows.Count <= 0

               )
            {
                tblResult.Rows[0]["status"] = "error";
                tblResult.Rows[0]["msg"] = "You do not have permission to create invoice !";
                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                return re;
            }
            db.beginTran();
            if (!vals.ContainsKey("invo_invoiceid"))
            {
                aVal.Add("invo_WorkflowID", "6");
                aVal.Add("invo_WorkflowItemID", "12");
            }

            if (vals.ContainsKey("invo_exrate"))
            {
                aVal.Add("invo_ExRate", vals["invo_exrate".ToLower()]);
                vals.Remove("invo_exrate".ToLower());
            }
            if (vals.ContainsKey("invo_cashin"))
            {
                aVal.Add("invo_CashIn", vals["invo_cashin".ToLower()]);
                vals.Remove("invo_CashIn".ToLower());
            }
            if (vals.ContainsKey("invo_cashin2"))
            {
                aVal.Add("invo_CashIn2", vals["invo_cashin2".ToLower()]);
                vals.Remove("invo_cashin2".ToLower());
            }

            if (vals.ContainsKey("invo_status"))
                vals["invo_status"] = "completed";

            if (vals.ContainsKey("invo_MobileSaleID".ToLower()))
            {
                aVal.Add("invo_MobileSaleID", vals["invo_MobileSaleID".ToLower()]);
                vals.Remove("invo_MobileSaleID".ToLower());
            }

            if (isCredit)
            {
                aVal["invo_CashIn"] = "0";
                aVal["invo_CashIn2"] = "0";
            }
            if (!vals.ContainsKey("invo_Date".ToLower()))
            {
                vals.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
            }
            else
            {
                vals.Remove("invo_Date".ToLower());
                vals.Add("invo_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd HH:mm"), 1));
            }

            re = cls.saveRecord("tblInvoiceNew", vals, db, aVals: aVal,ignoreROF:true);

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
                    if (Request.Form["N"] != null)
                    {

                        var sts = Request.Form["N"];
                        foreach (var st in sts.ToString().Split(','))
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
                                int n = sts.ToString().Split(',').Length;//Request.Form.GetValues("N").Length;
                                for (int st1 = 0; st1 < n; st1++)
                                {
                                    if (vals["init_ItemID".ToLower() + st] == vals["init_ItemID".ToLower() + st1])
                                    {
                                        qty = qty + db.cNum(vals["init_Qty".ToLower() + st1]);
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
                                            strErr = strErr + " - " + rowItem["item_Name"] + "(" +
                                                qty.ToString() + " / " +
                                                db.cNum(rowItem["itwh_Qty"].ToString()) + ")\n";
                                        }
                                    }
                                }
                            }

                            if (strErr.Length > 0)
                            {
                                db.rollback();

                                tblResult.Rows[0]["status"] = "error";
                                tblResult.Rows[0]["msg"] = "Items out of stock : \n" + strErr;
                                re = ("{\"tbl\":" + db.tblToJson(tblResult) + "}");
                                return re;
                            }
                        }



                        foreach (var st in Request.Form["N"].ToString().Split(','))
                        {
                            aVal.Clear();
                            aVal.Add("init_InvoiceID", hid);

                            /*
                            if (vals["txtdel" + st] != "")
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);
                                cls.delRecord(screenItem, vals["init_InvoiceItemID".ToLower() + st], db);
                                if (!string.IsNullOrEmpty(v["init_InvoiceItemID".ToLower()]))
                                {

                                }
                            }
                            else*/
                            {
                                Dictionary<string, string> v = cls.getItemVals(screenItem, vals, db, st);

                                double exQty = 0;

                                if (v.ContainsKey("inid_InvoiceItemID".ToLower()))
                                {
                                    exQty = db.cNum(db.readData("init_Qty",
                                        "Select init_Qty From tblInvoiceItem Where init_SaleOrderItemID=" + v["inid_InvoiceItemID".ToLower()]));
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

                                if (v.ContainsKey("init_Total".ToLower()))
                                {
                                    v["init_Total".ToLower()] = (quit_Qty * quit_Price).ToString();
                                }

                                if (!v.ContainsKey("init_invoiceitemid".ToLower() + st))
                                {
                                    aVal.Add("init_BQty", quit_Qty.ToString());
                                }
                                aVal.Add("init_RPrice", quit_Price.ToString());

                                double init_Cost = db.cNum(db.readData("item_Cost", "Select item_Cost From tblItem " +
                                    " Where item_ItemID = " + v["init_ItemID".ToLower()]));
                                aVal.Add("init_Cost", init_Cost.ToString());

                                re2 = cls.saveRecord(screenItem, v, db, aVal, st,ignoreROF:true);
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
                    //invoiceTotal(hid, db);
                    new clsGlobal().invoiceTotal(hid, db);

                    if (!isCredit)
                    {

                        vals.Clear();
                        aVal.Clear();

                        double invo_Total = 0;
                        string whid = "";
                        DataTable tblInv =   db.readData("Select * From tblInvoice Where invo_Deleted is null and invo_InvoiceID=" + hid);
                        foreach (DataRow rowInv in tblInv.Rows)
                        {
                            invo_Total = db.cNum(rowInv["invo_GTotal"].ToString());
                            whid = rowInv["invo_WarehouseID"].ToString();
                        }
                        vals.Add("ivpm_Date".ToLower(), db.getDate(DateTime.UtcNow.AddHours(7).ToString("yyyy-MM-dd"), 1));
                        vals.Add("ivpm_InvoiceID".ToLower(), hid);
                        vals.Add("ivpm_Amount".ToLower(), invo_Total.ToString());
                        vals.Add("ivpm_PaymentType".ToLower(), "Cash");

                        aVal.Add("ivpm_WarehouseID", whid);
                        re2 = cls.saveRecord("tblInvoicePaymentNew", vals, db, aVal,ignoreROF:true);
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
                                var tmp = db.execData("Update tblInvoice Set " +
                                " invo_PaidAmount = isNull(invo_PaidAmount,0) + " + invo_Total +
                                ",invo_Balance = isnull(invo_GTotal,0) - isnull(invo_PaidAmount,0) - " + invo_Total +
                                ",invo_isPaid = 'Y' " +
                                " Where invo_InvoiceID = " + hid);
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


                    db.commit();
                }
            }
            return re;
        }


        string getJSONtbl(String sql, string searchStr, string ord, int recPage, string cdb)
        {
            string re = "";
            double cPage = 0;
            //recPage = 1000;
            sapi.db db = new sapi.db();
            try
            {

                cPage = db.cNum(Request.Form["cPage"].ToString());
                if (db.connect())
                {
                    string search = "";
                    if (Request.Form["search"] != null)
                    {
                        /*
                        if (!string.IsNullOrEmpty(search))
                            search = searchStr;*/ //-- comment by Ty

                        //--- add by Tyt
                        if (!string.IsNullOrEmpty(searchStr))
                            search = searchStr;



                        //" and (lower(item_Name) like " + db.sqlStrLike(search.Trim().ToLower()) +
                        //" )";
                    }

                    if (Request.Form["sql"] != null)
                    {
                        if (!string.IsNullOrEmpty(Request.Form["sql"].ToString()))
                            search = search + " and " + Request.Form["sql"];
                    }
                    DataTable tbl = db.readData(
                        " Declare @cPage int = " + cPage + "; " +
                        " Declare @RecPage int = " + recPage + "; " +
                        "Select * from " +
                        "( Select Row_number() over (order by " + ord + ") ROWID,* from " +
                        //" vItem where item_Deleted is null " + search + " ) tblItem " +
                        sql + search + " ) tbl" +
                        (cPage == -1 ? "" : " Where ROWID> ((@cPage * @RecPage) - @RecPage) " +
                        " and ROWID<=(@cPage * @RecPage) ") +
                        " order by " + ord
                        );

                    DataTable tblNext = db.readData(
                        " Declare @cPage int = " + (cPage + 1) + "; " +
                        " Declare @RecPage int = " + recPage + "; " +
                        "Select * from " +
                        "( Select Row_number() over (order by " + ord + ") ROWID,* from " +
                        //"vItem where item_Deleted is null " + search + " ) tblItem " +
                        sql + search + " ) tbl" +
                        (cPage == -1 ? "" : " Where ROWID> ((@cPage * @RecPage) - @RecPage) " +
                        " and ROWID<=(@cPage * @RecPage) ") +
                        " order by " + ord
                        );
                    tbl.Columns.Add("status");
                    if (tbl.Rows.Count > 0)
                    {
                        if (tblNext.Rows.Count <= 0)
                        {
                            tbl.Rows[0]["status"] = "oef";
                        }
                    }
                    string tmp = db.tblToJson(tbl);
                    Response.Write("{" + "\"tbl\" :" + tmp +
                        "}");

                }
            }
            catch (Exception ex)
            {
                Response.Write("{\"tbl\":[{\"status\":\"error\",\"msg\":\"" + ex.Message + "\"}]}");
            }
            finally
            {
                db.close();
            }

            return re;
        }

    }
}