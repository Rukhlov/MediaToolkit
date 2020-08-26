using MediaToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Jupiter
{
    public class CPManager
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Jupiter");


        private CPClient cpClient = new CPClient();

        public NetworkCredential Credential { get; private set; } = new NetworkCredential("admin", "");

        public bool IsAuthenticated { get; private set; } = false;

        public void Open(string host, int port)
        {
            cpClient = new CPClient();
            cpClient.StateChanged += CpClient_StateChanged;
            cpClient.NotificationReceived += CpClient_NotificationReceived;

            cpClient.Connect(host, port);
        }

        private void CpClient_StateChanged()
        {
            var state = cpClient.State;

            logger.Debug("CpClient_StateChanged(...) " + state);


        }

        private void CpClient_NotificationReceived(CPNotification obj)
        {
            logger.Debug("CpClient_NotificationReceived(...)");


        }


        public void Close()
        {

        }


        public async Task<bool> AuthAsync(string user, string password)
        {

            Credential = new NetworkCredential(user, password);

            var authRequest = new CPRequest($"{Credential.UserName}\r\n{Credential.Password}\r\n");

            var resp = await cpClient.SendAsync(authRequest, 5000);

            IsAuthenticated = resp?.Success ?? false;

            return IsAuthenticated;

        }

    }
}
