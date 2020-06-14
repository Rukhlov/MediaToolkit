using MediaToolkit;
using MediaToolkit.Core;
using MediaToolkit.UI;
using NLog;
using ScreenStreamer.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.WinForms.App
{
    public partial class AudioSettingsForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public AudioSettingsForm()
        {
            InitializeComponent();

            UpdateCaptureInfo();
            LoadEncoderItems();


            //LoadCaptureItems();

            // LoadTransportItems();

            syncContext = SynchronizationContext.Current;

        }

        public AudioStreamSettings AudioSettings { get; private set; }

        public void Setup(AudioStreamSettings audioSettings)
        {

            this.AudioSettings = audioSettings;
            var captureDevice = AudioSettings.CaptureDevice;

            textBoxDevice.Text = captureDevice.Name;
            this.captFormatTextBox.Text = captureDevice.Description;

            //this.addressTextBox.Text = AudioSettings.Address;
            //this.portNumeric.Value = AudioSettings.Port;
            //this.transportComboBox.SelectedItem = AudioSettings.TransportMode;

            var encoder = AudioSettings.EncoderSettings;
            this.sampleRateNumeric.Value = encoder.SampleRate;
            this.channelsNumeric.Value = encoder.Channels;

        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            //AudioSettings.Address = this.addressTextBox.Text;
            //AudioSettings.Port = (int)this.portNumeric.Value;

            //AudioSettings.TransportMode = (TransportMode)this.transportComboBox.SelectedItem;

            AudioSettings.EncoderSettings.SampleRate = (int)this.sampleRateNumeric.Value;
            AudioSettings.EncoderSettings.Channels = (int)this.channelsNumeric.Value;


            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (previewForm != null)
            {
                previewForm.Close();
                previewForm = null;
            }

            base.OnClosing(e);
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

        private void UpdateCaptureInfo()
        {
            captureTextBox.Text = "Windows Audio Session API";
        }

        private void captureSettingsButton_Click(object sender, EventArgs e)
        {
            var captDevice = AudioSettings.CaptureDevice;
        
            AudioCaptSettingsForm f = new AudioCaptSettingsForm
            {
                StartPosition = FormStartPosition.CenterParent,
            };

            f.Setup(AudioSettings.CaptureDevice);

            f.ShowDialog();

            UpdateCaptureInfo();

            
        }

        private SynchronizationContext syncContext = null; 
        private AudioSource audioSource = null;
        private AudioPreviewForm previewForm = null;
        private bool capturing = false;
        private void previewButton_Click(object sender, EventArgs e)
        {
            logger.Debug("previewButton_Click(...)");
            try
            {
                if (audioSource == null)
                {
                    audioSource = new AudioSource();
                    var captureDevice = AudioSettings.CaptureDevice;

                    var deviceId = captureDevice.DeviceId;
                    var captureProps = captureDevice.Properties;

                    audioSource.Setup(deviceId, captureProps);

                    audioSource.CaptureStarted += AudioSource_CaptureStarted;
                    audioSource.CaptureStopped += AudioSource_CaptureStopped;
                    audioSource.DataAvailable += AudioSource_DataAvailable;
                }

                if (capturing)
                {
                    audioSource.Stop();
                }
                else
                {

                    audioSource.Start();
                }

                this.Cursor = Cursors.WaitCursor;
                this.Enabled = false;


            }
            catch(Exception ex)
            {
                capturing = false;

                this.Cursor = Cursors.Default;
                this.Enabled = true;

                MessageBox.Show(ex.Message);

                CloseAudioSource();


            }



        }


        private void AudioSource_CaptureStarted()
        {
            logger.Debug("AudioSource_CaptureStarted()");

            capturing = true;

            syncContext.Send(_ =>
            {
                try
                {
                    if (previewForm != null && !previewForm.IsDisposed)
                    {
                        previewForm.FormClosed -= PreviewForm_FormClosed;
                        previewForm.Close();
                        
 
                    }

                    previewForm = null;

                    if (previewForm == null)
                    {
                        previewForm = new AudioPreviewForm
                        {
                            Size = new Size(640, 240),

                            StartPosition = FormStartPosition.CenterScreen,
                            Icon = ScreenStreamer.WinForms.App.Properties.Resources.logo,
                            FormBorderStyle = FormBorderStyle.FixedSingle,

                            //ShowIcon = false,
                        };

                        previewForm.FormClosed += PreviewForm_FormClosed;

                        previewForm.Setup(audioSource.WaveFormat);
                        var device = AudioSettings.CaptureDevice;

                        var text = device.Name + " " + device.Description;

                        previewForm.Text = text;
                        previewForm.Visible = true;
                    }


                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    this.Enabled = true;
                }


            }, null);

        }

        private void AudioSource_DataAvailable(byte[] data)
        {
            if (previewForm != null)
            {
                previewForm.AddData(data);
            }
        }

        private void AudioSource_CaptureStopped(object obj)
        {
            logger.Debug("AudioSource_CaptureStopped()");

            capturing = false ;

            syncContext.Send(_ =>
            {
                try
                {

                    if (previewForm != null && !previewForm.IsDisposed)
                    {
                        previewForm.FormClosed -= PreviewForm_FormClosed;

                        previewForm.Close();
                        previewForm = null;
                    }

                    captureSettingsButton.Enabled = true;
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    this.Enabled = true;
                }


            }, null);

            CloseAudioSource();
        }

        private void PreviewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            logger.Debug("PreviewForm_FormClosed(...)");
            previewForm.FormClosed -= PreviewForm_FormClosed;

            if (audioSource != null)
            {
                audioSource.Stop();
            }

        }

        private void CloseAudioSource()
        {
            logger.Debug("CloseAudioSource()");

            capturing = false;
            if (audioSource != null)
            {
                audioSource.CaptureStarted -= AudioSource_CaptureStarted;
                audioSource.CaptureStopped -= AudioSource_CaptureStopped;
                audioSource.DataAvailable -= AudioSource_DataAvailable;

                audioSource.Close(true);
                audioSource = null;

            }
        }

        //private void LoadCaptureItems()
        //{
        //    var items = new List<AudioCapturesTypes>
        //    {
        //        AudioCapturesTypes.Wasapi,
        //        AudioCapturesTypes.WasapiLoopback,
        //        AudioCapturesTypes.WaveIn,
        //    };

        //    captureComboBox.DataSource = items;
        //}

    }


}
