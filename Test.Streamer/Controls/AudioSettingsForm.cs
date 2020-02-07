using MediaToolkit;
using MediaToolkit.Core;
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
    public partial class AudioSettingsForm : Form
    {
        public AudioSettingsForm()
        {
            InitializeComponent();


            LoadEncoderItems();
            LoadCaptureItems();

           // LoadTransportItems();

        }

        public AudioStreamSettings AudioSettings { get; private set; }

        public void Setup(AudioStreamSettings settingsParams)
        {

            this.AudioSettings = settingsParams;
            var captureSettings = AudioSettings.CaptureParams;

            textBoxDevice.Text = captureSettings.Name;
            this.captFormatTextBox.Text = captureSettings.Description;

            //this.addressTextBox.Text = AudioSettings.Address;
            //this.portNumeric.Value = AudioSettings.Port;
            //this.transportComboBox.SelectedItem = AudioSettings.TransportMode;

            var encoder = AudioSettings.EncodingParams;
            this.sampleRateNumeric.Value = encoder.SampleRate;
            this.channelsNumeric.Value = encoder.Channels;

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            //AudioSettings.Address = this.addressTextBox.Text;
            //AudioSettings.Port = (int)this.portNumeric.Value;

            //AudioSettings.TransportMode = (TransportMode)this.transportComboBox.SelectedItem;

            AudioSettings.EncodingParams.SampleRate = (int)this.sampleRateNumeric.Value;
            AudioSettings.EncodingParams.Channels = (int)this.channelsNumeric.Value;


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

        private void LoadEncoderItems()
        {
            var items = new List<AudioEncoderMode>
            {
                AudioEncoderMode.G711,
                AudioEncoderMode.AAC,
            };

            encoderComboBox.DataSource = items;
        }


        private void LoadCaptureItems()
        {
            var items = new List<AudioCapturesTypes>
            {
                AudioCapturesTypes.Wasapi,
                AudioCapturesTypes.WasapiLoopback,
                AudioCapturesTypes.WaveIn,
            };

            captureComboBox.DataSource = items;
        }

    }


}
