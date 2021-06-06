using System;

// ReSharper disable UnusedMember.Global

namespace MediaToolkit.Nvidia
{
	public enum CuCallbackResult
	{
		Failure = 0,
		Success = 1
	}

	public enum CuResult
	{
		/// <summary>
		/// The API call returned with no errors. In the case of query calls, this
		/// can also mean that the operation being queried is complete (see
		/// ::cuEventQuery() and ::cuStreamQuery()).
		/// </summary>
		Success = 0,

		/// <summary>
		/// This indicates that one or more of the parameters passed to the API call
		/// is not within an acceptable range of values.
		/// </summary>
		ErrorInvalidValue = 1,

		/// <summary>
		/// The API call failed because it was unable to allocate enough memory to
		/// perform the requested operation.
		/// </summary>
		ErrorOutOfMemory = 2,

		/// <summary>
		/// This indicates that the CUDA driver has not been initialized with
		/// ::cuInit() or that initialization has failed.
		/// </summary>
		ErrorNotInitialized = 3,

		/// <summary>
		/// This indicates that the CUDA driver is in the process of shutting down.
		/// </summary>
		ErrorDeinitialized = 4,

		/// <summary>
		/// This indicates profiler is not initialized for this run. This can
		/// happen when the application is running with external profiling tools
		/// like visual profiler.
		/// </summary>
		ErrorProfilerDisabled = 5,

		/// <summary>
		/// \deprecated
		/// This error return is deprecated as of CUDA 5.0. It is no longer an error
		/// to attempt to enable/disable the profiling via ::cuProfilerStart or
		/// ::cuProfilerStop without initialization.
		/// </summary>
		ErrorProfilerNotInitialized = 6,

		/// <summary>
		/// \deprecated
		/// This error return is deprecated as of CUDA 5.0. It is no longer an error
		/// to call cuProfilerStart() when profiling is already enabled.
		/// </summary>
		ErrorProfilerAlreadyStarted = 7,

		/// <summary>
		/// \deprecated
		/// This error return is deprecated as of CUDA 5.0. It is no longer an error
		/// to call cuProfilerStop() when profiling is already disabled.
		/// </summary>
		ErrorProfilerAlreadyStopped = 8,

		/// <summary>
		/// This indicates that no CUDA-capable devices were detected by the installed
		/// CUDA driver.
		/// </summary>
		ErrorNoDevice = 100,

		/// <summary>
		/// This indicates that the device ordinal supplied by the user does not
		/// correspond to a valid CUDA device.
		/// </summary>
		ErrorInvalidDevice = 101,

		/// <summary>
		/// This indicates that the device kernel image is invalid. This can also
		/// indicate an invalid CUDA module.
		/// </summary>
		ErrorInvalidImage = 200,

		/// <summary>
		/// This most frequently indicates that there is no context bound to the
		/// current thread. This can also be returned if the context passed to an
		/// API call is not a valid handle (such as a context that has had
		/// ::cuCtxDestroy() invoked on it). This can also be returned if a user
		/// mixes different API versions (i.e. 3010 context with 3020 API calls).
		/// See ::cuCtxGetApiVersion() for more details.
		/// </summary>
		ErrorInvalidContext = 201,

		/// <summary>
		/// This indicated that the context being supplied as a parameter to the
		/// API call was already the active context.
		/// \deprecated
		/// This error return is deprecated as of CUDA 3.2. It is no longer an
		/// error to attempt to push the active context via ::cuCtxPushCurrent().
		/// </summary>
		ErrorContextAlreadyCurrent = 202,

		/// <summary>
		/// This indicates that a map or register operation has failed.
		/// </summary>
		ErrorMapFailed = 205,

		/// <summary>
		/// This indicates that an unmap or unregister operation has failed.
		/// </summary>
		ErrorUnmapFailed = 206,

		/// <summary>
		/// This indicates that the specified array is currently mapped and thus
		/// cannot be destroyed.
		/// </summary>
		ErrorArrayIsMapped = 207,

		/// <summary>
		/// This indicates that the resource is already mapped.
		/// </summary>
		ErrorAlreadyMapped = 208,

		/// <summary>
		/// This indicates that there is no kernel image available that is suitable
		/// for the device. This can occur when a user specifies code generation
		/// options for a particular CUDA source file that do not include the
		/// corresponding device configuration.
		/// </summary>
		ErrorNoBinaryForGpu = 209,

		/// <summary>
		/// This indicates that a resource has already been acquired.
		/// </summary>
		ErrorAlreadyAcquired = 210,

		/// <summary>
		/// This indicates that a resource is not mapped.
		/// </summary>
		ErrorNotMapped = 211,

		/// <summary>
		/// This indicates that a mapped resource is not available for access as an
		/// array.
		/// </summary>
		ErrorNotMappedAsArray = 212,

		/// <summary>
		/// This indicates that a mapped resource is not available for access as a
		/// pointer.
		/// </summary>
		ErrorNotMappedAsPointer = 213,

		/// <summary>
		/// This indicates that an uncorrectable ECC error was detected during
		/// execution.
		/// </summary>
		ErrorEccUncorrectable = 214,

		/// <summary>
		/// This indicates that the ::CUlimit passed to the API call is not
		/// supported by the active device.
		/// </summary>
		ErrorUnsupportedLimit = 215,

		/// <summary>
		/// This indicates that the ::CUcontext passed to the API call can
		/// only be bound to a single CPU thread at a time but is already
		/// bound to a CPU thread.
		/// </summary>
		ErrorContextAlreadyInUse = 216,

