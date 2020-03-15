using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.UI;
using NAudio.CoreAudioApi;
using NLog;
//using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Streamer.Controls;
using TestStreamer.Controls;

namespace TestStreamer
{
    public partial class StreamingForm : Form, IScreenStreamerServiceControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamingForm()
        {
            InitializeComponent();

            InitializeSettings();

            UpdateVideoSources();
            UpdateAudioSources();


            audioSourceEnableCheckBox.Checked = audioSettings.Enabled;
            videoSourceEnableCheckBox.Checked = videoSettings.Enabled;

            streamNameTextBox.Text = serverSettings.StreamName;

            captureStatusDescriptionLabel.Text = "";
            captureStatusLabel.Text = "Ready to stream";

            captureStatusDescriptionLabel.Text = "";

        }



        private SynchronizationContext syncContext = null;

        private bool isStreaming = false;

        private VideoStreamer videoStreamer = null;
        private IVideoSource videoSource = null;

        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;


        private VideoStreamSettings videoSettings = null;
        private VideoEncoderSettings videoEncoderSettings = null;


        private AudioStreamSettings audioSettings = null;
        private AudioEncoderSettings audioEncoderSettings = null;

        private ScreencastCommunicationService communicationService = null;
        private string communicationAddress = "";


        private ServerSettings serverSettings = null;

        public List<ScreencastChannelInfo> ScreencastChannelsInfos { get; private set; } = new List<ScreencastChannelInfo>();


        private StatisticForm statisticForm = new StatisticForm();

        private RegionForm regionForm = null;

        private SelectAreaForm selectAreaForm = null;


        private void InitializeSettings()
        {
            serverSettings = Config.Data.ServerSettings;

            videoSettings = Config.Data.VideoSettings;
            videoEncoderSettings = videoSettings.EncoderSettings;

            audioSettings = Config.Data.AudioSettings;
            audioEncoderSettings = audioSettings.EncoderSettings;
        }

