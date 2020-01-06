using NLog;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.MediaFoundation
{
    public class MfAudioRenderer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MfAudioRenderer()
        { }
 
        private PresentationClock presentationClock = null;

        private StreamSink streamSink = null;
        private MediaSink audioSink = null;
        private SimpleAudioVolume simpleAudioVolume = null;
        private volatile int streamSinkRequestSample = 0;

        private MediaEventHandler streamSinkEventHandler = null;

        public event Action RendererStarted;
        public event Action RendererStopped;

        private volatile RendererState rendererState = RendererState.Closed;
        public RendererState State { get => rendererState; }

        private volatile int errorCode = 0;
        public int ErrorCode { get => errorCode; }

        private bool IsRunning => (rendererState == RendererState.Started || rendererState == RendererState.Paused);

        private object syncLock = new object();

        public void Setup(string endpointId, MfVideoArgs videoArgs)
        {
            logger.Debug("Setup(...) " + endpointId);

            if (rendererState != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid state: " + rendererState);
            }

            try
            {
                Activate activate = null;
                try
                {
                    MediaFactory.CreateAudioRendererActivate(out activate);

                    /*
                     * If this attribute is set, do not set the MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ROLE attribute.
                     * If both attributes are set, a failure will occur when the audio renderer is created.
                     */

                    activate.Set(AudioRendererAttributeKeys.EndpointId, endpointId);

                    audioSink = activate.ActivateObject<MediaSink>();
                }
                finally
                {
                    activate?.Dispose();
                }

                //using (MediaAttributes attributes = new MediaAttributes(1))
                //{
                //    /*
                //     * If this attribute is set, do not set the MF_AUDIO_RENDERER_ATTRIBUTE_ENDPOINT_ROLE attribute.
                //     * If both attributes are set, a failure will occur when the audio renderer is created.
                //     */
                //    attributes.Set(AudioRendererAttributeKeys.EndpointId, endpointId);
                //    //attributes.Set(AudioRendererAttributeKeys.EndpointRole, 0);

                //    MediaFactory.CreateAudioRenderer(attributes, out MediaSink sink);
                //    this.audioSink = sink;
                //}


                var characteristics = audioSink.Characteristics;
                var logCharacts = MfTool.LogEnumFlags((MediaSinkCharacteristics)characteristics);
                logger.Debug("AudioSinkCharacteristics: " + logCharacts);

                 using (var service = audioSink.QueryInterface<ServiceProvider>())
                { //MR_POLICY_VOLUME_SERVICE

                    simpleAudioVolume = service.GetService<SimpleAudioVolume>(MediaServiceKeys.PolicyVolume);
                }


                if (audioSink.StreamSinkCount == 0)
                {
                    //TODO:..
                }


                audioSink.GetStreamSinkByIndex(0, out streamSink);
                using (var handler = streamSink.MediaTypeHandler)
                {

                    //for (int i = 0; i < handler.MediaTypeCount; i++)
                    //{
                    //    using (var mediaType = handler.GetMediaTypeByIndex(i))
                    //    {
                    //        var result = handler._IsMediaTypeSupported(mediaType, out MediaType m);
                    //        if (result.Failure)
                    //        {
                    //            string log = "Media type not supported:\r\n" +
                    //                        MfTool.LogMediaType(mediaType) +
                    //                        "\r\n--------------------------------------";

                    //            logger.Info(log);
                    //            continue;
                               
                    //        }
                    //        handler.CurrentMediaType = mediaType;
                    //        logger.Info("CurrentMediaType:\r\n" + MfTool.LogMediaType(mediaType));

                    //        break;
                    //       // logger.Debug("CurrentMediaType:\r\n" + MfTool.LogMediaType(mediaType));
                    //    }
                    //}

                    using (var mediaType = new MediaType())
                    {
                        ///https://docs.microsoft.com/en-us/windows/win32/api/mmreg/ns-mmreg-waveformatex
                        ///
                        //Guid format = AudioFormatGuids.Pcm; 
                        Guid format = AudioFormatGuids.Float;
                        int sampleRate = 48000;
                        int channelsNum = 2;      
                        int bitsPerSample = 32;
                        int bytesPerSample = bitsPerSample / 8;

                        //This attribute corresponds to the nAvgBytesPerSec member of the WAVEFORMATEX structure. 
                        var avgBytesPerSecond = sampleRate * channelsNum * bytesPerSample; // х.з зачем это нужно, но без этого не работает!!!

                        var blockAlignment = 8;
                        if(format == AudioFormatGuids.Pcm || format == AudioFormatGuids.Float)
                        {
                            // If wFormatTag = WAVE_FORMAT_PCM or wFormatTag = WAVE_FORMAT_IEEE_FLOAT, 
                            //set nBlockAlign to (nChannels*wBitsPerSample)/8, 
                            //which is the size of a single audio frame. 
                            blockAlignment = channelsNum * bytesPerSample;
                        }
                        else
                        {
                            //not supported...
                        }

                        mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Audio);
                        mediaType.Set(MediaTypeAttributeKeys.Subtype, format);
                        mediaType.Set(MediaTypeAttributeKeys.AudioSamplesPerSecond, sampleRate);
                        mediaType.Set(MediaTypeAttributeKeys.AudioBitsPerSample, bitsPerSample);
                        mediaType.Set(MediaTypeAttributeKeys.AudioNumChannels, channelsNum);           
                        mediaType.Set(MediaTypeAttributeKeys.AudioAvgBytesPerSecond, avgBytesPerSecond);
                        mediaType.Set(MediaTypeAttributeKeys.AudioBlockAlignment, blockAlignment);


                        handler.IsMediaTypeSupported(mediaType, out MediaType _mediaType);

                        logger.Debug("CurrentMediaType:\r\n" + MfTool.LogMediaType(mediaType));

                        handler.CurrentMediaType = mediaType;
   
                        
                    }

                    streamSinkEventHandler = new MediaEventHandler(streamSink);
                    streamSinkEventHandler.EventReceived += StreamSinkEventHandler_EventReceived;

                }

                MediaFactory.CreatePresentationClock(out presentationClock);
                PresentationTimeSource timeSource = null;
                try
                {
                    MediaFactory.CreateSystemTimeSource(out timeSource);

                    presentationClock.TimeSource = timeSource;

                    audioSink.PresentationClock = presentationClock;

                }
                finally
                {
                    timeSource?.Dispose();
                }

                rendererState = RendererState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

        }


        public void SetMute(bool mute)
        {
            if (!IsRunning)
            {
                //logger.Warn("SetMute() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                simpleAudioVolume.Mute = mute;
            }

        }


        public void SetVolume(float volume)
        {
            if (rendererState == RendererState.Closed)
            {
                //logger.Warn("SetVolume() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                simpleAudioVolume.MasterVolume = volume;
            }

        }

        Stopwatch sw = new Stopwatch();
        private void StreamSinkEventHandler_EventReceived(MediaEvent mediaEvent)
        {

            var status = mediaEvent.Status;
            var typeInfo = mediaEvent.TypeInfo;

            if (status.Success)
            {
                if (typeInfo == MediaEventTypes.StreamSinkRequestSample)
                {
                    streamSinkRequestSample++;


                    //logger.Debug("StreamSinkRequestSample: " + sw.ElapsedMilliseconds);
                   //sw.Restart();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkStarted)
                {
                    logger.Debug(typeInfo);

                    rendererState = RendererState.Started;
                    RendererStarted?.Invoke();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkStopped)
                {
                    logger.Debug(typeInfo);

                    rendererState = RendererState.Stopped;

                    RendererStopped?.Invoke();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkPaused)
                {
                    logger.Debug(typeInfo);
                    rendererState = RendererState.Paused;
                }
                else if (typeInfo == MediaEventTypes.StreamSinkMarker)
                {
                    logger.Debug(typeInfo);
                }
                else if (typeInfo == MediaEventTypes.StreamSinkDeviceChanged)
                {
                    logger.Debug(typeInfo);

                    OnDeviceChanged();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkFormatChanged)
                {
                    logger.Debug(typeInfo);
                    OnFormatChanged();

                    //...
                    errorCode = 100500;

                }
                else
                {
                    logger.Debug(typeInfo);
                }
            }
            else
            {

            }

        }

         public void ProcessSample(Sample sample)
        {
            if (!IsRunning)
            {
                //logger.Debug("ProcessSample(...) return invalid render state: " + rendererState);
                return;
            }

            ClockState state = ClockState.Invalid;
            presentationClock?.GetState(0, out state);
            if (state != ClockState.Running)
            {
                //logger.Debug("ProcessSample(...) return invalid clock state: " + state);
                return;
            }

            var sampleTime = sample.SampleTime;
            var sampleDuration = sample.SampleDuration;

            bool lockTacken = false;
            try
            {
                Monitor.TryEnter(syncLock, 100, ref lockTacken);
                if (lockTacken)
                {
                    //if (streamSinkRequestSample > 0)
                    {
                        streamSink.ProcessSample(sample);

                        streamSinkRequestSample--;
                    }
                }
            }
            catch (SharpDXException ex)
            {
                var resultCode = ex.ResultCode;

                if (resultCode == ResultCode.InvalidTimestamp || resultCode == ResultCode.NoSampleTimestamp)
                {
                    logger.Warn(resultCode + ": " + sampleTime);
                }
                else if (resultCode == ResultCode.NoSampleDuration || resultCode == ResultCode.DurationTooLong)
                {
                    logger.Warn(resultCode + ": " + sampleDuration);
                }
                //else if (resultCode == ResultCode.NoClock || resultCode == ResultCode.NotInitializeD)
                //{// Not Initialized...
                //    logger.Warn(resultCode);
                //}
                else
                {
                    logger.Error(ex);
                    //throw;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                if (lockTacken)
                {
                    Monitor.Exit(syncLock);
                }
            }
        }


        public void Start(long time)
        {
            logger.Debug("Start(...) " + time);

            if (rendererState == RendererState.Closed || rendererState == RendererState.Started)
            {
                logger.Warn("Start(...) return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                presentationClock.GetState(0, out ClockState state);
                if (state != ClockState.Running)
                {
                    //var time = presentationClock.Time;

                    presentationClock.Start(time);
                }
                else
                {
                    logger.Warn("Start(...) return invalid clock state: " + state);
                }
            }
        }

        public void Stop()
        {
            logger.Debug("Stop()");

            if (!IsRunning)
            {
                logger.Warn("Stop() return invalid render state: " + rendererState);

                return;
            }

            lock (syncLock)
            {
                streamSinkRequestSample = 0;

                presentationClock.GetState(0, out ClockState state);
                if (state != ClockState.Stopped)
                {
                    presentationClock.Stop();
                }
                else
                {
                    logger.Warn("Stop() return invalid clock state: " + state);
                }
            }
        }

        private const long PRESENTATION_CURRENT_POSITION = 0x7fffffffffffffff;

        public void Pause()
        {
            logger.Debug("Pause()");
            if (!IsRunning)
            {
                logger.Warn("Pause() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                presentationClock.GetState(0, out ClockState state);
                if (state == ClockState.Running)
                {
                    presentationClock.Pause();
                }
                else if (state == ClockState.Paused)
                {
                    presentationClock.Start(PRESENTATION_CURRENT_POSITION);
                }
                else
                {
                    logger.Warn("Pause() return invalid clock state: " + state);
                }
            }

        }

        private void OnFormatChanged()
        {
            logger.Debug("OnFormatChanged()");
            //TODO:
        }

        private void OnDeviceChanged()
        {
            logger.Debug("OnDeviceChanged() " + streamSinkRequestSample);
            lock (syncLock)
            {


            }
        }


        public void Close()
        {
            logger.Debug("Close()");
            try
            {
                //lock (syncLock)
                {
                    CleanUp();
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                //...
            }

            rendererState = RendererState.Closed;

        }

        private void CleanUp()
        {
            logger.Debug("CleanUp()");

            if (audioSink != null)
            {
                audioSink.Shutdown();

                audioSink.Dispose();
                audioSink = null;
            }

            if (streamSinkEventHandler != null)
            {
                streamSinkEventHandler.EventReceived -= StreamSinkEventHandler_EventReceived;
                streamSinkEventHandler.Dispose();
                streamSinkEventHandler = null;
            }

            if (simpleAudioVolume != null)
            {
                simpleAudioVolume.Dispose();
                simpleAudioVolume = null;
            }


            if (presentationClock != null)
            {
                presentationClock.Dispose();
                presentationClock = null;
            }

            if (streamSink != null)
            {
                streamSink.Dispose();
                streamSink = null;
            }
        }


    }

}
