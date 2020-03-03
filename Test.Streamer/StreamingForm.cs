using MediaToolkit;
using MediaToolkit.Core;
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

            InitMediaSettings();

            UpdateVideoSources();
            UpdateAudioSources();

            audioSourceEnableCheckBox.Checked = audioSettings.Enabled;
            videoSourceEnableCheckBox.Checked = videoSettings.Enabled;

            streamNameTextBox.Text = serverSettings.StreamName;

            captureStatusDescriptionLabel.Text = "";
            captureStatusLabel.Text = captureStatus;

            captureStatusDescriptionLabel.Text = "";
            
            // base.OnPaintBackground(e);
            //using (var brush = new SolidBrush(BackColor))
            //{
            //    e.Graphics.FillRectangle(brush, ClientRectangle);
            //    e.Graphics.DrawRectangle(Pens.DarkGray, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            //}
            //groupBox4.Paint += (o, a) => 
            //{
            //    var rect = a.ClipRectangle;
            //    a.Graphics.Clear(Color.FromArgb(128, 34, 32, 63));

            //    a.Graphics.DrawLine(new Pen(new SolidBrush(Color.Blue)), new Point(rect.X, rect.Bottom), new Point(rect.Width, rect.Bottom));
            //};

            //updateNetworksButton.Text = "\u2630";
        }


        private SynchronizationContext syncContext = null;

        private bool isStreaming = false;
        private VideoStreamer videoStreamer = null;
        private IVideoSource videoSource = null;


        private StatisticForm statisticForm = new StatisticForm();
        //private PreviewForm previewForm = null;
        private RegionForm regionForm = null;

        private SelectAreaForm selectAreaForm = null;


        private AudioSource audioSource = null;
        private AudioStreamer audioStreamer = null;

        private TransportMode transportMode = TransportMode.Tcp;

        private VideoStreamSettings videoSettings = null;
        private VideoEncoderSettings videoEncoderSettings = null;
        private ScreenCaptureDeviceDescription screenCaptureDeviceDescr = null;


        private AudioStreamSettings audioSettings = null;
        private AudioEncoderSettings audioEncoderSettings = null;

        private ScreencastCommunicationService communicationService = null;
        private string communicationAddress = "";

        //public int CommunicationPort = -1;
        //private bool isMulticastMode = false;

        private ServerSettings serverSettings = null;

        public List<ScreencastChannelInfo> ScreencastChannelsInfos { get; private set; } = new List<ScreencastChannelInfo>();

        private bool showStatistic = true;


        private string captureStatus = "Ready to stream";

        private void InitMediaSettings()
        {
            var hostName = System.Net.Dns.GetHostName();

            //FIXME: порт может изменится!
            int port = -1;

            var freeTcpPorts = NetUtils.GetFreePortRange(ProtocolType.Tcp, 1, 808);
            if (freeTcpPorts != null && freeTcpPorts.Count() > 0)
            {
                port = freeTcpPorts.FirstOrDefault();
            }

            serverSettings = new ServerSettings
            {
                StreamName = hostName,
                NetworkIpAddress = "0.0.0.0",
                MutlicastAddress = "239.0.0.1",
                CommunicationPort = port,
                IsMulticast = false,
                TransportMode = TransportMode.Tcp,
                
            };

            screenCaptureDeviceDescr = new ScreenCaptureDeviceDescription
            {
                //Resolution = new Size(1920, 1080),
                CaptureMouse = true,
                AspectRatio = true,
                CaptureType = VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
                ShowDebugInfo = false,
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
                Enabled = false,
                SessionId = "audio_" + Guid.NewGuid().ToString(),
                NetworkParams = new NetworkSettings(),
                CaptureParams = new AudioCaptureSettings(),
                EncodingParams = audioEncoderSettings,
            };

        }

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
                captureStatus = "Stream starting...";
                captureStatusLabel.Text = captureStatus;

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
                        captureStatus = "Streaming attempt has failed";//"Capture Stopped";
                        captureStatusLabel.Text = captureStatus;

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

                                if (screenDescr.ShowCaptureBorder)
                                {
                                    regionForm = new RegionForm(screenDescr.CaptureRegion);
                                    regionForm.Visible = true;
                                }

                                if (screenDescr.ShowDebugInfo)
                                {
                                    statisticForm.Location = screenDescr.CaptureRegion.Location;
                                    if (showStatistic)
                                    {
                                        statisticForm.Start();
                                    }
                                }

                                if (selectAreaForm != null )
                                {
                                    selectAreaForm.Capturing = true;
                                }

                            }

                        }


                        captureStatus = "";//"Capturing...";

                        captureStatusLabel.Text = captureStatus;
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

                captureStatus = "Streaming attempt has failed";//Stopped";
                captureStatusLabel.Text = captureStatus;

                captureStatusDescriptionLabel.Text = "";
            }
        }

        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;

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

                captureStatus = "Stream stopping...";
                captureStatusLabel.Text = captureStatus;

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
                    captureStatus = "Ready to stream";//"Capture Stopped";

                    captureStatusLabel.Text = captureStatus;

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

                captureStatus = "Streaming attempt has failed";//"Capture Stopped";

                captureStatusLabel.Text = captureStatus;
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

        private void videoSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new VideoSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(videoSettings);

            f.ShowDialog();

            videoSettings.NetworkParams.TransportMode = transportMode;
        }


        private void audioSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(audioSettings);

            f.ShowDialog();

            audioSettings.NetworkParams.TransportMode = transportMode;
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

            transportMode = TransportMode.Tcp;//(TransportMode)transportComboBox.SelectedItem;
            if (serverSettings.IsMulticast)
            {
                transportMode = TransportMode.Udp;
            }

            if (!serverSettings.IsMulticast && transportMode == TransportMode.Udp)
            {
                throw new NotSupportedException("TransportMode.Udp currently not supported...");
            }
            videoSettings.NetworkParams.TransportMode = transportMode;
            audioSettings.NetworkParams.TransportMode = transportMode;


            var multicastAddr = serverSettings.MutlicastAddress;
            var multicastVideoPort = 1234;
            var multicastAudioPort = 1235;

            if (serverSettings.IsMulticast)
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
                " MulticastMode=" + serverSettings.IsMulticast +
                " VideoEnabled=" + videoEnabled +
                " AudioEnabled=" + audioEnabled);

            if (videoEnabled)
            {
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
                if (!videoSettings.EncodingParams.UseResoulutionFromSource)
                {
                    captureDescr.Resolution = videoSettings.EncodingParams.Resolution;
                }
                else
                {
                    captureDescr.Resolution = Size.Empty;
                }

                

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
                IsMulticast = serverSettings.IsMulticast,
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
                IsMulticast = serverSettings.IsMulticast,
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



        private BindingList<ComboBoxItem> videoSourceItems = null;
        private void UpdateVideoSources()
        {

            List<ComboBoxItem> items = new List<ComboBoxItem>();

            foreach(var screen in Screen.AllScreens)
            {
                var bounds = screen.Bounds;

                ScreenCaptureDeviceDescription descr = new ScreenCaptureDeviceDescription
                {
                    CaptureRegion = bounds,
                    DisplayRegion = bounds,
                    DisplayName = screen.DeviceName,

                    Resolution = bounds.Size,
                    CaptureMouse = true,
                    AspectRatio = true,
                    CaptureType = VideoCaptureType.DXGIDeskDupl,
                    UseHardware = true,
                    Fps = 30,
                    ShowDebugInfo = false,
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
                ScreenCaptureDeviceDescription descr = new ScreenCaptureDeviceDescription
                {
                    CaptureRegion = SystemInformation.VirtualScreen,
                    Resolution = SystemInformation.VirtualScreen.Size,
                    CaptureMouse = true,
                    AspectRatio = true,
                    CaptureType = VideoCaptureType.DXGIDeskDupl,
                    UseHardware = true,
                    Fps = 30,
                    ShowDebugInfo = false,
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


            ScreenCaptureDeviceDescription regionDescr = new ScreenCaptureDeviceDescription
            {
                CaptureRegion = new Rectangle(0, 0, 100, 100),
                DisplayRegion = new Rectangle(0, 0, 100, 100),

                Resolution = new Size(100, 100),
                CaptureMouse = true,
                AspectRatio = true,
                CaptureType = VideoCaptureType.DXGIDeskDupl,
                UseHardware = true,
                Fps = 30,
                ShowDebugInfo = false,

                CustomRegion = true,
            };

            items.Add(new ComboBoxItem
            {
                Name = "Screen Region",
                Tag = regionDescr,
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

            VideoCaptureDescription captureParams = null;
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
                        if (tag is ScreenCaptureDeviceDescription)
                        {
                            //captDeviceId = captDevice.DeviceId;
                            //captureParams = captDevice;
                            //videoSettings.EncodingParams.Resolution = captDevice.Resolution;

                            var screenDescr = tag as ScreenCaptureDeviceDescription;
                            isCustomRegion = screenDescr.CustomRegion;
                            displayRect = screenDescr.DisplayRegion;

                            captureParams = screenDescr;


                        }
                        else if (tag is VideoCaptureDeviceDescription)
                        {
                            var captDevice = tag as VideoCaptureDeviceDescription;

                            captDeviceId = captDevice.DeviceId;
                            captureParams = captDevice;
                            videoSettings.EncodingParams.Resolution = captDevice.Resolution;
                        }
                        //else if (tag is Rectangle)
                        //{
                        //    displayRect = (Rectangle)tag;

                        //    screenCaptureDeviceDescr.DisplayRegion = displayRect;
                        //    screenCaptureDeviceDescr.CaptureRegion = displayRect;
                        //    screenCaptureDeviceDescr.DisplayName = displayName;

                        //    //videoSettings.EncodingParams.Resolution = screenCaptureDeviceDescr.Resolution;

                        //    captureParams = screenCaptureDeviceDescr;

                        //}
  
                    }
                    else
                    {
                    }
                }


                if (isCustomRegion)
                {
                    if (selectAreaForm == null)
                    {
                        selectAreaForm = new SelectAreaForm();

                    }

                    var location = selectAreaForm.Location;
                    var size = selectAreaForm.ClientSize;
                    var rect = new Rectangle(location, size);

                    selectAreaForm.CaptureDeviceDescription = (ScreenCaptureDeviceDescription)captureParams;
                    selectAreaForm.UpdateDeviceDescr(rect);

                    //selectAreaForm.StartPosition = FormStartPosition.Manual;
                    //selectAreaForm.Location = displayRect.Location;
                    //selectAreaForm.Size = displayRect.Size;

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
            videoSettings.CaptureDescription = captureParams;

            videoSourceDetailsButton.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

            UpdateControls();
        }

        private void audioSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var captureSettings = GetCurrentAudioCaptureSettings();
            if(captureSettings == null)
            {
                audioSettings.Enabled = false;
            }

            //audioSettings.Enabled = (captureSettings != null);

            audioSettings.CaptureParams = captureSettings;

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

        private AudioCaptureSettings GetCurrentAudioCaptureSettings()
        {
            AudioCaptureSettings captureSettings = null;

            var item = audioSourceComboBox.SelectedItem;
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

            /*
            var videoEnabled = videoSettings.Enabled;
            videoSourceDetailsButton.Enabled = videoEnabled;
            videoSourceComboBox.Enabled = videoEnabled;

            videoSourceUpdateButton.Enabled = videoEnabled;

            var audioEnabled = audioSettings.Enabled;
            audioSourceDetailsButton.Enabled = audioEnabled;
            audioSourceComboBox.Enabled = audioEnabled;
            audioSourceUpdateButton.Enabled = audioEnabled;


            videoSourceComboBox.Enabled = !isStreaming;
            audioSourceComboBox.Enabled = !isStreaming;

            audioSourceDetailsButton.Enabled = !isStreaming;
            videoSourceDetailsButton.Enabled = !isStreaming;

            videoSourceEnableCheckBox.Enabled = !isStreaming;
            audioSourceEnableCheckBox.Enabled = !isStreaming;

            videoSourceUpdateButton.Enabled = !isStreaming;
            audioSourceUpdateButton.Enabled = !isStreaming;
            */




            //networkPanel.Enabled = !isStreaming;

            //videoPreviewButton.Enabled = true;
            //audioPreviewButton.Enabled = true;

            //this.fpsNumeric2.Enabled = !ServiceHostOpened;
            //this.inputSimulatorCheckBox2.Enabled = !ServiceHostOpened;
            //this.screensComboBox2.Enabled = !ServiceHostOpened;
            //this.screensUpdateButton2.Enabled = !ServiceHostOpened;

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
