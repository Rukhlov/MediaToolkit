using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaToolkit;
using System.Net.NetworkInformation;
using MediaToolkit.Common;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using MediaToolkit.UI;

namespace TestStreamer.Controls
{
    public partial class ScreenStreamerControl : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ScreenStreamerControl()
        {
            InitializeComponent();

            UpdateVideoSources();
            LoadTransportItems();

            UpdateAudioSources();

            SetupRoutingSchema();

            UpdateControls();

            statInfoCheckBox.Checked = showStatistic;

        }

        private readonly MainForm mainForm = null;

        private bool isStreaming = false;
        private ScreenStreamer videoStreamer = null;
        private ScreenSource screenSource = null;


        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = null;

        private RegionForm regionForm = null;


        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;
        

        private void startButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...)");

            if (videoEnabled)
            {
                StartVideo();
            }

            if (audioEnabled)
            {
                StartAudio();
            }
            
            isStreaming = true;

            UpdateControls();
        }

        private void StartVideo()
        {
            logger.Debug("StartVideo()");

            var localAddr = "0.0.0.0";

            if (videoSettings.DisplayRegion.IsEmpty)
            {
                logger.Debug("VideoSource Disabled");
                return;
            }

            StartVideoSource();

            var cmdOptions = new CommandLineOptions();
            cmdOptions.IpAddr = videoSettings.Address;
            cmdOptions.Port = videoSettings.Port;

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {

                LocalAddr = localAddr,
                LocalPort = cmdOptions.Port,

                RemoteAddr = cmdOptions.IpAddr,
                RemotePort = cmdOptions.Port,

                TransportMode = videoSettings.TransportMode,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = videoSettings.VideoResoulution.Width, // options.Width,
                Height = videoSettings.VideoResoulution.Height, // options.Height,
                FrameRate = cmdOptions.FrameRate,
                EncoderName = "libx264", // "h264_nvenc", //
                Bitrate = videoSettings.Bitrate,
                LowLatency = videoSettings.LowLatency,
                Profile = videoSettings.Profile,
                BitrateMode = videoSettings.BitrateMode,
                MaxBitrate = videoSettings.MaxBitrate,
            };

            videoStreamer = new ScreenStreamer(screenSource);

            videoStreamer.Setup(encodingParams, networkParams);

            regionForm = new RegionForm(videoSettings.CaptureRegion);
            regionForm.Visible = true;

            statisticForm.Location = videoSettings.CaptureRegion.Location;//currentScreenRect.Location;
            if (showStatistic)
            {
                statisticForm.Start();
            }

            var streamerTask = videoStreamer.Start();
        }

        private bool StartVideoSource()
        {
            bool Result = false;
            try
            {
                if (screenSource != null)
                {
                    screenSource.StateChanged -= ScreenSource_StateChanged;
                    screenSource.Dispose();
                    screenSource = null;
                }

                screenSource = new ScreenSource();
                screenSource.StateChanged += ScreenSource_StateChanged;

                ScreenCaptureParams captureParams = new ScreenCaptureParams
                {
                    SrcRect = videoSettings.CaptureRegion,
                    DestSize = videoSettings.VideoResoulution,
                    CaptureType = CaptureType.DXGIDeskDupl,
                    Fps = videoSettings.Fps,
                    CaptureMouse = videoSettings.CaptureMouse,
                    AspectRatio = videoSettings.AspectRatio,

                };

                screenSource.Setup(captureParams);

                screenSource.Start();

               

                Result = true;
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                if (screenSource != null)
                {
                    screenSource.StateChanged -= ScreenSource_StateChanged;
                    screenSource.Dispose();
                    screenSource = null;
                }
            }

            return Result;
        }

        private void ScreenSource_StateChanged(MediaToolkit.CaptureState state)
        {

            logger.Debug("ScreenSource_StateChanged(...) " + state);


        }

        private void StartAudio()
        {
            logger.Debug("StartAudio()");

            var currentMMDeviceId = currentAudioDevice?.ID ?? "";

            if (string.IsNullOrEmpty(currentMMDeviceId))
            {
                logger.Debug("Empty MMDeviceId...");
                return;
            }

            StartAudioSource(currentMMDeviceId);

            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",

            };


            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                LocalPort = audioSettings.Port,
                LocalAddr = "",
                RemoteAddr = audioSettings.Address,
                RemotePort = audioSettings.Port,
                TransportMode = audioSettings.TransportMode,
            };

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            // audioStreamer.SetWaveformPainter(new[] { this.waveformPainter1, this.waveformPainter2 });

            audioStreamer.StateChanged += AudioStreamer_StateChanged;


            audioStreamer.Start(audioParams, networkParams);

        }

        private void StartAudioSource(string currentMMDeviceId)
        {
            audioSource = new AudioSource();
            audioSource.Setup(currentMMDeviceId);

            audioStreamer = new AudioStreamer(audioSource);
            audioSource.Start();
        }

        private void AudioStreamer_StateChanged()
        {
            if (audioStreamer.IsStreaming)
            {

            }
            else
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            this.Invoke((Action)(() =>
            {
                //UpdateAudioControls();
            }));
        }

        private void StopAudio()
        {
            if (audioStreamer != null)
            {
                audioStreamer.SetWaveformPainter(null);
                audioStreamer.Close();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...)");

            StopVideo();

            StopAudio();

            isStreaming = false;

            UpdateControls();

        }

        private void StopVideo()
        {
            if (screenSource != null)
            {
                screenSource.Stop();
              
            }

            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm = null;
            }

            regionForm?.Close();
            regionForm = null;
        }

        private D3DImageProvider provider = new D3DImageProvider();

        private void previewButton_Click(object sender, EventArgs e)
        {
            logger.Debug("previewButton_Click(...)");

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = !previewForm.Visible;
            }
            else
            {
                if (StartVideoSource())
                {
                    //provider = new D3DImageProvider();

                    provider.Setup(screenSource);

                    previewForm = new PreviewForm
                    {
                        StartPosition = FormStartPosition.CenterParent,
                    };
                    
                    previewForm.Link(provider);

                    var pars = screenSource.CaptureParams;

                    var title = "Src" + pars.SrcRect + "->Dst" + pars.DestSize + " Fps=" + pars.Fps + " Ratio=" + pars.AspectRatio;

                    previewForm.Text = title;

                    previewForm.Visible = true;
                }



               
            }
        }

        private void screensUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateVideoSources();
        }

        private BindingList<ComboBoxItem> screenItems = null;
        private void UpdateVideoSources()
        {
            var screens = Screen.AllScreens
                .Select(s => new ComboBoxItem
                {
                    Name = s.DeviceName,//+ "" + s.Bounds.ToString(),
                    Tag = s.Bounds }
                ).ToList();

            screens.Add(new ComboBoxItem
            {
                Name = "_AllScreen",
                Tag = SystemInformation.VirtualScreen
            });

            //screens.Add(new ComboBoxItem
            //{
            //    Name = "_Disabled",
            //    Tag = null
            //});

            screenItems = new BindingList<ComboBoxItem>(screens);
            videoSourcesComboBox.DisplayMember = "Name";
            videoSourcesComboBox.DataSource = screenItems;
        }

        private bool videoEnabled = true;

        private void videoSourcesComboBox_SelectedValueChanged_1(object sender, EventArgs e)
        {

            Rectangle displayRect = Rectangle.Empty;
            string displayName = "";
            var obj = videoSourcesComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null)
                    {
                        displayRect = (Rectangle)tag;
                        displayName = item.Name ;
                    }
                }
            }


            videoSettings.DisplayName = displayName;
            videoSettings.DisplayRegion = displayRect;

            videoSettings.CaptureRegion = displayRect;

            videoEnabled = !displayRect.IsEmpty;

            UpdateControls();
        }


        private void UpdateControls()
        {
            //this.settingPanel.Enabled = !isStreaming;

            this.startButton.Enabled = !isStreaming;
            //this.videoPreviewButton.Enabled = isStreaming;

            this.stopButton.Enabled = isStreaming;


            videoSettingsButton.Enabled = videoEnabled;
            videoSourcesComboBox.Enabled = videoEnabled;

            videoUpdateButton.Enabled = videoEnabled;

            audioSettingButton.Enabled = audioEnabled;
            audioSourcesComboBox.Enabled = audioEnabled;
            audioUpdateButton.Enabled = audioEnabled;


            videoSourcesComboBox.Enabled = !isStreaming;
            audioSourcesComboBox.Enabled = !isStreaming;

            videoSettingsButton.Enabled = !isStreaming;
            audioSettingButton.Enabled = !isStreaming;

            videoEnabledCheckBox.Enabled = !isStreaming;
            audioEnabledCheckBox.Enabled = !isStreaming;

            videoUpdateButton.Enabled = !isStreaming;
            audioUpdateButton.Enabled = !isStreaming;
            networkPanel.Enabled = !isStreaming;

            //videoPreviewButton.Enabled = true;
            //audioPreviewButton.Enabled = true;

            //this.fpsNumeric2.Enabled = !ServiceHostOpened;
            //this.inputSimulatorCheckBox2.Enabled = !ServiceHostOpened;
            //this.screensComboBox2.Enabled = !ServiceHostOpened;
            //this.screensUpdateButton2.Enabled = !ServiceHostOpened;

        }

  

        private void MaxBitrateNumeric_ValueChanged(object sender, EventArgs e)
        {

        }

        private bool showStatistic = true;
        private void statInfoCheckBox_CheckedChanged(object sender, EventArgs e)
        {

            showStatistic = statInfoCheckBox.Checked;

            if (showStatistic)
            {
                statisticForm.Start();
            }
            else
            {
                if (statisticForm != null)
                {
                    statisticForm.Stop();
                    statisticForm.Visible = false;
                }
            }
        }

        private VideoSettingsParams videoSettings = new VideoSettingsParams();
        private void videoSettingsButton_Click(object sender, EventArgs e)
        {
            //videoSettings.CaptureRegion = currentScreenRect;
           // videoSettings.DisplayName = currentScreen?.DeviceName ?? "_AllScreen";

            var f = new VideoSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(videoSettings);

            f.ShowDialog();

            //currentScreenRect = videoSettings.CaptureRegion;
        }



        private bool isMulticastMode = true;

        private void multicastRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetupRoutingSchema();
        }

        private void unicastRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SetupRoutingSchema();
        }

        private void SetupRoutingSchema()
        {
            isMulticastMode = multicastRadioButton.Checked;
            multicastAddressTextBox.Enabled = isMulticastMode;
            transportComboBox.Enabled = !isMulticastMode;
        }

        private void LoadTransportItems()
        {

            var items = new List<TransportMode>
            {
                TransportMode.Udp,
                TransportMode.Tcp,

            };
            transportComboBox.DataSource = items;
        }

        private MMDevice GetCurrentAudioDevice()
        {
            MMDevice device = null;

            var item = audioSourcesComboBox.SelectedItem;
            if (item != null)
            {
                device = (MMDevice)((item as ComboBoxItem)?.Tag) ?? null;
            }

            return device;
        } 

        private void UpdateAudioSources()
        {

            List<MMDevice> mmdevices = new List<MMDevice>();

            try
            {
                using (var deviceEnum = new MMDeviceEnumerator())
                {

                    var defaultCaptureId = "";
                    try
                    {
                        if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console))
                        {
                            var captureDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                            if (captureDevice != null)
                            {

                                defaultCaptureId = captureDevice.ID;
                                mmdevices.Add(captureDevice);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }

                    var defaultRenderId = "";
                    try
                    {
                        if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                        {
                            var renderDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                            if (renderDevice != null)
                            {
                                defaultRenderId = renderDevice.ID;
                                mmdevices.Add(renderDevice);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }

                    try
                    {

                        var allDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
                        foreach (var d in allDevices)
                        {
                            if (d.ID == defaultRenderId || d.ID == defaultCaptureId)
                            {
                                continue;
                            }
                            mmdevices.Add(d);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            var dataSource = new BindingList<ComboBoxItem>(mmdevices.Select(d => new ComboBoxItem { Name = d.FriendlyName, Tag = d }).ToList());
            //dataSource.Add(new ComboBoxItem { Name = "Disabled", Tag = null, });

            audioSourcesComboBox.DataSource = dataSource;
            audioSourcesComboBox.DisplayMember = "Name";


        }

        private bool audioEnabled = true;
        private MMDevice currentAudioDevice = null;
        private void audioSrcComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            currentAudioDevice = GetCurrentAudioDevice();
            audioEnabled = (currentAudioDevice != null);

            audioSettingButton.Enabled = audioEnabled;

        }

        private AudioSettingsParams audioSettings = new AudioSettingsParams();
        private void audioSettingButton_Click(object sender, EventArgs e)
        {
            //videoSettings.CaptureRegion = currentScreenRect;

            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(audioSettings);

            f.ShowDialog();

            //currentScreenRect = videoSettings.CaptureRegion;
        }

        private void videoUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateVideoSources();
        }

        private void audioUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateAudioSources();
        }

        private void audioEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            audioEnabled = audioEnabledCheckBox.Checked;
            UpdateControls();
        }

        private void videoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            videoEnabled = videoEnabledCheckBox.Checked;
            UpdateControls();
        }

        private AudioPreviewForm audioPreviewForm = null;
        private void audioPreviewButton_Click(object sender, EventArgs e)
        {
            if (audioPreviewForm != null && !audioPreviewForm.IsDisposed)
            {
                audioPreviewForm.Visible = !audioPreviewForm.Visible;
            }
            else
            {

                audioPreviewForm = new AudioPreviewForm();

                audioPreviewForm.Visible = true;


            }
        }
    }

    public class VideoSettingsParams
    {
        public string Address = "127.0.0.1";
        public int Port = 1234;
        public TransportMode TransportMode = TransportMode.Udp;
        public Rectangle CaptureRegion = Rectangle.Empty;
        public string DisplayName = "";
        public Rectangle DisplayRegion = Rectangle.Empty;

        public bool CaptureMouse = true;
        public Size VideoResoulution = new Size(1920, 1080);
        public bool AspectRatio = true;
        public VideoEncoderMode Encoder = VideoEncoderMode.H264;
        public H264Profile Profile = H264Profile.Main;
        public BitrateControlMode BitrateMode = BitrateControlMode.CBR;
        public int Bitrate = 2500;
        public int MaxBitrate = 5000;
        public int Fps = 30;
        public bool LowLatency = true;

        public VideoSettingsParams Clone()
        {
            return (VideoSettingsParams)this.MemberwiseClone();
        }
    }


}
