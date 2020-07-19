using MediaToolkit.Logging;
using MediaToolkit.MediaFoundation;
using MediaToolkit.SharedTypes;

using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;


namespace MediaToolkit.UI
{

    public class D3DImageRenderer : INotifyPropertyChanged
    {

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.UI");

        public D3DImageRenderer()
        {
            //videoSource = new D3DImage();
        }

        private ID3DImageProvider d3dProvider = null;

        public bool EnableSoftwareFallback { get; set; } = false;

        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode ErrorCode => errorCode;

        private D3DImage videoSource = null;
        public D3DImage VideoSource
        {
            get { return videoSource; }
            private set
            {
                
                if (videoSource != value)
                {
                    videoSource = value;
                    OnPropertyChanged(nameof(VideoSource));
                }
            }
        }

        private Brush background = new SolidColorBrush(Colors.Black);
        public Brush Background
        {
            get { return background; }
            set
            {
                if (background != value)
                {
                    background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }

        private Stretch stretchMode = Stretch.Uniform;
        public Stretch StretchMode
        {
            get { return stretchMode; }
            set
            {
                if (stretchMode != value)
                {
                    stretchMode = value;
                    OnPropertyChanged(nameof(StretchMode));
                }
            }
        }

        private string status = "...";
        public string Status
        {
            get { return status; }
            set
            {
                if (status != value)
                {
                    status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }


        private bool initialized = false;
        public void Run(ID3DImageProvider provider)
        {
            logger.Debug("D3DImageRendererEx::Run(...)");

            if (initialized)
            {
                logger.Warn("D3DImageRendererEx::Run(...) invalid state");
                return;
            }

            try
            {
                d3dProvider = provider;
                d3dProvider.NewDataAvailable += D3dProvider_NewDataAvailable;
                VideoSource = new D3DImage();

                var pSurface = d3dProvider.GetSurfacePointer();

                if (pSurface != IntPtr.Zero)
                {
                    VideoSource.Lock();
                    VideoSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
                    VideoSource.Unlock();
                }

                System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;

                initialized = true;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Shutdown();

               // throw;
            }

        }

        private volatile bool needRedraw = false;
        private void D3dProvider_NewDataAvailable()
        {
            needRedraw = true;
        }

        private TimeSpan lastRender;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (!initialized)
            {
                return;
            }

            try
            {
                RenderingEventArgs args = (RenderingEventArgs)e;

                if (VideoSource.IsFrontBufferAvailable && lastRender != args.RenderingTime)
                {
                    if (needRedraw)
                    {
                        var pSurface = d3dProvider.GetSurfacePointer();

                        if (pSurface != IntPtr.Zero)
                        {
                            VideoSource.Lock();
                            // Repeatedly calling SetBackBuffer with the same IntPtr is 
                            // a no-op. There is no performance penalty.
                            VideoSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface, EnableSoftwareFallback);

                            var rect = new Int32Rect(0, 0, VideoSource.PixelWidth, VideoSource.PixelHeight);
                            VideoSource.AddDirtyRect(rect);

                            VideoSource.Unlock();
                        }

                        needRedraw = false;

                    }

                    lastRender = args.RenderingTime;

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Shutdown();

                errorCode = ErrorCode.Unexpected;

            }


        }


        public void Shutdown()
        {
            logger.Debug("D3DImageRendererEx::Shutdown()");

            initialized = false;
            needRedraw = false;

            if (d3dProvider != null)
            {
                d3dProvider.NewDataAvailable -= D3dProvider_NewDataAvailable;
                d3dProvider = null;
            }

            System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;

            VideoSource = null;
;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }


}
