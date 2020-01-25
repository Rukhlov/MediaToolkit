using MediaToolkit.NativeAPIs;
using MediaToolkit.NativeAPIs.Utils;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EVR = MediaToolkit.NativeAPIs.MF.EVR;

namespace MediaToolkit.MediaFoundation
{
    public class MfVideoRenderer : IVideoRenderer
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

        private EVR.IMFVideoMixerBitmap videoMixerBitmap = null;

        private VideoRenderer videoRenderer = null;

        private VideoSampleAllocator videoSampleAllocator = null;

        private volatile bool newSampleReceived = false;
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
        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private IntPtr WindowHandle = IntPtr.Zero;

        public void Setup(VideoRendererArgs videoArgs)
        {

            logger.Debug("MfVideoRenderer::Setup(...) " + videoArgs.ToString());

            var pixFmt = videoArgs.FourCC;
            var videoFormat = VideoFormatGuids.FromFourCC(new SharpDX.Multimedia.FourCC(pixFmt));
            var resolution = videoArgs.Resolution;
            var hWnd = videoArgs.hWnd;
            var interlaceMode = videoArgs.InterlaceMode;

            Setup(hWnd, videoFormat, resolution, interlaceMode);
        }

        public void Setup(IntPtr hWnd, Guid videoFormat, System.Drawing.Size videoResolution, int interlaceMode)
        {
            logger.Debug("MfVideoRenderer::Setup(...) " + hWnd);

            if (rendererState != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid state: " + rendererState);
            }

            this.WindowHandle = hWnd;
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

                using (var service = videoSink.QueryInterface<ServiceProvider>())
                {
                    videoMixerBitmap = service.GetNativeMfService<EVR.IMFVideoMixerBitmap>(MediaServiceKeysEx.MixerService);
                }

                /*
                EVR.IMFVideoProcessor videoProcessor = null;
                using (var service = videoSink.QueryInterface<ServiceProvider>())
                {
                    videoProcessor = service.GetNativeMfService<EVR.IMFVideoProcessor>(MediaServiceKeysEx.MixerService);
                }

                //videoProcessor.SetBackgroundColor(0x008000);
                ComBase.SafeRelease(videoProcessor);
                */


                videoSink.GetStreamSinkByIndex(0, out streamSink);
                using (var handler = streamSink.MediaTypeHandler)
                {
                    using (var attrs = streamSink.QueryInterface<MediaAttributes>())
                    {
                        var attrLog = MfTool.LogMediaAttributes(attrs);
                        logger.Debug("EVRStreamSinkAttrubutes:\r\n" + attrLog);
                    }

                    //for (int i = 0; i < handler.MediaTypeCount; i++)
                    //{
                    //    var mediaType = handler.GetMediaTypeByIndex(i);
                    //    logger.Debug(MfTool.LogMediaType(mediaType));
                    //}

                    do
                    {
                        /*
                       * The EVR does not report any preferred media types. 
                       * The client must test media types until it finds an acceptable type. 
                       * The media type for the reference stream must be set before the types can be set on any of the substreams.
                       * https://docs.microsoft.com/en-us/windows/win32/medfound/evr-media-type-negotiation
                       */

                        var mediaType = new MediaType();
                        try
                        {
   
                            mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                            mediaType.Set(MediaTypeAttributeKeys.Subtype, videoFormat); //VideoFormatGuids.NV12 
                            mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(videoResolution.Width, videoResolution.Height));

                            mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, interlaceMode);
                            mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                            //mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(30, 1));

                            //handler.IsMediaTypeSupported(mediaType, out MediaType mediaTypeOut);
                            //var res = handler._IsMediaTypeSupported(mediaType, out MediaType mediaTypeOut);

                            handler.CurrentMediaType = mediaType;
                            break;

                        }
                        catch (Exception)
                        {
                            //TODO: try other format...
                            var log = MfTool.LogMediaType(mediaType);
                            logger.Error("Not supported format: " + log);

                            if (videoFormat == VideoFormatGuids.Argb32)
                            {//Некоторые карты (например Datapath) не поддерживают Argb32, но могут поддерживать Rgb32
                                videoFormat = VideoFormatGuids.Rgb32;
                                continue;
                            }

                            throw new NotSupportedException("Not supported format: " + MfTool.GetMediaTypeName(videoFormat));
                        }
                        finally
                        {
                            if (mediaType != null)
                            {
                                mediaType.Dispose();
                                mediaType = null;
                            }
                        }
                        
                    }
                    while (true);


