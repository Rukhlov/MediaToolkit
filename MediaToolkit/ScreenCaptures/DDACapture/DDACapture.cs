
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

        public override void Init(ScreenCaptureParameters captParams)
        {
            try
            {
                base.Init(captParams);

                var device = captParams.D3D11Device;
                if(device != null)
                {
                    mainDevice = new Device(device.NativePointer);
                    ((IUnknown)mainDevice).AddReference();
                }
                

                this.CaptureMouse = captParams.CaptureMouse;
                this.OutputManager = captParams.DDAOutputMan;

                if (OutputManager == null)
                {
                    OutputManager = new DDAOutputManager();
                    internalOutputManager = true;
                }

                InitDx();

            }
            catch (SharpDXException ex)
            {
                // Process error...
                logger.Error(ex);

                throw new Exception("DXGI initialization error [" + ex.ResultCode + "]");
            }

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

                var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
                //deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
                SharpDX.Direct3D.FeatureLevel[] featureLevel =
                {
                        SharpDX.Direct3D.FeatureLevel.Level_11_1,
                        SharpDX.Direct3D.FeatureLevel.Level_11_0,
                        SharpDX.Direct3D.FeatureLevel.Level_10_1,
                };

                if (mainDevice == null)
                {

                    mainDevice = new Device(primaryAdapter, deviceCreationFlags, featureLevel);
                    using (var multiThread = mainDevice.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
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

                                    var outputDescr = output.Description;
                                    var desktopBounds = outputDescr.DesktopBounds;
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
                                        logger.Info("Screen source info: " + adapter.Description.Description + " " + outputDescr.DeviceName);

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
                                        logger.Debug("No common area: " + outputDescr.DeviceName + " " + SrcRect.ToString());
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


        public override ErrorCode TryGetFrame(out IVideoFrame frame, int timeout = 10)
        {
            frame = null;
            ErrorCode Result = ErrorCode.Unexpected;

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

                    frame = new D3D11VideoFrame(sharedTexture);
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

                if (activationNum > 0)
                {

                }
                else
                {
                    OutputManager.ReleaseOutput(d.duplOutput);
                }
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
            else
            {
                if (OutputManager != null)
                {

                }
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
        }

    }

}
