using MediaToolkit.Core;
using Newtonsoft.Json;

using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf
{


    public class PropertyNetworkModel
    {

        public string Network { get; set; } = "0.0.0.0";
        public int Port { get; set; } = 0;

        [JsonIgnore]
        public int CommunicationPort { get; set; } = 0;

        public bool IsUnicast { get; set; } = true;
        public ProtocolKind UnicastProtocol { get; set; } = ProtocolKind.TCP;

        public string MulticastIp { get; set; } = "239.0.0.1";
        public int MulticasVideoPort { get; set; } = 0;
        public int MulticasAudioPort { get; set; } = 0;

        public void Init()
        {
            //var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange((System.Net.Sockets.ProtocolType)UnicastProtocol, 1, Port);
            //if (freeTcpPorts != null)
            //{// может в любой момент изменится и свободный порт будет занят !!!
            //    var newPort = freeTcpPorts.FirstOrDefault();

            //    Port = newPort;
            //}
            //else
            //{
            //    //No avaliable tcp ports..;
            //}
        }

    }

    public class PropertyBorderModel
    {
        public bool IsBorderVisible { get; set; }

        // размеры в пикселях
        public int Top { get; set; } = 10;
        public int Left { get; set; } = 10;
        public int Width { get; set; } = 640;
        public int Height { get; set; } = 480;
       
        [JsonIgnore]
        public System.Drawing.Rectangle BorderRect => new System.Drawing.Rectangle(Left, Top, Width, Height);
    }

    public class PropertyVideoModel
    {
        //public string Display { get; set; } = ScreenHelper.ALL_DISPLAYS;

        public VideoSourceItem Display { get; set; }//= new ScreenItem();

        //public string DeviceId { get; set; }
        //public string DeviceName { get; set; }
        //public bool IsUvcDevice { get; set; }

        public bool IsRegion { get; set; }

        public int Top { get; set; } = 0;
        public int Left { get; set; } = 0;

        public int ResolutionWidth { get; set; } = 1920;
        public int ResolutionHeight { get; set; } = 1080;

       // public VideoCaptureType CaptType { get; set; }

        public ScreenCaptureItem CaptureType { get; set; } //= new ScreenItem();

        public bool CaptureMouse { get; set; } = true;

        public bool ShowCaptureBorder { get; set; } = false;

        public void Init()
        {
            if (Display == null)
            {
                Display = ScreenHelper.GetDisplayItems().FirstOrDefault();
            }

            if (this.CaptureType == null)
            {
                // this.CaptureType = new ScreenCaptureType { CaptType = VideoCaptureType.DXGIDeskDupl, Name = "DeskDupl" };
                CaptureType = ScreenCaptureItem.SupportedCaptures.FirstOrDefault();
            }
        }
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
        public bool IsEnabled { get; set; }
        public bool IsComputerSoundEnabled { get; set; } = true;
        public string DeviceId { get; set; }
    }


    public class AdvancedSettingsModel
    {
        public int Width { get; set; } = 1920;
        public int Height { get; set; } = 1080;
        public bool UseResolutionFromCaptureSource { get; set; } = true;
        public bool KeepAspectRatio { get; set; } = true;

        public int Bitrate { get; set; } = 2500;
        public int Fps { get; set; } = 30;
        public bool LowLatency { get; set; } = true;
        public int MaxBitrate { get; set; } = 5000;
        public H264Profile H264Profile { get; set; } = H264Profile.Main;

        public EncoderItem VideoEncoder { get; set; }

        public void Init()
        {
            if (VideoEncoder == null)
            {
                VideoEncoder = EncoderHelper.GetVideoEncoderItems().FirstOrDefault();

            }
        }

        //public VideoCodingFormat VideoEncoder { get; set; } = VideoCodingFormat.H264;
    }
}
