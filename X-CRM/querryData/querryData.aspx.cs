using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;

namespace X_CRM.querryData
{
    public partial class querryData : System.Web.UI.Page
    {
        System.Collections.Specialized.NameValueCollection url;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["app"] != null)
            {
                if (Request.Form["app"].ToString() == "getChartAccount")
                {
                    string id = Request.Form["chac_chartaccountid"];
                    sapi.db db = new sapi.db();
                    if (db.connect())
                    {
                        DataTable chac = db.readData("SELECT chac_ChartAccountID, chac_Code, chac_Name " +
                                                    " FROM tblChartAccount WHERE chac_ChartAccountID = " + id);
                        if (chac != null)
                            Response.Write(db.tblToJson(chac));
                    }
                    db.close();
                }

                if (Request.Form["app"].ToString() == "getJournalDetailList")
                {
                    string re = //"<table class='table border bordered striped hovered'>" +
                                "<table class='table border'>" +
                                    "<thead>" +
                                        "<tr>" +
                                            "<th class='sortable-column sort-asc' onclick='sortClick()'>Journal No.</th>" +
                                            "<th class='sortable-column onclick='sortClick()'>Date</th>" +
                                            "<th class='sortable-column onclick='sortClick()'>Account No.</th>" +
                                            "<th class='sortable-column onclick='sortClick()'>Account Name</th>" +
                                            "<th class='sortable-column onclick='sortClick()'>Debit</th>" +
                                            "<th class='sortable-column onclick='sortClick()'>Credit</th>" +
                                        "</tr>" +
                                    "</thead><tbody>";

                    string JournalNo = Request.Form["jour_journalno"].Trim();
                    string JournalDate = Request.Form["jour_date"].Trim();
                    string JournalDate_To = Request.Form["jour_date_to"].Trim();
                    string JournalDesc = Request.Form["jour_desc"].Trim();

                    string sqlStr = "";

                    sapi.db db = new sapi.db();
                    if (db.connect())
                    {
                        if (!string.IsNullOrEmpty(JournalNo)) sqlStr += " and jour_JournalNo = " + db.sqlStr(JournalNo);
                        if (!string.IsNullOrEmpty(JournalDesc)) sqlStr += " and lower(jour_Desc) LIKE " + db.sqlStr("%" + JournalDesc.ToLower() + "%");
                        if (!string.IsNullOrEmpty(JournalDate))
                        {
                            if (!string.IsNullOrEmpty(JournalDate_To))
                            {
                                //JournalDate = cls.getDate(Request.Form["tklv_To".ToLower() + n], 0);
                                sqlStr += " and cast(jour_Date as date) BETWEEN " + db.sqlStr(db.getDate(JournalDate, 0)) + " and " + db.sqlStr(db.getDate(JournalDate_To, 0));
                            }
                            else
                                //JournalDate = db.getDate(JournalDate, 0);//format to yyyy-MM-dd
                                sqlStr += " and cast(jour_Date as date) = " + db.sqlStr(db.getDate(JournalDate, 0));
                        }
                        sapi.sapi cls = new sapi.sapi();
                        DataTable tblJournal = db.readData("SELECT jour_JournalID, jour_Date, jour_JournalNo, jour_Desc, joud_JournalDetailID, chac_Code, joud_CAName, isnull(joud_Debit,0)joud_Debit, isnull(joud_Credit,0)joud_Credit " +
                                                        " FROM vJournalDetail " +
                                                        " WHERE 1=1 " + sqlStr +
                                                        " ORDER BY jour_JournalID, jour_Date ASC, joud_Debit DESC, joud_Credit ASC "
                                                        );

                        string journalID = "";
                        int stripedRow = 0;
                        Int16 stripedCount = 0;
                        double debit = 0;
                        double credit = 0;
                        string trStyle = "";
                        string tdStyle = " border-bottom: 1px dashed  #999999;";

                        foreach (DataRow rowJournal in tblJournal.Rows)
                        {
                            debit += double.Parse(rowJournal["joud_Debit"].ToString());
                            credit += double.Parse(rowJournal["joud_Credit"].ToString());

                            if (journalID != rowJournal["jour_JournalID"].ToString()) { stripedCount = 1; stripedRow += 1; }
                            else stripedCount += 1;

                            DataRow[] jour = tblJournal.Select("jour_JournalID=" + db.cNum(rowJournal["jour_JournalID"].ToString()));
                            int oneRow = jour.Length;

                            if (stripedCount <= oneRow)
                            {
                                if (stripedRow % 2 == 0)
                                    trStyle = " background: #F2F2F2;";
                            }

                            re += "<tr style='" + trStyle + "'>" +
                                        "<td>" +
                                        "<a href='" + url + "/accounting/journal.aspx?jour_journalid=" + rowJournal["jour_JournalID"].ToString() + "' target='_blank'>" +
                                        (db.cNum(journalID) == db.cNum(rowJournal["jour_JournalID"].ToString()) ? "" : rowJournal["jour_JournalNo"].ToString()) +
                                        "</a>" +
                                        "</td>" +
                                        "<td>" +
                                        "<a href='" + url + "/accounting/journal.aspx?jour_journalid=" + rowJournal["jour_JournalID"].ToString() + "' target='_blank'>" +
                                        (db.cNum(journalID) == db.cNum(rowJournal["jour_JournalID"].ToString()) ? "" : db.getDate(DateTime.Parse(rowJournal["jour_Date"].ToString()).ToString("yyyy-MM-dd"), 1)) +
                                        "</a>" +
                                        "</td>" +
                                        "<td style='text-align: right;" + tdStyle + "'>" + rowJournal["chac_Code"].ToString() + "</td>" +
                                        "<td style='" + tdStyle + "'>" + rowJournal["joud_CAName"].ToString() + "</td>" +
                                        "<td style='text-align: right;" + tdStyle + "'>" + (db.cNum(rowJournal["joud_Debit"].ToString()) <= 0 ? "-" : db.cNum(rowJournal["joud_Debit"].ToString()).ToString(cls.numFormat)) + "</td>" +
                                        "<td style='text-align: right;" + tdStyle + "'>" + (db.cNum(rowJournal["joud_Credit"].ToString()) <= 0 ? "-" : db.cNum(rowJournal["joud_Credit"].ToString()).ToString(cls.numFormat)) + "</td>" +
                                        "</tr>";

                            if (stripedCount == oneRow)
                            {
                                string desc = "";
                                desc = rowJournal["jour_Desc"].ToString();
                                if (!string.IsNullOrEmpty(JournalDesc))
                                {
                                    string desc1 = "";
                                    int start = desc.ToLower().IndexOf(JournalDesc.ToLower());

                                    desc1 = desc.Substring(start, JournalDesc.Length);
                                    desc = desc.Substring(0, start) +
                                            "<strong>" + desc1 + "</strong>" +
                                            desc.Substring((JournalDesc.Length + start), desc.Length - JournalDesc.Length - start);
                                }

                                re += "<tr style='" + trStyle + "'><td colspan=6 style='color: #9fa1a0 ;" + tdStyle + "'>" + desc + "</td></tr>";
                            }

                            journalID = rowJournal["jour_JournalID"].ToString();
                        }
                        re += "</tbody>";
                        re += "<tfoot>" +
                                "<tr><td colspan=4 style='text-align: right'>TOTAL</td>" +
                                    "<td style='text-align: right'><u>" + db.cNum(debit.ToString()).ToString(cls.numFormat) + "</u></td>" +
                                    "<td style='text-align: right'><u>" + db.cNum(credit.ToString()).ToString(cls.numFormat) + "</u></td></tr>" +
                                "</tfoot>";
                        re += "</table>";
                        Response.Write("ok" + re);
                    }
                    db.close();
                }


                if (Request.Form["app"].ToString() == "getBSList")
                {
                    string re = //"<table class='table border bordered striped hovered'>" +
                                "<table class='table border'>" +
                                    "<thead>" +
                                        "<tr>" +
                                            "<th colspan=2 style='width: 50%; color: #0b1bb4; background:  #5aacf8;' class='sortable-column onclick='sortClick()'>Assets</th>" +
                                            "<th colspan=2 style='width: 50%; color: #23782b; background:  #affab6;' class='sortable-column onclick='sortClick()'>Liabilities and Owner's Equity</th>" +
                                        "</tr>" +
                                    "</thead><tbody>";

                    string JournalDate = Request.Form["jour_date"].Trim();

                    string sqlStr = "";

                    sapi.db db = new sapi.db();
                    if (db.connect())
                    {

                        if (!string.IsNullOrEmpty(JournalDate))
                        {
                            sqlStr += " and year(jour_Date)=year(" + db.sqlStr(db.getDate(JournalDate, 0)) +
                                    ") and cast(jour_Date as date) <= cast(" + db.sqlStr(db.getDate(JournalDate, 0)) + " as date)";
                        }

                        sapi.sapi cls = new sapi.sapi();

                        //string sqlQuerry = "SELECT chac_Code, chac_Name, chac_Account, sum(debit)debit, sum(credit)credit, sum(dc)dc " +
                        //                    " FROM vJournalBS " +
                        //                    " WHERE 1=1 " + sqlStr +
                        //                    " GROUP BY chac_Code, chac_Name, chac_Account  ";

                        string sqlQuerry = "";
                        sqlQuerry = " SELECT chac_Code, chac_Name, chac_Account, sum(debit)debit, sum(credit)credit, sum(dc)dc " +
                                    " FROM vJournalBS " +
                                    " WHERE 1=1 and chac_Account<>'Revenue' and chac_Account<>'Expenses' " + sqlStr +
                                    " GROUP BY chac_Code, chac_Name, chac_Account " +
                                    " UNION" +
                                    " SELECT null,'Net Income','E', sum(case when chac_Account='Expenses' then dc else 0 end)debit, " +
                                            " sum(case when chac_Account='Revenue' then dc else 0 end)credit, " +
                                            " (sum(case when chac_Account='Revenue' then dc else 0 end)-sum(case when chac_Account='Expenses' then dc else 0 end))dc " +
                                    " FROM vJournalBS " +
                                    " WHERE 1=1 " + sqlStr +
                                    " ORDER BY chac_Account DESC, chac_Code ASC";

                        DataTable tblJournalBS = db.readData(sqlQuerry);

                        double debit = 0;
                        double credit = 0;
                        string re1 = "";
                        string re2 = "";

                        string tdStyle = " border-bottom: 1px dashed  #999999;";
                        string numColor = "";
                        foreach (DataRow rowJournalBS in tblJournalBS.Rows)
                        {
                            numColor = (db.cNum(rowJournalBS["dc"].ToString()) < 0 ? "color: red;" : "");
                            if (rowJournalBS["chac_Account"].ToString() == "Asset")
                            {
                                debit += db.cNum(rowJournalBS["dc"].ToString());
                                re1 += "<tr>";
                                re1 += "<td style='" + tdStyle + "'>" + rowJournalBS["chac_Name"].ToString() + "</td>" +
                                        "<td style='text-align: right;" + tdStyle + numColor + "'>" +
                                            cAccountNum(rowJournalBS["dc"].ToString(), "", db, cls) +
                                        "</td>";
                                re1 += "</tr>";
                            }
                            else
                            {
                                credit += db.cNum(rowJournalBS["dc"].ToString());
                                re2 += "<tr>";
                                re2 += "<td style='" + tdStyle + "'>" + rowJournalBS["chac_Name"].ToString() + "</td>" +
                                        "<td style='text-align: right;" + tdStyle + numColor + "'>" +
                                            cAccountNum(rowJournalBS["dc"].ToString(), "", db, cls) +
                                        "</td>";
                                re2 += "</tr>";
                            }

                            //if (rowJournalBS["chac_Account"].ToString() == "Revenue")
                            //{
                            //    credit += db.cNum(rowJournalBS["dc"].ToString());
                            //    iStatement += db.cNum(rowJournalBS["dc"].ToString());
                            //}
                            //if (rowJournalBS["chac_Account"].ToString() == "Expenses")
                            //{
                            //    credit -= db.cNum(rowJournalBS["dc"].ToString());
                            //    iStatement -= db.cNum(rowJournalBS["dc"].ToString());
                            //}
                        }

                        //re2 += "<tr>";
                        //re2 += "<td style='" + tdStyle + "'>Net Income</td>" +
                        //        "<td style='text-align: right;" + tdStyle + "'>" +
                        //            db.cNum(iStatement.ToString()).ToString(cls.numFormat) +
                        //        "</td>";
                        //re2 += "</tr>";
                        numColor = (db.cNum(debit.ToString()) < 0 ? "color: red;" : "");
                        re1 += "</tbody></table>";
                        re2 += "</tbody></table>";
                        re += "<tr><td colspan=2 valign='top' align='left' style='border-right: 1px solid  #999999;'><table class='table'><tbody>" + re1 + "</td>" +
                                    "<td colspan=2 valign='top' align='left'><table class='table'><tbody>" + re2 + "</td></tr>";
                        re += "</tbody>";
                        re += "<tfoot>" +
                                "<tr>" +
                                    "<td style='text-align: left'>TOTAL Assets</td>" +
                                    "<td style='text-align: right; border-right: 1px solid #999999;" + numColor + "'><u>" +
                                        cAccountNum(debit.ToString(), "", db, cls) + "</u></td>";

                        numColor = (db.cNum(credit.ToString()) < 0 ? "color: red;" : "");
                        re += "<td style='text-align: left'>TOTAL Liabilities and Owner's Equity</td>" +
                                    "<td style='text-align: right;" + numColor + "'><u>" +
                                        cAccountNum(credit.ToString(), "", db, cls) + "</u></td></tr>" +
                                "</tfoot>";
                        re += "</table>";
                        Response.Write("ok" + re);
                    }
                    db.close();
                }


                if (Request.Form["app"].ToString() == "getISList")
                {
                    string JournalDate = Request.Form["jour_date"].Trim();
                    string JournalDate_To = Request.Form["jour_date_to"].Trim();

                    string sqlStr = "";

                    sapi.db db = new sapi.db();
                    if (db.connect())
                    {

                        if (!string.IsNullOrEmpty(JournalDate))
                        {
                            if (!string.IsNullOrEmpty(JournalDate_To))
                            {
                                sqlStr += " and cast(jour_Date as date) BETWEEN cast(" + db.sqlStr(db.getDate(JournalDate, 0)) +
                                                                            " as date) and cast(" + db.sqlStr(db.getDate(JournalDate_To, 0)) + " as date)";
                            }
                            else
                            {
                                sqlStr += " and cast(jour_Date as date) = " + db.sqlStr(db.getDate(JournalDate, 0));
                            }
                        }

                        sapi.sapi cls = new sapi.sapi();
                        DataTable tblJournalIS = db.readData("SELECT * " +
                                                        " FROM vJournalIS " +
                                                        " WHERE 1=1 " + sqlStr +
                                                        " ORDER BY chac_Account DESC, chac_Code" +
                                                        "");
                        double debit = 0;
                        double credit = 0;

                        string tdStyle = " border-bottom: 1px dashed  #999999;";

                        string re = //"<table class='table border bordered striped hovered'>" +
                                    "<table class='table border'>" +
                                        "<thead>" +
                                            "<tr><th colspan=2 style='color: #5a8166; background:   #c0e7cc;'>Profit & Loss</th></tr>" +
                                        "</thead><tbody>";

                        int i = 0;
                        int countRevenue = tblJournalIS.Select(" chac_Account='Revenue' ").Length;
                        int countExpense = tblJournalIS.Select(" chac_Account='Expenses' ").Length;
                        string ac = "";

                        foreach (DataRow rowJournalIS in tblJournalIS.Rows)
                        {
                            i += 1;
                            string numColor = "";

                            if (rowJournalIS["chac_Account"].ToString() == "Expenses")
                                debit += db.cNum(rowJournalIS["dc"].ToString());
                            else
                                credit += db.cNum(rowJournalIS["dc"].ToString());

                            numColor = (db.cNum(rowJournalIS["dc"].ToString()) < 0 ? "color: red;" : "");
                            re += "<tr>";
                            re += "<td style='" + tdStyle + "padding-left: 30px;'>" + rowJournalIS["chac_Name"].ToString() + "</td>" +
                                    "<td style='text-align: right; padding-right: 30px;" + tdStyle + numColor + "'>" +
                                    cAccountNum(rowJournalIS["dc"].ToString(), "", db, cls) + "</td>";
                            re += "</tr>";

                            if (i == countRevenue && rowJournalIS["chac_Account"].ToString() == "Revenue")
                            {
                                numColor = (db.cNum(credit.ToString()) < 0 ? "color: red;" : "");
                                re += "<tr><td style='border-bottom: 1px solid  #999999;'><strong>" +
                                            rowJournalIS["chac_Account"].ToString() + "</strong></td>" +
                                        "<td style='text-align: right;border-bottom: 1px solid  #999999;" + numColor + "'><strong>" +
                                            cAccountNum(credit.ToString(), "", db, cls) + "</strong></td></tr>";
                            }

                            if (i == countRevenue + countExpense && rowJournalIS["chac_Account"].ToString() == "Expenses")
                            {
                                numColor = (db.cNum(debit.ToString()) < 0 ? "color: red;" : "");
                                re += "<tr><td style='border-bottom: 1px solid  #999999;'><strong>" +
                                            rowJournalIS["chac_Account"].ToString() + "</strong></td>" +
                                        "<td style='text-align: right;border-bottom: 1px solid  #999999;" + numColor + "'><strong>" +
                                            cAccountNum(debit.ToString(), "", db, cls) + "</strong></td></tr>";
                            }

                            ac = rowJournalIS["chac_Account"].ToString();
                        }

                        re += "</tbody>";
                        if (tblJournalIS.Rows.Count > 0)
                        {
                            string numColor = "color: blue;";
                            if (credit - debit < 0)
                                numColor = "color: red;";
                            re += "<tfoot><tr>" +
                                "<td style='text-align: left;'>Net Income</td>" +
                                "<td style='text-align: right;" + numColor + "'><u>" +
                                    cAccountNum((credit - debit).ToString(), "", db, cls) +
                                "</u></td>" +
                            "</tr></tfoot>";
                        }
                        re += "</table>";
                        Response.Write("ok" + re);
                    }
                    db.close();

                }
            }

        }

        string cAccountNum(string oriNum, string currency, sapi.db db, sapi.sapi cls)
        {
            string accNum = "";

            if (db.cNum(oriNum.ToString()) > 0)
            {
                accNum = db.cNum(oriNum.ToString()).ToString(cls.numFormat);
            }
            else if (db.cNum(oriNum.ToString()) < 0)
            {
                accNum = currency + "(" + Math.Abs(db.cNum(oriNum.ToString())).ToString(cls.numFormat) + ")";
            }
            else
            {
                accNum = "-";
            }

            return accNum;
        }

        string WordInBetween(string sentence, string wordOne, string wordTwo)
        {

            int start = sentence.IndexOf(wordOne) + wordOne.Length + 1;

            int end = sentence.IndexOf(wordTwo) - start - 1;

            return sentence.Substring(start, end);


        }



        void invoiceTotal(string eid, sapi.db db)
        {
            string invo_Discount = "";
            double invo_Disc = 0;
            double invo_SubTotal = 0;
            double invo_DiscountAmount = 0;
            double invo_Total = 0;
            double invo_IsTax = 0;
            double invo_Tax = 0;
            double invo_GTotal = 0;

            DataTable tbl = db.readData("Select * From tblInvoice " +
                " Where invo_Deleted is null and invo_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_Discount = row["invo_Discount"].ToString();
                invo_Disc = db.cNum(row["invo_Disc"].ToString());
                invo_IsTax = db.cNum(row["invo_IsTax"].ToString());

            }

            tbl = db.readData("Select SUM(isNull(init_Total,0)) init_Total From tblInvoiceItem " +
                " Where init_Deleted is null and init_InvoiceID = " + eid);
            foreach (DataRow row in tbl.Rows)
            {
                invo_SubTotal = db.cNum(row["init_Total"].ToString());
                if (invo_Discount == "P")
                {
                    invo_DiscountAmount = (invo_SubTotal * invo_Disc / 100);
                }
                else
                {
                    invo_DiscountAmount = invo_Disc;
                }
                invo_Total = invo_SubTotal - invo_DiscountAmount;

                //Tax Calculation
                invo_Tax = invo_Total * invo_IsTax / 100;
                invo_GTotal = invo_Total + invo_Tax;

                db.execData("Update tblInvoice Set " +
                    " invo_SubTotal = " + invo_SubTotal +
                    ",invo_DiscountAmount = " + invo_DiscountAmount +
                    ",invo_Total = " + invo_Total +
                    ",invo_GTotal = " + invo_GTotal +
                    ",invo_Tax = " + invo_Tax +
                    ",invo_Balance = " + invo_GTotal + " - isNull(invo_PaidAmount,0) " +
                    " Where invo_InvoiceID = " + eid
                    );
            }
        }

    }
}