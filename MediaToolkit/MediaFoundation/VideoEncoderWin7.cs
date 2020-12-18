using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FFmpegLib;
using MediaToolkit.Core;
using MediaToolkit.DirectX;
using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;

using SharpDX.DXGI;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System.Reflection;
using SharpDX.Direct3D;
using GDI = System.Drawing;
using MediaToolkit.NativeAPIs;

namespace MediaToolkit.MediaFoundation
{
	public class VideoEncoderWin7
	{

		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

		private readonly IVideoSource videoSource = null;

		public VideoEncoderWin7(IVideoSource source)
		{
			this.videoSource = source;
		}

		//private FFmpegVideoEncoder ffmpegEncoder = null;

		private H264Encoder ffmpegEncoder = null;
		
		private GDI.Size srcSize;
		private GDI.Size destSize;

		public void Open(VideoEncoderSettings encoderSettings)
		{
			logger.Debug("VideoEncoder::Setup(...)");

			var encoderName = encoderSettings.EncoderId;

			if (encoderName == "libx264" || encoderName == "h264_nvenc")
			{
				//ffmpegEncoder = new FFmpegVideoEncoder();
				ffmpegEncoder = new H264Encoder();
			}
			else
			{
				throw new NotSupportedException("Invalid encoder name: " + encoderName);
			}
			//ffmpegEncoder.Open(encoderSettings);
			ffmpegEncoder.Setup(encoderSettings);
			ffmpegEncoder.DataEncoded += FFmpegEncoder_DataEncoded;

			InitDx(encoderSettings);

		}

		private void FFmpegEncoder_DataEncoded(IntPtr data, int size, double timestamp)
		{
			var buf = new byte[size];
			Marshal.Copy(data, buf, 0, size);

			OnDataReady(buf, timestamp);
		}

		public void Encode()
		{
			//ffmpegEncoder.Encode(videoSource.SharedBitmap);
			PrepareTexture();

			EncodeTexture();

		}

		private void EncodeTexture()
		{
			//var descr = nv12Texture.Description;
			//var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, nv12Texture);
			//var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
			//File.WriteAllBytes(_fileName, bytes);
			//Console.WriteLine("OutputFile: " + _fileName);


			IntPtr ptr = IntPtr.Zero;
			try
			{
				CopyNv12TextureToMemory(out ptr, out var size);

				ffmpegEncoder.Encode(ptr, size, 0);
			}
			finally
			{
				if (ptr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(ptr);
					ptr = IntPtr.Zero;
				}
			}
		}

		public void Close()
		{
			logger.Debug("VideoEncoder::Close()");

			if (ffmpegEncoder != null)
			{
				ffmpegEncoder.DataEncoded -= FFmpegEncoder_DataEncoded;
				ffmpegEncoder.Close();
			}

			CloseDx();
		}

		public event Action<byte[], double> DataEncoded;
		private void OnDataReady(byte[] buf, double time)
		{
			DataEncoded?.Invoke(buf, time);
		}


		private SharpDX.Direct3D11.Device device = null;
		private Texture2DDescription sharedTextureDecription;

		private VertexShader defaultVS = null;
		private PixelShader defaultPS = null;
		private PixelShader rgbToYuvPS = null;
		private PixelShader downscalePS = null;

		private Texture2D rgbTexture = null;
		private ShaderResourceView rgbSRV = null;
		private RenderTargetView rgbRTV = null;
		private ShaderResourceView lumaSRV = null;
		private RenderTargetView lumaRT = null;

		private ShaderResourceView chromaSRV = null;
		private RenderTargetView chromaRT = null;
		private RenderTargetView nv12ChromaRT = null;

		private SamplerState textureSampler = null;

		private void InitDx(VideoEncoderSettings encoderSettings)
		{
			var hwBuffer = videoSource.SharedTexture;
			sharedTextureDecription = hwBuffer.Description;
			srcSize = new Size(sharedTextureDecription.Width, sharedTextureDecription.Height);

			destSize = encoderSettings.Resolution;//new Size(destParams.Width, destParams.Height);
			var adapterId = videoSource.AdapterId;

			InitDevice(adapterId);

			InitShaders();

			InitTextures();

		}

