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
    public partial class VideoCaptSettingsForm : Form
    {
        public VideoCaptSettingsForm()
        {
            InitializeComponent();

        }

        private ScreenCaptureDevice CaptDeviceSettings = null;

        public void Setup(VideoCaptureDevice captDeviceSettings)
        {
            CaptDeviceSettings = (ScreenCaptureDevice)captDeviceSettings;

            var captureProps = CaptDeviceSettings.Properties;

            captureMouseCheckBox.Checked = captureProps.CaptureMouse;
            //aspectRatioCheckBox.Checked = captureProps.AspectRatio;
            showDebugInfoCheckBox.Checked = captureProps.ShowDebugInfo;
            showCaptureBorderCheckBox.Checked = captureProps.ShowDebugBorder;

            screenCaptureDetailsPanel.Visible = true;
        }



        private void applyButton_Click(object sender, EventArgs e)
        {
            CaptDeviceSettings.Properties.CaptureMouse = this.captureMouseCheckBox.Checked;
            //screenCaptureParams.Properties.AspectRatio = this.aspectRatioCheckBox.Checked;
            CaptDeviceSettings.Properties.ShowDebugInfo = showDebugInfoCheckBox.Checked;
            CaptDeviceSettings.Properties.ShowDebugBorder = showCaptureBorderCheckBox.Checked;


            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {


            this.Close();

        }

    }
}
