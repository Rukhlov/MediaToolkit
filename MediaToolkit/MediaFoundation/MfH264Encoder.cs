using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
//using NLog;
using System.Linq;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;

using System.IO;
using MediaToolkit.Logging;

namespace MediaToolkit.MediaFoundation
{

    public class MfEncoderAsync
    {

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        public SharpDX.Direct3D11.Device device = null;

        private Transform encoder = null;

        private MediaEventGenerator mediaEventGenerator = null;
        private MediaEventHandler eventHandler = null;

        private Sample bufSample = null;
        private Texture2D bufTexture = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;


        public MediaType InputMediaType { get; private set; }
        public MediaType OutputMediaType { get; private set; }

        private volatile int inputRequests = 0;
        private volatile int outputRequests = 0;

        private static object syncRoot = new object();

        private volatile bool needUpdate = false;
        private volatile bool stopping = false;
        private volatile bool closing = false;

        private bool syncMode = false;

        public MfEncoderAsync(SharpDX.Direct3D11.Device d = null)
        {
            this.device = d;

        }


        public void Setup(MfVideoArgs args)
        {
             logger.Debug("MfEncoderAsync::Setup(...)");

            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                throw new NotSupportedException("Windows versions earlier than 8 are not supported.");
            }

            try
            {
                var adapterId = args.AdapterId;

                using (var adapter = FindAdapter(adapterId))
                {
                    var descr = adapter.Description;
                    int adapterVenId = descr.VendorId;
                    long adapterLuid = descr.Luid;

                    logger.Info("Adapter: " + descr.Description + " " + adapterVenId);


                    if (device == null)
                    {
                        var flags =  DeviceCreationFlags.VideoSupport |
                                     DeviceCreationFlags.BgraSupport;
                                    //DeviceCreationFlags.Debug;

                        device = new SharpDX.Direct3D11.Device(adapter, flags);
                        using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                        {
                            multiThread.SetMultithreadProtected(true);
                        }
                    }
                   



                    SetupSampleBuffer(args);

                    encoder = FindEncoder(adapterVenId);
                    syncMode = false;

                    //encoder = new Transform(ClsId.MSH264EncoderMFT);
                    //syncMode = true;

                }

                if (encoder == null)
                {
                    throw new NotSupportedException("Hardware encode acceleration is not available on this platform.");
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

        private Adapter1 FindAdapter(long adapterId)
        {
            Adapter1 adapter1 = null;
            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {

                if (adapterId > 0)
                {
                    var adapters = dxgiFactory.Adapters1;
                    for (int i = 0; i < adapters.Length; i++)
                    {
                        var _adapter = adapters[i];
                        if (_adapter.Description1.Luid == adapterId)
                        {
                            adapter1 = _adapter;
                            continue;
                        }

                        _adapter.Dispose();
                    }
                }

                if (adapter1 == null)
                {
                    adapter1 = dxgiFactory.GetAdapter1(0);
                }
            }

            return adapter1;
        }

        private void SetupSampleBuffer(MfVideoArgs args)
        {
            logger.Debug("SetupSampleBuffer(...)");

            int width = args.Width;
            int height = args.Height;

            //if (width % 2 != 0)
            //{// должно быть четным...
            //    width++;
            //}

            //if (height % 2 != 0)
            //{
            //    height++;
            //}

            Format format = MfTool.GetDXGIFormatFromVideoFormatGuid(args.Format);

            if(format == Format.Unknown)
            {
                throw new NotSupportedException("Format not suppored " + args.Format);
            }

            var _descr = new Texture2DDescription
            {
                Format = format,
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
                MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);
                bufSample = MediaFactory.CreateSample();
                bufSample.AddBuffer(mediaBuffer);
            }
            finally
            {
                mediaBuffer?.Dispose();
            }
        }


        private Transform FindEncoder(int adapterVenId)
        {
            logger.Debug("FindEncoder(...) " + adapterVenId);

            Transform preferTransform = null;

            var transformFlags = TransformEnumFlag.All | // TransformEnumFlag.All |
                                 TransformEnumFlag.SortAndFilter;

            var outputType = new TRegisterTypeInformation
            {
                GuidMajorType = MediaTypeGuids.Video,
                 GuidSubtype = VideoFormatGuids.H264
               // GuidSubtype = VideoFormatGuids.Hevc
            };

            //int vendorId = 0;
            //using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
            //{
            //    var adapter = dxgiDevice.Adapter;
            //    vendorId = adapter.Description.VendorId;
            //}


            var activates = MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder, transformFlags, null, outputType);
            try
            {
                Activate preferActivate = null;
                foreach (var activate in activates)
                {
                    var actLog = MfTool.LogMediaAttributes(activate);

                    logger.Debug("\r\nActivator:\r\n-----------------\r\n" + actLog);

                    string name = activate.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
                    Guid clsid = activate.Get(TransformAttributeKeys.MftTransformClsidAttribute);
                    TransformEnumFlag flags = (TransformEnumFlag)activate.Get(TransformAttributeKeys.TransformFlagsAttribute);

                    bool isAsync = !(flags.HasFlag(TransformEnumFlag.Syncmft));
                    isAsync |= (flags.HasFlag(TransformEnumFlag.Asyncmft));
                    bool isHardware = flags.HasFlag(TransformEnumFlag.Hardware);

   
                    if (isHardware)
                    {
                        string venIdStr = activate.Get(TransformAttributeKeys.MftEnumHardwareVendorIdAttribute);

                        if (MfTool.TryGetVendorId(venIdStr, out int activatorVendId))
                        {
                            if (activatorVendId == adapterVenId)
                            {
                                preferActivate = activate;
                                syncMode = false;
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

                if (preferActivate != null)
                {
                    preferTransform = preferActivate.ActivateObject<Transform>();

                }

            }
            finally
            {
                foreach (var activator in activates)
                {
                    activator.Dispose();
                }
            }

            return preferTransform;
        }



        private void SetupEncoder(MfVideoArgs args)
        {
            logger.Debug("SetupEncoder(...)");

            var fps = args.FrameRate;
            var width = args.Width;
            var height = args.Height;

            int avgBitrate = args.AvgBitrate * 1000;
            int maxBitrate = args.MaxBitrate * 1000;
            int mpegProfile = (int)args.Profile;
            //if (width % 2 != 0)
            //{// должно быть четным...
            //    width++;
            //}

            //if (height % 2 != 0)
            //{
            //    height++;
            //}

            //var inputFormat = VideoFormatGuids.Argb32;
            var inputFormat = args.Format; //VideoFormatGuids.NV12;

            logger.Info("Encoder input params: " + width + "x" + height + " fps=" + fps + " {" + inputFormat + "}");
            using (var attr = encoder.Attributes)
            {
                // TODO:
                // log pref

                if (!syncMode)
                {
                    var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
                    if (transformAsync)
                    {
                        attr.Set(TransformAttributeKeys.TransformAsyncUnlock, 1);
                        attr.Set(TransformAttributeKeys.MftSupportDynamicFormatChange, true);

                        bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                        if (d3d11Aware)
                        {
                            using (var devMan = new DXGIDeviceManager())
                            {
                                devMan.ResetDevice(device);
                                encoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                            }
                        }
                    }
                }

                attr.Set(CodecApiPropertyKeys.AVLowLatencyMode, args.LowLatency);
                attr.Set(CodecApiPropertyKeys.AVEncCommonRateControlMode, args.BitrateMode);
                attr.Set(CodecApiPropertyKeys.AVEncCommonQuality, args.Quality);

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
                //var log = MfTool.LogMediaType(mediaType);
                //logger.Warn(log);

                mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));             
                mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                mediaType.Set(MediaTypeAttributeKeys.Mpeg2Profile, mpegProfile);
                mediaType.Set(MediaTypeAttributeKeys.AvgBitrate, avgBitrate);
                mediaType.Set(CodecApiPropertyKeys.AVEncCommonMaxBitRate, maxBitrate);

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


            //mediaEventGenerator = encoder.QueryInterface<MediaEventGenerator>();
            //eventHandler = new MediaEventHandler(mediaEventGenerator);
            //eventHandler.EventReceived += HandleMediaEvent;


            //encoder.GetInputStreamInfo(0, out TInputStreamInformation inputStreamInfo);
            //var inputInfoFlags = (MftInputStreamInformationFlags)inputStreamInfo.DwFlags;
            //logger.Debug(MfTool.LogEnumFlags(inputInfoFlags));

            //encoder.GetOutputStreamInfo(0, out TOutputStreamInformation outputStreamInfo);
            //var outputInfoFlags = (MftOutputStreamInformationFlags)outputStreamInfo.DwFlags;
            //logger.Debug(MfTool.LogEnumFlags(outputInfoFlags));


        }


        public void Start()
        {
            logger.Debug("MfEncoderAsync::Start()");

            /*
             * If the client does not send this message, the MFT allocates resources on the first call to ProcessInput. 
             * Therefore, sending this message can reduce the initial latency when streaming begins.
             */
            encoder.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);

            //Notifies a Media Foundation transform (MFT) that the first sample is about to be processed.
            encoder.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

            if (!syncMode)
            {
                Task.Run(() =>
                {
                    EventProc();

                });
            }

        }

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

                        HandleMediaEvent(mediaEvent);
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
                using (var shutdown = eventGen.QueryInterface<Shutdownable>())
                {
                    shutdown.Shutdown();
                }

                eventGen?.Dispose();
                Close();
            }

            logger.Debug("EventProc() END");

        }


