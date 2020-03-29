using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
	public static class SECUR32
	{
		public enum WinStatusCodes : uint
		{
			STATUS_SUCCESS = 0
		}

		public enum WinErrors : uint
		{
			NO_ERROR = 0,
		}
		public enum WinLogonType
		{
			LOGON32_LOGON_INTERACTIVE = 2,
			LOGON32_LOGON_NETWORK = 3,
			LOGON32_LOGON_BATCH = 4,
			LOGON32_LOGON_SERVICE = 5,
			LOGON32_LOGON_UNLOCK = 7,
			LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
			LOGON32_LOGON_NEW_CREDENTIALS = 9
		}

		// SECURITY_LOGON_TYPE
		public enum SecurityLogonType
		{
			Interactive = 2,    // Interactively logged on (locally or remotely)
			Network,        // Accessing system via network
			Batch,          // Started via a batch queue
			Service,        // Service started by service controller
			Proxy,          // Proxy logon
			Unlock,         // Unlock workstation
			NetworkCleartext,   // Network logon with cleartext credentials
			NewCredentials,     // Clone caller, new default credentials
			RemoteInteractive,  // Remote, yet interactive. Terminal server
			CachedInteractive,  // Try cached credentials without hitting the net.
			CachedRemoteInteractive, // Same as RemoteInteractive, this is used internally for auditing purpose
			CachedUnlock    // Cached Unlock workstation
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LSA_UNICODE_STRING
		{
			public UInt16 Length;
			public UInt16 MaximumLength;
			public IntPtr Buffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct TOKEN_SOURCE
		{
			public TOKEN_SOURCE(string name)
			{
				SourceName = new byte[8];
				System.Text.Encoding.GetEncoding(1252).GetBytes(name, 0, name.Length, SourceName, 0);
				if (!AdvApi32.AllocateLocallyUniqueId(out SourceIdentifier))
					throw new System.ComponentModel.Win32Exception();
			}

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] SourceName;
			public IntPtr SourceIdentifier;
		}
		[StructLayout(LayoutKind.Sequential)]
		public struct KERB_INTERACTIVE_LOGON
		{
			public KERB_LOGON_SUBMIT_TYPE MessageType;
			public string LogonDomainName;
			public string UserName;
			public string Password;
		}

		public enum KERB_LOGON_SUBMIT_TYPE
		{
			KerbInteractiveLogon = 2,
			KerbSmartCardLogon = 6,
			KerbWorkstationUnlockLogon = 7,
			KerbSmartCardUnlockLogon = 8,
			KerbProxyLogon = 9,
			KerbTicketLogon = 10,
			KerbTicketUnlockLogon = 11,
			KerbS4ULogon = 12,
			KerbCertificateLogon = 13,
			KerbCertificateS4ULogon = 14,
			KerbCertificateUnlockLogon = 15
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
		[StructLayout(LayoutKind.Sequential)]
		public struct QUOTA_LIMITS
		{
			UInt32 PagedPoolLimit;
			UInt32 NonPagedPoolLimit;
			UInt32 MinimumWorkingSetSize;
			UInt32 MaximumWorkingSetSize;
			UInt32 PagefileLimit;
			Int64 TimeLimit;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LSA_STRING
		{
			public UInt16 Length;
			public UInt16 MaximumLength;
			public /*PCHAR*/ IntPtr Buffer;
		}


		[DllImport("secur32.dll", SetLastError = true)]
		public static extern WinStatusCodes LsaLogonUser(
			[In] IntPtr LsaHandle,
			[In] ref LSA_STRING OriginName,
			[In] SecurityLogonType LogonType,
			[In] UInt32 AuthenticationPackage,
			[In] IntPtr AuthenticationInformation,
			[In] UInt32 AuthenticationInformationLength,
			[In] /*PTOKEN_GROUPS*/ IntPtr LocalGroups,
			[In] ref TOKEN_SOURCE SourceContext,
			[Out] /*PVOID*/ out IntPtr ProfileBuffer,
			[Out] out UInt32 ProfileBufferLength,
			[Out] out Int64 LogonId,
			[Out] out IntPtr Token,
			[Out] out QUOTA_LIMITS Quotas,
			[Out] out WinStatusCodes SubStatus
			);

		[DllImport("secur32.dll", SetLastError = true)]
		public static extern WinStatusCodes LsaRegisterLogonProcess(
			IntPtr LogonProcessName,
			out IntPtr LsaHandle,
			out ulong SecurityMode
			);

		[DllImport("secur32.dll", SetLastError = false)]
		public static extern WinStatusCodes LsaLookupAuthenticationPackage([In] IntPtr LsaHandle, [In] ref LSA_STRING PackageName, [Out] out UInt32 AuthenticationPackage);

		[DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[ResourceExposure(ResourceScope.None)]
		internal static extern int LsaConnectUntrusted([In, Out] ref SafeLsaLogonProcessHandle LsaHandle);

		[DllImport("secur32.dll", SetLastError = false)]
		public static extern WinStatusCodes LsaConnectUntrusted([Out] out IntPtr LsaHandle);

		[System.Security.SecurityCritical]  // auto-generated
		internal sealed class SafeLsaLogonProcessHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			private SafeLsaLogonProcessHandle() : base(true) { }

			// 0 is an Invalid Handle
			internal SafeLsaLogonProcessHandle(IntPtr handle) : base(true)
			{
				SetHandle(handle);
			}

			internal static SafeLsaLogonProcessHandle InvalidHandle
			{
				get { return new SafeLsaLogonProcessHandle(IntPtr.Zero); }
			}

			[System.Security.SecurityCritical]
			override protected bool ReleaseHandle()
			{
				// LsaDeregisterLogonProcess returns an NTSTATUS
				return LsaDeregisterLogonProcess(handle) >= 0;
			}
		}

		[DllImport("secur32.dll", SetLastError = true)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[ResourceExposure(ResourceScope.None)]
		internal static extern int LsaDeregisterLogonProcess(IntPtr handle);


		public static void CreateNewSession()
		{
			var kli = new SECUR32.KERB_INTERACTIVE_LOGON()
			{
				MessageType = SECUR32.KERB_LOGON_SUBMIT_TYPE.KerbInteractiveLogon,
				UserName = "",
				Password = ""
			};
			IntPtr pluid;
			IntPtr lsaHan;
			uint authPackID;
			IntPtr kerbLogInfo;
			SECUR32.LSA_STRING logonProc = new SECUR32.LSA_STRING()
			{
				Buffer = Marshal.StringToHGlobalAuto("InstaLogon"),
				Length = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("InstaLogon")),
				MaximumLength = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("InstaLogon"))
			};
			SECUR32.LSA_STRING originName = new SECUR32.LSA_STRING()
			{
				Buffer = Marshal.StringToHGlobalAuto("InstaLogon"),
				Length = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("InstaLogon")),
				MaximumLength = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("InstaLogon"))
			};
			SECUR32.LSA_STRING authPackage = new SECUR32.LSA_STRING()
			{
				Buffer = Marshal.StringToHGlobalAuto("MICROSOFT_KERBEROS_NAME_A"),
				Length = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("MICROSOFT_KERBEROS_NAME_A")),
				MaximumLength = (ushort)Marshal.SizeOf(Marshal.StringToHGlobalAuto("MICROSOFT_KERBEROS_NAME_A"))
			};
			IntPtr hLogonProc = Marshal.AllocHGlobal(Marshal.SizeOf(logonProc));
			Marshal.StructureToPtr(logonProc, hLogonProc, false);
			AdvApi32.AllocateLocallyUniqueId(out pluid);
			LsaConnectUntrusted(out lsaHan);
			//SECUR32.LsaRegisterLogonProcess(hLogonProc, out lsaHan, out secMode);
			SECUR32.LsaLookupAuthenticationPackage(lsaHan, ref authPackage, out authPackID);

			kerbLogInfo = Marshal.AllocHGlobal(Marshal.SizeOf(kli));
			Marshal.StructureToPtr(kli, kerbLogInfo, false);

			var ts = new SECUR32.TOKEN_SOURCE("Insta");
			IntPtr profBuf;
			uint profBufLen;
			long logonID;
			IntPtr logonToken;
			SECUR32.QUOTA_LIMITS quotas;
			SECUR32.WinStatusCodes subStatus;
			SECUR32.LsaLogonUser(lsaHan, ref originName, SECUR32.SecurityLogonType.Interactive, authPackID, kerbLogInfo, (uint)Marshal.SizeOf(kerbLogInfo), IntPtr.Zero, ref ts, out profBuf, out profBufLen, out logonID, out logonToken, out quotas, out subStatus);
		}
	}
}
