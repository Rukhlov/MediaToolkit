using MediaToolkit.Logging;
using SharpDX.Direct3D9;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.MediaFoundation
{
    public interface ID3DImageProvider
    {
        IntPtr GetSurfacePointer();
        event Action NewDataAvailable;
    }

    public class D3D11RendererProvider : ID3DImageProvider
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.MediaFoundation");

        private Direct3DEx direct3D = null;
        private DeviceEx device = null;
        private Surface surface = null;

        private volatile RendererState state = RendererState.Closed;
        public RendererState State => state;

        private SharpDX.Direct3D11.Texture2D texture2D11 = null;
        public void Init(SharpDX.Direct3D11.Texture2D sharedTexture)
        {
            logger.Debug("D3DRenderer::Setup()");

            if (state != RendererState.Closed)
            {
                throw new InvalidOperationException("Invalid capture state " + State);
            }

            try
            {
                var descr = sharedTexture.Description;
                logger.Verb(string.Join(", ", descr.Width + "x" + descr.Height, descr.Format));

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

                this.texture2D11 = sharedTexture;
                var pSurface = surface.NativePointer;

                state = RendererState.Initialized;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();

                throw;
            }

        }

        public bool TestDevice()
        {
            bool result = false;
            SharpDX.Result hr = SharpDX.Result.Fail;
            if (device != null)
            {
               hr = device.TestCooperativeLevel();

            }

            result = (texture2D11 != null && !texture2D11.IsDisposed) && hr.Success && (surface?.NativePointer != IntPtr.Zero);

            return result;
        }

        public bool ReInit(SharpDX.Direct3D11.Texture2D sharedTexture)
        {
            bool result = false;
            try
            {
                Close();

                if (sharedTexture != null && !sharedTexture.IsDisposed)
                {
                    Init(sharedTexture);

                    result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }


            return result;

        }

        public event Action NewDataAvailable;
        public void OnNewDataAvailable()
        {
            NewDataAvailable?.Invoke();
        }

        public IntPtr GetSurfacePointer()
        {
            if (!TestDevice())
            {
                return IntPtr.Zero;
            }

            return surface?.NativePointer ?? IntPtr.Zero; 
        }


        public void Close()
        {
            logger.Verb("D3DRenderer::Close()");

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

            state = RendererState.Closed;
        }

    }
}