		private void InitDevice(long adapterId)
		{
			using (var adapter = DxTool.FindAdapter1(adapterId))
			{
				var descr = adapter.Description;
				int adapterVenId = descr.VendorId;

				logger.Info("Adapter: " + descr.Description + " " + adapterVenId);

				var flags = DeviceCreationFlags.BgraSupport;

				device = new SharpDX.Direct3D11.Device(adapter, flags);
				using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
				{
					multiThread.SetMultithreadProtected(true);
				}
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

		private void InitTextures()
		{
			logger.Debug("InitRenderResources(...) " + destSize);

			rgbTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = destSize.Width,
				Height = destSize.Height,
				Format = sharedTextureDecription.Format,

				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,

			});

			//// для NV12 размеры должны быть четными!!!
			//var newSize = MediaToolkit.Utils.GraphicTools.DecreaseToEven(new GDI.Size(width, height));
			rgbSRV = new ShaderResourceView(device, rgbTexture,
				new ShaderResourceViewDescription
				{
					Format = sharedTextureDecription.Format,
					Dimension = ShaderResourceViewDimension.Texture2D,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
				});

			rgbRTV = new RenderTargetView(device, rgbTexture,
				new RenderTargetViewDescription
				{
					Format = sharedTextureDecription.Format,//Format.R8G8B8A8_UNorm,
					Dimension = RenderTargetViewDimension.Texture2D,
					Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
				});

			var textureDescr = new SharpDX.Direct3D11.Texture2DDescription
			{
				Width = destSize.Width,
				Height = destSize.Height,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,

				Format = Format.R8_UNorm,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				CpuAccessFlags = CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
			};

			// Init luminance resources...
			using (var tex = new Texture2D(device, textureDescr))
			{
				lumaSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
				{
					Format = textureDescr.Format,
					Dimension = ShaderResourceViewDimension.Texture2D,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource
					{
						MipLevels = 1,
						MostDetailedMip = 0
					},
				});
				lumaRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
				{
					Format = textureDescr.Format,
					Dimension = RenderTargetViewDimension.Texture2D,
					Texture2D = new RenderTargetViewDescription.Texture2DResource { MipSlice = 0 },
				});
			}

