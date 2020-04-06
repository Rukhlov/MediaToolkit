using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs.MF;
using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Encoder
{
    class Program
    {



		static void Main(string[] args)
        {

            MediaToolkitManager.Startup();

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
						DeviceCreationFlags.BgraSupport|
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

			Texture2D stagingTexture = null;
			try
			{
				// Create Staging texture CPU-accessible
				stagingTexture = new Texture2D(device,
					new Texture2DDescription
					{
						CpuAccessFlags = CpuAccessFlags.Read,
						BindFlags = BindFlags.None,
						Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
						Width = 1920,
						Height = 1080,
						MipLevels = 1,
						ArraySize = 1,
						SampleDescription = { Count = 1, Quality = 0 },
						Usage = ResourceUsage.Staging,
						OptionFlags = ResourceOptionFlags.None,
					});

				device.ImmediateContext.CopyResource(texture, stagingTexture);
				device.ImmediateContext.Flush();

				//var destBmp = new System.Drawing.Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

				System.Drawing.Bitmap destBmp = null;
				int count = 10;

                byte[] bytes = null;
				while (count-->0)
				{
					DxTool.TextureToBitmap(stagingTexture, ref destBmp);

					bytes = DxTool.GetTextureBytes(stagingTexture);



					Thread.Sleep(10);
				}

                File.WriteAllBytes("d:\\test_argb32.raw", bytes);

				destBmp.Save("d:\\test.bmp");

				destBmp.Dispose();

			}
			finally
			{
				stagingTexture?.Dispose();
			}

			texture.Dispose();
			device.Dispose();
			bmp.Dispose();

			var report = SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects();
			Console.WriteLine(report);
			Console.ReadKey();


			return;



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


                var encoder = new MfEncoderAsync(null);
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
                while (count-->0)
                {
                    //encDevice.ImmediateContext.CopyResource(texture, bufTexture);

                    encoder.WriteTexture(texture);

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
    }
}
