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
        object LastError { get; }
        int ErrorCode { get; }

        CaptureState State { get; }
        VideoBufferBase VideoBuffer { get; }

        event Action<object> CaptureStopped;
        event Action CaptureStarted;
        event Action<IVideoFrame> FrameAcquired;

        void Setup(object pars);
        void Start();
        void Stop();
        void Close(bool force = false);

        Utils.StatCounter Stats { get; }
    }
}
