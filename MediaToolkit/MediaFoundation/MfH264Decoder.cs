using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;
using Device = SharpDX.Direct3D11.Device;
using System.IO;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Logging;

namespace MediaToolkit.MediaFoundation
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/medfound/h-264-video-decoder
    /// </summary>
    public class MfH264Decoder
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        public Device device = null;

        private Transform decoder = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        private long frameNumber = -1;
        private long frameDuration;

        public MfH264Decoder(Device device)
        {
            this.device = device;

        }

        public MediaType InputMediaType { get; private set; }
        public MediaType OutputMediaType { get; private set; }


        public void Setup(MfVideoArgs inputArgs)
        {
            logger.Debug("MfH264Decoder::Setup(...)");


            var frameRate = inputArgs.FrameRate;
            frameDuration = MfTool.TicksPerSecond / frameRate;

            var width = inputArgs.Width;
            var height = inputArgs.Height;
            var bufSize = width * height * 4;

            var inputFormat = VideoFormatGuids.H264;

            try
            {
                //device = new Device(adapter, DeviceCreationFlags.BgraSupport);
                if (device == null)
                {
                    using (var dxgiFactory = new SharpDX.DXGI.Factory1())
                    {
                        using (var adapter = dxgiFactory.GetAdapter1(0))
                        {
                            device = new Device(adapter,
                               //DeviceCreationFlags.Debug |
                               DeviceCreationFlags.VideoSupport |
                               DeviceCreationFlags.BgraSupport);

                            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                            {
                                multiThread.SetMultithreadProtected(true);
                            }
                        }
                    }
                }


                var transformFlags = //TransformEnumFlag.Hardware |
                                     TransformEnumFlag.SortAndFilter;
                var inputType = new TRegisterTypeInformation
                {
                    GuidMajorType = MediaTypeGuids.Video,
                    GuidSubtype = VideoFormatGuids.H264
                };

                var transformActivators = MediaFactory.FindTransform(TransformCategoryGuids.VideoDecoder, transformFlags, inputType, null);
                try
                {
                    foreach (var activator in transformActivators)
                    {

                        //bool isHardware = flags.HasFlag(TransformEnumFlag.Hardware);
                        //bool isAsync = flags.HasFlag(TransformEnumFlag.Asyncmft);

                        string name = activator.Get(TransformAttributeKeys.MftFriendlyNameAttribute);
                        Guid clsid = activator.Get(TransformAttributeKeys.MftTransformClsidAttribute);
                        TransformEnumFlag flags = (TransformEnumFlag)activator.Get(TransformAttributeKeys.TransformFlagsAttribute);


                        bool isAsync = !(flags.HasFlag(TransformEnumFlag.Syncmft));
                        isAsync |= !!(flags.HasFlag(TransformEnumFlag.Asyncmft));
                        bool isHardware = !!(flags.HasFlag(TransformEnumFlag.Hardware));


                        var _flags = Enum.GetValues(typeof(TransformEnumFlag))
                                     .Cast<TransformEnumFlag>()
                                     .Where(m => (m != TransformEnumFlag.None && flags.HasFlag(m)));

                        var transformInfo = name + " " + clsid.ToString() + " " + string.Join("|", _flags);

                        logger.Info(transformInfo);

                        //encoder = activator.ActivateObject<Transform>();
                        //break;

                        //var HardwareUrl = activator.Get(TransformAttributeKeys.MftEnumHardwareUrlAttribute);
                        //logger.Info(HardwareUrl);

                        //var TransformAsync = activator.Get(TransformAttributeKeys.TransformAsync);
                        //logger.Info(TransformAsync);
                        //logger.Info("-------------------------------------");
                    }
                }
                finally
                {
                    decoder = transformActivators[0].ActivateObject<Transform>();

                    foreach (var activator in transformActivators)
                    {
                        activator.Dispose();
                    }

                }


                using (var attr = decoder.Attributes)
                {
                    bool d3dAware = attr.Get(TransformAttributeKeys.D3DAware);

                    bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                    if (d3d11Aware)
                    {
                        using (DXGIDeviceManager devMan = new DXGIDeviceManager())
                        {
                            devMan.ResetDevice(device);
                            decoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                        }

                    }

                    attr.Set(SinkWriterAttributeKeys.LowLatency, true);

                    //attr.Set(MFAttributeKeys.MF_SA_MINIMUM_OUTPUT_SAMPLE_COUNT, 1);
                }


                int inputStreamCount = -1;
                int outputStreamsCount = -1;
                decoder.GetStreamCount(out inputStreamCount, out outputStreamsCount);
                int[] inputStreamIDs = new int[inputStreamCount];
                int[] outputStreamIDs = new int[outputStreamsCount];

                bool res = decoder.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
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
                    try
                    {
                        decoder.GetInputAvailableType(0, i, out MediaType mediaType);

                        if (mediaType == null)
                        {
                            logger.Warn("NoMoreType");
                            break;
                        }
                        
                        var formatId = mediaType.Get(MediaTypeAttributeKeys.Subtype);
                        if (formatId == inputFormat)
                        {
                            logger.Debug("inputFormat " + inputFormat);

                            InputMediaType = mediaType;
                            break;
                        }
                        mediaType.Dispose();
                        mediaType = null;
                        

                    }
                    catch (SharpDX.SharpDXException ex)
                    {
                        if (ex.ResultCode != SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                        {
                            throw;
                        }
                    }
                }

                if (InputMediaType == null)
                {
                    logger.Warn("Unsuported format: " + MfTool.GetMediaTypeName(inputFormat));
                    return;
                }

                InputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                InputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
                InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

               // InputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, MfTool.PackToLong(1, 1));

                InputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                decoder.SetInputType(inputStreamId, InputMediaType, 0);

                logger.Info("============== INPUT TYPE==================");
                logger.Info(MfTool.LogMediaType(InputMediaType));



                //MediaType outputMediaType = null;
                //for (int i = 0; ; i++)
                //{
                //    res = decoder.TryGetOutputAvailableType(outputStreamId, i, out outputMediaType);

                //    if (!res)
                //    {
                //        break;
                //    }

                //    //outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);

                //    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                //    outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(frameRate, 1));

                //    //outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                //    //outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                //    decoder.SetOutputType(outputStreamId, outputMediaType, 0);


                //    outputMediaType.Dispose();
                //    outputMediaType = null;
                //    break;
                //}

                OutputMediaType = new MediaType();

                OutputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                OutputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
                OutputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);
                OutputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                OutputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                OutputMediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

                OutputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                decoder.SetOutputType(outputStreamId, OutputMediaType, 0);

                logger.Info("============== OUTPUT TYPE==================");
                logger.Info(MfTool.LogMediaType(OutputMediaType));


            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }


        public void Start()
        {
            logger.Debug("MfH264Decoder::Start()");

            decoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            decoder.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            decoder.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);


            decoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);

            //logger.Debug(streamInformation.CbSize);


        }


        public bool ProcessSample(Sample inputSample, Action<Sample> OnSampleDecoded)
        {
 
            bool Result = false;


            if (inputSample == null)
            {
                return false;
            }

            //FIXME:
            frameNumber++;
            inputSample.SampleTime = frameNumber * frameDuration; //!!!!!!!!!1
            inputSample.SampleDuration = frameDuration;

            decoder.ProcessInput(0, inputSample, 0);

            //if (decoder.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
            {

                decoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

                MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
                bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

                do
                {
                    Sample pSample = null;
                    // Create output sample
                    if (createSample)
                    {
                        pSample = MediaFactory.CreateSample();
                        pSample.SampleTime = inputSample.SampleTime;
                        pSample.SampleDuration = inputSample.SampleDuration;
                        pSample.SampleFlags = inputSample.SampleFlags;
                        logger.Debug("CreateSample: " + inputSample.SampleTime);

                        using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize))
                        {
                            pSample.AddBuffer(mediaBuffer);
                        }
                    }

                    TOutputDataBuffer[] outputBuffers = new TOutputDataBuffer[1];

                    var outputBuffer = new TOutputDataBuffer
                    {
                        DwStatus = 0,
                        DwStreamID = 0,
                        PSample = pSample,
                        PEvents = null,
                    };
                    outputBuffers[0] = outputBuffer;


                    var res = decoder.TryProcessOutput(TransformProcessOutputFlags.None, outputBuffers, out TransformProcessOutputStatus status);

                    //logger.Info("TryProcessOutput(...) " + res + " " + outputDataBuffer[0].DwStatus);

                    if (res == SharpDX.Result.Ok)
                    {
                        var buf = outputBuffers[0];

                        var outputSample = buf.PSample;

                        var pEvents = buf.PEvents;
                        if (pEvents != null)
                        {
                            var eventsCount = pEvents.ElementCount;

                            Debug.Assert(eventsCount == 0, "eventsCount == 0");

                            if (eventsCount > 0)
                            {
                                for (int i = 0; i < eventsCount; i++)
                                {
                                    var e = pEvents.GetElement(i);
                                    if (e != null)
                                    {
                                        e.Dispose();
                                        e = null;
                                    }

                                }

                            }
                        }


                        Debug.Assert(outputSample != null, "res.Success && outputSample != null");

                        OnSampleDecoded?.Invoke(outputSample);
                        Result = true;
                        //continue;
                    }
                    else if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                    {
                        //logger.Info("-------------------------");
                        Result = true;

                        break;
                    }
                    else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                    {
                        logger.Warn(res.ToString() + " TransformStreamChange");

                        MediaType newOutputType = null;
                        try
                        {
                            decoder.TryGetOutputAvailableType(outputStreamId, 0, out newOutputType);
                            decoder.SetOutputType(outputStreamId, newOutputType, 0);

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

                        Result = false;
                        break;
                    }

                    //if (outputSample != null)
                    //{
                    //   // logger.Debug("DisposeSample: " + outputSample?.SampleTime);
                    //    outputSample?.Dispose();
                    //    outputSample = null;
                    //}


                }
                while (true);

            }

            return Result;
        }


        public void Stop()
        {
            logger.Debug("MfH264Decoder::Stop()");

            if (decoder != null)
            {
                decoder.ProcessMessage(TMessageType.CommandDrain, IntPtr.Zero);
                decoder.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
                decoder.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
                decoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);

            }

        }

        public void Close()
        {
            logger.Debug("MfH264Decoder::Close()");

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

            if (decoder != null)
            {
                decoder.Dispose();
                decoder = null;
            }

        }

    }

}
