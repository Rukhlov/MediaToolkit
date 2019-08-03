using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer.Utils
{


    public enum WinErrors : long
    {
        ERROR_SUCCESS = 0,

        ERROR_INVALID_FUNCTION = 1,
        ERROR_FILE_NOT_FOUND = 2,
        ERROR_PATH_NOT_FOUND = 3,
        ERROR_TOO_MANY_OPEN_FILES = 4,
        ERROR_ACCESS_DENIED = 5,
        ERROR_INVALID_HANDLE = 6,
        ERROR_ARENA_TRASHED = 7,
        ERROR_NOT_ENOUGH_MEMORY = 8,
        ERROR_INVALID_BLOCK = 9,
        ERROR_BAD_ENVIRONMENT = 10,
        ERROR_BAD_FORMAT = 11,
        ERROR_INVALID_ACCESS = 12,
        ERROR_INVALID_DATA = 13,
        ERROR_OUTOFMEMORY = 14,
        ERROR_INSUFFICIENT_BUFFER = 122,

        ERROR_MORE_DATA = 234,
        ERROR_NO_MORE_ITEMS = 259,
        ERROR_SERVICE_SPECIFIC_ERROR = 1066,
        ERROR_INVALID_USER_BUFFER = 1784
        

        //...
    }



    public enum TernaryRasterOperations : uint
    {
        SRCCOPY = 0x00CC0020,
        SRCPAINT = 0x00EE0086,
        SRCAND = 0x008800C6,
        SRCINVERT = 0x00660046,
        SRCERASE = 0x00440328,
        NOTSRCCOPY = 0x00330008,
        NOTSRCERASE = 0x001100A6,
        MERGECOPY = 0x00C000CA,
        MERGEPAINT = 0x00BB0226,
        PATCOPY = 0x00F00021,
        PATPAINT = 0x00FB0A09,
        PATINVERT = 0x005A0049,
        DSTINVERT = 0x00550009,
        BLACKNESS = 0x00000042,
        WHITENESS = 0x00FF0062,
        CAPTUREBLT = 0x40000000 //only if WinVer >= 5.0.0 (see wingdi.h)
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }


    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;

        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
        public RGBQUAD[] bmiColors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RGBQUAD
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbReserved;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct CURSORINFO
    {
        public int cbSize;
        public int flags;
        public IntPtr hCursor;
        public POINT ptScreenPos;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public Rectangle AsRectangle
        {
            get
            {
                return new Rectangle(this.Left, this.Top, this.Right - this.Left, this.Bottom - this.Top);
            }
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public static RECT FromRectangle(Rectangle rect)
        {
            return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public struct RECT
    //{
    //    public int Left;
    //    public int Top;
    //    public int Right;
    //    public int Bottom;
    //}



    [Flags]
    public enum StretchingMode
    {
        /// <summary>
        /// Performs a Boolean AND operation using the color values for the eliminated 
        /// and existing pixels. If the bitmap is a monochrome bitmap, this mode preserves
        /// black pixels at the expense of white pixels.
        /// </summary>
        BLACKONWHITE = 1,
        /// <summary>
        /// Deletes the pixels. This mode deletes all eliminated lines of pixels 
        /// without trying to preserve their information
        /// </summary>
        COLORONCOLOR = 3,
        /// <summary>
        /// Maps pixels from the source rectangle into blocks of pixels in the destination rectangle.
        /// The average color over the destination block of pixels approximates the color of the source pixels. 
        /// This option is not supported on Windows 95/98/Me
        /// </summary>
        HALFTONE = 4,
        /// <summary>
        /// Performs a Boolean AND operation using the color values for the eliminated 
        /// and existing pixels. If the bitmap is a monochrome bitmap, this mode preserves
        /// black pixels at the expense of white pixels (same as BLACKONWHITE)
        /// </summary>
        STRETCH_ANDSCANS = 1,
        /// <summary>
        /// Deletes the pixels. This mode deletes all eliminated lines of pixels 
        /// without trying to preserve their information (same as COLORONCOLOR)
        /// </summary>
        STRETCH_DELETESCANS = 3,
        /// <summary>
        /// Maps pixels from the source rectangle into blocks of pixels in the destination rectangle.
        /// The average color over the destination block of pixels approximates the color of the source pixels. 
        /// This option is not supported on Windows 95/98/Me (same as HALFTONE)
        /// </summary>
        STRETCH_HALFTONE = 4,
        /// <summary>
        /// Performs a Boolean OR operation using the color values for the eliminated and existing pixels.
        /// If the bitmap is a monochrome bitmap, this mode preserves white pixels at the expense of 
        /// black pixels(same as WHITEONBLACK)
        /// </summary>
        STRETCH_ORSCANS = 2,
        /// <summary>
        /// Performs a Boolean OR operation using the color values for the eliminated and existing pixels.
        /// If the bitmap is a monochrome bitmap, this mode preserves white pixels at the expense of black pixels.
        /// </summary>
        WHITEONBLACK = 2,
        /// <summary>
        /// Fail to stretch
        /// </summary>
        ERROR = 0
    }

    public class WinMM
    {
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint timeBeginPeriod(uint uMilliseconds);

        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint timeEndPeriod(uint uMilliseconds);
    }

    public sealed class Gdi32
    {
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
                                        IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
                                        TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, StretchingMode iStretchMode);


        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);


    }

    public sealed class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "CopyMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        [DllImport("kernel32.dll", CharSet = CharSet.None, EntryPoint = "RtlZeroMemory", ExactSpelling = false)]
        public static extern void ZeroMemory(IntPtr ptr, int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetSystemTimes(out System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);
    }


    [System.Security.SuppressUnmanagedCodeSecurity()]
    public sealed class User32
    {

        public const int CURSOR_SHOWING = 0x00000001;
        public const int CURSOR_SUPPRESSED = 0x00000002;

        [DllImport("user32.dll")]
        public static extern bool GetCursorInfo(out CURSORINFO pci);

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
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);



        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);


        public static Rectangle GetClientRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        public static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            Rectangle windowRect = User32.GetWindowRect(hWnd);
            Rectangle clientRect = User32.GetClientRect(hWnd);

            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }

    }

    [Flags]
    public enum CompositionAction : uint
    {
        DWM_EC_DISABLECOMPOSITION = 0,
        DWM_EC_ENABLECOMPOSITION = 1
    }

    class DwmApi
    {
        public static void DisableAero(bool disable)
        {
            var compositionAction = disable ? CompositionAction.DWM_EC_DISABLECOMPOSITION : CompositionAction.DWM_EC_ENABLECOMPOSITION;

            DwmEnableComposition(compositionAction);

        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableComposition(CompositionAction uCompositionAction);

    }

    public enum ProcessDPIAwareness
    {
        PROCESS_DPI_UNAWARE = 0,
        PROCESS_SYSTEM_DPI_AWARE = 1,
        PROCESS_PER_MONITOR_DPI_AWARE = 2
    }

    class Shcore { 

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
            catch(DllNotFoundException ex)
            {
                Debug.WriteLine(ex);
            }
            catch (EntryPointNotFoundException ex)
            {
                Debug.WriteLine(ex);
            }
        }

    }
    public class RngProvider
    {
        private static System.Security.Cryptography.RNGCryptoServiceProvider provider =
            new System.Security.Cryptography.RNGCryptoServiceProvider();

        public static uint GetRandomNumber()
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            provider.GetNonZeroBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }


    public class MediaTimer
    {
        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;

        public static double GetRelativeTimeMilliseconds()
        {
            return (Ticks / (double)TicksPerMillisecond);
        }

        public static double GetRelativeTime()
        {
            return (Ticks / (double)TicksPerSecond);
        }

        public static long Ticks
        {
            get
            {
                return (long)(Stopwatch.GetTimestamp() * TicksPerSecond / (double)Stopwatch.Frequency);
                //return DateTime.Now.Ticks;
                //return NativeMethods.timeGetTime() * TicksPerMillisecond;
            }
        }

        public static DateTime GetDateTimeFromNtpTimestamp(ulong ntmTimestamp)
        {
            uint TimestampMSW = (uint)(ntmTimestamp >> 32);
            uint TimestampLSW = (uint)(ntmTimestamp & 0x00000000ffffffff);

            return GetDateTimeFromNtpTimestamp(TimestampMSW, TimestampLSW);
        }

        public static DateTime GetDateTimeFromNtpTimestamp(uint TimestampMSW, uint TimestampLSW)
        {
            /*
            Timestamp, MSW: 3670566484 (0xdac86654)
            Timestamp, LSW: 3876982392 (0xe7160e78)
            [MSW and LSW as NTP timestamp: Apr 25, 2016 09:48:04.902680000 UTC]
             * */

            DateTime ntpDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            uint ntpTimeMilliseconds = (uint)(Math.Round((double)TimestampLSW / (double)uint.MaxValue, 3) * 1000);
            return ntpDateTime
                .AddSeconds(TimestampMSW)
                .AddMilliseconds(ntpTimeMilliseconds);
        }

        private DateTime startDateTime;
        private long startTimestamp;

        private bool isRunning = false;

        public void Start(DateTime dateTime)
        {
            if (isRunning == false)
            {
                startDateTime = dateTime;
                startTimestamp = Stopwatch.GetTimestamp();

                isRunning = true;
            }
        }

        public DateTime Now
        {
            get
            {
                DateTime dateTime = DateTime.MinValue;
                if (isRunning)
                {
                    dateTime = startDateTime.AddTicks(ElapsedTicks);
                }

                return dateTime;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.Zero;
                if (isRunning)
                {
                    timeSpan = new TimeSpan(ElapsedTicks);
                }
                return timeSpan;
            }
        }

        public long ElapsedTicks
        {
            get
            {
                long ticks = 0;
                if (isRunning)
                {
                    ticks = (long)((Stopwatch.GetTimestamp() - startTimestamp) * TicksPerSecond / (double)Stopwatch.Frequency);

                    if (ticks < 0)
                    {
                        Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ticks " + ticks);
                    }
                }
                return ticks;
            }
        }

        public void Stop()
        {

            if (isRunning)
            {
                isRunning = false;
            }

        }
    }

    public abstract class StatCounter
    {
        public abstract string GetReport();
        public abstract void Reset();
    }

    public class Statistic
    {

        public readonly static List<StatCounter> Stats = new List<StatCounter>();

        public static object syncRoot = new object();
        public static void RegisterCounter(StatCounter counter)
        {
            lock (syncRoot)
            {
                Stats.Add(counter);
            }
        }

        public static void UnregisterCounter(StatCounter counter)
        {
            lock (syncRoot)
            {
                Stats.Remove(counter);
            }
        }

        public static string GetReport()
        {
            string report = "";
            lock (syncRoot)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var stat in Stats)
                {
                    sb.AppendLine(stat.GetReport());
                }

                report = sb.ToString();

            }

            return report;
        }

        private static PerfCounter perfCounter = new PerfCounter();
        public static PerfCounter PerfCounter
        {
            get
            {
                if (perfCounter == null)
                {
                    perfCounter = new PerfCounter();
                }
                return perfCounter;
            }
        }
    }




    public class PerfCounter : StatCounter, IDisposable
    {
        public PerfCounter()
        {
            _PerfCounter();
        }

        private void _PerfCounter()
        {
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Disposed += Timer_Disposed;
            timer.Start();

        }

        public short CPU { get; private set; }

        private System.Timers.Timer timer = new System.Timers.Timer();
        private CPUCounter cpuCounter = new CPUCounter();

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.CPU = cpuCounter.GetUsage();
        }

        private void Timer_Disposed(object sender, EventArgs e)
        {
            cpuCounter?.Dispose();
        }

        public override string GetReport()
        {
            string cpuUsage = "";
            if (CPU >= 0 && CPU <= 100)
            {
                cpuUsage = "CPU=" + CPU + "%";
            }
            else
            {
                cpuUsage = "CPU=--%";
            }
            return cpuUsage;
        }

        public override void Reset()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }

        class CPUCounter : IDisposable
        {

            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysKernel;
            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysUser;

            private TimeSpan prevProcTotal;

            private short CPUUsage;
            //DateTime LastRun;

            private long lastTimestamp;

            private long runCount;

            private Process currentProcess;

            public CPUCounter()
            {
                CPUUsage = -1;
                lastTimestamp = 0;

                prevSysUser.dwHighDateTime = prevSysUser.dwLowDateTime = 0;
                prevSysKernel.dwHighDateTime = prevSysKernel.dwLowDateTime = 0;
                prevProcTotal = TimeSpan.MinValue;
                runCount = 0;

                currentProcess = Process.GetCurrentProcess();
            }

            public short GetUsage()
            {
                if (disposed)
                {
                    return 0;
                }

                short CPUCopy = CPUUsage;
                if (Interlocked.Increment(ref runCount) == 1)
                {
                    if (!EnoughTimePassed)
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    System.Runtime.InteropServices.ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                    if (!Kernel32.GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    TimeSpan procTime = currentProcess.TotalProcessorTime;

                    if (prevProcTotal != TimeSpan.MinValue)
                    {
                        ulong sysKernelDiff = SubtractTimes(sysKernel, prevSysKernel);
                        ulong sysUserDiff = SubtractTimes(sysUser, prevSysUser);
                        ulong sysTotal = sysKernelDiff + sysUserDiff;

                        long procTotal = procTime.Ticks - prevProcTotal.Ticks;
                        // long procTotal = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                        if (sysTotal > 0)
                        {
                            CPUUsage = (short)((100.0 * procTotal) / sysTotal);
                        }
                    }
   
                    prevProcTotal = procTime;
                    prevSysKernel = sysKernel;
                    prevSysUser = sysUser;

                    lastTimestamp = Stopwatch.GetTimestamp();

                    CPUCopy = CPUUsage;
                }
                Interlocked.Decrement(ref runCount);

                return CPUCopy;

            }

            private ulong SubtractTimes(System.Runtime.InteropServices.ComTypes.FILETIME a, System.Runtime.InteropServices.ComTypes.FILETIME b)
            {
                ulong aInt = ((ulong)(a.dwHighDateTime << 32)) | (ulong)a.dwLowDateTime;
                ulong bInt = ((ulong)(b.dwHighDateTime << 32)) | (ulong)b.dwLowDateTime;

                return aInt - bInt;
            }

            private bool EnoughTimePassed
            {
                get
                {
                    const int minimumElapsedMS = 250;

                    long ticks = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                    TimeSpan sinceLast = new TimeSpan(ticks);

                    return sinceLast.TotalMilliseconds > minimumElapsedMS;
                }
            }

            private volatile bool disposed = false;
            public void Dispose()
            {
                disposed = true;

                if (currentProcess != null)
                {
                    currentProcess.Dispose();
                    currentProcess = null;
                }
            }
        }

    }

    public class StringHelper
    {
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize,  SizeSuffixes[mag]);
        }
    }

    public static class TaskEx
    {
        public static Task<Task[]> WhenAllOrFirstException(params Task[] tasks)
        {
            var countdownEvent = new CountdownEvent(tasks.Length);
            var completer = new TaskCompletionSource<Task[]>();

            Action<Task> onCompletion = completed =>
            {
                if (completed.IsFaulted && completed.Exception != null)
                {
                    completer.TrySetException(completed.Exception.InnerExceptions);
                }

                if (countdownEvent.Signal() && !completer.Task.IsCompleted)
                {
                    completer.TrySetResult(tasks);
                }
            };

            foreach (var task in tasks)
            {
                task.ContinueWith(onCompletion);
            }

            return completer.Task;
        }
    }
}
