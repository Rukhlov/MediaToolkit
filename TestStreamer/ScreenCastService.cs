using MediaToolkit.Common;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using TestStreamer.Controls;

namespace TestStreamer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    UseSynchronizationContext = false)]
    class ScreencastCommunicationService : IScreenCastService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ScreenStreamerControl streamerControl = null;
        public ScreencastCommunicationService(ScreenStreamerControl control)
        {
            this.streamerControl = control;
        }

        private ServiceHost host = null;

        public bool IsOpened
        {
            get
            {
                return (host != null && host.State == CommunicationState.Opened);
            }
        }

        public string HostName { get; private set; }
        public string ServerId { get; private set; }


        public void Open(string address)
        {
            logger.Debug("ScreenCastService::Open(...) " + address);

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

                var endpoint = host.AddServiceEndpoint(typeof(IScreenCastService), binding, uri);
                var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
                endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"HostName/" + HostName));
                endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("HostName", HostName));

                //var addrInfos = MediaToolkit.Utils.NetworkHelper.GetActiveUnicastIpAddressInfos();
                //foreach (var addr in addrInfos)
                //{
                //    endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"ListenAddr/" + addr.Address));
                //}
                //endpoint.EndpointBehaviors.Add(endpointDiscoveryBehavior);


                ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();
                serviceDiscoveryBehavior.AnnouncementEndpoints.Add(new UdpAnnouncementEndpoint());
                host.Description.Behaviors.Add(serviceDiscoveryBehavior);
                host.Description.Endpoints.Add(new UdpDiscoveryEndpoint());


                host.Opened += Host_Opened;
                host.Faulted += Host_Faulted;
                host.Closed += Host_Closed;

                host.Open();

                //foreach (var _channel in host.ChannelDispatchers)
                //{
                //    logger.Debug(_channel.Listener.Uri);
                //}

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
            logger.Debug("ScreenCastService::Host_Opened()");
        }

        private void Host_Closed(object sender, EventArgs e)
        {
            logger.Debug("ScreenCastService::Host_Closed()");
        }

        private void Host_Faulted(object sender, EventArgs e)
        {
            logger.Debug("ScreenCastService::Host_Faulted()");
        }



        public void Close()
        {
            logger.Debug("ScreenCastService::Close()");

            if (host != null)
            {

                host.Opened -= Host_Opened;
                host.Faulted -= Host_Faulted;
                host.Closed -= Host_Closed;

                host.Close();
                host = null;
            }

        }


        public ScreencastChannelInfo[] GetChannelInfos()
        {
            logger.Debug("ScreenCastService::GetChannels()");

            var infos = streamerControl?.ScreencastChannelsInfos?.ToArray();

            return infos;


        }

        public ScreenCastResponse Play(ScreencastChannelInfo[] infos )
        {
            logger.Debug("ScreenCastService::Play()");

            streamerControl?.Play(infos);

            return null;
        }

        public void Teardown()
        {
            logger.Debug("ScreenCastService::Teardown()");


            streamerControl?.Teardown();
        }

        public void PostMessage(ServerRequest request)
        {
            logger.Debug("ScreenCastService::PostMessage(...) " + request.Command);
            
        }


    }
}
