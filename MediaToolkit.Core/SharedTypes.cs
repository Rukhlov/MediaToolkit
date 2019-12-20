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
        //...
    }

    public enum ErrorCode: int
    {
        OK = 0,

        //...
        UnknownError = 1,

    }
}
