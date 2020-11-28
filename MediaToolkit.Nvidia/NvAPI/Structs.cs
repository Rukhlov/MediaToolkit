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

	//NV_CHIPSET_INFO_v1
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV1
	{
		public ChipsetInfoV1()
		{
			version = NvAPI.MakeVersion<ChipsetInfoV1>(1);
		}

		//NvU32 version;        //structure version
		public uint version;

		//NvU32 vendorId;       //vendor ID
		public uint vendorId;

		//NvU32 deviceId;       //device ID
		public uint deviceId;

		//NvAPI_ShortString szVendorName;   //vendor Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.ShortStringMax)]
		public string vendorName;

		//NvAPI_ShortString szChipsetName;  //device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.ShortStringMax)]
		public string chipsetName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV2: ChipsetInfoV1
	{
		public ChipsetInfoV2()
		{
			version = NvAPI.MakeVersion<ChipsetInfoV2>(2);
		}
		//NvU32 flags;	//!< Chipset info flags
		public uint flags;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV3 : ChipsetInfoV2
	{
		public ChipsetInfoV3()
		{
			version = NvAPI.MakeVersion<ChipsetInfoV3> (3);
		}

		//NvU32 subSysVendorId;     //!< subsystem vendor ID
		uint subSysVendorId;

		//NvU32 subSysDeviceId;     //!< subsystem device ID
		uint subSysDeviceId;

		//NvAPI_ShortString szSubSysVendorName; //!< subsystem vendor Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.ShortStringMax)]
		public string subSysVendorName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV4 : ChipsetInfoV3
	{
		public ChipsetInfoV4()
		{
			version = NvAPI.MakeVersion<ChipsetInfoV4>(4);
		}

		//NvU32 HBvendorId;         //!< Host bridge vendor identification
		uint HBvendorId;

		//NvU32 HBdeviceId;         //!< Host bridge device identification
		uint HBdeviceId;

		//NvU32 HBsubSysVendorId;   //!< Host bridge subsystem vendor identification
		uint HBsubSysVendorId;

		//NvU32 HBsubSysDeviceId;   //!< Host bridge subsystem device identification
		uint HBsubSysDeviceId;
	}


	//NVDRS_APPLICATION_V1
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV1
	{
		public DRSApplicationV1()
		{
			version = NvAPI.MakeVersion<DRSApplicationV1>(1);
		}

		//Structure Version
		public uint version;

		//Is the application userdefined/predefined
		public uint isPredefined;

		// String name of the Application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		public string appName;

		//serFriendly name of the Application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		public string userFriendlyName;

		//Indicates the name(if any) of the launcher that starts the application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		public string launcher;
	}

	//NVDRS_APPLICATION_V2
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV2 : DRSApplicationV1
	{
		public DRSApplicationV2()
		{
			version = NvAPI.MakeVersion<DRSApplicationV2>(2);
		}

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		public string fileInFolder;
	}

	//NVDRS_APPLICATION_V3
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV3 : DRSApplicationV2
	{
		public DRSApplicationV3()
		{
			version = NvAPI.MakeVersion<DRSApplicationV3>(3);
		}

		public uint isMetro = 0;
		public uint isCommandLine;
		public uint reserved = 0;
	}

	//NVDRS_APPLICATION_V4
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV4 : DRSApplicationV3
	{
		public DRSApplicationV4()
		{
			version = NvAPI.MakeVersion<DRSApplicationV4>(4);
		}

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
		public string commandLine;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSProfile
    {
		public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)NvAPI.SettingsMaxValue)]
        public DRSSettingUnion[] settingValues;
    }

    //NVDRS_SETTING
    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSSetting
    {
        public uint version;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvAPI.UnicodeMaxString)]
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
