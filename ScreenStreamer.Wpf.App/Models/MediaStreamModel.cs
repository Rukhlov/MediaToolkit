using MediaToolkit.Core;
using NLog;
using ScreenStreamer.Common;


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.Models
{

    public class MediaStreamModel
	{
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MediaStreamModel()
        {
            mediaStreamer = new MediaStreamer();
            mediaStreamer.StateChanged += MediaStreamer_StateChanged;

        }

        public string Name { get; set; } = "";
        public AdvancedSettingsModel AdvancedSettings { get; set; } = new AdvancedSettingsModel();
        public PropertyVideoModel PropertyVideo { get; set; } = new PropertyVideoModel();
        public PropertyAudioModel PropertyAudio { get; set; } = new PropertyAudioModel();
        public PropertyBorderModel PropertyBorder { get; set; } = new PropertyBorderModel();
        public PropertyNetworkModel PropertyNetwork { get; set; } = new PropertyNetworkModel();

		// public PropertyQualityModel PropertyQuality { get; set; } = new PropertyQualityModel();
		//public PropertyCursorModel PropertyCursor { get; set; } = new PropertyCursorModel();


		public bool Validate(IEnumerable<EncoderItem> videoEncoders = null, 
            IEnumerable<VideoSourceItem> videoSources = null, 
            IEnumerable<AudioSourceItem> audioSources = null)
        {
            AdvancedSettings.Validate(videoEncoders);
            
            PropertyVideo.Validate(videoSources);

            PropertyAudio.Validate(audioSources);
            PropertyNetwork.Validate();

            return true;
        }


        private MediaStreamer mediaStreamer = null;
        private StreamSession streamSession = null;

        [Newtonsoft.Json.JsonIgnore]
        internal AudioStreamSettings AudioSettings => streamSession?.AudioSettings;

        [Newtonsoft.Json.JsonIgnore]
        internal VideoStreamSettings VideoSettings => streamSession?.VideoSettings;

        public event Action StateChanged;
        public event Action<object> ErrorOccurred;

        [Newtonsoft.Json.JsonIgnore]
        public int ErrorCode { get; private set; }

        [Newtonsoft.Json.JsonIgnore]
        public Exception ErrorObj { get; private set; }


        [Newtonsoft.Json.JsonIgnore]
        public bool IsStreaming
        {
            get
            {
                bool isStreaming = false;

                if (streamSession != null && mediaStreamer != null)
                {
                    isStreaming = (mediaStreamer.State == MediaStreamerState.Streaming);
                }

                return isStreaming;
            }
            //set;
        }

        [Newtonsoft.Json.JsonIgnore]
        public bool IsBusy
        {
            get
            {
                bool isBusy = false;
                if (streamSession != null && mediaStreamer != null)
                {
                    isBusy = (mediaStreamer.State == MediaStreamerState.Starting ||
                        mediaStreamer.State == MediaStreamerState.Stopping);
                }
                return isBusy;
            }
        }

        public void SwitchStreamingState()
        {
            logger.Debug("SwitchStreamingState()");

            if (!IsStreaming)
            {
                StartStreaming();
            }
            else
            {
                StopStreaming();
            }
        }

        public void StartStreaming()
        {
            logger.Debug("StartStreaming()");

            if (mediaStreamer.State == MediaStreamerState.Shutdown)
            {
                this.ErrorObj = null;
                this.ErrorCode = 0;

                streamSession = CreateSession();


                streamSession.Validate();

                mediaStreamer.Start(streamSession);
            }

        }

        public void StopStreaming()
        {
            logger.Debug("StopStreaming()");

            if (mediaStreamer != null)
            {
                mediaStreamer.Stop();
            }
        }

        private void MediaStreamer_StateChanged()
        {
            var state = mediaStreamer.State;

            logger.Debug("MediaStreamer_StateChanged() " + state);

            if (state == MediaStreamerState.Starting)
            {
                //PropertyNetwork.CommunicationPort = streamSession.CommunicationPort;
            }
            else if (state == MediaStreamerState.Streaming)
            {
                PropertyNetwork.CommunicationPort = streamSession.CommunicationPort;
            }
            else if (state == MediaStreamerState.Stopping)
            {

            }
            else if (state == MediaStreamerState.Stopped)
            {

                PropertyNetwork.CommunicationPort = PropertyNetwork.Port;

                var errorObj = mediaStreamer.ExceptionObj;
                var errorCode = mediaStreamer.ErrorCode;
                var internalCode = mediaStreamer.CheckError();
                var internalError = mediaStreamer.GetInternalError();

                if (errorObj != null || errorCode != 0 || internalCode != 0 || internalError != null)
                {
                    logger.Warn("Process error...");
                    //...
                    //Process error


                    this.ErrorCode = internalCode;
                    this.ErrorObj = internalError;

					if(ErrorObj == null)
					{
						ErrorObj = errorObj;
					}

					if (ErrorCode != 0)
					{
						ErrorCode = errorCode;
					}

                    ErrorOccurred?.Invoke(ErrorObj);

                }

                mediaStreamer.Shutdown();

            }
            else if (state == MediaStreamerState.Shutdown)
            {

            }



            StateChanged?.Invoke();

        }

        private StreamSession CreateSession()
        {

            var session = StreamSession.Default();

            var transport = TransportMode.Udp;
            if (PropertyNetwork.UnicastProtocol == ProtocolKind.TCP)
            {
                transport = TransportMode.Tcp;
            }


            session.StreamName = Name;
            session.NetworkIpAddress = PropertyNetwork.Network ?? "0.0.0.0";

            if(PropertyNetwork.CommunicationPort <=0)
            {
                session.CommunicationPort = PropertyNetwork.Port;
            }
            else
            {
                session.CommunicationPort = PropertyNetwork.CommunicationPort;
            }
        

            //int communicationPort = PropertyNetwork.Port;
            //if (communicationPort <= 0)
            //{// если порт не задан - ищем свободный начиная с 808

            //    //communicationPort = GetRandomTcpPort();

            //    var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(System.Net.Sockets.ProtocolType.Tcp, 1, 808);
            //    if (freeTcpPorts != null && freeTcpPorts.Count() > 0)
            //    {
            //        communicationPort = freeTcpPorts.FirstOrDefault();
            //    }
            //}

            ////PropertyNetwork.CommunicationPort = communicationPort;

            //session.CommunicationPort = communicationPort;


            session.TransportMode = transport;
            
            bool isMulticast = !PropertyNetwork.IsUnicast;
            session.IsMulticast = isMulticast;

            if (isMulticast)
            {
                //VAlidate....
                session.MutlicastAddress = PropertyNetwork.MulticastIp;

                session.MutlicastPort1 = PropertyNetwork.MulticasVideoPort;
                session.MutlicastPort2 = PropertyNetwork.MulticasAudioPort;
            }

            var videoSettings = session.VideoSettings;

            var encoderResolution = new Size(PropertyVideo.ResolutionWidth, PropertyVideo.ResolutionHeight);

            videoSettings.StreamFlags &= ~VideoStreamFlags.UseEncoderResoulutionFromSource;
            if (AdvancedSettings.UseResolutionFromCaptureSource)
            {
                videoSettings.StreamFlags |= VideoStreamFlags.UseEncoderResoulutionFromSource;
            }
            else
            {
                encoderResolution = new Size(AdvancedSettings.Width, AdvancedSettings.Height);
            }


            var videoEncoderSettings = videoSettings.EncoderSettings;

            videoEncoderSettings.EncoderId = AdvancedSettings.EncoderId;
            videoEncoderSettings.Bitrate = AdvancedSettings.Bitrate;
            videoEncoderSettings.MaxBitrate = AdvancedSettings.MaxBitrate;
            videoEncoderSettings.Width = encoderResolution.Width;
            videoEncoderSettings.Height = encoderResolution.Height;


            videoEncoderSettings.FrameRate = new MediaRatio(AdvancedSettings.Fps, 1);
            videoEncoderSettings.Profile = AdvancedSettings.H264Profile;
            videoEncoderSettings.LowLatency = AdvancedSettings.LowLatency;

            var captureRegion = PropertyVideo.VideoRect;
            var captureType = PropertyVideo.CaptType;
            //var captureFps = PropertyVideo.CaptFps;
            var captureFps = AdvancedSettings.Fps;
            var useHardware = PropertyVideo.CaptUseHardware;

            var screenCaptureProperties = new ScreenCaptureProperties
            {
                CaptureMouse = PropertyVideo.CaptureMouse,

                CaptureType = captureType, // VideoCaptureType.DXGIDeskDupl,
                UseHardware = useHardware,
                Fps = captureFps,
                ShowDebugInfo = false,
                ShowDebugBorder = PropertyVideo.ShowCaptureBorder,
                AspectRatio = AdvancedSettings.KeepAspectRatio,
            };

            VideoCaptureDevice captureDevice = null;
            //var videoSourceItem = PropertyVideo.Display;
            if (PropertyVideo.IsUvcDevice)
            {
                captureDevice = new UvcDevice
                {

                    Name = PropertyVideo.DeviceName,
                    Resolution = captureRegion.Size,
                    DeviceId = PropertyVideo.DeviceId,
                };
            }
            else
            {
                captureDevice = new ScreenCaptureDevice
                {
                    CaptureRegion = captureRegion,
                    DisplayRegion = captureRegion,
                    Name = PropertyVideo.DeviceName,

                    Resolution = captureRegion.Size,
                    Properties = screenCaptureProperties,
                    DeviceId = PropertyVideo.DeviceId,
                };
            }

            if (PropertyAudio.IsEnabled)
            {
                var deviceId = PropertyAudio.DeviceId;

                var audioDevice = MediaToolkit.AudioTool.GetAudioCaptureDevices().FirstOrDefault(d => d.DeviceId == deviceId);
                if (audioDevice != null)
                {
                    session.AudioSettings.Enabled = true;
                    audioDevice.Properties = new WasapiCaptureProperties();

                    session.AudioSettings.CaptureDevice = audioDevice;
                }
            }
            else
            {
                session.AudioSettings.Enabled = false;
            }

            logger.Info("CaptureDevice: " + captureRegion);

            session.VideoSettings.CaptureDevice = captureDevice;

            return session;
        }

        private static int GetRandomTcpPort()
        {
            var port = 0;
            var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(System.Net.Sockets.ProtocolType.Tcp, 1).ToList();
            if (freeTcpPorts != null && freeTcpPorts.Count > 0)
            {
                Random rnd = new Random();
                var index = rnd.Next(0, freeTcpPorts.Count);

                port = freeTcpPorts[index]; //freeTcpPorts.FirstOrDefault();

            }

            return port;
        }

        public void Dispose()
        {
            logger.Debug("Dispose()");

            if (mediaStreamer != null)
            {
                if (mediaStreamer != null && mediaStreamer.State != MediaStreamerState.Shutdown)
                {
                    //Stop and shutdown...
                    logger.Warn("mediaStreamer!=null && mediaStreamer.State != MediaStreamerState.Shutdown");
                }

                mediaStreamer.StateChanged -= MediaStreamer_StateChanged;

                mediaStreamer.Shutdown();

            }

        }

    }
}
