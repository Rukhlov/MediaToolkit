using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.NativeAPIs.Utils;
using MediaToolkit.UI;
using NLog;
using ScreenStreamer.Common;
using ScreenStreamer.WinForms.App;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms.App
{
    public partial class VideoSettingsForm : Form
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly int maxWidth = MediaToolkit.Core.Config.MaxVideoEncoderWidth;
        private readonly int minWidth = MediaToolkit.Core.Config.MinVideoEncoderWidth;
        private readonly int maxHeight = MediaToolkit.Core.Config.MaxVideoEncoderHeight;
        private readonly int minHeight = MediaToolkit.Core.Config.MinVideoEncoderHeight;

        public VideoSettingsForm()
        {
            InitializeComponent();

            syncContext = SynchronizationContext.Current;

        }

        public VideoStreamSettings VideoSettings { get; private set; }


        public void Setup(VideoStreamSettings settingsParams)
        {
            logger.Debug("Setup(...)");

            this.VideoSettings = settingsParams;

            var captureDevice = VideoSettings.CaptureDevice;
            if (captureDevice == null)
            {
                //...

            }


			LoadEncoderItems();


            UpdateCaptureInfo();


            var encoderSettings = VideoSettings.EncoderSettings;

            var encoderId = encoderSettings.EncoderId;
            var encoderItem = encoderItems.FirstOrDefault(e => e.Tag != null && ((VideoEncoderDescription)e.Tag).Id == encoderId);
            if(encoderItem == null)
            {
                encoderItem = encoderItems.FirstOrDefault();
            }

            encoderComboBox.SelectedItem = encoderItem;


            this.encWidthNumeric.Maximum = maxWidth;
            this.encWidthNumeric.Minimum = minWidth;

            this.encHeightNumeric.Maximum = maxHeight;
            this.encHeightNumeric.Minimum = minHeight;


            if (captureDevice.CaptureMode == CaptureMode.Screen)
            {
                var screenCaptureDevice = (ScreenCaptureDevice)captureDevice;
                var captureProps = screenCaptureDevice.Properties;
                // var captureDescr = screenCaptureParams.CaptureDescription;
                displayTextBox.Text = screenCaptureDevice.Name;

                captureRegion = screenCaptureDevice.CaptureRegion;


                //captureMouseCheckBox.Checked = captureProps.CaptureMouse;
                aspectRatioCheckBox.Checked = captureProps.AspectRatio;
                //showDebugInfoCheckBox.Checked = captureProps.ShowDebugInfo;
                //showCaptureBorderCheckBox.Checked = captureProps.ShowDebugBorder;

                if (screenCaptureDevice.DisplayRegion.IsEmpty)
                {
                    displayTextBox.Visible = false;
                    labelDisplay.Visible = false;
                }

                cameraTableLayoutPanel.Visible = false;
                screenCaptureTableLayoutPanel.Visible = true;
                captureSettingsButton.Enabled = true;

                windowsCaptureTLPanel.Visible = false;


                //screenCaptureDetailsPanel.Visible = true;
            }
            else if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
            {
                var uvcDevice = (UvcDevice)captureDevice;

                CaptureDeviceTextBox.Text = uvcDevice.Name;

                var profile = uvcDevice?.CurrentProfile;

                var frameSize = profile.FrameSize;
                var propsStr = frameSize.Width + "x" + frameSize.Height + ", " + profile.FrameRate + "fps" + ", " + profile.Format;

                var _profile = profile.Name + " (" + propsStr + ")";

                uvcProfileInfotextBox.Text = _profile;

                //List<ComboBoxItem> profileItems = new List<ComboBoxItem>
                //{
                //    new ComboBoxItem
                //    {
                //        Name = profile?.ToString() ?? "",
                //        Tag = profile,
                //    },
                //};

                //CaptureDeviceProfilesComboBox.DataSource = profileItems;
                //CaptureDeviceProfilesComboBox.DisplayMember = "Name";
                //CaptureDeviceProfilesComboBox.ValueMember = "Tag";

                cameraTableLayoutPanel.Visible = true;
                screenCaptureTableLayoutPanel.Visible = false;
                //screenCaptureDetailsPanel.Visible = false;

                //captureSettingsButton.Enabled = false;

                windowsCaptureTLPanel.Visible = false;
            }
            else if (captureDevice.CaptureMode == CaptureMode.AppWindow)
            {


                cameraTableLayoutPanel.Visible = false;
                screenCaptureTableLayoutPanel.Visible = false;


                UpdateWindows();

                windowsComboBox.DataSource = windowsItems;
                windowsComboBox.DisplayMember = "Name";
                windowsComboBox.ValueMember = "Tag";

                windowsComboBox.SelectedItem = windowsItems.FirstOrDefault();
                windowsCaptureTLPanel.Visible = true;


            }


            var videoEncoderSettings = VideoSettings.EncoderSettings;
            var encoderResolution = videoEncoderSettings.Resolution;
            if (encoderResolution.Width <= encWidthNumeric.Maximum && encoderResolution.Width >= encWidthNumeric.Minimum)
            {
                this.encWidthNumeric.Value = encoderResolution.Width;
            }

            if (encoderResolution.Height <= encHeightNumeric.Maximum && encoderResolution.Height >= encHeightNumeric.Minimum)
            {
                this.encHeightNumeric.Value = encoderResolution.Height;
            }

            this.captureRegionTextBox.Text = captureRegion.ToString();
            this.checkBoxResoulutionFromSource.Checked = VideoSettings.UseEncoderResoulutionFromSource;

            this.encoderComboBox.SelectedItem = videoEncoderSettings.EncoderFormat;
            //this.encProfileComboBox.SelectedItem = videoEncoderSettings.Profile;
            //this.bitrateModeComboBox.SelectedItem = videoEncoderSettings.BitrateMode;
            //this.MaxBitrateNumeric.Value = videoEncoderSettings.MaxBitrate;
            //this.bitrateNumeric.Value = videoEncoderSettings.Bitrate;
            //this.fpsNumeric.Value = videoEncoderSettings.FrameRate;
            //this.latencyModeCheckBox.Checked = videoEncoderSettings.LowLatency;


        }

        private void applyButton_Click(object sender, EventArgs e)
        {

            logger.Debug("applyButton_Click(...)");

            if (VideoSettings == null)
            {
                return;
            }

            var captureDevice = VideoSettings.CaptureDevice;
            if (captureDevice.CaptureMode == CaptureMode.Screen)
            {
                var screenCaptureParams = (ScreenCaptureDevice)captureDevice;
                screenCaptureParams.CaptureRegion = this.captureRegion;


                //screenCaptureParams.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;
                screenCaptureParams.Properties.AspectRatio = this.aspectRatioCheckBox.Checked;
                //screenCaptureParams.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
                //screenCaptureParams.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;

            }
            else if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
            {

            }
			else if (captureDevice.CaptureMode == CaptureMode.AppWindow)
			{
				var item = windowsComboBox.SelectedItem;
				if (item != null)
				{
					var tag = ((ComboBoxItem)item).Tag;
					if (tag != null)
					{
						WindowDescription window = tag as WindowDescription;
						if (window != null)
						{
							var windowCapture = (WindowCaptureDevice)captureDevice;
							windowCapture.ClientRect = window.clientRect;
							windowCapture.Resolution = window.clientRect.Size;

							windowCapture.hWnd = window.hWnd;
							windowCapture.ProcName = window.processName;
							windowCapture.WindowClass = window.windowClass;
							windowCapture.WindowTitle = window.windowTitle;

						}
					}
				}

			}
			else
            {

            }


            VideoSettings.StreamFlags &= ~VideoStreamFlags.UseEncoderResoulutionFromSource;
            if (this.checkBoxResoulutionFromSource.Checked)
            {
                VideoSettings.StreamFlags |= VideoStreamFlags.UseEncoderResoulutionFromSource;
            }

            //if (!VideoSettings.UseEncoderResoulutionFromSource)
            {
                int width = (int)this.encWidthNumeric.Value;
                int height = (int)this.encHeightNumeric.Value;

                if (width % 2 != 0)
                {
                    width--;
                }

                if (height % 2 != 0)
                {
                    height--;
                }

                VideoSettings.EncoderSettings.Width = width;
                VideoSettings.EncoderSettings.Height = height;
            }

            //var item = encoderComboBox.SelectedItem as ComboBoxItem;
            //if (item != null)
            //{
            //    var tag = item.Tag;
            //    if (tag != null)
            //    {
            //        var encoderDescr = (VideoEncoderDescription)tag;
            //        VideoSettings.EncoderSettings.EncoderId = encoderDescr.Id;
            //        VideoSettings.EncoderSettings.EncoderFormat = encoderDescr.Format; //(VideoCodingFormat)this.encoderComboBox.SelectedItem;
            //    }
            //}

            var encoderDescr = GetEncoderDescr();
            if (encoderDescr != null)
            {
                VideoSettings.EncoderSettings.EncoderId = encoderDescr.Id;
                VideoSettings.EncoderSettings.EncoderFormat = encoderDescr.Format; //(VideoCodingFormat)this.encoderComboBox.SelectedItem;

            }

            //VideoSettings.EncoderSettings.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
            //VideoSettings.EncoderSettings.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;
            //VideoSettings.EncoderSettings.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
            //VideoSettings.EncoderSettings.Bitrate = (int)this.bitrateNumeric.Value;
            //VideoSettings.EncoderSettings.FrameRate = (int)this.fpsNumeric.Value;
            //VideoSettings.EncoderSettings.LowLatency = this.latencyModeCheckBox.Checked;

            //TODO: Validate settings...

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }



        private void adjustAspectRatioButton_Click(object sender, EventArgs e)
        {
            logger.Debug("adjustAspectRatioButton_Click(...)");

            if (VideoSettings == null)
            {
                return;
            }

            var srcSize = new Size(this.captureRegion.Width, this.captureRegion.Height);
            var destSize = new Size((int)this.encWidthNumeric.Value, (int)this.encHeightNumeric.Value);

            var newSize = AdjustVideoResolution(srcSize, destSize);

            this.encWidthNumeric.Value = newSize.Width;
            this.encHeightNumeric.Value = newSize.Height;

            //var captureDevice = VideoSettings.CaptureDevice;
            //if (captureDevice.CaptureMode == CaptureMode.Screen)
            //{
            //    srcSize = ((ScreenCaptureDevice)captureDevice).CaptureRegion.Size;
            //}
            //else if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
            //{
            //    var profile = ((UvcDevice)captureDevice).CurrentProfile;
            //    srcSize = profile.FrameSize;
            //}


            //var destSize = new Size((int)this.encWidthNumeric.Value, (int)this.encHeightNumeric.Value);

            //var ratio = srcSize.Width / (double)srcSize.Height;
            //int destWidth = destSize.Width;


            //int destHeight = (int)(destWidth / ratio);
            //if (destHeight > maxHeight)
            //{
            //    destHeight = maxHeight;
            //    destWidth = (int)(destHeight * ratio);
            //}

            //if (destHeight < minHeight)
            //{
            //    destHeight = minHeight;
            //    destWidth = (int)(destHeight * ratio);
            //}


            //if (ratio < 1)
            //{
            //    destHeight = destSize.Height;
            //    destWidth = (int)(destHeight * ratio);


            //    if (destWidth > maxWidth)
            //    {
            //        destWidth = maxWidth;
            //        destHeight = (int)(destWidth / ratio);
            //    }

            //    if (destWidth < minWidth)
            //    {
            //        destWidth = minWidth;
            //        destHeight = (int)(destWidth / ratio);
            //    }
            //}


            //this.encWidthNumeric.Value = destWidth;
            //this.encHeightNumeric.Value = destHeight;

        }


        private Size AdjustVideoResolution(Size srcSize, Size destSize)
        {
            var srcRatio = srcSize.Width / (double)srcSize.Height;

            int destWidth = destSize.Width;
            int destHeight = destSize.Height;

            if (destWidth % 2 != 0)
            {// размеры должны быть четными
                destWidth--;
            }

            if (destHeight % 2 != 0)
            {
                destHeight--;
            }

            if (srcRatio > 1)
            {// ширина больше высоты самое распространенное
             // меняем ширину 

                if (destHeight < minHeight)
                {
                    destHeight = minHeight;
                }

                destWidth = (int)(destHeight * srcRatio);
                if (destWidth % 2 != 0)
                {
                    destWidth--;
                }

                if (destWidth > maxWidth)
                {
                    destWidth = maxWidth;
                    destHeight = (int)(destWidth / srcRatio);

                    if (destHeight % 2 != 0)
                    {
                        destHeight--;
                    }
                }

            }
            else
            { // меняем высоту

                if (destWidth < minWidth)
                {
                    destWidth = minWidth;
                }

                destHeight = (int)(destWidth / srcRatio);
                if (destHeight % 2 != 0)
                {
                    destHeight--;
                }

                if (destHeight > maxHeight)
                {
                    destHeight = maxHeight;
                    destWidth = (int)(destHeight * srcRatio);

                    if (destWidth % 2 != 0)
                    {
                        destWidth--;
                    }
                }

            }

            return new Size(destWidth, destHeight);

        }

        private void checkBoxResoulutionFromSource_CheckedChanged(object sender, EventArgs e)
        {
            bool useResoulutionFromSource = checkBoxResoulutionFromSource.Checked;

            panelEncoderResoulution.Enabled = !useResoulutionFromSource;
            adjustAspectRatioButton.Enabled = !useResoulutionFromSource;
            aspectRatioCheckBox.Enabled = !useResoulutionFromSource;

        }

        private VideoPreviewForm previewForm = null;
        private IVideoSource videoSource = null;

        private SynchronizationContext syncContext = null;


        private void previewButton_Click(object sender, EventArgs e)
        {
            logger.Debug("previewButton_Click(...)");

            if (VideoSettings == null)
            {
                return;
            }

            try
            {
                var captureDevice = this.VideoSettings.CaptureDevice;

                if (videoSource == null)
                {
                    if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
                    {
                        videoSource = new VideoCaptureSource();
                        videoSource.Setup(captureDevice);
                    }
                    else if (captureDevice.CaptureMode == CaptureMode.Screen)
                    {
                        videoSource = new ScreenSource();
                        videoSource.Setup(captureDevice);
                    }
					else if (captureDevice.CaptureMode == CaptureMode.AppWindow)
					{
						videoSource = new ScreenSource();
						videoSource.Setup(captureDevice);
					}

					videoSource.CaptureStarted += VideoSource_CaptureStarted;
                    videoSource.CaptureStopped += VideoSource_CaptureStopped;

                    videoSource.BufferUpdated += VideoSource_BufferUpdated;
                }


                if (videoSource.State == CaptureState.Capturing)
                {
                    videoSource.Stop();
                }
                else
                {
                    videoSource.Start();
                }

                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                this.Enabled = true;

                MessageBox.Show(ex.Message);

                if (videoSource != null)
                {
                    videoSource.BufferUpdated -= VideoSource_BufferUpdated;
                    videoSource.CaptureStarted -= VideoSource_CaptureStarted;
                    videoSource.CaptureStopped -= VideoSource_CaptureStopped;
                    videoSource.Close(true);
                    videoSource = null;
                }

            }
        }


        private void VideoSource_CaptureStarted()
        {
            logger.Debug("VideoSource_CaptureStarted()");

            syncContext.Send(_ =>
            {
                try
                {
                    OnVideoSourceStarted();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    this.Enabled = true;
                }


            }, null);

        }


        private void VideoSource_BufferUpdated()
        {
            if (previewForm != null)
            {
                previewForm.Render();
            }
        }


        private void VideoSource_CaptureStopped(object obj)
        {
            logger.Debug("VideoSource_CaptureStopped()");

            syncContext.Send(_ =>
            {
                try
                {
                    OnVideoSourceStopped();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    this.Enabled = true;
                }

            }, null);

        }



        private void OnVideoSourceStarted()
        {
            logger.Debug("OnVideoSourceStarted()");
            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm.FormClosed -= PreviewForm_FormClosed;
                previewForm = null;
            }

            var previewSettings = Config.Data.VideoPreviewSettings;
            if (previewForm == null || previewForm.IsDisposed)
            {

                previewForm = new VideoPreviewForm
                {
                    //ClientSize = VideoSettings.CaptureDevice.Resolution,
                    Size = new Size(640, 480),//previewSettings.PreviewSize,

                    StartPosition = FormStartPosition.CenterScreen,
                    Icon = ScreenStreamer.WinForms.App.Properties.Resources.logo,

                    //ShowIcon = false,
                };

                previewForm.FormClosed += PreviewForm_FormClosed;

                bool result = previewForm.Setup(videoSource);

                if (!result)
                {
                    //...
                }
            }

            var captureDevice = VideoSettings.CaptureDevice;
            var resolution = captureDevice.Resolution;

            var title = captureDevice.Name;
            var description = "";
            if (captureDevice is ScreenCaptureDevice)
            {
                var screenCapture = (ScreenCaptureDevice)captureDevice;
                if (screenCapture.DisplayRegion.IsEmpty)
                {
                    description = resolution.Width + "x" + resolution.Height + " " + screenCapture.Properties.Fps + " fps";
                }

            }
            else if (captureDevice is UvcDevice)
            {
                var uvcDevice = (UvcDevice)captureDevice;

                description = resolution.Width + "x" + resolution.Height + " " + uvcDevice.CurrentProfile.FrameRate + " fps";
            }
            else if(captureDevice is WindowCaptureDevice)
            {

            }

            if (!string.IsNullOrEmpty(description))
            {
                title += " (" + description + ")";
            }
            previewForm.Text = title;

            bool fitVideoToWindow = previewSettings.FitVideoToWindow;

            previewForm.UpdateWindow(fitVideoToWindow, captureDevice.Resolution);
            captureSettingsButton.Enabled = false;
            previewForm.Visible = true;



        }

        private void OnVideoSourceStopped()
        {
            logger.Debug("OnVideoSourceStopped()");

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();
                previewForm = null;
            }

            captureSettingsButton.Enabled = true;

            videoSource.Close(true);
            videoSource.CaptureStarted -= VideoSource_CaptureStarted;
            videoSource.CaptureStopped -= VideoSource_CaptureStopped;
            videoSource.BufferUpdated -= VideoSource_BufferUpdated;
            videoSource = null;

        }


        private void PreviewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.Debug("PreviewForm_FormClosed()");

            if (videoSource != null)
            {
                videoSource.Stop();
            }
        }




        protected override void OnClosed(EventArgs e)
        {
            logger.Debug("VideoSettingsForm::OnClosed()");

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Close();

                previewForm.FormClosed -= PreviewForm_FormClosed;
                previewForm = null;
            }

			CloseWindowHook();


			base.OnClosed(e);
        }





        private Rectangle captureRegion = Rectangle.Empty;

        private void snippingToolButton_Click(object sender, EventArgs e)
        {
            if (VideoSettings == null)
            {
                return;
            }
            var captureDevice = VideoSettings.CaptureDevice;
            var screenCapture = captureDevice as ScreenCaptureDevice;
            if (captureDevice == null)
            {
                return;
            }

            var selectedRegion = SnippingTool.Snip(screenCapture.DisplayRegion);
            if (!selectedRegion.IsEmpty)
            {
                this.captureRegion = selectedRegion;

                this.captureRegionTextBox.Text = captureRegion.ToString();
            }
            else
            {// error, cancelled...

            }

        }




        private void UpdateCaptureInfo()
        {

            List<ComboBoxItem> captureTypes = new List<ComboBoxItem>();


            var captureProps = Config.Data.ScreenCaptureProperties;
            var caputreDevice = VideoSettings.CaptureDevice;

            var captType = "Unknown";
            if (captureProps.CaptureType == VideoCaptureType.DXGIDeskDupl)
            {
                captType = "Desktop Duplication API";
            }
            else if (captureProps.CaptureType == VideoCaptureType.GDI)
            {
                captType = "GDI";
            }
			else if (captureProps.CaptureType == VideoCaptureType.GDILayered)
			{
				captType = "GDI Layered";
			}

			var resolution = caputreDevice.Resolution;
            var propsStr = resolution.Width + "x" + resolution.Height + ", " + captureProps.Fps + "fps" + ", " + (captureProps.UseHardware ? "GPU" : "CPU");

			captInfoTextBox.Text = captType + " (" + propsStr + ")"; 

            captWindowInfoTextBox.Text = captType + " (" + propsStr + ")";


            //List<VideoCaptureType> captureTypes = new List<VideoCaptureType>();
            //captureTypes.Add(VideoCaptureType.DXGIDeskDupl);
            //captureTypes.Add(VideoCaptureType.GDI);
            ////captureTypes.Add(CaptureType.GDIPlus);
            //captureTypes.Add(VideoCaptureType.Direct3D9);
            //captureTypes.Add(VideoCaptureType.Datapath);

            //captureTypesComboBox.DataSource = captureTypes;

        }

        //private void LoadEncoderItems()
        //{
        //    var items = new List<VideoCodingFormat>
        //    {
        //        VideoCodingFormat.H264,
        //        VideoCodingFormat.JPEG,
        //    };

        //    encoderComboBox.DataSource = items;

        //}


        private List<ComboBoxItem> encoderItems = new List<ComboBoxItem>();
        private void LoadEncoderItems()
        {

            encoderItems.Clear();

            var encoders = MediaToolkit.MediaFoundation.MfTool.FindVideoEncoders();

            foreach(var enc in encoders)
            {
                if(enc.Activatable && enc.Format == VideoCodingFormat.H264)
                {
                    var item = new ComboBoxItem
                    {
                        Name = enc.Name,
                        Tag = enc,
                    };

                    encoderItems.Add(item);
                }

            }

            VideoEncoderDescription libx264Description = new VideoEncoderDescription
            {
                Id = "libx264",
                Name = "libx264",
                Format = VideoCodingFormat.H264,
                IsHardware = false,
                Activatable = true,
               
            };

            encoderItems.Add(new ComboBoxItem
            {
                Name = libx264Description.Name,
                Tag = libx264Description,
            });

            encoderComboBox.DisplayMember = "Name";
            encoderComboBox.ValueMember = "Tag";
            encoderComboBox.DataSource = encoderItems;


        }

        private void encoderComboBox_SelectedValueChanged(object sender, EventArgs e)
        {

            //VideoEncoderDescription encoderDescr = GetEncoderDescr();
            //if (encoderDescr != null)
            //{
            //    VideoSettings.EncoderSettings.EncoderId = encoderDescr.Id;
            //    VideoSettings.EncoderSettings.EncoderFormat = encoderDescr.Format;
            //}


        }

        private VideoEncoderDescription GetEncoderDescr()
        {
            VideoEncoderDescription encoderDescr = null;
            var item = encoderComboBox.SelectedItem as ComboBoxItem;
            if (item != null)
            {
                var tag = item.Tag;
                if (tag != null)
                {
                    encoderDescr = (VideoEncoderDescription)tag;
                }
            }

            return encoderDescr;
        }

        private void encoderSettingsButton_Click(object sender, EventArgs e)
        {

            var encoderDescr = GetEncoderDescr();

            VideoEncoderSettingsForm f = new VideoEncoderSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };

            f.Setup(VideoSettings.EncoderSettings, encoderDescr);

            f.ShowDialog();
        }

        private void captureSettingsButton_Click(object sender, EventArgs e)
        {

            var captDevice = VideoSettings.CaptureDevice;

            if (captDevice.CaptureMode == CaptureMode.Screen || captDevice.CaptureMode == CaptureMode.AppWindow)
            {
                VideoCaptSettingsForm f = new VideoCaptSettingsForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                };

                f.Setup(VideoSettings.CaptureDevice);

                f.ShowDialog();

				UpdateCaptureInfo();

			}
            else if (captDevice.CaptureMode == CaptureMode.UvcDevice)
            {
                var deviceName = captDevice.Name;

                MediaToolkit.NativeAPIs.DShow.DsUtils.ShowVideoDevicePropertyPages(deviceName, this.Handle);
            }

        }


		private BindingList<ComboBoxItem> windowsItems = new BindingList<ComboBoxItem>();
		private void windowsUpdateButton_Click(object sender, EventArgs e)
		{
			//windowsComboBox.SelectedItem = null;
			UpdateWindows();
			SetWindow();


		}

		private void UpdateWindows()
		{
			windowsItems.Clear();

			var windows = DesktopManager.GetWindows();
			var currentPid = -1;
			using (Process p = Process.GetCurrentProcess())
			{
				currentPid = p.Id;
			}

			foreach (var window in windows)
			{
				if(window.processId == currentPid)
				{
					continue;
				}

				var item = new ComboBoxItem
				{
					Name = window.windowTitle + " [" + window.processName + "]",
					Tag = window,
				};

				windowsItems.Add(item);
			}

			//windowsComboBox.DataSource = windowsItems;

		}

		SelectAreaForm windowForm = null;
		private WindowHook windowHook = null;
		private void windowsComboBox_SelectedValueChanged(object sender, EventArgs e)
		{
			SetWindow();
		}

		private void SetWindow()
		{
			var captureDevice = VideoSettings.CaptureDevice;

			var item = windowsComboBox.SelectedItem;
			if (item != null)
			{
				var tag = ((ComboBoxItem)item).Tag;
				if (tag != null)
				{
					WindowDescription window = tag as WindowDescription;
					if (window != null)
					{
						var windowCapture = (WindowCaptureDevice)captureDevice;
						windowCapture.ClientRect = window.clientRect;
						windowCapture.Resolution = window.clientRect.Size;

						windowCapture.hWnd = window.hWnd;
						windowCapture.ProcName = window.processName;
						windowCapture.WindowClass = window.windowClass;
						windowCapture.WindowTitle = window.windowTitle;

						if (windowForm == null)
						{
							windowForm = new SelectAreaForm(true, false)
							{
								StartPosition = FormStartPosition.Manual,
								Locked = true,
							};
						}

						windowForm.Visible = false;

						if (windowHook != null)
						{
							windowHook.Close();
						}

						windowHook = new WindowHook();


						windowHook.LocationChanged += WindowHook_LocationChanged;
						windowHook.WindowClosed += WindowHook_WindowClosed;
						windowHook.VisibleChanged += WindowHook_VisibleChanged;

						windowHook.Setup(windowCapture.hWnd);

						var rect = windowHook.GetCurrentWindowRect();
                     
						SetWindowsPosition(rect);

						windowForm.Visible = true;
					}
				}
			}
		}

		private void WindowHook_VisibleChanged(bool visible)
		{
			if (windowForm != null)
			{
				windowForm.Visible = visible;
			}

		}

		private void WindowHook_LocationChanged(Rectangle rect)
		{
			SetWindowsPosition(rect);
		}

		private void WindowHook_WindowClosed()
		{
			CloseWindowHook();
		}

		private void CloseWindowHook()
		{
			if (windowHook != null)
			{

				windowHook.LocationChanged -= WindowHook_LocationChanged;
				windowHook.VisibleChanged -= WindowHook_VisibleChanged;
				windowHook.WindowClosed -= WindowHook_WindowClosed;

				windowHook.Close();
				windowHook = null;
			}

			if (windowForm != null)
			{
				windowForm.Visible = false;
				windowForm.Location = Point.Empty;
				windowForm.Size = Size.Empty;
			}
		}

		private void SetWindowsPosition(Rectangle rectangle)
		{
			//Debug.WriteLine(rectangle.ToString());

			if (windowForm != null)
			{
				windowForm.Location = rectangle.Location;
				windowForm.Size = rectangle.Size;
			}

            windowRegionTextBox.Text = rectangle.ToString();

        }

	}
}
