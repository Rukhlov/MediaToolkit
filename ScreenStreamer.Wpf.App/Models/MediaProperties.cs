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
using System.IO;
using MediaToolkit.NativeAPIs.Utils;

namespace ScreenStreamer.Wpf.Models
{

    public class PropertyNetworkModel
    {
        /// <summary>
        /// Адрес сетевого интерфейса где будет работать стример
        /// </summary>
        public string Network { get; set; } = "0.0.0.0";

        /// <summary>
        /// Порт заданный пользователем
        /// 0 - если авто
        /// </summary>
        public int Port { get; set; } = 808;


        /// <summary>
        /// Реальный порт на котором работает сервис
        /// </summary>
        [JsonIgnore]
        public int CommunicationPort { get; set; } = 0;

        public bool IsUnicast { get; set; } = true;
        public ProtocolKind UnicastProtocol { get; set; } = ProtocolKind.TCP;

        public string MulticastIp { get; set; } = "239.0.0.1";
        public int MulticasVideoPort { get; set; } = 0;
        public int MulticasAudioPort { get; set; } = 0;

        public void Init(IEnumerable<int> appPorts = null)
        {

            //if (Port <= 0)
            //{
            //    Port = NetworkHelper.FindAvailableTcpPort();
            //}

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

		public bool ShowDebugInfo { get; set; } = false;

		public void Init(IEnumerable<VideoSourceItem> videoSources, IEnumerable<ScreenCaptureItem> screenCaptures)
        {

			if (!screenCaptures.Any(c => c.CaptType == this.CaptType))
			{
				Debug.WriteLine("CaptType " + CaptType + " not supported");
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

        public void Init(IEnumerable<AudioSourceItem> audioSources)
        {

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
		public PixFormat PixelFormat { get; set; } = PixFormat.NV12;
		public ColorSpace ColorSpace { get; set; } = ColorSpace.BT709;
		public ColorRange ColorRange { get; set; } = ColorRange.Partial;
		public VideoDriverType DriverType { get; set; } = VideoDriverType.CPU;

		public ScalingFilter DownscaleFilter { get; set; } = ScalingFilter.Linear;

		//public EncoderItem VideoEncoder { get; set; }

		[JsonIgnore]
        public bool AutoStartStreamingOnStartup
        {
            get
            {
                string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
                //string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                string name = "ScreenStreamer";
                string shortcutFileName = Path.Combine(shortcutPath, name + ".lnk");
                return File.Exists(shortcutFileName);
            }
            set
            {
                if (value)
                {
                    EnableAutoStreaming();
                }
                else
                {
                    DisableAutoStreaming();
                }
                
            }

        }


        public void EnableAutoStreaming()
        {
            string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
            //string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string name = "ScreenStreamer";

            string shortcutFileName = Path.Combine(shortcutPath, name + ".lnk");

            //string workingDir = AppDomain.CurrentDomain.BaseDirectory;
            //string appName = AppConsts.ApplicationName
            //string fileName = Path.Combine(workingDir, "ScreenStreamer.Wpf.App.exe");

            string fileName = System.Reflection.Assembly.GetEntryAssembly().Location;
            string workingDir = AppDomain.CurrentDomain.BaseDirectory;

            string _args = "-autostream";

			MediaToolkit.Utils.ShortcutUtil.CreateShortcut(shortcutFileName, fileName, _args, workingDir, name);

        }

        public void DisableAutoStreaming()
        {
            string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
            //string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            string name = "ScreenStreamer";

            string shortcutFileName = Path.Combine(shortcutPath, name + ".lnk");

            string fileName = System.Reflection.Assembly.GetEntryAssembly().Location;

            if (File.Exists(shortcutFileName))
            {
                MediaToolkit.Utils.ShortcutUtil.DeleteShortcut(shortcutFileName, fileName);
            }
        }

        public void Init(IEnumerable<EncoderItem> encoders)
        {          
            var encoder = encoders.FirstOrDefault(e => e.Id == EncoderId) ?? encoders.FirstOrDefault();
            if(encoder == null)
            {
                // Что то пошло не так...
                // throw new Exception...
            }

            if(encoder.Id != EncoderId)
            {
                this.EncoderId = encoder.Id;
				this.DriverType = encoder.DriverType;
				this.PixelFormat = encoder.PixelFormat;
				this.ColorRange = encoder.ColorRange;
				this.ColorSpace = encoder.ColorSpace;
            }

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

		public void Setup()
		{
			if (Width <= 0)
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
				Width = 640;
				Height = 480;
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
