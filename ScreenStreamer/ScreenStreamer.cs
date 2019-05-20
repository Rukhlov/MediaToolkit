using FFmpegWrapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    class ScreenStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Run()
        {
            logger.Debug("Start()");

            int Width = 1280;
            int Height = 720;
            int Fps = 30;


            double sec = 0;

            Task.Run(() =>
            {
                RtpStreamer streamer = null;
                FFmpegVideoEncoder encoder = null;
                try
                {

                    streamer = new RtpStreamer();
                    streamer.Open("239.0.0.1", 1234);

                    encoder = new FFmpegVideoEncoder();
                    encoder.Open(Width, Height, Fps);

                    uint rtpTimestamp = 0;
                    encoder.DataEncoded += (ptr, len) =>
                    {
                        byte[] frame = new byte[len];
                        Marshal.Copy(ptr, frame, 0, len);

                        streamer.Send(frame, rtpTimestamp);

                    };

                    var frameInterval = (1000.0 / Fps);
                    Stopwatch sw = Stopwatch.StartNew();

                    var hWnd = NativeMethods.GetDesktopWindow();
                    var rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                    logger.Info("Streaming started...");
                    while (true)
                    {
                        sw.Restart();
                        Bitmap screen = null;
                        try
                        {
                            // screen = GDICapture.GetScreen(rect);
                            //screen = Direct3DCapture.CaptureRegionDirect3D(hWnd, rect);
                            screen = GDIPlusCapture.GetScreen(rect);
                            encoder.Encode(screen, 0);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }
                        finally
                        {
                            screen?.Dispose();
                        }


                        var mSec = sw.ElapsedMilliseconds;
                        var delay = (int)(frameInterval - mSec);

                        if (delay > 0)
                        {
                            Thread.Sleep(delay);
                        }

                        rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        sec += sw.ElapsedMilliseconds / 1000.0;
                    }
                }
                catch (Exception ex)
                {

                    logger.Fatal(ex);

                    Console.WriteLine("Press any key to exit...");

                }
                finally
                {
                    streamer?.Close();
                    //encoder?.Close();
                }

            });

        }
    }

}
