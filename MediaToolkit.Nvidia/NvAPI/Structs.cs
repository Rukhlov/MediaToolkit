using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Nvidia.NvAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DRSSessionHandle
    {
        internal readonly IntPtr Handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DRSProfileHandle
    {
        internal readonly IntPtr Handle;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSApplicationV1
    {
        //Structure Version
        public uint version;

        //Is the application userdefined/predefined
        public uint isPredefined;

        // String name of the Application
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string appName;

        //serFriendly name of the Application
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;

        //Indicates the name(if any) of the launcher that starts the application
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public class DRSApplicationV2
    {
        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string fileInFolder;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSApplicationV3
    {
        public uint version;
        public uint isPredefined;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string appName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string userFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string launcher;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string fileInFolder;

        public uint isMetro
        {
            get
            {
                return ((uint)((bitvector1 & 1)));
            }
            set
            {
                bitvector1 = ((uint)((value | bitvector1)));
            }
        }

        public uint isCommandLine;


        private uint bitvector1;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSProfile
    {
        public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string profileName;
        public DRSGPUSupport gpuSupport;
        public uint isPredefined;
        public uint numOfApps;
        public uint numOfSettings;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct DRSSettingValues
    {
        public uint version;
        public uint numSettingValues;
        public DRSSettingType settingType;
        public DRSSettingUnion defaultValue;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)NvAPI.NVAPI_SETTING_MAX_VALUES)]
        public DRSSettingUnion[] settingValues;
    }

    //NVDRS_SETTING
    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSSetting
    {
        public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.NVAPI_UNICODE_STRING_MAX)]
        public string settingName;
        public uint settingId;
        public DRSSettingType settingType;
        public DRSSettingLocation settingLocation;
        public uint isCurrentPredefined;
        public uint isPredefinedValid;
        public DRSSettingUnion predefinedValue;
        public DRSSettingUnion currentValue;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode, Size = 4100)]
    public struct DRSSettingUnion
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4100)]
        public byte[] rawData;

        public byte[] binaryValue
        {
            get
            {
                var length = BitConverter.ToUInt32(rawData, 0);
                var tmpData = new byte[length];
                Buffer.BlockCopy(rawData, 4, tmpData, 0, (int)length);
                return tmpData;
            }

            set
            {
                rawData = new byte[4100];
                if (value != null)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(value.Length), 0, rawData, 0, 4);
                    Buffer.BlockCopy(value, 0, rawData, 4, value.Length);
                }
            }
        }

        public uint dwordValue
        {
            get
            {
                return BitConverter.ToUInt32(rawData, 0);
            }

            set
            {
                rawData = new byte[4100];
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, rawData, 0, 4);
            }
        }

        public string stringValue
        {
            get
            {
                return Encoding.Unicode.GetString(rawData).Split(new[] { '\0' }, 2)[0];
            }

            set
            {
                rawData = new byte[4100];
                var bytesRaw = Encoding.Unicode.GetBytes(value);
                Buffer.BlockCopy(bytesRaw, 0, rawData, 0, bytesRaw.Length);
            }
        }

        public string ansiStringValue
        {
            get
            {
                return Encoding.Default.GetString(rawData).Split(new[] { '\0' }, 2)[0];
            }

            set
            {
                rawData = new byte[4100];
                var bytesRaw = Encoding.Default.GetBytes(value);
                Buffer.BlockCopy(bytesRaw, 0, rawData, 0, bytesRaw.Length);
            }
        }

    }

}
