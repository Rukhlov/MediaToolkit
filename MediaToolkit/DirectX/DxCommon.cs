using MediaToolkit.Core;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GDI = System.Drawing;
using MediaToolkit.SharpDXExt;

namespace MediaToolkit.DirectX
{
	public class WicTool
	{
		public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device)
		{
			using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
			{
				using (var bitmapSource = LoadBitmapSource(factory, fileName))
				{
					var descr = new SharpDX.Direct3D11.Texture2DDescription()
					{
						MipLevels = 1,
						ArraySize = 1,
						SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
						BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
						Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
						CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
						Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

						OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

					};

					return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
				}
			}
		}

		public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr)
		{
			using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
			{
				using (var bitmapSource = LoadBitmapSource(factory, fileName))
				{
					return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
				}
			}
		}

		public static SharpDX.WIC.BitmapSource LoadBitmapSource(SharpDX.WIC.ImagingFactory2 factory, string filename)
		{
			using (var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, filename, SharpDX.WIC.DecodeOptions.CacheOnDemand))
			{
				var formatConverter = new SharpDX.WIC.FormatConverter(factory);

				using (var frameDecode = bitmapDecoder.GetFrame(0))
				{
					formatConverter.Initialize(frameDecode,
						SharpDX.WIC.PixelFormat.Format32bppPRGBA,
						SharpDX.WIC.BitmapDitherType.None,
						null,
						0.0,
						SharpDX.WIC.BitmapPaletteType.Custom);
				}

				return formatConverter;
			}
		}

		public static Texture2D CreateTexture2DFromStream(Stream stream, SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr)
		{
			using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
			{			
				using (var bitmapSource = LoadBitmapSource(factory, stream))
				{
					return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
				}
			}
		}

		public static SharpDX.WIC.BitmapSource LoadBitmapSource(SharpDX.WIC.ImagingFactory2 factory, Stream stream)
		{
			using (var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, stream, SharpDX.WIC.DecodeOptions.CacheOnDemand))
			{
				var formatConverter = new SharpDX.WIC.FormatConverter(factory);

				using (var frameDecode = bitmapDecoder.GetFrame(0))
				{
					formatConverter.Initialize(frameDecode,
						SharpDX.WIC.PixelFormat.Format32bppPRGBA,
						SharpDX.WIC.BitmapDitherType.None,
						null,
						0.0,
						SharpDX.WIC.BitmapPaletteType.Custom);
				}

				return formatConverter;
			}
		}

		public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.WIC.BitmapSource bitmapSource)
		{
			var descr = new SharpDX.Direct3D11.Texture2DDescription()
			{
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
				Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,

				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

			};

			return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
		}

		public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr, SharpDX.WIC.BitmapSource bitmapSource)
		{
			var bitmapSize = bitmapSource.Size;

			descr.Width = bitmapSize.Width;
			descr.Height = bitmapSize.Height;
			descr.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

			int bitmapStride = bitmapSize.Width * 4;
			int bitmapLenght = bitmapSize.Height * bitmapStride;

			using (var buffer = new SharpDX.DataStream(bitmapLenght, true, true))
			{
				bitmapSource.CopyPixels(bitmapStride, buffer);

				var dataRect = new SharpDX.DataRectangle(buffer.DataPointer, bitmapStride);

				return new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
			}
		}

	}

	public enum Transform
	{
		R0,
		R90,
		R180,
		R270,

		FlipX,
		FlipY,
	}

	public class VertexHelper
	{
		public readonly static _Vertex[] DefaultQuad =
		{
			new _Vertex(new Vector3(-1f, -1f, 0f), new Vector2(0f, 1f)),
			new _Vertex(new Vector3(-1f, 1f, 0f), new Vector2(0f, 0f)),
			new _Vertex(new Vector3(1f, -1f, 0f), new Vector2(1f, 1f)),
			new _Vertex(new Vector3(1f, 1f, 0f), new Vector2(1f, 0f)),
		};

		public static _Vertex[] GetQuadVertices(GDI.Size viewSize, GDI.Size targetSize,
				bool aspectRatio = true, Transform transform = Transform.R0)
		{
			/* порядок задания вершин
			 * 1--3
			 * |  |
			 * 0--2
			 */

			//POSITION
			/* координаты вершин (XYZ) 
			 * -1, 1 -- 1, 1
			 *  |	    |
			 * -1,-1 -- 1,-1
			 */

			float x1 = -1f;
			float y1 = -1f;
			float x2 = -1f;
			float y2 = 1f;
			float x3 = 1f;
			float y3 = -1f;
			float x4 = 1f;
			float y4 = 1f;
			float z = 0f;

			//TEXCOORD
			/* координаты текстуры (UV) 
			 * 0,0--1,0
			 * |	 |
			 * 0,1--1,1
			 */

			float u1 = 0;
			float v1 = 1f;
			float u2 = 0;
			float v2 = 0;
			float u3 = 1;
			float v3 = 1;
			float u4 = 1;
			float v4 = 0;

			if (transform != Transform.R0)
			{
				var rotateDegrees = 0;
				var axisVector = new Vector3(0, 0, 0);
				var transformMatrix = Matrix.Identity;

				if (transform == Transform.R90)
				{
					rotateDegrees = 90;
				}
				else if (transform == Transform.R180)
				{
					rotateDegrees = 180;
				}
				else if (transform == Transform.R270)
				{
					rotateDegrees = 270;
				}
				else if (transform == Transform.FlipX)
				{
					axisVector = new Vector3(1f, 0, 0);
				}
				else if (transform == Transform.FlipY)
				{
					axisVector = new Vector3(0, 1f, 0);
				}

				// выполняем преобразование в обычных координатах
				// и потом пересчитываетм в координаты текстуры
				var coords = new Matrix(x1, y1, z, 0f, x2, y2, z, 0f, x3, y3, z, 0f, x4, y4, z, 0f);
				if (rotateDegrees > 0)
				{
					float angle = DegreesToRads(rotateDegrees);
					var rotationMatrix = Matrix.RotationZ(angle);
					coords = Matrix.Multiply(coords, rotationMatrix);
				}

				if (!axisVector.IsZero)
				{
					var flipMatrix = Matrix.RotationAxis(axisVector, (float)Math.PI);
					coords = Matrix.Multiply(coords, flipMatrix);
				}

				u1 = coords.M11 / 2.0f + 0.5f;
				v1 = -(coords.M12 / 2.0f - 0.5f);
				u2 = coords.M21 / 2.0f + 0.5f;
				v2 = -(coords.M22 / 2.0f - 0.5f);
				u3 = coords.M31 / 2.0f + 0.5f;
				v3 = -(coords.M32 / 2.0f - 0.5f);
				u4 = coords.M41 / 2.0f + 0.5f;
				v4 = -(coords.M42 / 2.0f - 0.5f);
			}

			if (aspectRatio)
			{
				double targetWidth = targetSize.Width;
				double targetHeight = targetSize.Height;

				if (transform == Transform.R90 || transform == Transform.R270)
				{
					targetWidth = targetSize.Height;
					targetHeight = targetSize.Width;
				}

				double srcWidth = viewSize.Width;
				double srcHeight = viewSize.Height;

				double targetRatio = targetWidth / targetHeight;
				double containerRatio = srcWidth / srcHeight;

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
				new _Vertex(new Vector3(x1, y1, z), new Vector2(u1, v1)),
				new _Vertex(new Vector3(x2, y2, z), new Vector2(u2, v2)),
				new _Vertex(new Vector3(x3, y3, z), new Vector2(u3, v3)),
				new _Vertex(new Vector3(x4, y4, z), new Vector2(u4, v4)),
			};
		}


		public static float DegreesToRads(int rotateDegrees)
		{
			return (float)(Math.PI * rotateDegrees / 180.0);
		}

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



	public class HlslCompiler
	{
		private static Dictionary<string, CompilationResult> compilationResultDict = new Dictionary<string, CompilationResult>();

		private readonly static string resourceNamespace = "MediaToolkit.DirectX.Shaders";
		private readonly static string profileLevel = "4_0";

		public readonly static string vsProfile = "vs_" + profileLevel;
		public readonly static string psProfile = "ps_" + profileLevel;

		public static byte[] GetShaderBytes(string name, string entryPoint, string profile)
		{
			ShaderBytecode bytes = null;
			string key = name + "_" + entryPoint + "_" + profile;

			if (compilationResultDict.ContainsKey(key))
			{
				var compResult = compilationResultDict[key];
				if (compResult.HasErrors)
				{//...
				}

				bytes = compResult.Bytecode;
			}
			else
			{
				var resourceName = name + ".hlsl";
				var compResult = CompileShaderFromResources(resourceName, entryPoint, profile, null);
				bytes = compResult.Bytecode;

				compilationResultDict.Add(key, compResult);
			}

			return bytes;
		}

		public static byte[] GetPixelShaderBytes(string name, string entryPoint)
		{
			return GetShaderBytes(name, entryPoint, psProfile);
		}

		public static PixelShader GetPixelShader(SharpDX.Direct3D11.Device device, string name, string entryPoint)
		{
			var bytes = GetPixelShaderBytes(name, entryPoint);
			return new PixelShader(device, bytes);
		}

		public static byte[] GetVertexShaderBytes(string name, string entryPoint)
		{
			return GetShaderBytes(name, entryPoint, vsProfile);
		}

		public static VertexShader GetVertexShader(SharpDX.Direct3D11.Device device, string name, string entryPoint)
		{
			var bytes = GetVertexShaderBytes(name, entryPoint);
			return new VertexShader(device, bytes);
		}

		public static void Shutdown()
		{
			foreach (var key in compilationResultDict.Keys)
			{
				var shader = compilationResultDict[key];
				if (!shader.IsDisposed)
				{
					shader.Dispose();
				}
			}
		}

		public static SharpDX.D3DCompiler.CompilationResult CompileShaderFromResources(string file, string entryPoint, string profile,
			SharpDX.Direct3D.ShaderMacro[] defines = null)
		{

			SharpDX.D3DCompiler.ShaderFlags flags = SharpDX.D3DCompiler.ShaderFlags.None;
#if DEBUG
			//flags |= SharpDX.D3DCompiler.ShaderFlags.Debug | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
#endif

			SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None;

			return CompileShaderFromResources(file, entryPoint, profile, flags, effectFlags);
		}

		private static SharpDX.D3DCompiler.CompilationResult CompileShaderFromResources(string file, string entryPoint, string profile,
			SharpDX.D3DCompiler.ShaderFlags shaderFlags = SharpDX.D3DCompiler.ShaderFlags.None,
			SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None,
			SharpDX.Direct3D.ShaderMacro[] defines = null)
		{

			var assembly = Assembly.GetAssembly(typeof(HlslCompiler));
			//var assembly = Assembly.GetExecutingAssembly();
			var resourceName = resourceNamespace + "." + file;

			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			{
				using (StreamReader ms = new StreamReader(stream))
				{
					var shaderSource = ms.ReadToEnd();
					var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderSource, entryPoint, profile, shaderFlags, effectFlags, defines, null, file);

					return result;
				}
			}
		}
	}

	public class DxTool
	{
        public static SharpDX.Direct3D11.Device CreateMultithreadDevice(Adapter adapter)
        {
            DeviceCreationFlags deviceCreationFlags = DeviceCreationFlags.BgraSupport;

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                SharpDX.Direct3D.FeatureLevel.Level_11_1,
                SharpDX.Direct3D.FeatureLevel.Level_11_0,
                SharpDX.Direct3D.FeatureLevel.Level_10_1,
            };

            return CreateMultithreadDevice(adapter, deviceCreationFlags, featureLevel);
        }


        public static SharpDX.Direct3D11.Device CreateMultithreadDevice(Adapter adapter,
            DeviceCreationFlags deviceCreationFlags,
            params SharpDX.Direct3D.FeatureLevel[] featureLevel)
		{
#if DEBUG
			//deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif

			var device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);
			using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
			{
				multiThread.SetMultithreadProtected(true);
			}

			return device;
		}

		public static void SafeDispose(SharpDX.DisposeBase dispose)
		{
			if (dispose != null && !dispose.IsDisposed)
			{
				dispose.Dispose();
				dispose = null;
			}
		}

		public static IEnumerable<Format> GetDxgiFormat(PixFormat pixFormat, bool videoFormatSupported = false)
		{
			List<Format> formats = new List<Format>();

			if (pixFormat == PixFormat.RGB32)
			{
				formats.Add(Format.R8G8B8A8_UNorm);
				//formats.Add(Format.B8G8R8A8_UNorm);
			}
			else if (pixFormat == PixFormat.RGB24)
			{
				formats.Add(Format.R8G8B8A8_UNorm);
				//formats.Add(Format.B8G8R8X8_UNorm);
			}
			else if(pixFormat == PixFormat.RGB16)
			{
				var f = videoFormatSupported ? Format.B5G6R5_UNorm : Format.R16_UNorm;
				formats.Add(f);
			}
			else if (pixFormat == PixFormat.RGB15)
			{
				var f = videoFormatSupported ? Format.B5G5R5A1_UNorm : Format.R16_UNorm;
				formats.Add(f);
			}
			else if (pixFormat == PixFormat.I444 || 
				pixFormat == PixFormat.I422 || 
				pixFormat == PixFormat.I420)
			{
				if (videoFormatSupported)
				{// 

				}

			    formats.Add(Format.R8_UNorm);
				formats.Add(Format.R8_UNorm);
				formats.Add(Format.R8_UNorm);
			}
			else if(pixFormat == PixFormat.NV12)
			{
				if (videoFormatSupported)
				{
					formats.Add(Format.NV12);
				}
				else
				{
					formats.Add(Format.R8_UNorm);
					formats.Add(Format.R8G8_UNorm);
				}
			}
			else if (pixFormat == PixFormat.R8)
			{
				formats.Add(Format.R8_UNorm);
			}
			else if (pixFormat == PixFormat.R8G8)
			{
				formats.Add(Format.R8G8_UNorm);
			}

			return formats;
		}

		public unsafe static Texture2D TextureFromDump(SharpDX.Direct3D11.Device device, Texture2DDescription descr, byte[] srcBuffer)
		{
			//descr.CpuAccessFlags = CpuAccessFlags.None;
			//descr.Usage = ResourceUsage.Default;
			//descr.BindFlags = SharpDX.Direct3D11.BindFlags.None;
			//descr.OptionFlags = ResourceOptionFlags.None;

			int width = descr.Width;
			int height = descr.Height;
			var format = descr.Format;

			int rowPitch = 0;
			int slicePitch = 0;
			if (format == Format.R8G8B8A8_UNorm)
			{
				rowPitch = 4 * width;
				slicePitch = rowPitch * height;

			}
			else if (format == Format.NV12)
			{// Width and height must be even.

				rowPitch = width;
				slicePitch = rowPitch * (height + height / 2);
			}
			//else if (format == Format.R16_UNorm || format == Format.R16_UInt)
			//{
			//    rowPitch = width;
			//}
			else if (format == Format.R8G8_UNorm
				|| format == Format.R16_UNorm
				|| format == Format.R16_SInt
				|| format == Format.R16_UInt || format == Format.B5G6R5_UNorm)
			{
				rowPitch = 2 * width;
			}
			else
			{// not supported...
				rowPitch = width;
			}

			fixed (byte* ptr = srcBuffer)
			{
				DataBox[] initData =
				{
					new DataBox((IntPtr)ptr,  rowPitch, 0),
				};

				return new Texture2D(device, descr, initData);
			}

		}

		public static byte[] DumpTexture(SharpDX.Direct3D11.Device device, Texture2D texture)
		{
			byte[] destBuffer = null;

			var stagingDescr = texture.Description;
			stagingDescr.BindFlags = BindFlags.None;
			stagingDescr.CpuAccessFlags = CpuAccessFlags.Read;
			stagingDescr.Usage = ResourceUsage.Staging;
			stagingDescr.OptionFlags = ResourceOptionFlags.None;
            stagingDescr.ArraySize = 1;
			using (var stagingTexture = new Texture2D(device, stagingDescr))
			{
				//device.ImmediateContext.CopyResource(texture, stagingTexture);
                device.ImmediateContext.CopySubresourceRegion(texture, 0, null, stagingTexture, 0);
                var dataBox = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
				try
				{
					int width = stagingDescr.Width;
					int height = stagingDescr.Height;

					var srcPitch = dataBox.RowPitch;
					var srcDataSize = dataBox.SlicePitch;
					var srcPtr = dataBox.DataPointer;

					var destPitch = 4 * width;
					var destRowNumber = height;
					var destBufferSize = destPitch * destRowNumber;

					if (stagingDescr.Format == Format.R8G8B8A8_SNorm || stagingDescr.Format == Format.B8G8R8A8_UNorm)
					{

					}
					else if (stagingDescr.Format == Format.NV12)
					{
						destPitch = width;
						destRowNumber = height + height / 2;
						destBufferSize = destPitch * destRowNumber;
					}
					else if (stagingDescr.Format == Format.R8_UNorm)
					{
						destPitch = width;
						destRowNumber = height;
						destBufferSize = destPitch * destRowNumber;
					}
					else if (stagingDescr.Format == Format.R8G8_UNorm || stagingDescr.Format == Format.R16_UNorm)
					{
						destPitch = 2 * width;
						destRowNumber = height;
						destBufferSize = destPitch * destRowNumber;
					}

					destBuffer = new byte[destBufferSize];
					int bufOffset = 0;
					for (int i = 0; i < destRowNumber; i++)
					{
						System.Runtime.InteropServices.Marshal.Copy(srcPtr, destBuffer, bufOffset, destPitch);
						bufOffset += destPitch;
						srcPtr += srcPitch;
					}

				}
				finally
				{
					device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
				}

			}

			return destBuffer;
		}

		public static Adapter1 FindAdapter1(long luid)
		{
			Adapter1 adapter1 = null;
			using (var dxgiFactory = new SharpDX.DXGI.Factory1())
			{

				if (luid > 0)
				{
					var adapters = dxgiFactory.Adapters1;
					for (int i = 0; i < adapters.Length; i++)
					{
						var _adapter = adapters[i];
						if (_adapter.Description1.Luid == luid)
						{
							adapter1 = _adapter;
							continue;
						}

						_adapter.Dispose();
					}
				}

				if (adapter1 == null)
				{
					adapter1 = dxgiFactory.GetAdapter1(0);
				}
			}

			return adapter1;
		}

		public static string LogDxInfo()
		{
			StringBuilder log = new StringBuilder();

			using (var dxgiFactory = new SharpDX.DXGI.Factory1())
			{
				var adapters = dxgiFactory.Adapters1;

				var adapterInfo = LogDxAdapters(adapters);

				log.AppendLine(adapterInfo);

			}

			return log.ToString();
		}


		public static string LogDxAdapters(Adapter1[] adapters)
		{
			StringBuilder log = new StringBuilder();
			log.AppendLine("");

			//foreach (var _adapter in adapters)
			for (int adapterIndex = 0; adapterIndex < adapters.Length; adapterIndex++)
			{
				var _adapter = adapters[adapterIndex];
				try
				{
					var adaptDescr = _adapter.Description1;

					var featureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(_adapter);
					//var isSupported = SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(_adapter, SharpDX.Direct3D.FeatureLevel.Level_12_1);

					bool success = GetSupportedFeatureLevel(_adapter, out var feature);

					log.AppendLine("-------------------------------------");
					log.AppendLine("#" + adapterIndex + " " + string.Join("| ", adaptDescr.Description, adaptDescr.DeviceId, adaptDescr.VendorId, featureLevel));

					var outputs = _adapter.Outputs;

					//foreach (var _output in _adapter.Outputs)
					for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
					{
						var _output = outputs[outputIndex];
						try
						{
							var outputDescr = _output.Description;
							var bound = outputDescr.DesktopBounds;
							var rect = new GDI.Rectangle
							{
								X = bound.Left,
								Y = bound.Top,
								Width = (bound.Right - bound.Left),
								Height = (bound.Bottom - bound.Top),
							};

							log.AppendLine("#" + outputIndex + " " + string.Join("| ", outputDescr.DeviceName, rect.ToString()));

						}
						finally
						{
							_output?.Dispose();
						}

					}
				}
				catch (SharpDXException ex)
				{
					log.AppendLine(ex.Message);
				}
				finally
				{
					_adapter?.Dispose();
				}

			}

			return log.ToString();
		}

		public static int GetDefaultAdapterFeatureLevel()
		{
			int featureLevel = 0;

			SharpDX.Direct3D.FeatureLevel[] features =
			{
                //SharpDX.Direct3D.FeatureLevel.Level_12_1,
                //SharpDX.Direct3D.FeatureLevel.Level_12_0,

                SharpDX.Direct3D.FeatureLevel.Level_11_1,
				SharpDX.Direct3D.FeatureLevel.Level_11_0,

				SharpDX.Direct3D.FeatureLevel.Level_10_1,
				SharpDX.Direct3D.FeatureLevel.Level_10_0,

				SharpDX.Direct3D.FeatureLevel.Level_9_3,
				SharpDX.Direct3D.FeatureLevel.Level_9_2,
				SharpDX.Direct3D.FeatureLevel.Level_9_1,
			};

			SharpDX.Direct3D11.Device device = null;
			try
			{
				var flags = DeviceCreationFlags.None;
				//var flags = DeviceCreationFlags.BgraSupport;
				device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags, features);
				featureLevel = (int)device.FeatureLevel;


			}
			catch (Exception ex) { }
			finally
			{
				if (device != null)
				{
					device.Dispose();
					device = null;
				}
			}

			return featureLevel;
		}

		public static bool GetSupportedFeatureLevel(Adapter adapter, out SharpDX.Direct3D.FeatureLevel outputLevel)
		{
			bool Result = false;
			outputLevel = SharpDX.Direct3D.FeatureLevel.Level_9_1;

			SharpDX.Direct3D.FeatureLevel[] features =
			{
				SharpDX.Direct3D.FeatureLevel.Level_12_1,
				SharpDX.Direct3D.FeatureLevel.Level_12_0,

				SharpDX.Direct3D.FeatureLevel.Level_11_1,
				SharpDX.Direct3D.FeatureLevel.Level_11_0,

				SharpDX.Direct3D.FeatureLevel.Level_10_1,
				SharpDX.Direct3D.FeatureLevel.Level_10_0,

				SharpDX.Direct3D.FeatureLevel.Level_9_3,
				SharpDX.Direct3D.FeatureLevel.Level_9_2,
				SharpDX.Direct3D.FeatureLevel.Level_9_1,
			};

			SharpDX.Direct3D11.Device device = null;
			try
			{
				device = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport, features);
				outputLevel = device.FeatureLevel;

				Result = true;

			}
			catch (Exception) { }
			finally
			{
				if (device != null)
				{
					device.Dispose();
					device = null;
				}
			}

			return Result;
		}

		//public static FeatureLevel GetSupportedFeatureLevel()
		//{
		//    SharpDX.Direct3D.FeatureLevel outputLevel;
		//    var device = new SharpDX.Direct3D11.Device(IntPtr.Zero);
		//    DeviceContext context;
		//    D3D11.CreateDevice(null, SharpDX.Direct3D.DriverType.Hardware, IntPtr.Zero, DeviceCreationFlags.None, null, 0, D3D11.SdkVersion, device, out outputLevel,
		//                       out context).CheckError();
		//    context.Dispose();
		//    device.Dispose();
		//    return outputLevel;
		//}

		public static void TextureToBitmap(Texture2D texture, ref GDI.Bitmap bmp)
		{

			var descr = texture.Description;
			if (descr.Format != Format.B8G8R8A8_UNorm)
			{
				throw new Exception("Invalid texture format " + descr.Format);
			}

			if (descr.Width <= 0 || descr.Height <= 0)
			{
				throw new Exception("Invalid texture size: " + descr.Width + " " + descr.Height);
			}

			if (bmp == null)
			{
				bmp = new GDI.Bitmap(descr.Width, descr.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
			}

			if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
			{
				throw new Exception("Invalid bitmap format " + bmp.PixelFormat);

			}

			if (bmp.Width != descr.Width || bmp.Height != descr.Height)
			{
				throw new Exception("Invalid params");

			}

			using (Surface surface = texture.QueryInterface<Surface>())
			{
				try
				{
					var srcData = surface.Map(SharpDX.DXGI.MapFlags.Read);

					int width = bmp.Width;
					int height = bmp.Height;
					var rect = new GDI.Rectangle(0, 0, width, height);
					var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
					try
					{
						int bytesPerPixel = GDI.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
						IntPtr srcPtr = srcData.DataPointer;
						int srcOffset = rect.Top * srcData.Pitch + rect.Left * bytesPerPixel;

						srcPtr = IntPtr.Add(srcPtr, srcOffset);

						var destPtr = destData.Scan0;
						for (int row = rect.Top; row < rect.Bottom; row++)
						{
							Utilities.CopyMemory(destPtr, srcPtr, width * bytesPerPixel);
							srcPtr = IntPtr.Add(srcPtr, srcData.Pitch);
							destPtr = IntPtr.Add(destPtr, destData.Stride);

						}
					}
					finally
					{
						bmp.UnlockBits(destData);
					}
				}
				finally
				{
					surface.Unmap();
				}
			}
		}

		public static byte[] GetTextureBytes(Texture2D texture)
		{
			byte[] textureBytes = null;
			using (Surface surface = texture.QueryInterface<Surface>())
			{
				try
				{
					surface.Map(SharpDX.DXGI.MapFlags.Read, out DataStream dataStream);
					textureBytes = dataStream.ReadRange<byte>((int)dataStream.Length);
				}
				finally
				{
					surface.Unmap();
				}
			}

			return textureBytes;
		}


		public static Texture2D GetTexture(GDI.Bitmap bitmap, SharpDX.Direct3D11.Device device)
		{
			Texture2D texture = null;

			var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);

			if (bitmap.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
			{
				var _bitmap = bitmap.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);

				bitmap.Dispose();
				bitmap = _bitmap;
			}

			var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
			try
			{
				//Windows 7 
				var descr = new SharpDX.Direct3D11.Texture2DDescription
				{
					Width = bitmap.Width,
					Height = bitmap.Height,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,
					Format = Format.B8G8R8A8_UNorm,
					BindFlags = BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,

				};

				/*
				var descr = new SharpDX.Direct3D11.Texture2DDescription
				{
					Width = bitmap.Width,
					Height = bitmap.Height,
					ArraySize = 1,
					//BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
					//Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
					//CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
					MipLevels = 1,
					//OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
					SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				};
				*/

				var dataRect = new SharpDX.DataRectangle(data.Scan0, data.Stride);

				texture = new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
			}
			finally
			{
				bitmap.UnlockBits(data);
			}

			return texture;
		}
	}

	public class ColorSpaceHelper
	{
		static ColorSpaceHelper()
		{
			rgbToYuvDict = new Dictionary<ColorSpace, Dictionary<ColorRange, float[]>>
			{
				{
					ColorSpace.BT601,
					new Dictionary<ColorRange, float[]>
					{
						{ ColorRange.Partial, RgbToYuv601 },
						{ ColorRange.Full, RgbToYuv601Full }
					}
				},

				{
					ColorSpace.BT709,
					new Dictionary<ColorRange, float[]>
					{
						{ ColorRange.Partial, RgbToYuv709 },
						{ ColorRange.Full, RgbToYuv709Full }
					}
				}
			};

			yuvToRgbDict = new Dictionary<ColorSpace, Dictionary<ColorRange, float[]>>
			{
				{
					ColorSpace.BT601,
					new Dictionary<ColorRange, float[]>
					{
						{ ColorRange.Partial, YuvToRgb601 },
						{ ColorRange.Full, YuvToRgb601Full }
					}
				},

				{
					ColorSpace.BT709,
					new Dictionary<ColorRange, float[]>
					{
						{ ColorRange.Partial, YuvToRgb709 },
						{ ColorRange.Full, YuvToRgb709Full }
					}
				}
			};
		}

		private readonly static Dictionary<ColorSpace, Dictionary<ColorRange, float[]>> rgbToYuvDict = null;
		private readonly static Dictionary<ColorSpace, Dictionary<ColorRange, float[]>> yuvToRgbDict = null;

		public static Matrix GetRgbToYuvMatrix(ColorSpace colorSpace = ColorSpace.BT601, ColorRange colorRange = ColorRange.Partial)
		{
			var colorDict = rgbToYuvDict[colorSpace];
			return new Matrix(colorDict[colorRange]);
		}

		public static Matrix GetYuvToRgbMatrix(ColorSpace colorSpace = ColorSpace.BT601, ColorRange colorRange = ColorRange.Partial)
		{
			var colorDict = yuvToRgbDict[colorSpace];
			return new Matrix(colorDict[colorRange]);
		}


		public readonly static float[] RgbToYuv601 =
		{
			 0.256788f,  0.504129f,  0.097906f,  0.062745f,
			-0.148223f, -0.290993f,  0.439216f,  0.501961f,
			 0.439216f, -0.367788f, -0.071427f,  0.501961f,
			 0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] RgbToYuv601Full =
		{
			 0.299000f,  0.587000f,  0.114000f,  0.000000f,
			-0.168074f, -0.329965f,  0.498039f,  0.501961f,
			 0.498039f, -0.417046f, -0.080994f,  0.501961f,
			 0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] RgbToYuv709 =
		{
			 0.182586f,  0.614231f,  0.062007f,  0.062745f,
			-0.100644f, -0.338572f,  0.439216f,  0.501961f,
			 0.439216f, -0.398942f, -0.040274f,  0.501961f,
			 0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] RgbToYuv709Full =
		{
			 0.212600f,  0.715200f,  0.072200f,  0.000000f,
			-0.114123f, -0.383916f,  0.498039f,  0.501961f,
			 0.498039f, -0.452372f, -0.045667f,  0.501961f,
			 0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] YuvToRgb601 =
		{
			1.000000f,  0.000000f,  1.407520f, -0.706520f,
			1.000000f, -0.345491f, -0.716948f,  0.533303f,
			1.000000f,  1.778976f,  0.000000f, -0.892976f,
			0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] YuvToRgb601Full =
		{
			1.164384f,  0.000000f,  1.596027f, -0.874202f,
			1.164384f, -0.391762f, -0.812968f,  0.531668f,
			1.164384f,  2.017232f,  0.000000f, -1.085631f,
			0.000000f,  0.000000f,  0.000000f,  1.000000f
		};

		public readonly static float[] YuvToRgb709 =
		{
			1.164384f, 0.000000f, 1.792741f, -0.972945f,
			1.164384f, -0.213249f, -0.532909f, 0.301483f,
			1.164384f, 2.112402f, 0.000000f, -1.133402f,
			0.000000f, 0.000000f, 0.000000f, 1.000000f
		};

		public readonly static float[] YuvToRgb709Full =
		{
			1.000000f, 0.000000f, 1.581000f, -0.793600f,
			1.000000f, -0.188062f, -0.469967f, 0.330305f,
			1.000000f, 1.862906f, 0.000000f, -0.935106f,
			0.000000f, 0.000000f, 0.000000f, 1.000000f
		};

	}

}

namespace MediaToolkit.SharpDXExt
{

	public static class SharpDXExt
	{
		public static void SetViewPort(this RasterizerStage resterizer, GDI.Rectangle rect, float minDepth = 0f, float maxDepth = 1f)
		{
			resterizer.SetViewport(rect.X, rect.Y, rect.Width, rect.Height, minDepth, maxDepth);
		}

		public static void SafeDispose(this SharpDX.DisposeBase dispose)
		{
			if (!dispose.IsDisposed)
			{
				dispose.Dispose();
				dispose = null;
			}
		}
	}

}