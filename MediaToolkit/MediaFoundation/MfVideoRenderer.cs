using MediaToolkit.SharedTypes;
using NLog;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.MediaFoundation
{
    public class MfVideoRenderer: IVideoRenderer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MfVideoRenderer()
        { }

        private VideoDisplayControl videoControl = null;

        private PresentationClock presentationClock = null;

        private StreamSink streamSink = null;
        private Sample videoSample = null;
        private MediaSink videoSink = null;
        private Direct3DDeviceManager deviceManager = null;

        private VideoRenderer videoRenderer = null;

        private VideoSampleAllocator videoSampleAllocator = null;

        byte[] videoBuffer = new byte[1024];

        private volatile int streamSinkRequestSample = 0;
        private MediaEventHandler streamSinkEventHandler = null;

        public event Action RendererStarted;
        public event Action RendererStopped;

        private volatile RendererState rendererState = RendererState.Closed;
        public RendererState State { get => rendererState; }

        private volatile int errorCode = 0;
        public int ErrorCode { get => errorCode; }

        private bool IsRunning => (rendererState == RendererState.Started || rendererState == RendererState.Paused);

        private object syncLock = new object();

        public void Setup(VideoRendererArgs videoArgs)
        {

            logger.Debug("MfVideoRenderer::Setup(...) " + videoArgs.ToString());

            var pixFmt = videoArgs.PixelFormat;
            var videoFormat = VideoFormatGuids.FromFourCC(new SharpDX.Multimedia.FourCC(pixFmt));
            var resolution = videoArgs.Resolution;
            var hWnd = videoArgs.hWnd;

            MfVideoArgs mfVideoArgs = new MfVideoArgs
            {
                Format = videoFormat,
                Width = resolution.Width,
                Height = resolution.Height,
            };

            Setup(hWnd, videoFormat, resolution);
        }

        public void Setup(IntPtr hWnd, Guid videoFormat, System.Drawing.Size videoResolution)
        {
            logger.Debug("MfVideoRenderer::Setup(...) " + hWnd);

            if (rendererState != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid state: " + rendererState);
            }

            try
            {
                Activate activate = null;
                try
                {
                    MediaFactory.CreateVideoRendererActivate(hWnd, out activate);
                    videoSink = activate.ActivateObject<MediaSink>();
                }
                finally
                {
                    activate?.Dispose();
                }

                //var characteristics = videoSink.Characteristics;
                //var logCharacts = MfTool.LogEnumFlags((MediaSinkCharacteristics)characteristics);
                //logger.Debug("VideoSinkCharacteristics: " + logCharacts);

                //using (var attrs = videoSink.QueryInterface<MediaAttributes>())
                //{
                //    var attrLog = MfTool.LogMediaAttributes(attrs);
                //    logger.Debug("EVRSinkAttrubutes:\r\n" + attrLog);
                //}

                videoRenderer = videoSink.QueryInterface<VideoRenderer>();
                videoRenderer.InitializeRenderer(null, null);

                using (var service = videoSink.QueryInterface<ServiceProvider>())
                {
                    deviceManager = service.GetService<Direct3DDeviceManager>(MediaServiceKeys.VideoAcceleration);

                    videoControl = service.GetService<VideoDisplayControl>(MediaServiceKeysEx.RenderService);

                    //var renderingPrefs = VideoRenderPrefs.DoNotClipToDevice;// | VideoRenderPrefs.DoNotRenderBorder;
                    //videoControl.RenderingPrefs = (int)renderingPrefs;
                    //videoControl.BorderColor = 0xFFFFFF;//0x008000;

                    videoControl.VideoWindow = hWnd;

                    var srcRect = new VideoNormalizedRect
                    {
                        Left = 0,
                        Top = 0,
                        Right = 1,
                        Bottom = 1,
                    };

                    videoControl.SetVideoPosition(srcRect, null);

                }


                //using (var service = videoSink.QueryInterface<ServiceProvider>())
                //{
                //    var pVideoProcessor = service.GetService(MediaServiceKeysEx.MixerService, typeof(IMFVideoProcessor).GUID);
                //    var comObj = Marshal.GetObjectForIUnknown(pVideoProcessor);
                //    try
                //    {
                //        var videoProcessor = (IMFVideoProcessor)comObj;
                //        videoProcessor.SetBackgroundColor(0x008000);
                //    }
                //    finally
                //    {
                //        Marshal.FinalReleaseComObject(comObj);
                //    }
                //}

                if (videoSink.StreamSinkCount == 0)
                {
                    //TODO:..
                }


                videoSink.GetStreamSinkByIndex(0, out streamSink);
                using (var handler = streamSink.MediaTypeHandler)
                {
                    using (var attrs = streamSink.QueryInterface<MediaAttributes>())
                    {
                        var attrLog = MfTool.LogMediaAttributes(attrs);
                        logger.Debug("EVRStreamSinkAttrubutes:\r\n" + attrLog);
                    }

                    //for(int i=0; i< handler.MediaTypeCount; i++)
                    //{
                    //    var mediaType = handler.GetMediaTypeByIndex(i);
                    //    logger.Debug(MfTool.LogMediaType(mediaType));
                    //}

                    using (var mediaType = new MediaType())
                    {
                        mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                        mediaType.Set(MediaTypeAttributeKeys.Subtype, videoFormat); //VideoFormatGuids.NV12 
                        mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(videoResolution.Width, videoResolution.Height));
                        mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                        //mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(30, 1));

                        //handler.IsMediaTypeSupported(mediaType, out MediaType _mediaType);

                        handler.CurrentMediaType = mediaType;
                    }

                    streamSinkEventHandler = new MediaEventHandler(streamSink);
                    streamSinkEventHandler.EventReceived += StreamSinkEventHandler_EventReceived;

                }


                InitSampleAllocator();

                rendererState = RendererState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

        }

        public void SetPresentationClock(PresentationClock clock)
        {
            if (videoSink != null)
            {
                videoSink.PresentationClock = clock;
            }
        }

        private void InitSampleAllocator()
        {
            logger.Debug("MfVideoRenderer::InitSampleAllocator()");
            using (var handler = streamSink.MediaTypeHandler)
            {
                using (var mediaType = handler.CurrentMediaType)
                {
                    using (var service = streamSink.QueryInterface<ServiceProvider>())
                    {
                        videoSampleAllocator = service.GetService<VideoSampleAllocator>(MediaServiceKeys.VideoAcceleration);
                        videoSampleAllocator.DirectXManager = deviceManager;
                        videoSampleAllocator.InitializeSampleAllocator(1, mediaType);
                        videoSampleAllocator.AllocateSample(out videoSample);
                        videoSample.SampleDuration = 0;
                        videoSample.SampleTime = 0;

                        //using (var mb = videoSample.ConvertToContiguousBuffer())
                        //{
                        //    if(videoBuffer.Length< mb.MaxLength)
                        //    {
                        //        videoBuffer = new byte[mb.MaxLength];
                        //    }
                        //}
                    }
                }
            }

        }


        public void Repaint()
        {
            if (!IsRunning)
            {
                //logger.Warn("Repaint() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                videoControl?.RepaintVideo();
            }

        }


        public void Resize(System.Drawing.Rectangle rect)
        {
            if (rendererState == RendererState.Closed)
            {
                //logger.Warn("Resize() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                var destRect = new RawRectangle
                {
                    Left = 0,
                    Top = 0,
                    Right = rect.Width,
                    Bottom = rect.Height,
                };

                videoControl?.SetVideoPosition(null, destRect);
            }

        }

        Stopwatch sw = new Stopwatch();
        private void StreamSinkEventHandler_EventReceived(MediaEvent mediaEvent)
        {

            var status = mediaEvent.Status;
            var typeInfo = mediaEvent.TypeInfo;

            if (status.Success)
            {
                if (typeInfo == MediaEventTypes.StreamSinkRequestSample)
                {
                    streamSinkRequestSample++;


                    //logger.Debug("StreamSinkRequestSample: " + sw.ElapsedMilliseconds);
                    sw.Restart();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkStarted)
                {
                    logger.Debug(typeInfo);

                    rendererState = RendererState.Started;
                    RendererStarted?.Invoke();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkStopped)
                {
                    logger.Debug(typeInfo);

                    rendererState = RendererState.Stopped;

                    RendererStopped?.Invoke();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkPaused)
                {
                    logger.Debug(typeInfo);
                    rendererState = RendererState.Paused;
                }
                else if (typeInfo == MediaEventTypes.StreamSinkMarker)
                {
                    logger.Debug(typeInfo);
                }
                else if (typeInfo == MediaEventTypes.StreamSinkDeviceChanged)
                {
                    logger.Debug(typeInfo);

                    OnDeviceChanged();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkFormatChanged)
                {
                    logger.Debug(typeInfo);
                    OnFormatChanged();

                    //...
                    errorCode = 100500;

                }
                else
                {
                    logger.Debug(typeInfo);
                }
            }
            else
            {

            }

        }



        public void ProcessSample(Sample sample)
        {
            if (!IsRunning)
            {
                logger.Debug("ProcessSample(...) return invalid render state: " + rendererState);
                return;
            }


            var sampleTime = sample.SampleTime;
            var sampleDuration = sample.SampleDuration;

            bool lockTacken = false;
            try
            {
                Monitor.TryEnter(syncLock, 10, ref lockTacken);
                if (lockTacken)
                {
                    videoSample.SampleDuration = sampleTime;
                    videoSample.SampleTime = sampleDuration;

                    using (var srcBuffer = sample.ConvertToContiguousBuffer())
                    {
                        try
                        {
                            var pSrcBuffer = srcBuffer.Lock(out int maxLen, out int curLen);

                            //// var srcArray = new byte[curLen];
                            // Marshal.Copy(pSrcBuffer, videoBuffer, 0, videoBuffer.Length);
                            // using (var dxBuffer = videoSample.ConvertToContiguousBuffer())
                            // {
                            //     using (var buffer2D = dxBuffer.QueryInterface<Buffer2D>())
                            //     {
                            //         buffer2D.ContiguousCopyFrom(videoBuffer, curLen);
                            //     }
                            // }

                            using (var buffer = videoSample.ConvertToContiguousBuffer())
                            {
                                using (var buffer2D = buffer.QueryInterface<Buffer2D>())
                                {
                                    buffer2D._ContiguousCopyFrom(pSrcBuffer, curLen);
                                }
                            }
                        }
                        finally
                        {
                            srcBuffer.Unlock();
                        }

                        if (streamSinkRequestSample > 0)
                        {
                            streamSink.ProcessSample(videoSample);

                            streamSinkRequestSample--;
                        }
                    }
                }
                else
                {
                    logger.Warn("Drop sample at " + sampleTime + " " + sampleDuration);
                }
            }
            catch (SharpDXException ex)
            {
                var resultCode = ex.ResultCode;

                if (resultCode == ResultCode.InvalidTimestamp || resultCode == ResultCode.NoSampleTimestamp)
                {
                    logger.Warn(resultCode + ": " + sampleTime);
                }
                else if (resultCode == ResultCode.NoSampleDuration || resultCode == ResultCode.DurationTooLong)
                {
                    logger.Warn(resultCode + ": " + sampleDuration);
                }
                //else if (resultCode == ResultCode.NoClock || resultCode == ResultCode.NotInitializeD)
                //{// Not Initialized...
                //    logger.Warn(resultCode);
                //}
                else
                {
                    logger.Error(ex);
                    //throw;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                if (lockTacken)
                {
                    Monitor.Exit(syncLock);
                }
            }
        }


        public void Start(long time)
        {
            logger.Debug("MfVideoRenderer::Start(...) " + time);

            if (rendererState == RendererState.Closed || rendererState == RendererState.Started)
            {
                logger.Warn("Start(...) return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {

                if (presentationClock != null)
                {
                    presentationClock.Dispose();
                    presentationClock = null;
                }

                MediaFactory.CreatePresentationClock(out presentationClock);

                PresentationTimeSource timeSource = null;
                try
                {
                    MediaFactory.CreateSystemTimeSource(out timeSource);

                    presentationClock.TimeSource = timeSource;

                    videoSink.PresentationClock = presentationClock;

                }
                finally
                {
                    timeSource?.Dispose();
                }


                presentationClock.GetState(0, out ClockState state);
                if (state != ClockState.Running)
                {
                    //var time = presentationClock.Time;

                    presentationClock.Start(time);
                }
                else
                {
                    logger.Warn("Start(...) return invalid clock state: " + state);
                }
            }
        }



        public void Stop()
        {
            logger.Debug("MfVideoRenderer::Stop()");

            if (!IsRunning)
            {
                logger.Warn("Stop() return invalid render state: " + rendererState);

                return;
            }

            lock (syncLock)
            {
                streamSinkRequestSample = 0;

                presentationClock.GetState(0, out ClockState state);
                if (state != ClockState.Stopped)
                {
                    presentationClock.Stop();
                }
                else
                {
                    logger.Warn("Stop() return invalid clock state: " + state);
                }
            }
        }

        private const long PRESENTATION_CURRENT_POSITION = 0x7fffffffffffffff;

        public void Pause()
        {
            logger.Debug("MfVideoRenderer::Pause()");
            if (!IsRunning)
            {
                logger.Warn("Pause() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                presentationClock.GetState(0, out ClockState state);
                if (state == ClockState.Running)
                {
                    presentationClock.Pause();
                }
                else if (state == ClockState.Paused)
                {
                    presentationClock.Start(PRESENTATION_CURRENT_POSITION);
                }
                else
                {
                    logger.Warn("Pause() return invalid clock state: " + state);
                }
            }

        }

        private void OnFormatChanged()
        {
            logger.Debug("MfVideoRenderer::OnFormatChanged()");
            //TODO:
        }

        private void OnDeviceChanged()
        {
            logger.Debug("MfVideoRenderer::OnDeviceChanged() " + streamSinkRequestSample);
            lock (syncLock)
            {

                CloseSampleAllocator();
                videoSample?.Dispose();

                InitSampleAllocator();


                //using (var dxBuffer = videoSample.ConvertToContiguousBuffer())
                //{
                //    using (var buffer2D = dxBuffer.QueryInterface<Buffer2D>())
                //    {
                //        buffer2D.ContiguousCopyFrom(videoBuffer, videoBuffer.Length);
                //    }
                //}

                //streamSink.ProcessSample(videoSample);

            }
        }


        public void Close()
        {
            logger.Debug("MfVideoRenderer::Close()");
            try
            {
                //lock (syncLock)
                {
                    CleanUp();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                //...
            }

            rendererState = RendererState.Closed;

        }

        private void CleanUp()
        {
            logger.Debug("MfVideoRenderer::CleanUp()");

            if (videoSink != null)
            {
                var clock = videoSink.PresentationClock;
                if (clock != null)
                {
                    clock.Dispose();
                }

                videoSink.PresentationClock = null;

                videoSink.Shutdown();

                videoSink.Dispose();
                videoSink = null;
            }

            if (streamSinkEventHandler != null)
            {
                streamSinkEventHandler.EventReceived -= StreamSinkEventHandler_EventReceived;
                streamSinkEventHandler.Dispose();
                streamSinkEventHandler = null;
            }

            if (videoRenderer != null)
            {
                videoRenderer.Dispose();
                videoRenderer = null;
            }

            if (videoControl != null)
            {
                videoControl.Dispose();
                videoControl = null;
            }

            if (presentationClock != null)
            {
                presentationClock.Dispose();
                presentationClock = null;
            }

            if (streamSink != null)
            {
                streamSink.Dispose();
                streamSink = null;
            }

            if (videoSample != null)
            {
                videoSample.Dispose();
                videoSample = null;
            }

            if (deviceManager != null)
            {
                deviceManager.Dispose();
                deviceManager = null;
            }

            CloseSampleAllocator();
        }

        private void CloseSampleAllocator()
        {
            if (videoSampleAllocator != null)
            {
                videoSampleAllocator.UninitializeSampleAllocator();
                videoSampleAllocator.Dispose();
                videoSampleAllocator = null;
            }
        }

    }


    public enum RendererState
    {
        Initialized,
        Started,
        Paused,
        Stopped,
        Closed,
    }
}
