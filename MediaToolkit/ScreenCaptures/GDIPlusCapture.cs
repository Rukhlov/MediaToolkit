using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.ScreenCaptures
{

    class GDIPlusCapture : ScreenCapture
    {

        public override bool UpdateBuffer(int timeout = 10)
        {
            return TryGetScreen(SrcRect, ref videoBuffer, timeout);
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
                            g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, srcSize, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
                            success = true;
                        }
                        finally
                        {
                            g.Dispose();
                            g = null;
                        }
                    }
                    else
                    {// очень медленно лучше не использовать
                        Bitmap buf = new Bitmap(srcSize.Width, srcSize.Height);
                        try
                        {
                            Graphics g = Graphics.FromImage(buf);
                            try
                            {
                                g.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, srcSize,
                                    CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

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
            return GetScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds);
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

}
