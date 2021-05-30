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
using System;

namespace MediaToolkit.DirectX
{
    public class RgbProcessor
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DirectX");

        public RgbProcessor()
        { }

        private SharpDX.Direct3D11.Device device = null;

        private VertexShader defaultVS = null;
        private PixelShader defaultPS = null;
        private PixelShader toRgb32Ps = null;
        private PixelShader downscaleBilinearPS = null;

        private Texture2D rgbTexture = null;

        private Texture2D tempTexture = null;
        private ShaderResourceView tempSRV = null;
        private RenderTargetView tempRTV = null;

        private SamplerState textureSampler = null;

        private ScalingFilter scalingFilter = ScalingFilter.Linear;
        private Vector2 scalingNorm;

        private GDI.Size srcSize;
        private PixFormat srcFormat = PixFormat.Unknown;

        private GDI.Size destSize;
        private PixFormat destFormat = PixFormat.Unknown;
        private SharpDX.DXGI.Format SrcDxgiFormat = Format.B8G8R8A8_UNorm;

        public bool KeepAspectRatio { get; set; } = true;
        public SharpDX.Color BackColor { get; set; } = SharpDX.Color.Blue;

        public void Init(SharpDX.Direct3D11.Device device,
            GDI.Size srcSize, PixFormat srcFormat,
            GDI.Size destSize, PixFormat destFormat,
            ScalingFilter scalingFilter = ScalingFilter.Linear)
        {
            if (srcFormat != PixFormat.RGB32 &&
                srcFormat != PixFormat.RGB24 &&
                srcFormat != PixFormat.RGB16 &&
                srcFormat != PixFormat.RGB15 &&
                srcFormat != PixFormat.NV12 && 
				srcFormat != PixFormat.I444 && srcFormat != PixFormat.I422 && srcFormat != PixFormat.I420)
            {
                throw new InvalidOperationException("Invalid source format: " + srcFormat);
            }

            if (destFormat != PixFormat.RGB32
                && destFormat != PixFormat.RGB24)
            {
                throw new InvalidOperationException("Invalid dest format: " + destFormat);
            }

            if (scalingFilter != ScalingFilter.Point
                && scalingFilter != ScalingFilter.FastLinear
                && scalingFilter != ScalingFilter.Linear)
            {
                throw new NotSupportedException("Invalid scaling filter: " + scalingFilter);
            }

            try
            {
                this.device = device;
                this.srcSize = srcSize;
                this.srcFormat = srcFormat;
                this.destFormat = destFormat;
                this.destSize = destSize;
                this.scalingFilter = scalingFilter;

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

            var vertexShaderBytes = HlslCompiler.GetVertexShaderBytes("DefaultVS", "VS");
            defaultVS = new VertexShader(device, vertexShaderBytes);
            var elements = new[]
            {
                new InputElement("POSITION",0,Format.R32G32B32_Float,0,0),
                new InputElement("TEXCOORD",0,Format.R32G32_Float,12,0)
            };

            using (var inputLayout = new InputLayout(device, vertexShaderBytes, elements))
            {
                device.ImmediateContext.InputAssembler.InputLayout = inputLayout;
            }

            defaultPS = HlslCompiler.GetPixelShader(device, "DefaultPS", "PS");

            if (srcFormat == PixFormat.RGB16)
            {
                toRgb32Ps = HlslCompiler.GetPixelShader(device, "Rgb16To32", "PS");
            }
            else if (srcFormat == PixFormat.RGB15)
            {
                toRgb32Ps = HlslCompiler.GetPixelShader(device, "Rgb15To32", "PS");
            }
            else if (srcFormat == PixFormat.NV12)
            {
                toRgb32Ps = HlslCompiler.GetPixelShader(device, "Nv12ToRgb", "PS");
            }
			else if (srcFormat == PixFormat.I444 || srcFormat == PixFormat.I422 || srcFormat == PixFormat.I420)
			{
				toRgb32Ps = HlslCompiler.GetPixelShader(device, "YuvToRgb", "PS");
			}
			if (scalingFilter == ScalingFilter.Linear)
            {
                downscaleBilinearPS = HlslCompiler.GetPixelShader(device, "DownscaleBilinear8", "PS");
            }

        }

        private PixelShader scalingShader = null;

        private void InitResources()
        {
            logger.Debug("InitRenderResources(...) " + destSize);

            rgbTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = destSize.Width,
                Height = destSize.Height,
                Format = SrcDxgiFormat,//sourceDescription.Format,

                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,

            });

