using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FFmpegLib;
using MediaToolkit.Core;
using MediaToolkit.DirectX;
using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;

namespace MediaToolkit.MediaFoundation
{

	public class VideoFrameEncoder
	{

		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

		private IVideoFrameEncoder encoder = null;
		private Device device = null;

		public void Open(VideoBufferBase sourceBuffer, VideoEncoderSettings encoderSettings)
		{
			logger.Debug("VideoEncoder::Open(...)");

			var encoderName = encoderSettings.EncoderId;
			if (encoderName == "libx264" || encoderName == "h264_nvenc")
			{
				if (sourceBuffer.DriverType != VideoDriverType.CPU)
				{
					throw new InvalidOperationException("Invalid video buffer driver type: " + sourceBuffer.DriverType);
				}

				encoder = new FFmpegH264Encoder();
			}
            else if (encoderName == "H264EncCpuNull" || encoderName == "H264EncGpuNull")
            {
                encoder = new H264NullEncoder();
            }
            else
			{
				if (sourceBuffer.DriverType != VideoDriverType.D3D11)
				{
					throw new InvalidOperationException("Invalid video buffer driver type: " + sourceBuffer.DriverType);
				}

				var d = ((D3D11VideoBuffer)sourceBuffer).D3D11Device;
				this.device = new Device(d.NativePointer);
				((IUnknown)device).AddReference();

				//using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
				//{
				//	using (var adapter = dxgiDevice.Adapter)
				//	{
				//		var descr = adapter.Description;
				//		var venId = descr.VendorId;
				//	}
				//}

				encoder = new MfH264EncoderEx(device);
			}

			encoder.Setup(encoderSettings);
			encoder.DataEncoded += Encoder_DataEncoded;
			encoder.Start();
		}

        public void Encode(IVideoFrame frame)
        {
            if (frame != null)
            {
                bool lockTaken = false;
                try
                {
                    lockTaken = frame.Lock(10);
                    if (lockTaken)
                    {
                        encoder?.ProcessFrame(frame);
                    }
					else
					{
						logger.Debug("Encoder drop frame...");
					}
                }
                finally
                {
                    if (lockTaken)
                    {
                        frame.Unlock();
                    }
                }
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
			}

			DxTool.SafeDispose(device);
		}

		public event Action<byte[], double> DataEncoded;
		private void OnDataReady(byte[] buf, double time)
		{
			DataEncoded?.Invoke(buf, time);
		}

	}

	public interface IVideoFrameEncoder
	{
		void Setup(VideoEncoderSettings settings);
		void Start();
		bool ProcessFrame(IVideoFrame frame);

		void Stop();
		void Close();

		event Action<IntPtr, int, double> DataEncoded;
	}

    class H264NullEncoder : IVideoFrameEncoder
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");
        public event Action<IntPtr, int, double> DataEncoded;

        public H264NullEncoder() { }

        public void Setup(VideoEncoderSettings settings){ }
        public void Start() { }
        public void Stop() { }
        public void Close() { }

        public bool ProcessFrame(IVideoFrame srcFrame)
        {
            return true;
        }

    }


    class FFmpegH264Encoder : IVideoFrameEncoder
	{
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");
		private FFmpegLib.H264Encoder encoder = null;

		public event Action<IntPtr, int, double> DataEncoded;

		public void Setup(VideoEncoderSettings settings)
		{
			encoder = new FFmpegLib.H264Encoder();
			encoder.Setup(settings);
			encoder.DataEncoded += Encoder_DataEncoded;

		}
		public void Start() { }
		public void Stop() { Close(); }
		public void Close()
		{
			if (encoder != null)
			{
				encoder.DataEncoded -= Encoder_DataEncoded;
				encoder.Close();
				encoder = null;
			}
		}

		public bool ProcessFrame(IVideoFrame srcFrame)
		{
			var Result = false;
			try
			{
				encoder.Encode(srcFrame);
				Result = true;
			}
			catch (Exception ex)
			{
				logger.Error(ex.Message);
				Result = false;
			}
			return Result;
		}

		private void Encoder_DataEncoded(IntPtr arg1, int arg2, double arg3)
		{
			DataEncoded?.Invoke(arg1, arg2, arg3);
		}
	}

	/*
	public interface IMfVideoTransform
	{
		void Setup(MfVideoArgs args);
		void Start();
		bool ProcessSample(Sample sample);

		void Stop();
		void Close();

		event Action<IntPtr, int, double> DataEncoded;
	}
	class MfFFMpegVideoEncoder : IMfVideoTransform
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
			catch (Exception ex)
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

	*/

}
