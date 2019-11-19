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

        private MainForm mainForm = null;

        private bool isStreaming = false;
        private ScreenStreamer videoStreamer = null;
        private ScreenSource screenSource = null;


        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = null;
        private RegionForm regionForm = null;


        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;

        private TransportMode transportMode = TransportMode.Udp;
        private VideoSettingsParams videoSettings = new VideoSettingsParams();
        private AudioSettingsParams audioSettings = new AudioSettingsParams();

        private ScreencastCommunicationService communicationService = null;

        public List<ScreencastChannelInfo> ScreencastChannelsInfos { get; private set; } = new List<ScreencastChannelInfo>();

        public void Link(MainForm parent)
        {
            this.mainForm = parent;
        }

        public void UnLink()
        {
            //...
            mainForm = null;
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...) ");

            try
            {
                

                Start();

                UpdateControls();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Stop();
            }

           
        }


        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...)");
            try
            {
                Stop();

                UpdateControls();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void Start()
        {
            var networkIpAddr = "0.0.0.0";
            var ipInfo = mainForm.GetCurrentIpAddrInfo();

            if (ipInfo != null)
            {
                networkIpAddr = ipInfo.Address.ToString();
            }

            transportMode = (TransportMode)transportComboBox.SelectedItem;
            if (isMulticastMode)
            {
                transportMode = TransportMode.Udp;
            }

            var videoEnabled = videoSettings.Enabled;
            var audioEnabled = audioSettings.Enabled;

            var multicastAddr = multicastAddressTextBox.Text;
            var multicastVideoPort = 1234;
            var multicastAudioPort = 1235;

            int communicationPort = 0;
            var communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";

            //var communicationAddress = "net.tcp://" + networkIpAddr +":"+ communicationPort + "/ScreenCaster/" + Guid.NewGuid();

            videoSettings.TransportMode = transportMode;
            audioSettings.TransportMode = transportMode;

            logger.Info("NetworkIP=" + networkIpAddr +
                " MulticastMode=" + isMulticastMode +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled +
                " CommunicationAddress=" + communicationAddress);

            if (videoEnabled)
            {
                if (videoSettings.DisplayRegion.IsEmpty)
                {
                    logger.Debug("VideoSource Disabled");
                    return;
                }

                StartVideoSource(videoSettings);

                regionForm = new RegionForm(videoSettings.CaptureRegion);
                regionForm.Visible = true;

                statisticForm.Location = videoSettings.CaptureRegion.Location;//currentScreenRect.Location;
                if (showStatistic)
                {
                    statisticForm.Start();
                }

                if (isMulticastMode)
                {
                    //videoAddr = multicastAddr;
                    //videoPort = multicastVideoPort;
                    videoSettings.Address = multicastAddr;
                    videoSettings.Port = multicastVideoPort;
                    StartVideoStream(videoSettings);
                }
                else
                {
                    if (transportMode == TransportMode.Tcp)
                    {
                        videoSettings.Address = networkIpAddr;
                        videoSettings.Port = 1234;
                        //videoAddr = networkIpAddr;
                        //videoPort = 1234;
                    }
                }

                VideoChannelInfo videoInfo = new VideoChannelInfo
                {
                    Id = videoSettings.SessionId,
                    VideoEncoder = videoSettings.Encoder,
                    Resolution = videoSettings.VideoResoulution,
                    Bitrate = videoSettings.Bitrate,

                    Fps = videoSettings.Fps
                };

                ScreencastChannelInfo videoChannelInfo = new ScreencastChannelInfo
                {
                    Address = videoSettings.Address,
                    Port = videoSettings.Port,
                    Transport = videoSettings.TransportMode,
                    IsMulticast = isMulticastMode,
                    MediaInfo = videoInfo,
                };

                ScreencastChannelsInfos.Add(videoChannelInfo);

            }

            if (audioEnabled)
            {
                var currentMMDeviceId = currentAudioDevice?.ID ?? "";

                if (string.IsNullOrEmpty(audioSettings.AudioDeviceId))
                {
                    logger.Debug("Empty MMDeviceId...");
                    return;
                }

                StartAudioSource(audioSettings);

                if (isMulticastMode)
                {
                    //audioAddr = multicastAddr;
                    //audioPort = multicastAudioPort;
                    audioSettings.Address = multicastAddr;
                    audioSettings.Port = multicastAudioPort;

                    StartAudioStream(audioSettings);
                }
                else
                {
                    if (transportMode == TransportMode.Tcp)
                    {
                        audioSettings.Address = networkIpAddr;
                        audioSettings.Port = 1235;
                        //audioAddr = networkIpAddr;
                        //audioPort = 1235;
                    }
                }

                AudioChannelInfo audioInfo = new AudioChannelInfo
                {
                    Id = videoSettings.SessionId,
                    AudioEncoder = audioSettings.Encoder,
                    SampleRate = audioSettings.Samplerate,
                    Channels = audioSettings.Channels,
                };

                ScreencastChannelInfo audioChannelInfo = new ScreencastChannelInfo
                {
                    Address = audioSettings.Address,
                    Port = audioSettings.Port,
                    IsMulticast = isMulticastMode,
                    Transport = audioSettings.TransportMode,
                    MediaInfo = audioInfo,
                };
                ScreencastChannelsInfos.Add(audioChannelInfo);

            }

            communicationService = new ScreencastCommunicationService(this);


            communicationService.Open(communicationAddress);

            isStreaming = true;


        }


        private bool StartVideoSource(VideoSettingsParams settings)
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
                    SrcRect = settings.CaptureRegion,
                    DestSize = settings.VideoResoulution,
                    CaptureType = CaptureType.DXGIDeskDupl,
                    Fps = settings.Fps,
                    CaptureMouse = settings.CaptureMouse,
                    AspectRatio = settings.AspectRatio,

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

        private void StartVideoStream(VideoSettingsParams settings)
        {
            logger.Debug("StartVideo()");

            var localAddr = "0.0.0.0";


            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {

                LocalAddr = localAddr,
                LocalPort = settings.Port,

                RemoteAddr = settings.Address,
                RemotePort = settings.Port,

                TransportMode = settings.TransportMode,
            };

            VideoEncodingParams encodingParams = new VideoEncodingParams
            {
                Width = settings.VideoResoulution.Width, // options.Width,
                Height = settings.VideoResoulution.Height, // options.Height,
                FrameRate = settings.Fps,
                Bitrate = settings.Bitrate,
                LowLatency = settings.LowLatency,
                Profile = settings.Profile,
                BitrateMode = settings.BitrateMode,
                MaxBitrate = settings.MaxBitrate,
            };

            videoStreamer = new ScreenStreamer(screenSource);

            videoStreamer.Setup(encodingParams, networkParams);

            var streamerTask = videoStreamer.Start();
        }



        private void StartAudioStream(AudioSettingsParams settings )
        {
            logger.Debug("StartAudio()");


            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",

            };


            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                LocalPort = settings.Port,
                LocalAddr = "",
                RemoteAddr = settings.Address,
                RemotePort = settings.Port,
                TransportMode = settings.TransportMode,
            };

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            // audioStreamer.SetWaveformPainter(new[] { this.waveformPainter1, this.waveformPainter2 });

            audioStreamer.StateChanged += AudioStreamer_StateChanged;


            audioStreamer.Start(audioParams, networkParams);

        }

        private void StartAudioSource(AudioSettingsParams settings)
        {
            audioSource = new AudioSource();

            audioSource.Setup(settings.AudioDeviceId);

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

        private void StopAudioSource()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void StopAudioStream()
        {
            if (audioStreamer != null)
            {
                audioStreamer.SetWaveformPainter(null);
                audioStreamer.Close();
            }
        }



        private void Stop()
        {
            logger.Debug("ScreenStreamerControl::Stop()");

            StopVideoSource();

            StopVideoStream();

            StopAudioSource();

            StopAudioStream();

            communicationService?.Close();

            isStreaming = false;
        }

        private void StopVideoStream()
        {
            logger.Debug("ScreenStreamerControl::StopVideoStream()");
            if (videoStreamer != null)
            {
                videoStreamer.Close();

            }
        }

        private void StopVideoSource()
        {
            logger.Debug("ScreenStreamerControl::StopVideoSource()");

            if (screenSource != null)
            {
                screenSource.Stop();

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
                if (StartVideoSource(videoSettings))
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

            videoSettings.Enabled = !displayRect.IsEmpty;

            UpdateControls();
        }


        private void UpdateControls()
        {
            //this.settingPanel.Enabled = !isStreaming;

            this.startButton.Enabled = !isStreaming;
            //this.videoPreviewButton.Enabled = isStreaming;

            this.stopButton.Enabled = isStreaming;

            var videoEnabled = videoSettings.Enabled;
            videoSettingsButton.Enabled = videoEnabled;
            videoSourcesComboBox.Enabled = videoEnabled;

            videoUpdateButton.Enabled = videoEnabled;

            var audioEnabled = audioSettings.Enabled;
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


        public void Play(ScreencastChannelInfo[] infos)
        {
            logger.Debug("ScreenStreamerControl::Play()");
            //...

            //videoSettings.Address = addr;
            //videoSettings.Port = port;

            if (isMulticastMode)
            {
                return;
            }

            if (videoSettings.Enabled)
            {
                if(transportMode == TransportMode.Udp)
                {
                    var clientInfo = infos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
                    if (clientInfo != null)
                    {
                        videoSettings.Address = clientInfo.Address;
                        videoSettings.Port = clientInfo.Port;
                    }

                }
                StartVideoStream(videoSettings);
            }

            if (audioSettings.Enabled)
            {
                if (transportMode == TransportMode.Udp)
                {
                    var clientInfo = infos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
                    if (clientInfo != null)
                    {
                        videoSettings.Address = clientInfo.Address;
                        videoSettings.Port = clientInfo.Port;
                    }
                    //videoSettings.Address = 
                }

                StartAudioStream(audioSettings);
            }

        }
  
        public void Teardown()
        {
            logger.Debug("ScreenStreamerControl::Teardown()");
            //...

            StopVideoStream();

            StopAudioStream();

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

            videoSettings.TransportMode = transportMode;

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
                TransportMode.Tcp,
                TransportMode.Udp,
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


        private MMDevice currentAudioDevice = null;
        private void audioSrcComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            currentAudioDevice = GetCurrentAudioDevice();
            audioSettings.Enabled = (currentAudioDevice != null);

            audioSettings.AudioDeviceId = currentAudioDevice?.ID ?? "";

            audioSettingButton.Enabled = audioSettings.Enabled;

        }

       
        private void audioSettingButton_Click(object sender, EventArgs e)
        {
            //videoSettings.CaptureRegion = currentScreenRect;

            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(audioSettings);

            f.ShowDialog();

            audioSettings.TransportMode = transportMode;

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
            audioSettings.Enabled = audioEnabledCheckBox.Checked;

            UpdateControls();
        }

        private void videoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            videoSettings.Enabled = videoEnabledCheckBox.Checked;
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

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void networkPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Play(ScreencastChannelsInfos.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Teardown();
        }

        private void transportComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = transportComboBox.SelectedItem;
            if (item!=null)
            {
                this.transportMode = (TransportMode)transportComboBox.SelectedItem;
            }
          
        }
    }

    public class AudioSettingsParams
    {
        public bool Enabled = true;

        public string Address = "";
        public int Port = -1;
        public TransportMode TransportMode = TransportMode.Udp;

        public AudioEncoderMode Encoder = AudioEncoderMode.G711;

        public int Samplerate = 8000;
        public int Channels = 1;

        public string SessionId = "";

        public string AudioDeviceId = "";

        public AudioSettingsParams Clone()
        {
            return (AudioSettingsParams)this.MemberwiseClone();
        }
    }

    public class VideoSettingsParams
    {
        public bool Enabled = true;
        public string SessionId = "";

        public string Address = "";
        public int Port = -1;
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
