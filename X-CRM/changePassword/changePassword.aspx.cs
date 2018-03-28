using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace X_CRM.changePassword
{
    public partial class changePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Form["app"] != null)
            {
                CryptLib _crypt = new CryptLib();
                string plainText = Request.Form["OldPwd"].ToString();
                string iv = "Xsoft";// CryptLib.GenerateRandomIV(16); //16 bytes = 128 bits
                string key = CryptLib.getHashSha256("@XSoft201701", 31); //32 bytes = 256 bits

                string password = _crypt.encrypt(plainText, key, iv);
                sapi.db db = new sapi.db();

                try
                {
                    if (db.connect())
                    {
                        DataTable tbl = db.readData("Select * from sys_user Where user_userID = " + db.sqlStr(Session["userid"].ToString()));
                        foreach (DataRow row in tbl.Rows)
                        {
                            if (row["user_Password"].ToString() != password)
                            {
                                db.close();
                                Response.Write("wrongpwd");
                                new sapi.sapi().endRequest();
                                Response.End();
                            }
                            else
                            {
                                password = _crypt.encrypt(Request.Form["NewPwd"].ToString(), key, iv);
                                var re = db.execData("Update sys_user Set user_Password = " +
                                    db.sqlStr(password) +
                                    " Where user_userID = " + db.sqlStr(Session["userid"].ToString()));
                                db.close();
                                Response.Write(re);
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
                finally
                {
                    db.close();
                }
            }
        }
    }
}