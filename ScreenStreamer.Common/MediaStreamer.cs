using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.MediaStreamers;
using MediaToolkit.Utils;
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


namespace ScreenStreamer.Common
{
    public enum MediaStreamerState
    {
        Starting,
        Streamming,
        Stopping,
        Stopped,
        Shutdown,
    }

    public class MediaStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        private IVideoSource videoSource = null;
        private VideoStreamer videoStreamer = null;

        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;

        private CommunicationService communicationService = null;

        public Uri ListenUri => communicationService?.ListenUri;

        public StreamSession Session { get; private set; }

        public event Action StateChanged;

        public Exception ExceptionObj { get; private set; }
        private AutoResetEvent syncEvent = null;

        private volatile MediaStreamerState state = MediaStreamerState.Shutdown;
        public MediaStreamerState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        private Thread streamerThread = null;


        public List<StatCounter> Stats { get; } = new List<StatCounter>();


        public bool Start(StreamSession session)
        {
            logger.Debug("ScreenStreamer::Start(...)");

            Debug.Assert(session != null, "session!=null");

            if (state != MediaStreamerState.Shutdown)
            {
                logger.Warn("ScreenStreamer::Start(...) return invalid state: " + state);
                return false;
            }

            state = MediaStreamerState.Starting;
            StateChanged?.Invoke();

            streamerThread = new Thread(DoStreaming);
            streamerThread.IsBackground = true;
            streamerThread.Start(session);

            return true;
        }

        private void DoStreaming(object session)
        {

            try
            {

                Session = session as StreamSession;
                syncEvent = new AutoResetEvent(false);

                // Session.Setup();

                StartStreaming();

                Stats.Clear();

                Stats.AddRange(new[] { videoSource?.Stats, videoStreamer?.Stats });

                if (state == MediaStreamerState.Starting)
                {
                    state = MediaStreamerState.Streamming;
                    StateChanged?.Invoke();
                }

                var videoEnabled = Session.VideoEnabled;
                var audioEnabled = Session.AudioEnabled;

                while (state == MediaStreamerState.Streamming)
                {
                    if (videoEnabled)
                    {
                        //TODO: check audio video state...
                        // get stats...
                        if (videoSource != null)
                        {
                            if (videoSource.State != CaptureState.Capturing)
                            {
                                var _errorCode = videoSource.ErrorCode;

                                logger.Warn("videoSource.State == " + videoSource.State + " " + _errorCode);
                                if (_errorCode != 0)
                                {
                                    this.errorCode = _errorCode;
                                    break;
                                }
                                
                            }
                        }

                        if (videoStreamer != null)
                        {
                            if (videoStreamer.State != StreamerState.Streaming)
                            {
                                logger.Warn("videoStreamer.State == " + videoStreamer.State);
                            }
                        }
                    }

                    if (audioEnabled)
                    {
                        if (audioSource != null)
                        {
                            if (audioSource.State != CaptureState.Capturing)
                            {
                                logger.Warn("audioSource.State == " + audioSource.State);
                            }
                        }

                        if (audioStreamer != null)
                        {

                        }
                    }


                    if (!communicationService.IsOpened)
                    {
                        logger.Warn("communicationService.IsOpened == false");
                        //...
                    }

                    syncEvent.WaitOne(1000);

                }

                if (errorCode != 0)
                {
                    logger.Warn("MediaStreamerState.Stopping: " + errorCode); 

                    state = MediaStreamerState.Stopping;
                    StateChanged?.Invoke();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                ProcessError(ex);
            }
            finally
            {
                StopStreaming();

                state = MediaStreamerState.Stopped;
                StateChanged?.Invoke();

            }
        }

        public bool Stop()
        {
            logger.Debug("ScreenStreamer::Stop()");

            if (state != MediaStreamerState.Streamming)
            {
                logger.Warn("ScreenStreamer::Stop() return invalid state: " + state);
                return false;
            }


            state = MediaStreamerState.Stopping;
            StateChanged?.Invoke();

            syncEvent?.Set();

            return true;
        }


        public Exception GetInternalError()
        {
           // string message = "";
            if (videoSource != null)
            {
                var obj = videoSource.LastError;
                if (obj != null)
                {
                    //var ex = obj as Exception;
                    //message = ex.Message;

                    return obj as Exception; 
                }
            }


            ////....

            return null;
        }

        public int CheckError()
        {
            int code = 0;
            if (videoSource != null)
            {
                code = videoSource.ErrorCode;
            }

            //if (videoStreamer != null)
            //{
            //    code |= videoStreamer.ErrorCode;
            //}

            ////....

            return code;
        }

        public void Shutdown()
        {
            logger.Trace("ScreenStreamer::Shutdown()");

			//if (streamerThread != null && streamerThread.ThreadState == System.Threading.ThreadState.Running)
			//{
			//    //...

			//}


			if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }

			if (ExceptionObj != null)
			{
				logger.Warn(ExceptionObj);
				ExceptionObj = null;
			}

			errorCode = 0;

			state = MediaStreamerState.Shutdown;

            StateChanged?.Invoke();

        }



