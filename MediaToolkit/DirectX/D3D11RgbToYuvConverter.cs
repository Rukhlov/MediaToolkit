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
using System.Collections.Generic;

namespace MediaToolkit.DirectX
{
	public class D3D11RgbToYuvConverter
	{
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.DirectX");

		public D3D11RgbToYuvConverter()
		{ }

		private SharpDX.Direct3D11.Device device = null;

		private VertexShader defaultVS = null;
		private PixelShader defaultPS = null;
		private PixelShader rgbToYuvPS = null;
		private PixelShader downscaleBilinearPS = null;

		private Texture2D rgbTexture = null;

		private ShaderResourceView CbSRV = null;
		private RenderTargetView CbRT = null;

		private ShaderResourceView CrSRV = null;
		private RenderTargetView CrRT = null;

		private ShaderResourceView CrCbSRV = null;
		private RenderTargetView CrCbRT = null;

		private SamplerState textureSampler = null;
		private SharpDX.Direct3D11.Buffer constBuffer = null;

		private Matrix colorMatrix;

		private ScalingFilter scalingFilter = ScalingFilter.Linear;

		private GDI.Size srcSize;
		private PixFormat srcFormat = PixFormat.RGB32;

		private GDI.Size destSize;
		private PixFormat destFormat = PixFormat.NV12;
		private SharpDX.DXGI.Format SrcDxgiFormat = Format.B8G8R8A8_UNorm;

		public bool KeepAspectRatio { get; set; } = true;
        public SharpDX.Color BackColor { get; set; } = SharpDX.Color.Blue;

