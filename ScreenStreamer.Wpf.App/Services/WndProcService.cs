using ScreenStreamer.Wpf.App.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.App.Services
{
    public class WndProcService
    {
        private const string SHOW_POLYWALL_STREAMER_MAIN_WINDOW = "SHOW_POLYWALL_STREAMER_MAIN_WINDOW";

        public static void ShowAnotherInstance()
        {
            int message = NativeMethods.RegisterWindowMessage(SHOW_POLYWALL_STREAMER_MAIN_WINDOW);

            if (message > 0)
            {
                bool success = NativeMethods.SendNotifyMessage((IntPtr)NativeMethods.HWND_BROADCAST, message, 0, 0);
                if (!success)
                {
                    var error = Marshal.GetLastWin32Error();
                    Debug.WriteLine("SendNotifyMessage(...) " + error);
                }
            }
        }

        private NotifyWindow nativeWindow = null;
        private int ShowMainWindowMessage = 0;

        public event Action ShowMainWindow;


        public void Init()
        {
            if (nativeWindow == null)
            {
                nativeWindow = new NotifyWindow(this);
                nativeWindow.CreateWindow();
            }

            if(ShowMainWindowMessage == 0)
            {
                ShowMainWindowMessage = NativeMethods.RegisterWindowMessage(SHOW_POLYWALL_STREAMER_MAIN_WINDOW);
            }

        }

        private bool ProcessMessage(System.Windows.Forms.Message m)
        {

            if (m.Msg == ShowMainWindowMessage)
            {
                //...
                ShowMainWindow?.Invoke();
            }

            return true;
        }

        public void Close()
        {
            if (nativeWindow != null)
            {
                nativeWindow.DestroyWindow();

                nativeWindow = null;
            }
        }


        class NotifyWindow : System.Windows.Forms.NativeWindow
        {

            private readonly WndProcService service = null;
            public NotifyWindow(WndProcService s)
            {
                this.service = s;
            }

            public bool CreateWindow()
            {
                IntPtr HWndMessage = new IntPtr(-3);

                if (Handle == IntPtr.Zero)
                {
                    CreateHandle(new System.Windows.Forms.CreateParams
                    {
                        ////Style = 0,
                        ////ExStyle = 0,
                        ////ClassStyle = 0,
                        //Caption = "NotifyWindow",

                        ////Parent = HWndMessage,
                    });
                }
                return Handle != IntPtr.Zero;
            }

            protected override void WndProc(ref System.Windows.Forms.Message m)
            {

                base.WndProc(ref m);

                service?.ProcessMessage(m);

            }

            public void DestroyWindow()
            {
                DestroyWindow(true, IntPtr.Zero);
            }

            private bool GetInvokeRequired(IntPtr hWnd)
            {
                if (hWnd == IntPtr.Zero) return false;
                int pid;
                var hwndThread = NativeMethods.GetWindowThreadProcessId(new HandleRef(this, hWnd), out pid);
                var currentThread = NativeMethods.GetCurrentThreadId();

                return (hwndThread != currentThread);
            }

            private void DestroyWindow(bool destroyHwnd, IntPtr hWnd)
            {
                if (hWnd == IntPtr.Zero)
                {
                    hWnd = Handle;
                }

                if (GetInvokeRequired(hWnd))
                {
                    NativeMethods.PostMessage(hWnd, NativeMethods.WM_CLOSE, 0, 0);
                    return;
                }

                lock (this)
                {
                    if (destroyHwnd)
                    {
                        base.DestroyHandle();
                    }
                }
            }

            public override void DestroyHandle()
            {
                DestroyWindow(false, IntPtr.Zero);
                base.DestroyHandle();
            }


        }


    }

}
