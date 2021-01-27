using MediaToolkit.Core;
using MediaToolkit.Utils;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

using MediaToolkit.Logging;
using System.Runtime.InteropServices;
using MediaToolkit.SharedTypes;
using System.ComponentModel;
using SharpDX.Direct3D;
using MediaToolkit.DirectX;

namespace MediaToolkit.ScreenCaptures
{
    /// <summary>
    /// Быстрее всего работает с выключенной композитной отрисовкой
    /// и с PixelFormat.Format32bppArgb 
    /// </summary>
    public class GDICapture : ScreenCapture//, ITexture2DSource
	{
		public GDICapture(Dictionary<string, object> args = null) : base()
		{
			if (args != null)
			{
				if (args.ContainsKey("WindowHandle"))
				{
					this.hWnd = (IntPtr)args["WindowHandle"];
				}
			}
		}

		private Device device = null;
        private Texture2D gdiTexture = null;
		private Texture2D sharedTexture = null;

		// public long AdapterId { get; private set; }
		public int AdapterIndex { get; private set; } = 0;
        public bool CaptureAllLayers { get; set; } = false;
		public IntPtr hWnd { get; set; } = IntPtr.Zero;

		private VideoFrameBase srcFrame = null;
		private D3D11RgbToYuvConverter pixConverter = null;

		public override void Init(Rectangle srcRect, Size destSize = default(Size))
        {
            base.Init(srcRect, destSize);

			InitDx();

			VideoBufferBase videoBuffer = null;

			if(DriverType == VideoDriverType.CPU)
			{
				videoBuffer = new MemoryVideoBuffer(DestSize, DestFormat, 32);

			}
			else if(DriverType == VideoDriverType.D3D11)
			{
				videoBuffer = new D3D11VideoBuffer(device, DestSize, DestFormat);
			}
			else
			{
				throw new InvalidOperationException("Invalid driver type: "+ DriverType);
			}

			base.VideoBuffer = videoBuffer;

			pixConverter = new D3D11RgbToYuvConverter();
			pixConverter.Init(device, SrcRect.Size, SrcFormat, DestSize, DestFormat, DownscaleFilter);

			srcFrame = new D3D11VideoFrame(sharedTexture);
		}

		private void InitDx()
        {
            logger.Debug("GDICapture::InitDx()");

            SharpDX.DXGI.Factory1 dxgiFactory = null;

            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();
                SharpDX.DXGI.Adapter1 adapter = null;
                try
                {
                    adapter = dxgiFactory.GetAdapter1(AdapterIndex);
                    //AdapterId = adapter.Description.Luid;
                    //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                    var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
                    //deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
                    device = new Device(adapter, deviceCreationFlags);
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

			gdiTexture = new Texture2D(device,
                new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    Width = SrcRect.Width,
                    Height = SrcRect.Height,

                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.GdiCompatible,

                });

			sharedTexture = new Texture2D(device,
				 new Texture2DDescription
				 {
					 CpuAccessFlags = CpuAccessFlags.None,
					 BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					 Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
					 Width = SrcRect.Width,
					 Height = SrcRect.Height,

					 MipLevels = 1,
					 ArraySize = 1,
					 SampleDescription = { Count = 1, Quality = 0 },
					 Usage = ResourceUsage.Default,
					 OptionFlags = ResourceOptionFlags.Shared,

				 });

		}


		public override ErrorCode UpdateBuffer(int timeout = 10)
        {
			ErrorCode result = BitBltToGdiSurface();

			if (result == ErrorCode.Ok)
			{
				device.ImmediateContext.CopyResource(gdiTexture, sharedTexture);
				device.ImmediateContext.Flush();

				var destFrame = VideoBuffer.GetFrame();
				pixConverter.Process(srcFrame, destFrame);
			}

			return result;
        }


