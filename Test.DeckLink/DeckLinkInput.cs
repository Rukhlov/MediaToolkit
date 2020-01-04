using DeckLinkAPI;
using NLog;
using System;
using System.Collections.Generic;
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

        private Thread captureThread = null;


        private IDeckLinkInput deckLinkInput = null;
        private IDeckLink deckLink = null;
        private IDeckLinkStatus deckLinkStatus;

        public _BMDDisplayMode BMDDisplayMode { get; private set; } = _BMDDisplayMode.bmdModeHD1080p5994;

        public string DisplayName { get; private set; } = "";

        public string ModelName { get; private set; } = "";

        public _BMDPixelFormat VideoPixelFormat { get; private set; } = _BMDPixelFormat.bmdFormat8BitYUV;
        public int VideoFrameWidth { get; private set; } = 1920;
        public int VideoFrameHeigth { get; private set; } = 1080;

        public _BMDAudioSampleType AudioSampleType { get; private set; } = _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public _BMDAudioSampleRate AudioSampleRate { get; private set; } = _BMDAudioSampleRate.bmdAudioSampleRate48kHz;
        public int AudioChannelsCount { get; private set; } = 2;

        private volatile bool isCapturing = false;
        public bool IsCapturing => isCapturing;

        private bool supportsFormatDetection = true;
        private bool applyDetectedInputMode = true;

        private volatile bool validInputSignal = false;

        private AutoResetEvent syncEvent = new AutoResetEvent(false);


        public event DeckLinkInputSignalHandler InputSignalChanged;
        public event DeckLinkFormatChangedHandler InputFormatChanged;

        public event Action<byte[], long> AudioDataArrived;
        public event Action<IntPtr, int, long> VideoDataArrived;

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        public void StartCapture(int inputIndex)
        {
            logger.Debug("DeckLinkInput::StartCapture(...) " + inputIndex);

            if (disposed)
            {
                throw new ObjectDisposedException(this.ToString());
            }

            if (isCapturing )
            {
                return;
            }

            captureThread = new Thread(DoCapture);
            captureThread.SetApartmentState(ApartmentState.MTA);

            captureThread.Start(inputIndex);

        }

        private void DoCapture(object inputArgs)
        {

            logger.Debug("DeckLinkInput::DoCapture(...) BEGIN " + inputArgs.ToString());

            if (isCapturing)
            {
                return;
            }


            var inputIndex = (int)inputArgs;
            

            IDeckLinkInput _deckLinkInput = null;
            IDeckLink _deckLink = null;

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
                        //_deckLinkInput = (IDeckLinkInput)_deckLink;

                        //Console.WriteLine("_deckLinkInput " + Thread.CurrentThread.ManagedThreadId);
                        break;
                    }

                    index++;
                }
                while (_deckLink != null);

            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (deckLinkIterator == null)
                {
                    errorMessage = "This application requires the DeckLink drivers installed.\n" +
                        "Please install the Blackmagic DeckLink drivers to use the features of this application";
                }

                //return false

            }
            finally
            {
                if (deckLinkIterator != null)
                {
                    Marshal.ReleaseComObject(deckLinkIterator);
                    deckLinkIterator = null;
                }

            }


            //if (_deckLinkInput == null)
            //{
            //    // not found return...

            //    throw new Exception("Input not found ");
            //}

            try
            {
                this.deckLink = _deckLink;
                this.deckLinkInput = (IDeckLinkInput)deckLink;
                this.deckLinkStatus = (IDeckLinkStatus)_deckLink;

                //...
                deckLink.GetDisplayName(out string displayName);
                deckLink.GetModelName(out string modelName);

                this.DisplayName = displayName;
                this.ModelName = modelName;


                IDeckLinkProfileAttributes attrs = (IDeckLinkProfileAttributes)deckLink;
                attrs.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsInputFormatDetection, out int supportsFormatDetectionFlag);
                supportsFormatDetection = supportsFormatDetectionFlag != 0;

                validInputSignal = false;

                var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
                // Enable input video mode detection if the device supports it
                if (supportsFormatDetection && applyDetectedInputMode)
                {
                    videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
                }
                // Console.WriteLine("deckLinkInput " + Thread.CurrentThread.ManagedThreadId);


                deckLinkInput.SetCallback(this);

                deckLinkInput.EnableVideoInput(BMDDisplayMode, VideoPixelFormat, videoInputFlags);

                deckLinkInput.EnableAudioInput(AudioSampleRate, AudioSampleType, (uint)AudioChannelsCount);

                deckLinkInput.StartStreams();


                logger.Debug("DeckLinkInput::DeckLinkInput: " + displayName);

    
                isCapturing = true;

                CaptureStarted?.Invoke();
                while (isCapturing)
                {

                    //...
                    syncEvent.WaitOne(1000);
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex);
                //...
            }
            finally
            {

                deckLinkInput.StopStreams();

                deckLinkInput.SetCallback(null);

                isCapturing = false;

                CaptureStopped?.Invoke(null);

                Dispose();

            }


            logger.Debug("DeckLinkInput::DoCapture() END");

        }



        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {
            logger.Debug("DeckLinkInput::IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            // Restart capture with the new video mode if told to
            if (!applyDetectedInputMode)
            {
                return;
            }

            VideoPixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
            BMDDisplayMode = newDisplayMode.GetDisplayMode();

            var videoConnection = _BMDVideoConnection.bmdVideoConnectionHDMI;
            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;
            deckLinkInput.DoesSupportVideoMode(videoConnection, BMDDisplayMode, VideoPixelFormat, videoModeFlags, out int supported);

            logger.Debug(VideoPixelFormat + " " + supported);


            deckLinkInput.PauseStreams();
            deckLinkInput.FlushStreams();

            //// Stop the capture
            //deckLinkInput.StopStreams();


            deckLinkInput.EnableVideoInput(BMDDisplayMode, VideoPixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);

            deckLinkInput.StartStreams();

            logger.Debug("DeckLinkInput::NewDisplayMode: " + newDisplayMode.GetDisplayMode());

            InputFormatChanged?.Invoke(newDisplayMode);

        }


        void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame videoFrame, IDeckLinkAudioInputPacket audioPacket)
        {
            if (!isCapturing)
            {
                return;
            }

            if (videoFrame != null)
            {

                try
                {
                    var frameFlags = videoFrame.GetFlags();

                    bool inputSignal = frameFlags.HasFlag(_BMDFrameFlags.bmdFrameHasNoInputSource);

                    if (inputSignal != validInputSignal)
                    {
                        validInputSignal = inputSignal;
                        InputSignalChanged?.Invoke(validInputSignal);
                    }
                    else
                    {

                        int width = videoFrame.GetWidth();
                        int height = videoFrame.GetHeight();
                        int stride = videoFrame.GetRowBytes();
                        var format = videoFrame.GetPixelFormat();

                        var bufferLength = stride * height;
                        videoFrame.GetBytes(out IntPtr pBuffer);

                        VideoDataArrived?.Invoke(pBuffer, bufferLength, 0);


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


            if (audioPacket != null)
            {
                try
                {
                    long packetTime = 0;
                    //audioPacket.GetPacketTime(out packetTime, 30000);

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

                            AudioDataArrived?.Invoke(data, packetTime);

                        }
                    }
                }
                finally
                {
                    Marshal.ReleaseComObject(audioPacket);
                }

            }

        }


        public void StopCapture()
        {
            logger.Debug("DeckLinkInput::StopCapture()");

            isCapturing = false;
            syncEvent?.Set();
        }

        private bool disposed = false;
        public void Dispose()
        {

            logger.Debug("DeckLinkInput::Dispose()");
            disposed = true;

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

    }


}
