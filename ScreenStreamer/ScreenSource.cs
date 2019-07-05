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
        
                ScreenCapture screenCapture = ScreenCapture.Create(CaptureType.GDI);

                try
                {
                    Statistic.RegisterCounter(captureStats);

                    double sec = 0;
                    int frameCount = 0;
                    var frameInterval = (1000.0 / frameRate);

                    screenCapture.Init(srcRect);

                    this.Buffer = screenCapture.VideoBuffer;

                    int bufferSize = (int)this.Buffer.Size;

                    uint rtpTimestamp = 0;
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
