using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaToolkit.Core
{

    public static class Config
    {
        public static int MaximumTransmissionUnit = 1300;

        public static int MaxVideoEncoderWidth = 4096;
        public static int MaxVideoEncoderHeight = 4096;

        public static int MinVideoEncoderWidth = 64;
        public static int MinVideoEncoderHeight = 64;

        //...
    }

    public class VideoFormat
    {
        public VideoFormat(int fourCC, string name, string description)
        {
            this.FourCC = fourCC;
            this.Name = name;
            this.Description = description;
        }

        public readonly int FourCC= -1;
        public readonly string Name = "";
        public readonly string Description = "";

    }

    public class VideoFormats
    {
        //https://docs.microsoft.com/en-us/windows/win32/medfound/video-subtype-guids

        public readonly static VideoFormat VideoFormatARGB32 = new VideoFormat(0x00000015, "ARGB32", "RGB, 32 bpp with alpha channel.");
        public readonly static VideoFormat VideoFormatUYVY = new VideoFormat(0x59565955, "UYVY", "");
        public readonly static VideoFormat VideoFormatV210 = new VideoFormat(30313276, "v210", "");
        
        //...
    }

    [Serializable]
    public class VideoEncoderSettings :ICloneable
    {

        [XmlAttribute]
        public string EncoderId { get; set; } = "";

        [XmlAttribute]
        public VideoCodingFormat EncoderFormat { get; set; } = VideoCodingFormat.H264;

        [XmlIgnore]
        public VideoEncoderDescription EncoderDescription { get; set; } = new VideoEncoderDescription();

        [XmlIgnore]
        public Size Resolution => new Size(Width, Height);

        [XmlAttribute]
        public int Width { get; set; } = 1920;

        [XmlAttribute]
        public int Height { get; set; } = 1080;

        [XmlElement]
        public MediaRatio FrameRate { get; set; } = new MediaRatio(30, 1);

        [XmlIgnore]
        public double FramePerSec => FrameRate.Num / (double)FrameRate.Den;

        /// <summary>
        /// Avarage bitrate  kbit/sec
        /// </summary>
        [XmlAttribute]
        public int Bitrate { get; set; } = 2500;

        /// <summary>
        /// Max bitrate kbit/sec
        /// </summary>
        [XmlAttribute]
        public int MaxBitrate { get; set; } = 5000;

        [XmlAttribute]
        public int GOPSize { get; set; } = 30;

        [XmlAttribute]
        public bool LowLatency { get; set; } = true;

        [XmlAttribute]
        public int Quality { get; set; } = 75;

        [XmlAttribute]
        public H264Profile Profile { get; set; } = H264Profile.Main;

		[XmlElement]
		public MediaRatio AspectRatio { get; set; } = MediaToolkit.Core.AspectRatio.AspectRatio_1_1;

        [XmlAttribute]
        public BitrateControlMode BitrateMode { get; set; } = BitrateControlMode.CBR;

        [XmlAttribute]
        public bool UseHardware { get; set; } = true;

        [XmlIgnore]
        public long AverageTimePerFrame { get; set; } = 0;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public static class AspectRatio
    {
        public static readonly MediaRatio AspectRatio_1_1 = new MediaRatio(1, 1);

        public static readonly MediaRatio AspectRatio_16_9 = new MediaRatio(16, 9);
        public static readonly MediaRatio AspectRatio_16_10 = new MediaRatio(16, 10);

        public static readonly MediaRatio AspectRatio_4_3 = new MediaRatio(4, 3);
        public static readonly MediaRatio AspectRatio_5_4 = new MediaRatio(5, 4);

    }

    [Serializable]
	public class MediaRatio
	{
		public MediaRatio()
		{
            this.Num = 1;
            this.Den = 1;
		}

		public MediaRatio(int num, int den)
		{
            this.Num = num;
            this.Den = den;

		}

        [XmlAttribute]
        public int Num { get; set; } = 1;

        [XmlAttribute]
		public int Den { get; set; } = 1;

		public override string ToString()
        {
            return Num + ":" + Den;
        }

        public static implicit operator MediaRatio(Tuple<int, int> tuple)
		{
			return new MediaRatio(tuple.Item1, tuple.Item2);
		}

		public static implicit operator Tuple<int, int>(MediaRatio aspectRatio)
		{
			return new Tuple<int, int> (aspectRatio.Num, aspectRatio.Den);
		}
	} 

    public enum VideoCodingFormat
    {
        Unknown,
        H264,
        HEVC,
        JPEG,
    }

    public enum H264Profile
    {
        Base,
        Main,
        High,
    }

    public enum BitrateControlMode
    {
        CBR,
        VBR,
        Quality,
    }

    public enum PixFormat
    {
        Unknown,

        RGB565,
        RGB24,
        RGB32,

        NV12,
        I420,
        I422,
        I444,

        R8,
        R8G8,
    }

    public enum ColorSpace
    {
        BT601 = 0,
        BT709 = 1,
		BT2020 = 2,
	}

    public enum ColorRange
    {
        Partial = 0,
        Full = 1,
    }

	public enum VideoDriverType
	{
		CPU,
		DirectX,

		//not supported...
		Cuda,
		OpenGL,
	}

    public interface IVideoFrame
    {
        IFrameBuffer[] Buffer {get;}

        double Time { get; }
        double Duration { get; }

        int Width { get; }
        int Height { get; }
        PixFormat Format { get; }
        int Align { get; }

        ColorSpace ColorSpace { get; }
        ColorRange ColorRange { get; }

        int Size { get; }

        VideoDriverType DriverType { get; }
    }

    public interface IFrameBuffer
    {
        IntPtr Data { get; }
        int Stride { get; }
    }

    public class FrameBuffer : IFrameBuffer
    {
        public IntPtr Data { get; } = IntPtr.Zero;
        public int Stride { get; } = 0;

        public FrameBuffer(IntPtr data, int stride)
        {
            this.Data = data;
            this.Stride = stride;
        }
    }
	
	public enum ScalingFilter
	{
		Point,
		FastLinear,
		Linear,
		Bicubic,
		Lanczos,
		Spline,
	}

	[Serializable]
    public class AudioEncoderSettings : ICloneable
    {
        [XmlAttribute]
        public int SampleRate { get; set; } = 8000;

        [XmlAttribute]
        public int Channels { get; set; } = 1;

        [XmlIgnore]
        public int BitsPerSample { get; set; } = 0;

        [XmlIgnore]
        public int BlockAlign { get; set; } = 0;

        [XmlIgnore]
        public string Encoding { get; set; } = "PCMU";

        [XmlIgnore]
        public string DeviceId { get; set; } = "";

        [XmlAttribute]
        public AudioEncoderMode Encoder { get; set; } = AudioEncoderMode.G711;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public enum AudioEncoderMode
    {
        AAC,
        G711,

    }

    public enum WaveEncodingTag
    {
        PCM = 0x0001,
        IeeeFloat = 0x0003,
    }


    public class AudioCaptureDevice
    {
        public string Name { get; set; } = "";
        public string DeviceId { get; set; } = "";


        [XmlIgnore]
        public int SampleRate { get; set; } = 0;

        [XmlIgnore]
        public int Channels { get; set; } = 0;

        [XmlIgnore]
        public int BitsPerSample { get; set; } = 0;

        [XmlIgnore]
        public string Description { get; set; } = "";

		[XmlIgnore]
		public object Properties { get; set; } = new WasapiCaptureProperties();

	}

    public enum AudioCapturesTypes
    {
        Wasapi,
        WasapiLoopback,
        WaveIn
    }


    [Serializable]
    [XmlInclude(typeof(UvcDevice))]
    [XmlInclude(typeof(ScreenCaptureDevice))]
	[XmlInclude(typeof(WindowCaptureDevice))]
	public abstract class VideoCaptureDevice
    {
        public string Name { get; set; } = "";
        public string DeviceId { get; set; } = "";

        // [XmlIgnore]
        [XmlElement(typeof(XmlSize))]
        public Size Resolution  { get; set; } = new Size(640, 480);

        //[XmlIgnore]
        public abstract CaptureMode CaptureMode { get; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

    }

    public enum CaptureMode
    {
        Screen,
        UvcDevice,
		AppWindow
	}

    [Serializable]
    public class UvcDevice: VideoCaptureDevice
    {
        public override CaptureMode CaptureMode => CaptureMode.UvcDevice;

        [XmlIgnore]
        public UvcProfile CurrentProfile = null;
    }

    public class UvcProfile
    {
        public string Name = "";
        public Size FrameSize = Size.Empty;
        public double FrameRate = 0;
        public string Format = "";

        public override string ToString()
        {
            return FrameSize.Width + "x" + FrameSize.Height + " " + FrameRate.ToString("0.0") + " fps " + Format; 
        }
    }

    public class VideoEncoderDescription
    {
        public string Id = string.Empty;

        public string Name { get; set; } = "";

        public VideoCodingFormat Format { get; set; } = VideoCodingFormat.Unknown;
        public bool IsHardware { get; set; } = true;

        public string Description { get; set; } = "";

        public bool Activatable = false;

        public int VendorId = -1;
        public Guid ClsId = Guid.Empty;
        public string HardwareUrl = "";
        public object Tags = null;

        public override string ToString()
        {
            return string.Join("; ", Name, Format, IsHardware, Activatable);
            //return base.ToString();
        }
    }

    [Serializable]
    public class ScreenCaptureDevice: VideoCaptureDevice
    {
        public override CaptureMode CaptureMode => CaptureMode.Screen;

        [XmlElement(typeof(XmlRect))]
        public Rectangle DisplayRegion = new Rectangle(0, 0, 640, 480);

        [XmlElement(typeof(XmlRect))]
        public Rectangle CaptureRegion = new Rectangle(0, 0, 640, 480);

        [XmlIgnore]
        public ScreenCaptureProperties Properties { get; set; } = new ScreenCaptureProperties();
  
    }

	[Serializable]
	public class WindowCaptureDevice : VideoCaptureDevice
	{
		public override CaptureMode CaptureMode => CaptureMode.AppWindow;

		[XmlIgnore]
		public IntPtr hWnd = IntPtr.Zero;

		[XmlElement]
		public string WindowTitle = "";

		[XmlElement]
		public string WindowClass = "";

		[XmlElement]
		public string ProcName = "";

		[XmlElement]
		public int ProcId = -1;

		[XmlIgnore]
		public Rectangle ClientRect = Rectangle.Empty;

		[XmlIgnore]
		public ScreenCaptureProperties Properties { get; set; } = new ScreenCaptureProperties();

	}

	[Serializable]
    public class ScreenCaptureProperties
    {
        [XmlAttribute]
        public VideoCaptureType CaptureType { get; set; } = VideoCaptureType.GDI;

        [XmlAttribute]
        public int Fps { get; set; } = 10;

        [XmlAttribute]
        public bool CaptureMouse { get; set; } = false;

        [XmlAttribute]
        public bool AspectRatio { get; set; } = true;

        [XmlAttribute]
        public bool UseHardware { get; set; } = true;

        [XmlAttribute]
        public bool ShowDebugBorder { get; set; } = true;

        [XmlAttribute]
        public bool ShowDebugInfo { get; set; } = true;

        [XmlIgnore]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

    }


    public enum VideoCaptureType:int
    {
        GDI,
		GDILayered,
        Direct3D9,
        GDIPlus,
        Datapath,
        DXGIDeskDupl,
    }

    [Serializable]
    public class WasapiCaptureProperties
    {
        [XmlAttribute]
        public bool EventSyncMode { get; set; } = true;

        [XmlAttribute]
        public int BufferMilliseconds { get; set; } = 50;

        [XmlAttribute]
        public bool ExclusiveMode { get; set; } = false;
    }

    [Serializable]
    public class NetworkSettings
    {
        [XmlIgnore]
        public string LocalAddr { get; set; } = "";

        [XmlIgnore]
        public int LocalPort { get; set; } = 0;

        [XmlIgnore] 
        public string RemoteAddr { get; set; } = "";

        [XmlIgnore]
        public int RemotePort { get; set; } = 0;

        [XmlIgnore]
        public int MulticastTimeToLive { get; set; } = 10;

        [XmlIgnore]
        public TransportMode TransportMode { get; set; } = TransportMode.Udp;

        [XmlAttribute]
        public uint SSRC { get; set; } = 0;

    }

    public enum TransportMode
    {
        Tcp,
        Udp,
        Unknown,
    }


    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IRemoteDesktopService
    {

        [OperationContract(IsInitiating = true)]
        ConnectionResponse Connect(RemoteDesktopRequest request);

        [OperationContract(IsTerminating = true)]
        void Disconnect(RemoteDesktopRequest request);

        [OperationContract]
        RemoteDesktopResponse Start(StartSessionRequest request);

        [OperationContract]
        RemoteDesktopResponse Stop(RemoteDesktopRequest request);


        [OperationContract]
        object SendMessage(string command, object[] args);

        [OperationContract(IsOneWay = true)]
        void PostMessage(string command, object[] args);
    }


    [DataContract]
    public class RemoteDesktopRequest
    {
        [DataMember]
        public string SenderId { get; set; }

    }


    [DataContract]
    public class RemoteDesktopResponse
    {
        [DataMember]
        public int FaultCode { get; set; } = 0;

        [DataMember]
        public string FaultDescription { get; set; }

        [DataMember]
        public string ServerId { get; set; }

        public bool IsSuccess
        {
            get
            {
                return (FaultCode == 0);
            }
        }

    }


    public class ConnectionResponse : RemoteDesktopResponse
    {
        [DataMember]
        public string HostName { get; set; }

        [DataMember]
        public List<RemoteScreen> Screens { get; set; } = new List<RemoteScreen>();

        //...
    }

    [DataContract]
    public class RemoteScreen
    {
        [DataMember]
        public string DeviceName { get; set; }

        [DataMember]
        public bool IsPrimary { get; set; }

        [DataMember]
        public Rectangle Bounds { get; set; } = Rectangle.Empty;

    }

    [DataContract]
    public class StartSessionRequest: RemoteDesktopRequest
    {
        [DataMember]
        public string DestAddr { get; set; } = "239.0.0.1";

        [DataMember]
        public int DestPort { get; set; } = 1234;

        [DataMember]
        public int FrameRate { get; set; } = 30;

        [DataMember]
        public bool EnableInputSimulator { get; set; } = true;

        [DataMember]
        public Rectangle SrcRect { get; set; } = Rectangle.Empty;

        [DataMember]
        public Size DstSize { get; set; } = new Size(1920, 1080);

        [DataMember]
        public bool AspectRatio { get; set; } = true;

        [DataMember]
        public bool ShowMouse { get; set; } = true;
    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IRemoteDesktopClient
    {

        [OperationContract]
        object SendMessage(string command, object[] args);

        [OperationContract(IsOneWay = true)]
        void PostMessage(ServerRequest request);
    }

    public class ServerRequest :RemoteDesktopRequest
    {

        [DataMember]
        public string Command { get; set; }

        [DataMember]
        public object[] Args { get; set; }
    }



    [DataContract]
    public class ScreencastChannelInfo
    {
        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public TransportMode Transport { get; set; }

        [DataMember]
        public bool IsMulticast { get; set; }

        [DataMember]
        public MediaChannelInfo MediaInfo { get; set; }

        [DataMember]
        public int ClientsCount { get; set; }

        [DataMember]
        public uint SSRC { get; set; }

    }

    public enum MediaType
    {
        Video,
        Audio,
    }

    [DataContract]
    [KnownType(typeof(VideoChannelInfo))]
    [KnownType(typeof(AudioChannelInfo))]
    public abstract class MediaChannelInfo
    {
        [DataMember]
        public string Id { get; set; }

        //[DataMember]
        //public abstract MediaType MediaType { get; }
    }

    [DataContract]
    public class VideoChannelInfo : MediaChannelInfo
    {
        [DataMember]
        public VideoCodingFormat VideoEncoder { get; set; }

        [DataMember]
        public int Bitrate { get; set; }

        [DataMember]
        public Size Resolution { get; set; }

        [DataMember]
        public int Fps { get; set; }

        //[DataMember]
        //public override MediaType MediaType => MediaType.Video;

    }

    [DataContract]
    public class AudioChannelInfo: MediaChannelInfo
    {
        [DataMember]
        public AudioEncoderMode AudioEncoder { get; set; }

        [DataMember]
        public int Bitrate { get; set; }

        [DataMember]
        public int Channels { get; set; }

        [DataMember]
        public int SampleRate { get; set; }

        //[DataMember]
        //public override MediaType MediaType => MediaType.Audio;

    }

    [DataContract]
    public class ScreenCastResponse
    {
        [DataMember]
        public int FaultCode { get; set; } = 0;

        [DataMember]
        public string FaultDescription { get; set; }

        [DataMember]
        public string ServerId { get; set; }

        public bool IsSuccess
        {
            get
            {
                return (FaultCode == 0);
            }
        }

    }

    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IScreenCastService
    {
        [OperationContract]
        [ServiceKnownType(typeof(VideoChannelInfo))]
        [ServiceKnownType(typeof(AudioChannelInfo))]
        ScreencastChannelInfo[] GetChannelInfos();

        [OperationContract]
        ScreenCastResponse Play(ScreencastChannelInfo[] infos);

        [OperationContract]
        void Teardown();

        [OperationContract(IsOneWay = true)]
        void PostMessage(ServerRequest request);
    }



	public class VideoBuffer
    {
        public VideoBuffer(int width, int height, System.Drawing.Imaging.PixelFormat fmt)
        {
            this.bitmap = new Bitmap(width, height, fmt);
            var channels = Image.GetPixelFormatSize(fmt) / 8;
            this.length = channels * width * height;

            this.FrameSize = new Size(width, height);
        }

        public readonly object syncRoot = new object();

        public Size FrameSize { get; private set; } = Size.Empty;

        public Bitmap bitmap { get; private set; }

        public double time = 0;

        private long length = -1;
        public long DataLength { get => length; }

		public void Dispose()
        {
            lock (syncRoot)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
        }
    }

    public enum MediaState
    {
        Initialized,
        Stopped,
        Starting,
        Started,
        Stopping,
        Closing,
        Closed,
    }



    [Serializable]
    public struct XmlRect
    {
        [XmlAttribute]
        public int X { get; set; }

        [XmlAttribute]
        public int Y { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        public static implicit operator Rectangle(XmlRect xmlRectangle)
        {
            return new Rectangle(xmlRectangle.X, xmlRectangle.Y, xmlRectangle.Width, xmlRectangle.Height);
        }

        public static implicit operator XmlRect(Rectangle rectangle)
        {
            return new XmlRect { X = rectangle.X, Y = rectangle.Y, Height = rectangle.Height, Width = rectangle.Width };
        }

    }

    [Serializable]
    public struct XmlSize
    {
        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int Height { get; set; }


        public static implicit operator Size(XmlSize xmlSize)
        {
            return new Size(xmlSize.Width, xmlSize.Height);
        }

        public static implicit operator XmlSize(Size size)
        {
            return new XmlSize { Width = size.Width, Height = size.Height };
        }

    }
}
