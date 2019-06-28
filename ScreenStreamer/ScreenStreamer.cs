using CommonData;
using FFmpegWrapper;
using NLog;
using ScreenStreamer.Utils;
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

    class VideoMulticastStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ScreenSource screenSource = null;
        public VideoMulticastStreamer(ScreenSource source)
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
                    RtpSession h264Session = new H264Session();

                    streamer = new RtpStreamer(h264Session);
                    streamer.Open(networkParams.MulitcastAddres, networkParams.Port);

                    encoder = new FFmpegVideoEncoder();
                    encoder.Open(encodingParams);

                    uint rtpTimestamp = 0;

                    encoder.DataEncoded += (ptr, len) =>
                    {// получили данные от энкодера 

                        byte[] frame = new byte[len];
                        Marshal.Copy(ptr, frame, 0, len);

                        double ralativeTime = MediaTimer.GetRelativeTime();
                        uint rtpTime =(uint)(ralativeTime * 90000);

                        streamer.Send(frame, rtpTime);

                        // streamer.Send(frame, rtpTimestamp);

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

    public class Statistic
    {

        private static CaptureStatistic captStats = new CaptureStatistic();
        public static CaptureStatistic CaptureStats
        {
            get
            {
                if (captStats == null)
                {
                    captStats = new CaptureStatistic();
                }
                return captStats;
            }        
        }

        public class CaptureStatistic
        {

            public long totalBytes = 0;
            public uint totalFrameCount = 0;

            public uint currentFrame = 0;
            public long currentBytes = 0;

            public double avgFrameInterval = 0;
            public double avgBytesPerSec = 0;

            public double lastTimestamp = 0;
            public long bufferSize = 0;

            public void Update(double timestamp)
            {

                if (lastTimestamp > 0)
                {
                    var time = timestamp - lastTimestamp;

                    avgFrameInterval = (time * 0.05 + avgFrameInterval * (1 - 0.05));
                    avgBytesPerSec = bufferSize / avgFrameInterval;

                }

                totalBytes += bufferSize;

                lastTimestamp = timestamp;
                totalFrameCount++;
            }


        }

        public class RtpStatistic
        {

            public uint packetsCount = 0;

            public long bytesSend = 0;
            public double sendBytesPerSec = 0;

            public double lastTimestamp = 0;

            public RtpStatistic()
            {
                System.Timers.Timer timer = new System.Timers.Timer();
                timer.Interval = 1000;

                double bytes =0;
                timer.Elapsed += (o, a) =>
                {

                    sendBytesPerSec = _sendBytesPerSec;
                    _sendBytesPerSec = 0;
                };

                timer.Start();
            }


            double _sendBytesPerSec = 0;
            public void Update(double timestamp, int packetSize)
            {

                if (lastTimestamp > 0)
                {
                    //var time = timestamp - lastTimestamp;
                    //var bytesPerSec = packetSize / time;

                    //sendBytesPerSec = (bytesPerSec * 0.05 + sendBytesPerSec * (1 - 0.05));

                }

                _sendBytesPerSec += packetSize;
                bytesSend += packetSize;

                lastTimestamp = timestamp;
                packetsCount++;
            }


        }

        private static RtpStatistic rtpStats = new RtpStatistic();
        public static RtpStatistic RtpStats
        {
            get
            {
                if (rtpStats == null)
                {
                    rtpStats = new RtpStatistic();
                }
                return rtpStats;
            }
        }

        private static PerfCounter perfCounter = new PerfCounter();
        public static PerfCounter PerfCounter
        {
            get
            {
                if (perfCounter == null)
                {
                    perfCounter = new PerfCounter();
                }
                return perfCounter;
            }
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

        public void Start(Rectangle srcRect, Size destSize, int frameRate = 30)
        {
            logger.Debug("ScreenSource::Start()");

            int frameCount = 0;

            //System.Timers.Timer timer = new System.Timers.Timer();
            //timer.Interval = 1000;
            //timer.Elapsed += (o, a) => 
            //{

            //    // var fps = frameCount / 1000.0;
            //    var avgInterval = Stats.avgFrameInterval;
            //    var fps = 1 / avgInterval;
            //    var totalFramesCount = Stats.totalFrameCount;
            //    var kbytesPerSec = Stats.avgBytesPerSec / 1024;
            //    var totalkBytes = Stats.totalBytes / 1024;
            //    logger.Debug("Stats: " + fps.ToString("0.0") + " " + kbytesPerSec.ToString("0.00") + " " + totalkBytes);

            //    frameCount = 0;
            //};

            //timer.Start();

            double sec = 0;
            Task.Run(() =>
            {
                logger.Info("Capturing thread started...");

                try
                {
  

                    var frameInterval = (1000.0 / frameRate);
                    Stopwatch sw = Stopwatch.StartNew();

                    var hWnd = User32.GetDesktopWindow();

                    //ScreenCapture screenCapture = new Direct3DCapture(hWnd);
                    //screenCapture.Init(srcRect, destSize);

                    //ScreenCapture screenCapture = new GDIPlusCapture();
                    //screenCapture.Init(srcRect);

                    ScreenCapture screenCapture = new GDICapture();
                    screenCapture.Init(srcRect);

                    this.Buffer = screenCapture.VideoBuffer;

                    Statistic.CaptureStats.bufferSize = this.Buffer.Size;

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
                                //videoBuffer.time = sec;
                                //OnBufferUpdated();

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
                            
                            Thread.Sleep(delay);
                        }

                        rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                        sec += sw.ElapsedMilliseconds / 1000.0;

                        var timestamp = MediaTimer.GetRelativeTime();
                        Statistic.CaptureStats.Update(timestamp);

                        //Statistic.PerfCounter.UpdateSignalStats();
                        //Statistic.PerfCounter.UpdatePresentStats();
                       
                    }

                    screenCapture.Close();

                    //direct3DCapture.Dispose();
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
