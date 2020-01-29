//using DeckLinkAPI;
using MediaToolkit;
using MediaToolkit.DeckLink;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
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
using System.Runtime.InteropServices;
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


            syncContext = SynchronizationContext.Current;

            fitToVideoCheckBox.Checked = fitToVideoMode;

            UpdateVideoWindow();
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;


        }



        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        private SynchronizationContext syncContext = null;

        private DeckLinkInput deckLinkInput = null;

        private MediaRenderSession renderSession = null;

        private IntPtr windowHandle = IntPtr.Zero;
        private VideoForm videoForm = null;

        private volatile bool isCapture = false;
        private void buttonStart_Click(object sender, EventArgs e)
        {
           
            logger.Debug("buttonStart_Click(...)");

            var deviceIndex = 0;

            var item = comboBoxDevices.SelectedItem;
            if (item != null)
            {
                var device = item as DeckLinkDeviceDescription;
                if (device != null)
                {
                    deviceIndex = device.DeviceIndex;
                }
            }

            try
            {
                OnCaputeStarted();

                renderSession = new MediaRenderSession();

                deckLinkInput = new DeckLinkInput();
                deckLinkInput.CaptureChanged += DeckLinkInput_CaptureChanged; 
                deckLinkInput.StartCapture(currentDevice, currentDisplayMode);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                OnCaptureStopped();
            }
        }





        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            try
            {
                if (deckLinkInput != null)
                {

                    deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;
                    deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                    deckLinkInput.StopCapture();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var stats = deckLinkInput?.Statistics;
            if (stats != null)
            {
                if (videoForm != null && !videoForm.IsDisposed)
                {
                    var noSignal = stats.NoSignal;
                    videoForm.UpdateStatusText(noSignal ? "No signal..." : "");
                }
            }

        }

        private void DeckLinkInput_CaptureChanged(bool formatChanged)
        {
            
            logger.Debug("DeckLinkInput_StateChanged(...) " + deckLinkInput.State + " "  + formatChanged);


            var state = deckLinkInput.State;
            if (formatChanged)
            {
                if (state == MediaToolkit.DeckLink.CaptureState.Starting)
                {// new format...
                    InitRenderSession();
                }
                else if (state == MediaToolkit.DeckLink.CaptureState.Capturing)
                {//  new format, restart media renderers...

                    CloseRenderSession();

                    InitRenderSession();

                    renderSession?.Start();

                }
            }
            else
            {
                if (state == MediaToolkit.DeckLink.CaptureState.Capturing)
                {
                    renderSession?.Start();
                }
                else if (state == MediaToolkit.DeckLink.CaptureState.Stopped)
                {

                    deckLinkInput.Shutdown();

                    CloseRenderSession();

                    syncContext.Send(_ =>
                    {
                        var errorCode = deckLinkInput.ErrorCode;
                        if (errorCode != 0)
                        {
                            var dialogResult = MessageBox.Show("Device stopped with error: " + errorCode, "", MessageBoxButtons.RetryCancel);

                            if (dialogResult == DialogResult.Retry)
                            {
                                deckLinkInput.StartCapture(currentDevice, currentDisplayMode);
                                return;
                            }
                        }

                        OnCaptureStopped();

                    }, null);
                }

                GC.Collect();
            }

        }

        private void CurrentDevice_VideoDataArrived(IntPtr frameData, int frameLength, double frameTime, double frameDuration)
        {

            renderSession?.ProcessVideoFrame(frameData, frameLength, frameTime, frameDuration);

        }

        private void CurrentDevice_AudioDataArrived(IntPtr data, int length, double time, double duration)
        {
            renderSession?.ProcessAudioPacket(data, length, time, duration);
        }

        private void VideoForm_Paint(object sender, PaintEventArgs e)
        {
            renderSession?.Repaint();
        }

        private void VideoForm_SizeChanged(object sender, EventArgs e)
        {
            renderSession?.Resize(videoForm.VideoRectangle);
        }

        private void VideoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            deckLinkInput?.StopCapture();
        }


        private void InitRenderSession()
        {
            AudioRendererArgs audioArgs = null;
            if (deckLinkInput.AudioEnabled)
            {

                var sampleRate = deckLinkInput.AudioSampleRate;
                var bitsPerSample = deckLinkInput.AudioBitsPerSample;
                var channelsCount = deckLinkInput.AudioChannelsCount;

                audioArgs = new AudioRendererArgs
                {
                    DeviceId = "",
                    SampleRate = sampleRate,
                    BitsPerSample = bitsPerSample,
                    Channels = channelsCount,
                    Encoding = MediaToolkit.Core.WaveEncodingTag.PCM,
                };
            }


            var videoResoulution = deckLinkInput.FrameSize;

            var fourCC = deckLinkInput.VideoFormat.FourCC;

            var videoArgs = new VideoRendererArgs
            {
                hWnd = windowHandle,
                Resolution = videoResoulution,
                FourCC = fourCC,

            };

            renderSession.Setup(videoArgs, audioArgs);

            deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
            deckLinkInput.AudioDataArrived += CurrentDevice_AudioDataArrived;

            syncContext.Send(_ =>
            {
                ShowVideo();


            }, null);
        }

        private void CloseRenderSession()
        {
            deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;
            deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;

            renderSession?.Close();
        }


        private void OnCaputeStarted()
        {
            buttonStart.Enabled = false;
            buttonStop.Enabled = true;
            devicesPanel.Enabled = false;
           
            videoForm = new VideoForm
            {
                BackColor = Color.Black,
                StartPosition = FormStartPosition.CenterScreen,
                ClientSize = new Size(640, 480),

                //ClientSize = videoResoulution,
            };

            windowHandle = videoForm.VideoHandle;

            videoForm.Paint += VideoForm_Paint;
            videoForm.SizeChanged += VideoForm_SizeChanged;
            videoForm.FormClosed += VideoForm_FormClosed;

            timer.Enabled = true;

            videoForm.Visible = true;
        }

        private void OnCaptureStopped()
        {
            timer.Enabled = false;

            CloseVideo();
            devicesPanel.Enabled = true;
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;


           
        }


        private void ShowVideo()
        {

            var videoResoulution = deckLinkInput.FrameSize;
            var videoFormat = deckLinkInput.VideoFormat;
            var frameRate = deckLinkInput.FrameRate;
            var fps = frameRate.Item2 / frameRate.Item1;
            string videoLog = "";
            {
                videoLog = videoFormat.Name + "/" + videoResoulution.Width + "x" + videoResoulution.Height + "/" + fps.ToString("0.00");
            }

            string audioLog = "";
            if (deckLinkInput.AudioEnabled)
            {
                audioLog = deckLinkInput.AudioSampleRate + "/" + deckLinkInput.AudioBitsPerSample + "/" + deckLinkInput.AudioChannelsCount;
            }

            videoForm.Text = deckLinkInput.DisplayName + " " + videoLog + " " + audioLog;
            // videoForm.Visible = true;

            UpdateVideoWindow();

            renderSession.Resize(videoForm.VideoRectangle);

        }

        private void CloseVideo()
        {
            if (videoForm != null)
            {
                videoForm.Close();
                videoForm = null;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FindDevices();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (deckLinkInput != null)
            {

                deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;
                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                deckLinkInput.StopCapture();
            }

            if (renderSession != null)
            {
                renderSession.Close();
                renderSession = null;
            }

            MediaToolkitManager.Shutdown();

            base.OnClosed(e);
        }

        private DeckLinkDeviceDescription currentDevice = null;
        private DeckLinkDisplayModeDescription currentDisplayMode = null;

        private List<DeckLinkDisplayModeDescription> displayModes = new List<DeckLinkDisplayModeDescription>();
        private List<DeckLinkDeviceDescription> decklinkDevices = new List<DeckLinkDeviceDescription>();

        private void buttonFind_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonFind_Click(...)");

            FindDevices();

        }

        private void FindDevices()
        {
            logger.Debug("FindDevices()");

            try
            {
                decklinkDevices = DeckLinkTools.GetDeckLinkInputDevices();

                if (decklinkDevices.Count == 0)
                {
                    MessageBox.Show("This application requires a DeckLink PCI card.\n" +
                        "You will not be able to use the features of this application until a DeckLink PCI card is installed.");

                }

                comboBoxDevices.DataSource = decklinkDevices;
                //comboBoxDevices.DisplayMember = "DeviceName";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void comboBoxDevices_SelectedValueChanged(object sender, EventArgs e)
        {
            logger.Debug("comboBoxDevices_SelectedValueChanged(...)");

            var selectedItem = comboBoxDevices.SelectedItem;
            if (selectedItem != null)
            {
                var selectedDevice = selectedItem as DeckLinkDeviceDescription;
                if (selectedDevice != null)
                {
                    currentDevice = selectedDevice;

                    comboBoxDisplayModes.DisplayMember = "Description";

                    displayModes.Add(new DeckLinkDisplayModeDescription { Description = "Auto"});
                    displayModes.AddRange(currentDevice.DisplayModeIds);

                    comboBoxDisplayModes.DataSource = displayModes;

                }
            }
        }

        private void comboBoxDisplayIds_SelectedValueChanged(object sender, EventArgs e)
        {
            var selectedItem = comboBoxDisplayModes.SelectedItem;
            {
                if (selectedItem != null)
                {
                    currentDisplayMode = selectedItem as DeckLinkDisplayModeDescription;

                }
            }
        }


        private Size prevVideoFormSize = Size.Empty;

        private void fitToVideoCheckBox_CheckedChanged(object sender, EventArgs e)
        {

            fitToVideoMode = fitToVideoCheckBox.Checked;

            UpdateVideoWindow();
            var rect = videoForm?.VideoRectangle ?? Rectangle.Empty;
            renderSession?.Resize(rect);

        }

        private bool fitToVideoMode = false;
        private void UpdateVideoWindow()
        {
            if (videoForm == null)
            {
                return;
            }

            var videoSize = deckLinkInput?.FrameSize ?? Size.Empty;
            videoForm.UpdateWindow(fitToVideoMode, videoSize);
        }

        private static DeckLinkDeviceManager deviceManager = null;

        private void button1_Click(object sender, EventArgs e)
        {

            if(deviceManager == null)
            {
                deviceManager = new DeckLinkDeviceManager();

                deviceManager.InputDeviceArrived += DeviceManager_DeviceArrived;
                deviceManager.InputDeviceRemoved += DeviceManager_DeviceRemoved;

                deviceManager.Startup();

            }


            //var inputs = deviceManager.GetDeckLinkInputs();


            // Task.Run(() =>
            // {

            //     while (true)
            //    {
            //        var inputs = deviceManager.GetDeckLinkInputs();

            //       // var inputs = deviceManager.GetDeckLinkInputs();

            //        logger.Debug(inputs.Count);

            //        Thread.Sleep(2000);
            //    }


            //});

        }


        private void DeviceManager_DeviceArrived(DeckLinkDeviceDescription device)
        {
            //logger.Debug("DeviceManager_DeviceArrived()");

            if (device != null)
            {
                var displayMode = device.DisplayModeIds.FirstOrDefault();

                logger.Info("DeviceArrived: " + device.ToString() + " " + displayMode.ToString());
            }


        }


        private void DeviceManager_DeviceRemoved(DeckLinkDeviceDescription device)
        {
            //logger.Debug("DeviceManager_DeviceRemoved() " + device?.ToString() ?? "");

            if (device != null)
            {
                var displayMode = device.DisplayModeIds.FirstOrDefault();

                logger.Info("DeviceRemoved: " + device.ToString() + " " + displayMode.ToString());
            }


        }



        private void buttonDiscoveryStop_Click(object sender, EventArgs e)
        {
            if (deviceManager != null)
            {
                deviceManager.InputDeviceArrived -= DeviceManager_DeviceArrived;
                deviceManager.InputDeviceRemoved -= DeviceManager_DeviceRemoved;

                deviceManager.Shutdown();
                deviceManager = null;
            }
           
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            try
            {
                var devices = deviceManager.GetInputsFromMTA();
                foreach (var d in devices)
                {
                    var mode = d.DisplayModeIds.FirstOrDefault();

                    logger.Info(d.ToString() + " " + mode?.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            //Task.Run(() =>
            //{
            //    var devices = deviceManager.GetInputs();

            //    foreach (var d in devices)
            //    {
            //        logger.Info(d.ToString());
            //    }
            //});

            //var t = new Thread(() =>
            //{

            //    var devices = deviceManager.GetInputs();

            //    foreach (var d in devices)
            //    {
            //        logger.Info(d.ToString());
            //    }
            //});
            //t.IsBackground = true;
            //t.SetApartmentState(ApartmentState.MTA);

            //t.Start();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var devices = deviceManager.FindInputs();
                foreach (var d in devices)
                {
                    logger.Info(d.ToString());
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }



}
