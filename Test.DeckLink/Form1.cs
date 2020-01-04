using DeckLinkAPI;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test.DeckLink
{
    public partial class Form1 : Form
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public Form1()
        {
            InitializeComponent();

            MediaManager.Startup();

            //SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = false;
            SharpDX.Configuration.EnableObjectTracking = true;
            SharpDX.Diagnostics.ObjectTracker.StackTraceProvider = null;
        }


        private Form videoForm = null;
        private DeckLinkInput deckLinkInput = null;

        private WasapiOut audioPlayer = null;
        private BufferedWaveProvider audioBuffer = null;
        private VolumeSampleProvider volumeProvider = null;

        private void buttonStart_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStart_Click(...)");

            deckLinkInput = new DeckLinkInput();
            deckLinkInput.CaptureStarted += DeckLinkInput_CaptureStarted;
            deckLinkInput.CaptureStopped += DeckLinkInput_CaptureStopped;
            deckLinkInput.InputFormatChanged += CurrentDevice_InputFormatChanged;


            //deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
            //deckLinkInput.AudioDataArrived += _selectedDevice_AudioDataArrived;
            deckLinkInput.StartCapture(0);

            buttonStart.Enabled = false;
            buttonStop.Enabled = true;

        }



        private void DeckLinkInput_CaptureStarted()
        {
            logger.Debug("DeckLinkInput_CaptureStarted(...)");

            this.Invoke((Action)(() =>
            {

                InitAudio();

                StartAudio();

                var UYVYFourCC = new SharpDX.Multimedia.FourCC(0x59565955);


                // var format = VideoFormatGuids.FromFourCC(v210FourCC);
                var format = VideoFormatGuids.FromFourCC(UYVYFourCC);

                //var format = VideoFormatGuids.NV12;
                var sampleArgs = new MfVideoArgs
                {
                    Width = 1920,
                    Height = 1080,
                    Format = format, //VideoFormatGuids.Uyvy, //VideoFormatGuids.NV12,//MFVideoFormat_v210,

                };

                videoForm = new Form
                {
                    BackColor = Color.Black,
                    StartPosition = FormStartPosition.CenterScreen,
                    ClientSize = new Size(sampleArgs.Width, sampleArgs.Height)
                };

                renderer = new MfVideoRenderer();
                videoForm.Paint += (o, a) =>
                {
                    renderer.Repaint();
                };

                videoForm.SizeChanged += (o, a) =>
                {
                    renderer.Resize(videoForm.ClientRectangle);
                };

                renderer.Setup(videoForm.Handle, sampleArgs);
                renderer.Start(0);

                videoForm.Visible = true;

                renderer.Resize(videoForm.ClientRectangle);

                deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
                deckLinkInput.AudioDataArrived += _selectedDevice_AudioDataArrived;

            }));
               

        }

        private void DeckLinkInput_CaptureStopped(object obj)
        {
            logger.Debug("DeckLinkInput_CaptureStopped(...)");

            this.Invoke((Action)(() => 
            {

                StopAudio();

                if (renderer != null)
                {
                    renderer.Close();

                }

                if (videoForm != null)
                {
                    videoForm.Close();
                    videoForm = null;
                }

                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;
                deckLinkInput.AudioDataArrived -= _selectedDevice_AudioDataArrived;

                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
            }));

        }

       

        private MfVideoRenderer renderer = null;


        Stopwatch sw = new Stopwatch();
        long _time = 0;
        private void CurrentDevice_VideoDataArrived(IntPtr ptr, int len, long time)
        {
            _time += sw.ElapsedMilliseconds;

            var sample = MediaFactory.CreateSample();

            sample.SampleTime = 0;//MfTool.SecToMfTicks((_time / 1000.0));
            sample.SampleDuration = 0;//MfTool.SecToMfTicks(((int)33 / 1000.0));


            var mb = MediaFactory.CreateMemoryBuffer(len);
            {
                var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);

                Kernel32.CopyMemory(pBuffer, ptr, (uint)len);

                //Marshal.Copy(testBytes, 0, pBuffer, len);

                mb.CurrentLength = len;
                mb.Unlock();

                sample.AddBuffer(mb);
            }

            renderer.ProcessSample(sample);

            sample.Dispose();
            mb?.Dispose();

            sw.Restart();
        }




        private void CurrentDevice_InputFormatChanged(IDeckLinkDisplayMode newDisplayMode)
        {
            logger.Debug("CurrentDevice_InputFormatChanged(...)");
        }


        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            if (deckLinkInput != null)
            {
                //deckLinkInput.CaptureStarted -= DeckLinkInput_CaptureStarted;
                //deckLinkInput.CaptureStopped -= DeckLinkInput_CaptureStopped;
                //deckLinkInput.InputFormatChanged -= CurrentDevice_InputFormatChanged;

                deckLinkInput.AudioDataArrived -= _selectedDevice_AudioDataArrived;
                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                deckLinkInput.StopCapture();
            }


        }



        public void InitAudio()
        {
            logger.Debug("InitAudio()");

            try
            {
                var sampleRate = (int)deckLinkInput.AudioSampleRate;
                var channelsCount = deckLinkInput.AudioChannelsCount;
                var bitsPerSample = (int)deckLinkInput.AudioSampleType;
                const int BufferMilliseconds = 100;

                MMDevice device = null;
                var deviceEnum = new MMDeviceEnumerator();
                if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                {
                    device = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                }

                if (device == null)
                {// если дефолтного девайса нет, берем первый активный
                    var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    device = devices.FirstOrDefault();
                }

                if (device != null)
                {

                    var shareMode = AudioClientShareMode.Shared;
                    var useSyncEvent = false;
                    var latency = 100;

                    var deviceLog = string.Join(" ", device.FriendlyName, sampleRate, channelsCount, bitsPerSample,
                        shareMode, useSyncEvent, latency, BufferMilliseconds);

                    logger.Debug("Audio device: " + deviceLog);


                    audioPlayer = new WasapiOut(device, shareMode, useSyncEvent, latency);
                    var audioFormat = new WaveFormat(sampleRate, bitsPerSample, channelsCount);
                    audioBuffer = new BufferedWaveProvider(audioFormat)
                    {
                        BufferDuration = TimeSpan.FromMilliseconds(BufferMilliseconds),
                        DiscardOnBufferOverflow = true,
                    };


                    volumeProvider = new VolumeSampleProvider(audioBuffer.ToSampleProvider());
                    //SetVolume();

                    audioPlayer.Init(volumeProvider);
                    audioPlayer.PlaybackStopped += AudioPlayer_PlaybackStopped;

                    //testFile = new FileStream(@"d:\BlackMagicAudio" + DateTime.Now.ToString("HH_mm_ss_fff") + ".pcm", FileMode.Create);

                    audioEnabled = true;
                }
                else
                {
                    logger.Warn("Audio device not found");
                    audioEnabled = false;
                    return;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                if (audioPlayer != null)
                {
                    audioPlayer.PlaybackStopped -= AudioPlayer_PlaybackStopped;
                    audioPlayer.Dispose();
                    audioPlayer = null;
                }

                audioEnabled = false;
            }

        }


        private void StartAudio()
        {
            logger.Debug("StartAudio()");

            if (!audioEnabled)
            {
                logger.Warn("AudioEnabled " + audioEnabled);
                return;
            }

            if (audioPlayer != null)
            {
                if (audioPlayer.PlaybackState == PlaybackState.Stopped)
                {
                    audioPlayer.Play();
                }
            }
        }
        private bool audioEnabled = true;
        private void StopAudio()
        {
            logger.Debug("StopAudio()");

            if (!audioEnabled)
            {
                logger.Warn("AudioEnabled " + audioEnabled);
                return;
            }

            if (audioPlayer != null)
            {
                audioPlayer.Stop();

            }
        }

        private void AudioPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("AudioPlayer_PlaybackStopped(...)");

            var ex = e.Exception;
            if (ex != null)
            {

                var comExeption = ex as System.Runtime.InteropServices.COMException;
                if (comExeption != null)
                {
                    //https://msdn.microsoft.com/en-us/library/windows/desktop/ee416776(v=vs.85).aspx
                    if (comExeption.ErrorCode == -2005401450) // 0x88780096 DSERR_BUFFERLOST 
                    {
                        // TODO: перезапустить девайс...
                        return;
                    }
                }

                logger.Error(ex);
            }

            audioPlayer?.Dispose();

            audioEnabled = false;

        }

        private bool _muteAudio = false;

        private void _selectedDevice_AudioDataArrived(byte[] data, long time)
        {// Получили аудио данные кладем их в буфер
            if (!_muteAudio)
            {
                if (audioBuffer != null)
                {
                    audioBuffer.AddSamples(data, 0, data.Length);
                }
            }

            //if (testFile != null)
            //{
            //    testFile.Write(data, 0, data.Length);
            //}

        }

        private void comboBoxDevices_SelectedValueChanged(object sender, EventArgs e)
        {
            logger.Debug("comboBoxDevices_SelectedValueChanged(...)");

            //var selectedItem = comboBoxDevices.SelectedItem;
            //if (selectedItem != null)
            //{
            //    var selectedDevice = selectedItem as DeckLinkDevice;
            //    if (selectedDevice != null)
            //    {
            //        currentDevice = selectedDevice;


            //    }
            //}
        }



        private List<DeckLinkDevice> decklinkDevices = new List<DeckLinkDevice>();

        SynchronizationContext synchronizationContext = null;

        private void buttonFind_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonFind_Click(...)");

            Thread thread = new Thread(() =>
            {

                IDeckLinkIterator deckLinkIterator = null;
                try
                {
                    deckLinkIterator = new CDeckLinkIterator();

                    IDeckLink deckLink = null;
                    do
                    {
                        deckLinkIterator.Next(out deckLink);

                        if (deckLink != null)
                        {
                            DeckLinkDevice device = new DeckLinkDevice(deckLink);
                            if (device.DeckLinkInput != null)
                            {
                                decklinkDevices.Add(device);
                            }
                        }

                    }
                    while (deckLink != null);

                }
                catch (Exception ex)
                {
                    var errorMessage = ex.Message;
                    if (deckLinkIterator == null)
                    {
                        errorMessage = "This application requires the DeckLink drivers installed.\n" +
                            "Please install the Blackmagic DeckLink drivers to use the features of this application";
                    }


                    MessageBox.Show(errorMessage, "Error");
                }


                if (decklinkDevices.Count == 0)
                {
                    MessageBox.Show("This application requires a DeckLink PCI card.\n" +
                        "You will not be able to use the features of this application until a DeckLink PCI card is installed.");

                }


            });


            //thread.SetApartmentState(ApartmentState.MTA);
            thread.Start();

            thread.Join();

            comboBoxDevices.DataSource = decklinkDevices;
            comboBoxDevices.DisplayMember = "DeviceName";
        }

    }
}
