using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
	public partial class LibCuda
	{
		public const int ApiVerison = 10020;

		private const string nvCudaPath = "nvcuda.dll";
		private const string _ver = "_v2";

		public static void Initialize()
		{
            var result = Initialize(0);
            CheckResult(result);
		}

		public static int DriverGetVersion()
		{
            var result = DriverGetVersion(out var version);

			CheckResult(result);

			return version;
		}


        public static string GetErrorString(CuResult error)
        {
            var errorStr = "Unknown error";
            var result = GetErrorString(error, out var str);
            CheckResult(result);
            if (str != IntPtr.Zero)
            {
                errorStr = Marshal.PtrToStringAnsi(str);
            }
            return errorStr;
        }

        public static string GetErrorName(CuResult error)
		{
            var errorStr = "Unknown error";
            var result = GetErrorName(error, out var str);
            CheckResult(result);
            if (str != IntPtr.Zero)
            {
                errorStr = Marshal.PtrToStringAnsi(str);
            }
            return errorStr;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void CheckResult(CuResult result, [CallerMemberName] string callerName = "")
		{
			if (result != CuResult.Success)
			{
				var errorName = GetErrorName(result);
				var errorString = GetErrorString(result);

				throw new LibNvEncException(callerName, result, errorName, errorString);
			}
		}
	}


    public class CuDevice
    {
        public readonly CuDevicePtr DevicePtr;
        internal CuDevice(CuDevicePtr devicePtr)
        {
            this.DevicePtr = devicePtr;
        }

        public static readonly CuDevice Empty = new CuDevice(CuDevicePtr.Empty);

        /// <summary>CU_DEVICE_CPU
        /// Device that represents the CPU</summary>
        public static readonly CuDevice DeviceCpu = new CuDevice(CuDevicePtr.DeviceCpu);

        /// <summary>CU_DEVICE_INVALID
        /// Device that represents an invalid device</summary>
        public static readonly CuDevice DeviceInvalid = new CuDevice(CuDevicePtr.DeviceInvalid);

        public static CuDevice GetDevice(int ordinal)
        {
            var result = LibCuda.DeviceGet(out var device, ordinal);
            LibCuda.CheckResult(result);

            return new CuDevice(device);
        }

        public static CuDevice GetFromDxgiAdapter(IntPtr adapter)
        {
            var result = LibCuda.D3D11GetDevice(out var device, adapter);
            LibCuda.CheckResult(result);

            return new CuDevice(device);
        }

        public static int GetCount()
        {
            var result = LibCuda.DeviceGetCount(out var count);
            LibCuda.CheckResult(result);

            return count;
        }

        public string GetPciBusId()
        {
            string pciBusId = "";
            unsafe
            {
                const int length = 20;

                var namePtr = stackalloc byte[length];
                Kernel32.ZeroMemory(namePtr, length);
                var result = LibCuda.DeviceGetPCIBusId(namePtr, length, DevicePtr);
                LibCuda.CheckResult(result);

                pciBusId = Marshal.PtrToStringAnsi((IntPtr)namePtr);
            }

            return pciBusId;

        }

        public unsafe string GetName()
        {
            string DeviceName = "";
            unsafe
            {
                const int inputLength = 256;
                var name = stackalloc byte[inputLength];
                Kernel32.ZeroMemory(name, inputLength);
                var result = LibCuda.DeviceGetName(name, inputLength, DevicePtr);
                LibCuda.CheckResult(result);

                DeviceName = Marshal.PtrToStringAnsi((IntPtr)name);
            }

            return DeviceName;
        }

        public long GetTotalMemory()
        {
            var result = LibCuda.DeviceTotalMemory(out var memorySize, DevicePtr);
            LibCuda.CheckResult(result);
            return memorySize;

        }


        public int GetAttribute(CuDeviceAttribute attribute)
        {
            var result = LibCuda.DeviceGetAttribute(out var output, attribute, DevicePtr);
            LibCuda.CheckResult(result);

            return output;
        }


        public CuContext CreateContext(CuContextFlags flags = CuContextFlags.SchedAuto)
        {
            var result = LibCuda.CtxCreate(out var contextPtr, flags, DevicePtr);
            LibCuda.CheckResult(result);

            return new CuContext(contextPtr);
        }
    }

	public class CuContext : IDisposable
	{
		private readonly CuContextPtr ContextPtr;
		internal CuContext(CuContextPtr contextPtr)
		{
			this.ContextPtr = contextPtr;
		}

		public class CuContextPush : IDisposable
		{
			private CuContextPtr context;

			internal CuContextPush(CuContextPtr ctx)
			{
				this.context = ctx;
			}

			public void Dispose()
			{
				var result = LibCuda.CtxPopCurrent(out _);
				LibCuda.CheckResult(result);
			}
		}


		public CuContextPush Push()
		{
			var result = LibCuda.CtxPushCurrent(ContextPtr);
			LibCuda.CheckResult(result);

			return new CuContextPush(ContextPtr);
		}


		public void SetCurrent()
		{
			var result = LibCuda.CtxSetCurrent(ContextPtr);
			LibCuda.CheckResult(result);
		}


		public uint GetApiVersion()
		{
			var result = LibCuda.CtxGetApiVersion(ContextPtr, out var version);
			LibCuda.CheckResult(result);

			return version;
		}


		public CuDevice GetDevice()
		{
			CuDevice device = null;
			using (var context = Push())
			{
				var result = LibCuda.CtxGetDevice(out var devicePtr);
				LibCuda.CheckResult(result);
				device = new CuDevice(devicePtr);
			}
			return device;
		}


		public static CuContextPtr GetCurrent()
		{
			var result = LibCuda.CtxGetCurrent(out var ctx);
			LibCuda.CheckResult(result);

			return ctx;
		}


		public static SharedMemoryConfig GetSharedMemConfig()
		{
			var result = LibCuda.CtxGetSharedMemConfig(out var config);
			LibCuda.CheckResult(result);

			return config;
		}


		public static void SetSharedMemConfig(SharedMemoryConfig config)
		{
			var result = LibCuda.CtxSetSharedMemConfig(config);
			LibCuda.CheckResult(result);
		}

		public static CuFunctionCache GetCacheConfig()
		{
			var result = LibCuda.CtxGetCacheConfig(out var config);
			LibCuda.CheckResult(result);

			return config;
		}


		public static void SetCacheConfig(CuFunctionCache config)
		{
			var result = LibCuda.CtxSetCacheConfig(config);
			LibCuda.CheckResult(result);
		}


		public static CuDevicePtr GetCurrentDevice()
		{
			var result = LibCuda.CtxGetDevice(out var device);
			LibCuda.CheckResult(result);

			return device;
		}

		public CuVideoContextLockObj CreateLock()
		{
			var result = NvCuVid.CtxLockCreate(out var contextLock, ContextPtr);
			LibCuda.CheckResult(result);

			return new CuVideoContextLockObj(contextLock);
		}

		private bool disposed = false;
		public void Dispose()
		{
			var result = LibCuda.CtxDestroy(ContextPtr);
			LibCuda.CheckResult(result);

			disposed = true;
		}
	}


	public class CuGraphicsResource : IDisposable
	{
		public readonly CuGraphicsResourcePtr ResourcePtr;
		private CuGraphicsResource(CuGraphicsResourcePtr res)
		{
			this.ResourcePtr = res;
		}

		public static CuGraphicsResource Register(IntPtr resourcePtr, CuGraphicsRegisters flags = CuGraphicsRegisters.None)
		{
			var result = LibCuda.GraphicsD3D11RegisterResource(out var resource, resourcePtr, flags);

			LibCuda.CheckResult(result);

			return new CuGraphicsResource(resource);
		}


		public void SetMapFlags(CuGraphicsMapResources flags)
		{
			var result = LibCuda.GraphicsResourceSetMapFlags(ResourcePtr, flags);
			LibCuda.CheckResult(result);
		}

		public CuGraphicsMappedResource Map()
		{
			return Map(CuStreamPtr.Empty);
		}


		public unsafe CuGraphicsMappedResource Map(CuStreamPtr stream)
		{
			var copy = ResourcePtr;
			var result = LibCuda.GraphicsMapResources(1, &copy, stream);
			LibCuda.CheckResult(result);

			return new CuGraphicsMappedResource(ResourcePtr, stream);
		}

		public class CuGraphicsMappedResource : IDisposable
		{
			private readonly CuGraphicsResourcePtr resource;
			private readonly CuStreamPtr stream;

			public CuGraphicsMappedResource(CuGraphicsResourcePtr resource, CuStreamPtr stream)
			{
				this.resource = resource;
				this.stream = stream;
			}

			public unsafe void Dispose()
			{
				var copy = resource;
				{
					var result = LibCuda.GraphicsUnmapResources(1, &copy, stream);
					LibCuda.CheckResult(result);
				}
			}
		}

		public CuArrayPtr GetMappedArray(int arrayIndex = 0, int mipLevel = 0)
		{
			var result = LibCuda.GraphicsSubResourceGetMappedArray(out var array, ResourcePtr, arrayIndex, mipLevel);
			LibCuda.CheckResult(result);

			return array;
		}

		public void Dispose()
		{
			var result= LibCuda.GraphicsUnregisterResource(ResourcePtr);
			LibCuda.CheckResult(result);
		}
	}

	public struct CuStream : IDisposable
	{
		public static readonly CuStream Empty = new CuStream(CuStreamPtr.Empty);

		private readonly CuStreamPtr streamPtr;
		internal CuStream(CuStreamPtr ptr)
		{
			this.streamPtr = ptr;
		}

		public static CuStream Create(CuStreamFlags flags = CuStreamFlags.Default)
		{
			var result = LibCuda.StreamCreate(out var stream, flags);
			LibCuda.CheckResult(result);

			return new CuStream(stream);
		}

		public static CuStreamPtr Create(int priority, CuStreamFlags flags = CuStreamFlags.Default)
		{
			var result = LibCuda.StreamCreateWithPriority(out var stream, flags, priority);
			LibCuda.CheckResult(result);

			return stream;
		}

		public CuResult Query()
		{
			return LibCuda.StreamQuery(streamPtr);
		}


		public void Synchronize()
		{
			var result = LibCuda.StreamSynchronize(streamPtr);
			LibCuda.CheckResult(result);
		}

		public void Dispose()
		{
			var result = LibCuda.StreamDestroy(streamPtr);
			LibCuda.CheckResult(result);
		}
	}


    public class CuHostMemory : IDisposable
    {
        public static readonly CuHostMemory Empty = new CuHostMemory (CuHostMemoryPtr.Empty);

        private readonly CuHostMemoryPtr memoryPtr;
        internal CuHostMemory(CuHostMemoryPtr ptr)
        {
            this.memoryPtr = ptr;
        }

        public static CuHostMemoryPtr Allocate(long bytesize)
        {
            var result = LibCuda.MemAllocHost(out var mem, (IntPtr)bytesize);
            LibCuda.CheckResult(result);
            return mem;
        }

        // TODO: Move?
        public static CuDevicePtr AllocateManaged(long bytesize, MemoryAttachFlags flags)
        {
            var result = LibCuda.MemAllocManaged(out var mem, (IntPtr)bytesize, flags);
            LibCuda.CheckResult(result);
            return mem;
        }

        public void Dispose()
        {
            var result = LibCuda.MemFreeHost(memoryPtr);
            LibCuda.CheckResult(result);
        }
    }

    public class CuDeviceMemoryObj : IDisposable
    {
        private readonly CuDevicePtr DevicePtr;
        private CuDeviceMemoryObj(CuDevicePtr devicePtr)
        {
            this.DevicePtr = devicePtr;
            //Handle = devicePtr.Handle;
        }

        public static CuDeviceMemoryObj Allocate(IntPtr bytesize)
        {
            if (bytesize.ToInt64() < 0) throw new ArgumentOutOfRangeException(nameof(bytesize));

            var result = LibCuda.MemAlloc(out var device, bytesize);
            LibCuda.CheckResult(result);

            return new CuDeviceMemoryObj(device);
        }

        public static CuDeviceMemoryObj Allocate(int bytesize)
        {
            if (bytesize < 0) throw new ArgumentOutOfRangeException(nameof(bytesize));

            return Allocate((IntPtr)bytesize);
        }


        public static CuDeviceMemoryObj AllocatePitch(out IntPtr pitch, IntPtr widthInBytes, IntPtr height, uint elementSizeBytes)
        {
            var result = LibCuda.MemAllocPitch(out var device, out pitch, widthInBytes, height, elementSizeBytes);

            LibCuda.CheckResult(result);

            return new CuDeviceMemoryObj(device);
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
            var result = LibCuda.MemcpyDtoH((IntPtr)hostDestination, DevicePtr, (IntPtr)size);
            LibCuda.CheckResult(result);
        }

        /// <inheritdoc cref="LibCuda.MemFree(CuDevicePtr)"/>
        public void Dispose()
        {
            var result = LibCuda.MemFree(DevicePtr);
            LibCuda.CheckResult(result);
        }

    }

}
