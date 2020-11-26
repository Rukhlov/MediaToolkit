using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia
{
    public static class NvEncodeAPI
    {

        private const string _path64 = "nvEncodeAPI64.dll";
        private const string _path32 = "nvEncodeAPI.dll";

        public const uint NVENCAPI_MAJOR_VERSION = 9;//11
        public const uint NVENCAPI_MINOR_VERSION = 1;

        public const uint NVENCAPI_VERSION = NVENCAPI_MAJOR_VERSION | (NVENCAPI_MINOR_VERSION << 24);

        public static readonly uint NV_ENC_CAPS_PARAM_VER = StructVersion(1);
        public static readonly uint NV_ENC_ENCODE_OUT_PARAMS_VER = StructVersion(1);
        public static readonly uint NV_ENC_CREATE_INPUT_BUFFER_VER = StructVersion(1);
        public static readonly uint NV_ENC_CREATE_BITSTREAM_BUFFER_VER = StructVersion(1);
        public static readonly uint NV_ENC_CREATE_MV_BUFFER_VER = StructVersion(1);
        public static readonly uint NV_ENC_RC_PARAMS_VER = StructVersion(1);
        public static readonly uint NV_ENC_CONFIG_VER = StructVersion(7, (uint)1 << 31);
        public static readonly uint NV_ENC_INITIALIZE_PARAMS_VER = StructVersion(5, (uint)1 << 31);
        public static readonly uint NV_ENC_RECONFIGURE_PARAMS_VER = StructVersion(1, (uint)1 << 31);
        public static readonly uint NV_ENC_PRESET_CONFIG_VER = StructVersion(4, (uint)1 << 31);
        public static readonly uint NV_ENC_PIC_PARAMS_MVC_VER = StructVersion(1);
        public static readonly uint NV_ENC_PIC_PARAMS_VER = StructVersion(4, (uint)1 << 31);
        public static readonly uint NV_ENC_MEONLY_PARAMS_VER = StructVersion(3);
        public static readonly uint NV_ENC_LOCK_BITSTREAM_VER = StructVersion(1);
        public static readonly uint NV_ENC_LOCK_INPUT_BUFFER_VER = StructVersion(1);
        public static readonly uint NV_ENC_MAP_INPUT_RESOURCE_VER = StructVersion(4);
        public static readonly uint NV_ENC_REGISTER_RESOURCE_VER = StructVersion(3);
        public static readonly uint NV_ENC_STAT_VER = StructVersion(1);
        public static readonly uint NV_ENC_SEQUENCE_PARAM_PAYLOAD_VER = StructVersion(1);
        public static readonly uint NV_ENC_EVENT_PARAMS_VER = StructVersion(1);
        public static readonly uint NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER = StructVersion(1);
        public static readonly uint NV_ENCODE_API_FUNCTION_LIST_VER = StructVersion(2);

        private static uint StructVersion(uint ver, uint and = 0)
        {
            // #define NVENCAPI_STRUCT_VERSION(ver) ((uint32_t)NVENCAPI_VERSION | ((ver)<<16) | (0x7 << 28))
            return NVENCAPI_VERSION | (ver << 16) | (0x7 << 28) | and;
        }

        // NvEncodeAPICreateInstance
        /**
         * \ingroup ENCODE_FUNC
         * Entry Point to the NvEncodeAPI interface.
         * 
         * Creates an instance of the NvEncodeAPI interface, and populates the
         * pFunctionList with function pointers to the API routines implemented by the
         * NvEncodeAPI interface.
         *
         * \param [out] functionList
         *
         * \return
         * ::NV_ENC_SUCCESS
         * ::NV_ENC_ERR_INVALID_PTR
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NvEncStatus NvEncodeAPICreateInstance(ref NvEncApiFunctionList functionList)
        {
            return Environment.Is64BitProcess ?
                 NvEncodeAPICreateInstance64(ref functionList) :
                 NvEncodeAPICreateInstance32(ref functionList);
        }

        [DllImport(_path32, SetLastError = true, EntryPoint = "NvEncodeAPICreateInstance")]
        private static extern NvEncStatus NvEncodeAPICreateInstance32(ref NvEncApiFunctionList functionList);
        [DllImport(_path64, SetLastError = true, EntryPoint = "NvEncodeAPICreateInstance")]
        private static extern NvEncStatus NvEncodeAPICreateInstance64(ref NvEncApiFunctionList functionList);


        // NvEncodeAPIGetMaxSupportedVersion
        /**
         * \brief Get the largest NvEncodeAPI version supported by the driver.
         *
         * This function can be used by clients to determine if the driver supports
         * the NvEncodeAPI header the application was compiled with.
         *
         * \param [out] version
         *   Pointer to the requested value. The 4 least significant bits in the returned
         *   indicate the minor version and the rest of the bits indicate the major
         *   version of the largest supported version.
         *
         * \return
         * ::NV_ENC_SUCCESS \n
         * ::NV_ENC_ERR_INVALID_PTR \n
         */
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NvEncStatus NvEncodeAPIGetMaxSupportedVersion(out uint version)
        {
            return Environment.Is64BitProcess ?
                NvEncodeAPIGetMaxSupportedVersion64(out version) :
                NvEncodeAPIGetMaxSupportedVersion32(out version);
        }


        [DllImport(_path32, SetLastError = true, EntryPoint = "NvEncodeAPIGetMaxSupportedVersion")]
        private static extern NvEncStatus NvEncodeAPIGetMaxSupportedVersion32(out uint version);
        [DllImport(_path64, SetLastError = true, EntryPoint = "NvEncodeAPIGetMaxSupportedVersion")]
        private static extern NvEncStatus NvEncodeAPIGetMaxSupportedVersion64(out uint version);

    }
}