			// Init chrominance resources...
			textureDescr.Format = Format.R8G8_UNorm;
			using (var tex = new Texture2D(device, textureDescr))
			{
				chromaSRV = new ShaderResourceView(device, tex, new ShaderResourceViewDescription
				{
					Format = Format.R8G8_UNorm,
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

			// Init nv12 chroma...
			textureDescr.Width = destSize.Width / 2;
			textureDescr.Height = destSize.Height / 2;
			using (var tex = new Texture2D(device, textureDescr))
			{
				nv12ChromaRT = new RenderTargetView(device, tex, new RenderTargetViewDescription
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


		private void PrepareTexture()
		{
			var sharedTexture = videoSource.SharedTexture;
			if (sharedTexture == null)
			{
				logger.Debug("sharedTexture == null");
				return;
			}


			using (var sharedRes = sharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
			{
				var handle = sharedRes.SharedHandle;
				if (handle == IntPtr.Zero)
				{
					logger.Debug("handle == IntPtr.Zero");
					return;
				}

				using (var sharedTex = device.OpenSharedResource<Texture2D>(handle))
				{
					var deviceContext = device.ImmediateContext;

					if (destSize == srcSize)
					{
						deviceContext.CopyResource(sharedTex, rgbTexture);
					}
					else
					{
						Texture2D srcTexture = null;
						try
						{
							//var descr = sharedTex.Description;
							//srcTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
							//{
							//	Width = descr.Width,
							//	Height = descr.Height,
							//	Format = descr.Format,

							//	SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
							//	BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
							//	Usage = SharpDX.Direct3D11.ResourceUsage.Default,
							//	CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
							//	OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
							//	MipLevels = 1,
							//	ArraySize = 1,
							//});
							//deviceContext.CopyResource(sharedTex, srcTexture);

							ResizeTexutre(sharedTex, destSize);
						}
						finally
						{
							if (srcTexture != null)
							{
								srcTexture.Dispose();
								srcTexture = null;
							}
						}

					}

					int destWidth = destSize.Width;
					int destHeight = destSize.Height;

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

					deviceContext.OutputMerger.SetTargets(lumaRT, chromaRT);
					deviceContext.ClearRenderTargetView(lumaRT, SharpDX.Color.Black);
					deviceContext.ClearRenderTargetView(chromaRT, SharpDX.Color.Black);
					deviceContext.PixelShader.SetShader(rgbToYuvPS, null, 0);
					deviceContext.PixelShader.SetShaderResources(0, rgbSRV);
					deviceContext.Draw(vertices.Length, 0);

					deviceContext.PixelShader.SetShader(defaultPS, null, 0);

					SetViewPort(0, 0, destWidth / 2, destHeight / 2);
					deviceContext.OutputMerger.SetTargets(nv12ChromaRT);
					deviceContext.ClearRenderTargetView(nv12ChromaRT, SharpDX.Color.Black);
					deviceContext.PixelShader.SetShaderResources(0, chromaSRV);
					deviceContext.Draw(vertices.Length, 0);

				}

			}

		}

		private void ResizeTexutre(Texture2D srcTexture, GDI.Size destSize)
		{
			DeviceContext deviceContext = device.ImmediateContext;

			int destWidth = destSize.Width;
			int destHeight = destSize.Height;

			ShaderResourceView srcSRV = null;
			try
			{
				srcSRV = new ShaderResourceView(device, srcTexture, new ShaderResourceViewDescription
				{
					Format = srcTexture.Description.Format,
					Dimension = ShaderResourceViewDimension.Texture2D,
					Texture2D = new ShaderResourceViewDescription.Texture2DResource { MipLevels = 1, MostDetailedMip = 0 },
				});

				var vertices = CreateVertices(srcSize, destSize, true);
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

				deviceContext.OutputMerger.SetTargets(rgbRTV);
				deviceContext.ClearRenderTargetView(rgbRTV, SharpDX.Color.Blue);
				//deviceContext.PixelShader.SetShader(pixelShader, null, 0);

				deviceContext.PixelShader.SetShaderResource(0, srcSRV);

				deviceContext.Draw(vertices.Length, 0);

			}
			finally
			{
				if (srcSRV != null)
				{
					srcSRV.Dispose();
					srcSRV = null;
				}
			}

		}

		private void CopyNv12TextureToMemory(out IntPtr destPtr, out int destBufferSize)
		{
			var width = destSize.Width;
			var height = destSize.Height;
			var destPitch = width;
			var destRowNumber = height + height / 2;
			destBufferSize = destPitch * destRowNumber;
			destPtr = Marshal.AllocHGlobal(destBufferSize);

			IntPtr _destPtr = destPtr;
			using (var tex = lumaRT.ResourceAs<Texture2D>())
			{
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
						var srcPitch = dataBox.RowPitch;
						var srcDataSize = dataBox.SlicePitch;
						var srcPtr = dataBox.DataPointer;

						destPitch = width;
						destRowNumber = height;

						for (int i = 0; i < destRowNumber; i++)
						{
							Kernel32.CopyMemory(_destPtr, srcPtr, (uint)destPitch);
							_destPtr += destPitch;
							srcPtr += srcPitch;
						}
					}
					finally
					{
						device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
					}
				}
			}

			using (var tex = nv12ChromaRT.ResourceAs<Texture2D>())
			{
				//var descr = tex.Description;
				//var bytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, tex);
				//var _fileName = "!!!!TEST_" + descr.Format + "_" + descr.Width + "x" + descr.Height + ".raw";
				//File.WriteAllBytes(_fileName, bytes);
				//Console.WriteLine("OutputFile: " + _fileName);

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
						width = stagingDescr.Width;
						height = stagingDescr.Height;

						var srcPitch = dataBox.RowPitch;
						var srcDataSize = dataBox.SlicePitch;
						var srcPtr = dataBox.DataPointer;

						destPitch = 2 * width;
						destRowNumber = height;

						for (int i = 0; i < destRowNumber; i++)
						{
							Kernel32.CopyMemory(_destPtr, srcPtr, (uint)destPitch);
							_destPtr += destPitch;
							srcPtr += srcPitch;
						}
					}
					finally
					{
						device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
					}
				}
			}
			//Utils.TestTools.WriteFile((destPtr), destBufferSize, "TEST_NV12.raw");
		}

		

		private void CloseDx()
		{
			SafeDispose(nv12ChromaRT);
			SafeDispose(chromaRT);
			SafeDispose(chromaSRV);
			SafeDispose(chromaSRV);
			SafeDispose(lumaRT);
			SafeDispose(lumaSRV);
			SafeDispose(rgbSRV);
			SafeDispose(rgbRTV);
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
