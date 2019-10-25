using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Common
{
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

        public object HwContext = null;

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

    public class VideoEncodingParams
    {
        public int Width = 0;
        public int Height = 0;
        public int FrameRate = 0;
        public string EncoderName = "";

    }

    public class AudioEncodingParams
    {
        public int SampleRate = 0;
        public int Channels = 0;
        public int BitsPerSample = 0;
        public int BlockAlign = 0;
        public string Encoding = "";
        public string DeviceId = "";
    }

    public class NetworkStreamingParams
    {
        public string LocalAddr = "";
        public int LocalPort = 0;

        public string RemoteAddr = "";
        public int RemotePort = 0;

        public int MulticastTimeToLive = 10;

        public TransportMode TransportMode = TransportMode.Udp;

    }

    public enum TransportMode
    {
        Tcp,
        Udp,
        Unknown,
    }

    public enum AudioEncoderMode
    {
        AAC,
        G711,
    }

    public enum VideoEncoderMode
    {
        H264,
        JPEG
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
}
