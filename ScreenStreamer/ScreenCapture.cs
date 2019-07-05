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
using CommonData;
using NLog;
using ScreenStreamer.Utils;
using SlimDX;
using SlimDX.Direct3D9;

namespace ScreenStreamer
{
    public abstract class ScreenCapture
    {
        protected Logger logger { get; private set; }

        internal ScreenCapture()
        {
            logger = LogManager.GetLogger(GetType().ToString());
        }

        public virtual void Init(Rectangle srcRect, Size destSize = new Size())
        {
            logger.Debug("Init(...) " + srcRect.ToString() + " " + destSize.ToString());

            if (srcRect.Width == 0 || srcRect.Height == 0)
            {
                new ArgumentException(srcRect.ToString());
            }

            if (destSize.IsEmpty)
            {
                destSize.Width = srcRect.Width;
                destSize.Height = srcRect.Height;
            }

            if (destSize.Width == 0 || destSize.Height == 0)
            {
                new ArgumentException(destSize.ToString());
            }

            this.srcRect = srcRect;
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format24bppRgb);
            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppRgb);

            //!!!!!
            this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppArgb);

        }

        public static ScreenCapture Create(CaptureType type, object[]args = null)
        {
            ScreenCapture capture = null;

            if (type == CaptureType.GDI)
            {
                capture = new GDICapture();
            }
            else if (type == CaptureType.Direct3D)
            {
                capture = new Direct3DCapture(args);
            }
            else if (type == CaptureType.GDIPlus)
            {
                capture = new GDIPlusCapture();
            }
            else if (type == CaptureType.Datapath)
            {
                capture = new DatapathDesktopCapture();
            }

            return capture;
        }

        protected Rectangle srcRect;
        protected VideoBuffer videoBuffer = null;

        public VideoBuffer VideoBuffer { get => videoBuffer; }

        public abstract bool UpdateBuffer(int timeout = 10);

        public virtual void Close()
        {
            logger.Debug("ScreenCapture::Close()");

            if (videoBuffer != null)
            {
                videoBuffer.Dispose();
                videoBuffer = null;
            }
        }

    }

    public enum CaptureType
    {
        GDI,
        Direct3D,
        GDIPlus,
        Datapath,
    }

    /// <summary>
    /// с включенной композитной отрисовкой работает лучше чем GDI
    /// </summary>
    public class Direct3DCapture : ScreenCapture
    {
       
        public Direct3DCapture(object[] args) : base()
        {
            if(args!=null && args.Length > 0)
            {
                this.hWnd = (IntPtr)args[0];
            }

        }

        private Direct3D direct3D9 = new Direct3D();
        private Device device = null;
        private AdapterInformation adapterInfo = null;
        private PresentParameters presentParams = null;

        private Surface srcSurface = null;
        private Surface destSurface = null;
        private Surface tmpSurface = null;

        private IntPtr hWnd = IntPtr.Zero;

        public override void Init(Rectangle srcRect, Size destSize)
        {
            logger.Debug("Direct3DCapture::Init(...)");

            base.Init(srcRect, destSize);

            //this.videoBuffer = new VideoBuffer(destSize.Width, destSize.Height, PixelFormat.Format32bppArgb);

            adapterInfo = direct3D9.Adapters.DefaultAdapter;//direct3D9.Adapters[1];

            var hMonitor = User32.GetMonitorFromRect(srcRect);
            if (hMonitor != IntPtr.Zero)
            {
                adapterInfo = direct3D9.Adapters.FirstOrDefault(a => a.Monitor == hMonitor);
            }

           

            logger.Info("DefaultAdapter " + " " + adapterInfo.Details.DeviceName + " " + adapterInfo.Details.Description);

            var displayMode = adapterInfo.CurrentDisplayMode;
            logger.Info("CurrentDisplayMode " + " "  + displayMode.Width +"x" + displayMode.Height + " "+ displayMode.Format);

            //Rectangle clientRect = NativeMethods.GetAbsoluteClientRect(hWnd);

            presentParams = new PresentParameters
            {
               // BackBufferFormat = adapterInfo.CurrentDisplayMode.Format,
                //BackBufferHeight = clientRect.Height,
                //BackBufferWidth = clientRect.Width,
                //BackBufferHeight = adapterInfo.CurrentDisplayMode.Height,
                //BackBufferWidth = adapterInfo.CurrentDisplayMode.Width,

                DeviceWindowHandle = hWnd,
                //Windowed = false,
                Multisample = MultisampleType.None,
                SwapEffect = SwapEffect.Discard,

                PresentFlags = PresentFlags.None,
                PresentationInterval = PresentInterval.Default,
                // FullScreenRefreshRateInHertz = 0

            };


            CreateFlags Flags = (CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing);

            //CreateFlags Flags = (CreateFlags.SoftwareVertexProcessing);

            device = new Device(direct3D9, adapterInfo.Adapter, DeviceType.Hardware, hWnd, Flags, presentParams);

            InitSurfaces();


        }

        private void InitSurfaces()
        {
            logger.Debug("InitSurfaces()");

            //AdapterInformation adapterInfo = direct3D9.Adapters.DefaultAdapter;
            var displayMode = adapterInfo.CurrentDisplayMode;

            srcSurface = Surface.CreateOffscreenPlain(device, displayMode.Width, displayMode.Height, Format.A8R8G8B8, Pool.SystemMemory);

            //tmpSurface = Surface.CreateRenderTarget(device, displayMode.Width, displayMode.Height, Format.X8R8G8B8, MultisampleType.None, 0, true);

            tmpSurface = Surface.CreateRenderTarget(device, displayMode.Width, displayMode.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
            destSurface = Surface.CreateRenderTarget(device, videoBuffer.bitmap.Width, videoBuffer.bitmap.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
        }

        private Stopwatch sw = new Stopwatch();

        public override bool UpdateBuffer(int timeout = 10)
        {
            sw.Restart();
            bool success = false;
            
            Result result = device.TestCooperativeLevel();
            if (result != ResultCode.Success)
            {
                logger.Warn("Result " + result.Name);

                if (result == ResultCode.DeviceLost)
                {//..

                }
                else if (result == ResultCode.DeviceNotReset)
                {
                    bool deviceReinit = ReInitDevice();

                    if (deviceReinit == false)
                    {
                        //TODO: error
                        logger.Warn("Reinitialize device fail");
                    }
                }

                return false;

            }


            var surfDescr = srcSurface.Description;

            result = device.GetFrontBufferData(0, srcSurface);

            //logger.Debug("GetFrontBufferData(...) " + sw.ElapsedMilliseconds);
            if (result != ResultCode.Success)
            {
                logger.Warn("GetFrontBufferData1(...) " + result);
                return false;
            }


            //srcSurface1.UnlockRectangle();

            if (result != ResultCode.Success)
            {
                logger.Warn("GetFrontBufferData(...) " + result);
                return false;
            }

            result = device.UpdateSurface(srcSurface, tmpSurface);

            if (result != ResultCode.Success)
            {
                logger.Warn("UpdateSurface(...) " + result);
                return false;
            }

            result = device.StretchRectangle(tmpSurface, destSurface, TextureFilter.Linear);
            if (result != ResultCode.Success)
            {
                logger.Warn("StretchRectangle(...) " + result);

                return false;
            }
            


            /*
            var render = device.GetRenderTarget(0);

           // var dc = render.GetDC();
            var descr = render.Description;
            var surf = Surface.CreateOffscreenPlain(device, descr.Width, descr.Height, Format.X8R8G8B8, Pool.SystemMemory);
            device.GetRenderTargetData(render, surf);

            */
            var syncRoot = videoBuffer.syncRoot;
            bool lockTaken = false;

            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref lockTaken);
                if (lockTaken)
                {
                    success = CopyToBitmap(videoBuffer.bitmap, destSurface);
                }

            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }

            //logger.Debug("CopyToBitmap(...) " + sw.ElapsedMilliseconds);
            //ReInitDevice1();
            return success;
        }


        private bool CopyToBitmap(Bitmap bmp, Surface surface)
        {
            bool result = false;

            var surfDescr = surface.Description;

            if (bmp.Width != surfDescr.Width || bmp.Height != surfDescr.Height)
            {
                //...
                logger.Warn("bmp.Width != surfDescr.Width || bmp.Height != surfDescr.Height");
                return result;
            }

            //if (surfDescr.Format != Format.A8R8G8B8)
            //{
            //    logger.Warn("Unsupported surface format " + surfDescr.Format);
            //    return result;
            //}

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                DataRectangle dataRect = surface.LockRectangle(LockFlags.ReadOnly);
                try
                {

                    int surfPitch = ((int)dataRect.Pitch / sizeof(ushort));
                    int bmpPitch = ((int)bitmapData.Stride / sizeof(ushort));
                    //int surfPitch = ((int)dataRect.Pitch / 3);
                    //int bmpPitch = ((int)bitmapData.Stride / 3);

                    DataStream dataStream = dataRect.Data;
                    unsafe
                    {
                        ushort* to = (ushort*)bitmapData.Scan0.ToPointer();
                        ushort* from = (ushort*)dataStream.DataPointer;

                        //ushort* to = (ushort*)dataStream.DataPointer;
                        //ushort* from = (ushort*)bitmapData.Scan0.ToPointer();

                        for (int j = 0; j < bmp.Height; j++)
                        {
                            for (int i = 0; i < bmpPitch; i++)
                            {
                                to[i + j * bmpPitch] = from[i + j * surfPitch];
                                //to[i + j * surfPitch] = from[i + j * bmpPitch];
                            }
                        }

                        result = true;
                    }

                }
                finally
                {
                    surface.UnlockRectangle();
                }
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }

            return result;

        }


        private bool ReInitDevice()
        {
            logger.Debug("OnReInitDevice");

            bool Result = false;
            try
            {
                OnLostDevice();

                // device.Dispose();

                // CreateFlags Flags = (CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing);
                //// CreateFlags Flags = (CreateFlags.SoftwareVertexProcessing);

                // AdapterInformation adapterInfo = direct3D9.Adapters[0];
                // device = new Device(direct3D9, adapterInfo.Adapter, DeviceType.Hardware, IntPtr.Zero, Flags, presentParams);

                // InitSurfaces();


                var resetResult = device.Reset(presentParams);

                if (resetResult.IsSuccess)
                {
                    InitSurfaces();
                    Result = true;
                }
                else
                {
                    logger.Warn("Graphic device reset result: " + resetResult);
                }
            }
            catch (Direct3D9Exception ex)
            {
                if (ex.ResultCode == ResultCode.DeviceLost)
                {
                    // deviceLost = true;
                }

                logger.Warn<Exception>(ex);
            }
            catch (Exception ex)
            {
                logger.Error<Exception>(ex);
            }

            return Result;
        }


        private void OnDeviceReset()
        {
            logger.Debug("OnDeviceReset");

            InitSurfaces();

        }

        private void OnLostDevice()
        {
            logger.Debug("OnLostDevice");

            DisposeSurfaces();

        }

        private void DisposeSurfaces()
        {
            logger.Debug("DisposeSurfaces()");

            if (srcSurface != null && !srcSurface.Disposed)
            {
                srcSurface.Dispose();
                srcSurface = null;
            }
            if (destSurface != null && !destSurface.Disposed)
            {
                destSurface.Dispose();
                destSurface = null;
            }

            if (tmpSurface != null && !tmpSurface.Disposed)
            {
                tmpSurface.Dispose();
                tmpSurface = null;
            }
        }


        public override void Close()
        {
            if (direct3D9 != null && !direct3D9.Disposed)
            {
                direct3D9.Dispose();
                direct3D9 = null;
            }

            if (device != null && !device.Disposed)
            {
                device.Dispose();
                device = null;
            }

            DisposeSurfaces();

            base.Close();
        }

    }
    /// <summary>
    /// Быстрее всего работает с выключенной композитной отрисовкой
    /// и с PixelFormat.Format32bppArgb х.з почему
    /// </summary>
    public class GDICapture : ScreenCapture
    {
        public override void Init(Rectangle srcRect, Size destSize = default(Size))
        {

            base.Init(srcRect, destSize);

            //screenDc = User32.GetDC(IntPtr.Zero);

            //compatibleDc = Gdi32.CreateCompatibleDC(screenDc);
            //compatibleBitmap = Gdi32.CreateCompatibleBitmap(screenDc, destSize.Width, destSize.Height);

            //videoBuffer.bitmap = Bitmap.FromHbitmap(compatibleBitmap);

            //hBitmap = videoBuffer.bitmap.GetHbitmap();
            //videoBuffer.bitmap = new Bitmap(destSize.Width, destSize.Height, PixelFormat.Format24bppRgb);

        }


        //static IntPtr screenDc;
        //static IntPtr compatibleDc;
        //static IntPtr compatibleBitmap;

        public override bool UpdateBuffer(int timeout = 10)
        {
            return TryGetScreen(base.srcRect, ref base.videoBuffer, timeout);
        }

        public static bool TryGetScreen(Rectangle srcRect, ref VideoBuffer videoBuffer, int timeout = 10)
        {
            bool success = false;

            var syncRoot = videoBuffer.syncRoot;
            bool lockTaken = false;

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            Graphics graphDest = null;
            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                if (lockTaken)
                {
                    var bmp = videoBuffer.bitmap;
                    graphDest = System.Drawing.Graphics.FromImage(bmp);
                    hdcDest = graphDest.GetHdc();
                    Size destSize = bmp.Size;

                    int nXDest = 0;
                    int nYDest = 0;
                    int nWidth = destSize.Width;
                    int nHeight = destSize.Height;

                    hdcSrc = User32.GetDC(IntPtr.Zero);

                    int nXSrc = srcRect.Left;
                    int nYSrc = srcRect.Top;

                    int nWidthSrc = srcRect.Width;
                    int nHeightSrc = srcRect.Height;

                    // в этом режиме мигает курсор, но захватывается все содержимое рабочего стола (ContextMenu, ToolTip-ы PopUp-ы ...)
                    // https://docs.microsoft.com/en-us/previous-versions/technet-magazine/dd392008(v=msdn.10)
                    var dwRop = TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY;
                    //var dwRop = TernaryRasterOperations.SRCCOPY;

                    if (destSize.Width == srcRect.Width && destSize.Height == srcRect.Height)
                    { 
                        //IntPtr hOldBmp = Gdi32.SelectObject(hMemoryDC, hBitmap);
                        //success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                        //hBitmap = Gdi32.SelectObject(hMemoryDC, hOldBmp);
                        //videoBuffer.bitmap = Bitmap.FromHbitmap(hBitmap);

                        success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                    }
                    else
                    {// Лучше не использовать !!!
                        Gdi32.SetStretchBltMode(hdcDest, StretchingMode.COLORONCOLOR); //самый быстрый режим

                        //самый качественный но все равно выглядит хуже чем масштабирование sws_scale
                        //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.HALFTONE);

                        success = Gdi32.StretchBlt(hdcDest, nXDest, nYDest, nWidth, nHeight,
                            hdcSrc, nXSrc, nYSrc, nWidthSrc, nHeightSrc,
                            dwRop);
                    }
  
                }

            }
            finally
            {
                Gdi32.DeleteDC(hdcSrc);

                graphDest?.ReleaseHdc(hdcDest);
                graphDest?.Dispose();
                graphDest = null;

                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }

               // videoBuffer.bitmap.Save("d:\\__test123.bmp", ImageFormat.Bmp);
            }



            return success;
        }


        public static Bitmap GetScreen(Rectangle rect)
        {
            return GetWindow(IntPtr.Zero, rect);
        }
        public static Bitmap GetWindow(IntPtr handle, Rectangle rect)
        {
            Bitmap bmp;

            Graphics g = null;
            IntPtr dest = IntPtr.Zero;
            IntPtr src = IntPtr.Zero;
            try
            {
                bmp = new Bitmap(rect.Width, rect.Height);
                g = System.Drawing.Graphics.FromImage(bmp);

                dest = g.GetHdc();
                src = User32.GetDC(handle);

                Gdi32.BitBlt(dest, rect.Left, rect.Top, rect.Width, rect.Height, src, 0, 0,
                    TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY);

            }
            finally
            {

                g.ReleaseHdc(dest);
                //g.ReleaseHdc(dc2);

                g.Dispose();
            }
            return bmp;

        }
    }


    class GDIPlusCapture : ScreenCapture
    {

        public override bool UpdateBuffer(int timeout = 10)
        {
            return TryGetScreen(srcRect, ref videoBuffer, timeout);
        }

        public static bool TryGetScreen(Rectangle bounds, ref VideoBuffer videoBuffer, int timeout = 10)
        {
            bool success = false;

            var syncRoot = videoBuffer.syncRoot;

            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, timeout, ref lockTaken);
                if (lockTaken)
                {
                    Size srcSize = new Size(bounds.Width, bounds.Height);

                    Bitmap bmp = videoBuffer.bitmap;
                    Size destSize = new Size(bmp.Width, bmp.Height);

                    if (srcSize == destSize)
                    {
                        Graphics g = Graphics.FromImage(bmp);
                        try
                        {
                            g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, srcSize, CopyPixelOperation.SourceCopy);
                            success = true;
                        }
                        finally
                        {
                            g.Dispose();
                            g = null;
                        }
                    }
                    else
                    {
                        Bitmap buf = new Bitmap(srcSize.Width, srcSize.Height);
                        try
                        {
                            Graphics g = Graphics.FromImage(buf);
                            try
                            {
                                g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, srcSize, CopyPixelOperation.SourceCopy);

                                Graphics _g = Graphics.FromImage(bmp);
                                try
                                {
                                    _g.DrawImage(buf, 0, 0);
                                    success = true;
                                }
                                finally
                                {
                                    _g.Dispose();
                                    _g = null;
                                }
                            }
                            finally
                            {
                                g.Dispose();
                                g = null;
                            }
                        }
                        finally
                        {
                            if (buf != null)
                            {
                                buf.Dispose();
                                buf = null;
                            }
                        }
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }

            return success;
        }


        public static Bitmap GetPrimaryScreen()
        {
            return GetScreen(Screen.PrimaryScreen.Bounds);
        }

        public static Bitmap GetScreen(Rectangle rect)
        {
            Size size = new Size(rect.Width, rect.Height);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height);
            Graphics g = Graphics.FromImage(bmp);
            try
            {

                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, size, CopyPixelOperation.SourceCopy);
            }
            finally
            {
                g.Dispose();
                g = null;
            }

            return bmp;
        }
    }

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

    class CaptureStats : StatCounter
    {
        public double totalTime = 0;
        public long totalBytes = 0;
        public uint totalFrameCount = 0;

        public uint currentFrame = 0;
        public long currentBytes = 0;

        public double avgFrameInterval = 0;
        public double avgBytesPerSec = 0;

        public double lastTimestamp = 0;

        public void Update(double timestamp, int bytesSize)
        {

            if (lastTimestamp > 0)
            {
                var time = timestamp - lastTimestamp;

                avgFrameInterval = (time * 0.05 + avgFrameInterval * (1 - 0.05));
                avgBytesPerSec = bytesSize / avgFrameInterval;

                totalTime += (timestamp - lastTimestamp);
            }

            totalBytes += bytesSize;

            lastTimestamp = timestamp;
            totalFrameCount++;
        }


        public override string GetReport()
        {
            StringBuilder sb = new StringBuilder();

            var fps = (1 / this.avgFrameInterval);

            //var mbytesPerSec = this.avgBytesPerSec / (1024.0 * 1024);
            //var mbytes = this.totalBytes / (1024.0 * 1024);

            TimeSpan time = TimeSpan.FromSeconds(totalTime);
            sb.AppendLine(time.ToString(@"hh\:mm\:ss\.fff"));
            sb.AppendLine(fps.ToString("0.0") + " FPS");

            sb.AppendLine("");
            sb.AppendLine(this.totalFrameCount + " Frames");

            //sb.AppendLine(mbytesPerSec.ToString("0.0") + " MByte/s");

            sb.AppendLine(StringHelper.SizeSuffix((long)avgBytesPerSec) + "/s");
            sb.AppendLine(StringHelper.SizeSuffix(totalBytes));

            return sb.ToString();
        }

        public override void Reset()
        {
            this.totalBytes = 0;
            this.totalFrameCount = 0;
            this.currentFrame = 0;
            this.currentBytes = 0;

            this.avgFrameInterval = 0;
            this.avgBytesPerSec = 0;
            this.lastTimestamp = 0;

        }
    }



    /*
    public class Direct3DCapture__
    {
        private static Direct3D direct3D9 = new Direct3D();
        private static Dictionary<IntPtr, Device> _direct3DDeviceCache = new Dictionary<IntPtr, Device>();

        private static Surface srcSurface = null;
        private static Surface destSurface = null;
        private static Surface tmpSurface = null;

        public static bool CaptureRegionDirect3D(IntPtr handle, Rectangle region, ref VideoBuffer videoBuffer)
        {
            bool success = false;

            IntPtr hWnd = handle;

            AdapterInformation adapterInfo = direct3D9.Adapters.DefaultAdapter;
            Device device;

            if (_direct3DDeviceCache.ContainsKey(hWnd))
            {
                device = _direct3DDeviceCache[hWnd];
            }
            else
            {
                Rectangle clientRect = User32.GetAbsoluteClientRect(hWnd);

                PresentParameters parameters = new PresentParameters
                {
                    BackBufferFormat = adapterInfo.CurrentDisplayMode.Format,
                    BackBufferHeight = clientRect.Height,
                    BackBufferWidth = clientRect.Width,
                    Multisample = MultisampleType.None,
                    SwapEffect = SwapEffect.Discard,
                    DeviceWindowHandle = hWnd,
                    PresentationInterval = PresentInterval.Default,
                    FullScreenRefreshRateInHertz = 0

                };

                CreateFlags Flags = (CreateFlags.Multithreaded | CreateFlags.FpuPreserve | CreateFlags.HardwareVertexProcessing);
                device = new Device(direct3D9, adapterInfo.Adapter, DeviceType.Hardware, hWnd, Flags, parameters);

                _direct3DDeviceCache.Add(hWnd, device);
            }


            if (srcSurface == null)
            {
                srcSurface = Surface.CreateOffscreenPlain(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, Format.A8R8G8B8, Pool.SystemMemory);
            }

            if (tmpSurface == null)
            {
                tmpSurface = Surface.CreateRenderTarget(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
            }

            if (destSurface == null)
            {
                destSurface = Surface.CreateRenderTarget(device, videoBuffer.bitmap.Width, videoBuffer.bitmap.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
            }


            if (srcSurface != null)
            {

                var syncRoot = videoBuffer.syncRoot;
                bool lockTaken = false;

                try
                {

                    Monitor.TryEnter(syncRoot, 10, ref lockTaken);
                    if (lockTaken)
                    {

                        Result result = device.GetFrontBufferData(0, srcSurface);

                        if (result.IsSuccess)
                        {

                            if (tmpSurface != null)
                            {
                                device.UpdateSurface(srcSurface, tmpSurface);

                                device.StretchRectangle(tmpSurface, destSurface, TextureFilter.Linear);
                            }

                            CopyToBitmap(videoBuffer.bitmap, destSurface);

                            success = true;
                        }

                        //using (var dataStream = Surface.ToStream(surface, ImageFileFormat.Bmp,
                        //    new Rectangle(region.Left, region.Top, region.Width, region.Height)))
                        //{
                        //    var bmp = videoBuffer.bitmap;
                        //    int width = bmp.Width;
                        //    int height = bmp.Height;

                        //    var data = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                        //    try
                        //    {

                        //        uint size = (uint)(data.Width * data.Height * 4);
                        //        NativeMethods.CopyMemory(data.Scan0, dataStream.DataPointer, size);


                        //        success = true;
                        //    }
                        //    finally
                        //    {
                        //        bmp.UnlockBits(data);
                        //    }
                        //}
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
        }


        unsafe static private void CopyToBitmap(Bitmap bmp, Surface surface)
        {
            var surfDescr = surface.Description;

            if (bmp.Width != surfDescr.Width || bmp.Height != surfDescr.Height)
            {
                //...
            }

            var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                DataRectangle t_data = surface.LockRectangle(LockFlags.ReadOnly);
                try
                {
                    int pitch = ((int)t_data.Pitch / sizeof(ushort));
                    int bitmapPitch = ((int)bitmapData.Stride / sizeof(ushort));

                    DataStream d_stream = t_data.Data;

                    ushort* to = (ushort*)bitmapData.Scan0.ToPointer();
                    ushort* from = (ushort*)d_stream.DataPointer;

                    //ushort* to = (ushort*)d_stream.DataPointer;
                    //ushort* from = (ushort*)bitmapData.Scan0.ToPointer();

                    for (int j = 0; j < bmp.Height; j++)
                    {
                        for (int i = 0; i < bitmapPitch; i++)
                        {
                            to[i + j * pitch] = from[i + j * bitmapPitch];
                        }
                    }
                }
                finally
                {
                    surface.UnlockRectangle();
                }
            }
            finally
            {
                bmp.UnlockBits(bitmapData);
            }

        }


    }
    */


}
