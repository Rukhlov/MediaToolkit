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

        private IDeckLinkNotification deckLinkNotification = null;

        public string DisplayName { get; private set; } = "";
        public string ModelName { get; private set; } = "";

        // форматы можно настроить на источнике видео 
        public _BMDDisplayMode DisplayModeId { get; private set; } = _BMDDisplayMode.bmdModeUnknown;//bmdModeHD1080p30;//_BMDDisplayMode.bmdModeHD1080p5994;

        private _BMDPixelFormat pixelFormat = _BMDPixelFormat.bmdFormatUnspecified;
        public _BMDPixelFormat PixelFormat { get => pixelFormat; set => pixelFormat = value; }

        public int PixelFormatCode => DeckLinkTools.GetPixelFormatFourCC(this.PixelFormat);

        public System.Drawing.Size FrameSize { get; private set; } = System.Drawing.Size.Empty;
        public Tuple<long, long> FrameRate { get; private set; }

        public _BMDFieldDominance FieldDominance { get; private set; } = _BMDFieldDominance.bmdUnknownFieldDominance;
        public int VideoInterlaceMode => DeckLinkTools.GetVideoInterlaceMode(FieldDominance);

        public bool AudioEnabled { get; private set; } = true;
        private _BMDAudioSampleType audioSampleType = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public int AudioBitsPerSample => (int)audioSampleType;

        private _BMDAudioSampleRate audioSampleRate = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
        public int AudioSampleRate => (int)audioSampleRate;
        public int AudioChannelsCount { get; private set; } = 2;
        private double audioBytesPerSeconds = 0;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

 
        private bool supportsFormatDetection = true;
        private bool applyDetectedInputMode = true;
        private bool supportsHDMITimecode = false;

        private volatile bool validInputSignal = false;
        private volatile bool initialized = false;
        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private Thread captureThread = null;


        public event Action<bool> InputSignalChanged;
        public event Action<object> InputFormatChanged;

        public event Action<byte[], double> AudioDataArrived;
        public event Action<IntPtr, int, double, double> VideoDataArrived;

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;
        public event Action ReadyToStart;

        private IDeckLinkMemoryAllocator memoryAllocator = null;

        private double audioDurationSeconds = 0;
        private double videoDurationSeconds = 0;

        private double lastAudioPacketTimeSec = 0;
        private double lastVideoFrameTimeSec = 0;

        private int inputDeviceIndex = -1;
        private IDeckLinkScreenPreviewCallback previewCallback = null;

        private volatile CaptureState captureState = CaptureState.Closed;

        public void StartCapture(DeckLinkDeviceDescription device, DeckLinkDisplayModeDescription mode = null)
        {
            var index = device.DeviceIndex;
            var modeId = mode?.ModeId ?? (long)_BMDDisplayMode.bmdModeUnknown;
            var pixFormat = mode?.PixFmt?? (long)_BMDPixelFormat.bmdFormatUnspecified;

            StartCapture(index, pixFormat, modeId);
        }

        //private volatile bool isRunning = false;
        public void StartCapture(int inputIndex, long pixFmt, long modeId)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + string.Join(" ", inputIndex, pixFmt, modeId));

            if (captureState != CaptureState.Closed)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);
                return;
            }

            captureState = CaptureState.Starting;

            captureThread = new Thread(DoCapture);
            // IDeckLink требует MTA!!!
            // т.е все вызовы внутри потока
            captureThread.SetApartmentState(ApartmentState.MTA);
            captureThread.IsBackground = true;
            captureThread.Start(new long[]
            {
                inputIndex,
                pixFmt,
                modeId,
            });

        }

        private void DoCapture(object args)
        {

            logger.Debug("DeckLinkInput::DoCapture(...) BEGIN " + args.ToString());

            if (captureState != CaptureState.Starting)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);

                return;
            }


            bool streamStarted = false;
            try
            {
                var pars = args as long[];

                this.inputDeviceIndex = (int)pars[0];

                this.PixelFormat = (_BMDPixelFormat)pars[1];
                this.DisplayModeId = (_BMDDisplayMode)pars[2];

                if (this.DisplayModeId != _BMDDisplayMode.bmdModeUnknown)
                {
                    applyDetectedInputMode = false;
                }


                DeckLinkTools.GetDeviceByIndex(inputDeviceIndex, out IDeckLink _deckLink);

                if (_deckLink == null)
                {
                    throw new Exception("Device not found " + inputDeviceIndex);
                }

                this.deckLink = _deckLink;
                this.deckLinkInput = (IDeckLinkInput)deckLink;
                this.deckLinkStatus = (IDeckLinkStatus)deckLink;
                this.deckLinkNotification = (IDeckLinkNotification)deckLink;
                this.deckLinkProfileAttrs = (IDeckLinkProfileAttributes)deckLink;

                while (captureState == CaptureState.Starting && !initialized)
                {
                    bool videoInputSignalLocked = GetVideoInputSignalLockedState();
                    if (!videoInputSignalLocked)
                    {
                        syncEvent.WaitOne(1000);
                        continue;
                    }

                    if (captureState != CaptureState.Starting)
                    {
                        return;
                    }

                    _BMDDeviceBusyState deviceBusyState = GetDeviceBusyState();
                    if (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy)
                    {
                        logger.Debug("bmdDeckLinkStatusBusy: " + deviceBusyState);
                        syncEvent.WaitOne(1000);

                        continue;
                    }

                    if (captureState != CaptureState.Starting)
                    {
                        return;
                    }


                    
                    deckLink.GetDisplayName(out string displayName);
                    deckLink.GetModelName(out string modelName);

                    this.DisplayName = displayName;
                    this.ModelName = modelName;

                    if (DisplayModeId == _BMDDisplayMode.bmdModeUnknown)
                    {
                        DisplayModeId = GetCurrentVideoInputMode();
                    }

                    if(PixelFormat == _BMDPixelFormat.bmdFormatUnspecified)
                    {// 
                        PixelFormat = GetCurrentVideoInputPixelFormat();
                    }

                    bool pixelFormatSupported = DeckLinkTools.ValidateAndCorrectPixelFormat(ref pixelFormat);
                    if (!pixelFormatSupported)
                    {
                        throw new NotSupportedException("Pixel format not supported: " + PixelFormat);
                    }

                    //PixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;

                    _BMDVideoConnection videoConnections = GetVideoInputConnections();
                    logger.Info(string.Join("; ", DisplayName, videoConnections, DisplayModeId, PixelFormat));

                    supportsFormatDetection = GetSupportsFormatDetection();
                    supportsHDMITimecode = GetSupportsHDMITimecode();


                   // memoryAllocator = new MemoryAllocator();
                    memoryAllocator = new SimpleMemoryAllocator();
                    deckLinkInput.SetVideoInputFrameMemoryAllocator(memoryAllocator);

                    if (previewCallback != null)
                    {
                        deckLinkInput.SetScreenPreviewCallback(previewCallback);
                    }

                    deckLinkInput.SetCallback(this);

                    var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
                    if (supportsFormatDetection && applyDetectedInputMode)
                    {
                        videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
                    }
                    deckLinkInput.EnableVideoInput(DisplayModeId, PixelFormat, videoInputFlags);

                    if (AudioEnabled)
                    {
                        deckLinkInput.EnableAudioInput(audioSampleRate, audioSampleType, (uint)AudioChannelsCount);
                        audioBytesPerSeconds = ((int)audioSampleRate * (int)AudioChannelsCount * (int)audioSampleType) / 8;
                    }

                    IDeckLinkDisplayMode displayMode = null;
                    try
                    {
                        deckLinkInput.GetDisplayMode(DisplayModeId, out displayMode);

                        int width = displayMode.GetWidth();
                        int height = displayMode.GetHeight();
                        displayMode.GetFrameRate(out long frameDuration, out long timeScale);

                        this.FieldDominance = displayMode.GetFieldDominance();

                        this.FrameSize = new System.Drawing.Size(width, height);
                        this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);
                        
                        logger.Info("Start input stream: " + DeckLinkTools.LogDisplayMode(displayMode) + " " + PixelFormat);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(displayMode);
                        displayMode = null;
                    }

                    this.lastAudioPacketTimeSec = 0;
                    this.lastVideoFrameTimeSec = 0;

                    this.validInputSignal = false;

                    ReadyToStart?.Invoke();

                    initialized = true;

                }

                if (captureState == CaptureState.Starting && initialized)
                {
                    logger.Debug("DeckLinkInput start capturing: " + DisplayName);

                    deckLinkInput.StartStreams();
                    streamStarted = true;

                    CaptureStarted?.Invoke();

                    captureState = CaptureState.Capturing;

                    while (captureState == CaptureState.Capturing)
                    {

                        #region Control device state, A/V sync, errors....
                        //TODO:
                        //

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
                        //    + "| vt " + videoTime1 + " - " + videoTime2 + " " + adiff1);

                        //Console.WriteLine("adiff: " + audioDiff.ToString("0.000") + " adiff1: " + audioDiff1.ToString("0.000") + 
                        //    " vdiff: " + videoDiff.ToString("0.000") + " vdiff1: " + videoDiff1.ToString("0.000"));
                        //Console.SetCursorPosition(0, Console.CursorTop - 1);

                        #endregion

                        if (errorCode != 0)
                        {
                            break;
                        }

                        syncEvent.WaitOne(1000);


                        //logger.Debug("DeckLinkInput capture result: " + errorCode);
                    }
                }
                else
                {
                    logger.Warn("Capture cancelled...");
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex);

                captureState = CaptureState.Closing;
                streamStarted = false;

                errorCode = 100500;
            }
            finally
            {

                if (deckLinkInput != null)
                {
                    deckLinkInput.FlushStreams();

                    try
                    {
                        if (streamStarted)
                        {
                            deckLinkInput.StopStreams();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex.Message);
                    }


                    deckLinkInput.SetScreenPreviewCallback(null);

                    // deckLinkInput.SetVideoInputFrameMemoryAllocator(null);

                    deckLinkInput.SetCallback(null);

                }

                initialized = false;

                //if (deckLinkNotification != null)
                //{
                //    deckLinkNotification.Unsubscribe(_BMDNotifications.bmdDeviceRemoved | _BMDNotifications.bmdStatusChanged, this);
                //}


                CaptureStopped?.Invoke(null);

                Close();

                captureState = CaptureState.Closed;


            }

            logger.Debug("DeckLinkInput::DoCapture() END");

        }

        void IDeckLinkNotificationCallback.Notify(_BMDNotifications topic, ulong param1, ulong param2)
        {
            logger.Debug("IDeckLinkNotificationCallback.Notify(...) " + topic + " " + param1 + " " + " " + param2);
            //...
        }

        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, 
            IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {// формат меняется на источнике... может измениться в любой момент 
            logger.Debug("IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            if (captureState != CaptureState.Capturing)
            {
                logger.Warn("VideoInputFormatChanged(...): " + captureState);
                return;
            }

            if (!applyDetectedInputMode)
            {
                logger.Warn("applyDetectedInputMode == false");
                return;
            }

            deckLinkInput.PauseStreams();
            deckLinkInput.FlushStreams();

            if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputColorspaceChanged))
            {//  изменилась цветовая схема 
                if (detectedSignalFlags.HasFlag(_BMDDetectedVideoInputFormatFlags.bmdDetectedVideoInputRGB444))
                {
                    this.PixelFormat = _BMDPixelFormat.bmdFormat8BitBGRA; //_BMDPixelFormat.bmdFormat10BitRGB;
                }
                else
                {
                    this.PixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
                }
            }

            if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputDisplayModeChanged))
            {// изменился видео формат
                this.DisplayModeId = newDisplayMode.GetDisplayMode();
            }

            if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputFieldDominanceChanged))
            { // изменилась развертка
                this.FieldDominance = newDisplayMode.GetFieldDominance();
            }

            var videoConnection = GetVideoInputConnections();
            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;
            deckLinkInput.DoesSupportVideoMode(videoConnection, DisplayModeId, PixelFormat, videoModeFlags, out int supported);

            if (supported == 0)
            {//TODO что то пошло не так... закрываем стрим
                string log = "Format not supported: " + string.Join(" ", videoConnection, DisplayModeId, PixelFormat, videoModeFlags);
                logger.Error(log);


                //throw new NotSupportedException(log);
                
            }


            deckLinkInput.EnableVideoInput(DisplayModeId, PixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);

            // deckLinkInput.GetDisplayMode(DisplayMode, out IDeckLinkDisplayMode displayMode);

            newDisplayMode.GetFrameRate(out long frameDuration, out long timeScale);
            int width = newDisplayMode.GetWidth();
            int height = newDisplayMode.GetHeight();

            this.FrameSize = new System.Drawing.Size(width, height);
            this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            logger.Warn("Format changed to: " + DeckLinkTools.LogDisplayMode(newDisplayMode) + " " + PixelFormat);

            var refCount = Marshal.ReleaseComObject(newDisplayMode);
            newDisplayMode = null;

            InputFormatChanged?.Invoke(null);


            deckLinkInput.StartStreams();

        }


        void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame videoFrame, IDeckLinkAudioInputPacket audioPacket)
        {
            try
            {
                if (captureState == CaptureState.Capturing)
                {
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
                else
                {
                    logger.Warn("VideoInputFrameArrived(...): " + captureState);
                }
            }
            finally
            {
                if (audioPacket != null)
                {
                    Marshal.ReleaseComObject(audioPacket);
                    audioPacket = null;
                }

                if (videoFrame != null)
                {
                    Marshal.ReleaseComObject(videoFrame);
                    videoFrame = null;
                }
            }
        }

        private void ProcessAudioPacket(IDeckLinkAudioInputPacket audioPacket)
        {
            if (captureState != CaptureState.Capturing)
            {
                logger.Warn("ProcessAudioPacket(...): " + captureState);
                return;
            }

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

                    var packetTimeSec = (double)packetTime / timeScale;

                    double dataDurationSec = dataLength / audioBytesPerSeconds;

                    // Console.WriteLine("audio: " + packetTimeSec + " " + packetTime + " "+ (packetTimeSec - lastAudioPacketTimeSec));

                    var time = audioDurationSeconds;
                    audioDurationSeconds += dataDurationSec;

                    AudioDataArrived?.Invoke(data, packetTimeSec);
                    lastAudioPacketTimeSec = packetTimeSec;

                }
            }

        }


        private void ProcessVideoFrame(IDeckLinkVideoInputFrame videoFrame)
        {
            if (captureState != CaptureState.Capturing)
            {
                logger.Warn("ProcessVideoFrame(...): " + captureState);
                return;
            }

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

                var frameTimeSec = (double)frameTime / timeScale;
                double frameDurationSec = (double)frameDuration / timeScale;

                //Console.WriteLine("video: " + frameTimeSec + " " + frameTime+ " "+(frameTimeSec - lastVideoFrameTimeSec));

                videoDurationSeconds += frameDurationSec;

                VideoDataArrived?.Invoke(pBuffer, bufferLength, frameTimeSec, frameDurationSec);
                lastVideoFrameTimeSec = frameTimeSec;

                //var fileName = @"d:\testBMP2\" + DateTime.Now.ToString("HH_mm_ss_fff") + " " + width + "x" + height + "_" + format + ".raw";
                //MediaToolkit.Utils.TestTools.WriteFile( pBuffer, bufferLength, fileName);
            }

        }


        public void StopCapture()
        {
            logger.Debug("DeckLinkInput::StopCapture()");

            captureState = CaptureState.Closing;

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


        private void Close()
        {

            logger.Trace("DeckLinkInput::CleanUp()");


            if (deckLink != null)
            {
                int refCount = Marshal.ReleaseComObject(deckLink);
                if (refCount != 0)
                {
                    System.Diagnostics.Debug.Fail("refCount == " + refCount);
                    logger.Warn("deckLink refCount == " + refCount);
                }
                deckLink = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }


        }

        enum CaptureState
        {
            Starting,
            Capturing,
            Closing,
            Closed,
        }

    }

}


