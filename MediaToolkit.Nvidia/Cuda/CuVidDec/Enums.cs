using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
	/// <summary>
	/// \enum cudaVideoCodec
	/// Video codec enums
	/// These enums are used in CUVIDDECODECREATEINFO and CUVIDDECODECAPS structures
	/// </summary>
	public enum CuVideoCodec
	{
		MPEG1 = 0,
		MPEG2,
		MPEG4,
		VC1,
		H264,
		JPEG,
		H264_SVC,
		H264_MVC,
		HEVC,
		VP8,
		VP9,
		NumCodecs,
		/// <summary>Uncompressed: Y,U,V (4:2:0)</summary>
		YUV420 = (('I' << 24) | ('Y' << 16) | ('U' << 8) | ('V')),
		/// <summary>Uncompressed: Y,V,U (4:2:0)</summary>
		YV12 = (('Y' << 24) | ('V' << 16) | ('1' << 8) | ('2')),
		/// <summary>Uncompressed: Y,UV  (4:2:0)</summary>
		NV12 = (('N' << 24) | ('V' << 16) | ('1' << 8) | ('2')),
		/// <summary>Uncompressed: YUYV/YUY2 (4:2:2)</summary>
		YUYV = (('Y' << 24) | ('U' << 16) | ('Y' << 8) | ('V')),
		/// <summary>Uncompressed: UYVY (4:2:2)</summary>
		UYVY = (('U' << 24) | ('Y' << 16) | ('V' << 8) | ('Y'))
	}



	/// <summary>
	/// \enum cudaVideoSurfaceFormat
	/// Video surface format enums used for output format of decoded output
	/// These enums are used in CUVIDDECODECREATEINFO structure
	///</summary>
	public enum CuVideoSurfaceFormat
	{
		/// <summary>NV12</summary>
		Default = 0,
		/// <summary>Semi-Planar YUV [Y plane followed by interleaved UV plane]</summary>
		NV12 = 0,
		/// <summary>16 bit Semi-Planar YUV [Y plane followed by interleaved UV plane].
		/// Can be used for 10 bit(6LSB bits 0), 12 bit (4LSB bits 0)</summary>
		P016 = 1,
		/// <summary>Planar YUV [Y plane followed by U and V planes]</summary>
		YUV444 = 2,
		/// <summary>16 bit Planar YUV [Y plane followed by U and V planes].
		/// Can be used for 10 bit(6LSB bits 0), 12 bit (4LSB bits 0)</summary>
		YUV444_16Bit = 3,
	}



	/// <summary>
	/// \enum cudaVideoDeinterlaceMode
	/// Deinterlacing mode enums
	/// These enums are used in CUVIDDECODECREATEINFO structure
	/// Use CuVideoDeinterlaceMode_Weave for progressive content and for content that doesn't need deinterlacing
	/// CuVideoDeinterlaceMode_Adaptive needs more video memory than other DImodes
	///</summary>
	public enum CuVideoDeinterlaceMode
	{
		/// <summary>Weave both fields (no deinterlacing)</summary>
		Weave = 0,
		/// <summary>Drop one field</summary>
		Bob,
		/// <summary>Adaptive deinterlacing</summary>
		Adaptive
	}

	/// <summary>
	/// \enum cudaVideoChromaFormat
	/// Chroma format enums
	/// These enums are used in CUVIDDECODECREATEINFO and CUVIDDECODECAPS structures
	/// </summary>
	public enum CuVideoChromaFormat
	{
		/// <summary>MonoChrome</summary>
		Monochrome = 0,
		/// <summary>YUV 4:2:0</summary>
		YUV420,
		/// <summary>YUV 4:2:2</summary>
		YUV422,
		/// <summary>YUV 4:4:4</summary>
		YUV444
	}


	/// <summary>
	/// \enum CuVideoCreateFlags
	/// Decoder flag enums to select preferred decode path
	/// CuVideoCreate_Default and CuVideoCreate_PreferCUVID are most optimized, use these whenever possible
	///</summary>
	[Flags]
	public enum CuVideoCreateFlags
	{
		/// <summary>Default operation mode: use dedicated video engines</summary>
		Default = 0x00,
		/// <summary>Use Cu-based decoder (requires valid vidLock object for multi-threading)</summary>
		PreferCUDA = 0x01,
		/// <summary>Go through DXVA internally if possible (requires D3D9 interop)</summary>
		PreferDXVA = 0x02,
		/// <summary>Use dedicated video engines directly</summary>
		PreferCUVID = 0x04
	}



	/// <summary>
	/// \enum cuvidDecodeStatus
	/// Decode status enums
	/// These enums are used in CUVIDGETDECODESTATUS structure
	///</summary>
	public enum CuVideoDecodeStatus
	{
		/// <summary>Decode status is not valid</summary>
		Invalid = 0,
		/// <summary>Decode is in progress</summary>
		InProgress = 1,
		/// <summary>Decode is completed without any errors</summary>
		Success = 2,
		// 3 to 7 enums are reserved for future use
		/// <summary>Decode is completed with an error (error is not concealed)</summary>
		Error = 8,
		/// <summary>Decode is completed with an error and error is concealed</summary>
		ErrorConcealed = 9
	}
}
