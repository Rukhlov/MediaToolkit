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
    public class SimpleSwapChain
    {

        public static void Run()
        {
            Shcore.SetProcessPerMonitorDpiAwareness();

            new SimpleSwapChain().Start();
        }

		private SharpDX.Direct2D1.DeviceContext d2dContext = null;
		private SharpDX.DirectWrite.TextFormat textFormat = null;
		private Direct2D.SolidColorBrush sceneColorBrush = null;

		private SharpDX.Direct3D11.Device device = null;
        private VertexShader defaultVertexShader = null;
        private InputLayout inputLayout = null;
        private PixelShader defaultPixelShader = null;
        private SamplerState sampler;

        private Texture2D sharedTexture = null;


        private SwapChain swapChain = null;

        //public int ImageWidth = 1920;
        //public int ImageHeight = 1080;
        public IntPtr ViewHandle = IntPtr.Zero;
        public int FramePerSec = 60;

        public int adapterIndex = 0;

        public int ImageWidth = 1080;
        public int ImageHeight = 1920;

        private string psName = "";
        public void Start()
        {
            Console.WriteLine("SimpleSwapChain::Start()");

            //var fileName = @"Files\1920x1080.bmp";
            // var fileName = @"D:\Dropbox\Public\1681_source.jpg";
            //var fileName = @"D:\Dropbox\Public\2.png";

            var fileName = @"Files\1080x1920.bmp";

            //var fileName = @"Files\2560x1440.bmp";
            // var fileName = @"Files\rgba_352x288.bmp";
            //var fileName = @"Files\Screen0_2560x1440.bmp";
            //var destSize = new GDI.Size(100, 100);
            //var destSize = new GDI.Size(ImageWidth, ImageHeight);

            //var destSize = new GDI.Size(1920, 1080);
			//var destSize = new GDI.Size(852, 480);
			 var destSize = new GDI.Size(1280, 720);

			//var destSize = new GDI.Size(2560, 1440);
			//var destSize = new GDI.Size(1920, 1080);

			//psName = "BiCubicPS.hlsl";
			psName = "DownscaleBilinear9.hlsl";
            //psName = "DownscaleBicubic.hlsl";
           // psName = "DownscaleLanczos6tap.hlsl";
            //psName = "BiCubicScale.hlsl";
            //psName = "BiLinearScaling.hlsl";



            var dxgiFactory = new SharpDX.DXGI.Factory1();
            var adapter = dxgiFactory.GetAdapter1(adapterIndex);

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
             };



            //var rect = Screen.PrimaryScreen.Bounds;
            //using (GDI.Bitmap bmp = new GDI.Bitmap(rect.Width, rect.Height))
            //{
            //    using (GDI.Graphics g = GDI.Graphics.FromImage(bmp))
            //    {
            //        g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size, GDI.CopyPixelOperation.SourceCopy);
            //    }
            //    fileName = @"Files\Screen0_2560x1440.bmp";
            //    bmp.Save(fileName, GDI.Imaging.ImageFormat.Bmp);
            //}




            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            //DeviceCreationFlags.Debug |
            //DeviceCreationFlags.VideoSupport |
            //DeviceCreationFlags.BgraSupport;

            device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);

            Console.WriteLine($"RendererAdapter {adapterIndex}: " + adapter.Description.Description);

            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }


			InitShaders();



            //var bmp = new System.Drawing.Bitmap(fileName);
            //ImageWidth = bmp.Width;
            //ImageHeight = bmp.Height;
            //if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            //{
            //    var rect = new GDI.Rectangle(0, 0, bmp.Width, bmp.Height);
            //    var _bmp = bmp.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);
            //    bmp.Dispose();
            //    bmp = _bmp;
            //}
            ////var sourceTexture0 = Program.GetDynamicRgbaTextureFromBitmap(bmp, device);
            //bmp.Dispose();

            var sourceTexture0 = WicTool.CreateTexture2DFromBitmapFile(fileName, device);

            var srcDescr = sourceTexture0.Description;

            sharedTexture = new Texture2D(device, new Texture2DDescription
            {
                //Width = srcDescr.Width,
                //Height = srcDescr.Height,
                Width = destSize.Width,
                Height = destSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = srcDescr.Format,//Format.R8G8B8A8_UNorm,
                                         // Format = Format,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,

            });

            var renderTargetView = new RenderTargetView(device, sharedTexture,
                new RenderTargetViewDescription
                {
                    Format = srcDescr.Format,//Format.R8G8B8A8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });

            var title = fileName + " >> " + destSize.Width + "x" + destSize.Height + "_" + srcDescr.Format + "_" + psName;
            Form f = new Form
            {
                //Width = destSize.Width,
                //Height = destSize.Height,
                Text = title,
            };

            f.ClientSize = destSize;


            ViewHandle = f.Handle;

            ImageWidth = srcDescr.Width;
            ImageHeight = srcDescr.Height;

            var scd = new SwapChainDescription
            {
                SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
                SwapEffect = SwapEffect.FlipSequential,
                ModeDescription = new ModeDescription
                {
                    Format = srcDescr.Format,
                    //Format = Format.B8G8R8A8_UNorm,
                    //Format = Format.B8G8R8A8_UNorm,
                    Scaling = DisplayModeScaling.Stretched,
                    //Scaling = DisplayModeScaling.Centered,
                    Width = destSize.Width,//ImageWidth,
                    Height = destSize.Height,//ImageHeight,
                    RefreshRate = new Rational(FramePerSec, 1),

                },
                IsWindowed = true,
                Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                Flags = SwapChainFlags.None,
                BufferCount = 4,

                OutputHandle = ViewHandle,
            };

            swapChain = new SwapChain(dxgiFactory, device, scd);


			using (var d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.SingleThreaded, SharpDX.Direct2D1.DebugLevel.Information))
			{
				// Create Direct2D device
				using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
				{
					using (var d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice))
					{
						// Create Direct2D context
						d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);

						var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
							new SharpDX.Direct2D1.PixelFormat(swapChain.Description.ModeDescription.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
							96, 96,
							SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);

						// Direct2D needs the dxgi version of the backbuffer surface pointer.
						// Get a D2D surface from the DXGI back buffer to use as the D2D render target.
						//using (var dxgiBackBuffer = swapChain.GetBackBuffer<SharpDX.DXGI.Surface>(0))
						//{
						//	d2dContext.Target = new SharpDX.Direct2D1.Bitmap1(d2dContext, dxgiBackBuffer, bitmapProperties);
						//}

						using (var surf = sharedTexture.QueryInterface<Surface>())
						{
							d2dContext.Target = new SharpDX.Direct2D1.Bitmap1(d2dContext, surf, bitmapProperties);
						}

						d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
						d2dContext.DotsPerInch = new SharpDX.Size2F(96, 96);
					}

				}

			}

			using (var dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared))
			{
				sceneColorBrush = new Direct2D.SolidColorBrush(d2dContext, Color.White);

				textFormat = new SharpDX.DirectWrite.TextFormat(dwriteFactory, "Calibri", 16)
				{
					TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
					ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center
				};
			}


			adapter.Dispose();
            dxgiFactory.Dispose();

            //device.ImmediateContext.CopyResource(sourceTexture0, sharedTexture);
            //device.ImmediateContext.Flush();

            //Direct2D.RenderTarget renderTarget = null;
            //using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded))
            //{
            //    using (var surf = sharedTexture.QueryInterface<Surface>())
            //    {
            //        //var pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore);

            //        var pixelFormat = new Direct2D.PixelFormat(srcDescr.Format, Direct2D.AlphaMode.Premultiplied);
            //        var renderTargetProps = new Direct2D.RenderTargetProperties(pixelFormat);

            //        renderTarget = new Direct2D.RenderTarget(factory2D1, surf, renderTargetProps);

            //        //var d2dContext = new SharpDX.Direct2D1.DeviceContext(surface);
            //    }

            //}

            bool running = true;
            var task = Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                int interval = (int)(1000.0 / FramePerSec);
                AutoResetEvent syncEvent = new AutoResetEvent(false);
                bool aspectRatio = true;

                /*
				 * Для варианта Точечная фильтрация текстур определенный приложением режим фильтрации заменяется на D3D11_FILTER_MIN_MAG_MIP_POINT,
				 * для варианта Билинейная фильтрация текстур он заменяется на D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT, 
				 * а для варианта Трилинейная фильтрация текстур он заменяется на D3D11_FILTER_MIN_MAG_MIP_LINEAR.

					Для варианта Анизотропная фильтрация текстур определенный приложением режим фильтрации заменяется на D3D11_FILTER_ANISOTROPIC, 
					а свойству "Максимальная анизотропия" присваивается значение 16.
				 */
                sampler = new SamplerState(device, new SamplerStateDescription
                {

                    //Filter = Filter.MinMagMipPoint,
                    //Filter = Filter.MinMagLinearMipPoint,
                    Filter = Filter.MinMagMipLinear,
                    //Filter = Filter.Anisotropic,
                    MaximumAnisotropy = 16,

                    //AddressU = TextureAddressMode.Wrap,
                    //AddressV = TextureAddressMode.Wrap,
                    //AddressW = TextureAddressMode.Wrap,

                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    //ComparisonFunction = Comparison.Never,
                    //BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                    //MinimumLod = 0,
                    //MaximumLod = float.MaxValue,
                });

                var deviceContext = device.ImmediateContext;
                deviceContext.PixelShader.SetSamplers(0, sampler);


                BufferDescription bufferDescription = new BufferDescription
                {
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ConstantBuffer,
                    SizeInBytes = 16,
                };

                //var baseDimensionI = new Vector2(1f / (1f * destSize.Width), 1f / (1f * destSize.Height));
                var baseDimensionI = new Vector2(1f / (3f * destSize.Width), 1f / (3f * destSize.Height));
                //var baseDimensionI = new Vector2(1f / (3f * srcDescr.Width), 1f / (3f * srcDescr.Height ));
                using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, ref baseDimensionI, bufferDescription))
                {
                    deviceContext.PixelShader.SetConstantBuffer(0, buffer);
                }
                //deviceContext.PixelShader.SetShader(downscalePixelShader, null, 0);
                //deviceContext.VertexShader.SetShader(defaultVertexShader, null, 0);
                 int count = 1;
                while (running)
                {
                    try
                    {
						var targetSize = new GDI.Size(ImageWidth, ImageHeight);
						var viewSize = f.ClientSize;
						aspectRatio = true;
						Transform transform = Transform.R90;
						_Vertex[] vertices = VertexHelper.GetQuadVertices(viewSize, targetSize, aspectRatio, transform);

						//_Vertex[] vertices = CreateVertices(f.ClientSize, new GDI.Size(ImageWidth, ImageHeight), aspectRatio);

                        using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices))
                        {
                            VertexBufferBinding vertexBuffer = new VertexBufferBinding
                            {
                                Buffer = buffer,
                                Stride = Utilities.SizeOf<_Vertex>(),
                                Offset = 0,
                            };
                            deviceContext.InputAssembler.SetVertexBuffers(0, vertexBuffer);
                        }


                        deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

                        deviceContext.Rasterizer.SetViewport(new SharpDX.Mathematics.Interop.RawViewportF
                        {
                            Width = destSize.Width,//ImageWidth,
                            Height = destSize.Height,//ImageHeight,
                            MinDepth = 0f,
                            MaxDepth = 1f,
                            X = 0,
                            Y = 0,

                        });


                        ShaderResourceView shaderResourceView = null;
                        try
                        {
                            shaderResourceView = new ShaderResourceView(device, sourceTexture0, new ShaderResourceViewDescription
                            {
                                Format = sourceTexture0.Description.Format,
                                Dimension = ShaderResourceViewDimension.Texture2D,
                                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                            });

                            deviceContext.OutputMerger.SetTargets(renderTargetView);
                            deviceContext.ClearRenderTargetView(renderTargetView, Color.Blue);

							//float degrees = -90;
							//float angle = (float)(Math.PI * degrees / 180.0);

							////var viewProj = Matrix.Identity;
							//var viewProj = Matrix.RotationZ(angle);
							//using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref viewProj))
							//{
							//	deviceContext.VertexShader.SetConstantBuffer(0, buffer);
							//}

							deviceContext.VertexShader.SetShader(defaultVertexShader, null, 0);
                            //deviceContext.PixelShader.SetShader(defaultPixelShader, null, 0);

                            deviceContext.PixelShader.SetShader(downscalePixelShader, null, 0);

                            deviceContext.PixelShader.SetShaderResource(0, shaderResourceView);

                            deviceContext.Draw(vertices.Length, 0);
                            //deviceContext.Flush();
                        }
                        finally
                        {
                            shaderResourceView?.Dispose();
                        }

						int msec = (int)sw.ElapsedMilliseconds;
						int delay = interval - msec;
						if (delay <= 0)
						{
							delay = 1;
						}

						syncEvent.WaitOne(delay);

						frameCount++;
						var averageTick = CalcAverageTick(sw.ElapsedTicks) / Stopwatch.Frequency;
						var text = string.Format("{0:F2} FPS ({1:F1} ms)", 1.0 / averageTick, averageTick * 1000.0);


						d2dContext.BeginDraw();
						d2dContext.Transform = Matrix3x2.Identity;
						var lineLength = 256;
						var location = new Point(8, 8);
						var rect = new RectangleF(location.X, location.Y, location.X + lineLength, location.Y + 16);
						d2dContext.DrawText(text, textFormat, rect, sceneColorBrush);
						d2dContext.EndDraw();



						using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                        {
                            deviceContext.CopyResource(sharedTexture, backBuffer);
                            swapChain.Present(1, PresentFlags.None);
                        }




						sw.Restart();
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.Message + " " + device.DeviceRemovedReason);
                    }

                }
                syncEvent.Dispose();


                //var descr = sharedTexture.Description;
                //var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, sharedTexture);
                //var _fileName = "!!!!NV12_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
                //File.WriteAllBytes(_fileName, bytes);
                //Console.WriteLine("OutputFile: " + _fileName);

                //var ptr = Marshal.AllocHGlobal(bytes.Length);
                //Marshal.Copy(bytes, 0, ptr, bytes.Length);
                //GDI.Bitmap bmp = new GDI.Bitmap(descr.Width, descr.Height, descr.Width, GDI.Imaging.PixelFormat.Format32bppRgb, ptr);
                //var _bmpfileName = "!!!!MinMagMipPoint_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".bmp";
                //bmp.Save(_bmpfileName);
                //Marshal.FreeHGlobal(ptr);



            });




            Application.Run(f);

            running = false;

            task.Wait();

            swapChain?.Dispose();
            sampler?.Dispose();
            inputLayout?.Dispose();
            defaultPixelShader?.Dispose();
            defaultVertexShader?.Dispose();
            downscalePixelShader?.Dispose();

            renderTargetView?.Dispose();
            sharedTexture?.Dispose();
            sourceTexture0?.Dispose();
            device?.Dispose();

        }

		private static _Vertex[] _CreateVertices(GDI.Size srcSize, GDI.Size targetSize, bool aspectRatio = true)
        {

			/*
			 * 1--3
			 * |  |
			 * 0--2
			 */

			/* TEXCOORD
			 * 0,0--1,0
			 * |	 |
			 * 0,1--1,1
			 */

			//float u1 = 0;
			//float v1 = 1f;
			//float u2 = 0;
			//float v2 = 0;
			//float u3 = 1;
			//float v3 = 1;
			//float u4 = 1;
			//float v4 = 0;

			float u1 = -1f;
			float v1 = -1f;
			float u2 = -1f;
			float v2 = 1f;
			float u3 = 1f;
			float v3 = -1f;
			float u4 = 1f;
			float v4 = 1f;


			/* POSITION
			 * -1, 1 -- 1, 1
			 *  |	    |
			 * -1,-1 -- 1,-1
			 */
			//0
			float x1 = -1f;
			float y1 = -1f;
			float x2 = -1f;
			float y2 = 1f;
			float x3 = 1f;
			float y3 = -1f;
			float x4 = 1f;
			float y4 = 1f;


			//float x4 = -1f;
			//float y4 = -1f;
			//float x3 = -1f;
			//float y3 = 1f;
			//float x2 = 1f;
			//float y2 = -1f;
			//float x1 = 1f;
			//float y1 = 1f;


			//90
			//float x1 = -1f;
			//float y1 = 1f;
			//float x2 = 1f;
			//float y2 = 1f;
			//float x3 = -1f;
			//float y3 = -1f;
			//float x4 = 1f;
			//float y4 = -1f;

			////180
			//float x1 = 1f;
			//float y1 = 1f;
			//float x2 = 1f;
			//float y2 = -1f;
			//float x3 = -1f;
			//float y3 = 1f;
			//float x4 = -1f;
			//float y4 = -1f;

			////270
			//float x1 = 1f;
			//float y1 = -1f;
			//float x2 = -1f;
			//float y2 = -1f;
			//float x3 = 1f;
			//float y3 = 1f;
			//float x4 = -1f;
			//float y4 = 1f;


			//
			//float x1 = -1f;
			//float y1 = 1f;
			//float x2 = -1f;
			//float y2 = -1f;
			//float x3 = 1f;
			//float y3 = 1f;
			//float x4 = 1f;
			//float y4 = -1f;

			float rotateDegrees = 90;

			if (aspectRatio)
            {
                double targetWidth = targetSize.Width;
                double targetHeight = targetSize.Height;

				if (rotateDegrees == 90 || rotateDegrees == 270)
				{
					targetWidth = targetSize.Height;
					targetHeight = targetSize.Width;
				}

				double srcWidth = srcSize.Width;
                double srcHeight = srcSize.Height;

                double targetRatio = targetWidth / targetHeight;
                double containerRatio = srcWidth / srcHeight;

                // в координатах формы
                double viewTop = 0;
                double viewLeft = 0;
                double viewWidth = srcWidth;
                double viewHeight = srcHeight;

                if (containerRatio < targetRatio)
                {
                    viewWidth = srcWidth;
                    viewHeight = (viewWidth / targetRatio);
                    viewTop = (srcHeight - viewHeight) / 2;
                    viewLeft = 0;
                }
                else
                {
                    viewHeight = srcHeight;
                    viewWidth = viewHeight * targetRatio;
                    viewTop = 0;
                    viewLeft = (srcWidth - viewWidth) / 2;
                }

                // в левых координатах 0f, 2f
                var normX = 2.0 / srcWidth;
                var normY = 2.0 / srcHeight;
                var left = viewLeft * normX;
                var top = viewTop * normY;
                var width = viewWidth * normX;
                var height = viewHeight * normY;

                // в правых координатах -1f, 1f 
                // сдвигаем на -1,-1 и инвертируем Y 
                x1 = (float)(left - 1);
                y1 = (float)(-((height + top) - 1));
                x2 = x1;
                y2 = (float)(-(top - 1));
                x3 = (float)((width + left) - 1);
                y3 = y1;
                x4 = x3;
                y4 = (float)(-(top - 1));
            }

			float angle = (float)(Math.PI * rotateDegrees / 180.0);

			//Matrix.LookAtLH()
			var viewProj = Matrix.Identity;
			//var viewProj = Matrix.RotationZ(angle);
			//var viewProj = Matrix.RotationX(angle);

			//var viewProj = Matrix.Scaling(-1, -1, -1);
			//var viewProj = Matrix.Transformation(new Vector3(0.5f, 0.5f, 0), new Quaternion(0), new Vector3(0.1f, -0.1f, 0), new Vector3(0, 0, 0), new Quaternion(0), new Vector3(0,0, 0));
			var vertex = new Matrix(x1, y1, 0f, 1f, x2, y2, 0f, 1f, x3, y3, 0f, 1f, x4, y4, 0f, 1f);

			var result = Matrix.Multiply(vertex, viewProj);
			
			x1 = result.M11;
			y1 = result.M12;
			x2 = result.M21;
			y2 = result.M22;
			x3 = result.M31;
			y3 = result.M32;
			x4 = result.M41;
			y4 = result.M42;


			var coordMatrix = new Matrix(u1, v1, 0f, 0f, u2, v2, 0f, 0f, u3, v3, 0f, 0f, u4, v4, 0f, 0f);
			var _degrees = rotateDegrees;
			float _angle = (float)(Math.PI * _degrees / 180.0);

			//var proj = Matrix.Transformation2D(new Vector2(0, 0), 1, new Vector2(1, 1), new Vector2(0.5f, 0.5f), 0f, new Vector2(0, 0));
			////Matrix.LookAtLH()
			var proj = Matrix.RotationZ(_angle);

			var _result = Matrix.Multiply(coordMatrix, proj);
			//_result = Matrix.Multiply(_result, trans);

			u1 = _result.M11 /2.0f + 0.5f;
			v1 = -(_result.M12 / 2.0f - 0.5f);
			u2 = _result.M21 / 2.0f + 0.5f;
			v2 = -(_result.M22 / 2.0f - 0.5f);
			u3 = _result.M31 / 2.0f + 0.5f;
			v3 = -(_result.M32 / 2.0f - 0.5f);
			u4 = _result.M41 / 2.0f + 0.5f;
			v4 = -(_result.M42 / 2.0f - 0.5f);

			//u1 = _result.M11 + 1;
			//v1 = _result.M12 + 1;
			//u2 = _result.M21 + 1;
			//v2 = _result.M22 + 1;
			//u3 = _result.M31 + 1;
			//v3 = _result.M32 + 1;
			//u4 = _result.M41 + 1;
			//v4 = _result.M42 + 1;
			//// 180
			//u1 = 1;
			//v1 = 0;
			//u2 = 1;
			//v2 = 1;
			//u3 = 0;
			//v3 = 0;
			//u4 = 0;
			//v4 = 1;

			//u1 = _result.M11;
			//v1 = _result.M12+1;
			//u2 = _result.M21;
			//v2 = _result.M22+1;
			//u3 = _result.M31;
			//v3 = _result.M32 + 1;
			//u4 = _result.M41;
			//v4 = _result.M42 + 1;
			//// 270
			//u1 = 1;
			//v1 = 1;
			//u2 = 0;
			//v2 = 1;
			//u3 = 1;
			//v3 = 0;
			//u4 = 0;
			//v4 = 0;

			//u1 = _result.M11 + 1;
			//v1 = _result.M12;
			//u2 = _result.M21 + 1;
			//v2 = _result.M22;
			//u3 = _result.M31 + 1;
			//v3 = _result.M32;
			//u4 = _result.M41 + 1;
			//v4 = _result.M42;

			//// 90
			//u1 = 0;
			//v1 = 0;
			//u2 = 1;
			//v2 = 0;
			//u3 = 0;
			//v3 = 1;
			//u4 = 1;
			//v4 = 1;

			//// flipX
			//u1 = 0;
			//v1 = 0;
			//u2 = 0;
			//v2 = 1f;
			//u3 = 1f;
			//v3 = 0;
			//u4 = 1f;
			//v4 = 1f;

			//// flipY
			//u1 = 1;
			//v1 = 1;
			//u2 = 1;
			//v2 = 0;
			//u3 = 0;
			//v3 = 1;
			//u4 = 0;
			//v4 = 0;



			//// 180
			//u1 = 1;
			//v1 = 0;
			//u2 = 1;
			//v2 = 1;
			//u3 = 0;
			//v3 = 0;
			//u4 = 0;
			//v4 = 1;



			return new _Vertex[]
			{

				//new _Vertex(new Vector3(x1, y1, 0f), new Vector2(0f, 1f)),
				//new _Vertex(new Vector3(x2, y2, 0f), new Vector2(1f, 1f)),
				//new _Vertex(new Vector3(x3, y3, 0f), new Vector2(0f, 0f)),
				//new _Vertex(new Vector3(x4, y4, 0f), new Vector2(1f, 0f)),

				new _Vertex(new Vector3(x1, y1, 0f), new Vector2(u1, v1)),
				new _Vertex(new Vector3(x2, y2, 0f), new Vector2(u2, v2)),
				new _Vertex(new Vector3(x3, y3, 0f), new Vector2(u3, v3)),
				new _Vertex(new Vector3(x4, y4, 0f), new Vector2(u4, v4)),


				//flipX
				//new _Vertex(new Vector3(x1, y1, 0f), new Vector2(1f, 1f)),
				//new _Vertex(new Vector3(x2, y2, 0f), new Vector2(1f, 0f)),
				//new _Vertex(new Vector3(x3, y3, 0f), new Vector2(0f, 1f)),
				//new _Vertex(new Vector3(x4, y4, 0f), new Vector2(0f, 0f)),


				//flipY
				//new _Vertex(new Vector3(x1, y1, 0f), new Vector2(0f, 0f)),
				//new _Vertex(new Vector3(x2, y2, 0f), new Vector2(0f, 1f)),
				//new _Vertex(new Vector3(x3, y3, 0f), new Vector2(1f, 0f)),
				//new _Vertex(new Vector3(x4, y4, 0f), new Vector2(1f, 1f)),

			};




		}



        private void InitShaders()
        {
            Console.WriteLine("SimpleSwapChain::InitShaders()");

            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var shaderPath = Path.Combine(appPath, "Shaders");

            var profileLevel = "4_0";
            //var profileLevel = "5_0";
            var vsProvile = "vs_" + profileLevel;
            var psProvile = "ps_" + profileLevel;

            var vsFile = Path.Combine(shaderPath, "DefaultVS.hlsl");
            using (var compResult = CompileShaderFromFile(vsFile, "VS", vsProvile))
            {
                defaultVertexShader = new VertexShader(device, compResult.Bytecode);
                var elements = new[]
                {
                    new InputElement("POSITION",0,Format.R32G32B32_Float,0,0),
                    new InputElement("TEXCOORD",0,Format.R32G32_Float,12,0)
                };

                using (var inputLayout = new InputLayout(device, compResult.Bytecode, elements))
                {
                    device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
                }
            }


            var psFile = Path.Combine(shaderPath, "DefaultPS.hlsl");
            using (var compResult = CompileShaderFromFile(psFile, "PS", psProvile))
            {
                defaultPixelShader = new PixelShader(device, compResult.Bytecode);
            }


            psFile = Path.Combine(shaderPath, psName);
            using (var compResult = CompileShaderFromFile(psFile, "main", psProvile))
            {
                downscalePixelShader = new PixelShader(device, compResult.Bytecode);
            }
        }


        private PixelShader downscalePixelShader = null;



        private static SharpDX.D3DCompiler.CompilationResult CompileShaderFromFile(string file, string entryPoint, string profile)
        {

            Console.WriteLine("CompileShaderFromFile() " + string.Join(" ", file, entryPoint, profile));

            SharpDX.D3DCompiler.ShaderFlags shaderFlags =
                SharpDX.D3DCompiler.ShaderFlags.EnableStrictness
                | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization
				| SharpDX.D3DCompiler.ShaderFlags.Debug;

            SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None;

            return SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(file, entryPoint, profile, shaderFlags, effectFlags);
        }


        private static void DrawScreen(SharpDX.Direct2D1.RenderTarget renderTarget, Texture2D texture, GDI.Size DestSize, bool AspectRatio = true)
        {
            using (var surf = texture.QueryInterface<Surface1>())
            {
                var descr = texture.Description;

                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(descr.Format, Direct2D.AlphaMode.Premultiplied));
                Direct2D.Bitmap screenBits = new Direct2D.Bitmap(renderTarget, surf, prop);
                try
                {
                    var srcDecr = surf.Description;
                    float srcWidth = srcDecr.Width;
                    float srcHeight = srcDecr.Height;

                    float destX = 0;
                    float destY = 0;
                    float destWidth = DestSize.Width;
                    float destHeight = DestSize.Height;

                    float scaleX = destWidth / srcWidth;
                    float scaleY = destHeight / srcHeight;

                    if (AspectRatio)
                    {
                        if (scaleY < scaleX)
                        {
                            scaleX = scaleY;
                            destX = ((destWidth - srcWidth * scaleX) / 2);
                        }
                        else
                        {
                            scaleY = scaleX;
                            destY = ((destHeight - srcHeight * scaleY) / 2);
                        }
                    }

                    destWidth = srcWidth * scaleX;
                    destHeight = srcHeight * scaleY;

                    var destRect = new SharpDX.Mathematics.Interop.RawRectangleF
                    {
                        Left = destX,
                        Right = destX + destWidth,
                        Top = destY,
                        Bottom = destY + destHeight,
                    };

                    renderTarget.DrawBitmap(screenBits, destRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);

                }
                finally
                {
                    screenBits?.Dispose();
                }
            }
        }

		//class FpsCalc
		//{
			const int MAXSAMPLES = 100;
			int tickindex = 0;
			long ticksum = 0;
			long[] ticklist = new long[MAXSAMPLES];
			long frameCount;

			/* need to zero out the ticklist array before starting */
			/* average will ramp up until the buffer is full */
			/* returns average ticks per frame over the MAXSAMPPLES last frames */
			//http://stackoverflow.com/questions/87304/calculating-frames-per-second-in-a-game/87732#87732
			double CalcAverageTick(long newtick)
			{
				ticksum -= ticklist[tickindex];  /* subtract value falling off */
				ticksum += newtick;              /* add new value */
				ticklist[tickindex] = newtick;   /* save new value so it can be subtracted later */
				if (++tickindex == MAXSAMPLES)    /* inc buffer index */
					tickindex = 0;

				/* return average */
				if (frameCount < MAXSAMPLES)
				{
					return (double)ticksum / frameCount;
				}
				else
				{
					return (double)ticksum / MAXSAMPLES;
				}
			}
		//}
    }
}
