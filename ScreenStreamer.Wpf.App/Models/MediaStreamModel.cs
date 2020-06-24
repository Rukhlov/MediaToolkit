using MediaToolkit.Core;
using NLog;
using ScreenStreamer.Common;

using ScreenStreamer.Wpf.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf
{
    public class MediaStreamModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MediaStreamModel()
        {
            mediaStreamer = new MediaStreamer();
            mediaStreamer.StateChanged += MediaStreamer_StateChanged;

            Init();
        }



        private MediaStreamer mediaStreamer = null;
        private StreamSession currentSession = null;

        public AudioStreamSettings AudioSettings => currentSession?.AudioSettings;
        public VideoStreamSettings VideoSettings => currentSession?.VideoSettings;


        public event Action OnStreamStateChanged;

        public string Name { get; set; } = "";

        public bool IsStarted
        {
            get
            {
                bool isStarted = false;

                if (currentSession != null && mediaStreamer != null)
                {
                    isStarted = (mediaStreamer.State == MediaStreamerState.Streamming);
                }

                return isStarted;
            }
            //set;
        }

        public bool IsBusy
        {
            get
            {
                bool isBusy = false;
                if (currentSession != null && mediaStreamer != null)
                {
                    isBusy = (mediaStreamer.State == MediaStreamerState.Starting ||
                        mediaStreamer.State == MediaStreamerState.Stopping);
                }
                return isBusy;
            }
        }



        public AdvancedSettingsModel AdvancedSettingsModel { get; set; } = new AdvancedSettingsModel();

        public PropertyVideoModel PropertyVideo { get; set; } = new PropertyVideoModel();

        public PropertyQualityModel PropertyQuality { get; set; } = new PropertyQualityModel();
        public PropertyCursorModel PropertyCursor { get; set; } = new PropertyCursorModel();
        public PropertyAudioModel PropertyAudio { get; set; } = new PropertyAudioModel();
        public PropertyBorderModel PropertyBorder { get; set; } = new PropertyBorderModel();
        public PropertyNetworkModel PropertyNetwork { get; set; } = new PropertyNetworkModel();

        public void Init()
        {
            AdvancedSettingsModel.Init();
            PropertyVideo.Init();

            PropertyNetwork.Init();
            //...
        }

        public void SwitchStreamingState()
        {
            logger.Debug("SwitchStreamingState()");

            if (!IsStarted)
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

                currentSession = CreateSession();


                currentSession.Validate();

                mediaStreamer.Start(currentSession);
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
                PropertyNetwork.CommunicationPort = currentSession.CommunicationPort;
            }
            else if (state == MediaStreamerState.Streamming)
            {

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

                    ErrorOccurred?.Invoke(ErrorObj);

                }

                mediaStreamer.Shutdown();

            }
            else if (state == MediaStreamerState.Shutdown)
            {

            }



            OnStreamStateChanged?.Invoke();

        }
        public event Action<object> ErrorOccurred;
        public int ErrorCode { get; private set; }
        public Exception ErrorObj { get; private set; }

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

            int communicationPort = PropertyNetwork.Port;
            if (communicationPort <= 0)
            {// если порт не задан - ищем свободный начиная с 808

                var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(System.Net.Sockets.ProtocolType.Tcp, 1, 808);
                if (freeTcpPorts != null && freeTcpPorts.Count() > 0)
                {
                    communicationPort = freeTcpPorts.FirstOrDefault();
                }
            }

            //PropertyNetwork.CommunicationPort = communicationPort;

            session.CommunicationPort = communicationPort;
            session.TransportMode = transport;

            session.IsMulticast = !PropertyNetwork.IsUnicast;


            var videoSettings = session.VideoSettings;
            var videoEncoderSettings = videoSettings.EncoderSettings;

            videoEncoderSettings.EncoderId = AdvancedSettingsModel.VideoEncoder.Id;
            videoEncoderSettings.Bitrate = AdvancedSettingsModel.Bitrate;
            videoEncoderSettings.MaxBitrate = AdvancedSettingsModel.MaxBitrate;


            videoEncoderSettings.FrameRate = new MediaRatio(AdvancedSettingsModel.Fps, 1);
            videoEncoderSettings.Profile = AdvancedSettingsModel.H264Profile;
            videoEncoderSettings.LowLatency = AdvancedSettingsModel.LowLatency;

            int x = (int)PropertyVideo.Left;
            int y = (int)PropertyVideo.Top;
            int w = (int)PropertyVideo.ResolutionWidth;
            int h = (int)PropertyVideo.ResolutionHeight;

            var captureRegion = new Rectangle(x, y, w, h);

            var captureType = PropertyVideo.CaptureType.CaptType;

            var screenCaptureProperties = new ScreenCaptureProperties
            {
                CaptureMouse = PropertyVideo.CaptureMouse,

                CaptureType = captureType, // VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
                ShowDebugInfo = false,
                ShowDebugBorder = PropertyVideo.ShowCaptureBorder,
            };

            VideoCaptureDevice captureDevice = null;
            var videoSourceItem = PropertyVideo.Display;
            if (videoSourceItem.IsUvcDevice)
            {
                captureDevice = new UvcDevice
                {

                    Name = videoSourceItem.Name,
                    Resolution = captureRegion.Size,
                    DeviceId = videoSourceItem.DeviceId,
                };
            }
            else
            {
                captureDevice = new ScreenCaptureDevice
                {
                    CaptureRegion = captureRegion,
                    DisplayRegion = captureRegion,
                    Name = PropertyVideo.Display.Name,

                    Resolution = captureRegion.Size,
                    Properties = screenCaptureProperties,
                    DeviceId = PropertyVideo.Display.DeviceId,
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
