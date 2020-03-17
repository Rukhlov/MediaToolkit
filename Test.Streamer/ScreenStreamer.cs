using MediaToolkit;
using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestStreamer;

namespace TestStreamer
{
    public enum StreamerState
    {
        Initialized,
        Starting,
        Streamming,
        Stopping,
        Stopped,
        Shutdown,
    }

    public class ScreenStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IVideoSource videoSource = null;
        private VideoStreamer videoStreamer = null;
       
        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;

        private CommunicationService communicationService = null;

        public List<ScreencastChannelInfo> ScreencastChannelsInfos { get; private set; } = new List<ScreencastChannelInfo>();
        public Uri ListenUri => communicationService?.ListenUri;
        //public bool IsStreaming { get; private set; }

        public StreamSession Session { get; private set; }

        public event Action StreamStarted;
        public event Action StreamStopped;

        public Exception ExceptionObj { get; private set; }
        private AutoResetEvent syncEvent = null;

        private volatile StreamerState state = StreamerState.Shutdown;
        public StreamerState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        private Task streamerTask = null;

        public void Start(StreamSession session)
        {
            logger.Debug("ScreenStreamer::Start(...)");

            if (state != StreamerState.Shutdown)
            {
                logger.Warn("ScreenStreamer::Start(...) return invalid state: " + state);
                return;
            }

            state = StreamerState.Starting;

            streamerTask = Task.Run(() =>
            {
                DoStreaming(session);

            });
        }

        private void DoStreaming(StreamSession session)
        {

            if (state != StreamerState.Starting)
            {
                logger.Warn("ScreenStreamer::DoStreaming(...) return invalid state: " + state);
                return;
            }

            try
            {
                syncEvent = new AutoResetEvent(false);

                StartStreaming(session);

                state = StreamerState.Streamming;

                StreamStarted?.Invoke();

                while (state == StreamerState.Streamming)
                {
                    //....


                    syncEvent.WaitOne(1000);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                ExceptionObj = ex;

            }
            finally
            {
                StopStreaming();

                state = StreamerState.Stopped;

                StreamStopped?.Invoke();

            }
        }

        public void Stop()
        {
            logger.Debug("ScreenStreamer::Stop()");


            if (state != StreamerState.Streamming)
            {
                logger.Warn("ScreenStreamer::Stop() return invalid state: " + state);
                //return;
            }


            state = StreamerState.Stopping;

            syncEvent?.Set();
        }


        public void Shutdown()
        {
            logger.Trace("DeckLinkInput::Shutdown()");

            if (streamerTask != null && streamerTask.Status == TaskStatus.Running)
            {
                //...
               
            }

            if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }

            state = StreamerState.Shutdown;

        }



