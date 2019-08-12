using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.IO;

using System.Threading;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
//using SharpDX.Direct2D1;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;
using ScreenStreamer.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
namespace ScreenStreamer.MediaFoundation
{


    public class VideoWriterArgs
    {
        public string FileName { get; set; }
        public int Width { get; set; } = 1280;
        public int Height { get; set; } = 720;

        //public IImageProvider ImageProvider { get; set; }
        public int FrameRate { get; set; } = 15;
        public int VideoQuality { get; set; } = 70;
        //public int AudioQuality { get; set; } = 50;
        //public IAudioProvider AudioProvider { get; set; }
    }

    enum RateControlMode
    {
        CBR,
        PeakConstrainedVBR,
        UnconstrainedVBR,
        Quality,
        LowDelayVBR,
        GlobalVBR,
        GlobalLowDelayVBR
    };


    class MfTool
    {
        public static long PackToLong(int left, int right)
        {
            return ((long)left << 32 | (uint)right);
        }
    }


    public class MfContext
    {
        public readonly object syncRoot = new object();

        public Texture2D StagingTexture { get; set; }
        public DXGIDeviceManager DeviceManager { get; private set; } = new DXGIDeviceManager();

        private SharpDX.DXGI.Factory1 dxgiFactory = null;

        private Device device = null;
        public Adapter1 adapter = null;

        public void Init()
        {

            dxgiFactory = new SharpDX.DXGI.Factory1();
            //adapter = dxgiFactory.Adapters1.FirstOrDefault();

            SharpDX.Direct3D.DriverType driverType = SharpDX.Direct3D.DriverType.Hardware;

            DeviceCreationFlags creationFlags = DeviceCreationFlags.VideoSupport |
                                        DeviceCreationFlags.BgraSupport |
                                        DeviceCreationFlags.Debug;

            SharpDX.Direct3D.FeatureLevel[] features =
            {
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                    SharpDX.Direct3D.FeatureLevel.Level_10_0,
                    SharpDX.Direct3D.FeatureLevel.Level_9_1,
             };

            // device3d11 = new Device(adapter, DeviceCreationFlags.BgraSupport);
            device = new Device(driverType, creationFlags, features);
            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            DeviceManager.ResetDevice(device);

        }

        public void Close()
        {
            if (StagingTexture != null)
            {
                StagingTexture.Dispose();
                StagingTexture = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }

            if (adapter != null)
            {
                adapter.Dispose();
                adapter = null;
            }

            if (dxgiFactory != null)
            {
                dxgiFactory.Dispose();
                dxgiFactory = null;
            }

            if (DeviceManager != null)
            {
                DeviceManager.Dispose();
                DeviceManager = null;
            }
        }
    }

}
