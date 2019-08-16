using CommonData;
using NLog;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScreenStreamer
{
    public class ScreenCaptureParams
    {
        public Rectangle SrcRect = new Rectangle(0, 0, 640, 480);
        public Size DestSize = new Size(640, 480);
        public CaptureType CaptureType = CaptureType.GDI;
        public int Fps = 10;
        public bool CaptureMouse = false;
    }

    public enum CaptureState
    {
        Create, 
        Capture,
        Close
    }

    public class ScreenSource
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenSource() { }

        public VideoBuffer Buffer { get; private set; }

        public CaptureState State { get; private set; } = CaptureState.Create;

        public DXGIDesktopDuplicationCapture hwContext = null;

        public event Action BufferUpdated;
        private void OnBufferUpdated()
        {
            BufferUpdated?.Invoke();
        }

        public event Action<CaptureState> StateChanged;
        private void OnStateChanged(CaptureState state)
        {
            StateChanged?.Invoke(state);
        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private ScreenCapture screenCapture = null;

        private ScreenCaptureParams captureParams = null;
        public void Setup(ScreenCaptureParams captureParams)
        {
            logger.Debug("ScreenSource::Setup()");

            this.captureParams = captureParams;

            var srcRect = captureParams.SrcRect;
            var destSize = captureParams.DestSize;


            try
            {
                screenCapture = ScreenCapture.Create(captureParams.CaptureType);
                screenCapture.CaptureMouse = captureParams.CaptureMouse;

                screenCapture.Init(srcRect, destSize);
                //screenCapture.Init(srcRect);

                DXGIDesktopDuplicationCapture capture = screenCapture as DXGIDesktopDuplicationCapture;
                if (capture != null)
                {
                    this.hwContext = capture;
                }

                this.Buffer = screenCapture.VideoBuffer;

            }
            catch (Exception ex)
            {
                logger.Error(ex);

                CleanUp();
                throw;
            }
        }

        public Task Start()
        {
            logger.Debug("ScreenSource::Start()");


            return Task.Run(() =>
            {
                logger.Info("Capturing thread started...");

                var frameRate = captureParams.Fps;

                CaptureStats captureStats = new CaptureStats();

                try
                {
                    Statistic.RegisterCounter(captureStats);
                    
                    var frameInterval = (1000.0 / frameRate);
                    captureStats.frameInterval = frameInterval;

                    double lastTime = 0;
                    double monotonicTime = 0;
                    double jitter = 0;

                    Stopwatch sw = Stopwatch.StartNew();
                    while (!closing)
                    {
                        sw.Restart();

                        try
                        {
                            var res = screenCapture.UpdateBuffer(30);
    
                            if (closing)
                            {
                                break;
                            }

                            if (res)
                            {
                                var time = (monotonicTime + sw.ElapsedMilliseconds / 1000.0); //MediaTimer.GetRelativeTime() ;

                                Buffer.time = time; //MediaTimer.GetRelativeTime() 

                                //var diff = time - lastTime;

                                lastTime = Buffer.time;

                                OnBufferUpdated();
 
                                captureStats.UpdateFrameStats(Buffer.time, (int)Buffer.Size);

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
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    screenCapture?.Close();

                    Statistic.UnregisterCounter(captureStats);

                    this.State = CaptureState.Close;

                    OnStateChanged(this.State);
                }

                logger.Info("Capturing thread stopped...");


            });

        }

        private bool closing = false;

        public void Close()
        {
            logger.Debug("ScreenSource::Close()");

            closing = true;
            syncEvent.Set();
        }

        private void CleanUp()
        {
            if (screenCapture != null)
            {
                screenCapture.Close();
                screenCapture = null;
            }
        }


    }


    class CaptureStats : ScreenStreamer.Utils.StatCounter
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

                jitterAvg += (int)((1.0 / 16.0) * ((double)Math.Abs(diff) - jitterAvg));

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
