using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace MediaToolkit.Nvidia
{
    public unsafe partial class NvCuVid
    {
        private const string nvCuVidPath = "nvcuvid.dll";

        /// <summary>
        /// typedef int (CUDAAPI *PFNVIDSOURCECALLBACK)(void *, CUVIDSOURCEDATAPACKET *);
        /// Callback for packet delivery
        /// </summary>
        public delegate int VideoSourceCallback(IntPtr data, ref CuVideoSourceDataPacket packet);

        // typedef int (CUDAAPI *PFNVIDSEQUENCECALLBACK)(void *, CUVIDEOFORMAT *);
        public delegate int VideoSequenceCallback(IntPtr userData, ref CuVideoFormat pvidfmt);

        // typedef int (CUDAAPI* PFNVIDDECODECALLBACK) (void*, CUVIDPICPARAMS*);
        public delegate int VideoDecodeCallback(IntPtr userData, ref CuVideoPicParams param);

        // typedef int (CUDAAPI* PFNVIDDISPLAYCALLBACK) (void*, CUVIDPARSERDISPINFO*);
        /// <summary>
        /// The callback when a decoded frame is ready for display.
        /// </summary>
        /// <param name="userData">The pointer passed to `CuVideoParserParams.UserData`</param>
        /// <param name="infoPtr">A pointer to a `CuVideoParseDisplayInfo` object or `IntPtr.Zero` if
        /// the end of stream has been reached. Use `CuVideoParseDisplayInfo.IsFinalFrame` to get the
        /// actual structure.</param>
        ///
        /// <seealso cref="CuVideoParseDisplayInfo.IsFinalFrame(IntPtr, out CuVideoParseDisplayInfo)" />
        public delegate int VideoDisplayCallback(IntPtr userData, IntPtr infoPtr);

        #region Obsolete
        /// <summary>
        /// \fn CUresult CUDAAPI cuvidCreateVideoSource(CUvideosource *pObj, const char *pszFileName, CUVIDSOURCEPARAMS *pParams)
        /// Create CUvideosource object. CUvideosource spawns demultiplexer thread that provides two callbacks:
        /// pfnVideoDataHandler() and pfnAudioDataHandler()
        /// NVDECODE API is intended for HW accelerated video decoding so CUvideosource doesn't have audio demuxer for all supported
        /// containers. It's recommended to clients to use their own or third party demuxer if audio support is needed.
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidCreateVideoSource", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern CuResult CreateVideoSource(CuVideoSourcePtr* pObj, string pszFileName, ref CuVideoSourceParams pParams);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidCreateVideoSourceW(CUvideosource *pObj, const wchar_t *pwszFileName, CUVIDSOURCEPARAMS *pParams)
        /// Create video source
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidCreateVideoSourceW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern CuResult CreateVideoSourceW(CuVideoSourcePtr* pObj, string pszFileName, ref CuVideoSourceParams pParams);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidDestroyVideoSource(CUvideosource obj)
        /// Destroy video source
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidDestroyVideoSource", SetLastError = true)]
        public static extern CuResult DestroyVideoSource(CuVideoSourcePtr obj);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidSetVideoSourceState(CUvideosource obj, cudaVideoState state)
        /// Set video source state to:
        /// cudaVideoState_Started - to signal the source to run and deliver data
        /// cudaVideoState_Stopped - to stop the source from delivering the data
        /// cudaVideoState_Error   - invalid source
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidSetVideoSourceState", SetLastError = true)]
        public static extern CuResult SetVideoSourceState(CuVideoSourcePtr obj, CuVideoState state);

        /// <summary>
        /// \fn cudaVideoState CUDAAPI cuvidGetVideoSourceState(CUvideosource obj)
        /// Get video source state
        /// </summary>
        /// <returns>
        /// cudaVideoState_Started - if Source is running and delivering data
        /// cudaVideoState_Stopped - if Source is stopped or reached end-of-stream
        /// cudaVideoState_Error   - if Source is in error state
        /// </returns>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidGetVideoSourceState", SetLastError = true)]
        public static extern CuVideoState GetVideoSourceState(CuVideoSourcePtr obj);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidGetSourceVideoFormat(CUvideosource obj, CUVIDEOFORMAT *pvidfmt, unsigned int flags)
        /// Gets video source format in pvidfmt, flags is set to combination of CUvideosourceformat_flags as per requirement
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidGetSourceVideoFormat", SetLastError = true)]
        public static extern CuResult GetSourceVideoFormat(CuVideoSourcePtr obj, ref CuVideoFormat pvidfmt, uint flags);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidGetSourceAudioFormat(CUvideosource obj, CUAUDIOFORMAT *paudfmt, unsigned int flags)
        /// Get audio source format
        /// NVDECODE API is intended for HW accelerated video decoding so CUvideosource doesn't have audio demuxer for all supported
        /// containers. It's recommended to clients to use their own or third party demuxer if audio support is needed.
        /// </summary>
        [Obsolete]
        [DllImport(nvCuVidPath, EntryPoint = "cuvidGetSourceAudioFormat", SetLastError = true)]
        public static extern CuResult GetSourceAudioFormat(CuVideoSourcePtr obj, ref CuAudioFormat paudfmt, uint flags);
        #endregion

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidCreateVideoParser(CUvideoparser *pObj, CUVIDPARSERPARAMS *pParams)
        /// Create video parser object and initialize
        /// </summary>
        [DllImport(nvCuVidPath, EntryPoint = "cuvidCreateVideoParser", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern CuResult CreateVideoParser(out CuVideoParserPtr parser, ref CuVideoParserParams @params);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidParseVideoData(CUvideoparser obj, CUVIDSOURCEDATAPACKET *pPacket)
        /// Parse the video data from source data packet in pPacket
        /// Extracts parameter sets like SPS, PPS, bitstream etc. from pPacket and
        /// calls back pfnDecodePicture with CUVIDPICPARAMS data for kicking of HW decoding
        /// calls back pfnSequenceCallback with CUVIDEOFORMAT data for initial sequence header or when
        /// the decoder encounters a video format change
        /// calls back pfnDisplayPicture with CUVIDPARSERDISPINFO data to display a video frame
        /// </summary>
        /// CUresult CUDAAPI cuvidParseVideoData(CUvideoparser obj, CUVIDSOURCEDATAPACKET *pPacket);
        [DllImport(nvCuVidPath, EntryPoint = "cuvidParseVideoData")]
        public static extern CuResult ParseVideoData(CuVideoParserPtr obj, ref CuVideoSourceDataPacket packet);

        /// <summary>
        /// \fn CUresult CUDAAPI cuvidDestroyVideoParser(CUvideoparser obj)
        /// Destroy the video parser
        /// </summary>
        /// CUresult CUDAAPI cuvidDestroyVideoParser(CUvideoparser obj);
        [DllImport(nvCuVidPath, EntryPoint = "cuvidDestroyVideoParser")]
        public static extern CuResult DestroyVideoParser(CuVideoParserPtr obj);




    }
}