using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.Networks;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit
{

    public class VideoReceiverEx
    {

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");

        private H264Session h264Session = null;
        public IRtpReceiver rtpReceiver = null;


        public void Setup(NetworkSettings networkPars)
        {
            logger.Debug("ScreenReceiver::Setup(...)");



            h264Session = new H264Session();

            if (networkPars.TransportMode == TransportMode.Tcp)
            {
                rtpReceiver = new RtpTcpReceiver(h264Session);
            }
            else if (networkPars.TransportMode == TransportMode.Udp)
            {
                rtpReceiver = new RtpUdpReceiver(h264Session);
            }
            else
            {
                throw new Exception("networkPars.TransportMode");
            }

            h264Session.SSRC = networkPars.SSRC;

            rtpReceiver.Open(networkPars);
            rtpReceiver.RtpPacketReceived += RtpReceiver_RtpPacketReceived;

        }


        public void Play()
        {
            logger.Debug("ScreenReceiver::Play()");

            //Statistic.RegisterCounter(receiverStats);

            receiverStats.Reset();


            rtpReceiver.Start();

        }

        public event Action<byte[], double> DataReceived;


        private Stopwatch sw = new Stopwatch();
        private void RtpReceiver_RtpPacketReceived(RtpPacket packet)
        {
            if (h264Session.SSRC != packet.SSRC)
            {
                logger.Warn("Invalid SSRC " + h264Session.SSRC + " != " + packet.SSRC);
                return;
            }

            var currentTime = MediaTimer.GetRelativeTime();

            byte[] rtpPayload = packet.Payload.ToArray();

            //totalPayloadReceivedSize += rtpPayload.Length;
            // logger.Debug("totalPayloadReceivedSize " + totalPayloadReceivedSize);


            var frame = h264Session.Depacketize(packet);
            if (frame != null)
            {
                //logger.Info("h264Session.Depacketize(...) " + frame.Time);
                sw.Restart();

                var frameData = frame.Data;
                var frameTime = frame.Time;
                if (frameData != null)
                {
                    DataReceived?.Invoke(frameData, frameTime);

                    //Decode(frameData, frameTime);

                    receiverStats.Update(currentTime, frameData.Length, sw.ElapsedMilliseconds);
                }
            }

        }




        public void Stop()
        {
            logger.Debug("ScreenReceiver::Stop()");

            if (rtpReceiver != null)
            {
                rtpReceiver.RtpPacketReceived -= RtpReceiver_RtpPacketReceived;
                rtpReceiver.Stop();
            }




            //Statistic.UnregisterCounter(receiverStats);
        }


        private ReceiverStats receiverStats = new ReceiverStats();
        public StatCounter Stats => receiverStats;

        class ReceiverStats : StatCounter
        {
            public uint buffersCount = 0;

            public long totalBytesReceived = 0;
            public double receiveBytesPerSec1 = 0;
            public double receiveBytesPerSec2 = 0;
            public double receiveBytesPerSec3 = 0;
            public double lastTimestamp = 0;

            public long avgEncodingTime = 0;

            public double totalTime = 0;

            public void Update(double timestamp, int bytesReceived, long encTime)
            {
                if (lastTimestamp > 0)
                {
                    var time = timestamp - lastTimestamp;
                    if (time > 0)
                    {
                        var bytesPerSec = bytesReceived / time;

                        receiveBytesPerSec1 = (bytesPerSec * 0.05 + receiveBytesPerSec1 * (1 - 0.05));

                        receiveBytesPerSec2 = (receiveBytesPerSec1 * 0.05 + receiveBytesPerSec2 * (1 - 0.05));

                        receiveBytesPerSec3 = (receiveBytesPerSec2 * 0.05 + receiveBytesPerSec3 * (1 - 0.05));


                        avgEncodingTime = (long)(encTime * 0.1 + avgEncodingTime * (1 - 0.1));

                        totalTime += (timestamp - lastTimestamp);

                        //if (totalTime > 0)
                        //{
                        //    receiveBytesPerSec3 = totalBytesReceived / totalTime;
                        //}

                    }
                }

                totalBytesReceived += bytesReceived;

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
                sb.AppendLine(StringHelper.SizeSuffix((long)receiveBytesPerSec3) + "/s");
                sb.AppendLine(StringHelper.SizeSuffix(totalBytesReceived));


                sb.AppendLine(avgEncodingTime + " ms");

                //sb.AppendLine(mbytes.ToString("0.0") + " MBytes");
                //sb.AppendLine(mbytesPerSec.ToString("0.000") + " MByte/s");

                return sb.ToString();
            }

            public override void Reset()
            {
                buffersCount = 0;

                totalBytesReceived = 0;
                receiveBytesPerSec1 = 0;

                lastTimestamp = 0;
            }
        }
    }
}
