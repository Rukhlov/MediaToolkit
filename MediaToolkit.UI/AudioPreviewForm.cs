using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public void Setup(WaveFormat waveFormat)
        {
            this.waveFormat = waveFormat;

            bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;

            sampleChannel = new SampleChannel(bufferedWaveProvider);

            sampleChannel.PreVolumeMeter += OnPreVolumeMeter;
        }

        private void OnPreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            var sampleValues = e.MaxSampleValues;
            if(sampleValues.Length == 2)
            {
                waveformPainter1.AddMax(e.MaxSampleValues[0]);
                waveformPainter2.AddMax(e.MaxSampleValues[1]);
            }
            else if (sampleValues.Length == 1)
            {
                waveformPainter1.AddMax(e.MaxSampleValues[0]);
            }

        }

        public void AddData(byte[] data)
        {
            bufferedWaveProvider.AddSamples(data, 0, data.Length);

            var audioBuffer = new float[data.Length];

            sampleChannel.Read(audioBuffer, 0, data.Length);

        }
    }
}
