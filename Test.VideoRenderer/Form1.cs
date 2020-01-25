using MediaToolkit.Core;
using MediaToolkit.MediaFoundation;
using MediaToolkit.SharedTypes;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
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
                FourCC = 0x59565955, //"UYVY",
                Resolution = new Size(1920, 1080),
            });

            videoRenderer.SetPresentationClock(presentationClock);
            videoRenderer.RendererStopped += () => 
            {
                videoRenderer.Close();

                GC.Collect();
            };

            videoRenderer.Resize(videoForm.ClientRectangle);
            sampleSource = new SampleSource();

            sampleSource.SampleReady += (sample) =>
            {
                sample.SampleTime = presentationClock.Time;
                sample.SampleDuration = 0;

                videoRenderer?.ProcessSample(sample);

                //sample?.Dispose();
            };

        }

        private SampleSource sampleSource = null;

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


                var testFile5 = @".\Test\1920x1080_bmdFormat10BitYUV.raw";
                var testFile2 = @".\Test\1920x1080_bmdFormat8BitYUV.raw";
                //var testFile3 = @".\Test\1920x1080_Argb32.raw";

                //var canvaspng = @".\Test\canvas.png";
                var testBytes = File.ReadAllBytes(testFile2);
                var testBytes5 = File.ReadAllBytes(testFile5);

                //var fourCC = new FourCC("V210");


                var V210FourCC = new FourCC(0x30313256);

                var UYVYFourCC = new FourCC(0x59565955);

                // var format = VideoFormatGuids.FromFourCC(v210FourCC);
                var format = VideoFormatGuids.FromFourCC(UYVYFourCC);

                //var format = VideoFormatGuids.NV12;
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
                    int fps = 1000;
                    int interval = (int)(1000.0 / fps);

                    int _count = 1;

                    long globalTime = 0;

                    var sample = MediaFactory.CreateSample();
                    using (var mb = MediaFactory.CreateMemoryBuffer(testBytes.Length))
                    {
                        var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

                        Marshal.Copy(testBytes, 0, pBuffer, testBytes.Length);

                        mb.CurrentLength = testBytes.Length;
                        mb.Unlock();

                        sample.AddBuffer(mb);
                    }

                    while (running)
                    {

                        if (paused)
                        {
                            Thread.Sleep(100);
                            continue;
                        }


                        sw.Restart();


                        //var sample = MediaFactory.CreateSample();
                        //using (var mb = MediaFactory.CreateMemoryBuffer(testBytes.Length))
                        //{
                        //    var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

                        //    //Marshal.Copy(testBytes, 0, pBuffer, testBytes.Length);

                        //    mb.CurrentLength = testBytes.Length;
                        //    mb.Unlock();

                        //    sample.AddBuffer(mb);
                        //}


                        globalTime += sw.ElapsedMilliseconds;
                        sample.SampleTime = 0;//MfTool.SecToMfTicks((globalTime / 1000.0));
                        sample.SampleDuration = 0;//MfTool.SecToMfTicks(((int)interval / 1000.0));

                        //sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0));
                        //sample.SampleDuration = MfTool.SecToMfTicks(((int)interval / 1000.0));

                        SampleReady?.Invoke(sample);


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

                        //Console.WriteLine(elapsedMilliseconds);
                        //Console.SetCursorPosition(0, Console.CursorTop - 1);

                        
                    }

                    sample?.Dispose();

                });
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
        }



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

                presentationClock.Start(0);
                //videoRenderer.Start(time);
                sampleSource.Start();

            }

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            if (videoRenderer != null)
            {
                presentationClock.Stop();

                //videoRenderer.Stop();

                sampleSource.Stop();

            }
        }
        private bool paused = false;
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

                //paused = !paused;

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
                videoRenderer.Stop();
                
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
                    var buffer = new byte[bytesPerSecond];

                    int count = 10;
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

                        Thread.Sleep(900);

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
    }
}
