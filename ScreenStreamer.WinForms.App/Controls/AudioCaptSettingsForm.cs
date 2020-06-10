using MediaToolkit.Core;
using ScreenStreamer.Common;
using ScreenStreamer.WinForms.App;
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
    public partial class AudioCaptSettingsForm : Form
    {
        public AudioCaptSettingsForm()
        {
            InitializeComponent();

			LoadCaptureTypes();

		}

        private AudioCaptureDevice CaptDeviceSettings = null;
        private WasapiCaptureProperties captureProps = null;

        public void Setup(AudioCaptureDevice captDeviceSettings)
        {
            CaptDeviceSettings = captDeviceSettings;

            captureProps = (WasapiCaptureProperties)CaptDeviceSettings.Properties;

            exclusiveModeCheckBox.Checked = captureProps.ExclusiveMode;

			var bufferMsec = captureProps.BufferMilliseconds;
			if (bufferMsec > bufferSizeNumeric.Maximum)
			{
				bufferMsec = (int)bufferSizeNumeric.Maximum;
			}
			else if(bufferMsec < bufferSizeNumeric.Minimum)
			{
				bufferMsec = (int)bufferSizeNumeric.Minimum;
			}
            bufferSizeNumeric.Value = bufferMsec;
            

			eventSyncModeCheckBox.Checked = captureProps.EventSyncMode;
            //showCaptureBorderCheckBox.Checked = captureProps.ShowDebugBorder;


        }



        private void applyButton_Click(object sender, EventArgs e)
        {

            captureProps.ExclusiveMode = exclusiveModeCheckBox.Checked;
            captureProps.EventSyncMode = eventSyncModeCheckBox.Checked;
            captureProps.BufferMilliseconds = (int)bufferSizeNumeric.Value;

            //var captureItem = (ComboBoxItem)captureTypesComboBox.SelectedItem;
            //var captureType = (AudioCapturesTypes)captureItem.Tag;



            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {


            this.Close();

        }

        private List<ComboBoxItem> captureTypes = new List<ComboBoxItem>();
        private void LoadCaptureTypes()
		{

            captureTypes.Clear();

            captureTypes.Add(new ComboBoxItem
			{
				Name = "Windows Audio Session API",
				Tag = AudioCapturesTypes.Wasapi,
			});

			captureTypesComboBox.DataSource = captureTypes;
			captureTypesComboBox.DisplayMember = "Name";
			captureTypesComboBox.ValueMember = "Tag";

			captureTypesComboBox.DataSource = captureTypes;


            captureTypesComboBox.SelectedItem = captureTypes.FirstOrDefault();

            //captureTypesComboBox.Enabled = false;
        }

	}
}
