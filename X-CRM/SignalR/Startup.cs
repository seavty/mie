using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(X_CRM.SignalR.Startup))]

namespace X_CRM.SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //string sqlConnectionString = @"server=(local)\MSSQLSERVER12;uid=sa;pwd=123456;database=SignalR";
            //GlobalHost.DependencyResolver.UseSqlServer(sqlConnectionString);
            app.MapSignalR(); // check newtonsoft.json
        }
    }
}