using MediaToolkit;
using MediaToolkit.Core;
using NAudio.CoreAudioApi;
using NLog;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Test.Streamer.Controls;
using TestStreamer.Controls;

namespace TestStreamer
{
    public partial class StreamingForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamingForm()
        {
            InitializeComponent();

            InitMediaSettings();

            UpdateVideoSources();
            UpdateAudioSources();

        

            //updateNetworksButton.Text = "\u2630";
        }


        private SynchronizationContext syncContext = null;

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

        private void startButton_Click(object sender, EventArgs e)
        {

        }

        private void stopStreamingButton_Click(object sender, EventArgs e)
        {

        }

        private void exitButton_Click(object sender, EventArgs e)
        {

        }

        private void networkSettingsButton_Click(object sender, EventArgs e)
        {
            var f = new NetworkSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            //f.Setup(videoSettings);

            f.ShowDialog();
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
            videoSourceComboBox.DisplayMember = "Name";
            videoSourceComboBox.DataSource = videoSourceItems;
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

            videoSourceDetailsButton.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

            UpdateControls();
        }

        private void audioSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var captureSettings = GetCurrentAudioCaptureSettings();

            audioSettings.Enabled = (captureSettings != null);

            audioSettings.CaptureParams = captureSettings;

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
            //this.settingPanel.Enabled = !isStreaming;

            this.startStreamingButton.Enabled = !isStreaming;
            //this.videoPreviewButton.Enabled = isStreaming;

            this.stopStreamingButton.Enabled = isStreaming;

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
            //networkPanel.Enabled = !isStreaming;

            //videoPreviewButton.Enabled = true;
            //audioPreviewButton.Enabled = true;

            //this.fpsNumeric2.Enabled = !ServiceHostOpened;
            //this.inputSimulatorCheckBox2.Enabled = !ServiceHostOpened;
            //this.screensComboBox2.Enabled = !ServiceHostOpened;
            //this.screensUpdateButton2.Enabled = !ServiceHostOpened;

        }
    }
}
