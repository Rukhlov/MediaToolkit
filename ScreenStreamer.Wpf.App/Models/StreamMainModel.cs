using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaToolkit.Core;
using Newtonsoft.Json;
using NLog;
using ScreenStreamer.Common;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamMainModel
    {
        public static StreamMainModel Default => CreateDefault();

        private static StreamMainModel CreateDefault()
        {
            var defaultStream = new StreamModel() { Name = "Stream 1" };

            defaultStream.InitStreamer();


            var @default = new StreamMainModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<StreamModel> StreamList { get; set; } = new List<StreamModel>();
    }

    public class StreamModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();


        public string Name { get; set; }
        public bool IsStarted
        {
            get
            {
                bool isStarted = false;

                if (Session != null && MediaStreamer != null)
                {
                    isStarted = (MediaStreamer.State == MediaStreamerState.Streamming);
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
                if (Session != null && MediaStreamer != null)
                {
                    isBusy = (MediaStreamer.State == MediaStreamerState.Starting ||
                        MediaStreamer.State == MediaStreamerState.Stopping);
                }

                return isBusy;
            }
            //set;
        }

        public AdvancedSettingsModel AdvancedSettingsModel { get; set; } = new AdvancedSettingsModel();
        public PropertyVideoModel PropertyVideo { get; set; } = new PropertyVideoModel();
        public PropertyQualityModel PropertyQuality { get; set; } = new PropertyQualityModel();
        public PropertyCursorModel PropertyCursor { get; set; } = new PropertyCursorModel();
        public PropertyAudioModel PropertyAudio { get; set; } = new PropertyAudioModel();
        public PropertyBorderModel PropertyBorder { get; set; } = new PropertyBorderModel();
        public PropertyNetworkModel PropertyNetwork { get; set; } = new PropertyNetworkModel();


        public MediaStreamer MediaStreamer { get; private set; }
        public StreamSession Session { get; private set; }


        public void InitStreamer()
        {
            logger.Debug("InitStreamer()");

            Session = StreamSession.Default();

            MediaStreamer = new MediaStreamer();
            MediaStreamer.StateChanged += MediaStreamer_StateChanged;


            var screen = System.Windows.Forms.Screen.PrimaryScreen;

            var bounds = screen.Bounds;

            var screenCaptureProperties = new ScreenCaptureProperties
            {
                CaptureMouse = true,
                AspectRatio = true,
                CaptureType = VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
                ShowDebugInfo = false,
            };

            ScreenCaptureDevice captureDevice = new ScreenCaptureDevice
            {
                CaptureRegion = bounds,
                DisplayRegion = bounds,
                Name = screen.DeviceName,

                Resolution = bounds.Size,
                Properties = screenCaptureProperties,
                DeviceId = screen.DeviceName,

            };

            Session.VideoSettings.CaptureDevice = captureDevice;

        }

        public void SwitchStreamingState()
        {
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

            if (MediaStreamer.State == MediaStreamerState.Shutdown)
            {
                Session.Setup();

                MediaStreamer.Start(Session);
            }

        }

        public event Action OnStreamStateChanged;

        private void MediaStreamer_StateChanged()
        {
            var state = MediaStreamer.State;

            logger.Debug("MediaStreamer_StateChanged() " + state);

            if (state == MediaStreamerState.Starting)
            {

            }
            else if (state == MediaStreamerState.Streamming)
            {
      
            }
            else if (state == MediaStreamerState.Stopping)
            {

            }
            else if (state == MediaStreamerState.Stopped)
            {

                MediaStreamer.Shutdown();
            }


            OnStreamStateChanged?.Invoke();

        }

        public void StopStreaming()
        {
            logger.Debug("StopStreaming()");

            MediaStreamer.Stop();

        }

        public void CloseStreamer()
        {
            logger.Debug("CloseStreamer()");

            if (MediaStreamer != null)
            {
                MediaStreamer.StateChanged -= MediaStreamer_StateChanged;

                //...
            }

        }

    }

    public class PropertyNetworkModel
    {
        public int Port { get; set; } = 2000;
        public bool IsUnicast { get; set; } = true;
        public ProtocolKind UnicastProtocol { get; set; } = ProtocolKind.TCP;
        public string MulticastIp { get; set; } = "239.0.0.1";
        public string Network { get; set; }
    }

    public class PropertyBorderModel
    {
        public bool IsBorderVisible { get; set; }
    }


    public class PropertyVideoModel
    {
        public string Display { get; set; } = ScreenHelper.ALL_DISPLAYS;
        public bool IsRegion { get; set; }

        public double Top { get; set; }
        public double Left { get; set; }
        public double ResolutionHeight { get; set; } = 600d;
        public double ResolutionWidth { get; set; } = 800d;
        public bool AspectRatio { get; set; }
    }
    public class PropertyQualityModel
    {
        public QualityPreset Preset { get; set; } = QualityPreset.Standard;
    }
    public class PropertyCursorModel
    {
        public bool IsCursorVisible { get; set; } = true;
    }
    public class PropertyAudioModel
    {
        public bool IsMicrophoneEnabled { get; set; }
        public bool IsComputerSoundEnabled { get; set; } = true;
        public string DeviceId { get; set; }
    }


    public class AdvancedSettingsModel
    {
        public int Bitrate { get; set; } = 2500;
        public int Fps { get; set; } = 30;
        public bool LowLatency { get; set; }
        public int MaxBitrate { get; set; } = 5000;
        public H264Profile H264Profile { get; set; } = H264Profile.Main;
        public VideoEncoderMode VideoEncoder { get; set; } = VideoEncoderMode.H264;
    }
}