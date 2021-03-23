using MediaToolkit.Core;
using FFmpegLib;

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
using MediaToolkit.Logging;

namespace MediaToolkit.MediaStreamers
{

    public class MJpegStreamer
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaStreamers");

        private readonly IVideoSource screenSource = null;


        public MJpegStreamer(IVideoSource source)
        {
            this.screenSource = source;
           
        }

        private volatile MediaState state = MediaState.Closed;
        public MediaState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        public event Action<object> StreamerStopped;
        public event Action StreamerStarted;

        private FFmpegVideoEncoder jpegEncoder = null;
        private MJpegHttpSender mjpegSender = null;

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private Task streamTask = null;

        NetworkSettings networkParams = null;

        public void Setup(VideoEncoderSettings encPars, NetworkSettings networkParams)
        {
            logger.Debug("VideoHttpStreamer::Setup(...) "
                + encPars.Resolution.Width + "x" + encPars.Resolution.Height + " " + encPars.EncoderId);

            if (State != MediaState.Closed)
            {
                throw new InvalidOperationException("Invalid state " + State);
            }

            try
            {
                this.networkParams = networkParams;

                mjpegSender = new Networks.MJpegHttpSender();

                jpegEncoder = new FFmpegVideoEncoder();
                jpegEncoder.Open(encPars);
                jpegEncoder.DataEncoded += Encoder_DataEncoded;

                state = MediaState.Initialized;
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                errorCode = 100503;
                if (jpegEncoder != null)
                {
                    jpegEncoder.DataEncoded -= Encoder_DataEncoded;
                    jpegEncoder.Close();
                    jpegEncoder = null;
                }

                state = MediaState.Closed;
                throw;
            }


        }

        public void Start()
        {
            logger.Debug("MJpegOverHttpStreamer::Start(...)");

            if (running)
            {
                throw new InvalidOperationException();
            }

            streamTask = Task.Run(() =>
            {
                if (!(State == MediaState.Initialized || State == MediaState.Stopped))
                {
                    throw new InvalidOperationException("Invalid state " + State);
                }

                screenSource.FrameAcquired+= ScreenSource_FrameAcquired; 
                try
                {
                    logger.Debug("Start main streaming loop...");
                    StreamerStarted?.Invoke();

                    state = MediaState.Started;

                    DoStream(networkParams);

                }
                catch(Exception ex)
                {
                    logger.Error(ex);
                    errorCode = 100502; 
                }
                finally
                {
                    logger.Debug("Stop main streaming loop...");

                    state = MediaState.Stopped;
                    StreamerStopped?.Invoke(null);

                }

            });
        }

        private void ScreenSource_FrameAcquired(IVideoFrame frame)
        {
            if (frame != null)
            {
                jpegEncoder._Encode(frame);
            }
            
        }

        private void DoStream(NetworkSettings networkParams)
        {

            var streamerTask = mjpegSender.Start(networkParams);

            streamerTask.ContinueWith(t =>
            {
                if (t.IsFaulted) { }
                var ex = t.Exception;
                if (ex != null)
                {
                    errorCode = 100501;
                    logger.Error(ex);
                    running = false;
                }
            });

            Stopwatch sw = Stopwatch.StartNew();
            while (State == MediaState.Started)
            {
                sw.Restart();

                try
                {
                    if (!syncEvent.WaitOne(1000))
                    {
                       // continue;
                    }

                    if (State != MediaState.Started)
                    {
                        break;
                    }
                    //var buffer = screenSource.VideoBuffer;
                    //var frame = buffer.GetFrame();
                    //encoder._Encode(frame);

                    //var buffer = screenSource.SharedBitmap;
                    //encoder.Encode(buffer);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    Thread.Sleep(1000);
                }

                //rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 90.0);

                var mSec = sw.ElapsedMilliseconds;

            }

            streamerTask.Wait();

        }


        private void Encoder_DataEncoded(IntPtr ptr, int len, double sec)
        {
            mjpegSender.TryToPush(ptr, len, sec);

            // File.WriteAllBytes("d:\\test_3.jpg", frame);
        }

        private volatile bool running = false;


        public void Stop()
        {
            logger.Debug("VideoHttpStreamer::Stop()");

            screenSource.FrameAcquired -= ScreenSource_FrameAcquired;

            //screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

            mjpegSender?.Stop();

            state = MediaState.Stopping;
            syncEvent.Set();
        }


        public void Close(bool force = false)
        {
            logger.Debug("VideoHttpStreamer::Close()");

            Stop();

            if (!force)
            {
                if (streamTask != null)
                {
                    if (streamTask.Status == TaskStatus.Running)
                    {
                        bool waitResult = false;
                        do
                        {
                            waitResult = streamTask.Wait(1000);
                            if (!waitResult)
                            {
                                logger.Warn("VideoHttpStreamer::Close() " + waitResult);
                            }
                        } while (!waitResult);

                    }
                }
            }

            screenSource.FrameAcquired -= ScreenSource_FrameAcquired;

            //screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

            if (jpegEncoder != null)
            {
                jpegEncoder.DataEncoded -= Encoder_DataEncoded;
                jpegEncoder.Close();
            }

            state = MediaState.Closed;

        }




    }
}
