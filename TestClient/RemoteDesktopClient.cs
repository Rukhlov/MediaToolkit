using MediaToolkit.Common;
using MediaToolkit.UI;
using MediaToolkit.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    class RemoteDesktopClient : IRemoteDesktopClient
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenReceiver VideoReceiver { get; private set; } = new ScreenReceiver();
        public InputManager InputManager { get; private set; } = new InputManager();


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }
        public string ServerName { get; private set; }

        private ChannelFactory<IRemoteDesktopService> factory = null;


        public void Connect(string _addr)
        {
            var address = "net.tcp://" + _addr + "/RemoteDesktop";
            try
            {
                var uri = new Uri(address);
                //NetTcpSecurity security = new NetTcpSecurity
                //{
                //    Mode = SecurityMode.Transport,
                //    Transport = new TcpTransportSecurity
                //    {
                //        ClientCredentialType = TcpClientCredentialType.Windows,
                //        ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign,
                //    },
                //};

                NetTcpSecurity security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None,
                };

                var binding = new NetTcpBinding
                {
                    ReceiveTimeout = TimeSpan.MaxValue,//TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(10),
                    Security = security,
                };

                factory = new ChannelFactory<IRemoteDesktopService>(binding, new EndpointAddress(uri));
                var channel = factory.CreateChannel();

                try
                {
                    this.ClientId = RngProvider.GetRandomNumber().ToString();

                    //var clientId = Guid.NewGuid().ToString();

                    var connectReq = new RemoteDesktopRequest
                    {
                        ClientId = ClientId,
                    };

                    var connectionResponse = channel.Connect(connectReq);
                    if (!connectionResponse.IsSuccess)
                    {
                        logger.Error("connectionResponse " + connectionResponse.FaultCode);
                        return;
                    }

                    this.ServerId = connectionResponse.ServerId;
                    this.ServerName = connectionResponse.HostName;

                    var screens = connectionResponse.Screens;
                    var primaryScreen = screens.FirstOrDefault(s => s.IsPrimary);

                    var startRequest = new StartSessionRequest
                    {
                        ClientId = this.ClientId,

                        SrcRect = primaryScreen.Bounds,
                        DestAddr = "", //"192.168.1.135",//localAddr.Address.ToString(), //localAddr.ToString(),
                        DestPort = 1234,
                        DstSize = new Size(1920, 1080),
                        EnableInputSimulator = true,

                    };


                    var startResponse = channel.Start(startRequest);
                    if (!startResponse.IsSuccess)
                    {
                        logger.Error("startResponse " + startResponse.FaultCode);
                        return;
                    }

                    var inputPars = new VideoEncodingParams
                    {
                        Width = startRequest.DstSize.Width,
                        Height = startRequest.DstSize.Height,

                        //Width = 640,//2560,
                        //Height = 480,//1440,
                        FrameRate = 30,
                    };

                    var outputPars = new VideoEncodingParams
                    {
                        //Width = 640,//2560,
                        //Height = 480,//1440,
                        Width = startRequest.DstSize.Width,
                        Height = startRequest.DstSize.Height,

                        FrameRate = 30,
                    };

                    var networkPars = new NetworkStreamingParams
                    {
                        SrcAddr = _addr,
                        SrcPort = 1234
                    };

                    this.Play(inputPars, outputPars, networkPars);

                    InputManager.Start(_addr, 8888);



                }
                finally
                {
                    var c = (IClientChannel)channel;
                    if (c.State != CommunicationState.Faulted)
                    {
                        c.Close();
                    }
                    else
                    {
                        c.Abort();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }
        }


        public void Play(VideoEncodingParams inputPars, VideoEncodingParams outputPars, NetworkStreamingParams networkPars)
        {
            VideoReceiver = new ScreenReceiver();

            VideoReceiver.Setup(inputPars, outputPars, networkPars);
            VideoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;

            VideoReceiver.Play();
        }

        public Action UpdateBuffer;
        private void VideoReceiver_UpdateBuffer()
        {
            UpdateBuffer?.Invoke();
        }

        public void Stop()
        {
            if (VideoReceiver != null)
            {
                VideoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
                VideoReceiver.Stop();
            }
        }

        public void Disconnect()
        {

            if (factory != null)
            {
                var channel = factory.CreateChannel();
                try
                {
                    var request = new RemoteDesktopRequest
                    {
                        ClientId = ClientId,
                    };

                    var resp = channel.Stop(request);
                    channel.Disconnect(request);

                    Stop();

                }
                finally
                {
                    if (channel != null)
                    {
                        ((IClientChannel)channel).Close();
                    }

                }
            }

        }


        private void CleanUp()
        {

            if (VideoReceiver != null)
            {
                VideoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
                VideoReceiver.Stop();
                VideoReceiver = null;
            }

            if (InputManager != null)
            {
                InputManager.Stop();
                InputManager = null;

            }


            if (factory != null)
            {
                factory.Abort();
                factory = null;
            }
        }

        public object SendMessage(string id, object[] pars)
        {
            logger.Debug("IRemoteDesktopClient::SendMessage(...) " + id);

            return id + "OK!";
        }

        public void PostMessage(string id, object[] pars)
        {
            logger.Debug("IRemoteDesktopClient::PostMessage(...) " + id);
            //...
        }

    }



    public class InputManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AutoResetEvent waitEvent = new AutoResetEvent(false);
        private object syncRoot = new object();
        private Socket socket = null;

        private string message = "";
        private bool running = false;

        public void Start(string address, int port)
        {
            Task.Run(() =>
            {
                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var ipaddr = IPAddress.Parse(address);
                    socket.Connect(ipaddr, port);
                    running = true;

                    while (running)
                    {

                        try
                        {
                            waitEvent.WaitOne();

                            lock (syncRoot)
                            {
                                if (running)
                                {
                                    byte[] data = Encoding.ASCII.GetBytes(message);
                                    socket.Send(data);
                                }

                                //client.Send(message);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                }
                finally
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }

                    running = false;
                }
            });
        }

        public void ProcessMessage(string _message)
        {
            if (!running)
            {
                return;
            }

            lock (syncRoot)
            {
                this.message = _message;
            }

            waitEvent.Set();

        }
        public void Stop()
        {
            running = false;
            waitEvent.Set();

        }
    }
}