        private void StartStreaming()
        {
            logger.Debug("ScreenStreamer::StartStreaming()");

            var videoSettings = Session.VideoSettings;

            if (videoSettings.Enabled)
            {
                var captureDevice = (VideoCaptureDevice)videoSettings.CaptureDevice.Clone();
                if(captureDevice.CaptureMode == CaptureMode.Screen)
                {
                    var screenDevice = (ScreenCaptureDevice)captureDevice;

                    if (screenDevice.CaptureRegion.Width > Config.MaxVideoEncoderWidth)
                    {
                        screenDevice.CaptureRegion.Width = Config.MaxVideoEncoderWidth;
                    }

                    if (screenDevice.CaptureRegion.Height > Config.MaxVideoEncoderHeight)
                    {
                        screenDevice.CaptureRegion.Height = Config.MaxVideoEncoderHeight;
                    }

                    screenDevice.Resolution = MediaToolkit.Utils.GraphicTools.DecreaseToEven(screenDevice.CaptureRegion.Size);
                }

                var videoEncoderSettings = (VideoEncoderSettings)videoSettings.EncoderSettings.Clone();
                if (videoSettings.UseEncoderResoulutionFromSource)
                {
                    videoEncoderSettings.Width = captureDevice.Resolution.Width;
                    videoEncoderSettings.Height = captureDevice.Resolution.Height;
                }
                else
                {
                    captureDevice.Resolution = videoEncoderSettings.Resolution;
                }

  
                if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
                {
                    videoSource = new VideoCaptureSource();
                }
                else if (captureDevice.CaptureMode == CaptureMode.Screen)
                {
                    videoSource = new ScreenSource();
                    
                }

                videoSource.Setup(captureDevice);
                videoSource.CaptureStarted += VideoSource_CaptureStarted;
                videoSource.CaptureStopped += VideoSource_CaptureStopped;

                if (Session.TransportMode == TransportMode.Tcp || Session.IsMulticast)
                {

                    videoStreamer = new VideoStreamer(videoSource);
                    videoStreamer.Setup(videoEncoderSettings, videoSettings.NetworkSettings);

                    //videoStreamer.Setup(videoSettings.EncoderSettings, videoSettings.NetworkSettings);
                }

            }

            var audioSettings = Session.AudioSettings;

            if (audioSettings.Enabled)
            {

                audioSource = new AudioSource();
				var captureDevice = audioSettings.CaptureDevice;

				var deviceId = captureDevice.DeviceId;
				var captureProps = captureDevice.Properties;

				audioSource.Setup(deviceId, captureProps);

                if (Session.TransportMode == TransportMode.Tcp || Session.IsMulticast)
                {
                    audioStreamer = new AudioStreamer(audioSource);
                    audioStreamer.Setup(audioSettings.EncoderSettings, audioSettings.NetworkSettings);
                    audioStreamer.StateChanged += AudioStreamer_StateChanged;

                }
            }

            var communicationAddress = Session.CommunicationAddress;
            var videoDeviceName = videoSettings.CaptureDevice?.Name ?? "";
            var hostName = Session.StreamName;
            if (!string.IsNullOrEmpty(videoDeviceName))
            {
                hostName += " (" + videoDeviceName + ")";
            }

            var audioDeviceName = audioSettings.CaptureDevice?.Name ?? "";

            Dictionary<string, string> extensions = new Dictionary<string, string>
            {
                { "StreamName",  Session.StreamName },
                { "AudioInfo", audioDeviceName },
                { "VideoInfo", videoDeviceName },
            };

            communicationService = new CommunicationService(this);
            communicationService.Open(communicationAddress, hostName, extensions);


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
				videoSource.CaptureStarted -= VideoSource_CaptureStarted;
				videoSource.CaptureStopped -= VideoSource_CaptureStopped;

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
				audioStreamer.StateChanged -= AudioStreamer_StateChanged;
				audioStreamer = null;
			}


