using MediaToolkit.Core;
using MediaToolkit.DirectX;
using MediaToolkit.Logging;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace MediaToolkit.ScreenCaptures
{
	/// <summary>
	/// Быстрее всего работает с выключенной композитной отрисовкой
	/// и с PixelFormat.Format32bppArgb 
	/// </summary>
	public class GDICapture : ScreenCapture
	{
		public GDICapture(Dictionary<string, object> args = null) : base()
		{
			//if (args != null)
			//{
			//	if (args.ContainsKey("WindowHandle"))
			//	{
			//		this.hWnd = (IntPtr)args["WindowHandle"];
			//	}
			//}
		}

		private Device device = null;
        private Texture2D gdiTexture = null;
		private Texture2D sharedTexture = null;

		private bool UseHwContext = true;
        public bool CaptureAllLayers { get; set; } = false;
		public IntPtr hWnd { get; set; } = IntPtr.Zero;

		private Bitmap bitmap = null;
		public override void Init(ScreenCaptureParameters captParams)
        {
            base.Init(captParams);

            this.CaptureMouse = captParams.CaptureMouse;
			this.UseHwContext = captParams.UseHwContext;
			if (UseHwContext)
			{
				InitDx(captParams.D3D11Device);
			}
			else
			{
				bitmap = new Bitmap(DestSize.Width, DestSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				//throw new NotSupportedException("UseHwContext == false");
			}
			
		}

		private void InitDx(Device d)
        {
            logger.Debug("GDICapture::InitDx()");
            if(d == null)
            {// TODO:...

            }

            device = new Device(d.NativePointer);
            ((SharpDX.IUnknown)device).AddReference();

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

        public override ErrorCode TryGetFrame(out IVideoFrame frame, int timeout = 10)
        {
            frame = null;
			ErrorCode result = ErrorCode.Unexpected;
			if (UseHwContext)
			{
				#region SrcSize changed
				/*
				 * 
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
					}
				}
				*/ 
				#endregion

				result = BitBltToGdiSurface();
                if (result == ErrorCode.Ok)
                {
                    device.ImmediateContext.CopyResource(gdiTexture, sharedTexture);
                }

				device.ImmediateContext.Flush();
               
                if (result == ErrorCode.Ok)
                {
                    frame = new D3D11VideoFrame(sharedTexture);
                }
                //result = ErrorCode.WaitTimeout;
            }
			else
			{
				//result = ErrorCode.NotSupported;

				var captFlags = TernaryRasterOperations.SRCCOPY;
				if (CaptureAllLayers)
				{
					captFlags |= TernaryRasterOperations.CAPTUREBLT;
				}
				var stretchingMode = StretchingMode.COLORONCOLOR;

				result = TryGetScreen(SrcRect, captFlags, stretchingMode, CaptureMouse, ref bitmap);
				if(result == ErrorCode.Ok)
				{
					frame = new GDIFrame(bitmap);
				}

			}
  
            return result;
        }

        private ErrorCode BitBltToGdiSurface()
        {
            ErrorCode errorCode = ErrorCode.Unexpected;
            try
            {
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

			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}

            CloseDx();
        }

        private void CloseDx()
        {
            logger.Debug("GDICapture::CloseDx()");

            DxTool.SafeDispose(device);
            DxTool.SafeDispose(gdiTexture);
            DxTool.SafeDispose(sharedTexture);

        }

		public static ErrorCode TryGetScreen(Rectangle srcRect,  
			ref Bitmap bmp )
		{
			var captFlags = TernaryRasterOperations.SRCCOPY;
			var stretchingMode = StretchingMode.COLORONCOLOR;
			var captureMouse = false;

			return TryGetScreen(srcRect, captFlags, stretchingMode, captureMouse, ref bmp);
		}

		public static ErrorCode TryGetScreen(Rectangle srcRect,
			TernaryRasterOperations captFlags,
			StretchingMode stretchingMode,
			bool captureMouse,
			ref Bitmap bmp)
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
