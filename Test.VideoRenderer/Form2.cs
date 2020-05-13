using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.VideoRenderer
{
    public partial class Form2 : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Form2()
        {
            InitializeComponent();


			textBox1.Text = sourceVideoFile;

		}

        private MfVideoRendererEx videoRenderer = new MfVideoRendererEx();
        private VideoForm videoForm = null;


        long globalTime = 0;
        private PresentationClock presentationClock = null;
		Stopwatch stopwatch = new Stopwatch();


		VideoFileSource fileSource = null;

		private List<byte[]> testBitmapSequence = new List<byte[]>();
        private void buttonSetup_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonSetup_Click(...)");

			sourceVideoFile = textBox1.Text;

			videoForm = new VideoForm
			{
				BackColor = Color.Black,
				//ClientSize = new Size(sampleArgs.Width, sampleArgs.Height),
				StartPosition = FormStartPosition.CenterScreen,
			};


			videoForm.Visible = true;

			if (presentationClock != null)
            {
                presentationClock.Dispose();
                presentationClock = null;
            }

            MediaFactory.CreatePresentationClock(out presentationClock);

            PresentationTimeSource timeSource = null;
            try
            {
                MediaFactory.CreateSystemTimeSource(out timeSource);
                presentationClock.TimeSource = timeSource;
            }
            finally
            {
                timeSource?.Dispose();
            }


			videoRenderer = new MfVideoRendererEx();

			videoRenderer.Init(videoForm.Handle, presentationClock);



			int sinkRequestSample = 0;
			int count = 0;

			fileSource = new VideoFileSource();

			fileSource.Setup(sourceVideoFile);//, videoRenderer.D3DDeviceManager);

			//videoRenderer.Prerolled += () => 
			//{
			//	presentationClock.Start(MfTool.SecToMfTicks(1));

			//};

			fileSource.SampleReady += (flags, sample) => 
			{
				var _flags = (SourceReaderFlags)flags;
				if(_flags == SourceReaderFlags.StreamTick)
				{

				}
				else if(_flags == SourceReaderFlags.Currentmediatypechanged)
				{
					var newMediaType = fileSource.GetCurrentMediaType();
					//var log = MfTool.LogMediaType(newMediaType);
					//Console.WriteLine("================Currentmediatypechanged======================");
					//Console.WriteLine(log);


					videoRenderer?.SetMediaType(newMediaType);
					newMediaType.Dispose();
				}

				if (sample != null)
				{

					var presentationTime = presentationClock.Time;
					var sampleTime = sample.SampleTime;
					var diff = sampleTime - presentationTime;
					if(diff > 0)
					{
						var delay = (int)(MfTool.MfTicksToSec(diff) * 1000);
						Console.WriteLine("Delay " + delay);
						Thread.Sleep(delay);
					}

					//videoRenderer._ProcessDxva2Sample(sample);
					videoRenderer.ProcessSample(sample);
					sample.Dispose();
					count++;
				}
			};

			fileSource.SourceStopped += () =>
			{
				Console.WriteLine("fileSource.SourceStopped()");
				fileSource.Close();
			};


			var srcMediaType = fileSource.GetCurrentMediaType();
			var subType = srcMediaType.Get(MediaTypeAttributeKeys.Subtype);
			var frameSize = srcMediaType.Get(MediaTypeAttributeKeys.FrameSize);
			var frameRate = srcMediaType.Get(MediaTypeAttributeKeys.FrameRate);

			using (var mediaType = new SharpDX.MediaFoundation.MediaType())
			{
				mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
				mediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.NV12); //
				mediaType.Set(MediaTypeAttributeKeys.FrameSize, frameSize);

				mediaType.Set(MediaTypeAttributeKeys.InterlaceMode, 2);
				mediaType.Set(MediaTypeAttributeKeys.AllSamplesIndependent, 1);

				mediaType.Set(MediaTypeAttributeKeys.FrameRate, frameRate);

				videoRenderer.SetMediaType(mediaType);

			}


			videoRenderer.RequestSample += () =>
			{
				//if (presentationClock != null)
				//{
				//	presentationClock.GetState(0, out var clockState);
				//	if(clockState == ClockState.Running)
				//	{
				//		//fileSource.NextSample();
				//	}
				//}

				fileSource.NextSample();

				sinkRequestSample++;

			};

			videoRenderer.RendererStopped += () =>
			{
				videoRenderer.Close();

				GC.Collect();
			};

			videoRenderer.Resize(videoForm.ClientRectangle);


			videoForm.Paint += (o, a) =>
			{
				videoRenderer.Repaint();
			};

			videoForm.SizeChanged += (o, a) =>
			{
				var rect = videoForm.ClientRectangle;

				//Console.WriteLine(rect);
				videoRenderer.Resize(rect);
			};





		}

		//private string sourceFile = @"C:\Users\Public\Videos\Sample Videos\Wildlife.wmv";
		private string sourceVideoFile = @"D:\Temp\test.mov";
		private void button1_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				Multiselect = false,
				
			};
			var result = dlg.ShowDialog();

			if(result == DialogResult.OK)
			{
				sourceVideoFile = dlg.FileName;

				textBox1.Text = sourceVideoFile;
			}
		}

		private void buttonStart_Click(object sender, EventArgs e)
		{
			logger.Debug("buttonStart_Click(...)");

			if (videoRenderer != null)
			{
				var time = MfTool.SecToMfTicks((globalTime / 1000.0));
				logger.Debug("renderer.Start(...) " + time);

				stopwatch.Start();

				//videoRenderer.Preroll(MfTool.SecToMfTicks(0.1));


				fileSource.Start();

				presentationClock.Start(0);

			}

		}



        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            if (videoRenderer != null)
            {
				stopwatch.Stop();
				presentationClock.Stop();

                //videoRenderer.Stop();

            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonPause_Click(...)");

            if (videoRenderer != null)
            {
                //videoRenderer.Pause();

                presentationClock.GetState(0, out ClockState state);
                if (state == ClockState.Running)
                {
                    presentationClock.Pause();
                }
                else if (state == ClockState.Paused)
                {
                    presentationClock.Start(long.MaxValue);
                }
                else
                {
                    logger.Warn("Pause() return invalid clock state: " + state);
                }

 

            }
        }

        private bool closing = false;
        private void buttonClose_Click(object sender, EventArgs e)
        {

            logger.Debug("buttonClose_Click(...)");


			fileSource?.Close();


            videoForm?.Close();
            closing = true;

            if (videoRenderer != null)
            {

                presentationClock.GetState(0, out ClockState state);
                if(state != ClockState.Stopped)
                {
                    presentationClock.Stop();
                }
                
                //videoRenderer.Stop();

                //videoRenderer.Close();


            }
        }



        private void buttonTest_Click(object sender, EventArgs e)
        {



            var bmp = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(new SolidBrush(Color.Red), new Rectangle(0, 0, bmp.Width, bmp.Height));
            }

            var displaySize = new Size(1920, 1080);
            var bitmapArea = new Rectangle(0, 0, 960, 540);

            RectangleF normalizedRect = NoralizeRect(displaySize, bitmapArea);

            //bmp.Save("d:\\test345.bmp");

            //var bmp = (Bitmap)Image.FromFile("d:\\TEMP\\test123.bmp");
            videoRenderer?.SetBitmap(bmp, normalizedRect, 0.5f);
            bmp.Dispose();


        }

        public static RectangleF NoralizeRect(Size displaySize, Rectangle bitmapArea)
        {
            var x = (float)bitmapArea.X / displaySize.Width;
            var y = (float)bitmapArea.Y / displaySize.Height;

            var width = (float)bitmapArea.Width / displaySize.Width;
            var height = (float)bitmapArea.Height / displaySize.Height;

            if (width > 1)
            {
                width = 1;
            }

            if (height > 1)
            {
                height = 1;
            }

            if (x < 0)
            {
                x = 0;
            }

            if (y < 0)
            {
                y = 0;
            }

            var normalizedRect = new RectangleF(x, y, width, height);

            return normalizedRect;
        }

        private void buttonClearBitmap_Click(object sender, EventArgs e)
        {
            videoRenderer?.SetBitmap(null);
        }

        private void buttonGetBitmap_Click(object sender, EventArgs e)
        {
            try
            {
                var bmp = videoRenderer?.GetCurrentImage();

                if (bmp != null)
                {
                    bmp.Save("d:\\test1.bmp");
                    bmp.Dispose();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void button2_Click(object sender, EventArgs e)
        {
            var direct3D = new SharpDX.Direct3D9.Direct3DEx();

            var hWnd = User32.GetDesktopWindow();

            var presentParams = new PresentParameters
            {
                //Windowed = true,
                //SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                //DeviceWindowHandle = IntPtr.Zero,
                //PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
                //BackBufferCount = 1,

                Windowed = true,
                MultiSampleType = MultisampleType.None,
                SwapEffect = SwapEffect.Discard,
                PresentFlags = PresentFlags.None,
            };

            var flags = CreateFlags.HardwareVertexProcessing |
                        CreateFlags.Multithreaded |
                        CreateFlags.FpuPreserve;

            int adapterIndex = 0;

            var device = new DeviceEx(direct3D, adapterIndex, DeviceType.Hardware, hWnd, flags, presentParams);
            //var format = (Format)(842094158);

            var format = Format.A8R8G8B8;

            var srcSurface = Surface.CreateOffscreenPlain(device, 1920, 1080, format, Pool.SystemMemory);

            //using (var texture3d9 = new SharpDX.Direct3D9.Texture(device, 1920, 1080, 1, Usage.RenderTarget, format, Pool.Default))
            //{
            //    var surface = texture3d9.GetSurfaceLevel(0);
            //    //videoRenderer.ProcessTexture(surface);

            //};


            Bitmap bmp = new Bitmap(@"D:\Temp\4.bmp");

            BitBlt(srcSurface, bmp);

            bmp.Dispose();

        }


        private bool BitBlt(Surface surface, Bitmap bmp)
        {
            bool success;
            var surfDescr = surface.Description;
            IntPtr hdcDest = IntPtr.Zero;
            Graphics graphDest = null;

            IntPtr hdcSrc = surface.GetDC();
            try
            {
                graphDest = System.Drawing.Graphics.FromImage(bmp);
                hdcDest = graphDest.GetHdc();
                Size destSize = bmp.Size;

                int nXDest = 0;
                int nYDest = 0;
                int nWidth = destSize.Width;
                int nHeight = destSize.Height;

                int nXSrc = 0;//SrcRect.Left;
                int nYSrc = 0;//SrcRect.Top;

                int nWidthSrc = surfDescr.Width;//SrcRect.Width;
                int nHeightSrc = surfDescr.Height;//SrcRect.Height;

                var dwRop = TernaryRasterOperations.SRCCOPY;

                success = Gdi32.BitBlt(hdcDest, nXDest, nYDest, nWidth, nHeight, hdcSrc, nXSrc, nYSrc, dwRop);
            }
            finally
            {
                graphDest?.ReleaseHdc(hdcDest);
                graphDest?.Dispose();
                graphDest = null;

                surface.ReleaseDC(hdcSrc);

            }

            return success;
        }


        private void DrawBitmap(Bitmap bmp, IntPtr hWnd)
        {

            IntPtr hdc = IntPtr.Zero;
            IntPtr hdcBmp = IntPtr.Zero;
            IntPtr hBitmap = IntPtr.Zero;
            try
            {
                hdc = User32.GetDC(hWnd);
                hdcBmp = Gdi32.CreateCompatibleDC(hdc);
                hBitmap = bmp.GetHbitmap();

                IntPtr hOld = IntPtr.Zero;
                try
                {
                    hOld = Gdi32.SelectObject(hdcBmp, hBitmap);

                    Gdi32.BitBlt(hdc, 0, 0, bmp.Width, bmp.Height, hdcBmp, 0, 0, TernaryRasterOperations.SRCCOPY);
                }
                finally
                {
                    Gdi32.SelectObject(hdcBmp, hOld);
                }
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    Gdi32.DeleteObject(hBitmap);
                    hBitmap = IntPtr.Zero;
                }

                if (hdcBmp != IntPtr.Zero)
                {
                    Gdi32.DeleteDC(hdcBmp);
                    hdcBmp = IntPtr.Zero;
                }

                if (hdc != IntPtr.Zero)
                {
                    User32.ReleaseDC(hWnd, hdc);
                    // NativeAPIs.Gdi32.DeleteDC(hdc);
                    hdc = IntPtr.Zero;
                }

            }

        }



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
					int fps = 10;
					int interval = (int)(1000.0 / fps);

					int _count = 1;

					long globalTime = 0;

					Bitmap _bmp = new Bitmap(@"D:\Temp\4.bmp");
					Bitmap bmp = new Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

					var g = Graphics.FromImage(bmp);

					g.DrawImage(_bmp, 0, 0);

					var text = DateTime.Now.ToString("HH:mm:ss.fff");
					var font = new System.Drawing.Font(FontFamily.GenericMonospace, 120);
					var textSize = g.MeasureString(text, font);
					g.FillRectangle(Brushes.Black, 0f, 0f, textSize.Width, textSize.Height);

					g.DrawString(text, font, Brushes.Yellow, 0f, 0f);

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

			Bitmap bmp = new Bitmap(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			Bitmap _bmp = new Bitmap(@"D:\Temp\4.bmp");
			double sampleDuration = 1;
			int sampleCount = 0;
			public Sample GetSample()
			{


				var g = Graphics.FromImage(bmp);
				g.DrawImage(_bmp, 0, 0);

				var currentTime = sampleCount * sampleDuration;
				var text = DateTime.Now.ToString("HH:mm:ss.fff") + " " + currentTime;
				var font = new System.Drawing.Font(FontFamily.GenericMonospace, 120);
				var textSize = g.MeasureString(text, font);
				g.FillRectangle(Brushes.Black, 0f, 0f, textSize.Width, textSize.Height);

				g.DrawString(text, font, Brushes.Yellow, 0f, 0f);


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
				mb.Dispose();

				sample.SampleDuration = 0;//MfTool.SecToMfTicks( sampleDuration);
				sample.SampleTime = MfTool.SecToMfTicks(currentTime);

				bmp.UnlockBits(data);


				g.Dispose();
				sampleCount++;

				return sample;

			}

			public void UpdateSample(Bitmap bmp, MediaBuffer mb)
			{
				var g = Graphics.FromImage(bmp);

				var text = DateTime.Now.ToString("HH:mm:ss.fff");
				var font = new System.Drawing.Font(FontFamily.GenericMonospace, 120);
				var textSize = g.MeasureString(text, font);
				g.FillRectangle(Brushes.Black, 0f, 0f, textSize.Width, textSize.Height);

				g.DrawString(text, font, Brushes.Yellow, 0f, 0f);
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
}
