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

        //TODO: на данный момент удалось проверить девайс только с одним форматом  - bmdModeHD1080p5994
        // как включать другие форматы?!?  
        public _BMDDisplayMode DisplayModeId { get; private set; } = _BMDDisplayMode.bmdModeUnknown;//bmdModeHD1080p30;//_BMDDisplayMode.bmdModeHD1080p5994;

        public _BMDPixelFormat PixelFormat { get; private set; } = _BMDPixelFormat.bmdFormatUnspecified;
        public string PixelFormatCode => DeckLinkTools.GetPixelFormatFourCC(this.PixelFormat);
        public System.Drawing.Size FrameSize { get; private set; } = System.Drawing.Size.Empty;
        public Tuple<long, long> FrameRate { get; private set; }

        public bool AudioEnabled { get; private set; } = true;
        private _BMDAudioSampleType audioSampleType = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public int AudioBitsPerSample => (int)audioSampleType;

        private _BMDAudioSampleRate audioSampleRate = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
        public int AudioSampleRate => (int)audioSampleRate;
        public int AudioChannelsCount { get; private set; } = 2;
        private double audioBytesPerSeconds = 0;

       // private volatile bool isCapturing = false;
        //public bool IsCapturing => isCapturing;

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
        public event Action ReadyToStart;

        // IDeckLinkMemoryAllocator memoryAllocator = new MemoryAllocator();

        private double audioDurationSeconds = 0;
        private double videoDurationSeconds = 0;

        private double lastAudioPacketTimeSec = 0;
        private double lastVideoFrameTimeSec = 0;

        private int inputIndex = -1;
        private IDeckLinkScreenPreviewCallback previewCallback = null;

        private volatile CaptureState captureState = CaptureState.Closed;


        //private volatile bool isRunning = false;
        public void StartCapture(int inputIndex)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + inputIndex);

            if (captureState != CaptureState.Closed)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);
                return;
            }

            captureState = CaptureState.Starting;

            this.inputIndex = inputIndex;

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

            if (captureState != CaptureState.Starting)
            {
                logger.Warn("DeckLinkInput::StartCapture(...) return invalid state: " + captureState);

                return;
            }

            bool streamStarted = false;
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

                Setup();

                if(captureState == CaptureState.Starting)
                {
                    logger.Debug("DeckLinkInput start capturing: " + DisplayName);

                    deckLinkInput.StartStreams();
                    streamStarted = true;

                    CaptureStarted?.Invoke();

                    captureState = CaptureState.Started;

                    while (captureState == CaptureState.Started)
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
                        //    + "| vt " + videoTime1 + " - " + videoTime2 + " " + adiff1);

                        //Console.WriteLine("adiff: " + audioDiff.ToString("0.000") + " adiff1: " + audioDiff1.ToString("0.000") + 
                        //    " vdiff: " + videoDiff.ToString("0.000") + " vdiff1: " + videoDiff1.ToString("0.000"));
                        //Console.SetCursorPosition(0, Console.CursorTop - 1);

                        if (errorCode != 0)
                        {
                            break;
                        }

                        syncEvent.WaitOne(1000);


                        logger.Debug("DeckLinkInput capture result: " + errorCode);
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

                    // deckLinkInput.SetVideoInputFrameMemoryAllocator(null);

                    deckLinkInput.SetScreenPreviewCallback(null);

                    deckLinkInput.SetCallback(null);

                }

                //if (deckLinkNotification != null)
                //{
                //    deckLinkNotification.Unsubscribe(_BMDNotifications.bmdDeviceRemoved | _BMDNotifications.bmdStatusChanged, this);
                //}


                CaptureStopped?.Invoke(null);

                CleanUp();

                captureState = CaptureState.Closed;


            }

            logger.Debug("DeckLinkInput::DoCapture() END");

        }


        private void Setup()
        {
            logger.Trace("StartUp(...)");

            // deckLinkNotification.Subscribe(_BMDNotifications.bmdStatusChanged | _BMDNotifications.bmdDeviceRemoved, this);

            //if (!videoInputSignalLocked)
            //{
            //    throw new Exception("Video input locked");
            //}

            bool videoInputSignalLocked = false;
            do
            {
                videoInputSignalLocked = GetVideoInputSignalLockedState();
                if (videoInputSignalLocked)
                {
                    break;
                }
                logger.Warn("videoInputSignalLocked " + videoInputSignalLocked);

                syncEvent.WaitOne(1000);

            } while (!videoInputSignalLocked && captureState == CaptureState.Starting);

            if (captureState != CaptureState.Starting)
            {
                return;
            }

            // проверяем доступен ли девайс
            //_BMDDeviceBusyState deviceBusyState = GetDeviceBusyState();
            //if (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy)
            //{// 
            //    //TODO: device busy...
            //    logger.Warn("bmdDeckLinkStatusBusy: " + deviceBusyState);
            //}

            _BMDDeviceBusyState deviceBusyState = _BMDDeviceBusyState.bmdDeviceCaptureBusy;
            do
            {
                deviceBusyState = GetDeviceBusyState();

                if (deviceBusyState != _BMDDeviceBusyState.bmdDeviceCaptureBusy)
                {
                    break;
                }

                logger.Warn("bmdDeckLinkStatusBusy: " + deviceBusyState);
                syncEvent.WaitOne(1000);


            } while (deviceBusyState == _BMDDeviceBusyState.bmdDeviceCaptureBusy && captureState == CaptureState.Starting);

            if (captureState != CaptureState.Starting)
            {
                return;
            }

            deckLink.GetDisplayName(out string displayName);
            deckLink.GetModelName(out string modelName);

            this.DisplayName = displayName;
            this.ModelName = modelName;
            this.DisplayModeId = GetCurrentVideoInputMode();
            // this.PixelFormat = GetCurrentVideoInputPixelFormat();
            //this.DisplayModeId = _BMDDisplayMode.bmdModeHD1080p30;
            this.PixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;

            _BMDVideoConnection videoConnections = GetVideoInputConnections();
            logger.Info(string.Join("; ", DisplayName, videoConnections, DisplayModeId, PixelFormat));

            supportsFormatDetection = GetSupportsFormatDetection();
            supportsHDMITimecode = GetSupportsHDMITimecode();


            //memoryAllocator = new MemoryAllocator();
            //deckLinkInput.SetVideoInputFrameMemoryAllocator(memoryAllocator);

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

        }


        void IDeckLinkNotificationCallback.Notify(_BMDNotifications topic, ulong param1, ulong param2)
        {
            logger.Debug("IDeckLinkNotificationCallback.Notify(...) " + topic + " " + param1 + " " + " " + param2);
            //...
        }

        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {// не понятно как менять форматы у девайса
            logger.Debug("IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            if (captureState != CaptureState.Started)
            {
                logger.Warn("VideoInputFormatChanged(...): " + captureState);
                return;
            }

            if (!applyDetectedInputMode)
            {
                logger.Warn("applyDetectedInputMode == false");
                return;
            }


            if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputColorspaceChanged))
            {//  изменилась цветовая схема 


            }
            else if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputDisplayModeChanged))
            {

            }
            else if (notificationEvents.HasFlag(_BMDVideoInputFormatChangedEvents.bmdVideoInputFieldDominanceChanged))
            {// ...

            }

            var newDisplayModeId = newDisplayMode.GetDisplayMode();
            var newPixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
            if (detectedSignalFlags.HasFlag(_BMDDetectedVideoInputFormatFlags.bmdDetectedVideoInputRGB444))
            {
                newPixelFormat = _BMDPixelFormat.bmdFormat8BitARGB; //_BMDPixelFormat.bmdFormat10BitRGB;
            }

            var videoConnection = GetVideoInputConnections();
            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;

            deckLinkInput.DoesSupportVideoMode(videoConnection, newDisplayModeId, newPixelFormat, videoModeFlags, out int supported);

            if (supported == 0)
            {
                logger.Error("Format not supported: " + string.Join(" ", videoConnection, newDisplayModeId, newPixelFormat, videoModeFlags));
                //TODO: что то пошло не так... закрываем стрим
            }

            this.PixelFormat = newPixelFormat;
            this.DisplayModeId = newDisplayModeId;

            //// Stop the capture
            //deckLinkInput.StopStreams();

            deckLinkInput.PauseStreams();
            deckLinkInput.FlushStreams();

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
            if (captureState != CaptureState.Started)
            {
                logger.Warn("VideoInputFrameArrived(...): " + captureState);

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
            //Console.WriteLine();

        }

        private void ProcessAudioPacket(IDeckLinkAudioInputPacket audioPacket)
        {
            try
            {
                if (captureState == CaptureState.Started)
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
                else
                {
                    logger.Warn("ProcessAudioPacket(...): " + captureState);
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
            try
            {
                if (captureState == CaptureState.Started)
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
                else
                {
                    logger.Warn("ProcessVideoFrame(...): " + captureState);
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

        private bool clenedUp = false;
        private void CleanUp()
        {

            logger.Trace("DeckLinkInput::CleanUp()");

            clenedUp = true;

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
            Started,
            Closing,
            Closed,
        }

    }

}