        private void StartStreaming(StreamSession session)
        {
            logger.Debug("ScreenStreamer::StartStreaming()");

            this.Session = session;

            var videoSettings = session.VideoSettings;
            var audioSettings = session.AudioSettings;

            var sourceId = Guid.NewGuid().ToString();

            var communicationPort = session.CommunicationPort;

            var networkIpAddr = session.NetworkIpAddress; //"0.0.0.0";

            var transportMode = session.TransportMode; //TransportMode.Tcp;//(TransportMode)transportComboBox.SelectedItem;
            if (session.IsMulticast)
            {
                transportMode = TransportMode.Udp;
            }

            if (!session.IsMulticast && transportMode == TransportMode.Udp)
            {
                throw new NotSupportedException("TransportMode.Udp currently not supported...");
            }

            var videoNetworkSettings = videoSettings.NetworkSettings;
            var audioNetworkSettings = audioSettings.NetworkSettings;

            videoNetworkSettings.TransportMode = transportMode;
            audioNetworkSettings.TransportMode = transportMode;



            if (session.IsMulticast)
            {
                var multicastAddr = session.MutlicastAddress;
                var multicastVideoPort = session.MutlicastPort1;
                var multicastAudioPort = multicastVideoPort + 1;

                videoNetworkSettings.RemoteAddr = multicastAddr;
                videoNetworkSettings.RemotePort = multicastVideoPort;

                audioNetworkSettings.RemoteAddr = multicastAddr;
                audioNetworkSettings.RemotePort = multicastAudioPort;

            }
            else
            {
                if (transportMode == TransportMode.Tcp)
                {
                    videoNetworkSettings.LocalAddr = networkIpAddr;
                    videoNetworkSettings.LocalPort = 0;

                    audioNetworkSettings.LocalAddr = networkIpAddr;
                    audioNetworkSettings.LocalPort = 0;
                }
                else
                {
                    throw new NotSupportedException("Mode currently not supported: " + transportMode);
                }
            }

            var screenCaptParams = (videoSettings.CaptureDevice as ScreenCaptureDevice);
            if (screenCaptParams != null)
            {
                if (screenCaptParams.DisplayRegion.IsEmpty)
                {
                    logger.Debug("VideoSource DisplayRegion.IsEmpty");
                    //videoSettings.Enabled = false;
                }
            }

            if (string.IsNullOrEmpty(audioSettings.CaptureDevice.DeviceId))
            {
                logger.Debug("Empty MMDeviceId...");
                audioSettings.Enabled = false;
            }


            // var communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";

            //var communicationAddress = "net.tcp://" + networkIpAddr +":"+ communicationPort + "/ScreenCaster/" + sourceId;
            // var communicationAddress = "http://" + "RAS-HOME10:8080"+ "/ScreenCaster/" + sourceId;
            // var communicationAddress = "net.tcp://" + "RAS-HOME10" + "/ScreenCaster/" + sourceId;

            var communicationAddress = "net.tcp://" + networkIpAddr + "/ScreenCaster/";

            if (communicationPort > 0)
            {
                communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";
            }


            var videoEnabled = videoSettings.Enabled;
            var audioEnabled = audioSettings.Enabled;


            logger.Info("CommunicationAddress=" + communicationAddress +
                " MulticastMode=" + session.IsMulticast +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled);

            if (videoEnabled)
            {

                SetupVideoSource(videoSettings);

                if (transportMode == TransportMode.Tcp || session.IsMulticast)
                {
                    SetupVideoStream(videoSettings);
                }

                ScreencastChannelInfo videoChannelInfo = GetVideoChannelInfo(videoSettings, session);

                ScreencastChannelsInfos.Add(videoChannelInfo);


            }

            if (audioEnabled)
            {
                SetupAudioSource(audioSettings);

                if (transportMode == TransportMode.Tcp || session.IsMulticast)
                {
                    SetupAudioStream(audioSettings);
                }

                ScreencastChannelInfo audioChannelInfo = GetAudioChannelInfo(audioSettings, session);

                ScreencastChannelsInfos.Add(audioChannelInfo);

            }

            communicationService = new CommunicationService(this);
            var hostName = session.StreamName; //System.Net.Dns.GetHostName();


            hostName += " (" + videoSettings.CaptureDevice.Name + ")";
            communicationService.Open(communicationAddress, hostName);


            if (videoSettings.Enabled)
            {
                videoSource.Start();
                videoStreamer.Start();
            }

            if (audioSettings.Enabled)
            {
                audioSource.Start();
                audioStreamer.Start();
            }

           // IsStreaming = true;
        }



        private void SetupVideoSource(VideoStreamSettings settings)
        {
            logger.Debug("SetupVideoSource(...)");

            try
            {

                var captureDevice = settings.CaptureDevice;

                var resolution = captureDevice.Resolution;
                int w = resolution.Width;
                if (w % 2 != 0)
                {
                    w--;
                }

                int h = resolution.Height;
                if (h % 2 != 0)
                {
                    h--;
                }
                captureDevice.Resolution = new Size(w, h);

                var encodingSettings = settings.EncoderSettings;
                if (settings.UseEncoderResoulutionFromSource)
                {
                    encodingSettings.Width = captureDevice.Resolution.Width;
                    encodingSettings.Height = captureDevice.Resolution.Height;
                }

                if (!settings.UseEncoderResoulutionFromSource)
                {
                    captureDevice.Resolution = settings.EncoderSettings.Resolution;
                }
                else
                {
                    //captureDevice.Resolution = Size.Empty;
                }


                if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
                {
                    videoSource = new VideoCaptureSource();
                    videoSource.Setup(captureDevice);
                }
                else if (captureDevice.CaptureMode == CaptureMode.Screen)
                {
                    videoSource = new ScreenSource();
                    videoSource.Setup(captureDevice);
                }

                videoSource.CaptureStarted += VideoSource_CaptureStarted;
                videoSource.CaptureStopped += VideoSource_CaptureStopped;

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                if (videoSource != null)
                {
                    videoSource.CaptureStarted -= VideoSource_CaptureStarted;
                    videoSource.CaptureStopped -= VideoSource_CaptureStopped;

                    videoSource.Close();
                    videoSource = null;
                }

                throw;
            }

        }



