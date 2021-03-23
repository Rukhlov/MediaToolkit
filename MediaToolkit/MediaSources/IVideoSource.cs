using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public enum CaptureState
    {
        Initialized,
        Stopped,
        Starting,
        Capturing,
        Stopping,
        Closed,
    }

    public interface IVideoSource
    {
       // VideoBuffer SharedBitmap { get; }

       // SharpDX.Direct3D11.Texture2D SharedTexture { get; }
        //int AdapterIndex { get; }
        //long AdapterId { get; }

        object LastError { get; }
        int ErrorCode { get; }

        CaptureState State { get; }
        event Action<IVideoFrame> FrameAcquired;

        event Action<object> CaptureStopped;
        event Action CaptureStarted;

        //System.Drawing.Size SrcSize { get; }

        VideoBufferBase VideoBuffer { get; }
        void Setup(object pars);
        void Start();
        void Stop();
        void Close(bool force = false);

        Utils.StatCounter Stats { get; }
    }
}
