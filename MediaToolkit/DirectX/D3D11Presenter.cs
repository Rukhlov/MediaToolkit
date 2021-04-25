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
        private TextRenderer fpsRenderer = null;

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
						//Scaling = DisplayModeScaling.Stretched,
						Scaling = DisplayModeScaling.Unspecified,
						Width = DestSize.Width,//ImageWidth,
						Height = DestSize.Height,//ImageHeight,
						RefreshRate = new Rational(FramePerSec, 1),

					},
					IsWindowed = true,
					Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
					Flags = SwapChainFlags.None,
					BufferCount = 4,

					OutputHandle = ViewHandle,

				};

				swapChain = new SwapChain(dxgiFactory, device, scd);

                fpsRenderer = new TextRenderer();
                using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                {
                    var backColor = GDI.Color.FromArgb(128, GDI.Color.Black);
                    var foreColor = GDI.Color.Yellow;
                    var font = new GDI.Font("Calibri", 16);

                    fpsRenderer.Init(device, backBuffer, font, foreColor, backColor);
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
                ulong count = 0;
                while (running)
                {
                    try
                    {
                        var text = DateTime.Now.ToString("HH: mm:ss.fff") + "\r\n" + (count++);
                        lock (syncLock)
                        {
                            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                            {
                                device.ImmediateContext.CopyResource(sharedTexture, backBuffer);

                                var scaleX = DestSize.Width / (float)RenderSize.Width;
                                var scaleY = DestSize.Height / (float)RenderSize.Height;

                                fpsRenderer.DrawText(text, new GDI.Point(0,0));
                                swapChain.Present(1, PresentFlags.None);
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

		public void Stop()
		{
			running = false;
			syncEvent?.Set();
		}

		public void Update(Texture2D srcTexture)
		{
			if(!running)
			{
				return;
			}

            lock (syncLock)
            {
                if (rgbProcessor != null)
                {
                    rgbProcessor.DrawTexture(srcTexture, sharedTexture, RenderSize, AspectRatio, Transform.R0);
                    //syncEvent?.Set();
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

    public class TextRenderer
    {

        private SharpDX.DirectWrite.TextFormat textFormat = null;
        private Direct2D.SolidColorBrush foreBrush = null;
        private Direct2D.SolidColorBrush backBrush = null;
        private Direct2D.DeviceContext d2dContext = null;
        private SharpDX.DirectWrite.Factory dwriteFactory = null;
        public GDI.Rectangle SrcRect { get; set; }
        private Direct2D.Bitmap1 targetBitmap = null;


        public void Init(SharpDX.Direct3D11.Device device, Texture2D texture, GDI.Font gdiFont, GDI.Color gdiForeColor, GDI.Color gdiBackColor)
        {

            var srcDescr = texture.Description;

            using (Direct2D.Factory1 factory2D1 = new Direct2D.Factory1(Direct2D.FactoryType.MultiThreaded))
            {
                using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
                {
                    using (var d2dDevice = new Direct2D.Device(factory2D1, dxgiDevice))
                    {
                        d2dContext = new Direct2D.DeviceContext(d2dDevice, Direct2D.DeviceContextOptions.None);

                        var bitmapProperties = new Direct2D.BitmapProperties1(
                        new SharpDX.Direct2D1.PixelFormat(srcDescr.Format, Direct2D.AlphaMode.Premultiplied),
                        96, 96,
                        Direct2D.BitmapOptions.Target | Direct2D.BitmapOptions.CannotDraw);

                        using (var surf = texture.QueryInterface<Surface>())
                        {
                            targetBitmap = new Direct2D.Bitmap1(d2dContext, surf, bitmapProperties);

                            d2dContext.Target = targetBitmap;
                            d2dContext.TextAntialiasMode = Direct2D.TextAntialiasMode.Grayscale;
                            d2dContext.DotsPerInch = new Size2F(96, 96);

                        }
                    }
                }
            }

            dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

            Color foreColor = new Color(gdiForeColor.R, gdiForeColor.G, gdiForeColor.B, gdiForeColor.A);
            foreBrush = new Direct2D.SolidColorBrush(d2dContext, foreColor);

            Color backColor = new Color(gdiBackColor.R, gdiBackColor.G, gdiBackColor.B, gdiBackColor.A);
            backBrush = new Direct2D.SolidColorBrush(d2dContext, backColor);

            // int fontSize = (int)(Math.Min(SrcRect.Width, SrcRect.Height) / 8);

            var fontSize = gdiFont.Size;
            var fontFamilyName = gdiFont.FontFamily.Name;
            textFormat = new SharpDX.DirectWrite.TextFormat(dwriteFactory, fontFamilyName, fontSize)
            {
               // TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
                TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
                ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near,
                //FlowDirection = SharpDX.DirectWrite.FlowDirection.TopToBottom,
                WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap,
            };

        }


        public void DrawText(string text, GDI.Point pos, float scaleX = 1f, float scaleY = 1f)
        {
            d2dContext.BeginDraw();      
            //d2dContext.Transform = Matrix3x2.Identity;
            d2dContext.Transform = Matrix3x2.Scaling(scaleX, scaleY);

            //SharpDX.Mathematics.Interop.RawRectangleF rect = new SharpDX.Mathematics.Interop.RawRectangleF
            //{
            //    Left = pos.X,
            //    Top = pos.Y,
            //    Right = 128,
            //    Bottom = 32,
            //};
            //d2dContext.FillRectangle(rect, backBrush);
            //d2dContext.DrawText(text, textFormat, rect, foreBrush);

            int maxWidth = 0;
            int maxHeight = 0;
            using (var textLayout = new SharpDX.DirectWrite.TextLayout(dwriteFactory, text, textFormat, maxWidth, maxHeight))
            {
                var metrics = textLayout.Metrics;

                SharpDX.Mathematics.Interop.RawRectangleF rect = new SharpDX.Mathematics.Interop.RawRectangleF
                {
                    Left = metrics.Left,
                    Top = metrics.Top,
                    Right = metrics.Width - metrics.Left,
                    Bottom = metrics.Height - metrics.Top,
                };
                d2dContext.FillRectangle(rect, backBrush);

                var origin = new Vector2(pos.X, pos.Y);
                d2dContext.DrawTextLayout(origin, textLayout, foreBrush, Direct2D.DrawTextOptions.None);
            }

            d2dContext.EndDraw();
        }

        public void Close()
        {
            DxTool.SafeDispose(d2dContext);
            DxTool.SafeDispose(textFormat);
            DxTool.SafeDispose(foreBrush);
            DxTool.SafeDispose(backBrush);
            DxTool.SafeDispose(dwriteFactory);
            DxTool.SafeDispose(targetBitmap);
        }

    }


}