        private void SetupAudioSource(AudioStreamSettings settings)
        {
            logger.Debug("SetupAudioSource(...)");
            try
            {
                audioSource = new AudioSource();
                var deviceId = settings.CaptureDevice.DeviceId;
                var eventSyncMode = true;
                var audioBufferMilliseconds = 50;
                var exclusiveMode = false;
                audioSource.Setup(deviceId, eventSyncMode, audioBufferMilliseconds, exclusiveMode);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (audioSource != null)
                {
                    audioSource.Close();
                    audioSource = null;
                }

                throw;
            }

        }


        private void SetupVideoStream(VideoStreamSettings settings)
        {
            logger.Debug("SetupVideoStream(...)");

            try
            {
                videoStreamer = new VideoStreamer(videoSource);

                videoStreamer.Setup(settings.EncoderSettings, settings.NetworkSettings);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (videoStreamer != null)
                {
                    videoStreamer.Close();
                    videoStreamer = null;
                }

                throw;
            }
        }


        private void SetupAudioStream(AudioStreamSettings settings)
        {
            logger.Debug("StartAudioStream(...)");

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            try
            {
                audioStreamer = new AudioStreamer(audioSource);
                audioStreamer.Setup(settings.EncoderSettings, settings.NetworkSettings);
                audioStreamer.StateChanged += AudioStreamer_StateChanged;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (audioStreamer != null)
                {
                    audioStreamer.Close();
                    audioStreamer.StateChanged -= AudioStreamer_StateChanged;
                    audioStreamer = null;
                }

                throw;
            }

        }

        private static ScreencastChannelInfo GetAudioChannelInfo(AudioStreamSettings settings, StreamSession serverSettings)
        {
            var networkSettings = settings.NetworkSettings;
            var encoderSettings = settings.EncoderSettings;

            AudioChannelInfo audioInfo = new AudioChannelInfo
            {
                Id = settings.Id,
                AudioEncoder = encoderSettings.Encoder,
                SampleRate = encoderSettings.SampleRate,
                Channels = encoderSettings.Channels,

            };


            var address = networkSettings.RemoteAddr;
            var port = networkSettings.RemotePort;

            var _transportMode = networkSettings.TransportMode;
            if (_transportMode == TransportMode.Tcp)
            {
                address = networkSettings.LocalAddr;
                port = networkSettings.LocalPort;
            }

            ScreencastChannelInfo audioChannelInfo = new ScreencastChannelInfo
            {
                Address = address,
                Port = port,
                IsMulticast = serverSettings.IsMulticast,
                Transport = _transportMode,
                MediaInfo = audioInfo,
                SSRC = networkSettings.SSRC,
            };
            return audioChannelInfo;
        }

        private static ScreencastChannelInfo GetVideoChannelInfo(VideoStreamSettings settings, StreamSession serverSettings)
        {
            var videoEncoderPars = settings.EncoderSettings;
            var networkSettings = settings.NetworkSettings;

            VideoChannelInfo videoInfo = new VideoChannelInfo
            {
                Id = settings.Id,
                VideoEncoder = videoEncoderPars.Encoder,
                Resolution = videoEncoderPars.Resolution,
                Bitrate = videoEncoderPars.Bitrate,

                Fps = videoEncoderPars.FrameRate,
            };

            var address = networkSettings.RemoteAddr;
            var port = networkSettings.RemotePort;
            var _transportMode = networkSettings.TransportMode;
            if (_transportMode == TransportMode.Tcp)
            {
                address = networkSettings.LocalAddr;
                port = networkSettings.LocalPort;
            }

            ScreencastChannelInfo videoChannelInfo = new ScreencastChannelInfo
            {
                Address = address,//videoSettings.Address,
                Port = port, // videoSettings.Port,
                Transport = _transportMode,
                IsMulticast = serverSettings.IsMulticast,
                MediaInfo = videoInfo,
                SSRC = networkSettings.SSRC,
            };
            return videoChannelInfo;
        }

