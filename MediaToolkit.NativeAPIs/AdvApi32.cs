using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
	public static class AdvApi32
	{

		[Flags]
		public enum ProcessAccessFlags : uint
		{
			All = 0x001F0FFF,
			Terminate = 0x00000001,
			CreateThread = 0x00000002,
			VirtualMemoryOperation = 0x00000008,
			VirtualMemoryRead = 0x00000010,
			VirtualMemoryWrite = 0x00000020,
			DuplicateHandle = 0x00000040,
			CreateProcess = 0x000000080,
			SetQuota = 0x00000100,
			SetInformation = 0x00000200,
			QueryInformation = 0x00000400,
			QueryLimitedInformation = 0x00001000,
			Synchronize = 0x00100000
		}

		public enum TokenAccess
		{
			TOKEN_ASSIGN_PRIMARY = 0x0001,
			TOKEN_DUPLICATE = 0x0002,
			TOKEN_IMPERSONATE = 0x0004,
			TOKEN_QUERY = 0x0008,
			TOKEN_QUERY_SOURCE = 0x0010,
			TOKEN_ADJUST_PRIVILEGES = 0x0020,
			TOKEN_ADJUST_GROUPS = 0x0040,
			TOKEN_ADJUST_DEFAULT = 0x0080,
			TOKEN_ADJUST_SESSIONID = 0x0100,
			STANDARD_RIGHTS_REQUIRED = 0x000F0000,
			STANDARD_RIGHTS_READ = 0x00020000,
			TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY),
			TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
				TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
				TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
				TOKEN_ADJUST_SESSIONID)
		}

		public struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
            public IntPtr Privileges;

            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
            //public LUID_AND_ATTRIBUTES[] Privileges;

        }

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct LUID_AND_ATTRIBUTES
		{
			public LUID Luid;
			public uint Attributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SID_AND_ATTRIBUTES
		{
			public IntPtr Sid;
			public uint Attributes;
		}

		public struct TOKEN_USER
		{
			public SID_AND_ATTRIBUTES User;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct TOKEN_GROUPS
		{
			public uint GroupCount;

			[MarshalAs(UnmanagedType.ByValArray)] 
			public SID_AND_ATTRIBUTES[] Groups;
		}

		public class USEROBJECTFLAGS
		{
			public int fInherit = 0;
			public int fReserved = 0;
			public int dwFlags = 0;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SECURITY_ATTRIBUTES
		{
			public int Length;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
			//public bool bInheritHandle;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PROCESS_INFORMATION
		{
			public IntPtr hProcess;
			public IntPtr hThread;
			public int dwProcessId;
			public int dwThreadId;
		}


		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct STARTUPINFO
		{
			public int cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public int dwX;
			public int dwY;
			public int dwXSize;
			public int dwYSize;
			public int dwXCountChars;
			public int dwYCountChars;
			public int dwFillAttribute;
			public int dwFlags;
			public short wShowWindow;
			public short cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		//TOKEN_INFORMATION_CLASS
		public enum TokenInformationClass
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation,
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUIAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass
		}

		//LOGON_TYPE
		public enum LogonType
		{
			LOGON32_LOGON_INTERACTIVE = 2,
			LOGON32_LOGON_NETWORK,
			LOGON32_LOGON_BATCH,
			LOGON32_LOGON_SERVICE,
			LOGON32_LOGON_UNLOCK = 7,
			LOGON32_LOGON_NETWORK_CLEARTEXT,
			LOGON32_LOGON_NEW_CREDENTIALS
		}

		//LOGON_PROVIDER
		public enum LogonProvider
		{
			LOGON32_PROVIDER_DEFAULT,
			LOGON32_PROVIDER_WINNT35,
			LOGON32_PROVIDER_WINNT40,
			LOGON32_PROVIDER_WINNT50
		}
		[Flags]
		public enum CreateProcessFlags
		{
			CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
			CREATE_DEFAULT_ERROR_MODE = 0x04000000,
			CREATE_NEW_CONSOLE = 0x00000010,
			CREATE_NEW_PROCESS_GROUP = 0x00000200,
			CREATE_NO_WINDOW = 0x08000000,
			CREATE_PROTECTED_PROCESS = 0x00040000,
			CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
			CREATE_SEPARATE_WOW_VDM = 0x00000800,
			CREATE_SHARED_WOW_VDM = 0x00001000,
			CREATE_SUSPENDED = 0x00000004,
			CREATE_UNICODE_ENVIRONMENT = 0x00000400,
			DEBUG_ONLY_THIS_PROCESS = 0x00000002,
			DEBUG_PROCESS = 0x00000001,
			DETACHED_PROCESS = 0x00000008,
			EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
			INHERIT_PARENT_AFFINITY = 0x00010000
		}

		//TOKEN_TYPE
		public enum TokenType : int
		{
			TokenPrimary = 1,
			TokenImpersonation = 2
		}

		//SECURITY_IMPERSONATION_LEVEL
		public enum SecurityImpersonationLevel : int
		{
			SecurityAnonymous = 0,
			SecurityIdentification = 1,
			SecurityImpersonation = 2,
			SecurityDelegation = 3,
		}

		//SID_NAME_USE
		public enum SidNameUse
		{
			SidTypeUser = 1,
			SidTypeGroup,
			SidTypeDomain,
			SidTypeAlias,
			SidTypeWellKnownGroup,
			SidTypeDeletedAccount,
			SidTypeInvalid,
			SidTypeUnknown,
			SidTypeComputer
		}

		public enum LogonFlags
		{
			WithProfile = 1,
			NetCredentialsOnly
		}
		public enum CreationFlags
		{
			DefaultErrorMode = 0x04000000,
			NewConsole = 0x00000010,
			NewProcessGroup = 0x00000200,
			SeparateWOWVDM = 0x00000800,
			Suspended = 0x00000004,
			UnicodeEnvironment = 0x00000400,
			ExtendedStartupInfoPresent = 0x00080000
		}

		public const int TOKEN_DUPLICATE = 0x0002;
		public const uint MAXIMUM_ALLOWED = 0x2000000;
		public const int CREATE_NEW_CONSOLE = 0x00000010;
		public const int CREATE_NO_WINDOW = 0x08000000;
		public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
		public const int STARTF_USESHOWWINDOW = 0x00000001;
		public const int DETACHED_PROCESS = 0x00000008;
		public const int TOKEN_ALL_ACCESS = 0x000f01ff;
		public const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;
		public const int STANDARD_RIGHTS_REQUIRED = 0x000F0000;
		public const int SYNCHRONIZE = 0x00100000;

		public const int IDLE_PRIORITY_CLASS = 0x40;
		public const int NORMAL_PRIORITY_CLASS = 0x20;
		public const int HIGH_PRIORITY_CLASS = 0x80;
		public const int REALTIME_PRIORITY_CLASS = 0x100;


		public const int ANYSIZE_ARRAY = 1;

		public const int UOI_FLAGS = 1;
		public const int UOI_NAME = 2;
		public const int UOI_TYPE = 3;
		public const int UOI_USER_SID = 4;
		public const int UOI_HEAPSIZE = 5;
		public const int UOI_IO = 6;


		//https://docs.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-token_groups_and_privileges
		public const uint SE_GROUP_LOGON_ID = 0xC0000000;



		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AdjustTokenPrivileges(IntPtr tokenHandle,
		   [MarshalAs(UnmanagedType.Bool)]bool disableAllPrivileges,
		   ref TOKEN_PRIVILEGES newState,
		   UInt32 bufferLengthInBytes,
		   ref TOKEN_PRIVILEGES previousState,
		   out UInt32 returnLengthInBytes);

		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool CreateProcessAsUser(
			IntPtr hToken,
			string lpApplicationName,
			string lpCommandLine,
			ref SECURITY_ATTRIBUTES lpProcessAttributes,
			ref SECURITY_ATTRIBUTES lpThreadAttributes,
			bool bInheritHandles,
			uint dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool AllocateLocallyUniqueId(out IntPtr pLuid);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = false)]
		public static extern SECUR32.WinErrors LsaNtStatusToWinError(SECUR32.WinStatusCodes status);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern bool GetTokenInformation(
			IntPtr TokenHandle,
			SECUR32.TOKEN_INFORMATION_CLASS TokenInformationClass,
			IntPtr TokenInformation,
			uint TokenInformationLength,
			out uint ReturnLength);


		[DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool ConvertSidToStringSid(IntPtr pSID, out IntPtr ptrSid);


		[DllImport("advapi32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool LogonUser(
			[MarshalAs(UnmanagedType.LPStr)] string pszUserName,
			[MarshalAs(UnmanagedType.LPStr)] string pszDomain,
			[MarshalAs(UnmanagedType.LPStr)] string pszPassword,
			int dwLogonType,
			int dwLogonProvider,
			out IntPtr phToken);

		[DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
		public static extern bool OpenProcessToken(IntPtr ProcessHandle, TokenAccess DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public extern static bool DuplicateTokenEx(
			IntPtr hExistingToken,
			TokenAccess dwDesiredAccess,
			//uint dwDesiredAccess,
			ref SECURITY_ATTRIBUTES lpTokenAttributes,
			SecurityImpersonationLevel ImpersonationLevel,
			TokenType TokenType,
			out IntPtr phNewToken);

		[DllImport("advapi32.dll", SetLastError = false)]
		public static extern uint LsaNtStatusToWinError(uint status);



		[DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern bool CreateProcessWithTokenW(
			IntPtr hToken,
			LogonFlags dwLogonFlags,
			string lpApplicationName,
			string lpCommandLine,
			CreationFlags dwCreationFlags,
			IntPtr lpEnvironment,
			string lpCurrentDirectory,
			[In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);



		[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool LookupPrivilegeName(string lpSystemName, IntPtr lpLuid, StringBuilder lpName, ref int cchName);

    }
}
