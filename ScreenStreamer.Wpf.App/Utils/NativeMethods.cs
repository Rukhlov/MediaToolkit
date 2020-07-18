
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.Utils
{
    public static class NativeMethods
    {
        public const uint HWND_BROADCAST = 0xFFFF;

        public const int WM_CLOSE = 0x0010;

        [DllImport("user32.Dll")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll", EntryPoint = "SendNotifyMessage", SetLastError = true)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);


        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int GetCurrentThreadId();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);




        private const int WM_SYSCOMMAND = 0x112;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        public static void ResizeWindow(IntPtr hWnd, ResizeDirection direction)
        {
            SendMessage(hWnd, WM_SYSCOMMAND, (IntPtr)direction, IntPtr.Zero);
        }


        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

    }

    public enum ResizeDirection
    {
        Left = 61441,
        Right = 61442,
        Top = 61443,
        TopLeft = 61444,
        TopRight = 61445,
        Bottom = 61446,
        BottomLeft = 61447,
        BottomRight = 61448,
    }


}
