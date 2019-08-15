using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.Linq;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;
using Device = SharpDX.Direct3D11.Device;
using System.IO;

namespace ScreenStreamer.MediaFoundation
{

    public class MfEncoderAsync
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DXGIDeviceManager deviceManager = null;
        private SharpDX.DXGI.Factory1 dxgiFactory = null;
        private SharpDX.DXGI.Adapter1 adapter = null;

        public Device device = null;

        private Transform encoder = null;

        private Sample bufSample = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        private long frameNumber = -1;
        private long frameDuration;

        public MfEncoderAsync()
        { }


        public void Setup(VideoWriterArgs args)
        {
            logger.Debug("MfEncoderAsync::Setup(...)");

            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                //logger.Warn("Windows versions earlier than 8 are not supported.");
                throw new NotSupportedException("Windows versions earlier than 8 are not supported.");
            }

            var inputFormat = VideoFormatGuids.Argb32;

            try
            {
                SetupDx(args);


                int adapterVenId = adapter.Description.VendorId;
                long adapterLuid = adapter.Description.Luid;

                encoder = FindEncoder(adapterVenId);

                if (encoder == null)
                { 

                    throw new NotSupportedException("Hardware encode acceleration is not available on this platform.");
                    //logger.Warn("Encoder not found");
                    //return;
                }

                SetupEncoder(args);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }


        private void SetupDx(VideoWriterArgs args)
        {
            logger.Debug("SetupDx(...)");

            int width = args.Width;
            int height = args.Height;

            dxgiFactory = new SharpDX.DXGI.Factory1();
            adapter = dxgiFactory.Adapters1[0];
            var descr = adapter.Description;

            logger.Info("Adapter: " + descr.Description + " " + descr.DeviceId + " " + descr.VendorId);

            device = new Device(adapter,
                // DeviceCreationFlags.Debug | //System.AccessViolationException CopyResource(...)
                DeviceCreationFlags.VideoSupport |
                DeviceCreationFlags.BgraSupport);

            deviceManager = new DXGIDeviceManager();
            deviceManager.ResetDevice(device);

            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            var _descr = new Texture2DDescription
            {
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1 },
            };

            bufTexture = new Texture2D(device, _descr);

