using CommonData;
using FFmpegWrapper;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

        private RtpStreamer rtpStreamer = null;
        private StreamStats streamStats = null;

        public Task Start(VideoEncodingParams encodingParams, NetworkStreamingParams networkParams)
        {
            logger.Debug("ScreenStreamer::Start()");
            
            return Task.Run(() =>
            {
                logger.Info("Streaming thread started...");


                FFmpegVideoEncoder encoder = null;
                try
                {
                    streamStats = new StreamStats();

                    Statistic.RegisterCounter(streamStats);
                    
                    RtpSession h264Session = new H264Session();

                    rtpStreamer = new RtpStreamer(h264Session);
                    rtpStreamer.Open(networkParams.Address, networkParams.Port);

                    encoder = new FFmpegVideoEncoder();
                    encoder.Open(encodingParams);
                    encoder.DataEncoded += Encoder_DataEncoded;

                    screenSource.BufferUpdated += ScreenSource_BufferUpdated;

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

                            var buffer = screenSource.Buffer;
  
                            encoder.Encode(buffer);

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
                    encoder.DataEncoded -= Encoder_DataEncoded;
                    screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

                    rtpStreamer?.Close();
                    encoder?.Close();

                    Statistic.UnregisterCounter(streamStats);
                }

                logger.Info("Streaming thread ended...");
            });

        }

        private void Encoder_DataEncoded(IntPtr ptr, int len, double time)
        {
            if (closing)
            {
                return;
            }

            if (ptr != IntPtr.Zero && len > 0)
            {
                // получили данные от энкодера 
                byte[] frame = new byte[len];
                Marshal.Copy(ptr, frame, 0, len);

                rtpStreamer.Send(frame, time);

                streamStats.Update(time, frame.Length);
                // streamer.Send(frame, rtpTimestamp);
            }

        }

        private void ScreenSource_BufferUpdated()
        {
            syncEvent.Set();
        }

        private bool closing = false;

        public void Close()
        {
 
            closing = true;
            syncEvent.Set();
        }


        class StreamStats : StatCounter
        {
            public uint buffersCount = 0;

            public long totalBytesSend = 0;
            public double sendBytesPerSec1 = 0;
            public double sendBytesPerSec2 = 0;
            public double sendBytesPerSec3 = 0;
            public double lastTimestamp = 0;

            public double totalTime = 0;
            public StreamStats() { }

            public void Update(double timestamp, int bytesSend)
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

                        totalTime += (timestamp - lastTimestamp);
                    }
                }

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
