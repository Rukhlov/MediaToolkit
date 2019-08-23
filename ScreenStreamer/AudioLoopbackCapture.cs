using CommonData;
using FFmpegWrapper;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.Compression;
using NAudio.Wave.SampleProviders;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    class AudioLoopbackSource
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public AudioLoopbackSource() { }

        private WasapiLoopbackCapture capture = null;
        private AudioEncoder audioResampler = null;
        private RtpStreamer streamer = null;

        public void Start(AudioEncodingParams outputParams)
        {
            //FileStream fs = new FileStream("d:\\test_audio_4", FileMode.Create, FileAccess.ReadWrite);
            Task.Run(() =>
            {
                capture = new WasapiLoopbackCapture();
                capture.DataAvailable += Capture_DataAvailable;
                capture.RecordingStopped += Capture_RecordingStopped;

                audioResampler = new AudioEncoder();

                var waveFormat = capture.WaveFormat;
                var captureParams = new AudioEncodingParams
                {
                    SampleRate = waveFormat.SampleRate,
                    Channels = waveFormat.Channels,

                };


                //audioResampler.Open(captureParams, outputParams);

                //PCMUSession session = new PCMUSession();

                //streamer = new RtpStreamer(session);
                //streamer.Open("239.0.0.1", 1236);


                capture.StartRecording();
            });
        }

        private uint rtpTimestamp = 0;
        private Stopwatch sw = new Stopwatch();
        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (closing)
            {
                return;
            }

            if (e.BytesRecorded > 0 )
            {
                byte[] src = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, src, src.Length);

                byte[] dest = null;
                audioResampler.Resample2(src, out dest);
                if (dest != null && dest.Length > 0)
                {
                    //Debug.WriteLine("dest.Length " + dest.Length);

                    rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 8.0);
                    sw.Restart();

                    double ralativeTime = MediaTimer.GetRelativeTime();
                    //uint rtpTime = (uint)(ralativeTime * 8000);

                    streamer.Send(dest, ralativeTime);

                    //fs.Write(dest, 0, dest.Length);
                    //fs.Write(a.Buffer, 0, a.BytesRecorded);
                }
            }
        }
        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {

            streamer?.Close();

            capture?.Dispose();
        }


        private bool closing = false;
        public void Close()
        {
            closing = true;
        }


    }


}
