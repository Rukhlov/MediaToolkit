using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        private readonly IVideoSource videoSource = null;

        public VideoEncoder(IVideoSource source)
        {
            this.videoSource = source;
        }

        private MfEncoderAsync encoder = null;
        private MfVideoProcessor processor = null;

        private Texture2D bufTexture = null;

        public void Open( VideoEncoderSettings destParams)
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
            if(destParams.Profile == H264Profile.High)
            {
                profile = eAVEncH264VProfile.High;
            }
            else if (destParams.Profile == H264Profile.Base)
            {
                profile = eAVEncH264VProfile.Base;
            }

            var bitrateMode = RateControlMode.CBR;
            if(destParams.BitrateMode == BitrateControlMode.VBR)
            {
                bitrateMode = RateControlMode.LowDelayVBR;
            }
            else if(destParams.BitrateMode == BitrateControlMode.Quality)
            {
                bitrateMode = RateControlMode.Quality;
            }

            encoder = new MfEncoderAsync();
            encoder.Setup(new MfVideoArgs
            {
                Width = srcSize.Width,
                Height = srcSize.Height,
                Format = VideoFormatGuids.NV12,//VideoFormatGuids.Argb32,
                FrameRate = destParams.FrameRate,
                AvgBitrate = destParams.Bitrate,
                LowLatency = destParams.LowLatency,
                AdapterId = videoSource.AdapterId,
                Profile = profile,
                BitrateMode = bitrateMode,
                MaxBitrate = destParams.MaxBitrate,

            });

            var encDevice = encoder.device;

            processor = new MfVideoProcessor(encDevice);
            var inProcArgs = new MfVideoArgs
            {
                Width = srcSize.Width,
                Height = srcSize.Height,
                Format = srcFormat, //SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
            };

            var outProcArgs = new MfVideoArgs
            {
                Width = destSize.Width,
                Height = destSize.Height,
                Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
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

   
            encoder.DataReady += MfEncoder_DataReady;

            processor?.Start();

            encoder.Start();
        }

        private void MfEncoder_DataReady(byte[] obj)
        {
            OnDataReady(obj);
        }

        public void Encode()
        {
            var texture = videoSource?.SharedTexture;

            Encode(texture);
        }

        private static Guid uuidTexture2d = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));

        public void Encode(Texture2D texture)
        {
            using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
            {
                var device = encoder.device;
                using (var sharedTexture = device.OpenSharedResource<Texture2D>(sharedRes.SharedHandle))
                {
                    device.ImmediateContext.CopyResource(sharedTexture, bufTexture);

                }

            }


            if(processor != null)
            {
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

                    Sample nv12Sample = null;
                    try
                    {
                        bool result = processor.ProcessSample(inputSample, out nv12Sample);
                        if (result)
                        {
                            using (var buffer = nv12Sample.ConvertToContiguousBuffer())
                            {
                                using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                                {

                                    dxgiBuffer.GetResource(uuidTexture2d, out IntPtr intPtr);
                                    using (Texture2D nv12Texture = new Texture2D(intPtr))
                                    {
                                        encoder.WriteTexture(nv12Texture);
                                    };
                                }
                            }
                        }
                    }
                    finally
                    {
                        nv12Sample?.Dispose();
                    }
                }
                finally
                {
                    inputSample?.Dispose();
                }
            }
            else
            {
                encoder.WriteTexture(bufTexture);
            }


        }


        public void Close()
        {
            logger.Debug("VideoEncoder::Close()");

            if (encoder != null)
            {
                encoder.DataReady -= MfEncoder_DataReady;
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
