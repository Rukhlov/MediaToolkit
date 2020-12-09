using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX.Direct3D;
using GDI = System.Drawing;



namespace Test.Encoder
{
    public class RgbToNv12Converter
    {

        public static void Run()
        {
            Console.WriteLine("RgbToNv12Converter::Run()");
            try
            {
                RgbToNv12Converter converter = new RgbToNv12Converter();
                converter.Start();



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        private SharpDX.Direct3D11.Device device = null;
        private VertexShader vertexShader = null;

        private PixelShader pixelShader = null;
        private SamplerState samplerLinear;
        private PixelShader rgbToYuvPixShader = null;

        private Texture2D rgbTexture = null;
        private ShaderResourceView rgbSRV = null;

        private ShaderResourceView lumaSRV = null;
        private RenderTargetView lumaRT = null;
        private ShaderResourceView chromaSRV = null;
        private RenderTargetView chromaRT = null;

        private Texture2D nv12Texture = null;
        private RenderTargetView nv12LumaRT = null;
        private RenderTargetView nv12ChromaRT = null;



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

            //Format[] formats =
            //{
            //        Format.NV12,
            //        Format.B8G8R8A8_UNorm,
            //        Format.R8_UNorm,
            //        Format.R8G8_SNorm,
            //    };

            //foreach (var format in formats)
            //{
            //    var formatSupport = device.CheckFormatSupport(format);
            //    var log = LogEnumFlags(formatSupport);

            //    Console.WriteLine(format + " support level:\r\n" + log);
            //    Console.WriteLine("-----------------------------------");
            //}

            InitShaders();

            //var fileName = @"Files\2560x1440.bmp";
            //var fileName = @"Files\1920x1080.bmp";          
             var fileName = @"Files\rgba_640x480.bmp";
            //var fileName = @"D:\Dropbox\Public\1681_source.jpg";
            rgbTexture = WicTool.CreateTexture2DFromBitmapFile(fileName, device);

            //var fileName = @"Files\rgba_352x288.raw";
            //var sourceTexture0 = MediaToolkit.DirectX.DxTool.TextureFromDump(device, );
            var srcDescr = rgbTexture.Description;
            int width = srcDescr.Width;
            int height = srcDescr.Height;

            // для NV12 размеры должны быть четными!!!
            var newSize = MediaToolkit.Utils.GraphicTools.DecreaseToEven(new GDI.Size(width, height));
            width = newSize.Width;
            height = newSize.Height;

            ShaderResourceViewDescription description = new ShaderResourceViewDescription
            {
                Format = Format.R8G8B8A8_UNorm,
                Dimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
            };

            rgbSRV = new ShaderResourceView(device, rgbTexture, description);


            InitLumaRenderResources(width, height);

            InitChromaRenderResources(width, height);

            InitNv12RenderResources(width, height);

            var deviceContext = device.ImmediateContext;
            var size = new GDI.Size(width, height);

            _Vertex[] vertices = CreateVertices(size, new GDI.Size(width, height), false);
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

            samplerLinear = new SamplerState(device, new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                //Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                ComparisonFunction = Comparison.Never,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                MinimumLod = 0,
                MaximumLod = float.MaxValue,
            });

            deviceContext.PixelShader.SetSamplers(0, samplerLinear);
            // RBG->NV12
            SetViewPort(width, height);
            deviceContext.VertexShader.SetShader(vertexShader, null, 0);

            deviceContext.OutputMerger.SetTargets(lumaRT, chromaRT);
            deviceContext.ClearRenderTargetView(lumaRT, Color.Black);
            deviceContext.ClearRenderTargetView(chromaRT, Color.Black);  
            deviceContext.PixelShader.SetShader(rgbToYuvPixShader, null, 0);
            deviceContext.PixelShader.SetShaderResources(0, rgbSRV);
            deviceContext.Draw(vertices.Length, 0);

            deviceContext.OutputMerger.SetTargets(nv12LumaRT);
            deviceContext.ClearRenderTargetView(nv12LumaRT, Color.Black);
            deviceContext.PixelShader.SetShader(pixelShader, null, 0);
            deviceContext.PixelShader.SetShaderResources(0, lumaSRV);
            deviceContext.Draw(vertices.Length, 0);

            SetViewPort(width / 2, height / 2);
            deviceContext.OutputMerger.SetTargets(nv12ChromaRT);
            deviceContext.ClearRenderTargetView(nv12ChromaRT, Color.Black);
            deviceContext.PixelShader.SetShaderResources(0, chromaSRV);
            deviceContext.Draw(vertices.Length, 0);



            //using (var tex = lumaRT.ResourceAs<Texture2D>())
            //{
            //    var descr = tex.Description;
            //    var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, tex);
            //    var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
            //    File.WriteAllBytes(_fileName, bytes);
            //    Console.WriteLine("OutputFile: " + _fileName);
            //}

            //using (var tex = chromaRT.ResourceAs<Texture2D>())
            //{
            //    var descr = tex.Description;
            //    var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, tex);
            //    var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
            //    File.WriteAllBytes(_fileName, bytes);
            //    Console.WriteLine("OutputFile: " + _fileName);
            //}


            {
                var descr = nv12Texture.Description;
                var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, nv12Texture);

                var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
                File.WriteAllBytes(_fileName, bytes);

                Console.WriteLine("OutputFile: " + _fileName);
            }