                    streamSinkEventHandler = new MediaEventHandler(streamSink);
                    streamSinkEventHandler.EventReceived += StreamSinkEventHandler_EventReceived;

                    //nativeStreamSink = (NativeAPIs.MF.IMFStreamSink)Marshal.GetObjectForIUnknown(streamSink.NativePointer);

                }

                InitSampleAllocator();


                //TryGetVideoCaps();
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

                        //videoSampleAllocatorCallback = (NativeAPIs.MF.IMFVideoSampleAllocatorCallback)videoSampleAllocator;

                        videoSampleAllocator.DirectXManager = deviceManager;
                        videoSampleAllocator.InitializeSampleAllocator(1, mediaType);
                        videoSampleAllocator.AllocateSample(out videoSample);
                        videoSample.SampleDuration = 0;
                        videoSample.SampleTime = 0;

                        //nativeVideoSample = (NativeAPIs.MF.Objects.IMFSample)Marshal.GetObjectForIUnknown(videoSample.NativePointer);

                    }   
                }
            }

        }

        private void StreamSinkEventHandler_EventReceived(MediaEvent mediaEvent)
        {
            try
            {
                var status = mediaEvent.Status;
                var typeInfo = mediaEvent.TypeInfo;

                if (status.Success)
                {
                    if (typeInfo == MediaEventTypes.StreamSinkRequestSample)
                    {
                        //logger.Debug(typeInfo);
                        OnRequestSample();

                    }
                    else if (typeInfo == MediaEventTypes.StreamSinkStarted)
                    {
                        logger.Debug(typeInfo);

                        OnStarted();
                    }
                    else if (typeInfo == MediaEventTypes.StreamSinkStopped)
                    {
                        logger.Debug(typeInfo);

                        OnStopped();
                    }
                    else if (typeInfo == MediaEventTypes.StreamSinkPaused)
                    {
                        logger.Debug(typeInfo);
                        rendererState = RendererState.Paused;
                    }
                    else if (typeInfo == MediaEventTypes.StreamSinkMarker)
                    {
                        logger.Debug(typeInfo);
                        //...
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
                    logger.Error("Event status: " + status);
                    // errorCode = 100500;
                    //..
                }
            }
            finally
            {
                mediaEvent?.Dispose();
            }
        }

        public void ProcessSample(Sample sample)
        {
            if (!IsRunning)
            {
                logger.Debug("MfVideoRenderer::ProcessSample(...) return invalid render state: " + rendererState);
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
                    using (var srcBuffer = sample.ConvertToContiguousBuffer())
                    {
                        try
                        {
                            var pSrcBuffer = srcBuffer.Lock(out int maxLen, out int curLen);
                            using (var buffer = videoSample.ConvertToContiguousBuffer())
                            {
                                using (var buffer2D = buffer.QueryInterface<Buffer2D>())
                                {
                                    //var isContiguousFormat = buffer2D.IsContiguousFormat;
                                    //if (isContiguousFormat)
                                    {
                                        buffer2D._ContiguousCopyFrom(pSrcBuffer, curLen);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            srcBuffer.Unlock();
                        }

                        newSampleReceived = true;
                        syncEvent.Set();
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

                    //streamSink.Flush();
                }
                else if (state == ClockState.Paused)
                {
                    presentationClock.Start(long.MaxValue);
                }
                else
                {
                    logger.Warn("Pause() return invalid clock state: " + state);
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

        public void SetBitmap(System.Drawing.Bitmap bmp, System.Drawing.RectangleF? destRect = null, float alpha = 1f)
        {
            logger.Debug("MfVideoRenderer::SetBitmap(...)");
            if (videoMixerBitmap == null)
            {
                logger.Warn("MfVideoRenderer::SetBitmap(...) videoMixerBitmap == null");
                return;
            }

            if (bmp != null)
            {
                IntPtr hdc = IntPtr.Zero;
                IntPtr hdcBmp = IntPtr.Zero;
                IntPtr hBitmap = IntPtr.Zero;
                try
                {
                    hdc = NativeAPIs.User32.GetDC(this.WindowHandle);
                    hdcBmp = NativeAPIs.Gdi32.CreateCompatibleDC(hdc);
                    hBitmap = bmp.GetHbitmap();

                    IntPtr hOld = IntPtr.Zero;
                    try
                    {
                        hOld = NativeAPIs.Gdi32.SelectObject(hdcBmp, hBitmap);

                        EVR.MFVideoNormalizedRect nrcDest = destRect.HasValue ?
                            new EVR.MFVideoNormalizedRect(destRect.Value) :
                            new EVR.MFVideoNormalizedRect();

                        EVR.MFVideoAlphaBitmapParams mfBmpParams = new EVR.MFVideoAlphaBitmapParams
                        {
                            rcSrc = new NativeAPIs.RECT(0, 0, bmp.Width, bmp.Height),
                            nrcDest = nrcDest,
                            dwFlags = (EVR.MFVideoAlphaBitmapFlags.Alpha | EVR.MFVideoAlphaBitmapFlags.DestRect),
                            fAlpha = alpha,

                        };

                        EVR.MFVideoAlphaBitmap mfBitmap = new EVR.MFVideoAlphaBitmap
                        {
                            GetBitmapFromDC = true,
                            Params = mfBmpParams,
                            Data = hdcBmp,
                        };

                        logger.Debug("MfVideoRenderer::SetAlphaBitmap()");

                        videoMixerBitmap.SetAlphaBitmap(mfBitmap);
                    }
                    finally
                    {
                        NativeAPIs.Gdi32.SelectObject(hdcBmp, hOld);
                    }
                }
                finally
                {
                    if (hBitmap != IntPtr.Zero)
                    {
                        NativeAPIs.Gdi32.DeleteObject(hBitmap);
                        hBitmap = IntPtr.Zero;
                    }

                    if (hdcBmp != IntPtr.Zero)
                    {
                        NativeAPIs.Gdi32.DeleteDC(hdcBmp);
                        hdcBmp = IntPtr.Zero;
                    }

                    if (hdc != IntPtr.Zero)
                    {
                        NativeAPIs.User32.ReleaseDC(WindowHandle, hdc);
                       // NativeAPIs.Gdi32.DeleteDC(hdc);
                        hdc = IntPtr.Zero;
                    }
                    
                }
            }
            else
            {
                logger.Debug("MfVideoRenderer::ClearAlphaBitmap()");

                videoMixerBitmap.ClearAlphaBitmap();
            }

        }


        public System.Drawing.Bitmap GetCurrentImage()
        {
            logger.Debug("MfVideoRenderer::GetCurrentImage()");

            System.Drawing.Bitmap bmp = null;
            EVR.IMFVideoDisplayControl control = null;
            try
            {
                control = (EVR.IMFVideoDisplayControl)Marshal.GetTypedObjectForIUnknown(videoControl.NativePointer, typeof(EVR.IMFVideoDisplayControl));

                var bih = new NativeAPIs.BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf(typeof(NativeAPIs.BITMAPINFOHEADER)),
                };

                IntPtr pBih = IntPtr.Zero;
                IntPtr pDib = IntPtr.Zero;
                try
                {
                    pBih = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeAPIs.BITMAPINFOHEADER)));

                    Marshal.StructureToPtr(bih, pBih, true);
                    var res = control.GetCurrentImage(pBih, out pDib, out int pcbDib, out long time);
                    //var res = videoControl.TryGetCurrentImage(pBih, out IntPtr pDib, out int pcbDib, out long time);
                    if (res == NativeAPIs.HResult.S_OK)
                    {
                        bih = (NativeAPIs.BITMAPINFOHEADER)Marshal.PtrToStructure(pBih, typeof(NativeAPIs.BITMAPINFOHEADER));
                        if (pcbDib > 0)
                        {
                            System.Drawing.Imaging.PixelFormat pixFmt = System.Drawing.Imaging.PixelFormat.Undefined;
                            if(bih.biCompression == 0)
                            {
                                if(bih.biBitCount == 32)
                                {
                                    pixFmt = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
                                }
                                else if (bih.biBitCount == 24)
                                {
                                    pixFmt = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                                }
                                //else if (bih.biBitCount == 16)
                                //{
                                //    pixFmt = System.Drawing.Imaging.PixelFormat.Format16bppRgb555;
                                //}
                            }

                            if(pixFmt == System.Drawing.Imaging.PixelFormat.Undefined)
                            {
                                throw new FormatException("Unsupported bitmap format");
                            }

                            bmp = new System.Drawing.Bitmap(bih.biWidth, bih.biHeight, pixFmt);
                            var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                                System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

                            NativeAPIs.Kernel32.CopyMemory(data.Scan0, pDib, (uint)pcbDib);
                            bmp.UnlockBits(data);

                            bmp.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid operation: " + res);
                    }

                }
                finally
                {
                    if (pDib != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pDib);
                        pDib = IntPtr.Zero;
                    }

                    if (pBih != null)
                    {
                        Marshal.FreeHGlobal(pBih);
                        pBih = IntPtr.Zero;
                    }

                }
            }
            finally
            {
                ComBase.SafeRelease(control);
            }

            return bmp;
        }


        private void OnStarted()
        {
            logger.Debug("MfVideoRenderer::OnStarted()");

            rendererState = RendererState.Started;

            DoRender();

            RendererStarted?.Invoke();
        }

        private void DoRender()
        {
            logger.Debug("MfVideoRenderer::DoRender()");
            Task.Run(() =>
            {
                //Stopwatch sw = new Stopwatch();
                while (rendererState == RendererState.Started)
                {
                    lock (syncLock)
                    {
                        if (streamSinkRequestSample > 0)
                        {
                            if (newSampleReceived)
                            {
                                streamSink.ProcessSample(videoSample);
                                streamSinkRequestSample--;
                                newSampleReceived = false;

                                //logger.Debug("StreamSinkRequestSample: " + sw.ElapsedMilliseconds + " " + streamSinkRequestSample);
                                //sw.Restart();
                            }

                        }
                    }

                    syncEvent.WaitOne(100);
                }
            });
        }

        private void OnRequestSample()
        {
            lock (syncLock)
            {
                streamSinkRequestSample++;
            }

            syncEvent.Set();
        }

        private void OnStopped()
        {

            logger.Debug("MfVideoRenderer::OnStopped()");

            rendererState = RendererState.Stopped;

            RendererStopped?.Invoke();

            syncEvent.Set();
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

                //byte[] buffer = null;
                //using (var dxBuffer = videoSample.ConvertToContiguousBuffer())
                //{
                //    var data = dxBuffer.Lock(out int maxLen, out int curLen);

                //    buffer = new byte[maxLen];
                //    Marshal.Copy(data, buffer, 0, buffer.Length);

                //    dxBuffer.Unlock();

                //}

                CloseSampleAllocator();
                videoSample?.Dispose();

                InitSampleAllocator();

                //if (buffer != null && buffer.Length > 0)
                //{
                //    using (var dxBuffer = videoSample.ConvertToContiguousBuffer())
                //    {
                //        using (var buffer2D = dxBuffer.QueryInterface<Buffer2D>())
                //        {
                //            buffer2D.ContiguousCopyFrom(buffer, buffer.Length);
                //        }
                //    }
                //    streamSink.ProcessSample(videoSample);
                //}

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
                // stops listening presentaton clock
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

            if (videoMixerBitmap != null)
            {
                ComBase.SafeRelease(videoMixerBitmap);
                videoMixerBitmap = null;
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


        private void TryGetVideoCaps()
        {// не работает!
            IntPtr hDevice = IntPtr.Zero;
            try
            {
                deviceManager.OpenDeviceHandle(out hDevice);
                Guid IID_IDirectXVideoProcessorService = new Guid("fc51a552-d5e7-11d9-af55-00054e43ff02");
                deviceManager.GetVideoService(hDevice, IID_IDirectXVideoProcessorService, out IntPtr pDxProcServ);

                using (VideoProcessorService directXVideoProcessorService = new VideoProcessorService(pDxProcServ))
                {
                    VideoDesc videoDesc = new VideoDesc
                    {
                        Format = SharpDX.Direct3D9.Format.A8R8G8B8,
                        //SampleFormat = new ExtendedFormat { },
                        //SampleWidth = videoResolution.Width,
                        //SampleHeight = videoResolution.Height,

                    };
                    Guid[] guids = new Guid[16];
                    directXVideoProcessorService.GetVideoProcessorDeviceGuids(ref videoDesc, out int count, guids);

                    SharpDX.Direct3D9.Format[] formats = new SharpDX.Direct3D9.Format[1024];
                    directXVideoProcessorService.GetVideoProcessorRenderTargets(guids[0], ref videoDesc, out int fcount, formats);

                }

            }
            finally
            {
                deviceManager.CloseDeviceHandle(hDevice);
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
