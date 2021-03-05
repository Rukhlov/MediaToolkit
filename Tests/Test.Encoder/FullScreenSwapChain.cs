using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDI = System.Drawing;
using SharpDX.WIC;
using System.Runtime.InteropServices;
using Direct2D = SharpDX.Direct2D1;
using MediaToolkit.NativeAPIs;
using MediaToolkit.DirectX;

namespace Test.Encoder
{
    class FullScreenSwitchTest
    {

        public static void Run()
        {
            Console.WriteLine("FullScreenSwapChain BEGIN");

            bool fullScreen = false;
            Form f = new Form
            { };

            var size = new GDI.Size(640, 480);
            f.ClientSize = size;

            FullScreenSwitchTest swapChain = new FullScreenSwitchTest();

            swapChain.ViewHandle = f.Handle;
            var task = Task.Run(() =>
            {
                try
                {
                    swapChain.Setup();

                    while (true)
                    {
                        Console.WriteLine("'F' to switch full screen state, 'Esc' to exit...");
                        var key = Console.ReadKey();
                        if (key.Key == ConsoleKey.Escape)
                        {
                            break;
                        }
                        else if (key.Key == ConsoleKey.F)
                        {
                            fullScreen = !fullScreen;
                            swapChain.SetFullScreen(fullScreen);
                        }
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    swapChain.Close();
                }

            });

            task.Wait();

            f.Dispose();
            Console.WriteLine("FullScreenSwapChain END");
        }

        public int AdapterIndex = 0;
        public int ImageWidth = 1280;
        public int ImageHeight = 720;
        public IntPtr ViewHandle = IntPtr.Zero;
        public int FramePerSec = 60;

        private SharpDX.Direct3D11.Device device = null;
        private SwapChain swapChain = null;
        private Adapter adapter = null;
        private SharpDX.DXGI.Factory1 dxgiFactory = null;
        private int outputIndex = 0;

        public void Setup()
        {
            dxgiFactory = new SharpDX.DXGI.Factory1();
            adapter = dxgiFactory.GetAdapter1(AdapterIndex);

            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
             };

            var deviceCreationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

            device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);

            Console.WriteLine($"RendererAdapter {AdapterIndex}: " + adapter.Description.Description);

            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            var scd = new SwapChainDescription
            {
                SampleDescription = new SampleDescription { Count = 1, Quality = 0 },
                SwapEffect = SwapEffect.FlipSequential,
                ModeDescription = new ModeDescription
                {
                    Format = Format.R8G8B8A8_UNorm,
                    Scaling = DisplayModeScaling.Stretched,
                    //Scaling = DisplayModeScaling.Centered,
                    Width = ImageWidth,
                    Height = ImageHeight,
                    RefreshRate = new Rational(FramePerSec, 1),
                },
                IsWindowed = true,
                Usage = Usage.RenderTargetOutput | Usage.BackBuffer,
                Flags = SwapChainFlags.None,
                BufferCount = 4,

                OutputHandle = ViewHandle,
            };

            swapChain = new SwapChain(dxgiFactory, device, scd);

        }

        public void SetFullScreen(bool fullScreen)
        {
            Console.WriteLine("SetFullScreen(...) " + fullScreen);
            Output o = null;
            try
            {
                if (fullScreen)
                {
                    o = adapter.GetOutput(outputIndex);
                }
                swapChain.SetFullscreenState(fullScreen, o);
            }
            finally
            {
                DxTool.SafeDispose(o);
            }
        }

        private volatile bool running = false;
        public void Start()
        {

            Task.Run(() =>
            {

                AutoResetEvent syncEvent = new AutoResetEvent(false);
                running = true;
                while (running)
                {
                    using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0))
                    {

                        //swapChain.Present(1, PresentFlags.None);
                    }

                    syncEvent.WaitOne(33);
                }

                syncEvent.Dispose();

                Close();

            });

        }

        public void Stop()
        {
            running = false;

        }

        public void Close()
        {
            DxTool.SafeDispose(dxgiFactory);
            DxTool.SafeDispose(device);
            DxTool.SafeDispose(adapter);
            DxTool.SafeDispose(swapChain);
        }
    }
}
