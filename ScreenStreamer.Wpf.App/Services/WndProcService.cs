
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ScreenStreamer.Wpf.Utils;

namespace ScreenStreamer.Wpf.Services
{
    public class WndProcServiceCommand
    {
        public const string ShowMainWindow = "SHOW_POLYWALL_STREAMER_MAIN_WINDOW";
        //...
    }

    public class WndProcService
    {

        public static bool ShowAnotherInstance()
        {
			bool Result = false;
            int messageCode = NativeMethods.RegisterWindowMessage(WndProcServiceCommand.ShowMainWindow);

            if (messageCode != 0)
            {
               var hWnd = NativeMethods.FindWindow(null, NotifyWindow.NotifyWindowCaption);

                if (hWnd !=IntPtr.Zero)
                {
                    Result = NativeMethods.SendNotifyMessage(hWnd, messageCode, 0, 0);
                    if (!Result)
                    {
                        var error = Marshal.GetLastWin32Error();
                        Debug.WriteLine("SendNotifyMessage(...) " + error);
                    }
                }

            }

			return Result;
        }

        private NotifyWindow nativeWindow = null;
        private bool initialized = false;

        public event Action<string>DispatchMessage;

        private Dictionary<int, string> messageDict = new Dictionary<int, string>();

        public void Init()
        {
            if (nativeWindow == null)
            {
                nativeWindow = new NotifyWindow(this);
                nativeWindow.CreateWindow();
            }

            var messageCode = NativeMethods.RegisterWindowMessage(WndProcServiceCommand.ShowMainWindow);

            if (messageCode != 0)
            {
                if (!messageDict.ContainsKey(messageCode))
                {
                    messageDict.Add(messageCode, WndProcServiceCommand.ShowMainWindow);
                }
            }
            
            initialized = true;
        }

        private bool ProcessMessage(System.Windows.Forms.Message m)
        {
            var messagCode = m.Msg;

            if (messageDict.ContainsKey(messagCode))
            {
                var message = messageDict[messagCode];
                DispatchMessage?.Invoke(message);
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
            public const string NotifyWindowCaption = "PolywallStreamerNotifyWindow";

            private readonly WndProcService service = null;
            public NotifyWindow(WndProcService s)
            {
                this.service = s;
            }

            public bool CreateWindow()
            {// окно только для сообщений!
                IntPtr HWndMessage = new IntPtr(-3);

                if (Handle == IntPtr.Zero)
                {
                    CreateHandle(new System.Windows.Forms.CreateParams
                    {
                        Style = 0,
                        ExStyle = 0,
                        ClassStyle = 0,
                        Caption = NotifyWindow.NotifyWindowCaption,
                        Parent = HWndMessage,
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
