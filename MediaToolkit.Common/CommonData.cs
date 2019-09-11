using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            this.size = channels * width * height;
        }
    
        public readonly object syncRoot = new object();

        public Bitmap bitmap = null;
        public double time = 0;

        private long size = -1;
        public long Size { get => size; }

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
        public string Address = "";
        public int Port = 0;

    }
}
