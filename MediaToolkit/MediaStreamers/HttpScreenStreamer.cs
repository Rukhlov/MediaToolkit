using MediaToolkit.Core;
using MediaToolkit.SharedTypes;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.MediaStreamers
{
    public class HttpScreenStreamer : SharedTypes.IHttpScreenStreamer
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private VideoHttpStreamer httpStreamer = null;
        private IVideoSource httpScreenSource = null;

        public int ErrorCode { get; private set; } = 0;

        public event Action<object> StreamerStopped;
        public event Action StreamerStarted;

        public MediaState State { get; private set; } = MediaState.Closed;

        private SynchronizationContext syncContext = null;
        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private Task streamerTask = null;

        public HttpScreenStreamer()
        {
            syncContext = SynchronizationContext.Current;
        }

        public void Setup(HttpScreenStreamerArgs args)
        {
            logger.Debug("HttpScreenStreamer::Setup() " + args.ToString());

            if(State!= MediaState.Closed)
            {
                throw new InvalidOperationException("Invalid state " + State);
            }

            this.ErrorCode = 0;

            //var srcRect = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            //var destSize = new Size(1920, 1080);

            var srcRect = args.CaptureRegion;
            var destSize = args.Resolution;

            var ratio = srcRect.Width / (double)srcRect.Height;
            int destWidth = destSize.Width;
            int destHeight = (int)(destWidth / ratio);
            if (ratio < 1)
            {
                destHeight = destSize.Height;
                destWidth = (int)(destHeight * ratio);
            }

            destSize = new Size(destWidth, destHeight);
            var captureType = args.CaptureTypes;

            ScreenCaptureDeviceDescription captureParams = new ScreenCaptureDeviceDescription
            {
                CaptureRegion = srcRect,
                Resolution = destSize,

                CaptureType = captureType,
                Fps = (int)args.Fps,
                CaptureMouse = args.CaptureMouse,
                AspectRatio = true,
                UseHardware = false,
            };


            if (captureType == VideoCaptureType.GDI || captureType == VideoCaptureType.GDIPlus)
            {// масштабируем на энкодере
                captureParams.Resolution = new Size(srcRect.Width, srcRect.Height);
            }

            VideoEncoderSettings encodingParams = new VideoEncoderSettings
            {
                Resolution = destSize,
                FrameRate = captureParams.Fps,
                EncoderName = "mjpeg",
            };

            NetworkSettings networkParams = new NetworkSettings
            {
                RemoteAddr = args.Addres,
                RemotePort = args.Port,
            };

            try
            {
                httpScreenSource = new ScreenSource();
                httpScreenSource.Setup(captureParams);
                httpScreenSource.CaptureStopped += HttpScreenSource_CaptureStopped;

                httpStreamer = new VideoHttpStreamer(httpScreenSource);
                httpStreamer.Setup(encodingParams, networkParams);
                httpStreamer.StreamerStopped += HttpStreamer_StreamerStopped;

                State = MediaState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                this.ErrorCode = 100503;

                Close();
                throw;
            }

        }


        public void Start()
        {
            logger.Debug("HttpScreenStreamer::Start()");

            if (!(State == MediaState.Initialized || State == MediaState.Stopped))
            {
                throw new InvalidOperationException("Invalid state " + State);
            }

            State = MediaState.Starting;
            streamerTask = Task.Run(() => 
            {
                try
                {
                    httpScreenSource.Start();
                    httpStreamer.Start();

                    State = MediaState.Started;
                    OnStreamerStarted();

                    while (State == MediaState.Started)
                    {

                        if (httpScreenSource.ErrorCode != 0 || httpStreamer.ErrorCode != 0)
                        {//В одном из потоков ошибка - останавливаем стрим

                            logger.Warn("HttpScreenStreamer: Error occurred " + httpScreenSource.ErrorCode + ", " + httpStreamer.ErrorCode);

                            httpScreenSource.Stop();
                            httpStreamer.Stop();

                            ErrorCode = 100500;
                            break;
                        }

                        syncEvent.WaitOne(500);
                    }


                    while (State == MediaState.Stopping)
                    {// ждем остановки потоков
                        if (httpScreenSource.State == CaptureState.Stopped && httpStreamer.State == MediaState.Stopped)
                        {
                            logger.Debug("HttpScreenStreamer: All thread stopped...");
                            break;
                        }
                        else
                        {//...

                        }

                        syncEvent.WaitOne(500);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }

                object obj = null;
                if (ErrorCode != 0)
                {
                    obj = ErrorCode;
                }

                State = MediaState.Stopped;
                OnStreamerStopped(obj);

            });
        }


        private void OnStreamerStarted()
        {
            if (syncContext != null)
            {
                syncContext.Post( _=> StreamerStarted?.Invoke(), null);
            }
            else
            {
                StreamerStarted?.Invoke();
            }        
        }


        private void OnStreamerStopped(object obj)
        {
            if (syncContext != null)
            {
                syncContext.Post(_ => StreamerStopped?.Invoke(obj), null);
            }
            else
            {
                StreamerStopped?.Invoke(obj);
            }
        }

  
        private void HttpStreamer_StreamerStopped(object obj)
        {
            logger.Debug("HttpStreamer_StreamerStopped(...)");

        }

        private void HttpScreenSource_CaptureStopped(object obj)
        {
            logger.Debug("HttpScreenSource_CaptureStopped");


        }

        public void Stop()
        {
            logger.Debug("HttpScreenStreamer::Stop()");

            if(!(State == MediaState.Starting || State== MediaState.Started))
            {
                throw new InvalidOperationException("Invalid state " + State);
            }

            if (httpScreenSource != null)
            {
                httpScreenSource.Stop();
            }

            if (httpStreamer != null)
            {
                httpStreamer.Stop();
            }

            State = MediaState.Stopping;

        }

        public void Close(bool force = false)
        {
            logger.Debug("HttpScreenStreamer::Close() " + force + " " + State);
            //if (State == MediaState.Closed)
            //{
            //    return;
            //}

            if (State == MediaState.Started || State == MediaState.Starting)
            {
                Stop();
            }

            if (!force)
            {
                if (streamerTask != null)
                {
                    if (streamerTask.Status == TaskStatus.Running)
                    {
                        int tryCount = 10;
                        bool waitResult = false;
                        do
                        {
                            waitResult = streamerTask.Wait(1000);
                            if (!waitResult)
                            {
                                logger.Warn("HttpScreenStreamer::Close() " + waitResult);
                                State = MediaState.Stopping;

                            }

                        } while (!waitResult && tryCount-->0);

                        if(tryCount == 0)
                        {//FATAL:

                        }

                    }
                }
            }

            if (httpStreamer != null)
            {
                httpStreamer.StreamerStopped -= HttpStreamer_StreamerStopped;
                httpStreamer.Close(force);
            }


            if (httpScreenSource != null)
            { 
                httpScreenSource.CaptureStopped -= HttpScreenSource_CaptureStopped;
                httpScreenSource.Close(force);
            }

            State = MediaState.Closed;


        }

    }

}
