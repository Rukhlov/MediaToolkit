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
    public partial class StreamingForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamingForm()
        {
            InitializeComponent();


            screenStreamer = new ScreenStreamer();

            screenStreamer.StreamStarted += ScreenStreamer_StreamStarted;
            screenStreamer.StreamStopped += ScreenStreamer_StreamStopped;

            //Validate session...
            currentSession = Config.Data.Session;

            var videoSettings = currentSession.VideoSettings;
            var audioSettings = currentSession.AudioSettings;


            UpdateVideoSources();

            UpdateAudioSources();


            audioSourceEnableCheckBox.Checked = audioSettings.Enabled;
            videoSourceEnableCheckBox.Checked = videoSettings.Enabled;

            streamNameTextBox.Text = currentSession.StreamName;
            captureStatusLabel.Text = "Ready to stream";      
            captureStatusDescriptionLabel.Text = "";

            videoSourceComboBox.SelectedItem = null;
            videoSourceComboBox.SelectedValueChanged += videoSourceComboBox_SelectedValueChanged;

            ComboBoxItem videoItem = null;
            var currentVideoDeviceId = videoSettings?.CaptureDevice?.DeviceId;
            if (!string.IsNullOrEmpty(currentVideoDeviceId))
            {
                videoItem = videoSourceItems.FirstOrDefault(i => i.Tag != null && ((VideoCaptureDevice)i.Tag).DeviceId == currentVideoDeviceId);
            }

            if(videoItem == null)
            {
                videoItem = videoSourceItems.FirstOrDefault();
            }

            if (videoItem != null)
            {
                videoSourceComboBox.SelectedItem = videoItem;
            }

            audioSourceComboBox.SelectedItem = null;
            audioSourceComboBox.SelectedValueChanged += audioSourceComboBox_SelectedValueChanged;

            ComboBoxItem audioItem = audioSourceItems.FirstOrDefault();
            var currentAudioDeviceId = audioSettings?.CaptureDevice?.DeviceId;
            if (!string.IsNullOrEmpty(currentAudioDeviceId))
            {
                audioItem = audioSourceItems.FirstOrDefault(i => i.Tag != null && ((AudioCaptureDeviceDescription)i.Tag).DeviceId == currentAudioDeviceId);
            }

            if (audioItem == null)
            {
                audioItem = audioSourceItems.FirstOrDefault();
            }

            if (audioItem != null)
            {
                audioSourceComboBox.SelectedItem = audioItem;
            }

            syncContext = SynchronizationContext.Current;

        }

        private ScreenStreamer screenStreamer = null;
        private StreamSession currentSession = null;

        private SynchronizationContext syncContext = null;
        private StatisticForm statisticForm = null;
        private RegionForm regionForm = null;
        private SelectAreaForm selectAreaForm = null;


        private void switchStreamingStateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("startButton_Click(...) ");


            if (screenStreamer.State == StreamerState.Shutdown)
            {
                Start();
            }
            else
            {
                Stop();
            }

        }
        private void stopStreamingButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...) ");

        }

        private void Start()
        {
            try
            {

                //contextMenu.Enabled = false;

                networkSettingsLayoutPanel.Enabled = false;
                videoSourceSettingsLayoutPanel.Enabled = false;
                audioSourceSettingsLayoutPanel.Enabled = false;
                switchStreamingStateButton.Enabled = false;

                captureStatusLabel.Text = "Stream starting...";

                this.Cursor = Cursors.WaitCursor;

                screenStreamer.Start(currentSession);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                MessageBox.Show(ex.Message);

                networkSettingsLayoutPanel.Enabled = true;
                videoSourceSettingsLayoutPanel.Enabled = true;
                audioSourceSettingsLayoutPanel.Enabled = true;
                switchStreamingStateButton.Enabled = true;

                captureStatusLabel.Text = "Streaming attempt has failed";

                captureStatusDescriptionLabel.Text = "";
            }
        }


        private void ScreenStreamer_StreamStarted()
        {
            syncContext.Send(_ =>
            {
                OnStreamStarted();

            }, null);

        }

        private void OnStreamStarted()
        {

            networkSettingsLayoutPanel.Enabled = false;
            videoSourceSettingsLayoutPanel.Enabled = false;
            audioSourceSettingsLayoutPanel.Enabled = false;

            switchStreamingStateButton.Enabled = true;
            switchStreamingStateButton.Text = "Stop Streaming";

            this.Cursor = Cursors.Default;

            //contextMenu.Enabled = true;

            var ex = screenStreamer.ExceptionObj;
            if (ex != null)
            {
                captureStatusLabel.Text = "Streaming attempt has failed";

                var iex = ex.InnerException;
                MessageBox.Show(iex.Message);

            }
            else
            {
                var videoSettings = currentSession.VideoSettings;

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
                            if(statisticForm == null)
                            {
                                statisticForm = new StatisticForm();
                            }

                            statisticForm.Location = screenDescr.CaptureRegion.Location;

                            statisticForm.Start();

                        }

                        if (selectAreaForm != null)
                        {
                            selectAreaForm.Capturing = true;
                        }

                    }

                }

                captureStatusLabel.Text = "";
                var statusDescription = "";
                var _port = currentSession.CommunicationPort;
                if (currentSession.CommunicationPort >= 0)
                {
                    var listenUri = screenStreamer.ListenUri;
                    statusDescription = "Stream running on port " + listenUri.Port;
                }


                //"Waiting for connection at " + _port + " port";
                captureStatusDescriptionLabel.Text = statusDescription;

            }
        }

        private void ScreenStreamer_StreamStopped()
        {

            syncContext.Send(_ =>
            {
                OnStreamStopped();

            }, null);

            
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

                screenStreamer.Stop();
            }
            catch (Exception ex)
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

        private void OnStreamStopped()
        {

            networkSettingsLayoutPanel.Enabled = true;
            videoSourceSettingsLayoutPanel.Enabled = true;
            audioSourceSettingsLayoutPanel.Enabled = true;

            switchStreamingStateButton.Enabled = true;
            switchStreamingStateButton.Text = "Start Streaming";

            //contextMenu.Enabled = true;

            captureStatusLabel.Text = "Ready to stream";

            captureStatusDescriptionLabel.Text = "";

            this.Cursor = Cursors.Default;

            if (screenStreamer != null)
            {
                screenStreamer.Shutdown();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (regionForm != null)
            {
                regionForm.Close();
                regionForm = null;
            }


            if (selectAreaForm != null)
            {
                selectAreaForm.Capturing = false;
            }


            var ex = screenStreamer.ExceptionObj;
            if (ex != null)
            {
                var iex = ex.InnerException;
                MessageBox.Show(iex.Message);
            }
            else
            {


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

            f.Init(currentSession);

            f.ShowDialog();


            streamNameTextBox.Text = currentSession.StreamName;
        }

        private void streamNameTextBox_TextChanged(object sender, EventArgs e)
        {
            var streamName = streamNameTextBox.Text;
            currentSession.StreamName = streamName;

        }


        private void videoSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new VideoSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };

            f.Setup(currentSession.VideoSettings);

            f.ShowDialog();

            //videoSettings.NetworkSettings.TransportMode = transportMode;
        }


        private void audioSourceDetailsButton_Click(object sender, EventArgs e)
        {
            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(currentSession.AudioSettings);

            f.ShowDialog();

            //audioSettings.NetworkParams.TransportMode = transportMode;
        }


        private BindingList<ComboBoxItem> videoSourceItems = null;
        private void UpdateVideoSources()
        {

            List<ComboBoxItem> items = new List<ComboBoxItem>();

            var captureProperties = Config.Data.ScreenCaptureProperties;

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
                    DeviceId = screen.DeviceName,

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
                    DeviceId = "AllScreens",
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

            var customRegion = Config.Data.SelectAreaRectangle;
            ScreenCaptureDevice customRegionDescr = new ScreenCaptureDevice
            {
                CaptureRegion = customRegion,
                DisplayRegion = Rectangle.Empty,

                Resolution = customRegion.Size,
                Properties = captureProperties,
                Name = "Screen Region",
                DeviceId = "ScreenRegion",

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

        private BindingList<ComboBoxItem> audioSourceItems = null;
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
            audioSourceItems = new BindingList<ComboBoxItem>();
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

                audioSourceItems.Add(item);
                d?.Dispose();
            }
            mmdevices.Clear();

            //dataSource.Add(new ComboBoxItem { Name = "Disabled", Tag = null, });

            audioSourceComboBox.DataSource = audioSourceItems;
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
                        var customRegion = Config.Data.SelectAreaRectangle;
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
            currentSession.VideoSettings.CaptureDevice = captureParams;

            videoSourceDetailsButton.Enabled = true;//!(displayRect.IsEmpty && displayName == string.Empty);

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

                Config.Data.SelectAreaRectangle = new Rectangle(selectAreaForm.Location,selectAreaForm.Size);
            }
        }

        private void audioSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var captureDevice = GetCurrentAudioCaptureDevice();
            if(captureDevice == null)
            {
                currentSession.AudioSettings.Enabled = false;
            }

            //audioSettings.Enabled = (captureSettings != null);

            currentSession.AudioSettings.CaptureDevice = captureDevice;
 
        }


        private void audioEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            currentSession.AudioSettings.Enabled = audioSourceEnableCheckBox.Checked;


        }

        private void videoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            currentSession.VideoSettings.Enabled = videoSourceEnableCheckBox.Checked;

        }

        private AudioCaptureDeviceDescription GetCurrentAudioCaptureDevice()
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
