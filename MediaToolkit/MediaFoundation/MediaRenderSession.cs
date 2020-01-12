using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using NLog;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public class MediaRenderSession : IMediaRenderSession
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MfVideoRenderer videoRenderer = null;
        private MfAudioRenderer audioRenderer = null;

        private PresentationClock presentationClock = null;

        private double lastAudioTime = 0;
        private double lastVideoTime = 0;


        private volatile RendererState rendererState = RendererState.Closed;
        public RendererState State { get => rendererState; }

        private volatile int errorCode = 0;
        public int ErrorCode { get => errorCode; }

        public bool Mute
        {
            get
            {
                return audioRenderer?.Mute ?? false;
            }
            set
            {
                if (audioRenderer != null)
                {
                    if(audioRenderer.Mute!= value)
                    {
                        audioRenderer.Mute = value;
                    }
                }
            }
        }

        public float Volume
        {
            get
            {
                return audioRenderer?.Volume ?? 1f;
            }
            set
            {
                if (audioRenderer != null)
                {
                    if (audioRenderer.Volume != value)
                    {
                        audioRenderer.Volume = value;
                    }
                }
            }
        }

        public void Resize(System.Drawing.Rectangle rect)
        {
            if (videoRenderer != null)
            {
                videoRenderer.Resize(rect);
            }
        }

        public void Repaint()
        {
            if (videoRenderer != null)
            {
                videoRenderer.Repaint();
            }
        }

        public void Setup(VideoRendererArgs videoArgs, AudioRendererArgs audioArgs)
        {
            logger.Debug("MediaSession::Setup(...)");

            try
            {
                MediaFactory.CreatePresentationClock(out presentationClock);

                if (audioArgs != null)
                {
                    audioRenderer = new MfAudioRenderer();

                    audioRenderer.Setup(audioArgs);
                    audioRenderer.SetPresentationClock(presentationClock);
                }

                if (videoArgs != null)
                {
                    videoRenderer = new MfVideoRenderer();
                    videoRenderer.Setup(videoArgs);

                    videoRenderer.SetPresentationClock(presentationClock);
                }

                rendererState = RendererState.Initialized;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }

        }

        public void ProcessAudioPacket(byte[] data, double time)
        {
            Sample sample = null;
            try
            {
                sample = MediaFactory.CreateSample();

                MediaBuffer mediaBuffer = null;
                try
                {
                    mediaBuffer = MediaFactory.CreateMemoryBuffer(data.Length);
                    {
                        sample.AddBuffer(mediaBuffer);
                    }

                    sample.SampleDuration = MfTool.SecToMfTicks(time);
                    sample.SampleTime = MfTool.SecToMfTicks(time);

                    var pBuffer = mediaBuffer.Lock(out int cbMaxLen, out int cbCurLen);
                    try
                    {
                        Marshal.Copy(data, 0, pBuffer, data.Length);
                        mediaBuffer.CurrentLength = data.Length;
                    }
                    finally
                    {
                        mediaBuffer.Unlock();
                    }

                    //Console.WriteLine("presentation_audio: " + MfTool.MfTicksToSec(presentationClock.Time) + " " + time);
                    audioRenderer.ProcessSample(sample);

                    lastAudioTime = time;

                }
                finally
                {
                    mediaBuffer?.Dispose();
                }
            }
            finally
            {
                sample?.Dispose();
            }
        }

        public void ProcessVideoFrame(IntPtr frameData, int frameLength, double frameTime, double frameDuration)
        {
            Sample sample = null;
            try
            {
                sample = MediaFactory.CreateSample();

                sample.SampleTime = MfTool.SecToMfTicks(frameTime);
                sample.SampleDuration = MfTool.SecToMfTicks(frameDuration);

                using (var mb = MediaFactory.CreateMemoryBuffer(frameLength))
                {
                    var pBuffer = mb.Lock(out int cbMaxLen, out int cbCurLen);
                    try
                    {
                        Kernel32.CopyMemory(pBuffer, frameData, (uint)frameLength);
                        mb.CurrentLength = frameLength;
                    }
                    finally
                    {
                        mb.Unlock();
                    }

                    sample.AddBuffer(mb);
                }

                videoRenderer.ProcessSample(sample);
                //Console.WriteLine("presentation_video: " + MfTool.MfTicksToSec(presentationClock.Time) + " "+ frameTime);

                lastVideoTime = frameTime;

            }
            finally
            {
                sample.Dispose();
            }

        }


        public void Start(long startOffset = 0)
        {
            logger.Debug("MediaSession::Start(...)");
            presentationClock.Start(startOffset);
            rendererState = RendererState.Started;

        }

        public void Stop()
        {
            logger.Debug("MediaSession::Stop(...)");

            presentationClock.Stop();
            rendererState = RendererState.Stopped;
        }

        public void Close()
        {
            logger.Debug("MediaSession::Close(...)");

            if (presentationClock != null)
            {
                presentationClock.Stop();
                presentationClock = null;
            }

            if (audioRenderer != null)
            {
                audioRenderer.Close();
                audioRenderer = null;
            }

            if (videoRenderer != null)
            {
                videoRenderer.Close();
                videoRenderer = null;

            }

            if (presentationClock != null)
            {
                using (var clock = presentationClock.QueryInterface<Shutdownable>())
                {
                    clock.Shutdown();
                }

                presentationClock.Dispose();
                presentationClock = null;
            }

            rendererState = RendererState.Closed;

        }

    }

}
