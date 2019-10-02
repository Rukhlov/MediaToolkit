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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    class RemoteDesktopEngine : IRemoteDesktopService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private ServiceHost host = null;

        public bool ServiceHostOpened
        {
            get
            {
                return (host != null && host.State == CommunicationState.Opened);
            }
        }

        public void Open(string address)
        {
            logger.Debug("RemoteDesktopEngine::Open(...) " + address);

            try
            {
                var uri = new Uri(address);
                var hostName = Dns.GetHostName();

                var binding = new NetTcpBinding
                {
                    ReceiveTimeout = TimeSpan.MaxValue,//TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(10),
                };

                host = new ServiceHost(this, uri);
                var endpoint = host.AddServiceEndpoint(typeof(IRemoteDesktopService), binding, uri);

                var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
                endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"HostName/" + hostName));
                endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("HostName", hostName));
                endpoint.EndpointBehaviors.Add(endpointDiscoveryBehavior);


                ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();
                serviceDiscoveryBehavior.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
                host.Description.Behaviors.Add(serviceDiscoveryBehavior);
                host.Description.Endpoints.Add(new UdpDiscoveryEndpoint());

                //behavior.Scopes.Add(scope);

                //foreach (ServiceEndpoint endpoint in host.Description.Endpoints)
                //{
                //    if (endpoint.IsSystemEndpoint || endpoint is DiscoveryEndpoint ||
                //       endpoint is AnnouncementEndpoint || endpoint is ServiceMetadataEndpoint)
                //        continue;

                //    endpoint.Behaviors.Add(behavior);
                //}

                //host.AddServiceEndpoint(new UdpDiscoveryEndpoint());


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

            if (host != null)
            {

                host.Opened -= Host_Opened;
                host.Faulted -= Host_Faulted;
                host.Closed -= Host_Closed;

                host.Close();
                host = null;
            }

        }

        private bool isStreaming = false;

        private VideoStreamer videoStreamer = null;
        private ScreenSource screenSource = null;
        private DesktopManager desktopMan = null;
        public void StartStreaming(RemoteDesktopOptions options)
        {
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

            screenSource = new ScreenSource();
            ScreenCaptureParams captureParams = new ScreenCaptureParams
            {
                SrcRect = srcRect,
                DestSize = destSize,
                CaptureType = CaptureType.DXGIDeskDupl,
                //CaptureType = CaptureType.Direct3D,
                //CaptureType = CaptureType.GDI,
                Fps = fps,
                CaptureMouse = showMouse,
            };

            screenSource.Setup(captureParams);



            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                DestAddr = options.DestAddr,
                DestPort = options.DestPort,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = destSize.Width, // options.Width,
                Height = destSize.Height, // options.Height,
                FrameRate = options.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
            };

            videoStreamer = new VideoStreamer(screenSource);
            videoStreamer.Setup(encodingParams, networkParams);

            var captureTask = screenSource.Start();
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
            if (screenSource != null)
            {
                screenSource.Close();
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


        public object Connect(string id, object[] args)
        {
            logger.Debug("RemoteDesktopEngine::Connect()");

            return null;
        }

        public bool Setup()
        {
            logger.Debug("RemoteDesktopEngine::Setup()");

            return false;
        }

        public bool Start(RemoteDesktopOptions options )
        {
            logger.Debug("RemoteDesktopEngine::Start()");
            bool Result = false;

            try
            {

                var destAddr = options.DestAddr;
                //if (string.IsNullOrEmpty(destAddr))
                {
                    OperationContext context = OperationContext.Current;

                    //var endpoint = context.Channel.RemoteAddress;
                    //var addr = endpoint.Uri.AbsoluteUri;

                    MessageProperties properties = context.IncomingMessageProperties;
                    RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

                    options.DestAddr = endpoint.Address;

                }

                StartStreaming(options);

                Result = true;
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }

            return Result;
        }


        public bool Stop()
        {
            logger.Debug("RemoteDesktopEngine::Stop()");

            StopStreaming();

            return true;
        }

        public void Disconnect()
        {
            logger.Debug("RemoteDesktopEngine::Disconnect()");

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
    }
}
