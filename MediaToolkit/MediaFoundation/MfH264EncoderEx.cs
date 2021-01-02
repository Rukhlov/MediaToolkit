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

    public class MfH264EncoderEx : IMfVideoTransform
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

        public MfH264EncoderEx(SharpDX.Direct3D11.Device d)
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
                int adapterVenId = -1;
                using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
                {
                    using (var adapter = dxgiDevice.Adapter)
                    {
                        adapterVenId = adapter.Description.VendorId;
                    }
                }

                //encoder = new Transform(ClsId.MSH264EncoderMFT);
                //syncMode = true;

                var encoderId = args.EncoderId;
                Guid.TryParse(encoderId, out var clsId);
                if(clsId != Guid.Empty)
                {
                    encoder = new Transform(clsId);

                }
                else
                {
                    encoder = FindEncoder(adapterVenId);
                    //syncMode = false;
                }


                SetupSampleBuffer(args);

  
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

            if (format == Format.Unknown)
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


            var width = args.Width;
            var height = args.Height;

            var frameRate = args.FrameRate;
            int avgBitrate = args.AvgBitrate;
            int maxBitrate = args.MaxBitrate;
            int mpegProfile = (int)args.Profile;

			long pixelAspectRatio = args.AspectRatio;

            //var inputFormat = VideoFormatGuids.Argb32;//VideoFormatGuids.NV12;
            var inputFormat = args.Format;

            var ratio = MfTool.UnPackLongToInts(frameRate);
            var fps = ratio[0] / (double)ratio[1];

            logger.Info("Encoder input params: " + width + "x" + height + " fps=" + fps + " {" + inputFormat + "}");
            using (var attr = encoder.Attributes)
            {
                // TODO:
                // log pref

                try
                {
                    syncMode = !(attr.Get(TransformAttributeKeys.TransformAsync) == 1);
                }
                catch (SharpDX.SharpDXException ex)
                {
                    syncMode = true;
                }
                

                if (!syncMode)
                {
                    //var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
                    //if (transformAsync)
                    //{
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
                    //}
                }

                attr.Set(CodecApiPropertyKeys.AVLowLatencyMode, args.LowLatency);
                attr.Set(CodecApiPropertyKeys.AVEncCommonRateControlMode, args.BitrateMode);
                attr.Set(CodecApiPropertyKeys.AVEncCommonQuality, args.Quality);

                attr.Set(CodecApiPropertyKeys.AVEncMPVGOPSize, args.GopSize);

                // отключаем B-фреймы
                attr.Set(CodecApiPropertyKeys.AVEncMPVDefaultBPictureCount, 0);


                //attr.Set(CodecApiPropertyKeys.AVEncNumWorkerThreads, 4);

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
                mediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);
                mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                mediaType.Set(MediaTypeAttributeKeys.Mpeg2Profile, mpegProfile);
                mediaType.Set(MediaTypeAttributeKeys.AvgBitrate, avgBitrate);
				mediaType.Set(CodecApiPropertyKeys.AVEncCommonMaxBitRate, maxBitrate);

				mediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, pixelAspectRatio);

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
                        break;
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
            InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

            InputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
            InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
            encoder.SetInputType(inputStreamId, InputMediaType, 0);

            var mediaLog = MfTool.LogMediaType(InputMediaType);
            logger.Debug("\r\nInputMediaType:\r\n-----------------\r\n" + mediaLog);


            if (!syncMode)
            {
                mediaEventGenerator = encoder.QueryInterface<MediaEventGenerator>();
                eventHandler = new MediaEventHandler(mediaEventGenerator);
                eventHandler.EventReceived += HandleMediaEvent;
            }



            //encoder.GetInputStreamInfo(0, out TInputStreamInformation inputStreamInfo);
            //var inputInfoFlags = (MftInputStreamInformationFlags)inputStreamInfo.DwFlags;
            //logger.Debug(MfTool.LogEnumFlags(inputInfoFlags));

            //encoder.GetOutputStreamInfo(0, out TOutputStreamInformation outputStreamInfo);
            //var outputInfoFlags = (MftOutputStreamInformationFlags)outputStreamInfo.DwFlags;
            //logger.Debug(MfTool.LogEnumFlags(outputInfoFlags));


            //var guid = new Guid("901db4c7-31ce-41a2-85dc-8fa0bf41b8da");
            //encoder.QueryInterface(guid, out var pUnk);

            //var codecApi = (NativeAPIs.DShow.ICodecAPI)Marshal.GetObjectForIUnknown(pUnk);
            ////var code = codecApi.GetParameterRange(CodecApiPropertyKeys.AVEncMPVDefaultBPictureCount.Guid, out var valMin, out var valMax, out var sDelta);
            //var code = codecApi.GetValue(CodecApiPropertyKeys.AVEncMPVDefaultBPictureCount.Guid, out var val);

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

            //if (!syncMode)
            {
                StartEncoderProc();
            }

        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private void StartEncoderProc()
        {
            Task.Run(() =>
            {
                logger.Debug("DoEncode() BEGIN");
                try
                {
                    while (!closing)
                    {
                        if (!syncMode)
                        {
                            while (inputRequests > 0 && needUpdate)
                            {
                                lock (syncRoot)
                                {
                                    encoder.ProcessInput(inputStreamId, bufSample, 0);
                                    inputRequests--;
                                    needUpdate = false;
                                }

                                if (closing)
                                {
                                    break;
                                }
                            }

                            while (outputRequests > 0)
                            {
                                ProcessOutput();
                                lock (syncRoot)
                                {
                                    outputRequests--;
                                }

                                if (closing)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (needUpdate)
                            {
                                encoder.ProcessInput(inputStreamId, bufSample, 0);

                                ProcessOutput();
                            }

                        }
                        syncEvent.WaitOne(1000);

                    }
                }
                finally
                {
                    Close();
                }


                logger.Debug("DoEncode() END");

            });
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
                Interlocked.Increment(ref inputRequests);
                syncEvent.Set();

                //logger.Debug("inputRequests " + inputRequests);
            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformHaveOutput)
            {

                Interlocked.Increment(ref outputRequests);
                syncEvent.Set();

            }
            else if (mediaEvent.TypeInfo == MediaEventTypes.TransformDrainComplete)
            {
                logger.Warn("_MediaEventTypes.TransformDrainComplete ");

                closing = true;
                //Close();
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
                    FinalizeSample(outputSample);
                   // SampleReady?.Invoke(outputSample);

                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                {
                    //logger.Debug(res.ToString() + " TransformNeedMoreInput");
                    //Result = true;
                }
                else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                {// не должны приходить для энкодера...

                    // но приходят для Intel MFT !!!
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
            finally
            {
                if (outputSample != null)
                {
                    outputSample.Dispose();
                    outputSample = null;
                }
            }

        }

        public event Action<IntPtr, int, double> DataEncoded;
        private void FinalizeSample(Sample encodedSample)
        {
            using (var buffer = encodedSample.ConvertToContiguousBuffer())
            {
                var ptr = buffer.Lock(out int cbMaxLength, out int cbCurrentLength);
                try
                {
                    if (cbCurrentLength > 0)
                    {
                        var sampleTime = encodedSample.SampleTime;
                        var timeSec = MfTool.MfTicksToSec(sampleTime);
                        DataEncoded?.Invoke(ptr, cbCurrentLength, timeSec);

                        //byte[] buf = new byte[cbCurrentLength];
                        //Marshal.Copy(ptr, buf, 0, buf.Length);

                        //OnDataReady(buf);
                    }
                }
                finally
                {
                    buffer.Unlock();
                }
            }
        }


        public bool ProcessSample(Sample sample)
        {

			bool Result = false;
            if (stopping)
            {
                logger.Warn("ProcessSample(...) stopping");
                return Result;
            }

            if (closing)
            {
                logger.Warn("ProcessSample(...) closing");
                return Result;
            }

			try
			{
				EncodeSampleAsync(sample);
				Result = true;
			}
			catch(Exception ex)
			{
				Result = false;
				logger.Error(ex);
			}
            

            return Result;
        }

        private void EncodeSampleAsync(Sample sample)
        {

            using (var buffer = sample.ConvertToContiguousBuffer())
            {
                using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                {
                    dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                    using (Texture2D texture = new Texture2D(intPtr))
                    {
                        lock (syncRoot)
                        {
                            bufSample.SampleTime = sample.SampleTime;
                            bufSample.SampleDuration = sample.SampleDuration;
                            bufSample.SampleFlags = sample.SampleFlags;

                            device.ImmediateContext.CopyResource(texture, bufTexture);
                            needUpdate = true;
                            // ProcessInput();

                        }
                        syncEvent.Set();

                    };
                }
            }
        }


        public bool EncodeSample(Sample inputSample, out Sample outputSample)
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

                if (syncMode)
                {
                    Drain();
                    Close();
                }
            }

        }

        private void Drain()
        {
            encoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

            MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
            bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

            bool drain = true;
            do
            {
                Sample outputSample = null;
                try
                {
                    if (createSample)
                    {
                        outputSample = MediaFactory.CreateSample();
                        outputSample.SampleTime = 0;
                        outputSample.SampleDuration = 0;

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

                    var res = encoder.TryProcessOutput(TransformProcessOutputFlags.None, outputDataBuffer, out var status);

                    drain = res.Success; //(res != SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput);
                }
                finally
                {

                    outputSample?.Dispose();
                }

            }
            while (drain);
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