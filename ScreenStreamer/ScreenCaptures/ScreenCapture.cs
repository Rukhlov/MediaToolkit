using CommonData;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer
{

    public enum CaptureType
    {
        GDI,
        Direct3D,
        GDIPlus,
        Datapath,
        DXGI,
    }


    public abstract class ScreenCapture
    {
        protected Logger logger { get; private set; }

        internal ScreenCapture()
        {
            logger = LogManager.GetLogger(GetType().ToString());
        }

        public virtual void Init(Rectangle srcRect, Size destSize = new Size())
        {
            logger.Debug("Init(...) " + srcRect.ToString() + " " + destSize.ToString());

            if (srcRect.Width == 0 || srcRect.Height == 0)
            {
                new ArgumentException(srcRect.ToString());
            }

            if (destSize.IsEmpty)
            {
                destSize.Width = srcRect.Width;
                destSize.Height = srcRect.Height;
            }

            if (destSize.Width == 0 || destSize.Height == 0)
            {
                new ArgumentException(destSize.ToString());
            }

            this.srcRect = srcRect;
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format24bppRgb);
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppRgb);

            this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppArgb);

        }

        public static ScreenCapture Create(CaptureType type, object[] args = null)
        {
            ScreenCapture capture = null;

            if (type == CaptureType.GDI)
            {
                capture = new GDICapture();
            }
            else if (type == CaptureType.Direct3D)
            {
                capture = new Direct3DCapture(args);
            }
            else if (type == CaptureType.GDIPlus)
            {
                capture = new GDIPlusCapture();
            }
            else if (type == CaptureType.Datapath)
            {
                capture = new DatapathDesktopCapture();
            }
            else if (type == CaptureType.DXGI)
            {
                capture = new DXGIDesktopDuplicationCapture(args);
            }

            return capture;
        }

        protected Rectangle srcRect;
        protected VideoBuffer videoBuffer = null;

        public VideoBuffer VideoBuffer { get => videoBuffer; }

        public abstract bool UpdateBuffer(int timeout = 10);


        public bool CaptureMouse { get; set; }


        public virtual void Close()
        {
            logger.Debug("ScreenCapture::Close()");

            if (videoBuffer != null)
            {
                videoBuffer.Dispose();
                videoBuffer = null;
            }
        }

    }



    class CaptureStats : ScreenStreamer.Utils.StatCounter
    {
        public double totalTime = 0;
        public long totalBytes = 0;
        public uint totalFrameCount = 0;

        public uint currentFrame = 0;
        public long currentBytes = 0;

        public double avgFrameInterval = 0;
        public double avgBytesPerSec = 0;

        public double lastTimestamp = 0;

        public void Update(double timestamp, int bytesSize)
        {

            if (lastTimestamp > 0)
            {
                var time = timestamp - lastTimestamp;

                avgFrameInterval = (time * 0.05 + avgFrameInterval * (1 - 0.05));
                avgBytesPerSec = bytesSize / avgFrameInterval;

                totalTime += (timestamp - lastTimestamp);
            }

            totalBytes += bytesSize;

            lastTimestamp = timestamp;
            totalFrameCount++;
        }


        public override string GetReport()
        {
            StringBuilder sb = new StringBuilder();

            var fps = (1 / this.avgFrameInterval);

            //var mbytesPerSec = this.avgBytesPerSec / (1024.0 * 1024);
            //var mbytes = this.totalBytes / (1024.0 * 1024);

            TimeSpan time = TimeSpan.FromSeconds(totalTime);
            sb.AppendLine(time.ToString(@"hh\:mm\:ss\.fff"));
            sb.AppendLine(fps.ToString("0.0") + " FPS");

            sb.AppendLine("");
            sb.AppendLine(this.totalFrameCount + " Frames");

            //sb.AppendLine(mbytesPerSec.ToString("0.0") + " MByte/s");

            sb.AppendLine(StringHelper.SizeSuffix((long)avgBytesPerSec) + "/s");
            sb.AppendLine(StringHelper.SizeSuffix(totalBytes));

            return sb.ToString();
        }

        public override void Reset()
        {
            this.totalBytes = 0;
            this.totalFrameCount = 0;
            this.currentFrame = 0;
            this.currentBytes = 0;

            this.avgFrameInterval = 0;
            this.avgBytesPerSec = 0;
            this.lastTimestamp = 0;

        }
    }


}
