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
    public class D3DImageRenderer : INotifyPropertyChanged
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Dispatcher dispatcher = null;
        public D3DImageRenderer()
        {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            //ScreenView = new D3DImage();
        }

        public D3DImageRenderer(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private AutoResetEvent waitEvent = null;

        private Direct3DEx direct3D = null;
        private DeviceEx device = null;
        private Surface surface = null;

        private volatile RendererState state = RendererState.Closed;
        public RendererState State { get => state; }

        public int ErrorCode { get; private set; } = 0;

        //public D3DImage ScreenView = null;

        //private D3DImage screenView = new D3DImage();
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

        private string status = "_NotConnected";
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


        public event Action RenderStarted;
        public event Action<object> RenderStopped;

        public void Setup(Texture2D sharedTexture)
        {
            logger.Debug("D3DImageProvider::Setup()");

            if (state != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            try
            {
                //System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;

                var descr = sharedTexture.Description;

                if (descr.Format != SharpDX.DXGI.Format.B8G8R8A8_UNorm)
                {
                    throw new InvalidOperationException("Invalid renderer state " + State);
                }

                direct3D = new Direct3DEx();

                var hWnd = NativeAPIs.User32.GetDesktopWindow();

                var presentParams = new PresentParameters
                {
                    //Windowed = true,
                    //SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                    //DeviceWindowHandle = IntPtr.Zero,
                    //PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
                    //BackBufferCount = 1,

                    Windowed = true,
                    MultiSampleType = MultisampleType.None,
                    SwapEffect = SwapEffect.Discard,
                    PresentFlags = PresentFlags.None,
                };

                var flags = CreateFlags.HardwareVertexProcessing |
                            CreateFlags.Multithreaded |
                            CreateFlags.FpuPreserve;

                int adapterIndex = 0;

                device = new DeviceEx(direct3D, adapterIndex, DeviceType.Hardware, hWnd, flags, presentParams);

                using (var resource = sharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
                {
                    var handle = resource.SharedHandle;

                    if (handle == IntPtr.Zero)
                    {
                        throw new ArgumentNullException(nameof(handle));
                    }

                    // D3DFMT_A8R8G8B8 или D3DFMT_X8R8G8B8
                    // D3DUSAGE_RENDERTARGET
                    // D3DPOOL_DEFAULT
                    using (var texture3d9 = new SharpDX.Direct3D9.Texture(device,
                            descr.Width, descr.Height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default,
                            ref handle))
                    {
                        surface = texture3d9.GetSurfaceLevel(0);
                    };
                }

                waitEvent = new AutoResetEvent(false);

                dispatcher.Invoke(() =>
                {
                    ScreenView = new D3DImage();
                    
                    ScreenView.Lock();
                    ScreenView.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                    ScreenView.Unlock();


                }, DispatcherPriority.Send);

                state = RendererState.Initialized;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();

                throw;
            }

        }

        private Task renderTask = null;
        public void Start()
        {
            logger.Debug("D3DImageProvider::Start()");

            if (!(state == RendererState.Stopped || state == RendererState.Initialized))
            {
                throw new InvalidOperationException("Invalid renderer state " + State);
            }
           // OnPropertyChanged(nameof(ScreenView));
            renderTask = Task.Run(() =>
            {
                logger.Debug("Render thread started...");

                state = RendererState.Rendering;
                RenderStarted?.Invoke();

                //OnPropertyChanged(nameof(ScreenView));
                try
                {
                    while (State == RendererState.Rendering)
                    {

                        Draw();

                        waitEvent.WaitOne(1000);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    ErrorCode = 100500;
                }
                finally
                {
                    state = RendererState.Stopped;
                    RenderStopped?.Invoke(null);

                    logger.Debug("Render thread stopped...)");
                }
            });
        }

        public void Stop()
        {
            logger.Debug("D3DImageProvider::Stop()");

            state = RendererState.Stopping;
            ScreenView = null;
            waitEvent?.Set();
        }

        public void Update()
        {
            waitEvent?.Set();
        }


        private void Draw()
        {

            dispatcher.Invoke(() =>
            {
                // if (_D3DImage.IsFrontBufferAvailable)
                if(state == RendererState.Rendering)
                {
                    if (surface == null)
                    {
                        return;
                    }

                    var pSurface = surface.NativePointer;
                    if (pSurface != IntPtr.Zero)
                    {
                        ScreenView.Lock();
                        // Repeatedly calling SetBackBuffer with the same IntPtr is 
                        // a no-op. There is no performance penalty.
                        ScreenView.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
                        ScreenView.AddDirtyRect(new Int32Rect(0, 0, ScreenView.PixelWidth, ScreenView.PixelHeight));

                        ScreenView.Unlock();
                    }

                }

            }, DispatcherPriority.Render);


        }

        public void Close(bool force = false)
        {
            logger.Debug("D3DImageProvider::Close()");

            Stop();

            if (!force)
            {
                if (renderTask != null)
                {
                    if (renderTask.Status == TaskStatus.Running)
                    {
                        bool waitResult = false;
                        do
                        {
                            waitResult = renderTask.Wait(1000);
                            if (!waitResult)
                            {
                                logger.Warn("D3DImageProvider::Close() " + waitResult);
                                state = RendererState.Stopping;
                            }
                        } while (!waitResult);

                    }
                }
            }

            CleanUp();

        }

        private void CleanUp()
        {
            logger.Debug("D3DImageProvider::CleanUp()");

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

            if (waitEvent != null)
            {
                waitEvent.Dispose();
                waitEvent = null;
            }

            state = RendererState.Closed;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }

    public enum RendererState
    {
        Initialized,
        Stopped,
        Starting,
        Rendering,
        Stopping,
        Closed,
    }

}
