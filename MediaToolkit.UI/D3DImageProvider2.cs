using NLog;
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
using System.Windows.Threading;


namespace MediaToolkit.UI
{
    public class D3DImageProvider2 : INotifyPropertyChanged
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dispatcher dispatcher = null;
        public D3DImageProvider2(System.Windows.Threading.Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private SharpDX.Direct3D9.Direct3DEx direct3D = null;
        private SharpDX.Direct3D9.DeviceEx device = null;
        private Surface surface = null;

        private System.Windows.Interop.D3DImage screenView = null;
        public System.Windows.Interop.D3DImage ScreenView
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


        private AutoResetEvent waitEvent = null;
        public void Start(Texture2D sharedTexture)
        {
            logger.Debug("D3DImageProvider::Start()");

            if (sharedTexture == null)
            {
                return;
            }

            //ScreenView = new System.Windows.Interop.D3DImage();


            Task.Run(() =>
            {
                waitEvent = new AutoResetEvent(false);
                Stopwatch sw = new Stopwatch();

                try
                {
                    StartUp(sharedTexture);

                    while (running)
                    {

                        Draw();

                        waitEvent.WaitOne(1000);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                 
                    CleanUp();
                }
            });
        }

        public void Update()
        {
            waitEvent?.Set();
        }


        private void StartUp(Texture2D sharedTexture)
        {
            try
            {
                logger.Debug("D3DImageProvider::Setup()");

                //System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;

                var descr = sharedTexture.Description;

                direct3D = new SharpDX.Direct3D9.Direct3DEx();

                var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

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

                running = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();

                throw;
            }

        }


        private void Draw()
        {

            dispatcher.Invoke(() =>
            {
                // if (_D3DImage.IsFrontBufferAvailable)
                {
                    if (surface == null)
                    {
                        return;
                    }

                    if (ScreenView == null)
                    {
                        ScreenView = new D3DImage();
                    }

                    var ptr = surface.NativePointer;

                    ScreenView.Lock();
                    ScreenView.SetBackBuffer(D3DResourceType.IDirect3DSurface9, ptr);

                    if (ptr != IntPtr.Zero)
                    {
                        ScreenView.AddDirtyRect(new Int32Rect(0, 0, ScreenView.PixelWidth, ScreenView.PixelHeight));
                    }

                    ScreenView.Unlock();
                }

            }, System.Windows.Threading.DispatcherPriority.Render);


        }

        public bool running = true;
        public void Close()
        {
            logger.Debug("D3DImageProvider::Close()");
            running = false;

            waitEvent?.Set();

        }

        private void CleanUp()
        {
            //System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;

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

            //if (waitEvent != null)
            //{
            //    waitEvent.Dispose();
            //    waitEvent = null;
            //}

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

}
