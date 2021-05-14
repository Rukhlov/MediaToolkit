using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Probe
{
    class EnumWindowsTest
    {
        class ProcWindow
        {
            public string title = "";
            public IntPtr hWnd = IntPtr.Zero;
            public int pid = 0;

        }

        public static void Run()
        {
            var targetTitle = "яндекс";
            Process proc = Process.Start(@"C:\Program Files\Mozilla Firefox\firefox.exe", "yandex.ru");

            List<ProcWindow> procWindows = new List<ProcWindow>();



            User32.EnumDelegate filterCb = new User32.EnumDelegate((hWnd, pParam) =>
            {
                string windowTitle = "";
                int windowTextLength = User32.GetWindowTextLength(hWnd);
                if (windowTextLength > 0)
                {
                    windowTextLength++;
                    StringBuilder buf = new StringBuilder(windowTextLength);
                    windowTextLength = User32.GetWindowText(hWnd, buf, buf.Capacity);
                    windowTitle = buf.ToString();
                }

                string className = "";
                {
                    StringBuilder buf = new StringBuilder(256);
                    var len = User32.GetClassName(hWnd, buf, buf.Capacity);
                    if (len > 0)
                    {
                        className = buf.ToString();
                    }

                }

                var procName = "";
                var procId = 0;
                var threadId = User32.GetWindowThreadProcessId(new HandleRef(null, hWnd), out procId);
                if (threadId > 0 && procId > 0)
                {
                    using (Process p = Process.GetProcessById(procId))
                    {
                        procName = p.ProcessName;
                    }
                }
                var isVisible = User32.IsWindowVisible(hWnd);

                if (className == "MozillaWindowClass" && isVisible)
                {
                    procWindows.Add(new ProcWindow
                    {
                        pid = procId,
                        title = windowTitle,
                        hWnd = hWnd,
                    });

                    Console.WriteLine(string.Join("; ", windowTitle, className, procId, procName, isVisible));
                }
               

                return true;
            });



            ProcWindow _p = null;
            while(_p == null)
            {
                var success = User32.EnumDesktopWindows(IntPtr.Zero, filterCb, IntPtr.Zero);
                if (!success)
                {
                    var error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(error);
                }

                _p = procWindows.FirstOrDefault(w => w.title.ToLower().Contains(targetTitle));
                Thread.Sleep(100);

            }


            Console.WriteLine(_p.title);
            User32.SetWindowPos(_p.hWnd, new IntPtr(-1), 100, 100, 100, 100, 0x0040);
            const int WM_CLOSE = 0x10;

            User32.SendMessage(_p.hWnd, WM_CLOSE, 0, 0);

            //var windows = DesktopManager.GetWindows();

            //foreach(var w in windows)
            //{
            //    Console.WriteLine(w);
            //}
        }
    }
}
