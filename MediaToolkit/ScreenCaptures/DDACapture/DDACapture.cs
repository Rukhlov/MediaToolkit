
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
using MediaToolkit.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;
using MediaToolkit.NativeAPIs;
using MediaToolkit.DirectX;
using MediaToolkit.Core;

namespace MediaToolkit.ScreenCaptures
{
    public class DDACapture : ScreenCapture
    { 
        public DDACapture(Dictionary<string, object> args = null) : base()
        { }

        //private static TraceSource logger = TraceManager.GetTrace("DXGIDesktopDuplicationCapture");

        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private Device mainDevice = null;

        private Texture2D compositionTexture = null;

        private Texture2D sharedTexture = null;

        public int AdapterIndex { get; private set; }

        //public bool UseHwContext { get; set; } = true;

        public int PrimaryAdapterIndex { get; set; } = 0;

        private bool internalOutputManager = false;
        public DDAOutputManager OutputManager { get; set; } = new DDAOutputManager();

        // private List<_DesktopDuplicator> deskDupls = new List<_DesktopDuplicator>();

        private List<DDAOutputProvider> providers = new List<DDAOutputProvider>();

        private GDI.Rectangle normalizedSrcRect = GDI.Rectangle.Empty;

        private Dictionary<int, Device> adapterToDeviceMap = new Dictionary<int, Device>();

        private VideoFrameBase srcFrame = null;
        private D3D11RgbToYuvConverter processor = null;

        public override void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Init() " + srcRect.ToString() + " " + destSize.ToString());

            base.Init(srcRect, destSize);

            this.SrcRect = srcRect;
            if (destSize.IsEmpty)
            {
                destSize.Width = srcRect.Width;
                destSize.Height = srcRect.Height;
            }
            this.DestSize = new GDI.Size(destSize.Width, destSize.Height);

            if (OutputManager == null)
            {
                OutputManager = new DDAOutputManager();
                internalOutputManager = true;
            }

            try
            {
                InitDx();

                if (DriverType == VideoDriverType.D3D11)
                {
                    VideoBuffer = new D3D11VideoBuffer(mainDevice, DestSize, DestFormat);
                }
                else if (DriverType == VideoDriverType.CPU)
                {

                    VideoBuffer = new MemoryVideoBuffer(DestSize, DestFormat, 16);
                }
                else
                {
                    throw new InvalidOperationException("Invalid driver type: " + DriverType);
                }

                processor = new D3D11RgbToYuvConverter();
                processor.KeepAspectRatio = AspectRatio;
                processor.Init(mainDevice, SrcRect.Size, SrcFormat, DestSize, DestFormat);

                srcFrame = new D3D11VideoFrame(sharedTexture);

            }
            catch (SharpDXException ex)
            {
                // Process error...
                logger.Error(ex);

                throw new Exception("DXGI initialization error [" + ex.ResultCode + "]");
            }

        }


        private GDI.Rectangle SetupRegions(RawRectangle screenRect, GDI.Rectangle srcRect)
        {

            int left, right, top, bottom;

            if (srcRect.X < screenRect.Left)
            {// за левой границей экрана
                left = screenRect.Left;
            }
            else
            {
                left = srcRect.X;
            }

            if (srcRect.Right > screenRect.Right)
            { // за правой границей
                right = screenRect.Right;
            }
            else
            {
                right = srcRect.Right;
            }

            if (srcRect.Y < screenRect.Top)
            {// за верхней границей
                top = screenRect.Top;
            }
            else
            {
                top = srcRect.Y;
            }

            if (srcRect.Bottom > screenRect.Bottom)
            {// за нижней границей
                bottom = screenRect.Bottom;
            }
            else
            {
                bottom = srcRect.Bottom;
            }

            // в координатах сцены
            var drawRect = new GDI.Rectangle
            {
                X = left - srcRect.X,
                Y = top - srcRect.Y,
                Width = right - left,
                Height = bottom - top,
            };

            // в координатах захвата экрана
            var duplLeft = left - screenRect.Left;
            var duplRight = right - screenRect.Left;
            var duplTop = top - screenRect.Top;
            var duplBottom = bottom - screenRect.Top;

            var duplRect = new GDI.Rectangle
            {
                X = duplLeft,
                Y = duplTop,
                Width = duplRight - duplLeft,
                Height = duplBottom - duplTop,
            };


            logger.Debug("duplRect=" + duplRect.ToString() + " drawRect=" + drawRect.ToString());

            return duplRect;
        }


