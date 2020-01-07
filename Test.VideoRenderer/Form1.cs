using MediaToolkit.MediaFoundation;
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

        private MfVideoRenderer renderer = new MfVideoRenderer();
        private VideoForm videoForm = null;

        private Task producerTask = null;


        long globalTime = 0;

        private List<byte[]> testBitmapSequence = new List<byte[]>();
        private void buttonSetup_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonSetup_Click(...)");



            var testSeqDir = @"D:\testBMP\";
            var di = new DirectoryInfo(testSeqDir);
            var files = di.GetFiles().Take(60);
            foreach (var f in files)
            {
                var bytes = File.ReadAllBytes(f.FullName);
                testBitmapSequence.Add(bytes);
            }




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

            videoForm = new VideoForm
            {
                BackColor = Color.Black,
                ClientSize = new Size(sampleArgs.Width, sampleArgs.Height),
                StartPosition = FormStartPosition.CenterScreen,
            };

            renderer = new MfVideoRenderer();

            renderer.RendererStarted += Renderer_RendererStarted;
            renderer.RendererStopped += Renderer_RendererStopped;


            videoForm.Paint += (o, a) =>
            {
                renderer.Repaint();
            };

            videoForm.SizeChanged += (o, a) =>
            {
                var rect = videoForm.ClientRectangle;

                //Console.WriteLine(rect);
                renderer.Resize(rect);
            };

            videoForm.Visible = true;

            renderer.Setup(videoForm.Handle, sampleArgs);

            renderer.Resize(videoForm.ClientRectangle);

            closing = false;


            Stopwatch sw = new Stopwatch();
            int fps = 60;
            int interval = (int)(1000.0 / fps);

            int _count = 100000;
       

            producerTask = Task.Run(() =>
            {
                var sample = MediaFactory.CreateSample();
                var mb = MediaFactory.CreateMemoryBuffer(testBytes.Length);
                {
                    var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

                    Marshal.Copy(testBytes, 0, pBuffer, testBytes.Length);

                    mb.CurrentLength = testBytes.Length;
                    mb.Unlock();

                    sample.AddBuffer(mb);
                }

                while (true) //_count-- > 0)
                {
                    if (closing)
                    {
                        break;
                    }

                    {
                        int index = _count % testBitmapSequence.Count;
                        var bytes = testBitmapSequence[index];


                        var _pBuffer = mb.Lock(out int _cbMaxLen, out int _cbCurLen);

                        Marshal.Copy(bytes, 0, _pBuffer, bytes.Length);



                        //if (_count % 10 != 0)
                        //{
                        //    Marshal.Copy(testBytes, 0, _pBuffer, testBytes.Length);
                        //}
                        //else
                        //{
                        //    Marshal.Copy(testBytes5, 0, _pBuffer, testBytes.Length);
                        //}

                        _count++;
                        sw.Restart();


                        mb.CurrentLength = testBytes.Length;
                        mb.Unlock();

                        globalTime += sw.ElapsedMilliseconds;
                        sample.SampleTime = MfTool.SecToMfTicks((globalTime / 1000.0));
                        sample.SampleDuration = MfTool.SecToMfTicks(((int)interval / 1000.0));

                        renderer.ProcessSample(sample);

                        var msec = sw.ElapsedMilliseconds;

                        var delay = interval - msec;
                        if (delay < 0)
                        {
                            delay = 1;
                        }

                        Thread.Sleep((int)delay);

                        globalTime += sw.ElapsedMilliseconds;

                        

                        // Console.WriteLine(msec);
                    }

                }


                sample?.Dispose();
                mb?.Dispose();

            });


            //Task.Run(() =>
            //{
            //    while (true)
            //    {

            //        Thread.Sleep(1000);
            //        Console.WriteLine("FPS: " + fps);
            //        fps = 0;
            //    }
            //});

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
                renderer.Close();
            }

        }



        private void buttonStart_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStart_Click(...)");

            if (renderer != null)
            {
               var time =  MfTool.SecToMfTicks((globalTime / 1000.0));
                logger.Debug("renderer.Start(...) " + time);
                renderer.Start(time);

            }

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            if (renderer != null)
            {
                renderer.Stop();

            }
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonPause_Click(...)");

            if (renderer != null)
            {
                renderer.Pause();

            }
        }

        private bool closing = false;
        private void buttonClose_Click(object sender, EventArgs e)
        {

            logger.Debug("buttonClose_Click(...)");

            videoForm?.Close();
            closing = true;

            if (renderer != null)
            {
                renderer.Stop();

                //renderer.Close();

                
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

                    //var format = new NAudio.Wave.WaveFormat(44100, 2);
                    //var res = device.AudioClient.IsFormatSupported(AudioClientShareMode.Shared, format);
                }

                string deviceId = device.ID;
                NAudio.Wave.WaveFormat mixFormat = device.AudioClient.MixFormat;

                device.Dispose();

                signalGenerator = new SignalGenerator(48000, 2);
                var inputWaveFormat = signalGenerator.WaveFormat;

                audioRenderer = new MfAudioRenderer();
                audioRenderer.Setup(deviceId, mixFormat, inputWaveFormat);

            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }



        }

        private void buttonAudioStart_Click(object sender, EventArgs e)
        {
            try
            {
                audioRenderer?.Start(0);

                audioRenderer.SetMute(false);
                audioRenderer.SetVolume(1f);
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

                   
                    //var waveSignalGen = signalGenerator.ToWaveProvider();
                    
                    var testBytes = new float[100* 1024];
                    int dataSize = testBytes.Length * sizeof(float);
                    var sample = MediaFactory.CreateSample();
                    var mb = MediaFactory.CreateMemoryBuffer(dataSize);
                    {
                        sample.AddBuffer(mb);
                    }

                    sample.SampleDuration = 0;
                    sample.SampleTime = 0;
                    int count = 10;
                    //while (count-- > 0)
                    {
                        
                        //var testBytes = new float[1024];
                        signalGenerator.Read(testBytes, 0, testBytes.Length);

                        var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

                        Marshal.Copy(testBytes, 0, pBuffer, testBytes.Length);

                        mb.CurrentLength = dataSize;
                        mb.Unlock();

                        audioRenderer?.ProcessSample(sample);
                        //Thread.Sleep(2000);
                    }

                    mb.Dispose();
                    sample.Dispose();

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
    }
}
