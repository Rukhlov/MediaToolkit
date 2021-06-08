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
	public unsafe partial class LibCuda
	{
		public const int ApiVerison = 10020;

		private const string nvCudaPath = "nvcuda.dll";
		private const string _ver = "_v2";

		/// <summary>CUDA stream callback</summary>
		/// <param name="hStream">The stream the callback was added to, as passed to ::cuStreamAddCallback.  May be NULL.</param>
		/// <param name="status">::CUDA_SUCCESS or any persistent error on the stream.</param>
		/// <param name="userData">User parameter provided at registration.</param>
		public delegate void CuStreamCallback(CuStreamPtr hStream, CuResult status, IntPtr userData);

		/// <summary>Block size to per-block dynamic shared memory mapping for a certain kernel.</summary>
		/// <param name="blockSize">blockSize Block size of the kernel</param>
		/// <returns>The dynamic shared memory needed by a block.</returns>
		public delegate IntPtr CuOccupancyB2DSize(int blockSize);

		/// <summary>CUresult cuInit(unsigned int Flags)
		/// Initialize the CUDA driver API.</summary>
		[DllImport(nvCudaPath, EntryPoint = "cuInit")]
		public static extern CuResult Initialize(uint flags);

		/// <inheritdoc cref="LibCuda.Initialize(uint)"/>
		public static void Initialize()
		{
			CheckResult(Initialize(0));
		}

		/// <summary>Returns the CUDA driver version</summary>
		///
		/// <remarks>
		/// Returns in *<paramref name="driverVersion"/> the version number of the installed CUDA
		/// driver. This function automatically returns ::CUDA_ERROR_INVALID_VALUE if
		/// the <paramref name="driverVersion"/> argument is NULL.
		/// </remarks>
		///
		/// <param name="driverVersion">Returns the CUDA driver version</param>
		///
		/// <returns>
		/// ::CUDA_SUCCESS,
		/// ::CUDA_ERROR_INVALID_VALUE
		/// </returns>
		/// \notefnerr
		///
		/// \sa
		/// ::cudaDriverGetVersion,
		/// ::cudaRuntimeGetVersion
		/// CUresult CUDAAPI cuDriverGetVersion(int *driverVersion);
		[DllImport(nvCudaPath, EntryPoint = "cuDriverGetVersion")]
		public static extern CuResult DriverGetVersion(out int driverVersion);

		/// <inheritdoc cref="LibCuda.DriverGetVersion(out int)"/>
		public static int DriverGetVersion()
		{
			CheckResult(DriverGetVersion(out var version));
			return version;
		}

		/// <summary>Gets the string description of an error code</summary>
		///
		/// <remarks>
		/// Sets *<paramref name="str"/> to the address of a NULL-terminated string description
		/// of the error code <paramref name="error"/>.
		/// If the error code is not recognized, ::CUDA_ERROR_INVALID_VALUE
		/// will be returned and *<paramref name="str"/> will be set to the NULL address.
		/// </remarks>
		///
		/// <param name="error">Error code to convert to string</param>
		/// <param name="str">Address of the string pointer.</param>
		///
		/// <returns>
		/// ::CUDA_SUCCESS,
		/// ::CUDA_ERROR_INVALID_VALUE
		/// </returns>
		/// \sa
		/// ::CUresult,
		/// ::cudaGetErrorString
		/// CUresult CUDAAPI cuGetErrorString(CUresult error, const char **pStr);
		[DllImport(nvCudaPath, EntryPoint = "cuGetErrorString")]
		public static extern CuResult GetErrorString(CuResult error, out IntPtr str);

		/// <inheritdoc cref="LibCuda.GetErrorString(CuResult, out IntPtr)"/>
		public static string GetErrorString(CuResult error)
		{
			CheckResult(GetErrorString(error, out var str));

			return str == IntPtr.Zero
				? "Unknown error"
				: Marshal.PtrToStringAnsi(str);
		}

		/// <summary>Gets the string representation of an error code enum name</summary>
		///
		/// <remarks>
		/// Sets *<paramref name="str"/> to the address of a NULL-terminated string representation
		/// of the name of the enum error code <c>error</c>.
		/// If the error code is not recognized, ::CUDA_ERROR_INVALID_VALUE
		/// will be returned and *<paramref name="str"/> will be set to the NULL address.
		/// </remarks>
		///
		/// <param name="error">Error code to convert to string</param>
		/// <param name="str">Address of the string pointer.</param>
		///
		/// <returns>
		/// ::CUDA_SUCCESS,
		/// ::CUDA_ERROR_INVALID_VALUE
		/// </returns>
		/// \sa
		/// ::CUresult,
		/// ::cudaGetErrorName
		/// CUresult CUDAAPI cuGetErrorName(CUresult error, const char **pStr);
		[DllImport(nvCudaPath, EntryPoint = "cuGetErrorName")]
		public static extern CuResult GetErrorName(CuResult error, out IntPtr str);

		/// <inheritdoc cref="LibCuda.GetErrorName(CuResult, out IntPtr)"/>
		public static string GetErrorName(CuResult error)
		{

			CheckResult(GetErrorName(error, out var str));

			return str == IntPtr.Zero
				? "Unknown error"
				: Marshal.PtrToStringAnsi(str);
		}

		/// CUresult CUDAAPI cuGetExportTable(const void **ppExportTable, const CUuuid *pExportTableId);
		[DllImport(nvCudaPath, EntryPoint = "cuGetExportTable")]
		public static extern CuResult GetExportTable(IntPtr* ppExportTable, Guid* pExportTableId);

		/// <summary>
		/// Exception if <paramref name="result"/> is not <c>CuResult.Success</c>.
		/// </summary>
		/// <param name="result">The CuResult from a LibCuda API call to check.</param>
		/// <param name="callerName"></param>
		/// <exception cref="LibNvEncException">Thrown if <paramref name="result"/> is not <c>CuResult.Success</c>.</exception>
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

		public CuVideoContextLock CreateLock()
		{
			var result = NvCuVid.CtxLockCreate(out var contextLock, ContextPtr);
			LibCuda.CheckResult(result);

			return contextLock;
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

		public CuArray GetMappedArray(int arrayIndex = 0, int mipLevel = 0)
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
}
