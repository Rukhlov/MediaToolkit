using MediaToolkit.DirectX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class DxTextureTest
	{
		public static void Run()
		{

			Console.WriteLine("DxTextureTest::Run()");

			SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

			var index = 1;

			var adapter0 = factory1.GetAdapter(index);

			var _flags = DeviceCreationFlags.BgraSupport;
			var device0 = new SharpDX.Direct3D11.Device(adapter0, _flags);
            using (var multiThread = device0.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            var fileName = @"Files\rgba_640x480.bmp";
            //var fileName = @"D:\Dropbox\Public\1681_source.jpg";
            var rgbTexture = WicTool.CreateTexture2DFromBitmapFile(fileName, device0);

            var descr = rgbTexture.Description;

            var texture = new Texture2D(device0,
				new SharpDX.Direct3D11.Texture2DDescription
				{
					Width = descr.Width,
					Height = descr.Height,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					Format = descr.Format,//SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    //BindFlags = BindFlags.ShaderResource,
                    //CpuAccessFlags = CpuAccessFlags.Read,
                   // BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                   //OptionFlags = ResourceOptionFlags.Shared,

				});

            device0.ImmediateContext.CopyResource(rgbTexture, texture);
            device0.ImmediateContext.Flush();

            //var bytes = DxTool.DumpTexture(device0, texture);

            //File.WriteAllBytes("TexDump.raw", bytes);

            rgbTexture.Dispose();

			var surf = texture.QueryInterface<Surface>();
			var parentDevice = surf.GetDevice<SharpDX.Direct3D11.Device>();

			var parentTexture = surf.QueryInterface<Texture2D>();


            var dxgiDevice = surf.GetDevice<SharpDX.DXGI.Device>();
            var parentAdapter = dxgiDevice.Adapter;
            surf?.Dispose();

            //var parentDevice = parentTexture.Device;
            //parentDevice.ImmediateContext
            //var dxgiDevice = parentDevice.QueryInterface<SharpDX.DXGI.Device>();


            //var parentAdapter = dxgiDevice.Adapter;
			parentDevice?.Dispose();

			dxgiDevice?.Dispose();

           

            SharpDX.Direct3D11.Device device1 = null;
            var task = Task.Run(() =>
            {
                SharpDX.DXGI.Factory1 factory = new SharpDX.DXGI.Factory1();
                var adapter1 = factory.GetAdapter(index);
                device1 = new SharpDX.Direct3D11.Device(adapter1, DeviceCreationFlags.None);
                //var device1 = new SharpDX.Direct3D11.Device(parentAdapter, _flags);
                using (var multiThread = device1.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }

                factory.Dispose();
                parentAdapter?.Dispose();
                adapter1?.Dispose();
            });

            task.Wait();

            parentTexture?.Dispose();
			//parentDevice?.Dispose();

			//System.Threading.Thread.Sleep(1000);


			var texture1 = new Texture2D(device1,
				new SharpDX.Direct3D11.Texture2DDescription
				{
					Width = descr.Width,
					Height = descr.Height,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					Format = descr.Format,//SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                                          //BindFlags = BindFlags.ShaderResource,
                                          //CpuAccessFlags = CpuAccessFlags.Read,
                   // BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    //OptionFlags = ResourceOptionFlags.Shared,

				});

            //using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
            //{
            //    var handle = sharedRes.SharedHandle;
            //    if (handle != IntPtr.Zero)
            //    {
            //        using (var sharedTex = device1.OpenSharedResource<Texture2D>(handle))
            //        {
            //            device1.ImmediateContext.CopyResource(sharedTex, texture1);

            //            var bytes = DxTool.DumpTexture(device1, texture1);

            //            File.WriteAllBytes("TexDump.raw", bytes);
            //        }
            //    }
            //}

            device1.ImmediateContext.CopyResource(texture, texture1);
            var bytes = DxTool.DumpTexture(device1, texture1);
            File.WriteAllBytes("TexDump.raw", bytes);

            rgbTexture.Dispose();
            texture1?.Dispose();
			device1?.Dispose();

			texture?.Dispose();
			//
			device0?.Dispose();
			adapter0?.Dispose();
			factory1?.Dispose();

			//parentDevice?.Dispose();
		}

	}
}
