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
        public Task Start(Rectangle srcRect, Size destSize, int frameRate = 30)
        {
            logger.Debug("ScreenSource::Start()");

            return Task.Run(() =>
            {
                logger.Info("Capturing thread started...");

                CaptureStats captureStats = new CaptureStats();
                Statistic.Stats.Add(captureStats);

                //var hWnd = User32.GetDesktopWindow();

                //ScreenCapture screenCapture = new Direct3DCapture(hWnd);
                //screenCapture.Init(srcRect, destSize);

                //ScreenCapture screenCapture = new GDIPlusCapture();
                //screenCapture.Init(srcRect);

                ScreenCapture screenCapture = new GDICapture();

                try
                {
                    double sec = 0;
                    int frameCount = 0;
                    var frameInterval = (1000.0 / frameRate);
                    Stopwatch sw = Stopwatch.StartNew();

                    screenCapture.Init(srcRect);

                    this.Buffer = screenCapture.VideoBuffer;

                    int bufferSize = (int)this.Buffer.Size;

                    uint rtpTimestamp = 0;
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
                                Buffer.time = sec;

                                OnBufferUpdated();
                                frameCount++;
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

                        rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        sec += sw.ElapsedMilliseconds / 1000.0;

                        captureStats.Update(sec, bufferSize);

                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    screenCapture?.Close();

                    //direct3DCapture.Dispose();
                }

                logger.Info("Capturing thread stopped...");

                Statistic.Stats.Remove(captureStats);
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