            //ScreencastChannelsInfos.Clear();

            communicationService?.Close();

            //IsStreaming = false;

            //logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
        }


        private void ProcessError(Exception ex)
        {
            //...
            ExceptionObj = ex;
        }


        public ScreencastChannelInfo[] GetScreencastInfo()
        {
            var channels = new List<ScreencastChannelInfo>();

            if (videoStreamer != null)
            {
                //if(videoStreamer.State== StreamerState.Streaming)
                {
                    var networkSettings = videoStreamer.NetworkSettings;
                    var encoderSettings = videoStreamer.EncoderSettings;


                    VideoChannelInfo videoInfo = new VideoChannelInfo
                    {
                        Id = videoStreamer.Id,
                        VideoEncoder = encoderSettings.EncoderFormat,
                        Resolution = encoderSettings.Resolution,
                        Bitrate = encoderSettings.Bitrate,

                        Fps = (int)encoderSettings.FramePerSec, //encoderSettings.FrameRate.Num,


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
                        IsMulticast = Session.IsMulticast,
                        MediaInfo = videoInfo,
                        SSRC = networkSettings.SSRC,

                        ClientsCount = videoStreamer.ClientsCount,
                    };

                    channels.Add(videoChannelInfo);
                }


            }

            if (audioStreamer != null)
            {
                //if (audioStreamer.IsStreaming)
                {
                    var networkSettings = audioStreamer.NetworkSettings;
                    var encoderSettings = audioStreamer.EncoderSettings;

                    AudioChannelInfo audioInfo = new AudioChannelInfo
                    {
                        Id = audioStreamer.Id,
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
                        IsMulticast = Session.IsMulticast,
                        Transport = _transportMode,
                        MediaInfo = audioInfo,
                        SSRC = networkSettings.SSRC,
                        ClientsCount = audioStreamer.ClientsCount
                    };

                    channels.Add(audioChannelInfo);
                }
            }

            return channels.ToArray();

            //var vci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
            //if (vci != null)
            //{
            //    vci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            //}

            //var aci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is AudioChannelInfo);
            //if (aci != null)
            //{
            //    aci.ClientsCount = audioStreamer?.ClientsCount ?? 0;
            //}

            //return ScreencastChannelsInfos?.ToArray();
        }


        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
        class CommunicationService : IScreenCastService
        {
            private static Logger logger = LogManager.GetCurrentClassLogger();

            private readonly MediaStreamer screenStreamer = null;
            public CommunicationService(MediaStreamer streamer)
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

            public void Open(string address, string hostName, Dictionary<string,string> extensions = null)
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

                    endpointDiscoveryBehavior.Extensions.Add(new System.Xml.Linq.XElement("StreamId", ServerId));

                    if (extensions != null)
                    {
                        foreach(var key in extensions.Keys)
                        {
                            var element = new System.Xml.Linq.XElement(key, extensions[key]);

                            endpointDiscoveryBehavior.Extensions.Add(element);
                        }
                    }

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
                //logger.Trace("ScreenCastService::PostMessage(...) " + request.Command);

            }


        }

    }
}
