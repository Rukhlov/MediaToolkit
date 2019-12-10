using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;

namespace TestStreamer
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        UseSynchronizationContext = false)]
        //IncludeExceptionDetailInFaults = true)]
    class RemoteDesktopService : IRemoteDesktopService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ServiceHost host = null;

        public bool IsOpened
        {
            get
            {
                return (host != null && host.State == CommunicationState.Opened);
            }
        }

        public string HostName  { get; private set; }
        public string ServerId { get; private set; }

        public void Open(string address)
        {
            logger.Debug("RemoteDesktopEngine::Open(...) " + address);

            
            try
            {
                var uri = new Uri(address);

                this.HostName = Dns.GetHostName();
                this.ServerId = MediaToolkit.Utils.RngProvider.GetRandomNumber().ToString();


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

                host = new ServiceHost(this, uri);

                // var listenUri = new Uri("net.tcp://" + HostName + "/RemoteDesktop");

                ////var ips = Dns.GetHostAddresses(hostName);
                //var addrInfos = MediaToolkit.Utils.NetworkHelper.GetActiveUnicastIpAddressInfos();

                //foreach (var ip in addrInfos)
                //{
                //    var addr = ip.Address;
                //    if (addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                //    {
                //        continue;
                //    }

                //    var _addr = new Uri("net.tcp://" + addr + "/RemoteDesktop");

                //    var endpoint = host.AddServiceEndpoint(typeof(IRemoteDesktopService), binding, _addr, listenUri);

                //    //var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
                //    //endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"HostName/" + hostName));
                //    //endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("HostName", hostName));
                //    //endpoint.EndpointBehaviors.Add(endpointDiscoveryBehavior);

                //}


                var endpoint = host.AddServiceEndpoint(typeof(IRemoteDesktopService), binding, uri);
                var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
                endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"HostName/" + HostName));
                endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("HostName", HostName));

                var addrInfos = MediaToolkit.Utils.NetworkHelper.GetActiveUnicastIpAddressInfos();
                foreach (var addr in addrInfos)
                {
                    endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"ListenAddr/" + addr));
                }
                endpoint.EndpointBehaviors.Add(endpointDiscoveryBehavior);


                ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();
                serviceDiscoveryBehavior.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
                host.Description.Behaviors.Add(serviceDiscoveryBehavior);
                host.Description.Endpoints.Add(new UdpDiscoveryEndpoint());


                host.Opened += Host_Opened;
                host.Faulted += Host_Faulted;
                host.Closed += Host_Closed;

                host.Open();
               
                logger.Debug("Service opened: " + uri.ToString());

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

        }

        private void Host_Opened(object sender, EventArgs e)
        {
            logger.Debug("RemoteDesktopEngine::Host_Opened()");
        }

        private void Host_Closed(object sender, EventArgs e)
        {
            logger.Debug("RemoteDesktopEngine::Host_Closed()");
        }

        private void Host_Faulted(object sender, EventArgs e)
        {
            logger.Debug("RemoteDesktopEngine::Host_Faulted()");
        }



        public void Close()
        {
            logger.Debug("RemoteDesktopEngine::Close()");

            Clients.Clear();

            if (host != null)
            {

                host.Opened -= Host_Opened;
                host.Faulted -= Host_Faulted;
                host.Closed -= Host_Closed;

                host.Close();
                host = null;
            }

        }

        private Dictionary<string, object> Clients = new Dictionary<string, object>();

        public ConnectionResponse Connect(RemoteDesktopRequest request)
        {
            //var sessionId = OperationContext.Current.SessionId;

            var clientId = request.SenderId;
            logger.Debug("RemoteDesktopEngine::Connect() " + clientId);


            ConnectionResponse response = new ConnectionResponse
            {
                ServerId = this.ServerId,
                HostName = this.HostName,
            };

            if(Clients.Count > 0)
            {
                response.FaultCode = -100501;
                response.FaultDescription = "Max clients limit";
            }

            if (!Clients.ContainsKey(clientId))
            {
                Clients.Add(clientId, null);

                var screens = System.Windows.Forms.Screen.AllScreens;
                var screeenBounds = screens.Select(s => new RemoteScreen
                {
                    DeviceName = s.DeviceName,
                    Bounds = s.Bounds,
                    IsPrimary = s.Primary,
                });

                response.Screens = screeenBounds.ToList();
                //...
            }
            else
            {
                response.FaultCode = -100502;
                response.FaultDescription = "Client is already connected";
            }

            return response;
        }

        public RemoteDesktopResponse Start(StartSessionRequest request)
        {
            logger.Debug("RemoteDesktopEngine::Start()");

            var clientId = request.SenderId;

            int faultCode = 0;
            RemoteDesktopResponse response = new RemoteDesktopResponse
            {
                ServerId = this.ServerId,
            };

            if (!Clients.ContainsKey(clientId))
            {
                response.FaultCode = -100503;
                response.FaultDescription = "Client not connected";

                return response;
            }


            try
            {

                var destAddr = request.DestAddr;
                if (string.IsNullOrEmpty(destAddr))
                {
                    OperationContext context = OperationContext.Current;

                    //var endpoint = context.Channel.RemoteAddress;
                    //var addr = endpoint.Uri.AbsoluteUri;

                    MessageProperties properties = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                    request.DestAddr = endpoint.Address;

                }

                StartStreaming(request);


            }
            catch(Exception ex)
            {
                StopStreaming();

                faultCode = -100500;
                logger.Error(ex);
            }

            response.FaultCode = faultCode;

            return response;
        }


        public RemoteDesktopResponse Stop(RemoteDesktopRequest request)
        {
            logger.Debug("RemoteDesktopEngine::Stop()");

            RemoteDesktopResponse response = new RemoteDesktopResponse
            {
                ServerId = this.ServerId,
            };

            StopStreaming();

            return response;
        }

        public void Disconnect(RemoteDesktopRequest request)
        {
            var sessionId = OperationContext.Current.Channel.SessionId;
            var clientId = request.SenderId;

            logger.Debug("RemoteDesktopEngine::Disconnect() " + clientId);
         
            if (Clients.ContainsKey(clientId))
            {
                Clients.Remove(clientId);
            }


        }

        public object SendMessage(string id, object[] pars)
        {
            logger.Debug("RemoteDesktopEngine::SendMessage(...) " + id);

            return id + "OK!";
        }

        public void PostMessage(string id, object[] pars)
        {
            logger.Debug("RemoteDesktopEngine::PostMessage(...) " + id);
            //...
        }



        private bool isStreaming = false;

        private VideoStreamer videoStreamer = null;
        private IVideoSource videoSource = null;
        private DesktopManager desktopMan = null;

        public void StartStreaming(StartSessionRequest options)
        {

            logger.Debug("StartStreaming()");

            int fps = options.FrameRate;

            bool showMouse = options.ShowMouse;
            bool enableInputSimulator = options.EnableInputSimulator;

            bool aspectRatio = options.AspectRatio;


            var srcRect = options.SrcRect;
            if (srcRect.IsEmpty)
            {
                srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            }

            var destSize = options.DstSize;
            if (destSize.IsEmpty)
            {
                destSize = new Size(srcRect.Width, srcRect.Height);
            }


            if (aspectRatio)
            {
                var ratio = srcRect.Width / (double)srcRect.Height;
                int destWidth = destSize.Width;
                int destHeight = (int)(destWidth / ratio);
                if (ratio < 1)
                {
                    destHeight = destSize.Height;
                    destWidth = (int)(destHeight * ratio);
                }

                destSize = new Size(destWidth, destHeight);
            }

            videoSource = new ScreenSource();
            ScreenCaptureDeviceDescription captureParams = new ScreenCaptureDeviceDescription
            {
                CaptureRegion = srcRect,
                Resolution = destSize,
            };

            captureParams.CaptureType = CaptureType.DXGIDeskDupl;
            captureParams.Fps = fps;
            captureParams.CaptureMouse = showMouse;

            videoSource.Setup(captureParams);



            NetworkSettings networkParams = new NetworkSettings
            {
                RemoteAddr = options.DestAddr,
                RemotePort = options.DestPort,
            };

            VideoEncoderSettings encodingParams = new VideoEncoderSettings
            {
                //Width = destSize.Width, // options.Width,
                //Height = destSize.Height, // options.Height,
                Resolution = destSize,
                FrameRate = options.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
            };

            videoStreamer = new VideoStreamer(videoSource);
            videoStreamer.Setup(encodingParams, networkParams);

            videoSource.Start();
            var streamerTask = videoStreamer.Start();

            //previewForm = new PreviewForm();
            //previewForm.Setup(screenSource);

            if (enableInputSimulator)
            {
                desktopMan = new DesktopManager();
                var inputSimulatorTask = desktopMan.Start();
            }

            isStreaming = true;
        }

        public void StopStreaming()
        {
            logger.Debug("StopStreaming()");

            if (videoSource != null)
            {
                videoSource.Stop();
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }

            if (desktopMan != null)
            {
                desktopMan.Stop();
                desktopMan = null;
            }

            isStreaming = false;
        }

   


    }
}
