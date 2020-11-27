using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static MediaToolkit.Nvidia.NvEncodeAPI;

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

        public static NvEncApiFunctionList NvEncApiFunc;

        private static bool _isInitialized = false;

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
                var descr = $"Installed NvEnc version is {version >> 4}.{version & 0xF}, version must be at least { NVENCAPI_MAJOR_VERSION}.{ NVENCAPI_MINOR_VERSION}. Please upgrade the nvidia display drivers.";
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
