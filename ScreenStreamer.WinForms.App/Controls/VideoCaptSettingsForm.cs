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

			

		}

        private ScreenCaptureDevice ScreenCaptSettings = null;


        private WindowCaptureDevice WindowsCaptSettings = null;

        public void Setup(VideoCaptureDevice captDeviceSettings)
        {

            if(captDeviceSettings.CaptureMode == CaptureMode.Screen)
            {
                

                ScreenCaptSettings = (ScreenCaptureDevice)captDeviceSettings;
                LoadCaptureTypes();


                var captureProps = ScreenCaptSettings.Properties;


                captureMouseCheckBox.Checked = captureProps.CaptureMouse;

                var fps = captureProps.Fps;
                if (fps > fpsNumeric.Maximum)
                {
                    fps = (int)fpsNumeric.Maximum;
                }
                else if (fps < fpsNumeric.Minimum)
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
            else if (captDeviceSettings.CaptureMode == CaptureMode.AppWindow)
            {
               

                WindowsCaptSettings = (WindowCaptureDevice)captDeviceSettings;

                LoadCaptureTypes();

                var captureProps = WindowsCaptSettings.Properties;

                captureMouseCheckBox.Checked = captureProps.CaptureMouse;

                var fps = captureProps.Fps;
                if (fps > fpsNumeric.Maximum)
                {
                    fps = (int)fpsNumeric.Maximum;
                }
                else if (fps < fpsNumeric.Minimum)
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

        }



        private void applyButton_Click(object sender, EventArgs e)
        {
            if (ScreenCaptSettings != null)
            {
                ScreenCaptSettings.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;

                ScreenCaptSettings.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
                ScreenCaptSettings.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;

                ScreenCaptSettings.Properties.Fps = (int)fpsNumeric.Value;

                var captureItem = (ComboBoxItem)captureTypesComboBox.SelectedItem;
                var captureType = (VideoCaptureType)captureItem.Tag;

                ScreenCaptSettings.Properties.CaptureType = captureType;

            }

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

            if (ScreenCaptSettings != null)
            {
                captureTypes.Add(new ComboBoxItem
                {
                    Name = "Desktop Duplication API",
                    Tag = VideoCaptureType.DXGIDeskDupl,
                });
            }


			captureTypes.Add(new ComboBoxItem
			{
				Name = "GDI Layered",
				Tag = VideoCaptureType.GDILayered,
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
