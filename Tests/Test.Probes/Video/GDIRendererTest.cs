using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.Probe.Video
{
    internal class GDIRendererTest
    {
        public static void Run()
        {
            //var fileName = @"Files\rgba_640x480.bmp";
            var fileName = @"Files\1920x1080.bmp";

            Bitmap bmp = (Bitmap)Bitmap.FromFile(fileName);

            GdiRenderer renderer = new GdiRenderer();

            Form f = new Form
            {
                Text = fileName,
                BackColor = Color.Black,
                ClientSize = new Size(bmp.Width, bmp.Height),
            };
            f.Resize += (o, a) =>
            {
                renderer.SetSize(f.ClientSize);
            };

            bool running = true;
            f.Shown += (o, a) =>
            {
                renderer.Init(f.Handle);
                renderer.SetSize(f.ClientSize);
                Task.Run(() =>
                {
                    while (running)
                    {
                        renderer.Draw(bmp);

                        System.Threading.Thread.Sleep(33);
                    }

                    renderer.Close();
                });
            };

            f.Paint += (o, a) =>
            {

            };

            Application.Run(f);

            running = false;
        }

        class GdiRenderer
        {
            // private Graphics g = null;
            private IntPtr pTarget = IntPtr.Zero;
            private IntPtr pSource = IntPtr.Zero;
            private IntPtr hWnd = IntPtr.Zero;

            public void Init(IntPtr hWnd)
            {
                this.hWnd = hWnd;
                // this.g = Graphics.FromHwnd(hWnd);
                // this.pTarget = g.GetHdc();
                this.pTarget = User32.GetDC(hWnd);
                this.pSource = Gdi32.CreateCompatibleDC(pTarget);
            }

            public void Draw(Bitmap bmp)
            {

                var rect = new Rectangle(0, 0, drawSize.Width, drawSize.Height);

                //IntPtr pTarget = g.GetHdc();

                //IntPtr pTarget = User32.GetDC(hWnd);
                //IntPtr pSource = Gdi32.CreateCompatibleDC(pTarget);

                var hBitmap = bmp.GetHbitmap();
                if (resized)
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
                    resized = false;
                }

                Gdi32.SelectObject(pSource, hBitmap);
                if (bmp.Width == rect.Width && bmp.Height == rect.Height)
                {
                    Gdi32.BitBlt(pTarget, 0, 0, bmp.Width, bmp.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
                }
                else
                {
                    var targetRect = MediaToolkit.Utils.GraphicTools.CalcAspectRatio(rect, new Size(bmp.Width, bmp.Height));

                    Gdi32.SetStretchBltMode(pTarget, StretchingMode.COLORONCOLOR);
                    Gdi32.StretchBlt(pTarget, targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height,
                        pSource, 0, 0, bmp.Width, bmp.Height, TernaryRasterOperations.SRCCOPY);
                }

                //Gdi32.DeleteDC(pSource);
                Gdi32.DeleteObject(hBitmap);

            }


            private Size drawSize = Size.Empty;
            private volatile bool resized = false;
            private object syncObj = new object();
            public void SetSize(Size size)
            {
                resized = true;
                drawSize = size;
            }

            public void Close()
            {
                //if (g != null)
                //{
                //    g.ReleaseHdc(pTarget);
                //    g.Dispose();
                //}

                Gdi32.DeleteDC(pTarget);
                Gdi32.DeleteDC(pSource);
            }
        }

    }
}
