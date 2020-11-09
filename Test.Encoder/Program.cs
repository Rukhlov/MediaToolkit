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
    public partial class  Program
    {

 
        [STAThread]
        static void Main(string[] args)
        {
            //string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
            ////string shortcutPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
            //string name = "ScreenStreamer";

            //string shortcutFileName = Path.Combine(shortcutPath, name + ".lnk");

            //string workingDir = @"Y:\Users\Alexander\source\repos\ScreenStreamer\bin\Debug\ScreenStreamer.Wpf.App";

            //string fileName = Path.Combine(workingDir, "ScreenStreamer.Wpf.App.exe");
            //string _args = "-autostream";

            //ShortcutUtil.CreateShortcut(shortcutFileName, fileName, _args, workingDir, name);

            //Console.WriteLine("ShortcutUtil.CreateShortcut(...)");
            //Console.ReadKey();


            //if (File.Exists(shortcutFileName))
            //{
            //	ShortcutUtil.DeleteShortcut(shortcutFileName, fileName);
            //}
            //Console.WriteLine("File.Delete(...)");
            //Console.ReadKey();
            //return;

            MediaToolkitManager.Startup();

            SimpleSwapChain simpleSwapChain = new SimpleSwapChain();

            simpleSwapChain.Run();
			//CopyAcrossGPU();

			//NewMethod1();

			//Console.WriteLine(DxTool.LogDxInfo());

			Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

			Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;

            // NewMethod1();



            ////DxTool.FindAdapter1(4313);

            var videoEncoders = MfTool.FindVideoEncoders();

            foreach (var enc in videoEncoders)
            {
                Console.WriteLine(enc.ToString());
            }


            foreach (var enc in videoEncoders)
            {
                if (enc.Format == VideoCodingFormat.H264 && enc.Activatable)
                {
                    Console.WriteLine(enc.Name + " " + enc.ClsId + " isHardware: " + enc.IsHardware);
                }
            }

            Console.ReadKey();
            Console.WriteLine("-------------------------------");
            return;


            //NewMethod();


            //return;



            //MarshalHelper.ToArray(ppOutputTypes, (int)inputTypesNum, out MFTRegisterTypeInfo[] outputTypes);

            //MediaAttributes mediaAttributes = new MediaAttributes(ppAttributes);


            //Console.WriteLine(MfTool.LogMediaAttributes(mediaAttributes));




            //Guid VideoProcessorMFT = new Guid("88753B26-5B24-49BD-B2E7-0C445C78C982");

            //SharpDX.MediaFoundation.MediaFactory.TGetInfo(VideoProcessorMFT, IntPtr.Zero, 
            //	null, out int cInputTypesRef, 
            //	null, out int cOutputTypesRef, 
            //	out SharpDX.MediaFoundation.MediaAttributes attrs);


            //var transformFlags = TransformEnumFlag.All | // TransformEnumFlag.All |
            //		 TransformEnumFlag.SortAndFilter;

            //var outputType = new TRegisterTypeInformation
            //{
            //	GuidMajorType = MediaTypeGuids.Video,
            //	GuidSubtype = VideoFormatGuids.H264
            //	// GuidSubtype = VideoFormatGuids.Hevc
            //};
            //var category = SharpDX.MediaFoundation.TransformCategoryGuids.VideoDecoder;
            //MediaAttributes mediaAttributes = new MediaAttributes();
            //Activate[] activates = new Activate[1024];

            //SharpDX.MediaFoundation.MediaFactory.TEnum2(category, (int)transformFlags, null, outputType, mediaAttributes, activates, out int activatesNum);





            var flags = DeviceCreationFlags.VideoSupport |
                        DeviceCreationFlags.BgraSupport |
                        DeviceCreationFlags.Debug;

            var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags);
            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }


            //var descr = new SharpDX.Direct3D11.Texture2DDescription
            //{
            //	Width = 1920,
            //	Height = 1080,
            //	MipLevels = 1,
            //	ArraySize = 1,
            //	SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            //	Usage = SharpDX.Direct3D11.ResourceUsage.Default,
            //	Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
            //	BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
            //	CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
            //	OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

            //};

            //var texture = new SharpDX.Direct3D11.Texture2D(device, descr);


            var bmp = new System.Drawing.Bitmap(@"D:\Temp\4.bmp");
            var texture = DxTool.GetTexture(bmp, device);

            //Texture2D stagingTexture = null;
            //try
            //{
            //    // Create Staging texture CPU-accessible
            //    stagingTexture = new Texture2D(device,
            //        new Texture2DDescription
            //        {
            //            CpuAccessFlags = CpuAccessFlags.Read,
            //            BindFlags = BindFlags.None,
            //            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
            //            Width = 1920,
            //            Height = 1080,
            //            MipLevels = 1,
            //            ArraySize = 1,
            //            SampleDescription = { Count = 1, Quality = 0 },
            //            Usage = ResourceUsage.Staging,
            //            OptionFlags = ResourceOptionFlags.None,
            //        });

            //    device.ImmediateContext.CopyResource(texture, stagingTexture);
            //    device.ImmediateContext.Flush();

            //    //var destBmp = new System.Drawing.Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            //    System.Drawing.Bitmap destBmp = null;
            //    int count = 10;

            //    byte[] bytes = null;
            //    while (count-- > 0)
            //    {
            //        DxTool.TextureToBitmap(stagingTexture, ref destBmp);

            //        bytes = DxTool.GetTextureBytes(stagingTexture);



            //        Thread.Sleep(10);
            //    }

            //    File.WriteAllBytes("d:\\test_argb32.raw", bytes);

            //    destBmp.Save("d:\\test.bmp");

            //    destBmp.Dispose();

            //}
            //finally
            //{
            //    stagingTexture?.Dispose();
            //}

            //texture.Dispose();
            //device.Dispose();
            //bmp.Dispose();

            //var report = SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
            //Console.WriteLine(report);
            //Console.ReadKey();


            //return;



            //var flags = DeviceCreationFlags.VideoSupport |
            //     DeviceCreationFlags.BgraSupport;
            ////DeviceCreationFlags.Debug;

            //var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags);
            //using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            //{
            //    multiThread.SetMultithreadProtected(true);
            //}




            do
            {
                // var videoSource = new ScreenSource();
                // ScreenCaptureDeviceDescription captureParams = new ScreenCaptureDeviceDescription
                // {
                //     CaptureRegion = new System.Drawing.Rectangle(0, 0, 1920, 1080),
                //     Resolution = new System.Drawing.Size(1920, 1080),
                // };

                // captureParams.CaptureType = VideoCaptureType.DXGIDeskDupl;
                // captureParams.Fps = 10;
                // captureParams.CaptureMouse = true;

                // videoSource.Setup(captureParams);

                // VideoEncoderSettings encodingParams = new VideoEncoderSettings
                // {
                //     //Width = destSize.Width, // options.Width,
                //     //Height = destSize.Height, // options.Height,
                //     Resolution = new System.Drawing.Size(1920, 1080),
                //     FrameRate = 10,


                // };

                // //var videoEncoder = new VideoEncoder(videoSource);
                // //videoEncoder.Open(encodingParams);
                // var hwBuffer = videoSource.SharedTexture;
                // var hwDevice = hwBuffer.Device;

                // using (var dxgiDevice = hwDevice.QueryInterface<SharpDX.DXGI.Device>())
                // {
                //     using (var adapter = dxgiDevice.Adapter)
                //     {
                //         var adapterLuid = adapter.Description.Luid;
                //     }
                // }
                //// hwDevice.Dispose();
                // // Marshal.Release(hwDevice.NativePointer);

                // //ComObject comObject = new ComObject();
                // //comObject.

                //var flags = DeviceCreationFlags.VideoSupport |
                //    DeviceCreationFlags.BgraSupport;
                ////DeviceCreationFlags.Debug;

                //var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags);
                //using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                //{
                //    multiThread.SetMultithreadProtected(true);
                //}


                var encoder = new MfH264Encoder(null);
                encoder.Setup(new MfVideoArgs
                {
                    Width = 1920,
                    Height = 1080,
                    FrameRate = 30,
                    Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,

                });

                var encDevice = encoder.device;


                var bufTexture = new Texture2D(encDevice,
                 new Texture2DDescription
                 {
                     // Format = Format.NV12,
                     Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                     Width = 1920,
                     Height = 1080,
                     MipLevels = 1,
                     ArraySize = 1,
                     SampleDescription = { Count = 1 },
                 });



                //var processor = new MfVideoProcessor(encDevice);
                //var inProcArgs = new MfVideoArgs
                //{
                //    Width = 1920,
                //    Height = 1080,
                //    Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
                //};

                //var outProcArgs = new MfVideoArgs
                //{
                //    Width = 1920,
                //    Height = 1080,
                //    Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
                //};

                //processor.Setup(inProcArgs, outProcArgs);
                //processor.Start();

                encoder.Start();

                int count = 3;
                while (count-- > 0)
                {
                    //encDevice.ImmediateContext.CopyResource(texture, bufTexture);

                    //encoder.WriteTexture(texture);

                    Thread.Sleep(100);

                    Console.WriteLine("Try next " + count);
                }

                encoder.Stop();


                //processor.Stop();
                //processor.Close();

                bufTexture.Dispose();
                texture.Dispose();
                // videoEncoder.Close();



                //videoSource.Close();
                //hwDevice.Dispose();

                //Console.WriteLine(MfTool.GetActiveObjectsReport());

                Thread.Sleep(300);
                //encoder.Close();
                GC.Collect();

                Console.WriteLine("----------------------------------------\r\n" + MfTool.GetActiveObjectsReport());
                Console.WriteLine("Spacebar to next...");
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Spacebar)
                {
                    continue;
                }
                else
                {
                    break;
                }



            } while (true);

            Console.WriteLine("Any key to exit...");
            Console.ReadKey();

            MediaToolkitManager.Shutdown();


        }

        private static void NewMethod1()
        {
            var _flags = DeviceCreationFlags.VideoSupport |
            DeviceCreationFlags.BgraSupport |
            DeviceCreationFlags.Debug;

            var _device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, _flags);


            using (var multiThread = _device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }
            var featureLevel = _device.FeatureLevel;

            using (var dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>())
            {
                using (var adapter = dxgiDevice.Adapter)
                {
                    var descr = adapter.Description;
                    Console.WriteLine(string.Join(" ", descr.Description));
                }
            }
        }

        //private static void NewMethod1()
        //{

        //    var flags = DeviceCreationFlags.VideoSupport |
        //    DeviceCreationFlags.BgraSupport |
        //    DeviceCreationFlags.Debug;

        //    var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags);
        //    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
        //    {
        //        multiThread.SetMultithreadProtected(true);
        //    }


        //    System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"D:\Temp\4.bmp");
        //    Texture2D rgbTexture = DxTool.GetTexture(bmp, device);

        //    var bufTexture = new Texture2D(device,
        //        new Texture2DDescription
        //        {
        //                        // Format = Format.NV12,
        //            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        //            Width = 1920,
        //            Height = 1080,
        //            MipLevels = 1,
        //            ArraySize = 1,
        //            SampleDescription = { Count = 1 },
        //        });

        //    device.ImmediateContext.CopyResource(rgbTexture, bufTexture);

        //    var processor = new MfVideoProcessor(null);
        //    var inProcArgs = new MfVideoArgs
        //    {
        //        Width = 1920,
        //        Height = 1080,
        //        Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
        //    };

        //    var outProcArgs = new MfVideoArgs
        //    {
        //        Width = 1920,
        //        Height = 1080,
        //        Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
        //    };

        //    processor.Setup(inProcArgs, outProcArgs);
        //    processor.Start();


        //    var msEncoder = new MfH264EncoderMS();

        //    var encArgs = new MfVideoArgs
        //    {
        //        Width = 1920,
        //        Height = 1080,
        //        FrameRate = 30,
        //        Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,

        //    };

        //    msEncoder.Setup(encArgs);

        //    msEncoder.Start();

        //    var rgbSample = MediaFactory.CreateVideoSampleFromSurface(null);

        //    // Create the media buffer from the texture
        //    MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out var mediaBuffer);

        //    using (var buffer2D = mediaBuffer.QueryInterface<Buffer2D>())
        //    {
        //        mediaBuffer.CurrentLength = buffer2D.ContiguousLength;
        //    }

        //    rgbSample.AddBuffer(mediaBuffer);

        //    var ffEncoder = new H264Encoder();
        //    ffEncoder.Setup(new VideoEncoderSettings
        //    {
        //        Width = encArgs.Width,
        //        Height  = encArgs.Height,
        //        FrameRate = 30,
        //    });

        //    while (true)
        //    {
        //        rgbSample.SampleTime = 0;
        //        rgbSample.SampleDuration = 0;

        //       var result = processor.ProcessSample(rgbSample, out var nv12Sampel);

        //        if (result)
        //        {
        //            using (var buffer = nv12Sampel.ConvertToContiguousBuffer())
        //            {
        //                var ptr = buffer.Lock(out var maxLen, out var curLen);
        //                ffEncoder.Encode(ptr, curLen, 0);

        //                buffer.Unlock();
        //            }


        //            //result = msEncoder.ProcessSample(nv12Sampel, out var outputSample);

        //            //if (result)
        //            //{

        //            //}

        //        }

        //        nv12Sampel?.Dispose();

        //        Thread.Sleep(300);
        //    }







        //    //Console.ReadKey();
        //    //Console.WriteLine("-------------------------------");
        //    //return;
        //}

        private static void NewMethod()
        {
            Guid CColorConvertDMO = new Guid("98230571-0087-4204-b020-3282538e57d3");
            Guid VideoProcessorMFT = new Guid("88753B26-5B24-49BD-B2E7-0C445C78C982");


            Guid NVidiaH264EncoderMFT = new Guid("60F44560-5A20-4857-BFEF-D29773CB8040");
            Guid IntelQSVH264EncoderMFT = new Guid("4BE8D3C0-0515-4A37-AD55-E4BAE19AF471");

            //ArrayList inputTypes = new ArrayList();
            //ArrayList outputTypes = new ArrayList();

            //MFInt inputTypesNum = new MFInt();
            //MFInt outputTypesNum = new MFInt();
            //IntPtr ip = IntPtr.Zero;

            var result = MfApi.MFTGetInfo(CColorConvertDMO, out string pszName,
                out IntPtr ppInputTypes, out uint inputTypesNum,
                out IntPtr ppOutputTypes, out uint outputTypesNum,
                out IntPtr ppAttributes);

            if (result == MediaToolkit.NativeAPIs.HResult.S_OK)
            {
                MediaAttributes mediaAttributes = new MediaAttributes(ppAttributes);
                Console.WriteLine(MfTool.LogMediaAttributes(mediaAttributes));

                Console.WriteLine("InputTypes-------------------------------------");
                MarshalHelper.PtrToArray(ppInputTypes, (int)inputTypesNum, out MFTRegisterTypeInfo[] inputTypes);

                foreach (var type in inputTypes)
                {
                    var majorType = type.guidMajorType;
                    var subType = type.guidSubtype;

                    //Console.WriteLine(MfTool.GetMediaTypeName(majorType));

                    Console.WriteLine(MfTool.GetMediaTypeName(subType));
                }

                Console.WriteLine("");

                Console.WriteLine("OutputTypes-------------------------------------");
                MarshalHelper.PtrToArray(ppOutputTypes, (int)outputTypesNum, out MFTRegisterTypeInfo[] outputTypes);


                foreach (var type in outputTypes)
                {
                    var majorType = type.guidMajorType;
                    var subType = type.guidSubtype;

                    //Console.WriteLine(MfTool.GetMediaTypeName(majorType));

                    Console.WriteLine(MfTool.GetMediaTypeName(subType));
                }

            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }


        private static void CopyAcrossGPU()
        {
            Console.WriteLine("CopyAcrossGPU() BEGIN");
            SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

            var index = 0;

            var adapter0 = factory1.GetAdapter(index);

            Console.WriteLine( "Adapter" + index + ": " + adapter0.Description.Description);

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

            Console.WriteLine("Copy texture from " + adapter0.Description.Description + " to " + adapter1.Description.Description );

            Console.WriteLine("CopyCount " + copyCount);
            Console.WriteLine("Test started...");

            Stopwatch sw = Stopwatch.StartNew();
            Stopwatch stopwatch = new Stopwatch();
            int count = copyCount;
            int interval = 16;

            stopwatch.Start();
            while (count-->0)
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
