using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using MediaToolkit.Utils;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
using SharpDX;
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
    public partial class Form1 : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Form1()
        {
            InitializeComponent();

			

		}

        private MfVideoRenderer videoRenderer = new MfVideoRenderer();
        private VideoForm videoForm = null;

        private Task producerTask = null;


        long globalTime = 0;
        private PresentationClock presentationClock = null;
		Stopwatch stopwatch = new Stopwatch();

		private List<byte[]> testBitmapSequence = new List<byte[]>();
        private void buttonSetup_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonSetup_Click(...)");



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




            videoForm = new VideoForm
            {
                BackColor = Color.Black,
                //ClientSize = new Size(sampleArgs.Width, sampleArgs.Height),
                StartPosition = FormStartPosition.CenterScreen,
            };

            videoRenderer = new MfVideoRenderer();
            

            videoRenderer.RendererStarted += Renderer_RendererStarted;
            videoRenderer.RendererStopped += Renderer_RendererStopped;


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

            videoForm.Visible = true;

            videoRenderer.Setup(new VideoRendererArgs
            {
                hWnd = videoForm.Handle,
               // FourCC = new FourCC("NV12"),
               //FourCC = 0x59565955, //"UYVY",
                FourCC = new FourCC((int)Format.A8R8G8B8),
                Resolution = new Size(1920, 1080),
                FrameRate = new Tuple<int, int>(30, 1),

            });

            videoRenderer.SetPresentationClock(presentationClock);
            videoRenderer.RendererStopped += () => 
            {
                videoRenderer.Close();

                GC.Collect();
            };

            videoRenderer.Resize(videoForm.ClientRectangle);
            sampleSource = new SampleSource();


			bool isFirstTimestamp = true;

			long timeAdjust = 0;

            sampleSource.SampleReady += (sample) =>
            {
				if (isFirstTimestamp)
				{
					var _sampleTime = sample.SampleTime;

					var presetnationTime = presentationClock.Time;
					var stopwatchTime = MfTool.SecToMfTicks(stopwatch.ElapsedMilliseconds / 1000.0);
					timeAdjust = presetnationTime - _sampleTime; //stopwatchTime;
					Console.WriteLine(presetnationTime + " - " + _sampleTime +  " = " + timeAdjust);

					isFirstTimestamp = false;
				}

				//var sampleTime = sample.SampleTime;
				//var presetnationTime = presentationClock.Time;

				//var diff = sampleTime - presetnationTime;
				//Console.WriteLine(MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(presetnationTime) + " " + MfTool.MfTicksToSec(diff));

				//var stopwatchTime = MfTool.SecToMfTicks(stopwatch.ElapsedMilliseconds / 1000.0);
				//var diff2 = stopwatchTime - presetnationTime;

				//Console.WriteLine (MfTool.MfTicksToSec(stopwatchTime) + " "  + MfTool.MfTicksToSec(presetnationTime) + " " + MfTool.MfTicksToSec(diff2));


				var sampleTime = sample.SampleTime;

				sample.SampleTime = sampleTime + timeAdjust;

				//sample.SampleDuration = 0;




				videoRenderer?.ProcessSample(sample);

                //sample?.Dispose();
            };

        }





        private SampleSource sampleSource = null;


        private void Renderer_RendererStarted()
        {
            logger.Debug("Renderer_RendererStarted()");
            //...
        }


        private void Renderer_RendererStopped()
        {
            logger.Debug("Renderer_RendererStopped()");


            if (closing)
            {
                videoRenderer.Close();
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

				presentationClock.Start(0);
                //videoRenderer.Start(time);

                sampleSource.Start();
               // sampleSource.Start1();
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

                sampleSource.Stop();

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

                sampleSource.Pause();

            }
        }

        private bool closing = false;
        private void buttonClose_Click(object sender, EventArgs e)
        {

            logger.Debug("buttonClose_Click(...)");
            sampleSource?.Stop();

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



        MfAudioRenderer audioRenderer = null;
        private SignalGenerator signalGenerator = null;

        private void buttonAudioSetup_Click(object sender, EventArgs e)
        {


            try
            {

                MMDevice device = null;

                var deviceEnum = new MMDeviceEnumerator();
                if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                {
                    device = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

                }

                string deviceId = device.ID;
                NAudio.Wave.WaveFormat deviceFormat = device.AudioClient.MixFormat;

                device.Dispose();

                signalGenerator = new SignalGenerator(16000, 2);
                var signalFormat = signalGenerator.WaveFormat;

                audioRenderer = new MfAudioRenderer();
                AudioRendererArgs audioArgs = new AudioRendererArgs
                {
                    DeviceId = "",
                    SampleRate = signalFormat.SampleRate,
                    BitsPerSample = signalFormat.BitsPerSample,
                    Encoding = (WaveEncodingTag)signalFormat.Encoding,
                    Channels = signalFormat.Channels,

                };

                audioRenderer.Setup(audioArgs);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }



        }

        private void buttonAudioStart_Click(object sender, EventArgs e)
        {
            try
            {
                audioRenderer?.Start(0);

                audioRenderer.Mute = false;
                audioRenderer.Volume = 1f;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        private void buttonAudioStop_Click(object sender, EventArgs e)
        {
            try
            {
                audioRenderer?.Stop();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        private void buttonCloseAudio_Click(object sender, EventArgs e)
        {
            try
            {
                audioRenderer?.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }



        private void buttonProcessSample_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonProcessSample_Click(...)");

            try
            {

                Task.Run(() =>
                {

                    logger.Debug("signal generator start...");


                    var waveSignalGen = signalGenerator.ToWaveProvider();
                    var signalFormat = waveSignalGen.WaveFormat;
                    var bytesPerSecond = signalFormat.AverageBytesPerSecond;
                    var buffer = new byte[bytesPerSecond/100];

                    int count = 10000000;
                    while (count-- > 0)
                    {

                        var sample = MediaFactory.CreateSample();
                        var mb = MediaFactory.CreateMemoryBuffer(buffer.Length);
                        {
                            sample.AddBuffer(mb);
                        }

                        sample.SampleDuration = 0;
                        sample.SampleTime = 0;

                        var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);
                        var bytesRead = waveSignalGen.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            Marshal.Copy(buffer, 0, pBuffer, bytesRead);
                        }

                        mb.CurrentLength = bytesRead;
                        mb.Unlock();

                        audioRenderer?.ProcessSample(sample);
                        mb?.Dispose();
                        sample?.Dispose();

                        Thread.Sleep(9);

                        logger.Debug("Next sample...");

                    }




                    logger.Debug("Signal generator stop...");
                });


                //index++;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }



        }


        public static byte[] GetSamplesWaveData(float[] samples, int samplesCount)
        {
            var pcm = new byte[samplesCount * 2];
            int sampleIndex = 0,
                pcmIndex = 0;

            while (sampleIndex < samplesCount)
            {
                var outsample = (short)(samples[sampleIndex] * short.MaxValue);
                pcm[pcmIndex] = (byte)(outsample & 0xff);
                pcm[pcmIndex + 1] = (byte)((outsample >> 8) & 0xff);

                sampleIndex++;
                pcmIndex += 2;
            }

            return pcm;
        }



        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            if (audioRenderer != null)
            {
                var vol = trackBarVolume.Value / 100.0;
                audioRenderer.Volume = (float)vol;
            }
        }

        private void checkBoxMute_CheckedChanged(object sender, EventArgs e)
        {
            if (audioRenderer != null)
            {
                audioRenderer.Mute = (checkBoxMute.Checked);
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

        private void button1_Click(object sender, EventArgs e)
        {
            GC.Collect();
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


		SimpleRenderer renderer = null;
		private void button3_Click(object sender, EventArgs e)
		{
			try
			{
				var testFile3 = @".\TestBmp\1920x1088_Nv12.raw";

				var bytes = File.ReadAllBytes(testFile3);

				renderer = new SimpleRenderer();

				renderer.Setup();

				d3DImageControl1.DataContext = renderer;

				//var devMan = renderer.D3DDeviceManager;
				//devMan.OpenDeviceHandle(out var hDevice);

				//SharpDX.Direct3D9.DeviceEx device = new SharpDX.Direct3D9.DeviceEx(hDevice);
				var device = renderer.device;


				var NV12FourCC = new FourCC("NV12");

				// var format = VideoFormatGuids.FromFourCC(v210FourCC);
				// var format = VideoFormatGuids.FromFourCC(UYVYFourCC);

				var format = (Format)(uint)NV12FourCC; //VideoFormatGuids.NV12;
				//var format = (Format)(842094158);
				//var format = Format.A8R8G8B8;

				var nv12Surface = Surface.CreateOffscreenPlain(device, 1920, 1088, (Format)(842094158), Pool.Default);
				var argb32Surface = Surface.CreateOffscreenPlain(device, 1920, 1080, Format.A8R8G8B8, Pool.Default);


				//DataRectangle dataRect = nv12Surface.LockRectangle(LockFlags.None);

				//Marshal.Copy(bytes, 0, dataRect.DataPointer, bytes.Length);

				//nv12Surface.UnlockRectangle();


				//var data = nv12Surface.LockRectangle(LockFlags.ReadOnly);

				//TestTools.WriteFile(data.DataPointer, 3133440, @"d:\test.raw");

				//nv12Surface.UnlockRectangle();


				//Bitmap sourceBitmap = new Bitmap(@"D:\Temp\4.bmp");
				//Bitmap argb32Bitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				//var g = Graphics.FromImage(argb32Bitmap);
				//g.DrawImage(sourceBitmap, 0, 0);
				//g.Dispose();

				//BitmapToSurface(argb32Surface, argb32Bitmap);

				//argb32Bitmap.Dispose();

				var sample = MediaFactory.CreateVideoSampleFromSurface(nv12Surface);

				sample.SampleDuration = 0;
				sample.SampleTime = 0;

				renderer.ProcessSample(sample);


				//devMan.CloseDeviceHandle(hDevice);

				renderer.Start();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex);
			}

		}

		private bool BitmapToSurface(Surface surface, Bitmap bmp)
		{
			bool result = false;

			var surfDescr = surface.Description;

			if (bmp.Width != surfDescr.Width || bmp.Height != surfDescr.Height)
			{
				//...
				logger.Warn("bmp.Width != surfDescr.Width || bmp.Height != surfDescr.Height");
				return result;
			}

			if (!(surfDescr.Format == Format.A8R8G8B8 || surfDescr.Format == Format.X8R8G8B8))
			{
				logger.Warn("Unsupported surface format " + surfDescr.Format);
				return result;
			}

			var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
			try
			{
				DataRectangle dataRect = surface.LockRectangle(LockFlags.None);
				try
				{

					int height = bmp.Height;
					int width = bmp.Width;
					int pictWidth = width * 4;

					var sourcePtr = bitmapData.Scan0;
					var destPtr = dataRect.DataPointer;
					for (int line = 0; line < height; line++)
					{
						Utilities.CopyMemory(destPtr, sourcePtr, bitmapData.Stride);

						sourcePtr = IntPtr.Add(sourcePtr, bitmapData.Stride);
						destPtr = IntPtr.Add(destPtr, dataRect.Pitch );
					}
					result = true;
				}
				finally
				{
					surface.UnlockRectangle();
				}
			}
			finally
			{
				bmp.UnlockBits(bitmapData);
			}

			return result;

		}

		private void button4_Click(object sender, EventArgs e)
		{

		}
	}
}
