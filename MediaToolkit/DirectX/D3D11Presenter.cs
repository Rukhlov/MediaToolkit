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
using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;

namespace MediaToolkit.DirectX
{

    public class D3D11Presenter
    {
        public D3D11Presenter(SharpDX.Direct3D11.Device d = null)
        {
            this.device = d;
        }

        private RgbProcessor rgbProcessor = null;
        private D2D1TextRenderer fpsRenderer = null;

        private SharpDX.Direct3D11.Device device = null;
        private Texture2D sharedTexture = null;
		private SwapChain swapChain = null;

        public IntPtr ViewHandle { get; private set; } = IntPtr.Zero;
        public int FramePerSec { get; set; } = 60;

        private int adapterIndex = 0;

        public GDI.Size SrcSize { get; private set; }
		public GDI.Size DestSize { get; private set; }
		public bool AspectRatio { get; set; } = true;

		public GDI.Size RenderSize { get; set; } = GDI.Size.Empty;

		public bool ShowLabel { get; set; } = false;
		public bool VSync { get; set; } = true;

		public void Setup(GDI.Size srcSize, GDI.Size destSize, IntPtr hWnd, int adapterIndex = 0)
		{
			if (running)
			{
				return;
			}
			this.SrcSize = srcSize;
			this.DestSize = destSize;
			this.ViewHandle = hWnd;
			this.adapterIndex = adapterIndex;
			Factory1 dxgiFactory = null;
			try
			{
				dxgiFactory = new SharpDX.DXGI.Factory1();
				if (device == null)
				{
					using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
					{
						device = DxTool.CreateMultithreadDevice(adapter);
					}					
				}

                rgbProcessor = new RgbProcessor();
                rgbProcessor.Init(device, SrcSize, Core.PixFormat.RGB32, DestSize, Core.PixFormat.RGB32);

				sharedTexture = new Texture2D(device, new Texture2DDescription
				{
					Width = DestSize.Width,
					Height = DestSize.Height,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					Format = Format.R8G8B8A8_UNorm,

					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,

				});

				var scd = new SwapChainDescription
				{
					SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
					SwapEffect = SwapEffect.FlipSequential,
					ModeDescription = new ModeDescription
					{
						Format = Format.R8G8B8A8_UNorm,
						//Scaling = DisplayModeScaling.Centered,
						Scaling = DisplayModeScaling.Stretched,
						//Scaling = DisplayModeScaling.Unspecified,
						Width = DestSize.Width,
						Height = DestSize.Height,
						RefreshRate = new Rational(FramePerSec, 1),				
					},
					
					IsWindowed = true,
					Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
					Flags = SwapChainFlags.None,
					BufferCount = 4,

					OutputHandle = ViewHandle,

				};

				swapChain = new SwapChain(dxgiFactory, device, scd);

                fpsRenderer = new D2D1TextRenderer();
				//using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
				{
					var backColor = GDI.Color.FromArgb(128, GDI.Color.Black);
					var foreColor = GDI.Color.Yellow;
					var font = new GDI.Font("Calibri", 16);

                    fpsRenderer.Init(device, sharedTexture, font, foreColor, backColor);
                    //fpsRenderer.Init(device, backBuffer, font, foreColor, backColor);
                }
			}
			finally
			{
				DxTool.SafeDispose(dxgiFactory);
			}

		}


		private volatile bool running = false;
		private AutoResetEvent syncEvent = new AutoResetEvent(false);
		private Task renderTask = null;

        private object syncLock = new object();
		public void Start()
        {
            Console.WriteLine("SimpleSwapChain::Start()");
			if (running)
			{
				return;
			}

			renderTask = Task.Run(() =>
			{
				running = true;
				Stopwatch sw = Stopwatch.StartNew();
                int interval = (int)(1000.0 / FramePerSec);
                int count = 0;
                while (running)
                {
                    try
                    {
						count++;
						//count = swapChain.FrameStatistics.PresentCount;
						//var text = DateTime.Now.ToString("HH: mm:ss.fff") + "\r\n" + count;
                        lock (syncLock)
                        {
                            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                            {
                                device.ImmediateContext.CopyResource(sharedTexture, backBuffer);

                                //if (ShowLabel)
                                //{
                                //    fpsRenderer.DrawText(text, new GDI.Point(0, 0), 16);
                                //}

                                swapChain.Present((VSync ? 1 : 0), PresentFlags.None);
                            }
                        }


                        int msec = (int)sw.ElapsedMilliseconds;
                        int delay = interval - msec;
                        if (delay <= 0)
                        {
                            delay = 1;
                        }

						syncEvent.WaitOne(delay);
						//syncEvent.WaitOne(1000);
						sw.Restart();
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message + " " + device.DeviceRemovedReason);
                    }

                }

            });

        }

		public void Resize(GDI.Size newSize)
		{
			if (swapChain != null)
			{
				//var modeDescription = new ModeDescription
				//{
				//	Format = Format.R8G8B8A8_UNorm,
				//	//Scaling = DisplayModeScaling.Centered,
				//	Scaling = DisplayModeScaling.Centered,
				//	//Scaling = DisplayModeScaling.Unspecified,
				//	Width = newSize.Width,
				//	Height = newSize.Height,
				//	RefreshRate = new Rational(FramePerSec, 1),
				//};

				//swapChain.ResizeTarget(ref modeDescription);


				//lock (syncLock)
				//{
				//	swapChain.ResizeBuffers(4, newSize.Width, newSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
				//}

			}

			RenderSize = newSize;
		}

		public void Stop()
		{
			running = false;
			syncEvent?.Set();
		}

		public void Update(Texture2D srcTexture, string text = "")
		{
			if(!running)
			{
				return;
			}

            lock (syncLock)
            {
				using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
				{
					if (rgbProcessor != null)
					{
                        //rgbProcessor.DrawTexture(srcTexture, backBuffer, RenderSize, AspectRatio, Transform.R0);
                        //swapChain.Present((VSync ? 1 : 0), PresentFlags.None);
                        rgbProcessor.DrawTexture(srcTexture, sharedTexture, RenderSize, AspectRatio, Transform.R0);

                        if (ShowLabel && !string.IsNullOrEmpty(text))
                        {
                            fpsRenderer.DrawText(text, new GDI.Point(0, 0), 16);
                        }

                        //syncEvent?.Set();
                    }
				}

            }

		}

		public void Close(bool forceClose = true)
		{
			Stop();

			if (forceClose)
			{
				if (renderTask != null)// && renderTask.Status == TaskStatus.Running)
				{
					bool waitResult = false;
					do
					{
						waitResult = renderTask.Wait(1000);
						if (!waitResult)
						{
							Console.WriteLine("D3D11Presenter::Close() " + waitResult);
						}
					} while (!waitResult);

					renderTask = null;
				}
			}

			if (rgbProcessor != null)
			{
				rgbProcessor.Close();
				rgbProcessor = null;
			}

            if (fpsRenderer != null)
            {
                fpsRenderer.Close();
                fpsRenderer = null;
            }

			DxTool.SafeDispose(swapChain);
			DxTool.SafeDispose(sharedTexture);
			DxTool.SafeDispose(device);

			if (syncEvent != null)
			{
				syncEvent.Dispose();
				syncEvent = null;
			}
		}

    }


}
