using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    [System.Security.SuppressUnmanagedCodeSecurity()]
    public sealed class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "CopyMemory")]
        public static extern void CopyMemory(IntPtr destination, IntPtr source, uint length);

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        public static extern IntPtr GetDC(IntPtr ptr);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

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
            Rectangle windowRect = NativeMethods.GetWindowRect(hWnd);
            Rectangle clientRect = NativeMethods.GetClientRect(hWnd);

            int chromeWidth = (int)((windowRect.Width - clientRect.Width) / 2);

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
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


        public static Bitmap SnapShot(IntPtr hWnd, int width, int height)
        {
            Bitmap bmp;
            IntPtr dc1;
            IntPtr dc2;
            Graphics g;

            bmp = new Bitmap(width, height);
            g = System.Drawing.Graphics.FromImage(bmp);

            dc1 = g.GetHdc();
            dc2 = GetDC(IntPtr.Zero);

            // dc2 = GetWindowDC(hWnd);

            BitBlt(dc1, 0, 0, width, height, dc2, 0, 0, (TernaryRasterOperations)13369376);

            g.ReleaseHdc(dc1);
            //g.ReleaseHdc(dc2);

            g.Dispose();

            return bmp;
            //bmp.Save(@"c:\snapshot.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

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
}
