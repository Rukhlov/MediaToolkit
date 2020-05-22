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

            this.Text = descr.Name;

            this.formatTextBox.Text = EncoderSettings.EncoderFormat.ToString();

            this.encProfileComboBox.SelectedItem = EncoderSettings.Profile;
            this.bitrateModeComboBox.SelectedItem = EncoderSettings.BitrateMode;
            this.MaxBitrateNumeric.Value = EncoderSettings.MaxBitrate;
            this.bitrateNumeric.Value = EncoderSettings.Bitrate;
            this.fpsNumeric.Value = EncoderSettings.FrameRate;
            this.latencyModeCheckBox.Checked = EncoderSettings.LowLatency;
            this.gopSizeNumeric.Value = EncoderSettings.GOPSize;

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

            EncoderSettings.GOPSize = (int)this.gopSizeNumeric.Value;

            this.Close();
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
