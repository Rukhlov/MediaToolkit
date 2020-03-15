using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaToolkit.Core
{

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
    public class VideoEncoderSettings
    {

        public VideoEncoderMode Encoder { get; set; } = VideoEncoderMode.H264;

        [XmlIgnore]
        public Size Resolution => new Size(Width, Height); //{ get; set; } = Size.Empty;

        public int Width { get; set; } = 1920;

        public int Height { get; set; } = 1080;

        public int FrameRate { get; set; } = 30;

        public string EncoderName { get; set; } = "";

        public int Bitrate { get; set; } = 2500;

        public int MaxBitrate { get; set; } = 5000;

        public bool LowLatency { get; set; } = true;

        public int Quality { get; set; } = 75;

        public H264Profile Profile { get; set; } = H264Profile.Main;

        public BitrateControlMode BitrateMode { get; set; } = BitrateControlMode.CBR;

    }

    public enum VideoEncoderMode
    {
        H264,
        JPEG
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

    [Serializable]
    public class AudioEncoderSettings
    {
        public int SampleRate { get; set; } = 8000;

        public int Channels { get; set; } = 1;

        [XmlIgnore]
        public int BitsPerSample { get; set; } = 0;

        [XmlIgnore]
        public int BlockAlign { get; set; } = 0;

        [XmlIgnore]
        public string Encoding { get; set; } = "PCMU";

        [XmlIgnore]
        public string DeviceId { get; set; } = "";

        public AudioEncoderMode Encoder { get; set; } = AudioEncoderMode.G711;
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


    public class AudioCaptureDeviceDescription
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

        //public AudioCapturesTypes CapturesTypes { get; set; } = AudioCapturesTypes.Wasapi;
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
    public abstract class VideoCaptureDevice
    {
        public string Name = "";
        public string DeviceId { get; set; } = "";

        // [XmlIgnore]
        [XmlElement(typeof(XmlSize))]
        public Size Resolution = new Size(640, 480);

        //[XmlIgnore]
        public abstract CaptureMode CaptureMode { get; }

    }

    public enum CaptureMode
    {
        Screen,
        UvcDevice,
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
        public Size FrameSize = Size.Empty;
        public double FrameRate = 0;
        public string Format = "";

        public override string ToString()
        {
            return FrameSize.Width + "x" + FrameSize.Height + " " + FrameRate.ToString("0.0") + " fps " + Format; 
        }
    }

    [Serializable]
    public class ScreenCaptureDevice: VideoCaptureDevice
    {
        public override CaptureMode CaptureMode => CaptureMode.Screen;

        //[XmlIgnore]
        [XmlElement(typeof(XmlRect))]
        public Rectangle DisplayRegion = new Rectangle(0, 0, 640, 480);

        [XmlElement(typeof(XmlRect))]
        public Rectangle CaptureRegion = new Rectangle(0, 0, 640, 480);

        [XmlIgnore]
        public ScreenCaptureProperties Properties { get; set; } = new ScreenCaptureProperties();

    
    }

    [Serializable]
    public class ScreenCaptureProperties
    {
        public VideoCaptureType CaptureType { get; set; } = VideoCaptureType.GDI;

        public int Fps { get; set; } = 10;

        public bool CaptureMouse { get; set; } = false;

        public bool AspectRatio { get; set; } = true;

        public bool UseHardware { get; set; } = true;

        public bool ShowDebugBorder { get; set; } = true;

        public bool ShowDebugInfo { get; set; } = true;

        //public bool CustomRegion = false;
    }


    public enum VideoCaptureType
    {
        GDI,
        Direct3D9,
        GDIPlus,
        Datapath,
        DXGIDeskDupl,
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
        public VideoEncoderMode VideoEncoder { get; set; }

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
