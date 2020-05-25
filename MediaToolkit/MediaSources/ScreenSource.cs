﻿using MediaToolkit.Core;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Logging;
using MediaToolkit.ScreenCaptures;

namespace MediaToolkit
{
    public class ScreenSource : IVideoSource
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");

        public ScreenSource() { }

        public VideoBuffer SharedBitmap { get; private set; }

        public SharpDX.Direct3D11.Texture2D SharedTexture
        {
            get { return hwContext?.SharedTexture; }
        }
        
        public long AdapterId { get; private set; } = -1;


        public Size SrcSize
        {
            get
            {
                return new Size(SharedBitmap.bitmap.Width, SharedBitmap.bitmap.Height);
            }
        }

        private volatile CaptureState state = CaptureState.Closed;
        public CaptureState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        //private DXGIDesktopDuplicationCapture hwContext = null;

        private ITexture2DSource hwContext = null;

		public event Action BufferUpdated;
        private void OnBufferUpdated()
        {
            BufferUpdated?.Invoke();
        }

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        private AutoResetEvent syncEvent = null;

        private ScreenCapture screenCapture = null;

        public ScreenCaptureDevice CaptureParams { get; private set; }

        private bool deviceReady = false;
        public void Setup(object pars)//ScreenCaptureParams captureParams)
        {

            logger.Debug("ScreenSource::Setup()");

            if (state != CaptureState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            syncEvent = new AutoResetEvent(false);
            ScreenCaptureDevice captureParams = pars as ScreenCaptureDevice;

            if(captureParams == null)
            {
                throw new ArgumentException();
            }

            this.CaptureParams = captureParams;

            var srcRect = captureParams.CaptureRegion;

            var srcLocation = srcRect.Location;
            var srcSize = GraphicTools.DecreaseToEven(srcRect.Size);
            srcRect = new Rectangle(srcLocation, srcSize);

            //int x = srcRect.X;
            //int y = srcRect.Y;
            //int width = srcRect.Width;
            //if (width % 2!= 0)
            //{
            //    width--;
            //}

            //int height = srcRect.Height;
            //if (height % 2 != 0)
            //{
            //    height--;
            //}

            //srcRect = new Rectangle(x, y, width, height);
            if (captureParams.CaptureRegion != srcRect)
            {
                captureParams.CaptureRegion = srcRect;
            }
            

            var destSize = captureParams.Resolution;

            if (destSize.IsEmpty)
            {
                destSize = new Size(srcRect.Width, srcRect.Height);
            }

            try
            {
                //var captureDescr = captureParams.CaptureDescription;
                var captureProp = captureParams.Properties;
                screenCapture = ScreenCapture.Create(captureProp.CaptureType);
                screenCapture.CaptureMouse = captureProp.CaptureMouse;
                screenCapture.AspectRatio = captureProp.AspectRatio;

				var d3d11Capture = screenCapture as ITexture2DSource;
				if (d3d11Capture != null)
				{
					d3d11Capture.UseHwContext = captureProp.UseHardware;
					this.hwContext = d3d11Capture;
					this.AdapterId = d3d11Capture.AdapterId;
				}

				//DXGIDesktopDuplicationCapture capture = screenCapture as DXGIDesktopDuplicationCapture;
    //            if (capture != null)
    //            {
    //                capture.UseHwContext = captureProp.UseHardware;

    //                this.hwContext = capture;
    //                this.AdapterId = capture.AdapterId;
    //            }


				screenCapture.Init(srcRect, destSize);
				//screenCapture.Init(srcRect);


				this.SharedBitmap = screenCapture.VideoBuffer;

                deviceReady = true;

                state = CaptureState.Initialized;


            }
            catch (Exception ex)
            {
  
                logger.Error(ex);
                errorCode = 100504;

                CleanUp();

                state = CaptureState.Closed;
                throw;
            }
        }
        private Task captureTask = null;
        public void Start()
        {
            logger.Debug("ScreenSource::Start()");
            if (!(state == CaptureState.Stopped || state == CaptureState.Initialized))
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

                    errorCode = 100500;
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
            state = CaptureState.Capturing;

            CaptureStats captureStats = new CaptureStats();
            try
            {                

                Statistic.RegisterCounter(captureStats);

                var frameRate = CaptureParams.Properties.Fps;
                var frameInterval = (1000.0 / frameRate);
                captureStats.frameInterval = frameInterval;

                double lastTime = 0;
                double monotonicTime = 0;

                Stopwatch sw = Stopwatch.StartNew();


                while (state == CaptureState.Capturing && deviceReady)
                {
                    sw.Restart();

                    try
                    {
                        var res = screenCapture.UpdateBuffer(30);

                        if (state != CaptureState.Capturing || !deviceReady)
                        {
                            break;
                        }

                        if (res == SharedTypes.ErrorCode.Ok)
                        {
                            var time = (monotonicTime + sw.ElapsedMilliseconds / 1000.0); //MediaTimer.GetRelativeTime() ;

                            SharedBitmap.time = time; //MediaTimer.GetRelativeTime() 

                            //var diff = time - lastTime;

                            lastTime = SharedBitmap.time;

                            OnBufferUpdated();

                            captureStats.UpdateFrameStats(SharedBitmap.time, (int)SharedBitmap.DataLength);

                        }
                        else if (res == SharedTypes.ErrorCode.WaitTimeout)
                        {
                            //logger.Warn("No screen buffer...");
                        }
                        else if (res == SharedTypes.ErrorCode.AccessDenied)
                        {
                            //logger.Warn("No screen buffer...");

                            logger.Warn("screenCapture.UpdateBuffer(...) == ERROR_ACCESS_DENIED try SwitchToInputDesktop()");

                            NativeAPIs.Utils.DesktopManager.SwitchToInputDesktop();

                        }
                        else
                        {
                            logger.Warn("screenCapture.UpdateBuffer(...): " + res);
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

                state = CaptureState.Stopped;

            }

        }

  
        public void Stop()
        {
            logger.Debug("ScreenSource::Close()");

            state = CaptureState.Stopping;

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

            state = CaptureState.Closed;
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