            MediaBuffer mediaBuffer = null;
            try
            {
                //mediaBuffer = MediaFactory.CreateMemoryBuffer(4 * width * height);
                MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);
                bufSample = MediaFactory.CreateSample();
                bufSample.AddBuffer(mediaBuffer);
            }
            finally
            {
                mediaBuffer.Dispose();
            }
        }

        private Transform FindEncoder(int adapterVenId)
        {
            logger.Debug("FindEncoder(...) " + adapterVenId);

            Transform preferEncoder = null;

            var transformFlags = TransformEnumFlag.Hardware | // TransformEnumFlag.All |
                                 TransformEnumFlag.SortAndFilter;

            var outputType = new TRegisterTypeInformation
            {
                GuidMajorType = MediaTypeGuids.Video,
                GuidSubtype = VideoFormatGuids.H264
            };

            //int vendorId = 0;
            //using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
            //{
            //    var adapter = dxgiDevice.Adapter;
            //    vendorId = adapter.Description.VendorId;
            //}


            var transformActivators = MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder, transformFlags, null, outputType);
            try
            {
                Activate preferActivator = null;
                string preferActivatorDescr = "";
                foreach (var activator in transformActivators)
                {

                    //bool isHardware = flags.HasFlag(TransformEnumFlag.Hardware);
                    //bool isAsync = flags.HasFlag(TransformEnumFlag.Asyncmft);
                    //Guid clsid = activator.Get(TransformAttributeKeys.);
                    string name = activator.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
                    Guid clsid = activator.Get(TransformAttributeKeys.MftTransformClsidAttribute);
                    TransformEnumFlag flags = (TransformEnumFlag)activator.Get(TransformAttributeKeys.TransformFlagsAttribute);


                    bool isAsync = !(flags.HasFlag(TransformEnumFlag.Syncmft));
                    isAsync |= (flags.HasFlag(TransformEnumFlag.Asyncmft));
                    bool isHardware = (flags.HasFlag(TransformEnumFlag.Hardware));

                    if (isHardware)
                    {
                        string venIdStr = activator.Get(TransformAttributeKeys.MftEnumHardwareVendorIdAttribute);
                        var index = venIdStr.IndexOf("VEN_");
                        if (index >= 0)
                        {
                            var startIndex = "VEN_".Length - index;
                            var venid = venIdStr.Substring(startIndex, venIdStr.Length - startIndex);

                            if (!string.IsNullOrEmpty(venid))
                            {
                                int.TryParse(venid, System.Globalization.NumberStyles.HexNumber, null, out int activatorVendorId);
                                if (activatorVendorId == adapterVenId)
                                {
                                    preferActivator = activator;
                                }
                            }
                        }
                    }


                    var _flags = Enum.GetValues(typeof(TransformEnumFlag))
                                 .Cast<TransformEnumFlag>()
                                 .Where(m => (m != TransformEnumFlag.None && flags.HasFlag(m)));

                    var transformInfo = name + " " + clsid.ToString() + " " + string.Join("|", _flags);

                    logger.Info(transformInfo);

                    logger.Debug(MfTool.LogMediaAttributes(activator));

                    //var HardwareUrl = activator.Get(TransformAttributeKeys.MftEnumHardwareUrlAttribute);
                    //logger.Info(HardwareUrl);

                    //var TransformAsync = activator.Get(TransformAttributeKeys.TransformAsync);
                    //logger.Info(TransformAsync);
                    //logger.Info("-------------------------------------");
                }

               // preferEncoder = transformActivators[1].ActivateObject<Transform>();

                if (preferActivator != null)
                {
                   preferEncoder = preferActivator.ActivateObject<Transform>();

                }

            }
            finally
            {
                foreach (var activator in transformActivators)
                {
                    activator.Dispose();
                }
            }

            return preferEncoder;
        }



        private void SetupEncoder(VideoWriterArgs args)
        {
            logger.Debug("SetupEncoder(...)");

            var fps = args.FrameRate;
            var width = args.Width;
            var height = args.Height;

            var inputFormat = VideoFormatGuids.Argb32;

            logger.Info("Encoder input params: " + width + "x" + height + " fps=" + fps + " {" + inputFormat+"}");
            using (var attr = encoder.Attributes)
            {
                // TODO:
                // log pref

                var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
                if (transformAsync)
                {
                    attr.Set(TransformAttributeKeys.TransformAsyncUnlock, 1);
                    attr.Set(TransformAttributeKeys.MftSupportDynamicFormatChange, true);

                    bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                    if (d3d11Aware)
                    {
                        encoder.ProcessMessage(TMessageType.SetD3DManager, deviceManager.NativePointer);                      
                    }

                    attr.Set(MFAttributeKeys.CODECAPI_AVLowLatencyMode, true);

                    //attr.Set(SinkWriterAttributeKeys.LowLatency, true);
                    //attr.Set(CODECAPI_AVEncNumWorkerThreads, 8);
                }

                var attrLog = MfTool.LogMediaAttributes(attr);

                logger.Debug("\r\nMFT:\r\n-----------------\r\n" + attrLog);
            }


            int inputStreamCount = -1;
            int outputStreamsCount = -1;
            encoder.GetStreamCount(out inputStreamCount, out outputStreamsCount);
            int[] inputStreamIDs = new int[inputStreamCount];
            int[] outputStreamIDs = new int[outputStreamsCount];

            bool res = encoder.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
            if (res)
            {
                inputStreamId = inputStreamIDs[0];
                outputStreamId = outputStreamIDs[0];
            }
            else
            {
                inputStreamId = 0;
                outputStreamId = 0;
            }


            MediaType outputMediaType = null;
            for (int i = 0; ; i++)
            {
                res = encoder.TryGetOutputAvailableType(outputStreamId, i, out outputMediaType);

                if (!res)
                {
                    break;
                }

                //outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);
                outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));

                outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                encoder.SetOutputType(outputStreamId, outputMediaType, 0);

                var mediaLog = MfTool.LogMediaType(outputMediaType);
                logger.Debug("\r\nOutputMediaType:\r\n-----------------\r\n" + mediaLog);
                outputMediaType.Dispose();
                outputMediaType = null;
                break;
            }


            MediaType inputMediaType = null;
            try
            {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        encoder.GetInputAvailableType(0, i, out inputMediaType);

                        var formatId = inputMediaType.Get(MediaTypeAttributeKeys.Subtype);
                        if (formatId == inputFormat)
                        {
                            //logger.Debug("inputFormat " + inputFormat);
                            break;
                        }
                        inputMediaType.Dispose();
                        inputMediaType = null;
                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        if (ex.ResultCode != SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                        {
                            throw;
                        }
                        break;
                    }
                }

                if (inputMediaType == null)
                {
                    throw new FormatException("Unsuported format: " + inputFormat);
                }

                //inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                //inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
                inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));

                inputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                inputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                encoder.SetInputType(inputStreamId, inputMediaType, 0);

                var mediaLog = MfTool.LogMediaType(inputMediaType);
                logger.Debug("\r\nInputMediaType:\r\n-----------------\r\n" + mediaLog);
            }
            finally
            {
                inputMediaType?.Dispose();
            }

            //TOutputStreamInformation sinfo;
            //transform.GetOutputStreamInfo(0, out sinfo);
        }

        public void Start()
        {
            logger.Debug("MfEncoderAsync::Start()");


            encoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

            Task.Run(() =>
            {
                EventProc();

            });

        }

        private volatile bool closing = false;
        private void EventProc()
        {
            logger.Debug("EventProc() BEGIN");

            MediaEventGenerator eventGen = null;

            try
            {
                eventGen = encoder.QueryInterface<MediaEventGenerator>();

                while (!closing)
                {
                    MediaEvent mediaEvent = null;
                    try
                    {
                        mediaEvent = eventGen.GetEvent(true);

                        if (closing)
                        {
                            break;
                        }

                        // logger.Debug("GetEvent(...) " + mediaEvent.TypeInfo);

                        if (mediaEvent.TypeInfo == MediaEventTypes.TransformNeedInput)
                        {
                            lock (syncRoot)
                            {
                                inputRequests++;

                                ProcessInput();
                            }

                            //logger.Debug("inputRequests " + inputRequests);
                        }
                        else if (mediaEvent.TypeInfo == MediaEventTypes.TransformHaveOutput)
                        {
                            ProcessOutput();

                        }
                        else if (mediaEvent.TypeInfo == MediaEventTypes.TransformDrainComplete)
                        {
                            logger.Warn("_MediaEventTypes.TransformDrainComplete");
                            closing = true;
                        }
                        else if (mediaEvent.TypeInfo == MediaEventTypes.TransformMarker)
                        {
                            logger.Warn("_MediaEventTypes.TransformMarker");
                        }
                        else if (mediaEvent.TypeInfo == MediaEventTypes.TransformInputStreamStateChanged)
                        {
                            logger.Warn("_MediaEventTypes.TransformInputStreamStateChanged");
                        }
                        else if (mediaEvent.TypeInfo == MediaEventTypes.TransformUnknown)
                        {
                            logger.Warn("_MediaEventTypes.TransformUnknown");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);

                        Thread.Sleep(10);
                    }
                    finally
                    {
                        if (mediaEvent != null)
                        {
                            mediaEvent.Dispose();
                            mediaEvent = null;
                        }                   
                    }
                }

            }
            finally
            {

                eventGen?.Dispose();
                Close();
            }

            logger.Debug("EventProc() END");

        }

         private void ProcessInput()
        {
            if (closing)
            {
                return;
            }

            if (needUpdate)
            {
                if (inputRequests > 0)
                {
                    inputRequests--;
                    encoder.ProcessInput(inputStreamId, bufSample, 0);

                    needUpdate = false;

                    //logger.Debug("ProcessInput() " + inputRequests);
                }
                else
                {
                    //logger.Debug("inputRequests == 0 " + inputRequests);
                }
            }
        }


        private void ProcessOutput()
        {
            if (closing)
            {
                return;
            }

            encoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

            MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
            bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

            // Create output sample
            Sample outputSample = null;
            try
            {
                if (createSample)
                {
                    Debug.Assert(streamInfo.CbSize > 0, "streamInfo.CbSize > 0");

                    outputSample = MediaFactory.CreateSample();
                    using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize))
                    {
                        outputSample.AddBuffer(mediaBuffer);
                    }
                }

                TOutputDataBuffer[] outputDataBuffer = new TOutputDataBuffer[1];

                var data = new TOutputDataBuffer
                {
                    DwStatus = 0,
                    DwStreamID = 0,
                    PSample = outputSample,
                    PEvents = null,
                };
                outputDataBuffer[0] = data;


                //bool res = true;
                // processor.ProcessOutput(TransformProcessOutputFlags.None,  1, outputDataBuffer, out TransformProcessOutputStatus status);
                //var res = processor.ProcessOutput(TransformProcessOutputFlags.None,  data, out TransformProcessOutputStatus status);

                var res = encoder.TryProcessOutput(TransformProcessOutputFlags.None, 1, outputDataBuffer, out TransformProcessOutputStatus status);

                // var res = encoder.ProcessOutput(TransformProcessOutputFlags.None, data, out TransformProcessOutputStatus status);
                if (res.Success)
                {

                    if (outputSample == null)
                    {
                        outputSample = outputDataBuffer[0].PSample;
                    }


                    Debug.Assert(outputSample != null, "res.Success && outputSample != null");

                    var buffer = outputSample.ConvertToContiguousBuffer();
                    try
                    {
                        var ptr = buffer.Lock(out int cbMaxLength, out int cbCurrentLength);
                        try
                        {
                            if (cbCurrentLength > 0)
                            {
                                byte[] buf = new byte[cbCurrentLength];
                                Marshal.Copy(ptr, buf, 0, buf.Length);

                                OnDataReady(buf);
                            }
                        }
                        finally
                        {
                            buffer.Unlock();
                        }

                        // logger.Info(outputSample.SampleTime + " " + buffer.CurrentLength);
                    }
                    finally
                    {
                        buffer?.Dispose();
                    }
                }
                else
                {
                    if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                    {
                        logger.Warn(res.ToString() + " TransformNeedMoreInput");

                        //Result = true;

                    }
                    else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                    {
                        //...
                        //transform.TryGetOutputAvailableType(outputStreamId, 0, out MediaType typeOut);
                        //transform.SetOutputType(outputStreamId, typeOut, 0);

                        logger.Warn(res.ToString() + " TransformStreamChange");
                    }
                    else
                    {
                        res.CheckError();
                    }
                }
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

        public event Action<byte[]> DataReady;

        private void OnDataReady(byte[] buf)
        {
            DataReady?.Invoke(buf);
        }

        private volatile int inputRequests = 0;

        private static object syncRoot = new object();
        private Texture2D bufTexture = null;
        private volatile bool needUpdate = false;

        public void WriteTexture(Texture2D texture)
        {
            if (closing)
            {
                return;
            }

            lock (syncRoot)
            {
                using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
                {
                    using (var sharedTexture = device.OpenSharedResource<Texture2D>(sharedRes.SharedHandle))
                    {
                        device.ImmediateContext.CopyResource(sharedTexture, bufTexture);

                        needUpdate = true;

                        ProcessInput();
                    }
                }

                //device.ImmediateContext.CopyResource(texture, bufTexture);
                //needUpdate = true;

                //ProcessInput();
            }

        }



        public void Stop()
        {
            logger.Debug("MfEncoderAsync::Stop()");

            if (encoder != null)
            {
                encoder.ProcessMessage(TMessageType.CommandDrain, IntPtr.Zero);
                encoder.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
                encoder.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
                encoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);

            }

        }

        public void Close()
        {
            logger.Debug("MfEncoderAsync::Close()");

            closing = true;
            if (encoder != null)
            {
                encoder.Dispose();
                encoder = null;
            }

            if (bufSample != null)
            {
                bufSample.Dispose();
                bufSample = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (adapter != null)
            {
                adapter.Dispose();
                adapter = null;
            }

            if (dxgiFactory != null)
            {
                dxgiFactory.Dispose();
                dxgiFactory = null;
            }

            if (deviceManager != null)
            {
                deviceManager.Dispose();
                deviceManager = null;
            }

        }

        public Format GetFormatFromGuid(Guid guid)
        {
            Format format = Format.Unknown;
            if (guid == VideoFormatGuids.NV12)
            {
                format = Format.NV12;
            }
            else if (guid == VideoFormatGuids.Argb32)
            {
                format = Format.B8G8R8A8_UNorm;
            }
            return format;
        }

    }



    //public class MfEncoder
    //{
    //    private static Logger logger = LogManager.GetCurrentClassLogger();

    //    public Device device = null;

    //    private Transform transform = null;
    //    private MediaEventGenerator eventGen = null;


    //    private Texture2D bufTexture = null;
    //    private Sample videoSample = null;
    //    private MediaBuffer mediaBuffer = null;

    //    private int inputStreamId = -1;
    //    private int outputStreamId = -1;

    //    private long frameNumber = -1;
    //    private long frameDuration;

    //    // Keep this separate. First frame might be a RepeatFrame.
    //    bool isFirstFrame = true;

    //    public MfEncoder(Device device)
    //    {
    //        this.device = device;

    //    }

    //    public void Setup(VideoWriterArgs Args)
    //    {
    //        logger.Debug("MfWriter::Init(...)");

    //        frameDuration = 10_000_000 / Args.FrameRate;

    //        var width = Args.Width;
    //        var height = Args.Height;
    //        var bufSize = width * height * 4;

    //        var inputFormat = VideoFormatGuids.NV12;
    //        //  var inputFormat = VideoFormatGuids.Rgb32; // VideoFormatGuids.NV12

    //        var device = new Device(SharpDX.Direct3D.DriverType.Hardware,
    //            DeviceCreationFlags.Debug | DeviceCreationFlags.VideoSupport);

    //        DXGIDeviceManager devMan = new DXGIDeviceManager();
    //        devMan.ResetDevice(device);

    //        bufTexture = new Texture2D(device, new Texture2DDescription
    //        {
    //            Format = Format.NV12,
    //            Width = width,
    //            Height = height,
    //            MipLevels = 1,
    //            ArraySize = 1,
    //            SampleDescription = { Count = 1, Quality = 0 },
    //        });


    //        try
    //        {
    //            TRegisterTypeInformation inputType = new TRegisterTypeInformation();

    //            TRegisterTypeInformation outputType = new TRegisterTypeInformation
    //            {
    //                GuidMajorType = MediaTypeGuids.Video,
    //                GuidSubtype = VideoFormatGuids.H264
    //            };

    //            var transformActivators = MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder,
    //                TransformEnumFlag.Hardware);// | TransformEnumFlag.SortAndFilter, inputType, outputType);

    //            foreach (var activator in transformActivators)
    //            {
    //                var FriendlyName = activator.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
    //                logger.Info(FriendlyName);

    //                var HardwareUrl = activator.Get(TransformAttributeKeys.MftEnumHardwareUrlAttribute);
    //                logger.Info(HardwareUrl);

    //                var TransformAsync = activator.Get(TransformAttributeKeys.TransformAsync);
    //                logger.Info(TransformAsync);
    //                logger.Info("-------------------------------------");

    //            }

    //            var activatior = transformActivators[0];

    //            transform = activatior.ActivateObject<Transform>();

    //            using (var attr = transform.Attributes)
    //            {
    //                var transformName = attr.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
    //                logger.Info(transformName);

    //                var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
    //                if (transformAsync)
    //                {
    //                    attr.Set(TransformAttributeKeys.TransformAsyncUnlock, 1);
    //                    attr.Set(SinkWriterAttributeKeys.LowLatency, true);
    //                    //attr.Set(SinkWriterAttributeKeys.DisableThrottling, 1);

    //                    attr.Set(SinkWriterAttributeKeys.D3DManager, devMan);
    //                }
    //            }

    //            eventGen = transform.QueryInterface<MediaEventGenerator>();



    //            uint MF_E_TRANSFORM_ASYNC_LOCKED = 0xC00D6D77;
    //            uint MF_E_SHUTDOWN = 0xC00D3E85;

    //            int inputStreamCount = -1;
    //            int outputStreamsCount = -1;
    //            transform.GetStreamCount(out inputStreamCount, out outputStreamsCount);
    //            int[] inputStreamIDs = new int[inputStreamCount];
    //            int[] outputStreamIDs = new int[outputStreamsCount];
    //            try
    //            {
    //                bool res = transform.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
    //                if (!res)
    //                {
    //                    inputStreamId = 0;
    //                    outputStreamId = 0;
    //                }
    //                else
    //                {
    //                    inputStreamId = inputStreamIDs[0];
    //                    outputStreamId = outputStreamIDs[0];
    //                }
    //            }
    //            catch (Exception ex)
    //            {

    //                logger.Error(ex);
    //            }

    //            transform.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);

    //            var outputMediaType = new MediaType();
    //            try 
    //            {
    //                outputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
    //                outputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
    //                outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 8_000_000);
    //                outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
    //                outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, PackLong(width, height));
    //                outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(Args.FrameRate, 1));
    //                outputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, PackLong(1, 1));
    //                outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

    //                transform.SetOutputType(outputStreamId, outputMediaType, 0);
    //            }
    //            finally
    //            {
    //                outputMediaType?.Dispose();
    //            }

    //            MediaType inputMediaType = null;
    //            try
    //            {
    //                transform.GetInputAvailableType(0, 0, out inputMediaType);
    //                inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
    //                inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
    //                inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, PackLong(width, height));
    //                inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(Args.FrameRate, 1));
    //                transform.SetInputType(inputStreamId, inputMediaType, 0);
    //            }
    //            finally
    //            {
    //                inputMediaType?.Dispose();
    //            }



    //            //Callback callback = new Callback(this);
    //            //mediaEventGenerator.BeginGetEvent(callback, this);
    //            //MediaSessionCallback callback = new MediaSessionCallback()



    //            //bufTexture = new Texture2D(device, new Texture2DDescription
    //            //{
    //            //    CpuAccessFlags = CpuAccessFlags.Read,
    //            //    BindFlags = BindFlags.None,
    //            //    Format = Format.B8G8R8A8_UNorm,
    //            //    Width = width,
    //            //    Height = height,
    //            //    OptionFlags = ResourceOptionFlags.None,
    //            //    MipLevels = 1,
    //            //    ArraySize = 1,
    //            //    SampleDescription = { Count = 1, Quality = 0 },
    //            //    Usage = ResourceUsage.Staging
    //            //});

    //            /*
    //            videoSample = MediaFactory.CreateVideoSampleFromSurface(null);

    //            // Create the media buffer from the texture
    //            MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);

    //            using (var buffer2D = mediaBuffer.QueryInterface<Buffer2D>())
    //            {
    //                mediaBuffer.CurrentLength = buffer2D.ContiguousLength;
    //            }

    //            // Attach the created buffer to the sample
    //            videoSample.AddBuffer(mediaBuffer);
    //            */

    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error(ex);

    //            Close();
    //            throw;
    //        }
    //    }

    //    public void Start()
    //    {
    //        logger.Debug("MfWriter::Start()");

    //        transform.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
    //        transform.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
    //        transform.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

    //        Task.Run(() => 
    //        {
    //            bool encoding = false;
    //            while (true)
    //            {

    //                try
    //                {
    //                    var ev = eventGen.GetEvent(true);
    //                    if (ev != null)
    //                    {
    //                        if (ev.TypeInfo == MediaEventTypes.TransformNeedInput)
    //                        {
    //                            encoding = true;

    //                            MediaBuffer inputBuffer = null;
    //                            MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out inputBuffer);
    //                            Sample sample = MediaFactory.CreateSample();
    //                            sample.AddBuffer(inputBuffer);

    //                            transform.ProcessInput(inputStreamId, sample, 0);

    //                        }
    //                        else if (ev.TypeInfo == MediaEventTypes.TransformHaveOutput)
    //                        {
    //                            if (!encoding)
    //                            {
    //                                logger.Warn("encoding " + encoding);
    //                            }                                

    //                            encoding = false;



    //                            //TOutputDataBuffer outputBuffer = new TOutputDataBuffer();
    //                            //outputBuffer.DwStreamID = 0;
    //                            TOutputDataBuffer[] outputDataBuffer = new TOutputDataBuffer[]
    //                            {
    //                                new TOutputDataBuffer
    //                                {
    //                                    DwStreamID = outputStreamId,
    //                                    DwStatus = 0,
    //                                },
    //                            };

    //                            //TOutputDataBuffer[] outputDataBuffer = new TOutputDataBuffer[1];

    //                            TransformProcessOutputStatus status;
    //                            bool res = transform.ProcessOutput(0, outputDataBuffer, out status);

    //                            foreach(var data in outputDataBuffer)
    //                            {
    //                                var dataSample = data.PSample;
    //                                if (dataSample != null)
    //                                {
    //                                    dataSample.Dispose();
    //                                    dataSample = null;
    //                                }

    //                                var dataEvent = data.PEvents;
    //                                if (dataEvent != null)
    //                                {
    //                                    dataEvent.Dispose();
    //                                    dataEvent = null;
    //                                }
    //                            }

    //                        }

    //                        ev.Dispose();
    //                    }
    //                }
    //                catch(Exception ex)
    //                {
    //                    logger.Error(ex.Message);

    //                    Thread.Sleep(10);
    //                }

    //            }

    //        });


    //    }

    //    public void WriteTexture(Texture2D texture)
    //    {
    //        if (closed)
    //        {
    //            return;
    //        }

    //        //device.ImmediateContext.CopyResource(texture, bufTexture);

    //        //WriteSample(videoSample);
    //    }


    //    private readonly object syncLock = new object();
    //    public void WriteSample(Sample sample)
    //    {
    //        if (closed)
    //        {
    //            return;
    //        }

    //    }

    //    public void Stop()
    //    {
    //        logger.Debug("MfWriter::Stop()");
    //        /*
    //         * 	hr = transform->ProcessMessage(MFT_MESSAGE_NOTIFY_END_OF_STREAM, NULL);
    //         CHECK_HR(hr, "Failed to process END_OF_STREAM command on H.264 MFT");

    //         hr = transform->ProcessMessage(MFT_MESSAGE_NOTIFY_END_STREAMING, NULL);
    //         CHECK_HR(hr, "Failed to process END_STREAMING command on H.264 MFT");

    //         hr = transform->ProcessMessage(MFT_MESSAGE_COMMAND_FLUSH, NULL);
    //         CHECK_HR(hr, "Failed to process FLUSH command on H.264 MFT");
    //         */


    //        transform.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
    //        transform.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
    //        transform.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);

    //    }

    //    private volatile bool closed = false;

    //    public void Close()
    //    {
    //        logger.Debug("MfWriter::Close()");

    //        closed = true;
    //        if (transform != null)
    //        {
    //            transform.Dispose();
    //            transform = null;
    //        }

    //        if (eventGen != null)
    //        {
    //            eventGen.Dispose();
    //            eventGen = null;
    //        }

    //        if (bufTexture != null)
    //        {
    //            bufTexture.Dispose();
    //            bufTexture = null;
    //        }

    //        if (videoSample != null)
    //        {
    //            videoSample.Dispose();
    //            videoSample = null;
    //        }

    //        if (mediaBuffer != null)
    //        {
    //            mediaBuffer.Dispose();
    //            mediaBuffer = null;
    //        }

    //    }

    //    public static long PackLong(int Left, int Right)
    //    {
    //        return new PackedLong
    //        {
    //            Low = Right,
    //            High = Left
    //        }.Long;
    //    }

    //    private class Callback : AsyncCallbackBase
    //    {
    //        private readonly MfEncoder encoder = null;
    //        //private readonly MediaEventGenerator eventGen = null;
    //        internal Callback(MfEncoder encoder)
    //        {
    //            this.encoder = encoder;

    //        }

    //        public override void Invoke(AsyncResult asyncResult)
    //        {
    //            logger.Debug("TransformEventCallback::Invoke(...) " + asyncResult.Status);

    //            var mediaEvent = encoder.eventGen.EndGetEvent(asyncResult);

    //            logger.Debug("mediaEvent.TypeInfo " + mediaEvent.TypeInfo);

    //            if(mediaEvent.TypeInfo == MediaEventTypes.TransformNeedInput)
    //            {
    //                //...
    //                encoder.transform.ProcessInput(0, encoder.videoSample, 0);
    //            }
    //            else if(mediaEvent.TypeInfo == MediaEventTypes.TransformHaveOutput)
    //            {
    //                TOutputStreamInformation streamInfo;
    //                encoder.transform.GetOutputStreamInfo(0, out streamInfo);

    //                MftOutputStreamInformationFlags streamFlags =(MftOutputStreamInformationFlags)streamInfo.DwFlags;
    //                if (streamFlags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples))
    //                {
    //                    /*
    //                        The MFT provides the output samples for this stream, either by allocating them internally or by operating directly on the input samples. 
    //                        The MFT cannot use output samples provided by the client for this stream.
    //                        If this flag is not set, the MFT must set cbSize to a nonzero value in the MFT_OUTPUT_STREAM_INFO structure, 
    //                        so that the client can allocate the correct buffer size. For more information, see IMFTransform::GetOutputStreamInfo.
    //                        This flag cannot be combined with the MFT_OUTPUT_STREAM_CAN_PROVIDE_SAMPLES flag.
    //                     */
    //                }
    //                Sample sample = MediaFactory.CreateSample();
    //                MediaBuffer mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize);
    //                sample.AddBuffer(mediaBuffer);

    //                TOutputDataBuffer data = new TOutputDataBuffer();
    //                data.PSample = sample;

    //                TOutputDataBuffer[] buf = new TOutputDataBuffer[] { data };

    //                TransformProcessOutputStatus status;
    //                var res = encoder.transform.ProcessOutput(TransformProcessOutputFlags.None, buf, out status);

    //                var s = buf[0].PSample;
    //                if (s != null)
    //                {
    //                    //s.
    //                }
    //                logger.Debug("ProcessOutput " + res + " " + status.ToString());

    //            }

    //            encoder.eventGen.BeginGetEvent(this, null);
    //        }
    //    }
    //}

}