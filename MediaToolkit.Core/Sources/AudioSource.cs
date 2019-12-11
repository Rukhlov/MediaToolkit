using NAudio.CoreAudioApi;
using NAudio.Wave;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{

    public class AudioSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const long ReftimesPerSec = 10000000;
        private const long ReftimesPerMillisec = 10000;

        public AudioSource(){ }

        private MMDevice captureDevice = null;
        private AudioClient audioClient;

        private byte[] recordBuffer;
        private int bytesPerFrame;
        private WaveFormat waveFormat;
        private int audioBufferMillisecondsLength;

        private bool isUsingEventSync;
        private AutoResetEvent frameEventWaitHandle;
        private Task captureTask;

        public event Action<byte[]> DataAvailable;
        //public event Action<byte[], int> DataAvailable;
        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        private volatile CaptureState captureState = CaptureState.Closed;
        public CaptureState State => captureState;
        public int ErrorCode { get; private set; } = 0;

        public AudioClientShareMode ShareMode { get; private set; }
        public WaveFormat WaveFormat
        {
            get
            {
                // for convenience, return a WAVEFORMATEX, instead of the real
                // WAVEFORMATEXTENSIBLE being used
                return waveFormat.AsStandardWaveFormat();
            }
            //set { waveFormat = value; }
        }


        public void Setup(string DeviceId, bool useEventSync = false, int audioBufferMillisecondsLength = 100, bool exclusiveMode = false)
        {
            logger.Debug("AudioSourceEx::Setup(...) " + DeviceId );

            if (captureState != CaptureState.Closed)
            {
                throw new InvalidOperationException("Invalid audio capture state " + captureState);
            }

            using (var deviceEnum = new MMDeviceEnumerator())
            {
                var mmDevices = deviceEnum.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

                for(int i= 0; i< mmDevices.Count; i++)
                {
                    var d = mmDevices[i];
                    if(d.ID == DeviceId)
                    {
                        captureDevice = d;
                        continue;
                    }
                    d.Dispose();
                }
            }

            if (captureDevice == null)
            {
                throw new Exception("MMDevice not found...");
            }

            this.isUsingEventSync = useEventSync;
            this.audioBufferMillisecondsLength = audioBufferMillisecondsLength;

            this.audioClient = captureDevice.AudioClient;
            this.ShareMode = exclusiveMode? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared;

            this.waveFormat = audioClient.MixFormat;

            long requestedDuration = ReftimesPerMillisec * audioBufferMillisecondsLength;

            if (!audioClient.IsFormatSupported(ShareMode, waveFormat))
            {
                throw new ArgumentException("Unsupported Wave Format");
            }

            try
            {
                var streamFlags = AudioClientStreamFlags.None;
                if (captureDevice.DataFlow != DataFlow.Capture)
                {
                    streamFlags = AudioClientStreamFlags.Loopback;
                }

                // If using EventSync, setup is specific with shareMode
                if (isUsingEventSync)
                {
                    var flags = AudioClientStreamFlags.EventCallback | streamFlags;

                    // Init Shared or Exclusive
                    if (ShareMode == AudioClientShareMode.Shared)
                    {
                        // With EventCallBack and Shared, both latencies must be set to 0
                        audioClient.Initialize(ShareMode, flags, requestedDuration, 0, waveFormat, Guid.Empty);
                    }
                    else
                    {
                        // With EventCallBack and Exclusive, both latencies must equals
                        audioClient.Initialize(ShareMode, flags, requestedDuration, requestedDuration, waveFormat, Guid.Empty);
                    }

                    // Create the Wait Event Handle
                    frameEventWaitHandle = new AutoResetEvent(false);
                    audioClient.SetEventHandle(frameEventWaitHandle.SafeWaitHandle.DangerousGetHandle());
                }
                else
                {
                    // Normal setup for both sharedMode
                    audioClient.Initialize(ShareMode,
                    streamFlags,
                    requestedDuration,
                    0,
                    waveFormat,
                    Guid.Empty);
                }

                int bufferFrameCount = audioClient.BufferSize;
                bytesPerFrame = waveFormat.Channels * waveFormat.BitsPerSample / 8;
                recordBuffer = new byte[bufferFrameCount * bytesPerFrame];

                captureState = CaptureState.Initialized;

            }
            catch(Exception ex)
            {
                logger.Error(ex);

                CleanUp();

                throw;
            }

        }



        public void Start()
        {
            logger.Debug("AudioSource::Start()");

            if (!(captureState == CaptureState.Stopped || captureState == CaptureState.Initialized))
            {
                throw new InvalidOperationException("Previous recording still in progress");
            }

            captureState = CaptureState.Starting;

            captureTask = Task.Run(() => 
            {
                try
                {
                    logger.Info("Capture thread started...");
                    captureState = CaptureState.Capturing;

                    CaptureStarted?.Invoke();

                    DoCapture();

                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    this.ErrorCode = 100500;
                }
                finally
                {
                    logger.Info("Capture thread stopped...");

                    captureState = CaptureState.Stopped;
                    CaptureStopped?.Invoke(null);

                }

            });

        }

        private void DoCapture()
        {
            try
            {
                //Debug.WriteLine(String.Format("Client buffer frame count: {0}", client.BufferSize));
                int bufferFrameCount = audioClient.BufferSize;

                // Calculate the actual duration of the allocated buffer.
                long actualDuration = (long)((double)ReftimesPerSec *
                                 bufferFrameCount / waveFormat.SampleRate);
                int sleepMilliseconds = (int)(actualDuration / ReftimesPerMillisec / 2);
                int waitMilliseconds = (int)(3 * actualDuration / ReftimesPerMillisec);

                var capture = audioClient.AudioCaptureClient;
                audioClient.Start();

                // avoid race condition where we stop immediately after starting
                if (captureState == CaptureState.Starting)
                {
                    captureState = CaptureState.Capturing;
                }
                while (captureState == CaptureState.Capturing)
                {
                    bool readBuffer = true;
                    if (isUsingEventSync)
                    {
                        readBuffer = frameEventWaitHandle.WaitOne(waitMilliseconds, false);
                    }
                    else
                    {
                        Thread.Sleep(sleepMilliseconds);
                    }

                    if (captureState != CaptureState.Capturing)
                    {
                        break;
                    }

                    // If still recording and notification is ok
                    if (readBuffer)
                    {
                        ReadNextPacket(capture);
                    }
                }
            }
            finally
            {
                //...
            }
        }


        private void ReadNextPacket(AudioCaptureClient capture)
        {
            int packetSize = capture.GetNextPacketSize();
            int recordBufferOffset = 0;
            //Debug.WriteLine(string.Format("packet size: {0} samples", packetSize / 4));

            while (packetSize != 0)
            {
                IntPtr buffer = capture.GetBuffer(out int framesAvailable, out AudioClientBufferFlags flags);

                int bytesAvailable = framesAvailable * bytesPerFrame;

                // apparently it is sometimes possible to read more frames than we were expecting?
                // fix suggested by Michael Feld:
                int spaceRemaining = Math.Max(0, recordBuffer.Length - recordBufferOffset);
                if (spaceRemaining < bytesAvailable && recordBufferOffset > 0)
                {
                    OnDataAvailable(recordBuffer, recordBufferOffset);
                    //DataAvailable?.Invoke(this, new WaveInEventArgs(recordBuffer, recordBufferOffset));
                    recordBufferOffset = 0;
                }

                // if not silence...
                if ((flags & AudioClientBufferFlags.Silent) != AudioClientBufferFlags.Silent)
                {
                    Marshal.Copy(buffer, recordBuffer, recordBufferOffset, bytesAvailable);
                }
                else
                {
                    Array.Clear(recordBuffer, recordBufferOffset, bytesAvailable);
                }
                recordBufferOffset += bytesAvailable;
                capture.ReleaseBuffer(framesAvailable);
                packetSize = capture.GetNextPacketSize();
            }

            OnDataAvailable(recordBuffer, recordBufferOffset);
           // DataAvailable?.Invoke(this, new WaveInEventArgs(recordBuffer, recordBufferOffset));
        }

        private void OnDataAvailable(byte[]buffer, int bytesRecorded)
        {
            if (bytesRecorded > 0)
            {
                byte[] data = new byte[bytesRecorded];
                Array.Copy(buffer, data, data.Length);

                DataAvailable?.Invoke(data);

                //DataAvailable?.Invoke(recordBuffer, recordBufferOffset);
            }
           
        }

        public void Stop()
        {
            logger.Debug("AudioSource::Stop()");

            if (captureState != CaptureState.Stopped)
            {
                captureState = CaptureState.Stopping;
            }

        }


        public void Close(bool force = false)
        {
            logger.Debug("AudioSource::Close(...) " + force);

            Stop();

            if (!force)
            {
                if (captureTask != null)
                {
                    if (captureTask.Status == TaskStatus.Running)
                    {
                        bool waitResult = false;
                        do
                        {
                            waitResult = captureTask.Wait(1000);
                            if (!waitResult)
                            {
                                logger.Warn("ScreenSource::Close() " + waitResult);
                            }
                        } while (!waitResult);

                    }
                }
            }

            CleanUp();

            captureState = CaptureState.Closed;
        }

        private void CleanUp()
        {
            logger.Debug("AudioSource::CleanUp()");

            if (captureDevice != null)
            {
                captureDevice.Dispose();
                captureDevice = null;
            }

            if (audioClient != null)
            {
                audioClient.Dispose();
                audioClient = null;
            }

            if (frameEventWaitHandle != null)
            {
                frameEventWaitHandle.Dispose();
                frameEventWaitHandle = null;
            }
        }
    }
}
