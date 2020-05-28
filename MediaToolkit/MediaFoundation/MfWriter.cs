using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;


using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.MediaFoundation;
using Device = SharpDX.Direct3D11.Device;
using MediaToolkit.Logging;

namespace MediaToolkit.MediaFoundation
{
    public class MfWriter
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        public readonly Device device = null;

        private SinkWriter sinkWriter;
 
        private Texture2D bufTexture = null;
        private Sample videoSample = null;
        private MediaBuffer mediaBuffer = null;

        private int videoStreamIndex = -1;
        private long frameNumber = -1;
        private long frameDuration;

        // Keep this separate. First frame might be a RepeatFrame.
        bool isFirstFrame = true;

        public MfWriter(Device device)
        {
            this.device = device;

        }

        public void Setup(string fileName, MfVideoArgs Args)
        {
            logger.Debug("MfWriter::Init(...)");

            var inputFormat = VideoFormatGuids.NV12;
            //  var inputFormat = VideoFormatGuids.Rgb32; // VideoFormatGuids.NV12


            frameDuration = 10_000_000 / Args.FrameRate;

            var width = Args.Width;
            var height = Args.Height;
            var bufSize = width * height * 4;


            try
            {
                using (var attr = new MediaAttributes(6))
                {
                    attr.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);
                    attr.Set(SinkWriterAttributeKeys.ReadwriteDisableConverters, 0);
                    attr.Set(TranscodeAttributeKeys.TranscodeContainertype, TranscodeContainerTypeGuids.Mpeg4);
                    attr.Set(SinkWriterAttributeKeys.LowLatency, true);
                    attr.Set(SinkWriterAttributeKeys.DisableThrottling, 1);

                    using (var devMan = new DXGIDeviceManager())
                    {
                        devMan.ResetDevice(device);
                        attr.Set(SinkWriterAttributeKeys.D3DManager, devMan);
                    }

                    sinkWriter = MediaFactory.CreateSinkWriterFromURL(fileName, null, attr);
                }

                using (var outputMediaType = new MediaType())
                {
                    outputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    outputMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.H264);
                    outputMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 8_000_000);
                    outputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                    outputMediaType.Set(MediaTypeAttributeKeys.FrameRate, Args.FrameRate);
                    outputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, MfTool.PackToLong(1, 1));


                    sinkWriter.AddStream(outputMediaType, out videoStreamIndex);

                    Debug.WriteLine("mediaTypeOut " + videoStreamIndex);
                }


                using (var inputMediaType= new MediaType())
                {
                    inputMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
                    inputMediaType.Set(MediaTypeAttributeKeys.Subtype, inputFormat);
                    inputMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int)VideoInterlaceMode.Progressive);
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameSize, MfTool.PackToLong(width, height));
                    inputMediaType.Set(MediaTypeAttributeKeys.FrameRate, Args.FrameRate);
                    inputMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, MfTool.PackToLong(1, 1));

                    inputMediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

                    using (var encoderParams = new MediaAttributes(2))
                    {                      
                        encoderParams.Set(CodecApiPropertyKeys.AVEncCommonRateControlMode, RateControlMode.Quality);
                        encoderParams.Set(CodecApiPropertyKeys.AVEncCommonQuality, Args.Quality);

                        sinkWriter.SetInputMediaType(0, inputMediaType, encoderParams);
                    }
                }

                bufTexture = new Texture2D(device, new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = width,
                    Height = height,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging
                });

                videoSample = MediaFactory.CreateVideoSampleFromSurface(null);

                // Create the media buffer from the texture
                MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);

                using (var buffer2D = mediaBuffer.QueryInterface<Buffer2D>())
                {
                    mediaBuffer.CurrentLength = buffer2D.ContiguousLength;
                }

                // Attach the created buffer to the sample
                videoSample.AddBuffer(mediaBuffer);
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }

        public void Start()
        {
            logger.Debug("MfWriter::Start()");

            sinkWriter.BeginWriting();
        }

        public void WriteTexture(Texture2D texture)
        {
            if (closed)
            {
                return;
            }

            device.ImmediateContext.CopyResource(texture, bufTexture);

            WriteSample(videoSample);
        }


        private readonly object syncLock = new object();
        public void WriteSample(Sample sample)
        {
            if (closed)
            {
                return;
            }

            frameNumber++;
            sample.SampleTime = frameNumber * frameDuration;
            sample.SampleDuration = frameDuration;

            if (isFirstFrame)
            {
                logger.Verb("MfWriter::isFirstFrame");
                //sinkWriter.BeginWriting();

                sinkWriter.SendStreamTick(videoStreamIndex, sample.SampleTime);
                sample.Set(SampleAttributeKeys.Discontinuity, true);
                isFirstFrame = false;
            }

            sinkWriter.WriteSample(videoStreamIndex, sample);
            //sinkWriter.Flush(VideoStreamIndex);
            
        }

        public void Stop()
        {
            logger.Debug("MfWriter::Stop()");

            if (sinkWriter != null)
            {
                sinkWriter.Finalize();
            }
        }

        private volatile bool closed = false;

        public void Close()
        {
            logger.Debug("MfWriter::Close()");

            closed = true;
            if (sinkWriter != null)
            {
                sinkWriter.Dispose();
                sinkWriter = null;
            }

            if (bufTexture != null)
            {
                bufTexture.Dispose();
                bufTexture = null;
            }

            if (videoSample != null)
            {
                videoSample.Dispose();
                videoSample = null;
            }

            if (mediaBuffer != null)
            {
                mediaBuffer.Dispose();
                mediaBuffer = null;
            }
            
        }

    }



}