		private RgbProcessor rgbProcessor = null;
        public void Init(SharpDX.Direct3D11.Device device,
			GDI.Size srcSize, PixFormat srcFormat,
			GDI.Size destSize, PixFormat destFormat,
			ScalingFilter scalingFilter = ScalingFilter.Linear,
			ColorSpace colorSpace = ColorSpace.BT709,
			ColorRange colorRange = ColorRange.Partial)
		{

			if (srcFormat != PixFormat.RGB32 
				&& srcFormat != PixFormat.RGB24 
				&& srcFormat != PixFormat.RGB16 
				&& srcFormat != PixFormat.RGB15)
			{
				throw new InvalidOperationException("Invalid source format: " + srcFormat);
			}

			if (destFormat != PixFormat.I444
				&& destFormat != PixFormat.I422
				&& destFormat != PixFormat.I420
				&& destFormat != PixFormat.NV12)
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
				this.colorMatrix = ColorSpaceHelper.GetRgbToYuvMatrix(colorSpace, colorRange);

				InitShaders();

				InitResources();

				rgbProcessor = new RgbProcessor();
				rgbProcessor.Init(device, srcSize, srcFormat, destSize, PixFormat.RGB32, scalingFilter);

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

			var rgbToYuvShaderName = destFormat == PixFormat.NV12 ? "RgbToNv12" : "RgbToYuv";
			rgbToYuvPS = HlslCompiler.GetPixelShader(device, rgbToYuvShaderName, "PS");

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

			var textureDescr = new Texture2DDescription
			{
				Format = Format.R8_UNorm,
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


			if (destFormat == PixFormat.I444 || destFormat == PixFormat.I422 || destFormat == PixFormat.I420)
			{
				using (var tex = new Texture2D(device, textureDescr))
				{
					CbSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
					{
						Format = textureDescr.Format,
						Dimension = ShaderResourceViewDimension.Texture2D,
						Texture2D = new ShaderResourceViewDescription.Texture2DResource
						{
							MipLevels = 1,
							MostDetailedMip = 0
						},
					});

					CbRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
					{
						Format = textureDescr.Format,
						Dimension = RenderTargetViewDimension.Texture2D,
						Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
					});
				}

				using (var tex = new Texture2D(device, textureDescr))
				{
					CrSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
					{
						Format = textureDescr.Format,
						Dimension = ShaderResourceViewDimension.Texture2D,
						Texture2D = new ShaderResourceViewDescription.Texture2DResource
						{
							MipLevels = 1,
							MostDetailedMip = 0
						},
					});

					CrRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
					{
						Format = textureDescr.Format,
						Dimension = RenderTargetViewDimension.Texture2D,
						Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
					});
				}
			}
			else if (destFormat == PixFormat.NV12)
			{
				textureDescr.Format = Format.R8G8_UNorm;
				using (var tex = new Texture2D(device, textureDescr))
				{
					CrCbSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
					{
						Format = textureDescr.Format,
						Dimension = ShaderResourceViewDimension.Texture2D,
						Texture2D = new ShaderResourceViewDescription.Texture2DResource
						{
							MipLevels = 1,
							MostDetailedMip = 0
						},
					});

					CrCbRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
					{
						Format = textureDescr.Format,
						Dimension = RenderTargetViewDimension.Texture2D,
						Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
					});
				}
			}
			else
			{
				throw new InvalidOperationException("Invalid video buffer format: " + destFormat);
			}

			constBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref colorMatrix);

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
				if (srcSize.Width > destSize.Width || srcSize.Height > destSize.Height)
				{
					scalingShader = downscaleBilinearPS;
				}
			}
			else if (scalingFilter == ScalingFilter.Point)
			{
				samplerDescr.Filter = Filter.MinMagMipPoint;
			}

			textureSampler = new SamplerState(device, samplerDescr);
		}

		public void Process(IVideoFrame rgbFrame, IVideoFrame yuvFrame)
		{
			if (rgbFrame.Format != PixFormat.RGB32 
                && rgbFrame.Format != PixFormat.RGB24 
                && rgbFrame.Format != PixFormat.RGB16
                && rgbFrame.Format != PixFormat.RGB15)
			{
				throw new InvalidOperationException("Invalid source frame format: " + rgbFrame.Format);
			}

			if (rgbFrame.DriverType != VideoDriverType.D3D11)
			{
				throw new InvalidOperationException("Invalid frame driver type: " + rgbFrame.DriverType);
			}

			if (yuvFrame.Format != PixFormat.I444
				&& yuvFrame.Format != PixFormat.I422
				&& yuvFrame.Format != PixFormat.I420
				&& yuvFrame.Format != PixFormat.NV12)
			{
				throw new InvalidOperationException("Invalid dest frame format: " + rgbFrame.Format);
			}

			if (yuvFrame.Format != destFormat)
			{
				throw new InvalidOperationException("Invalid output frame format: " + yuvFrame.Format);
			}

			if (yuvFrame.Width != destSize.Width || yuvFrame.Height != destSize.Height)
			{
				throw new InvalidOperationException("Invalid output frame size: " + yuvFrame.Width + "x" + yuvFrame.Height);
			}

			IReadOnlyList<Texture2D> yuvTextures = null;
			try
			{
				if (yuvFrame.DriverType == VideoDriverType.D3D11)
				{
					yuvTextures = ((D3D11VideoFrame)yuvFrame).GetTextures();
				}
				else if (yuvFrame.DriverType == VideoDriverType.CPU)
				{
					yuvTextures = CreateYuvTextures(device, yuvFrame.Width, yuvFrame.Height, yuvFrame.Format);
				}
				else
				{
					throw new InvalidOperationException("Invalid frame driver type: " + yuvFrame.DriverType);
				}

				var srcTextures = ((D3D11VideoFrame)rgbFrame).GetTextures();
				try
				{
					var rgbTex = srcTextures[0];
					Process(rgbTex, yuvTextures.ToArray());

					if (yuvFrame.DriverType == VideoDriverType.CPU)
					{// gpu->cpu

						CopyYuvTextureToMemory(yuvTextures, yuvFrame);
					}

					yuvFrame.Time = rgbFrame.Time;
					yuvFrame.Duration = rgbFrame.Duration;
				}
				finally
				{
					foreach (var t in srcTextures)
					{
						DxTool.SafeDispose(t);
					}
				}

			}
			finally
			{
				if (yuvTextures != null && yuvTextures.Count > 0)
				{
					foreach (var tex in yuvTextures)
					{
						DxTool.SafeDispose(tex);
					}
				}
			}
		}


		public void Process(Texture2D srcTexture, params Texture2D[] yuvTextures)
		{

			var srcDescr = srcTexture.Description;
			var rgbDesct = rgbTexture.Description;

			////if (srcDescr.Format != rgbDesct.Format)
			////{
			////	throw new InvalidOperationException("Invalid texture format: " + srcDescr.Format);
			////}

			//// resize source texture...
			//var srcSize = new GDI.Size(srcDescr.Width, srcDescr.Height);
			//if (destSize == srcSize)
			//{
			//	device.ImmediateContext.CopyResource(srcTexture, rgbTexture);
			//}
			//else
			//{
			//	ResizeTexutre(srcTexture, rgbTexture, KeepAspectRatio);
			//}

			rgbProcessor.Process(srcTexture, rgbTexture, KeepAspectRatio);

			// draw rgb to YCbCr
			RenderTargetView[] yuvTargets = null;
			try
			{
				yuvTargets = SetRenderTargets(yuvTextures).ToArray();
				DrawRgbToYuv(rgbTexture, yuvTargets);
			}
			finally
			{
				foreach (var rt in yuvTargets)
				{
					DxTool.SafeDispose(rt);
				}

			}
		}

		private IEnumerable<RenderTargetView> SetRenderTargets(IReadOnlyList<Texture2D> textures)
		{
			List<RenderTargetView> renderTargets = new List<RenderTargetView>();

			if (destFormat == PixFormat.NV12)
			{
				if (textures.Count == 2)
				{   //две текстуры DXGI_FORMAT_R8_UNORM и DXGI_FORMAT_R8G8_UNORM
					for (int i = 0; i < textures.Count; i++)
					{
						RenderTargetView renderTarget = GetRenderTargetView(textures[i]);
						renderTargets.Add(renderTarget);
					}
				}
				else if (textures.Count == 1)
				{// если поддерживаются видео форматы DXGI_FORMAT_NV12
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

					var lumaRTV = new RenderTargetView(device, nv12Texture, rtvDescr);

					rtvDescr.Format = Format.R8G8_UNorm;
					var chromaRTV = new RenderTargetView(device, nv12Texture, rtvDescr);

					renderTargets.Add(lumaRTV);
					renderTargets.Add(chromaRTV);

				}
				else
				{
					throw new InvalidOperationException("Invalid video frame format");
				}
			}
			else
			{
				if (textures.Count == 3)
				{
					for (int i = 0; i < textures.Count; i++)
					{ // 3 текстуры формата DXGI_FORMAT_R8_UNORM
						RenderTargetView renderTarget = GetRenderTargetView(textures[i]);
						renderTargets.Add(renderTarget);
					}
				}
				else
				{
					throw new InvalidOperationException("Invalid video frame format");
				}
			}
			return renderTargets;
		}


		private void DrawRgbToYuv(Texture2D rgbTexture, RenderTargetView[] yuvTargets)
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

			deviceContext.Rasterizer.SetViewport(0, 0, destWidth, destHeight);
			deviceContext.VertexShader.SetShader(defaultVS, null, 0);

			ShaderResourceView rgbSRV = null;
			try
			{  // convert rgb to YCbCr
				rgbSRV = new ShaderResourceView(device, rgbTexture,
					new ShaderResourceViewDescription
					{
						Format = rgbDescr.Format,
						Dimension = ShaderResourceViewDimension.Texture2D,
						Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
					});

				RenderTargetView lumaRT = yuvTargets[0];
				RenderTargetView[] targers = null;
				if (destFormat == PixFormat.NV12)
				{
					targers = new RenderTargetView[] { lumaRT, CrCbRT };
				}
				else
				{
					targers = new RenderTargetView[] { lumaRT, CbRT, CrRT };
				}

				deviceContext.OutputMerger.SetTargets(targers);
				for (int i = 0; i < targers.Length; i++)
				{
					deviceContext.ClearRenderTargetView(targers[i], SharpDX.Color.Black);
				}

				//using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref colorMatrix))
				//{
				//    deviceContext.PixelShader.SetConstantBuffer(0, buffer);
				//}

				deviceContext.PixelShader.SetConstantBuffer(0, constBuffer);
				deviceContext.PixelShader.SetShader(rgbToYuvPS, null, 0);
				deviceContext.PixelShader.SetShaderResources(0, rgbSRV);
				deviceContext.Draw(vertices.Length, 0);
			}
			finally
			{
				DxTool.SafeDispose(rgbSRV);
			}

			if (destFormat == PixFormat.NV12)
			{
				RenderTargetView nv12ChromaRT = yuvTargets[1];

				deviceContext.Rasterizer.SetViewport(0, 0, destWidth / 2f, destHeight / 2f);
				deviceContext.PixelShader.SetShader(defaultPS, null, 0);
				deviceContext.OutputMerger.SetTargets(nv12ChromaRT);
				deviceContext.ClearRenderTargetView(nv12ChromaRT, SharpDX.Color.Black);
				deviceContext.PixelShader.SetShaderResources(0, CrCbSRV);
				deviceContext.Draw(vertices.Length, 0);
			}
			else
			{
				RenderTargetView destCbRT = yuvTargets[1];
				RenderTargetView destCrRT = yuvTargets[2];

				if (destFormat == PixFormat.I444)
				{
					//SetViewPort(0, 0, destWidth, destHeight);
				}
				else if (destFormat == PixFormat.I422)
				{
					deviceContext.Rasterizer.SetViewport(0, 0, destWidth / 2f, destHeight);
				}
				else if (destFormat == PixFormat.I420)
				{
					deviceContext.Rasterizer.SetViewport(0, 0, destWidth / 2f, destHeight / 2f);
				}

				deviceContext.PixelShader.SetShader(defaultPS, null, 0);

				deviceContext.OutputMerger.SetTargets(destCbRT);
				deviceContext.ClearRenderTargetView(destCbRT, SharpDX.Color.Black);
				deviceContext.PixelShader.SetShaderResources(0, CbSRV);
				deviceContext.Draw(vertices.Length, 0);

				deviceContext.OutputMerger.SetTargets(destCrRT);
				deviceContext.ClearRenderTargetView(destCrRT, SharpDX.Color.Black);
				deviceContext.PixelShader.SetShaderResources(0, CrSRV);
				deviceContext.Draw(vertices.Length, 0);
			}


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

				var vertices = VertexHelper.GetQuadVertices(destSize, srcSize, aspectRatio);
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


				if (scalingFilter == ScalingFilter.Linear)
				{
					//var baseDimensionI = new Vector2(1f / (1f * destSize.Width), 1f / (1f * destSize.Height));
					var baseDimensionI = new Vector2(1f / (3f * destSize.Width), 1f / (3f * destSize.Height));
					using (var buffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.ConstantBuffer, ref baseDimensionI, 16))
					{
						deviceContext.PixelShader.SetConstantBuffer(0, buffer);
					}
				}

				device.ImmediateContext.PixelShader.SetSamplers(0, textureSampler);
				deviceContext.PixelShader.SetShader(scalingShader, null, 0);

				deviceContext.Rasterizer.SetViewport(0, 0, destWidth, destHeight);
				deviceContext.VertexShader.SetShader(defaultVS, null, 0);

				deviceContext.OutputMerger.SetTargets(destRTV);
				deviceContext.ClearRenderTargetView(destRTV, BackColor);
				//deviceContext.PixelShader.SetShader(pixelShader, null, 0);

				deviceContext.PixelShader.SetShaderResource(0, srcSRV);

				deviceContext.Draw(vertices.Length, 0);

			}
			finally
			{
				DxTool.SafeDispose(destRTV);
				DxTool.SafeDispose(srcSRV);
			}
		}

		private void CopyYuvTextureToMemory(IReadOnlyList<Texture2D> yuvTextures, IVideoFrame destFrame)
		{
			bool lockTaken = false;
			try
			{
				lockTaken = destFrame.Lock(int.MaxValue);
				if (lockTaken)
				{
					var dataBuffer = destFrame.Buffer;
					if (yuvTextures.Count != dataBuffer.Length)
					{

					}

					for (int i = 0; i < yuvTextures.Count; i++)
					{
						var tex = yuvTextures[i];
						var destBuffer = dataBuffer[i];

						IntPtr destPtr = destBuffer.Data;
						var destPitch = destBuffer.Stride;

						var stagingDescr = tex.Description;
						stagingDescr.BindFlags = BindFlags.None;
						stagingDescr.CpuAccessFlags = CpuAccessFlags.Read;
						stagingDescr.Usage = ResourceUsage.Staging;
						stagingDescr.OptionFlags = ResourceOptionFlags.None;

						using (var stagingTexture = new Texture2D(device, stagingDescr))
						{
							device.ImmediateContext.CopyResource(tex, stagingTexture);
							var dataBox = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
							try
							{
								var width = stagingDescr.Width;
								var height = stagingDescr.Height;

								var srcPitch = dataBox.RowPitch;
								var srcDataSize = dataBox.SlicePitch;
								var srcPtr = dataBox.DataPointer;

								for (int row = 0; row < height; row++)
								{
									Kernel32.CopyMemory(destPtr, srcPtr, (uint)destPitch);
									destPtr += destPitch;
									srcPtr += srcPitch;
								}
							}
							finally
							{
								device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
							}
						}
					}
				}
			}
			finally
			{
				if (lockTaken)
				{
					destFrame.Unlock();
				}
			}
		}

		public void Close()
        {
            if (rgbProcessor != null)
            {
                rgbProcessor.Close();
                rgbProcessor = null;
            }

			DxTool.SafeDispose(constBuffer);

			DxTool.SafeDispose(CrCbRT);
			DxTool.SafeDispose(CrCbSRV);

			DxTool.SafeDispose(CbRT);
			DxTool.SafeDispose(CbSRV);

			DxTool.SafeDispose(CrRT);
			DxTool.SafeDispose(CrSRV);

			DxTool.SafeDispose(rgbTexture);
			DxTool.SafeDispose(rgbToYuvPS);
			DxTool.SafeDispose(textureSampler);
			DxTool.SafeDispose(downscaleBilinearPS);
			DxTool.SafeDispose(defaultPS);
			DxTool.SafeDispose(defaultVS);
			DxTool.SafeDispose(device);



		}


		public static IReadOnlyList<Texture2D> CreateYuvTextures(SharpDX.Direct3D11.Device device, int width, int height, PixFormat format)
		{
			List<Texture2D> textures = new List<Texture2D>();

			var textureDescr = new SharpDX.Direct3D11.Texture2DDescription
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

			// Init luminance resource...
			var lumaTex = new Texture2D(device, textureDescr);

			// Init chrominance resource...
			if (format == PixFormat.I422)
			{
				textureDescr.Width = width / 2;
			}
			else if (format == PixFormat.I420)
			{
				textureDescr.Width = width / 2;
				textureDescr.Height = height / 2;
			}

			if (format != PixFormat.NV12)
			{// YUV444, YUV422, YUV420...

				textures.Add(lumaTex);
				textures.Add(new Texture2D(device, textureDescr));
				textures.Add(new Texture2D(device, textureDescr));
			}
			else
			{
				textureDescr.Width = width / 2;
				textureDescr.Height = height / 2;
				textureDescr.Format = Format.R8G8_UNorm;
				textures.Add(lumaTex);
				textures.Add(new Texture2D(device, textureDescr));
			}

			return textures;
		}

		private RenderTargetView GetRenderTargetView(Texture2D texture)
		{
			return new RenderTargetView(device, texture, new RenderTargetViewDescription
			{
				Format = texture.Description.Format,
				Dimension = RenderTargetViewDimension.Texture2D,
				Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
			});
		}

	}


}