        private ErrorCode BitBltToGdiSurface()
        {

            ErrorCode errorCode = ErrorCode.Unexpected;
            try
            {
                if (hWnd != IntPtr.Zero)
                {
                    var isIconic = User32.IsIconic(hWnd);
                    if (!isIconic)
                    {
                        var clientRect = User32.GetClientRect(hWnd);

                        if (this.SrcRect != clientRect)
                        {
                            logger.Info(SrcRect.ToString());

                            this.SrcRect = clientRect;

                            if (gdiTexture != null)
                            {
                                gdiTexture.Dispose();
                                gdiTexture = null;
                            }
                        }
                    }
                    else
                    {
                        logger.Debug("IsIconic: " + hWnd);
                        return ErrorCode.Ok;
                    }

                    var srcWidth = SrcRect.Width;
                    var srcHeight = SrcRect.Height;

                    if (srcWidth == 0 || srcHeight == 0)
                    {
                        logger.Error("Invalid rect: " + SrcRect.ToString());
                        return ErrorCode.Unexpected;
                    }

                    if (gdiTexture == null)
                    {
                        gdiTexture = new Texture2D(device,
                         new Texture2DDescription
                         {
                             CpuAccessFlags = CpuAccessFlags.None,
                             BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                             Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                             Width = SrcRect.Width,
                             Height = SrcRect.Height,
                             MipLevels = 1,
                             ArraySize = 1,
                             SampleDescription = { Count = 1, Quality = 0 },
                             Usage = ResourceUsage.Default,
                             OptionFlags = ResourceOptionFlags.GdiCompatible,

                         });

						srcFrame = new D3D11VideoFrame(gdiTexture);
                    }
                }


                using (var surf = gdiTexture.QueryInterface<SharpDX.DXGI.Surface1>())
                {
                    int nXSrc = SrcRect.Left;
                    int nYSrc = SrcRect.Top;
                    int nWidthSrc = SrcRect.Width;
                    int nHeightSrc = SrcRect.Height;

                    var descr = surf.Description;

                    int nXDest = 0;
                    int nYDest = 0;
                    int nWidth = descr.Width;
                    int nHeight = descr.Height;

					IntPtr hdcDest = IntPtr.Zero;
					try
                    {
                        hdcDest = surf.GetDC(true);
                        IntPtr hdcSrc = IntPtr.Zero;
                        try
                        {
                            hdcSrc = User32.GetDC(hWnd);
                            //hdcSrc = User32.GetDC(IntPtr.Zero);
                            //hdcSrc = User32.GetWindowDC(hWnd);

                            var dwRop = TernaryRasterOperations.SRCCOPY;
                            if (CaptureAllLayers)
                            {
                                dwRop |= TernaryRasterOperations.CAPTUREBLT;
                            }

                            bool success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                            if (!success)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }

                            if (CaptureMouse)
                            {
                                var x = nXSrc;
                                var y = nYSrc;

                                if (hWnd != IntPtr.Zero)
                                {
                                    POINT lpPoint = new POINT
                                    {
                                        x = 0,
                                        y = 0,
                                    };

                                    User32.ClientToScreen(hWnd, ref lpPoint);

                                    x = lpPoint.x;
                                    y = lpPoint.y;
                                }

                                User32.DrawCursorEx(hdcDest, x, y);
                            }

                            errorCode = ErrorCode.Ok;

                        }
                        finally
                        {
                            Gdi32.DeleteDC(hdcSrc);
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        logger.Warn(ex);

                        if (ex.NativeErrorCode == Win32ErrorCodes.ERROR_ACCESS_DENIED ||
                            ex.NativeErrorCode == Win32ErrorCodes.ERROR_INVALID_HANDLE)
                        {
                            errorCode = ErrorCode.AccessDenied;
                        }
                    }
                    finally
                    {
						if(hdcDest!= IntPtr.Zero)
						{
							surf.ReleaseDC();
						}

                    }
				}
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return errorCode;
        }

        public override void Close()
        {
            base.Close();

            CloseDx();
        }

        private void CloseDx()
        {
            logger.Debug("GDICapture::CloseDx()");

            if (gdiTexture != null)
            {
                gdiTexture.Dispose();
                gdiTexture = null;

            }

			if (sharedTexture != null)
			{
				sharedTexture.Dispose();
				sharedTexture = null;
			}

			if (srcFrame != null)
			{
				srcFrame.Dispose();
				srcFrame = null;
			}

            if (device != null)
            {
                device.Dispose();
                device = null;

            }

        }


        public static ErrorCode TryGetScreen(Rectangle srcRect, ref Bitmap bmp, 
            TernaryRasterOperations captFlags = TernaryRasterOperations.SRCCOPY, 
            StretchingMode stretchingMode = StretchingMode.COLORONCOLOR,
            bool captureMouse = false)
        {

            bool success = false;
            ErrorCode errorCode = ErrorCode.Unexpected;

            IntPtr hdcSrc = IntPtr.Zero;
            IntPtr hdcDest = IntPtr.Zero;
            Graphics graphDest = null;
            try
            {
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
                // var dwRop = TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY;
                //var dwRop = TernaryRasterOperations.SRCCOPY;

                if (destSize.Width == srcRect.Width && destSize.Height == srcRect.Height)
                {
                    //IntPtr hOldBmp = Gdi32.SelectObject(hMemoryDC, hBitmap);
                    //success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
                    //hBitmap = Gdi32.SelectObject(hMemoryDC, hOldBmp);
                    //videoBuffer.bitmap = Bitmap.FromHbitmap(hBitmap);

                    success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, captFlags);
                    if (!success)
                    {
                        int code = Marshal.GetLastWin32Error();
                        throw new Win32Exception(code);
                    }

                    if (captureMouse)
                    {
                        User32.DrawCursorEx(hdcDest, nXSrc, nYSrc);
                    }
                }
                else
                {// Лучше не использовать масштабирование StretchBlt !!!

                    //самый быстрый и самый кривой режим масштабирования
                    //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.COLORONCOLOR);

                    //самый качественный но все равно выглядит хуже чем масштабирование sws_scale
                    //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.HALFTONE);

                    Gdi32.SetStretchBltMode(hdcDest, stretchingMode);

                    success = Gdi32.StretchBlt(hdcDest, nXDest, nYDest, nWidth, nHeight,
                        hdcSrc, nXSrc, nYSrc, nWidthSrc, nHeightSrc,
                        captFlags);

                    if (!success)
                    {
                        int code = Marshal.GetLastWin32Error();
                        throw new Win32Exception(code);
                    }

                    // TODO: draw cursor...
                }

                errorCode = ErrorCode.Ok;

            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex);

                if (ex.ErrorCode == (int)HResult.E_ACCESSDENIED)
                {
                    errorCode = ErrorCode.AccessDenied;
                }
            }
            finally
            {
                Gdi32.DeleteDC(hdcSrc);

                graphDest?.ReleaseHdc(hdcDest);
                graphDest?.Dispose();
                graphDest = null;

                // videoBuffer.bitmap.Save("d:\\__test123.bmp", ImageFormat.Bmp);
            }



