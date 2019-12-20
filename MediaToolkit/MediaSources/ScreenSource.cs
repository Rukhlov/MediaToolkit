using MediaToolkit.Core;
using NLog;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MediaToolkit
{
    public class ScreenSource : IVideoSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenSource() { }

        public VideoBuffer SharedBitmap { get; private set; }

        public SharpDX.Direct3D11.Texture2D SharedTexture
        {
            get { return hwContext?.SharedTexture; }
        }

        public Size SrcSize
        {
            get
            {
                return new Size(SharedBitmap.bitmap.Width, SharedBitmap.bitmap.Height);
            }
        }

        public CaptureState State { get; private set; } = CaptureState.Closed;
        public int ErrorCode { get; private set; } = 0;

        private DXGIDesktopDuplicationCapture hwContext = null;

        public event Action BufferUpdated;
        private void OnBufferUpdated()
        {
            BufferUpdated?.Invoke();
        }

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;


        private AutoResetEvent syncEvent = null;

        private ScreenCapture screenCapture = null;

        public ScreenCaptureDeviceDescription CaptureParams { get; private set; }

        private bool deviceReady = false;
        public void Setup(object pars)//ScreenCaptureParams captureParams)
        {

            logger.Debug("ScreenSource::Setup()");

            if (State != CaptureState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            syncEvent = new AutoResetEvent(false);
            ScreenCaptureDeviceDescription captureParams = pars as ScreenCaptureDeviceDescription;

            if(captureParams == null)
            {
                throw new ArgumentException();
            }

            this.CaptureParams = captureParams;

            var srcRect = captureParams.CaptureRegion;
            var destSize = captureParams.Resolution;


            try
            {
                //var captureDescr = captureParams.CaptureDescription;
                screenCapture = ScreenCapture.Create(captureParams.CaptureType);
                screenCapture.CaptureMouse = captureParams.CaptureMouse;
                screenCapture.AspectRatio = captureParams.AspectRatio;

                screenCapture.Init(srcRect, destSize);
                //screenCapture.Init(srcRect);

                DXGIDesktopDuplicationCapture capture = screenCapture as DXGIDesktopDuplicationCapture;
                if (capture != null)
                {
                    capture.UseHwContext = captureParams.UseHardware;

                    this.hwContext = capture;
                }

                this.SharedBitmap = screenCapture.VideoBuffer;

                deviceReady = true;

                State = CaptureState.Initialized;


            }
            catch (Exception ex)
            {
  
                logger.Error(ex);
                ErrorCode = 100504;

                CleanUp();

                State = CaptureState.Closed;
                throw;
            }
        }
        private Task captureTask = null;
        public void Start()
        {
            logger.Debug("ScreenSource::Start()");
            if (!(State == CaptureState.Stopped || State == CaptureState.Initialized))
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

           // State = CaptureState.Starting;

            captureTask = Task.Run(() =>
            {
                try
                {
                    logger.Info("Capture thread started...");
                    CaptureStarted?.Invoke();

                    DoCapture();

                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                    this.ErrorCode = 100500;
                }
                finally
                {
                    CaptureStopped?.Invoke(null);

                    logger.Info("Capture thread stopped...");
                }
            });

        }

        private void DoCapture()
        {
            State = CaptureState.Capturing;

            CaptureStats captureStats = new CaptureStats();
            try
            {                

                Statistic.RegisterCounter(captureStats);

                var frameRate = CaptureParams.Fps;
                var frameInterval = (1000.0 / frameRate);
                captureStats.frameInterval = frameInterval;

                double lastTime = 0;
                double monotonicTime = 0;

                Stopwatch sw = Stopwatch.StartNew();


                while (State == CaptureState.Capturing && deviceReady)
                {
                    sw.Restart();

                    try
                    {
                        var res = screenCapture.UpdateBuffer(30);

                        if (State != CaptureState.Capturing || !deviceReady)
                        {
                            break;
                        }

                        if (res)
                        {
                            var time = (monotonicTime + sw.ElapsedMilliseconds / 1000.0); //MediaTimer.GetRelativeTime() ;

                            SharedBitmap.time = time; //MediaTimer.GetRelativeTime() 

                            //var diff = time - lastTime;

                            lastTime = SharedBitmap.time;

                            OnBufferUpdated();

                            captureStats.UpdateFrameStats(SharedBitmap.time, (int)SharedBitmap.DataLength);

                        }
                        else
                        {
                            //logger.Warn("No screen buffer...");

                        }


                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        Thread.Sleep(1000);
                    }

                    var mSec = sw.ElapsedMilliseconds;
                    var delay = (int)(frameInterval - mSec);

                    if (delay > 0 && delay < 5000)
                    {
                        syncEvent.WaitOne(delay);
                    }
                    else
                    {
                        //logger.Warn("delay " + delay);
                    }

                    monotonicTime += sw.ElapsedMilliseconds / 1000.0;
                    captureStats.Update(monotonicTime);
                }

            }
            finally
            {
                //screenCapture?.Close();

                Statistic.UnregisterCounter(captureStats);

                this.State = CaptureState.Stopped;

            }

        }

  
        public void Stop()
        {
            logger.Debug("ScreenSource::Close()");

            State = CaptureState.Stopping;

            syncEvent?.Set();

        }

        public void Close(bool force = false)
        {
            logger.Debug("ScreenSource::Close(...) " + force);

            Stop();
            deviceReady = false;

            if (!force)
            {
                if (captureTask != null)
                {
                    if (captureTask.Status == TaskStatus.Running)
                    {
                        bool waitResult = false;
                        do
                        {
                            waitResult = captureTask.Wait(1000);
                            if (!waitResult)
                            {
                                logger.Warn("ScreenSource::Close() " + waitResult);
                            }
                        } while (!waitResult);

                    }
                }
            }

            CleanUp();

            State = CaptureState.Closed;
        }

        private void CleanUp()
        {
            if (screenCapture != null)
            {
                screenCapture.Close();
                screenCapture = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Dispose();
                syncEvent = null;
            }
        }
    }


    class CaptureStats : MediaToolkit.Utils.StatCounter
    {
        public double totalTime = 0;
        public long totalBytes = 0;
        public uint totalFrameCount = 0;

        public uint currentFrame = 0;
        public long currentBytes = 0;

        public double avgFrameInterval = 0;
        public double avgBytesPerSec = 0;

        public double lastTimestamp = 0;

        public double frameInterval = 0;
        public double jitterAvg = 0;

        public void Update(double timestamp)
        {
            if (lastTimestamp > 0)
            { 

                var interval = (timestamp - lastTimestamp) * 1000;

                var diff = (interval - frameInterval);

                jitterAvg = (diff * 0.05 + jitterAvg * (1 - 0.05));

                //jitterAvg += (int)((1.0 / 16.0) * ((double)Math.Abs(diff) - jitterAvg));

            }
            //lastTimestamp = timestamp;

        }

        public void UpdateFrameStats(double timestamp, int bytesSize)
        {

            if (lastTimestamp > 0)
            {
                var time = timestamp - lastTimestamp;

                avgFrameInterval = (time * 0.05 + avgFrameInterval * (1 - 0.05));
                avgBytesPerSec = bytesSize / avgFrameInterval;

                totalTime += (timestamp - lastTimestamp);
            }

            totalBytes += bytesSize;

            lastTimestamp = timestamp;
            totalFrameCount++;
        }


        public override string GetReport()
        {
            StringBuilder sb = new StringBuilder();

            var fps = (1 / this.avgFrameInterval);

            //var mbytesPerSec = this.avgBytesPerSec / (1024.0 * 1024);
            //var mbytes = this.totalBytes / (1024.0 * 1024);

            TimeSpan time = TimeSpan.FromSeconds(totalTime);
            sb.AppendLine(time.ToString(@"hh\:mm\:ss\.fff"));
            sb.AppendLine(fps.ToString("0.0") + " FPS");

            sb.AppendLine("");
            sb.AppendLine(this.totalFrameCount + " Frames");

            //sb.AppendLine(mbytesPerSec.ToString("0.0") + " MByte/s");

            sb.AppendLine(StringHelper.SizeSuffix((long)avgBytesPerSec) + "/s");
            sb.AppendLine(StringHelper.SizeSuffix(totalBytes));

            sb.AppendLine((jitterAvg).ToString("0.0") + " ms");

            return sb.ToString();
        }

        public override void Reset()
        {
            this.totalBytes = 0;
            this.totalFrameCount = 0;
            this.currentFrame = 0;
            this.currentBytes = 0;

            this.avgFrameInterval = 0;
            this.avgBytesPerSec = 0;
            this.lastTimestamp = 0;

        }
    }

}