        private void switchStreamingStateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...) ");

            if (!isStreaming)
            {
                Start();
            }
            else
            {
                Stop();
            }

        }

        private void Start()
        {
            try
            {
                UpdateSettings();

                //contextMenu.Enabled = false;

                networkSettingsLayoutPanel.Enabled = false;
                videoSourceSettingsLayoutPanel.Enabled = false;
                audioSourceSettingsLayoutPanel.Enabled = false;

                switchStreamingStateButton.Enabled = false;

                captureStatusLabel.Text = "Stream starting...";

                //this.Enabled = false;
                this.Cursor = Cursors.WaitCursor;
                

                Task.Run(() =>
                {
                    try
                    {
                        SetupStreaming(communicationAddress);

                        StartStreaming();
                    }
                    catch (Exception ex)
                    {
                        //logger.Error(ex);

                        StopStreaming();
                        throw;
                    }

                }).ContinueWith(t =>
                {

                    UpdateControls();

                    this.Cursor = Cursors.Default;
                    this.Enabled = true;

                    //contextMenu.Enabled = true;

                    var ex = t.Exception;
                    if (ex != null)
                    {
                        captureStatusLabel.Text = "Streaming attempt has failed";

                        var iex = ex.InnerException;
                        MessageBox.Show(iex.Message);

                    }
                    else
                    {
                        if (videoSettings.Enabled)
                        {
                            var captureDescr = videoSettings?.CaptureDevice;
                            if (captureDescr.CaptureMode == CaptureMode.Screen)
                            {
                                var screenDescr = (ScreenCaptureDevice)captureDescr;

                                if (screenDescr.Properties.ShowDebugBorder)
                                {
                                    regionForm = new RegionForm(screenDescr.CaptureRegion);
                                    regionForm.Visible = true;
                                }

                                if (screenDescr.Properties.ShowDebugInfo)
                                {
                                    statisticForm.Location = screenDescr.CaptureRegion.Location;
             
                                    statisticForm.Start();
                                    
                                }

                                if (selectAreaForm != null )
                                {
                                    selectAreaForm.Capturing = true;
                                }

                            }

                        }

                        captureStatusLabel.Text = "";
                        var statusDescription = "";
                        var _port = serverSettings.CommunicationPort;
                        if (serverSettings.CommunicationPort >= 0)
                        {
                            var listenUri = communicationService.ListenUri;
                            statusDescription = "Stream running on port " + listenUri.Port;
                        }

                       
                        //"Waiting for connection at " + _port + " port";
                        captureStatusDescriptionLabel.Text = statusDescription;

                    }

                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                MessageBox.Show(ex.Message);

                //contextMenu.Enabled = true;

                networkSettingsLayoutPanel.Enabled = true;
                videoSourceSettingsLayoutPanel.Enabled = true;
                audioSourceSettingsLayoutPanel.Enabled = true;

                switchStreamingStateButton.Enabled = true;

                captureStatusLabel.Text = "Streaming attempt has failed";

                captureStatusDescriptionLabel.Text = "";
            }
        }


        private void stopStreamingButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...) ");

            //try
            //{
            //    Process.Start(Path.Combine(CurrentDirectory, "Test.Client.exe"));
            //}
            //catch(Exception ex)
            //{
            //    logger.Error(ex);
            //}
            

            //logger.Info(MediaToolkit.MediaFoundation.MfTool.GetActiveObjectsReport());
            // Stop();
        }

        private void Stop()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                //contextMenu.Enabled = false;

                networkSettingsLayoutPanel.Enabled = false;
                videoSourceSettingsLayoutPanel.Enabled = false;
                audioSourceSettingsLayoutPanel.Enabled = false;

                switchStreamingStateButton.Enabled = false;

                captureStatusLabel.Text = "Stream stopping...";

                captureStatusDescriptionLabel.Text = "";

                Task.Run(() =>
                {
                    StopStreaming();

                }).ContinueWith(t =>
                {
                    //logger.Info(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

                    UpdateControls();
                    //contextMenu.Enabled = true;
                    switchStreamingStateButton.Enabled = true;

                    captureStatusLabel.Text = "Ready to stream";

                    captureStatusDescriptionLabel.Text = "";

                    this.Cursor = Cursors.Default;
                    this.Enabled = true;

                    if (statisticForm != null)
                    {
                        statisticForm.Stop();
                        statisticForm.Visible = false;
                    }

                    //if (previewForm != null && !previewForm.IsDisposed)
                    //{
                    //    previewForm.Close();
                    //    previewForm = null;
                    //}

                    if (regionForm != null)
                    {
                        regionForm.Close();
                        regionForm = null;
                    }


                    if (selectAreaForm != null)
                    {
                        selectAreaForm.Capturing = false;
                    }


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
            catch(Exception ex)
            {
                logger.Error(ex);
                MessageBox.Show(ex.ToString());

                //contextMenu.Enabled = true;
                networkSettingsLayoutPanel.Enabled = true;
                videoSourceSettingsLayoutPanel.Enabled = true;
                audioSourceSettingsLayoutPanel.Enabled = true;

                switchStreamingStateButton.Enabled = true;


                captureStatusLabel.Text = "Streaming attempt has failed";
                captureStatusDescriptionLabel.Text = "";
            }


        }


        
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private bool closing = false;
        private void DoClose()
        {
            closing = true;
            this.Close();
        }

        private void networkSettingsButton_Click(object sender, EventArgs e)
        {
            var f = new NetworkSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };

            f.Init(serverSettings);

            f.ShowDialog();


            streamNameTextBox.Text = serverSettings.StreamName;
        }

        private void streamNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var streamName = streamNameTextBox.Text;
            serverSettings.StreamName = streamName;

        }


        private void videoSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new VideoSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(videoSettings);

            f.ShowDialog();

            //videoSettings.NetworkSettings.TransportMode = transportMode;
        }


        private void audioSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(audioSettings);

            f.ShowDialog();

            //audioSettings.NetworkParams.TransportMode = transportMode;
        }




        private void UpdateSettings()
        {
            var sourceId = Guid.NewGuid().ToString();

            var communicationPort = serverSettings.CommunicationPort;

            var networkIpAddr = serverSettings.NetworkIpAddress; //"0.0.0.0";
            //var ipInfo = mainForm.GetCurrentIpAddrInfo();

            //if (ipInfo != null)
            //{
            //    networkIpAddr = ipInfo.Address.ToString();
            //}

            var transportMode = serverSettings.TransportMode; //TransportMode.Tcp;//(TransportMode)transportComboBox.SelectedItem;
            if (serverSettings.IsMulticast)
            {
                transportMode = TransportMode.Udp;
            }

            if (!serverSettings.IsMulticast && transportMode == TransportMode.Udp)
            {
                throw new NotSupportedException("TransportMode.Udp currently not supported...");
            }
            videoSettings.NetworkSettings.TransportMode = transportMode;
            audioSettings.NetworkParams.TransportMode = transportMode;


            var multicastAddr = serverSettings.MutlicastAddress;
            var multicastVideoPort = 1234;
            var multicastAudioPort = 1235;

            if (serverSettings.IsMulticast)
            {
                videoSettings.NetworkSettings.RemoteAddr = multicastAddr;
                videoSettings.NetworkSettings.RemotePort = multicastVideoPort;

                audioSettings.NetworkParams.RemoteAddr = multicastAddr;
                audioSettings.NetworkParams.RemotePort = multicastAudioPort;

            }
            else
            {
                if (transportMode == TransportMode.Tcp)
                {
                    videoSettings.NetworkSettings.LocalAddr = networkIpAddr;
                    videoSettings.NetworkSettings.LocalPort = 0;

                    audioSettings.NetworkParams.LocalAddr = networkIpAddr;
                    audioSettings.NetworkParams.LocalPort = 0;//1235;
                }
            }

            var screenCaptParams = (videoSettings.CaptureDevice as ScreenCaptureDevice);
            if (screenCaptParams != null)
            {
                if (screenCaptParams.DisplayRegion.IsEmpty)
                {
                    logger.Debug("VideoSource DisplayRegion.IsEmpty");
                    //videoSettings.Enabled = false;
                }
            }

            if (string.IsNullOrEmpty(audioSettings.CaptureDevice.DeviceId))
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
            var transportMode = serverSettings.TransportMode;

            logger.Info("CommunicationAddress=" + communicationAddress +
                " MulticastMode=" + serverSettings.IsMulticast +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled);

            if (videoEnabled)
            {
                var captureDescription = videoSettings.CaptureDevice;
              
                var resolution = captureDescription.Resolution;
                int w = resolution.Width;
                if (w % 2 != 0)
                {
                    w--;
                }

                int h = resolution.Height;
                if (h % 2 != 0)
                {
                    h--;
                }
                captureDescription.Resolution = new Size(w, h);

               var encodingParams = videoSettings.EncoderSettings;
                if (videoSettings.UseEncoderResoulutionFromSource)
                {
                    encodingParams.Width = videoSettings.CaptureDevice.Resolution.Width;
                    encodingParams.Height = videoSettings.CaptureDevice.Resolution.Height;
                }

                SetupVideoSource(videoSettings);
                
                if (transportMode == TransportMode.Tcp || serverSettings.IsMulticast)
                {
                    SetupVideoStream(videoSettings);
                }

                SetVideoChannelInfo(videoSettings);

            }

            if (audioEnabled)
            {
                SetupAudioSource(audioSettings);

                if (transportMode == TransportMode.Tcp || serverSettings.IsMulticast)
                {
                    SetupAudioStream(audioSettings);
                }

                SetAudioChannelInfo(audioSettings);

            }

            communicationService = new ScreencastCommunicationService(this);
            var hostName = serverSettings.StreamName; //System.Net.Dns.GetHostName();


            hostName += " (" + videoSettings.CaptureDevice.Name + ")";
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
                var captureDescr = settings.CaptureDevice;

                if (!videoSettings.UseEncoderResoulutionFromSource)
                {
                    captureDescr.Resolution = videoSettings.EncoderSettings.Resolution;
                }
                else
                {
                    captureDescr.Resolution = Size.Empty;
                }

                

                if (captureDescr.CaptureMode == CaptureMode.UvcDevice)
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
                var deviceId = settings.CaptureDevice.DeviceId;
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

                videoStreamer.Setup(settings.EncoderSettings, settings.NetworkSettings);

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
                audioStreamer.Setup(audioSettings.EncoderSettings, audioSettings.NetworkParams);
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
                AudioEncoder = settings.EncoderSettings.Encoder,
                SampleRate = settings.EncoderSettings.SampleRate,
                Channels = settings.EncoderSettings.Channels,

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
                IsMulticast = serverSettings.IsMulticast,
                Transport = _transportMode,
                MediaInfo = audioInfo,
                SSRC = settings.NetworkParams.SSRC,
            };
            ScreencastChannelsInfos.Add(audioChannelInfo);
        }

        private void SetVideoChannelInfo(VideoStreamSettings settings)
        {
            var videoEncoderPars = settings.EncoderSettings;

            VideoChannelInfo videoInfo = new VideoChannelInfo
            {
                Id = settings.SessionId,
                VideoEncoder = videoEncoderPars.Encoder,
                Resolution = videoEncoderPars.Resolution,
                Bitrate = videoEncoderPars.Bitrate,

                Fps = videoEncoderPars.FrameRate,
            };

            var address = settings.NetworkSettings.RemoteAddr;
            var port = settings.NetworkSettings.RemotePort;
            var _transportMode = settings.NetworkSettings.TransportMode;
            if (_transportMode == TransportMode.Tcp)
            {
                address = settings.NetworkSettings.LocalAddr;
                port = settings.NetworkSettings.LocalPort;
            }

            ScreencastChannelInfo videoChannelInfo = new ScreencastChannelInfo
            {
                Address = address,//videoSettings.Address,
                Port = port, // videoSettings.Port,
                Transport = _transportMode,
                IsMulticast = serverSettings.IsMulticast,
                MediaInfo = videoInfo,
                SSRC = settings.NetworkSettings.SSRC,
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



        private BindingList<ComboBoxItem> videoSourceItems = null;
        private void UpdateVideoSources()
        {

            List<ComboBoxItem> items = new List<ComboBoxItem>();

            var captureProperties = Config.Data.VideoSettings.ScreenCaptureProperties;

            //ScreenCaptureProperties captureProperties = new ScreenCaptureProperties
            //{
            //    CaptureMouse = true,
            //    AspectRatio = true,
            //    CaptureType = VideoCaptureType.DXGIDeskDupl,
            //    UseHardware = true,
            //    Fps = 30,
            //    ShowDebugInfo = false,
            //};


            foreach (var screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;

                ScreenCaptureDevice descr = new ScreenCaptureDevice
                {
                    CaptureRegion = bounds,
                    DisplayRegion = bounds,
                    Name = screen.DeviceName,

                    Resolution = bounds.Size,
                    Properties = captureProperties,

                };

                items.Add(new ComboBoxItem
                {
                    Name = screen.DeviceName,//+ "" + s.Bounds.ToString(),
                    Tag = descr,
                });
            }

            //var items = Screen.AllScreens
            //    .Select(s => new ComboBoxItem
            //    {
            //        Name = s.DeviceName,//+ "" + s.Bounds.ToString(),
            //        Tag = s.Bounds
            //    }
            //    ).ToList();

            if (items.Count > 1)
            {
                ScreenCaptureDevice descr = new ScreenCaptureDevice
                {
                    DisplayRegion = SystemInformation.VirtualScreen,
                    CaptureRegion = SystemInformation.VirtualScreen,
                    Resolution = SystemInformation.VirtualScreen.Size,
                    Properties = captureProperties,
                    Name = "All Screens",
                };

                items.Add(new ComboBoxItem
                {
                    Name = "All Screens",//+ "" + s.Bounds.ToString(),
                    Tag = descr,
                });

                //items.Add(new ComboBoxItem
                //{
                //    Name = "All Screens",
                //    Tag = SystemInformation.VirtualScreen
                //});

            }

            var customRegion = videoSettings.CustomRegion;
            ScreenCaptureDevice customRegionDescr = new ScreenCaptureDevice
            {
                CaptureRegion = customRegion,
                DisplayRegion = Rectangle.Empty,

                Resolution = customRegion.Size,
                Properties = captureProperties,
                Name = "Screen Region",

            };

            items.Add(new ComboBoxItem
            {
                Name = customRegionDescr.Name,
                Tag = customRegionDescr,
            });

            var captDevices = MediaToolkit.MediaFoundation.MfTool.GetVideoCaptureDevices();
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
            videoSourceComboBox.DisplayMember = "Name";
            videoSourceComboBox.DataSource = videoSourceItems;
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
                AudioCaptureDeviceDescription captureSettings = null;
                var client = d.AudioClient;
                if (client != null)
                {
                    var mixFormat = client.MixFormat;
                    if (mixFormat != null)
                    {

                        captureSettings = new AudioCaptureDeviceDescription
                        {
                            DeviceId = d.ID,
                            Name = d.FriendlyName,

                            BitsPerSample = mixFormat.BitsPerSample,
                            SampleRate = mixFormat.SampleRate,
                            Channels = mixFormat.Channels,
                            Description = $"{mixFormat.BitsPerSample} bit PCM: {mixFormat.SampleRate / 1000}kHz {mixFormat.Channels} channels",

                            //Properties = prop,
                        };

                    }
                    //Screen.PrimaryScreen.
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

            audioSourceComboBox.DataSource = dataSource;
            audioSourceComboBox.DisplayMember = "Name";


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

        private void videoSourceUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateVideoSources();
        }

        private void audioSourceUpdateButton_Click(object sender, EventArgs e)
        {
            UpdateAudioSources();
        }

 
        private void videoSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            bool isCustomRegion = false;

            Rectangle displayRect = Rectangle.Empty;

            VideoCaptureDevice captureParams = null;
            string captDeviceId = "";

            string displayName = "";
            var obj = videoSourceComboBox.SelectedItem;
            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    displayName = item.Name;

                    var tag = item.Tag;
                    if (tag != null)
                    {
                        if (tag is ScreenCaptureDevice)
                        {
                            var screenDescr = tag as ScreenCaptureDevice;
                            isCustomRegion = screenDescr.DisplayRegion.IsEmpty;

                            captureParams = screenDescr;


                        }
                        else if (tag is UvcDevice)
                        {
                            var captDevice = tag as UvcDevice;

                            captDeviceId = captDevice.DeviceId;
                            captureParams = captDevice;
                            //videoSettings.EncodingParams.Resolution = captDevice.Resolution;
                        }  
                    }
                    else
                    {
                    }
                }


                if (isCustomRegion)
                {
                    if (selectAreaForm == null)
                    {
                        var customRegion = videoSettings.CustomRegion;
                        selectAreaForm = new SelectAreaForm
                        {
                            StartPosition = FormStartPosition.Manual,
                            Location = customRegion.Location,
                            Size = customRegion.Size,
                        };

                        selectAreaForm.AreaChanged += SelectAreaForm_AreaChanged;
                       
                    }

                    selectAreaForm.Tag = captureParams;


                    selectAreaForm.Visible = true;
                }
                else
                {
                    if (selectAreaForm != null)
                    {
                        selectAreaForm.Visible = false;
                    }
                }

            }


            //videoSettings.CaptureDescription.Name = displayName;
            videoSettings.CaptureDevice = captureParams;

            videoSourceDetailsButton.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

            UpdateControls();
        }


        private void SelectAreaForm_AreaChanged(Rectangle rect)
        {
            var tag = selectAreaForm.Tag;
            if(tag != null)
            {
                var descr = (ScreenCaptureDevice)tag;
                var selectedArea = selectAreaForm.SelectedArea;

                descr.DisplayRegion = Rectangle.Empty;
                descr.CaptureRegion = selectedArea;
                descr.Resolution = selectedArea.Size;

                videoSettings.CustomRegion = new Rectangle(selectAreaForm.Location,selectAreaForm.Size);
            }
        }

        private void audioSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var captureSettings = GetCurrentAudioCaptureSettings();
            if(captureSettings == null)
            {
                audioSettings.Enabled = false;
            }

            //audioSettings.Enabled = (captureSettings != null);

            audioSettings.CaptureDevice = captureSettings;

            UpdateControls();
        }


        private void audioEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            audioSettings.Enabled = audioSourceEnableCheckBox.Checked;

            UpdateControls();
        }

        private void videoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            videoSettings.Enabled = videoSourceEnableCheckBox.Checked;
            UpdateControls();
        }

        private AudioCaptureDeviceDescription GetCurrentAudioCaptureSettings()
        {
            AudioCaptureDeviceDescription captureSettings = null;

            var item = audioSourceComboBox.SelectedItem;
            if (item != null)
            {
                var tag = ((item as ComboBoxItem)?.Tag);
                if (tag != null)
                {
                    captureSettings = tag as AudioCaptureDeviceDescription;
                }
            }

            return captureSettings;
        }


        private void UpdateControls()
        {

            //this.startStreamingButton.Enabled = !isStreaming;

            //this.stopStreamingButton.Enabled = isStreaming;

            if (isStreaming)
            {
               // startToolStripMenuItem.Text = "Stop Streaming";
                switchStreamingStateButton.Text = "Stop Streaming";
            }
            else
            {
                //startToolStripMenuItem.Text = "Start Streaming";
                switchStreamingStateButton.Text = "Start Streaming";
            }

            

            networkSettingsLayoutPanel.Enabled = !isStreaming;
            videoSourceSettingsLayoutPanel.Enabled = !isStreaming;
            audioSourceSettingsLayoutPanel.Enabled = !isStreaming;

            switchStreamingStateButton.Enabled = true;


        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000;      // WS_EX_COMPOSITED
                return handleParam;
            }
        }


        //private void settingToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    this.Visible = true;
        //}

        //private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        this.allowshowdisplay = true;

        //        this.Visible = !this.Visible;
        //    }
        //}


        //private bool allowshowdisplay = true;

        //protected override void SetVisibleCore(bool value)
        //{
        //    base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        //}

        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    if (!closing)
        //    {
        //        e.Cancel = true;
        //        this.Visible = false;
        //    }
        //    base.OnClosing(e);
        //}

        //protected override void OnClosed(EventArgs e)
        //{

        //    base.OnClosed(e);
        //}



    }

    public class CustomComboBox : ComboBox
    {
        private const int WM_PAINT = 0xF;
        private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    // Uncomment this if you don't want the "highlight border".
                    /*
                    using (var p = new Pen(this.BorderColor, 1))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                    }*/
                    using (var p = new Pen(this.BorderColor, 2))
                    {
                        g.DrawRectangle(p, 2, 2, Width - buttonWidth - 4, Height - 4);
                    }


                    //using (var g = Graphics.FromHwnd(Handle))
                    //{
                    //    using (var p = new Pen(this.BorderColor, 1))
                    //    {
                    //        g.DrawRectangle(p, 0, 0, Width - buttonWidth - 1, Height - 1);
                    //    }

                    //    if (Properties.Settings.Default.Theme == "Dark")
                    //    {
                    //        g.DrawImageUnscaled(Properties.Resources.dropdown, new Point(Width - buttonWidth - 1));
                    //    }
                    //}
                }
            }
        }

        public CustomComboBox()
        {
            BorderColor = Color.DimGray;
        }

        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "DimGray")]
        public Color BorderColor { get; set; }
    }

    public class FlatCombo : ComboBox
    {
        private const int WM_PAINT = 0xF;
        private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT)
            {
                using (var g = Graphics.FromHwnd(Handle))
                {
                    using (var p = new Pen(this.ForeColor))
                    {
                        g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                        g.DrawLine(p, Width - buttonWidth, 0, Width - buttonWidth, Height);
                    }
                }
            }
        }
    }

    public interface IScreenStreamerServiceControl
    {
        ScreencastChannelInfo[] GetScreencastInfo();
    }

    public class ColoredCombo : ComboBox
    {
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
                e.Graphics.DrawRectangle(Pens.Red, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }
    }
}