            return errorCode;
        }


        public static ErrorCode TryGetScreen(Rectangle srcRect, ref VideoBuffer videoBuffer,
            bool captureMouse = false,
            int timeout = 10,
            StretchingMode stretchingMode = StretchingMode.COLORONCOLOR)
        {
            bool success = false;
            ErrorCode errorCode = ErrorCode.Unexpected;

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
                        if (!success)
                        {
                            int code = Marshal.GetLastWin32Error();
                            throw new Win32Exception(code);
                        }

                        if (captureMouse)
                        {
                            User32.DrawCursorEx(hdcDest, nXSrc, nYSrc);
                        }
                    }
                    else
                    {// Лучше не использовать масштабирование StretchBlt !!!

                        //самый быстрый и самый кривой режим масштабирования
                        //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.COLORONCOLOR);

                        //самый качественный но все равно выглядит хуже чем масштабирование sws_scale
                        //Gdi32.SetStretchBltMode(hdcDest, StretchingMode.HALFTONE);

                        Gdi32.SetStretchBltMode(hdcDest, stretchingMode);

                        success = Gdi32.StretchBlt(hdcDest, nXDest, nYDest, nWidth, nHeight,
                            hdcSrc, nXSrc, nYSrc, nWidthSrc, nHeightSrc,
                            dwRop);

                        if (!success)
                        {
                            int code = Marshal.GetLastWin32Error();
                            throw new Win32Exception(code);
                        }

                        // TODO: draw cursor...
                    }

                    errorCode = ErrorCode.Ok;

                }
                else
                {
                    errorCode = ErrorCode.WaitTimeout;
                }

            }
            catch (Win32Exception ex)
            {
                logger.Warn(ex);

                if (ex.ErrorCode == (int)HResult.E_ACCESSDENIED)
                {
                    errorCode = ErrorCode.AccessDenied;
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



            return errorCode;
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

}
