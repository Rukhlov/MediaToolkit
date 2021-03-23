using MediaToolkit.Core;
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
using SharpDX.Direct3D11;
using MediaToolkit.DirectX;
using FFmpegLib;

namespace MediaToolkit
{
    public class ScreenCaptureSource : IVideoSource
    {
        // private static Logger logger = LogManager.GetCurrentClassLogger();
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit");

        public ScreenCaptureSource() { }

        public int AdapterIndex { get; private set; } = 0;

        private Device device = null;

        public VideoBufferBase VideoBuffer { get; private set; }

        private volatile CaptureState state = CaptureState.Closed;
        public CaptureState State => state;

        private volatile int errorCode = 0;
        public int ErrorCode => errorCode;

        private CaptureStats captureStats = new CaptureStats();
        public StatCounter Stats => captureStats;

        //public event Action BufferUpdated;
        //private void OnBufferUpdated()
        //{
        //    BufferUpdated?.Invoke();
        //}

        public event Action CaptureStarted;
        public event Action<object> CaptureStopped;

        private AutoResetEvent syncEvent = null;

        private ScreenCapture screenCapture = null;
        private D3D11RgbToYuvConverter pixConverter = null;

        private FFmpegPixelConverter ffmpegConverter = null;

        public ScreenCaptureProperties CaptureProps { get; private set; }

        private static DDAOutputManager outputManager = new DDAOutputManager();
        private bool deviceReady = false;


        public void Setup(object pars)//ScreenCaptureParams captureParams)
        {
            logger.Debug("ScreenSource::Setup()");

            VideoCaptureDevice captureParams = pars as VideoCaptureDevice;
            if (captureParams == null)
            {
                throw new ArgumentException();
            }

            if (state != CaptureState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            var srcRect = Rectangle.Empty;
            var destSize = Size.Empty;
            var hwnd = IntPtr.Zero;

            try
            {
                if (captureParams.CaptureMode == CaptureMode.Screen)
                {
                    var screenCaptParams = (ScreenCaptureDevice)pars;
                    this.CaptureProps = screenCaptParams.Properties;

                    srcRect = screenCaptParams.CaptureRegion;

                    var srcLocation = srcRect.Location;
                    var srcSize = GraphicTools.DecreaseToEven(srcRect.Size);
                    srcRect = new Rectangle(srcLocation, srcSize);

                    //srcRect = new Rectangle(x, y, width, height);
                    if (screenCaptParams.CaptureRegion != srcRect)
                    {
                        screenCaptParams.CaptureRegion = srcRect;
                    }

                    destSize = captureParams.Resolution;

                    if (destSize.IsEmpty)
                    {
                        destSize = new Size(srcRect.Width, srcRect.Height);
                    }

                }
                else if (captureParams.CaptureMode == CaptureMode.AppWindow)
                {
                    var windowCaptParams = (WindowCaptureDevice)pars;
                    this.CaptureProps = windowCaptParams.Properties;

                    srcRect = windowCaptParams.ClientRect;

                    var srcLocation = srcRect.Location;
                    var srcSize = GraphicTools.DecreaseToEven(srcRect.Size);
                    srcRect = new Rectangle(srcLocation, srcSize);

                    destSize = captureParams.Resolution;

                    if (destSize.IsEmpty)
                    {
                        destSize = new Size(srcRect.Width, srcRect.Height);
                    }

                    hwnd = windowCaptParams.hWnd;

                }

                InitDx();

                screenCapture = ScreenCapture.Create(CaptureProps.CaptureType);
                var captParams = new ScreenCaptureParameters
                {
                    SrcRect = srcRect,
                    DestSize = destSize,
                    CaptureMouse = CaptureProps.CaptureMouse,
                    UseHwContext = true,
                    D3D11Device = device,
                    DDAOutputMan = outputManager,
                };

                screenCapture.Init(captParams);
  
                var driverType = captureParams.DriverType;
                var destFormat = captureParams.Format;
                if (driverType == VideoDriverType.CPU)
                {
                    VideoBuffer = new MemoryVideoBuffer(destSize, destFormat, 32);
                }
                else if (driverType == VideoDriverType.D3D11)
                {
                    VideoBuffer = new D3D11VideoBuffer(device, destSize, destFormat);
                }
                else
                {
                    throw new InvalidOperationException("Invalid driver type: " + driverType);
                }

                var captSize = screenCapture.SrcRect.Size;
                var captFormat = screenCapture.SrcFormat;
                var downscaleFilter = captureParams.DownscaleFilter;

                var colorSpace = captureParams.ColorSpace;
                var colorRange = captureParams.ColorRange;

                if (captParams.UseHwContext)
                {
                    pixConverter = new D3D11RgbToYuvConverter();
                    pixConverter.KeepAspectRatio = CaptureProps.AspectRatio;
                    pixConverter.Init(device, captSize, captFormat, destSize, destFormat, downscaleFilter, colorSpace, colorRange);
                }
                else
                {
                    ffmpegConverter = new FFmpegPixelConverter();
                    ffmpegConverter.Init(captSize, captFormat, destSize, destFormat, downscaleFilter);
                }

                syncEvent = new AutoResetEvent(false);

                deviceReady = true;

                state = CaptureState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                LastError = ex;

                errorCode = (int)SharedTypes.ErrorCode.NotInitialized;

                CleanUp();

                state = CaptureState.Closed;
                throw;
            }
        }

        private void InitDx()
        {
            SharpDX.DXGI.Factory1 dxgiFactory = null;

            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();
                SharpDX.DXGI.Adapter1 adapter = null;
                try
                {
                    adapter = dxgiFactory.GetAdapter1(AdapterIndex);
                    //AdapterId = adapter.Description.Luid;
                    //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                    device = DxTool.CreateMultithreadDevice(adapter);
                }
                finally
                {
                    if (adapter != null)
                    {
                        adapter.Dispose();
                        adapter = null;
                    }
                }
            }
            finally
            {
                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }

        }


