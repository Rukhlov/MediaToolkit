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
    public partial class AudioSettingsForm : Form
    {
        public AudioSettingsForm()
        {
            InitializeComponent();


            LoadEncoderItems();
           // LoadTransportItems();

        }

        public AudioSettingsParams AudioSettings { get; private set; }

        public void Setup(AudioSettingsParams settingsParams)
        {

            this.AudioSettings = settingsParams;
            //this.addressTextBox.Text = AudioSettings.Address;
            //this.portNumeric.Value = AudioSettings.Port;
            //this.transportComboBox.SelectedItem = AudioSettings.TransportMode;

            this.sampleRateNumeric.Value = AudioSettings.Samplerate;
            this.channelsNumeric.Value = AudioSettings.Channels;

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            //AudioSettings.Address = this.addressTextBox.Text;
            //AudioSettings.Port = (int)this.portNumeric.Value;

            //AudioSettings.TransportMode = (TransportMode)this.transportComboBox.SelectedItem;

            AudioSettings.Samplerate = (int)this.sampleRateNumeric.Value;
            AudioSettings.Channels = (int)this.channelsNumeric.Value;


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
    }
}
