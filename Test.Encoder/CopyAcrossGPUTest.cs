using FFmpegLib;
using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs.MF;
using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using SharpDX.DXGI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDI = System.Drawing;
using MediaToolkit.NativeAPIs;
using System.Diagnostics;
using SharpDX.Direct3D;
using System.Windows.Forms;


namespace Test.Encoder
{
    class CopyAcrossGPUTest
    {

        public static void Run()
        {
            Console.WriteLine("CopyAcrossGPUTest() BEGIN");
            SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

            var index = 0;

            var adapter0 = factory1.GetAdapter(index);

            Console.WriteLine("Adapter" + index + ": " + adapter0.Description.Description);

            //var _flags = DeviceCreationFlags.VideoSupport |
            //             DeviceCreationFlags.BgraSupport |
            //             DeviceCreationFlags.Debug;

            var _flags = DeviceCreationFlags.None;

            var device0 = new SharpDX.Direct3D11.Device(adapter0, _flags);

            using (var multiThread = device0.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            var fileName = @"Files\1920x1080.bmp";
            var bmp = new System.Drawing.Bitmap(fileName);
            var w = bmp.Width * 2;
            var h = bmp.Height * 2;

            var b = new GDI.Bitmap(w, h, GDI.Imaging.PixelFormat.Format32bppArgb);

            var g = GDI.Graphics.FromImage(b);
            g.DrawImage(bmp, 0, 0, b.Width, b.Height);
            g.Dispose();
            bmp.Dispose();

            bmp = b;

            Console.WriteLine(string.Join(" ", fileName, bmp.Width, bmp.Height, bmp.PixelFormat));
            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                var rect = new GDI.Rectangle(0, 0, bmp.Width, bmp.Height);
                var _bmp = bmp.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);
                bmp.Dispose();
                bmp = _bmp;
            }

            var srcSize = new GDI.Size(bmp.Width, bmp.Height);

            var sourceTexture0 = GetDynamicRgbaTextureFromBitmap(bmp, device0);

            bmp.Dispose();
            bmp = null;

            var stagingTexture0 = new Texture2D(device0,
                new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Staging,
                    Format = Format.B8G8R8A8_UNorm,
                    //BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.None,

                });

            device0.ImmediateContext.CopyResource(sourceTexture0, stagingTexture0);

            index = 1;
            var adapter1 = factory1.GetAdapter(index);

            Console.WriteLine("Adapter" + index + ": " + adapter1.Description.Description);
            var device1 = new SharpDX.Direct3D11.Device(adapter1, _flags);

