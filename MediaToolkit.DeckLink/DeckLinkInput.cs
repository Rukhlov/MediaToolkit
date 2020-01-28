using DeckLinkAPI;
using MediaToolkit.Core;
using MediaToolkit.SharedTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.DeckLink
{

    public class DeckLinkInput : IDeckLinkInputCallback, IDeckLinkInputDevice
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDeckLinkInput deckLinkInput = null;
        private IDeckLink deckLink = null;
        private IDeckLinkStatus deckLinkStatus = null;
        private IDeckLinkProfileAttributes deckLinkProfileAttrs = null;

        private IDeckLinkMemoryAllocator memoryAllocator = null;

        private IDeckLinkScreenPreviewCallback previewCallback = null;

        public int DeviceIndex { get; private set; } = -1;
        public string DeviceHandle { get; private set; } = "";
        public string DisplayName { get; private set; } = "";
        public string ModelName { get; private set; } = "";

        // форматы можно настроить на источнике видео 
        public _BMDDisplayMode DisplayModeId { get; private set; } = _BMDDisplayMode.bmdModeUnknown;//bmdModeHD1080p30;//_BMDDisplayMode.bmdModeHD1080p5994;

        private _BMDPixelFormat pixelFormat = _BMDPixelFormat.bmdFormatUnspecified;
        public _BMDPixelFormat PixelFormat { get => pixelFormat; set => pixelFormat = value; }

        public VideoFormat VideoFormat => DeckLinkTools.GetVideoFormat(this.PixelFormat);
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

        private volatile ErrorCode errorCode = 0;
        public ErrorCode ErrorCode => errorCode;

        public CaptureState State => captureState;
        private volatile CaptureState captureState = CaptureState.Shutdown;

        public CaptureStats Statistics { get; private set; } = new CaptureStats(); 
        private bool supportsFormatDetection = true;
        private bool applyDetectedInputMode = true;
        private bool supportsHDMITimecode = false;


        private AutoResetEvent syncEvent = null;
        private Thread captureThread = null;

        public event Action<bool> CaptureChanged;
        private void OnCaptureChanged(bool formatChanged = false)
        {
            CaptureChanged?.Invoke(formatChanged);
        }

        //public event Action<byte[], double> AudioDataArrived;
        public event Action<IntPtr, int, double, double> AudioDataArrived;
        public event Action<IntPtr, int, double, double> VideoDataArrived;


        private volatile _BMDFrameFlags lastFrameFlags = _BMDFrameFlags.bmdFrameFlagDefault;

        private double lastAudioPacketTimeSec = 0;
        private double lastVideoFrameTimeSec = 0;

        private double audioDurationSeconds = 0;
        private double videoDurationSeconds = 0;

        private uint lostAudioPacketsCount = 0;
        public uint lostVideoFramesCount = 0;

        public void StartCapture(DeckLinkDeviceDescription device, DeckLinkDisplayModeDescription mode = null)
        {
            var index = device.DeviceIndex;
            var modeId = mode?.ModeId ?? (long)_BMDDisplayMode.bmdModeUnknown;
            var pixFormat = mode?.PixFmt ?? (long)_BMDPixelFormat.bmdFormatUnspecified;

            StartCapture(index, pixFormat, modeId);
        }

        public void StartCapture(int inputIndex,
            long pixFmt = (long)_BMDPixelFormat.bmdFormatUnspecified,
            long modeId = (long)_BMDDisplayMode.bmdModeUnknown)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + string.Join(" ", inputIndex, pixFmt, modeId));

            if (captureState != CaptureState.Shutdown)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);
                return;
            }

            captureState = CaptureState.Starting;

            var captureArgs = new Dictionary<string, object>
            {
                { "DeviceIndex", inputIndex },
                { "PixelFormat", (_BMDPixelFormat)pixFmt },
                { "DisplayModeId", (_BMDDisplayMode)modeId },
            };

            captureThread = new Thread(DoCapture);
            // IDeckLink требует MTA!!!
            // т.е все вызовы внутри потока
            captureThread.SetApartmentState(ApartmentState.MTA);
            captureThread.IsBackground = true;
            captureThread.Start(captureArgs);

        }

        private void DoCapture(object args)
        {

            logger.Debug("DeckLinkInput::DoCapture(...) BEGIN");

            if (captureState != CaptureState.Starting)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);

                return;
            }

            syncEvent = new AutoResetEvent(false);
            errorCode = ErrorCode.Ok;
            bool streamStarted = false;
            try
            {

                var captureParams = args as Dictionary<string, object>;
                if (captureParams != null)
                {
                    this.DeviceIndex = (int)captureParams["DeviceIndex"];

                    this.PixelFormat = (_BMDPixelFormat)captureParams["PixelFormat"];
                    this.DisplayModeId = (_BMDDisplayMode)captureParams["DisplayModeId"];

                    if (this.DisplayModeId != _BMDDisplayMode.bmdModeUnknown)
                    {
                        applyDetectedInputMode = false;
                    }
                }

                do
                {

                    DeckLinkTools.GetDeviceByIndex(DeviceIndex, out IDeckLink _deckLink);

                    if (_deckLink == null)
                    {
                        errorCode = ErrorCode.NotFound;
                        logger.Warn("Device not found: " + DeviceIndex);

                        break;
                    }

                    this.deckLink = _deckLink;
                    this.deckLinkInput = (IDeckLinkInput)deckLink;
                    this.deckLinkStatus = (IDeckLinkStatus)deckLink;
                    this.deckLinkProfileAttrs = (IDeckLinkProfileAttributes)deckLink;


                    //bool videoInputSignalLocked = GetVideoInputSignalLockedState();
                    //if (!videoInputSignalLocked)
                    //{
                    //    logger.Warn("VideoInputSignalLockedState() " + videoInputSignalLocked);
                    //    errorCode = ErrorCode.NotReady;
                    //    break;
                    //}

                    _BMDDeviceBusyState deviceBusyState = GetDeviceBusyState();
                    if (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy)
                    {
                        logger.Debug("bmdDeckLinkStatusBusy: " + deviceBusyState);
                        errorCode = ErrorCode.IsBusy;
                        break;

                    }

                    this.DeviceHandle = GetDeviceHandle();

                    deckLink.GetDisplayName(out string displayName);
                    deckLink.GetModelName(out string modelName);

                    this.DisplayName = displayName;
                    this.ModelName = modelName;

                    if (DisplayModeId == _BMDDisplayMode.bmdModeUnknown)
                    {
                        DisplayModeId = GetCurrentVideoInputMode();
                    }

                    if (PixelFormat == _BMDPixelFormat.bmdFormatUnspecified)
                    {// 
                        PixelFormat = GetCurrentVideoInputPixelFormat();
                    }

                    bool pixelFormatSupported = DeckLinkTools.ValidateAndCorrectPixelFormat(ref pixelFormat);
                    if (!pixelFormatSupported)
                    {
                        // throw new NotSupportedException("Pixel format not supported: " + PixelFormat);
                        PixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
                    }

                    _BMDVideoConnection videoConnections = GetVideoInputConnections();
                    logger.Info(string.Join("; ", DisplayName, videoConnections, DisplayModeId, PixelFormat));

                    supportsFormatDetection = GetSupportsFormatDetection();
                    supportsHDMITimecode = GetSupportsHDMITimecode();


                   // memoryAllocator = new MemoryAllocator(5);
                    memoryAllocator = new SimpleMemoryAllocator(5);
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
                        audioBytesPerSeconds = ((int)audioSampleRate * AudioChannelsCount * (int)audioSampleType) / 8;
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


                    OnCaptureChanged(true);


                    this.lastAudioPacketTimeSec = 0;
                    this.lastVideoFrameTimeSec = 0;
                    this.lastFrameFlags = _BMDFrameFlags.bmdFrameFlagDefault;


                    uint _lostAudioPacketsCount = lostAudioPacketsCount;
                    uint _lostVideoFramesCount = lostVideoFramesCount;
                    bool _noSignal = lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource) ||
                        lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputDevice);

                    if (captureState == CaptureState.Starting )
                    {
                        logger.Debug("DeckLinkInput start capturing: " + DisplayName);

                        deckLinkInput.StartStreams();
                        streamStarted = true;

                        captureState = CaptureState.Capturing;
                        OnCaptureChanged();
                    }
                    else
                    {
                        errorCode = ErrorCode.Cancelled;
                        logger.Warn("Capture cancelled...");
                        break;
                    }

                    while (captureState == CaptureState.Capturing)
                    {

                        bool noSignal = lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource) ||
                            lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputDevice);


                        if (_noSignal != noSignal)
                        {
                            _noSignal = noSignal;

                        }

                        if (_noSignal)
                        {
                            logger.Warn("No signal...");
                        }

                        var audioPacketLoss = (lostAudioPacketsCount - _lostAudioPacketsCount);
                        if (audioPacketLoss > 0)
                        {
                            _lostAudioPacketsCount = lostAudioPacketsCount;
                            logger.Warn("Audio packets loss: " + audioPacketLoss);
                        }

                        var videoFrameLoss = (lostVideoFramesCount - _lostVideoFramesCount);
                        if (videoFrameLoss > 0)
                        {
                            _lostVideoFramesCount = lostVideoFramesCount;
                            logger.Warn("Video frame loss: " + videoFrameLoss);
                        }

                        Statistics.NoSignal = noSignal;
                        Statistics.VideoFramesLoss = videoFrameLoss;
                        Statistics.AudioPacketsLoss = audioPacketLoss;

                        if (errorCode != 0)
                        {
                            break;
                        }

                        syncEvent.WaitOne(1000);

                    }

                    logger.Debug("DeckLinkInput capture result: " + errorCode);
                }
                while (false);

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                captureState = CaptureState.Stopping;
                streamStarted = false;

                errorCode = ErrorCode.Fail;
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

                //if (deckLinkNotification != null)
                //{
                //    deckLinkNotification.Unsubscribe(_BMDNotifications.bmdDeviceRemoved | _BMDNotifications.bmdStatusChanged, this);
                //}

                captureState = CaptureState.Stopped;
                OnCaptureChanged();

                //Shutdown();

            }

            logger.Debug("DeckLinkInput::DoCapture() END " + errorCode);

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
            {//что то пошло не так... закрываем стрим
                string log = "Format not supported: " + string.Join(" ", videoConnection, DisplayModeId, PixelFormat, videoModeFlags);
                logger.Error(log);

                errorCode = ErrorCode.Unexpected;
                return;

                //throw new NotSupportedException(log);

            }

            deckLinkInput.EnableVideoInput(DisplayModeId, PixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);


            newDisplayMode.GetFrameRate(out long frameDuration, out long timeScale);
            int width = newDisplayMode.GetWidth();
            int height = newDisplayMode.GetHeight();

            this.FrameSize = new System.Drawing.Size(width, height);
            this.FrameRate = new Tuple<long, long>(frameDuration, timeScale);

            logger.Warn("Format changed to: " + DeckLinkTools.LogDisplayMode(newDisplayMode) + " " + PixelFormat);

            var refCount = Marshal.ReleaseComObject(newDisplayMode);
            newDisplayMode = null;

            //inputFormatChanged = true;
            //syncEvent?.Set();

            OnCaptureChanged(true);

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
                        //logger.Warn("No video frame...");
                        lostVideoFramesCount++;
                    }

                    if (AudioEnabled)
                    {
                        if (audioPacket != null)
                        {
                            ProcessAudioPacket(audioPacket);
                        }
                        else
                        {
                            //logger.Warn("No audio packet...");
                            lostAudioPacketsCount++;
                        }
                    }
                }
                else
                {
                    logger.Warn("VideoInputFrameArrived(...): " + captureState);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                errorCode = ErrorCode.Fail;
            }
            finally
            {
                if (audioPacket != null)
                {
                    int refCount = Marshal.ReleaseComObject(audioPacket);
                    Debug.Assert(refCount == 0, "refCount == 0");
                    audioPacket = null;
                }

                if (videoFrame != null)
                {
                    int refCount = Marshal.ReleaseComObject(videoFrame);
                    Debug.Assert(refCount == 0, "refCount == 0");
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
                    var packetTimeSec = (double)packetTime / timeScale;
                    double dataDurationSec = dataLength / audioBytesPerSeconds;
                    audioDurationSeconds += dataDurationSec;

                    // Console.WriteLine("audio: " + packetTimeSec + " " + packetTime + " "+ (packetTimeSec - lastAudioPacketTimeSec));

                    AudioDataArrived?.Invoke(pBuffer, dataLength, packetTimeSec, dataDurationSec);
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

            lastFrameFlags = videoFrame.GetFlags();
            if (lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource) ||
                lastFrameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputDevice))
            {
                //logger.Warn("Video no signal...");
                return;
            }

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
            videoDurationSeconds += frameDurationSec;

            //Console.WriteLine("video: " + frameTimeSec + " " + frameTime+ " "+(frameTimeSec - lastVideoFrameTimeSec));

            VideoDataArrived?.Invoke(pBuffer, bufferLength, frameTimeSec, frameDurationSec);
            lastVideoFrameTimeSec = frameTimeSec;

            //var fileName = @"d:\testBMP2\" + DateTime.Now.ToString("HH_mm_ss_fff") + " " + width + "x" + height + "_" + format + ".raw";
            //MediaToolkit.Utils.TestTools.WriteFile( pBuffer, bufferLength, fileName);


        }


        public void StopCapture()
        {
            logger.Debug("DeckLinkInput::StopCapture()");

            captureState = CaptureState.Stopping;

            syncEvent?.Set();
        }

        private string GetDeviceHandle()
        {
            deckLinkProfileAttrs.GetString(_BMDDeckLinkAttributeID.BMDDeckLinkDeviceHandle, out string deviceHandle);

            return deviceHandle;
        }

        private bool GetVideoInputSignalLockedState()
        {
            deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
            bool videoInputSignalLocked = (videoInputSignalLockedFlag != 0);
            return videoInputSignalLocked;
        }

        private _BMDDeviceBusyState GetDeviceBusyState()
        {
            deckLinkStatus.GetInt(_BMDDeckLinkStatusID.bmdDeckLinkStatusBusy, out long deviceBusyStateFlag);
            _BMDDeviceBusyState deviceBusyState = (_BMDDeviceBusyState)deviceBusyStateFlag;
            return deviceBusyState;
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






        public void Shutdown()
        {

            logger.Trace("DeckLinkInput::Shutdown()");


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

            captureState = CaptureState.Shutdown;

        }


    }

    public class CaptureStats
    {
        public bool NoSignal { get; set; } = false;

        public uint AudioPacketsLoss { get; set; } = 0;
        public uint VideoFramesLoss { get; set; } = 0;

        public uint AudioPackesTotalReceiveCount { get; set; } = 0;
        public uint VideoFramesTotalReceiveCount { get; set; } = 0;
        public double Fps { get; set; } = 0;

        //...
    }

    public enum CaptureState
    {
        Starting,
        Capturing,
        Stopping,
        Stopped,
        Shutdown,
    }

}


