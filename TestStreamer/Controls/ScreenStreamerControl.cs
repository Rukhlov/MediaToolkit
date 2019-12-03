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
using SharpDX.MediaFoundation;

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
        private VideoStreamer videoStreamer = null;
        private IVideoSource videoSource = null;


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

                if(!isMulticastMode && transportMode == TransportMode.Udp)
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

                    //videoSettings.Address = multicastAddr;
                    //videoSettings.Port = multicastVideoPort;

                    audioSettings.NetworkParams.RemoteAddr = multicastAddr;
                    audioSettings.NetworkParams.RemotePort = multicastAudioPort;

                }
                else
                {
                    if (transportMode == TransportMode.Tcp)
                    {
                        videoSettings.NetworkParams.LocalAddr = networkIpAddr;
                        videoSettings.NetworkParams.LocalPort = 0;

                        //videoSettings.Address = networkIpAddr;
                        //videoSettings.Port = 0;//1234;

                        audioSettings.NetworkParams.LocalAddr = networkIpAddr;
                        audioSettings.NetworkParams.LocalPort = 0;//1235;
                    }
                }


                // var communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";

                //var communicationAddress = "net.tcp://" + networkIpAddr +":"+ communicationPort + "/ScreenCaster/" + sourceId;
                // var communicationAddress = "http://" + "RAS-HOME10:8080"+ "/ScreenCaster/" + sourceId;
                // var communicationAddress = "net.tcp://" + "RAS-HOME10" + "/ScreenCaster/" + sourceId;

                var communicationAddress = "net.tcp://" + networkIpAddr + "/ScreenCaster/";

                if (communicationPort > 0)
                {
                    communicationAddress = "net.tcp://" + networkIpAddr + ":" + communicationPort + "/ScreenCaster";
                }

                mainForm.Cursor = Cursors.WaitCursor;
                this.Enabled = false;

                Task.Run(() =>
                {
                    try
                    {
                        StartStreaming(communicationAddress);
                    }
                    catch(Exception ex)
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
                            if(videoSettings.CaptureDeviceId == "")
                            {
                                regionForm = new RegionForm(videoSettings.CaptureRegion);
                                regionForm.Visible = true;

                                statisticForm.Location = videoSettings.CaptureRegion.Location;//currentScreenRect.Location;
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

            mainForm.Cursor = Cursors.WaitCursor;
            this.Enabled = false;

            Task.Run(() =>
            {

                    StopStreaming();



            }).ContinueWith(t =>
            {

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


            //logger.Debug("stopButton_Click(...)");
            //try
            //{
            //    StopStreaming();

            //    UpdateControls();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex);

            //    MessageBox.Show(ex.Message);
            //}
        }

        private void StartStreaming(string communicationAddress)
        {
            var videoEnabled = videoSettings.Enabled;
            var audioEnabled = audioSettings.Enabled;

            logger.Info("CommunicationAddress=" + communicationAddress +
                " MulticastMode=" + isMulticastMode +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled);

            if (videoEnabled)
            {
                if (videoSettings.CaptureDeviceId == string.Empty)
                {
                    if (videoSettings.DisplayRegion.IsEmpty)
                    {
                        logger.Debug("VideoSource Disabled");
                        return;
                    }
                }


                StartVideoSource(videoSettings);

                if (isMulticastMode)
                {
                    StartVideoStream(videoSettings);
                }
                else
                {
                    if (transportMode == TransportMode.Tcp)
                    {
                        StartVideoStream(videoSettings);
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

                var address = videoSettings.NetworkParams.RemoteAddr;
                var port = videoSettings.NetworkParams.RemotePort;
                var _transportMode = videoSettings.NetworkParams.TransportMode;
                if (_transportMode == TransportMode.Tcp)
                {
                    address = videoSettings.NetworkParams.LocalAddr;
                    port = videoSettings.NetworkParams.LocalPort;
                }

                ScreencastChannelInfo videoChannelInfo = new ScreencastChannelInfo
                {
                    Address = address,//videoSettings.Address,
                    Port = port, // videoSettings.Port,
                    Transport = _transportMode,
                    IsMulticast = isMulticastMode,
                    MediaInfo = videoInfo,
                };

                ScreencastChannelsInfos.Add(videoChannelInfo);

            }

            if (audioEnabled)
            {
               // var currentMMDeviceId = currentAudioDevice?.ID ?? "";

                if (string.IsNullOrEmpty(audioSettings.CaptureParams.DeviceId))
                {
                    logger.Debug("Empty MMDeviceId...");
                    return;
                }

                StartAudioSource(audioSettings);

                if (isMulticastMode)
                {
                    StartAudioStream(audioSettings);
                }
                else
                {
                    if (transportMode == TransportMode.Tcp)
                    {
                        StartAudioStream(audioSettings);
                    }
                }

                AudioChannelInfo audioInfo = new AudioChannelInfo
                {
                    Id = videoSettings.SessionId,
                    AudioEncoder = audioSettings.EncodingParams.Encoder,
                    SampleRate = audioSettings.EncodingParams.SampleRate,
                    Channels = audioSettings.EncodingParams.Channels,
                };


                var address = audioSettings.NetworkParams.RemoteAddr;
                var port = audioSettings.NetworkParams.RemotePort;
                var _transportMode = audioSettings.NetworkParams.TransportMode;
                if (_transportMode == TransportMode.Tcp)
                {
                    address = audioSettings.NetworkParams.LocalAddr;
                    port = audioSettings.NetworkParams.LocalPort;
                }

                ScreencastChannelInfo audioChannelInfo = new ScreencastChannelInfo
                {
                    Address = address,
                    Port = port,
                    IsMulticast = isMulticastMode,
                    Transport = _transportMode,
                    MediaInfo = audioInfo,
                };
                ScreencastChannelsInfos.Add(audioChannelInfo);

            }

            communicationService = new ScreencastCommunicationService(this);
            var hostName = System.Net.Dns.GetHostName();

            
            hostName += " (" + videoSettings.DisplayName + ")";
            communicationService.Open(communicationAddress, hostName);

            isStreaming = true;
        }

        private bool StartVideoSource(VideoSettingsParams settings)
        {
            bool Result = false;
            try
            {
                if (videoSource != null)
                {
                    videoSource.StateChanged -= ScreenSource_StateChanged;
                    videoSource.Stop();
                    //videoSource.Close();
                    videoSource = null;
                }


                if (settings.CaptureDeviceId != string.Empty)
                {
                    videoSource = new VideoCaptureSource();

                    var captPars = new MfVideoCaptureParams
                    {
                        DestSize = settings.VideoResoulution,
                        DeviceId = settings.CaptureDeviceId,
                    };

                    videoSource.Setup(captPars);
                }
                else
                {
 
                    videoSource = new ScreenSource();

                    ScreenCaptureParams captureParams = new ScreenCaptureParams
                    {
                        SrcRect = settings.CaptureRegion,
                        DestSize = settings.VideoResoulution,
                        CaptureType = CaptureType.DXGIDeskDupl,
                        Fps = settings.Fps,
                        CaptureMouse = settings.CaptureMouse,
                        AspectRatio = settings.AspectRatio,

                    };

                    videoSource.Setup(captureParams);
                }


                videoSource.StateChanged += ScreenSource_StateChanged;


                videoSource.Start();         

                Result = true;
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                if (videoSource != null)
                {
                    videoSource.StateChanged -= ScreenSource_StateChanged;
                    videoSource.Close();
                    videoSource = null;
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


            //NetworkStreamingParams networkParams = new NetworkStreamingParams
            //{

            //    LocalAddr = localAddr,
            //    LocalPort = settings.Port,

            //    RemoteAddr = settings.Address,
            //    RemotePort = settings.Port,

            //    TransportMode = settings.TransportMode,
            //};

            var networkParams = settings.NetworkParams;
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

            videoStreamer = new VideoStreamer(videoSource);

            videoStreamer.Setup(encodingParams, networkParams);
            var rtpSender = videoStreamer.RtpSender;

            if(networkParams.TransportMode == TransportMode.Tcp)
            {
                var localEndpoint = rtpSender.LocalEndpoint;

                settings.NetworkParams.LocalPort = localEndpoint.Port;
                settings.NetworkParams.LocalAddr = localEndpoint.Address.ToString();
            }



            var streamerTask = videoStreamer.Start();
        }



        private void StartAudioStream(AudioSettingsParams settings )
        {
            logger.Debug("StartAudio()");

            audioSettings.EncodingParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",

            };

            var networkParams = settings.NetworkParams;

            //NetworkStreamingParams networkParams = new NetworkStreamingParams
            //{
            //    LocalPort = settings.Port,
            //    LocalAddr = "",
            //    RemoteAddr = settings.Address,
            //    RemotePort = settings.Port,
            //    TransportMode = settings.TransportMode,
            //};

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            // audioStreamer.SetWaveformPainter(new[] { this.waveformPainter1, this.waveformPainter2 });

            audioStreamer.StateChanged += AudioStreamer_StateChanged;

            audioStreamer.Setup(audioSettings.EncodingParams, networkParams);

            var rtpSender = audioStreamer.RtpSender;

            if (networkParams.TransportMode == TransportMode.Tcp)
            {
                var localEndpoint = rtpSender.LocalEndpoint;
                settings.NetworkParams.LocalPort= localEndpoint.Port;

                settings.NetworkParams.LocalAddr = localEndpoint.Address.ToString();
            }

            audioStreamer.Start();



        }

        private void StartAudioSource(AudioSettingsParams settings)
        {
            audioSource = new AudioSource();

            audioSource.Setup(settings.CaptureParams.DeviceId);

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

            logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
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
                videoSource.Stop();

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
                if (StartVideoSource(videoSettings))
                {
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
                    Tag = s.Bounds }
                ).ToList();

            items.Add(new ComboBoxItem
            {
                Name = "_AllScreen",
                Tag = SystemInformation.VirtualScreen
            });

            //screens.Add(new ComboBoxItem
            //{
            //    Name = "_Disabled",
            //    Tag = null
            //});

            var captDevices = GetVideoCaptureDevices();
            if (captDevices.Count > 0)
            {
                var captItems = captDevices.Select(d => new ComboBoxItem
                {
                    Name = d.Name,
                    Tag = d.SymLink,
                });

                items.AddRange(captItems);
            }


            videoSourceItems = new BindingList<ComboBoxItem>(items);
            videoSourcesComboBox.DisplayMember = "Name";
            videoSourcesComboBox.DataSource = videoSourceItems;
        }

        public class VideoCaptureDevice
        {
            public string Name = "";
            public string SymLink = "";
        }

        public List<VideoCaptureDevice> GetVideoCaptureDevices()
        {
            List<VideoCaptureDevice> devices = new List<VideoCaptureDevice>();

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

                            using (var mediaSource = activate.ActivateObject<MediaSource>())
                            {
                                using (var mediaType = GetCurrentMediaType(mediaSource))
                                {

                                    var frameSize = MediaToolkit.MediaFoundation.MfTool.GetFrameSize(mediaType);
                                    var frameRate = MediaToolkit.MediaFoundation.MfTool.GetFrameRate(mediaType);

                                    var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);

                                    var deviceDescription = friendlyName + " (" + frameSize.Width + "x" + frameSize.Height + " " + frameRate.ToString("0") + " fps)";

                                    devices.Add(new VideoCaptureDevice { Name = deviceDescription, SymLink = symbolicLink });
                                }

                            }


                            //using (var sourceReader = VideoCaptureSource.CreateSourceReader(activate))
                            //{
                            //    using (var mediaType = sourceReader.GetCurrentMediaType(SourceReaderIndex.FirstVideoStream))
                            //    {
                            //        var frameSize = MediaToolkit.MediaFoundation.MfTool.GetFrameSize(mediaType);
                            //        var frameRate = MediaToolkit.MediaFoundation.MfTool.GetFrameRate(mediaType);

                            //        var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);

                            //        var deviceDescription = friendlyName + " (" + frameSize.Width + "x" + frameSize.Height + " " + frameRate.ToString("0") + " fps)";

                            //        devices.Add(new VideoCaptureDevice { Name = deviceDescription, SymLink = symbolicLink });

                            //    }
                            //}       
                        }
                        catch(Exception ex)
                        {
                            logger.Error(ex);
                        }

                    }
                }

            }
            catch(Exception ex)
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

            return devices;

        }

        private static SharpDX.MediaFoundation.MediaType GetCurrentMediaType(MediaSource mediaSource)
        {
            SharpDX.MediaFoundation.MediaType mediaType = null;
            PresentationDescriptor presentationDescriptor = null;
            try
            {
                mediaSource.CreatePresentationDescriptor(out presentationDescriptor);

                for (int streamIndex = 0; streamIndex < presentationDescriptor.StreamDescriptorCount; streamIndex++)
                {
                    using (var steamDescriptor = presentationDescriptor.GetStreamDescriptorByIndex(streamIndex, out SharpDX.Mathematics.Interop.RawBool selected))
                    {
                        if (selected)
                        {
                            using (var mediaHandler = steamDescriptor.MediaTypeHandler)
                            {
                                 mediaType = mediaHandler.CurrentMediaType;

                            }
                        }
                    }
                }
            }
            finally
            {
                presentationDescriptor?.Dispose();
            }

            return mediaType;
        }

        private void videoSourcesComboBox_SelectedValueChanged_1(object sender, EventArgs e)
        {

            Rectangle displayRect = Rectangle.Empty;

            string captDeviceId = "";

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
                        if(tag is Rectangle)
                        {
                            displayRect = (Rectangle)tag;
                            displayName = item.Name;
                        }
                        else if(tag is string)
                        {
                            displayName = item.Name;
                            captDeviceId = item.Tag.ToString();
                        }
                    }
                }
            }


            videoSettings.DisplayName = displayName;
            videoSettings.DisplayRegion = displayRect;
            videoSettings.CaptureDeviceId = captDeviceId;

            videoSettings.CaptureRegion = displayRect;

            videoSettings.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

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
                vci.ClientsCount = videoStreamer?.ClientsCount ?? 0;
            }

            return ScreencastChannelsInfos?.ToArray();
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
                        //videoSettings.Address = clientInfo.Address;
                        //videoSettings.Port = clientInfo.Port;
                    }

                }
                //StartVideoStream(videoSettings);
            }

            if (audioSettings.Enabled)
            {
                if (transportMode == TransportMode.Udp)
                {
                    var clientInfo = infos.FirstOrDefault(i => i.MediaInfo is VideoChannelInfo);
                    if (clientInfo != null)
                    {
                        //videoSettings.Address = clientInfo.Address;
                        //videoSettings.Port = clientInfo.Port;
                    }
                    //videoSettings.Address = 
                }

                //StartAudioStream(audioSettings);
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

        private string currentAudioDeviceId = null;

        private MMDevice currentAudioDevice = null;
        private void audioSrcComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            currentAudioDevice = GetCurrentAudioDevice();
            audioSettings.Enabled = (currentAudioDevice != null);

            audioSettings.CaptureParams.DeviceId = currentAudioDevice?.ID ?? "";

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
            if (item!=null)
            {
                this.transportMode = (TransportMode)transportComboBox.SelectedItem;
            }
          
        }

        private void videoSourcesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

    public class AudioSettingsParams
    {
        public bool Enabled = true;

        public string SessionId = "";

        public NetworkStreamingParams NetworkParams = new NetworkStreamingParams();

        public AudioEncodingParams EncodingParams = new AudioEncodingParams();

        public AudioCaptureParams CaptureParams = new AudioCaptureParams();

        //public string Address = "";
        //public int Port = -1;
        //public TransportMode TransportMode = TransportMode.Udp;

        //public AudioEncoderMode Encoder = AudioEncoderMode.G711;

        //public int Samplerate = 8000;
        //public int Channels = 1;


        //public string AudioDeviceId = "";

        //public AudioSettingsParams Clone()
        //{
        //    return (AudioSettingsParams)this.MemberwiseClone();
        //}
    }

    public class CaptureParams { }

    public class VideoSettingsParams
    {
        public bool Enabled = true;
        public string SessionId = "";

        public NetworkStreamingParams NetworkParams = new NetworkStreamingParams();

        //public string Address = "";
        //public int Port = -1;
        //public TransportMode TransportMode = TransportMode.Udp;

        public string CaptureDeviceId = "";

        public Rectangle CaptureRegion = Rectangle.Empty;
        public string DisplayName = "";
        
        public Rectangle DisplayRegion = Rectangle.Empty;
        public bool CaptureMouse = true;
        public bool AspectRatio = true;

        public VideoEncodingParams EncodingParams = new VideoEncodingParams();

        public Size VideoResoulution = new Size(1920, 1080);
        
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
