using MediaToolkit.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Jupiter
{
    public class CPManager
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Jupiter");


        private CPClient cpClient = null;

        public string Host => cpClient?.Host ?? "";
        public int Port => cpClient?.Port ?? -1;
        public NetworkCredential Credential { get; private set; }

        private volatile bool opened = false;
        public bool Opened => opened;

        public async Task Open(string host, int port, NetworkCredential credential = null)
        {

            if(credential == null)
            {
                credential = new NetworkCredential("admin", "");
            }

            this.Credential = credential;

            cpClient = new CPClient();
            cpClient.NotificationReceived += CpClient_NotificationReceived;
            cpClient.StateChanged += CpClient_StateChanged;
            try
            {
                if (!running)
                {
                    running = true;
                }

                while (running)
                {
                    try
                    {
                        var clientTask = cpClient.Connect(host, port).ConfigureAwait(false);

                        while(cpClient.State == ClientState.Connecting)
                        {
                            Thread.Sleep(100);
                        }

                        if(cpClient.State == ClientState.Connected)
                        {
                            await cpClient.Authenticate(Credential.UserName, Credential.Password).ConfigureAwait(false);

                            var servInfo = await cpClient.ConfigSys.GetServerInfo().ConfigureAwait(false);

                            // validate info...
                            logger.Info("CPServer Version: " + servInfo.CPVersion);

                            await cpClient.WinServer.RegisterNotifyTarget().ConfigureAwait(false);
                            var windows = await cpClient.WinServer.QueryAllWindows().ConfigureAwait(false);
                            foreach (var window in windows)
                            {
                                logger.Info(window.WindowId + " " + window.IsVisible + " " + window.IsMaximized + " " + window.IsMinimized + " " + window.Rect);
                  
                            }


                            var rgbWindows = windows.Where(w => w.Kind == SubSystemKind.RGBCapture);
                            foreach(var rgb in rgbWindows)
                            {
                                logger.Info(rgb.ToString());

                               // await cpClient.Window.GetState(rgb.)
                            }

                            opened = true;
                        }
 
                        await clientTask;

                    }
                    catch(Exception ex)
                    {
                        logger.Error(ex.Message);

                        Thread.Sleep(3000);
                    }

                }
            }
            finally
            {
                if (cpClient != null)
                {
                    cpClient.NotificationReceived -= CpClient_NotificationReceived;
                    cpClient.StateChanged -= CpClient_StateChanged;
                    cpClient.Close();

                    cpClient = null;
                }

            }
        }

        private void CpClient_StateChanged()
        {
            var state = cpClient.State;

            logger.Debug("CpClient_StateChanged(...) " + state);
        }


        private bool running = false;

        private void CpClient_NotificationReceived(CPNotification notification)
        {
            logger.Debug("CpClient_NotificationReceived(...) " + notification);


        }


        public void Close()
        {
            running = false;

            if (cpClient != null)
            {
                cpClient.Disconnect();
            }
        }



    }
}
