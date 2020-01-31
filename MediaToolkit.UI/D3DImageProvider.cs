using NLog;
using MediaToolkit.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

using Direct3D9 = SharpDX.Direct3D9;
using MediaToolkit.NativeAPIs;

namespace MediaToolkit.UI
{

    public class D3DImageProvider : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Dispatcher dispatcher = null;
        
        public D3DImageProvider()
        {

            this.dispatcher = Dispatcher.CurrentDispatcher;
            //System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;

        }


        private Direct3DEx direct3D = null;
        private DeviceEx device = null;

        private Surface surface = null;

        public Size SurfSize
        {
            get
            {
                var size = Size.Empty;
                if (surface != null)
                {
                    var descr = surface.Description;
                    size = new Size(descr.Width, descr.Height);

                }
                return size;
            }
        }

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

        private IVideoSource screenSource = null;

        public void Setup(IVideoSource source)
        {
            logger.Debug("D3DImageProvider::Setup(...)");

            try
            {
                Close();

                this.screenSource = source;
                var sharedTexture = screenSource.SharedTexture;
                screenSource.BufferUpdated += ScreenSource_BufferUpdated;

                VideoSource = new D3DImage();
                VideoSource.IsFrontBufferAvailableChanged += ScreenView_IsFrontBufferAvailableChanged;

                SetupDx(sharedTexture);
                Update(sharedTexture);

 
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }

        }

        private void ScreenView_IsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            logger.Debug("ScreenView_IsFrontBufferAvailableChanged(...) ");

            if (!VideoSource.IsFrontBufferAvailable)
            {
                ClosDx();
            }
            else
            {
                SetupDx(screenSource.SharedTexture);
            }
        }

        bool deviceReady = false;
        private void SetupDx(Texture2D sharedTexture)
        {
            logger.Debug("D3DImageProvider::SetupDx(...)");

            if(sharedTexture == null)
            {
                logger.Warn("sharedTexture == null");
                return;
            }

            var descr = sharedTexture.Description;

            direct3D = new Direct3DEx();

            var hWnd = User32.GetDesktopWindow();

            var presentparams = new PresentParameters
            {
                Windowed = true,
                MultiSampleType = MultisampleType.None,
                SwapEffect = SwapEffect.Discard,
                PresentFlags = PresentFlags.None,
            };

            var flags = CreateFlags.HardwareVertexProcessing |
                        CreateFlags.Multithreaded |
                        CreateFlags.FpuPreserve;

            int adapterIndex = 0;

            device = new DeviceEx(direct3D, adapterIndex, SharpDX.Direct3D9.DeviceType.Hardware, hWnd, flags, presentparams);

            using (var resource = sharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
            {
                var handle = resource.SharedHandle;
                if (handle == IntPtr.Zero)
                {
                    throw new ArgumentNullException(nameof(handle));
                }

                using (var texture3d9 = new Texture(device, descr.Width, descr.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, ref handle))
                {
                    surface = texture3d9.GetSurfaceLevel(0);
                }

            }
            deviceReady = true;
        }

        private void ScreenSource_BufferUpdated()
        {

            var sharedTexture = screenSource.SharedTexture;

            Update(sharedTexture);

        }

        

        public void Update(Texture2D sharedTexture)
        {
            if (/*surface == null || */sharedTexture == null)
            {
                logger.Warn("sharedTexture == null");

                return;
            }


           dispatcher.Invoke(() =>
           {
               if (videoSource.IsFrontBufferAvailable)
               {
                   if (!deviceReady)
                   {
                       SetupDx(sharedTexture);
                   }

                   if (deviceReady)
                   {
                       var surfPtr = surface.NativePointer;

                       if (surfPtr != IntPtr.Zero)
                       {
                           VideoSource.Lock();
                           VideoSource.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surfPtr, true);

                           VideoSource.AddDirtyRect(new Int32Rect(0, 0, VideoSource.PixelWidth, VideoSource.PixelHeight));
                           VideoSource.Unlock();
                       }
                   }

  
               }
               else
               {
                   //ClosDx();
               }


           }, DispatcherPriority.Render);

        }

        public bool running = true;

        private void ClosDx()
        {
            logger.Debug("D3DImageProvider::ClosDx()");

            deviceReady = false;
            if (surface != null)
            {
                surface.Dispose();
                surface = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (direct3D != null)
            {
                direct3D.Dispose();
                direct3D = null;
            }

        }

        public void Close()
        {
            logger.Debug("D3DImageProvider::Close()");

            if (screenSource != null)
            {
                screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

            }

            if (VideoSource != null)
            {
                VideoSource.IsFrontBufferAvailableChanged -= ScreenView_IsFrontBufferAvailableChanged;
            }

            ClosDx();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
