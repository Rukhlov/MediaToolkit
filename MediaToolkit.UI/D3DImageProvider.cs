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

namespace MediaToolkit
{

    public class D3DImageProvider : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public Dispatcher dispatcher = null;
        
        public D3DImageProvider()
        {

            this.dispatcher = Dispatcher.CurrentDispatcher;

        }

        private Direct3DEx direct3D = null;
        private DeviceEx device = null;

        private Surface surface = null;

        private D3DImage screenView = null;
        public D3DImage ScreenView
        {
            get { return screenView; }
            private set
            {
                if (screenView != value)
                {
                    screenView = value;
                    OnPropertyChanged(nameof(ScreenView));
                }
            }
        }

        private ScreenSource screenSource = null;

        public void Setup(ScreenSource source)
        {
            logger.Debug("D3DImageProvider::Setup(...)");

            try
            {
                this.screenSource = source;
                var sharedTexture = screenSource.hwContext.SharedTexture;
                screenSource.BufferUpdated += ScreenSource_BufferUpdated;

                ScreenView = new D3DImage();
                ScreenView.IsFrontBufferAvailableChanged += ScreenView_IsFrontBufferAvailableChanged;

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

            if (!ScreenView.IsFrontBufferAvailable)
            {
                ClosDx();
            }
            else
            {
                SetupDx(screenSource.hwContext.SharedTexture);
            }
        }

        bool deviceReady = false;
        private void SetupDx(Texture2D sharedTexture)
        {
            logger.Debug("D3DImageProvider::SetupDx(...)");

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

            var sharedTexture = screenSource.hwContext.SharedTexture;

            Update(sharedTexture);

        }

        

        public void Update(Texture2D sharedTexture)
        {
            if (surface == null)
            {
                return;
            }


           dispatcher.Invoke(() =>
           {
               if (screenView.IsFrontBufferAvailable)
               {
                   if (!deviceReady)
                   {
                       SetupDx(sharedTexture);
                   }

                   var ptr = surface.NativePointer;
                   screenView.Lock();
                   screenView.SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);

                   if (ptr != IntPtr.Zero)
                   {
                       screenView.AddDirtyRect(new Int32Rect(0, 0, ScreenView.PixelWidth, ScreenView.PixelHeight));
                   }

                   screenView.Unlock();
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

            ScreenView.IsFrontBufferAvailableChanged -= ScreenView_IsFrontBufferAvailableChanged;

            ClosDx();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
