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
        }


        private SynchronizationContext syncContext = null;

        private DeckLinkInput deckLinkInput = null;

        private MediaRenderSession renderSession = null;

        private IntPtr windowHandle = IntPtr.Zero;
        private Form videoForm = null;


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

                renderSession = new MediaRenderSession();

                videoForm = new Form
                {
                    BackColor = Color.Black,
                    StartPosition = FormStartPosition.CenterScreen,
                    //ClientSize = videoResoulution,
                };
                windowHandle = videoForm.Handle;

                videoForm.Paint += (o, a) =>
                {
                    renderSession?.Repaint();
                };

                videoForm.SizeChanged += (o, a) =>
                {
                    renderSession?.Resize(videoForm.ClientRectangle);
                };

                videoForm.FormClosed += (o, a) =>
                {
                    deckLinkInput?.StopCapture();
                };
                videoForm.Visible = true;

                deckLinkInput = new DeckLinkInput();
                deckLinkInput.CaptureStarted += DeckLinkInput_CaptureStarted;
                deckLinkInput.ReadyToStart += DeckLinkInput_ReadyToStart;
                deckLinkInput.CaptureStopped += DeckLinkInput_CaptureStopped;
                deckLinkInput.InputFormatChanged += DeckLinkInput_InputFormatChanged;

                //deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
                //deckLinkInput.AudioDataArrived += _selectedDevice_AudioDataArrived;

                
                deckLinkInput.StartCapture(deviceIndex);//this.Handle);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                CloseVideo();

                buttonStart.Enabled = true;
                buttonStop.Enabled = false;
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
            var pixelFormat = deckLinkInput.PixelFormatCode;

            var videoArgs = new VideoRendererArgs
            {
                hWnd = windowHandle,
                Resolution = videoResoulution,
                PixelFormat = pixelFormat,

            };

            renderSession.Setup(videoArgs, audioArgs);

            deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
            deckLinkInput.AudioDataArrived += CurrentDevice_AudioDataArrived;
        
            syncContext.Send(_ =>
            {
                ShowVideo();


            }, null);
        }

        private void ShowVideo()
        {

            var videoResoulution = deckLinkInput.FrameSize;
            var pixelFormat = deckLinkInput.PixelFormatCode;
            var frameRate = deckLinkInput.FrameRate;
            var fps = frameRate.Item2 / frameRate.Item1;
            string videoLog = "";
            {
                videoLog = pixelFormat + "/" + videoResoulution.Width + "x" + videoResoulution.Height + "/" + fps.ToString("0.00");
            }

            string audioLog = "";
            if (deckLinkInput.AudioEnabled)
            {
                audioLog = deckLinkInput.AudioSampleRate + "/" + deckLinkInput.AudioBitsPerSample + "/" + deckLinkInput.AudioChannelsCount;
            }

            videoForm.Text = deckLinkInput.DisplayName + " " + videoLog + " " + audioLog;
           // videoForm.Visible = true;


            renderSession.Resize(videoForm.ClientRectangle);
        }

        private void CloseVideo()
        {
            if (videoForm != null)
            {
                videoForm.Close();
                videoForm = null;
            }
        }

        private void DeckLinkInput_CaptureStarted()
        {
            logger.Debug("DeckLinkInput_CaptureStarted(...)");

            renderSession?.Start();


        }

        private void DeckLinkInput_CaptureStopped(object obj)
        {

            logger.Debug("DeckLinkInput_CaptureStopped(...)");

            deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;
            deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;

            renderSession.Close();

            syncContext.Send(_ =>
            {

                var errorCode = deckLinkInput.ErrorCode;
                if (errorCode != 0)
                {
                    MessageBox.Show("DeckLink unknown error: " + errorCode);
                }

                CloseVideo();

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

        private void DeckLinkInput_InputFormatChanged(object obj) //IDeckLinkDisplayMode newDisplayMode
        {
            logger.Debug("DeckLinkInput_InputFormatChanged(...)");
            //...
        }


        protected override void OnClosed(EventArgs e)
        {

            if (deckLinkInput != null)
            {
                deckLinkInput.CaptureStarted -= DeckLinkInput_CaptureStarted;
                deckLinkInput.ReadyToStart -= DeckLinkInput_ReadyToStart;
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


                }
            }
        }


        private List<DeckLinkDeviceDescription> decklinkDevices = new List<DeckLinkDeviceDescription>();

        class DeckLinkDeviceDescription
        {
            public int DeviceIndex = -1;
            public string DeviceName = "";
            public bool Available = false;

            public override string ToString()
            {
                return DeviceName + " " + (Available ? "(Available)" : "(Not Available)");
            }
        }

        private void buttonFind_Click(object sender, EventArgs e)
        {
            logger.Debug("buttonFind_Click(...)");


            decklinkDevices = new List<DeckLinkDeviceDescription>();
            IDeckLinkIterator deckLinkIterator = null;
            try
            {
                deckLinkIterator = new CDeckLinkIterator();

                int index = 0;
                IDeckLink deckLink = null;
                do
                {
                    if (deckLink != null)
                    {
                        Marshal.ReleaseComObject(deckLink);
                        deckLink = null;
                    }

                    deckLinkIterator.Next(out deckLink);

                    if(deckLink == null)
                    {
                        break;
                    }

                    deckLink.GetDisplayName(out string deviceName);

                    try
                    {
                        var deckLinkInput = (IDeckLinkInput)deckLink;
                        var deckLinkStatus = (IDeckLinkStatus)deckLink;

                        bool available = false;
                        deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
                        available = (videoInputSignalLockedFlag != 0);

                        DeckLinkDeviceDescription deviceDescription = new DeckLinkDeviceDescription
                        {
                            DeviceIndex = index,
                            DeviceName = deviceName,
                            Available = available
                        };

                        decklinkDevices.Add(deviceDescription);

                        //Marshal.ReleaseComObject(deckLinkInput);
                        //Marshal.ReleaseComObject(deckLinkStatus);

                    }
                    catch (InvalidCastException)
                    {

                    }

                    index++;

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

            comboBoxDevices.DataSource = decklinkDevices;
           //comboBoxDevices.DisplayMember = "DeviceName";
        }

        private void FullScreenCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (FullScreenCheckBox.Checked)
            {
                this.videoForm.FormBorderStyle = FormBorderStyle.None;
                this.videoForm.Location = new Point(0, 0);
                this.videoForm.Size = new Size(1920, 1080);
            }
            else
            {
                this.videoForm.FormBorderStyle = FormBorderStyle.Sizable;
            }
            
        }
    }



}
