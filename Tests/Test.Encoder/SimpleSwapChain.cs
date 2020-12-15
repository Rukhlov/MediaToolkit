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

namespace Test.Encoder
{
    public class SimpleSwapChain
    {

        public static void Run()
        {
            Shcore.SetProcessPerMonitorDpiAwareness();

            new SimpleSwapChain().Start();
        }


        private SharpDX.Direct3D11.Device device = null;
        private VertexShader defaultVertexShader = null;
        private InputLayout inputLayout = null;
        private PixelShader defaultPixelShader = null;
        private SamplerState sampler;

        private Texture2D sharedTexture = null;


        private SwapChain swapChain = null;

        public int ImageWidth = 1920;
        public int ImageHeight = 1080;
        public IntPtr ViewHandle = IntPtr.Zero;
        public int FramePerSec = 60;

        public int adapterIndex = 0;


        private string psName = "";
        public void Start()
        {
            Console.WriteLine("SimpleSwapChain::Start()");

            //var fileName = @"Files\1920x1080.bmp";
            // var fileName = @"D:\Dropbox\Public\1681_source.jpg";
            //var fileName = @"D:\Dropbox\Public\2.png";

            var fileName = @"Files\2560x1440.bmp";
            // var fileName = @"Files\rgba_352x288.bmp";
            //var fileName = @"Files\Screen0_2560x1440.bmp";
            //var destSize = new GDI.Size(100, 100);
            //var destSize = new GDI.Size(ImageWidth, ImageHeight);

            var destSize = new GDI.Size(640, 360);
			//var destSize = new GDI.Size(852, 480);
			// var destSize = new GDI.Size(1280, 720);

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

                        _Vertex[] vertices = CreateVertices(f.ClientSize, new GDI.Size(ImageWidth, ImageHeight), aspectRatio);

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

                        //renderTarget.BeginDraw();
                        //renderTarget.Clear(Color.Black);//(Color.Red);
                        //DrawScreen(renderTarget, sourceTexture0, destSize, aspectRatio);
                        //renderTarget.EndDraw();

                        using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                        {
                            deviceContext.CopyResource(sharedTexture, backBuffer);
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




        private static _Vertex[] CreateVertices(GDI.Size srcSize, GDI.Size targetSize, bool aspectRatio = true)
        {
            float x1 = -1f;
            float y1 = -1f;
            float x2 = -1f;
            float y2 = 1f;
            float x3 = 1f;
            float y3 = -1f;
            float x4 = 1f;
            float y4 = 1f;

            if (aspectRatio)
            {
                double targetWidth = targetSize.Width;
                double targetHeight = targetSize.Height;
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

            return new _Vertex[]
            {
                new _Vertex(new Vector3(x1, y1, 0f), new Vector2(0f, 1f)),
                new _Vertex(new Vector3(x2, y2, 0f), new Vector2(0f, 0f)),
                new _Vertex(new Vector3(x3, y3, 0f), new Vector2(1f, 1f)),
                new _Vertex(new Vector3(x4, y4, 0f), new Vector2(1f, 0f)),
            };


        }

        public struct _Vertex
        {
            public _Vertex(Vector3 pos, Vector2 tex)
            {
                this.Position = pos;
                this.TextureCoord = tex;
            }
            public Vector3 Position;
            public Vector2 TextureCoord;
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
    }
}
