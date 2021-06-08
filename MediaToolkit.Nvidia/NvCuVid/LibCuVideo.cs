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
    public unsafe struct CuVideoDecoderPtr { public IntPtr Handle; }


    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoFramePtr { public IntPtr Handle; }


    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public unsafe struct CuVideoParserPtr { public IntPtr Handle; } 

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

}
