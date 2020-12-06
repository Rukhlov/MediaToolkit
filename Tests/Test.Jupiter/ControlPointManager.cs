using MediaToolkit.Jupiter;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.IO;

namespace Test.Jupiter
{
    public class ControlPointManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private CPClient client = null;

        public string Host => client?.Host ?? "";
        public int Port => client?.Port ?? -1;
        public NetworkCredential Credential { get; private set; }

        private volatile bool started = false;
        public bool Opened => started;

        public async Task Start(string host, int port, NetworkCredential credential = null)
        {
            if (running)
            {
                throw new InvalidOperationException("running == " + running);
            }

            if(credential == null)
            {
                credential = new NetworkCredential("admin", "");
            }

            this.Credential = credential;

            Debug.Assert(client == null, "pClient == null");

            client = new CPClient();
            client.NotificationReceived += CpClient_NotificationReceived;
            client.StateChanged += CpClient_StateChanged;

            await Task.Run(async () => 
            {
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
                            var clientTask = client.Connect(host, port).ConfigureAwait(false);

                            while (client.State == ClientState.Connecting && running)
                            {
                                Thread.Sleep(100);
                            }

                            if (client.State == ClientState.Connected)
                            {
                                await client.Authenticate(Credential.UserName, Credential.Password).ConfigureAwait(false);

                                var servInfo = await client.ConfigSys.GetServerInfo().ConfigureAwait(false);

                                // validate info...
                                logger.Info("CPServer Version: " + servInfo.CPVersion);

                                await client.WinServer.RegisterNotifyTarget().ConfigureAwait(false);
                                var windows = await client.WinServer.QueryAllWindows().ConfigureAwait(false);
                                foreach (var window in windows)
                                {
                                    logger.Info(window.WindowId + " " + Utils.LogEnumFlags((StateFlag)window.State) + " " + window.Rect);
                                }

                                var rgbWindows = windows.Where(w => w.Kind == SubSystemKind.RGBCapture);
                                foreach (var rgb in rgbWindows)
                                {
                                    logger.Info(rgb.ToString());

                                    // await cpClient.Window.GetState(rgb.)
                                }

                                started = true;
                            }

                            await clientTask;

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message);

                            Thread.Sleep(3000);
                        }

                    }
                }
                finally
                {
                    Close();
                }

            }).ConfigureAwait(false);
           
        }



        private void CpClient_StateChanged()
        {
            var state = client.State;

            logger.Debug("CpClient_StateChanged(...) " + state);
        }


        private bool running = false;

        private void CpClient_NotificationReceived(CPNotification notification)
        {
            logger.Debug("CpClient_NotificationReceived(...)");

            if (notification.ObjectName == "Notify")
            {
                if (notification.Method == "WindowsState")
                {// +Notify WindowsState { nCount TWindowState pData[ ] } (Id Kind nState nStateChange x y w h ZAfter)

                    var valueList = notification.ValueList;
                    var windows = new TWindowStateList(valueList);

                    foreach(var w in windows)
                    {
                        if (w.WindowId < 10000)
                        {
                            var state = w.State + " {" + Utils.LogEnumFlags((StateFlag)w.State) + "}";
                            var set = w.StateChange + " {" + Utils.LogEnumFlags((StateFlag)w.StateChange) + "}";
                            logger.Debug(string.Join(" ", w.WindowId, state, set, w.Rect));
                        }
                    }

                    //logger.Debug(windows.ToString());
                }
            }



        }

        public async Task<bool> OpenWindow(int windowId)
        {
            if (!running || !started)
            {
                throw new InvalidOperationException("Invalid state: " + running + " " + started);
            }

            var winId = new WinId(windowId);
            return await client.RGBSys.NewWindowWithId(winId);
        }

        public async Task<bool> ChangeWindow(int windowId, Rectangle rect)
        {
            if (!running || !started)
            {
                throw new InvalidOperationException("Invalid state: " + running + " " + started);
            }


            var winId = new WinId(windowId);

            var state = new TWindowState
            {
                Id = new WinId(windowId),

                Kind = SubSystemKind.RGBCapture,
                State = (uint)(StateFlag.wsVisible | StateFlag.wsFramed | StateFlag.wsAlwaysOnTop),//41
                StateChange = (uint)(StateFlag.wsVisible | StateFlag.wsSize | StateFlag.wsPosition | StateFlag.wsAlwaysOnTop),//3105

                x = rect.X,
                y = rect.Y,
                w = rect.Width,
                h = rect.Height,

                ZAfter = new WinId(-1),
            };

            var _state = await client.Window.SetState(state);

            return true;
        }

        public async Task<bool> SetWindowInput(int windowId, int inputId)
        {

            if (!running || !started)
            {
                throw new InvalidOperationException("Invalid state: "+ running + " " + started);
            }

            var winId = new WinId(windowId);
            bool result = await client.RGBSys.SetChannel(winId, inputId);
            if (!result)
            {
                throw new Exception("RGBSys.SetChannel() false");
            }

            result = await client.RGBSys.SetAutoDetectTiming(winId, true);
            if (!result)
            {
                throw new Exception("RGBSys.SetAutoDetectTiming() false");
            }

            //var timing = await client.RGBSys.DetectTiming(winId);

            result = await client.RGBSys.Start(winId);
            if (!result)
            {
                throw new Exception("RGBSys.Start() false");
            }

            return result;
        }


        public async Task<Bitmap> GetWindowPreview(int windowId)
        {
            if (!running || !started)
            {
                throw new InvalidOperationException("Invalid state: " + running + " " + started);
            }

            Bitmap bmp = null;
            var winId = new WinId(windowId);

            var preview = await client.Window.GetPreview(winId);
            if (preview != null)
            {
                bmp = preview.GetBitmap();
            }

            return bmp;
        }


        public async Task<Bitmap> GetWindowImage(int windowId)
        {
            if (!running || !started)
            {
                throw new InvalidOperationException("Invalid state: " + running + " " + started);
            }


            Bitmap bmp = null;
            var winId = new WinId(windowId);

            var imageName = await client.Window.GrabImage(winId);

            if (!string.IsNullOrEmpty(imageName))
            {// TODO: проверить файл это или только название
             // т.к в доках эта функция возвращает полый путь к файлу, а реальный сервер только имя!
             // может быть в зависимости от версии CPServer, функция работает по разному
                imageName = imageName.Replace("\"", "");

                var imageFile = Path.Combine(CPEnvironment.ImagesPath, imageName);

                if (File.Exists(imageFile))
                {
                    using (var _b = Bitmap.FromFile(imageFile))
                    {
                        bmp = new Bitmap(_b);
                    }

                    try
                    {
                        File.Delete(imageFile);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }    
                }
            }

            return bmp;
        }


        public void Stop()
        {
            running = false;

            if (client != null)
            {
                client.Disconnect();
            }
        }

        public void Close()
        {
            if (client != null)
            {
                client.NotificationReceived -= CpClient_NotificationReceived;
                client.StateChanged -= CpClient_StateChanged;
                client.Close();

                client = null;
            }
        }


    }
}
