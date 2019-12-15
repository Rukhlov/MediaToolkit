using MediaToolkit;

using MediaToolkit.Core;
using MediaToolkit.RTP;
using MediaToolkit.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public class VideoStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public readonly IVideoSource videoSource = null;
        public VideoStreamer(IVideoSource source)
        {
            this.videoSource = source;

        }


        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        public RtpSession H264Session { get; private set; }
        public IRtpSender RtpSender { get; private set; }

        public int ClientsCount
        {
            get
            {
                int count = 0;
                if (RtpSender != null)
                {
                    count = RtpSender.ClientsCount;
                }

                return count;
            }
        }

        //private RtpStreamer rtpStreamer = null;
        private StreamStats streamStats = null;
        //private FFmpegVideoEncoder encoder = null;

        private VideoEncoder videoEncoder = null;


        public void Setup(VideoEncoderSettings encodingSettings, NetworkSettings networkSettings)
        {
            logger.Debug("ScreenStreamer::Setup()");

            try
            {
                H264Session = new H264Session();
                if(networkSettings.TransportMode == TransportMode.Tcp)
                {
                    RtpSender = new RtpTcpSender(H264Session);
                }
                else if(networkSettings.TransportMode == TransportMode.Udp)
                {
                    RtpSender = new RtpUdpSender(H264Session);
                }
                else
                {
                    throw new FormatException("NotSupportedFormat " +  networkSettings.TransportMode);
                }

                //rtpStreamer = new RtpStreamer(h264Session);
                RtpSender.Setup(networkSettings);

                networkSettings.SSRC = H264Session.SSRC;

                RtpSender.Start();

                //var hwContext = screenSource.hwContext;
                //var hwDevice = hwContext.device;

                var srcSize = videoSource.SrcSize; //new Size(screenSource.Buffer.bitmap.Width, screenSource.Buffer.bitmap.Height);

                var destSize = encodingSettings.Resolution;//new Size(encodingParams.Width, encodingParams.Height);


                //encoder = new FFmpegVideoEncoder();
                //encoder.Open(encodingParams);
                //encoder.DataEncoded += Encoder_DataEncoded;

                videoEncoder = new VideoEncoder(videoSource);
                videoEncoder.Open(encodingSettings);
                videoEncoder.DataEncoded += VideoEncoder_DataEncoded;

                videoSource.BufferUpdated += ScreenSource_BufferUpdated;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();

                throw;
            }

        }



        // private Texture2D SharedTexture = null;

        private Stopwatch sw = new Stopwatch();
        public Task Start()
        {
            logger.Debug("ScreenStreamer::Start()");

            return Task.Run(() =>
            {
                logger.Info("Streaming thread started...");


                try
                {
                    streamStats = new StreamStats();

                    Statistic.RegisterCounter(streamStats);

                    //var hwContext = screenSource.hwContext;
                    //mfEncoder.Start();


                    while (!closing)
                    {
                        try
                        {
                            if (!syncEvent.WaitOne(1000))
                            {
                                continue;
                            }

                            if (closing)
                            {
                                break;
                            }

                            sw.Restart();
                            
                            videoEncoder.Encode();

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                }
                finally
                {
                    CleanUp();

                    Statistic.UnregisterCounter(streamStats);
                }

                logger.Info("Streaming thread ended...");
            });

        }


        // FileStream file = new FileStream(@"d:\test_enc3.h264", FileMode.Create);
        //private void MfEncoder_DataReady(byte[] buf)
        private void VideoEncoder_DataEncoded(byte[] buf)
        {
            //throw new NotImplementedException();
            var time = MediaTimer.GetRelativeTime();

            // var memo = new MemoryStream(buf);
            // memo.CopyTo(file);

            RtpSender.Push(buf, time);

            // rtpStreamer.Send(buf, time);
            var processingTime = sw.ElapsedMilliseconds;
            //logger.Debug(processingTime);


            //var ts = hwContext.sw.ElapsedMilliseconds;
            //Console.WriteLine("ElapsedMilliseconds " + ts);
            streamStats.Update(time, buf.Length, processingTime);
        }


        private void ScreenSource_BufferUpdated()
        {
            syncEvent.Set();
        }

        private bool closing = false;

        public void Close()
        {
            logger.Debug("VideoMulticastStreamer::Close()");
            closing = true;
            syncEvent.Set();
        }

        private void CleanUp()
        {
            logger.Debug("VideoMulticastStreamer::CleanUp()");

            if (videoEncoder != null)
            {
                videoEncoder.Close();
                videoEncoder = null;
            }


            videoSource.BufferUpdated -= ScreenSource_BufferUpdated;

            RtpSender?.Close();
        }

        class StreamStats : StatCounter
        {
            public uint buffersCount = 0;

            public long totalBytesSend = 0;
            public double sendBytesPerSec1 = 0;
            public double sendBytesPerSec2 = 0;
            public double sendBytesPerSec3 = 0;
            public double avgBitrate = 0;

            public double lastTimestamp = 0;

            public double totalTime = 0;
            public long avgEncodingTime = 0;

            public StreamStats() { }

            public void Update(double timestamp, int bytesSend, long encTime)
            {
                if (lastTimestamp > 0)
                {
                    var time = timestamp - lastTimestamp;
                    if (time > 0)
                    {
                        var bytesPerSec = bytesSend / time;

                        sendBytesPerSec1 = (bytesPerSec * 0.05 + sendBytesPerSec1 * (1 - 0.05));

                        sendBytesPerSec2 = (sendBytesPerSec1 * 0.05 + sendBytesPerSec2 * (1 - 0.05));

                        sendBytesPerSec3 = (sendBytesPerSec2 * 0.05 + sendBytesPerSec3 * (1 - 0.05));

                        avgBitrate = sendBytesPerSec3  * 8 / 1000.0;

                        totalTime += (timestamp - lastTimestamp);
                    }
                }

                avgEncodingTime = (long)(encTime * 0.1 + avgEncodingTime * (1 - 0.1));

                totalBytesSend += bytesSend;

                lastTimestamp = timestamp;
                buffersCount++;
            }

            public override string GetReport()
            {
                StringBuilder sb = new StringBuilder();

                //var mbytesPerSec = sendBytesPerSec / (1024.0 * 1024);
                //var mbytes = totalBytesSend / (1024.0 * 1024);


                TimeSpan time = TimeSpan.FromSeconds(totalTime);
                sb.AppendLine(time.ToString(@"hh\:mm\:ss\.fff"));

                sb.AppendLine(buffersCount + " Buffers");

                //sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec1) + "/s");
                //sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec2) + "/s");
                sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec3) + "/s");
                sb.AppendLine(StringHelper.SizeSuffix(totalBytesSend));

                sb.AppendLine(avgBitrate.ToString("0.0") + " kbps");

                sb.AppendLine(avgEncodingTime + " ms");

                //sb.AppendLine(mbytes.ToString("0.0") + " MBytes");
                //sb.AppendLine(mbytesPerSec.ToString("0.000") + " MByte/s");

                return sb.ToString();
            }

            public override void Reset()
            {
                buffersCount = 0;

                totalBytesSend = 0;
                sendBytesPerSec1 = 0;

                lastTimestamp = 0;
            }
        }
    }
}
