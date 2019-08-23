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
using ScreenStreamer.MediaFoundation;

namespace ScreenStreamer.MediaFoundation
{

    public class MfH264Decoder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Device device = null;

        private Transform decoder = null;
        private MediaEventGenerator eventGen = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        private long frameNumber = -1;
        private long frameDuration;

        public MfH264Decoder(Device device)
        {
            this.device = device;

        }

  
        private Texture2D bufTexture = null;
        public void Setup(VideoWriterArgs Args)
        {
            logger.Debug("MfH264Decoder::Setup(...)");


            var frameRate = Args.FrameRate;
            frameDuration = 10_000_000 / Args.FrameRate;

            var width = Args.Width;
            var height = Args.Height;
            var bufSize = width * height * 4;

            var inputFormat = VideoFormatGuids.H264;

            try
            {
                //device = new Device(adapter, DeviceCreationFlags.BgraSupport);
                if (device == null)
                {
                    var dxgiFactory = new SharpDX.DXGI.Factory1();
                    var adapter = dxgiFactory.Adapters1[0];

                    device = new Device(adapter,
                        DeviceCreationFlags.Debug |
                        DeviceCreationFlags.VideoSupport |
                        DeviceCreationFlags.BgraSupport);

                    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
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
                    bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                    if (d3d11Aware)
                    {
                        DXGIDeviceManager devMan = new DXGIDeviceManager();
                        devMan.ResetDevice(device);

                        decoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                    }

                    attr.Set(SinkWriterAttributeKeys.LowLatency, true);
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

                //encoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);




                MediaType inputMediaType = null;
                try
                {
                    for (int i = 0; ; i++)
                    {
                        try
                        {
                            decoder.GetInputAvailableType(0, i, out inputMediaType);

                            var formatId = inputMediaType.Get(MediaTypeAttributeKeys.Subtype);
                            if (formatId == inputFormat)
                            {
                                logger.Debug("inputFormat " + inputFormat);
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
                        }
                    }

                    if (inputMediaType == null)
                    {
                        logger.Warn("Unsuported format");
                        return;
                    }

                    inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(frameRate, 1));

                    inputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    inputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                    decoder.SetInputType(inputStreamId, inputMediaType, 0);

                    logger.Info("============== INPUT ==================");
                    logger.Info(MfTool.LogMediaType(inputMediaType));
                }
                finally
                {
                    inputMediaType?.Dispose();
                }


                //MediaType outputMediaType = null;
                //for (int i = 0; ; i++)
                //{
                //    res = decoder.TryGetOutputAvailableType(outputStreamId, i, out outputMediaType);

                //    if (!res)
                //    {
                //        break;
                //    }

                //    //outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);

                //    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, PackLong(width, height));
                //    outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(frameRate, 1));

                //    //outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                //    //outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                //    decoder.SetOutputType(outputStreamId, outputMediaType, 0);


                //    outputMediaType.Dispose();
                //    outputMediaType = null;
                //    break;
                //}

                var outputMediaType = new MediaType();
                try
                {
                    outputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    outputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
                    //outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);
                    //outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(frameRate, 1));

                    // outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                    decoder.SetOutputType(outputStreamId, outputMediaType, 0);

                    logger.Info("============== OUTPUT ==================");
                    logger.Info(MfTool.LogMediaType(outputMediaType));
                }
                finally
                {
                    outputMediaType?.Dispose();
                }


                //TOutputStreamInformation sinfo;
                //transform.GetOutputStreamInfo(0, out sinfo);

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

            logger.Debug(streamInformation.CbSize);


        }



        public bool ProcessSample(Sample inputSample, out Sample outputSample)
        {
            

            bool Result = false;
            outputSample = null;

            if (inputSample == null)
            {
                return false;
            }

            frameNumber++;
            inputSample.SampleTime = frameNumber * frameDuration;
            inputSample.SampleDuration = frameDuration;

            //decoder.ProcessInput(0, bufSample, 0);

           // logger.Debug("ProcessInput ");

            decoder.ProcessInput(0, inputSample, 0);


            //if (decoder.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
            {

                decoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

                MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
                bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

                // Create output sample


                if (createSample)
                {
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

                //logger.Debug("TryProcessOutput BEGIN");

                var res = decoder.TryProcessOutput(TransformProcessOutputFlags.None, 1, outputDataBuffer, out TransformProcessOutputStatus status);

               

                //var res = decoder.ProcessOutput(TransformProcessOutputFlags.None, data, out TransformProcessOutputStatus status);

                //logger.Debug("TryProcessOutput END " + res);
                if (res.Success)
                {


                    if (outputSample == null)
                    {
                        outputSample = outputDataBuffer[0].PSample;
                    }

                    Debug.Assert(outputSample != null, "res.Success && outputSample != null");

                    Result = true;
                }
                else
                {
                    if (res == SharpDX.MediaFoundation.ResultCode.TransformNeedMoreInput)
                    {
                        logger.Warn(res.ToString() + " TransformNeedMoreInput");

                        Result = true;

                    }
                    else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                    {
                        //...
                        decoder.TryGetOutputAvailableType(outputStreamId, 0, out MediaType typeOut);
                        decoder.SetOutputType(outputStreamId, typeOut, 0);

                        logger.Warn(res.ToString() + " TransformStreamChange");

                        logger.Info("============== NEW OUTPUT ==================");
                        logger.Info(MfTool.LogMediaType(typeOut));

                    }
                    else
                    {
                        res.CheckError();
                    }
                }

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

            if (decoder != null)
            {
                decoder.Dispose();
                decoder = null;
            }

            if (eventGen != null)
            {
                eventGen.Dispose();
                eventGen = null;
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


        /// <summary>
        /// https://github.com/tpn/winsdk-10/blob/master/Include/10.0.10240.0/um/codecapi.h
        /// https://docs.microsoft.com/en-us/windows/win32/medfound/h-264-video-encoder
        /// </summary>
        class CodecApiAttributeKeys
        {
            //#define STATIC_CODECAPI_AVEncNumWorkerThreads   0xb0c8bf60, 0x16f7, 0x4951, 0xa3, 0xb, 0x1d, 0xb1, 0x60, 0x92, 0x93, 0xd6
            public readonly MediaAttributeKey<int> AVEncNumWorkerThreads = new MediaAttributeKey<int>(new Guid(0xb0c8bf60, 0x16f7, 0x4951, 0xa3, 0xb, 0x1d, 0xb1, 0x60, 0x92, 0x93, 0xd6));

            //#define STATIC_CODECAPI_AVLowLatencyMode  0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee
            public readonly MediaAttributeKey<bool> AVLowLatencyMode = new MediaAttributeKey<bool>(new Guid(0x9c27891a, 0xed7a, 0x40e1, 0x88, 0xe8, 0xb2, 0x27, 0x27, 0xa0, 0x24, 0xee));


        }


    }

}
