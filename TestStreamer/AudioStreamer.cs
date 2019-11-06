using FFmpegLib;
using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.RTP;
using MediaToolkit.Utils;
using NAudio.CoreAudioApi;
using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestStreamer
{

    public class AudioStreamer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public AudioStreamer(AudioSource source)
        {
            this.audioSource = source;
        }

        private AudioSource audioSource = null;

        private BufferedWaveProvider bufferedWaveProvider = null;

        private SampleChannel sampleChannel = null;

        private AudioEncoder audioResampler = null;

        private IRtpSender streamer = null;


        public bool IsStreaming { get; private set; }
        private int sampleByteSize = 0;

        private WaveformPainter[] wavePainters = null;

        //public void SetWaveformPainter(WaveformPainter painter)
        //{
        //    this.waveformPainter = painter;

        //}

        public void SetWaveformPainter(WaveformPainter[] wavePainters)
        {
            this.wavePainters = wavePainters;

        }

        public void Start(AudioEncodingParams outputParams, NetworkStreamingParams networkPars)
        {
            logger.Debug("AudioLoopbackSource::Start(...) ");

            //FileStream fs = new FileStream("d:\\test_audio_4", FileMode.Create, FileAccess.ReadWrite);
            Task.Run(() =>
            {
                try
                {
                    
                    var capture = audioSource.Capture;

                    bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat);
                    bufferedWaveProvider.DiscardOnBufferOverflow = true;

                    sampleChannel = new SampleChannel(bufferedWaveProvider);
                    sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;

                    audioResampler = new AudioEncoder();

                    var waveFormat = capture.WaveFormat;
                    var captureParams = new AudioEncodingParams
                    {
                        SampleRate = waveFormat.SampleRate,
                        Channels = waveFormat.Channels,

                    };


                    audioResampler.Open(captureParams, outputParams);

                    PCMUSession session = new PCMUSession();

                    if (networkPars.TransportMode == TransportMode.Tcp)
                    {
                        streamer = new RtpTcpSender(session);
                    }
                    else if (networkPars.TransportMode == TransportMode.Udp)
                    {
                        streamer = new RtpStreamer(session);
                    }
                    else
                    {
                        throw new FormatException("NotSupportedFormat " + networkPars.TransportMode);
                    }


                    audioSource.DataAvailable += AudioSource_DataAvailable;
                    streamer.Start(networkPars);
                   


                    IsStreaming = true;
                    OnStateChanged();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    Close();
                }
            });
        }

        private uint rtpTimestamp = 0;
        private Stopwatch sw = new Stopwatch();
        private void AudioSource_DataAvailable(byte[] data)
        {
            if (closing)
            {
                return;
            }

            if (data.Length> 0)
            {

                bufferedWaveProvider.AddSamples(data, 0, data.Length);

                var audioBuffer = new float[data.Length];

                sampleChannel.Read(audioBuffer, 0, data.Length);

                byte[] dest = null;
                audioResampler.Resample2(data, out dest);
                if (dest != null && dest.Length > 0)
                {
                    //Debug.WriteLine("dest.Length " + dest.Length);

                    rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 8.0);
                    sw.Restart();

                    double ralativeTime = MediaTimer.GetRelativeTime();
                    //uint rtpTime = (uint)(ralativeTime * 8000);

                    // streamer.Push(dest, ralativeTime);

                    streamer.Push(dest, ralativeTime);

                    //fs.Write(dest, 0, dest.Length);
                    //fs.Write(a.Buffer, 0, a.BytesRecorded);
                }
            }
        }

        private void SampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            if (wavePainters == null)
            {
                return;
            }

            var samples = e.MaxSampleValues;

            for (int i = 0; i < samples.Length; i++)
            {
                var s = samples[i];

                if (i < wavePainters.Length)
                {
                    var painter = wavePainters[i];

                    painter?.AddMax(s);
                }
            }
        }

        public event Action StateChanged;

        private void OnStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("AudioLoopbackSource::Capture_RecordingStopped(...)");

            Close();

            IsStreaming = false;
            OnStateChanged();
        }

        public void Close()
        {
            logger.Debug("AudioLoopbackSource::Close()");
            closing = true;

            if (streamer != null)
            {
                streamer.Close();
                streamer = null;
            }

            if (audioSource != null)
            {
                audioSource.DataAvailable += AudioSource_DataAvailable;
            }

            if (audioResampler != null)
            {
                audioResampler.Close();
                audioResampler = null;
            }

        }

        private bool closing = false;



    }

}
