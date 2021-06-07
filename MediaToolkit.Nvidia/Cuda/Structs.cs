using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using static MediaToolkit.Nvidia.LibCuda;
//using static MediaToolkit.Nvidia.LibCuVideo;

#pragma warning disable 169

namespace MediaToolkit.Nvidia
{

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public unsafe struct CuDevice
	{
		public static readonly CuDevice Empty = new CuDevice { Handle = 0 };

		public int Handle;
		public bool IsEmpty => Handle == 0;

		public CuDevice(int handle)
		{
			Handle = handle;
		}

		/// <summary>CU_DEVICE_CPU
		/// Device that represents the CPU</summary>
		public static readonly CuDevice DeviceCpu = new CuDevice(-1);

		/// <summary>CU_DEVICE_INVALID
		/// Device that represents an invalid device</summary>
		public static readonly CuDevice DeviceInvalid = new CuDevice(-2);

		/// <inheritdoc cref="LibCuda.DeviceGet(out CuDevice, int)"/>
		public static CuDevice GetDevice(int ordinal)
		{
			var result = DeviceGet(out var device, ordinal);
			CheckResult(result);

			return device;
		}

		/// <inheritdoc cref="LibCuda.DeviceGetCount(out int)"/>
		public static int GetCount()
		{
			var result = DeviceGetCount(out var count);
			CheckResult(result);

			return count;
		}

		public static IEnumerable<CuDeviceDescription> GetDescriptions()
		{
			var count = GetCount();

			for (var i = 0; i < count; ++i)
			{
				yield return new CuDeviceDescription(new CuDevice(i));
			}
		}

		/// <inheritdoc cref="LibCuda.DeviceGetPCIBusId(byte*, int, CuDevice)"/>
		public string GetPciBusId()
		{
			const int length = 20;
			var namePtr = stackalloc byte[length];
			Kernel32.ZeroMemory(namePtr, length);
			var result = DeviceGetPCIBusId(namePtr, length, this);

			CheckResult(result);
			var pciBusId = Marshal.PtrToStringAnsi((IntPtr)namePtr);

			return pciBusId;

			// return Marshal.PtrToStringAnsi((IntPtr) namePtr, length);
		}

		/// <inheritdoc cref="LibCuda.D3D11GetDevice(out CuDevice, IntPtr)"/>
		public static CuDevice GetD3D11Device(IntPtr adapter)
		{
			var result = D3D11GetDevice(out var device, adapter);
			CheckResult(result);

			return device;
		}

		/// <inheritdoc cref="LibCuda.DeviceGetName(byte*, int, CuDevice)"/>
		public string GetName()
		{
			const int inputLength = 256;
			var name = stackalloc byte[inputLength];
			Kernel32.ZeroMemory(name, inputLength);

			var result = DeviceGetName(name, inputLength, this);
			CheckResult(result);

			var nameStr = Marshal.PtrToStringAnsi((IntPtr)name);

			return nameStr;
			//return Marshal.PtrToStringAnsi((IntPtr)name, inputLength);
		}

		/// <inheritdoc cref="LibCuda.DeviceTotalMemory(out IntPtr, CuDevice)"/>
		public long GetTotalMemory()
		{
			var result = DeviceTotalMemory(out var memorySize, this);
			CheckResult(result);
			return memorySize;
			//return memorySize.ToInt64();
		}

		public CuDeviceDescription GetDescription()
		{
			return new CuDeviceDescription(this);
		}

		/// <inheritdoc cref="LibCuda.DeviceGetAttribute(out int, CuDeviceAttribute, CuDevice)"/>
		public int GetAttribute(CuDeviceAttribute attribute)
		{
			var result = DeviceGetAttribute(out var output, attribute, this);
			CheckResult(result);

			return output;
		}

		/// <inheritdoc cref="LibCuda.CtxCreate(out CuContext, CuContextFlags, CuDevice)"/>
		public CuContext CreateContext(CuContextFlags flags = CuContextFlags.SchedAuto)
		{
			var result = CtxCreate(out var ctx, flags, this);
			CheckResult(result);

			return ctx;
		}
	}