        private void HandleMediaEvent(MediaEvent mediaEvent)
        {
            // logger.Debug("GetEvent(...) " + mediaEvent.TypeInfo);

            if (closing)
            {
                logger.Warn("HandleMediaEvent(...) " + mediaEvent.ToString());
                return;
            }


            if (mediaEvent.TypeInfo == MediaEventTypes.TransformNeedInput)
            {
                OnTransformNeedInput();

                //logger.Debug("inputRequests " + inputRequests);
            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformHaveOutput)
            {
                OnTransformHaveOutput();

            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformDrainComplete)
            {
                logger.Warn("_MediaEventTypes.TransformDrainComplete ");

                OnTransformDrainComplete();
            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformMarker)
            {
                logger.Warn("_MediaEventTypes.TransformMarker");
            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformInputStreamStateChanged)
            {
                logger.Warn("_MediaEventTypes.TransformInputStreamStateChanged");
            }
            else
            {
                logger.Warn("_MediaEventTypes " + mediaEvent.TypeInfo);
            }

        }

        private void OnTransformNeedInput()
        {
            lock (syncRoot)
            {
                inputRequests++;

                ProcessInput();
            }
        }

        private void OnTransformHaveOutput()
        {
            //outputRequests++;

            ProcessOutput();

            //outputRequests--;
        }

