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

namespace Test.Encoder
{
	class D3D11ConvertersTest
	{

		public static void Run()
		{
			Console.WriteLine("D3D11ConvertersTest::Run()");

			Device device = null; 
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
						device = new Device(adapter, deviceCreationFlags);
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

                var DestSize = new Size(1280, 960);
                //var DestSize = new Size(2560, 1440);
                var videoBuffer = new MemoryVideoBuffer(DestSize, PixFormat.NV12, 32);

				var fileName = @"Files\1920x1080.bmp";

				var texture = MediaToolkit.DirectX.WicTool.CreateTexture2DFromBitmapFile(fileName, device);


				D3D11RgbToYuvConverter converter = new D3D11RgbToYuvConverter();

				//D3D11RgbToNv12Converter converter = new D3D11RgbToNv12Converter();

				converter.Init(device, DestSize, videoBuffer.Format);

				var frame = videoBuffer.GetFrame();
				converter.Process(texture, frame);

				var destSize = new Size(frame.Width, frame.Height);
				var destFormat = frame.Format;
				var destBuffer = ConvertToContiguousBuffer(frame.Buffer, destSize, destFormat);

				if (destBuffer != null)
				{
					var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
					if (!Directory.Exists(outputPath))
					{
						Directory.CreateDirectory(outputPath);

					}
					var _fileName = destFormat + "_" + destSize.Width + "x" + destSize.Height + ".raw";
					var outputFile = Path.Combine(outputPath, _fileName);

					Console.WriteLine("Output File: " + outputFile);
					File.WriteAllBytes(outputFile, destBuffer);

				}
				else
				{
					Console.WriteLine("!!!!!!!!!destData == null");
				}

				converter.Close();
				videoBuffer.Dispose();
				texture?.Dispose();
				device?.Dispose();



			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

		}


		private static unsafe byte[] ConvertToContiguousBuffer(IFrameBuffer[] frameBuffer, Size size, MediaToolkit.Core.PixFormat format)
		{
			byte[] buffer = null;
			if (format == MediaToolkit.Core.PixFormat.NV12)
			{
				var lumaStride = size.Width;
				var lumaHeight = size.Height;

				var chromaStride = size.Width;
				var chromaHeight = size.Height / 2;

				//lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
				//chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);
				var bufferSize = lumaStride * lumaHeight + chromaStride * chromaHeight;
				buffer = new byte[bufferSize];

				int offset = 0;
				var pData = frameBuffer[0].Data;
				var dataStride = frameBuffer[0].Stride;
				for (int row = 0; row < lumaHeight; row++)
				{ //Y
					Marshal.Copy(pData, buffer, offset, lumaStride);
					offset += lumaStride;
					pData += dataStride;
				}

				pData = frameBuffer[1].Data;
				dataStride = frameBuffer[1].Stride;
				for (int row = 0; row < chromaHeight; row++)
				{// packed CbCr
					Marshal.Copy(pData, buffer, offset, chromaStride);
					offset += chromaStride;
					pData += dataStride;
				}
			}
			else if (format == MediaToolkit.Core.PixFormat.RGB32
			   || format == MediaToolkit.Core.PixFormat.RGB24
			   || format == MediaToolkit.Core.PixFormat.RGB565)
			{
				int bytesPerPixel = 4;
				if (format == MediaToolkit.Core.PixFormat.RGB24)
				{
					bytesPerPixel = 3;
				}
				else if (format == MediaToolkit.Core.PixFormat.RGB565)
				{
					bytesPerPixel = 2;
				}

				var rgbStride = bytesPerPixel * size.Width;

				var bufferSize = rgbStride * size.Height;
				buffer = new byte[bufferSize];

				int offset = 0;
				var pData = frameBuffer[0].Data;
				var dataStride = frameBuffer[0].Stride;

				for (int row = 0; row < size.Height; row++)
				{
					Marshal.Copy(pData, buffer, offset, rgbStride);
					offset += rgbStride;
					pData += dataStride;
				}
			}
			else if (format == MediaToolkit.Core.PixFormat.I444 ||
				format == MediaToolkit.Core.PixFormat.I422 ||
				format == MediaToolkit.Core.PixFormat.I420)
			{
				var lumaHeight = size.Height;
				var chromaHeight = size.Height;

				var lumaStride = size.Width;
				var chomaStride = size.Width;

				if (format == MediaToolkit.Core.PixFormat.I420)
				{
					chromaHeight = size.Height / 2;
					chomaStride = size.Width / 2;

				}
				else if (format == MediaToolkit.Core.PixFormat.I422)
				{
					chomaStride = size.Width / 2;
				}

				//lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
				//chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);

				var bufferSize = lumaStride * lumaHeight + 2 * chomaStride * chromaHeight;

				buffer = new byte[bufferSize];

				int offset = 0;
				var pData = frameBuffer[0].Data;
				var dataStride = frameBuffer[0].Stride;

				for (int row = 0; row < lumaHeight; row++)
				{//Y
					Marshal.Copy(pData, buffer, offset, lumaStride);
					offset += lumaStride;
					pData += dataStride;
				}

				for (int i = 1; i < 3; i++)
				{// CbCr
					pData = frameBuffer[i].Data;
					dataStride = frameBuffer[i].Stride;
					for (int row = 0; row < chromaHeight; row++)
					{
						Marshal.Copy(pData, buffer, offset, chomaStride);
						offset += chomaStride;
						pData += dataStride;
					}

				}
			}


			return buffer;
		}

	}
}
