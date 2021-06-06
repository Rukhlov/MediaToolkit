using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{

	/// <summary>
	/// \enum cudaVideoState
	/// Video source state enums
	/// Used in cuvidSetVideoSourceState and cuvidGetVideoSourceState APIs
	/// </summary>
	public enum CuVideoState
	{
		/// <summary>Error state (invalid source)</summary>
		Error = -1,
		/// <summary>Source is stopped (or reached end-of-stream)</summary>
		Stopped = 0,
		/// <summary>Source is running and delivering data</summary>
		Started = 1
	}


	/// <summary>
	/// \enum CUvideopacketflags
	/// Data packet flags
	/// Used in CUVIDSOURCEDATAPACKET structure
	/// </summary>
	[Flags]
	public enum CuVideoPacketFlags
	{
		None = 0,
		/// <summary>CUVID_PKT_ENDOFSTREAM: Set when this is the last packet for this stream</summary>
		EndOfStream = 0x01,
		/// <summary>CUVID_PKT_TIMESTAMP: Timestamp is valid</summary>
		Timestamp = 0x02,
		/// <summary>CUVID_PKT_DISCONTINUITY: Set when a discontinuity has to be signalled </summary>
		Discontinuity = 0x04,
		/// <summary>CUVID_PKT_ENDOFPICTURE: Set when the packet contains exactly one frame or one field</summary>
		EndOfPicture = 0x08,
		/// <summary>CUVID_PKT_NOTIFY_EOS: If this flag is set along with CUVID_PKT_ENDOFSTREAM, an additional (dummy)
		/// display callback will be invoked with null value of CUVIDPARSERDISPINFO which
		/// should be interpreted as end of the stream.</summary>
		NotifyEos = 0x10,
	}

	/// <summary>
	/// \struct CUAUDIOFORMAT
	/// Audio formats
	/// Used in cuvidGetSourceAudioFormat API
	/// </summary>
	public enum CuAudioCodec
	{
		/// <summary>MPEG-1 Audio</summary>
		MPEG1 = 0,
		/// <summary>MPEG-2 Audio</summary>
		MPEG2,
		/// <summary>MPEG-1 Layer III Audio</summary>
		MP3,
		/// <summary>Dolby Digital (AC3) Audio</summary>
		AC3,
		/// <summary>PCM Audio</summary>
		LPCM,
		/// <summary>AAC Audio</summary>
		AAC,
	}


	/// <summary>
	/// \enum cudaVideosourceformat_flags
	/// CUvideosourceformat_flags
	/// Used in cuvidGetSourceVideoFormat API
	/// </summary>
	public enum CuVideoSourceFormat
	{
		Default = 0,
		/// <summary>Return extended format structure (CUVIDEOFORMATEX)</summary>
		ExtFormatInfo = 0x100
	}

}