        private void VideoSource_CaptureStarted()
        {
            logger.Debug("VideoSource_CaptureStarted(...)");
            //...
        }

        private void VideoSource_CaptureStopped(object obj)
        {
            logger.Debug("VideoSource_CaptureStopped(...)");

            var errorCode = videoSource.ErrorCode;
            if (errorCode > 0)
            {
                //...
                logger.Error("VideoSource_CaptureStopped(...) " + errorCode);
            }

        }


        private void AudioStreamer_StateChanged()
        {
            //...
            logger.Debug("AudioStreamer_StateChanged()");
        }



        private void StopStreaming()
        {
            logger.Debug("StopStreaming()");

            if (videoSource != null)
            {
                videoSource.Close();
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();
            }

            if (audioSource != null)
            {
                audioSource.Stop();
            }

            if (audioStreamer != null)
            {
                //audioStreamer.SetWaveformPainter(null);
                audioStreamer.Close();
            }


            ScreencastChannelsInfos.Clear();

            communicationService?.Close();

            //IsStreaming = false;

            //logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
        }


        public ScreencastChannelInfo[] GetScreencastInfo()
        {
            var vci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
            if (vci != null)
            {
                vci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            }

            var aci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is AudioChannelInfo);
            if (aci != null)
            {
                aci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            }

            return ScreencastChannelsInfos?.ToArray();
        }


        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        class CommunicationService : IScreenCastService
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();

            private readonly ScreenStreamer screenStreamer = null;
            public CommunicationService(ScreenStreamer streamer)
            {
                this.screenStreamer= streamer;
            }

            private ServiceHost host = null;
            //public int Port
            //{
            //    get
            //    {
            //        int port = -1;

            //        try
            //        {
            //            if (host != null)
            //            {
            //                var channelDispatcher = host.ChannelDispatchers?.FirstOrDefault();
            //                if (channelDispatcher != null)
            //                {
            //                    port = channelDispatcher.Listener.Uri.Port;
            //                }
            //            }

            //        }
            //        catch(Exception ex)
            //        {
            //            logger.Error(ex);
            //        }

            //        return port;
            //    }
            //}

            public bool IsOpened
            {
                get
                {
                    return (host != null && host.State == CommunicationState.Opened);
                }
            }

            public string HostName { get; private set; }
            public string ServerId { get; private set; }

            public Uri ListenUri { get; private set; }

            public void Open(string address, string hostName)
            {
                logger.Debug("ScreenCastService::Open(...) " + address);

                try
                {

                    this.ListenUri = new Uri(address);

                    this.HostName = hostName;
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

                    //NetHttpBinding binding = new NetHttpBinding
                    //{
                    //    ReceiveTimeout = TimeSpan.MaxValue,//TimeSpan.FromSeconds(10),
                    //    SendTimeout = TimeSpan.FromSeconds(10),
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
                        // PortSharingEnabled = true,
                    };

                    host = new ServiceHost(this, ListenUri);

                    var endpoint = host.AddServiceEndpoint(typeof(IScreenCastService), binding, ListenUri);

                    //endpoint.ListenUriMode = System.ServiceModel.Description.ListenUriMode.Unique;

                    var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();
                    // endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"HostName/" + HostName));
                    endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("HostName", HostName));

                    //var addrInfos = MediaToolkit.Utils.NetworkHelper.GetActiveUnicastIpAddressInfos();
                    //foreach (var addr in addrInfos)
                    //{
                    //    endpointDiscoveryBehavior.Scopes.Add(new Uri(uri, @"ListenAddr/" + addr.Address));
                    //}

                    endpoint.EndpointBehaviors.Add(endpointDiscoveryBehavior);


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

                    logger.Debug("Service opened: " + ListenUri.ToString());

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

                var infos = screenStreamer?.GetScreencastInfo();

                return infos;


            }

            public ScreenCastResponse Play(ScreencastChannelInfo[] infos)
            {
                logger.Debug("ScreenCastService::Play()");

                //streamerControl?.Play(infos);

                return null;
            }

            public void Teardown()
            {
                logger.Debug("ScreenCastService::Teardown()");


                // streamerControl?.Teardown();
            }

            public void PostMessage(ServerRequest request)
            {
                logger.Debug("ScreenCastService::PostMessage(...) " + request.Command);

            }


        }

    }
}