        public object LastError { get; private set; }

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
                    LastError = ex;
                    errorCode = (int)SharedTypes.ErrorCode.Unexpected;
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

            captureStats.Reset();

            try
            {

                // Statistic.RegisterCounter(captureStats);

                var fps = CaptureProps.Fps;
                var frameInterval = (1000.0 / fps);
                captureStats.frameInterval = frameInterval;

                double monotonicTime = 0;

                Stopwatch sw = Stopwatch.StartNew();

                while (state == CaptureState.Capturing && deviceReady)
                {
                    sw.Restart();

                    IVideoFrame screenFrame = null;
                    try
                    {
                        var res = screenCapture.TryGetFrame(out screenFrame);

                        if (state != CaptureState.Capturing || !deviceReady)
                        {
                            break;
                        }

                        IVideoFrame targetFrame = null;
                        if (res == SharedTypes.ErrorCode.Ok)
                        {
                            targetFrame = VideoBuffer.GetFrame();
                            ProcessFrame(screenFrame, targetFrame);
                        }
                        else if (res == SharedTypes.ErrorCode.WaitTimeout)
                        {
                            logger.Warn("WaitTimeout");
                            targetFrame = VideoBuffer.GetFrame();
                        }
                        else if (res == SharedTypes.ErrorCode.AccessDenied)
                        {
                            //logger.Warn("No screen buffer...");

                            logger.Warn("screenCapture.UpdateBuffer(...) == ERROR_ACCESS_DENIED try SwitchToInputDesktop()");

                            DesktopManager.SwitchToInputDesktop();
                        }
                        else
                        {
                            logger.Warn("screenCapture.UpdateBuffer(...): " + res);
                        }


                        if (targetFrame != null)
                        {
                            var time = (monotonicTime + sw.ElapsedMilliseconds / 1000.0); //MediaTimer.GetRelativeTime() ;
                            targetFrame.Time = time;
                            targetFrame.Duration = 0;// !!!!!!

                            VideoBuffer.OnBufferUpdated(targetFrame);
                            captureStats.UpdateFrameStats(targetFrame.Time, targetFrame.DataLength);
                        }
                    }

                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        Thread.Sleep(1000);
                    }
                    finally
                    {
                        if (screenFrame != null)
                        {
                            ((VideoFrameBase)screenFrame).Dispose();
                            screenFrame = null;
                        }
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

                //Statistic.UnregisterCounter(captureStats);

                state = CaptureState.Stopped;

            }

        }

        private void ProcessFrame(IVideoFrame screenFrame, IVideoFrame targetFrame)
        {
            if(screenFrame.DriverType == VideoDriverType.D3D11)
            {
                pixConverter.Process(screenFrame, targetFrame);
            }
            else if (screenFrame.DriverType == VideoDriverType.GDI)
            {
                ProcessGdiFrame(screenFrame, targetFrame);
            }
            else
            {
                throw new InvalidOperationException("Invalid frame drive type: " + screenFrame.DriverType);
            }
        }

        private void ProcessGdiFrame(IVideoFrame screenFrame, IVideoFrame targetFrame)
        {
            //ffmpegConverter.Convert()
            var bmp = ((GDIFrame)screenFrame).GdiBitmap;
            if (bmp != null)
            {
                //bmp.Save("test.jpg");

                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                try
                {
                    var srcPtr = bmpData.Scan0;
                    var srcStride = bmpData.Stride;
                    IFrameBuffer[] srcBuffer = new FrameBuffer[]
                    {
                        new FrameBuffer(srcPtr, srcStride)
                    };

                    var destBuffer = targetFrame.Buffer;
                    var _res = ffmpegConverter.Convert(ref srcBuffer, ref destBuffer);
                }
                finally
                {
                    bmp.UnlockBits(bmpData);
                }
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
            CloseDx();

            if (ffmpegConverter != null)
            {
                ffmpegConverter.Close();
                ffmpegConverter = null;
            }

            if (pixConverter != null)
            {
                pixConverter.Close();
                pixConverter = null;
            }

            if (VideoBuffer != null)
            {
                VideoBuffer.Dispose();
                VideoBuffer = null;
            }

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

        private void CloseDx()
        {
            if (device != null)
            {
                device.Dispose();
                device = null;
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
