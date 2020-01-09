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

namespace MediaToolkit.DeckLink
{
    public class DeckLinkInput : IDeckLinkInputCallback, IDeckLinkNotificationCallback
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDeckLinkInput deckLinkInput = null;
        private IDeckLink deckLink = null;
        private IDeckLinkStatus deckLinkStatus = null;
        private IDeckLinkProfileAttributes deckLinkProfileAttrs = null;

        private IDeckLinkNotification deckLinkNotification;

        public string DisplayName { get; private set; } = "";
        public string ModelName { get; private set; } = "";

        //TODO: на данный момент удалось проверить девайс только с одним форматом  - bmdModeHD1080p5994
        // как включать другие форматы?!?  
        public _BMDDisplayMode DisplayMode { get; private set; } = _BMDDisplayMode.bmdModeUnknown;//bmdModeHD1080p30;//_BMDDisplayMode.bmdModeHD1080p5994;

        public _BMDPixelFormat PixelFormat { get; private set; } = _BMDPixelFormat.bmdFormatUnspecified;
        public string PixelFormatCode => DeckLinkTools.GetPixelFormatFourCC(this.PixelFormat);
        public System.Drawing.Size FrameSize { get; private set; } = System.Drawing.Size.Empty;
        public Tuple<long, long> FrameRate { get; private set; }

        public bool AudioEnabled { get; private set; } = true;
        private _BMDAudioSampleType audioSampleType  = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public int AudioBitsPerSample => (int)audioSampleType;

        private _BMDAudioSampleRate audioSampleRate = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
        public int AudioSampleRate => (int)audioSampleRate;
        public int AudioChannelsCount { get; private set; } = 2;
        private double audioBytesPerSeconds = 0;

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


        public event Action<bool> InputSignalChanged;
        public event Action<object> InputFormatChanged;

        public event Action<byte[], double> AudioDataArrived;
        public event Action<IntPtr, int, double, double> VideoDataArrived;

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

       // IDeckLinkMemoryAllocator memoryAllocator = new MemoryAllocator();

        private double audioDurationSeconds = 0;
        private double videoDurationSeconds = 0;

        private double lastAudioPacketTimeSec = 0;
        private double lastVideoFrameTimeSec = 0;


