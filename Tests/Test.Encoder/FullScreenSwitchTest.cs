using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDI = System.Drawing;
using SharpDX.WIC;
using System.Runtime.InteropServices;
using Direct2D = SharpDX.Direct2D1;
using MediaToolkit.NativeAPIs;
using MediaToolkit.DirectX;

namespace Test.Encoder
{
    class FullScreenSwitchTest
    {

        public static void Run()
        {
            Console.WriteLine("FullScreenSwapChain BEGIN");

            bool fullScreen = false;
            Form f = new Form
            { };

			f.KeyDown += (o, a) => 
			{
				//Console.WriteLine(a.KeyValue);
			};

            var size = new GDI.Size(640, 480);
            f.ClientSize = size;

            FullScreenSwitchTest swapChain = new FullScreenSwitchTest();

            swapChain.ViewHandle = f.Handle;
            var task = Task.Run(() =>
            {
                try
                {
					//var fileName = @"Files\2560x1440.bmp";
					var fileName = @"Files\1920x1080.bmp";
					swapChain.Setup(fileName);
					swapChain.Start();

					while (true)
                    {
                        Console.WriteLine("'F' to switch full screen state, 'Esc' to exit...");
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.Escape)
                        {
                            break;
                        }
                        else if (key.Key == ConsoleKey.F)
                        {
                            fullScreen = !fullScreen;
                            swapChain.SetFullScreen(fullScreen);
                        }
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    swapChain.Close();
                }

            });
			//f.Show();
			Application.Run(f);
            task.Wait();

            f.Dispose();
            Console.WriteLine("FullScreenSwapChain END");
        }

        public int AdapterIndex = 1;
        public int ImageWidth = 1280;
        public int ImageHeight = 720;
        public IntPtr ViewHandle = IntPtr.Zero;
        public int FramePerSec = 60;

        private SharpDX.Direct3D11.Device device = null;
        private SwapChain swapChain = null;
        private Adapter adapter = null;
        private SharpDX.DXGI.Factory1 dxgiFactory = null;
        private int outputIndex = 0;
		private Texture2D sourceTexture0 = null;
		private Texture2D screenTexture = null;

		private SharpDX.DirectWrite.Factory dwriteFactory = null;
		private SharpDX.DirectWrite.TextFormat textFormat = null;
		private Direct2D.SolidColorBrush sceneColorBrush = null;
		private SharpDX.Direct2D1.DeviceContext d2dContext = null;

		public void Setup(string fileName)
        {
            dxgiFactory = new SharpDX.DXGI.Factory1();
            adapter = dxgiFactory.GetAdapter1(AdapterIndex);

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
             };

            var deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

            device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);