            var defaultTexture1 = new Texture2D(device1,
                new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,

                });

            var stagingTexture1 = new Texture2D(device1,
                new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Staging,
                    Format = Format.B8G8R8A8_UNorm,
                    CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.None,

                });

            var dynamicTexture1 = new Texture2D(device1,
                new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Dynamic,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,

                });


            int copyCount = 1000;

            Console.WriteLine("Copy texture from " + adapter0.Description.Description + " to " + adapter1.Description.Description);

            Console.WriteLine("CopyCount " + copyCount);
            Console.WriteLine("Test started...");

            Stopwatch sw = Stopwatch.StartNew();
            Stopwatch stopwatch = new Stopwatch();
            int count = copyCount;
            int interval = 16;

            stopwatch.Start();
            while (count-- > 0)
            {
                {// stagingTexture0->stagingTexture1->defaultTexture1 ~30msec
                    var srcBox = device0.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                    var destBox = device1.ImmediateContext.MapSubresource(stagingTexture1, 0, MapMode.Write, SharpDX.Direct3D11.MapFlags.None);
                    Kernel32.CopyMemory(destBox.DataPointer, srcBox.DataPointer, (uint)destBox.SlicePitch);
                    device0.ImmediateContext.UnmapSubresource(stagingTexture0, 0);
                    device1.ImmediateContext.UnmapSubresource(stagingTexture1, 0);
                    device1.ImmediateContext.CopyResource(stagingTexture1, defaultTexture1);
                }

                //{// stagingTexture0->dynamicTexture1->defaultTexture1 ~40
                //	var srcBox = device0.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                //	var destBox = device1.ImmediateContext.MapSubresource(dynamicTexture1, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
                //	Kernel32.CopyMemory(destBox.DataPointer, srcBox.DataPointer, (uint)destBox.SlicePitch);
                //	device0.ImmediateContext.UnmapSubresource(stagingTexture0, 0);
                //	device1.ImmediateContext.UnmapSubresource(dynamicTexture1, 0);

                //	device1.ImmediateContext.CopyResource(dynamicTexture1, defaultTexture1);
                //	device1.ImmediateContext.Flush();
                //}


                //{ // UpdateSubresource(...) ~44 msec
                //	var srcBox = device0.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                //	device1.ImmediateContext.UpdateSubresource(srcBox, defaultTexture1);
                //	device0.ImmediateContext.UnmapSubresource(stagingTexture0, 0);
                //}


                //{ //~ 35msec
                //	var srcBox = device0.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                //	var data = new DataBox[] { srcBox };

                //	//var tempTexture1 = new Texture2D(device1,
                //	//    new SharpDX.Direct3D11.Texture2DDescription
                //	//    {
                //	//        Width = srcSize.Width,
                //	//        Height = srcSize.Height,
                //	//        MipLevels = 1,
                //	//        ArraySize = 1,
                //	//        SampleDescription = new SampleDescription(1, 0),
                //	//        Usage = ResourceUsage.Staging,
                //	//        Format = Format.B8G8R8A8_UNorm,
                //	//        CpuAccessFlags = CpuAccessFlags.Write,//| CpuAccessFlags.Read,
                //	//                            OptionFlags = ResourceOptionFlags.None,
                //	//    }, data);

                //	var tempTexture1 = new Texture2D(device1,
                //			new SharpDX.Direct3D11.Texture2DDescription
                //			{
                //				Width = srcSize.Width,
                //				Height = srcSize.Height,
                //				MipLevels = 1,
                //				ArraySize = 1,
                //				SampleDescription = new SampleDescription(1, 0),
                //				Usage = ResourceUsage.Dynamic,
                //				Format = Format.B8G8R8A8_UNorm,
                //				BindFlags = BindFlags.ShaderResource,
                //				CpuAccessFlags = CpuAccessFlags.Write,//| CpuAccessFlags.Read,
                //				OptionFlags = ResourceOptionFlags.None,
                //			}, data);


                //	//var tempTexture1 = new Texture2D(device1,
                //	//        new SharpDX.Direct3D11.Texture2DDescription
                //	//        {
                //	//            Width = srcSize.Width,
                //	//            Height = srcSize.Height,
                //	//            MipLevels = 1,
                //	//            ArraySize = 1,
                //	//            SampleDescription = new SampleDescription(1, 0),
                //	//            Usage = ResourceUsage.Default,
                //	//            Format = Format.B8G8R8A8_UNorm,
                //	//            //BindFlags = BindFlags.ShaderResource,
                //	//            CpuAccessFlags = CpuAccessFlags.Write| CpuAccessFlags.Read,
                //	//            OptionFlags = ResourceOptionFlags.None,
                //	//        }, data);

                //	device0.ImmediateContext.UnmapSubresource(stagingTexture0, 0);

                //	device1.ImmediateContext.CopyResource(tempTexture1, defaultTexture1);
                //	device1.ImmediateContext.Flush();
                //	tempTexture1.Dispose();
                //}



                //var msec = stopwatch.ElapsedMilliseconds;
                //var delay = interval - msec;
                //if (delay > 0)
                //{
                //    Thread.Sleep((int)delay);
                //}
                //else
                //{
                //    Console.WriteLine(delay);
                //}
                //stopwatch.Restart();
            }

            var totalTime = sw.ElapsedMilliseconds;
            Console.WriteLine("ElapsedMilliseconds: " + totalTime);
            Console.WriteLine("mSecPerCopy: " + (double)totalTime / copyCount);

            // device1.ImmediateContext.CopyResource(dynamicTexture1, stagingTexture1);
            device1.ImmediateContext.CopyResource(defaultTexture1, stagingTexture1);

            GDI.Bitmap destBmp = null;
            TextureToBitmap(stagingTexture1, ref destBmp);

            destBmp.Save("Test.bmp", GDI.Imaging.ImageFormat.Bmp);



            device0.Dispose();
            adapter0.Dispose();
            sourceTexture0.Dispose();
            stagingTexture0.Dispose();

            defaultTexture1.Dispose();
            stagingTexture1.Dispose();
            dynamicTexture1.Dispose();
            device1.Dispose();
            adapter1.Dispose();

            factory1.Dispose();


            Console.WriteLine("CopyAcrossGPU() END");
        }

        public static void TextureToBitmap(Texture2D texture, ref GDI.Bitmap bmp)
        {

            var descr = texture.Description;
            if (descr.Format != Format.B8G8R8A8_UNorm)
            {
                throw new Exception("Invalid texture format " + descr.Format);
            }

            if (descr.Width <= 0 || descr.Height <= 0)
            {
                throw new Exception("Invalid texture size: " + descr.Width + " " + descr.Height);
            }

            if (bmp == null)
            {
                bmp = new GDI.Bitmap(descr.Width, descr.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
            }

            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                throw new Exception("Invalid bitmap format " + bmp.PixelFormat);

            }

            if (bmp.Width != descr.Width || bmp.Height != descr.Height)
            {
                throw new Exception("Invalid params");

            }

            using (Surface surface = texture.QueryInterface<Surface>())
            {
                try
                {
                    var srcData = surface.Map(SharpDX.DXGI.MapFlags.Read);

                    int width = bmp.Width;
                    int height = bmp.Height;
                    var rect = new GDI.Rectangle(0, 0, width, height);
                    var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                    try
                    {
                        int bytesPerPixel = GDI.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                        IntPtr srcPtr = srcData.DataPointer;
                        int srcOffset = rect.Top * srcData.Pitch + rect.Left * bytesPerPixel;

                        srcPtr = IntPtr.Add(srcPtr, srcOffset);

                        var destPtr = destData.Scan0;
                        for (int row = rect.Top; row < rect.Bottom; row++)
                        {
                            Utilities.CopyMemory(destPtr, srcPtr, width * bytesPerPixel);
                            srcPtr = IntPtr.Add(srcPtr, srcData.Pitch);
                            destPtr = IntPtr.Add(destPtr, destData.Stride);

                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(destData);
                    }
                }
                finally
                {
                    surface.Unmap();
                }
            }
        }


        public static Texture2D GetDynamicRgbaTextureFromBitmap(GDI.Bitmap bitmap, SharpDX.Direct3D11.Device device)
        {
            Texture2D texture = null;

            var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                //Windows 7 
                var descr = new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Dynamic,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    OptionFlags = ResourceOptionFlags.None,

                };

                var dataRect = new SharpDX.DataRectangle(data.Scan0, data.Stride);

                texture = new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return texture;
        }

    }
}
