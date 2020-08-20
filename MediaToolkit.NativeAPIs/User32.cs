using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace MediaToolkit.NativeAPIs
{

    public static class User32
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);


		[DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DrawIconEx(IntPtr hdc, int xLeft, int yTop, IntPtr hIcon, int cxWidth, int cyHeight, int istepIfAniCur, IntPtr hbrFlickerFreeDraw, int diFlags);


        public const int CURSOR_SHOWING = 0x00000001;
        public const int CURSOR_SUPPRESSED = 0x00000002;

        public const int DI_NORMAL = 0x0003;

        [DllImport("user32.dll")]
        internal static extern bool GetCursorInfo(out CURSORINFO pci);


        public static void DrawCursorEx(IntPtr hDc, int screenX, int screenY)
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

            if (GetCursorInfo(out pci))
            {
                if (pci.flags == CURSOR_SHOWING)
                {
                    var hIcon = CopyIcon(pci.hCursor);
                    try
                    {
                        ICONINFO pIconInfo = default(ICONINFO);
                        try
                        {
                            bool result = GetIconInfo(hIcon, out pIconInfo);
                            if (!result)
                            {
                                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                            }

                            var pos = pci.ptScreenPos;
                            int x = pos.x - pIconInfo.xHotspot - screenX;
                            int y = pos.y - pIconInfo.yHotspot - screenY;
                            result = DrawIconEx(hDc, x, y, hIcon, 0, 0, 0, IntPtr.Zero, DI_NORMAL);

                            if (!result)
                            {
                                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

                            }
                        }
                        finally
                        {
                            Gdi32.DeleteObject(pIconInfo.hbmColor);
                            Gdi32.DeleteObject(pIconInfo.hbmMask);
                        }
                    }
                    finally
                    {

                        DestroyIcon(hIcon);
                    }
                }
            }
        }

        public static void DrawCursor(IntPtr hDc)
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

            if (GetCursorInfo(out pci))
            {
                if (pci.flags == CURSOR_SHOWING)
                {
                    int offsetX = 0;//12;
                    int offsetY = 0;//12;

                    var pos = pci.ptScreenPos;
                    int x = pos.x - offsetX;
                    int y = pos.y - offsetY;
                    DrawIcon(hDc, x, y, pci.hCursor);
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

		[DllImport("User32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

		public const uint MONITOR_DEFAULTTONULL = 0x00000000;
        public const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        public const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        public static IntPtr GetMonitorFromRect(Rectangle screen)
        {
            RECT rect = new RECT
            {
                Left = screen.Left,
                Top = screen.Top,
                Right = screen.Right,
                Bottom = screen.Bottom,
            };

            return MonitorFromRect(ref rect, MONITOR_DEFAULTTOPRIMARY);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, DeviceContextValues flags);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
		ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

		// Define the callback delegate's type.
		public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        public static Rectangle GetClientRect(IntPtr hwnd)
        {

            var success = GetClientRect(hwnd, out var rect);

			if (!success)
			{
				var error = Marshal.GetLastWin32Error();
				throw new Win32Exception(error);
			}

			return rect.AsRectangle;
        }

		public static Rectangle GetWindowRect(IntPtr hwnd)
        {
			var success = GetWindowRect(hwnd, out var rect);

			if (!success)
			{
				var error = Marshal.GetLastWin32Error();
				throw new Win32Exception(error);
			}

			return rect.AsRectangle;
        }

        public static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = User32.GetWindowRect(hWnd);
            Rectangle clientRect = User32.GetClientRect(hWnd);

            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr notificationFilter, int flags);

        [DllImport("user32.dll")]
        public static extern bool UnregisterDeviceNotification(IntPtr handle);


        [DllImport("user32.dll")]
        public static extern int GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS flags, 
            out uint numPathArrayElements, out uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        public static extern int QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS flags,
            ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray,
            ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray,
            IntPtr currentTopologyId);


        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(IntPtr displayConfig);

        public static int DisplayConfigGetDeviceInfo<T>(ref T displayConfig) where T : DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            int result = 0;
            int size = Marshal.SizeOf(displayConfig);

            var ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(displayConfig, ptr, false);

                result = DisplayConfigGetDeviceInfo(ptr);

                displayConfig = (T)Marshal.PtrToStructure(ptr, displayConfig.GetType());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return result;
        }


		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SwitchDesktop(IntPtr hDesktop);

		public delegate bool EnumDesktopsDelegate(string desktop, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumDesktopsA(IntPtr hwinsta, EnumDesktopsDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, ACCESS_MASK dwDesiredAccess);

		public delegate bool EnumWindowStationsDelegate(string windowsStation, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumWindowStations(EnumWindowStationsDelegate lpEnumFunc, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr GetShellWindow();

		public sealed class SafeWindowStationHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			public SafeWindowStationHandle()
				: base(true)
			{
			}

			protected override bool ReleaseHandle()
			{
				return CloseWindowStation(handle);

			}
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool CloseWindowStation(IntPtr hWinsta);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		[DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeWindowStationHandle OpenWindowStation([MarshalAs(UnmanagedType.LPTStr)] string lpszWinSta, [MarshalAs(UnmanagedType.Bool)] bool fInherit, ACCESS_MASK dwDesiredAccess);

		[DllImport("user32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr OpenWindowStationW([MarshalAs(UnmanagedType.LPTStr)] string lpszWinSta, [MarshalAs(UnmanagedType.Bool)] bool fInherit, ACCESS_MASK dwDesiredAccess);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetProcessWindowStation(IntPtr hWinSta);


		public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern IntPtr GetProcessWindowStation();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetThreadDesktop(IntPtr hDesktop);

		[DllImport("user32.dll")]
		public static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, ACCESS_MASK dwDesiredAccess);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool CloseDesktop(IntPtr hDesktop);

		public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32.dll")]
		public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam);



		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetUserObjectInformationW(IntPtr hObj, int nIndex, [Out] byte[] pvInfo, uint nLength, out uint lpnLengthNeeded);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetUserObjectInformationW(IntPtr hObj, int nIndex, [Out] IntPtr pvInfo, uint nLength, out uint lpnLengthNeeded);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		[ResourceExposure(ResourceScope.Process)]
		public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
		private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		// This static method is required because Win32 does not support
		// GetWindowLongPtr directly
		public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			if (IntPtr.Size == 8)
			{
				return GetWindowLongPtr64(hWnd, nIndex);
			}
			else
			{
				return GetWindowLongPtr32(hWnd, nIndex);
			}
		}


		public delegate void WinEventDelegate(IntPtr hWinEventHook, SWEH_Events eventType, IntPtr hwnd, SWEH_ObjectId idObject, long idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWinEventHook(SWEH_Events eventMin, SWEH_Events eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, SWEH_dwFlags dwFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

	}

}
