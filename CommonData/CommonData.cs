using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonData
{
    public class VideoBuffer
    {
        public VideoBuffer(int width, int height, System.Drawing.Imaging.PixelFormat fmt)
        {
             this.bitmap = new Bitmap(width, height, fmt);
        }
    
        public readonly object syncRoot = new object();
        public readonly Bitmap bitmap = null;
        public double time = 0;
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
        public string MulitcastAddres = "";
        public int Port = 0;

    }
}
