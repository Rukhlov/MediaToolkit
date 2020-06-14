using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.UI
{
    public partial class AudioPreviewForm : Form
    {
        public AudioPreviewForm()
        {
            InitializeComponent();
        }

        private BufferedWaveProvider bufferedWaveProvider = null;
        private SampleChannel sampleChannel = null;
        private WaveFormat waveFormat = null;

        public void Setup(short tag, int sampleRate, int channels, int avgBytesPerSec, int blockAlign, int bitPerSample)
        {
            var format = WaveFormat.CreateCustomFormat((WaveFormatEncoding)tag, sampleRate, channels, avgBytesPerSec, blockAlign, bitPerSample);

            Setup(format);

        }

        public void Setup(WaveFormat waveFormat)
        {
            this.waveFormat = waveFormat;
            bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;

            sampleChannel = new SampleChannel(bufferedWaveProvider);

            sampleChannel.PreVolumeMeter += OnPreVolumeMeter;

            SetupControls();

        }

        private List<WaveformPainter> waveformPainters = new List<WaveformPainter>();
        private void SetupControls()
        {
            this.Controls.Clear();

            var channels = waveFormat.Channels;
            if (channels > 0)
            {
                TableLayoutPanel table = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                };

                table.RowCount = channels;
                for(int ch = 0; ch < channels; ch++)
                {
                    var wavePainter = new WaveformPainter
                    {
                        Tag = ch,
                        Dock = DockStyle.Fill,
                        BackColor = System.Drawing.SystemColors.Info,
                    };

                    waveformPainters.Add(wavePainter);

                    table.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                    table.Controls.Add(wavePainter, 0, ch);

                }

               
                this.Controls.Add(table);

                //this.AutoSize = true;
            }
            


        }

        private void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {

            var sampleValues = e.MaxSampleValues;
            var channels = sampleValues.Length;

            Debug.Assert(channels == waveformPainters.Count, "channels == waveformPainters.Count");

            if (channels == waveformPainters.Count)
            {
                for(int ch = 0; ch< channels; ch++)
                {
                    var painter = waveformPainters[ch];
                    var sampleVal = e.MaxSampleValues[ch];
                    painter.AddMax(sampleVal);
                }
            }

        }

        public void AddData(byte[] data)
        {
            bufferedWaveProvider.AddSamples(data, 0, data.Length);

            var temp = new float[data.Length];

            var samplesNum = sampleChannel.Read(temp, 0, temp.Length);

        }
    }
}
