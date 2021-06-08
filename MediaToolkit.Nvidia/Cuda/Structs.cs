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

        public CuDevicePtr(int handle)
        {
            Handle = (IntPtr)handle; 
        }

        /// <summary>CU_DEVICE_CPU
        /// Device that represents the CPU</summary>
        public static readonly CuDevicePtr DeviceCpu = new CuDevicePtr(-1);

        /// <summary>CU_DEVICE_INVALID
        /// Device that represents an invalid device</summary>
        public static readonly CuDevicePtr DeviceInvalid = new CuDevicePtr(-2);

        public static implicit operator ulong(CuDevicePtr d) => (ulong)d.Handle.ToInt64();
		public static implicit operator CuDevicePtr(byte* d) => new CuDevicePtr(d);
	}


    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuEventPtr
    {
        public static readonly CuEventPtr Empty = new CuEventPtr { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;
    }


    [StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuContextPtr 
	{
		public static readonly CuContextPtr Empty = new CuContextPtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuGraphicsResourcePtr
	{
		public static readonly CuGraphicsResourcePtr Empty = new CuGraphicsResourcePtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;

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
		public static CuDevicePtr AllocateManaged(long bytesize, MemoryAttachFlags flags)
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

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuStreamPtr 
	{
		public static readonly CuStreamPtr Empty = new CuStreamPtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
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
        public CuContextPtr SrcContext;
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
        public CuContextPtr DstContext;
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
        public CuStreamPtr Stream;
        /// <summary>Array of pointers to kernel parameters</summary>
        public IntPtr KernelParams;
    }
}
