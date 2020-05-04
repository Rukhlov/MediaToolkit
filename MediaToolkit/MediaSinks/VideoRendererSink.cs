﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Networks;
using MediaToolkit.SharedTypes;
using MediaToolkit.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;

namespace MediaToolkit
{
    public class VideoRendererSink
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");


        private MfH264Dxva2Decoder decoder = null;
        private PresentationClock presentationClock = null;
        public MfVideoRenderer videoRenderer = null;

        private static object syncRoot = new object();
        private IntPtr hWnd = IntPtr.Zero;

        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode ErrorCode => errorCode;

        public void Setup(VideoEncoderSettings inputPars, IntPtr hwnd)
        {

            logger.Debug("VideoRendererSink::Setup()");

            this.hWnd = hwnd;

            var inputArgs = new MfVideoArgs
            {
                Width = inputPars.Resolution.Width,
                Height = inputPars.Resolution.Height,
                FrameRate = inputPars.FrameRate,
            };

            MediaFactory.CreatePresentationClock(out presentationClock);

            PresentationTimeSource timeSource = null;
            try
            {
                MediaFactory.CreateSystemTimeSource(out timeSource);
                presentationClock.TimeSource = timeSource;
            }
            finally
            {
                timeSource?.Dispose();
            }


            videoRenderer = new MfVideoRenderer();


            //TODO: нужно настраивать когда декодер пришлет свой формат
            videoRenderer.Setup(new VideoRendererArgs
            {
                hWnd = hWnd,
                FourCC = new SharpDX.Multimedia.FourCC("NV12"),
                //FourCC = 0x59565955, //"UYVY",
                //Resolution = inputPars.Resolution, // 

                
                Resolution = new System.Drawing.Size(1920, 1088),
                FrameRate = new Tuple<int, int>(inputPars.FrameRate, 1),

            });

            videoRenderer.RendererStarted += VideoRenderer_RendererStarted;
            videoRenderer.RendererStopped += VideoRenderer_RendererStopped;

            videoRenderer.SetPresentationClock(presentationClock);
            videoRenderer.Resize(new System.Drawing.Rectangle(0, 0, 100, 100));

            var d3dManager = videoRenderer.D3DManager;

            decoder = new MfH264Dxva2Decoder(d3dManager);

            decoder.Setup(inputArgs);
        }


        public void Start()
        {
            logger.Debug("VideoRendererSink::Start()");

            decoder.Start();

            presentationClock.Start(0);

        }



        public void ProcessData(byte[] data, double time)
        {
            try
            {
                var encodedSample = MediaFactory.CreateSample();
                try
                {
                    using (MediaBuffer mb = MediaFactory.CreateMemoryBuffer(data.Length))
                    {
                        var dest = mb.Lock(out int cbMaxLength, out int cbCurrentLength);
                        //logger.Debug(sampleCount + " Marshal.Copy(...) " + nal.Length);
                        Marshal.Copy(data, 0, dest, data.Length);

                        mb.CurrentLength = data.Length;
                        mb.Unlock();

                        encodedSample.AddBuffer(mb);

                        if (!double.IsNaN(time) && time > 0)
                        {
                            // может глючить рендерер если метки будут кривые!!
                            // TODO: сделать валидацию вр.меток до декодера и после...
                            var sampleTime = MfTool.SecToMfTicks(time); 
                            encodedSample.SampleTime = sampleTime; 
                            encodedSample.SampleDuration = 0;// MfTool.SecToMfTicks(0.033);
                        }
                        else
                        {
                            encodedSample.SampleTime = 0;
                            encodedSample.SampleDuration = 0;
                        }
                    }

                    //logger.Debug("ProcessData " + time);

                    var res = decoder.ProcessSample(encodedSample, OnSampleDecoded);
                    if (!res)
                    {
                        logger.Debug("decoder.ProcessSample() " + res);
                        //...
                    }


                }
                finally
                {
                    encodedSample?.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        private long prevSampleTime = 0;
        private long unwrappedSampleTime = 0;
       
        private void OnSampleDecoded(Sample sample)
        {
            if (sample == null)
            {
                return;
            }

            try
            {
                var sampleTime = sample.SampleTime;
                if(prevSampleTime == 0)
                {
                    prevSampleTime = sampleTime;
                }
                var timeDiff = (sampleTime - prevSampleTime);

                if (timeDiff < 0)
                {
                    logger.Warn("Not monotonic time: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff));
                }
                else if (timeDiff > MfTool.SecToMfTicks(1))
                {
                    logger.Warn("Large time gap: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff));
                }

                unwrappedSampleTime += timeDiff;

                sample.SampleTime = unwrappedSampleTime;

                videoRenderer.ProcessDxva2Sample(sample);

                prevSampleTime = sampleTime;

                //logger.Debug(sampleTime +  " " + prevSampleTime + " " + (sampleTime - prevSampleTime));


            }
            finally
            {

                sample.Dispose();
                sample = null;

            }
        }

        public void Repaint()
        {
            if (videoRenderer != null)
            {
                videoRenderer.Repaint();

            }
        }

        public void Resize(System.Drawing.Rectangle rect)
        {
            if (videoRenderer != null)
            {
                videoRenderer.Resize(rect);
               
            }
        }

        private void VideoRenderer_RendererStarted()
        {
            logger.Debug("VideoRenderer_RendererStarted()");
        }

        private void VideoRenderer_RendererStopped()
        {
            logger.Debug("VideoRenderer_RendererStopped()");

            if (videoRenderer != null)
            {
                videoRenderer.Close();
                videoRenderer = null;
            }

        }

        public void Stop()
        {
            logger.Debug("VideoRendererSink::Stop()");

            decoder.Stop();

            presentationClock.Stop();

        }

        public void Close()
        {

            logger.Debug("VideoRendererSink::Close()");

            if (presentationClock != null)
            {       
                presentationClock.Dispose();
                presentationClock = null;
            }

            if (videoRenderer != null)
            {
                videoRenderer.Close();
                videoRenderer = null;
            }

            if (decoder != null)
            {
                decoder.Close();
                decoder = null;
            }
        }

    }

}