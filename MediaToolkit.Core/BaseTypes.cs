using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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


    public class VideoEncoderSettings
    {
        public VideoEncoderMode Encoder = VideoEncoderMode.H264;

        public Size Resolution = Size.Empty;
        //public int Width = 0;
        //public int Height = 0;
        public int FrameRate = 0;
        public string EncoderName = "";

        public int Bitrate = 2500;
        public int MaxBitrate = 5000;
        public bool LowLatency = true;
        public int Quality = 75;

        public H264Profile Profile = H264Profile.Main;

        public BitrateControlMode BitrateMode = BitrateControlMode.CBR;

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


    public class AudioEncoderSettings
    {
        public int SampleRate = 8000;
        public int Channels = 1;
        public int BitsPerSample = 0;
        public int BlockAlign = 0;
        public string Encoding = "";
        public string DeviceId = "";
        public AudioEncoderMode Encoder = AudioEncoderMode.G711;
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


    public class AudioCaptureSettings
    {
        public string Name = "";
        public string DeviceId = "";

        public int BitsPerSample = 0;
        public int SampleRate = 0;
        public int Channels = 0;

        public string Description = "";

        public AudioCapturesTypes CapturesTypes = AudioCapturesTypes.Wasapi;
    }

    public enum AudioCapturesTypes
    {
        Wasapi,
        WasapiLoopback,
        WaveIn
    }

    public abstract class VideoCaptureDescription
    {
        public string Name = "";
        public Size Resolution = new Size(640, 480);
        public abstract CaptureMode CaptureMode { get; }

    }

    public enum CaptureMode
    {
        Screen,
        CaptDevice,
    }

    public class VideoCaptureDeviceDescription: VideoCaptureDescription
    {
        public override CaptureMode CaptureMode => CaptureMode.CaptDevice;
        public string DeviceId = "";

        public VideoCaptureDeviceProfile CurrentProfile = null;
    }

    public class VideoCaptureDeviceProfile
    {
        public Size FrameSize = Size.Empty;
        public double FrameRate = 0;
        public string Format = "";

        public override string ToString()
        {
            return FrameSize.Width + "x" + FrameSize.Height + " " + FrameRate.ToString("0.0") + " fps " + Format; 
        }
    }

    public class ScreenCaptureDeviceDescription: VideoCaptureDescription
    {
        public override CaptureMode CaptureMode => CaptureMode.Screen;
        public string DisplayName = "";
        public Rectangle CaptureRegion = new Rectangle(0, 0, 640, 480);
        public Rectangle DisplayRegion = new Rectangle(0, 0, 640, 480);

        //public ScreenCaptureDescription CaptureDescription = new ScreenCaptureDescription();

        public VideoCaptureType CaptureType = VideoCaptureType.GDI;
        public int Fps = 10;
        public bool CaptureMouse = false;
        public bool AspectRatio = true;
        public bool UseHardware = true;
    }

    public class ScreenCaptureDescription
    {
        public VideoCaptureType CaptureType = VideoCaptureType.GDI;
        public int Fps = 10;
        public bool CaptureMouse = false;
        public bool AspectRatio = true;
        public bool UseHardware = true;
    }

    public enum VideoCaptureType
    {
        GDI,
        Direct3D9,
        GDIPlus,
        Datapath,
        DXGIDeskDupl,
    }



    public class NetworkSettings
    {
        public string LocalAddr = "";
        public int LocalPort = 0;

        public string RemoteAddr = "";
        public int RemotePort = 0;

        public int MulticastTimeToLive = 10;

        public TransportMode TransportMode = TransportMode.Udp;

        public uint SSRC = 0;

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

}
