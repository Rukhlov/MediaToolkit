using MediaToolkit.NativeAPIs;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.Utils
{

    static class WinConsole
    {
        private static bool consoleAllocated = false;
        private static SafeFileHandle conOutSafeHandle = null;
        private static SafeFileHandle conInSafeHandle = null;

        public static void AllocConsole()
        {
            var hWnd = Kernel32.GetConsoleWindow();
            if (hWnd == IntPtr.Zero)
            {
                bool result = Kernel32.AllocConsole();
                if (result)
                {
                    // var hStdOut = Kernel32.GetStdHandle(Kernel32.STD_OUTPUT_HANDLE);
                    //var fileHandle = new SafeFileHandle(hStdOut, true);
                    conOutSafeHandle = Kernel32.CreateConOutSafeHandle();
                    if (!conOutSafeHandle.IsInvalid)
                    {
                        var fs = new FileStream(conOutSafeHandle, FileAccess.Write);
                        if (fs != null)
                        {
                            var writer = new StreamWriter(fs, Encoding.Default) { AutoFlush = true };
                            Console.OutputEncoding = writer.Encoding;
                            //var writer = new StreamWriter(fs, Console.OutputEncoding) { AutoFlush = true };
                            Console.SetOut(writer);
                            Console.SetError(writer);
                        }
                    }

                    //conInSafeHandle = Kernel32.CreateConInSafeHandle();
                    //if (!conInSafeHandle.IsInvalid)
                    //{
                    //    var fs = new FileStream(conInSafeHandle, FileAccess.Read);
                    //    if (fs != null)
                    //    {
                    //        var reader = new StreamReader(fs, Encoding.Default);
                    //        Console.InputEncoding = reader.CurrentEncoding;
                    //        Console.SetIn(reader);
                    //    }
                    //}


                    consoleAllocated = true;
                }
            }
        }


        public static void FreeConsole()
        {
            if (consoleAllocated)
            {
                Kernel32.FreeConsole();
            }

            if (conOutSafeHandle != null)
            {
                conOutSafeHandle.Dispose();
                conOutSafeHandle = null;
            }

            if (conInSafeHandle != null)
            {
                conInSafeHandle.Dispose();
                conInSafeHandle = null;
            }
        }

    }
}
