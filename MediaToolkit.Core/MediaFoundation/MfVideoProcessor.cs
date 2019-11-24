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
using MediaToolkit.MediaFoundation;
using System.Drawing;

namespace MediaToolkit.MediaFoundation
{
    public class MfVideoProcessor
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Transform processor = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        public readonly Device device = null;

        public MfVideoProcessor(Device d)
        {
            this.device = d;
        }

        public MediaType InputMediaType { get; private set; }
        public MediaType OutputMediaType { get; private set; }


        public bool Setup(MfVideoArgs inputArgs, MfVideoArgs outputArgs)
        {

            logger.Debug("MfProcessor::Setup(...)");

            //Debug.Assert(device != null, "device != null");

            try
            {
                //processor = new Transform(CLSID.MJPEGDecoderMFT);
                processor = new Transform(CLSID.VideoProcessorMFT);
                // processor = new Transform(CLSID.CColorConvertDMO);
                if (device != null)
                {
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

                }

                int inputStreamCount = -1;
                int outputStreamsCount = -1;
                processor.GetStreamCount(out inputStreamCount, out outputStreamsCount);
                int[] inputStreamIDs = new int[inputStreamCount];
                int[] outputStreamIDs = new int[outputStreamsCount];

                if (processor.TryGetStreamIDs(inputStreamIDs, outputStreamIDs))
                {
                    inputStreamId = inputStreamIDs[0];
                    outputStreamId = outputStreamIDs[0];
                }
                else
                {
                    inputStreamId = 0;
                    outputStreamId = 0;
                }

                InputMediaType = new MediaType();

                InputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                InputMediaType.Set(MediaTypeAttributeKeys.Subtype, inputArgs.Format); //VideoFormatGuids.NV12 

                InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(inputArgs.Width, inputArgs.Height));
                // InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(30, 1));

                // InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                //InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, bufSize);
                //InputMediaType.Set(MediaTypeAttributeKeys.FixedSizeSamples, 1);
                //InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, PackLong(30, 1));

                processor.SetInputType(inputStreamId, InputMediaType, 0);


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
                        if (subType == outputArgs.Format) //VideoFormatGuids.Argb32)//Argb32)//YUY2)//NV12)
                        {
                            OutputMediaType = mediaType;
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

                if (OutputMediaType == null)
                {
                    logger.Warn("Format not supported");
                    return false;
                }


                //OutputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                //OutputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb24);

                OutputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(outputArgs.Width, outputArgs.Height));

                //OutputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(outputArgs.Width, outputArgs.Height));

                //OutputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                //OutputMediaType.Set(MediaTypeAttributeKeys.FixedSizeSamples, 1);

                processor.SetOutputType(outputStreamId, OutputMediaType, 0);


            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

            return true;
        }

        public void SetMirror(VideoProcessorMirror mirror)
        {
            if (processor != null)
            {
                using (VideoProcessorControl control = processor.QueryInterface<VideoProcessorControl>())
                {
                    control.Mirror = mirror;
                }
            }
  
        }

        public void Start()
        {
            logger.Debug("MfProcessor::Start()");


            processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);


            processor.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);

            logger.Debug(streamInformation.CbSize);
        }


        public unsafe bool ProcessSample(Sample inputSample, out Sample outputSample)
        {
            bool Result = false;
            outputSample = null;

            if (inputSample == null)
            {
                return false;
            }

            processor.ProcessInput(0, inputSample, 0);

            //if (processor.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady)
            {

                processor.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

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

                //bool res = true;
                // processor.ProcessOutput(TransformProcessOutputFlags.None,  1, outputDataBuffer, out TransformProcessOutputStatus status);
                // var res = processor.ProcessOutput(TransformProcessOutputFlags.None,  data, out TransformProcessOutputStatus status);

                //((NAudio.MediaFoundation.IMFTransform)Marshal.GetObjectForIUnknown(processor.NativePointer)).ProcessOutput(0, 1, buf, out _);

                var res = processor.TryProcessOutput(TransformProcessOutputFlags.None,  outputDataBuffer, out TransformProcessOutputStatus status);

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

                        logger.Warn(res.ToString() + " TransformStreamChange");

                        MediaType newOutputType = null;
                        try
                        {
                            processor.TryGetOutputAvailableType(outputStreamId, 0, out newOutputType);
                            processor.SetOutputType(outputStreamId, newOutputType, 0);

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

            }

            return Result;
        }

        public void Stop()
        {
            logger.Debug("MfVideoProcessor::Stop()");

            processor.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
            processor.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
        }


        public void Close()
        {

            logger.Debug("MfVideoProcessor::Close()");

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

            if (processor != null)
            {
                processor.Dispose();
                processor = null;
            }

        }

    }
}
