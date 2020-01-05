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

    public delegate void DeckLinkInputSignalHandler(bool inputSignal);
    public delegate void DeckLinkFormatChangedHandler(IDeckLinkDisplayMode newDisplayMode);

    public class DeckLinkDevice : IDeckLinkInputCallback, IEnumerable<IDeckLinkDisplayMode>
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private IDeckLink deckLink;
        private IDeckLinkInput deckLinkInput;
        private IDeckLinkStatus deckLinkStatus;

        private bool applyDetectedInputMode = true;

        private bool validInputSignal = false;


        public DeckLinkDevice(IDeckLink deckLink)
        {
            this.deckLink = deckLink;

            // Get input interface
            try
            {
                deckLinkInput = (IDeckLinkInput)this.deckLink;
                deckLinkStatus = (IDeckLinkStatus)this.deckLink;

                bool videoInputSignalLocked = false;
                deckLinkStatus.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out int videoInputSignalLockedFlag);
                videoInputSignalLocked = (videoInputSignalLockedFlag != 0);

                if (videoInputSignalLocked)
                {
                    throw new Exception("Video input locked"); 
                }

                this.deckLink.GetDisplayName(out deviceName);
                this.deckLink.GetModelName(out modelName);


                var deckLinkAttributes = (IDeckLinkProfileAttributes)deckLink;
                deckLinkAttributes.GetFlag(_BMDDeckLinkAttributeID.BMDDeckLinkSupportsInputFormatDetection, out int supportsFormatDetectionFlag);
                supportsFormatDetection = (supportsFormatDetectionFlag != 0);
              

                logger.Debug("------------------------- " + deviceName + " -------------------------");

                IDeckLinkDisplayModeIterator iterator = null;
                deckLinkInput.GetDisplayModeIterator(out iterator);
                IDeckLinkDisplayMode displayMode = null;
                do
                {
                    if (iterator != null)
                    {
                        iterator.Next(out displayMode);
                        if (displayMode != null)
                        {
                            displayMode.GetName(out string displayName);
                            displayMode.GetFrameRate(out long frameDuration, out long timeScale);

                            int width = displayMode.GetWidth();
                            int height = displayMode.GetHeight();
                            var bdmDisplayMode = displayMode.GetDisplayMode();
                            var displayModeFlags = displayMode.GetFlags();
                            var fieldDominance = displayMode.GetFieldDominance();


                            var resolution = width + "x" + height;

                           // var log = string.Join(", " , displayName, resolution, bdmDisplayMode, displayModeFlags, frameDuration, timeScale, fieldDominance);
                            var videoModeFlags = _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault;

                            //var allPixelFormats = Enum.GetValues(typeof(_BMDPixelFormat));
                            var formatLog = "";
                            foreach(var fmtObj in pixelFormats)
                            {
                                var pixelFormat = (_BMDPixelFormat)fmtObj;
                                deckLinkInput.DoesSupportVideoMode(_BMDVideoConnection.bmdVideoConnectionHDMI, bdmDisplayMode, pixelFormat, videoModeFlags, out int supported);
                                if (supported != 0)
                                {
                                    formatLog += " " + pixelFormat;
                                }

                            }

                            var log = string.Join(", ", displayName, resolution );
                            logger.Debug(displayName + " " + resolution + " (" + formatLog + ")");
                        }
                        else
                        {
                            break;
                        }
                 
                    }  

                }
                while (displayMode != null);


                logger.Debug("-----------------------------------------------------");

            }
            catch (InvalidCastException)
            {
                // No output interface found, eg in case of DeckLink Mini Monitor
                return;
            }
        }

        //{ bmdFormat8BitYUV,     "8-bit YUV" },
        //{ bmdFormat10BitYUV,    "10-bit YUV" },
        //{ bmdFormat8BitARGB,    "8-bit ARGB" },
        //{ bmdFormat8BitBGRA,    "8-bit BGRA" },
        //{ bmdFormat10BitRGB,    "10-bit RGB" },
        //{ bmdFormat12BitRGB,    "12-bit RGB" },
        //{ bmdFormat12BitRGBLE,  "12-bit RGBLE" },
        //{ bmdFormat10BitRGBXLE, "10-bit RGBXLE" },
        //{ bmdFormat10BitRGBX,   "10-bit RGBX" },

        List<_BMDPixelFormat> pixelFormats = new List<_BMDPixelFormat>
        {
            _BMDPixelFormat.bmdFormat8BitYUV,
            _BMDPixelFormat.bmdFormat10BitYUV,
            _BMDPixelFormat.bmdFormat8BitARGB,
            _BMDPixelFormat.bmdFormat8BitBGRA,
            _BMDPixelFormat.bmdFormat10BitRGB,
            _BMDPixelFormat.bmdFormat12BitRGB,
            _BMDPixelFormat.bmdFormat12BitRGBLE,
            _BMDPixelFormat.bmdFormat10BitRGBXLE,
            _BMDPixelFormat.bmdFormat10BitRGBX,

        };

        public event DeckLinkInputSignalHandler InputSignalChanged;
        public event DeckLinkFormatChangedHandler InputFormatChanged;

        public event Action<byte[], long> AudioDataArrived;
        public event Action<IntPtr, int, long> VideoDataArrived;

        public IDeckLink DeckLink => deckLink;
        public IDeckLinkInput DeckLinkInput => deckLinkInput;


        public bool VideoInputSignalLocked
        {
            get
            {
                int flag;
                var status = (IDeckLinkStatus)deckLink;
                status.GetFlag(_BMDDeckLinkStatusID.bmdDeckLinkStatusVideoInputSignalLocked, out flag);
                return flag != 0;
            }
        }

        private string deviceName = "";
        public string DeviceName => deviceName;

        private string modelName = "";
        public string ModelName => modelName;


        public _BMDPixelFormat  VideoPixelFormat => _BMDPixelFormat.bmdFormat8BitYUV;

        public _BMDAudioSampleType AudioSampleType => _BMDAudioSampleType.bmdAudioSampleType32bitInteger;
        public int AudioChannelsCount => 2;
        public _BMDAudioSampleRate AudioSampleRate => _BMDAudioSampleRate.bmdAudioSampleRate48kHz;


        private bool supportsFormatDetection = false;
        public bool SupportsFormatDetection => supportsFormatDetection;


        private bool currentlyCapturing = false;
        public bool isCapturing => currentlyCapturing;

        public void StartCapture(_BMDDisplayMode mode, IDeckLinkScreenPreviewCallback screenPreviewCallback, bool applyDetectedInputMode)
        {
            logger.Debug("StartCapture(...) " + mode);

            if (currentlyCapturing)
            {
                return;
            }
            deckLinkInput.GetDisplayMode(_BMDDisplayMode.bmdModeHD1080i5994, out IDeckLinkDisplayMode displayMode);

            StartCapture(displayMode, screenPreviewCallback, applyDetectedInputMode);


        }

        public void StartCapture(IDeckLinkDisplayMode displayMode, IDeckLinkScreenPreviewCallback screenPreviewCallback, bool applyDetectedInputMode)
        {

            logger.Debug("StartCapture(...)");

            if (currentlyCapturing)
            {
                return;
            }

            validInputSignal = false;
            this.applyDetectedInputMode = applyDetectedInputMode;


            var videoInputFlags = _BMDVideoInputFlags.bmdVideoInputFlagDefault;
            // Enable input video mode detection if the device supports it
            if (SupportsFormatDetection && this.applyDetectedInputMode)
            {
                videoInputFlags |= _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection;
            }

            var pixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;
            var bdmDisplayMode = displayMode.GetDisplayMode();


            // Set the screen preview
            //_deckLinkInput.SetScreenPreviewCallback(screenPreviewCallback);

            // Set capture callback
            deckLinkInput.SetCallback(this);


            // Set the video input mode
            deckLinkInput.EnableVideoInput(bdmDisplayMode, pixelFormat, videoInputFlags);

            deckLinkInput.EnableAudioInput(AudioSampleRate, AudioSampleType, (uint)AudioChannelsCount);

            // Start the capture
            deckLinkInput.StartStreams();


            currentlyCapturing = true;
        }




        void IDeckLinkInputCallback.VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents notificationEvents, IDeckLinkDisplayMode newDisplayMode, _BMDDetectedVideoInputFormatFlags detectedSignalFlags)
        {
            
            logger.Debug("IDeckLinkInputCallback.VideoInputFormatChanged(...) " + notificationEvents);

            // Restart capture with the new video mode if told to
            if (!applyDetectedInputMode)
            {
                return;
            }

            //var pixelFormat = _BMDPixelFormat.bmdFormat10BitYUV;
            //if ((detectedSignalFlags & _BMDDetectedVideoInputFormatFlags.bmdDetectedVideoInputRGB444) != 0)
            //{
            //    pixelFormat = _BMDPixelFormat.bmdFormat10BitRGB;
            //}


            var pixelFormat = _BMDPixelFormat.bmdFormat8BitYUV;//bmdFormat8BitYUV; //!!!!!!!!!!
            var dispMode = newDisplayMode.GetDisplayMode();

            deckLinkInput.DoesSupportVideoMode(_BMDVideoConnection.bmdVideoConnectionHDMI, dispMode, pixelFormat, _BMDSupportedVideoModeFlags.bmdSupportedVideoModeDefault, out int supported);

            logger.Debug(pixelFormat + " " + supported);



            // Stop the capture
            deckLinkInput.StopStreams();

            // Set the video input mode
            deckLinkInput.EnableVideoInput(dispMode, pixelFormat, _BMDVideoInputFlags.bmdVideoInputEnableFormatDetection);

            // Start the capture
            deckLinkInput.StartStreams();

            logger.Debug("NewDisplayMode: " + newDisplayMode.GetDisplayMode());
            InputFormatChanged?.Invoke(newDisplayMode);
        }


        void IDeckLinkInputCallback.VideoInputFrameArrived(IDeckLinkVideoInputFrame videoFrame, IDeckLinkAudioInputPacket audioPacket)
        {

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
            logger.Debug("StopCapture()");

            if (!currentlyCapturing)
            {
                return;
            }
               

            RemoveAllListeners();

            // Stop the capture
            deckLinkInput.StopStreams();

            // disable callbacks
            deckLinkInput.SetScreenPreviewCallback(null);
            deckLinkInput.SetCallback(null);

            currentlyCapturing = false;
        }

        void RemoveAllListeners()
        {
            InputSignalChanged = null;
            InputFormatChanged = null;
        }


        IEnumerator<IDeckLinkDisplayMode> IEnumerable<IDeckLinkDisplayMode>.GetEnumerator()
        {
            IDeckLinkDisplayModeIterator displayModeIterator;
            deckLinkInput.GetDisplayModeIterator(out displayModeIterator);
            return new DisplayModeEnum(displayModeIterator);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new InvalidOperationException();
        }
    }

    class DisplayModeEnum : IEnumerator<IDeckLinkDisplayMode>
    {
        private IDeckLinkDisplayModeIterator _displayModeIterator;
        private IDeckLinkDisplayMode _displayMode;

        public DisplayModeEnum(IDeckLinkDisplayModeIterator displayModeIterator)
        {
            _displayModeIterator = displayModeIterator;
        }

        IDeckLinkDisplayMode IEnumerator<IDeckLinkDisplayMode>.Current
        {
            get { return _displayMode; }
        }

        bool System.Collections.IEnumerator.MoveNext()
        {
            _displayModeIterator.Next(out _displayMode);
            return _displayMode != null;
        }

        void IDisposable.Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return _displayMode; }
        }

        void System.Collections.IEnumerator.Reset()
        {
            throw new InvalidOperationException();
        }
    }


    class DeckLinkDeviceDiscovery : IDeckLinkDeviceNotificationCallback
    {
        private IDeckLinkDiscovery deckLinkDiscovery;

        public event Action<IDeckLink> DeviceArrived;
        public event Action<IDeckLink> DeviceRemoved;

        public DeckLinkDeviceDiscovery()
        {
            deckLinkDiscovery = new CDeckLinkDiscovery();
        }


        public void Enable()
        {
            deckLinkDiscovery.InstallDeviceNotifications(this);
        }
        public void Disable()
        {
            deckLinkDiscovery.UninstallDeviceNotifications();
        }

        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceArrived(IDeckLink deckLinkDevice)
        {
            DeviceArrived?.Invoke(deckLinkDevice);
        }

        void IDeckLinkDeviceNotificationCallback.DeckLinkDeviceRemoved(IDeckLink deckLinkDevice)
        {
            DeviceRemoved?.Invoke(deckLinkDevice);
        }
    }
}