        private void OnTransformDrainComplete()
        {
            closing = true;

            Close();
        }


        private void ProcessInput()
        {
            if (stopping)
            {
                logger.Debug("ProcessInput() stopping");
                return;
            }

            if (closing)
            {
                logger.Debug("ProcessInput() closing");
                return;
            }

            if (needUpdate)
            {
                if (inputRequests > 0)
                {
                    inputRequests--;
                   // sw.Restart();
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
            if (stopping)
            {
                logger.Warn("ProcessOutput() stopping...");
            }

            if (closing)
            {
                logger.Warn("ProcessOutput() closing...");
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

                var res = encoder.TryProcessOutput(TransformProcessOutputFlags.None, outputDataBuffer, out TransformProcessOutputStatus status);

                if (res == SharpDX.Result.Ok)
                {
                    if (outputSample == null)
                    {
                        outputSample = outputDataBuffer[0].PSample;
                    }

                    Debug.Assert(outputSample != null, "res.Success && outputSample != null");

                    SampleReady?.Invoke(outputSample);
 
                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                {

                    //logger.Debug(res.ToString() + " TransformNeedMoreInput");

                    //Result = true;

                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                {// не должны приходить для энкодера...
 
                    logger.Warn(res.ToString() + " TransformStreamChange");
                }
                else
                {
                    res.CheckError();
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

        private static Guid uuidTexture2d = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));
        public Sample ProcessSample(Sample sample)
        {
            if (stopping)
            {
                logger.Warn("ProcessSample(...) stopping");
                return null;
            }

            if (closing)
            {
                logger.Warn("ProcessSample(...) closing");
                return null;
            }

            Sample outputSample = null;
            if (!syncMode)
            {
                EncodeSampleAsync(sample);
            }
            else
            {
                var result = EncodeSample(sample, out outputSample);
                if (!result)
                {
                    //...
                }

            }

            return outputSample;
        }

        private void EncodeSampleAsync(Sample sample)
        {

            using (var buffer = sample.ConvertToContiguousBuffer())
            {
                using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                {
                    dxgiBuffer.GetResource(uuidTexture2d, out IntPtr intPtr);
                    using (Texture2D texture = new Texture2D(intPtr))
                    {
                        lock (syncRoot)
                        {
                            bufSample.SampleTime = sample.SampleTime;
                            bufSample.SampleDuration = sample.SampleDuration;
                            //..

                            device.ImmediateContext.CopyResource(texture, bufTexture);
                            needUpdate = true;

                            ProcessInput();

                        }
                    };
                }
            }
        }

        public event Action<Sample> SampleReady;

        private void OnSampleReady(Sample sample)
        {
            SampleReady?.Invoke(sample);
        }


        public unsafe bool EncodeSample(Sample inputSample, out Sample outputSample)
        {
            bool Result = false;
            outputSample = null;

            if (inputSample == null)
            {
                return false;
            }

            encoder.ProcessInput(0, inputSample, 0);

            //if (processor.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
            {

                encoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

                MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
                bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

                // Create output sample
                if (createSample)
                {
                    outputSample = MediaFactory.CreateSample();

                    outputSample.SampleTime = inputSample.SampleTime;
                    outputSample.SampleDuration = inputSample.SampleDuration;
                    outputSample.SampleFlags = inputSample.SampleFlags;

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

                var res = encoder.TryProcessOutput(TransformProcessOutputFlags.None, outputDataBuffer, out TransformProcessOutputStatus status);
                if (res == SharpDX.Result.Ok)
                {
                    if (outputSample == null)
                    {
                        outputSample = outputDataBuffer[0].PSample;
                    }

                    Debug.Assert(outputSample != null, "res.Success && outputSample != null");

                    Result = true;
                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                {
                    logger.Warn(res.ToString() + " TransformNeedMoreInput");

                    Result = true;
                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                {
                    logger.Warn(res.ToString() + " TransformStreamChange");

                    MediaType newOutputType = null;
                    try
                    {
                        encoder.TryGetOutputAvailableType(outputStreamId, 0, out newOutputType);
                        encoder.SetOutputType(outputStreamId, newOutputType, 0);

                        if (OutputMediaType != null)
                        {
                            OutputMediaType.Dispose();
                            OutputMediaType = null;
                        }
                        OutputMediaType = newOutputType;

                        logger.Info("============== NEW OUTPUT TYPE==================");
                        logger.Info(MfTool.LogMediaType(OutputMediaType));
                    }
                    finally
                    {
                        newOutputType?.Dispose();
                        newOutputType = null;
                    }
                }
                else
                {
                    res.CheckError();
                }

            }

            return Result;
        }

        public void Stop()
        {
            logger.Debug("MfEncoderAsync::Stop()");
            
            if (encoder != null)
            {            

                stopping = true;
              
                encoder.ProcessMessage(TMessageType.CommandDrain, IntPtr.Zero);

                encoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
                
                encoder.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
                encoder.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);

            }

        }

        public void Close()
        {
            logger.Debug("MfEncoderAsync::Close()");

            closing = true;

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

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
                //using (var shutdown = encoder.QueryInterface<Shutdownable>())
                //{
                //    shutdown.Shutdown();
                //    //while(shutdown.ShutdownStatus != ShutdownStatus.Completed)
                //    //{
                //    //    logger.Warn("shutdown.ShutdownStatus " + shutdown.ShutdownStatus);
                //    //    Thread.Sleep(100);
                       
                //    //}
                //}

                encoder.Dispose();
                encoder = null;
            }

            if (bufSample != null)
            {
                bufSample.Dispose();
                bufSample = null;
            }

            if (bufTexture != null)
            {
                bufTexture.Dispose();
                bufTexture = null;
            }


            if (eventHandler != null)
            {
                eventHandler.EventReceived -= HandleMediaEvent;
                eventHandler.Dispose();
                eventHandler = null;
            }

            if (mediaEventGenerator != null)
            {
                mediaEventGenerator.Dispose();
                mediaEventGenerator = null;
            }



            // logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

        }

    }
}