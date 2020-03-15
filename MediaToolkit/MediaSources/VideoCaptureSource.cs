using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;

using MediaToolkit.NativeAPIs;

using NLog;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using GDI = System.Drawing;

namespace MediaToolkit
{
    public class VideoCaptureSource : IVideoSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public VideoCaptureSource()
        {
            this.state = CaptureState.Closed;

        }

        private SharpDX.Direct3D11.Device device = null;
        public Texture2D SharedTexture { get; private set; }
        public long AdapterId { get; private set; } = -1;

        public VideoBuffer SharedBitmap { get; private set; }

        public event Action BufferUpdated;
        private void OnBufferUpdated()
        {
            BufferUpdated?.Invoke();
        }

        private GDI.Size srcSize = GDI.Size.Empty;
        public GDI.Size SrcSize
        {
            get
            {
                return srcSize;
            }
        }

        private volatile CaptureState state = CaptureState.Closed;
        public CaptureState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        private Texture2D stagingTexture = null;
        private SourceReader sourceReader = null;

        private MfVideoProcessor processor = null;

        public void Setup(object pars)
        {
            logger.Debug("VideoCaptureSource::Setup()");

            if (State != CaptureState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            UvcDevice captureParams = pars as UvcDevice;

            var deviceId = captureParams.DeviceId;

            try
            {
                int adapterIndex = 0;
                using (var dxgiFactory = new SharpDX.DXGI.Factory1())
                {
                    using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
                    {
                        var deviceCreationFlags = //DeviceCreationFlags.Debug |
                                                  DeviceCreationFlags.VideoSupport |
                                                  DeviceCreationFlags.BgraSupport;

                        device = new Device(adapter, deviceCreationFlags);

                        using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                        {
                            multiThread.SetMultithreadProtected(true);
                        }
                    }

                }

                sourceReader = CreateSourceReaderByDeviceId(deviceId);

                if(sourceReader == null)
                {
                    throw new Exception("Unable to create media source reader " + deviceId);
                }
     
                var mediaType = sourceReader.GetCurrentMediaType(SourceReaderIndex.FirstVideoStream);

                logger.Debug("------------------CurrentMediaType-------------------");
                logger.Debug(MfTool.LogMediaType(mediaType));

                srcSize = MfTool.GetFrameSize(mediaType);

                var destSize = captureParams.Resolution;

                if (destSize.IsEmpty)
                {
                    destSize = srcSize;
                }

                var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);


                mediaType?.Dispose();


                SharedTexture = new Texture2D(device,
                     new Texture2DDescription
                     {

                         CpuAccessFlags = CpuAccessFlags.None,
                         BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                         Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                         Width = destSize.Width,
                         Height = destSize.Height,
                         MipLevels = 1,
                         ArraySize = 1,
                         SampleDescription = { Count = 1, Quality = 0 },
                         Usage = ResourceUsage.Default,
                         OptionFlags = ResourceOptionFlags.Shared,

                     });

                stagingTexture = new Texture2D(device,
                        new Texture2DDescription
                        {
                            CpuAccessFlags = CpuAccessFlags.Read,
                            //BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                            BindFlags = BindFlags.None,
                            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                            Width = destSize.Width,
                            Height = destSize.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDescription = { Count = 1, Quality = 0 },
                            Usage = ResourceUsage.Staging,
                            OptionFlags = ResourceOptionFlags.None,
                        });


                processor = new MfVideoProcessor(null);
                var intupArgs = new MfVideoArgs
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    Format = subtype,//VideoFormatGuids.NV12,
                };


                var outputArgs = new MfVideoArgs
                {
                    Width = destSize.Width,
                    Height = destSize.Height,
                    Format = VideoFormatGuids.Argb32,
                };

                processor.Setup(intupArgs, outputArgs);
                processor.SetMirror(VideoProcessorMirror.MirrorVertical);

                state = CaptureState.Initialized;


            }
            catch (Exception ex)
            {
                logger.Error(ex);

                CleanUp();

                throw;
            }

        }

        private SourceReader CreateSourceReaderByDeviceId(string symLink)
        {
            Activate activate = null;
            SourceReader reader = null;
            try
            {
                activate = GetActivateBySymLink(symLink);
                if (activate != null)
                {
                    reader = CreateSourceReader(activate);
                }

            }
            finally
            {
                activate?.Dispose();
            }
            return reader;
        }

        private bool asyncMode = true;

        private SourceReaderCallback sourceReaderCallback = null;
        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private SourceReader CreateSourceReader(Activate activate)
        {
            SourceReader reader = null;

            using (var source = activate.ActivateObject<MediaSource>())
            {
                using (var mediaAttributes = new MediaAttributes())
                { 
                    if (asyncMode)
                    {
                        sourceReaderCallback = new SourceReaderCallback();
                        sourceReaderCallback.OnReadSample += SourceReaderCallback_OnReadSample;
                        sourceReaderCallback.OnFlush += SourceReaderCallback_OnFlush;

                        var pUnk = Marshal.GetIUnknownForObject(sourceReaderCallback);
                        try
                        {
                            using (var comObj = new SharpDX.ComObject(pUnk))
                            {
                                mediaAttributes.Set(SourceReaderAttributeKeys.AsyncCallback, comObj);
                            }
                        }
                        finally
                        {
                            if (pUnk != IntPtr.Zero)
                            {
                                Marshal.Release(pUnk);
                            } 
                        }
                    }

                    //mediaAttributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, 1);

                    /* //Не все камеры поддерживают!
                    mediaAttributes.Set(SinkWriterAttributeKeys.LowLatency, true);

                    mediaAttributes.Set(SourceReaderAttributeKeys.EnableAdvancedVideoProcessing, true);
                    mediaAttributes.Set(SinkWriterAttributeKeys.ReadwriteDisableConverters, 0);

                    mediaAttributes.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);
                    using (var devMan = new DXGIDeviceManager())
                    {
                        devMan.ResetDevice(device);
                        mediaAttributes.Set(SourceReaderAttributeKeys.D3DManager, devMan);
                    }
                    */


                    reader = new SourceReader(source, mediaAttributes);

                }
            }


            return reader;
        }



        private static Activate GetActivateBySymLink(string symLink)
        {
            Activate activate = null;
            Activate[] activates = null;
            using (var attributes = new MediaAttributes())
            {
                MediaFactory.CreateAttributes(attributes, 2);
                attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);
                //attributes.Set(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink, symLink);

                activates = MediaFactory.EnumDeviceSources(attributes);

            }

            if (activates == null || activates.Length == 0)
            {
                logger.Error("SourceTypeVideoCapture not found");
                return null;
            }


            foreach (var _activate in activates)
            {
                Console.WriteLine("---------------------------------------------");
                var friendlyName = _activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                var isHwSource = _activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapHwSource);
                //var maxBuffers = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapMaxBuffers);
                var symbolicLink = _activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);


                logger.Info("FriendlyName " + friendlyName + "\r\n" +
                "isHwSource " + isHwSource + "\r\n" +
                //"maxBuffers " + maxBuffers + 
                "symbolicLink " + symbolicLink);

                if (symbolicLink == symLink)
                {
                    activate = _activate;

                    continue;
                }

                _activate?.Dispose();
            }

            return activate;
        }

        private Task captureTask = null;
        public void Start()
        {
            logger.Debug("VideoCaptureSource::Start()");

            if (!(State == CaptureState.Stopped || State == CaptureState.Initialized))
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            state = CaptureState.Starting;

            captureTask = Task.Run(() =>
            {
                logger.Info("Capture thread started...");
                try
                {
                    CaptureStarted?.Invoke();

                    DoCapture();

                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    this.errorCode = 100500;
                }
                finally
                {
                    CaptureStopped?.Invoke(null);

                    logger.Info("Capture thread stopped...");
                }
            });

        }

        private CaptureStats captureStats = new CaptureStats();

        private long firstTimestamp = 0;
        private long prevTimestamp = 0;
        //private long sampleCount = 0;

        private void DoCapture()
        {

            try
            {
                state = CaptureState.Capturing;

                MediaToolkit.Utils.Statistic.RegisterCounter(captureStats);

                //int sampleCount = 0;

                processor.Start();


                if (asyncMode)
                {
                    //var comObj = Marshal.GetObjectForIUnknown(sourceReader.NativePointer);
                    //readerAsync = (IMFSourceReaderAsync)comObj;
                    // readerAsync.ReadSample(0, MF_SOURCE_READER_CONTROL_FLAG.None, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                    sourceReader.ReadSampleAsync((int)SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None);

                }

                while (State == CaptureState.Capturing)
                {
                    if (asyncMode)
                    {
                        syncEvent.WaitOne();

                        if (State == CaptureState.Capturing)
                        {
                            sourceReader.ReadSampleAsync((int)SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None);
                        }
                    }
                    else
                    {
                        var sample = sourceReader.ReadSample(SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None,
                            out int actualIndex, out SourceReaderFlags flags, out long timestamp);
                        try
                        {
                            //Console.WriteLine("#" + sampleCount + " Timestamp " + timestamp + " Flags " + flags);
                            if (flags != SourceReaderFlags.None)
                            {
                                logger.Debug("sourceReader.ReadSample(...) " + flags);
                            }

                            PrepareSample(flags, timestamp, sample);
                        }
                        finally
                        {
                            sample?.Dispose();
                        }
                    }

                    //sampleCount++;

                }
            }
            finally
            {

                if (processor != null)
                {
                    processor.Stop();
                }

                state = CaptureState.Stopped;
                MediaToolkit.Utils.Statistic.UnregisterCounter(captureStats);
            }
        }

        private int SourceReaderCallback_OnReadSample(SharpDX.Result result, int index, SourceReaderFlags flags, long timestamp, IntPtr pSample)
        {
            // logger.Debug(timestamp + " " + " " + flags + " " + result);
            if (result.Failure)
            {
                //...
            }

            if (State != CaptureState.Capturing)
            {
                return 0;
            }

            if (flags != SourceReaderFlags.None)
            {
                logger.Debug(timestamp + " " + " " + flags + " " + result);
            }

            PrepareSample(flags, timestamp, (Sample)pSample);

            syncEvent.Set();

            return 0;
        }

        private int SourceReaderCallback_OnFlush(int arg)
        {
            logger.Debug("SourceReaderCallback_OnFlush(...) " + arg);
            return 0;
        }

        private void PrepareSample(SourceReaderFlags flags, long timestamp, Sample sample)
        {
            if (flags.HasFlag(SourceReaderFlags.StreamTick))
            {
                firstTimestamp = timestamp;
            }

            if (sample != null)
            {
                //Console.WriteLine("time " + time + " Timestamp " + timestamp + " Flags " + flags);
                var sampleDuration = timestamp - prevTimestamp;
                var sampleTimestamp = timestamp - firstTimestamp;

                sample.SampleTime = sampleTimestamp;
                sample.SampleDuration = sampleDuration;

                ProcessSample(sample);
            }
            prevTimestamp = timestamp;
        }

        private void ProcessSample(Sample sample)
        {
            if(State != CaptureState.Capturing)
            {
                return;
            }

            Sample outputSample = null;
            try
            {
                var res = processor.ProcessSample(sample, out outputSample);

                if (res)
                {
                    // Console.WriteLine("outputSample!=null" + (outputSample != null));

                    var mediaBuffer = outputSample.ConvertToContiguousBuffer();
                    try
                    {
                        var pBuffer = mediaBuffer.Lock(out int cbMaxLengthRef, out int cbCurrentLengthRef);
                        var immediateContext = device.ImmediateContext;
                       
                        var dataBox = immediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, MapFlags.None);

                        Kernel32.CopyMemory(dataBox.DataPointer, pBuffer, (uint)cbCurrentLengthRef);

                        immediateContext.UnmapSubresource(stagingTexture, 0);

                        immediateContext.CopyResource(stagingTexture, SharedTexture);
                        immediateContext.Flush();
                        

                        OnBufferUpdated();

                        //var time = (double)(sample.SampleTime) / 10_000_000;

                        var time = MfTool.MfTicksToSec(sample.SampleTime);
                        captureStats.UpdateFrameStats(time, cbCurrentLengthRef);

                        mediaBuffer.Unlock();

                    }
                    finally
                    {
                        mediaBuffer?.Dispose();
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                if (outputSample != null)
                {
                    outputSample.Dispose();
                    outputSample = null;
                }
            }
        }

        public void Stop()
        {
            logger.Debug("VideoCaptureSource::Stop()");

            state = CaptureState.Stopping;

            if (asyncMode)
            {
                try
                {
                    if (sourceReader != null)
                    {
                        sourceReader.Flush(SourceReaderIndex.FirstVideoStream);
                    }
                }
                catch(Exception ex)
                {
                    logger.Debug(ex);
                }

                syncEvent.Set();
            }

        }



        public void Close(bool force = false)
        {
            logger.Debug("VideoCaptureSource::Close()");

            Stop();

            if (!force)
            {
                if (captureTask != null && captureTask.Status == TaskStatus.Running)
                {
                    bool waitResult = false;
                    do
                    {
                        waitResult = captureTask.Wait(1000);
                        if (!waitResult)
                        {
                            logger.Warn("ScreenSource::Close() " + waitResult);
                        }
                    } while (!waitResult);

                    captureTask = null;
                }
                
            }

            CleanUp();

            state = CaptureState.Closed;
        }

        private void CleanUp()
        {
            if (sourceReaderCallback != null)
            {
                sourceReaderCallback.OnReadSample -= SourceReaderCallback_OnReadSample;
                sourceReaderCallback.OnFlush -= SourceReaderCallback_OnFlush;
            }

            if (sourceReader != null)
            {
                //sourceReader.Flush(SourceReaderIndex.FirstVideoStream);

                sourceReader.Dispose();
                sourceReader = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (SharedTexture != null)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (stagingTexture != null)
            {
                stagingTexture.Dispose();
                stagingTexture = null;
            }

            if (processor != null)
            {
                processor.Close();
                processor = null;
            }


        }

        [ComImport, System.Security.SuppressUnmanagedCodeSecurity, 
            InterfaceType(ComInterfaceType.InterfaceIsIUnknown), 
            Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")]
        interface IMFSourceReaderCallback
        {
            [PreserveSig]
            int OnReadSample(SharpDX.Result hrStatus, int dwStreamIndex, SourceReaderFlags dwStreamFlags, long llTimestamp, IntPtr pSample);

            [PreserveSig]
            int OnFlush(int dwStreamIndex);

            [PreserveSig]
            int OnEvent(int dwStreamIndex, IntPtr pEvent);
        }


        class SourceReaderCallback : IMFSourceReaderCallback
        {
            public event Func<SharpDX.Result,  int, SourceReaderFlags, long, IntPtr, int> OnReadSample;
            public event Func<int, int> OnFlush;
            public event Func<int, IntPtr, int> OnEvent;

            int IMFSourceReaderCallback.OnReadSample(SharpDX.Result hrStatus, int dwStreamIndex, SourceReaderFlags dwStreamFlags, long llTimestamp, IntPtr pSample)
            {
                return OnReadSample?.Invoke(hrStatus, dwStreamIndex, dwStreamFlags, llTimestamp, pSample) ?? 0;
                //return (int)HResult.S_OK;
            }

            int IMFSourceReaderCallback.OnFlush(int dwStreamIndex)
            {
                return OnFlush?.Invoke(dwStreamIndex) ?? 0;
            }

            int IMFSourceReaderCallback.OnEvent(int dwStreamIndex, IntPtr pEvent)
            {
                return OnEvent?.Invoke(dwStreamIndex, pEvent) ?? 0;
            }
        }
    }
}
