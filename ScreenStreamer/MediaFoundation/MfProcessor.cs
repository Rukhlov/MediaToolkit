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
    public class MfProcessor
    {
        public readonly Guid CLSID_VideoProcessorMFT = new Guid("88753B26-5B24-49BD-B2E7-0C445C78C982");

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Transform processor = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        public Device device = null;
        private DXGIDeviceManager devMan = null;
        public MfProcessor(Device d)
        {
            this.device = d;
        }

        public bool Setup(VideoWriterArgs inputArgs, VideoWriterArgs outputArgs)
        {

            logger.Debug("MfProcessor::Setup(...)");



            var inputWidth = inputArgs.Width;
            var inputHeight = inputArgs.Height;

            //var bufSize = inputWidth * unputHeight * 4;

            try
            {
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

                processor = new Transform(CLSID_VideoProcessorMFT);

                using (var attr = processor.Attributes)
                {
                    bool d3d11Aware = attr.Get(TransformAttributeKeys.D3D11Aware);
                    if (d3d11Aware)
                    {
                        DXGIDeviceManager devMan = new DXGIDeviceManager();
                        devMan.ResetDevice(device);

                        processor.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);
                    }
                }

                //    devMan = new DXGIDeviceManager();
                //devMan.ResetDevice(device);

                //processor.ProcessMessage(TMessageType.SetD3DManager, devMan.NativePointer);

                {

                    int inputStreamCount = -1;
                    int outputStreamsCount = -1;
                    processor.GetStreamCount(out inputStreamCount, out outputStreamsCount);
                    int[] inputStreamIDs = new int[inputStreamCount];
                    int[] outputStreamIDs = new int[outputStreamsCount];

                    bool res = processor.TryGetStreamIDs(inputStreamIDs, outputStreamIDs);
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
                }


                var inputMediaType = new MediaType();
                try
                {
                    inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12);
                    // inputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Argb32);
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(inputWidth, inputHeight));
                    // inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(30, 1));

                    // inputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                    //inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, bufSize);
                    //inputMediaType.Set(MediaTypeAttributeKeys.FixedSizeSamples, 1);
                    //inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(30, 1));

                    processor.SetInputType(inputStreamId, inputMediaType, 0);
                }
                finally
                {
                    inputMediaType.Dispose();
                }



                MediaType outputMediaType = null;
                try
                {
                    for (int i = 0; ; i++)
                    {
                        var res = processor.TryGetOutputAvailableType(0, i, out MediaType mediaType);
                        if (!res)
                        {
                            logger.Warn("NoMoreTypes");
                            break;
                        }

                        var subType = mediaType.Get(MediaTypeAttributeKeys.Subtype);
                        logger.Info(subType);
                        if (subType == VideoFormatGuids.Argb32)//Argb32)//YUY2)//NV12)
                        {
                            outputMediaType = mediaType;
                            break;
                        }
                        mediaType.Dispose();
                    }
                }
                catch (SharpDX.SharpDXException ex)
                {
                    if (ex.ResultCode != SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                    {
                        throw;
                    }
                }

                if (outputMediaType == null)
                {
                    logger.Warn("Format not supported");
                    return false;
                }

                try
                {
                    //outputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    //outputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb24);

                    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(outputArgs.Width, outputArgs.Height));

                    //outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(outputArgs.Width, outputArgs.Height));

                    //outputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                    //outputMediaType.Set(MediaTypeAttributeKeys.FixedSizeSamples, 1);

                    //outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(1, 1));

                    processor.SetOutputType(outputStreamId, outputMediaType, 0);
                }
                finally
                {
                    outputMediaType.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

            return true;
        }


        public void Start()
        {
            logger.Debug("MfProcessor::Start()");


            processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);


            processor.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);

            logger.Debug(streamInformation.CbSize);


            //processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            //processor.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            //processor.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

            //processor.GetOutputStreamInfo(0, out TOutputStreamInformation outputStreamInformation);
            //MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)outputStreamInformation.DwFlags;

            //bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);
            ////MftOutputStreamInformationFlags.MftOutputStreamCanProvideSamples);
            //if (createSample)
            //{
            //    //...
            //}
        }


        public bool ProcessSample(Sample inputSample, out Sample outputSample)
        {
            bool Result = false;
            outputSample = null;

            if (inputSample == null)
            {
                return false;
            }


            // if (processor.OutputStatus == 0)
            {
                try
                {
                    processor.ProcessInput(0, inputSample, 0);

                }
                catch (Exception ex)
                {
                    //processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
                    //return false;
                }
            }

            //if (processor.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
            {

                processor.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

                MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
                bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);

                // Create output sample


                if (createSample)
                {
                    outputSample = MediaFactory.CreateSample();
                    var mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize);

                    outputSample.AddBuffer(mediaBuffer);

                }

                TOutputDataBuffer[] outputDataBuffer = new TOutputDataBuffer[3];

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

                var res = processor.TryProcessOutput(TransformProcessOutputFlags.None, 1, outputDataBuffer, out TransformProcessOutputStatus status);

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
                        processor.TryGetOutputAvailableType(0, 0, out MediaType typeOut);
                        processor.SetOutputType(0, typeOut, 0);

                        logger.Warn(res.ToString() + " TransformStreamChange");
                    }
                    else
                    {
                        res.CheckError();
                    }
                }

            }

            return true;
        }

        public void Stop()
        {
            logger.Debug("Stop()");

            processor.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
        }


        public void Close()
        {
            if (processor != null)
            {
                processor.Dispose();
                processor = null;
            }

            if (devMan != null)
            {
                devMan.Dispose();
                devMan = null;
            }



        }

    }
}
