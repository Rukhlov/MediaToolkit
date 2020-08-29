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



    }
}
