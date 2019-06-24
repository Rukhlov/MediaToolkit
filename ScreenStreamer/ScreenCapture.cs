using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonData;
using SlimDX;
using SlimDX.Direct3D9;

namespace ScreenStreamer
{

    public class Direct3DCapture
    {
        private static Direct3D direct3D9 = new Direct3D();
        private static Dictionary<IntPtr, Device> _direct3DDeviceCache = new Dictionary<IntPtr, Device>();

        private static Surface surface = null;
        private static Surface destSurface = null;

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

        static Texture texture = null;
        static Surface tmp = null;
        public static bool CaptureRegionDirect3D(IntPtr handle, Rectangle region, ref VideoBuffer videoBuffer)
        {
            bool success = false;

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




            if (texture == null)
            {
                texture = new Texture(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);
                tmp = texture.GetSurfaceLevel(0);
            }

            

            if (destSurface == null)
            {
                //destSurface = texture.GetSurfaceLevel(0);

                //destSurface = Surface.CreateRenderTarget(device, 640, 480, Format.A8R8G8B8, MultisampleType.None, 0, false);

                destSurface = Surface.CreateRenderTarget(device, adapterInfo.CurrentDisplayMode.Width, adapterInfo.CurrentDisplayMode.Height, Format.A8R8G8B8, MultisampleType.None, 0, true);
            }

            if (surface != null)
            {
               
                var syncRoot = videoBuffer.syncRoot;
                bool lockTaken = false;
                
                try
                {

                    Monitor.TryEnter(syncRoot, 10, ref lockTaken);
                    if (lockTaken)
                    {

                        Result result = device.GetFrontBufferData(0, surface);

                        if (result.IsSuccess)
                        {
                            CopyToBitmap(videoBuffer.bitmap, surface);

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
            var bitmapData = bmp.LockBits(new Rectangle(0,0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                DataRectangle t_data = surface.LockRectangle(LockFlags.None);
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




    public class GDICapture
    {
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
                src = NativeMethods.GetDC(handle);

                NativeMethods.BitBlt(dest, rect.Left, rect.Top, rect.Width, rect.Height, src, 0, 0, (NativeMethods.TernaryRasterOperations)13369376);

            }
            finally
            {

                g.ReleaseHdc(dest);
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

        public unsafe static bool GetScreen(Rectangle rect, ref VideoBuffer videoBuffer)
        {
            bool success = false;

            var syncRoot = videoBuffer.syncRoot;
            bool lockTaken = false;
            
            IntPtr dest = IntPtr.Zero;
            Graphics g = null;
            try
            {
                Monitor.TryEnter(syncRoot, 10, ref lockTaken);

                if (lockTaken)
                {
                    var bmp = videoBuffer.bitmap;
                    g = System.Drawing.Graphics.FromImage(bmp);
                    dest = g.GetHdc();

                    IntPtr src = NativeMethods.GetDC(IntPtr.Zero);

                    success = NativeMethods.BitBlt(dest, rect.Left, rect.Top, rect.Width, rect.Height, src, 0, 0,
                        (NativeMethods.TernaryRasterOperations)13369376);
                }
                
            }
            finally
            {
                g?.ReleaseHdc(dest);
                g?.Dispose();

                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }



            return success;
        }

    }
    class GDIPlusCapture
    {
        public static Bitmap GetScreen()
        {
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            return GetScreen(rect);
        }


        public unsafe static bool GetScreen(Rectangle rect, ref VideoBuffer videoBuffer)
        {
            bool success = false;

            var syncRoot = videoBuffer.syncRoot;

            bool lockTaken = false;
            
            try
            {
                Monitor.TryEnter(syncRoot, 10, ref lockTaken);
                if (lockTaken)
                {
                    var bmp = videoBuffer.bitmap;
                    Size size = new Size(rect.Width, rect.Height);
                    Graphics g = Graphics.FromImage(bmp);
                    try
                    {
                        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, size, CopyPixelOperation.SourceCopy);
                        success = true;
                    }
                    finally
                    {
                        g.Dispose();
                        g = null;
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
