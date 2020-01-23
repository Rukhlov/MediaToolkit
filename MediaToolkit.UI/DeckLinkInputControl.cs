using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MediaToolkit.UI;

using System.Windows.Threading;
using NLog;
using System.ServiceModel;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using MediaToolkit;
using MediaToolkit.Core;
using System.ServiceModel.Discovery;
using MediaToolkit.Utils;

using MediaToolkit.SharedTypes;
using MediaToolkit.DeckLink;

namespace MediaToolkit.UI
{
    public partial class DeckLinkInputControl : UserControl, IDeckLinkInputControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DeckLinkInputControl()
        {
            InitializeComponent();

            syncContext = SynchronizationContext.Current;

            debugPanel.Visible = false;
            statusLabel.Text = "";
            labelStatus.Text = "";

            UptateDetailsButton();

        }


        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode Code => errorCode;

        private volatile bool isCapture = false;
        public bool IsCapture => isCapture;

        private readonly SynchronizationContext syncContext = null;

        private DeckLinkInput deckLinkInput = null;
        private MediaRenderSession renderSession = null;
        private IntPtr windowHandle = IntPtr.Zero;

        public int DeviceIndex { get; private set; } = -1;

        public event Action CaptureStarted;
        public event Action CaptureStopped;

        private int volume = 70;
        public int Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if (volume != value)
                {
                    volume = value;
                }
            }
        }

        private bool mute = false;
        public bool Mute
        {
            get
            {
                
                return mute;
            }
            set
            {
                if (mute != value)
                {
                    mute = value;

                    if (renderSession != null)
                    {
                        renderSession.Mute = mute;
                    }
                }
            }
        }

        public List<DeckLinkDeviceDescription> FindDevices()
        {
            logger.Debug("IDeckLinkInputControl::FindDevices()");

            return DeckLinkTools.GetDeckLinkInputDevices();


        }

        public void StartCapture(int deviceIndex)
        {
            logger.Debug("IDeckLinkInputControl::StartCapture(...) " + deviceIndex);

            StartCapture(deviceIndex, 0, 1769303659);
        }


        public void StartCapture(int deviceIndex, long pixelFormat, long displayModeId )
        { 
            logger.Debug("IDeckLinkInputControl::StartCapture(...) " + string.Join(" ", deviceIndex, pixelFormat, displayModeId));

            try
            {
                DeviceIndex = deviceIndex;
                windowHandle = this.Handle;

                renderSession = new MediaRenderSession();

                deckLinkInput = new DeckLinkInput();
                deckLinkInput.CaptureChanged += DeckLinkInput_CaptureChanged;

                deckLinkInput.VideoDataArrived += CurrentDevice_VideoDataArrived;
                deckLinkInput.AudioDataArrived += CurrentDevice_AudioDataArrived;

                deckLinkInput.StartCapture(deviceIndex, pixelFormat, displayModeId);
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                renderSession?.Close();

                if (deckLinkInput != null)
                {

                    deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;
                    deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;

                    deckLinkInput.CaptureChanged -= DeckLinkInput_CaptureChanged;
                    deckLinkInput.Shutdown();
                    deckLinkInput = null;
                }

                throw;
            }

        }

        public void StopCapture()
        {
            logger.Debug("IDeckLinkInputControl::StopCapture()");

            if (deckLinkInput != null)
            {

                deckLinkInput.AudioDataArrived -= CurrentDevice_AudioDataArrived;
                deckLinkInput.VideoDataArrived -= CurrentDevice_VideoDataArrived;

                deckLinkInput.StopCapture();
            }
        }


        private void DeckLinkInput_CaptureChanged(bool formatChanged)
        {

            logger.Debug("DeckLinkInput_StateChanged(...) " + deckLinkInput.State + " " + formatChanged);


            var state = deckLinkInput.State;
            if (formatChanged)
            {
                if (state == DeckLink.CaptureState.Starting)
                {// new format...

                    AudioRendererArgs audioArgs = GetAudioRenderArgs();
                    VideoRendererArgs videoArgs = GetVideoRenderArgs();

                    renderSession?.Setup(videoArgs, audioArgs);

                    OnCaptureInitialized();
                }
                else if (state == DeckLink.CaptureState.Capturing)
                {//  new format, restart media renderers...

                    if (renderSession != null)
                    {
                        renderSession.Close();

                        AudioRendererArgs audioArgs = GetAudioRenderArgs();
                        VideoRendererArgs videoArgs = GetVideoRenderArgs();

                        renderSession.Setup(videoArgs, audioArgs);

                        OnCaptureInitialized();

                        renderSession.Start();
                    }


                }
            }
            else
            {
                if (state == DeckLink.CaptureState.Capturing)
                {
                    renderSession?.Start();

                    OnCaptureStarted();
                }
                else if (state == DeckLink.CaptureState.Stopped)
                {
                    errorCode = deckLinkInput.ErrorCode;

                    deckLinkInput.Shutdown();

                    renderSession?.Close();

                    OnCaptureStopped();

                }
            }
        }


        private void CurrentDevice_VideoDataArrived(IntPtr frameData, int frameLength, double frameTime, double frameDuration)
        {

            renderSession?.ProcessVideoFrame(frameData, frameLength, frameTime, frameDuration);

        }

        private void CurrentDevice_AudioDataArrived(IntPtr data, int length, double time, double duration)//byte[] data, double time)
        {
            renderSession?.ProcessAudioPacket(data, length, time, duration);
        }



        private void OnCaptureStarted()
        {
            syncContext.Send(_ =>
            {
                if (DebugMode)
                {
                    this.switchCaptureStateButton.Enabled = true;
                    this.switchCaptureStateButton.Text = "_Stop";

                }

                isCapture = true;
                CaptureStarted?.Invoke();

            }, null);
        }

        private void OnCaptureInitialized()
        {
            syncContext.Send(_ =>
            {
                var videoResolution = deckLinkInput.FrameSize;
                var videoFormat = deckLinkInput.VideoFormat;
                var frameRate = deckLinkInput.FrameRate;
                var fps = frameRate.Item2 / frameRate.Item1;
                string videoLog = "";
                {
                    videoLog = videoFormat.Name + "/" + videoResolution.Width + "x" + videoResolution.Height + "/" + fps.ToString("0.00");
                }

                string audioLog = "";
                if (deckLinkInput.AudioEnabled)
                {
                    audioLog = deckLinkInput.AudioSampleRate + "/" + deckLinkInput.AudioBitsPerSample + "/" + deckLinkInput.AudioChannelsCount;
                }

                this.Text = deckLinkInput.DisplayName + " " + videoLog + " " + audioLog;

                renderSession.Resize(this.ClientRectangle);


            }, null);
        }

        private void OnCaptureStopped()
        {
            syncContext.Send(_ =>
            {
                if (DebugMode)
                {
                    switchCaptureStateButton.Text = "_Start";

                    switchCaptureStateButton.Enabled = true;
                    comboBoxDevices.Enabled = true;
                    comboBoxDisplayModes.Enabled = true;
                    findServiceButton.Enabled = true;

                }


                isCapture = false;
                CaptureStopped?.Invoke();

            }, null);
        }



        private VideoRendererArgs GetVideoRenderArgs()
        {

            var frameSize = deckLinkInput.FrameSize;

            var fourCC = deckLinkInput.VideoFormat.FourCC;

            var videoArgs = new VideoRendererArgs
            {
                hWnd = windowHandle,
                Resolution = frameSize,
                FourCC = fourCC,

            };

            return videoArgs;
        }

        private AudioRendererArgs GetAudioRenderArgs()
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

            return audioArgs;
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            renderSession?.Repaint();
            base.OnPaint(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            renderSession?.Resize(this.ClientRectangle);
            base.OnSizeChanged(e);
        }


        private bool debugMode = false;
        public bool DebugMode
        {
            get
            {
                return debugMode;
            }
            set
            {
                if (debugMode != value)
                {
                    debugMode = value;

                    debugPanel.Visible = debugMode;

                    if (debugMode)
                    {
                        findServiceButton.PerformClick();
                    }

                }
            }
        }


        private void switchCaptureStateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("switchCaptureStateButton_Click()");

            if (!isCapture)
            {
                try
                {
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

                    comboBoxDevices.Enabled = false;
                    comboBoxDisplayModes.Enabled = false;
                    findServiceButton.Enabled = false;
                    switchCaptureStateButton.Enabled = false;

                    StartCapture(currentDevice.DeviceIndex, currentDisplayMode.PixFmt, currentDisplayMode.ModeId);


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                    switchCaptureStateButton.Enabled = true;
                    comboBoxDevices.Enabled = true;
                    comboBoxDisplayModes.Enabled = true;
                    findServiceButton.Enabled = true;

                }
            }
            else
            {
                try
                {
                    StopCapture();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void showDetailsButton_Click(object sender, EventArgs e)
        {
            controlPanel.Visible = !controlPanel.Visible;

            UptateDetailsButton();
        }

        private DeckLinkDeviceDescription currentDevice = null;
        private DeckLinkDisplayModeDescription currentDisplayMode = null;

        private List<DeckLinkDisplayModeDescription> displayModes = new List<DeckLinkDisplayModeDescription>();
        private List<DeckLinkDeviceDescription> decklinkDevices = new List<DeckLinkDeviceDescription>();

        private void findServiceButton_Click(object sender, EventArgs e)
        {
            logger.Debug("findServiceButton_Click(...)");

            try
            {
                decklinkDevices = FindDevices();

                if (decklinkDevices.Count == 0)
                {
                    throw new Exception("This application requires a DeckLink PCI card.\n" +
                        "You will not be able to use the features of this application until a DeckLink PCI card is installed.");

                }

                comboBoxDevices.DataSource = decklinkDevices;
                //comboBoxDevices.DisplayMember = "DeviceName";
            }
            catch (Exception ex)
            {
                logger.Error(ex);


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

                    displayModes.Add(new DeckLinkDisplayModeDescription { Description = "Auto" });
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

        private void UptateDetailsButton()
        {

            showDetailsButton.Text = controlPanel.Visible ? "<<" : ">>";

        }
    }


}
