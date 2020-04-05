using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Utils;

namespace MediaToolkit.NativeAPIs.MF
{
	public class MfApi
	{

		[DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
		public static extern HResult MFTGetInfo([In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
			[MarshalAs(UnmanagedType.LPWStr)] out string pszName,
			out IntPtr ppInputTypes, out uint pcInputTypes,
			out IntPtr ppOutputTypes, out uint pcOutputTypes,
			out IntPtr ppAttributes);

		[DllImport("mfplat.dll", CharSet = CharSet.Unicode, ExactSpelling = true), SuppressUnmanagedCodeSecurity]
		public static extern HResult _MFTGetInfo(
			[In, MarshalAs(UnmanagedType.Struct)] Guid clsidMFT,
			[MarshalAs(UnmanagedType.LPWStr)] out string pszName,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))]
			ArrayList ppInputTypes,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "0", MarshalTypeRef = typeof(RTIMarshaler))]
			MFInt pcInputTypes,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))]
			ArrayList ppOutputTypes,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "1", MarshalTypeRef = typeof(RTIMarshaler))]
			MFInt pcOutputTypes,
			IntPtr ip // Must be IntPtr.Zero due to MF bug, but should be out IMFAttributes ppAttributes
			);

		[DllImport("mfplat.dll", ExactSpelling = true), SuppressUnmanagedCodeSecurity]
		public static extern HResult MFTEnumEx(
			[In] Guid MFTransformCategory,
			MFT_EnumFlag Flags,
			[In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pInputType,
			[In, MarshalAs(UnmanagedType.LPStruct)] MFTRegisterTypeInfo pOutputType,
			[Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.IUnknown, SizeParamIndex = 5)] out IMFActivate[] pppMFTActivate,
			out int pnumMFTActivate
		);


		[Flags]
		public enum MFT_EnumFlag
		{
			None = 0x00000000,
			SyncMFT = 0x00000001,   // Enumerates V1 MFTs. This is default.
			AsyncMFT = 0x00000002,   // Enumerates only software async MFTs also known as V2 MFTs
			Hardware = 0x00000004,   // Enumerates V2 hardware async MFTs
			FieldOfUse = 0x00000008,   // Enumerates MFTs that require unlocking
			LocalMFT = 0x00000010,   // Enumerates Locally (in-process) registered MFTs
			TranscodeOnly = 0x00000020,   // Enumerates decoder MFTs used by transcode only
			SortAndFilter = 0x00000040,   // Apply system local, do not use and preferred sorting and filtering
			SortAndFilterApprovedOnly = 0x000000C0,   // Similar to MFT_ENUM_FLAG_SORTANDFILTER, but apply a local policy of: MF_PLUGIN_CONTROL_POLICY_USE_APPROVED_PLUGINS
			SortAndFilterWebOnly = 0x00000140,   // Similar to MFT_ENUM_FLAG_SORTANDFILTER, but apply a local policy of: MF_PLUGIN_CONTROL_POLICY_USE_WEB_PLUGINS
			SortAndFilterWebOnlyEdgemode = 0x00000240,
			All = 0x0000003F    // Enumerates all MFTs including SW and HW MFTs and applies filtering
		}

	}


}
