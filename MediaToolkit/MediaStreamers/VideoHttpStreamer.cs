using MediaToolkit.Core;
using FFmpegLib;
using NLog;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Networks;

namespace MediaToolkit
{

    public class VideoHttpStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IVideoSource screenSource = null;


        public VideoHttpStreamer(IVideoSource source)
        {
            this.screenSource = source;
           
        }

        private FFmpegVideoEncoder encoder = null;
        private Networks.HttpStreamer httpStreamer = null;

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private Task streamingTask = null;

        NetworkSettings networkParams = null;

        public void Setup(VideoEncoderSettings encPars, NetworkSettings networkParams)
        {
            logger.Debug("VideoHttpStreamer::Setup(...) "
                + encPars.Resolution.Width + "x" + encPars.Resolution.Height + " " + encPars.EncoderName);

            try
            {
                this.networkParams = networkParams;

                httpStreamer = new Networks.HttpStreamer();

                encoder = new FFmpegVideoEncoder();
                encoder.Open(encPars);
                encoder.DataEncoded += Encoder_DataEncoded;
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                if (encoder != null)
                {
                    encoder.DataEncoded -= Encoder_DataEncoded;
                    encoder.Close();
                    encoder = null;
                }

                throw;
            }


        }

        public void Start()
        {
            logger.Debug("MJpegOverHttpStreamer::Start(...)");
            streamingTask = Task.Run(() =>
            {
                running = true;
                screenSource.BufferUpdated += ScreenSource_BufferUpdated;
                try
                {
                    logger.Debug("Start main streaming loop...");

                    DoStream(networkParams);

                }
                finally
                {
                    logger.Debug("Stop main streaming loop...");

                    this.Close();

                }

            });
        }


        private void DoStream(NetworkSettings networkParams)
        {

            var streamerTask = httpStreamer.Start(networkParams);

            streamerTask.ContinueWith(t =>
            {
                if (t.IsFaulted) { }
                var ex = t.Exception;
                if (ex != null)
                {
                    logger.Error(ex);
                    running = false;
                }
            });

            Stopwatch sw = Stopwatch.StartNew();
            while (running)
            {
                sw.Restart();

                try
                {
                    if (!syncEvent.WaitOne(1000))
                    {
                        continue;
                    }

                    if (!running)
                    {
                        break;
                    }

                    var buffer = screenSource.SharedBitmap;

                    encoder.Encode(buffer);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Thread.Sleep(1000);
                }

                //rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                var mSec = sw.ElapsedMilliseconds;

            }
        }


        private void Encoder_DataEncoded(IntPtr ptr, int len, double sec)
        {
            httpStreamer.TryToPush(ptr, len, sec);

            // File.WriteAllBytes("d:\\test_3.jpg", frame);
        }

        private void ScreenSource_BufferUpdated()
        {
            syncEvent?.Set();
        }

        private bool running = false;


        public void Stop()
        {
            logger.Debug("VideoHttpStreamer::Stop()");

            running = false;

            this.screenSource.BufferUpdated -= ScreenSource_BufferUpdated;
            syncEvent.Set();
        }


        public void Close()
        {
            logger.Debug("VideoHttpStreamer::Close()");

            screenSource.BufferUpdated -= ScreenSource_BufferUpdated;
            if (encoder != null)
            {
                encoder.DataEncoded -= Encoder_DataEncoded;
                encoder.Close();
            }

            httpStreamer?.Stop();
        }




    }
}
