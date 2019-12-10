using FFmpegLib;
using MediaToolkit.Common;
using MediaToolkit.RTP;
using NAudio.Codecs;
using NAudio.Gui;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Core
{
    public class AudioReceiver
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private PCMUSession session = null;
        private IRtpReceiver rtpReceiver = null;


        private IWavePlayer wavePlayer;

        private BufferedWaveProvider waveBuffer = null;
        private VolumeSampleProvider volumeProvider = null;
        private WaveFormat waveFormat = null;

        private volatile bool bufferLost = false;

        private AudioDecoder decoder = null;
         

        public void Setup(AudioEncoderSettings inputPars, NetworkSettings networkPars)
        {
            logger.Debug("AudioReceiver::Setup(...)");

            try
            {
                decoder = new AudioDecoder();

                decoder.Open(inputPars);

                waveFormat = new WaveFormat(8000, 16, 1);

                var _deviceId = inputPars.DeviceId;

                Guid deviceId = Guid.Empty;
                if (!string.IsNullOrEmpty(_deviceId))
                {
                    Guid.TryParse(_deviceId, out deviceId);
                }

                DirectSoundDeviceInfo deviceInfo = null;
                var DSDevices = DirectSoundOut.Devices;
                if (DSDevices != null && DSDevices.Count() > 0)
                {
                    //DirectSoundOut.DSDEVID_DefaultPlayback
                    deviceInfo = DSDevices.FirstOrDefault(d => d.Guid == deviceId) ?? DSDevices.FirstOrDefault();
                }

                if(deviceId == null)
                {
                    throw new Exception("Audio device not found...");
                }


                if (deviceInfo != null)
                {
                    logger.Info(deviceInfo.Description + " " + deviceInfo.ModuleName + " " + deviceInfo.Guid);

                    wavePlayer = new NAudio.Wave.DirectSoundOut(deviceInfo.Guid);

                    wavePlayer.PlaybackStopped += WavePlayer_PlaybackStopped;

  
 
                    waveBuffer = new BufferedWaveProvider(waveFormat)
                    {
                        BufferDuration = TimeSpan.FromMilliseconds(300),
                        DiscardOnBufferOverflow = true
                    };

                    volumeProvider = new VolumeSampleProvider(waveBuffer.ToSampleProvider());
                    
                    var meteringSampleProvider = new MeteringSampleProvider(volumeProvider);

                    meteringSampleProvider.StreamVolume += PostVolumeMeter_StreamVolume;

                    wavePlayer.Init(meteringSampleProvider);


                    bufferLost = false;
                }
                else
                {
                    throw new Exception("DirectSound devices is not available...");
                }


                session = new PCMUSession();

                if(networkPars.TransportMode == TransportMode.Tcp)
                {
                    rtpReceiver = new RtpTcpReceiver(session);
                }
                else 
                {
                    rtpReceiver = new RtpUdpReceiver(session);
                }
                



                rtpReceiver.Open(networkPars.LocalAddr, networkPars.LocalPort);
                rtpReceiver.RtpPacketReceived += RtpReceiver_RtpPacketReceived;

            }
            catch (Exception ex)
            {
                logger.Debug(ex);
                CleanUp();

                throw;
            }
        }

        public float Volume
        {
            get
            {
                return volumeProvider?.Volume ?? 0;
            }
            set
            {
                if (volumeProvider != null)
                {
                    float vol = volumeProvider.Volume;
                    if (vol != value)
                    {
                        //logger.Debug("Volume: " + vol + " newVolume: " + value);
                        volumeProvider.Volume = value;
                    }
                }
            }
        }


        private WaveformPainter waveformPainter = null;
        public void SetWaveformPainter(WaveformPainter painter)
        {
            this.waveformPainter = painter;

        }

        private void PostVolumeMeter_StreamVolume(object sender, StreamVolumeEventArgs e)
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

        private void WavePlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            var ex = e.Exception;
            if (ex != null)
            {
                //var mmExeption = ex as NAudio.MmException;

                //if (mmExeption != null)
                //{
                //    if (mmExeption.Result == NAudio.MmResult.NoDriver)
                //    {
                //        deviceLost = true;
                //        return;
                //    }
                //}

                var comExeption = ex as COMException;
                if (comExeption != null)
                {
                    //https://msdn.microsoft.com/en-us/library/windows/desktop/ee416776(v=vs.85).aspx
                    if (comExeption.ErrorCode == -2005401450) // 0x88780096 DSERR_BUFFERLOST 
                    {
                        // TODO: перезапустить девайс...
                        bufferLost = true;
                        return;
                    }
                }

                logger.Error(ex);
                //...
            }
        }

        //FileStream fileStream = new FileStream("d:\\test.wav", FileMode.Create);
        //FileStream fileStream1 = new FileStream("d:\\test1.wav", FileMode.Create);

        private void RtpReceiver_RtpPacketReceived(RtpPacket packet)
        {
            if (closing)
            {
                return;
            }
            //byte[] rtpPayload = packet.Payload.ToArray();

            var data = session.Depacketize(packet);
            if (data != null)
            {
               //fileStream.Write(data, 0, data.Length);


                byte[] decoded = new byte[2 * data.Length];
                int j = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    short sample = MuLawDecoder.MuLawToLinearSample(data[i]);
                    decoded[j++] = (byte)(sample & 0xFF);
                    decoded[j++] = (byte)(sample >> 8);
                }

                //decoder.Decode(data, out byte[] decoded);


                //fileStream1.Write(decoded, 0, decoded.Length);

                if (decoded != null && decoded.Length > 0)
                {
                    waveBuffer.AddSamples(decoded, 0, decoded.Length);
                }
            }
        }


        public void Play()
        {
            logger.Debug("AudioReceiver::Play()");


            if ((wavePlayer.PlaybackState == PlaybackState.Paused || 
                wavePlayer.PlaybackState == PlaybackState.Stopped) 
                && bufferLost == false)
            {
                wavePlayer.Play();
            }
   
            //...
            rtpReceiver.Start();
        }

        public void Stop()
        {
            logger.Debug("AudioReceiver::Stop()");

            if (rtpReceiver != null)
            {
                rtpReceiver.RtpPacketReceived -= RtpReceiver_RtpPacketReceived;
                rtpReceiver.Close();
            }

            if (wavePlayer != null)
            {                
                //wavePlayer.Dispose();
                wavePlayer.Stop();
                wavePlayer = null;
            }

            CleanUp();

        }

        private bool closing = false;
        private void CleanUp()
        {
            if (rtpReceiver != null)
            {
                rtpReceiver.RtpPacketReceived -= RtpReceiver_RtpPacketReceived;
                rtpReceiver.Close();
            }

            if (wavePlayer != null)
            {
                wavePlayer.PlaybackStopped -= WavePlayer_PlaybackStopped;
                wavePlayer.Dispose();
                wavePlayer = null;

            }

            if (decoder != null)
            {
                decoder.Close();
                decoder = null;
            }
        }
    }
}
