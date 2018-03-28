using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace X_CRM
{
    public partial class About : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            sapi.sapi cls = new sapi.sapi();
            string comp = cls.getCompany();
            if(!string.IsNullOrEmpty(comp))
            {
                lblInfo.InnerHtml = "<strong>" + comp + "</strong>";
                ulModules.InnerHtml = cls.getModules();
            }else{
                lblInfo.InnerHtml = "<strong style='color:RED'>No License !</strong>&nbsp;<a href='javascript:alert(\"" + cls.getUUID() + "\")'>Register</a>";

            } 
        }
    }
}