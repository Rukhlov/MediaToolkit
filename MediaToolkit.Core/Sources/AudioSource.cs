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

        public IWaveIn Capture { get; private set; }

        public void Setup(string DeviceId)
        {
            logger.Debug("AudioSource::Setup(...) " + DeviceId);

            MMDevice device = null;
            try
            {
                using (var deviceEnum = new MMDeviceEnumerator())
                {
                    var mmDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
                    device = mmDevices.FirstOrDefault(d => d.ID == DeviceId);

                }

                if (device == null)
                {
                    //...
                    throw new Exception("MMDevice not found...");
                }

                if (device.DataFlow == DataFlow.Capture)
                {
                    Capture = new WasapiCapture(device);
                }
                else
                {
                    Capture = new WasapiLoopbackCapture(device);
                }

                Capture.DataAvailable += Capture_DataAvailable; ;
                Capture.RecordingStopped += Capture_RecordingStopped;



            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();

            }
            finally
            {
                if (device != null)
                {
                    device.Dispose();
                    device = null;
                }
            }
        }

        public void Start()
        {
            logger.Debug("AudioSource::Start(...)");

            Capture.StartRecording();

        }

        public event Action<byte[]> DataAvailable;

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded > 0)
            {
                byte[] data = new byte[e.BytesRecorded];
                Array.Copy(e.Buffer, data, data.Length);

                DataAvailable?.Invoke(data);

            }
        }


        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            logger.Debug("Capture_RecordingStopped(...)");
            
            var ex = e.Exception;
            if (ex != null)
            {
                logger.Error(ex);
            }

            Close();
        }



        public void Stop()
        {
            logger.Debug("AudioSource::Stop()");
            if (Capture != null)
            {
                Capture.StopRecording();
            }
        }

        public void Close()
        {
            logger.Debug("AudioSource::Close()");

            if (Capture != null)
            {
                Capture.DataAvailable -= Capture_DataAvailable;
                Capture.RecordingStopped -= Capture_RecordingStopped;
                Capture.Dispose();
                Capture = null;
            }

        }

    }


}
