using MediaToolkit.NativeAPIs;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.IO;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Test.Encoder
{
    public class WicTool
    {

        public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device)
        {
            using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
            {
                using (var bitmapSource = LoadBitmapSource(factory, fileName))
                {
                    
                    var descr = new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                    };

                    return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
                }
            }
        }

        public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr)
        {
            using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
            {
                using (var bitmapSource = LoadBitmapSource(factory, fileName))
                {
                    return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
                }
            }
        }

        public static SharpDX.WIC.BitmapSource LoadBitmapSource(SharpDX.WIC.ImagingFactory2 factory, string filename)
        {
            using (var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, filename, SharpDX.WIC.DecodeOptions.CacheOnDemand))
            {
                var formatConverter = new SharpDX.WIC.FormatConverter(factory);

                using (var frameDecode = bitmapDecoder.GetFrame(0))
                {
                    formatConverter.Initialize(frameDecode,
                        SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                        SharpDX.WIC.BitmapDitherType.None,
                        null,
                        0.0,
                        SharpDX.WIC.BitmapPaletteType.Custom);
                }

                return formatConverter;
            }
        }

        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            var descr = new SharpDX.Direct3D11.Texture2DDescription()
            {
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,


                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

            };

            return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
        }

        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr, SharpDX.WIC.BitmapSource bitmapSource)
        {
            var bitmapSize = bitmapSource.Size;

            descr.Width = bitmapSize.Width;
            descr.Height = bitmapSize.Height;
            descr.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;


            int bitmapStride = bitmapSize.Width * 4;
            int bitmapLenght = bitmapSize.Height * bitmapStride;

            using (var buffer = new SharpDX.DataStream(bitmapLenght, true, true))
            {
                bitmapSource.CopyPixels(bitmapStride, buffer);

                var dataRect = new SharpDX.DataRectangle(buffer.DataPointer, bitmapStride);

                //GDI.Bitmap bitmap = new GDI.Bitmap(size.Width, size.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
                //var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                //var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                //Kernel32.CopyMemory(data.Scan0, buffer.DataPointer, (uint)dataLen);
                //bitmap.Save(@"d:\test23424.jpg");
                //bitmap.UnlockBits(data);

                return new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }
        }

    }

    static class WicTest1
    {
        public static void Run()
        {

            int width = 512;
            int height = 512;

            var dxgiFactory = new SharpDX.DXGI.Factory1();
            var adapter = dxgiFactory.GetAdapter1(0);

            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;

            //var deviceCreationFlags =
            //	//DeviceCreationFlags.Debug |
            //	DeviceCreationFlags.VideoSupport |
            //	DeviceCreationFlags.BgraSupport;

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
            };

            var device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);
            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

             var fileName = @"Files\1920x1080.bmp";
            //var fileName = @"Files\2560x1440.bmp";
           //  var fileName = @"Files\rgba_352x288.bmp";

            var sourceTexture0 = WicTool.CreateTexture2DFromBitmapFile(fileName, device);
            var srcDescr = sourceTexture0.Description;
            width = srcDescr.Width;
            height = srcDescr.Height;

            var destTexture = new Texture2D(device,
                new Texture2DDescription
                {

                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    //Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    //Width = width,
                    //Height = height,

                    Format = srcDescr.Format,

                    Width = srcDescr.Width,
                    Height = srcDescr.Height,

                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                    //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                    OptionFlags = ResourceOptionFlags.Shared,

                });


            //device.ImmediateContext.CopyResource(sourceTexture0, destTexture);
            //device.ImmediateContext.Flush();


            using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded))
            {
                var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>();

                Direct2D.Device device2d = new Direct2D.Device(factory2D1, dxgiDevice);
                var device2dContext = new Direct2D.DeviceContext(device2d, Direct2D.DeviceContextOptions.EnableMultithreadedOptimizations);

                var imagingFactory2 = new ImagingFactory2();

                //WICStream stream = new WICStream(factory, filename, NativeFileAccess.Write);

                MemoryStream memoryStream = new MemoryStream();
                WICStream wicStream = new WICStream(imagingFactory2, memoryStream);

                Stopwatch sw = new Stopwatch();

                //sw.Restart();
                //var wicEncoder = new JpegBitmapEncoder(imagingFactory2);
                var wicEncoder = new BitmapEncoder(imagingFactory2, ContainerFormatGuids.Jpeg);
                wicEncoder.Initialize(wicStream);

                var wicFrameEncode = new BitmapFrameEncode(wicEncoder);
                wicFrameEncode.Options.ImageQuality = 0.8f;
                wicFrameEncode.Options.JpegYCrCbSubsampling = JpegYCrCbSubsamplingOption.Mode420;
                //wicFrameEncode.Options.CompressionQuality = 1f;
                wicFrameEncode.Initialize();
                //var bitmapSource = WicTool.LoadBitmapSource(imagingFactory2, fileName);

                int count = 2;
                while (count-- > 0)
                {
                    sw.Restart();

                    //wicFrameEncode.WriteSource(bitmapSource);

                    var imageEncoder = new ImageEncoder(imagingFactory2, device2d);

                    // device.ImmediateContext.CopyResource(sourceTexture0, destTexture);
                    //device.ImmediateContext.Flush();
                    //using (var surf = destTexture.QueryInterface<Surface1>())
                    using (var surf = sourceTexture0.QueryInterface<Surface1>())
                    {
                        var pixFmt = new Direct2D.PixelFormat(srcDescr.Format, Direct2D.AlphaMode.Premultiplied);
                        var prop1 = new Direct2D.BitmapProperties1(pixFmt);

                        Direct2D.Bitmap1 bitmap2d = new Direct2D.Bitmap1(device2dContext, surf);

                        ImageParameters parameters = new ImageParameters(pixFmt, 96f, 96f, 0, 0, width, height);
                        imageEncoder.WriteFrame(bitmap2d, wicFrameEncode, parameters);
                       
                        bitmap2d.Dispose();

                    }

                    imageEncoder.Dispose();

                    Console.WriteLine("ElapsedMilliseconds " + sw.ElapsedMilliseconds);
                }

                wicFrameEncode.Commit();
                wicEncoder.Commit();


                wicEncoder.Dispose();
                wicFrameEncode.Dispose();

                //Thread.Sleep(33);
                //bitmapSource.Dispose();

                wicStream.Dispose();

               //imageEncoder.Dispose();
                device2d.Dispose();
                dxgiDevice.Dispose();
                device2dContext.Dispose();
                imagingFactory2.Dispose();
                //stream.Commit(SharpDX.Win32.CommitFlags.Default);

                File.WriteAllBytes("output.jpg", memoryStream.ToArray());
                //File.WriteAllBytes("output.png", memoryStream.ToArray());

            }


            adapter.Dispose();
            destTexture.Dispose();
            sourceTexture0.Dispose();
            device.Dispose();
            dxgiFactory.Dispose();
        }
    }

    static class WicTest
    {
        public static void Run()
        {
            const int width = 512;
            const int height = 512;
            const string filename = "output.jpg";

            var factory = new ImagingFactory();

            WICStream stream = null;

            // ------------------------------------------------------
            // Encode a JPG image
            // ------------------------------------------------------

            // Create a WIC outputstream 
            if (File.Exists(filename))
                File.Delete(filename);

            stream = new WICStream(factory, filename, NativeFileAccess.Write);

            // Initialize a Jpeg encoder with this stream
            var encoder = new JpegBitmapEncoder(factory);


            encoder.Initialize(stream);

            // Create a Frame encoder
            var bitmapFrameEncode = new BitmapFrameEncode(encoder);
            bitmapFrameEncode.Options.ImageQuality = 0.8f;
            bitmapFrameEncode.Initialize();
            bitmapFrameEncode.SetSize(width, height);
            var guid = PixelFormat.Format24bppBGR;
            bitmapFrameEncode.SetPixelFormat(ref guid);

            // Write a pseudo-plasma to a buffer
            int stride = PixelFormat.GetStride(PixelFormat.Format24bppBGR, width);
            var bufferSize = height * stride;
            var buffer = new DataStream(bufferSize, true, true);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer.WriteByte((byte)(x / 2.0 + 20.0 * Math.Sin(y / 40.0)));
                    buffer.WriteByte((byte)(y / 2.0 + 30.0 * Math.Sin(x / 80.0)));
                    buffer.WriteByte((byte)(x / 2.0));
                }
            }

            // Copy the pixels from the buffer to the Wic Bitmap Frame encoder
            bitmapFrameEncode.WritePixels(512, new DataRectangle(buffer.DataPointer, stride));

            // Commit changes
            bitmapFrameEncode.Commit();
            encoder.Commit();
            bitmapFrameEncode.Dispose();
            encoder.Dispose();
            stream.Dispose();

            // ------------------------------------------------------
            // Decode the previous JPG image
            // ------------------------------------------------------

            // Read input
            stream = new WICStream(factory, filename, NativeFileAccess.Read);
            var decoder = new JpegBitmapDecoder(factory);
            decoder.Initialize(stream, DecodeOptions.CacheOnDemand);
            var bitmapFrameDecode = decoder.GetFrame(0);
            var queryReader = bitmapFrameDecode.MetadataQueryReader;

            // Dump MetadataQueryreader
            queryReader.Dump(Console.Out);
            queryReader.Dispose();

            bitmapFrameDecode.Dispose();
            decoder.Dispose();
            stream.Dispose();

            // Dispose
            factory.Dispose();

            System.Diagnostics.Process.Start(Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, filename)));
        }
    }
}
