﻿using MediaToolkit;

using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Networks;

using MediaToolkit.Utils;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.MediaStreamers
{
    public enum StreamerState
    {
        Initialized,
        Starting,
        Streaming,
        Closing,
        Closed,
    }

    public class VideoStreamer
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaStreamers");

        public readonly IVideoSource videoSource = null;
        public VideoStreamer(IVideoSource source)
        {
            this.videoSource = source;

        }

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        private volatile StreamerState state = StreamerState.Closed;
        public StreamerState State => state;

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        public RtpSession H264Session { get; private set; }
        public IRtpSender RtpSender { get; private set; }

        public string Id { get; private set; }
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

        private StreamStats streamStats = new StreamStats();
        public StatCounter Stats => streamStats;


		//private FFmpegVideoEncoder encoder = null;
		//private RtpStreamer rtpStreamer = null;


		private VideoFrameEncoder videoEncoder = null;
		//private VideoEncoderWin7 videoEncoder = null;

		public VideoEncoderSettings EncoderSettings { get; private set; }
        public NetworkSettings NetworkSettings { get; private set; }

        public event Action StateChanged;


        public void Setup(VideoEncoderSettings encoderSettings, NetworkSettings networkSettings)
        {
            logger.Debug("ScreenStreamer::Setup()");

            this.Id = "VideoStreamer_" + Guid.NewGuid().ToString();

            this.EncoderSettings = encoderSettings;
            this.NetworkSettings = networkSettings;
            
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

                videoEncoder = new VideoFrameEncoder();
				videoEncoder.Open(videoSource.VideoBuffer, encoderSettings);
                videoEncoder.DataEncoded += VideoEncoder_DataEncoded;

                videoSource.FrameAcquired += VideoSource_FrameAcquired;

                state = StreamerState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();

                throw;
            }

        }

        private void VideoSource_FrameAcquired(IVideoFrame frame)
        {
            if (frame != null)
            {
                videoEncoder.Encode(frame);
            }
        }


        private volatile bool running = false;

        private Stopwatch sw = new Stopwatch();
        public bool Start()
        {
            logger.Debug("ScreenStreamer::Start()");
            if (running)
            {
                return false;
            }

            if(state != StreamerState.Initialized)
            {
                //...
                return false;
            }

            state = StreamerState.Starting;

            Task.Run(() =>
            {
                DoStreaming();

            });

            return true;

        }

        private void DoStreaming()
        {
            running = true;

            logger.Info("Streaming thread started...");
            if (state != StreamerState.Starting)
            {
                //...
            }

            try
            {

                streamStats.Reset(); 

               // Statistic.RegisterCounter(streamStats);

                //var hwContext = screenSource.hwContext;
                //mfEncoder.Start();

  
                state = StreamerState.Streaming;
                StateChanged?.Invoke();

                while (state == StreamerState.Streaming)
                {
                    try
                    {
                        if (!syncEvent.WaitOne(1000))
                        {
                            //continue;
                        }

                        if (state != StreamerState.Streaming)
                        {
                            break;
                        }

                        sw.Restart();

                       // videoEncoder.Encode();

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

                //Statistic.UnregisterCounter(streamStats);

                running = false;

                state = StreamerState.Closed;
                StateChanged?.Invoke();

            }

            logger.Info("Streaming thread ended...");
        }


        // FileStream file = new FileStream(@"d:\test_enc3.h264", FileMode.Create);
        //private void MfEncoder_DataReady(byte[] buf)
        private void VideoEncoder_DataEncoded(byte[] buf, double _time)
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


		//private void ScreenSource_BufferUpdated()
  //      {
  //          syncEvent?.Set();
  //      }

        public void Close()
        {
            logger.Debug("VideoStreamer::Close()");
            if (running)
            {
                state = StreamerState.Closing;

                syncEvent.Set();
            }
            else
            {
                CleanUp();
            }
        }

        private void CleanUp()
        {
            logger.Debug("VideoStreamer::CleanUp()");

            if (videoEncoder != null)
            {
                videoEncoder.DataEncoded -= VideoEncoder_DataEncoded;
                videoEncoder.Close();
                videoEncoder = null;
            }


            //videoSource.BufferUpdated -= ScreenSource_BufferUpdated;
			
            RtpSender?.Close();

            state = StreamerState.Closed;
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
