using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.SharedTypes
{
    public interface IMediaToolkitBootstrapper
    {
        void Startup();
        void Shutdown();
       
    }


    public class HttpScreenStreamerArgs
    {
        public string Addres = "0.0.0.0";
        public int Port = 8086;
        public Rectangle CaptureRegion = Rectangle.Empty;
        public Size Resolution = Size.Empty;
        public int Fps = 10;
        public bool CaptureMouse = true;
        public Core.VideoCaptureType CaptureTypes = Core.VideoCaptureType.GDI;

        public override string ToString()
        {
            return string.Join("; ", new { Addres, Port, CaptureRegion, Resolution, Fps, CaptureTypes, CaptureMouse });
        }

    }

    public interface IHttpScreenStreamer
    {
        void Setup(HttpScreenStreamerArgs args);
        void Start();
        void Stop();
        void Close(bool force = false);

        event Action<object> StreamerStopped;
        event Action StreamerStarted;
        int ErrorCode { get; }

        Core.MediaState State { get; }

    }

    public interface IScreenCasterControl
    {
        void Connect(string addr, int port);
        void Disconnect();

        bool ShowDebugPanel { get; set; }

        ClientState State { get; }
        ErrorCode Code { get; }

        event Action Connected;
        event Action<object> Disconnected;

        //...
    }


    public class AudioRendererArgs
    {
        public string DeviceId { get; set; } = "";

        //Input wave format...
        public int SampleRate { get; set; } = 44100;
        public int Channels { get; set; } = 2;
        public WaveEncodingTag Encoding { get; set; } = WaveEncodingTag.PCM;
        public int BitsPerSample { get; set; } = 32;

        public override string ToString()
        {
            return string.Join(";" , DeviceId ?? "", SampleRate, Encoding, BitsPerSample, Channels);
        }
    }


    public interface IAudioRenderer
    {
        bool Mute { get; set; }
        float Volume { get; set; }

        void Setup(AudioRendererArgs args);
        void Start(long time);
        void Stop();
        void Close();


        int ErrorCode { get; }
    }

    public class VideoRendererArgs
    {
        public IntPtr hWnd { get; set; }

        public Size Resolution { get; set; } = Size.Empty;
        public int FourCC { get; set; } = 0; //"ARGB"; //UYVY //FourCC code
        public int BitsPerPixel { get; set; } = 32;
        public int InterlaceMode { get; set; } = 2; //Progressive
        public override string ToString()
        {
            return string.Join(";", hWnd, Resolution, FourCC, BitsPerPixel);
        }
    }

    public interface IVideoRenderer
    {

        void Setup(VideoRendererArgs args);
        void Start(long time);
        void Stop();
        void Close();


        void Resize(System.Drawing.Rectangle rect);
        void Repaint();

        int ErrorCode { get; }
    }

    public interface IMediaRenderSession
    {
        void Setup(VideoRendererArgs videoArgs, AudioRendererArgs audioArgs);
        void Start(long time);
        void Stop();
        void Close();

        bool Mute { get; set; }
        float Volume { get; set; }

        void Resize(System.Drawing.Rectangle rect);
        void Repaint();

        void ProcessAudioPacket(IntPtr data, int length, double time, double duration);
        void ProcessVideoFrame(IntPtr data, int length, double time, double duration);

        int ErrorCode { get; }
    }


    public interface IDeckLinkInputDevice
    {
        int DeviceIndex { get; }
        string DisplayName { get; }
        string ModelName { get; }

        VideoFormat VideoFormat { get; }
        System.Drawing.Size FrameSize { get; }
        Tuple<long, long> FrameRate { get; }
        int VideoInterlaceMode { get; }

        int AudioSampleRate { get; }
        int AudioChannelsCount { get; }
        int AudioBitsPerSample { get; }

        ErrorCode ErrorCode { get; }
        void StartCapture(DeckLinkDeviceDescription device, DeckLinkDisplayModeDescription mode = null);
        void StopCapture();

        event Action<bool> CaptureChanged;

        //event Action<byte[], double> AudioDataArrived;
        event Action<IntPtr, int, double, double> AudioDataArrived;
        event Action<IntPtr, int, double, double> VideoDataArrived;

        void Shutdown();

    }

    public interface IDeckLinkDeviceManager
    {

        List<DeckLinkDeviceDescription> FindDevices();
    }

    public interface IDeckLinkInputControl
    {

        List<DeckLinkDeviceDescription> FindDevices();

        int Volume { get; set; }
        bool Mute { get; set; }

        void StartCapture(int deviceIndex);
        void StopCapture();
        ErrorCode Code { get; }
        bool IsCapture { get; }

        int DeviceIndex { get; }

        event Action CaptureStarted;
        event Action CaptureStopped;

        bool DebugMode { get; set; }

        void SetStatusText(string text);

    }


    public class DeckLinkDeviceDescription
    {
        public string DeviceHandle { get; set; } = "";

        public int DeviceIndex { get; set; } = -1;
        public string DeviceName { get; set; } = "";
        public bool Available { get; set; } = false;
        public bool IsBusy { get; set; } = false;

        public List<DeckLinkDisplayModeDescription> DisplayModeIds { get; set; } = null;

        public override string ToString()
        {
            return DeviceName + " " + (Available ? "(Available)" : "(Not Available)");
        }
    }

    public class DeckLinkDisplayModeDescription
    {
        public long ModeId { get; set; } = 1769303659; //(long)_BMDDisplayMode.bmdModeUnknown;

        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public double Fps { get; set; } = 0;
        public long PixFmt { get; set; } = 0;//(long)_BMDPixelFormat.bmdFormatUnspecified;

        public string Description { get; set; } = "";


        public override string ToString()
        {
            return Description + " (" + Width + "x" + Height + "_" + Fps.ToString("0.##") + ")";
        }

    }




    public enum ErrorCode : int
    {
        Ok = 0,
        Fail = 1,
        Unexpected,
        Abort,
        Cancelled, 
        Interrupted,
        IsBusy,
        NotFound, 
        NotReady,
        NotSupported,
        //...

    }

    public enum ClientState
    {
        Connecting,
        Connected,
        Running,
        Interrupted,
        Disconnecting,
        Disconnected,

    }

}
