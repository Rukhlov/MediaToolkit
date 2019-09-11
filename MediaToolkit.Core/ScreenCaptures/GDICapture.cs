using MediaToolkit.Common;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{
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


        //private IntPtr screenDc;
        //private IntPtr compatibleDc;
        //private IntPtr compatibleBitmap;

        public override bool UpdateBuffer(int timeout = 10)
        {
            return TryGetScreen(base.srcRect, ref base.videoBuffer, this.CaptureMouse, timeout);
        }

        public static bool TryGetScreen(Rectangle srcRect, ref VideoBuffer videoBuffer, bool captureMouse = false, int timeout = 10)
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

                        if (captureMouse)
                        {
                            User32.DrawCursorEx(hdcDest);
                        }

                    }
                    else
                    {// Лучше не использовать масштабирование StretchBlt !!!

                        //самый быстрый и самый кривой режим масштабирования
                        Gdi32.SetStretchBltMode(hdcDest, StretchingMode.COLORONCOLOR);

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

}
