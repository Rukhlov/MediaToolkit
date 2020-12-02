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

    //NV_CHIPSET_INFO_FLAGS
    public enum ChipsetInfoFlags : uint
    {
        //NV_CHIPSET_INFO_HYBRID
        Hybrid = 0x00000001, //не работает, как определить гибридный GPU?!?! 
    }

    //NV_CHIPSET_INFO_v1
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV1
	{
		public ChipsetInfoV1()
		{
			version = NvApi.MakeVersion<ChipsetInfoV1>(1);
		}

        /// <summary>
        /// structure version
        /// </summary>
        public uint version;

        /// <summary>
        /// vendor ID
        /// </summary>
        public uint vendorId;

        /// <summary>
        /// device ID
        /// </summary>
        public uint deviceId;

        /// <summary>
        ///  vendor Name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string vendorName;

        /// <summary>
        /// device Name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string chipsetName;


        public override string ToString()
        {
            return "version: " + version + "\r\n" +
                   "vendorId: " + vendorId + "\r\n" +
                   "deviceId: " + deviceId + "\r\n" +
                   "vendorName: " + vendorName + "\r\n" +
                   "chipsetName: " + chipsetName + "\r\n";
        }
    }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV2: ChipsetInfoV1
	{
		public ChipsetInfoV2()
		{
			version = NvApi.MakeVersion<ChipsetInfoV2>(2);
		}

        /// <summary>
        /// Chipset info flags - obsolete
        /// </summary>
        public uint flags;

        public override string ToString()
        {
            return base.ToString() + 
                "flags: " + flags + "\r\n";
        }
    }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV3 : ChipsetInfoV2
	{
		public ChipsetInfoV3()
		{
			version = NvApi.MakeVersion<ChipsetInfoV3> (3);
		}

        /// <summary>
        /// subsystem vendor ID
        /// </summary>
        public uint subSysVendorId;

        /// <summary>
        ///  subsystem device ID
        /// </summary>
        public uint subSysDeviceId;

        /// <summary>
        /// subsystem vendor Name
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NvApi.ShortStringMax)]
		public string subSysVendorName;

        public override string ToString()
        {
            return base.ToString() +
                "subSysVendorId: " + subSysVendorId + "\r\n" +
                "subSysDeviceId: " + subSysDeviceId + "\r\n" +
                "subSysVendorName: " + subSysVendorName + "\r\n";
        }
    }

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class ChipsetInfoV4 : ChipsetInfoV3
	{
		public ChipsetInfoV4()
		{
			version = NvApi.MakeVersion<ChipsetInfoV4>(4);
		}

        /// <summary>
        /// Host bridge vendor identification
        /// </summary>
        public uint HBvendorId;

        /// <summary>
        /// Host bridge device identification
        /// </summary>
        public uint HBdeviceId;

        /// <summary>
        /// Host bridge subsystem vendor identification
        /// </summary>
        public uint HBsubSysVendorId;

        /// <summary>
        /// Host bridge subsystem device identification
        /// </summary>
        public uint HBsubSysDeviceId;

        public override string ToString()
        {
            return base.ToString() +
                   "HBvendorId: " + HBvendorId + "\r\n" +
                   "HBdeviceId: " + HBdeviceId + "\r\n" +
                   "HBsubSysVendorId: " + HBsubSysVendorId + "\r\n" +
                   "HBsubSysDeviceId: " + HBsubSysDeviceId + "\r\n";
        }

    }


	//NVDRS_APPLICATION_V1
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DRSApplicationV1
	{
		public DRSApplicationV1()
		{
			version = NvApi.MakeVersion<DRSApplicationV1>(1);
		}

        /// <summary>
        /// Structure Version
        /// </summary>
        public uint version;

        /// <summary>
        /// Is the application userdefined/predefined
        /// </summary>
        public uint isPredefined;

        /// <summary>
        ///  String name of the Application
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string appName;

        /// <summary>
        /// UserFriendly name of the Application
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
		public string userFriendlyName;

        /// <summary>
        /// Indicates the name(if any) of the launcher that starts the application
        /// </summary>
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

        /// <summary>
        ///Select this application only if this file is found.
        /// When specifying multiple files, separate them using the ':' character.
        /// </summary>
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

        /// <summary>
        ///  не реализовано т.к нигде не используется...
        /// </summary>
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

        /// <summary>
        /// If isCommandLine is set to 0 this must be an empty.
        /// If isCommandLine is set to 1 this contains application's command line as if it was returned by GetCommandLineW.
        /// </summary>

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

        /// <summary>
        /// NvAPI_UnicodeString profileName; String name of the Profile
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NvApi.UnicodeStringMax)]
        public string profileName;

        /// <summary>
        /// This read-only flag indicates the profile support on either Quadro, or Geforce, or both.
        /// NVDRS_GPU_SUPPORT gpuSupport;
        /// </summary>
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

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode, Size = (NvApi.BinaryDataMax + 4))] //BinaryDataMax + sizeof(Uint32)
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
