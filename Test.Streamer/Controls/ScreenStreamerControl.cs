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
using MediaToolkit.Core;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using MediaToolkit.UI;
using SharpDX.MediaFoundation;
using MediaToolkit.Utils;
using MediaToolkit.NativeAPIs;
using System.Threading;
using MediaToolkit.Managers;

namespace TestStreamer.Controls
{
    public partial class ScreenStreamerControl : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ScreenStreamerControl()
        {
            InitializeComponent();


            syncContext = SynchronizationContext.Current;

            UsbManager.RegisterNotification(this.Handle, KS.KSCATEGORY_VIDEO_CAMERA);

            InitMediaSettings();

            
            LoadTransportItems();

            UpdateVideoSources();
            UpdateAudioSources();

            SetupRoutingSchema();

            UpdateControls();

            statInfoCheckBox.Checked = showStatistic;

        }

        private SynchronizationContext syncContext = null;

        private MainForm mainForm = null;

        private bool isStreaming = false;
        private VideoStreamer videoStreamer = null;
        private IVideoSource videoSource = null;


        private StatisticForm statisticForm = new StatisticForm();
        private PreviewForm previewForm = null;
        private RegionForm regionForm = null;


        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;

        private TransportMode transportMode = TransportMode.Tcp;

        private VideoStreamSettings videoSettings = null;
        private VideoEncoderSettings videoEncoderSettings = null;
        private ScreenCaptureDeviceDescription screenCaptureDeviceDescr = null;


        private AudioStreamSettings audioSettings = null;
        private AudioEncoderSettings audioEncoderSettings = null;

        private void InitMediaSettings()
        {
            screenCaptureDeviceDescr = new ScreenCaptureDeviceDescription
            {
                Resolution = new Size(1920, 1080),
                CaptureMouse = true,
                AspectRatio = true,
                CaptureType = VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
            };

            videoEncoderSettings = new VideoEncoderSettings
            {
                Resolution = new Size(1920, 1080),
                Encoder = VideoEncoderMode.H264,
                Profile = H264Profile.Main,
                BitrateMode = BitrateControlMode.CBR,
                Bitrate = 2500,
                MaxBitrate = 5000,
                FrameRate = 30,
                LowLatency = true,
            };

            videoSettings = new VideoStreamSettings
            {
                Enabled = true,
                SessionId = "video_" + Guid.NewGuid().ToString(),
                NetworkParams = new NetworkSettings(),
                CaptureDescription = null,
                EncodingParams = videoEncoderSettings,
            };

            audioEncoderSettings = new AudioEncoderSettings
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",

            };