        public void StartCapture(int inputIndex)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + inputIndex);

            if (clenedUp)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            if (isCapturing)
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

                DeckLinkTools.GetDeviceByIndex(inputIndex, out IDeckLink _deckLink);

                if (_deckLink == null)
                {
                    throw new Exception("Device not found " + inputIndex);
                }

                this.deckLink = _deckLink;
                this.deckLinkInput = (IDeckLinkInput)deckLink;
                this.deckLinkStatus = (IDeckLinkStatus)deckLink;
                this.deckLinkNotification = (IDeckLinkNotification)deckLink;
                this.deckLinkProfileAttrs = (IDeckLinkProfileAttributes)deckLink;

                StartUp();

                logger.Debug("DeckLinkInput start capturing: " + DisplayName);

                CaptureStarted?.Invoke();
                while (isCapturing)
                {
                    //TODO:
                    //Control device state, A/V sync, errors....

                    //logger.Debug("PacketTime: " + lastVideoFrameTimeSec.ToString("0.000") +
                    //    " - " + lastAudioPacketTimeSec.ToString("0.000") +
                    //    " = " + (lastVideoFrameTimeSec - lastAudioPacketTimeSec).ToString("0.000"));

                    //logger.Debug("DurationTime: " + videoDurationSeconds.ToString("0.000") +
                    //    " - " + audioDurationSeconds.ToString("0.000") +
                    //    " = " + (videoDurationSeconds - audioDurationSeconds).ToString("0.000") + "\r\n");


                    //var diff1 = (videoDurationSeconds - audioDurationSeconds).ToString("0.000");
                    //var diff2 = (lastVideoFrameTimeSec - lastAudioPacketTimeSec).ToString("0.000");
                    //var audioTime1 = TimeSpan.FromSeconds(audioDurationSeconds).ToString("hh\\:mm\\:ss\\.fff");
                    //var audioTime2 = TimeSpan.FromSeconds(lastAudioPacketTimeSec).ToString("hh\\:mm\\:ss\\.fff"); 

                    //var videoTime1 = TimeSpan.FromSeconds(videoDurationSeconds).ToString("hh\\:mm\\:ss\\.fff");
                    //var videoTime2 = TimeSpan.FromSeconds(lastVideoFrameTimeSec).ToString("hh\\:mm\\:ss\\.fff");
                    //var adiff1 = (audioDurationSeconds - lastAudioPacketTimeSec).ToString("0.000");
                    //var vdiff1 = (videoDurationSeconds - lastVideoFrameTimeSec).ToString("0.000");
                    //Console.WriteLine("AVDiff: " + diff1 + " " + diff2
                    //    + "| at " + audioTime1 + " - " + audioTime2 + " " + adiff1  
                    //    + "| vt " + videoTime1 + " - " + videoTime2 + " " +  adiff1);

                    //Console.SetCursorPosition(0, Console.CursorTop - 1);

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

                if (deckLinkInput != null)
                {
                    deckLinkInput.FlushStreams();

                    if (isCapturing)
                    {
                        deckLinkInput.StopStreams();
                    }

                   // deckLinkInput.SetVideoInputFrameMemoryAllocator(null);


                    deckLinkInput.SetCallback(null);

                }

                //if (deckLinkNotification != null)
                //{
                //    deckLinkNotification.Unsubscribe(_BMDNotifications.bmdDeviceRemoved | _BMDNotifications.bmdStatusChanged, this);
                //}

                isCapturing = false;

                CaptureStopped?.Invoke(null);

                CleanUp();

            }

            logger.Debug("DeckLinkInput::DoCapture() END");

        }


        private void StartUp()
        {
            logger.Trace("StartUp(...)");

            // deckLinkNotification.Subscribe(_BMDNotifications.bmdStatusChanged | _BMDNotifications.bmdDeviceRemoved, this);

            bool videoInputSignalLocked = GetVideoInputSignalLockedState();

            if (!videoInputSignalLocked)
            {
                throw new Exception("Video input locked");
            }

            // проверяем доступен ли девайс
            _BMDDeviceBusyState deviceBusyState = GetDeviceBusyState();
            if (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy)
            {// 
                //TODO: device busy...
                logger.Warn("bmdDeckLinkStatusBusy: " + deviceBusyState);
            }

            deckLink.GetDisplayName(out string displayName);
            deckLink.GetModelName(out string modelName);

            this.DisplayName = displayName;
            this.ModelName = modelName;
            this.DisplayMode = GetCurrentVideoInputMode();
            this.PixelFormat = GetCurrentVideoInputPixelFormat();

            _BMDVideoConnection videoConnections = GetVideoInputConnections();
            logger.Info(string.Join("; ", DisplayName, videoConnections, DisplayMode, PixelFormat));

            supportsFormatDetection = GetSupportsFormatDetection();
            supportsHDMITimecode = GetSupportsHDMITimecode();


            //memoryAllocator = new MemoryAllocator();
            //deckLinkInput.SetVideoInputFrameMemoryAllocator(memoryAllocator);


            deckLinkInput.SetCallback(this);

            var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
            if (supportsFormatDetection && applyDetectedInputMode)
            {
                videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
            }
            deckLinkInput.EnableVideoInput(DisplayMode, PixelFormat, videoInputFlags);

            if (AudioEnabled)
            {
                deckLinkInput.EnableAudioInput(audioSampleRate, audioSampleType, (uint)AudioChannelsCount);
                audioBytesPerSeconds = ((int)audioSampleRate * (int)AudioChannelsCount * (int)audioSampleType) / 8;
            }

            deckLinkInput.GetDisplayMode(DisplayMode, out IDeckLinkDisplayMode displayMode);

            int width = displayMode.GetWidth();
            int height = displayMode.GetHeight();
            displayMode.GetFrameRate(out long frameDuration, out long timeScale);

            this.FrameSize = new System.Drawing.Size(width, height);
            this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            this.lastAudioPacketTimeSec = 0;
            this.lastVideoFrameTimeSec = 0;

            this.validInputSignal = false;

            logger.Info("Start input stream: " + DeckLinkTools.LogDisplayMode(displayMode) + " " + PixelFormat);
            deckLinkInput.StartStreams();

            isCapturing = true;

        }


        void IDeckLinkNotificationCallback.Notify(_BMDNotifications topic, ulong param1, ulong param2)
        {
            logger.Debug("IDeckLinkNotificationCallback.Notify(...) " + topic + " " + param1 + " " + " " + param2);
            //...
        }

        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {// не понятно как менять форматы у девайса
            logger.Debug("IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            if (!isCapturing)
            {
                logger.Warn("VideoInputFormatChanged(...) IsCapturing: " + isCapturing);
                return;
            }

            if (!applyDetectedInputMode)
            {
                logger.Warn("applyDetectedInputMode == false");
                return;
            }

            var newVideoDisplayMode = this.DisplayMode;
            if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputColorspaceChanged))
            {//  изменилась цветовая схема 


            }
            else if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputDisplayModeChanged))
            {
                newVideoDisplayMode = newDisplayMode.GetDisplayMode();
            }
            else if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputFieldDominanceChanged))
            {// ...

            }

            var newPixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
            if (detectedSignalFlags.HasFlag(_BMDDetectedVideoInputFormatFlags.bmdDetectedVideoInputRGB444))
            {
                newPixelFormat = _BMDPixelFormat.bmdFormat8BitARGB; //_BMDPixelFormat.bmdFormat10BitRGB;
            }

            var videoConnection = GetVideoInputConnections();
            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;

            deckLinkInput.DoesSupportVideoMode(videoConnection, newVideoDisplayMode, newPixelFormat, videoModeFlags, out int supported);

            if (supported == 0)
            {
                logger.Error("Format not supported: " + string.Join(" ", videoConnection, newVideoDisplayMode, newPixelFormat, videoModeFlags));
                //TODO: что то пошло не так... закрываем стрим
            }

            this.PixelFormat = newPixelFormat;
            this.DisplayMode = newVideoDisplayMode;

            //// Stop the capture
            //deckLinkInput.StopStreams();

            deckLinkInput.PauseStreams();
            deckLinkInput.FlushStreams();

            deckLinkInput.EnableVideoInput(DisplayMode, PixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);

            deckLinkInput.GetDisplayMode(DisplayMode, out IDeckLinkDisplayMode displayMode);
            displayMode.GetFrameRate(out long frameDuration, out long timeScale);
            int width = displayMode.GetWidth();
            int height = displayMode.GetHeight();

            this.FrameSize = new System.Drawing.Size(width, height);
            this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            deckLinkInput.StartStreams();


            logger.Warn("Format changed to: " + DeckLinkTools.LogDisplayMode(displayMode) + " " + PixelFormat);

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
            else
            {
                logger.Warn("No video frame...");
            }

            if (AudioEnabled)
            {
                if (audioPacket != null)
                {
                    ProcessAudioPacket(audioPacket);
                }
                else
                {
                    logger.Warn("No audio packet...");
                }
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

                int sampleSize = ((int)audioSampleType / 8); //32bit
                int samplesCount = audioPacket.GetSampleFrameCount();
                int dataLength = sampleSize * AudioChannelsCount * samplesCount;

                if (dataLength > 0)
                {
                    audioPacket.GetBytes(out IntPtr pBuffer);

                    if (pBuffer != IntPtr.Zero)
                    {
                        byte[] data = new byte[dataLength];
                        Marshal.Copy(pBuffer, data, 0, data.Length);

                        //var fileName = @"d:\testPCM\" + DateTime.Now.ToString("HH_mm_ss_fff") + ".raw";
                        //MediaToolkit.Utils.TestTools.WriteFile(data, fileName);

                        lastAudioPacketTimeSec = packetTime / timeScale;

                        double dataDurationSec = dataLength / audioBytesPerSeconds;
                        audioDurationSeconds += dataDurationSec;

                        AudioDataArrived?.Invoke(data, lastAudioPacketTimeSec);

                    }
                }
            }
            //catch (Exception ex)
            //{
            //    logger.Error(ex);
            //}
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

                bool inputSignal = frameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource); // no signal !!!
                if (inputSignal)
                {
                    logger.Warn("Video no signal...");
                }

                if (inputSignal != validInputSignal)
                {
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
                    _BMDPixelFormat format = videoFrame.GetPixelFormat();

                    var bufferLength = stride * height;
                    videoFrame.GetBytes(out IntPtr pBuffer);

                    lastVideoFrameTimeSec = (double)frameTime / timeScale;

                    double frameDurationSec = (double)frameDuration / timeScale;
                    videoDurationSeconds += frameDurationSec;


                    VideoDataArrived?.Invoke(pBuffer, bufferLength, lastVideoFrameTimeSec, frameDurationSec);

                    //var fileName = @"d:\testBMP2\" + DateTime.Now.ToString("HH_mm_ss_fff") + " " + width + "x" + height + "_" + format + ".raw";
                    //MediaToolkit.Utils.TestTools.WriteFile( pBuffer, bufferLength, fileName);
                }

            }
            //catch(Exception ex)
            //{
            //    logger.Error(ex);
            //}
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

        private bool GetVideoInputSignalLockedState()
        {
            deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
            bool videoInputSignalLocked = (videoInputSignalLockedFlag != 0);
            return videoInputSignalLocked;
        }

        private _BMDPixelFormat GetCurrentVideoInputPixelFormat()
        {
            deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusCurrentVideoInputPixelFormat, out long currentVideoInputPixelFormatFlag);
            _BMDPixelFormat currentVideoInputPixelFormat = (_BMDPixelFormat)currentVideoInputPixelFormatFlag;
            return currentVideoInputPixelFormat;
        }

        private _BMDDisplayMode GetCurrentVideoInputMode()
        {
            deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusCurrentVideoInputMode, out long bmdDeckLinkStatusCurrentVideoInputModeFlag);
            _BMDDisplayMode currentVideoInputMode = (_BMDDisplayMode)bmdDeckLinkStatusCurrentVideoInputModeFlag;
            return currentVideoInputMode;
        }


        private _BMDDeviceBusyState GetDeviceBusyState()
        {
            deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusBusy, out long deviceBusyStateFlag);
            _BMDDeviceBusyState deviceBusyState = (_BMDDeviceBusyState)deviceBusyStateFlag;
            return deviceBusyState;
        }


        private bool GetSupportsHDMITimecode()
        {
            deckLinkProfileAttrs.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsHDMITimecode, out int supportsHDMITimecodeFlag);
            var supportsHDMITimecode = (supportsHDMITimecodeFlag != 0);
            return supportsHDMITimecode;
        }

        private bool GetSupportsFormatDetection()
        {
            deckLinkProfileAttrs.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsInputFormatDetection, out int supportsFormatDetectionFlag);
            var supportsFormatDetection = (supportsFormatDetectionFlag != 0);
            return supportsFormatDetection;
        }

        private _BMDVideoConnection GetVideoInputConnections()
        {
            deckLinkProfileAttrs.GetInt(_BMDDeckLinkAttributeID.BMDDeckLinkVideoInputConnections, out long videoInputConnectionsFlag);
            _BMDVideoConnection videoConnection = (_BMDVideoConnection)videoInputConnectionsFlag;
            return videoConnection;
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

            if (deckLinkNotification != null)
            {
                Marshal.ReleaseComObject(deckLinkNotification);
                deckLinkNotification = null;
            }

            if (deckLinkProfileAttrs != null)
            {
                Marshal.ReleaseComObject(deckLinkProfileAttrs);
                deckLinkProfileAttrs = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }

        }

    }

}
