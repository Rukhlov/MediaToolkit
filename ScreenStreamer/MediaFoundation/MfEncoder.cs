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

        public Device device = null;

        private Transform encoder = null;

        private Texture2D texture = null;
        private Sample bufSample = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        private long frameNumber = -1;
        private long frameDuration;

        public MfEncoderAsync(Device device)
        {
            this.device = device;

        }


        public void Setup(VideoWriterArgs Args)
        {
            logger.Debug("MfEncoderAsync::Setup(...)");


            var frameRate = Args.FrameRate;
            frameDuration = 10_000_000 / Args.FrameRate;

            var width = Args.Width;
            var height = Args.Height;

            var inputFormat = VideoFormatGuids.Argb32;

            try
            {
                //device = new Device(SharpDX.Direct3D.DriverType.Hardware,
                //    DeviceCreationFlags.Debug | DeviceCreationFlags.VideoSupport | DeviceCreationFlags.BgraSupport);


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
                    //MediaBuffer mediaBuffer = MediaFactory.CreateMemoryBuffer(4 * width * height);
                    MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);
                    bufSample = MediaFactory.CreateSample();
                    bufSample.AddBuffer(mediaBuffer);
                }
                finally
                {
                    mediaBuffer.Dispose();
                }


                var transformFlags = TransformEnumFlag.Hardware |
                                     TransformEnumFlag.SortAndFilter;

                var outputType = new TRegisterTypeInformation
                {
                    GuidMajorType = MediaTypeGuids.Video,
                    GuidSubtype = VideoFormatGuids.H264
                };

                var transformActivators = MediaFactory.FindTransform(TransformCategoryGuids.VideoEncoder, transformFlags, null, outputType);
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

                        encoder = activator.ActivateObject<Transform>();
                        break;

                        //var HardwareUrl = activator.Get(TransformAttributeKeys.MftEnumHardwareUrlAttribute);
                        //logger.Info(HardwareUrl);

                        //var TransformAsync = activator.Get(TransformAttributeKeys.TransformAsync);
                        //logger.Info(TransformAsync);
                        //logger.Info("-------------------------------------");
                    }
                }
                finally
                {
                    foreach (var activator in transformActivators)
                    {
                        activator.Dispose();
                    }

                }


                using (var attr = encoder.Attributes)
                {
                    var transformAsync = (attr.Get(TransformAttributeKeys.TransformAsync) == 1);
                    if (transformAsync)
                    {
                        attr.Set(TransformAttributeKeys.TransformAsyncUnlock, 1);
                        attr.Set(TransformAttributeKeys.MftSupportDynamicFormatChange, true);
                        attr.Set(SinkWriterAttributeKeys.LowLatency, true);


                        //attr.Set(CODECAPI_AVEncNumWorkerThreads, 8);


                        //attr.Set(SinkWriterAttributeKeys.DisableThrottling, 1);

                        // attr.Set(SinkWriterAttributeKeys.D3DManager, devMan);

                        using (DXGIDeviceManager devMan = new DXGIDeviceManager())
                        {
                           
                            devMan.ResetDevice(device);
                            encoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                        }

                    }
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

                //encoder.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);


                var outputMediaType = new MediaType();
                try
                {
                    outputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    outputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
                    outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 30000000);
                    outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, Packer.ToLong(width, height));
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, Packer.ToLong(frameRate, 1));

                    outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                    encoder.SetOutputType(outputStreamId, outputMediaType, 0);
                }
                finally
                {
                    outputMediaType?.Dispose();
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

                    //inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    //inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, Packer.ToLong(width, height));
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, Packer.ToLong(frameRate, 1));

                    inputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    inputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                    encoder.SetInputType(inputStreamId, inputMediaType, 0);
                }
                finally
                {
                    inputMediaType?.Dispose();
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
                        mediaEvent?.Dispose();
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
                            byte[] buf = new byte[cbCurrentLength];
                            Marshal.Copy(ptr, buf, 0, buf.Length);

                            OnDataReady(buf);
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

                    outputSample?.Dispose();

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

                device.ImmediateContext.CopyResource(texture, bufTexture);
                needUpdate = true;

                //bufSample = MediaFactory.CreateVideoSampleFromSurface(bufTexture);

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
            if (encoder != null)
            {
                encoder.Dispose();
                encoder = null;
            }

            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }

            if (bufSample != null)
            {
                bufSample.Dispose();
                bufSample = null;
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


        //public static long PackLong(int Left, int Right)
        //{
        //    return new PackedLong
        //    {
        //        Low = Right,
        //        High = Left
        //    }.Long;
        //}

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