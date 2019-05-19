using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX.Direct3D9;

namespace ScreenStreamer
{

    public class Direct3DCapture
    {
        private static Direct3D direct3D9 = new Direct3D();
        private static Dictionary<IntPtr, Device> _direct3DDeviceCache = new Dictionary<IntPtr, Device>();

        private static Surface surface = null;

        public static Bitmap CaptureRegionDirect3D(IntPtr handle, Rectangle region)
        {
            IntPtr hWnd = handle;
            Bitmap bitmap = null;

            AdapterInformation adapterInfo = direct3D9.Adapters.DefaultAdapter;
            Device device;

            if (_direct3DDeviceCache.ContainsKey(hWnd))
            {
                device = _direct3DDeviceCache[hWnd];
            }

            else
            {
                Rectangle clientRect = NativeMethods.GetAbsoluteClientRect(hWnd);

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


            if (surface == null)
            {
                surface = Surface.CreateOffscreenPlain(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, Format.A8R8G8B8, Pool.SystemMemory);
            }


            if (surface != null)
            {
                device.GetFrontBufferData(0, surface);

                using (var dataStream = Surface.ToStream(surface, ImageFileFormat.Bmp, 
                    new Rectangle(region.Left, region.Top, region.Width, region.Height)))
                {
                    MemoryStream memory = new MemoryStream();

                    //dataStream.CopyTo(memory);

                    byte[] buf = new byte[1024];
                    int count = 0;
                    do
                    {
                        count = dataStream.Read(buf, 0, buf.Length);
                        memory.Write(buf, 0, buf.Length);

                    } while (count > 0);

                    bitmap = new Bitmap(memory);

                }
            }

            return bitmap;
        }
    }
    public class GDICapture
    {
        public static Bitmap GetWindow(IntPtr handle, Rectangle rect)
        {
            Bitmap bmp;

            Graphics g = null;
            IntPtr dc1 = IntPtr.Zero;
            IntPtr dc2 = IntPtr.Zero;
            try
            {
                bmp = new Bitmap(rect.Width, rect.Height);
                g = System.Drawing.Graphics.FromImage(bmp);

                dc1 = g.GetHdc();
                dc2 = NativeMethods.GetDC(handle);

                NativeMethods.BitBlt(dc1, rect.Left, rect.Top, rect.Width, rect.Height, dc2, 0, 0, (NativeMethods.TernaryRasterOperations)13369376);

            }
            finally
            {

                g.ReleaseHdc(dc1);
                //g.ReleaseHdc(dc2);

                g.Dispose();
            }
            return bmp;
            //bmp.Save(@"c:\snapshot.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }


        public static Bitmap GetScreen(Rectangle rect)
        {
            return GetWindow(IntPtr.Zero, rect);

        }
    }
    class GDIPlusCapture
    {
        public static Bitmap GetScreen()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            return GetScreen(rect);
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
