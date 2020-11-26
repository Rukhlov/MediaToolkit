using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MediaToolkit.Nvidia
{
    public enum LibNcEncInitializeStatus
    {
        Success,
        DllNotFound,
        UnsupportedVersion,
        Failure
    }

    public static class LibNvEnc
    {
        private const string _path64 = "nvEncodeAPI64.dll";
        private const string _path32 = "nvEncodeAPI.dll";

        public const uint NVENCAPI_MAJOR_VERSION = 9;
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

        public static NvEncApiFunctionList NvEncApiFunc;

        private static bool _isInitialized = false;

        [DllImport(_path64, SetLastError = true, EntryPoint = "NvEncodeAPICreateInstance")]
        private static extern NvEncStatus NvEncodeAPICreateInstance64(ref NvEncApiFunctionList functionList);

        [DllImport(_path64, SetLastError = true, EntryPoint = "NvEncodeAPIGetMaxSupportedVersion")]
        private static extern NvEncStatus NvEncodeAPIGetMaxSupportedVersion64(out uint version);

        [DllImport(_path32, SetLastError = true, EntryPoint = "NvEncodeAPICreateInstance")]
        private static extern NvEncStatus NvEncodeAPICreateInstance32(ref NvEncApiFunctionList functionList);

        [DllImport(_path32, SetLastError = true, EntryPoint = "NvEncodeAPIGetMaxSupportedVersion")]
        private static extern NvEncStatus NvEncodeAPIGetMaxSupportedVersion32(out uint version);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NvEncStatus NvEncodeAPICreateInstance(ref NvEncApiFunctionList functionList) =>
            Environment.Is64BitProcess ? NvEncodeAPICreateInstance64(ref functionList) : NvEncodeAPICreateInstance32(ref functionList);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NvEncStatus NvEncodeAPIGetMaxSupportedVersion(out uint version) =>
            Environment.Is64BitProcess ? NvEncodeAPIGetMaxSupportedVersion64(out version) : NvEncodeAPIGetMaxSupportedVersion32(out version);


        private static uint StructVersion(uint ver, uint and = 0)
        {
            // #define NVENCAPI_STRUCT_VERSION(ver) ((uint32_t)NVENCAPI_VERSION | ((ver)<<16) | (0x7 << 28))
            return NVENCAPI_VERSION | (ver << 16) | (0x7 << 28) | and;
        }


        public static bool Initialize()
        {
            if (_isInitialized)
            {
                return false;
            }

            var status = NvEncodeAPIGetMaxSupportedVersion(out var version);
            if (status != NvEncStatus.Success)
            {
                throw new LibNvEncException(nameof(NvEncodeAPIGetMaxSupportedVersion), "", status);
            }

            const uint currentVersion = (NVENCAPI_MAJOR_VERSION << 4) | NVENCAPI_MINOR_VERSION;
            if (currentVersion > version)
            {
                var descr = $"Installed NvEnc version is {version >> 4}.{version & 0xF}, version must be at least {NVENCAPI_MAJOR_VERSION}.{NVENCAPI_MINOR_VERSION}. Please upgrade the nvidia display drivers.";
                throw new NotSupportedException(descr);
            }

            var functionList = new NvEncApiFunctionList
            {
                Version = NV_ENCODE_API_FUNCTION_LIST_VER
            };

            status = NvEncodeAPICreateInstance(ref functionList);
            if (status != NvEncStatus.Success)
            {
                throw new LibNvEncException(nameof(NvEncodeAPICreateInstance), "", status);
            }

            NvEncApiFunc = functionList;
            _isInitialized = true;

            return true;

        }

        public static LibNcEncInitializeStatus TryInitialize(out string failedDescription)
        {
            failedDescription = "";

            LibNcEncInitializeStatus result = LibNcEncInitializeStatus.Success;
            try
            {
                Initialize();
            }
            catch (NotSupportedException ex)
            {
                failedDescription = ex.Message;
                result = LibNcEncInitializeStatus.UnsupportedVersion;
            }
            catch (DllNotFoundException e)
            {
                failedDescription = e.Message;
                result = LibNcEncInitializeStatus.DllNotFound;
            }
            catch (Exception e)
            {
                failedDescription = e.ToString();
                result = LibNcEncInitializeStatus.Failure;
            }

            return result;
        }


        public static NvEncoder OpenEncoder(ref NvEncOpenEncodeSessionExParams sessionParams)
        {
            Initialize();

            var status = NvEncApiFunc.OpenEncodeSessionEx(ref sessionParams, out var encoderPtr);
            if (status != NvEncStatus.Success)
            {
                throw new LibNvEncException("OpenEncodeSessionEx", "", status);
            }

            return new NvEncoder(encoderPtr);
        }

        public static NvEncoder OpenEncoderForDirectX(IntPtr deviceHandle)
        {
            var ps = new NvEncOpenEncodeSessionExParams
            {
                Version = NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
                ApiVersion = NVENCAPI_VERSION,
                Device = deviceHandle,
                DeviceType = NvEncDeviceType.Directx
            };

            return OpenEncoder(ref ps);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CheckResult(NvEncoder encoder, NvEncStatus status, [CallerMemberName] string callerName = "")
        {

            if (status != NvEncStatus.Success)
            {
                var descr = "";

                if (encoder != null)
                {
                    descr = encoder.GetLastError();
                }

                throw new LibNvEncException(callerName, descr, status);
            }
        }

    }
}
