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

        public Rectangle SrcRect { get; protected set; }
		public PixFormat SrcFormat { get; protected set; } = PixFormat.RGB32;

		public Size DestSize { get; protected set; }

        public bool CaptureMouse { get; set; }

        public abstract ErrorCode TryGetFrame(out IVideoFrame frame, int timeout = 10);

		public virtual void Init(ScreenCaptureParameters captureParams)
		{
			var srcRect = captureParams.SrcRect;
			var destSize = captureParams.DestSize;

            Init(srcRect, destSize);
        }


		protected void Init(Rectangle srcRect, Size destSize = new Size())
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
			this.DestSize = new Size(destSize.Width, destSize.Height);

		}


		public virtual void Close()
        {
            logger.Debug("ScreenCapture::Close()");

            //if (VideoBuffer != null)
            //{
            //    VideoBuffer.Dispose();
            //    VideoBuffer = null;
            //}
        }


		public static ScreenCapture Create(VideoCaptureType type)
		{
			ScreenCapture capture = null;

			if (type == VideoCaptureType.GDI || type == VideoCaptureType.GDILayered)
			{
				var gdiCapt = new GDICapture();
				gdiCapt.CaptureAllLayers = (type == VideoCaptureType.GDILayered);

				capture = gdiCapt;
			}
			else if (type == VideoCaptureType.DXGIDeskDupl)
			{
				capture = new DDACapture();
			}
			else if (type == VideoCaptureType.Datapath)
			{
				//capture = new DatapathDesktopCapture();
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

	}

	public class ScreenCaptureParameters
	{
		public Rectangle SrcRect { get; set; }
		public Size DestSize { get; set; }
		public bool CaptureMouse { get; set; }

        public SharpDX.Direct3D11.Device D3D11Device { get; set; }
        public DDAOutputManager DDAOutputMan { get; set; }

    }
}
