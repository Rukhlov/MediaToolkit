using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Encoder
{
    class DatapathCaptTest
    {
        public static void Run()
        {

            Console.WriteLine("DatapathCaptTest::Run()");
            try
            {
                DatapathDesktopCapture.Load();

                DatapathDesktopCapture capt = new DatapathDesktopCapture();

                var allScreenRect = SystemInformation.VirtualScreen;
                //var rect = new Rectangle(0, 0, 1920, 1080);
                var srcRect = allScreenRect;
                var destSize = allScreenRect.Size;
                //var destSize = new Size(1920, 1080);

                capt.Init(srcRect, destSize);
                capt.RunTest();

                capt.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                DatapathDesktopCapture.Unload();
            }



        }


    }

    public class DatapathDesktopCapture
    {
        public Rectangle SrcRect { get; protected set; }
        public Size DestSize { get; protected set; }

        public VideoBufferBase VideoBuffer { get; protected set; }

        public static bool Initialized { get; private set; }

        public static bool Load()
        {
            Console.WriteLine("Load()");
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
                catch (DllNotFoundException)
                {
                    Console.WriteLine("Datapath capture not found");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return Initialized;
        }

        public static void Unload()
        {
            Console.WriteLine("Unload()");

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
                Console.WriteLine(ex);
            }
        }

        private static IntPtr hLoad = IntPtr.Zero;
        private IntPtr hCapt = IntPtr.Zero;

        private BITMAPINFO bitmapInfo = default(BITMAPINFO);
        private IntPtr pBuffer = IntPtr.Zero;

        public void Init(Rectangle captArea, Size destSize)
        {
            Console.WriteLine("Init(...) " + captArea.ToString() + " " + destSize.ToString());

            if (!Initialized)
            {
                if (!Load())
                {
                    throw new Exception("DCapt not initialized");
                }
            }

            this.SrcRect = captArea;
            this.DestSize = destSize;

            var videoBuffer = new MemoryVideoBuffer(DestSize, PixFormat.RGB16, 16);
            this.VideoBuffer = videoBuffer;

            InitCapture(captArea, DestSize);

            InitDx();

        }

        private Device device = null;
        private void InitDx()
        {
            SharpDX.DXGI.Factory1 dxgiFactory = null;

            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();

                //logger.Info(DirectX.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

                SharpDX.DXGI.Adapter1 adapter = null;
                try
                {
                    var AdapterIndex = 0;
                    adapter = dxgiFactory.GetAdapter1(AdapterIndex);
                    //AdapterId = adapter.Description.Luid;
                    //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                    var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
                    //var deviceCreationFlags = DeviceCreationFlags.None;
#if DEBUG
                    //deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
                    //device = new Device(adapter, deviceCreationFlags);
                    device = new Device(SharpDX.Direct3D.DriverType.Hardware, deviceCreationFlags);
                    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
                }
                finally
                {
                    if (adapter != null)
                    {
                        adapter.Dispose();
                        adapter = null;
                    }
                }
            }
            finally
            {
                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }


            //var gdiTexture = new Texture2D(device,
            // new Texture2DDescription
            // {
            //     CpuAccessFlags = CpuAccessFlags.None,
            //     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            //     Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
            //     Width = width, //DestSize.Width,
            //     Height = height, //DestSize.Height,

            //     MipLevels = 1,
            //     ArraySize = 1,
            //     SampleDescription = { Count = 1, Quality = 0 },
            //     Usage = ResourceUsage.Default,
            //     OptionFlags = ResourceOptionFlags.GdiCompatible,

            // });

        }

        private void InitCapture(Rectangle captArea, Size resolution)
        {
            try
            {
                var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");

                //Console.WriteLine("DCaptCreateCapture() " + result);

                RECT srcRect = new RECT
                {
                    Left = captArea.Left,
                    Right = captArea.Right,
                    Bottom = captArea.Bottom,
                    Top = captArea.Top,
                };

                SIZE destSize = new SIZE
                {
                    cx = resolution.Width,
                    cy = resolution.Height,
                };

                // The bits per pixel of the saved data. Must be 2 
                int bitsPerPixel = 2;

                result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref destSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY,
                   out var pBuffer, out var hBmi);

                DCapt.ThrowIfError(result, "DCaptConfigureCapture");

                var bmiHeader = (BITMAPINFOHEADER)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFOHEADER));

                Console.WriteLine(bmiHeader.ToString());

                this.bitmapInfo.bmiHeader = bmiHeader;
                this.pBuffer = pBuffer;

                var dataSize = this.bitmapInfo.bmiHeader;
                var width = this.bitmapInfo.bmiHeader.biWidth;
                var height = this.bitmapInfo.bmiHeader.biHeight;
                if (height < 0)
                {
                    height = -height;
                }

                PixFormat pixFormat = PixFormat.Unknown;
                if (bmiHeader.biBitCount == 16)
                { //rgb 16 bit
                    if (bmiHeader.biCompression == (uint)BI.BITFIELDS)
                    {// может быть rgb555 или rgb565
                     // читаем маску цветов что бы определить формат
                        var hColor = IntPtr.Add(hBmi, (int)bmiHeader.biSize);
                        uint[] bmiColors = MediaToolkit.NativeAPIs.Utils.MarshalHelper.GetArrayData<uint>(hColor, 3);

                        var red = bmiColors[0];
                        var green = bmiColors[1];
                        var blue = bmiColors[2];
                        if (red == 0xF800 && green == 0x7E0 && blue == 0x1F)
                        {// rgb 565
                            pixFormat = PixFormat.RGB16;
                        }
                        else if (red == 0x7C00 && green == 0x3E0 && blue == 0x1F)
                        {// rgb 555
                            pixFormat = PixFormat.RGB15;
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid color mask: " + string.Join(",", red, green, blue));
                        }
                    }
                    else if (bmiHeader.biCompression == (uint)BI.RGB)
                    {// только rgb555
                        pixFormat = PixFormat.RGB15;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid compression field: " + bmiHeader.biCompression);
                    }
                }
                else
                {
                    throw new NotSupportedException("_bmiHeader.biBitCount == " + bmiHeader.biBitCount);
                }

                var videoBuffer = new MemoryVideoBuffer(new Size(width, height), pixFormat, 16);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Close();
                throw;
            }
        }

        internal void RunTest()
        {
            uint bufSize = this.bitmapInfo.bmiHeader.biSizeImage;

            var width = this.bitmapInfo.bmiHeader.biWidth;
            var height = this.bitmapInfo.bmiHeader.biHeight;
            if (height < 0)
            {
                height = -height;
            }


            Stopwatch sw = new Stopwatch();
            var num = 10;
            int count = num;
            sw.Restart();
            long totalMsec = 0;

            Task.Run(() =>
            {
                while (count-- > 0)
                {
                    Kernel32.ZeroMemory(pBuffer, (int)bufSize);
                    var result = DCapt.DCaptUpdate(hCapt);
                    DCapt.ThrowIfError(result, "DCaptCreateCapture");

                    var msec = sw.ElapsedMilliseconds;
                    totalMsec += msec;
                    Console.WriteLine(msec);
                    sw.Restart();
                    Thread.Sleep(1);
                }
            }).Wait();


            //var totalMsec = sw.ElapsedMilliseconds;
            var timePerFrame = totalMsec / num;
            Console.WriteLine("--------------------DCaptUpdate----------------------");
            Console.WriteLine("TotalTime: " + totalMsec);
            Console.WriteLine("TimePerFrame: " + timePerFrame);
            Console.WriteLine("FramePerSec: " + 1000.0 / timePerFrame);

            sw.Restart();
            count = num;
            while (count-- > 0)
            {

                Kernel32.ZeroMemory(pBuffer, (int)bufSize);

                var result = DCapt.DCaptUpdate(hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");


                //using (var surf = gdiTexture.QueryInterface<SharpDX.DXGI.Surface1>())
                //{
                //    var hdc = surf.GetDC(true);
                //    try
                //    {
                //        var res = Gdi32.SetDIBitsToDevice(hdc, 0, 0, width, height, 0, 0, 0, height, pBuffer, hBmi, DIB_PAL_COLORS);
                //    }
                //    finally
                //    {
                //        surf.ReleaseDC();
                //    }

                //}
            }

            totalMsec = sw.ElapsedMilliseconds;

            timePerFrame = totalMsec / num;
            Console.WriteLine("--------------------Dirctx----------------------");
            Console.WriteLine("TotalTime: " + totalMsec);
            Console.WriteLine("TimePerFrame: " + timePerFrame);
            Console.WriteLine("FramePerSec: " + 1000.0 / timePerFrame);


            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            ////Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format16bppRgb565);
            count = num;
            sw.Restart();
            while (count-- > 0)
            {
                Kernel32.ZeroMemory(pBuffer, (int)bufSize);

                var result = DCapt.DCaptUpdate(hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");

                //var g = Graphics.FromImage(bitmap);
                //var hdc = g.GetHdc();
                //var res = Gdi32.SetDIBitsToDevice(hdc, 0, 0, width, height, 0, 0, 0, height, pBuffer, hBmi, DIB_PAL_COLORS);


                //if (g != null)
                //{
                //    g.ReleaseHdc(hdc);
                //    g.Dispose();
                //}
            }


            totalMsec = sw.ElapsedMilliseconds;
            timePerFrame = totalMsec / num;
            Console.WriteLine("--------------------GDI----------------------");
            Console.WriteLine("TotalTime: " + totalMsec);
            Console.WriteLine("TimePerFrame: " + timePerFrame);
            Console.WriteLine("FramePerSec: " + 1000.0 / timePerFrame);

        }


        //private void Init(Rectangle captArea, Bitmap bmp)
        //{
        //    if (!(bmp.PixelFormat == PixelFormat.Format16bppRgb565 || bmp.PixelFormat == PixelFormat.Format16bppRgb565))
        //    {
        //        throw new FormatException("Unsuppoted pix format " + bmp.PixelFormat);
        //    }

        //    try
        //    {
        //        var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
        //        DCapt.ThrowIfError(result, "DCaptCreateCapture");

        //        logger.Debug("DCaptCreateCapture() " + result);

        //        int biWidth = bmp.Width;
        //        int biHeight = bmp.Height;
        //        // 
        //        int biBitCount = Image.GetPixelFormatSize(bmp.PixelFormat);
        //        uint biSizeImage = (uint)(biWidth * biHeight * biBitCount / 8);

        //        const int BI_BITFIELDS = 3;

        //        var bmiHeader = new BITMAPINFOHEADER
        //        {
        //            biWidth = biWidth,
        //            biHeight = -biHeight,
        //            biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),

        //            biBitCount = (ushort)biBitCount,
        //            biPlanes = 1,

        //            biClrUsed = 0,
        //            biClrImportant = 0,
        //            biSizeImage = biSizeImage,
        //            biCompression = BI_BITFIELDS,

        //        };

        //        var bmiColors = GetColourMask(bmp.PixelFormat);

        //        //var bmiColors = new RGBQUAD[]
        //        //{
        //        //     new RGBQUAD
        //        //     {
        //        //         rgbRed = 0,
        //        //         rgbBlue = 248,
        //        //         rgbGreen = 0
        //        //     }
        //        //};

        //        BITMAPINFO bmi = new BITMAPINFO
        //        {
        //            bmiHeader = bmiHeader,
        //           // bmiColors = bmiColors,
        //        };


        //        var dstSize = bmp.Size;

        //        RECT srcRect = new RECT
        //        {
        //            Left = captArea.Left,
        //            Right = captArea.Right,
        //            Bottom = captArea.Bottom,
        //            Top = captArea.Top,
        //        };

        //        IntPtr _hBmi = IntPtr.Zero;
        //        try
        //        {
        //            _hBmi = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BITMAPINFO)));
        //            Marshal.StructureToPtr(bmi, _hBmi, false);

        //            IntPtr hBmi = _hBmi;

        //            // The bits per pixel of the saved data. Must be 2 
        //            int bitsPerPixel = 2;//biBitCount / 8; 
        //            result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref dstSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY, ref pBuffer, ref hBmi);
        //            DCapt.ThrowIfError(result, "DCaptConfigureCapture");

        //            this.bmi = (BITMAPINFO)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFO));
        //            var _bmiHeader = bmi.bmiHeader;

        //            logger.Debug("_bmiHeader " + _bmiHeader.biWidth + "x" + _bmiHeader.biHeight + " "
        //                + _bmiHeader.biBitCount + " " + _bmiHeader.biCompression + " " + _bmiHeader.biSizeImage);

        //        }
        //        finally
        //        {
        //            if (_hBmi != IntPtr.Zero)
        //            {
        //                Marshal.FreeHGlobal(_hBmi);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);

        //        Close();
        //        throw;
        //    }
        //}


        public ErrorCode UpdateBuffer(int timeout = 10)
        {
            //logger.Verb("Update(...) " + timeout);

            ErrorCode errorCode = ErrorCode.Unexpected;

            if (!Initialized)
            {
                return ErrorCode.NotInitialized;
            }

            var bmiHeader = bitmapInfo.bmiHeader;
            var bufSize = bmiHeader.biSizeImage;
            if (bufSize > 0)
            {
                Kernel32.ZeroMemory(pBuffer, (int)bufSize);

                var result = DCapt.DCaptUpdate(hCapt);
                DCapt.ThrowIfError(result, "DCaptCreateCapture");

                var frame = VideoBuffer.GetFrame();

                bool lockTaken = false;
                try
                {
                    lockTaken = frame.Lock(timeout);

                    if (lockTaken)
                    {
                        var width = DestSize.Width;
                        var height = DestSize.Height;

                        var destPtr = frame.Buffer[0].Data;
                        var destStride = frame.Buffer[0].Stride;

                        //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader
                        // Calculating Surface Stride
                        var srcStride = ((((bmiHeader.biWidth * bmiHeader.biBitCount) + 31) & ~31) >> 3);
                        var srcPtr = this.pBuffer;

                        for (int i = 0; i < height; i++)
                        {
                            Kernel32.CopyMemory(destPtr, srcPtr, (uint)destStride);
                            destPtr += destStride;
                            srcPtr += srcStride;
                        }

                    }
                    else
                    {
                        Console.WriteLine("Drop bits...");
                    }

                }
                finally
                {
                    if (lockTaken)
                    {
                        frame.Unlock();
                    }
                }
            }

            return errorCode;

            // Console.WriteLine("DCaptCreateCapture() " + result);
        }

        //public override ErrorCode UpdateBuffer(int timeout = 10)
        //{
        //    //logger.Verb("Update(...) " + timeout);

        //    ErrorCode errorCode = ErrorCode.Unexpected;

        //    if (!Initialized)
        //    {
        //        return ErrorCode.NotInitialized;
        //    }

        //    var bufSize = bmi.bmiHeader.biSizeImage;
        //    if (bufSize > 0)
        //    {
        //        Kernel32.ZeroMemory(pBuffer, (int)bufSize);

        //        var result = DCapt.DCaptUpdate(hCapt);
        //        DCapt.ThrowIfError(result, "DCaptCreateCapture");

        //        var syncRoot = videoBuffer.syncRoot;

        //        bool lockTaken = false;
        //        try
        //        {
        //            Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

        //            if (lockTaken)
        //            {
        //                var sharedBits = videoBuffer.bitmap;
        //                var rect = new Rectangle(0, 0, sharedBits.Width, sharedBits.Height);
        //                var data = sharedBits.LockBits(rect, ImageLockMode.ReadWrite, sharedBits.PixelFormat);
        //                try
        //                {
        //                    IntPtr scan0 = data.Scan0;

        //                    Kernel32.CopyMemory(scan0, this.pBuffer, (uint)bufSize);

        //                    errorCode = ErrorCode.Ok;

        //                }
        //                finally
        //                {
        //                    sharedBits.UnlockBits(data);
        //                }
        //            }
        //            else
        //            {
        //                logger.Warn("Drop bits...");
        //            }

        //        }
        //        finally
        //        {
        //            if (lockTaken)
        //            {
        //                Monitor.Exit(syncRoot);
        //            }
        //        }
        //    }

        //    return errorCode;

        //    // Console.WriteLine("DCaptCreateCapture() " + result);
        //}


        public void Close()
        {

            Console.WriteLine("DatapathDesktopCapture::Close()");

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

                this.bitmapInfo = default(BITMAPINFO);

            }


            if (VideoBuffer != null)
            {
                VideoBuffer.Dispose();
                VideoBuffer = null;
            }

        }

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint pila, IntPtr ppvBits, IntPtr hSection, uint dwOffset);

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
            internal static extern CaptError _DCaptConfigureCapture(IntPtr hCapt, ref RECT srcRect, ref Size dstSize, int bitsPerPixel, CaptFlags flags, ref IntPtr hBuf, ref IntPtr hInfo);

            [DllImport("capt.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptConfigureCapture")]
            internal static extern CaptError DCaptConfigureCapture(IntPtr hCapt, ref RECT srcRect, ref SIZE dstSize, int bitsPerPixel, CaptFlags flags, out IntPtr hBuf, out IntPtr hInfo);



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

        // Таблица цветов из WallControll-a
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
