using CommonData;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{

    public class DatapathDesktopCapture : ScreenCapture
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public static bool Initialized { get; private set; }
        public static bool Load()
        {
            _logger.Debug("Load()");
            if (!Initialized)
            {
                try
                {
                    var result = DCapt.DCaptLoad(out hLoad);

                    if (result == DCapt.CaptError.DESKCAPT_ERROR_API_ALREADY_LOADED)
                    {
                        //...

                    }

                    DCapt.ThrowIfError(result, "DCaptLoad");

                    Initialized = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return Initialized;
        }

        public static void Unload()
        {
            _logger.Debug("Unload()");

            try
            {

                if (hLoad != IntPtr.Zero)
                {
                    DCapt.DCaptFree(hLoad);
                    hLoad = IntPtr.Zero;
                    Initialized = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static IntPtr hLoad = IntPtr.Zero;
        private IntPtr hCapt = IntPtr.Zero;

        private BITMAPINFO bmi = default(BITMAPINFO);
        private IntPtr pBuffer = IntPtr.Zero;


        public override void Init(Rectangle captArea, Size destSize)
        {
            logger.Debug("Init(...) " + captArea.ToString() + " " + destSize.ToString());

            if (!Initialized)
            {
                if (!Load())
                {
                    throw new Exception("DCapt not initialized");
                }
            }

            videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format16bppRgb565);
            var bmp = videoBuffer.bitmap;

            Init(captArea, bmp);

        }

        private void Init(Rectangle captArea, Bitmap bmp)
        {
            if (!(bmp.PixelFormat == PixelFormat.Format16bppRgb565 || bmp.PixelFormat == PixelFormat.Format16bppRgb565))
            {
                throw new FormatException("Unsuppoted pix format " + bmp.PixelFormat);
            }

            try
            {
                var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");

                logger.Debug("DCaptCreateCapture() " + result);

                int biWidth = bmp.Width;
                int biHeight = bmp.Height;
                // 
                int biBitCount = Image.GetPixelFormatSize(bmp.PixelFormat);
                uint biSizeImage = (uint)(biWidth * biHeight * biBitCount / 8);

                const int BI_BITFIELDS = 3;

                var bmiHeader = new BITMAPINFOHEADER
                {
                    biWidth = biWidth,
                    biHeight = -biHeight,
                    biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),

                    biBitCount = (ushort)biBitCount,
                    biPlanes = 1,

                    biClrUsed = 0,
                    biClrImportant = 0,
                    biSizeImage = biSizeImage,
                    biCompression = BI_BITFIELDS,

                };

                //var bmiColors = GetColourMask(bmp.PixelFormat);

                var bmiColors = new RGBQUAD[]
                {
                     new RGBQUAD
                     {
                         rgbRed = 0,
                         rgbBlue = 248,
                         rgbGreen = 0
                     }
                };

                BITMAPINFO bmi = new BITMAPINFO
                {
                    bmiHeader = bmiHeader,
                    //bmiColors = bmiColors,
                };


                var dstSize = bmp.Size;

                RECT srcRect = new RECT
                {
                    Left = captArea.Left,
                    Right = captArea.Right,
                    Bottom = captArea.Bottom,
                    Top = captArea.Top,
                };

                IntPtr _hBmi = IntPtr.Zero;
                try
                {
                    _hBmi = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BITMAPINFO)));
                    Marshal.StructureToPtr(bmi, _hBmi, false);

                    IntPtr hBmi = _hBmi;

                    // The bits per pixel of the saved data. Must be 2 
                    int bitsPerPixel = 2;//biBitCount / 8; 
                    result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref dstSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY, ref pBuffer, ref hBmi);
                    DCapt.ThrowIfError(result, "DCaptConfigureCapture");

                    this.bmi = (BITMAPINFO)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFO));
                    var _bmiHeader = bmi.bmiHeader;

                    logger.Debug("_bmiHeader " + _bmiHeader.biWidth + "x" + _bmiHeader.biHeight + " "
                        + _bmiHeader.biBitCount + " " + _bmiHeader.biCompression + " " + _bmiHeader.biSizeImage);

                }
                finally
                {
                    if (_hBmi != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(_hBmi);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }

        public override bool UpdateBuffer(int timeout = 10)
        {
            logger.Trace("Update()");

            bool success = false;

            if (!Initialized)
            {
                return false;
            }

            var bufSize = bmi.bmiHeader.biSizeImage;
            if (bufSize > 0)
            {
                Kernel32.ZeroMemory(pBuffer, (int)bufSize);

                var result = DCapt.DCaptUpdate(hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");

                var syncRoot = videoBuffer.syncRoot;

                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                    if (lockTaken)
                    {
                        var sharedBits = videoBuffer.bitmap;
                        var rect = new Rectangle(0, 0, sharedBits.Width, sharedBits.Height);
                        var data = sharedBits.LockBits(rect, ImageLockMode.ReadWrite, sharedBits.PixelFormat);
                        try
                        {
                            IntPtr scan0 = data.Scan0;

                            Kernel32.CopyMemory(scan0, this.pBuffer, (uint)bufSize);

                            success = true;

                        }
                        finally
                        {
                            sharedBits.UnlockBits(data);
                        }
                    }
                    else
                    {
                        logger.Warn("Drop bits...");
                    }

                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(syncRoot);
                    }
                }
            }

            return success;

            // Console.WriteLine("DCaptCreateCapture() " + result);
        }


        public override void Close()
        {

            logger.Debug("DatapathDesktopCapture::Close()");

            if (Initialized)
            {
                if (hCapt != IntPtr.Zero)
                {
                    var result = DCapt.DCaptFreeCapture(hCapt);
                    DCapt.ThrowIfError(result, "DCaptFreeCapture");
                    hCapt = IntPtr.Zero;
                }

                if (pBuffer != IntPtr.Zero)
                {// создается в DCaptConfigureCapture соответственно удаляется в DCaptFreeCapture() !!

                    // Marshal.FreeHGlobal(pBuffer);
                    pBuffer = IntPtr.Zero;
                }

                this.bmi = default(BITMAPINFO);

            }

            base.Close();

        }


        /// <summary>
        /// https://www.datapath.co.uk/supportdownloads/windows/imagedp4-sdk/Desktop-Capture-SDK.pdf
        /// https://www.datapath.co.uk/datapath-current-downloads/video-wall-downloads-1/sdks-10/74-deskcapt-v1-0-2
        /// </summary>
        class DCapt
        {
            public const string CaptDll = "capt.dll";

            private const uint DESKCAPT_ERROR_BASE = 0x011B0000;

            internal enum CaptError : uint
            {
                DESKCAPTERROR_NO_ERROR = 0,

                DESKCAPT_ERROR_UNKNOWN_ERROR = (DESKCAPT_ERROR_BASE + 0),
                DESKCAPT_ERROR_INVALID_HANDLE = (DESKCAPT_ERROR_BASE + 1),
                DESKCAPT_ERROR_BUFFER_TOO_SMALL = (DESKCAPT_ERROR_BASE + 2),
                DESKCAPT_ERROR_INVALID_RECT = (DESKCAPT_ERROR_BASE + 3),
                DESKCAPT_ERROR_INVALID_COLOUR = (DESKCAPT_ERROR_BASE + 4),
                DESKCAPT_ERROR_INVALID_FLAGS = (DESKCAPT_ERROR_BASE + 5),
                DESKCAPT_ERROR_API_ALREADY_LOADED = (DESKCAPT_ERROR_BASE + 6),
            }

            internal enum CaptFlags
            {
                CAPTURE_FLAG_OVERLAY = 1
            }

            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptLoad")]
            internal static extern CaptError DCaptLoad(out IntPtr hLoad);


            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptFree")]
            internal static extern CaptError DCaptFree(IntPtr hLoad);


            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptCreateCapture")]
            internal static extern CaptError DCaptCreateCapture(IntPtr hLoad, out IntPtr hCapt);

            /// <summary>
            /// This function configures a desktop capture rectangle for a copy into the required source buffer size. 
            /// *ppBuffer will not contain the captureuntil a successful call to DCaptUpdate has been made.
            /// </summary>
            /// <param name="hCapt">The Capture to configure</param>
            /// <param name="srcRect">The rectangle on the desktop to capture</param>
            /// <param name="dstSize">The size of the buffer to capture the desktop in</param>
            /// <param name="bitsPerPixel">The bits per pixel of the saved data. Must be 2</param>
            /// <param name="flags">Combination of CAPTURE_FLAG_*** flags</param>
            /// <param name="hBuf"> Address of pointer to to capture the desktop image</param>
            /// <param name="hInfo">Address of pointer to an RGBBITMAPINFO structure</param>
            /// <returns></returns>
            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptConfigureCapture")]
            internal static extern CaptError DCaptConfigureCapture(IntPtr hCapt, ref RECT srcRect, ref Size dstSize, int bitsPerPixel, CaptFlags flags, ref IntPtr hBuf, ref IntPtr hInfo);

            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptUpdate")]
            internal static extern CaptError DCaptUpdate(IntPtr hCapt);

            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptFreeCapture")]
            internal static extern CaptError DCaptFreeCapture(IntPtr hCapt);

            internal static void ThrowIfError(CaptError code, string message = "")
            {
                if (code != CaptError.DESKCAPTERROR_NO_ERROR)
                {
                    throw new Exception(message + " " + code);
                }
            }
        }

        // Этот код взят из WallControll и он не работает!!!
        private static RGBQUAD[] GetColourMask(PixelFormat format)
        {
            RGBQUAD[] array = new RGBQUAD[3];
            if (format == PixelFormat.Format16bppRgb555)
            {
                array[0].rgbBlue = 0;
                array[0].rgbGreen = 124;
                array[0].rgbRed = 0;
                array[1].rgbBlue = 224;
                array[1].rgbGreen = 3;
                array[1].rgbRed = 0;
                array[2].rgbBlue = 31;
                array[2].rgbGreen = 0;
                array[2].rgbRed = 0;
            }
            else if (format == PixelFormat.Format16bppRgb565)
            {
                array[0].rgbBlue = 0;
                array[0].rgbGreen = 248;
                array[0].rgbRed = 0;
                array[1].rgbBlue = 224;
                array[1].rgbGreen = 7;
                array[1].rgbRed = 0;
                array[2].rgbBlue = 31;
                array[2].rgbGreen = 0;
                array[2].rgbRed = 0;
            }
            else if (format == PixelFormat.Format32bppRgb)
            {
                array[0].rgbBlue = 0;
                array[0].rgbGreen = 0;
                array[0].rgbRed = byte.MaxValue;
                array[1].rgbBlue = 0;
                array[1].rgbGreen = byte.MaxValue;
                array[1].rgbRed = 0;
                array[2].rgbBlue = byte.MaxValue;
                array[2].rgbGreen = 0;
                array[2].rgbRed = 0;
            }
            else
            {
                throw new Exception("Invalid Pixel Format");
            }

            return array;
        }

    }
}
