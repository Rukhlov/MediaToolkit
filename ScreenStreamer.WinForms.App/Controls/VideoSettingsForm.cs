using MediaToolkit;
using MediaToolkit.Core;
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

            LoadEncoderProfilesItems();

            LoadEncoderItems();

            LoadRateModeItems();

            LoadCaptureTypes();

            this.encWidthNumeric.Maximum = maxWidth; 
            this.encWidthNumeric.Minimum = minWidth;

            this.encHeightNumeric.Maximum = maxHeight;
            this.encHeightNumeric.Minimum = minHeight;


            if(captureDevice.CaptureMode == CaptureMode.Screen)
            {
                var screenCaptureDevice = (ScreenCaptureDevice)captureDevice;
				var captureProps = screenCaptureDevice.Properties;
				// var captureDescr = screenCaptureParams.CaptureDescription;
				displayTextBox.Text = screenCaptureDevice.Name;

				captureRegion = screenCaptureDevice.CaptureRegion;
                captureMouseCheckBox.Checked = captureProps.CaptureMouse;
                aspectRatioCheckBox.Checked = captureProps.AspectRatio;
                showDebugInfoCheckBox.Checked = captureProps.ShowDebugInfo;
                showCaptureBorderCheckBox.Checked = captureProps.ShowDebugBorder;

                if (screenCaptureDevice.DisplayRegion.IsEmpty)
                {
                    displayTextBox.Visible = false;
                    labelDisplay.Visible = false;
                }

				cameraTableLayoutPanel.Visible = false;
				screenCaptureTableLayoutPanel.Visible = true;
				screenCaptureDetailsPanel.Visible = true;
			}
            else if(captureDevice.CaptureMode == CaptureMode.UvcDevice)
            {
                var uvcDevice = (UvcDevice)captureDevice;

                CaptureDeviceTextBox.Text = uvcDevice.Name;

                var profile = uvcDevice?.CurrentProfile;
                List<ComboBoxItem> profileItems = new List<ComboBoxItem>
                {
                    new ComboBoxItem
                    {
                        Name = profile?.ToString() ?? "",
                        Tag = profile,
                    },
                };

                CaptureDeviceProfilesComboBox.DataSource = profileItems;
                CaptureDeviceProfilesComboBox.DisplayMember = "Name";
                CaptureDeviceProfilesComboBox.ValueMember = "Tag";

                cameraTableLayoutPanel.Visible = true;
                screenCaptureTableLayoutPanel.Visible = false;
                screenCaptureDetailsPanel.Visible = false;

            }
            else
            {

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

            this.encoderComboBox.SelectedItem = videoEncoderSettings.Encoder;
            this.encProfileComboBox.SelectedItem = videoEncoderSettings.Profile;
            this.bitrateModeComboBox.SelectedItem = videoEncoderSettings.BitrateMode;
            this.MaxBitrateNumeric.Value = videoEncoderSettings.MaxBitrate;
            this.bitrateNumeric.Value = videoEncoderSettings.Bitrate;
            this.fpsNumeric.Value = videoEncoderSettings.FrameRate;
            this.latencyModeCheckBox.Checked = videoEncoderSettings.LowLatency;


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
                screenCaptureParams.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;
                screenCaptureParams.Properties.AspectRatio = this.aspectRatioCheckBox.Checked;
                screenCaptureParams.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
                screenCaptureParams.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;
                
            }
            else if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
            {

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


            VideoSettings.EncoderSettings.Encoder = (VideoEncoderMode)this.encoderComboBox.SelectedItem;
            VideoSettings.EncoderSettings.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
            VideoSettings.EncoderSettings.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;
            VideoSettings.EncoderSettings.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
            VideoSettings.EncoderSettings.Bitrate = (int)this.bitrateNumeric.Value;
            VideoSettings.EncoderSettings.FrameRate = (int)this.fpsNumeric.Value;
            VideoSettings.EncoderSettings.LowLatency = this.latencyModeCheckBox.Checked;

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

            var captureDevice = VideoSettings.CaptureDevice;
            if (captureDevice.CaptureMode == CaptureMode.Screen)
            {
                srcSize = ((ScreenCaptureDevice)captureDevice).CaptureRegion.Size;
            }
            else if (captureDevice.CaptureMode == CaptureMode.UvcDevice)
            {
                var profile = ((UvcDevice)captureDevice).CurrentProfile;
                srcSize = profile.FrameSize;
            }

           
            var destSize= new Size((int)this.encWidthNumeric.Value, (int)this.encHeightNumeric.Value);

            var ratio = srcSize.Width / (double)srcSize.Height;
            int destWidth = destSize.Width;


            int destHeight = (int)(destWidth / ratio);
			if(destHeight > maxHeight)
			{
				destHeight = maxHeight;
				destWidth = (int)(destHeight * ratio);
			}
			
			if(destHeight< minHeight)
			{
				destHeight = minHeight;
				destWidth = (int)(destHeight * ratio);
			}


            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);


				if (destWidth > maxWidth)
				{
					destWidth = maxWidth;
					destHeight = (int)(destWidth / ratio);
				}

				if (destWidth < minWidth)
				{
					destWidth = minWidth;
					destHeight = (int)(destWidth / ratio);
				}
			}
			

            this.encWidthNumeric.Value = destWidth;
            this.encHeightNumeric.Value = destHeight;
            
        }


        private void checkBoxResoulutionFromSource_CheckedChanged(object sender, EventArgs e)
        {
            bool useResoulutionFromSource = checkBoxResoulutionFromSource.Checked;

            panelEncoderResoulution.Enabled = !useResoulutionFromSource;
            adjustAspectRatioButton.Enabled = !useResoulutionFromSource;

        }

        private PreviewForm previewForm = null;
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
						var screenCapture = (ScreenCaptureDevice)captureDevice;
						var screenCaptureProps = screenCapture.Properties;
						screenCaptureProps.CaptureType = VideoCaptureType.GDI;
						screenCaptureProps.UseHardware = true;
						screenCaptureProps.Fps = 30;

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
			catch(Exception ex)
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
                catch(Exception ex)
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
               
                previewForm = new PreviewForm
                {
                    //ClientSize = VideoSettings.CaptureDevice.Resolution,
                    Size = previewSettings.PreviewSize,

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
            else if(captureDevice is UvcDevice)
            {
                var uvcDevice = (UvcDevice)captureDevice;

                description = resolution.Width + "x" + resolution.Height + " " + uvcDevice.CurrentProfile.FrameRate + " fps";
            }

            if (!string.IsNullOrEmpty(description))
            {
                title += " (" + description + ")";
            }
            previewForm.Text = title;

            bool fitVideoToWindow = previewSettings.FitVideoToWindow;

            previewForm.UpdateWindow(fitVideoToWindow, captureDevice.Resolution);
            previewForm.Visible = true;



        }

        private void OnVideoSourceStopped()
        {
            logger.Debug("OnVideoSourceStopped()");

            if (previewForm != null && !previewForm.IsDisposed)
            {
                previewForm.Visible = false;
            }

            videoSource.Close(true);
            videoSource.CaptureStarted -= VideoSource_CaptureStarted;
            videoSource.CaptureStopped -= VideoSource_CaptureStopped;

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


        private void LoadRateModeItems()
        {

            var items = new List<BitrateControlMode>
            {
               BitrateControlMode.CBR,
               BitrateControlMode.VBR,
               BitrateControlMode.Quality,

            };

            bitrateModeComboBox.DataSource = items;
        }

        private void LoadEncoderProfilesItems()
        {

            var items = new List<H264Profile>
            {
               H264Profile.Main,
               H264Profile.Base,
               H264Profile.High,

            };

            encProfileComboBox.DataSource = items;
        }

        private void LoadEncoderItems()
        {
            var items = new List<VideoEncoderMode>
            {
                VideoEncoderMode.H264,
                VideoEncoderMode.JPEG,
            };

            encoderComboBox.DataSource = items;

        }

        private void LoadCaptureTypes()
        {

            List<ComboBoxItem> captureTypes = new List<ComboBoxItem>();


            var captureProps = Config.Data.ScreenCaptureProperties;
            var caputreDevice = VideoSettings.CaptureDevice;

            var resolution = caputreDevice.Resolution;
            var propsStr = resolution.Width + "x" + resolution.Height + ", " + captureProps.Fps + "fps" + ", " + (captureProps.UseHardware ? "GPU" : "CPU");

            // var propsStr = resolution.Width + "x" + resolution.Height + (captureProps.UseHardware ? "GPU" : "CPU") + ", " + captureProps.Fps + " Fps";
            captureTypes.Add(new ComboBoxItem
            {
                Name = "Desktop Duplication API (" + propsStr + ")",
                Tag = VideoCaptureType.DXGIDeskDupl,
            });


            captureTypesComboBox.DataSource = captureTypes;
            captureTypesComboBox.DisplayMember = "Name";
            captureTypesComboBox.ValueMember = "Tag";


            //List<VideoCaptureType> captureTypes = new List<VideoCaptureType>();
            //captureTypes.Add(VideoCaptureType.DXGIDeskDupl);
            //captureTypes.Add(VideoCaptureType.GDI);
            ////captureTypes.Add(CaptureType.GDIPlus);
            //captureTypes.Add(VideoCaptureType.Direct3D9);
            //captureTypes.Add(VideoCaptureType.Datapath);

            //captureTypesComboBox.DataSource = captureTypes;

        }


    }
}
