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

            LoadMMDevicesCombo();

            UpdateAudioControls();
        }


        private void audioUpdateButton_Click(object sender, EventArgs e)
        {

            logger.Debug("updateButton_Click(...)");
            LoadMMDevicesCombo();
        }


        private AudioStreamer audioStreamer = null;

        private void audioStartButton_Click(object sender, EventArgs e)
        {
            logger.Debug("audioStartButton_Click(...)");

            if(string.IsNullOrEmpty(currentMMDeviceId ))
            {
                logger.Warn("Empty MMDeviceId...");
                return;
            }

            //audioStreamer = new AudioStreamer();


            var audioParams = new AudioEncodingParams
            {
                SampleRate = 8000,
                Channels = 1,
                Encoding = "PCMU",
                DeviceId = currentMMDeviceId,

            };

            NetworkStreamingParams networkParams = audioSettings.NetworkParams;

            //NetworkStreamingParams networkParams = new NetworkStreamingParams
            //{
            //    LocalPort = audioSettings.Port,
            //    LocalAddr = "",
            //    RemoteAddr = audioSettings.Address,
            //    RemotePort = audioSettings.Port,
            //    TransportMode = audioSettings.TransportMode,
            //};

            if (audioStreamer != null)
            {
                audioStreamer.StateChanged -= AudioStreamer_StateChanged;
            }

            audioStreamer.SetWaveformPainter(new[] { this.waveformPainter1, this.waveformPainter2 });

            audioStreamer.StateChanged += AudioStreamer_StateChanged;
            audioStreamer.Setup(audioParams, networkParams);
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

            try
            {
                using (var deviceEnum = new MMDeviceEnumerator())
                {

                    var defaultCaptureId = "";
                    try
                    {
                        if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Capture, Role.Console))
                        {
                            var captureDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
                            if (captureDevice != null)
                            {
                               
                                defaultCaptureId = captureDevice.ID;
                                mmdevices.Add(captureDevice);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(ex);
                    }

                    var defaultRenderId = "";
                    try
                    {
                        if (deviceEnum.HasDefaultAudioEndpoint(DataFlow.Render, Role.Console))
                        {
                            var renderDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                            if (renderDevice != null)
                            {
                                defaultRenderId = renderDevice.ID;
                                mmdevices.Add(renderDevice);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }

                    try
                    {

                        var allDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
                        foreach (var d in allDevices)
                        {
                            if (d.ID == defaultRenderId || d.ID == defaultCaptureId)
                            {
                                continue;
                            }
                            mmdevices.Add(d);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }

            audioSrcComboBox.DataSource = mmdevices;
            audioSrcComboBox.DisplayMember = "FriendlyName";


        }


        private void UpdateAudioControls()
        {

            bool isStreaming = (audioStreamer != null && audioStreamer.IsStreaming);
            audioStartButton.Enabled = !isStreaming;
            audioStopButton.Enabled = isStreaming;

            settingPanel.Enabled = !isStreaming;
        }

        private AudioSettingsParams audioSettings = new AudioSettingsParams();
        private void settingButton_Click(object sender, EventArgs e)
        {
            //videoSettings.CaptureRegion = currentScreenRect;

            var f = new AudioSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,

            };
            f.Setup(audioSettings);

            f.ShowDialog();

            //currentScreenRect = videoSettings.CaptureRegion;
        }
    }



}
