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

namespace Test.Encoder.DDATest
{
    public class Renderer
    {


        private SharpDX.Direct3D11.Device device = null;
        private VertexShader vertexShader = null;
        private InputLayout inputLayout = null;
        private PixelShader pixelShader = null;
        private SamplerState samplerLinear;

        private Texture2D renderTexture = null;
        private RenderTargetView renderTargetView = null;
        private Texture2D sourceTexture = null;

        private SwapChain swapChain = null;

        //public int ImageWidth = 1920;
        //public int ImageHeight = 1080;
        public IntPtr hWnd = IntPtr.Zero;
        public int FramePerSec = 60;

        public int adapterIndex = 0;

        private GDI.Size destSize = new GDI.Size(1280, 720);
        private GDI.Size srcSize = new GDI.Size(1280, 720);
        private Format srcFormat = Format.B8G8R8A8_UNorm;

        public void Init(IntPtr _hWnd, GDI.Size _srcSize, GDI.Size _destSize)
        //public void Init(IntPtr hWnd, Texture2D texture)
        {
            this.hWnd = _hWnd;
            this.srcSize = _srcSize;
            this.destSize = _destSize;

            var dxgiFactory = new SharpDX.DXGI.Factory1();
            try
            {
                using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
                {
                    SharpDX.Direct3D.FeatureLevel[] featureLevel =
                    {
                        FeatureLevel.Level_11_1,
                        FeatureLevel.Level_11_0,
                        FeatureLevel.Level_10_1,
                    };

                    var deviceCreationFlags = DeviceCreationFlags.None;
                    //DeviceCreationFlags.Debug |

                    device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);

                    Console.WriteLine($"RendererAdapter {adapterIndex}: " + adapter.Description.Description);

                    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
                }

                var scd = new SwapChainDescription
                {
                    SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
                    SwapEffect = SwapEffect.FlipSequential,
                    ModeDescription = new ModeDescription
                    {
                        Format = srcFormat,
                        Scaling = DisplayModeScaling.Stretched,
                        //Scaling = DisplayModeScaling.Centered,
                        Width = destSize.Width,
                        Height = destSize.Height,
                        RefreshRate = new Rational(FramePerSec, 1),
                    },
                    IsWindowed = true,
                    Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                    Flags = SwapChainFlags.None,
                    BufferCount = 4,
                    OutputHandle = hWnd,
                };

                swapChain = new SwapChain(dxgiFactory, device, scd);
            }
            finally
            {
                dxgiFactory.Dispose();
            }


            InitShaders();

            var sapmplerDescr = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                //Filter = Filter.Anisotropic,
                //MaximumAnisotropy = 16,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                //ComparisonFunction = Comparison.Always,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                MinimumLod = 0,
                MaximumLod = float.MaxValue,

            };

            samplerLinear = new SamplerState(device, sapmplerDescr);


            //using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
            //{
            //    var sharedHandle = sharedRes?.SharedHandle ?? IntPtr.Zero;

            //    if (sharedHandle != IntPtr.Zero)
            //    {
            //        sharedTexture = device.OpenSharedResource<Texture2D>(sharedHandle);
            //        //device.ImmediateContext.Flush();
            //    }
            //}

            sourceTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription
            {
                Format = srcFormat,
                Width = srcSize.Width,
                Height = srcSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                // OptionFlags = ResourceOptionFlags.Shared,
            });


            renderTexture = new Texture2D(device, new Texture2DDescription
            {
                Width = destSize.Width,
                Height = destSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = srcFormat,

                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,

            });

            renderTargetView = new RenderTargetView(device, renderTexture,
                new RenderTargetViewDescription
                {
                    Format = srcFormat,//Format.R8G8B8A8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });


            syncEvent = new AutoResetEvent(false);
        }


        private volatile bool running = false;
        private AutoResetEvent syncEvent = null;
        public void Start()
        {
            if (running)
            {
                return;
            }
         
            var task = Task.Run(() =>
            {
                running = true;
                Stopwatch sw = Stopwatch.StartNew();
                int interval = (int)(1000.0 / FramePerSec);
                
                bool aspectRatio = true;
                
                while (running)
                {
                    try
                    {
                        _Vertex[] vertices = CreateVertices(viewSize, srcSize, aspectRatio);

                        var deviceContext = device.ImmediateContext;

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
                            Width = destSize.Width,
                            Height = destSize.Height,
                            MinDepth = 0f,
                            MaxDepth = 1f,
                            X = 0,
                            Y = 0,
                        });

                        ShaderResourceView shaderResourceView = null;
                        try
                        {
                            shaderResourceView = new ShaderResourceView(device, sharedTexture, new ShaderResourceViewDescription
                            //shaderResourceView = new ShaderResourceView(device, sourceTexture, new ShaderResourceViewDescription
                            {
                                Format = sourceTexture.Description.Format,
                                Dimension = ShaderResourceViewDimension.Texture2D,
                                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                            });

                            deviceContext.OutputMerger.SetTargets(renderTargetView);
                            deviceContext.ClearRenderTargetView(renderTargetView, Color.Black);

                            deviceContext.VertexShader.SetShader(vertexShader, null, 0);
                            deviceContext.PixelShader.SetShader(pixelShader, null, 0);
                            deviceContext.PixelShader.SetShaderResource(0, shaderResourceView);
                            deviceContext.PixelShader.SetSamplers(0, samplerLinear);

                            deviceContext.Draw(vertices.Length, 0);
                            //deviceContext.Flush();
                        }
                        finally
                        {
                            shaderResourceView?.Dispose();
                        }

                        using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                        {
                            deviceContext.CopyResource(renderTexture, backBuffer);
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

                running = false;

            });

        }


        public void Stop()
        {

            running = false;
            syncEvent?.Set();
        }

        private Texture2D sharedTexture = null;
        public void UpdateTexture(Texture2D texture)
        {

            if (sharedTexture != null)
            { //...

            }


            using (var sharedRes = texture.QueryInterface<SharpDX.DXGI.Resource>())
            {
                var sharedHandle = sharedRes?.SharedHandle ?? IntPtr.Zero;

                if (sharedHandle != IntPtr.Zero)
                {
                    sharedTexture = device.OpenSharedResource<Texture2D>(sharedHandle);
                    //using (var sharedTexture = device.OpenSharedResource<Texture2D>(sharedHandle))
                    //{
                    //    device.ImmediateContext.CopyResource(sharedTexture, sourceTexture);
                    //    //syncEvent?.Set();
                    //    //device.ImmediateContext.Flush();
                    //}

                }
            }
        }

        private GDI.Size viewSize = new GDI.Size(1280, 720);
        public void Resize(GDI.Size destSize)
        {
            this.viewSize = destSize;

        }

        public void Close()
        {
            if (sharedTexture != null && !sharedTexture.IsDisposed)
            {
                sharedTexture.Dispose();
                sharedTexture = null;

            }
            if (renderTargetView!=null && !renderTargetView.IsDisposed)
            {
                renderTargetView.Dispose();
                renderTargetView = null;
            }

            if (renderTexture != null && !renderTexture.IsDisposed)
            {
                renderTexture.Dispose();
                renderTexture = null;
            }

            if (sourceTexture != null && !sourceTexture.IsDisposed)
            {
                sourceTexture.Dispose();
                sourceTexture = null;
            }

            if (swapChain != null && !swapChain.IsDisposed)
            {
                swapChain.Dispose();
                swapChain = null;
            }

            if (samplerLinear != null && !samplerLinear.IsDisposed)
            {
                samplerLinear.Dispose();
                samplerLinear = null;
            }

            if (inputLayout != null && !inputLayout.IsDisposed)
            {
                inputLayout.Dispose();
                inputLayout = null;
            }

            if (pixelShader != null && !pixelShader.IsDisposed)
            {
                pixelShader.Dispose();
                pixelShader = null;
            }

            if (vertexShader != null && !vertexShader.IsDisposed)
            {
                vertexShader.Dispose();
                vertexShader = null;
            }

            if (device != null && !device.IsDisposed)
            {
                device.Dispose();
                device = null;
            }


            syncEvent?.Dispose();
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

            var vsFile = Path.Combine(shaderPath, "VertexShader.hlsl");
            using (var compResult = CompileShaderFromFile(vsFile, "VS", vsProvile))
            {
                vertexShader = new VertexShader(device, compResult.Bytecode);
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

            var psFile = Path.Combine(shaderPath, "PixelShader.hlsl");
            using (var compResult = CompileShaderFromFile(psFile, "PS", psProvile))
            {
                pixelShader = new PixelShader(device, compResult.Bytecode);
            }
        }


        private static SharpDX.D3DCompiler.CompilationResult CompileShaderFromFile(string file, string entryPoint, string profile)
        {

            Console.WriteLine("CompileShaderFromFile() " + string.Join(" ", file, entryPoint, profile));

            SharpDX.D3DCompiler.ShaderFlags shaderFlags =
                SharpDX.D3DCompiler.ShaderFlags.EnableStrictness
                | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
            //| SharpDX.D3DCompiler.ShaderFlags.Debug;

            SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None;

            return SharpDX.D3DCompiler.ShaderBytecode.CompileFromFile(file, entryPoint, profile, shaderFlags, effectFlags);
        }

    }
}
