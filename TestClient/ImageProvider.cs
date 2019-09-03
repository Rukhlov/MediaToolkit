using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace MfTransformTest
{

    class D3DImageProvider : INotifyPropertyChanged
    {
        private readonly Dispatcher dispatcher = null;
        public D3DImageProvider(System.Windows.Threading.Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private SharpDX.Direct3D9.Direct3DEx direct3D = null;
        private SharpDX.Direct3D9.DeviceEx device = null;

        private System.Windows.Interop.D3DImage d3dImage = null;
        public System.Windows.Interop.D3DImage _D3DImage
        {
            get { return d3dImage; }
            private set
            {
                if (d3dImage != value)
                {
                    d3dImage = value;
                    OnPropertyChanged(nameof(_D3DImage));
                }
            }
        }

        //IntPtr BackBufferPtr = IntPtr.Zero;
        //int Width;
        //int Height;

        public void Setup(Texture2D sharedTexture)
        {
            var descr = sharedTexture.Description;
            //this.BackBufferPtr = BackBufferPtr;
            //this.Width = Width;
            //this.Height = Height;

            direct3D = new SharpDX.Direct3D9.Direct3DEx();

            var hWnd = GetDesktopWindow();

            var presentParams = new SharpDX.Direct3D9.PresentParameters
            {
                //Windowed = true,
                //SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                //DeviceWindowHandle = IntPtr.Zero,
                //PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default

                //BackBufferCount = 1,
                Windowed = true,
                MultiSampleType = SharpDX.Direct3D9.MultisampleType.None,
                SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                PresentFlags = SharpDX.Direct3D9.PresentFlags.None,
            };

            var flags = SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing |
                        SharpDX.Direct3D9.CreateFlags.Multithreaded |
                        SharpDX.Direct3D9.CreateFlags.FpuPreserve;

            int adapterIndex = 0;

            device = new SharpDX.Direct3D9.DeviceEx(direct3D, adapterIndex, SharpDX.Direct3D9.DeviceType.Hardware, hWnd, flags, presentParams);

            using (var resource = sharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
            {
                var handle = resource.SharedHandle;

                if (handle == IntPtr.Zero)
                {
                    throw new ArgumentNullException(nameof(handle));
                }

                using (var texture3d9 = new SharpDX.Direct3D9.Texture(device,
                        descr.Width,
                        descr.Height,
                        1,
                        SharpDX.Direct3D9.Usage.RenderTarget,
                        SharpDX.Direct3D9.Format.A8R8G8B8,
                        SharpDX.Direct3D9.Pool.Default,
                        ref handle))
                {
                    surface = texture3d9.GetSurfaceLevel(0);
                };
            }

            _D3DImage = new System.Windows.Interop.D3DImage();

            this.Update();

            running = true;
        }


        private Surface surface = null;
        public void Update()
        {

            if(surface ==null)
            { 
                return;
            }

            var ptr = surface.NativePointer;

            dispatcher.Invoke(() =>
            {
               // if (_D3DImage.IsFrontBufferAvailable)
                {
                    _D3DImage.Lock();
                    _D3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);

                    if (ptr != IntPtr.Zero)
                    {
                        _D3DImage.AddDirtyRect(new Int32Rect(0, 0, _D3DImage.PixelWidth, _D3DImage.PixelHeight));
                    }

                    _D3DImage.Unlock();
                }


            }, System.Windows.Threading.DispatcherPriority.Render);



            //resource.Dispose();

            //surface.Dispose();
            //texture3d9.Dispose();

            //waitEvent.Set();
        }

        public bool running = true;
        public void Close()
        {
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

            if (_D3DImage != null)
            {
               
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
        public static extern IntPtr GetDesktopWindow();


    }


}