        private void InitDx()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::InitDx(...) " + SrcRect.ToString());


            SharpDX.DXGI.Factory1 dxgiFactory = null;
            Adapter1 primaryAdapter = null;
            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();

                logger.Info(MediaToolkit.DirectX.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

                //PrimaryAdapterIndex = 0;
                // первым идет адаптер с которому подключен primary монитор
                primaryAdapter = dxgiFactory.GetAdapter1(PrimaryAdapterIndex);
                //AdapterId = primaryAdapter.Description.Luid;
                AdapterIndex = PrimaryAdapterIndex;

                //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                var deviceCreationFlags =
                    //DeviceCreationFlags.Debug |
                    DeviceCreationFlags.BgraSupport;

                SharpDX.Direct3D.FeatureLevel[] featureLevel =
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    SharpDX.Direct3D.FeatureLevel.Level_10_1,
                };

                mainDevice = new Device(primaryAdapter, deviceCreationFlags, featureLevel);
                using (var multiThread = mainDevice.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }

                adapterToDeviceMap[0] = mainDevice;

                if (providers != null)
                {
                    //...
                }

                providers = new List<DDAOutputProvider>();

                var adaptersCount = dxgiFactory.GetAdapterCount1();
                //if (adaptersCount > 2)
                {// обычно 2 адаптера hardware(GPU) + software(Microsoft Basic Render Driver)
                    var adapters = dxgiFactory.Adapters1;
                    for (int adapterIndex = 0; adapterIndex < adapters.Length; adapterIndex++)
                    {
                        var adapter = adapters[adapterIndex];
                        var adapterDescr = adapter.Description1;
                        var flags = adapterDescr.Flags;
                        if (flags == AdapterFlags.None)
                        {
                            var outputCount = adapter.GetOutputCount();
                            if (outputCount > 0)
                            {
                                var outputs = adapter.Outputs;
                                for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                                {
                                    var output = outputs[outputIndex];

                                    var descr = output.Description;
                                    var desktopBounds = descr.DesktopBounds;
                                    var desktopRect = new GDI.Rectangle
                                    {
                                        X = desktopBounds.Left,
                                        Y = desktopBounds.Top,
                                        Width = desktopBounds.Right - desktopBounds.Left,
                                        Height = desktopBounds.Bottom - desktopBounds.Top,
                                    };

                                    var rect = GDI.Rectangle.Intersect(desktopRect, SrcRect);
                                    if (rect.Width > 0 && rect.Height > 0)
                                    {
                                        logger.Info("Screen source info: " + adapter.Description.Description + " " + descr.DeviceName);

                                        Device device = null;
                                        if (adapterToDeviceMap.ContainsKey(adapterIndex))
                                        {
                                            device = adapterToDeviceMap[adapterIndex];
                                        }
                                        else
                                        {
                                            device = new Device(adapter, deviceCreationFlags, featureLevel);
                                            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                                            {
                                                multiThread.SetMultithreadProtected(true);
                                            }

                                            adapterToDeviceMap[adapterIndex] = device;
                                        }

                                        Device destDevice = null;
                                        if (PrimaryAdapterIndex != adapterIndex)
                                        {
                                            destDevice = mainDevice;
                                        }

                                        DDAOutput duplOutput = OutputManager.GetOutput(adapterIndex, outputIndex);
                                        duplOutput.CaptureMouse = this.CaptureMouse;

                                        DDAOutputProvider prov = new DDAOutputProvider(duplOutput);
                                        prov.Init(output, device, SrcRect, destDevice);
                                        providers.Add(prov);

                                    }
                                    else
                                    {
                                        logger.Debug("No common area: " + descr.DeviceName + " " + SrcRect.ToString());
                                        //continue;
                                    }
                                }

                                for (int i = 0; i < outputs.Length; i++)
                                {
                                    var o = outputs[i];
                                    o.Dispose();
                                    o = null;
                                }
                            }
                        }
                    }


                    for (int i = 0; i < adapters.Length; i++)
                    {
                        var a = adapters[i];
                        a.Dispose();
                        a = null;
                    }

                }

            }
            finally
            {
                if (primaryAdapter != null)
                {
                    primaryAdapter.Dispose();
                    primaryAdapter = null;
                }

                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }

            sharedTexture = new Texture2D(mainDevice,
                new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = SrcRect.Width,
                    Height = SrcRect.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                    OptionFlags = ResourceOptionFlags.None,

                });

            compositionTexture = new Texture2D(mainDevice,
               new Texture2DDescription
               {
                   CpuAccessFlags = CpuAccessFlags.None,
                   BindFlags = BindFlags.ShaderResource,
                   Format = Format.B8G8R8A8_UNorm,

                   Width = SrcRect.Width,
                   Height = SrcRect.Height,
                   MipLevels = 1,
                   ArraySize = 1,
                   SampleDescription = { Count = 1, Quality = 0 },
                   Usage = ResourceUsage.Default,
               });

            initialized = true;

        }

        private volatile bool activateCapture = false;

        private bool initialized = false;

        public override ErrorCode UpdateBuffer(int timeout = 10)
        {
            ErrorCode Result = ErrorCode.Ok;

            try
            {
                if (!initialized)
                {
                    InitDx();
                }

                if (!activateCapture)
                {
                    foreach (var d in providers)
                    {
                        int activationNum = d.ActivateCapture();
                        logger.Debug("ActivateCapture: " + activationNum);
                    }

                    activateCapture = true;
                }


                foreach (var dupl in providers)
                {
                    try
                    {
                        Result = dupl.TryGetScreenTexture(out Rectangle destRect, out Texture2D texture);
                        if (Result != ErrorCode.Ok)
                        {
                            //...
                            logger.Warn("TryGetScreenTexture(...) " + Result);
                            continue;
                        }


                        var desrc = texture.Description;
                        var srcRegion = new ResourceRegion
                        {
                            Left = 0,
                            Top = 0,
                            Right = desrc.Width,
                            Bottom = desrc.Height,
                            Back = 1,
                        };

                        mainDevice.ImmediateContext.CopySubresourceRegion(texture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);
                        mainDevice.ImmediateContext.Flush();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }

                }
                //------------------------------------------------------

                if (Result == ErrorCode.Ok)
                {
                    mainDevice.ImmediateContext.CopyResource(compositionTexture, sharedTexture);
                    mainDevice.ImmediateContext.Flush();

                    var destFrame = VideoBuffer.GetFrame();
                    processor.Process(srcFrame, destFrame);
                    Result = ErrorCode.Ok;
                }



            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);
                // Process error...

                CloseDx();

                //throw;
                Thread.Sleep(100);

            }

            return Result;
        }

        public override void Close()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Close()");
            base.Close();

            foreach (var d in providers)
            {
                int activationNum = d.DeactivateCapture();
                logger.Debug("DeactivateCapture: " + activationNum);
            }
            activateCapture = false;

            if (internalOutputManager)
            {
                if (OutputManager != null)
                {
                    OutputManager.Dispose();
                    OutputManager = null;
                }
                internalOutputManager = false;
            }

            CloseDx();


            //if (OutputManager != null)
            //{
            //    OutputManager.Dispose();
            //    OutputManager = null;
            //}
        }

        private void CloseDx()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::CloseDx()");

            initialized = false;

            if (providers != null)
            {
                foreach (var dupl in providers)
                {
                    dupl?.Close();
                }
                providers = null;
            }

            if (compositionTexture != null && !compositionTexture.IsDisposed)
            {
                compositionTexture.Dispose();
                compositionTexture = null;
            }

            if (sharedTexture != null && !sharedTexture.IsDisposed)
            {
                sharedTexture.Dispose();
                sharedTexture = null;
            }

            if (mainDevice != null && !mainDevice.IsDisposed)
            {
                mainDevice.Dispose();
                mainDevice = null;
            }


            for (int i = 0; i < adapterToDeviceMap.Count; i++)
            {
                var d = adapterToDeviceMap[i];
                if (d != null && !d.IsDisposed)
                {
                    d.Dispose();
                    d = null;
                }
            }

            if (srcFrame != null)
            {
                srcFrame.Dispose();
                srcFrame = null;
            }

            if (processor != null)
            {
                processor.Close();
                processor = null;
            }

        }


    }

}
