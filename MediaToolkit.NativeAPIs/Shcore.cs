using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
    public class Shcore
    {

        [DllImport("shcore.dll")]
        internal static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);


        public static void SetDpiAwareness()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDpiAwareness(ProcessDPIAwareness.PROCESS_PER_MONITOR_DPI_AWARE);
                }
            }
            catch (DllNotFoundException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (EntryPointNotFoundException ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
}
