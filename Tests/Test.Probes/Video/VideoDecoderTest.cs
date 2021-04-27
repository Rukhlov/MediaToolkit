using MediaToolkit.MediaFoundation;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class VideoDecoderTest
	{

		public static void Run()
		{
			Console.WriteLine("RgbToNv12Converter::Run()");
			try
			{
				VideoDecoderTest decoder = new VideoDecoderTest();
				decoder.Start();


				var direct3D = new Direct3DEx();

				var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

				var presentParams = new PresentParameters
				{
					//Windowed = true,
					//SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
					//DeviceWindowHandle = IntPtr.Zero,
					//PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
					//BackBufferCount = 1,

					Windowed = true,
					MultiSampleType = MultisampleType.None,
					SwapEffect = SwapEffect.Discard,
					PresentFlags = PresentFlags.None,
				};

				var flags = CreateFlags.HardwareVertexProcessing |
							CreateFlags.Multithreaded |
							CreateFlags.FpuPreserve;

				int adapterIndex = 0;

				var device = new DeviceEx(direct3D, adapterIndex, SharpDX.Direct3D9.DeviceType.Hardware, hWnd, flags, presentParams);




				using (var resource = texture11.QueryInterface<SharpDX.DXGI.Resource>())
				{
					var handle = resource.SharedHandle;
					//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
					//var format = (Format)((int)fourCC);
					//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
					var format = Format.A8R8G8B8;
					using (var texture3d9 = new SharpDX.Direct3D9.Texture(device,
						1920, 1080, 1, Usage.RenderTarget, format, Pool.Default,
						ref handle))
					{
						var surface = texture3d9.GetSurfaceLevel(0);
					};

					//var surface = Surface.CreateOffscreenPlain(device, 1920, 1080, format, Pool.Default, ref handle);
				}


					//Direct3DDeviceManager manager = new Direct3DDeviceManager();
					//manager.ResetDevice(device, manager.CreationToken);

					//MfH264Dxva2Decoder decoder = new MfH264Dxva2Decoder(manager);
					//var inputArgs = new MfVideoArgs
					//{
					//	Width = 1920,
					//	Height = 1080,
					//	FrameRate = MfTool.PackToLong(30, 1),
					//};

					//decoder.Setup(inputArgs);
					//decoder.Start();
				}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static Texture2D texture11 = null;
		private SharpDX.Direct3D11.Device device = null;
		public void Start()
		{
			SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

			var index = 0;

			var adapter = factory1.GetAdapter(index);

			Console.WriteLine("Adapter" + index + ": " + adapter.Description.Description);

			var _flags = DeviceCreationFlags.None;

			device = new SharpDX.Direct3D11.Device(adapter, _flags);
			using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
			{
				multiThread.SetMultithreadProtected(true);
			}

			var inputArgs = new MfVideoArgs
			{
				Width = 1920,
				Height = 1080,
				FrameRate = MfTool.PackToLong(30, 1),
			};


			texture11 = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = inputArgs.Width,
				Height = inputArgs.Height,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | BindFlags.ShaderResource,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
				//Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
				//Format = SharpDX.DXGI.Format.NV12,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared,

			});

			//MfH264Decoder decoder = new MfH264Decoder(device);

			//decoder.Setup(inputArgs);
		}


	}
}
