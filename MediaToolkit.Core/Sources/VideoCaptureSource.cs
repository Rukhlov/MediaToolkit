using MediaToolkit.Common;
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

        public VideoCaptureSource() { }

        private SharpDX.Direct3D11.Device device = null;
        public Texture2D SharedTexture { get; private set; }

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

    
        public CaptureState State { get; private set; } = CaptureState.Stopped;

        public event Action<CaptureState> StateChanged;
        private void OnStateChanged(CaptureState state)
        {
            StateChanged?.Invoke(state);
        }

        private Texture2D texture = null;
        private SourceReader sourceReader = null;

        private MfVideoProcessor processor = null;

        public void Setup(object pars)
        {
            logger.Debug("VideoCaptureSource::Setup()");

            MfVideoCaptureParams captureParams = pars as MfVideoCaptureParams;

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

                logger.Debug("------------------CurrentMediaType-------------------");
                var mediaType = sourceReader.GetCurrentMediaType(SourceReaderIndex.FirstVideoStream);
                logger.Debug(MfTool.LogMediaType(mediaType));

                srcSize = MfTool.GetFrameSize(mediaType);

                var destSize = captureParams.DestSize;

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
                         //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                         OptionFlags = ResourceOptionFlags.Shared,

                     });

                texture = new Texture2D(device,
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
                            //OptionFlags = ResourceOptionFlags.Shared,
                            OptionFlags = ResourceOptionFlags.None,
                        });


                processor = new MfVideoProcessor(null);
                var inProcArgs = new MfVideoArgs
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    // Format = VideoFormatGuids.Rgb24,
                    Format = subtype,//VideoFormatGuids.NV12,
                };


                var outProcArgs = new MfVideoArgs
                {
                    Width = destSize.Width,
                    Height = destSize.Height,
                    Format = VideoFormatGuids.Argb32,
                    //Format = VideoFormatGuids.Rgb32,//VideoFormatGuids.Argb32,
                };

                processor.Setup(inProcArgs, outProcArgs);


                //processor.SetMirror(VideoProcessorMirror.MirrorHorizontal);
                processor.SetMirror(VideoProcessorMirror.MirrorVertical);


                State = CaptureState.Initialized;
                OnStateChanged(State);

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

        private SourceReader CreateSourceReader(Activate activate)
        {
            SourceReader reader = null;

            using (var source = activate.ActivateObject<MediaSource>())
            {
                using (var mediaAttributes = new MediaAttributes(IntPtr.Zero))
                {
                    /* //Не все камеры поддерживают!
                    MediaFactory.CreateAttributes(mediaAttributes, 10);
                    //mediaAttributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, 1);

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
                return null ;
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

        public void Start()
        {
            logger.Debug("VideoCaptureSource::Start()");

            if(State != CaptureState.Initialized)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            State = CaptureState.Starting;
            OnStateChanged(State);

            Task.Run(() =>
            {
                processor.Start();

                int sampleCount = 0;

                try
                {
                    State = CaptureState.Capturing;
                    OnStateChanged(State);

                    while (State == CaptureState.Capturing)
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

                            if (sample != null)
                            {
                                ProcessSample(sample);
                            }
                        }
                        finally
                        {
                            sample?.Dispose();
                        }

                        sampleCount++;

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    if (processor != null)
                    {
                        processor.Stop();
                    }

                    CleanUp();

                    State = CaptureState.Stopped;
                    OnStateChanged(State);
                }

            });

        }

        private void ProcessSample(Sample sample)
        {
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
                        var ptr = mediaBuffer.Lock(out int cbMaxLengthRef, out int cbCurrentLengthRef);

                        ////var width = outProcArgs.Width;
                        ////var height = outProcArgs.Height;

                        var dataBox = device.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);

                        Kernel32.CopyMemory(dataBox.DataPointer, ptr, (uint)cbCurrentLengthRef);

                        device.ImmediateContext.UnmapSubresource(texture, 0);


                        device.ImmediateContext.CopyResource(texture, SharedTexture);
                        device.ImmediateContext.Flush();

                        OnBufferUpdated();

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

            State = CaptureState.Stopping;
           
        }



        public void Close()
        {
            logger.Debug("VideoCaptureSource::Close()");

            CleanUp();
        }

        private void CleanUp()
        {
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

            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }

            if (processor != null)
            {
                processor.Close();
                processor = null;
            }

            State = CaptureState.Stopped;

        }

        public static void LogSourceTypes(SourceReader sourceReader)
        {
            int streamIndex = 0;
            while (true)
            {
                bool invalidStreamNumber = false;

                int _streamIndex = -1;

                for (int mediaIndex = 0; ; mediaIndex++)
                {
                    try
                    {
                        var nativeMediaType = sourceReader.GetNativeMediaType(streamIndex, mediaIndex);

                        if (_streamIndex != streamIndex)
                        {
                            _streamIndex = streamIndex;
                            Console.WriteLine("====================== StreamIndex#" + streamIndex + "=====================");
                        }

                        Console.WriteLine(MfTool.LogMediaType(nativeMediaType));
                        nativeMediaType?.Dispose();

                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                        {
                            Console.WriteLine("");
                            break;
                        }
                        else if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidStreamNumber)
                        {
                            invalidStreamNumber = true;
                            break;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (invalidStreamNumber)
                {
                    break;
                }

                streamIndex++;
            }
        }

    }

}