            Console.WriteLine($"RendererAdapter {AdapterIndex}: " + adapter.Description.Description);

            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }


			sourceTexture0 = WicTool.CreateTexture2DFromBitmapFile(fileName, device);
			var srcDescr = sourceTexture0.Description;
			
			screenTexture = new Texture2D(device,
			  new Texture2DDescription
			  {
				  CpuAccessFlags = CpuAccessFlags.None,
				  BindFlags = BindFlags.RenderTarget,
				  Format = srcDescr.Format,
				  Width = srcDescr.Width,
				  Height = srcDescr.Height,
				  MipLevels = 1,
				  ArraySize = 1,
				  SampleDescription = { Count = 1, Quality = 0 },
				  Usage = ResourceUsage.Default,

				  OptionFlags = ResourceOptionFlags.None,

			  });

			ImageWidth = srcDescr.Width;
			ImageHeight = srcDescr.Height;

			var scd = new SwapChainDescription
            {
                SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
                SwapEffect = SwapEffect.Discard,
                ModeDescription = new ModeDescription
                {
					Format = srcDescr.Format,
                    //Format = Format.R8G8B8A8_UNorm,
                    Scaling = DisplayModeScaling.Stretched,
                    //Scaling = DisplayModeScaling.Centered,
                    Width = ImageWidth,
                    Height = ImageHeight,
                    RefreshRate = new Rational(FramePerSec, 1),
                },
                IsWindowed = true,
                Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                Flags = SwapChainFlags.None,
                BufferCount = 4,

                OutputHandle = ViewHandle,
				
            };

            swapChain = new SwapChain(dxgiFactory, device, scd);


			using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(Direct2D.FactoryType.MultiThreaded))
			{
				using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
				{
					using (var d2dDevice = new SharpDX.Direct2D1.Device(factory2D1, dxgiDevice))
					{
						d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);

						var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
						new SharpDX.Direct2D1.PixelFormat(screenTexture.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
						96, 96,
						SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);

						using (var surf = screenTexture.QueryInterface<Surface>())
						{
							d2dContext.Target = new SharpDX.Direct2D1.Bitmap1(d2dContext, surf, bitmapProperties);
							d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
							d2dContext.DotsPerInch = new SharpDX.Size2F(96, 96);

						}
					}
				}

			}

			dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
			 
			sceneColorBrush = new Direct2D.SolidColorBrush(d2dContext, Color.Yellow);
			int fontSize = (int)(Math.Min(ImageWidth, ImageHeight) / 8);

			textFormat = new SharpDX.DirectWrite.TextFormat(dwriteFactory, "Calibri", fontSize)
			{
				TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
				ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center
			};

		}



		public void SetFullScreen(bool fullScreen)
        {
            Console.WriteLine("SetFullScreen(...) " + fullScreen);
            Output o = null;
            try
            {
                if (fullScreen)
                {
                    o = adapter.GetOutput(outputIndex);
                }
                swapChain.SetFullscreenState(fullScreen, o);
            }
            finally
            {
                DxTool.SafeDispose(o);
            }
        }

        private volatile bool running = false;
        public void Start()
        {
			Console.WriteLine("Start()");
            Task.Run(() =>
            {

                AutoResetEvent syncEvent = new AutoResetEvent(false);
				Stopwatch sw = Stopwatch.StartNew();
				int interval = (int)(1000.0 / FramePerSec);
				long drawCount = 0;
				running = true;

				DateTime time = DateTime.Now; 

				while (running)
                {

					//d2dContext.Clear(Color.Black);
					device.ImmediateContext.CopyResource(sourceTexture0, screenTexture);

					DateTime t = DateTime.Now;
					drawCount++;
					var elapsedMilliseconds = (t - time).TotalMilliseconds;
					time = t;
					var fps = 1000.0 / elapsedMilliseconds;

					var text = ImageWidth + "x" + ImageHeight + "\r\n" +
						time.ToString("HH:mm:ss.fff") + "\r\n" +
						"------------------------\r\n" +
						"FPS: " + fps.ToString("F1") + "\r\n" +
						"Interval: " + elapsedMilliseconds + "\r\n" +
						"Count: " + drawCount;

					d2dContext.BeginDraw();
					//d2dContext.Transform = Matrix3x2.Identity;

					using (var textLayout = new SharpDX.DirectWrite.TextLayout(dwriteFactory, text, textFormat, ImageWidth, ImageHeight))
					{
						d2dContext.DrawTextLayout(new Vector2(0, 0), textLayout, sceneColorBrush, Direct2D.DrawTextOptions.None);
					}

					d2dContext.EndDraw();

					using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                    {
						device.ImmediateContext.CopyResource(screenTexture, backBuffer);
                        swapChain.Present(1, PresentFlags.None);
                    }


					int msec = (int)sw.ElapsedMilliseconds;
					int delay = interval - msec;
					if (delay <= 0)
					{
						delay = 1;
					}
					syncEvent.WaitOne(delay);
					sw.Restart();
                }

                syncEvent.Dispose();

                Close();

            });

        }

        public void Stop()
        {
			Console.WriteLine("Stop()");
			running = false;

        }

        public void Close()
        {
            DxTool.SafeDispose(dxgiFactory);
            DxTool.SafeDispose(device);
            DxTool.SafeDispose(adapter);
            DxTool.SafeDispose(swapChain);
			DxTool.SafeDispose(sourceTexture0);
			DxTool.SafeDispose(screenTexture);

			DxTool.SafeDispose(dwriteFactory);
			DxTool.SafeDispose(d2dContext);
			DxTool.SafeDispose(sceneColorBrush);
			DxTool.SafeDispose(textFormat);
			
		}
    }
}
