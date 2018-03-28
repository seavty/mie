using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
namespace X_CRM.report
{
    public partial class crViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportDocument rpt = new ReportDocument();
            rpt.Load(HttpContext.Current.Server.MapPath("~/Report/CrystalReport1.rpt"));
            CR.ReportSource = rpt;
            CR.DisplayToolbar = true;

            rpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "ExportedReport");

            rpt.Dispose();
        }
    }
}