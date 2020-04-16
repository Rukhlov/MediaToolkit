using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;

using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;

using System.IO;
using MediaToolkit.MediaFoundation;
using System.Drawing;
using MediaToolkit.Logging;

namespace MediaToolkit.MediaFoundation
{
    public class MfH264EncoderMS
    {

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        private Transform encoder = null;

        private int inputStreamId = -1;
        private int outputStreamId = -1;

        public MfH264EncoderMS()
        { }

        public MediaType InputMediaType { get; private set; }
        public MediaType OutputMediaType { get; private set; }


        public bool Setup(MfVideoArgs args)
        {

            logger.Debug("MfH264EncoderMS::Setup(...)");

            //var winVersion = Environment.OSVersion.Version;
            //bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 1);

            //if (!isCompatibleOSVersion)
            //{
            //    throw new NotSupportedException("Windows versions earlier than 7 are not supported.");
            //}

            var fps = args.FrameRate;
            var width = args.Width;
            var height = args.Height;

            var inputFormat = args.Format; 

            try
            {
                encoder = new Transform(ClsId.MSH264EncoderMFT);
                using (var attr = encoder.Attributes)
                {
                    //attr.Set(CodecApiPropertyKeys.AVLowLatencyMode, args.LowLatency);
                    attr.Set(CodecApiPropertyKeys.AVEncCommonRateControlMode, args.BitrateMode);
                    attr.Set(CodecApiPropertyKeys.AVEncCommonQuality, args.Quality);

                    var log = MfTool.LogMediaAttributes(attr);

                    logger.Debug(log);
                }

                int inputStreamCount = -1;
                int outputStreamsCount = -1;
                encoder.GetStreamCount(out inputStreamCount, out outputStreamsCount);
                int[] inputStreamIDs = new int[inputStreamCount];
                int[] outputStreamIDs = new int[outputStreamsCount];

                if (encoder.TryGetStreamIDs(inputStreamIDs, outputStreamIDs))
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
                        logger.Warn("NoMoreOutputTypes");
                        break;
                    }
                    //var log = MfTool.LogMediaType(mediaType);
                    //logger.Warn(log);

                    mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    mediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                    mediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));

                    mediaType.Set(MediaTypeAttributeKeys.Mpeg2Profile, (int)args.Profile);

                    mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                    int avgBitrate = args.AvgBitrate * 1000;

                    mediaType.Set(MediaTypeAttributeKeys.AvgBitrate, avgBitrate);

                    int maxBitrate = args.MaxBitrate * 1000;
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

                logger.Debug("MfH264EncoderMS::SetOutputType\r\n" + MfTool.LogMediaType(OutputMediaType));
                encoder.SetOutputType(outputStreamId, OutputMediaType, 0);


                InputMediaType = new MediaType();
                InputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                InputMediaType.Set(MediaTypeAttributeKeys.Subtype, inputFormat);

                InputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                InputMediaType.Set(MediaTypeAttributeKeys.FrameRate, MfTool.PackToLong(fps, 1));
                InputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                InputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);
                //InputMediaType.Set(MediaTypeAttributeKeys.FixedSizeSamples, 1);

                logger.Debug("MfH264EncoderMS::SetInputType\r\n" + MfTool.LogMediaType(InputMediaType));

                encoder.SetInputType(inputStreamId, InputMediaType, 0);



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
            logger.Debug("MfH264EncoderMS::Start()");

            encoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

           // encoder.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);
            // logger.Debug(streamInformation.CbSize);
        }


        public unsafe bool ProcessSample(Sample inputSample, out Sample outputSample)
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
            logger.Debug("MfH264EncoderMS::Stop()");

            encoder.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
            encoder.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
        }


        public void Close()
        {

            logger.Debug("MfH264EncoderMS::Close()");

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

        }

    }
}

