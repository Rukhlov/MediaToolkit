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

		public struct TOKEN_PRIVILEGES
		{
			public struct LUID
			{
				public UInt32 LowPart;
				public Int32 HighPart;
			}

			[StructLayout(LayoutKind.Sequential, Pack = 4)]
			public struct LUID_AND_ATTRIBUTES
			{
				public LUID Luid;
				public UInt32 Attributes;
			}

			public int PrivilegeCount;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = ANYSIZE_ARRAY)]
			public LUID_AND_ATTRIBUTES[] Privileges;
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
			public bool bInheritHandle;
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
			public Int32 cb;
			public string lpReserved;
			public string lpDesktop;
			public string lpTitle;
			public Int32 dwX;
			public Int32 dwY;
			public Int32 dwXSize;
			public Int32 dwYSize;
			public Int32 dwXCountChars;
			public Int32 dwYCountChars;
			public Int32 dwFillAttribute;
			public Int32 dwFlags;
			public Int16 wShowWindow;
			public Int16 cbReserved2;
			public IntPtr lpReserved2;
			public IntPtr hStdInput;
			public IntPtr hStdOutput;
			public IntPtr hStdError;
		}

		public enum TOKEN_INFORMATION_CLASS
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

		public enum LOGON_TYPE
		{
			LOGON32_LOGON_INTERACTIVE = 2,
			LOGON32_LOGON_NETWORK,
			LOGON32_LOGON_BATCH,
			LOGON32_LOGON_SERVICE,
			LOGON32_LOGON_UNLOCK = 7,
			LOGON32_LOGON_NETWORK_CLEARTEXT,
			LOGON32_LOGON_NEW_CREDENTIALS
		}
		public enum LOGON_PROVIDER
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

		public enum TOKEN_TYPE : int
		{
			TokenPrimary = 1,
			TokenImpersonation = 2
		}

		public enum SECURITY_IMPERSONATION_LEVEL : int
		{
			SecurityAnonymous = 0,
			SecurityIdentification = 1,
			SecurityImpersonation = 2,
			SecurityDelegation = 3,
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
		public const UInt32 SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
		public const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
		public const UInt32 SE_PRIVILEGE_REMOVED = 0x00000004;
		public const UInt32 SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
		public const Int32 ANYSIZE_ARRAY = 1;

		public const int UOI_FLAGS = 1;
		public const int UOI_NAME = 2;
		public const int UOI_TYPE = 3;
		public const int UOI_USER_SID = 4;
		public const int UOI_HEAPSIZE = 5;
		public const int UOI_IO = 6;

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
		public static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public extern static bool DuplicateTokenEx(
			IntPtr hExistingToken,
			uint dwDesiredAccess,
			ref SECURITY_ATTRIBUTES lpTokenAttributes,
			SECURITY_IMPERSONATION_LEVEL ImpersonationLevel,
			TOKEN_TYPE TokenType,
			out IntPtr phNewToken);

		[DllImport("advapi32.dll", SetLastError = false)]
		public static extern uint LsaNtStatusToWinError(uint status);


	}
}
