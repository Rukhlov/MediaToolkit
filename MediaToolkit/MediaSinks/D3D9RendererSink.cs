using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
    public class D3D9RendererSink
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");


        private MfH264Dxva2Decoder decoder = null;
        private PresentationClock presentationClock = null;
        public MfVideoRenderer videoRenderer = null;

        private static object syncRoot = new object();
        private IntPtr hWnd = IntPtr.Zero;

        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode ErrorCode => errorCode;

        //public bool UseHardware = true;
        //public bool LowLatency = true;

        public VideoEncoderSettings EncoderSettings { get; private set; }

        public void Setup(VideoEncoderSettings settings, IntPtr hwnd)
        {

            logger.Debug("D3D9RendererSink::Setup()");

            this.hWnd = hwnd;
            this.EncoderSettings = settings;


            var avgTimePerFrame = MfTool.FrameRateToAverageTimePerFrame(EncoderSettings.FrameRate);
            this.EncoderSettings.AverageTimePerFrame = avgTimePerFrame;

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
                Resolution = settings.Resolution, // 


                //Resolution = new System.Drawing.Size(1920, 1088),
                FrameRate = settings.FrameRate, //new Tuple<int, int>(settings.FrameRate, 1),

            });

            videoRenderer.RendererStarted += VideoRenderer_RendererStarted;
            videoRenderer.RendererStopped += VideoRenderer_RendererStopped;

            videoRenderer.SetPresentationClock(presentationClock);
            videoRenderer.Resize(new System.Drawing.Rectangle(0, 0, 100, 100));

            SharpDX.MediaFoundation.DirectX.Direct3DDeviceManager d3dManager = null;
            if (EncoderSettings.UseHardware)
            {
                d3dManager = videoRenderer.D3DDeviceManager;
            }

            decoder = new MfH264Dxva2Decoder(d3dManager);

            var inputArgs = new MfVideoArgs
            {
                Width = EncoderSettings.Resolution.Width,
                Height = EncoderSettings.Resolution.Height,
                FrameRate = MfTool.PackToLong(EncoderSettings.FrameRate),
                LowLatency = EncoderSettings.LowLatency,
            };

            decoder.Setup(inputArgs);
        }


        private long presentationAdjust = long.MaxValue;
        public void Start()
        {
            logger.Debug("D3D9RendererSink::Start()");

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
                        try
                        {
                            var dest = mb.Lock(out int cbMaxLength, out int cbCurrentLength);
                            Marshal.Copy(data, 0, dest, data.Length);

                            mb.CurrentLength = data.Length;
                        }
                        finally
                        {
                            mb.Unlock();
                        }

                        encodedSample.AddBuffer(mb);
                    }
                    


                    if (!double.IsNaN(time) && time > 0)
                    {

   
                        // может глючить рендерер если метки будут кривые!!
                        // TODO: сделать валидацию вр.меток до декодера и после...
                        var sampleTime = MfTool.SecToMfTicks(time);

                        if (presentationAdjust == long.MaxValue)
                        {
                            var presentaionTime = presentationClock.Time;
                            presentationAdjust = presentaionTime - sampleTime;

                        }




                        encodedSample.SampleTime = sampleTime;// + presentationAdjust; 
                        encodedSample.SampleDuration = 0;// MfTool.SecToMfTicks(0.033);

                        //logger.Debug(">>>>>>>>>>> " + sampleTime);
                    }
                    else
                    {
                        encodedSample.SampleTime = 0;
                        encodedSample.SampleDuration = 0;
                    }
                    

                    //logger.Debug("ProcessData " + time);

                    var res = decoder.ProcessSample(encodedSample, OnSampleDecoded, OnMediaTypeChanged);
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

        private void OnMediaTypeChanged(SharpDX.MediaFoundation.MediaType mediaType)
        {
            if (videoRenderer != null)
            {
                videoRenderer.SetMediaType(mediaType);
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

				//using (var buffer = sample.ConvertToContiguousBuffer())
				//{
				//	MediaFactory.GetService(buffer, MediaServiceKeys.Buffer, IID.D3D9Surface, out var pSurf);

				//	var surf = new SharpDX.Direct3D9.Surface(pSurf);

				//	var descr = surf.Description;
				//	logger.Debug(descr.Format);
				//}

				var sampleTime = sample.SampleTime;


                //sampleTime += presentationAdjust;

                if (prevSampleTime == 0)
                {
                    prevSampleTime = sampleTime;
                }
                //_sampleTime += 33333;

                var timeDiff = (sampleTime - prevSampleTime);

                //if (timeDiff < 0)
                //{
                //    logger.Warn("Not monotonic time: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff));
                //}
                //else if (timeDiff > MfTool.SecToMfTicks(0.033))
                //{
                //    //logger.Warn("Large time gap: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff));

                //    var _delay = MfTool.MfTicksToSec(timeDiff - MfTool.SecToMfTicks(0.033)) * 1000;

                   
                //    //logger.Warn("Large time gap: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff, _delay));
                //    Thread.Sleep((int)_delay);
                //}

                unwrappedSampleTime += timeDiff;
                sample.SampleTime = unwrappedSampleTime;


                var avgTimePerFrame = EncoderSettings.AverageTimePerFrame;
                sample.SampleDuration = avgTimePerFrame; // MfTool.SecToMfTicks(0.033);

                var presentationTime = presentationClock.Time;

                if (!EncoderSettings.LowLatency)
                {
                    var delta = unwrappedSampleTime - presentationTime;

                    
                    //var avgDuration = MfTool.SecToMfTicks(0.033);
                    if (delta < 0)
                    {
                        //logger.Warn("delta: " + string.Join(" ",  delta));
                    }
                    else if (delta > avgTimePerFrame)
                    {
                        //logger.Warn("Large time gap: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff));

                        var _delay = MfTool.MfTicksToSec(delta - avgTimePerFrame) * 1000;


                        logger.Warn("Large time gap: " + string.Join(" ", sampleTime, prevSampleTime, timeDiff, _delay));
                        Thread.Sleep((int)_delay);
                    }
                }


                sample.SampleTime = 0;
                sample.SampleDuration = 0;


                if (EncoderSettings.UseHardware)
                {
                    videoRenderer.ProcessDxva2Sample(sample);
                }
                else
                {
                    videoRenderer.ProcessSample(sample);
                }

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

            this.Close();

            Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

        }

        public void Stop()
        {
            logger.Debug("D3D9RendererSink::Stop()");

            decoder.Stop();

            presentationClock.Stop();

        }

        public void Close()
        {

            logger.Debug("D3D9RendererSink::Close()");

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