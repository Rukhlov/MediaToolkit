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
        public int PixelFormat { get; set; } = 0; //"ARGB"; //UYVY //FourCC code
        public int BitsPerPixel { get; set; } = 32;
        public int InterlaceMode { get; set; } = 2;
        public override string ToString()
        {
            return string.Join(";", hWnd, Resolution, PixelFormat, BitsPerPixel);
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

        void ProcessAudioPacket(byte[] data, double time);
        void ProcessVideoFrame(IntPtr frameData, int frameLength, double frameTime, double frameDuration);

        int ErrorCode { get; }
    }

    public enum ErrorCode : int
    {
        Ok = 0,
        Unknown = 1,
        Cancelled = 2,
        Interrupted = 3,
        IsBusy = 4,
        NotFound = 5,
        NotReady = 6,
        
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
