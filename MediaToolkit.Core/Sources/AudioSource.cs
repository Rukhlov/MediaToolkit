using MediaToolkit.Common;
using FFmpegLib;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.Compression;
using NAudio.Wave.SampleProviders;
using MediaToolkit.RTP;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Gui;

namespace MediaToolkit
{
    public class AudioSource
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        public AudioSource() { }

        private IWaveIn capture = null;
        private BufferedWaveProvider bufferedWaveProvider = null;

        private SampleChannel sampleChannel = null;

        private AudioEncoder audioResampler = null;
        private IRtpSender streamer = null;

        //public bool IsCapturing
        //{
        //    get
        //    {
        //        bool isCapturing = false;
        //        if (capture != null)
        //        {
        //            isCapturing = (capture.CaptureState == NAudio.CoreAudioApi.CaptureState.Capturing);
        //        }

        //        return IsCapturing;
        //    }
        //}

        public bool IsStreaming { get; private set; }
        private int sampleByteSize = 0;

        private WaveformPainter waveformPainter = null;
        public void SetWaveformPainter(WaveformPainter painter)
        {
            this.waveformPainter = painter;

        }

        public void Start(AudioEncodingParams outputParams, NetworkStreamingParams networkPars)
        {
            logger.Debug("AudioLoopbackSource::Start(...) ");

            //FileStream fs = new FileStream("d:\\test_audio_4", FileMode.Create, FileAccess.ReadWrite);
            Task.Run(() =>
            {
                try
                {
                    MMDevice device = null;

                    var deviceId = outputParams.DeviceId;
                    if (!string.IsNullOrEmpty(deviceId))
                    {

                    }
                    else
                    {
                        //...
                    }
                   
                    using (var deviceEnum = new MMDeviceEnumerator())
                    {
   
                        var mmDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
                        device = mmDevices.FirstOrDefault(d => d.ID == deviceId);

                    }

                    if(device == null)
                    {
                        //...
                        throw new Exception("MMDevice not found...");
                    }

                    if (device.DataFlow == DataFlow.Capture)
                    {
                        capture = new WasapiCapture(device);
                    }
                    else
                    {
                        capture = new WasapiLoopbackCapture(device);
                    }
                    

                    capture.DataAvailable += Capture_DataAvailable;
                    capture.RecordingStopped += Capture_RecordingStopped;

                    sampleByteSize = capture.WaveFormat.BitsPerSample / 8;

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

                    //streamer = new RtpStreamer(session);

                    streamer.Start(networkPars);


                    capture.StartRecording();

                    IsStreaming = true;
                    OnStateChanged();
                }
                catch(Exception ex)
                {
                    logger.Error(ex);

                    CleanUp();
                }
            });
        }

        private void SampleChannel_PreVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            var maxSample0 = e.MaxSampleValues[0];
            //if (e.MaxSampleValues.Count() > 1)
            //{
     
            //    var maxSample1 = e.MaxSampleValues[1];
            //    logger.Debug("MaxSampleValues" + maxSample0 + " " + maxSample1);
            //}
            //else
            //{
            //    logger.Debug("MaxSampleValues" + maxSample0);
            //}

            waveformPainter?.AddMax(maxSample0);
        }

        private uint rtpTimestamp = 0;
        private Stopwatch sw = new Stopwatch();

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (closing)
            {
                return;
            }

            if (e.BytesRecorded > 0)
            {

                bufferedWaveProvider.AddSamples(e.Buffer, 0, e.BytesRecorded);
                
                var audioBuffer = new float[e.BytesRecorded];

                sampleChannel.Read(audioBuffer, 0, e.BytesRecorded );


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

                    // streamer.Push(dest, ralativeTime);

                    streamer.Push(dest, ralativeTime);

                    //fs.Write(dest, 0, dest.Length);
                    //fs.Write(a.Buffer, 0, a.BytesRecorded);
                }
            }
        }

        //private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        //{
        //    if (closing)
        //    {
        //        return;
        //    }

        //    if (e.BytesRecorded > 0 )
        //    {

        //        byte[] src = new byte[e.BytesRecorded];
        //        Array.Copy(e.Buffer, src, src.Length);

        //        byte[] dest = null;
        //        audioResampler.Resample2(src, out dest);
        //        if (dest != null && dest.Length > 0)
        //        {
        //            //Debug.WriteLine("dest.Length " + dest.Length);

        //            rtpTimestamp += (uint)(sw.ElapsedMilliseconds * 8.0);
        //            sw.Restart();

        //            double ralativeTime = MediaTimer.GetRelativeTime();
        //            //uint rtpTime = (uint)(ralativeTime * 8000);

        //           // streamer.Push(dest, ralativeTime);

        //            streamer.Send(dest, ralativeTime);

        //            //fs.Write(dest, 0, dest.Length);
        //            //fs.Write(a.Buffer, 0, a.BytesRecorded);
        //        }
        //    }
        //}


        public event Action StateChanged;

        private void OnStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("AudioLoopbackSource::Capture_RecordingStopped(...)");

            CleanUp();

            IsStreaming = false;
            OnStateChanged();
        }

        private void CleanUp()
        {

            logger.Debug("AudioLoopbackSource::CleanUp()");

            if (streamer != null)
            {
                streamer.Close();
                streamer = null;
            }
            

            if (capture != null)
            {

                capture.DataAvailable -= Capture_DataAvailable;
                capture.RecordingStopped -= Capture_RecordingStopped;
                capture.Dispose();
                capture = null;
            }

            if (audioResampler != null)
            {
                audioResampler.Close();
                audioResampler = null;
            }

        }

        private bool closing = false;
        public void Close()
        {
            logger.Debug("AudioLoopbackSource::Close()");
            closing = true;
            if (capture != null)
            {
                capture.StopRecording();
            }
        }


    }


}
