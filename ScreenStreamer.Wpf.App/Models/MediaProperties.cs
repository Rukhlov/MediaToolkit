using MediaToolkit.Core;
using Newtonsoft.Json;

using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ScreenStreamer.Wpf.Models
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

        public void Validate()
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
        [JsonIgnore]
        public bool IsBorderVisible { get; set; }

        // размеры в пикселях
        public int Top { get; set; } = 10;
        public int Left { get; set; } = 10;
        public int Width { get; set; } = 640;
        public int Height { get; set; } = 480;
       
        [JsonIgnore]
        public System.Drawing.Rectangle BorderRect => new System.Drawing.Rectangle(Left, Top, Width, Height);

        public void Validate()
        {

            if(Width <= 0)
            {
                Width = 10;
            }
            
            if (Height <= 0)
            {
                Height = 10;
            }

            var decktopRect = System.Windows.Forms.SystemInformation.VirtualScreen;
            var rect = System.Drawing.Rectangle.Intersect(decktopRect, BorderRect);

            if (rect.Width > 0 && rect.Height > 0)
            { // внутри экрана все ок...

            }
            else
            {// за границей экрана, сбрасываем по дефолту

                Debug.WriteLine("Invalid region: " + BorderRect);

                Left = 10;
                Top = 10;            
                Width  = 640;
                Height = 480;
            }
               
        }
    }

    public class PropertyVideoModel
    {
        //public string Display { get; set; } = ScreenHelper.ALL_DISPLAYS;

        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public bool IsUvcDevice { get; set; } = false;

        [JsonIgnore]
        public bool IsRegion => (DeviceId == "ScreenRegion");

        //public bool IsRegion { get; set; }

        public int Top { get; set; } = 0;
        public int Left { get; set; } = 0;

        public int ResolutionWidth { get; set; } = 1920;
        public int ResolutionHeight { get; set; } = 1080;

        [JsonIgnore]
        public System.Drawing.Rectangle VideoRect => new System.Drawing.Rectangle(Left, Top, ResolutionWidth, ResolutionHeight);


        public VideoCaptureType CaptType { get; set; } = VideoCaptureType.GDI;
        public int CaptFps { get; set; } = 30;
        public bool CaptUseHardware { get; set; } = true;


        public bool CaptureMouse { get; set; } = true;

        public bool ShowCaptureBorder { get; set; } = false;

        public void Validate(IEnumerable<VideoSourceItem> videoSources = null)
        {
            // Validate device id...
            if(videoSources == null)
            {
                videoSources = ScreenHelper.GetVideoSources();
            }

            if (!videoSources.Any(i=>i.DeviceId == DeviceId))
            {// TODO: если девайс больше не доступен, то что то делаем...
                Debug.WriteLine("Device " + DeviceId + " not found");


                // оставляем как есть что бы пользователь сам выбрал нужный
                //DeviceId = "";
            }

            if (string.IsNullOrEmpty(DeviceId))
            {
                var device = videoSources.FirstOrDefault();

                this.DeviceId = device.DeviceId;
                this.DeviceName = device.Name;
                this.IsUvcDevice = device.IsUvcDevice;

                var rect = device.CaptureRegion;
                this.Left = rect.Left;
                this.Top = rect.Top;

                this.ResolutionWidth = rect.Width;
                this.ResolutionHeight = rect.Height;
            }

            if (!ScreenHelper.SupportedCaptures.Any(c=>c.CaptType == this.CaptType))
            {
                Debug.WriteLine("CaptType " + CaptType + " not supported");
            }

            //if (this.CaptureType == null)
            //{
            //    // this.CaptureType = new ScreenCaptureType { CaptType = VideoCaptureType.DXGIDeskDupl, Name = "DeskDupl" };
            //    CaptureType = ScreenCaptureItem.SupportedCaptures.FirstOrDefault();
            //}
        }
    }


    public class PropertyAudioModel
    {
        public bool IsEnabled { get; set; }
        public bool IsComputerSoundEnabled { get; set; } = true;
        public string DeviceId { get; set; }

        public void Validate(IEnumerable<AudioSourceItem> audioSources = null)
        {
            if(audioSources == null)
            {
                audioSources = AudioHelper.GetAudioSources();

            }

            //if(!devices.Any(d=>d.DeviceId == DeviceId))
            //{// девайс больше не доступен сбрасываем на дефолтный

            //    // TODO: может быть лучше оставлять как есть чтобы пользователь сам выбрал правильный !!!

            //    DeviceId = "";
            //}

            if (string.IsNullOrEmpty(DeviceId))
            {
                var device = audioSources.FirstOrDefault();
                DeviceId = device?.DeviceId;
            }
        }
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

        public string EncoderId { get; set; } = "";
        //public EncoderItem VideoEncoder { get; set; }

        public void Validate(IEnumerable<EncoderItem> encoders = null)
        {
            if(encoders == null)
            {
                encoders = EncoderHelper.GetVideoEncoders();
            }
           
            var encoder = encoders.FirstOrDefault(e => e.Id == EncoderId) ?? encoders.FirstOrDefault();
            if(encoder == null)
            {
                // Что то пошло не так...
                // throw new Exception...
            }

            if(encoder.Id != EncoderId)
            {
                this.EncoderId = encoder.Id;
            }

        }

    }


    // не нужно
    public class PropertyQualityModel
    {
        public QualityPreset Preset { get; set; } = QualityPreset.Standard;
    }

    // не нужно
    public class PropertyCursorModel
    {
        public bool IsCursorVisible { get; set; } = true;
    }
}
