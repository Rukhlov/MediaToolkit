
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.App.Utils
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
    }



}
