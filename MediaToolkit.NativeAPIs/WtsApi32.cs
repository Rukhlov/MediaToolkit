using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
	public static class WtsApi32
	{
		public const int NOTIFY_FOR_THIS_SESSION = 0;
		public const int NOTIFY_FOR_ALL_SESSIONS = 1;

		public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

		public enum WTS_CONNECTSTATE_CLASS
		{
			WTSActive,
			WTSConnected,
			WTSConnectQuery,
			WTSShadow,
			WTSDisconnected,
			WTSIdle,
			WTSListen,
			WTSReset,
			WTSDown,
			WTSInit
		}

		public enum WTS_INFO_CLASS
		{
			WTSInitialProgram,
			WTSApplicationName,
			WTSWorkingDirectory,
			WTSOEMId,
			WTSSessionId,
			WTSUserName,
			WTSWinStationName,
			WTSDomainName,
			WTSConnectState,
			WTSClientBuildNumber,
			WTSClientName,
			WTSClientDirectory,
			WTSClientProductId,
			WTSClientHardwareId,
			WTSClientAddress,
			WTSClientDisplay,
			WTSClientProtocolType,
			WTSIdleTime,
			WTSLogonTime,
			WTSIncomingBytes,
			WTSOutgoingBytes,
			WTSIncomingFrames,
			WTSOutgoingFrames,
			WTSClientInfo,
			WTSSessionInfo
		}



		[DllImport("wtsapi32.dll")]
		public static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

		[DllImport("wtsapi32.dll")]
		public static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);


		[DllImport("wtsapi32.dll", SetLastError = true)]
		public static extern int WTSEnumerateSessions(IntPtr hServer,int Reserved, int Version,
			ref System.IntPtr ppSessionInfo, ref int pCount);

		[DllImport("wtsapi32.dll", ExactSpelling = true, SetLastError = false)]
		public static extern void WTSFreeMemory(IntPtr memory);

		[DllImport("Wtsapi32.dll")]
		public static extern bool WTSQuerySessionInformation(IntPtr hServer, uint sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern IntPtr WTSOpenServer(string pServerName);

		[StructLayout(LayoutKind.Sequential)]
		public struct WTS_SESSION_INFO
		{
			public uint SessionID;

			[MarshalAs(UnmanagedType.LPStr)]
			public string pWinStationName;

			public WTS_CONNECTSTATE_CLASS State;
		}
		

		public static class WTS
		{
			public const int CONSOLE_CONNECT = 0x1;
			public const int CONSOLE_DISCONNECT = 0x2;

			public const int REMOTE_CONNECT = 0x3;
			public const int REMOTE_DISCONNECT = 0x4;

			public const int SESSION_LOCK = 0x7;
			public const int SESSION_UNLOCK = 0x8;
			//...
		}


	}


}
