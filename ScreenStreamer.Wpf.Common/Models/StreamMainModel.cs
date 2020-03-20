using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using MediaToolkit.Core;
using Newtonsoft.Json;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamMainModel
    {
        public static StreamMainModel Default => CreateDefault();

        private static StreamMainModel CreateDefault()
        {
            var defaultStream = new StreamModel(){Name = "Stream 1"};
            var @default = new StreamMainModel();
            @default.StreamList.Add(defaultStream);
            return @default;
        }

        public List<StreamModel> StreamList { get; set; } = new List<StreamModel>();
    }

    public class StreamModel
    {
        public string Name { get; set; }
        public bool IsStarted { get; set; }

        public AdvancedSettingsModel AdvancedSettingsModel { get; set; } = new AdvancedSettingsModel();
        public PropertyVideoModel PropertyVideo { get; set; } = new PropertyVideoModel();
        public PropertyQualityModel PropertyQuality { get; set; } = new PropertyQualityModel();
        public PropertyCursorModel PropertyCursor { get; set; } = new PropertyCursorModel();
        public PropertyAudioModel PropertyAudio { get; set; } = new PropertyAudioModel();
        public PropertyBorderModel PropertyBorder { get; set; } = new PropertyBorderModel();
        public PropertyNetworkModel PropertyNetwork { get; set; } = new PropertyNetworkModel();
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