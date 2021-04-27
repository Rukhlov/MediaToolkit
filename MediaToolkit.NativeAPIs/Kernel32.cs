using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace MediaToolkit.NativeAPIs
{

    public static class Kernel32
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", ExactSpelling = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", ExactSpelling = true)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "CopyMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        [DllImport("kernel32.dll", CharSet = CharSet.None, EntryPoint = "RtlZeroMemory", ExactSpelling = false)]
        public static extern void ZeroMemory(IntPtr ptr, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true,  CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr CreateFileW(
              string lpFileName,
              uint dwDesiredAccess,
              uint dwShareMode,
              IntPtr lpSecurityAttributes,
              uint dwCreationDisposition,
              uint dwFlagsAndAttributes,
              IntPtr hTemplateFile
            );
       
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 0x00000003;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;

		public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateConOutSafeHandle()
        {
            var hFile = CreateFileW("CONOUT$", GENERIC_WRITE , FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            return new Microsoft.Win32.SafeHandles.SafeFileHandle(hFile, true);
        }

        public static Microsoft.Win32.SafeHandles.SafeFileHandle CreateConInSafeHandle()
        {
			var hFile = CreateFileW("CONIN$", GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
			//var hFile = CreateFileW("CONIN$", GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
			return new Microsoft.Win32.SafeHandles.SafeFileHandle(hFile, true);
        }

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern void SetStdHandle(int nStdHandle, IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hSnapshot);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr GetCommandLine();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CreateProcess(
			string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
			ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, int dwCreationFlags,
			IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo,
			out PROCESS_INFORMATION lpProcessInformation);

		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll")]
		public static extern bool ProcessIdToSessionId(uint dwProcessId, ref uint pSessionId);

		[DllImport("kernel32.dll")]
		public static extern uint WTSGetActiveConsoleSessionId();


		[DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[ResourceExposure(ResourceScope.Process)]
		public static extern int GetCurrentThreadId();

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public class MEMORYSTATUSEX
		{
			public uint dwLength;
			public uint dwMemoryLoad;
			public ulong ullTotalPhys;
			public ulong ullAvailPhys;
			public ulong ullTotalPageFile;
			public ulong ullAvailPageFile;
			public ulong ullTotalVirtual;
			public ulong ullAvailVirtual;
			public ulong ullAvailExtendedVirtual;

			public MEMORYSTATUSEX()
			{
				this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);


		[DllImport("kernel32.dll")]
		public static extern IntPtr LocalFree(IntPtr hMem);


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);
	}

}