            audioSettings = new AudioStreamSettings
            {
                Enabled = true,
                SessionId = "audio_" + Guid.NewGuid().ToString(),
                NetworkParams = new NetworkSettings(),
                CaptureParams = new AudioCaptureSettings(),
                EncodingParams = audioEncoderSettings,
            };

        }

        private ScreencastCommunicationService communicationService = null;
        private string communicationAddress = "";

        public List<ScreencastChannelInfo> ScreencastChannelsInfos { get; private set; } = new List<ScreencastChannelInfo>();

        public void Link(MainForm parent)
        {
            this.mainForm = parent;
        }

        public void UnLink()
        {
            StopStreaming();

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

            //...
            mainForm = null;

        }

        private void startButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...) ");

            try
            {
                UpdateSettings();

                mainForm.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                Task.Run(() =>
                {
                    try
                    {
                        SetupStreaming(communicationAddress);

                        StartStreaming();
                    }
                    catch (Exception ex)
                    {
                        StopStreaming();
                        throw;
                    }

                }).ContinueWith(t =>
                {

                    UpdateControls();

                    mainForm.Cursor = Cursors.Default;
                    this.Enabled = true;

                    var ex = t.Exception;
                    if (ex != null)
                    {
                        var iex = ex.InnerException;
                        MessageBox.Show(iex.Message);
                    }
                    else
                    {
                        if (videoSettings.Enabled)
                        {
                            var captureDescr = videoSettings?.CaptureDescription;
                            if (captureDescr.CaptureMode == CaptureMode.Screen)
                            {
                                var screenDescr = (ScreenCaptureDeviceDescription)captureDescr;

                                regionForm = new RegionForm(screenDescr.CaptureRegion);
                                regionForm.Visible = true;

                                statisticForm.Location = screenDescr.CaptureRegion.Location;
                                if (showStatistic)
                                {
                                    statisticForm.Start();
                                }
                            }
                        }
                    }

                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                MessageBox.Show(ex.Message);
            }
        }


        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...) ");

            mainForm.Cursor = Cursors.WaitCursor;
            this.Enabled = false;

            Task.Run(() =>
            {
                StopStreaming();

            }).ContinueWith(t =>
            {
                logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

                UpdateControls();

                mainForm.Cursor = Cursors.Default;
                this.Enabled = true;

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

                var ex = t.Exception;
                if (ex != null)
                {
                    var iex = ex.InnerException;
                    MessageBox.Show(iex.Message);
                }
                else
                {


                }

            }, TaskScheduler.FromCurrentSynchronizationContext());

        }


        private void UpdateSettings()
        {
            var sourceId = Guid.NewGuid().ToString();

            var communicationPort = mainForm.CommunicationPort;

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

            if (!isMulticastMode && transportMode == TransportMode.Udp)
            {
                throw new NotSupportedException("TransportMode.Udp currently not supported...");
            }
            videoSettings.NetworkParams.TransportMode = transportMode;
            audioSettings.NetworkParams.TransportMode = transportMode;


            var multicastAddr = multicastAddressTextBox.Text;
            var multicastVideoPort = 1234;
            var multicastAudioPort = 1235;

            if (isMulticastMode)
            {
                videoSettings.NetworkParams.RemoteAddr = multicastAddr;
                videoSettings.NetworkParams.RemotePort = multicastVideoPort;

                audioSettings.NetworkParams.RemoteAddr = multicastAddr;
                audioSettings.NetworkParams.RemotePort = multicastAudioPort;

            }
            else
            {
                if (transportMode == TransportMode.Tcp)
                {
                    videoSettings.NetworkParams.LocalAddr = networkIpAddr;
                    videoSettings.NetworkParams.LocalPort = 0;

                    audioSettings.NetworkParams.LocalAddr = networkIpAddr;
                    audioSettings.NetworkParams.LocalPort = 0;//1235;
                }
            }

            var screenCaptParams = (videoSettings.CaptureDescription as ScreenCaptureDeviceDescription);
            if (screenCaptParams != null)
            {
                if (screenCaptParams.DisplayRegion.IsEmpty)
                {
                    logger.Debug("VideoSource DisplayRegion.IsEmpty");
                    videoSettings.Enabled = false;
                }
            }

            if (string.IsNullOrEmpty(audioSettings.CaptureParams.DeviceId))
            {
                logger.Debug("Empty MMDeviceId...");
                audioSettings.Enabled = false;
            }


            // var communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";

            //var communicationAddress = "net.tcp://" + networkIpAddr +":"+ communicationPort + "/ScreenCaster/" + sourceId;
            // var communicationAddress = "http://" + "RAS-HOME10:8080"+ "/ScreenCaster/" + sourceId;
            // var communicationAddress = "net.tcp://" + "RAS-HOME10" + "/ScreenCaster/" + sourceId;

            communicationAddress = "net.tcp://" + networkIpAddr + "/ScreenCaster/";

            if (communicationPort > 0)
            {
                communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";
            }
        }

        private void SetupStreaming(string communicationAddress)
        {
            var videoEnabled = videoSettings.Enabled;
            var audioEnabled = audioSettings.Enabled;

            logger.Info("CommunicationAddress=" + communicationAddress +
                " MulticastMode=" + isMulticastMode +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled);

            if (videoEnabled)
            {
                SetupVideoSource(videoSettings);

                if (transportMode == TransportMode.Tcp || isMulticastMode)
                {
                    SetupVideoStream(videoSettings);
                }

                SetVideoChannelInfo(videoSettings);

            }

            if (audioEnabled)
            {
                SetupAudioSource(audioSettings);

                if (transportMode == TransportMode.Tcp || isMulticastMode)
                {
                    SetupAudioStream(audioSettings);
                }

                SetAudioChannelInfo(audioSettings);

            }

            communicationService = new ScreencastCommunicationService(this);
            var hostName = System.Net.Dns.GetHostName();


            hostName += " (" + videoSettings.CaptureDescription.Name + ")";
            communicationService.Open(communicationAddress, hostName);

        }

        private void StartStreaming()
        {
            var videoEnabled = videoSettings.Enabled;
            var audioEnabled = audioSettings.Enabled;

            if (videoSettings.Enabled)
            {
                videoSource.Start();
                videoStreamer.Start();
            }

            if (audioSettings.Enabled)
            {
                audioSource.Start();
                audioStreamer.Start();
            }

            isStreaming = true;
        }


        private void SetupVideoSource(VideoStreamSettings settings)
        {
            logger.Debug("SetupVideoSource(...)");

            try
            {
                var captureDescr = settings.CaptureDescription;
                captureDescr.Resolution = videoSettings.EncodingParams.Resolution;

                if (captureDescr.CaptureMode == CaptureMode.CaptDevice)
                {
                    videoSource = new VideoCaptureSource();
                    videoSource.Setup(captureDescr);
                }
                else if (captureDescr.CaptureMode == CaptureMode.Screen)
                {
                    videoSource = new ScreenSource();
                    videoSource.Setup(captureDescr);
                }

                videoSource.CaptureStarted += VideoSource_CaptureStarted;
                videoSource.CaptureStopped += VideoSource_CaptureStopped;

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                if (videoSource != null)
                {
                    videoSource.CaptureStarted -= VideoSource_CaptureStarted;
                    videoSource.CaptureStopped -= VideoSource_CaptureStopped;

                    videoSource.Close();
                    videoSource = null;
                }

                throw;
            }

        }



        private void SetupAudioSource(AudioStreamSettings settings)
        {
            logger.Debug("SetupAudioSource(...)");
            try
            {
                audioSource = new AudioSource();
                var deviceId = settings.CaptureParams.DeviceId;
                var eventSyncMode = true;
                var audioBufferMilliseconds = 50;
                var exclusiveMode = false;
                audioSource.Setup(deviceId, eventSyncMode, audioBufferMilliseconds, exclusiveMode);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (audioSource != null)
                {
                    audioSource.Close();
                    audioSource = null;
                }

                throw;
            }

        }


        private void SetupVideoStream(VideoStreamSettings settings)
        {
            logger.Debug("SetupVideoStream(...)");

            try
            {
                videoStreamer = new VideoStreamer(videoSource);

                videoStreamer.Setup(settings.EncodingParams, settings.NetworkParams);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (videoStreamer != null)
                {
                    videoStreamer.Close();
                    videoStreamer = null;
                }

                throw;
            }
        }

 
        private void SetupAudioStream(AudioStreamSettings settings)
        {
            logger.Debug("StartAudioStream(...)");

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            try
            {
                audioStreamer = new AudioStreamer(audioSource);
                audioStreamer.Setup(audioSettings.EncodingParams, audioSettings.NetworkParams);
                audioStreamer.StateChanged += AudioStreamer_StateChanged;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                if (audioStreamer != null)
                {
                    audioStreamer.Close();
                    audioStreamer.StateChanged -= AudioStreamer_StateChanged;
                    audioStreamer = null;
                }

                throw;
            }

        }

        private void SetAudioChannelInfo(AudioStreamSettings settings)
        {
            AudioChannelInfo audioInfo = new AudioChannelInfo
            {
                Id = settings.SessionId,
                AudioEncoder = settings.EncodingParams.Encoder,
                SampleRate = settings.EncodingParams.SampleRate,
                Channels = settings.EncodingParams.Channels,
                
            };


            var address = settings.NetworkParams.RemoteAddr;
            var port = settings.NetworkParams.RemotePort;
            var _transportMode = settings.NetworkParams.TransportMode;
            if (_transportMode == TransportMode.Tcp)
            {
                address = settings.NetworkParams.LocalAddr;
                port = settings.NetworkParams.LocalPort;
            }

            ScreencastChannelInfo audioChannelInfo = new ScreencastChannelInfo
            {
                Address = address,
                Port = port,
                IsMulticast = isMulticastMode,
                Transport = _transportMode,
                MediaInfo = audioInfo,
                SSRC = settings.NetworkParams.SSRC,
            };
            ScreencastChannelsInfos.Add(audioChannelInfo);
        }

        private void SetVideoChannelInfo(VideoStreamSettings settings)
        {
            var videoEncoderPars = settings.EncodingParams;

            VideoChannelInfo videoInfo = new VideoChannelInfo
            {
                Id = settings.SessionId,
                VideoEncoder = videoEncoderPars.Encoder,
                Resolution = videoEncoderPars.Resolution,
                Bitrate = videoEncoderPars.Bitrate,

                Fps = videoEncoderPars.FrameRate,
            };

            var address = settings.NetworkParams.RemoteAddr;
            var port = settings.NetworkParams.RemotePort;
            var _transportMode = settings.NetworkParams.TransportMode;
            if (_transportMode == TransportMode.Tcp)
            {
                address = settings.NetworkParams.LocalAddr;
                port = settings.NetworkParams.LocalPort;
            }

            ScreencastChannelInfo videoChannelInfo = new ScreencastChannelInfo
            {
                Address = address,//videoSettings.Address,
                Port = port, // videoSettings.Port,
                Transport = _transportMode,
                IsMulticast = isMulticastMode,
                MediaInfo = videoInfo,
                SSRC = settings.NetworkParams.SSRC,
            };

            ScreencastChannelsInfos.Add(videoChannelInfo);
        }

        private void VideoSource_CaptureStarted()
        {
            logger.Debug("VideoSource_CaptureStarted(...)");
            //...
        }

        private void VideoSource_CaptureStopped(object obj)
        {
            logger.Debug("VideoSource_CaptureStopped(...)");

            var errorCode = videoSource.ErrorCode;
            if (errorCode > 0)
            {
                //...
                logger.Error("VideoSource_CaptureStopped(...) " + errorCode);
            }

        }


        private void AudioStreamer_StateChanged()
        {

            syncContext.Send(_ =>
            {
                //UpdateAudioControls();
            }, null);

            if (audioStreamer.IsStreaming)
            {

            }
            else
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }
        }


        private void StopStreaming()
        {
            logger.Debug("ScreenStreamerControl::Stop()");

            StopVideoSource();

            StopVideoStream();

            StopAudioSource();

            StopAudioStream();


            ScreencastChannelsInfos.Clear();

            communicationService?.Close();

            isStreaming = false;

            //logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
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

            if (videoSource != null)
            {
                videoSource.Close();

            }
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
                //audioStreamer.SetWaveformPainter(null);
                audioStreamer.Close();
            }
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
                try
                {
                    SetupVideoSource(videoSettings);
                    //provider = new D3DImageProvider();

                    provider.Setup(videoSource);

                    previewForm = new PreviewForm
                    {
                        StartPosition = FormStartPosition.CenterParent,
                    };

                    previewForm.Link(provider);

                    // var pars = screenSource.CaptureParams;

                    var title = "";//"Src" + pars.SrcRect + "->Dst" + pars.DestSize + " Fps=" + pars.Fps + " Ratio=" + pars.AspectRatio;

                    previewForm.Text = title;

                    previewForm.Visible = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }

            }
        }

        private void screensUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateVideoSources();
        }


        private BindingList<ComboBoxItem> videoSourceItems = null;
        private void UpdateVideoSources()
        {
            var items = Screen.AllScreens
                .Select(s => new ComboBoxItem
                {
                    Name = s.DeviceName,//+ "" + s.Bounds.ToString(),
                    Tag = s.Bounds
                }
                ).ToList();

            items.Add(new ComboBoxItem
            {
                Name = "_AllScreen",
                Tag = SystemInformation.VirtualScreen
            });

            var captDevices = GetVideoCaptureDevices();
            if (captDevices.Count > 0)
            {
                var captItems = captDevices.Select(d => new ComboBoxItem
                {
                    Name = d.Name,
                    Tag = d,
                });

                items.AddRange(captItems);
            }


            videoSourceItems = new BindingList<ComboBoxItem>(items);
            videoSourcesComboBox.DisplayMember = "Name";
            videoSourcesComboBox.DataSource = videoSourceItems;
        }


        public List<VideoCaptureDeviceDescription> GetVideoCaptureDevices()
        {
            List<VideoCaptureDeviceDescription> deviceDescriptions = new List<VideoCaptureDeviceDescription>();

            Activate[] activates = null;
            try
            {
                using (var attributes = new MediaAttributes())
                {
                    MediaFactory.CreateAttributes(attributes, 1);
                    attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);

                    activates = MediaFactory.EnumDeviceSources(attributes);

                    foreach (var activate in activates)
                    {
                        try
                        {
                            var friendlyName = activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                            var symbolicLink = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);
                            var deviceDescription = new VideoCaptureDeviceDescription
                            {
                                Name = friendlyName,
                                DeviceId = symbolicLink,
                            };

                            try
                            {
                                using (var mediaSource = activate.ActivateObject<MediaSource>())
                                {
                                    using (var mediaType = MediaToolkit.MediaFoundation.MfTool.GetCurrentMediaType(mediaSource))
                                    {

                                        var frameSize = MediaToolkit.MediaFoundation.MfTool.GetFrameSize(mediaType);
                                        var frameRate = MediaToolkit.MediaFoundation.MfTool.GetFrameRate(mediaType);

                                        var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);
                                        var subtypeName = MediaToolkit.MediaFoundation.MfTool.GetMediaTypeName(subtype);

                                        var profile = new VideoCaptureDeviceProfile
                                        {
                                            FrameSize = frameSize,
                                            FrameRate = frameRate,
                                            Format = subtypeName,
                                        };

                                        deviceDescription.Resolution = frameSize;
                                        deviceDescription.CurrentProfile = profile;


                                    }
                                }

                                deviceDescriptions.Add(deviceDescription);

                            }
                            catch(Exception ex)
                            {
                                logger.Warn("Device not supported: " + friendlyName + " " + symbolicLink);
                            }

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                if (activates != null)
                {
                    foreach (var act in activates)
                    {
                        act.Dispose();
                    }
                }
            }

            return deviceDescriptions;

        }

        //private Size defaultEncoderResolution = new Size(1920, 1080);


        private void videoSourcesComboBox_SelectedValueChanged(object sender, EventArgs e)
        {

            Rectangle displayRect = Rectangle.Empty;

            VideoCaptureDescription captureParams = null;
            string captDeviceId = "";

            string displayName = "";
            var obj = videoSourcesComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    displayName = item.Name;

                    var tag = item.Tag;
                    if (tag != null)
                    {

                        if (tag is Rectangle)
                        {
                            displayRect = (Rectangle)tag;

                            screenCaptureDeviceDescr.DisplayRegion = displayRect;
                            screenCaptureDeviceDescr.CaptureRegion = displayRect;
                            screenCaptureDeviceDescr.DisplayName = displayName;

                            videoSettings.EncodingParams.Resolution = screenCaptureDeviceDescr.Resolution;

                            captureParams = screenCaptureDeviceDescr;

                        }
                        else if (tag is VideoCaptureDeviceDescription)
                        {
                            var captDevice = tag as VideoCaptureDeviceDescription;

                            captDeviceId = captDevice.DeviceId;
                            captureParams = captDevice;
                            videoSettings.EncodingParams.Resolution = captDevice.Resolution;
                        }
                    }
                }
            }


            //videoSettings.CaptureDescription.Name = displayName;
            videoSettings.CaptureDescription = captureParams;

            videoSettings.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

            UpdateControls();
        }

        public ScreencastChannelInfo[] GetScreencastInfo()
        {
            var vci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
            if (vci != null)
            {
                vci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            }

            var aci = ScreencastChannelsInfos.FirstOrDefault(i => i.MediaInfo is AudioChannelInfo);
            if (aci != null)
            {
                aci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            }

            return ScreencastChannelsInfos?.ToArray();
        }

        public void Play(ScreencastChannelInfo[] infos)
        {
            logger.Debug("ScreenStreamerControl::Play()");
            //...
        }

        public void Teardown()
        {
            logger.Debug("ScreenStreamerControl::Teardown()");
            //...


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

            videoSettings.NetworkParams.TransportMode = transportMode;

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
            transportComboBox.SelectedItem = transportMode;
        }

        private AudioCaptureSettings GetCurrentAudioCaptureSettings()
        {
            AudioCaptureSettings captureSettings = null;

            var item = audioSourcesComboBox.SelectedItem;
            if (item != null)
            {
                var tag = ((item as ComboBoxItem)?.Tag);
                if (tag != null)
                {
                    captureSettings = tag as AudioCaptureSettings;
                }
            }

            return captureSettings;
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

            //var dataSource = new BindingList<ComboBoxItem>(mmdevices.Select(d => new ComboBoxItem { Name = d.FriendlyName, Tag = d.ID }).ToList());
            var dataSource = new BindingList<ComboBoxItem>();
            foreach (var d in mmdevices)
            {
                //$"{bitsPerSample} bit PCM: {sampleRate / 1000}kHz {channels} channels"
                AudioCaptureSettings captureSettings = null;
                var client = d.AudioClient;
                if (client != null)
                {
                    var mixFormat = client.MixFormat;
                    if (mixFormat != null)
                    {
                        captureSettings = new AudioCaptureSettings
                        {
                            DeviceId = d.ID,
                            Name = d.FriendlyName,
                            BitsPerSample = mixFormat.BitsPerSample,
                            SampleRate = mixFormat.SampleRate,
                            Channels = mixFormat.Channels,
                            Description = $"{mixFormat.BitsPerSample} bit PCM: {mixFormat.SampleRate / 1000}kHz {mixFormat.Channels} channels",
                        };
                        
                    }
                }

                ComboBoxItem item = new ComboBoxItem
                {
                    Name = d.FriendlyName,
                    Tag = captureSettings,
                };

                dataSource.Add(item);
                d?.Dispose();
            }
            mmdevices.Clear();

            //dataSource.Add(new ComboBoxItem { Name = "Disabled", Tag = null, });

            audioSourcesComboBox.DataSource = dataSource;
            audioSourcesComboBox.DisplayMember = "Name";


        }


        private void audioSrcComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            //using (var audioDevice = GetCurrentAudioDevice())

            var captureSettings = GetCurrentAudioCaptureSettings();

            audioSettings.Enabled = (captureSettings != null);

            audioSettings.CaptureParams = captureSettings;

            UpdateControls();

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

            audioSettings.NetworkParams.TransportMode = transportMode;

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
            if (item != null)
            {
                this.transportMode = (TransportMode)transportComboBox.SelectedItem;
            }

        }


        private readonly UsbDeviceManager UsbManager = new UsbDeviceManager();

        protected override void WndProc(ref Message m)
        {
            if (this.Disposing)
            {
                base.WndProc(ref m);
                return;
            }

            switch ((uint)m.Msg)
            {
                case WM.DEVICECHANGE:
                    {
                        uint eventCode = (uint)m.WParam;

                        if (eventCode == DBT.DEVICEARRIVAL || eventCode == DBT.DEVICEREMOVECOMPLETE)
                        {
                            if (UsbDeviceManager.TryPtrToDeviceName(m.LParam, out string deviceName))
                            {   // получили информацию о подключенном устройстве в виде:
                                // \\?\USB#VID_0A89&PID_000C#6&2c24ce2e&0&4#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
                                if (eventCode == DBT.DEVICEARRIVAL)
                                {
                                    OnUsbDeviceArrival(deviceName);
                                }
                                else
                                {
                                    OnUsbDeviceMoveComplete(deviceName);
                                }
                            }
                            else
                            {//TODO:
                                //...

                            }
                        }

                        //logger.Debug("WM_DEVICECHANGE");
                        return;
                    }
            }

            base.WndProc(ref m);
        }

        private void OnUsbDeviceArrival(string deviceId)
        {
            logger.Debug("OnUsbDeviceArrival(...) " + deviceId);

            //TODO: Update devices list..
        }

        private void OnUsbDeviceMoveComplete(string deviceId)
        {
            logger.Debug("OnUsbDeviceMoveComplete(...) " + deviceId);

            var captureDescr = videoSettings.CaptureDescription;
            if (captureDescr != null)
            {
                if (captureDescr.CaptureMode == CaptureMode.CaptDevice)
                {
                    var videoDeviceDescr = (VideoCaptureDeviceDescription)captureDescr;
                    var _deviceId = videoDeviceDescr.DeviceId;
                    if (deviceId.Equals(_deviceId, StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.Warn("Capture device disconnected " + videoDeviceDescr.Name + " " + videoDeviceDescr.DeviceId);
                        //TODO: Close if capturing or update device list...

                        stopButton.PerformClick(); //!!!!!
                    }
                }
            }
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

    }

    public class AudioStreamSettings
    {
        public bool Enabled = false;
        public string SessionId = "";
        public NetworkSettings NetworkParams = null;
        public AudioEncoderSettings EncodingParams = null;
        public AudioCaptureSettings CaptureParams = null;
    }


    public class VideoStreamSettings
    {
        public bool Enabled = false;
        public string SessionId = "";
        public NetworkSettings NetworkParams = null;
        public VideoCaptureDescription CaptureDescription = null;
        public VideoEncoderSettings EncodingParams = null;

    }


}
