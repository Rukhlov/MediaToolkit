using ScreenStreamer.Utils;
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

namespace ScreenStreamer
{

    public class D3DImageProvider : INotifyPropertyChanged
    {
        public Dispatcher dispatcher = null;
        
        public D3DImageProvider()
        {

            this.dispatcher = Dispatcher.CurrentDispatcher;

        }


        private SharpDX.Direct3D9.Direct3DEx direct3D = null;
        private SharpDX.Direct3D9.DeviceEx device = null;

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
            this.screenSource = source;
            var sharedTexture = screenSource.hwContext.SharedTexture;

            var descr = sharedTexture.Description;

            direct3D = new Direct3DEx();

            var hWnd = User32.GetDesktopWindow();

            var presentparams = new PresentParameters
            {
                Windowed = true,
                MultiSampleType = MultisampleType.None,
                SwapEffect = SwapEffect.Discard,
                PresentFlags =PresentFlags.None,
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
            ScreenView = new D3DImage();

            this.Update();

            screenSource.BufferUpdated += ScreenSource_BufferUpdated;


        }

        private void ScreenSource_BufferUpdated()
        {

            Update();

        }

        public void Update()
        {
            if (surface == null)
            {
                return;
            }

           dispatcher.Invoke(() =>
           {
               if (screenView.IsFrontBufferAvailable)
               {
                   var ptr = surface.NativePointer;
                   screenView.Lock();
                   screenView.SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);

                   if (ptr != IntPtr.Zero)
                   {
                       screenView.AddDirtyRect(new Int32Rect(0, 0, ScreenView.PixelWidth, ScreenView.PixelHeight));
                   }

                   screenView.Unlock();
               }


           }, DispatcherPriority.Render);

        }

        public bool running = true;
        public void Close()
        {
            if (screenSource != null)
            {
                screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

            }

            if (surface != null)
            {
                surface.Dispose();
                surface = null;
            }

            if (direct3D != null)
            {
                direct3D.Dispose();
                direct3D = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
