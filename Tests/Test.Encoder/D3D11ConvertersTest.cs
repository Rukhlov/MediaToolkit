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
						//deviceCreationFlags |= DeviceCreationFlags.Debug;
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

                ////var fileName = @"Files\2560x1440.bmp";
                ////var srcSize = new Size(2560, 1440);
                //var fileName = @"Files\1920x1080.bmp";
                //var srcSize = new Size(1920, 1080);
                //var srcFormat = PixFormat.RGB32;
                //var texture = MediaToolkit.DirectX.WicTool.CreateTexture2DFromBitmapFile(fileName, device);


                var fileName = @"Files\dcapt_rgb565_1920x1080.raw";
                //var fileName = @"Files\rgb565_1920x1080.raw";
                var srcSize = new Size(1920, 1080);
                var srcFormat = PixFormat.RGB16;
                var srcBytes = File.ReadAllBytes(fileName);


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

				var texture = MediaToolkit.DirectX.DxTool.TextureFromDump(device, texDescr, srcBytes);



				var DestSize = new Size(640, 480);
				var destFormat = PixFormat.NV12;
				//var DestSize = new Size(2560, 1440);

				var videoBuffer = new MemoryVideoBuffer(DestSize, destFormat, 32);

				var scalingFilter = ScalingFilter.Linear;

				D3D11RgbToYuvConverter converter = new D3D11RgbToYuvConverter();
				converter.KeepAspectRatio = true;
				converter.Init(device, srcSize, srcFormat, DestSize, destFormat, scalingFilter);

				var srcFrame = new D3D11VideoFrame(PixFormat.RGB32, texture);

				var destFrame = videoBuffer.GetFrame();

				converter.Process(srcFrame, destFrame);

				var destSize = new Size(destFrame.Width, destFrame.Height);


				var destBuffer = ((VideoFrame)destFrame).ConvertToContiguousBuffer();

				if (destBuffer != null)
				{
					var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
					if (!Directory.Exists(outputPath))
					{
						Directory.CreateDirectory(outputPath);

					}
					var _fileName = destFormat + "_" + destSize.Width + "x" + destSize.Height + "_" +scalingFilter + ".raw";
					var outputFile = Path.Combine(outputPath, _fileName);

					Console.WriteLine("Output File: " + outputFile);
					File.WriteAllBytes(outputFile, destBuffer);

				}
				else
				{
					Console.WriteLine("!!!!!!!!!destData == null");
				}

				destFrame.Dispose();
				srcFrame.Dispose();
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


	}
}
