using MediaToolkit.Core;
using MediaToolkit.Utils;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

using MediaToolkit.Logging;
using System.Runtime.InteropServices;
using MediaToolkit.SharedTypes;
using System.ComponentModel;
using SharpDX.Direct3D;
using MediaToolkit.DirectX;
using MediaToolkit;
using System.IO;
using SharpDX;

namespace Test.Encoder
{
    class RgbHelperTest
    {
        public static void Run()
        {
            Console.WriteLine("D3D11ConvertersTest::Run()");
            RgbHelperTest helperTest = new RgbHelperTest();
            helperTest.Start();


            helperTest.Close();
        }

		public void Start()
		{

			try
			{

				SharpDX.DXGI.Factory1 dxgiFactory = null;

				try
				{
					dxgiFactory = new SharpDX.DXGI.Factory1();

					//logger.Info(DirectX.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

					SharpDX.DXGI.Adapter1 adapter = null;
					try
					{
						var AdapterIndex = 0;
						adapter = dxgiFactory.GetAdapter1(AdapterIndex);
						//AdapterId = adapter.Description.Luid;
						//logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

						var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
						deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
						var featureLevel = FeatureLevel.Level_10_1;
						device = new Device(adapter, deviceCreationFlags, featureLevel);
						using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
						{
							multiThread.SetMultithreadProtected(true);
						}
					}
					finally
					{
						if (adapter != null)
						{
							adapter.Dispose();
							adapter = null;
						}
					}
				}
				finally
				{
					if (dxgiFactory != null)
					{
						dxgiFactory.Dispose();
						dxgiFactory = null;
					}
				}


				var fileName = @"Files\rgb565_640x480.raw";
				var srcSize = new Size(640, 480);
				var srcFormat = PixFormat.RGB16;

				//var fileName = @"Files\rgba_1920x1080.raw";
				//var srcSize = new Size(1920, 1080);
				//var srcFormat = PixFormat.RGB32;

				var destSize = new Size(800, 600);
				var destFormat = PixFormat.RGB32;

				RgbProcessor rgbProcessor = new RgbProcessor();
				rgbProcessor.Init(device, srcSize, srcFormat, destSize, destFormat);

				var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
				{
					Width = srcSize.Width,
					Height = srcSize.Height,
					Format = DxTool.GetDxgiFormat(srcFormat).ToArray()[0],
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
					BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
					Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
					CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					//Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
					//Format = SharpDX.DXGI.Format.R16_UNorm,
					//Format = SharpDX.DXGI.Format.R8G8_UNorm,
					//Format = SharpDX.DXGI.Format.R16_UInt,
					//Format = SharpDX.DXGI.Format.B5G6R5_UNorm,
					OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

				};
				var srcBytes = File.ReadAllBytes(fileName);

				srcTexture = MediaToolkit.DirectX.DxTool.TextureFromDump(device, texDescr, srcBytes);

				destTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
				{
					//Width = srcSize.Width,
					//Height = srcSize.Height,
					Width = destSize.Width,
					Height = destSize.Height,
					Format = DxTool.GetDxgiFormat(destFormat).ToArray()[0],

					//Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
					//Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,

					SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
					BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
					Usage = SharpDX.Direct3D11.ResourceUsage.Default,
					CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
					MipLevels = 1,
					ArraySize = 1,

				});

				rgbProcessor.Process(srcTexture, destTexture, true);


				{
					var descr = destTexture.Description;
					var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, destTexture);

					var _fileName = "Dest_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
					File.WriteAllBytes(_fileName, bytes);

					Console.WriteLine("DestFile: " + _fileName);
				}

				rgbProcessor.Close();

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

		}


