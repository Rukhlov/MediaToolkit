using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.MediaFoundation
{


    public class VideoWriterArgs
    {
        public string FileName { get; set; }
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;

        //public IImageProvider ImageProvider { get; set; }
        public int FrameRate { get; set; } = 15;
        public int VideoQuality { get; set; } = 70;
        //public int AudioQuality { get; set; } = 50;
        //public IAudioProvider AudioProvider { get; set; }
    }

    enum RateControlMode
    {
        CBR,
        PeakConstrainedVBR,
        UnconstrainedVBR,
        Quality,
        LowDelayVBR,
        GlobalVBR,
        GlobalLowDelayVBR
    };

    class Packer
    {
        public static long ToLong(int left, int right)
        {
            return ((long)left << 32 | (uint)right);
        }

    }
    class MfCommon
    {

    }

}
