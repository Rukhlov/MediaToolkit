using MediaToolkit.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.MediaStreamers
{
    public class HttpScreenStreamer : SharedTypes.IHttpScreenStreamer
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private VideoHttpStreamer httpStreamer = null;
        private IVideoSource httpScreenSource = null;

        public bool Setup(string addr, int port, Rectangle srcRect, Size destSize)
        {
            logger.Debug("HttpScreenStreamer::Setup()");

            bool Result = false;

            //var addr = "0.0.0.0";
            //var port = 8086;

            NetworkSettings networkParams = new NetworkSettings
            {
                RemoteAddr = addr,
                RemotePort = port,
            };


            //var srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //var destSize = new Size(1920, 1080);

            var ratio = srcRect.Width / (double)srcRect.Height;
            int destWidth = destSize.Width;
            int destHeight = (int)(destWidth / ratio);
            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);
            }

            destSize = new Size(destWidth, destHeight);

            int fps = 10;

            VideoCaptureType captureType = VideoCaptureType.GDI;

            httpScreenSource = new ScreenSource();
            ScreenCaptureDeviceDescription captureParams = new ScreenCaptureDeviceDescription
            {
                CaptureRegion = srcRect,
                Resolution = destSize,

                CaptureType = captureType,
                Fps = (int)fps,
                CaptureMouse = true,
                AspectRatio = true,
                UseHardware = false,
            };


            if (captureType == VideoCaptureType.GDI || captureType == VideoCaptureType.GDIPlus)
            {// масштабируем на энкодере
                captureParams.Resolution = new Size(srcRect.Width, srcRect.Height);
            }

            VideoEncoderSettings encodingParams = new VideoEncoderSettings
            {
                Resolution = destSize,
                FrameRate = (int)fps,
                EncoderName = "mjpeg",
            };

            try
            {
                httpScreenSource.Setup(captureParams);

                httpStreamer = new VideoHttpStreamer(httpScreenSource);

                httpStreamer.Setup(encodingParams, networkParams);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return Result;

        }

        public void Start()
        {
            logger.Debug("HttpScreenStreamer::Start()");

            httpScreenSource.Start();
            httpStreamer.Start();


        }

        public void Stop()
        {

            logger.Debug("HttpScreenStreamer::Stop()");

            httpScreenSource?.Stop();
            httpStreamer?.Stop();
        }

        public void Close()
        {

            logger.Debug("HttpScreenStreamer::Close()");

            httpStreamer?.Close();
            httpScreenSource?.Close();

        }

    }
}
