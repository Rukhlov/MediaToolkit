using MediaToolkit.Common;
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
using MediaToolkit.Core.Networks;

namespace MediaToolkit
{

    public class HttpScreenStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IVideoSource screenSource = null;

        public HttpScreenStreamer(IVideoSource source)
        {
            this.screenSource = source;
           
        }


        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private HttpStreamer httpStreamer = null;

        public Task Start(VideoEncodingParams encPars, NetworkStreamingParams networkParams)
        {
            logger.Debug("MJpegOverHttpStreamer::Start(...) " 
                + encPars.Width + "x" + encPars.Height + " "+ encPars.EncoderName );

            return Task.Run(() =>
            {
                running = true;


                FFmpegVideoEncoder encoder = new FFmpegVideoEncoder();
               
                httpStreamer = new HttpStreamer();
                try
                {
                    logger.Debug("Start main streaming loop...");

                    encoder.Open(encPars);
                    encoder.DataEncoded += Encoder_DataEncoded;

                    screenSource.BufferUpdated += ScreenSource_BufferUpdated;

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
                finally
                {
                    logger.Debug("Stop main streaming loop...");

                    screenSource.BufferUpdated -= ScreenSource_BufferUpdated;
                    if (encoder != null)
                    {
                        encoder.DataEncoded -= Encoder_DataEncoded;
                        encoder.Close();
                    }

                    httpStreamer?.Stop();

                }

            });
        }

        private void Encoder_DataEncoded(IntPtr ptr, int len, double sec)
        {
            httpStreamer.TryToPush(ptr, len, sec);

            // File.WriteAllBytes("d:\\test_3.jpg", frame);
        }

        private void ScreenSource_BufferUpdated()
        {
            syncEvent.Set();
        }

        private bool running = false;

        public void Close()
        {
            logger.Debug("MJpegOverHttpStreamer::Close()");

            running = false;

            this.screenSource.BufferUpdated -= ScreenSource_BufferUpdated;
            syncEvent.Set();
        }


    }
}
