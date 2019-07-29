using CommonData;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    class ScreenCaptureParams
    {
        public Rectangle SrcRect = new Rectangle(0, 0, 640, 480);
        public Size DestSize = new Size(640, 480);
        public CaptureType CaptureType = CaptureType.GDI;
        public int Fps = 10;
        public bool CaptureMouse = false;
    }

    class ScreenSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenSource() { }

        public VideoBuffer Buffer { get; private set; }

        public event Action BufferUpdated;
        private void OnBufferUpdated()
        {
            BufferUpdated?.Invoke();
        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        public Task Start(ScreenCaptureParams captureParams)
        {
            logger.Debug("ScreenSource::Start()");



            return Task.Run(() =>
            {
                logger.Info("Capturing thread started...");

                var frameRate = captureParams.Fps;
                var srcRect = captureParams.SrcRect;
                var destSize = captureParams.DestSize;
                var captureType = captureParams.CaptureType;
                var captureMouse = captureParams.CaptureMouse;

                CaptureStats captureStats = new CaptureStats();

                ScreenCapture screenCapture = ScreenCapture.Create(captureType);
                screenCapture.CaptureMouse = captureMouse;

                try
                {
                    Statistic.RegisterCounter(captureStats);


                    var frameInterval = (1000.0 / frameRate);

                    //screenCapture.Init(srcRect, destSize);
                    screenCapture.Init(srcRect);

                    this.Buffer = screenCapture.VideoBuffer;

                    double lastTime = 0;
                    double monotonicTime = 0;
                    
                    Stopwatch sw = Stopwatch.StartNew();
                    while (!closing)
                    {
                        sw.Restart();

                        try
                        {
                            var res = screenCapture.UpdateBuffer();
    
                            if (closing)
                            {
                                break;
                            }

                            if (res)
                            {
                                var time = (monotonicTime + sw.ElapsedMilliseconds / 1000.0); //MediaTimer.GetRelativeTime() ;

                                Buffer.time = time; //MediaTimer.GetRelativeTime() 

                                //var diff = time - lastTime;

                                lastTime = Buffer.time;

                                OnBufferUpdated();
 
                                captureStats.Update(Buffer.time, (int)Buffer.Size);

                            }
                            else
                            {
                                logger.Warn("Drop buffer...");
                            }


                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }

                        var mSec = sw.ElapsedMilliseconds;
                        var delay = (int)(frameInterval - mSec);

                        if (delay > 0)
                        {

                            syncEvent.WaitOne(delay);
                        }

                        monotonicTime += sw.ElapsedMilliseconds / 1000.0;

                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    screenCapture?.Close();

                    Statistic.UnregisterCounter(captureStats);
                }

                logger.Info("Capturing thread stopped...");


            });

        }

        private bool closing = false;

        public void Close()
        {
            logger.Debug("ScreenSource::Close()");

            closing = true;
            syncEvent.Set();
        }


    }
}
