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

        private IMfVideoEncoder encoder = null;

        //private MfH264Encoder encoder = null;
        //private MfH264EncoderEx encoder = null;
        //private FFmpegLib.H264Encoder ffEncoder = null;

        private MfVideoProcessor processor = null;

        private Texture2D bufTexture = null;

        private Device device = null;
        public void Open(VideoEncoderSettings encoderSettings)
		{
			logger.Debug("VideoEncoder::Setup(...)");

			//var hwContext = videoSource.hwContext;
			// var hwDevice = hwContext.Device3D11;

			var hwBuffer = videoSource.SharedTexture;

			var hwDescr = hwBuffer.Description;

			var srcSize = new Size(hwDescr.Width, hwDescr.Height);
			var srcFormat = MfTool.GetVideoFormatGuidFromDXGIFormat(hwDescr.Format);

			var destSize = encoderSettings.Resolution;//new Size(destParams.Width, destParams.Height);

			var adapterId = videoSource.AdapterId;

			using (var adapter = DxTool.FindAdapter1(adapterId))
			{
				var descr = adapter.Description;
				int adapterVenId = descr.VendorId;

				logger.Info("Adapter: " + descr.Description + " " + adapterVenId);

				var flags = DeviceCreationFlags.VideoSupport |
								DeviceCreationFlags.BgraSupport;
				//DeviceCreationFlags.Debug;

				device = new SharpDX.Direct3D11.Device(adapter, flags);
				using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
				{
					multiThread.SetMultithreadProtected(true);
				}

			}
			var profile = MfTool.GetMfH264Profile(encoderSettings.Profile);

			var bitrateMode = MfTool.GetMfBitrateMode(encoderSettings.BitrateMode);

			var aspectRatio = encoderSettings.AspectRatio;
			var encArgs = new MfVideoArgs
			{
				Width = destSize.Width, //srcSize.Width,
				Height = destSize.Height, //srcSize.Height,
				Format = VideoFormatGuids.NV12,//VideoFormatGuids.Argb32,

				FrameRate = MfTool.PackToLong(encoderSettings.FrameRate),

                MaxBitrate = encoderSettings.MaxBitrate * 1000, //kbps->bps
				AvgBitrate = encoderSettings.Bitrate * 1000,
				LowLatency = encoderSettings.LowLatency,
				AdapterId = videoSource.AdapterId,
				Profile = profile,
				BitrateMode = bitrateMode,
				
               
                EncoderId = encoderSettings.EncoderId,

				AspectRatio= MfTool.PackToLong(aspectRatio)
			};



			processor = new MfVideoProcessor(device);
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


			bufTexture = new Texture2D(device,
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

            var encoderName = encoderSettings.EncoderId;

            if (encoderName == "libx264" || encoderName == "h264_nvenc")
            {
                encoder = new MfFFMpegVideoEncoder();
            }
            else
            {
                encoder = new MfH264EncoderEx(device);
            }
           
			encoder.Setup(encArgs);

			encoder.DataEncoded += Encoder_DataEncoded;

			////encoder.DataReady += MfEncoder_DataReady;
			encoder.Start();

		}



		public void Encode()
        {
            var texture = videoSource?.SharedTexture;

            Encode(texture);

        }


        public void Encode(Texture2D texture)
        {
           // var device = encoder?.device;

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
                    MediaFactory.CreateDXGISurfaceBuffer(IID.D3D11Texture2D, bufTexture, 0, false, out mediaBuffer);
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
                            encoder.ProcessSample(processedSample);
                            //EncodeSample(processedSample);
                        }
                    }
                    finally
                    {
                        processedSample?.Dispose();
                    }
                }
                else
                {
                    encoder.ProcessSample(inputSample);
                    //EncodeSample(inputSample);
                }


            }
            finally
            {
                inputSample?.Dispose();
            }

        }


        private void Encoder_DataEncoded(IntPtr data, int size, double timestamp)
        {
            var buf = new byte[size];
            Marshal.Copy(data, buf, 0, size);

            OnDataReady(buf, timestamp);
        }

 
        public void Close()
        {
            logger.Debug("VideoEncoder::Close()");

            if (encoder != null)
            {
                encoder.DataEncoded -= Encoder_DataEncoded;
                encoder.Stop();
                //encoder.Close();
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
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

        public event Action<byte[], double> DataEncoded;

        private void OnDataReady(byte[] buf, double time)
        {
            DataEncoded?.Invoke(buf, time);
        }

    }


    public interface IMfVideoEncoder
    {
        void Setup(MfVideoArgs args);
        void Start();
		bool ProcessSample(Sample sample);

		void Stop();
        void Close();

        event Action<IntPtr, int, double> DataEncoded;
    }

    class MfFFMpegVideoEncoder : IMfVideoEncoder
    {
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");
		private FFmpegLib.H264Encoder encoder = null;

        public void Setup(MfVideoArgs args)
        {

            encoder = new FFmpegLib.H264Encoder();
            VideoEncoderSettings settings = new VideoEncoderSettings
            {
                EncoderId = "libx264",
                FrameRate = MfTool.LongToInts(args.FrameRate),
                Width = args.Width,
                Height = args.Height,

            };

            encoder.Setup(settings);
            encoder.DataEncoded += Encoder_DataEncoded;
        }

        private void Encoder_DataEncoded(IntPtr arg1, int arg2, double arg3)
        {
            DataEncoded?.Invoke(arg1, arg2, arg3);
        }

        public void Start()
        {

        }

        public bool ProcessSample(Sample sample)
        {
			var Result = false;
			try
			{
				using (var buffer = sample.ConvertToContiguousBuffer())
				{
					var ptr = buffer.Lock(out var maxLen, out var curLen);
					encoder.Encode(ptr, curLen, 0);

					buffer.Unlock();

					Result = true;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex);
				Result = false;
			}


            return Result;
        }

        public void Stop()
        {
            Close();
        }

        public void Close()
        {
            if (encoder != null)
            {
                encoder.DataEncoded -= Encoder_DataEncoded;
                encoder.Close();
                encoder = null;
            }
        }

        public event Action<IntPtr, int, double> DataEncoded;
    }



}
