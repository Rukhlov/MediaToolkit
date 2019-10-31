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

namespace MediaToolkit.MediaFoundation
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


        public MediaType InputMediaType { get; private set; }
        public MediaType OutputMediaType { get; private set; }


        public MfEncoderAsync()
        { }


        public void Setup(MfVideoArgs args)
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


        private void SetupDx(MfVideoArgs args)
        {
            logger.Debug("SetupDx(...)");

            int width = args.Width;
            int height = args.Height;

            dxgiFactory = new SharpDX.DXGI.Factory1();

            var adapterId = args.AdapterId;
            if (adapterId > 0)
            {
                adapter = dxgiFactory.Adapters1.FirstOrDefault(a => a.Description1.Luid == adapterId);
            }

            if (adapter == null)
            {
                adapter = dxgiFactory.Adapters1.FirstOrDefault();
            }

            var descr = adapter.Description;

            logger.Info("Adapter: " + descr.Description + " " + descr.DeviceId + " " + descr.VendorId);

            device = new Device(adapter,
                // DeviceCreationFlags.Debug | 
                DeviceCreationFlags.VideoSupport |
                DeviceCreationFlags.BgraSupport);


            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            deviceManager = new DXGIDeviceManager();
            deviceManager.ResetDevice(device);

            var _descr = new Texture2DDescription
            {
                Format = Format.NV12,
                //Format = Format.B8G8R8A8_UNorm,
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

            var transformFlags = TransformEnumFlag.All | // TransformEnumFlag.All |
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
                foreach (var activator in transformActivators)
                {

                    var actLog = MfTool.LogMediaAttributes(activator);

                    logger.Debug("\r\nActivator:\r\n-----------------\r\n" + actLog);


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
                    else
                    {
                        //TODO:...
                    }


                    //var _flags = Enum.GetValues(typeof(TransformEnumFlag))
                    //             .Cast<TransformEnumFlag>()
                    //             .Where(m => (m != TransformEnumFlag.None && flags.HasFlag(m)));

                    //var transformInfo = name + " " + clsid.ToString() + " " + string.Join("|", _flags);

                    //logger.Info(transformInfo);

                    //logger.Debug(MfTool.LogMediaAttributes(activator));

                    //var HardwareUrl = activator.Get(TransformAttributeKeys.MftEnumHardwareUrlAttribute);
                    //logger.Info(HardwareUrl);

                    //var TransformAsync = activator.Get(TransformAttributeKeys.TransformAsync);
                    //logger.Info(TransformAsync);
                    //logger.Info("-------------------------------------");
                }

                // preferEncoder = transformActivators[0].ActivateObject<Transform>();

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



        private void SetupEncoder(MfVideoArgs args)
        {
            logger.Debug("SetupEncoder(...)");

            var fps = args.FrameRate;
            var width = args.Width;
            var height = args.Height;

            //var inputFormat = VideoFormatGuids.Argb32;
            var inputFormat = VideoFormatGuids.NV12;

            logger.Info("Encoder input params: " + width + "x" + height + " fps=" + fps + " {" + inputFormat + "}");
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

                    attr.Set(MFAttributeKeys.CODECAPI_AVLowLatencyMode, args.LowLatency);

                    

                    // attr.Set(SinkWriterAttributeKeys.LowLatency, true);
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


            for (int i = 0; ; i++)
            {
                if (!encoder.TryGetOutputAvailableType(outputStreamId, i, out MediaType mediaType))
                {
                    //
                    logger.Warn("NoMoreOutputTypes");
                    break;
                }

               
                mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));

                mediaType.Set(MediaTypeAttributeKeys.Mpeg2Profile, (int)args.Profile);

                mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                int avgBitrate = args.Bitrate * 1000;

                mediaType.Set(MediaTypeAttributeKeys.AvgBitrate, avgBitrate);

                encoder.SetOutputType(outputStreamId, mediaType, 0);

                OutputMediaType = mediaType;

                var _mediaLog = MfTool.LogMediaType(mediaType);
                logger.Debug("\r\nOutputMediaType:\r\n-----------------\r\n" + _mediaLog);

                //logger.Debug("\r\n" + i + ". AvailableOutputMediaType:\r\n-----------------\r\n" + mediaLog);
                //mediaType.Dispose();
                //mediaType = null;
                break;
            }

            if (OutputMediaType == null)
            {
                //...
            }

            for (int i = 0; ; i++)
            {
                try
                {
                    encoder.GetInputAvailableType(0, i, out MediaType availableType);

                    //var log = MfTool.LogMediaType(availableType);
                    //logger.Debug("\r\n" + i + ". AvalibleInputMediaType:\r\n-----------------\r\n" + log);

                    var formatId = availableType.Get(MediaTypeAttributeKeys.Subtype);
                    if (formatId == inputFormat)
                    {
                        InputMediaType = availableType;
                        availableType = null;
                        //logger.Debug("inputFormat " + inputFormat);
                        //break;
                    }

                    if (availableType != null)
                    {
                        availableType.Dispose();
                        availableType = null;
                    }
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

            if (InputMediaType == null)
            {
                throw new FormatException("Unsuported input format: " + MfTool.GetMediaTypeName(inputFormat));
            }

            //InputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
            //InputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
            InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
            InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));

            InputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
            InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
            encoder.SetInputType(inputStreamId, InputMediaType, 0);

            var mediaLog = MfTool.LogMediaType(InputMediaType);
            logger.Debug("\r\nInputMediaType:\r\n-----------------\r\n" + mediaLog);


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

        Stopwatch sw = new Stopwatch();
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
                    sw.Restart();
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

                                //var ts = sw.ElapsedMilliseconds;
                                //Console.WriteLine("ElapsedMilliseconds " + ts);
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
                //using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
                //{
                //    using (var sharedTexture = device.OpenSharedResource<Texture2D>(sharedRes.SharedHandle))
                //    {
                //        device.ImmediateContext.CopyResource(sharedTexture, bufTexture);

                //        needUpdate = true;

                //        ProcessInput();
                //    }
                //}

                device.ImmediateContext.CopyResource(texture, bufTexture);
                needUpdate = true;

                ProcessInput();
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

            if (InputMediaType != null)
            {
                InputMediaType.Dispose();
                InputMediaType = null;
            }

            if (OutputMediaType != null)
            {
                OutputMediaType.Dispose();
                OutputMediaType = null;
            }

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

            if(bufTexture!=null)
            {
                bufTexture.Dispose();
                bufTexture = null;
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
            
            logger.Debug(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());


        }

    }
}