using DeckLinkAPI;
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

            // MediaManager.Startup();

            //SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = false;
            //SharpDX.Configuration.EnableObjectTracking = true;
            //SharpDX.Diagnostics.ObjectTracker.StackTraceProvider = null;

            MediaToolkitManager.Startup();

            syncContext = SynchronizationContext.Current;


            fitToVideoCheckBox.Checked = fitToVideoMode;

            UpdateVideoWindow();

        }


        private SynchronizationContext syncContext = null;

        private DeckLinkInput deckLinkInput = null;

        private MediaRenderSession renderSession = null;

        private IntPtr windowHandle = IntPtr.Zero;
        private VideoForm videoForm = null;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FindDevices();
        }


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
                
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                devicesPanel.Enabled = false;

                renderSession = new MediaRenderSession();

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

                videoForm.Visible = true;

                deckLinkInput = new DeckLinkInput();
                deckLinkInput.CaptureStarted += DeckLinkInput_CaptureStarted;
                deckLinkInput.CaptureInitialized += DeckLinkInput_ReadyToStart;
                deckLinkInput.CaptureStopped += DeckLinkInput_CaptureStopped;
                deckLinkInput.InputFormatChanged += DeckLinkInput_InputFormatChanged;
                deckLinkInput.InputSignalChanged += DeckLinkInput_InputSignalChanged;
                deckLinkInput.StartCapture(currentDevice, currentDisplayMode);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                CloseVideo();

                devicesPanel.Enabled = false;
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
            }
        }

        private void DeckLinkInput_InputSignalChanged(bool noSignal)
        {
            logger.Debug("DeckLinkInput_InputSignalChanged(...) " + noSignal);

            if (noSignal)
            {
                if (renderSession != null)
                {
                    //renderSession.UpdateStatusText("sfsd");
                }
            }
            else
            {

            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonStop_Click(...)");

            try
            {
                StopCapture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void StopCapture()
        {
            if (deckLinkInput != null)
            {

                deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;
                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                deckLinkInput.StopCapture();
            }
        }

        private void DeckLinkInput_ReadyToStart()
        {
            logger.Debug("DeckLinkInput_ReadyToStart()");

            InitRenderSession();
        }


        private void DeckLinkInput_CaptureStarted()
        {
            logger.Debug("DeckLinkInput_CaptureStarted(...)");

            renderSession?.Start();

        }


        private void DeckLinkInput_InputFormatChanged(object obj) //IDeckLinkDisplayMode newDisplayMode
        {
            logger.Debug("DeckLinkInput_InputFormatChanged(...)");

            CloseRenderSession();

            InitRenderSession();

            renderSession?.Start();

        }


        private void DeckLinkInput_CaptureStopped(object obj)
        {
            logger.Debug("DeckLinkInput_CaptureStopped(...)");

     
            var errorCode = deckLinkInput.ErrorCode;
            deckLinkInput.Shutdown();

            syncContext.Send(_ =>
            {

                CloseRenderSession();

                if (errorCode != 0)
                {
                    var dialogResult = MessageBox.Show("DeckLink unknown error: " + errorCode, "", MessageBoxButtons.RetryCancel);

                    if (dialogResult == DialogResult.Retry)
                    {
                        deckLinkInput.StartCapture(currentDevice, currentDisplayMode);
                        return;
                    }
                }


                CloseVideo();
                devicesPanel.Enabled = true;
                buttonStart.Enabled = true;
                buttonStop.Enabled = false;

            }, null);

        }


        private void CurrentDevice_VideoDataArrived(IntPtr frameData, int frameLength, double frameTime, double frameDuration)
        {

            renderSession.ProcessVideoFrame(frameData, frameLength, frameTime, frameDuration);

        }

        private void CurrentDevice_AudioDataArrived(byte[] data, double time)
        {
            renderSession.ProcessAudioPacket(data, time);
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
            renderSession.Close();
        }


        protected override void OnClosed(EventArgs e)
        {

            if (deckLinkInput != null)
            {
                deckLinkInput.CaptureStarted -= DeckLinkInput_CaptureStarted;
                deckLinkInput.CaptureInitialized -= DeckLinkInput_ReadyToStart;
                deckLinkInput.CaptureStopped -= DeckLinkInput_CaptureStopped;
                deckLinkInput.InputFormatChanged -= DeckLinkInput_InputFormatChanged;

                deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;
                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                deckLinkInput.StopCapture();
            }

            if (renderSession != null)
            {
                renderSession.Close();
                renderSession = null;
            }
                

            base.OnClosed(e);
        }



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


        List<DeckLinkDisplayModeDescription> displayModes = new List<DeckLinkDisplayModeDescription>();

        private DeckLinkDeviceDescription currentDevice = null;
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

        private DeckLinkDisplayModeDescription currentDisplayMode = null;
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

    }



}
