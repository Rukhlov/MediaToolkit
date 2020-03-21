﻿
using NLog;

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


using ScreenStreamer.Common;
using MediaToolkit.Core;
using MediaToolkit.UI;
using NAudio.CoreAudioApi;
using TestStreamer.Controls;
using Test.Streamer.Controls;

namespace ScreenStreamer.WinForms.App
{
    public partial class MainForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            InitializeComponent();


            mediaStreamer = new MediaStreamer();
            mediaStreamer.StateChanged += MediaStreamer_StateChanged;

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


        private ScreenStreamer.Common.MediaStreamer mediaStreamer = null;
        private StreamSession currentSession = null;

        private SynchronizationContext syncContext = null;
        private StatisticForm statisticForm = null;
        private RegionForm debugBorderForm = null;
        private SelectAreaForm selectAreaForm = null;


        private void switchStreamingStateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("switchStreamingStateButton_Click(...) ");


            if (mediaStreamer.State == MediaStreamerState.Shutdown)
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

            Stop();

        }

        private void Start()
        {
            logger.Debug("StreamingForm::Start()");

            try
            {
                bool starting = mediaStreamer.Start(currentSession);
                if (!starting)
                {
                    //...
                    logger.Warn("screenStreamer.Start(currentSession) == " + starting);
                }
 

            }
            catch (Exception ex)
            {
                OnStreamError();

                logger.Error(ex);
                MessageBox.Show(ex.ToString());


            }
        }

        private void Stop()
        {
            logger.Debug("StreamingForm::Stop()");

            try
            {
  
                bool stopping = mediaStreamer.Stop();

                if (!stopping)
                {
                    //...
                    logger.Warn("screenStreamer.Stop() == " + stopping);
                }

            }
            catch (Exception ex)
            {

                OnStreamError();

                logger.Error(ex);
                MessageBox.Show(ex.ToString());

            }

        }



        private void MediaStreamer_StateChanged()
        {
            var state = mediaStreamer.State;

            logger.Debug("ScreenStreamer_StateChanged(...) " + state);

            if (state == MediaStreamerState.Starting)
            {
                OnSteramStarting();
            }
            else if (state == MediaStreamerState.Streamming)
            {
                syncContext.Send(_ =>
                {
                    OnStreamStarted();

                }, null);
            }
            else if(state == MediaStreamerState.Stopping)
            {
                OnStreamStopping();
            }
            else if (state == MediaStreamerState.Stopped)
            {
                syncContext.Send(_ =>
                {
                    OnStreamStopped();

                }, null);
            }
            else
            {

            }
        }

        private void OnSteramStarting()
        {
            //contextMenu.Enabled = false;

            this.Cursor = Cursors.WaitCursor;

            networkSettingsLayoutPanel.Enabled = false;
            videoSourceSettingsLayoutPanel.Enabled = false;
            audioSourceSettingsLayoutPanel.Enabled = false;
            switchStreamingStateButton.Enabled = false;

            captureStatusLabel.Text = "Stream starting...";


            if (selectAreaForm != null)
            {
                selectAreaForm.Capturing = true;
            }

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

            var ex = mediaStreamer.ExceptionObj;
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
                            debugBorderForm = new RegionForm(screenDescr.CaptureRegion);
                            debugBorderForm.Visible = true;
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

                        //if (selectAreaForm != null)
                        //{
                        //    selectAreaForm.Capturing = true;
                        //}

                    }

                }

                captureStatusLabel.Text = "";
                var statusDescription = "";
                var _port = currentSession.CommunicationPort;
                if (currentSession.CommunicationPort >= 0)
                {
                    var listenUri = mediaStreamer.ListenUri;
                    statusDescription = "Stream running on port " + listenUri.Port;
                }


                //"Waiting for connection at " + _port + " port";
                captureStatusDescriptionLabel.Text = statusDescription;

            }
        }


        private void OnStreamError()
        {
            this.Cursor = Cursors.Default;


            //contextMenu.Enabled = true;
            networkSettingsLayoutPanel.Enabled = true;
            videoSourceSettingsLayoutPanel.Enabled = true;
            audioSourceSettingsLayoutPanel.Enabled = true;
            switchStreamingStateButton.Enabled = true;


            captureStatusLabel.Text = "Streaming attempt has failed";
            captureStatusDescriptionLabel.Text = "";

            if (selectAreaForm != null)
            {
                selectAreaForm.Capturing = false;
            }
        }

        private void OnStreamStopping()
        {
            this.Cursor = Cursors.WaitCursor;


            //contextMenu.Enabled = false;

            networkSettingsLayoutPanel.Enabled = false;
            videoSourceSettingsLayoutPanel.Enabled = false;
            audioSourceSettingsLayoutPanel.Enabled = false;
            switchStreamingStateButton.Enabled = false;

            captureStatusLabel.Text = "Stream stopping...";

            captureStatusDescriptionLabel.Text = "";
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

            if (mediaStreamer != null)
            {
                mediaStreamer.Shutdown();

            }

            if (statisticForm != null)
            {
                statisticForm.Stop();
                statisticForm.Visible = false;
            }

            if (debugBorderForm != null)
            {
                debugBorderForm.Close();
                debugBorderForm = null;
            }


            if (selectAreaForm != null)
            {
                selectAreaForm.Capturing = false;
            }


            var ex = mediaStreamer.ExceptionObj;
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
            logger.Debug("exitMenuItem_Click(...)");

            DoClose();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            logger.Debug("exitButton_Click(...)");

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
            logger.Debug("networkSettingsButton_Click(...)");

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
            logger.Debug("videoSourceDetailsButton_Click(...)");

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
            logger.Debug("audioSourceDetailsButton_Click(...)");

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

            int monitorIndex = 1;
            foreach (var screen in Screen.AllScreens)
            {
                var friendlyName = MediaToolkit.Utils.DisplayHelper.GetFriendlyScreenName(screen);

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

                var monitorDescr = bounds.Width + "x" + bounds.Height;
                if (!string.IsNullOrEmpty(friendlyName))
                {
                    monitorDescr = friendlyName + " " + monitorDescr;
                }

                var monitorName = "Screen " + monitorIndex + " (" + monitorDescr + ")";

                //var name = screen.DeviceName;
                //if (!string.IsNullOrEmpty(friendlyName))
                //{
                //    name += " (" + friendlyName + " " + bounds.Width  + "x" + bounds.Height + ") ";
                //}
                items.Add(new ComboBoxItem
                {
                    Name = monitorName,//screen.DeviceName,//+ "" + s.Bounds.ToString(),
                    Tag = descr,
                });

                monitorIndex++;
            }

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


            audioSourceItems = new BindingList<ComboBoxItem>();
            foreach (var d in mmdevices)
            {
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


            audioSourceComboBox.DataSource = audioSourceItems;
            audioSourceComboBox.DisplayMember = "Name";


        }

        private void videoSourceUpdateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("videoSourceUpdateButton_Click(...)");

            UpdateVideoSources();
        }

        private void audioSourceUpdateButton_Click(object sender, EventArgs e)
        {
            logger.Debug("audioSourceUpdateButton_Click(...)");

            UpdateAudioSources();
        }

 
        private void videoSourceComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            logger.Debug("videoSourceComboBox_SelectedValueChanged(...)");

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
            logger.Debug("audioSourceComboBox_SelectedValueChanged(...)");

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
            logger.Debug("audioEnabledCheckBox_CheckedChanged(...)");

            if (currentSession == null)
            {
                return;
            }
            var audioSettings = currentSession.AudioSettings;
            if (audioSettings == null)
            {
                return;
            }

            audioSettings.Enabled = audioSourceEnableCheckBox.Checked;

            logger.Debug("AudioSettings.Enabled == " + audioSettings.Enabled);

        }

        private void videoEnabledCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            logger.Debug("videoEnabledCheckBox_CheckedChanged(...)");

            if(currentSession == null)
            {
                return;
            }

            var videoSettings = currentSession.VideoSettings;

            if(videoSettings == null)
            {
                return;
            }

            videoSettings.Enabled = videoSourceEnableCheckBox.Checked;

            logger.Debug("VideoSettings.Enabled == " + videoSettings.Enabled);

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


    class RegionForm : Form
    {
        internal RegionForm(Rectangle region)
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = region.Location;
            this.Size = new Size(region.Width, region.Height);

            this.TransparencyKey = Color.White;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            RegionPanel panel = new RegionPanel();
            panel.Dock = DockStyle.Fill;

            this.Controls.Add(panel);
        }

        const int WS_EX_LAYERED = 0x00080000;
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams createParams = base.CreateParams;
        //        createParams.ExStyle |= WS_EX_LAYERED;
        //        return createParams;
        //    }
        //}

        class RegionPanel : Panel
        {
            internal RegionPanel()
            {
                timer.Tick += Timer_Tick;
                timer.Interval = 1000;
                timer.Enabled = true;

            }


            private byte tick = 0;
            private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            private void Timer_Tick(object sender, EventArgs e)
            {
                DrawBorder();

                tick++;


            }

            private void DrawBorder()
            {
                var color = Color.Red;
                var color2 = Color.Green;

                if (tick % 2 == 0)
                {
                    color = Color.Green;
                    color2 = Color.Red;
                }

                var r = this.ClientRectangle;
                var rect = new Rectangle(r.X, r.Y, r.Width - 1, r.Height - 1);
                var g = Graphics.FromHwnd(this.Handle);

                using (var b = new SolidBrush(color))
                {
                    using (var pen = new Pen(b, 3))
                    {
                        g.DrawRectangle(pen, rect);
                    }
                }

                using (var b = new SolidBrush(color2))
                {
                    using (var pen = new Pen(b, 3))
                    {
                        pen.DashPattern = new float[] { 5, 5 };

                        g.DrawRectangle(pen, rect);
                    }
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                DrawBorder();

                base.OnPaint(e);
            }

            protected override void Dispose(bool disposing)
            {

                timer.Tick -= Timer_Tick;
                timer.Dispose();

                base.Dispose(disposing);
            }

        }
    }

    class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag { get; set; }
    }

}