/*
private void Setup()
{
    //logger.Trace("StartUp(...)");

    // deckLinkNotification.Subscribe(_BMDNotifications.bmdStatusChanged | _BMDNotifications.bmdDeviceRemoved, this);



    //bool videoInputSignalLocked = false;
    //while (!videoInputSignalLocked && captureState == CaptureState.Starting)
    //{
    //    videoInputSignalLocked = GetVideoInputSignalLockedState();
    //    if (videoInputSignalLocked)
    //    {
    //        break;
    //    }

    //    logger.Debug("videoInputSignalLocked " + videoInputSignalLocked);

    //    syncEvent.WaitOne(1000);

    //}

    // проверяем доступен ли девайс
    //_BMDDeviceBusyState deviceBusyState = _BMDDeviceBusyState.bmdDeviceCaptureBusy;
    //while (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy && captureState == CaptureState.Starting)
    //{
    //    deviceBusyState = GetDeviceBusyState();

    //    if (deviceBusyState != _BMDDeviceBusyState.bmdDeviceCaptureBusy)
    //    {
    //        break;
    //    }

    //    logger.Debug("bmdDeckLinkStatusBusy: " + deviceBusyState);
    //    syncEvent.WaitOne(1000);

    //}

    //if (captureState != CaptureState.Starting)
    //{
    //    return;
    //}

    //deckLink.GetDisplayName(out string displayName);
    //deckLink.GetModelName(out string modelName);

    //this.DisplayName = displayName;
    //this.ModelName = modelName;
    //this.DisplayModeId = GetCurrentVideoInputMode();

    //PixelFormat = GetCurrentVideoInputPixelFormat();
    //if(PixelFormat == _BMDPixelFormat.bmdFormat8BitARGB)
    //{
    //    this.PixelFormat = _BMDPixelFormat.bmdFormat8BitBGRA;
    //}

    ////this.DisplayModeId = _BMDDisplayMode.bmdModeHD1080p30;
    ////this.PixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
    ////this.PixelFormat = _BMDPixelFormat.bmdFormat8BitBGRA;

    //_BMDVideoConnection videoConnections = GetVideoInputConnections();
    //logger.Info(string.Join("; ", DisplayName, videoConnections, DisplayModeId, PixelFormat));

    //supportsFormatDetection = GetSupportsFormatDetection();
    //supportsHDMITimecode = GetSupportsHDMITimecode();


    //memoryAllocator = new MemoryAllocator();
    //deckLinkInput.SetVideoInputFrameMemoryAllocator(memoryAllocator);

    //if (previewCallback != null)
    //{
    //    deckLinkInput.SetScreenPreviewCallback(previewCallback);
    //}

    //deckLinkInput.SetCallback(this);

    //var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
    //if (supportsFormatDetection && applyDetectedInputMode)
    //{
    //    videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
    //}
    //deckLinkInput.EnableVideoInput(DisplayModeId, PixelFormat, videoInputFlags);

    //if (AudioEnabled)
    //{
    //    deckLinkInput.EnableAudioInput(audioSampleRate, audioSampleType, (uint)AudioChannelsCount);
    //    audioBytesPerSeconds = ((int)audioSampleRate * (int)AudioChannelsCount * (int)audioSampleType) / 8;
    //}


    //IDeckLinkDisplayMode displayMode = null;
    //try
    //{
    //    deckLinkInput.GetDisplayMode(DisplayModeId, out displayMode);

    //    int width = displayMode.GetWidth();
    //    int height = displayMode.GetHeight();
    //    displayMode.GetFrameRate(out long frameDuration, out long timeScale);

    //    this.FrameSize = new System.Drawing.Size(width, height);
    //    this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);

    //    logger.Info("Start input stream: " + DeckLinkTools.LogDisplayMode(displayMode) + " " + PixelFormat);
    //}
    //finally
    //{
    //    Marshal.ReleaseComObject(displayMode);
    //    displayMode = null;
    //}

    //this.lastAudioPacketTimeSec = 0;
    //this.lastVideoFrameTimeSec = 0;

    //this.validInputSignal = false;

    //initialized = true;

    //ReadyToStart?.Invoke();

}
*/

