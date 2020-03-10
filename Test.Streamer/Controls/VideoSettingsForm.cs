using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestStreamer.Controls
{
    public partial class VideoSettingsForm : Form
    {
        public VideoSettingsForm()
        {
            InitializeComponent();

            LoadEncoderProfilesItems();

            //LoadTransportItems();
            LoadEncoderItems();

            LoadRateModeItems();

            LoadCaptureTypes();
        }

        public VideoStreamSettings VideoSettings { get; private set; }


        public void Setup(VideoStreamSettings settingsParams)
        {

            this.VideoSettings = settingsParams;

            //this.addressTextBox.Text = VideoSettings.Address;
            //this.portNumeric.Value = VideoSettings.Port;
            //this.transportComboBox.SelectedItem = VideoSettings.TransportMode;

            var screenCaptureParams = VideoSettings.CaptureDescription as ScreenCaptureDeviceDescription;
            if (screenCaptureParams != null)
            {
               // var captureDescr = screenCaptureParams.CaptureDescription;
                this.displayTextBox.Text = screenCaptureParams.DisplayName;

                this.CaptureRegion = screenCaptureParams.CaptureRegion;
                this.captureMouseCheckBox.Checked = screenCaptureParams.Properties.CaptureMouse;

                this.aspectRatioCheckBox.Checked = screenCaptureParams.Properties.AspectRatio;

                WebCamGroup.Visible = false;
                ScreenCaptureGroup.Visible = true;

                adjustAspectRatioButton.Visible = true;

                showDebugInfoCheckBox.Checked = screenCaptureParams.Properties.ShowDebugInfo;
                showCaptureBorderCheckBox.Checked = screenCaptureParams.Properties.ShowCaptureBorder;

                if (screenCaptureParams.DisplayRegion.IsEmpty)
                {
                    displayTextBox.Visible = false;
                    labelDisplay.Visible = false;
                }

            }

            var webCamCaptureParams = VideoSettings.CaptureDescription as VideoCaptureDeviceDescription;
            if (webCamCaptureParams != null)
            {
                CaptureDeviceTextBox.Text = webCamCaptureParams.Name;

                var profile = webCamCaptureParams?.CurrentProfile;
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

                WebCamGroup.Visible = true;
                ScreenCaptureGroup.Visible = false;

                adjustAspectRatioButton.Visible = false;
            }

            

            this.captureRegionTextBox.Text =  CaptureRegion.ToString();

            var videoEncoderPars = VideoSettings.EncodingParams;

            var resolution = videoEncoderPars.Resolution;

            if (resolution.Width <= destWidthNumeric.Maximum && resolution.Width >= destWidthNumeric.Minimum)
            {
                this.destWidthNumeric.Value = resolution.Width;
            }
            if (resolution.Height <= destHeightNumeric.Maximum && resolution.Height >= destHeightNumeric.Minimum)
            {
                this.destHeightNumeric.Value = resolution.Height;
            }


            this.checkBoxResoulutionFromSource.Checked = videoEncoderPars.UseResoulutionFromSource;

            this.encoderComboBox.SelectedItem = videoEncoderPars.Encoder;
            this.encProfileComboBox.SelectedItem = videoEncoderPars.Profile;
            this.bitrateModeComboBox.SelectedItem = videoEncoderPars.BitrateMode;
            this.MaxBitrateNumeric.Value = videoEncoderPars.MaxBitrate;
            this.bitrateNumeric.Value = videoEncoderPars.Bitrate;
            this.fpsNumeric.Value = videoEncoderPars.FrameRate;
            this.latencyModeCheckBox.Checked = videoEncoderPars.LowLatency;

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            //VideoSettings.Address = this.addressTextBox.Text;
            //VideoSettings.Port = (int)this.portNumeric.Value;

            //VideoSettings.TransportMode = (TransportMode)this.transportComboBox.SelectedItem;

            var screenCaptureParams = VideoSettings.CaptureDescription as ScreenCaptureDeviceDescription;
            if (screenCaptureParams != null)
            {
                screenCaptureParams.CaptureRegion = this.CaptureRegion;
                screenCaptureParams.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;
                screenCaptureParams.Properties.AspectRatio = this.aspectRatioCheckBox.Checked;

                screenCaptureParams.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
                screenCaptureParams.Properties.ShowCaptureBorder = showCaptureBorderCheckBox.Checked;
            }

            var deviceDescr = VideoSettings.CaptureDescription as VideoCaptureDeviceDescription;
            if (deviceDescr != null)
            {

            }

            VideoSettings.EncodingParams.UseResoulutionFromSource = this.checkBoxResoulutionFromSource.Checked;

            if (!VideoSettings.EncodingParams.UseResoulutionFromSource)
            {
                int width = (int)this.destWidthNumeric.Value;
                int height = (int)this.destHeightNumeric.Value;

                if (width % 2 != 0)
                {
                    width--;
                }

                if (height % 2 != 0)
                {
                    height--;
                }

                VideoSettings.EncodingParams.Resolution = new Size(width, height);
            }


            VideoSettings.EncodingParams.Encoder = (VideoEncoderMode)this.encoderComboBox.SelectedItem;
            VideoSettings.EncodingParams.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
            VideoSettings.EncodingParams.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;

            VideoSettings.EncodingParams.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
            VideoSettings.EncodingParams.Bitrate = (int)this.bitrateNumeric.Value;

            VideoSettings.EncodingParams.FrameRate = (int)this.fpsNumeric.Value;

            VideoSettings.EncodingParams.LowLatency = this.latencyModeCheckBox.Checked;


            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //private void LoadTransportItems()
        //{
        //    var items = new List<TransportMode>
        //    {
        //        TransportMode.Tcp,
        //        TransportMode.Udp,
        //    };
        //    transportComboBox.DataSource = items;
        //}

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

            List<VideoCaptureType> captureTypes = new List<VideoCaptureType>();
            captureTypes.Add(VideoCaptureType.DXGIDeskDupl);
            captureTypes.Add(VideoCaptureType.GDI);
            //captureTypes.Add(CaptureType.GDIPlus);
            captureTypes.Add(VideoCaptureType.Direct3D9);
            captureTypes.Add(VideoCaptureType.Datapath);

            captureTypesComboBox.DataSource = captureTypes;
        }

        private Rectangle CaptureRegion = Rectangle.Empty;

        private void snippingToolButton_Click(object sender, EventArgs e)
        {
            var captureDescr = VideoSettings.CaptureDescription as ScreenCaptureDeviceDescription;
            if (captureDescr == null)
            {
                return;
            }

            var selectedRegion = SnippingTool.Snip(captureDescr.DisplayRegion);
            if (!selectedRegion.IsEmpty)
            {
                this.CaptureRegion = selectedRegion;

                this.captureRegionTextBox.Text = CaptureRegion.ToString();
            }
            else
            {// error, cancelled...

            }

        }

        private void adjustAspectRatioButton_Click(object sender, EventArgs e)
        {
            var srcSize = new Size (this.CaptureRegion.Width, this.CaptureRegion.Height);
            var destSize= new Size((int)this.destWidthNumeric.Value, (int)this.destHeightNumeric.Value);

  
            var ratio = srcSize.Width / (double)srcSize.Height;
            int destWidth = destSize.Width;
            int destHeight = (int)(destWidth / ratio);
            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);
            }

            this.destWidthNumeric.Value = destWidth;
            this.destHeightNumeric.Value = destHeight;
            
        }

        private void showDebugInfoCheckBox_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxResoulutionFromSource_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxResoulutionFromSource.Checked)
            {
                panelEncoderResoulution.Enabled = false;
                aspectRatioCheckBox.Enabled = false;
            }
            else
            {
                panelEncoderResoulution.Enabled = true;
                aspectRatioCheckBox.Enabled = true;
            }
        }
    }
}
