
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using MediaToolkit.Core;
using MediaToolkit.DirectX;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Logging;

using GDI = System.Drawing;


namespace MediaToolkit.DirectX
{
    class D3D11RgbToNv12Converter
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DirectX");

        public D3D11RgbToNv12Converter()
        { }

        private SharpDX.Direct3D11.Device device = null;

        private VertexShader defaultVS = null;
        private PixelShader defaultPS = null;
        private PixelShader rgbToYuvPS = null;
        private PixelShader downscalePS = null;

        private Texture2D rgbTexture = null;

        private ShaderResourceView chromaSRV = null;
        private RenderTargetView chromaRT = null;
        private RenderTargetView nv12ChromaRT = null;

        private SamplerState textureSampler = null;


        //private GDI.Size srcSize;
        private GDI.Size destSize;
        private SharpDX.DXGI.Format SrcFormat = Format.B8G8R8A8_UNorm;

        private D3D11VideoBuffer videoBuffer = null;
        public void Init(SharpDX.Direct3D11.Device device, D3D11VideoBuffer videoBuffer)
        {
            try
            {
                this.device = device;
                this.videoBuffer = videoBuffer;
                this.destSize = new GDI.Size(videoBuffer.Width, videoBuffer.Height);

                if (videoBuffer.Format != PixFormat.NV12)
                {
                    throw new InvalidOperationException("Invalid buffer format: " + videoBuffer.Format);
                }

                if (videoBuffer.DriverType == VideoDriverType.D3D11)
                {

                }
                else if (videoBuffer.DriverType == VideoDriverType.CPU)
                {
                    throw new NotImplementedException("VideoDriverType.CPU");
                }
                else
                {
                    throw new NotSupportedException();
                }

                InitShaders();

                InitResources();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();
                throw;
            }

        }

        private void InitShaders()
        {
            logger.Debug("InitShaders()");

            var profileLevel = "4_0";
            //var profileLevel = "5_0";
            var vsProvile = "vs_" + profileLevel;
            var psProvile = "ps_" + profileLevel;

            using (var compResult = HlslCompiler.CompileShaderFromResources("DefaultVS.hlsl", "VS", vsProvile))
            {
                defaultVS = new VertexShader(device, compResult.Bytecode);
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

            using (var compResult = HlslCompiler.CompileShaderFromResources("DefaultPS.hlsl", "PS", psProvile))
            {
                defaultPS = new PixelShader(device, compResult.Bytecode);
            }

            using (var compResult = HlslCompiler.CompileShaderFromResources("RgbToNv12.hlsl", "PS", psProvile))
            {
                rgbToYuvPS = new PixelShader(device, compResult.Bytecode);
            }

            using (var compResult = HlslCompiler.CompileShaderFromResources("DownscaleBilinear8.hlsl", "PS", psProvile))
            //using (var compResult = CompileShader("DownscaleBilinear9.hlsl", "PS", psProvile))
            {
                downscalePS = new PixelShader(device, compResult.Bytecode);
            }
        }

        private void InitResources()
        {
            logger.Debug("InitRenderResources(...) " + destSize);

            rgbTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = destSize.Width,
                Height = destSize.Height,
                Format = SrcFormat,//sourceDescription.Format,

                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,

            });

            var textureDescr = new Texture2DDescription
            {
                Format = Format.R8G8_UNorm,
                Width = destSize.Width,
                Height = destSize.Height,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            };

            using (var tex = new Texture2D(device, textureDescr))
            {
                chromaSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
                {
                    Format = textureDescr.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource
                    {
                        MipLevels = 1,
                        MostDetailedMip = 0
                    },
                });

                chromaRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
                {
                    Format = textureDescr.Format,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });
            }

            textureSampler = new SamplerState(device, new SamplerStateDescription
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
        }



        public void Process(Texture2D srcTexture, IVideoFrame destFrame)
        {
            var srcDescr = srcTexture.Description;
            var rgbDesct = rgbTexture.Description;

            if (srcDescr.Format != rgbDesct.Format)
            {
                throw new InvalidOperationException("Invalid texture format: " + srcDescr.Format);
            }
            // resize source texture...
            var srcSize = new GDI.Size(srcDescr.Width, srcDescr.Height);
            if (destSize == srcSize)
            {
                device.ImmediateContext.CopyResource(srcTexture, rgbTexture);
            }
            else
            {
                ResizeTexutre(srcTexture, rgbTexture, false);
            }

            // draw rgb to nv12
            RenderTargetView nv12LumaRT = null;
            RenderTargetView nv12ChromaRT = null;
            try
            {
                SetRenderTargets(destFrame, out nv12LumaRT, out nv12ChromaRT);

                DrawRgbToNv12(rgbTexture, nv12LumaRT, nv12ChromaRT);

                if (destFrame.DriverType == VideoDriverType.CPU)
                {// gpu->cpu

                }
            }
            finally
            {
                SafeDispose(nv12LumaRT);
                SafeDispose(nv12ChromaRT);
            }
        }

        private void SetRenderTargets(IVideoFrame frame, out RenderTargetView lumaRTV, out RenderTargetView chromaRTV)
        {
            lumaRTV = null;
            chromaRTV = null;

            IReadOnlyList<Texture2D> textures = null;

            try
            {
                if (frame.DriverType == VideoDriverType.D3D11)
                {
                    textures = ((D3D11VideoFrame)frame).GetTextures();
                }
                else if (frame.DriverType == VideoDriverType.CPU)
                {
                    var textureDescr = new SharpDX.Direct3D11.Texture2DDescription
                    {
                        Width = frame.Width,
                        Height = frame.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,

                        Format = Format.R8_UNorm,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None,
                    };

                    // Init luminance resource...
                    var lumaTex = new Texture2D(device, textureDescr);

                    // Init chrominance resource...
                    textureDescr.Format = Format.R8G8_UNorm;
                    textureDescr.Width = frame.Width / 2;
                    textureDescr.Height = frame.Height / 2;
                    var chromaTex = new Texture2D(device, textureDescr);

                    textures = new List<Texture2D>()
                    {
                        lumaTex,
                        chromaTex,
                    };
                }
                else
                {
                    throw new InvalidOperationException("Invalid frame driver type: " + frame.DriverType);
                }

                if (textures.Count == 2)
                {   // DXGI_FORMAT_R8_UNORM
                    var lumaTex = textures[0];
                    lumaRTV = new RenderTargetView(device, lumaTex, new RenderTargetViewDescription
                    {
                        Format = lumaTex.Description.Format,
                        Dimension = RenderTargetViewDimension.Texture2D,
                        Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                    });

                    // DXGI_FORMAT_R8G8_UNORM
                    var chromaTex = textures[1];
                    chromaRTV = new RenderTargetView(device, chromaTex, new RenderTargetViewDescription
                    {
                        Format = chromaTex.Description.Format,
                        Dimension = RenderTargetViewDimension.Texture2D,
                        Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                    });
                }
                else if (textures.Count == 1)
                {// DXGI_FORMAT_NV12
                    var nv12Texture = textures[0];
                    var descr = nv12Texture.Description;
                    if (descr.Format != Format.NV12)
                    {
                        throw new InvalidOperationException("Invalid texture format: " + descr.Format);
                    }

                    RenderTargetViewDescription rtvDescr = new RenderTargetViewDescription
                    {
                        Format = Format.R8_UNorm,
                        Dimension = RenderTargetViewDimension.Texture2D,
                        Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                    };

                    lumaRTV = new RenderTargetView(device, nv12Texture, rtvDescr);

                    rtvDescr.Format = Format.R8G8_UNorm;
                    chromaRTV = new RenderTargetView(device, nv12Texture, rtvDescr);
                }
                else
                {
                    throw new InvalidOperationException("Invalid video frame format");
                }
            }
            finally
            {
                if (textures != null && textures.Count > 0)
                {
                    foreach (var tex in textures)
                    {
                        SafeDispose(tex);
                    }
                }
            }
        }

        private void DrawRgbToNv12(Texture2D rgbTexture, RenderTargetView nv12LumaRT, RenderTargetView nv12ChromaRT)
        {         
            var rgbDescr = rgbTexture.Description;
            int destWidth = rgbDescr.Width;
            int destHeight = rgbDescr.Height;

            var deviceContext = device.ImmediateContext;
            var vertices = new _Vertex[]
            {
                new _Vertex(new Vector3(-1f, -1f, 0f), new Vector2(0f, 1f)),
                new _Vertex(new Vector3(-1f, 1f, 0f), new Vector2(0f, 0f)),
                new _Vertex(new Vector3(1f, -1f, 0f), new Vector2(1f, 1f)),
                new _Vertex(new Vector3(1f, 1f, 0f), new Vector2(1f, 0f)),
            };

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

            SetViewPort(0, 0, destWidth, destHeight);
            deviceContext.VertexShader.SetShader(defaultVS, null, 0);

            ShaderResourceView rgbSRV = null;
            try
            {
                rgbSRV = new ShaderResourceView(device, rgbTexture,
                    new ShaderResourceViewDescription
                    {
                        Format = rgbDescr.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                    });
                // convert rgb to YCbCr
                deviceContext.OutputMerger.SetTargets(nv12LumaRT, chromaRT);
                deviceContext.ClearRenderTargetView(nv12LumaRT, SharpDX.Color.Black);

                deviceContext.ClearRenderTargetView(chromaRT, SharpDX.Color.Black);
                deviceContext.PixelShader.SetShader(rgbToYuvPS, null, 0);
                deviceContext.PixelShader.SetShaderResources(0, rgbSRV);
                deviceContext.Draw(vertices.Length, 0);
            }
            finally
            {
                SafeDispose(rgbSRV);
            }

            // CbCr -> NV12 chroma
            deviceContext.PixelShader.SetShader(defaultPS, null, 0);
            SetViewPort(0, 0, destWidth / 2, destHeight / 2);
            deviceContext.OutputMerger.SetTargets(nv12ChromaRT);
            deviceContext.ClearRenderTargetView(nv12ChromaRT, SharpDX.Color.Black);
            deviceContext.PixelShader.SetShaderResources(0, chromaSRV);
            deviceContext.Draw(vertices.Length, 0);

        }


        private void ResizeTexutre(Texture2D srcTexture, Texture2D destTexture, bool aspectRatio = true)
        {
            DeviceContext deviceContext = device.ImmediateContext;

            var srcDescr = srcTexture.Description;
            var srcSize = new GDI.Size(srcDescr.Width, srcDescr.Height);

            var destDescr = destTexture.Description;

            int destWidth = destDescr.Width;
            int destHeight = destDescr.Height;

            ShaderResourceView srcSRV = null;
            RenderTargetView destRTV = null;
            try
            {
                srcSRV = new ShaderResourceView(device, srcTexture, new ShaderResourceViewDescription
                {
                    Format = srcTexture.Description.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                });

                destRTV = new RenderTargetView(device, rgbTexture, new RenderTargetViewDescription
                {
                    Format = destDescr.Format,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });

                var vertices = CreateVertices(srcSize, destSize, aspectRatio);
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

                ////var baseDimensionI = new Vector2(1f / (1f * destSize.Width), 1f / (1f * destSize.Height));
                //var baseDimensionI = new Vector2(1f / (3f * destSize.Width), 1f / (3f * destSize.Height));
                ////var baseDimensionI = new Vector2(1f / (3f * srcDescr.Width), 1f / (3f * srcDescr.Height ));
                //using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref baseDimensionI, 16))
                //{
                //    deviceContext.PixelShader.SetConstantBuffer(0, buffer);
                //}

                device.ImmediateContext.PixelShader.SetSamplers(0, textureSampler);
                deviceContext.PixelShader.SetShader(downscalePS, null, 0);
                //deviceContext.PixelShader.SetShader(defaultPS, null, 0);

                SetViewPort(0, 0, destWidth, destHeight);
                deviceContext.VertexShader.SetShader(defaultVS, null, 0);

                deviceContext.OutputMerger.SetTargets(destRTV);
                deviceContext.ClearRenderTargetView(destRTV, SharpDX.Color.Blue);
                //deviceContext.PixelShader.SetShader(pixelShader, null, 0);

                deviceContext.PixelShader.SetShaderResource(0, srcSRV);

                deviceContext.Draw(vertices.Length, 0);

            }
            finally
            {
                SafeDispose(destRTV);
                SafeDispose(srcSRV);
            }

        }



        //private void CopyNv12TextureToMemory(RenderTargetView lumaRT, RenderTargetView chromaRT, 
        //    out IntPtr destPtr, out int destBufferSize)
        //{
        //    var width = destSize.Width;
        //    var height = destSize.Height;
        //    var destPitch = width;
        //    var destRowNumber = height + height / 2;
        //    destBufferSize = destPitch * destRowNumber;
        //    destPtr = Marshal.AllocHGlobal(destBufferSize);

        //    IntPtr _destPtr = destPtr;
        //    using (var tex = lumaRT.ResourceAs<Texture2D>())
        //    {
        //        var stagingDescr = tex.Description;
        //        stagingDescr.BindFlags = BindFlags.None;
        //        stagingDescr.CpuAccessFlags = CpuAccessFlags.Read;
        //        stagingDescr.Usage = ResourceUsage.Staging;
        //        stagingDescr.OptionFlags = ResourceOptionFlags.None;

        //        using (var stagingTexture = new Texture2D(device, stagingDescr))
        //        {
        //            device.ImmediateContext.CopyResource(tex, stagingTexture);
        //            var dataBox = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
        //            try
        //            {
        //                var srcPitch = dataBox.RowPitch;
        //                var srcDataSize = dataBox.SlicePitch;
        //                var srcPtr = dataBox.DataPointer;

        //                destPitch = width;
        //                destRowNumber = height;

        //                for (int i = 0; i < destRowNumber; i++)
        //                {
        //                    Kernel32.CopyMemory(_destPtr, srcPtr, (uint)destPitch);
        //                    _destPtr += destPitch;
        //                    srcPtr += srcPitch;
        //                }
        //            }
        //            finally
        //            {
        //                device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
        //            }
        //        }
        //    }

        //    using (var tex = nv12ChromaRT.ResourceAs<Texture2D>())
        //    {
        //        //var descr = tex.Description;
        //        //var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, tex);
        //        //var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
        //        //File.WriteAllBytes(_fileName, bytes);
        //        //Console.WriteLine("OutputFile: " + _fileName);

        //        var stagingDescr = tex.Description;
        //        stagingDescr.BindFlags = BindFlags.None;
        //        stagingDescr.CpuAccessFlags = CpuAccessFlags.Read;
        //        stagingDescr.Usage = ResourceUsage.Staging;
        //        stagingDescr.OptionFlags = ResourceOptionFlags.None;
        //        using (var stagingTexture = new Texture2D(device, stagingDescr))
        //        {
        //            device.ImmediateContext.CopyResource(tex, stagingTexture);

        //            var dataBox = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
        //            try
        //            {
        //                width = stagingDescr.Width;
        //                height = stagingDescr.Height;

        //                var srcPitch = dataBox.RowPitch;
        //                var srcDataSize = dataBox.SlicePitch;
        //                var srcPtr = dataBox.DataPointer;

        //                destPitch = 2 * width;
        //                destRowNumber = height;

        //                for (int i = 0; i < destRowNumber; i++)
        //                {
        //                    Kernel32.CopyMemory(_destPtr, srcPtr, (uint)destPitch);
        //                    _destPtr += destPitch;
        //                    srcPtr += srcPitch;
        //                }
        //            }
        //            finally
        //            {
        //                device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
        //            }
        //        }
        //    }
        //    //Utils.TestTools.WriteFile((destPtr), destBufferSize, "TEST_NV12.raw");
        //}



        public void Close()
        {
            SafeDispose(nv12ChromaRT);
            SafeDispose(chromaRT);
            SafeDispose(chromaSRV);

            //SafeDispose(lumaRT);
            //SafeDispose(lumaSRV);

            SafeDispose(rgbTexture);
            SafeDispose(rgbToYuvPS);
            SafeDispose(textureSampler);
            SafeDispose(downscalePS);
            SafeDispose(defaultPS);
            SafeDispose(defaultVS);
            SafeDispose(device);

        }


        private void SetViewPort(int x, int y, int width, int height)
        {
            device.ImmediateContext.Rasterizer.SetViewport(new SharpDX.Mathematics.Interop.RawViewportF
            {
                Width = width,
                Height = height,
                MinDepth = 0f,
                MaxDepth = 1f,
                X = x,
                Y = y,
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

        struct _Vertex
        {
            public _Vertex(Vector3 pos, Vector2 tex)
            {
                this.Position = pos;
                this.TextureCoord = tex;
            }
            public Vector3 Position;
            public Vector2 TextureCoord;
        }

        private static void SafeDispose(SharpDX.DisposeBase dispose)
        {
            if (dispose != null && !dispose.IsDisposed)
            {
                dispose.Dispose();
                dispose = null;
            }
        }


    }
}
