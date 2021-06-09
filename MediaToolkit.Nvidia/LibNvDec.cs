using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{

	public class NvDecodeApi
	{

		public static CuVideoParser CreateParser(CuVideoParserParams pars)
		{
			var result = NvCuVid.CreateVideoParser(out var parser, ref pars);
			LibCuda.CheckResult(result);

			return new CuVideoParser(parser, pars);
		}

        public static CuVideoDecoder CreateDecoder(CuVideoDecodeCreateInfo info)
        {
            var result = NvCuVid.CreateDecoder(out var decoder, ref info);
            LibCuda.CheckResult(result);

            return new CuVideoDecoder(decoder, info);
        }


        public static bool IsFormatSupportedByDecoder(CuVideoFormat format, out string error, out CuVideoDecodeCaps caps)
		{
			//caps = CuVideoDecodeCaps.GetDecoderCaps(Codec, ChromaFormat, BitDepthLumaMinus8);

			caps = new CuVideoDecodeCaps
			{
				CodecType = format.Codec,
				ChromaFormat = format.ChromaFormat,
				BitDepthMinus8 = format.BitDepthLumaMinus8
			};

			var result = NvCuVid.GetDecoderCaps(ref caps);
			LibCuda.CheckResult(result);

			if (!caps.IsSupported)
			{
				error = $"Codec {format.Codec} is not supported.";
				return false;
			}

			var codedWidth = format.CodedWidth;
			var codedHeight = format.CodedHeight;
			// validate the content resolution supported on underlying hardware
			if (codedWidth > caps.MaxWidth || codedHeight > caps.MaxHeight)
			{
				error = $"Unsupported video dimentions. Requested: {codedWidth}x{codedHeight}. Supported max: {caps.MaxWidth}x{caps.MaxHeight}.";
				return false;
			}

			// Max supported macroblock count CodedWidth*CodedHeight/256 must be <= nMaxMBCount
			var mbCount = (codedWidth >> 4) * (codedHeight >> 4);
			if (mbCount > caps.MaxMBCount)
			{
				error = "MBCount not supported on this GPU";
				return false;
			}

			//TODO:
			// Check supported output format
			// Check if histogram is supported

			error = "";
			return true;
		}

		public static CuVideoDecodeCaps GetDecoderCaps(CuVideoCodec codecType, CuVideoChromaFormat chromaFormat, int bitDepthMinus8)
		{
			var caps = new CuVideoDecodeCaps
			{
				CodecType = codecType,
				ChromaFormat = chromaFormat,
				BitDepthMinus8 = bitDepthMinus8
			};

			GetDecoderCaps(ref caps);

			return caps;
		}

		public static void GetDecoderCaps(ref CuVideoDecodeCaps caps)
		{
			var result = NvCuVid.GetDecoderCaps(ref caps);
			LibCuda.CheckResult(result);

		}

	}

	public class CuVideoParser : IDisposable
	{
		private readonly CuVideoParserPtr parserHandle;
		public CuVideoParserParams ParserParams { get; private set; }

		internal CuVideoParser(CuVideoParserPtr handle, CuVideoParserParams pars)
		{
			this.parserHandle = handle;
			this.ParserParams = pars;
		}


		public void ParseVideoData(ref CuVideoSourceDataPacket packet)
		{
			var result = NvCuVid.ParseVideoData(parserHandle, ref packet);
			LibCuda.CheckResult(result);
		}

		public unsafe void ParseVideoData(byte[] payload, CuVideoPacketFlags flags = CuVideoPacketFlags.None, long timestamp = 0)
		{
			fixed (byte* payloadPtr = payload)
			{
				var packet = new CuVideoSourceDataPacket
				{
					Flags = flags,
					Payload = payloadPtr,
					PayloadSize = (uint)payload.Length,
					Timestamp = timestamp
				};

				ParseVideoData(ref packet);
			}
		}


		public unsafe void ParseVideoData(IntPtr payload, int payloadLength, CuVideoPacketFlags flags = CuVideoPacketFlags.None, long timestamp = 0)
		{
			var packet = new CuVideoSourceDataPacket
			{
				Flags = flags,
				Payload = (byte*)payload,
				PayloadSize = (uint)payloadLength,
				Timestamp = timestamp
			};

			ParseVideoData(ref packet);
		}

		private bool disposed = false;
		public void Dispose()
		{
			var result = NvCuVid.DestroyVideoParser(parserHandle);
			LibCuda.CheckResult(result);
			disposed = true;
		}
	}


    public class CuVideoDecoder : IDisposable
    {
        private readonly CuVideoDecoderPtr decoderHandle;
        public bool IsEmpty => decoderHandle.Handle == IntPtr.Zero;

        public CuVideoDecodeCreateInfo DecodeInfo { get; private set; }
        
        internal CuVideoDecoder(CuVideoDecoderPtr handle, CuVideoDecodeCreateInfo info)
        {
            this.decoderHandle = handle;
            this.DecodeInfo = info;
        }

        public CuVideoDecodeStatus GetDecodeStatus(int picIndex = 0)
        {
            var result = NvCuVid.GetDecodeStatus(decoderHandle, picIndex, out var status);
            LibCuda.CheckResult(result);
            return status;
        }


        public void Reconfigure(ref CuVideoReconfigureDecoderInfo decReconfigParams)
        {
            var result =NvCuVid.ReconfigureDecoder(decoderHandle, ref decReconfigParams);
            LibCuda.CheckResult(result);
        }

        public void Reconfigure(ref CuVideoFormat format)
        {
            // TODO
            var info = new CuVideoReconfigureDecoderInfo
            {
                Width = format.CodedWidth,
                Height = format.CodedHeight,
            };

            Reconfigure(ref info);
        }

        public void DecodePicture(ref CuVideoPicParams param)
        {
            var result = NvCuVid.DecodePicture(decoderHandle, ref param);
            LibCuda.CheckResult(result);
        }


        public CuVideoFrame MapVideoFrame(int picIndex, ref CuVideoProcParams param, out int pitch)
        {
            CuDevicePtr devicePtr;

            var result = Environment.Is64BitProcess
                ? NvCuVid.MapVideoFrame64(decoderHandle, picIndex, out devicePtr, out pitch, ref param)
                : NvCuVid.MapVideoFrame(decoderHandle, picIndex, out devicePtr, out pitch, ref param);

            LibCuda.CheckResult(result);

            return new CuVideoFrame(devicePtr, decoderHandle);
        }

        private bool disposed = false;
        public void Dispose()
        {
            var result = NvCuVid.DestroyDecoder(decoderHandle);
            LibCuda.CheckResult(result);
            disposed = true;
        }
    }


    public class CuVideoFrame : IDisposable
    {
        public readonly CuVideoDecoderPtr DecoderPtr;
        public readonly CuDevicePtr DevicePtr;
        public IntPtr Handle => DevicePtr.Handle;

        internal CuVideoFrame(CuDevicePtr devicePtr, CuVideoDecoderPtr decoderPtr)
        {
            this.DevicePtr = devicePtr;
            this.DecoderPtr = decoderPtr;
        }

        public void Dispose()
        {
            var result = Environment.Is64BitProcess
                ? NvCuVid.UnmapVideoFrame64(DecoderPtr, DevicePtr)
                : NvCuVid.UnmapVideoFrame(DecoderPtr, DevicePtr);

            LibCuda.CheckResult(result);
        }
    }


    public class CuVideoContextLock : IDisposable
    {
        public readonly CuVideoContextLockPtr NativePtr;
        internal CuVideoContextLock(CuVideoContextLockPtr ptr)
        {
            this.NativePtr = ptr;
        }

        public class AutoCuVideoContextLock : IDisposable
        {
            private readonly CuVideoContextLock contextLock;
            private int _disposed;

            public AutoCuVideoContextLock(CuVideoContextLock obj)
            {
                contextLock = obj;
                _disposed = 0;
            }

            public void Dispose()
            {
                var disposed = Interlocked.Exchange(ref _disposed, 1);
                if (disposed != 0) return;

                contextLock.Unlock();
            }
        }

        public AutoCuVideoContextLock Lock()
        {
            var result = NvCuVid.CtxLock(NativePtr, 0);
            LibCuda.CheckResult(result);

            return new AutoCuVideoContextLock(this);
        }

        public void Unlock()
        {
            var result = NvCuVid.CtxUnlock(NativePtr, 0);
            LibCuda.CheckResult(result);
        }

        public void Dispose()
        {
            var result = NvCuVid.CtxLockDestroy(NativePtr);
            LibCuda.CheckResult(result);
        }

    }

}