            if (srcFormat == PixFormat.RGB15 || srcFormat == PixFormat.RGB16 
				|| srcFormat == PixFormat.NV12 
				|| srcFormat == PixFormat.I444 || srcFormat == PixFormat.I422 || srcFormat == PixFormat.I420)
            {
                tempTexture = new Texture2D(device, new Texture2DDescription
                {
                    Format = SrcDxgiFormat,
                    Width = srcSize.Width,
                    Height = srcSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                });

                tempSRV = new ShaderResourceView(device, tempTexture, new ShaderResourceViewDescription
                {
                    Format = tempTexture.Description.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                });

                tempRTV = new RenderTargetView(device, tempTexture, new RenderTargetViewDescription
                {
                    Format = tempTexture.Description.Format,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                });

            }

            var samplerDescr = new SamplerStateDescription
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
            };

            scalingShader = defaultPS;
            if (scalingFilter == ScalingFilter.Linear)
            {
                var downscaleX = srcSize.Width / (float)destSize.Width;
                var downscaleY = srcSize.Height / (float)destSize.Height;

                var normX = 1f / destSize.Width;
                if (downscaleX > 3.0)
                {
                    normX = 1f / (3f * destSize.Width);
                }
                else if (downscaleX > 2.0)
                {
                    normX = 1f / (2f * destSize.Width);
                }


                var normY = 1f / destSize.Height;
                if (downscaleY > 3.0)
                {
                    normY = 1f / (3f * destSize.Height);
                }
                else if (downscaleY > 2.0)
                {
                    normY = 1f / (2f * destSize.Height);
                }

                if (downscaleX > 1.0 || downscaleY > 1.0)
                {
                    scalingShader = downscaleBilinearPS;
                    scalingNorm = new Vector2(normX, normY);
                }
            }
            else if (scalingFilter == ScalingFilter.Point)
            {
                samplerDescr.Filter = Filter.MinMagMipPoint;
            }

            textureSampler = new SamplerState(device, samplerDescr);


		}

		public void DrawTexture(Texture2D srcTexture,
            Texture2D destTexture, GDI.Size viewSize,
            bool aspectRatio = true, Transform transform = Transform.R0)
        {
            DrawTexture(new Texture2D[] { srcTexture }, destTexture, viewSize, aspectRatio, transform);
        }

        public void DrawTexture(Texture2D srcTexture, Texture2D destTexture, bool aspectRatio = true, Transform transform = Transform.R0)
        {

            var destDescr = destTexture.Description;
            if (destSize != new GDI.Size(destDescr.Width, destDescr.Height))
            {
                //...
            }

            DrawTexture(new Texture2D[] { srcTexture }, destTexture, destSize, aspectRatio, transform);

        }

        public void DrawTexture(Texture2D[] srcTextures,
            Texture2D destTexture, GDI.Size viewSize,
            bool aspectRatio = true, Transform transform = Transform.R0)

        {
            lock (device)
            {
                DrawTextureInternal(srcTextures, destTexture, viewSize, aspectRatio, transform);
            }
        }

        private void DrawTextureInternal(Texture2D[] srcTextures,
            Texture2D destTexture, GDI.Size viewSize,
            bool aspectRatio = true, Transform transform = Transform.R0)

        {

            DeviceContext deviceContext = device.ImmediateContext;

            var srcTexture = srcTextures[0];
            var srcDescr = srcTexture.Description;
            var srcSize = new GDI.Size(srcDescr.Width, srcDescr.Height);
            var destDescr = destTexture.Description;
            var destSize = new GDI.Size(destDescr.Width, destDescr.Height);

            ShaderResourceView srcSRV = null;
            try
            {
                if (srcFormat == PixFormat.RGB15 || srcFormat == PixFormat.RGB16
                    || srcFormat == PixFormat.RGB24
                    || srcFormat == PixFormat.RGB32)
                {
                    srcSRV = new ShaderResourceView(device, srcTexture, new ShaderResourceViewDescription
                    {
                        Format = srcTexture.Description.Format,
                        Dimension = ShaderResourceViewDimension.Texture2D,
                        Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                    });
                }



                var rgb32SRV = srcSRV;
                var vertices = VertexHelper.DefaultQuad;
                deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
                deviceContext.PixelShader.SetSamplers(0, textureSampler);
                deviceContext.VertexShader.SetShader(defaultVS, null, 0);

                deviceContext.Rasterizer.SetViewport(0, 0, srcSize.Width, srcSize.Height);


                if (srcFormat == PixFormat.RGB16 || srcFormat == PixFormat.RGB15)
                {
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

                    deviceContext.PixelShader.SetShader(toRgb32Ps, null, 0);
                    deviceContext.OutputMerger.SetTargets(tempRTV);
                    deviceContext.ClearRenderTargetView(tempRTV, BackColor);
                    deviceContext.PixelShader.SetShaderResource(0, srcSRV);
                    deviceContext.Draw(vertices.Length, 0);

                    rgb32SRV = tempSRV;
                }
                else if (srcFormat == PixFormat.NV12)
                {
                    ShaderResourceView[] shaderResourceViews = null;
                    ShaderResourceView lumaSRV = null;
                    ShaderResourceView chromaSRV = null;
                    try
                    {
                        if (srcTextures.Length == 2)
                        {
                            ShaderResourceViewDescription description = new ShaderResourceViewDescription
                            {
                                Format = Format.R8_UNorm,
                                Dimension = ShaderResourceViewDimension.Texture2D,
                                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                            };
                            lumaSRV = new ShaderResourceView(device, srcTextures[0], description);

                            description.Format = Format.R8G8_UNorm;
                            chromaSRV = new ShaderResourceView(device, srcTextures[1], description);
                        }
                        else if (srcTextures.Length == 1)
                        {
                            ShaderResourceViewDescription description = new ShaderResourceViewDescription
                            {
                                Format = Format.R8_UNorm,
                                Dimension = ShaderResourceViewDimension.Texture2D,
                                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                            };

                            lumaSRV = new ShaderResourceView(device, srcTexture, description);

                            description.Format = Format.R8G8_UNorm;
                            chromaSRV = new ShaderResourceView(device, srcTexture, description);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid source textures number");
                        }

                        //shaderResourceViews = new ShaderResourceView[]
                        //{
                        //	lumaSRV,
                        //	chromaSRV
                        //};

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

                        var yuvToRgbMatrix = ColorSpaceHelper.GetYuvToRgbMatrix();
                        using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref yuvToRgbMatrix))
                        {
                            deviceContext.PixelShader.SetConstantBuffer(0, buffer);
                        }

                        deviceContext.PixelShader.SetShader(toRgb32Ps, null, 0);
                        deviceContext.OutputMerger.SetTargets(tempRTV);
                        deviceContext.ClearRenderTargetView(tempRTV, BackColor);
                        deviceContext.PixelShader.SetShaderResources(0, lumaSRV, chromaSRV);
                        deviceContext.Draw(vertices.Length, 0);

                    }
                    finally
                    {
                        DxTool.SafeDispose(lumaSRV);
                        DxTool.SafeDispose(chromaSRV);

                    }

                    rgb32SRV = tempSRV;
                }
                else if (srcFormat == PixFormat.I444 || srcFormat == PixFormat.I422 || srcFormat == PixFormat.I420)
                {
                    ShaderResourceView lumaSRV = null;
                    ShaderResourceView CbSRV = null;
                    ShaderResourceView CrSRV = null;
                    try
                    {
                        if (srcTextures.Length == 3)
                        {
                            ShaderResourceViewDescription description = new ShaderResourceViewDescription
                            {
                                Format = Format.R8_UNorm,
                                Dimension = ShaderResourceViewDimension.Texture2D,
                                Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
                            };

                            lumaSRV = new ShaderResourceView(device, srcTextures[0], description);
                            CbSRV = new ShaderResourceView(device, srcTextures[1], description);
                            CrSRV = new ShaderResourceView(device, srcTextures[2], description);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid source textures number");
                        }

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

                        var yuvToRgbMatrix = ColorSpaceHelper.GetYuvToRgbMatrix();
                        using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref yuvToRgbMatrix))
                        {
                            deviceContext.PixelShader.SetConstantBuffer(0, buffer);
                        }

                        deviceContext.PixelShader.SetShader(toRgb32Ps, null, 0);
                        deviceContext.OutputMerger.SetTargets(tempRTV);
                        deviceContext.ClearRenderTargetView(tempRTV, BackColor);
                        deviceContext.PixelShader.SetShaderResources(0, lumaSRV, CbSRV, CrSRV);
                        deviceContext.Draw(vertices.Length, 0);

                    }
                    finally
                    {
                        DxTool.SafeDispose(lumaSRV);
                        DxTool.SafeDispose(CbSRV);
                        DxTool.SafeDispose(CrSRV);
                    }

                    rgb32SRV = tempSRV;
                }

                if (srcSize != destSize || transform != Transform.R0 || srcSize != viewSize)
                {
                    vertices = VertexHelper.GetQuadVertices(viewSize, srcSize, aspectRatio, transform);
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

                    if (scalingFilter == ScalingFilter.Linear)
                    {
                        using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref scalingNorm, 16))
                        {
                            deviceContext.PixelShader.SetConstantBuffer(0, buffer);
                        }
                    }
                    deviceContext.PixelShader.SetShader(scalingShader, null, 0);
                }
                else
                {
                    vertices = VertexHelper.DefaultQuad;
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

                    deviceContext.PixelShader.SetShader(defaultPS, null, 0);
                }

                RenderTargetView destRTV = null;
                try
                {
                    destRTV = new RenderTargetView(device, destTexture, new RenderTargetViewDescription
                    {
                        Format = destTexture.Description.Format,
                        Dimension = RenderTargetViewDimension.Texture2D,
                        Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
                    });

                    deviceContext.Rasterizer.SetViewport(0, 0, destSize.Width, destSize.Height);
                    deviceContext.OutputMerger.SetTargets(destRTV);
                    deviceContext.ClearRenderTargetView(destRTV, BackColor);
                    deviceContext.PixelShader.SetShaderResource(0, rgb32SRV);
                    deviceContext.Draw(vertices.Length, 0);
                    //deviceContext.Flush();
                    rgb32SRV = null;

                }
                finally
                {
                    DxTool.SafeDispose(destRTV);
                }
            }
            finally
            {

                DxTool.SafeDispose(srcSRV);
            }
        }

        public void Close()
        {
            DxTool.SafeDispose(tempSRV);
            DxTool.SafeDispose(tempRTV);
            DxTool.SafeDispose(tempTexture);
            DxTool.SafeDispose(rgbTexture);
            DxTool.SafeDispose(textureSampler);
            DxTool.SafeDispose(downscaleBilinearPS);
            DxTool.SafeDispose(toRgb32Ps);
            DxTool.SafeDispose(defaultPS);
            DxTool.SafeDispose(defaultVS);
            DxTool.SafeDispose(device);

        }

    }
}
