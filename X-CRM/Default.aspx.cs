using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;


using System.Configuration;
using System.Web.Configuration;


namespace X_CRM
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            sapi.sapi cls = new sapi.sapi();
            string comp = cls.getCompany();
            if (!string.IsNullOrEmpty(comp))
            {
                lblInfo.InnerHtml = "<strong>" + comp + "</strong>";
            }
            else
            {
                lblInfo.InnerHtml = "<strong style='color:RED'>No License !</strong>&nbsp;<a href='javascript:alert(\"" + cls.getUUID() + "\")'>Register</a>";

            } 

            //Response.Write(new sapi.sapi().getLic() + "<br/>");
            if (Request.QueryString["app"] != null)
            { 
                if (Request.QueryString["app"].ToString() == "logout")
                {
                    HttpCookie cookier = Request.Cookies["userSettings"];
                    if (cookier != null)
                    {
                        cookier.Expires = DateTime.Now.AddDays(-1);
                        Response.Cookies.Add(cookier);
                    }
                    Session.Abandon();
                }
                else
                {
                    checkCookie();
                }

                if (Request.QueryString["app"].ToString() == "session")
                {
                    dvInfo.InnerHtml = "Session Expired !";
                }
            }
            else
            {
                checkCookie();
            }

            
            

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
                                    Session["userid"] = tbl.Rows[0]["user_userID"].ToString();
                                    Session["user"] = tbl.Rows[0]["user_userName"].ToString();
                                    Session["SID"] = _crypt.encrypt(plainText, key, iv);
                                    Session["lang"] = "";
                                    Session["gender"] = tbl.Rows[0]["user_gender"].ToString();

                                    string prof = "";
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
                                    Session["profiles"] = prof;

                                    Response.Write("ok");
                                    if(Request.Form["chk"] != null)
                                        if (Request.Form["chk"].ToString() == "Y")
                                        {
                                            HttpCookie myCookie = new HttpCookie("userSettings");
                                            myCookie["sid"] = Session["SID"].ToString();
                                            myCookie.Expires = DateTime.Now.AddDays(7d);
                                            Response.Cookies.Add(myCookie);
                                        }

                                }
                                else
                                {
                                    Response.Write(re);
                                }
                            }
                            else
                            {
                                Response.Write("Invalid User Name or Password !");
                            }

                        }
                        else
                        {
                            Response.Write("Unable To Connect To Server !");
                        }
                    }
                    catch (Exception ex) { Response.Write(ex.Message); }
                    finally { db.close(); }

                }
                new sapi.sapi().endRequest();
                Response.End();

            }
        }

        void checkCookie()
        {
            HttpCookie cookier = HttpContext.Current.Request.Cookies["userSettings"];
            if (cookier != null)
            {
                string sid = (cookier.Values["sid"].ToString());
                sapi.db db = new sapi.db();
                try
                {
                    if (db.connect())
                    {
                        DataTable tbl = db.readData("Select * from sys_user Where user_SID=" + db.sqlStr(sid));
                        if (tbl.Rows.Count > 0)
                        {

                            Session["userid"] = tbl.Rows[0]["user_userID"].ToString();
                            Session["user"] = tbl.Rows[0]["user_userName"].ToString();
                            Session["SID"] = sid;
                            Session["lang"] = "";
                            Session["gender"] = tbl.Rows[0]["user_gender"].ToString();

                            string prof = "";
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
                            Session["profiles"] = prof;

                            HttpCookie myCookie = new HttpCookie("userSettings");
                            myCookie["sid"] = Session["SID"].ToString();
                            myCookie.Expires = DateTime.Now.AddDays(7d);
                            Response.Cookies.Add(myCookie);
                            db.close();
                            Response.Redirect("Home/Default.aspx");


                        }
                    }
                }
                catch (Exception ex) { }
                finally
                {
                    db.close();
                }

            }
        }
    }
}