	public readonly struct CuDeviceDescription
	{
		public readonly CuDevice Device;
		public readonly string Name;
		public readonly long TotalMemory;

		public int Handle => Device.Handle;

		public CuDeviceDescription(CuDevice device)
		{
			Device = device;
			Name = device.GetName();
			TotalMemory = device.GetTotalMemory();
		}

		/// <inheritdoc cref="LibCuda.DeviceGetAttribute(out int, CuDeviceAttribute, CuDevice)"/>
		public int GetAttribute(CuDeviceAttribute attribute) => Device.GetAttribute(attribute);
		/// <inheritdoc cref="LibCuda.DeviceGetPCIBusId(byte*, int, CuDevice)"/>
		public string GetPciBusId() => Device.GetPciBusId();
	}


	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuEvent
	{
		public static readonly CuEvent Empty = new CuEvent { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public unsafe struct CuDevicePtr
	{
		public static readonly CuDevicePtr Empty = new CuDevicePtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

		public CuDevicePtr(IntPtr handle)
		{
			Handle = handle;
		}

		public CuDevicePtr(byte* handle)
		{
			Handle = (IntPtr)handle;
		}

		public CuDevicePtr(long handle)
		{
			Handle = (IntPtr)handle;
		}

		public static implicit operator ulong(CuDevicePtr d) => (ulong)d.Handle.ToInt64();
		public static implicit operator CuDevicePtr(byte* d) => new CuDevicePtr(d);
	}



	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public partial struct CuContext : IDisposable
	{
		public static readonly CuContext Empty = new CuContext { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

		public struct CuContextPush : IDisposable
		{
			private CuContext _context;
			private int _disposed;

			internal CuContextPush(CuContext context)
			{
				_context = context;
				_disposed = 0;
			}

			/// <inheritdoc cref="CtxPopCurrent(out CuContext)"/>
			public void Dispose()
			{
				var disposed = Interlocked.Exchange(ref _disposed, 1);
				if (disposed != 0) return;

				CtxPopCurrent(out _);
			}
		}


		/// <inheritdoc cref="CtxPushCurrent(CuContext)"/>
		public CuContextPush Push()
		{
			var result = CtxPushCurrent(this);
			CheckResult(result);

			return new CuContextPush(this);
		}

		/// <inheritdoc cref="CtxSetCurrent(CuContext)"/>
		public void SetCurrent()
		{
			var result = CtxSetCurrent(this);
			CheckResult(result);
		}

		/// <inheritdoc cref="CtxGetApiVersion(CuContext, out uint)"/>
		public uint GetApiVersion()
		{
			var result = CtxGetApiVersion(this, out var version);
			CheckResult(result);

			return version;
		}

		/// <inheritdoc cref="CtxGetDevice(out CuDevice)"/>
		public CuDevice GetDevice()
		{
			//using var _ = Push();
			var context = Push();

			var result = CtxGetDevice(out var device);
			CheckResult(result);
			context.Dispose();

			return device;
		}

		/// <inheritdoc cref="CtxGetCurrent(out CuContext)"/>
		public static CuContext GetCurrent()
		{
			var result = CtxGetCurrent(out var ctx);
			CheckResult(result);

			return ctx;
		}

		/// <inheritdoc cref="CtxGetSharedMemConfig(out SharedMemoryConfig)"/>
		public static SharedMemoryConfig GetSharedMemConfig()
		{
			var result = CtxGetSharedMemConfig(out var config);
			CheckResult(result);

			return config;
		}

		/// <inheritdoc cref="CtxSetSharedMemConfig(SharedMemoryConfig)"/>
		public static void SetSharedMemConfig(SharedMemoryConfig config)
		{
			var result = CtxSetSharedMemConfig(config);
			CheckResult(result);
		}

		/// <inheritdoc cref="CtxGetCacheConfig(out CuFunctionCache)"/>
		public static CuFunctionCache GetCacheConfig()
		{
			var result = CtxGetCacheConfig(out var config);
			CheckResult(result);

			return config;
		}

		/// <inheritdoc cref="CtxSetCacheConfig(CuFunctionCache)"/>
		public static void SetCacheConfig(CuFunctionCache config)
		{
			var result = CtxSetCacheConfig(config);
			CheckResult(result);
		}

		/// <inheritdoc cref="CtxGetDevice(out CuDevice)"/>
		public static CuDevice GetCurrentDevice()
		{
			var result = CtxGetDevice(out var device);
			CheckResult(result);

			return device;
		}

		/// <inheritdoc cref="CtxDestroy(CuContext)"/>
		public void Dispose()
		{
			var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
			if (handle == IntPtr.Zero) return;
			var obj = new CuContext { Handle = handle };

			CtxDestroy(obj);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuGraphicsResource : IDisposable
	{
		public static readonly CuGraphicsResource Empty = new CuGraphicsResource { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

		/// <inheritdoc cref="GraphicsD3D11RegisterResource(out CuGraphicsResource, IntPtr, CuGraphicsRegisters)"/>
		public static CuGraphicsResource Register(IntPtr resourcePtr, CuGraphicsRegisters flags = CuGraphicsRegisters.None)
		{
			var result = GraphicsD3D11RegisterResource(out var resource, resourcePtr, flags);

			CheckResult(result);

			return resource;
		}

		/// <inheritdoc cref="GraphicsResourceSetMapFlags(CuGraphicsResource, CuGraphicsMapResources)"/>
		public void SetMapFlags(CuGraphicsMapResources flags)
		{
			var result = GraphicsResourceSetMapFlags(this, flags);
			CheckResult(result);
		}

		/// <inheritdoc cref="GraphicsMapResources(int, CuGraphicsResource*, CuStream)"/>
		public CuGraphicsMappedResource Map()
		{
			return Map(CuStream.Empty);
		}

		/// <inheritdoc cref="GraphicsMapResources(int, CuGraphicsResource*, CuStream)"/>
		public unsafe CuGraphicsMappedResource Map(CuStream stream)
		{
			var copy = this;
			var result = GraphicsMapResources(1, &copy, stream);
			CheckResult(result);

			return new CuGraphicsMappedResource(this, stream);
		}

		public unsafe struct CuGraphicsMappedResource : IDisposable
		{
			private readonly CuGraphicsResource _resource;
			private readonly CuStream _stream;

			public CuGraphicsMappedResource(CuGraphicsResource resource, CuStream stream)
			{
				_resource = resource;
				_stream = stream;
			}

			public void Dispose()
			{
				var copy = _resource;
				GraphicsUnmapResources(1, &copy, _stream);
			}
		}

		/// <inheritdoc cref="GraphicsSubResourceGetMappedArray(out CuArray, CuGraphicsResource, int, int)"/>
		public CuArray GetMappedArray(int arrayIndex = 0, int mipLevel = 0)
		{
			var result = GraphicsSubResourceGetMappedArray(out var array, this, arrayIndex, mipLevel);
			CheckResult(result);

			return array;
		}

		/// <inheritdoc cref="GraphicsUnregisterResource(CuGraphicsResource)"/>
		public void Dispose()
		{
			var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
			if (handle == IntPtr.Zero) return;
			var obj = new CuGraphicsResource { Handle = handle };

			GraphicsUnregisterResource(obj);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuMipMappedArray
	{
		public static readonly CuMipMappedArray Empty = new CuMipMappedArray { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuModule
	{
		public static readonly CuModule Empty = new CuModule { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuLinkState
	{
		public static readonly CuLinkState Empty = new CuLinkState { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuSurfRef
	{
		public static readonly CuSurfRef Empty = new CuSurfRef { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuFunction
	{
		public static readonly CuFunction Empty = new CuFunction { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuTextRef
	{
		public static readonly CuTextRef Empty = new CuTextRef { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuIpcEventHandle
	{
		public fixed byte Handle[64];
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuIpcMemHandle
	{
		public fixed byte Handle[64];
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuTexObject
	{
		public static readonly CuTexObject Empty = new CuTexObject { Handle = 0 };
		public long Handle;
		public bool IsEmpty => Handle == 0;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuSurfObject
	{
		public static readonly CuSurfObject Empty = new CuSurfObject { Handle = 0 };
		public long Handle;
		public bool IsEmpty => Handle == 0;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuHostMemory : IDisposable
	{
		public static readonly CuHostMemory Empty = new CuHostMemory { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

		/// <inheritdoc cref="MemAllocHost(out CuHostMemory, IntPtr)"/>
		public static CuHostMemory Allocate(long bytesize)
		{
			CheckResult(MemAllocHost(out var mem, (IntPtr)bytesize));
			return mem;
		}

		// TODO: Move?
		/// <inheritdoc cref="MemAllocManaged(out CuDevicePtr, IntPtr, MemoryAttachFlags)"/>
		public static CuDevicePtr AllocateManaged(
			long bytesize, MemoryAttachFlags flags)
		{
			CheckResult(MemAllocManaged(out var mem, (IntPtr)bytesize, flags));
			return mem;
		}

		/// <inheritdoc cref="MemFreeHost(CuHostMemory)"/>
		public void Dispose()
		{
			var handle = Interlocked.Exchange(ref Handle, IntPtr.Zero);
			if (handle == IntPtr.Zero) return;
			var obj = new CuHostMemory { Handle = handle };

			MemFreeHost(obj);
		}
	}

	/// <summary>CUDA_MEMCPY2D:
	/// 2D memory copy parameters</summary>
	public struct CuMemcopy2D
    {
        /// <summary>Source X in bytes</summary>
        public IntPtr SrcXInBytes;
        /// <summary>Source Y</summary>
        public IntPtr SrcY;

        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArray SrcArray;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public IntPtr SrcPitch;

        /// <summary>Destination X in bytes</summary>
        public IntPtr DstXInBytes;
        /// <summary>Destination Y</summary>
        public IntPtr DstY;

        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArray DstArray;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public IntPtr DstPitch;

        /// <summary>Width of 2D memory copy in bytes</summary>
        public IntPtr WidthInBytes;
        /// <summary>Height of 2D memory copy</summary>
        public IntPtr Height;

        /// <inheritdoc cref="LibCuda.Memcpy2D(ref CuMemcopy2D)"/>
        public void Memcpy2D()
        {
            var result = LibCuda.Memcpy2D(ref this);
            CheckResult(result);
        }
    }

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuArray
	{
		public static readonly CuArray Empty = new CuArray { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

		public CuArray(IntPtr handle)
		{
			Handle = handle;
		}
	}

	/// <summary>CUDA_MEMCPY3D</summary>
	public struct CuMemcpy3D
    {
        /// <summary>Source X in bytes</summary>
        public uint SrcXInBytes;
        /// <summary>Source Y</summary>
        public uint SrcY;
        /// <summary>Source Z</summary>
        public uint SrcZ;
        /// <summary>Source LOD</summary>
        public uint SrcLod;
        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArray SrcArray;
        /// <summary>Must be NULL</summary>
        private IntPtr _reserved0;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public uint SrcPitch;
        /// <summary>Source height (ignored when src is array; may be 0 if Depth==1)</summary>
        public uint SrcHeight;

        /// <summary>Destination X in bytes</summary>
        public uint DstXInBytes;
        /// <summary>Destination Y</summary>
        public uint DstY;
        /// <summary>Destination Z</summary>
        public uint DstZ;
        /// <summary>Destination LOD</summary>
        public uint DstLod;
        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArray DstArray;
        /// <summary>Must be NULL</summary>
        public IntPtr Reserved1;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public uint DstPitch;
        /// <summary>Destination height (ignored when dst is array; may be 0 if Depth==1)</summary>
        public uint DstHeight;

        /// <summary>Width of 3D memory copy in bytes</summary>
        public uint WidthInBytes;
        /// <summary>Height of 3D memory copy</summary>
        public uint Height;
        /// <summary>Depth of 3D memory copy</summary>
        public uint Depth;
    }

    public struct CuMemcpy3DPeer
    {
        /// <summary>Source X in bytes</summary>
        public IntPtr SrcXInBytes;
        /// <summary>Source Y</summary>
        public IntPtr SrcY;
        /// <summary>Source Z</summary>
        public IntPtr SrcZ;
        /// <summary>Source LOD</summary>
        public IntPtr SrcLod;
        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArray SrcArray;
        /// <summary>Source context (ignored with srcMemoryType is ::CU_MEMORYTYPE_ARRAY)</summary>
        public CuContext SrcContext;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public IntPtr SrcPitch;
        /// <summary>Source height (ignored when src is array; may be 0 if Depth==1)</summary>
        public IntPtr SrcHeight;

        /// <summary>Destination X in bytes</summary>
        public IntPtr DstXInBytes;
        /// <summary>Destination Y</summary>
        public IntPtr DstY;
        /// <summary>Destination Z</summary>
        public IntPtr DstZ;
        /// <summary>Destination LOD</summary>
        public IntPtr DstLod;
        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArray DstArray;
        /// <summary>Destination context (ignored with dstMemoryType is ::CU_MEMORYTYPE_ARRAY)</summary>
        public CuContext DstContext;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public IntPtr DstPitch;
        /// <summary>Destination height (ignored when dst is array; may be 0 if Depth==1)</summary>
        public IntPtr DstHeight;

        /// <summary>Width of 3D memory copy in bytes</summary>
        public IntPtr WidthInBytes;
        /// <summary>Height of 3D memory copy</summary>
        public IntPtr Height;
        /// <summary>Depth of 3D memory copy</summary>
        public IntPtr Depth;
    }

    public struct CuArrayDescription
    {
        /// <summary>Width of array</summary>
        public int Width;
        /// <summary>Height of array</summary>
        public int Height;

        /// <summary>Array format</summary>
        public CuArrayFormat Format;
        /// <summary>Channels per array element</summary>
        public int NumChannels;
    }

    public struct CuArray3DDescription
    {
        /// <summary>Width of 3D array</summary>
        public int Width;
        /// <summary>Height of 3D array</summary>
        public int Height;
        /// <summary>Depth of 3D array</summary>
        public int Depth;

        /// <summary>Array format</summary>
        public CuArrayFormat Format;
        /// <summary>Channels per array element</summary>
        public int NumChannels;
        /// <summary>Flags</summary>
        public CuArray3DFlags Flags;
    }

    public struct CuStreamMemOpWaitValueParams
    {
        public CuStreamBatchMemOpType Operation;
        public CuDevicePtr Address;
        public long Value64;
        public int Flags;
        /// <summary>For driver internal use. Initial value is unimportant.</summary>
        public CuDevicePtr Alias;
    }

    public struct CuStreamMemOpWriteValueParams
    {
        public CuStreamBatchMemOpType Operation;
        public CuDevicePtr Address;
        public long Value64;
        public int Flags;
        /// <summary>For driver internal use. Initial value is unimportant.</summary>
        public CuDevicePtr Alias;
    }

    public struct CuStreamMemOpFlushRemoteWritesParams
    {
        public CuStreamBatchMemOpType Operation;
        public int Flags;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CuSreamBatchMemOpParams
    {
        [FieldOffset(0)]
        public CuStreamBatchMemOpType Operation;
        [FieldOffset(0)]
        public CuStreamMemOpWaitValueParams WaitValue;
        [FieldOffset(0)]
        public CuStreamMemOpWriteValueParams WriteValue;
        [FieldOffset(0)]
        public CuStreamMemOpFlushRemoteWritesParams FlushRemoteWrites;
        [FieldOffset(0)]
        private fixed long _pad[6];
    }

    public struct CuResourceDescArray
    {
        /// <summary>CUDA array</summary>
        public CuArray Array;
    }

    public struct CuResourceDescMipmap
    {
        /// <summary>CUDA mipmapped array</summary>
        public CuMipMappedArray MipmappedArray;
    }

    public struct CuResourceDescLinear
    {
        /// <summary>Device pointer</summary>
        public CuDevicePtr DevPtr;
        /// <summary>Array format</summary>
        public CuArrayFormat Format;
        /// <summary>Channels per array element</summary>
        public int NumChannels;
        /// <summary>Size in bytes</summary>
        public IntPtr SizeInBytes;
    }

    public struct CuResourceDescPitch2D
    {
        /// <summary>Device pointer</summary>
        public CuDevicePtr DevPtr;
        /// <summary>Array format</summary>
        public CuArrayFormat Format;
        /// <summary>Channels per array element</summary>
        public int NumChannels;
        /// <summary>Width of the array in elements</summary>
        public IntPtr Width;
        /// <summary>Height of the array in elements</summary>
        public IntPtr Height;
        /// <summary>Pitch between two rows in bytes</summary>
        public IntPtr PitchInBytes;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CuResourceDescData
    {
        [FieldOffset(0)]
        public CuResourceDescArray Array;

        [FieldOffset(0)]
        public CuResourceDescMipmap Mipmap;

        [FieldOffset(0)]
        public CuResourceDescLinear Linear;

        [FieldOffset(0)]
        public CuResourceDescPitch2D Pitch2D;

        [FieldOffset(0)]
        private fixed int _reserved[32];
    }

    /// <summary>CUDA_RESOURCE_DESC:
    /// CUDA Resource descriptor</summary>
    public struct CuResourceDescription
    {
        /// <summary>Resource type</summary>
        public CuResourceType ResType;
        public CuResourceDescData Data;
        /// <summary>Flags (must be zero)</summary>
        public int Flags;
    }

    /// <summary>CUDA_TEXTURE_DESC:
    /// Texture descriptor</summary>
    public unsafe struct CuTextureDescription
    {
        /// <summary>Address modes</summary>
        public AddressMode AddressMode1;
        public AddressMode AddressMode2;
        public AddressMode AddressMode3;
        /// <summary>Filter mode</summary>
        public FilterMode FilterMode;
        /// <summary>Flags</summary>
        public int Flags;
        /// <summary>Maximum anisotropy ratio</summary>
        public int MaxAnisotropy;
        /// <summary>Mipmap filter mode</summary>
        public FilterMode MipmapFilterMode;
        /// <summary>Mipmap level bias</summary>
        public float MipmapLevelBias;
        /// <summary>Mipmap minimum level clamp</summary>
        public float MinMipmapLevelClamp;
        /// <summary>Mipmap maximum level clamp</summary>
        public float MaxMipmapLevelClamp;
        /// <summary>Border Color</summary>
        public fixed float BorderColor[4];
        private fixed int _reserved[12];
    }

    /// <summary>CUDA_RESOURCE_VIEW_DESC:
    /// Resource view descriptor</summary>
    public unsafe struct CuResourceViewDescription
    {
        /// <summary>Resource view format</summary>
        public CuResourceViewFormat Format;
        /// <summary>Width of the resource view</summary>
        public IntPtr Width;
        /// <summary>Height of the resource view</summary>
        public IntPtr Height;
        /// <summary>Depth of the resource view</summary>
        public IntPtr Depth;
        /// <summary>First defined mipmap level</summary>
        public int FirstMipmapLevel;
        /// <summary>Last defined mipmap level</summary>
        public int LastMipmapLevel;
        /// <summary>First layer index</summary>
        public int FirstLayer;
        /// <summary>Last layer index</summary>
        public int LastLayer;
        private fixed int _reserved[16];
    }

    /// <summary>CUDA_LAUNCH_PARAMS:
    /// Kernel launch parameters</summary>
    public unsafe struct LaunchParameters
    {
        /// <summary>Kernel to launch</summary>
        public CuFunction Function;
        /// <summary>Width of grid in blocks</summary>
        public int GridDimX;
        /// <summary>Height of grid in blocks</summary>
        public int GridDimY;
        /// <summary>Depth of grid in blocks</summary>
        public int GridDimZ;
        /// <summary>X dimension of each thread block</summary>
        public int BlockDimX;
        /// <summary>Y dimension of each thread block</summary>
        public int BlockDimY;
        /// <summary>Z dimension of each thread block</summary>
        public int BlockDimZ;
        /// <summary>Dynamic shared-memory size per thread block in bytes</summary>
        public int SharedMemBytes;
        /// <summary>Stream identifier</summary>
        public CuStream Stream;
        /// <summary>Array of pointers to kernel parameters</summary>
        public IntPtr KernelParams;
    }
}
