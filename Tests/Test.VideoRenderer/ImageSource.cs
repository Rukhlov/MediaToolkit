using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.VideoRenderer
{
	class SampleSource
	{

		public void Start()
		{
			if (running)
			{
				return;
			}

			//var testSeqDir = @"D:\testBMP\";
			//var di = new DirectoryInfo(testSeqDir);
			//var files = di.GetFiles().Take(60);
			//foreach (var f in files)
			//{
			//    var bytes = File.ReadAllBytes(f.FullName);
			//    testBitmapSequence.Add(bytes);
			//}


			var testFile5 = @".\TestBmp\1920x1080_bmdFormat10BitYUV.raw";
			var testFile2 = @".\TestBmp\1920x1080_bmdFormat8BitYUV.raw";
			var testFile3 = @".\TestBmp\1920x1080_Argb32.raw";

			var testArgb = File.ReadAllBytes(testFile3);

			//var canvaspng = @".\TestBmp\canvas.png";
			var testBytes = File.ReadAllBytes(testFile2);
			var testBytes5 = File.ReadAllBytes(testFile5);

			//var fourCC = new FourCC("V210");


			var V210FourCC = new FourCC(0x30313256);

			var UYVYFourCC = new FourCC(0x59565955);

			var NV12FourCC = new FourCC("NV12");

			// var format = VideoFormatGuids.FromFourCC(v210FourCC);
			// var format = VideoFormatGuids.FromFourCC(UYVYFourCC);

			//var format = VideoFormatGuids.FromFourCC(NV12FourCC); //VideoFormatGuids.NV12;

			var format = VideoFormatGuids.Argb32;
			var sampleArgs = new MfVideoArgs
			{
				Width = 1920,
				Height = 1080,
				Format = format, //VideoFormatGuids.Uyvy, //VideoFormatGuids.NV12,//MFVideoFormat_v210,

			};




			var producerTask = Task.Run(() =>
			{

				running = true;
				Stopwatch sw = new Stopwatch();
				int fps = 30;
				int interval = (int)(1000.0 / fps);

				int _count = 1;

				long globalTime = 0;

				Bitmap bmp = new Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


				var g = Graphics.FromImage(bmp);
				g.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), new System.Drawing.Font(FontFamily.GenericMonospace, 120), Brushes.Yellow, 0f, 0f);


				var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
				var size = data.Stride * data.Height;


				var sample = MediaFactory.CreateSample();
				var mb = MediaFactory.CreateMemoryBuffer(size);

				var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

				Kernel32.CopyMemory(pBuffer, data.Scan0, (uint)size);
				//Marshal.Copy(testArgb, 0, pBuffer, testArgb.Length);

				mb.CurrentLength = size;

				mb.Unlock();

				sample.AddBuffer(mb);

				bmp.UnlockBits(data);
				g.Dispose();



				Random rnd = new Random();

				Stopwatch timer = Stopwatch.StartNew();
				while (running)
				{

					if (paused)
					{
						Thread.Sleep(100);
						continue;
					}


					UpdateSample(bmp, mb);


					globalTime += sw.ElapsedMilliseconds;
					sw.Restart();

					var _rndOffset = 0;//rnd.Next(-16, 16);

					//if (_count%2 == 0)
					//{
					//	_rndOffset = 66;
					//}


					//globalTime += _rndOffset;

					var time = timer.ElapsedMilliseconds + _rndOffset;
					sample.SampleTime = MfTool.SecToMfTicks((time / 1000.0));

					//sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0) );
					sample.SampleDuration = MfTool.SecToMfTicks((interval / 1000.0));

					//sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0));
					//sample.SampleDuration = MfTool.SecToMfTicks(((int)interval / 1000.0));

					SampleReady?.Invoke(sample);


					var msec = sw.ElapsedMilliseconds;

					var delay = interval - msec;
					if (delay < 0)
					{
						delay = 1;
					}
					//Console.WriteLine(delay);
					// var delay = 1;
					Thread.Sleep((int)delay);
					//var elapsedMilliseconds = sw.ElapsedMilliseconds;
					//sw.Restart();

					//globalTime += elapsedMilliseconds;
					_count++;

					//Console.WriteLine(globalTime/1000.0 + " " + _count + " " + delay);

					//Console.SetCursorPosition(0, Console.CursorTop - 1);

				}

				sample?.Dispose();

				mb.Dispose();
				bmp.Dispose();

			});
		}



		public void UpdateSample(Bitmap bmp, MediaBuffer mb)
		{
			var g = Graphics.FromImage(bmp);

			g.FillRectangle(Brushes.Black, new Rectangle(0, 0, bmp.Width, bmp.Height));
			g.DrawString(DateTime.Now.ToString("HH:mm:ss.fff"), new System.Drawing.Font(FontFamily.GenericMonospace, 120), Brushes.Yellow, 0f, 0f);

			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
			var size = data.Stride * data.Height;

			var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

			Kernel32.CopyMemory(pBuffer, data.Scan0, (uint)size);
			//Marshal.Copy(testArgb, 0, pBuffer, testArgb.Length);

			mb.CurrentLength = size;

			mb.Unlock();



			bmp.UnlockBits(data);
			g.Dispose();
			//bmp.Dispose();
		}


		public event Action<Sample> SampleReady;
		public void Pause()
		{
			paused = !paused;
		}

		private bool paused = false;
		private bool running = false;

		public void Stop()
		{
			running = false;
		}

		public void Start1()
		{

			var flags = DeviceCreationFlags.VideoSupport |
			DeviceCreationFlags.BgraSupport |
			DeviceCreationFlags.Debug;

			var device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags);
			using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
			{
				multiThread.SetMultithreadProtected(true);
			}


			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(@"D:\Temp\4.bmp");
			Texture2D rgbTexture = DxTool.GetTexture(bmp, device);

			var bufTexture = new Texture2D(device,
				new Texture2DDescription
				{

						// Format = Format.NV12,
						Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
					Width = 1920,
					Height = 1080,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = { Count = 1 },
				});

			device.ImmediateContext.CopyResource(rgbTexture, bufTexture);

			var processor = new MfVideoProcessor(device);
			var inProcArgs = new MfVideoArgs
			{
				Width = 1920,
				Height = 1080,
				Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
			};



			var outProcArgs = new MfVideoArgs
			{
				Width = 1920,
				Height = 1080,
				Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
			};

			processor.Setup(inProcArgs, outProcArgs);
			processor.Start();


			var rgbSample = MediaFactory.CreateVideoSampleFromSurface(null);

			// Create the media buffer from the texture
			MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, bufTexture, 0, false, out var mediaBuffer);

			using (var buffer2D = mediaBuffer.QueryInterface<Buffer2D>())
			{
				mediaBuffer.CurrentLength = buffer2D.ContiguousLength;
			}

			rgbSample.AddBuffer(mediaBuffer);

			rgbSample.SampleTime = 0;
			rgbSample.SampleDuration = 0;

			var result = processor.ProcessSample(rgbSample, out var nv12Sample);

			Task.Run(() =>
			{


				Stopwatch sw = new Stopwatch();
				int fps = 60;
				int interval = (int)(1000.0 / fps);

				int _count = 1;

				long globalTime = 0;


				while (true)
				{

					if (result)
					{

						globalTime += sw.ElapsedMilliseconds;
						sw.Restart();


						nv12Sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0));
						nv12Sample.SampleDuration = MfTool.SecToMfTicks(((int)interval / 1000.0));

						//sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0));
						//sample.SampleDuration = MfTool.SecToMfTicks(((int)interval / 1000.0));

						SampleReady?.Invoke(nv12Sample);


						var msec = sw.ElapsedMilliseconds;

						var delay = interval - msec;
						if (delay < 0)
						{
							delay = 1;
						}

						// var delay = 1;
						Thread.Sleep((int)delay);
						var elapsedMilliseconds = sw.ElapsedMilliseconds;
						globalTime += elapsedMilliseconds;
						_count++;



					}

					//nv12Sample?.Dispose();

					//Thread.Sleep(30);
				}

			});



		}




	}

}
