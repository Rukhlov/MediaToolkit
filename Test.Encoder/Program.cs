using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
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

                var bmp = new System.Drawing.Bitmap(@"D:\Temp\4.bmp");
                var texture = DxTool.GetTexture(bmp, encDevice);



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
