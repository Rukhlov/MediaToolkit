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

                        // File.WriteAllBytes("d:\\test_123.jpg", frame);


                        double ralativeTime = MediaTimer.GetRelativeTime();
                        uint rtpTime = (uint)(ralativeTime * 90000);

                        streamer.Send(frame, rtpTime);

                        // streamer.Send(frame, rtpTimestamp);


                    };


                    double sec = 0;
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

}
