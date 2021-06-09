﻿using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.Nvidia
{
    public static unsafe partial class LibCuda
    {
        /// <summary>CUresult cuDeviceGet ( CUdevice* device, int  ordinal )
        /// Returns a handle to a compute device.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGet")]
        public static extern CuResult DeviceGet(out CuDevicePtr device, int ordinal);

        /// <summary>Returns information about the device</summary>
        ///
        /// <remarks>
        /// Returns in *<paramref name="pi"/> the integer value of the attribute <paramref name="attrib"/> on device
        /// <paramref name="device"/>. The supported attributes are:
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_BLOCK: Maximum number of threads per
        ///   block;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_X: Maximum x-dimension of a block;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_Y: Maximum y-dimension of a block;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_BLOCK_DIM_Z: Maximum z-dimension of a block;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_X: Maximum x-dimension of a grid;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_Y: Maximum y-dimension of a grid;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_GRID_DIM_Z: Maximum z-dimension of a grid;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK: Maximum amount of
        ///   shared memory available to a thread block in bytes;
        /// - ::CU_DEVICE_ATTRIBUTE_TOTAL_CONSTANT_MEMORY: Memory available on device for
        ///   __constant__ variables in a CUDA C kernel in bytes;
        /// - ::CU_DEVICE_ATTRIBUTE_WARP_SIZE: Warp size in threads;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_PITCH: Maximum pitch in bytes allowed by the
        ///   memory copy functions that involve memory regions allocated through
        ///   ::cuMemAllocPitch();
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE1D_WIDTH: Maximum 1D
        ///  texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE1D_LINEAR_WIDTH: Maximum width
        ///  for a 1D texture bound to linear memory;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE1D_MIPMAPPED_WIDTH: Maximum
        ///  mipmapped 1D texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_WIDTH: Maximum 2D
        ///  texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_HEIGHT: Maximum 2D
        ///  texture height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LINEAR_WIDTH: Maximum width
        ///  for a 2D texture bound to linear memory;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LINEAR_HEIGHT: Maximum height
        ///  for a 2D texture bound to linear memory;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LINEAR_PITCH: Maximum pitch
        ///  in bytes for a 2D texture bound to linear memory;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_MIPMAPPED_WIDTH: Maximum
        ///  mipmapped 2D texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_MIPMAPPED_HEIGHT: Maximum
        ///  mipmapped 2D texture height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_WIDTH: Maximum 3D
        ///  texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_HEIGHT: Maximum 3D
        ///  texture height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_DEPTH: Maximum 3D
        ///  texture depth;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_WIDTH_ALTERNATE:
        ///  Alternate maximum 3D texture width, 0 if no alternate
        ///  maximum 3D texture size is supported;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_HEIGHT_ALTERNATE:
        ///  Alternate maximum 3D texture height, 0 if no alternate
        ///  maximum 3D texture size is supported;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE3D_DEPTH_ALTERNATE:
        ///  Alternate maximum 3D texture depth, 0 if no alternate
        ///  maximum 3D texture size is supported;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURECUBEMAP_WIDTH:
        ///  Maximum cubemap texture width or height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE1D_LAYERED_WIDTH:
        ///  Maximum 1D layered texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE1D_LAYERED_LAYERS:
        ///   Maximum layers in a 1D layered texture;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LAYERED_WIDTH:
        ///  Maximum 2D layered texture width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LAYERED_HEIGHT:
        ///   Maximum 2D layered texture height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURE2D_LAYERED_LAYERS:
        ///   Maximum layers in a 2D layered texture;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURECUBEMAP_LAYERED_WIDTH:
        ///   Maximum cubemap layered texture width or height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_TEXTURECUBEMAP_LAYERED_LAYERS:
        ///   Maximum layers in a cubemap layered texture;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE1D_WIDTH:
        ///   Maximum 1D surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE2D_WIDTH:
        ///   Maximum 2D surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE2D_HEIGHT:
        ///   Maximum 2D surface height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE3D_WIDTH:
        ///   Maximum 3D surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE3D_HEIGHT:
        ///   Maximum 3D surface height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE3D_DEPTH:
        ///   Maximum 3D surface depth;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE1D_LAYERED_WIDTH:
        ///   Maximum 1D layered surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE1D_LAYERED_LAYERS:
        ///   Maximum layers in a 1D layered surface;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE2D_LAYERED_WIDTH:
        ///   Maximum 2D layered surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE2D_LAYERED_HEIGHT:
        ///   Maximum 2D layered surface height;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACE2D_LAYERED_LAYERS:
        ///   Maximum layers in a 2D layered surface;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACECUBEMAP_WIDTH:
        ///   Maximum cubemap surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACECUBEMAP_LAYERED_WIDTH:
        ///   Maximum cubemap layered surface width;
        /// - ::CU_DEVICE_ATTRIBUTE_MAXIMUM_SURFACECUBEMAP_LAYERED_LAYERS:
        ///   Maximum layers in a cubemap layered surface;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_REGISTERS_PER_BLOCK: Maximum number of 32-bit
        ///   registers available to a thread block;
        /// - ::CU_DEVICE_ATTRIBUTE_CLOCK_RATE: The typical clock frequency in kilohertz;
        /// - ::CU_DEVICE_ATTRIBUTE_TEXTURE_ALIGNMENT: Alignment requirement; texture
        ///   base addresses aligned to ::textureAlign bytes do not need an offset
        ///   applied to texture fetches;
        /// - ::CU_DEVICE_ATTRIBUTE_TEXTURE_PITCH_ALIGNMENT: Pitch alignment requirement
        ///   for 2D texture references bound to pitched memory;
        /// - ::CU_DEVICE_ATTRIBUTE_GPU_OVERLAP: 1 if the device can concurrently copy
        ///   memory between host and device while executing a kernel, or 0 if not;
        /// - ::CU_DEVICE_ATTRIBUTE_MULTIPROCESSOR_COUNT: Number of multiprocessors on
        ///   the device;
        /// - ::CU_DEVICE_ATTRIBUTE_KERNEL_EXEC_TIMEOUT: 1 if there is a run time limit
        ///   for kernels executed on the device, or 0 if not;
        /// - ::CU_DEVICE_ATTRIBUTE_INTEGRATED: 1 if the device is integrated with the
        ///   memory subsystem, or 0 if not;
        /// - ::CU_DEVICE_ATTRIBUTE_CAN_MAP_HOST_MEMORY: 1 if the device can map host
        ///   memory into the CUDA address space, or 0 if not;
        /// - ::CU_DEVICE_ATTRIBUTE_COMPUTE_MODE: Compute mode that device is currently
        ///   in. Available modes are as follows:
        ///   - ::CU_COMPUTEMODE_DEFAULT: Default mode - Device is not restricted and
        ///     can have multiple CUDA contexts present at a single time.
        ///   - ::CU_COMPUTEMODE_PROHIBITED: Compute-prohibited mode - Device is
        ///     prohibited from creating new CUDA contexts.
        ///   - ::CU_COMPUTEMODE_EXCLUSIVE_PROCESS:  Compute-exclusive-process mode - Device
        ///     can have only one context used by a single process at a time.
        /// - ::CU_DEVICE_ATTRIBUTE_CONCURRENT_KERNELS: 1 if the device supports
        ///   executing multiple kernels within the same context simultaneously, or 0 if
        ///   not. It is not guaranteed that multiple kernels will be resident
        ///   on the device concurrently so this feature should not be relied upon for
        ///   correctness;
        /// - ::CU_DEVICE_ATTRIBUTE_ECC_ENABLED: 1 if error correction is enabled on the
        ///    device, 0 if error correction is disabled or not supported by the device;
        /// - ::CU_DEVICE_ATTRIBUTE_PCI_BUS_ID: PCI bus identifier of the device;
        /// - ::CU_DEVICE_ATTRIBUTE_PCI_DEVICE_ID: PCI device (also known as slot) identifier
        ///   of the device;
        /// - ::CU_DEVICE_ATTRIBUTE_PCI_DOMAIN_ID: PCI domain identifier of the device
        /// - ::CU_DEVICE_ATTRIBUTE_TCC_DRIVER: 1 if the device is using a TCC driver. TCC
        ///    is only available on Tesla hardware running Windows Vista or later;
        /// - ::CU_DEVICE_ATTRIBUTE_MEMORY_CLOCK_RATE: Peak memory clock frequency in kilohertz;
        /// - ::CU_DEVICE_ATTRIBUTE_GLOBAL_MEMORY_BUS_WIDTH: Global memory bus width in bits;
        /// - ::CU_DEVICE_ATTRIBUTE_L2_CACHE_SIZE: Size of L2 cache in bytes. 0 if the device doesn't have L2 cache;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_THREADS_PER_MULTIPROCESSOR: Maximum resident threads per multiprocessor;
        /// - ::CU_DEVICE_ATTRIBUTE_UNIFIED_ADDRESSING: 1 if the device shares a unified address space with
        ///   the host, or 0 if not;
        /// - ::CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MAJOR: Major compute capability version number;
        /// - ::CU_DEVICE_ATTRIBUTE_COMPUTE_CAPABILITY_MINOR: Minor compute capability version number;
        /// - ::CU_DEVICE_ATTRIBUTE_GLOBAL_L1_CACHE_SUPPORTED: 1 if device supports caching globals
        ///    in L1 cache, 0 if caching globals in L1 cache is not supported by the device;
        /// - ::CU_DEVICE_ATTRIBUTE_LOCAL_L1_CACHE_SUPPORTED: 1 if device supports caching locals
        ///    in L1 cache, 0 if caching locals in L1 cache is not supported by the device;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_MULTIPROCESSOR: Maximum amount of
        ///   shared memory available to a multiprocessor in bytes; this amount is shared
        ///   by all thread blocks simultaneously resident on a multiprocessor;
        /// - ::CU_DEVICE_ATTRIBUTE_MAX_REGISTERS_PER_MULTIPROCESSOR: Maximum number of 32-bit
        ///   registers available to a multiprocessor; this number is shared by all thread
        ///   blocks simultaneously resident on a multiprocessor;
        /// - ::CU_DEVICE_ATTRIBUTE_MANAGED_MEMORY: 1 if device supports allocating managed memory
        ///   on this system, 0 if allocating managed memory is not supported by the device on this system.
        /// - ::CU_DEVICE_ATTRIBUTE_MULTI_GPU_BOARD: 1 if device is on a multi-GPU board, 0 if not.
        /// - ::CU_DEVICE_ATTRIBUTE_MULTI_GPU_BOARD_GROUP_ID: Unique identifier for a group of devices
        ///   associated with the same board. Devices on the same multi-GPU board will share the same identifier.
        /// - ::CU_DEVICE_ATTRIBUTE_HOST_NATIVE_ATOMIC_SUPPORTED: 1 if Link between the device and the host
        ///   supports native atomic operations.
        /// - ::CU_DEVICE_ATTRIBUTE_SINGLE_TO_DOUBLE_PRECISION_PERF_RATIO: Ratio of single precision performance
        ///   (in floating-point operations per second) to double precision performance.
        /// - ::CU_DEVICE_ATTRIBUTE_PAGEABLE_MEMORY_ACCESS: Device suppports coherently accessing
        ///   pageable memory without calling cudaHostRegister on it.
        /// - ::CU_DEVICE_ATTRIBUTE_CONCURRENT_MANAGED_ACCESS: Device can coherently access managed memory
        ///   concurrently with the CPU.
        /// - ::CU_DEVICE_ATTRIBUTE_COMPUTE_PREEMPTION_SUPPORTED: Device supports Compute Preemption.
        /// - ::CU_DEVICE_ATTRIBUTE_CAN_USE_HOST_POINTER_FOR_REGISTERED_MEM: Device can access host registered
        ///   memory at the same virtual address as the CPU.
        /// -  ::CU_DEVICE_ATTRIBUTE_MAX_SHARED_MEMORY_PER_BLOCK_OPTIN: The maximum per block shared memory size
        ///    suported on this device. This is the maximum value that can be opted into when using the cuFuncSetAttribute() call.
        ///    For more details see ::CU_FUNC_ATTRIBUTE_MAX_DYNAMIC_SHARED_SIZE_BYTES</remarks>
        ///
        /// <param name="pi">Returned device attribute value</param>
        /// <param name="attrib">Device attribute to query</param>
        /// <param name="device">Device handle</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_DEVICE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDeviceGetCount,
        /// ::cuDeviceGetName,
        /// ::cuDeviceGet,
        /// ::cuDeviceTotalMem,
        /// ::cudaDeviceGetAttribute,
        /// ::cudaGetDeviceProperties
        /// CUresult CUDAAPI cuDeviceGetAttribute(int *pi, CUdevice_attribute attrib, CUdevice dev);
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetAttribute")]
        public static extern CuResult DeviceGetAttribute(out int pi, CuDeviceAttribute attrib, CuDevicePtr device);

        /// <summary>CUresult cuDeviceGetCount ( int* count )
        /// Returns the number of compute-capable devices.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetCount")]
        public static extern CuResult DeviceGetCount(out int count);

        /// <summary>CUresult cuDeviceGetLuid ( char* luid, unsigned int* deviceNodeMask, CUdevice dev )
        /// Return an LUID and device node mask for the device.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetLuid")]
        public static extern CuResult DeviceGetLuid(out byte luid, out uint deviceNodeMask, CuDevicePtr device);

        /// <summary>CUresult cuDeviceGetName ( char* name, int  len, CUdevice dev )
        /// Returns an identifer string for the device.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetName")]
        public static extern CuResult DeviceGetName(byte* name, int len, CuDevicePtr device);

		[DllImport(nvCudaPath, EntryPoint = "cuDeviceGetName")]
		public static extern CuResult DeviceGetName([MarshalAs(UnmanagedType.LPStr)] StringBuilder name, int len, CuDevicePtr device);

		/// <summary>CUresult cuDeviceGetNvSciSyncAttributes ( void* nvSciSyncAttrList, CUdevice dev, int  flags )
		/// Return NvSciSync attributes that this device can support.</summary>
		[DllImport(nvCudaPath, EntryPoint = "cuDeviceGetNvSciSyncAttributes")]
        public static extern CuResult DeviceGetNvSciSyncAttributes(IntPtr nvSciSyncAttrList, CuDevicePtr device, int flags);

        /// <summary>CUresult cuDeviceGetUuid ( CUuuid* uuid, CUdevice dev )
        /// Return an UUID for the device.</summary>
        // TODO: Does CUuuid == GUID?
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetUuid")]
        public static extern CuResult DeviceGetUuid(out Guid uuid, CuDevicePtr device);

        /// <summary>CUresult cuDeviceTotalMem ( size_t* bytes, CUdevice dev )
        /// Returns the total amount of memory on the device.</summary>
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceTotalMem" + _ver)]
		//public static extern CuResult DeviceTotalMemory(out IntPtr bytes, CuDevice device);
		public static extern CuResult DeviceTotalMemory(out long bytes, CuDevicePtr device);

		/// <summary>Retain the primary context on the GPU
		///
		/// Retains the primary context on the device, creating it if necessary,
		/// increasing its usage count. The caller must call
		/// ::cuDevicePrimaryCtxRelease() when done using the context.
		/// Unlike ::cuCtxCreate() the newly created context is not pushed onto the stack.
		///
		/// Context creation will fail with ::CUDA_ERROR_UNKNOWN if the compute mode of
		/// the device is ::CU_COMPUTEMODE_PROHIBITED.  The function ::cuDeviceGetAttribute()
		/// can be used with ::CU_DEVICE_ATTRIBUTE_COMPUTE_MODE to determine the compute mode
		/// of the device.
		/// The <i>nvidia-smi</i> tool can be used to set the compute mode for
		/// devices. Documentation for <i>nvidia-smi</i> can be obtained by passing a
		/// -h option to it.
		///
		/// Please note that the primary context always supports pinned allocations. Other
		/// flags can be specified by ::cuDevicePrimaryCtxSetFlags().</summary>
		///
		/// <param name="pctx">Returned context handle of the new context</param>
		/// <param name="dev">Device for which primary context is requested</param>
		///
		/// <returns>
		/// ::CUDA_SUCCESS,
		/// ::CUDA_ERROR_DEINITIALIZED,
		/// ::CUDA_ERROR_NOT_INITIALIZED,
		/// ::CUDA_ERROR_INVALID_CONTEXT,
		/// ::CUDA_ERROR_INVALID_DEVICE,
		/// ::CUDA_ERROR_INVALID_VALUE,
		/// ::CUDA_ERROR_OUT_OF_MEMORY,
		/// ::CUDA_ERROR_UNKNOWN
		/// </returns>
		/// \notefnerr
		///
		/// \sa ::cuDevicePrimaryCtxRelease,
		/// ::cuDevicePrimaryCtxSetFlags,
		/// ::cuCtxCreate,
		/// ::cuCtxGetApiVersion,
		/// ::cuCtxGetCacheConfig,
		/// ::cuCtxGetDevice,
		/// ::cuCtxGetFlags,
		/// ::cuCtxGetLimit,
		/// ::cuCtxPopCurrent,
		/// ::cuCtxPushCurrent,
		/// ::cuCtxSetCacheConfig,
		/// ::cuCtxSetLimit,
		/// ::cuCtxSynchronize
		/// CUresult CUDAAPI cuDevicePrimaryCtxRetain(CUcontext *pctx, CUdevice dev);
		[DllImport(nvCudaPath, EntryPoint = "cuDevicePrimaryCtxRetain")]
        public static extern CuResult DevicePrimaryCtxRetain(out CuContextPtr pctx, CuDevicePtr dev);

        /// <summary>Release the primary context on the GPU
        ///
        /// Releases the primary context interop on the device by decreasing the usage
        /// count by 1. If the usage drops to 0 the primary context of device <paramref name="dev"/>
        /// will be destroyed regardless of how many threads it is current to.
        ///
        /// Please note that unlike ::cuCtxDestroy() this method does not pop the context
        /// from stack in any circumstances.</summary>
        ///
        /// <param name="dev">Device which primary context is released</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_DEVICE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa ::cuDevicePrimaryCtxRetain,
        /// ::cuCtxDestroy,
        /// ::cuCtxGetApiVersion,
        /// ::cuCtxGetCacheConfig,
        /// ::cuCtxGetDevice,
        /// ::cuCtxGetFlags,
        /// ::cuCtxGetLimit,
        /// ::cuCtxPopCurrent,
        /// ::cuCtxPushCurrent,
        /// ::cuCtxSetCacheConfig,
        /// ::cuCtxSetLimit,
        /// ::cuCtxSynchronize
        /// CUresult CUDAAPI cuDevicePrimaryCtxRelease(CUdevice dev);
        [DllImport(nvCudaPath, EntryPoint = "cuDevicePrimaryCtxRelease")]
        public static extern CuResult DevicePrimaryCtxRelease(CuDevicePtr dev);

        /// <summary>Set flags for the primary context
        ///
        /// Sets the flags for the primary context on the device overwriting perviously
        /// set ones. If the primary context is already created
        /// ::CUDA_ERROR_PRIMARY_CONTEXT_ACTIVE is returned.
        ///
        /// The three LSBs of the <paramref name="flags"/> parameter can be used to control how the OS
        /// thread, which owns the CUDA context at the time of an API call, interacts
        /// with the OS scheduler when waiting for results from the GPU. Only one of
        /// the scheduling flags can be set when creating a context.
        ///
        /// - ::CU_CTX_SCHED_SPIN: Instruct CUDA to actively spin when waiting for
        /// results from the GPU. This can decrease latency when waiting for the GPU,
        /// but may lower the performance of CPU threads if they are performing work in
        /// parallel with the CUDA thread.
        ///
        /// - ::CU_CTX_SCHED_YIELD: Instruct CUDA to yield its thread when waiting for
        /// results from the GPU. This can increase latency when waiting for the GPU,
        /// but can increase the performance of CPU threads performing work in parallel
        /// with the GPU.
        ///
        /// - ::CU_CTX_SCHED_BLOCKING_SYNC: Instruct CUDA to block the CPU thread on a
        /// synchronization primitive when waiting for the GPU to finish work.
        ///
        /// - ::CU_CTX_BLOCKING_SYNC: Instruct CUDA to block the CPU thread on a
        /// synchronization primitive when waiting for the GPU to finish work.
        /// <b>Deprecated:</b> This flag was deprecated as of CUDA 4.0 and was
        /// replaced with ::CU_CTX_SCHED_BLOCKING_SYNC.
        ///
        /// - ::CU_CTX_SCHED_AUTO: The default value if the <paramref name="flags"/> parameter is zero,
        /// uses a heuristic based on the number of active CUDA contexts in the
        /// process \e C and the number of logical processors in the system \e P. If
        /// \e C > \e P, then CUDA will yield to other OS threads when waiting for
        /// the GPU (::CU_CTX_SCHED_YIELD), otherwise CUDA will not yield while
        /// waiting for results and actively spin on the processor (::CU_CTX_SCHED_SPIN).
        /// However, on low power devices like Tegra, it always defaults to
        /// ::CU_CTX_SCHED_BLOCKING_SYNC.
        ///
        /// - ::CU_CTX_LMEM_RESIZE_TO_MAX: Instruct CUDA to not reduce local memory
        /// after resizing local memory for a kernel. This can prevent thrashing by
        /// local memory allocations when launching many kernels with high local
        /// memory usage at the cost of potentially increased memory usage.</summary>
        ///
        /// <param name="dev">Device for which the primary context flags are set</param>
        /// <param name="flags">New flags for the device</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_DEVICE,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_PRIMARY_CONTEXT_ACTIVE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa ::cuDevicePrimaryCtxRetain,
        /// ::cuDevicePrimaryCtxGetState,
        /// ::cuCtxCreate,
        /// ::cuCtxGetFlags,
        /// ::cudaSetDeviceFlags
        /// CUresult CUDAAPI cuDevicePrimaryCtxSetFlags(CUdevice dev, unsigned int flags);
        [DllImport(nvCudaPath, EntryPoint = "cuDevicePrimaryCtxSetFlags")]
        public static extern CuResult DevicePrimaryCtxSetFlags(CuDevicePtr dev, CuContextFlags flags);

        /// <summary>Get the state of the primary context
        ///
        /// Returns in *<paramref name="flags"/> the flags for the primary context of <paramref name="dev"/>, and in
        /// *<paramref name="active"/> whether it is active.  See ::cuDevicePrimaryCtxSetFlags for flag
        /// values.</summary>
        ///
        /// <param name="dev">Device to get primary context flags for</param>
        /// <param name="flags">Pointer to store flags</param>
        /// <param name="active">Pointer to store context state; 0 = inactive, 1 = active</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_DEVICE,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDevicePrimaryCtxSetFlags,
        /// ::cuCtxGetFlags,
        /// ::cudaGetDeviceFlags
        /// CUresult CUDAAPI cuDevicePrimaryCtxGetState(CUdevice dev, unsigned int *flags, int *active);
        [DllImport(nvCudaPath, EntryPoint = "cuDevicePrimaryCtxGetState")]
        public static extern CuResult DevicePrimaryCtxGetState(CuDevicePtr dev, out CuContextFlags flags, out bool active);

        /// <summary>Destroy all allocations and reset all state on the primary context
        ///
        /// Explicitly destroys and cleans up all resources associated with the current
        /// device in the current process.
        ///
        /// Note that it is responsibility of the calling function to ensure that no
        /// other module in the process is using the device any more. For that reason
        /// it is recommended to use ::cuDevicePrimaryCtxRelease() in most cases.
        /// However it is safe for other modules to call ::cuDevicePrimaryCtxRelease()
        /// even after resetting the device.</summary>
        ///
        /// <param name="dev">Device for which primary context is destroyed</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_DEVICE,
        /// ::CUDA_ERROR_PRIMARY_CONTEXT_ACTIVE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa ::cuDevicePrimaryCtxRetain,
        /// ::cuDevicePrimaryCtxRelease,
        /// ::cuCtxGetApiVersion,
        /// ::cuCtxGetCacheConfig,
        /// ::cuCtxGetDevice,
        /// ::cuCtxGetFlags,
        /// ::cuCtxGetLimit,
        /// ::cuCtxPopCurrent,
        /// ::cuCtxPushCurrent,
        /// ::cuCtxSetCacheConfig,
        /// ::cuCtxSetLimit,
        /// ::cuCtxSynchronize,
        /// ::cudaDeviceReset
        /// CUresult CUDAAPI cuDevicePrimaryCtxReset(CUdevice dev);
        [DllImport(nvCudaPath, EntryPoint = "cuDevicePrimaryCtxReset")]
        public static extern CuResult DevicePrimaryCtxReset(CuDevicePtr dev);

        /// <summary>Returns a handle to a compute device
        ///
        /// Returns in *<paramref name="dev"/> a device handle given a PCI bus ID string.</summary>
        ///
        /// <param name="dev">Returned device handle</param>
        /// <param name="pciBusId">String in one of the following forms:
        /// [domain]:[bus]:[device].[function]
        /// [domain]:[bus]:[device]
        /// [bus]:[device].[function]
        /// where <c>domain</c>, <c>bus</c>, <c>device</c>, and <c>function</c> are all hexadecimal values</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_DEVICE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDeviceGet,
        /// ::cuDeviceGetAttribute,
        /// ::cuDeviceGetPCIBusId,
        /// ::cudaDeviceGetByPCIBusId
        /// CUresult CUDAAPI cuDeviceGetByPCIBusId(CUdevice *dev, const char *pciBusId);
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetByPCIBusId", CharSet = CharSet.Ansi)]
        public static extern CuResult DeviceGetByPCIBusId(out CuDevicePtr dev, string pciBusId);

        /// <summary>Returns a PCI Bus Id string for the device
        ///
        /// Returns an ASCII string identifying the device <paramref name="dev"/> in the NULL-terminated
        /// string pointed to by <paramref name="pciBusId"/>. <paramref name="len"/> specifies the maximum length of the
        /// string that may be returned.</summary>
        ///
        /// <param name="pciBusId">Returned identifier string for the device in the following format
        /// [domain]:[bus]:[device].[function]
        /// where <c>domain</c>, <c>bus</c>, <c>device</c>, and <c>function</c> are all hexadecimal values.
        /// pciBusId should be large enough to store 13 characters including the NULL-terminator.</param>
        /// <param name="len">Maximum length of string to store in <paramref name="pciBusId"/></param>
        /// <param name="dev">Device to get identifier string for</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_VALUE,
        /// ::CUDA_ERROR_INVALID_DEVICE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDeviceGet,
        /// ::cuDeviceGetAttribute,
        /// ::cuDeviceGetByPCIBusId,
        /// ::cudaDeviceGetPCIBusId
        /// CUresult CUDAAPI cuDeviceGetPCIBusId(char *pciBusId, int len, CUdevice dev);
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetPCIBusId", CharSet = CharSet.Ansi)]
        public static extern CuResult DeviceGetPCIBusId(byte* pciBusId, int len, CuDevicePtr dev);

		[DllImport(nvCudaPath, EntryPoint = "cuDeviceGetPCIBusId", CharSet = CharSet.Ansi)]
		public static extern CuResult DeviceGetPCIBusId([MarshalAs(UnmanagedType.LPStr)] StringBuilder pciBusId, int len, CuDevicePtr dev);

		#region Peer access
		/// <summary>Queries if a device may directly access a peer device's memory.
		///
		/// Returns in *<paramref name="canAccessPeer"/> a value of 1 if contexts on <paramref name="dev"/> are capable of
		/// directly accessing memory from contexts on <paramref name="peerDev"/> and 0 otherwise.
		/// If direct access of <paramref name="peerDev"/> from <paramref name="dev"/> is possible, then access may be
		/// enabled on two specific contexts by calling ::cuCtxEnablePeerAccess().</summary>
		///
		/// <param name="canAccessPeer">Returned access capability</param>
		/// <param name="dev">Device from which allocations on <paramref name="peerDev"/> are to
		///                        be directly accessed.</param>
		/// <param name="peerDev">Device on which the allocations to be directly accessed
		///                        by <paramref name="dev"/> reside.</param>
		///
		/// <returns>
		/// ::CUDA_SUCCESS,
		/// ::CUDA_ERROR_DEINITIALIZED,
		/// ::CUDA_ERROR_NOT_INITIALIZED,
		/// ::CUDA_ERROR_INVALID_DEVICE
		/// </returns>
		/// \notefnerr
		///
		/// \sa
		/// ::cuCtxEnablePeerAccess,
		/// ::cuCtxDisablePeerAccess,
		/// ::cudaDeviceCanAccessPeer
		/// CUresult CUDAAPI cuDeviceCanAccessPeer(int *canAccessPeer, CUdevice dev, CUdevice peerDev);
		[DllImport(nvCudaPath, EntryPoint = "cuDeviceCanAccessPeer")]
        public static extern CuResult DeviceCanAccessPeer(out bool canAccessPeer, CuDevicePtr dev, CuDevicePtr peerDev);

        /// <summary>Enables direct access to memory allocations in a peer context.
        ///
        /// If both the current context and <paramref name="peerContext"/> are on devices which support unified
        /// addressing (as may be queried using ::CU_DEVICE_ATTRIBUTE_UNIFIED_ADDRESSING) and same
        /// major compute capability, then on success all allocations from <paramref name="peerContext"/> will
        /// immediately be accessible by the current context.  See \ref CUDA_UNIFIED for additional
        /// details.
        ///
        /// Note that access granted by this call is unidirectional and that in order to access
        /// memory from the current context in <paramref name="peerContext"/>, a separate symmetric call
        /// to ::cuCtxEnablePeerAccess() is required.
        ///
        /// There is a system-wide maximum of eight peer connections per device.
        ///
        /// Returns ::CUDA_ERROR_PEER_ACCESS_UNSUPPORTED if ::cuDeviceCanAccessPeer() indicates
        /// that the ::CUdevice of the current context cannot directly access memory
        /// from the ::CUdevice of <paramref name="peerContext."/>
        ///
        /// Returns ::CUDA_ERROR_PEER_ACCESS_ALREADY_ENABLED if direct access of
        /// <paramref name="peerContext"/> from the current context has already been enabled.
        ///
        /// Returns ::CUDA_ERROR_TOO_MANY_PEERS if direct peer access is not possible
        /// because hardware resources required for peer access have been exhausted.
        ///
        /// Returns ::CUDA_ERROR_INVALID_CONTEXT if there is no current context, <paramref name="peerContext"/>
        /// is not a valid context, or if the current context is <paramref name="peerContext."/>
        ///
        /// Returns ::CUDA_ERROR_INVALID_VALUE if <paramref name="flags"/> is not 0.</summary>
        ///
        /// <param name="peerContext">Peer context to enable direct access to from the current context</param>
        /// <param name="flags">Reserved for future use and must be set to 0</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_PEER_ACCESS_ALREADY_ENABLED,
        /// ::CUDA_ERROR_TOO_MANY_PEERS,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// ::CUDA_ERROR_PEER_ACCESS_UNSUPPORTED,
        /// ::CUDA_ERROR_INVALID_VALUE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDeviceCanAccessPeer,
        /// ::cuCtxDisablePeerAccess,
        /// ::cudaDeviceEnablePeerAccess
        /// CUresult CUDAAPI cuCtxEnablePeerAccess(CUcontext peerContext, unsigned int Flags);
        [DllImport(nvCudaPath, EntryPoint = "cuCtxEnablePeerAccess")]
        public static extern CuResult CtxEnablePeerAccess(CuContextPtr peerContext, int flags = 0);

        /// <summary>Disables direct access to memory allocations in a peer context and
        /// unregisters any registered allocations.
        ///
        /// Returns ::CUDA_ERROR_PEER_ACCESS_NOT_ENABLED if direct peer access has
        /// not yet been enabled from <paramref name="peerContext"/> to the current context.
        ///
        /// Returns ::CUDA_ERROR_INVALID_CONTEXT if there is no current context, or if
        /// <paramref name="peerContext"/> is not a valid context.</summary>
        ///
        /// <param name="peerContext">Peer context to disable direct access to</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_PEER_ACCESS_NOT_ENABLED,
        /// ::CUDA_ERROR_INVALID_CONTEXT,
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuDeviceCanAccessPeer,
        /// ::cuCtxEnablePeerAccess,
        /// ::cudaDeviceDisablePeerAccess
        /// CUresult CUDAAPI cuCtxDisablePeerAccess(CUcontext peerContext);
        [DllImport(nvCudaPath, EntryPoint = "cuCtxDisablePeerAccess")]
        public static extern CuResult CtxDisablePeerAccess(CuContextPtr peerContext);

        /// <summary>Queries attributes of the link between two devices.
        ///
        /// Returns in *<paramref name="value"/> the value of the requested attribute <paramref name="attrib"/> of the
        /// link between <paramref name="srcDevice"/> and <paramref name="dstDevice"/>. The supported attributes are:
        /// - ::CU_DEVICE_P2P_ATTRIBUTE_PERFORMANCE_RANK: A relative value indicating the
        ///   performance of the link between two devices.
        /// - ::CU_DEVICE_P2P_ATTRIBUTE_ACCESS_SUPPORTED P2P: 1 if P2P Access is enable.
        /// - ::CU_DEVICE_P2P_ATTRIBUTE_NATIVE_ATOMIC_SUPPORTED: 1 if Atomic operations over
        ///   the link are supported.
        ///
        /// Returns ::CUDA_ERROR_INVALID_DEVICE if <paramref name="srcDevice"/> or <paramref name="dstDevice"/> are not valid
        /// or if they represent the same device.
        ///
        /// Returns ::CUDA_ERROR_INVALID_VALUE if <paramref name="attrib"/> is not valid or if <paramref name="value"/> is
        /// a null pointer.</summary>
        ///
        /// <param name="value">Returned value of the requested attribute</param>
        /// <param name="attrib">The requested attribute of the link between <paramref name="srcDevice"/> and <paramref name="dstDevice"/>.</param>
        /// <param name="srcDevice">The source device of the target link.</param>
        /// <param name="dstDevice">The destination device of the target link.</param>
        ///
        /// <returns>
        /// ::CUDA_SUCCESS,
        /// ::CUDA_ERROR_DEINITIALIZED,
        /// ::CUDA_ERROR_NOT_INITIALIZED,
        /// ::CUDA_ERROR_INVALID_DEVICE,
        /// ::CUDA_ERROR_INVALID_VALUE
        /// </returns>
        /// \notefnerr
        ///
        /// \sa
        /// ::cuCtxEnablePeerAccess,
        /// ::cuCtxDisablePeerAccess,
        /// ::cuDeviceCanAccessPeer,
        /// ::cudaDeviceGetP2PAttribute
        /// CUresult CUDAAPI cuDeviceGetP2PAttribute(int* value, CUdevice_P2PAttribute attrib, CUdevice srcDevice, CUdevice dstDevice);
        [DllImport(nvCudaPath, EntryPoint = "cuDeviceGetP2PAttribute")]
        public static extern CuResult DeviceGetP2PAttribute(out int value, DeviceP2PAttribute attrib, CuDevicePtr srcDevice, CuDevicePtr dstDevice);
        #endregion
    }
}