using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
	public unsafe partial class NvCuVid
	{

		/// <summary>
		/// \fn CUresult CuAPI cuvidCtxLockCreate(CUvideoctxlock *pLock, CUcontext ctx)
		/// This API is used to create CtxLock object
		///
		/// Context-locking: to facilitate multi-threaded implementations, the following 4 functions
		/// provide a simple mutex-style host synchronization. If a non-NULL context is specified
		/// in CUVIDDECODECREATEINFO, the codec library will acquire the mutex associated with the given
		/// context before making any Cu calls.
		/// A multi-threaded application could create a lock associated with a context handle so that
		/// multiple threads can safely share the same Cu context:
		/// - use cuCtxPopCurrent immediately after context creation in order to create a 'floating' context
		/// that can be passed to cuvidCtxLockCreate.
		/// - When using a floating context, all Cu calls should only be made within a cuvidCtxLock/cuvidCtxUnlock section.
		///
		/// NOTE: This is a safer alternative to cuCtxPushCurrent and cuCtxPopCurrent, and is not related to video
		/// decoder in any way (implemented as a critical section associated with cuCtx{Push|Pop}Current calls).
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidCtxLockCreate")]
		public static extern CuResult CtxLockCreate(out CuVideoContextLock pLock, CuContext ctx);

		/// <summary>
		/// \fn CUresult CuAPI cuvidCtxLockDestroy(CUvideoctxlock lck)
		/// This API is used to free CtxLock object
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidCtxLockDestroy")]
		public static extern CuResult CtxLockDestroy(CuVideoContextLock lck);

		/// <summary>
		/// \fn CUresult CuAPI cuvidCtxLock(CUvideoctxlock lck, unsigned int reserved_flags)
		/// This API is used to acquire ctxlock
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidCtxLock")]
		public static extern CuResult CtxLock(CuVideoContextLock lck, uint reservedFlags);

		/// <summary>
		/// \fn CUresult CuAPI cuvidCtxUnlock(CUvideoctxlock lck, unsigned int reserved_flags)
		/// This API is used to release ctxlock
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidCtxUnlock")]
		public static extern CuResult CtxUnlock(CuVideoContextLock lck, uint reservedFlags);

		/// <summary>
		/// \fn CUresult CuAPI cuvidGetDecoderCaps(CUVIDDECODECAPS *pdc)
		/// Queries decode capabilities of NVDEC-HW based on CodecType, ChromaFormat and BitDepthMinus8 parameters.
		/// 1. Application fills IN parameters CodecType, ChromaFormat and BitDepthMinus8 of CUVIDDECODECAPS structure
		/// 2. On calling cuvidGetDecoderCaps, driver fills OUT parameters if the IN parameters are supported
		/// If IN parameters passed to the driver are not supported by NVDEC-HW, then all OUT params are set to 0.
		/// E.g. on Geforce GTX 960:
		/// App fills - eCodecType = CuVideoCodec_H264; eChromaFormat = CuVideoChromaFormat_420; nBitDepthMinus8 = 0;
		/// Given IN parameters are supported, hence driver fills: bIsSupported = 1; nMinWidth = 48; nMinHeight = 16;
		/// nMaxWidth = 4096; nMaxHeight = 4096; nMaxMBCount = 65536;
		/// CodedWidth*CodedHeight/256 must be less than or equal to nMaxMBCount
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidGetDecoderCaps")]
		public static extern CuResult GetDecoderCaps(ref CuVideoDecodeCaps pdc);

		/// <summary>
		/// \fn CUresult CuAPI cuvidCreateDecoder(CUvideodecoder *phDecoder, CUVIDDECODECREATEINFO *pdci)
		/// Create the decoder object based on pdci. A handle to the created decoder is returned
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidCreateDecoder")]
		public static extern CuResult CreateDecoder(out CuVideoDecoder decoder, ref CuVideoDecodeCreateInfo pdci);

		/// <summary>
		/// \fn CUresult CuAPI cuvidDestroyDecoder(CUvideodecoder hDecoder)
		/// Destroy the decoder object
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidDestroyDecoder")]
		public static extern CuResult DestroyDecoder(CuVideoDecoder decoder);

		/// <summary>
		/// \fn CUresult CuAPI cuvidDecodePicture(CUvideodecoder hDecoder, CUVIDPicParams *pPicParams)
		/// Decode a single picture (field or frame)
		/// Kicks off HW decoding
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidDecodePicture")]
		public static extern CuResult DecodePicture(CuVideoDecoder decoder, ref CuVideoPicParams picParams);

		/// <summary>
		/// \fn CUresult CuAPI cuvidGetDecodeStatus(CUvideodecoder hDecoder, int nPicIdx);
		/// Get the decode status for frame corresponding to nPicIdx
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidGetDecodeStatus")]
		public static extern CuResult GetDecodeStatus(CuVideoDecoder decoder, int nPicIdx, out CuVideoDecodeStatus decodeStatus);

		/// <summary>
		/// \fn CUresult CuAPI cuvidReconfigureDecoder(CUvideodecoder hDecoder, CUVIDRECONFIGUREDECODERINFO *pDecReconfigParams)
		/// Used to reuse single decoder for multiple clips. Currently supports resolution change, resize params, display area
		/// params, target area params change for same codec. Must be called during CUVIDPARSERPARAMS::pfnSequenceCallback
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidReconfigureDecoder")]
		public static extern CuResult ReconfigureDecoder(CuVideoDecoder decoder, ref CuVideoReconfigureDecoderInfo decReconfigParams);

		/// <summary>
		/// \fn CUresult CuAPI cuvidMapVideoFrame(CUvideodecoder hDecoder, int nPicIdx, unsigned int *pDevPtr,
		/// uint *pPitch, CUVIDPROCPARAMS *pVPP);
		/// Post-process and map video frame corresponding to nPicIdx for use in Cu. Returns Cu device pointer and associated
		/// pitch of the video frame
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidMapVideoFrame")]
		public static extern CuResult MapVideoFrame(CuVideoDecoder decoder, int picIdx,
			out CuDevicePtr devPtr, out int pitch, ref CuVideoProcParams vpp);

		/// <summary>
		/// \fn CUresult CuAPI cuvidUnmapVideoFrame(CUvideodecoder hDecoder, unsigned int DevPtr)
		/// Unmap a previously mapped video frame
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidUnmapVideoFrame")]
		public static extern CuResult UnmapVideoFrame(CuVideoDecoder decoder, CuDevicePtr devPtr);

		/// <summary>
		/// \fn CUresult CuAPI cuvidMapVideoFrame64(CUvideodecoder hDecoder, int nPicIdx, unsigned long long *pDevPtr,
		/// uint * pPitch, CUVIDPROCPARAMS *pVPP);
		/// Post-process and map video frame corresponding to nPicIdx for use in Cu. Returns Cu device pointer and associated
		/// pitch of the video frame
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidMapVideoFrame64")]
		public static extern CuResult MapVideoFrame64(CuVideoDecoder decoder, int nPicIdx,
			out CuDevicePtr devPtr, out int pitch, ref CuVideoProcParams vpp);

		/// <summary>
		/// \fn CUresult CuAPI cuvidUnmapVideoFrame64(CUvideodecoder hDecoder, unsigned long long DevPtr);
		/// Unmap a previously mapped video frame
		/// </summary>
		[DllImport(nvCuVidPath, EntryPoint = "cuvidUnmapVideoFrame64")]
		public static extern CuResult UnmapVideoFrame64(CuVideoDecoder decoder, CuDevicePtr devPtr);
	}
}