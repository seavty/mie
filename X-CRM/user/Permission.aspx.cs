using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.user
{
    public partial class Permission : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            sapi.db db = new sapi.db();
            try
            {
                if (db.connect())
                {

                    if (Request.Form["app"] == null)
                    {
                        DataTable tblProfile = db.readData("Select * from sys_Profile ");
                        foreach (DataRow rowProfile in tblProfile.Rows)
                        {
                            cboProfile.Items.Add(new ListItem(rowProfile["prof_Name"].ToString(), rowProfile["prof_ProfileID"].ToString()));
                        }
                    }
                    else
                    {
                        if (Request.Form["app"].ToString() == "savePermission")
                        {
                            string re = "no";
                            db.beginTran();
                            if (Request.Form["tabl_tableid"] != null)
                            {
                                
                                foreach (var st in Request.Form.GetValues("tabl_tableid"))
                                {
                                    DataTable tbl = db.readData("Select pfpm_ProfilePermissionID from sys_ProfilePermission " +
                                        " Where pfpm_TableID = " + st +
                                        " and pfpm_ProfileID = " + Request.Form["prof_profileid"].ToString());
                                    if (tbl.Rows.Count == 0)
                                    {
                                        re = db.execData("Insert into sys_ProfilePermission(pfpm_TableID,pfpm_ProfileID,pfpm_V,pfpm_I,pfpm_E,pfpm_D) " +
                                            "VALUES(" + st + "," +
                                                Request.Form["prof_profileid"].ToString() + "," +
                                                (Request.Form["pfpm_V" + st] == null ? "NULL" : "'Y'") + "," +
                                                (Request.Form["pfpm_I" + st] == null ? "NULL" : "'Y'") + "," +
                                                (Request.Form["pfpm_E" + st] == null ? "NULL" : "'Y'") + "," +
                                                (Request.Form["pfpm_D" + st] == null ? "NULL" : "'Y'") +
                                            ")");
                                        if (re != "ok")
                                        {
                                            Response.Write(re);
                                            db.close();
                                            new sapi.sapi().endRequest();
                                            Response.End();
                                        }
                                    }
                                    else
                                    {
                                        re = db.execData("Update sys_ProfilePermission set " +
                                                " pfpm_V = " + (Request.Form["pfpm_V" + st] == null ? "NULL" : "'Y'") +
                                                ",pfpm_I = " + (Request.Form["pfpm_I" + st] == null ? "NULL" : "'Y'") +
                                                ",pfpm_E = " + (Request.Form["pfpm_E" + st] == null ? "NULL" : "'Y'") +
                                                ",pfpm_D = " + (Request.Form["pfpm_D" + st] == null ? "NULL" : "'Y'") +
                                            " Where pfpm_ProfilePermissionID = " + tbl.Rows[0]["pfpm_ProfilePermissionID"].ToString());
                                        if (re != "ok")
                                        {
                                            Response.Write(re);
                                            db.close();
                                            new sapi.sapi().endRequest();
                                            Response.End();
                                        }
                                    }
                                }
                               
                            }

                            re = "no";
                            if (Request.Form["smnu_SubMenuID"] != null)
                            {

                                foreach (var st in Request.Form.GetValues("smnu_SubMenuID"))
                                {
                                    DataTable tbl = db.readData("Select rppm_ReportPermissionID from sys_ReportPermission " +
                                        " Where rppm_ReportID = " + st +
                                        " and rppm_ProfileID = " + Request.Form["prof_profileid"].ToString());
                                    if (tbl.Rows.Count == 0)
                                    {
                                        re = db.execData("Insert Into sys_ReportPermission(rppm_ReportID,rppm_ProfileID,rppm_V,rppm_VO,rppm_Type) " +
                                            "VALUES(" + st + "," +
                                                Request.Form["prof_profileid"].ToString() + "," +
                                                (Request.Form["rppm_V" + st] == null ? "NULL" : "'Y'") + "," +
                                                (Request.Form["rppm_VO" + st] == null ? "NULL" : "'Y'") + "," +
                                                Request.Form["_type" + st].ToString() +
                                            ")");
                                        if (re != "ok")
                                        {
                                            Response.Write(re);
                                            db.close();
                                            new sapi.sapi().endRequest();
                                            Response.End();
                                        }
                                    }
                                    else
                                    {
                                        re = db.execData("Update sys_ReportPermission set" +
                                                " rppm_V = " + (Request.Form["rppm_V" + st] == null ? "NULL" : "'Y'") +
                                                ",rppm_VO = " + (Request.Form["rppm_VO" + st] == null ? "NULL" : "'Y'") +
                                            " Where rppm_ReportPermissionID = " + tbl.Rows[0]["rppm_ReportPermissionID"].ToString());
                                        if (re != "ok")
                                        {
                                            Response.Write(re);
                                            db.close();
                                            new sapi.sapi().endRequest();
                                            Response.End();
                                        }

                                    }
                                }
                            }

                            db.commit();
                            re = "ok";
                            Response.Write(re);
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getReports")
                        {
                            string re = "";
                            if (Session["userid"].ToString() != "1")
                            {
                                re = ("<tr><td colspan='6'><div class=\"padding10 bg-red fg-white text-accent align-center\">You do not have permission to access this module !</div></td></tr>");

                            }
                            else
                            {
                                DataTable tblPermission = db.readData(
                                    "Select * from ( " +
                                    " select mmnu_MainMenuID smnu_SubMenuID,mmnu_Text smnu_Text,'1' _type from sys_mainMenu " +
                                    " where mmnu_GroupID = 4 and mmnu_Url is not null " +
                                    " UNION all " +
                                    " select smnu_SubMenuID,smnu_Text,'2' _type from sys_subMenu " +
                                    " where smnu_Url is not null and smnu_MainMenuID is not null " +
                                    " ) A " +
                                    " left join [dbo].[sys_ReportPermission] on rppm_ReportID = smnu_SubMenuID " +
                                    " and rppm_ProfileID = " + Request.Form["prof_profileid"].ToString());
                                foreach (DataRow rowPermission in tblPermission.Rows)
                                {
                                    re = re + "<tr n='" + rowPermission["smnu_SubMenuID"].ToString() + "'>" +
                                        "<td>" +
                                            rowPermission["smnu_Text"].ToString() +
                                            "<input type='hidden' name='smnu_SubMenuID' class='smnu_SubMenuID' value='" + rowPermission["smnu_SubMenuID"].ToString() + "'/>" +
                                            "<input type='hidden' name='_type" + rowPermission["smnu_SubMenuID"].ToString() + "' class='_type' value='" + rowPermission["_type"].ToString() + "'/>" +
                                        "</td>" +
                                        "<td>" +
                                           "<input class='rppm_VO' type='checkbox' " +
                                            " id='rppm_VO" + rowPermission["smnu_SubMenuID"].ToString() + "' " +
                                            " name='rppm_VO" + rowPermission["smnu_SubMenuID"].ToString() + "' " +
                                            " value='" + (string.IsNullOrEmpty(rowPermission["rppm_VO"].ToString()) ? "Y'" : "Y' checked='checked' ") + "/>" +
                                        "</td>" +

                                        "<td>" +
                                           "<input class='rppm_V' type='checkbox' " +
                                            " id='rppm_V" + rowPermission["smnu_SubMenuID"].ToString() + "' " +
                                            " name='rppm_V" + rowPermission["smnu_SubMenuID"].ToString() + "' " +
                                            " value='" + (string.IsNullOrEmpty(rowPermission["rppm_V"].ToString()) ? "Y'" : "Y' checked='checked' ") + "/>" +
                                        "</td>" +
                                        "</tr>";
                                }
                            }
                            Response.Write(re);
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                        if (Request.Form["app"].ToString() == "getPermission")
                        {
                            string re = "";
                            if (Session["userid"].ToString() != "1")
                            {
                                re = ("<tr><td colspan='6'><div class=\"padding10 bg-red fg-white text-accent align-center\">You do not have permission to access this module !</div></td></tr>");

                            }
                            else
                            {

                                DataTable tblPermission = db.readData("select pfpm_TableID,pfpm_ProfileID " +
                                    ",pfpm_V,pfpm_I,pfpm_E,pfpm_D,prof_ProfileID,prof_Name,tabl_Name,tabl_Text,tabl_TableID " +
                                " from sys_ProfilePermission " +
                                " inner join sys_Profile on pfpm_ProfileID = prof_ProfileID " +
                                " and prof_ProfileID = " + Request.Form["prof_profileid"].ToString() +
                                " right join sys_table on tabl_TableID = pfpm_TableID " +
                                " Order by tabl_Text ");
                                foreach (DataRow rowPermission in tblPermission.Rows)
                                {
                                    string _V = rowPermission["pfpm_V"].ToString();
                                    string _I = rowPermission["pfpm_I"].ToString();
                                    string _E = rowPermission["pfpm_E"].ToString();
                                    string _D = rowPermission["pfpm_D"].ToString();
                                    string _A = "<input class='pfpm_A' type='checkbox' eid='" + rowPermission["tabl_TableID"].ToString() + "' value='" + (string.IsNullOrEmpty(_D) ? "'" : "' checked='checked' ") + "/>";
                                    _V = "<input class='pfpm_V' type='checkbox' " +
                                        " id='pfpm_V" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " name='pfpm_V" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " value='" + (string.IsNullOrEmpty(_V) ? "Y'" : "Y' checked='checked' ") + "/>";
                                    _I = "<input class='pfpm_I' type='checkbox'" +
                                        " id='pfpm_I" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " name='pfpm_I" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " value='" + (string.IsNullOrEmpty(_I) ? "Y'" : "Y' checked='checked' ") + "/>";
                                    _E = "<input class='pfpm_E' type='checkbox' " +
                                        " id='pfpm_E" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " name='pfpm_E" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " value='" + (string.IsNullOrEmpty(_E) ? "Y'" : "Y' checked='checked' ") + "/>";
                                    _D = "<input class='pfpm_D' type='checkbox'" +
                                        " id='pfpm_D" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " name='pfpm_D" + rowPermission["tabl_TableID"].ToString() + "' " +
                                        " value='" + (string.IsNullOrEmpty(_D) ? "Y'" : "Y' checked='checked' ") + "/>";
                                    re = re + "<tr n='" + rowPermission["tabl_TableID"].ToString() + "'>" +
                                        "<td>" +
                                            rowPermission["tabl_Text"].ToString() +
                                            "<input type='hidden' name='tabl_tableid' class='tabl_tableid' value='" + rowPermission["tabl_TableID"].ToString() + "'/>" +
                                        "</td>" +
                                        "<td>" +
                                            _V +
                                        "</td>" +
                                        "<td>" +
                                            _I +
                                        "</td>" +
                                        "<td>" +
                                            _E +
                                        "</td>" +
                                        "<td>" +
                                            _D +
                                        "</td>" +
                                        "<td>" +
                                            _A +
                                        "</td>" +
                                        "</tr>";
                                }
                            }
                            Response.Write(re);
                            db.close();
                            new sapi.sapi().endRequest();
                            Response.End();
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write(ex.Message);

            }
            finally { db.close(); }
        }
    }
}