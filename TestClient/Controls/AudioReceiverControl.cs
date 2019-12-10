using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using MediaToolkit.Common;
using MediaToolkit.Core;

namespace TestClient.Controls
{
    public partial class AudioReceiverControl : UserControl
    {
        public AudioReceiverControl()
        {
            InitializeComponent();

            LoadMMDevicesCombo();

            LoadTransportItems();
        }

        private AudioReceiver audioReceiver = null;

        private void audioPlayButton_Click(object sender, EventArgs e)
        {
            audioReceiver = new AudioReceiver();
            var addr = audioAddrTextBox.Text;
            var port = (int)audioPortNumeric.Value;

            var transport = (TransportMode)transportComboBox.SelectedItem;

            var sampleRate = (int)sampleRateNumeric.Value;
            var channels = (int)channelsNumeric.Value;
            var networkPars = new NetworkSettings
            {
                LocalAddr = addr,
                LocalPort = port,
                TransportMode = transport,

            };

            var audioPars = new AudioEncoderSettings
            {
                SampleRate = sampleRate,
                Channels = channels,
                Encoding = "ulaw",
                DeviceId = currentDirectSoundDeviceInfo?.Guid.ToString() ?? "",
            };

            audioReceiver.SetWaveformPainter(this.waveformPainter1);

            audioReceiver.Setup(audioPars, networkPars);
            audioReceiver.Play();

        }

        private void audioStopButton_Click(object sender, EventArgs e)
        {
            audioReceiver?.Stop();
        }

        private DirectSoundDeviceInfo currentDirectSoundDeviceInfo = null;

        private void audioUpdateButton_Click(object sender, EventArgs e)
        {
            LoadMMDevicesCombo();
        }

        private void LoadMMDevicesCombo()
        {


            //List<MMDevice> mmdevices = new List<MMDevice>();

            //using (var deviceEnum = new MMDeviceEnumerator())
            //{

            //    var renderDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToList();
            //    var defaultDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            //    if (defaultDevice != null)
            //    {
            //        mmdevices.Add(defaultDevice);
            //        foreach(var device in renderDevices)
            //        {
            //            if(device.ID == defaultDevice.ID)
            //            {
            //                continue;
            //            }
            //            mmdevices.Add(device);
            //        }
            //    }
            //    else
            //    {

            //        mmdevices.AddRange(renderDevices);
            //    }

            //}

            IEnumerable<DirectSoundDeviceInfo> devices = null;
            try
            {
                devices = DirectSoundOut.Devices;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            audioRenderComboBox.DataSource = devices;
            audioRenderComboBox.DisplayMember = "Description";


        }

        private void LoadTransportItems()
        {

            var items = new List<TransportMode>
            {
                TransportMode.Udp,
                TransportMode.Tcp,

            };
            transportComboBox.DataSource = items;
        }

        private void audioRenderComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var selectedItem = audioRenderComboBox.SelectedItem;
            if (selectedItem != null)
            {
                currentDirectSoundDeviceInfo = selectedItem as DirectSoundDeviceInfo;

            }
        }


        private void volumeSlider_VolumeChanged(object sender, EventArgs e)
        {
            if (audioReceiver != null)
            {
                float volume = volumeSlider.Volume;
                audioReceiver.Volume = volume;
            }

        }
    }
}
