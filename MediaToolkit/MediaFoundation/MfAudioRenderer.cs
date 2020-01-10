using MediaToolkit.SharedTypes;

using NLog;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.MediaFoundation
{
    public class MfAudioRenderer: IAudioRenderer
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
        private event Action<int> EventOccured;

        private volatile RendererState rendererState = RendererState.Closed;
        public RendererState State { get => rendererState; }

        private volatile int errorCode = 0;
        public int ErrorCode { get => errorCode; }

        private bool IsRunning => (rendererState == RendererState.Started || rendererState == RendererState.Paused);

        private object syncLock = new object();
        private Transform resampler = null;
        private MediaType inputMediaType = null;
        private MediaType deviceMediaType = null;

        public void Setup(AudioRendererArgs args)
        {
            logger.Debug("MfAudioRenderer::Setup(...) " + args.ToString());

            var deviceId = args.DeviceId;
            var sampleRate = args.SampleRate;
            var channelsNum = args.Channels;
            var bitsPerSample = args.BitsPerSample;

            NAudio.CoreAudioApi.MMDevice device = GetAudioDevice(deviceId);

            NAudio.Wave.WaveFormat mixWaveFormat = null;
            if (device != null)
            {
                deviceId = device.ID;
                mixWaveFormat = device.AudioClient.MixFormat;
                device.Dispose();
            }
            else
            {
                throw new Exception("Audio render device not found...");
            }

            NAudio.Wave.WaveFormat inputWaveFormat = new NAudio.Wave.WaveFormat(sampleRate, bitsPerSample, channelsNum);
            if (args.Encoding == Core.WaveEncodingTag.IeeeFloat)
            {
                inputWaveFormat = NAudio.Wave.WaveFormat.CreateIeeeFloatWaveFormat(args.SampleRate, args.Channels);
            }

            Setup(deviceId, mixWaveFormat, inputWaveFormat);

        }



        public void Setup(string endpointId, NAudio.Wave.WaveFormat mixWaveFormat, NAudio.Wave.WaveFormat inputWaveFormat)
        {
            
            logger.Debug("MfAudioRenderer::Setup(...) " + endpointId);


            if (rendererState != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid state: " + rendererState);
            }


            try
            {
                //TODO: validate wave formats
                if (!(mixWaveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.Pcm || 
                    mixWaveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.IeeeFloat || 
                    mixWaveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.Extensible))
                {
                    throw new FormatException("Invalid device format "+ mixWaveFormat.Encoding);
                }

                if (!(inputWaveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.Pcm ||
                        inputWaveFormat.Encoding == NAudio.Wave.WaveFormatEncoding.IeeeFloat))
                {
                    throw new FormatException("Invalid input format " + inputWaveFormat.Encoding);
                }

                deviceMediaType = MfTool.CreateMediaTypeFromWaveFormat(mixWaveFormat);
                
                inputMediaType = MfTool.CreateMediaTypeFromWaveFormat(inputWaveFormat);

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


                var characteristics = audioSink.Characteristics;
                var logCharacts = MfTool.LogEnumFlags((MediaSinkCharacteristics)characteristics);
                logger.Debug("AudioSinkCharacteristics: " + logCharacts);

                using (var service = audioSink.QueryInterface<ServiceProvider>())
                { //MR_POLICY_VOLUME_SERVICE

                    simpleAudioVolume = service.GetService<SimpleAudioVolume>(MediaServiceKeys.PolicyVolume);
                    volume = simpleAudioVolume.MasterVolume;
                    mute = simpleAudioVolume.Mute;
                }


                audioSink.GetStreamSinkByIndex(0, out streamSink);
                using (var handler = streamSink.MediaTypeHandler)
                {

                    //LogMediaTypeHanldler(handler);

                    var rest = handler._IsMediaTypeSupported(inputMediaType, out MediaType _mediaType);
                    if (rest.Success)
                    {
                        logger.Debug("CurrentMediaType:\r\n" + MfTool.LogMediaType(inputMediaType));

                        handler.CurrentMediaType = inputMediaType;
                    }
                    else
                    {// рендерер не поддерживает данный формат - создаем ресемплер

                        logger.Debug("Input not supported, try to create resampler...");

                        //внутненний формат девайса должен поддерживатся
                        handler.IsMediaTypeSupported(deviceMediaType, out MediaType _deviceMediaType);
                        handler.CurrentMediaType = deviceMediaType;

                        logger.Debug("Resapmler input type:\r\n" + MfTool.LogMediaType(inputMediaType));
                        logger.Debug("Resapmler output type:\r\n" + MfTool.LogMediaType(deviceMediaType));

                        resampler = new Transform(CLSID.CResamplerMediaObject);

                        resampler.SetInputType(0, inputMediaType, 0);
                        resampler.SetOutputType(0, deviceMediaType, 0);

                    }


                    streamSinkEventHandler = new MediaEventHandler(streamSink);
                    streamSinkEventHandler.EventReceived += StreamSinkEventHandler_EventReceived;

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

        public void SetPresentationClock(PresentationClock clock)
        {
            PresentationTimeSource timeSource = null;
            try
            {
                MediaFactory.CreateSystemTimeSource(out timeSource);

                clock.TimeSource = timeSource;

                audioSink.PresentationClock = clock;

            }
            finally
            {
                timeSource?.Dispose();
            }
        }


        private bool mute = false;
        public bool Mute
        {
            get
            {
                return mute;
            }
            set
            {
                if (mute != value)
                {
                    mute = value;
                    SetMute(mute);
                }
            }
        }

        private void SetMute(bool mute)
        {
            if (!IsRunning)
            {
                //logger.Warn("SetMute() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                if (simpleAudioVolume != null)
                {
                    simpleAudioVolume.Mute = mute;
                }
                
            }

        }

        private float volume = 1f;
        public float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                if(volume != value )
                {
                    volume = value;
                    SetVolume(volume);
                }
            }
        }

        private void SetVolume(float volume)
        {
            if (rendererState == RendererState.Closed)
            {
                //logger.Warn("SetVolume() return invalid render state: " + rendererState);
                return;
            }

            lock (syncLock)
            {
                if (simpleAudioVolume != null)
                {
                    simpleAudioVolume.MasterVolume = volume;
                }
                
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

                    if (resampler != null)
                    {
                        resampler.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
                        resampler.ProcessMessage(TMessageType.NotifyBeginStreaming, IntPtr.Zero);
                        resampler.ProcessMessage(TMessageType.NotifyStartOfStream, IntPtr.Zero);

                        resampler.GetOutputStreamInfo(0, out TOutputStreamInformation streamInformation);

                    }

                    rendererState = RendererState.Started;
                    RendererStarted?.Invoke();
                }
                else if (typeInfo == MediaEventTypes.StreamSinkStopped)
                {
                    logger.Debug(typeInfo);

                    if (resampler != null)
                    {

                        resampler.ProcessMessage(TMessageType.NotifyEndOfStream, IntPtr.Zero);
                        resampler.ProcessMessage(TMessageType.NotifyEndStreaming, IntPtr.Zero);
                        resampler.ProcessMessage(TMessageType.CommandFlush, IntPtr.Zero);
                    }

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
                else if (typeInfo == MediaEventTypes.AudioSessionDeviceRemoved)
                {// TODO: пересоздаем девайс

                    logger.Debug(typeInfo);

                   
                }
                else if (typeInfo == MediaEventTypes.AudioSessionVolumeChanged)
                {
                    logger.Debug(typeInfo);

                    volume = simpleAudioVolume.MasterVolume;
                    mute = simpleAudioVolume.Mute;

                    EventOccured?.Invoke((int)typeInfo);
                    
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
                logger.Debug("ProcessSample(...) return invalid render state: " + rendererState);
                return;
            }

            //ClockState state = ClockState.Invalid;
            //presentationClock?.GetState(0, out state);
            //if (state != ClockState.Running)
            //{
            //    //logger.Debug("ProcessSample(...) return invalid clock state: " + state);
            //    return;
            //}


            var sampleTime = sample.SampleTime;
            var sampleDuration = sample.SampleDuration;

            bool lockTacken = false;
            try
            {
                Monitor.TryEnter(syncLock, 10, ref lockTacken);
                if (lockTacken)
                {
                    if (resampler != null)
                    {
                        if (DoResample(sample, out Sample resample))
                        {
                            streamSink.ProcessSample(resample);
                            streamSinkRequestSample--;
                            resample?.Dispose();
                        }
                    }
                    else
                    {
                        streamSink.ProcessSample(sample);
                        streamSinkRequestSample--;
                    }

                    sample?.Dispose();
                }
                else
                {
                    logger.Warn("Drop audio sample at: " + sampleTime + " + " + sampleDuration);
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



        public bool DoResample(Sample inputSample, out Sample outputSample)
        {
            bool Result = false;
            outputSample = null;

            if (inputSample == null)
            {
                return false;
            }

 
            resampler.ProcessInput(0, inputSample, 0);

           // if (resampler.OutputStatus == (int)MftOutputStatusFlags.MftOutputStatusSampleReady) //E_NOTIMPL
            {

                resampler.GetOutputStreamInfo(0, out TOutputStreamInformation streamInfo);

                MftOutputStreamInformationFlags flags = (MftOutputStreamInformationFlags)streamInfo.DwFlags;
                bool createSample = !flags.HasFlag(MftOutputStreamInformationFlags.MftOutputStreamProvidesSamples);
                if (!createSample)
                {// TODO:

                }


                long inputSampleTime = inputSample.SampleTime;
                long totalDuration = 0;
                int totalLength = 0;

                List<MediaBuffer> resampledBuffers = new List<MediaBuffer>();

                Sample sampleBuffer = null;

                try
                {
                    sampleBuffer = MediaFactory.CreateSample();
                    sampleBuffer.SampleTime = inputSample.SampleTime;
                    sampleBuffer.SampleDuration = inputSample.SampleDuration;
                    sampleBuffer.SampleFlags = inputSample.SampleFlags;


                    TOutputDataBuffer[] outputDataBuffer = new TOutputDataBuffer[1];

                    var data = new TOutputDataBuffer
                    {
                        DwStatus = 0,
                        DwStreamID = 0,
                        PSample = sampleBuffer,
                        PEvents = null,
                    };
                    outputDataBuffer[0] = data;


                    do
                    {
                        using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(streamInfo.CbSize * 1024))//streamInfo.CbSize))
                        {
                            sampleBuffer.AddBuffer(mediaBuffer);
                        }

                        var res = resampler.TryProcessOutput(TransformProcessOutputFlags.None, outputDataBuffer, out TransformProcessOutputStatus status);
                        if (res == SharpDX.Result.Ok)
                        {
                            //outputSample = outputDataBuffer[0].PSample;
                            //s.Dispose();

                            if (sampleBuffer == null)
                            {
                                sampleBuffer = outputDataBuffer[0].PSample;
                            }

                            Debug.Assert(sampleBuffer != null, "res.Success && outputSample != null");

                            var resampledBuffer = sampleBuffer.ConvertToContiguousBuffer();
                            totalLength += resampledBuffer.CurrentLength;
                            totalDuration += sampleBuffer.SampleDuration;
                            resampledBuffers.Add(resampledBuffer);

                            sampleBuffer.RemoveBufferByIndex(0);

                            continue;

                        }
                        else if (res == ResultCode.TransformNeedMoreInput)
                        {
                            Debug.Assert(resampledBuffers.Count > 0, "processedBuffers.Count > 0");
                            Debug.Assert(totalLength > 0, " totalLength > 0");

                            if (resampledBuffers.Count > 0 && totalLength > 0)
                            {
                                outputSample = MediaFactory.CreateSample();

                                outputSample.SampleTime = inputSampleTime;
                                outputSample.SampleDuration = totalDuration;

                                using (var destBuffer = MediaFactory.CreateMemoryBuffer(totalLength))
                                {
                                    int offset = 0;
                                    var pDest = destBuffer.Lock(out int maxLen, out int curLen);
                                    for (int i = 0; i < resampledBuffers.Count; i++)
                                    {
                                        MediaBuffer srcBuffer = resampledBuffers[i];
                                        if (srcBuffer != null)
                                        {
                                            IntPtr pSrc = srcBuffer.Lock(out int srcMaxLen, out int srcCurLen);
                                            NativeAPIs.Kernel32.CopyMemory(pDest, pSrc, (uint)srcCurLen);
                                            pDest += srcCurLen;
                                            offset += srcCurLen;
                                            srcBuffer.Unlock();

                                            srcBuffer.Dispose();
                                            srcBuffer = null;
                                        }       
                                    }

                                    destBuffer.Unlock();
                                    destBuffer.CurrentLength = offset;
                                    outputSample.AddBuffer(destBuffer);
                                }
                            }
                            else
                            {

                                logger.Warn(res.ToString() + totalLength + " " + resampledBuffers.Count);
                            }

                            Result = true;
                            break;

                        }
                        //else if (res == SharpDX.MediaFoundation.ResultCode.TransformStreamChange)
                        //{
                        //    logger.Warn(res.ToString() + " TransformStreamChange");
                        //}
                        else
                        {
                            res.CheckError();
                        }

                    } while (true);
                }
                finally
                {
                    if (sampleBuffer != null)
                    {
                        sampleBuffer.Dispose();
                        sampleBuffer = null;
                    }
                }

            }

            return Result;
        }


        private void OnFormatChanged()
        {
            logger.Debug("MfAudioRenderer::OnFormatChanged()");
            //TODO:
        }

        private void OnDeviceChanged()
        {
            logger.Debug("MfAudioRenderer::OnDeviceChanged() " + streamSinkRequestSample);
            lock (syncLock)
            {


            }
        }



        public void Start(long time)
        {
            logger.Debug("MfAudioRenderer::Start(...) " + time);

            if (rendererState == RendererState.Closed || rendererState == RendererState.Started)
            {
                logger.Warn("Start(...) return invalid render state: " + rendererState);
                return;
            }


            lock (syncLock)
            {
                if (presentationClock != null)
                {
                    presentationClock.Dispose();
                    presentationClock = null;
                }

                MediaFactory.CreatePresentationClock(out presentationClock);

                SetPresentationClock(presentationClock);

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
            logger.Debug("MfAudioRenderer::Stop()");

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
        { // не работает!!
            logger.Debug("MfAudioRenderer::Pause()");
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


        public void Close()
        {
            logger.Debug("MfAudioRenderer::Close()");
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
            logger.Debug("MfAudioRenderer::CleanUp()");

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
            if (inputMediaType != null)
            {
                inputMediaType.Dispose();
                inputMediaType = null;
            }

            if (deviceMediaType != null)
            {
                deviceMediaType.Dispose();
                deviceMediaType = null;
            }

        }

        private static NAudio.CoreAudioApi.MMDevice GetAudioDevice(string deviceId)
        {
            NAudio.CoreAudioApi.MMDevice device = null;
            var deviceEnum = new NAudio.CoreAudioApi.MMDeviceEnumerator();
            if (string.IsNullOrEmpty(deviceId))
            {// default render...
                if (deviceEnum.HasDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Console))
                {
                    device = deviceEnum.GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Console);
                }
            }
            else
            {
                device = deviceEnum.GetDevice(deviceId);
            }
            NAudio.CoreAudioApi.MMDeviceCollection devices = null;
            if (device == null)
            {
                devices = deviceEnum.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.DeviceState.Active);
                if (devices.Count > 0)
                {
                    device = devices[0];
                    for(int i= 1; i< devices.Count; i++)
                    {
                        device = devices[i];
                        device.Dispose();
                        device = null;
                    }
                }
                
            }

            return device;
        }

        private static void LogMediaTypeHanldler(MediaTypeHandler handler)
        {
            for (int i = 0; i < handler.MediaTypeCount; i++)
            {
                using (var mediaType = handler.GetMediaTypeByIndex(i))
                {
                    var result = handler._IsMediaTypeSupported(mediaType, out MediaType m);
                    if (result.Failure)
                    {
                        string log = "Media type not supported:\r\n" +
                                    MfTool.LogMediaType(mediaType) +
                                    "\r\n--------------------------------------";

                        logger.Info(log);
                        continue;

                    }
                    //handler.CurrentMediaType = mediaType;
                    logger.Info("CurrentMediaType:\r\n" + MfTool.LogMediaType(mediaType));

                    break;
                    // logger.Debug("CurrentMediaType:\r\n" + MfTool.LogMediaType(mediaType));
                }
            }
        }
    }


}




