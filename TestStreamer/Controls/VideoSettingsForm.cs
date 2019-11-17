using MediaToolkit;
using MediaToolkit.Common;
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

        public VideoSettingsParams VideoSettings { get; private set; }

        public void Setup(VideoSettingsParams settingsParams)
        {

            this.VideoSettings = settingsParams;
            //this.addressTextBox.Text = VideoSettings.Address;
            //this.portNumeric.Value = VideoSettings.Port;
            //this.transportComboBox.SelectedItem = VideoSettings.TransportMode;

            this.displayTextBox.Text = VideoSettings.DisplayName;

            this.CaptureRegion = VideoSettings.CaptureRegion;
            this.captureRegionTextBox.Text =  CaptureRegion.ToString();
            this.captureMouseCheckBox.Checked = VideoSettings.CaptureMouse;

            var resolution = VideoSettings.VideoResoulution;

            if (resolution.Width <= destWidthNumeric.Maximum && resolution.Width >= destWidthNumeric.Minimum)
            {
                this.destWidthNumeric.Value = resolution.Width;
            }
            if (resolution.Height <= destHeightNumeric.Maximum && resolution.Height >= destHeightNumeric.Minimum)
            {
                this.destHeightNumeric.Value = resolution.Height;
            }
                
            this.aspectRatioCheckBox.Checked = VideoSettings.AspectRatio;
            this.encoderComboBox.SelectedItem = VideoSettings.Encoder;
            this.encProfileComboBox.SelectedItem = VideoSettings.Profile;
            this.bitrateModeComboBox.SelectedItem = VideoSettings.BitrateMode;
            this.MaxBitrateNumeric.Value = VideoSettings.MaxBitrate;
            this.bitrateNumeric.Value = VideoSettings.Bitrate;
            this.fpsNumeric.Value = VideoSettings.Fps;
            this.latencyModeCheckBox.Checked = VideoSettings.LowLatency;

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            //VideoSettings.Address = this.addressTextBox.Text;
            //VideoSettings.Port = (int)this.portNumeric.Value;

            //VideoSettings.TransportMode = (TransportMode)this.transportComboBox.SelectedItem;

            VideoSettings.CaptureRegion = this.CaptureRegion;
            VideoSettings.CaptureMouse = this.captureMouseCheckBox.Checked;

            VideoSettings.VideoResoulution = new Size((int)this.destWidthNumeric.Value, (int)this.destHeightNumeric.Value);
            var resolution = VideoSettings.VideoResoulution;


            VideoSettings.AspectRatio = this.aspectRatioCheckBox.Checked;
            VideoSettings.Encoder = (VideoEncoderMode)this.encoderComboBox.SelectedItem;
            VideoSettings.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
            VideoSettings.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;

            VideoSettings.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
            VideoSettings.Bitrate = (int)this.bitrateNumeric.Value;

            VideoSettings.Fps = (int)this.fpsNumeric.Value;

            VideoSettings.LowLatency = this.latencyModeCheckBox.Checked;


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

            List<CaptureType> captureTypes = new List<CaptureType>();
            captureTypes.Add(CaptureType.DXGIDeskDupl);
            captureTypes.Add(CaptureType.GDI);
            //captureTypes.Add(CaptureType.GDIPlus);
            captureTypes.Add(CaptureType.Direct3D9);
            captureTypes.Add(CaptureType.Datapath);

            captureTypesComboBox.DataSource = captureTypes;
        }

        private Rectangle CaptureRegion = Rectangle.Empty;
        private SnippingTool snippingTool = new SnippingTool();
        private void snippingToolButton_Click(object sender, EventArgs e)
        {
            if (snippingTool != null)
            {
                snippingTool.Dispose();
            }

            snippingTool = new SnippingTool();
  

            var areaSelected = new Action<Rectangle, Rectangle>((a, s) =>
            {
                int left = a.Left + s.Left;
                int top = a.Top + s.Top;
                this.CaptureRegion = new Rectangle(left, top, a.Width, a.Height);

                this.captureRegionTextBox.Text =  CaptureRegion.ToString();

                //regionForm?.Close();

                //regionForm = new RegionForm(rect);
                //regionForm.Visible = true;

                //MessageBox.Show(a.ToString() + " " + s.ToString() + " " + rect.ToString());
            });

            //regionForm?.Close();
            //regionForm = null;
            snippingTool.Snip(VideoSettings.DisplayRegion, areaSelected);
        }
    }
}
