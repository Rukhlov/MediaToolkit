using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public unsafe struct CuVideoDecoderPtr { public IntPtr Handle; }


    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoFramePtr { public IntPtr Handle; }


    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public unsafe struct CuVideoParserPtr { public IntPtr Handle; }


    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    public struct CuVideoContextLockPtr
    {
        public static readonly CuVideoContextLockPtr Empty = new CuVideoContextLockPtr { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;

    }

    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{" + nameof(Handle) + "}")]
    [Obsolete]
    public struct CuVideoSourcePtr
    {
        public static readonly CuVideoSourcePtr Empty = new CuVideoSourcePtr { Handle = IntPtr.Zero };
        public IntPtr Handle;
        public bool IsEmpty => Handle == IntPtr.Zero;
    }


    /// <summary>
    /// \struct CUVIDSOURCEPARAMS
    /// Describes parameters needed in cuvidCreateVideoSource API
    /// NVDECODE API is intended for HW accelerated video decoding so CUvideosource doesn't have audio demuxer for all supported
    /// containers. It's recommended to clients to use their own or third party demuxer if audio support is needed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuVideoSourceParams
	{
		/// <summary>IN: Time stamp units in Hz (0=default=10000000Hz)</summary>
		public int ClockRate;
		/// <summary>Reserved for future use - set to zero</summary>
		private fixed uint _reserved1[7];
		/// <summary>IN: User private data passed in to the data handlers</summary>
		public IntPtr UserData;
		/// <summary>IN: Called to deliver video packets</summary>
		// TODO: Fix non-delegate type.
		public NvCuVid.VideoSourceCallback VideoDataHandler;
		/// <summary>IN: Called to deliver audio packets.</summary>
		public NvCuVid.VideoSourceCallback AudioDataHandler;
		private IntPtr _reserved21;
		private IntPtr _reserved22;
		private IntPtr _reserved23;
		private IntPtr _reserved24;
		private IntPtr _reserved25;
		private IntPtr _reserved26;
		private IntPtr _reserved27;
		private IntPtr _reserved28;
	}


	/// <summary>
	/// \struct CUVIDSOURCEDATAPACKET
	/// Data Packet
	/// Used in cuvidParseVideoData API
	/// IN for cuvidParseVideoData
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuVideoSourceDataPacket
	{
		/// <summary>IN: Combination of CUVID_PKT_XXX flags</summary>
		public CuVideoPacketFlags Flags;
		/// <summary>IN: number of bytes in the payload (may be zero if EOS flag is set)</summary>
		public uint PayloadSize;
		/// <summary>IN: Pointer to packet payload data (may be NULL if EOS flag is set)</summary>
		public byte* Payload;
		/// <summary>IN: Presentation time stamp (10MHz clock), only valid if
		/// CUVID_PKT_TIMESTAMP flag is set</summary>
		public long Timestamp;
	}

	/// <summary>
	/// \struct CUVIDEOFORMAT
	/// Video format
	/// Used in cuvidGetSourceVideoFormat API
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuVideoFormat
	{
		/// <summary>OUT: Compression format</summary>
		public CuVideoCodec Codec;
		/// <summary>OUT: frame rate numerator   (0 = unspecified or variable frame rate)</summary>
		public int FrameRateNumerator;
		/// <summary>OUT: frame rate denominator (0 = unspecified or variable frame rate)</summary>
		public int FrameRateDenominator;
		/// <summary>OUT: 0=interlaced, 1=progressive</summary>
		public ByteBool ProgressiveSequence;
		/// <summary>OUT: high bit depth luma. E.g, 2 for 10-bitdepth, 4 for 12-bitdepth</summary>
		public byte BitDepthLumaMinus8;
		/// <summary>OUT: high bit depth chroma. E.g, 2 for 10-bitdepth, 4 for 12-bitdepth</summary>
		public byte BitDepthChromaMinus8;
		/// <summary>OUT: Minimum number of decode surfaces to be allocated for correct
		/// decoding. The client can send this value in ulNumDecodeSurfaces
		/// (in CUVIDDECODECREATEINFO structure).
		/// This guarantees correct functionality and optimal video memory
		/// usage but not necessarily the best performance, which depends on
		/// the design of the overall application. The optimal number of
		/// decode surfaces (in terms of performance and memory utilization)
		/// should be decided by experimentation for each application, but it
		/// cannot go below min_num_decode_surfaces.
		/// If this value is used for ulNumDecodeSurfaces then it must be
		/// returned to parser during sequence callback.</summary>
		public byte MinNumDecodeSurfaces;
		/// <summary>OUT: coded frame width in pixels</summary>
		public int CodedWidth;
		/// <summary>OUT: coded frame height in pixels</summary>
		public int CodedHeight;
		public CuRectangle DisplayArea;
		/// <summary>OUT:  Chroma format</summary>
		public CuVideoChromaFormat ChromaFormat;
		/// <summary>OUT: video bitrate (bps, 0=unknown)</summary>
		public int Bitrate;
		/// <summary>OUT: Display Aspect Ratio = x:y (4:3, 16:9, etc)</summary>
		public int DisplayAspectRatioX;
		/// <summary>OUT: Display Aspect Ratio = x:y (4:3, 16:9, etc)</summary>
		public int DisplayAspectRatioY;
		internal fixed byte BitField1[1];

		/// <summary>OUT: 0-Component, 1-PAL, 2-NTSC, 3-SECAM, 4-MAC, 5-Unspecified</summary>
		public byte VideoFormat
		{
			get { fixed (byte* ptr = &BitField1[0]) { return (byte)((*(byte*)ptr >> 0) & 3); } }
			set => BitField1[0] = (byte)((BitField1[0] & ~24) | (((value) << 0) & 24));
		}

		/// <summary>OUT: indicates the black level and luma and chroma range</summary>
		public bool VideoFullRange
		{
			get => (BitField1[0] & 4) != 0;
			set => BitField1[0] = value ? (byte)(BitField1[0] | 4) : (byte)(BitField1[0] & -5);
		}

		/// <summary>OUT: chromaticity coordinates of source primaries</summary>
		public byte ColorPrimaries;
		/// <summary>OUT: opto-electronic transfer characteristic of the source picture</summary>
		public byte TransferCharacteristics;
		/// <summary>OUT: used in deriving luma and chroma signals from RGB primaries</summary>
		public byte MatrixCoefficients;
		/// <summary>UT: Additional bytes following (CUVIDEOFORMATEX)</summary>
		public int SeqHdrDataLength;

		public CuVideoSurfaceFormat GetSurfaceFormat()
		{
			var surfFormat = CuVideoSurfaceFormat.Default;

			switch (ChromaFormat)
			{
				case CuVideoChromaFormat.YUV420:
					{// 8 bit
						surfFormat = BitDepthLumaMinus8 > 0 ? CuVideoSurfaceFormat.P016 : CuVideoSurfaceFormat.NV12;
						break;
					}

				case CuVideoChromaFormat.YUV444:
					{ //16 bit
						surfFormat = BitDepthLumaMinus8 > 0 ? CuVideoSurfaceFormat.YUV444_16Bit : CuVideoSurfaceFormat.YUV444;
						break;
					}
			}

			return surfFormat;
		}

	}


	[StructLayout(LayoutKind.Sequential)]
	public struct CuRectangle
	{
		public static CuRectangle Null { get; } = new CuRectangle(0, 0, 0, 0);

		public int Width => Right - Left;
		public int Height => Bottom - Top;

		/// <summary>left position of rect</summary>
		public int Left;
		/// <summary>top position of rect</summary>
		public int Top;
		/// <summary>right position of rect</summary>
		public int Right;
		/// <summary>bottom position of rect</summary>
		public int Bottom;

		public CuRectangle(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}
	}

	/// <summary>
	/// \struct CUAUDIOFORMAT
	/// Audio formats
	/// Used in cuvidGetSourceAudioFormat API
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CuAudioFormat
	{
		/// <summary>OUT: Compression format</summary>
		public CuAudioCodec Codec;
		/// <summary>OUT: number of audio channels</summary>
		public uint Channels;
		/// <summary>OUT: sampling frequency</summary>
		public uint Samplespersec;
		/// <summary>OUT: For uncompressed, can also be used to determine bits per sample</summary>
		public uint Bitrate;
		/// <summary> Reserved for future use</summary>
		private uint _reserved1;
		/// <summary> Reserved for future use</summary>
		private uint _reserved2;
	}


	/// <summary>
	/// \struct CUVIDEOFORMATEX
	/// Video format including raw sequence header information
	/// Used in cuvidGetSourceVideoFormat API
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuVideoFormatEx
	{
		/// <summary>OUT: CUVIDEOFORMAT structure</summary>
		public CuVideoFormat Format;
		/// <summary>OUT: Sequence header data</summary>
		public fixed byte RawSeqHdrData[1024];
	}



	/// <summary>
	/// \struct CUVIDPARSERPARAMS
	/// Used in cuvidCreateVideoParser API
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CuVideoParserParams
	{
		/// <summary>IN: cudaVideoCodec_XXX</summary>
		public CuVideoCodec CodecType;
		/// <summary>IN: Max # of decode surfaces (parser will cycle through these)</summary>
		public uint MaxNumDecodeSurfaces;
		/// <summary>IN: Timestamp units in Hz (0=default=10000000Hz)</summary>
		public uint ClockRate;
		/// <summary>IN: % Error threshold (0-100) for calling pfnDecodePicture (100=always
		/// IN: call pfnDecodePicture even if picture bitstream is fully corrupted)</summary>
		public uint ErrorThreshold;
		/// <summary>IN: Max display queue delay (improves pipelining of decode with display)
		/// 0=no delay (recommended values: 2..4)</summary>
		public uint MaxDisplayDelay;
		/// <summary>IN: Reserved for future use - set to 0</summary>
		public fixed uint Reserved1[5];
		/// <summary>IN: User data for callbacks</summary>
		public IntPtr UserData;
		/// <summary>IN: Called before decoding frames and/or whenever there is a fmt change</summary>
		public NvCuVid.VideoSequenceCallback SequenceCallback;
		/// <summary>IN: Called when a picture is ready to be decoded (decode order)</summary>
		public NvCuVid.VideoDecodeCallback DecodePicture;
		/// <summary>IN: Called whenever a picture is ready to be displayed (display order)</summary>
		public NvCuVid.VideoDisplayCallback DisplayPicture;
		private IntPtr _reserved21;
		private IntPtr _reserved22;
		private IntPtr _reserved23;
		private IntPtr _reserved24;
		private IntPtr _reserved25;
		private IntPtr _reserved26;
		private IntPtr _reserved27;
		/// <summary>IN: [Optional] sequence header data from system layer </summary>
		public CuVideoFormatEx* ExtVideoInfo;
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

		public override string ToString()
		{
			return (Value != 0).ToString();
		}
	}

}
