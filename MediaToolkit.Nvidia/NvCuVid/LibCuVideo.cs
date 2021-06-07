using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using static MediaToolkit.Nvidia.LibCuda;
using static MediaToolkit.Nvidia.NvCuVid;

namespace MediaToolkit.Nvidia
{
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    [Obsolete]
    public struct CuVideoSource : IDisposable
    {
        public static readonly CuVideoSource Empty = new CuVideoSource { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        /// <inheritdoc cref="DestroyVideoSource(CuVideoSource)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuVideoSource { Handle = handle };

			CheckResult(DestroyVideoSource(obj));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuStream : IDisposable
    {
        public static readonly CuStream Empty = new CuStream { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        /// <inheritdoc cref="StreamCreate(out CuStream, CuStreamFlags)"/>
        public static CuStream Create(CuStreamFlags flags = CuStreamFlags.Default)
        {
            var result = StreamCreate(out var stream, flags);
			CheckResult(result);

            return stream;
        }

        /// <inheritdoc cref="StreamCreateWithPriority(out CuStream, CuStreamFlags, int)"/>
        public static CuStream Create(int priority, CuStreamFlags flags = CuStreamFlags.Default)
        {
            var result = StreamCreateWithPriority(out var stream, flags, priority);
			CheckResult(result);

            return stream;
        }

        /// <inheritdoc cref="StreamQuery(CuStream)"/>
        public CuResult Query()
        {
            return StreamQuery(this);
        }

        /// <inheritdoc cref="StreamQuery(CuStream)"/>
        public bool HasPendingOperations()
        {
            return StreamQuery(this) == CuResult.ErrorNotReady;
        }

        /// <inheritdoc cref="StreamSynchronize(CuStream)"/>
        public void Synchronize()
        {
            var result = StreamSynchronize(this);
			CheckResult(result);
        }

        /// <inheritdoc cref="StreamDestroy(CuStream)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuStream { Handle = handle };

            CheckResult(StreamDestroy(obj));
        }
    }

	public partial struct CuContext 
	{
		/// <inheritdoc cref="CtxLockCreate(out CuVideoContextLock, CuContext)"/>
		public CuVideoContextLock CreateLock()
		{
			var result = CtxLockCreate(out var lok, this);
			CheckResult(result);

			return lok;
		}

	}

	[StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoDecoder : IDisposable
    {
        public static readonly CuVideoDecoder Empty = new CuVideoDecoder { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        /// <inheritdoc cref="CreateDecoder(out CuVideoDecoder, ref CuVideoDecodeCreateInfo)"/>
        public static CuVideoDecoder Create(ref CuVideoDecodeCreateInfo pdci)
        {
            var result = CreateDecoder(out var decoder, ref pdci);
            CheckResult(result);
            return decoder;
        }

        /// <inheritdoc cref="NvCuVid.GetDecodeStatus(CuVideoDecoder, int, out CuVideoDecodeStatus)"/>
        public CuVideoDecodeStatus GetDecodeStatus(int picIndex = 0)
        {
            var result = NvCuVid.GetDecodeStatus(this, picIndex, out var status);
            CheckResult(result);
            return status;
        }

        /// <inheritdoc cref="ReconfigureDecoder(CuVideoDecoder, ref CuVideoReconfigureDecoderInfo)"/>
        public void Reconfigure(ref CuVideoReconfigureDecoderInfo decReconfigParams)
        {
            var result = ReconfigureDecoder(this, ref decReconfigParams);
            CheckResult(result);
        }

        /// <inheritdoc cref="ReconfigureDecoder(CuVideoDecoder, ref CuVideoReconfigureDecoderInfo)"/>
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

        /// <inheritdoc cref="NvCuVid.DecodePicture(CuVideoDecoder, ref CuVideoPicParams)"/>
        public void DecodePicture(ref CuVideoPicParams param)
        {
            var result = NvCuVid.DecodePicture(this, ref param);
            CheckResult(result);
        }

        /// <inheritdoc cref="NvCuVid.MapVideoFrame64(CuVideoDecoder, int, out CuDevicePtr, out int, ref CuVideoProcParams)"/>
        public CuVideoFrame MapVideoFrame(int picIndex, ref CuVideoProcParams param, out int pitch)
        {
            CuDevicePtr devicePtr;

            var result = Environment.Is64BitProcess
                ? NvCuVid.MapVideoFrame64( this, picIndex, out devicePtr,out pitch, ref param)
                : NvCuVid.MapVideoFrame( this, picIndex, out devicePtr, out pitch, ref param);

            CheckResult(result);

            return new CuVideoFrame(devicePtr, this);
        }

        /// <inheritdoc cref="NvCuVid.DestroyDecoder(CuVideoDecoder)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuVideoDecoder { Handle = handle };

            CheckResult(DestroyDecoder(obj));
        }
    }

    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoFrame : IDisposable
    {
        public static readonly CuVideoFrame Empty = new CuVideoFrame { Handle = IntPtr.Zero };
        public IntPtr Handle;

        private readonly CuVideoDecoder _decoder;
        public bool IsEmpty => Handle == IntPtr.Zero;

        public CuVideoFrame(CuDevicePtr devicePtr, CuVideoDecoder decoder)
        {
            Handle = devicePtr.Handle;
            _decoder = decoder;
        }

        /// <inheritdoc cref="NvCuVid.UnmapVideoFrame64(CuVideoDecoder, CuDevicePtr)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuDevicePtr { Handle = handle };

            var result = Environment.Is64BitProcess
                ? UnmapVideoFrame64(_decoder, obj)
                : UnmapVideoFrame(_decoder, obj);

            CheckResult(result);
        }

        public static implicit operator CuDevicePtr(CuVideoFrame d) => new CuDevicePtr(d.Handle);
    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public unsafe struct CuVideoParser : IDisposable
    {
        public static readonly CuVideoParser Empty = new CuVideoParser { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        /// <inheritdoc cref="NvCuVid.ParseVideoData(CuVideoParser, ref CuVideoSourceDataPacket)"/>
        public void ParseVideoData(ref CuVideoSourceDataPacket packet)
        {
            var result = NvCuVid.ParseVideoData(this, ref packet);
            CheckResult(result);
		}

        /// <inheritdoc cref="NvCuVid.ParseVideoData(CuVideoParser, ref CuVideoSourceDataPacket)"/>
		/// public void ParseVideoData(Span<byte> payload, CuVideoPacketFlags flags = CuVideoPacketFlags.None, long timestamp = 0)
        public void ParseVideoData(byte[] payload, CuVideoPacketFlags flags = CuVideoPacketFlags.None, long timestamp = 0)
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

        /// <inheritdoc cref="NvCuVid.ParseVideoData(CuVideoParser, ref CuVideoSourceDataPacket)"/>
        public void ParseVideoData(IntPtr payload, int payloadLength, CuVideoPacketFlags flags = CuVideoPacketFlags.None,long timestamp = 0)
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

        /// <inheritdoc cref="NvCuVid.ParseVideoData(CuVideoParser, ref CuVideoSourceDataPacket)"/>
        public void SendEndOfStream()
        {
            var eosPacket = new CuVideoSourceDataPacket
            {
                Flags = CuVideoPacketFlags.EndOfStream | CuVideoPacketFlags.NotifyEos
            };

            ParseVideoData(ref eosPacket);
        }

        /// <inheritdoc cref="NvCuVid.CreateVideoParser(out CuVideoParser, ref CuVideoParserParams)"/>
        public static CuVideoParser Create(ref CuVideoParserParams @params)
        {
            var result = CreateVideoParser(out var parser, ref @params);
            CheckResult(result);
            return parser;
        }

        /// <inheritdoc cref="NvCuVid.DestroyVideoParser(CuVideoParser)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuVideoParser { Handle = handle };

            CheckResult(DestroyVideoParser(obj));
        }
    }


    /// <summary>
    /// Wraps the CuDevicePtr with a safe memory deallocator.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuDeviceMemory : IDisposable
    {
        public static readonly CuDeviceMemory Empty = new CuDeviceMemory { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        private CuDeviceMemory(CuDevicePtr devicePtr)
        {
            Handle = devicePtr.Handle;
        }

        public static CuDeviceMemory Allocate(IntPtr bytesize)
        {
            if (bytesize.ToInt64() < 0) throw new ArgumentOutOfRangeException(nameof(bytesize));

            var result = LibCuda.MemAlloc(out var device, bytesize);
            CheckResult(result);

            return new CuDeviceMemory(device);
        }

        public static CuDeviceMemory Allocate(int bytesize)
        {
            if (bytesize < 0) throw new ArgumentOutOfRangeException(nameof(bytesize));

            return Allocate((IntPtr)bytesize);
        }

        /// <inheritdoc cref="LibCuda.MemAllocPitch(out CuDevicePtr, out IntPtr, IntPtr, IntPtr, uint)"/>
        public static CuDeviceMemory AllocatePitch(out IntPtr pitch, IntPtr widthInBytes,  IntPtr height, uint elementSizeBytes)
        {
            var result = LibCuda.MemAllocPitch(out var device, out pitch, widthInBytes, height, elementSizeBytes);

            CheckResult(result);

            return new CuDeviceMemory(device);
        }

        public unsafe byte[] CopyToHost(int size)
        {
            var host = new byte[size];

            fixed (byte* hostPtr = host)
            {
                CopyToHost(hostPtr, size);
            }

            return host;
        }

        public unsafe void CopyToHost(byte* hostDestination, int size)
        {
            var result = MemcpyDtoH((IntPtr)hostDestination, this, (IntPtr)size);
            CheckResult(result);
        }

        /// <inheritdoc cref="LibCuda.MemFree(CuDevicePtr)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuDevicePtr { Handle = handle };

            MemFree(obj);
        }

        public static implicit operator CuDevicePtr(CuDeviceMemory d) => new CuDevicePtr(d.Handle);
        public static implicit operator IntPtr(CuDeviceMemory d) => d.Handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoContextLock : IDisposable
    {
        public static readonly CuVideoContextLock Empty = new CuVideoContextLock { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

        public struct AutoCuVideoContextLock : IDisposable
        {
            private readonly CuVideoContextLock _lock;
            private int _disposed;

            public AutoCuVideoContextLock(CuVideoContextLock lok)
            {
                _lock = lok;
                _disposed = 0;
            }

            /// <inheritdoc cref="NvCuVid.CtxUnlock(CuVideoContextLock, uint)"/>
            public void Dispose()
            {
                var disposed = Interlocked.Exchange(ref _disposed, 1);
                if (disposed != 0) return;

                _lock.Unlock();
            }
        }

        /// <inheritdoc cref="NvCuVid.CtxLock(CuVideoContextLock, uint)"/>
        public AutoCuVideoContextLock Lock()
        {
            CheckResult(CtxLock(this, 0));
            return new AutoCuVideoContextLock(this);
        }

        /// <inheritdoc cref="NvCuVid.CtxUnlock(CuVideoContextLock, uint)"/>
        public void Unlock()
        {
            CheckResult(CtxUnlock(this, 0));
        }

        /// <inheritdoc cref="NvCuVid.CtxLockDestroy(CuVideoContextLock)"/>
        public void Dispose()
        {
            var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
            if (handle == IntPtr.Zero) return;
            var obj = new CuVideoContextLock { Handle = handle };

            CheckResult(CtxLockDestroy(obj));
        }
    }

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Value) + "}")]
	public struct ByteBool
	{
		public byte Value;

		public unsafe ByteBool(bool b)
		{
			//Value = Unsafe.As<bool, byte>(ref b);
			Value = *((byte*)(&b));
		}

		public static implicit operator bool(ByteBool d) => d.Value != 0;
		public static implicit operator ByteBool(bool d) => new ByteBool(d);
	}
}