		/// <summary>
		/// This indicates that peer access is not supported across the given
		/// devices.
		/// </summary>
		ErrorPeerAccessUnsupported = 217,

		/// <summary>
		/// This indicates that the device kernel source is invalid.
		/// </summary>
		ErrorInvalidSource = 300,

		/// <summary>
		/// This indicates that the file specified was not found.
		/// </summary>
		ErrorFileNotFound = 301,

		/// <summary>
		/// This indicates that a link to a shared object failed to resolve.
		/// </summary>
		ErrorSharedObjectSymbolNotFound = 302,

		/// <summary>
		/// This indicates that initialization of a shared object failed.
		/// </summary>
		ErrorSharedObjectInitFailed = 303,

		/// <summary>
		/// This indicates that an OS call failed.
		/// </summary>
		ErrorOperatingSystem = 304,

		/// <summary>
		/// This indicates that a resource handle passed to the API call was not
		/// valid. Resource handles are opaque types like ::CUstream and ::CUevent.
		/// </summary>
		ErrorInvalidHandle = 400,

		/// <summary>
		/// This indicates that a named symbol was not found. Examples of symbols
		/// are global/constant variable names, texture names, and surface names.
		/// </summary>
		ErrorNotFound = 500,

		/// <summary>
		/// This indicates that asynchronous operations issued previously have not
		/// completed yet. This result is not actually an error, but must be indicated
		/// differently than ::CUDA_SUCCESS (which indicates completion). Calls that
		/// may return this value include ::cuEventQuery() and ::cuStreamQuery().
		/// </summary>
		ErrorNotReady = 600,

		/// <summary>
		/// An exception occurred on the device while executing a kernel. Common
		/// causes include dereferencing an invalid device pointer and accessing
		/// out of bounds shared memory. The context cannot be used, so it must
		/// be destroyed (and a new one should be created). All existing device
		/// memory allocations from this context are invalid and must be
		/// reconstructed if the program is to continue using CUDA.
		/// </summary>
		ErrorLaunchFailed = 700,

		/// <summary>
		/// This indicates that a launch did not occur because it did not have
		/// appropriate resources. This error usually indicates that the user has
		/// attempted to pass too many arguments to the device kernel, or the
		/// kernel launch specifies too many threads for the kernel's register
		/// count. Passing arguments of the wrong size (i.e. a 64-bit pointer
		/// when a 32-bit int is expected) is equivalent to passing too many
		/// arguments and can also result in this error.
		/// </summary>
		ErrorLaunchOutOfResources = 701,

		/// <summary>
		/// This indicates that the device kernel took too long to execute. This can
		/// only occur if timeouts are enabled - see the device attribute
		/// ::CU_DEVICE_ATTRIBUTE_KERNEL_EXEC_TIMEOUT for more information. The
		/// context cannot be used (and must be destroyed similar to
		/// ::CUDA_ERROR_LAUNCH_FAILED). All existing device memory allocations from
		/// this context are invalid and must be reconstructed if the program is to
		/// continue using CUDA.
		/// </summary>
		ErrorLaunchTimeout = 702,

		/// <summary>
		/// This error indicates a kernel launch that uses an incompatible texturing
		/// mode.
		/// </summary>
		ErrorLaunchIncompatibleTexturing = 703,

		/// <summary>
		/// This error indicates that a call to ::cuCtxEnablePeerAccess() is
		/// trying to re-enable peer access to a context which has already
		/// had peer access to it enabled.
		/// </summary>
		ErrorPeerAccessAlreadyEnabled = 704,

		/// <summary>
		/// This error indicates that ::cuCtxDisablePeerAccess() is
		/// trying to disable peer access which has not been enabled yet
		/// via ::cuCtxEnablePeerAccess().
		/// </summary>
		ErrorPeerAccessNotEnabled = 705,

		/// <summary>
		/// This error indicates that the primary context for the specified device
		/// has already been initialized.
		/// </summary>
		ErrorPrimaryContextActive = 708,

		/// <summary>
		/// This error indicates that the context current to the calling thread
		/// has been destroyed using ::cuCtxDestroy, or is a primary context which
		/// has not yet been initialized.
		/// </summary>
		ErrorContextIsDestroyed = 709,

		/// <summary>
		/// A device-side assert triggered during kernel execution. The context
		/// cannot be used anymore, and must be destroyed. All existing device
		/// memory allocations from this context are invalid and must be
		/// reconstructed if the program is to continue using CUDA.
		/// </summary>
		ErrorAssert = 710,

		/// <summary>
		/// This error indicates that the hardware resources required to enable
		/// peer access have been exhausted for one or more of the devices
		/// passed to ::cuCtxEnablePeerAccess().
		/// </summary>
		ErrorTooManyPeers = 711,

		/// <summary>
		/// This error indicates that the memory range passed to ::cuMemHostRegister()
		/// has already been registered.
		/// </summary>
		ErrorHostMemoryAlreadyRegistered = 712,

		/// <summary>
		/// This error indicates that the pointer passed to ::cuMemHostUnregister()
		/// does not correspond to any currently registered memory region.
		/// </summary>
		ErrorHostMemoryNotRegistered = 713,

		/// <summary>
		/// This error indicates that the attempted operation is not permitted.
		/// </summary>
		ErrorNotPermitted = 800,

		/// <summary>
		/// This error indicates that the attempted operation is not supported
		/// on the current system or device.
		/// </summary>
		ErrorNotSupported = 801,

		/// <summary>
		/// This indicates that an unknown internal error has occurred.
		/// </summary>
		ErrorUnknown = 999
	}




}
