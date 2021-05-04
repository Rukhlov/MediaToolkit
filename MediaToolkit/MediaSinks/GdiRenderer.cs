using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Renderers
{
    public class GdiRenderer
    {
        private IntPtr offScreenDc = IntPtr.Zero;
        private IntPtr offScreenBitmap = IntPtr.Zero;
        private IntPtr offScreenBits = IntPtr.Zero;
        private int offScreenPitch = 0;
        private IntPtr hWnd = IntPtr.Zero;
        private int bitCount = 0;

        public int Width { get; private set; } = 0;
        public int Height { get; private set; } = 0;
        public System.Drawing.Imaging.PixelFormat Format { get; private set; } = System.Drawing.Imaging.PixelFormat.Undefined;

        private bool initialized = false;
        public void Init(IntPtr hWnd, int biWidth, int biHeight, System.Drawing.Imaging.PixelFormat pixFormat)
        {
            if (pixFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb &&
                pixFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb &&
                pixFormat != System.Drawing.Imaging.PixelFormat.Format32bppRgb)
            {
                throw new FormatException("Invalid format: " + pixFormat);
            }

            this.hWnd = hWnd;
            this.Width = biWidth;
            this.Height = biHeight;
            this.Format = pixFormat;

            var windowDc = User32.GetDC(hWnd);

            try
            {
				var planes = Gdi32.GetDeviceCaps(windowDc, (int)Gdi32.DeviceCap.PLANES);
				var bitspixel = Gdi32.GetDeviceCaps(windowDc, (int)Gdi32.DeviceCap.BITSPIXEL);

				bitCount = Image.GetPixelFormatSize(Format);
                uint biSizeImage = (uint)(biWidth * biHeight * bitCount / 8);

                const int BI_RGB = 0;
                const int DIB_RGB_COLORS = 0;

                var bih = new BITMAPINFOHEADER
                {
                    biWidth = Width,
                    biHeight = -Height,
                    biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),

                    biBitCount = (ushort)bitCount,
                    biPlanes = 1,

                    biClrUsed = 0,
                    biClrImportant = 0,
                    biSizeImage = biSizeImage,
                    biCompression = BI_RGB,

                };

                BITMAPINFO bi = new BITMAPINFO();
                bi.bmiHeader = bih;

                this.offScreenPitch = (bitCount * biWidth) / 8;
                IntPtr hSection = IntPtr.Zero;
                this.offScreenBitmap = Gdi32.CreateDIBSection(windowDc, ref bi, DIB_RGB_COLORS, out var ppvBits, hSection, 0);
                this.offScreenBits = ppvBits;

                this.offScreenDc = Gdi32.CreateCompatibleDC(windowDc);

                Gdi32.SelectObject(offScreenDc, offScreenBitmap);
            }
            finally
            {
                User32.ReleaseDC(hWnd, windowDc);
            }

            initialized = true;
        }


        public void Update(Bitmap bmp)
        {
            if (!initialized)
            {// invalid state...
                return;
            }

            if (bmp.Width != this.Width || bmp.Height != this.Height || bmp.PixelFormat != this.Format)
            {
                // invalid format...
                return;
            }

            var bmpRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(bmpRect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            //var size = data.Stride * bmp.Height;

            //Kernel32.CopyMemory(offScreenBits, data.Scan0, (uint)size);

            Update(data.Scan0, data.Stride);

            bmp.UnlockBits(data);

        }

        public void Update(IntPtr bmpPtr, int bmpPitch)
        {
            var dest = offScreenBits;
            var destPitch = offScreenPitch;

            var src = bmpPtr;
            var srcPitch = bmpPitch;

            var widthInBytes = Width * bitCount / 8;
            var rowNumber = Height;
            //MediaToolkit.Utils.GraphicTools.CopyImage(dest, destPitch, src, srcPitch, widthInBytes, rowNumber);
            SharpDX.MediaFoundation.MediaFactory.CopyImage(dest, destPitch, src, srcPitch, widthInBytes, rowNumber);
        }

        public void Draw(bool aspectRatio = false, bool background = false)
        {
            if (!initialized)
            {
                return;
            }

            var rect = User32.GetClientRect(hWnd);
            IntPtr windowDc = User32.GetDC(hWnd);
            try
            {
                Gdi32.SelectObject(offScreenDc, offScreenBitmap);

                if (this.Width != rect.Width || this.Height != rect.Height)
                {
                    if (background)
                    {
                        RECT r = new RECT
                        {
                            Left = rect.Left,
                            Top = rect.Top,
                            Right = rect.Right,
                            Bottom = rect.Bottom,
                        };
                        var bb = Gdi32.GetStockObject(Gdi32.StockObjects.BLACK_BRUSH);

                        User32.FillRect(windowDc, ref r, bb);
                        Gdi32.DeleteObject(bb);
                    }

                    var targetRect = MediaToolkit.Utils.GraphicTools.CalcAspectRatio(rect, new Size(Width, Height), aspectRatio);

                    //Gdi32.SetStretchBltMode(windowDc, StretchingMode.HALFTONE);
                    Gdi32.SetStretchBltMode(windowDc, StretchingMode.COLORONCOLOR);
                    Gdi32.StretchBlt(windowDc, targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height,
                        offScreenDc, 0, 0, this.Width, this.Height, TernaryRasterOperations.SRCCOPY);
                }
                else
                {
                    Gdi32.BitBlt(windowDc, rect.X, rect.Y, rect.Width, rect.Height, offScreenDc, 0, 0, TernaryRasterOperations.SRCCOPY);
                }

            }
            finally
            {
                User32.ReleaseDC(hWnd, windowDc);
            }


        }

        public void Close()
        {
            if (offScreenBitmap != IntPtr.Zero)
            {
                Gdi32.DeleteObject(offScreenBitmap);
            }

            if (offScreenDc != IntPtr.Zero)
            {
                Gdi32.DeleteDC(offScreenDc);
            }

            initialized = false;
        }


        public static void Draw(IntPtr hWnd, IntPtr hBitmap, Rectangle bmpRect, bool aspectRatio = false, bool background = false)
        {
            var rect = User32.GetClientRect(hWnd);

            IntPtr pTarget = User32.GetDC(hWnd);
            IntPtr pSource = Gdi32.CreateCompatibleDC(pTarget);

            Gdi32.SelectObject(pSource, hBitmap);

            if (bmpRect.Width == rect.Width && bmpRect.Height == rect.Height)
            {
                Gdi32.BitBlt(pTarget, rect.X, rect.Y, rect.Width, rect.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
            }
            else
            {
                if (background)
                {
                    RECT r = new RECT
                    {
                        Left = rect.Left,
                        Top = rect.Top,
                        Right = rect.Right,
                        Bottom = rect.Bottom,
                    };
                    var bb = Gdi32.GetStockObject(Gdi32.StockObjects.BLACK_BRUSH);

                    User32.FillRect(pTarget, ref r, bb);
                    Gdi32.DeleteObject(bb);
                    background = false;
                }

                var targetRect = MediaToolkit.Utils.GraphicTools.CalcAspectRatio(rect, bmpRect.Size, aspectRatio);

                Gdi32.SetStretchBltMode(pTarget, StretchingMode.COLORONCOLOR);
                Gdi32.StretchBlt(pTarget, targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height,
                    pSource, 0, 0, bmpRect.Width, bmpRect.Height, TernaryRasterOperations.SRCCOPY);
            }


            User32.ReleaseDC(hWnd, pTarget);
            Gdi32.DeleteDC(pSource);
        }
    }

}
