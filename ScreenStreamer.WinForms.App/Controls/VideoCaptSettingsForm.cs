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
    public partial class VideoCaptSettingsForm : Form
    {
        public VideoCaptSettingsForm()
        {
            InitializeComponent();

			LoadCaptureTypes();

		}

        private ScreenCaptureDevice CaptDeviceSettings = null;

        public void Setup(VideoCaptureDevice captDeviceSettings)
        {
            CaptDeviceSettings = (ScreenCaptureDevice)captDeviceSettings;

            var captureProps = CaptDeviceSettings.Properties;

            captureMouseCheckBox.Checked = captureProps.CaptureMouse;

			var fps = captureProps.Fps;
			if (fps > fpsNumeric.Maximum)
			{
				fps = (int)fpsNumeric.Maximum;
			}
			else if(fps < fpsNumeric.Minimum)
			{
				fps = (int)fpsNumeric.Minimum;
			}

            var captureType = captureProps.CaptureType;

            var captureItem = captureTypes.FirstOrDefault(i => (VideoCaptureType)i.Tag == captureType) 
                ?? captureTypes.FirstOrDefault();

            captureTypesComboBox.SelectedItem = captureItem;

            fpsNumeric.Value = fps;

			showDebugInfoCheckBox.Checked = captureProps.ShowDebugInfo;
            showCaptureBorderCheckBox.Checked = captureProps.ShowDebugBorder;

            screenCaptureDetailsPanel.Visible = true;
        }



        private void applyButton_Click(object sender, EventArgs e)
        {
            CaptDeviceSettings.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;

            CaptDeviceSettings.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
            CaptDeviceSettings.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;

			CaptDeviceSettings.Properties.Fps = (int)fpsNumeric.Value;

            var captureItem = (ComboBoxItem)captureTypesComboBox.SelectedItem;
            var captureType = (VideoCaptureType)captureItem.Tag;

            CaptDeviceSettings.Properties.CaptureType = captureType;

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
				Name = "Desktop Duplication API",
				Tag = VideoCaptureType.DXGIDeskDupl,
			});

			captureTypes.Add(new ComboBoxItem
			{
				Name = "GDI",
				Tag = VideoCaptureType.GDI,
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

			captureTypesComboBox.DataSource = captureTypes;

		}

	}
}
