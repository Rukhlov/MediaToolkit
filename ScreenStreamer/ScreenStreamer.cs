using CommonData;
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

        private readonly ScreenSource screenSource = null;
        public ScreenStreamer(ScreenSource source)
        {
            this.screenSource = source;
        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        public void Start(VideoEncodingParams encodingParams, NetworkStreamingParams networkParams)
        {
            logger.Debug("ScreenStreamer::Start()");

            Task.Run(() =>
            {
                logger.Info("Streaming thread started...");
                screenSource.BufferUpdated += () => syncEvent.Set();

                RtpStreamer streamer = null;
                FFmpegVideoEncoder encoder = null;
                try
                {
                    streamer = new RtpStreamer();
                    streamer.Open(networkParams.MulitcastAddres, networkParams.Port);

                    encoder = new FFmpegVideoEncoder();
                    encoder.Open(encodingParams);

                    uint rtpTimestamp = 0;

                    encoder.DataEncoded += (ptr, len) =>
                    {// получили данные от энкодера 

                        byte[] frame = new byte[len];
                        Marshal.Copy(ptr, frame, 0, len);

                        streamer.Send(frame, rtpTimestamp);

                    };


 
                    Stopwatch sw = Stopwatch.StartNew();
                    while (!closing)
                    {
                        sw.Restart();
  
                        try
                        {
                            syncEvent.WaitOne();

                            if (closing)
                            {
                                break;
                            }

                            var buffer = screenSource.Buffer;
                            encoder.Encode(buffer, 0);
                           
   
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }

                        rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        var mSec = sw.ElapsedMilliseconds;
  
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                }
                finally
                {
                    streamer?.Close();
                    encoder?.Close();
                }

                logger.Info("Streaming thread ended...");
            });

        }

        private bool closing = false;

        public void Close()
        {
            closing = false;
            syncEvent.Set();
        }

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

        public void Start(VideoBuffer videoBuffer, int frameRate = 30)
        {
            logger.Debug("ScreenSource::Start()");

            this.Buffer = videoBuffer;

            int Width = videoBuffer.bitmap.Width;
            int Height = videoBuffer.bitmap.Height;


            double sec = 0;
            Task.Run(() =>
            {
                logger.Info("Capturing thread started...");

                try
                {
                    var frameInterval = (1000.0 / frameRate);
                    Stopwatch sw = Stopwatch.StartNew();

                    var hWnd = NativeMethods.GetDesktopWindow();
                    var rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                    uint rtpTimestamp = 0;
                    while (!closing)
                    {
                        sw.Restart();
    
                        try
                        {
                            var res = GDICapture.GetScreen(rect, ref videoBuffer);

                            if (closing)
                            {
                                break;
                            }

                            if (res)
                            {
                                videoBuffer.time = sec;
                            }
                            OnBufferUpdated();

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
                            Thread.Sleep(delay);
                        }

                        rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        sec += sw.ElapsedMilliseconds / 1000.0;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex); ;
                }

                logger.Info("Capturing thread stopped...");

            });

        }

        private bool closing = false;

        public void Close()
        {
            closing = false;
        }


    }


}