            // NV12->RGB
            var rgbdescr = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = Format.R8G8B8A8_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            using (var rgbDestTexture = new Texture2D(device, rgbdescr))
            {
                RenderTargetViewDescription rtvDescr = new RenderTargetViewDescription
                {
                    Format = Format.R8G8B8A8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                };

                using (var rgbRT = new RenderTargetView(device, rgbDestTexture, rtvDescr))
                {
                    ShaderResourceViewDescription srvDescr = new ShaderResourceViewDescription
                    {
                        Format = Format.R8_UNorm,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                    };

                    using (var nv12LumaSRV = new ShaderResourceView(device, nv12Texture, srvDescr))
                    {
                        srvDescr.Format = Format.R8G8_UNorm;
                        using (var nv12ChromaSRV = new ShaderResourceView(device, nv12Texture, srvDescr))
                        {
                            SetViewPort(width, height);
                            deviceContext.OutputMerger.SetTargets(rgbRT);
                            deviceContext.ClearRenderTargetView(rgbRT, Color.Black);

                            deviceContext.PixelShader.SetShader(nv12ToRgbPixShader, null, 0);
                            deviceContext.PixelShader.SetShaderResources(0, nv12LumaSRV, nv12ChromaSRV);
                            deviceContext.Draw(vertices.Length, 0);
                        }
                    }

                }



                {
                    var descr = rgbDestTexture.Description;
                    var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, rgbDestTexture);

                    var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
                    File.WriteAllBytes(_fileName, bytes);

                    Console.WriteLine("OutputFile: " + _fileName);
                }



            }





            nv12ChromaRT.Dispose();
            nv12LumaRT.Dispose();
            nv12Texture.Dispose();
            chromaSRV.Dispose();
            chromaRT.Dispose();
            lumaSRV.Dispose();
            lumaRT.Dispose();
            samplerLinear.Dispose();

            nv12ToRgbPixShader.Dispose();
            rgbToYuvPixShader.Dispose();
            vertexShader.Dispose();
            pixelShader.Dispose();
            
            rgbSRV.Dispose();
            rgbTexture.Dispose();

            device.Dispose();
            adapter.Dispose();
            factory1.Dispose();
        }


        private void InitShaders()
        {
            Console.WriteLine("RgbToNv12Converter::InitShaders()");

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

            psFile = Path.Combine(shaderPath, "RgbToYuvPS.hlsl");
            using (var compResult = CompileShaderFromFile(psFile, "PS", psProvile))
            {
                rgbToYuvPixShader = new PixelShader(device, compResult.Bytecode);
            }

            psFile = Path.Combine(shaderPath, "Nv12ToRgbPs.hlsl");
            using (var compResult = CompileShaderFromFile(psFile, "PS", psProvile))
            {
                nv12ToRgbPixShader = new PixelShader(device, compResult.Bytecode);
            }
        }
        private PixelShader nv12ToRgbPixShader = null;


        private void InitNv12RenderResources(int width, int height)
        {
            Console.WriteLine("InitRenderNV12(...) " + width + " " + height);
            var descr = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = Format.NV12,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            nv12Texture = new Texture2D(device, descr);
            {
                RenderTargetViewDescription d = new RenderTargetViewDescription
                {
                    Format = Format.R8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                };

                nv12LumaRT = new RenderTargetView(device, nv12Texture, d);

                d = new RenderTargetViewDescription
                {
                    Format = Format.R8G8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                };

                nv12ChromaRT = new RenderTargetView(device, nv12Texture, d);
            }

        }




        private void InitLumaRenderResources(int width, int height)
        {
            Console.WriteLine("InitLumaRenderResources(...) " + width + " " + height);
            var descr = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = Format.R8_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            using (var tex = new Texture2D(device, descr))
            {
                ShaderResourceViewDescription description = new ShaderResourceViewDescription
                {
                    Format = Format.R8_UNorm,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                };

                lumaSRV = new ShaderResourceView(device, tex, description);

                RenderTargetViewDescription d = new RenderTargetViewDescription
                {
                    Format = Format.R8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                };

                lumaRT = new RenderTargetView(device, tex, d);
            }

        }


        private void InitChromaRenderResources(int width, int height)
        {
            Console.WriteLine("InitChromaRenderResources(...) " + width + " " + height);
            var descr = new SharpDX.Direct3D11.Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Format = Format.R8G8_UNorm,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            using (var tex = new Texture2D(device, descr))
            {
                ShaderResourceViewDescription description = new ShaderResourceViewDescription
                {
                    Format = Format.R8G8_UNorm,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                };

                chromaSRV = new ShaderResourceView(device, tex, description);

                RenderTargetViewDescription d = new RenderTargetViewDescription
                {
                    Format = Format.R8G8_UNorm,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                };

                chromaRT = new RenderTargetView(device, tex, d);
            }

        }

        private void SetViewPort(int width, int height)
        {
            device.ImmediateContext.Rasterizer.SetViewport(new SharpDX.Mathematics.Interop.RawViewportF
            {
                Width = width,
                Height = height,
                MinDepth = 0f,
                MaxDepth = 1f,
                X = 0,
                Y = 0,
            });
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

        public static string LogEnumFlags(Enum flags)
        {
            string log = "";

            Type type = flags.GetType();

            var values = Enum.GetValues(type).Cast<Enum>().Where(f => flags.HasFlag(f));
            log = string.Join(" | ", values);

            return log;
        }

    }
}
