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
        public string Address = "";
        public int Port = 0;

    }
}
