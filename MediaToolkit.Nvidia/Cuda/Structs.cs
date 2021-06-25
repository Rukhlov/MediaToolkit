using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

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
	public struct CuHostMemoryPtr 
	{
		public static readonly CuHostMemoryPtr Empty = new CuHostMemoryPtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuStreamPtr 
	{
		public static readonly CuStreamPtr Empty = new CuStreamPtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuDeviceMemoryPtr // !?!?
    {
        public static readonly CuDeviceMemoryPtr Empty = new CuDeviceMemoryPtr { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;
    }


    /// <summary>CUDA_MEMCPY2D:
    /// 2D memory copy parameters</summary>
    public struct CuMemcopy2D
    {
        /// <summary>Source X in bytes</summary>
        public SizeT SrcXInBytes;
        /// <summary>Source Y</summary>
        public SizeT SrcY;

        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArrayPtr SrcArray;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public SizeT SrcPitch;

        /// <summary>Destination X in bytes</summary>
        public SizeT DstXInBytes;
        /// <summary>Destination Y</summary>
        public SizeT DstY;

        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArrayPtr DstArray;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public SizeT DstPitch;

        /// <summary>Width of 2D memory copy in bytes</summary>
        public SizeT WidthInBytes;
        /// <summary>Height of 2D memory copy</summary>
        public SizeT Height;

    }

	[StructLayout(LayoutKind.Sequential)]
	[DebuggerDisplay("{" + nameof(Handle) + "}")]
	public struct CuArrayPtr
	{
		public static readonly CuArrayPtr Empty = new CuArrayPtr { Handle = IntPtr.Zero };
		public IntPtr Handle;
		public bool IsEmpty => Handle == IntPtr.Zero;
	}

	/// <summary>CUDA_MEMCPY3D</summary>
	public struct CuMemcpy3D
    {
        /// <summary>Source X in bytes</summary>
        public SizeT SrcXInBytes;
        /// <summary>Source Y</summary>
        public SizeT SrcY;
        /// <summary>Source Z</summary>
        public SizeT SrcZ;
        /// <summary>Source LOD</summary>
        public SizeT SrcLod;
        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArrayPtr SrcArray;
        /// <summary>Must be NULL</summary>
        private IntPtr _reserved0;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public SizeT SrcPitch;
        /// <summary>Source height (ignored when src is array; may be 0 if Depth==1)</summary>
        public SizeT SrcHeight;

        /// <summary>Destination X in bytes</summary>
        public SizeT DstXInBytes;
        /// <summary>Destination Y</summary>
        public SizeT DstY;
        /// <summary>Destination Z</summary>
        public SizeT DstZ;
        /// <summary>Destination LOD</summary>
        public SizeT DstLod;
        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArrayPtr DstArray;
        /// <summary>Must be NULL</summary>
        public IntPtr Reserved1;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public SizeT DstPitch;
        /// <summary>Destination height (ignored when dst is array; may be 0 if Depth==1)</summary>
        public SizeT DstHeight;

        /// <summary>Width of 3D memory copy in bytes</summary>
        public SizeT WidthInBytes;
        /// <summary>Height of 3D memory copy</summary>
        public SizeT Height;
        /// <summary>Depth of 3D memory copy</summary>
        public SizeT Depth;
    }

    public struct CuMemcpy3DPeer
    {
        /// <summary>Source X in bytes</summary>
        public SizeT SrcXInBytes;
        /// <summary>Source Y</summary>
        public SizeT SrcY;
        /// <summary>Source Z</summary>
        public SizeT SrcZ;
        /// <summary>Source LOD</summary>
        public SizeT SrcLod;
        /// <summary>Source memory type (host, device, array)</summary>
        public CuMemoryType SrcMemoryType;
        /// <summary>Source host pointer</summary>
        public IntPtr SrcHost;
        /// <summary>Source device pointer</summary>
        public CuDevicePtr SrcDevice;
        /// <summary>Source array reference</summary>
        public CuArrayPtr SrcArray;
        /// <summary>Source context (ignored with srcMemoryType is ::CU_MEMORYTYPE_ARRAY)</summary>
        public CuContextPtr SrcContext;
        /// <summary>Source pitch (ignored when src is array)</summary>
        public SizeT SrcPitch;
        /// <summary>Source height (ignored when src is array; may be 0 if Depth==1)</summary>
        public SizeT SrcHeight;

        /// <summary>Destination X in bytes</summary>
        public SizeT DstXInBytes;
        /// <summary>Destination Y</summary>
        public SizeT DstY;
        /// <summary>Destination Z</summary>
        public SizeT DstZ;
        /// <summary>Destination LOD</summary>
        public IntPtr DstLod;
        /// <summary>Destination memory type (host, device, array)</summary>
        public CuMemoryType DstMemoryType;
        /// <summary>Destination host pointer</summary>
        public IntPtr DstHost;
        /// <summary>Destination device pointer</summary>
        public CuDevicePtr DstDevice;
        /// <summary>Destination array reference</summary>
        public CuArrayPtr DstArray;
        /// <summary>Destination context (ignored with dstMemoryType is ::CU_MEMORYTYPE_ARRAY)</summary>
        public CuContextPtr DstContext;
        /// <summary>Destination pitch (ignored when dst is array)</summary>
        public SizeT DstPitch;
        /// <summary>Destination height (ignored when dst is array; may be 0 if Depth==1)</summary>
        public SizeT DstHeight;

        /// <summary>Width of 3D memory copy in bytes</summary>
        public SizeT WidthInBytes;
        /// <summary>Height of 3D memory copy</summary>
        public SizeT Height;
        /// <summary>Depth of 3D memory copy</summary>
        public SizeT Depth;
    }

    public struct CuArrayDescription
    {
        /// <summary>Width of array</summary>
        public SizeT Width;
        /// <summary>Height of array</summary>
        public SizeT Height;

        /// <summary>Array format</summary>
        public CuArrayFormat Format;
        /// <summary>Channels per array element</summary>
        public int NumChannels;
    }

    public struct CuArray3DDescription
    {
        /// <summary>Width of 3D array</summary>
        public SizeT Width;
        /// <summary>Height of 3D array</summary>
        public SizeT Height;
        /// <summary>Depth of 3D array</summary>
        public SizeT Depth;

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
        public CuArrayPtr Array;
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
        public SizeT SizeInBytes;
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
        public SizeT Width;
        /// <summary>Height of the array in elements</summary>
        public SizeT Height;
        /// <summary>Pitch between two rows in bytes</summary>
        public SizeT PitchInBytes;
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
        public SizeT Width;
        /// <summary>Height of the resource view</summary>
        public SizeT Height;
        /// <summary>Depth of the resource view</summary>
        public SizeT Depth;
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

	[StructLayout(LayoutKind.Sequential)]
	public struct CUuuid
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.I1)]
		public byte[] bytes;

		public Guid ToGuid()
		{
			//TODO: uuid -> guid
			return new Guid(bytes);
		}
	}

	/// <summary>
	///  ->> https://github.com/surban/managedCuda
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct SizeT
	{
		private UIntPtr value;

		public SizeT(int value)
		{
			this.value = new UIntPtr((uint)value);
		}

		public SizeT(uint value)
		{
			this.value = new UIntPtr(value);
		}

		public SizeT(long value)
		{
			this.value = new UIntPtr((ulong)value);
		}

		public SizeT(ulong value)
		{
			this.value = new UIntPtr(value);
		}

		public SizeT(UIntPtr value)
		{
			this.value = value;
		}

		public SizeT(IntPtr value)
		{
			this.value = new UIntPtr((ulong)value.ToInt64());
		}

		public static implicit operator int(SizeT t)
		{
			return (int)t.value.ToUInt32();
		}

		public static implicit operator uint(SizeT t)
		{
			return (t.value.ToUInt32());
		}

		public static implicit operator long(SizeT t)
		{
			return (long)t.value.ToUInt64();
		}

		public static implicit operator ulong(SizeT t)
		{
			return (t.value.ToUInt64());
		}

		public static implicit operator UIntPtr(SizeT t)
		{
			return t.value;
		}

		public static implicit operator IntPtr(SizeT t)
		{
			return new IntPtr((long)t.value.ToUInt64());
		}

		public static implicit operator SizeT(int value)
		{
			return new SizeT(value);
		}

		public static implicit operator SizeT(uint value)
		{
			return new SizeT(value);
		}

		public static implicit operator SizeT(long value)
		{
			return new SizeT(value);
		}

		public static implicit operator SizeT(ulong value)
		{
			return new SizeT(value);
		}

		public static implicit operator SizeT(IntPtr value)
		{
			return new SizeT(value);
		}

		public static implicit operator SizeT(UIntPtr value)
		{
			return new SizeT(value);
		}

		public static bool operator !=(SizeT val1, SizeT val2)
		{
			return (val1.value != val2.value);
		}

		public static bool operator ==(SizeT val1, SizeT val2)
		{
			return (val1.value == val2.value);
		}

		#region +
		/// <summary>
		/// Define operator + on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator +(SizeT val1, SizeT val2)
		{
			return new SizeT(val1.value.ToUInt64() + val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator + on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator +(SizeT val1, int val2)
		{
			return new SizeT(val1.value.ToUInt64() + (ulong)val2);
		}
		/// <summary>
		/// Define operator + on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator +(int val1, SizeT val2)
		{
			return new SizeT((ulong)val1 + val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator + on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator +(uint val1, SizeT val2)
		{
			return new SizeT((ulong)val1 + val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator + on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator +(SizeT val1, uint val2)
		{
			return new SizeT(val1.value.ToUInt64() + (ulong)val2);
		}
		#endregion

		#region -
		/// <summary>
		/// Define operator - on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator -(SizeT val1, SizeT val2)
		{
			return new SizeT(val1.value.ToUInt64() - val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator - on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator -(SizeT val1, int val2)
		{
			return new SizeT(val1.value.ToUInt64() - (ulong)val2);
		}
		/// <summary>
		/// Define operator - on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator -(int val1, SizeT val2)
		{
			return new SizeT((ulong)val1 - val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator - on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator -(SizeT val1, uint val2)
		{
			return new SizeT(val1.value.ToUInt64() - (ulong)val2);
		}
		/// <summary>
		/// Define operator - on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator -(uint val1, SizeT val2)
		{
			return new SizeT((ulong)val1 - val2.value.ToUInt64());
		}
		#endregion

		#region *
		/// <summary>
		/// Define operator * on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator *(SizeT val1, SizeT val2)
		{
			return new SizeT(val1.value.ToUInt64() * val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator * on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator *(SizeT val1, int val2)
		{
			return new SizeT(val1.value.ToUInt64() * (ulong)val2);
		}
		/// <summary>
		/// Define operator * on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator *(int val1, SizeT val2)
		{
			return new SizeT((ulong)val1 * val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator * on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator *(SizeT val1, uint val2)
		{
			return new SizeT(val1.value.ToUInt64() * (ulong)val2);
		}
		/// <summary>
		/// Define operator * on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator *(uint val1, SizeT val2)
		{
			return new SizeT((ulong)val1 * val2.value.ToUInt64());
		}
		#endregion

		#region /
		/// <summary>
		/// Define operator / on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator /(SizeT val1, SizeT val2)
		{
			return new SizeT(val1.value.ToUInt64() / val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator / on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator /(SizeT val1, int val2)
		{
			return new SizeT(val1.value.ToUInt64() / (ulong)val2);
		}
		/// <summary>
		/// Define operator / on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator /(int val1, SizeT val2)
		{
			return new SizeT((ulong)val1 / val2.value.ToUInt64());
		}
		/// <summary>
		/// Define operator / on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator /(SizeT val1, uint val2)
		{
			return new SizeT(val1.value.ToUInt64() / (ulong)val2);
		}
		/// <summary>
		/// Define operator / on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static SizeT operator /(uint val1, SizeT val2)
		{
			return new SizeT((ulong)val1 / val2.value.ToUInt64());
		}
		#endregion

		#region >
		/// <summary>
		/// Define operator &gt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator >(SizeT val1, SizeT val2)
		{
			return val1.value.ToUInt64() > val2.value.ToUInt64();
		}
		/// <summary>
		/// Define operator &gt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator >(SizeT val1, int val2)
		{
			return val1.value.ToUInt64() > (ulong)val2;
		}
		/// <summary>
		/// Define operator &gt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator >(int val1, SizeT val2)
		{
			return (ulong)val1 > val2.value.ToUInt64();
		}
		/// <summary>
		/// Define operator &gt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator >(SizeT val1, uint val2)
		{
			return val1.value.ToUInt64() > (ulong)val2;
		}
		/// <summary>
		/// Define operator &gt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator >(uint val1, SizeT val2)
		{
			return (ulong)val1 > val2.value.ToUInt64();
		}
		#endregion

		#region <
		/// <summary>
		/// Define operator &lt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator <(SizeT val1, SizeT val2)
		{
			return val1.value.ToUInt64() < val2.value.ToUInt64();
		}
		/// <summary>
		/// Define operator &lt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator <(SizeT val1, int val2)
		{
			return val1.value.ToUInt64() < (ulong)val2;
		}
		/// <summary>
		/// Define operator &lt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator <(int val1, SizeT val2)
		{
			return (ulong)val1 < val2.value.ToUInt64();
		}
		/// <summary>
		/// Define operator &lt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator <(SizeT val1, uint val2)
		{
			return val1.value.ToUInt64() < (ulong)val2;
		}
		/// <summary>
		/// Define operator &lt; on converted to ulong values to avoid fall back to int
		/// </summary>
		/// <param name="val1"></param>
		/// <param name="val2"></param>
		/// <returns></returns>
		public static bool operator <(uint val1, SizeT val2)
		{
			return (ulong)val1 < val2.value.ToUInt64();
		}
		#endregion

		public override bool Equals(object obj)
		{
			if (!(obj is SizeT)) return false;
			SizeT o = (SizeT)obj;
			return this.value.Equals(o.value);
		}

		public override string ToString()
		{
			if (IntPtr.Size == 4)
				return ((uint)this.value.ToUInt32()).ToString();
			else
				return ((ulong)this.value.ToUInt64()).ToString();
		}

		public override int GetHashCode()
		{
			return this.value.GetHashCode();
		}
	}
}
