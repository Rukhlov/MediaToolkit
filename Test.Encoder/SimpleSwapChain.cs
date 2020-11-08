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

namespace Test.Encoder
{
    public class SimpleSwapChain
    {

        private SharpDX.Direct3D11.Device device =null;
        private VertexShader vertexShader = null;
        private InputLayout inputLayout = null;
        private PixelShader pixelShader = null;
        private SamplerState samplerLinear;

        private Texture2D sharedTexture = null;

        public void Init()
        {
            Console.WriteLine("Init()");


            var fileName = @"Files\1920x1080.bmp";

            int BufferWidth = 1920;
            int BufferHeight = 1080;
            IntPtr ViewHandle = IntPtr.Zero;
            int FramePerSec = 60;

            int adapterIndex = 0;
            var dxgiFactory = new SharpDX.DXGI.Factory1();
            var adapter = dxgiFactory.GetAdapter1(adapterIndex);

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
                };


            var deviceCreationFlags = DeviceCreationFlags.None;
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


            SamplerStateDescription defaultDescr = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                MinimumLod = 0,
                MaximumLod = float.MaxValue,
            };

            samplerLinear = new SamplerState(device, defaultDescr);

            var bmp = new System.Drawing.Bitmap(fileName);
            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                var rect = new GDI.Rectangle(0, 0, bmp.Width, bmp.Height);
                var _bmp = bmp.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);
                bmp.Dispose();
                bmp = _bmp;
            }
            var sourceTexture0 = Program.GetTexture(bmp, device);



            bmp.Dispose();

            var srcDescr = sourceTexture0.Description;

            sharedTexture = new Texture2D(device, new Texture2DDescription
            {
                Width = srcDescr.Width,
                Height = srcDescr.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = srcDescr.Format,//Format.R8G8B8A8_UNorm,

                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,

            });

            RenderTargetViewDescription d = new RenderTargetViewDescription
            {
                Format = srcDescr.Format,//Format.R8G8B8A8_UNorm,
                Dimension = RenderTargetViewDimension.Texture2D,
                Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
            };

            var renderTargetView = new RenderTargetView(device, sharedTexture, d);



            Form f = new Form
            {
                Width = 640,
                Height = 480,
                Text = fileName,
            };

            ViewHandle = f.Handle;

            BufferWidth = sourceTexture0.Description.Width;
            BufferHeight = sourceTexture0.Description.Height;

            var scd = new SwapChainDescription
            {
                SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
                SwapEffect = SwapEffect.FlipSequential,
                ModeDescription = new ModeDescription
                {
                    Format = Format.B8G8R8A8_UNorm,
                    Scaling = DisplayModeScaling.Stretched,
                    //Scaling = DisplayModeScaling.Centered,
                    Width = BufferWidth,
                    Height = BufferHeight,
                    RefreshRate = new Rational(FramePerSec, 1),

                },
                IsWindowed = true,
                Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                Flags = SwapChainFlags.None,
                BufferCount = 4,

                OutputHandle = ViewHandle,

            };

            var swapChain = new SwapChain(dxgiFactory, device, scd);

            adapter.Dispose();
            dxgiFactory.Dispose();

			//device.ImmediateContext.CopyResource(sourceTexture0, sharedTexture);
			//device.ImmediateContext.Flush();


			//f.Visible = true;

			Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                int interval = (int)(1000.0 / FramePerSec);
                AutoResetEvent syncEvent = new AutoResetEvent(false);
				bool aspectRatio = true;

                int count = 1000000;
                while (count-- > 0)
                {
                    try
					{
						var deviceContext = device.ImmediateContext;

						_Vertex[] vertices = CreateVertices(f.Size, new GDI.Size(BufferWidth, BufferHeight), aspectRatio);

						using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices))
						{
							VertexBufferBinding vertexBuffer = new VertexBufferBinding
							{
								Buffer = buffer,
								Stride = Utilities.SizeOf<_Vertex>(),
								Offset = 0,
							};
							device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBuffer);
						}

						device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

						ShaderResourceView shaderResourceView = null;
						try
						{
							shaderResourceView = new ShaderResourceView(device, sourceTexture0, new ShaderResourceViewDescription
							{
								Format = sourceTexture0.Description.Format,
								Dimension = ShaderResourceViewDimension.Texture2D,
								Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
							});

							deviceContext.Rasterizer.SetViewport(new SharpDX.Mathematics.Interop.RawViewportF
							{
								Width = BufferWidth,
								Height = BufferHeight,
								MinDepth = 0f,
								MaxDepth = 1f,
								X = 0,
								Y = 0,
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
							device.ImmediateContext.CopyResource(sharedTexture, backBuffer);
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

                sourceTexture0.Dispose();
                device.Dispose();

            });

            Application.Run(f);



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

				// в правых координатах -1f, -1f 
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
            Console.WriteLine("InitShaders()");


           var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var shaderPath = Path.Combine(appPath, "Shaders");


            /*
             *  { float3(-0.5f,  0.5f, 0.f), float2(0.f, 0.f) },
                { float3( 0.5f,  0.5f, 0.f), float2(1.f, 0.f) },
                { float3( 0.5f, -0.5f, 0.f), float2(1.f, 1.f) },
                { float3(-0.5f, -0.5f, 0.f), float2(0.f, 1.f) },
             */

            _Vertex[] _vertices = new _Vertex[]
            {
                    new _Vertex(new Vector3(-0.7f, -1f, 0f), new Vector2(0f, 1f)),
                    new _Vertex(new Vector3(-0.7f,  1f, 0f), new Vector2(0f, 0f)),
                    new _Vertex(new Vector3(0.7f, -1f, 0f), new Vector2(1f, 1f)),
                    new _Vertex(new Vector3(0.7f, 1f, 0f), new Vector2(1f, 0f)),

            };

			//_Vertex[] _vertices = new _Vertex[]
			//    {
			//        new _Vertex(new Vector3(-1.0f, -1.0f, 0), new Vector2(0.0f, 1.0f)),
			//        new _Vertex(new Vector3(-1.0f, 1.0f, 0), new Vector2(0.0f, 0.0f)),
			//        new _Vertex(new Vector3(1.0f, -1.0f, 0), new Vector2(1.0f, 1.0f)),
			//        new _Vertex(new Vector3(1.0f, -1.0f, 0), new Vector2(1.0f, 1.0f)),
			//        new _Vertex(new Vector3(-1.0f, 1.0f, 0), new Vector2(0.0f, 0.0f)),
			//        new _Vertex(new Vector3(1.0f, 1.0f, 0), new Vector2(1.0f, 0.0f)),
			//    };

			//VertexBufferBinding vertexBuffer = new VertexBufferBinding
			//{
			//	Buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, _vertices),
			//	Stride = Utilities.SizeOf<_Vertex>(),
			//	Offset = 0,
			//};
			//device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertexBuffer);
			//vertexBuffer.Buffer.Dispose();


			//int[] strides = { Utilities.SizeOf<_Vertex>() };

			//int[] offsets = { 0 };

			//SharpDX.Direct3D11.Buffer[] vertices =
			//{
			//    SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, _vertices)
			//};

			//device.ImmediateContext.InputAssembler.SetVertexBuffers(0, vertices, strides, offsets);


			//device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			//device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;



            var profileLevel = "4_0";
            //var profileLevel = "5_0";
            var vsProvile = "vs_" + profileLevel;
            var psProvile = "ps_" + profileLevel;

            {
                var vsFile = Path.Combine(shaderPath, "VertexShader.hlsl");

                var compilationResult = CompileShaderFromFile(vsFile, "VS", vsProvile);
                vertexShader = new VertexShader(device, compilationResult.Bytecode);

                /*
                 * 
                 * D3D11_INPUT_ELEMENT_DESC Layout[] =
	                {
		                { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		                { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 }
	                };

                 */
                //InputElement[] elements =
                //{
                //    new InputElement
                //    {
                //        SemanticName = "POSITION",
                //        SemanticIndex = 0,
                //        Format = Format.R32G32B32_Float,
                //        Slot = 0,
                //        AlignedByteOffset = 0,
                //        Classification = InputClassification.PerVertexData,
                //        InstanceDataStepRate = 0,
                //    },
                //    new InputElement
                //    {
                //        SemanticName = "TEXCOORD",
                //        SemanticIndex = 0,
                //        Format = Format.R32G32_Float,
                //        Slot = 0,
                //        AlignedByteOffset = 12,
                //        Classification = InputClassification.PerVertexData,
                //        InstanceDataStepRate = 0,
                //    }
                //};

                var elements = new[]
                {
                    new InputElement("POSITION",0,Format.R32G32B32_Float,0,0),
                    new InputElement("TEXCOORD",0,Format.R32G32_Float,12,0)
                };

                inputLayout = new InputLayout(device, compilationResult.Bytecode, elements);

                device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
            }

            {
                var psFile = Path.Combine(shaderPath, "PixelShader.hlsl");

                var compilationResult = CompileShaderFromFile(psFile, "PS", psProvile);
                pixelShader = new PixelShader(device, compilationResult.Bytecode);
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
