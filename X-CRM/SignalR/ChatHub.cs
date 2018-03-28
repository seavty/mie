using System;
using System.Web;
using Microsoft.AspNet.SignalR;


namespace X_CRM.SignalR
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message, string connectionid)
        {
            // Call the broadcastMessage method to update clients.
            //Clients.All.broadcastMessage(name, message);
            Clients.All.addNewMessageToPage(name, message, connectionid);
        }
    }
}