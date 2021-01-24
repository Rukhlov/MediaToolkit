using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.ScreenCaptures
{
    public abstract class ScreenCapture
    {
        //protected static Logger logger =  LogManager.GetCurrentClassLogger();
        protected static TraceSource logger = TraceManager.GetTrace("MediaToolkit.ScreenCaptures");

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

            this.SrcRect = srcRect;
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format24bppRgb);
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppRgb);

            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppArgb);

            //this._VideoBuffer = new 

            this.DestSize = new Size(destSize.Width, destSize.Height);

        }

        public static ScreenCapture Create(VideoCaptureType type, Dictionary<string, object> args = null)
        {
            ScreenCapture capture = null;

            if (type == VideoCaptureType.GDI || type == VideoCaptureType.GDILayered)
            {
				var gdiCapt = new GDICapture(args);
				gdiCapt.CaptureAllLayers = (type == VideoCaptureType.GDILayered);

				capture = gdiCapt;
			}
            else if (type == VideoCaptureType.DXGIDeskDupl)
            {
                capture = new DDACapture(args);
            }
            else if (type == VideoCaptureType.Datapath)
            {
                capture = new DatapathDesktopCapture();
            }
            //else if (type == VideoCaptureType.GDIPlus)
            //{
            //    capture = new GDIPlusCapture();
            //}
            //else if (type == VideoCaptureType.Direct3D9)
            //{
            //    capture = new Direct3D9Capture(args);
            //}


            return capture;
        }

        public Rectangle SrcRect { get; protected set; }
		public PixFormat SrcFormat { get; protected set; } = PixFormat.RGB32;

		public Size DestSize { get; protected set; }
		public PixFormat DestFormat { get; set; } = PixFormat.NV12;

		public ScalingFilter DownscaleFilter { get; set; } = ScalingFilter.Linear;

		public ColorSpace ColorSpace { get; set; } = ColorSpace.BT709;
		public ColorRange ColorRange { get; set; } = ColorRange.Partial;

		public VideoBufferBase VideoBuffer { get; protected set; }
        public abstract ErrorCode UpdateBuffer(int timeout = 10);

        public bool CaptureMouse { get; set; }

        public bool AspectRatio { get; set; }

        public virtual void Close()
        {
            logger.Debug("ScreenCapture::Close()");

            if (VideoBuffer != null)
            {
                VideoBuffer.Dispose();
                VideoBuffer = null;
            }
        }

    }

}
