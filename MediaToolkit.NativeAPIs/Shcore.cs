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


        public static bool SetProcessPerMonitorDpiAwareness()
        {
            bool Success = false;
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    Success = (SetProcessDpiAwareness(ProcessDPIAwareness.PROCESS_PER_MONITOR_DPI_AWARE) == 0);
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

            return Success;
        }

    }
}
