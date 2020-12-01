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
			version = NvApi.MakeVersion<ChipsetInfoV1>(1);
		}

		//NvU32 version;        //structure version
		public uint version;

		//NvU32 vendorId;       //vendor ID
		public uint vendorId;

		//NvU32 deviceId;       //device ID
		public uint deviceId;

		//NvAPI_ShortString szVendorName;   //vendor Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string vendorName;

		//NvAPI_ShortString szChipsetName;  //device Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string chipsetName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV2: ChipsetInfoV1
	{
		public ChipsetInfoV2()
		{
			version = NvApi.MakeVersion<ChipsetInfoV2>(2);
		}
		//NvU32 flags;	//!< Chipset info flags
		public uint flags;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV3 : ChipsetInfoV2
	{
		public ChipsetInfoV3()
		{
			version = NvApi.MakeVersion<ChipsetInfoV3> (3);
		}

		//NvU32 subSysVendorId;     //!< subsystem vendor ID
		uint subSysVendorId;

		//NvU32 subSysDeviceId;     //!< subsystem device ID
		uint subSysDeviceId;

		//NvAPI_ShortString szSubSysVendorName; //!< subsystem vendor Name
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string subSysVendorName;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV4 : ChipsetInfoV3
	{
		public ChipsetInfoV4()
		{
			version = NvApi.MakeVersion<ChipsetInfoV4>(4);
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
			version = NvApi.MakeVersion<DRSApplicationV1>(1);
		}

		//Structure Version
		public uint version;

		//Is the application userdefined/predefined
		public uint isPredefined;

		// String name of the Application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string appName;

		//serFriendly name of the Application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string userFriendlyName;

		//Indicates the name(if any) of the launcher that starts the application
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string launcher;
	}

	//NVDRS_APPLICATION_V2
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV2 : DRSApplicationV1
	{
		public DRSApplicationV2()
		{
			version = NvApi.MakeVersion<DRSApplicationV2>(2);
		}

        //Select this application only if this file is found.
        // When specifying multiple files, separate them using the ':' character.
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string fileInFolder;
	}

	//NVDRS_APPLICATION_V3
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV3 : DRSApplicationV2
	{
		public DRSApplicationV3()
		{
			version = NvApi.MakeVersion<DRSApplicationV3>(3);
		}

        //-----------------------------------------
        // не реализовано т.к нигде не используется...
        public uint bitvector;
        //NvU32 isMetro:1;//!< Windows 8 style app
        //public uint isMetro
        //{
        //    get
        //    {
        //        return ((uint)((bitvector & 1)));
        //    }
        //    set
        //    {
        //        bitvector = ((uint)((value | bitvector)));
        //    }
        //}
        //NvU32 isCommandLine:1; //!< Command line parsing for the application name
        //public uint isCommandLine ........
        //NvU32 reserved:30;//!< Reserved. Should be 0.
        //public uint reserved ...
        //----------------------------------------------
    }

    //NVDRS_APPLICATION_V4
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV4 : DRSApplicationV3
	{
		public DRSApplicationV4()
		{
			version = NvApi.MakeVersion<DRSApplicationV4>(4);
		}

        //If isCommandLine is set to 0 this must be an empty.
        //If isCommandLine is set to 1 this contains application's command line as if it was returned by GetCommandLineW.
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string commandLine;
	}

    //NVDRS_PROFILE
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class DRSProfile
    {
        public DRSProfile()
        {
            version = NvApi.MakeVersion<DRSProfile>(1);
        }

        public uint version;

        //NvAPI_UnicodeString profileName; String name of the Profile
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
        public string profileName;

        //NVDRS_GPU_SUPPORT gpuSupport; //!< This read-only flag indicates the profile support on either Quadro, or Geforce, or both.
        public DRSGPUSupport gpuSupport;

        //NvU32 isPredefined; //!< Is the Profile user-defined, or predefined
        public uint isPredefined;

        //NvU32 numOfApps; //!< Total number of applications that belong to this profile. Read-only
        public uint numOfApps;

        //NvU32 numOfSettings;//!< Total number of settings applied for this Profile. Read-only
        public uint numOfSettings;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    internal struct DRSSettingValues
    {
        public uint version;
        public uint numSettingValues;
        public DRSSettingType settingType;
        public DRSSettingUnion defaultValue;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =NvApi.SettingsMaxValue)]
        public DRSSettingUnion[] settingValues;
    }

    //NVDRS_SETTING_V1
    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct DRSSettingV1
    {
		//public DRSSettingV1()
		//{
		//	version = NvApi.MakeVersion<DRSSettingV1>(1);
		//}

		// NvU32 version; //!< Structure Version
		public uint version;

        //NvAPI_UnicodeString settingName; //!< String name of setting
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
        public string settingName;

        //NvU32 settingId; //!< 32 bit setting Id
        public uint settingId;

        //NVDRS_SETTING_TYPE settingType; //!< Type of setting value.  
        public DRSSettingType settingType;

        //NVDRS_SETTING_LOCATION settingLocation; //!< Describes where the value in CurrentValue comes from. 
        public DRSSettingLocation settingLocation;

        // NvU32 isCurrentPredefined;    //!< It is different than 0 if the currentValue is a predefined Value, 
        public uint isCurrentPredefined;

        //NvU32 isPredefinedValid; //!< It is different than 0 if the PredefinedValue union contains a valid value. 
        public uint isPredefinedValid;

        public DRSSettingUnion predefinedValue;
        public DRSSettingUnion currentValue;

    }



    /*
    *   union //!< Setting can hold either DWORD or Binary value or string. Not mixed types.
        {
            NvU32                      u32PredefinedValue;    //!< Accessing default DWORD value of this setting.
            NVDRS_BINARY_SETTING       binaryPredefinedValue; //!< Accessing default Binary value of this setting.
                                                            //!< Must be allocated by caller with valueLength specifying buffer size, 
                                                            //!< or only valueLength will be filled in.
            NvAPI_UnicodeString        wszPredefinedValue;    //!< Accessing default unicode string value of this setting.
        };
     */

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode, Size = (NvApi.BinaryDataMax + 4))]
    public struct DRSSettingUnion
    {
        public const int size = (NvApi.BinaryDataMax + 4);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (NvApi.BinaryDataMax + 4) )]
        public byte[] rawData;

        public uint dwordValue
        {
            get
            {
                return BitConverter.ToUInt32(rawData, 0);
            }

            set
            {
                rawData = new byte[size];
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, rawData, 0, 4);
            }
        }

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
                rawData = new byte[size];
                if (value != null)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(value.Length), 0, rawData, 0, 4);
                    Buffer.BlockCopy(value, 0, rawData, 4, value.Length);
                }
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
                rawData = new byte[size];
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
                rawData = new byte[size];
                var bytesRaw = Encoding.Default.GetBytes(value);
                Buffer.BlockCopy(bytesRaw, 0, rawData, 0, bytesRaw.Length);
            }
        }

    }

}