		public void Start2()
        {

            try
            {

                SharpDX.DXGI.Factory1 dxgiFactory = null;

                try
                {
                    dxgiFactory = new SharpDX.DXGI.Factory1();

                    //logger.Info(DirectX.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

                    SharpDX.DXGI.Adapter1 adapter = null;
                    try
                    {
                        var AdapterIndex = 0;
                        adapter = dxgiFactory.GetAdapter1(AdapterIndex);
                        //AdapterId = adapter.Description.Luid;
                        //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                        var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
                        deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
						var featureLevel = FeatureLevel.Level_10_1;
                        device = new Device(adapter, deviceCreationFlags, featureLevel);
                        using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                        {
                            multiThread.SetMultithreadProtected(true);
                        }
                    }
                    finally
                    {
                        if (adapter != null)
                        {
                            adapter.Dispose();
                            adapter = null;
                        }
                    }
                }
                finally
                {
                    if (dxgiFactory != null)
                    {
                        dxgiFactory.Dispose();
                        dxgiFactory = null;
                    }
                }

                //var fileName = @"Files\rgba_1920x1080.raw";
                var fileName = @"Files\rgb565_640x480.raw";
                var srcSize = new Size(640, 480);
				var destSize = srcSize;// new Size(1920, 1080);



				RgbProcessor rgbProcessor = new RgbProcessor();
				rgbProcessor.Init(device, srcSize, PixFormat.RGB16, destSize, PixFormat.RGB32);


				//var fileName = @"Files\1920x1080.bmp";
				//var srcSize = new Size(1920, 1080);
				var srcFormat = PixFormat.RGB16;

                var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					Format = SharpDX.DXGI.Format.R16_UNorm,
					//Format = SharpDX.DXGI.Format.R8G8_UNorm,
					//Format = SharpDX.DXGI.Format.R16_UInt,
					//Format = SharpDX.DXGI.Format.B5G6R5_UNorm,
					OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                };
                var srcBytes = File.ReadAllBytes(fileName);

                srcTexture = MediaToolkit.DirectX.DxTool.TextureFromDump(device, texDescr, srcBytes);

                ShaderResourceViewDescription description = new ShaderResourceViewDescription
                {
                    Format = srcTexture.Description.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                };

                rgb16SRV = new ShaderResourceView(device, srcTexture, description);

                InitShaders();


                destTexture = new Texture2D (device, new SharpDX.Direct3D11.Texture2DDescription()
                {
					//Width = srcSize.Width,
					//Height = srcSize.Height,
					Width = destSize.Width,
					Height = destSize.Height,
					Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

				    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,

                });

                renderTargetView = new RenderTargetView(device, destTexture,
                new RenderTargetViewDescription
                {
                    Format = destTexture.Description.Format,//Format.R8G8B8A8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });


                var sampler = new SamplerState(device, new SamplerStateDescription
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

                _Vertex[] vertices = VertexHelper.GetQuadVertices(srcSize, destSize, false);

                var deviceContext = device.ImmediateContext;

                deviceContext.PixelShader.SetSamplers(0, sampler);

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
					//Width = srcSize.Width,//ImageWidth,
					//Height = srcSize.Height,//ImageHeight,
					Width = destSize.Width,//ImageWidth,
					Height = destSize.Height,//ImageHeight,
					MinDepth = 0f,
                    MaxDepth = 1f,
                    X = 0,
                    Y = 0,

                });


                deviceContext.OutputMerger.SetTargets(renderTargetView);

                deviceContext.ClearRenderTargetView(renderTargetView, SharpDX.Color.Blue);
                deviceContext.VertexShader.SetShader(defaultVertexShader, null, 0);

               // deviceContext.PixelShader.SetShader(defaultPixelShader, null, 0);
                deviceContext.PixelShader.SetShader(rgb16ToRgb32Shader, null, 0);

                deviceContext.PixelShader.SetShaderResource(0, rgb16SRV);

                deviceContext.Draw(vertices.Length, 0);


                {
                    var descr = destTexture.Description;
                    var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, destTexture);

                    var _fileName = "Dest_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
                    File.WriteAllBytes(_fileName, bytes);

                    Console.WriteLine("DestFile: " + _fileName);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public void Close()
        {


            destTexture?.Dispose();
            rgb16SRV?.Dispose();
            srcTexture?.Dispose();
            destTexture?.Dispose();

            device?.Dispose();
            renderTargetView?.Dispose();
            rgb16ToRgb32Shader?.Dispose();
            defaultPixelShader?.Dispose();
            defaultVertexShader?.Dispose();
        }
        private RenderTargetView renderTargetView = null;

        private ShaderResourceView rgb16SRV = null;
        private Texture2D srcTexture = null;
        private Texture2D destTexture = null;
        private Device device = null;

        private PixelShader rgb16ToRgb32Shader = null;
        private PixelShader defaultPixelShader = null;
        private VertexShader defaultVertexShader = null;

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
                    new InputElement("POSITION",0, SharpDX.DXGI.Format.R32G32B32_Float,0,0),
                    new InputElement("TEXCOORD",0,SharpDX.DXGI.Format.R32G32_Float,12,0)
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


            psFile = Path.Combine(shaderPath, "Rgb565ToRgbaPs.hlsl");
            using (var compResult = CompileShaderFromFile(psFile, "PS", psProvile))
            {
                rgb16ToRgb32Shader = new PixelShader(device, compResult.Bytecode);
            }
        }



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

    }
}
