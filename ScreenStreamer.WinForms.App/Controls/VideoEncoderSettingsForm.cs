using MediaToolkit.Core;
using ScreenStreamer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms
{
    public partial class VideoEncoderSettingsForm : Form
    {
        public VideoEncoderSettingsForm()
        {
            InitializeComponent();

        }

        private VideoEncoderSettings EncoderSettings = null;

        public void Setup(VideoEncoderSettings encoderSettings, VideoEncoderDescription descr)
        {
            this.EncoderSettings = encoderSettings;

            LoadEncoderProfilesItems();

            LoadRateModeItems();

			LoadAspectRatioItems();

			this.Text = descr.Name;

            this.formatTextBox.Text = EncoderSettings.EncoderFormat.ToString();

            this.encProfileComboBox.SelectedItem = EncoderSettings.Profile;
            this.bitrateModeComboBox.SelectedItem = EncoderSettings.BitrateMode;
            this.MaxBitrateNumeric.Value = EncoderSettings.MaxBitrate;
            this.bitrateNumeric.Value = EncoderSettings.Bitrate;
            this.fpsNumeric.Value = EncoderSettings.FrameRate;
            this.latencyModeCheckBox.Checked = EncoderSettings.LowLatency;
			this.qualityNumeric.Value = EncoderSettings.Quality;
            this.gopSizeNumeric.Value = EncoderSettings.GOPSize;


			var aspectRatio = EncoderSettings.AspectRatio ?? AspectRatio.Default;
			var aspectItem = aspectRatios.FirstOrDefault(i => i.Width == aspectRatio.Width && i.Height == aspectRatio.Height);
			if(aspectItem == null)
			{
				aspectItem = aspectRatios.FirstOrDefault();
			}
			this.aspectRatioComboBox.SelectedItem = aspectItem;

		}

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();

        }

		private void applyButton_Click(object sender, EventArgs e)
		{

			EncoderSettings.Profile = (H264Profile)this.encProfileComboBox.SelectedItem;
			EncoderSettings.BitrateMode = (BitrateControlMode)this.bitrateModeComboBox.SelectedItem;
			EncoderSettings.MaxBitrate = (int)this.MaxBitrateNumeric.Value;
			EncoderSettings.Bitrate = (int)this.bitrateNumeric.Value;
			EncoderSettings.FrameRate = (int)this.fpsNumeric.Value;
			EncoderSettings.LowLatency = this.latencyModeCheckBox.Checked;

			EncoderSettings.Quality = (int)qualityNumeric.Value;
			EncoderSettings.GOPSize = (int)this.gopSizeNumeric.Value;

			AspectRatio aspectRatio = null;
			var item = aspectRatioComboBox.SelectedItem;
			if (item != null)
			{
				aspectRatio = item as AspectRatio;
			}

			EncoderSettings.AspectRatio = aspectRatio ?? AspectRatio.Default;

			this.Close();
        }

		private List<AspectRatio> aspectRatios = new List<AspectRatio>();
		private void LoadAspectRatioItems()
		{

			aspectRatios = new List<AspectRatio>
			{
			   AspectRatio.Default,
			   AspectRatio.AspectRatio_4_3,
			   AspectRatio.AspectRatio_5_4,
			   AspectRatio.AspectRatio_16_9,
			   AspectRatio.AspectRatio_16_10,
			};

			aspectRatioComboBox.DataSource = aspectRatios;
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

    }
}
