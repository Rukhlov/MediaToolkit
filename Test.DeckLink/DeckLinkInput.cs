using DeckLinkAPI;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.DeckLink
{
    public class DeckLinkInput : IDeckLinkInputCallback
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

 
        private IDeckLinkInput deckLinkInput = null;
        private IDeckLink deckLink = null;
        private IDeckLinkStatus deckLinkStatus;


        public string DisplayName { get; private set; } = "";
        public string ModelName { get; private set; } = "";

        public _BMDDisplayMode VideoDisplayMode { get; private set; } = _BMDDisplayMode.bmdModeHD1080p5994;
        public _BMDPixelFormat VideoPixelFormat { get; private set; } = _BMDPixelFormat.bmdFormat8BitYUV;

        public bool VideoEnabled { get; private set; } = true;
        public int VideoFrameWidth { get; private set; } = 1920;
        public int VideoFrameHeigth { get; private set; } = 1080;
        public Tuple<long, long> FrameRate { get; private set; } = new Tuple<long, long>(1001, 60000);


        public bool AudioEnabled { get; private set; } = true;
        public _BMDAudioSampleType AudioSampleType { get; private set; } = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public _BMDAudioSampleRate AudioSampleRate { get; private set; } = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
        public int AudioChannelsCount { get; private set; } = 2;

        private volatile bool isCapturing = false;
        public bool IsCapturing => isCapturing;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        private bool supportsFormatDetection = true;
        private bool applyDetectedInputMode = true;
        private bool supportsHDMITimecode = false;

        private volatile bool validInputSignal = false;

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private Thread captureThread = null;

        public event DeckLinkInputSignalHandler InputSignalChanged;
        public event DeckLinkFormatChangedHandler InputFormatChanged;

        public event Action<byte[], double> AudioDataArrived;
        public event Action<IntPtr, int, double, double> VideoDataArrived;

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        IDeckLinkMemoryAllocator memoryAllocator = new MemoryAllocator();

        public void StartCapture(int inputIndex)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + inputIndex);

            if (clenedUp)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            if (isCapturing )
            {
                return;
            }

            captureThread = new Thread(DoCapture);
            // IDeckLink требует MTA!!!
            // т.е все вызовы внутри потока
            captureThread.SetApartmentState(ApartmentState.MTA);
            captureThread.IsBackground = true;
            captureThread.Start(inputIndex);

        }

        private void DoCapture(object inputArgs)
        {

            logger.Debug("DeckLinkInput::DoCapture(...) BEGIN " + inputArgs.ToString());

            if (isCapturing)
            {
                return;
            }

            try
            {
                var inputIndex = (int)inputArgs;

                GetDeviceByIndex(inputIndex, out IDeckLink _deckLink);

                if (_deckLink == null)
                {
                    throw new Exception("Device not found");
                }

                StartUp(_deckLink);

                logger.Debug("DeckLinkInput start capturing: " + DisplayName);

                CaptureStarted?.Invoke();
                while (isCapturing)
                { 
                    //TODO:
                    //Control device state...
                    
                    syncEvent.WaitOne(1000);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                isCapturing = false;

                errorCode = 100500;
            }
            finally
            {
                deckLinkInput.FlushStreams();

                if (isCapturing)
                {
                    deckLinkInput.StopStreams();
                }

                // deckLinkInput.SetVideoInputFrameMemoryAllocator(null);

                deckLinkInput.SetCallback(null);

                isCapturing = false;

                CaptureStopped?.Invoke(null);

                CleanUp();

            }

            logger.Debug("DeckLinkInput::DoCapture() END");

        }

        private bool GetDeviceByIndex(int inputIndex, out IDeckLink _deckLink)
        {
            logger.Trace("GetDeviceByIndex(...) " + inputIndex);

            bool Success = false;

            _deckLink = null;
            IDeckLinkIterator deckLinkIterator = null;
            try
            {
                deckLinkIterator = new CDeckLinkIterator();

                int index = 0;
                do
                {
                    if (_deckLink != null)
                    {
                        Marshal.ReleaseComObject(_deckLink);
                        _deckLink = null;
                    }

                    deckLinkIterator.Next(out _deckLink);
                    if (index == inputIndex)
                    {
                        Success = true;
                        break;
                    }

                    index++;
                }
                while (_deckLink != null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

            }
            finally
            {
                if (deckLinkIterator != null)
                {
                    Marshal.ReleaseComObject(deckLinkIterator);
                    deckLinkIterator = null;
                }

            }

            return Success;
        }

        private void StartUp(IDeckLink _deckLink)
        {
            logger.Trace("StartUp(...)");

            this.deckLink = _deckLink;
            this.deckLinkInput = (IDeckLinkInput)deckLink;
            this.deckLinkStatus = (IDeckLinkStatus)deckLink;


            bool videoInputSignalLocked = false;
            deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
            videoInputSignalLocked = (videoInputSignalLockedFlag != 0);

            if (!videoInputSignalLocked)
            {
                throw new Exception("Video input locked");
            }


            deckLink.GetDisplayName(out string displayName);
            deckLink.GetModelName(out string modelName);

            this.DisplayName = displayName;
            this.ModelName = modelName;


            IDeckLinkProfileAttributes attrs = (IDeckLinkProfileAttributes)deckLink;
            attrs.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsInputFormatDetection, out int supportsFormatDetectionFlag);
            supportsFormatDetection = (supportsFormatDetectionFlag != 0);

            attrs.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsHDMITimecode, out int supportsHDMITimecodeFlag);
            supportsHDMITimecode = (supportsHDMITimecodeFlag != 0);

            validInputSignal = false;

            var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
            // Enable input video mode detection if the device supports it
            if (supportsFormatDetection && applyDetectedInputMode)
            {
                videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
            }

            // memoryAllocator = new MemoryAllocator();
            //deckLinkInput.SetVideoInputFrameMemoryAllocator(memoryAllocator);


            deckLinkInput.SetCallback(this);

            if (VideoEnabled)
            { // не работает!
                deckLinkInput.EnableVideoInput(VideoDisplayMode, VideoPixelFormat, videoInputFlags);

            }


            if (AudioEnabled)
            {
                deckLinkInput.EnableAudioInput(AudioSampleRate, AudioSampleType, (uint)AudioChannelsCount);

            }


            deckLinkInput.GetDisplayMode(VideoDisplayMode, out IDeckLinkDisplayMode displayMode);
            displayMode.GetFrameRate(out long frameDuration, out long timeScale);
            FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            deckLinkInput.StartStreams();

            isCapturing = true;

        }

        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {
            logger.Debug("IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            if (!isCapturing)
            {
                logger.Warn("VideoInputFormatChanged(...) IsCapturing: " + isCapturing);
                return;
            }

            // Restart capture with the new video mode if told to
            if (!applyDetectedInputMode)
            {
                return;
            }

            VideoPixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
            VideoDisplayMode = newDisplayMode.GetDisplayMode();

            var videoConnection = _BMDVideoConnection.bmdVideoConnectionHDMI;
            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;
            deckLinkInput.DoesSupportVideoMode(videoConnection, VideoDisplayMode, VideoPixelFormat, videoModeFlags, out int supported);

            logger.Debug(VideoPixelFormat + " " + supported);

            //// Stop the capture
            //deckLinkInput.StopStreams();

            deckLinkInput.PauseStreams();
            deckLinkInput.FlushStreams();

            deckLinkInput.EnableVideoInput(VideoDisplayMode, VideoPixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);

            deckLinkInput.GetDisplayMode(VideoDisplayMode, out IDeckLinkDisplayMode displayMode);
            displayMode.GetFrameRate(out long frameDuration, out long timeScale);
            FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            deckLinkInput.StartStreams();

            logger.Debug("DeckLinkInput::NewDisplayMode: " + newDisplayMode.GetDisplayMode());

            InputFormatChanged?.Invoke(newDisplayMode);

        }


        void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame videoFrame, IDeckLinkAudioInputPacket audioPacket)
        {
            if (!isCapturing)
            {
                logger.Warn("VideoInputFrameArrived(...) IsCapturing: " + isCapturing);

                return;
            }

            if (videoFrame != null)
            {
                ProcessVideoFrame(videoFrame);
            }

            if (audioPacket != null)
            {
                ProcessAudioPacket(audioPacket);
            }

        }

        private void ProcessAudioPacket(IDeckLinkAudioInputPacket audioPacket)
        {
            if (!isCapturing)
            {
                logger.Warn("ProcessAudioPacket(...) IsCapturing: " + isCapturing);

                return;
            }

            try
            {
                long timeScale = FrameRate.Item2;
           
                audioPacket.GetPacketTime(out long packetTime, timeScale);
                
                int sampleSize = ((int)AudioSampleType / 8); //32bit
                int samplesCount = audioPacket.GetSampleFrameCount();
                int dataLength = sampleSize * AudioChannelsCount * samplesCount;
               
                if (dataLength > 0)
                {
                    audioPacket.GetBytes(out IntPtr pBuffer);

                    if (pBuffer != IntPtr.Zero)
                    {
                        byte[] data = new byte[dataLength];
                        Marshal.Copy(pBuffer, data, 0, data.Length);



                        //var f = File.Create(@"d:\testPCM\" + DateTime.Now.ToString("HH_mm_ss_fff") + ".raw");

   
                        //Marshal.Copy(pBuffer, data, 0, data.Length);
                        //f.Write(data, 0, data.Length);
                        //f.Close();

                        double timeSec = packetTime / timeScale;
                        AudioDataArrived?.Invoke(data, timeSec);

                    }
                }
            }
            finally
            {
                Marshal.ReleaseComObject(audioPacket);
            }
        }

        private void ProcessVideoFrame(IDeckLinkVideoInputFrame videoFrame)
        {
            if (!isCapturing)
            {
                logger.Warn("ProcessVideoFrame(...) IsCapturing: " + isCapturing);

                return;
            }

            try
            {
                var frameFlags = videoFrame.GetFlags();

                bool inputSignal = frameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource);

                if (inputSignal != validInputSignal)
                { // х.з что это
                    validInputSignal = inputSignal;
                    InputSignalChanged?.Invoke(validInputSignal);
                }
                else
                {
                    long timeScale = FrameRate.Item2;

                    videoFrame.GetStreamTime(out long frameTime, out long frameDuration, timeScale);

                    //if (supportsHDMITimecode)
                    //{ // Blackmagic DeckLink SDK 2.4.9.1 Timecode Capture
                    //    videoFrame.GetHardwareReferenceTimestamp(timeScale, out long frameTime, out long frameDuration);
                    //}
                    //else
                    //{
                    //    videoFrame.GetTimecode(_BMDTimecodeFormat.bmdTimecodeLTC, out IDeckLinkTimecode timecode);
                    //    //_BMDTimecodeFlags timecodeFlags = timecode.GetFlags();
                    //    timecode.GetTimecodeUserBits(out uint userBits);
                    //}


                    int width = videoFrame.GetWidth();
                    int height = videoFrame.GetHeight();
                    int stride = videoFrame.GetRowBytes();
                    //_BMDPixelFormat format = videoFrame.GetPixelFormat();

                    var bufferLength = stride * height;
                    videoFrame.GetBytes(out IntPtr pBuffer);

                    double frameTimeSec = (double)frameTime / timeScale;
                    double frameDurationSec = (double)frameDuration / timeScale;

                    VideoDataArrived?.Invoke(pBuffer, bufferLength, frameTimeSec, frameDurationSec);


                    //var f = File.Create(@"d:\testBMP2\" + DateTime.Now.ToString("HH_mm_ss_fff") + " " + width + "x" + height + "_" + format + ".raw");

                    //byte[] data = new byte[bufferLength];
                    //Marshal.Copy(pBuffer, data, 0, data.Length);
                    //f.Write(data, 0, data.Length);
                    //f.Close();
                }

            }
            finally
            {
                Marshal.ReleaseComObject(videoFrame);
            }
        }

        public void StopCapture()
        {
            logger.Debug("DeckLinkInput::StopCapture()");

            isCapturing = false;
            syncEvent?.Set();
        }

        private bool clenedUp = false;
        private void CleanUp()
        {

            logger.Trace("DeckLinkInput::CleanUp()");

            clenedUp = true;

            if (deckLink != null)
            {
                Marshal.ReleaseComObject(deckLink);
                deckLink = null;
            }

            if (deckLinkInput != null)
            {
                Marshal.ReleaseComObject(deckLinkInput);
                deckLinkInput = null;
            }

            if (deckLinkStatus != null)
            {
                Marshal.ReleaseComObject(deckLinkStatus);
                deckLinkStatus = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }


        }


        class MemoryAllocator : IDeckLinkMemoryAllocator
        {
            public void AllocateBuffer(uint bufferSize, out IntPtr allocatedBuffer)
            {
                allocatedBuffer = Marshal.AllocCoTaskMem((int)bufferSize);
            }

            public void Commit()
            {
            }

            public void Decommit()
            {
            }

            public void ReleaseBuffer(IntPtr buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }

    }


}
