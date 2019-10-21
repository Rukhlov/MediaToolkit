using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MediaToolkit;
using MediaToolkit.Common;
using NLog;
using NAudio.CoreAudioApi;

namespace TestStreamer.Controls
{
    public partial class AudioStreamerControl : UserControl
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AudioStreamerControl()
        {
            InitializeComponent();


            LoadTransportItems();
            LoadEncoderItems();
            LoadMMDevicesCombo();

            UpdateAudioControls();
        }


        private void audioUpdateButton_Click(object sender, EventArgs e)
        {

            logger.Debug("updateButton_Click(...)");
            LoadMMDevicesCombo();
        }


        private AudioLoopbackSource audioStreamer = null;

        private void audioStartButton_Click(object sender, EventArgs e)
        {
            logger.Debug("audioStartButton_Click(...)");
            audioStreamer = new AudioLoopbackSource();

            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",
                DeviceId = currentMMDeviceId,

            };

            var addr = audioAddrTextBox.Text;
            var port = (int)audioPortNumeric.Value;

            NetworkStreamingParams networkParams = new NetworkStreamingParams
            {
                LocalAddr = "",
                RemoteAddr = addr,
                RemotePort = port,
            };

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            audioStreamer.SetWaveformPainter(this.waveformPainter1);
            audioStreamer.StateChanged += AudioStreamer_StateChanged;
            audioStreamer.Start(audioParams, networkParams);
        }

        private void audioStopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("audioStopButton_Click(...)");

            if (audioStreamer != null)
            {
                audioStreamer.SetWaveformPainter(null);
                audioStreamer.Close();
            }
        }


        private void AudioStreamer_StateChanged()
        {
            if (audioStreamer.IsStreaming)
            {

            }
            else
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            this.Invoke((Action)(() =>
            {
                UpdateAudioControls();
            }));
        }

        private string currentMMDeviceId = "";

        private void audioSrcComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var item = audioSrcComboBox.SelectedItem;
            if (item != null)
            {
                var device = item as MMDevice;
                if (device != null)
                {
                    currentMMDeviceId = device.ID;

                    //MessageBox.Show(device.FriendlyName + " " + device.DataFlow);
                }
            }
        }

        private void LoadMMDevicesCombo()
        {
            List<MMDevice> mmdevices = new List<MMDevice>();

            using (var deviceEnum = new MMDeviceEnumerator())
            {
                var captureDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                var renderDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);

                mmdevices.Add(captureDevice);
                mmdevices.Add(renderDevice);

                //var captureDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
                //mmdevices.AddRange(captureDevices);

            }

            audioSrcComboBox.DataSource = mmdevices;
            audioSrcComboBox.DisplayMember = "FriendlyName";


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

        private void LoadEncoderItems()
        {
            var items = new List<AudioEncoderMode>
            {
                AudioEncoderMode.G711,
                AudioEncoderMode.AAC,
            };

            encoderComboBox.DataSource = items;

        }

        private void UpdateAudioControls()
        {

            bool isStreaming = (audioStreamer != null && audioStreamer.IsStreaming);
            audioStartButton.Enabled = !isStreaming;
            audioStopButton.Enabled = isStreaming;

            settingPanel.Enabled = !isStreaming;
        }
    }
}
