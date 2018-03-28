using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
using System.Text;

namespace X_CRM.customer
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

                    if (Request.Form["app"].ToString() == "getCompInfo")
                        GetCompInfo();
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);
                db.close();
                cls.endRequest();
            }
        }

        private void GetCompInfo()
        {
            try
            {
                string sql = "SELECT  * FROm tblCustomer WHERE cust_Deleted IS NULL AND cust_CustomerID =" + db.sqlStr(Request.Form["compID"]);
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

        private void NumberOfReuireTechnician(string serviceOrderID)
        {
            string sql = "UPDATE tblServiceOrder SET seor_RequireTechnician = " +
                         " (SELECT count(*) AS CR FROM tblTechnician where teci_Deleted IS NULL and teci_ServiceOrderID =" + db.sqlStr(serviceOrderID) + ")" +
                         " WHERE seor_Deleted IS NULL AND seor_ServiceOrderID = " + db.sqlStr(serviceOrderID);
            db.execData(sql);
        }

        private void RemoveTechnician()
        {
            db.beginTran();
            try
            {
                string technicianID = Request.Form["technicianID"].ToString();
                string sql = "UPDATE tblTechnician SET teci_Deleted = 1 WHERE teci_TechnicianID = " + db.sqlStr(technicianID);
                db.execData(sql);

                sql = "SELECT * FROM tblTechnician WHERE /*teci_Deleted IS NULL AND */ teci_TechnicianID = " + db.sqlStr(technicianID);
                NumberOfReuireTechnician(db.readData(sql).Rows[0]["teci_ServiceOrderID"].ToString());

                db.commit();
                db.close();
                Response.Write("ok");
                cls.endRequest();
            }
            catch (Exception ex)
            {
                db.rollback();
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }

        }

        private void SubmitTechnician()
        {
            db.beginTran();
            try
            {
                string[] idArr = Request.Form["ids[]"].ToString().Split(',');
                string sqlInsert = "";
                foreach (string id in idArr)
                {
                    sqlInsert = "INSERT INTO tblTechnician" +
                                            " (teci_CreatedDate, teci_UserID, teci_ServiceOrderID)" +
                                            " VALUES(" +
                                                    "GETDATE()," +
                                                    db.sqlStr(id) + "," +
                                                    db.sqlStr(Request.Form["serviceOrderID"].ToString()) +
                                                ")";
                    db.execData(sqlInsert);
                }
                NumberOfReuireTechnician(Request.Form["serviceOrderID"].ToString());

                db.commit();
                db.close();
                Response.Write("ok");
                cls.endRequest();
            }
            catch (Exception ex)
            {
                db.rollback();
                db.close();
                Response.Write(ex.Message);
                cls.endRequest();
            }
        }

        private void LoadFormUser()
        {
            try
            {
                string sql = "SELECT * FROM sys_user" +
                             " WHERE user_Deleted IS NULL";
                DataTable tbl = db.readData(sql);
                StringBuilder sbTbl = new StringBuilder();

                sbTbl.Append("<button type=\"button\" id=\"btnAdd\" name=\"btnName\" class=\"button rounded success\" onClick=\"submitAdd()\"> <span class=\"mif-plus\"> </span> Add Tehnician </button>");
                sbTbl.Append("<table class='table border bordered striped hovered'>" +
                                "<thead>" +
                                    "<tr>" +
                                        "<th> User Name </th>" +
                                        "<th> First Name </th>" +
                                        "<th> Last Name </th>" +
                                        "<th style=\"width:20px;\"> Select </th>" +
                                    "</tr>" +
                                "</thead>" +
                                "<tbody>");
                foreach (DataRow row in tbl.Rows)
                {
                    string userID = row["user_userID"].ToString();
                    sbTbl.Append("<tr>" +
                        "<td>" + row["user_userName"].ToString() + "</td>" +
                        "<td>" + row["user_FirstName"].ToString() + "</td>" +
                        "<td>" + row["user_LastName"].ToString() + "</td>" +
                        "<td>" +
                            "<input type=\"checkbox\" name=userID" + userID +
                                    " id=userID" + userID +
                                    " userIDValue=\"" + userID + "\" class=\"userID\" />" +
                        "</td>" +
                    "</tr>");
                }
                sbTbl.Append("</tbody></table>");
                Response.Write(sbTbl.ToString());
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