using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    }

    public class NetworkStreamingParams
    {
        public string SrcAddr = "";
        public int SrcPort = 0;

        public string DestAddr = "";
        public int DestPort = 0;

        public int MulticastTimeToLive = 10;

    }


    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IRemoteDesktopService
    {

        [OperationContract(IsInitiating = true)]
        SessionDescriptionParams Connect(string ClientId);

        [OperationContract(IsTerminating = true)]
        void Disconnect();

        [OperationContract]
        bool Start(SessionOptions options);

        [OperationContract]
        bool Stop();

        [OperationContract]
        object SendMessage(string command, object[] args);

        //[OperationContractAttribute(AsyncPattern = true)]
        //IAsyncResult BeginSendMessage1(string command, object[] args, AsyncCallback callback, object asyncState);
        //object EndSendMessage1(IAsyncResult result);

        [OperationContract(IsOneWay = true)]
        void PostMessage(string command, object[] args);
    }



    public class SessionDescriptionParams
    {
        public string ServerId { get; set; }

        public List<RemoteScreen> Screens { get; set; } = new List<RemoteScreen>();


        //...
    }

    public class RemoteScreen
    {
        public string DeviceName { get; set; }
        public bool IsPrimary { get; set; }

        public Rectangle Bounds { get; set; } = Rectangle.Empty;

    }

    public class SessionOptions
    {
        public string DestAddr { get; set; } = "239.0.0.1";

        public int DestPort { get; set; } = 1234;

        public int FrameRate { get; set; } = 30;

        public bool EnableInputSimulator { get; set; } = true;

        public Rectangle SrcRect { get; set; } = Rectangle.Empty;

        public Size DstSize { get; set; } = new Size(1920, 1080);

        public bool AspectRatio { get; set; } = true;

        public bool ShowMouse { get; set; } = true;
    }
}
