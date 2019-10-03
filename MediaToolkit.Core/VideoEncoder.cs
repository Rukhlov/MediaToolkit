using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.Common;
using MediaToolkit.MediaFoundation;
using NLog;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;

namespace MediaToolkit.Core
{
    public class VideoEncoder
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ScreenSource videoSource = null;

        public VideoEncoder(ScreenSource source)
        {
            this.videoSource = source;
        }

        private MfEncoderAsync encoder = null;
        private MfVideoProcessor processor = null;

        private Texture2D bufTexture = null;

        public void Open( VideoEncodingParams destParams)
        {
            logger.Debug("VideoEncoder::Setup(...)");

            var hwContext = videoSource.hwContext;
            var hwDevice = hwContext.device;
            var srcSize = new Size(videoSource.Buffer.bitmap.Width, videoSource.Buffer.bitmap.Height);

            var destSize = new Size(destParams.Width, destParams.Height);

            long adapterLuid = -1;
            using (var dxgiDevice = hwDevice.QueryInterface<SharpDX.DXGI.Device>())
            {
                var adapter = dxgiDevice.Adapter;
                adapterLuid = adapter.Description.Luid;

            }

            encoder = new MfEncoderAsync();
            encoder.Setup(new MfVideoArgs
            {
                Width = srcSize.Width,
                Height = srcSize.Height,
                FrameRate = destParams.FrameRate,

                AdapterId = adapterLuid,

            });

            var encDevice = encoder.device;
            processor = new MfVideoProcessor(encDevice);
            var inProcArgs = new MfVideoArgs
            {
                Width = srcSize.Width,
                Height = srcSize.Height,
                Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
            };

            var outProcArgs = new MfVideoArgs
            {
                Width = destSize.Width,
                Height = destSize.Height,
                Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
            };

            processor.Setup(inProcArgs, outProcArgs);

            var _descr = new Texture2DDescription
            {
                // Format = Format.NV12,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = srcSize.Width,
                Height = srcSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1 },
            };

            bufTexture = new Texture2D(encDevice, _descr);

   
            encoder.DataReady += MfEncoder_DataReady;


            processor.Start();
            encoder.Start();


        }

        private void MfEncoder_DataReady(byte[] obj)
        {
            OnDataReady(obj);
        }
        public void Encode()
        {
            var texture = videoSource?.hwContext?.SharedTexture;

            Encode(texture);
        }

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

            Sample inputSample = null;
            try
            {
                MediaBuffer mediaBuffer = null;
                try
                {
                    MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out mediaBuffer);
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
                                var uuid = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));
                                dxgiBuffer.GetResource(uuid, out IntPtr intPtr);
                                using (Texture2D nv12Texture = new Texture2D(intPtr))
                                {
                                    //processor.device.ImmediateContext.CopyResource(nv12Texture, SharedTexture);
                                    //processor.device.ImmediateContext.Flush();

                                    //mfEncoder.WriteTexture(SharedTexture);

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


        public void Close()
        {
            logger.Debug("VideoEncoder::Close()");

            if (encoder != null)
            {
                encoder.DataReady -= MfEncoder_DataReady;
                encoder.Stop();
                //mfEncoder.Close();
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
