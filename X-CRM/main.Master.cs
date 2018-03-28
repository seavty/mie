using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace X_CRM
{
    public partial class main : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            

            if (Session["SID"] == null)
            {
                Response.Redirect("~/Default.aspx?app=session");
            }
            if (Session["user"] != null)
            {
                lblUserName.InnerHtml = Session["user"].ToString();
            }

            if (System.IO.File.Exists(Server.MapPath("~/imgs/" + Session["userid"].ToString() + ".png")))
            {
                imgProfile.Src = "~/imgs/" + Session["userid"].ToString() + ".png";
            }
            else
            {
                if (Session["gender"] != null)
                {
                    if (Session["gender"].ToString().ToLower() == "m")
                        imgProfile.Src = "~/imgs/male.jpg";
                    else
                        imgProfile.Src = "~/imgs/female.jpg";
                }
            }

            string tmp = ConfigurationManager.AppSettings["numFormatJS"];
            if (tmp != null) numFormat.Value = tmp;
        }
    }
}