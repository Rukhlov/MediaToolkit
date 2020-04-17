using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;

namespace MediaToolkit.MediaFoundation
{
    public class VideoEncoder
    {

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        private readonly IVideoSource videoSource = null;

        public VideoEncoder(IVideoSource source)
        {
            this.videoSource = source;
        }

        private MfH264Encoder encoder = null;

        private MfVideoProcessor processor = null;

        private Texture2D bufTexture = null;


        public void Open(VideoEncoderSettings destParams)
        {
            logger.Debug("VideoEncoder::Setup(...)");

            //var hwContext = videoSource.hwContext;
            // var hwDevice = hwContext.Device3D11;

            var hwBuffer = videoSource.SharedTexture;

            var hwDescr = hwBuffer.Description;

            var srcSize = new Size(hwDescr.Width, hwDescr.Height);
            var srcFormat = MfTool.GetVideoFormatGuidFromDXGIFormat(hwDescr.Format);


            var destSize = destParams.Resolution;//new Size(destParams.Width, destParams.Height);

            //var hwDevice = hwBuffer.Device;
            //long adapterLuid = -1;
            //using (var dxgiDevice = hwDevice.QueryInterface<SharpDX.DXGI.Device>())
            //{
            //    using (var adapter = dxgiDevice.Adapter)
            //    {
            //        adapterLuid = adapter.Description.Luid;
            //    }
            //}
            //var refCount = ((IUnknown)hwDevice).Release();

            var profile = eAVEncH264VProfile.Main;
            if (destParams.Profile == H264Profile.High)
            {
                profile = eAVEncH264VProfile.High;
            }
            else if (destParams.Profile == H264Profile.Base)
            {
                profile = eAVEncH264VProfile.Base;
            }

            var bitrateMode = RateControlMode.CBR;
            if (destParams.BitrateMode == BitrateControlMode.VBR)
            {
                bitrateMode = RateControlMode.LowDelayVBR;
            }
            else if (destParams.BitrateMode == BitrateControlMode.Quality)
            {
                bitrateMode = RateControlMode.Quality;
            }


            var encArgs = new MfVideoArgs
            {
                Width = destSize.Width, //srcSize.Width,
                Height = destSize.Height, //srcSize.Height,
                Format = VideoFormatGuids.NV12,//VideoFormatGuids.Argb32,

                FrameRate = destParams.FrameRate,
                AvgBitrate = destParams.Bitrate,
                LowLatency = destParams.LowLatency,
                AdapterId = videoSource.AdapterId,
                Profile = profile,
                BitrateMode = bitrateMode,
                MaxBitrate = destParams.MaxBitrate,

            };

            encoder = new MfH264Encoder();
            encoder.Setup(encArgs);

            var encDevice = encoder?.device;

            processor = new MfVideoProcessor(encDevice);
            var inProcArgs = new MfVideoArgs
            {
                Width = srcSize.Width,
                Height = srcSize.Height,
                Format = srcFormat, //SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
            };

            var outProcArgs = new MfVideoArgs
            {
                Width = encArgs.Width,
                Height = encArgs.Height,
                Format = encArgs.Format, //VideoFormatGuids.NV12,//.Argb32,
            };

            processor.Setup(inProcArgs, outProcArgs);


            bufTexture = new Texture2D(encDevice,
                new Texture2DDescription
                {
                    // Format = Format.NV12,
                    Format = hwDescr.Format,//SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1 },
                });

            processor?.Start();


            encoder.SampleReady += Encoder_SampleReady;
            //encoder.DataReady += MfEncoder_DataReady;
            encoder.Start();

        }



        public void Encode()
        {
            var texture = videoSource?.SharedTexture;

            Encode(texture);
        }

        private static Guid uuidTexture2d = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));

        public void Encode(Texture2D texture)
        {
            var device = encoder?.device;

            if (device != null)
            {
                using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
                {
                    using (var sharedTexture = device.OpenSharedResource<Texture2D>(sharedRes.SharedHandle))
                    {
                        device.ImmediateContext.CopyResource(sharedTexture, bufTexture);

                    }
                }

            }

            Sample inputSample = null;
            try
            {
                MediaBuffer mediaBuffer = null;
                try
                {
                    MediaFactory.CreateDXGISurfaceBuffer(uuidTexture2d, bufTexture, 0, false, out mediaBuffer);
                    inputSample = MediaFactory.CreateSample();
                    inputSample.AddBuffer(mediaBuffer);

                    inputSample.SampleTime = 0;
                    inputSample.SampleDuration = 0;
                }
                finally
                {
                    mediaBuffer?.Dispose();
                }

                if (processor != null)
                {
                    Sample processedSample = null;
                    try
                    {
                        bool result = processor.ProcessSample(inputSample, out processedSample);
                        if (result)
                        {
                            EncodeSample(processedSample);
                        }
                    }
                    finally
                    {
                        processedSample?.Dispose();
                    }
                }
                else
                {
                    EncodeSample(inputSample);
                }


            }
            finally
            {
                inputSample?.Dispose();
            }



        }

        private void EncodeSample(Sample sample)
        {
            Sample encodedSample = null;
            try
            {
                encodedSample = encoder.ProcessSample(sample);
                if (encodedSample != null)
                {
                    FinalizeSample(encodedSample);
                }

            }
            finally
            {
                if (encodedSample != null)
                {
                    encodedSample.Dispose();
                    encodedSample = null;
                }
            }
        }

        private void FinalizeSample(Sample encodedSample)
        {
            using (var buffer = encodedSample.ConvertToContiguousBuffer())
            {
                var ptr = buffer.Lock(out int cbMaxLength, out int cbCurrentLength);
                try
                {
                    if (cbCurrentLength > 0)
                    {
                        byte[] buf = new byte[cbCurrentLength];
                        Marshal.Copy(ptr, buf, 0, buf.Length);

                        OnDataReady(buf);
                    }
                }
                finally
                {
                    buffer.Unlock();
                }
            }
        }

        private void Encoder_SampleReady(Sample outputSample)
        {
            FinalizeSample(outputSample);
        }

        public void Close()
        {
            logger.Debug("VideoEncoder::Close()");

            if (encoder != null)
            {
                encoder.SampleReady -= Encoder_SampleReady;
                encoder.Stop();
                //encoder.Close();
            }

            if (processor != null)
            {
                processor.Close();
                processor = null;
            }

            if (bufTexture != null)
            {
                bufTexture.Dispose();
                bufTexture = null;
            }


        }


        public event Action<byte[]> DataEncoded;

        private void OnDataReady(byte[] buf)
        {
            DataEncoded?.Invoke(buf);
        }


    }

}
