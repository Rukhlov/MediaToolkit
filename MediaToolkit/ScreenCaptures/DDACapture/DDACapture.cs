
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

namespace MediaToolkit.ScreenCaptures
{
	public interface ITexture2DSource
	{
		Texture2D SharedTexture { get; }
		long AdapterId { get; }
        int AdapterIndex { get; }
        bool UseHwContext { get; set; }
	}

	public class DDACapture : ScreenCapture, ITexture2DSource
    {
        public DDACapture(object[] args) : base()
        { }

        private Device mainDevice = null;

        private Texture2D compositionTexture = null;

        private Texture2D renderTexture = null;
        Direct2D.RenderTarget renderTarget = null;

        public Texture2D SharedTexture { get; private set; }
        public long AdapterId { get; private set; }
        public int AdapterIndex { get; private set; } = 0;
        public bool UseHwContext { get; set; } = true;

        private List<DesktopDuplicator> deskDupls = new List<DesktopDuplicator>();

        //private static DesktopDuplicationManager duplicationManager = null;

        //private static DesktopDuplicationManager DuplicationManager
        //{
        //    get
        //    {
        //        if(duplicationManager == null)
        //        {
        //            duplicationManager = new DesktopDuplicationManager();
        //            duplicationManager.Init();
        //        }

        //        return duplicationManager;

        //    }
        //}

        private GDI.Rectangle normalizedSrcRect = GDI.Rectangle.Empty;
        public override void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Init() " + srcRect.ToString() + " " + destSize.ToString());

            base.Init(srcRect, destSize);

            ////---------------------------------------------
            //var AllScreensRect = System.Windows.Forms.SystemInformation.VirtualScreen;
            //DuplicationManager.ToString();


            //var rawRect = new RawRectangle
            //{
            //    Left = AllScreensRect.Left,
            //    Right = AllScreensRect.Right,
            //    Top = AllScreensRect.Top,
            //    Bottom = AllScreensRect.Bottom,
            //};

            //normalizedSrcRect = SetupRegions(rawRect, srcRect);
            ////---------------------------------------------

            try
            {
				InitDx();
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


        private Dictionary<int, Device> adapterToDeviceMap = new Dictionary<int, Device>();
        private void InitDx()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::InitDx(...) " + SrcRect.ToString());


            SharpDX.DXGI.Factory1 dxgiFactory = null;
            Adapter1 mainAdapter = null;
            Output output = null;
            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();
                
                logger.Info(MediaFoundation.DxTool.LogDxAdapters(dxgiFactory.Adapters1));

                //var hMonitor = NativeAPIs.User32.GetMonitorFromRect(this.srcRect);
                //if (hMonitor != IntPtr.Zero)
                //{
                //    foreach (var _adapter in dxgiFactory.Adapters1)
                //    {
                //        foreach (var _output in _adapter.Outputs)
                //        {
                //            var descr = _output.Description;
                //            if (descr.MonitorHandle == hMonitor)
                //            {
                //                adapter = _adapter;
                //                output = _output;

                //                break;
                //            }
                //        }
                //    }
                //}

                //if (adapter == null)
                //{// первым идет адаптер с которому подключен primary монитор
                //    adapter = dxgiFactory.GetAdapter1(0);
                //}

                AdapterIndex = 0;
                // первым идет адаптер с которому подключен primary монитор
                mainAdapter = dxgiFactory.GetAdapter1(AdapterIndex);
                AdapterId = mainAdapter.Description.Luid;

                //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                var deviceCreationFlags =
                    //DeviceCreationFlags.Debug |
                    DeviceCreationFlags.VideoSupport |
                    DeviceCreationFlags.BgraSupport;

                SharpDX.Direct3D.FeatureLevel[] featureLevel =
                {
                    SharpDX.Direct3D.FeatureLevel.Level_11_1,
                    SharpDX.Direct3D.FeatureLevel.Level_11_0,
                    //SharpDX.Direct3D.FeatureLevel.Level_10_1,
                };

                mainDevice = new Device(mainAdapter, deviceCreationFlags, featureLevel);
                using (var multiThread = mainDevice.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }

                adapterToDeviceMap[0] = mainDevice;

                if (deskDupls != null)
                {
                    //...
                }

                deskDupls = new List<DesktopDuplicator>();

                /*
                foreach (var _output in mainAdapter.Outputs)
                {
                    var descr = _output.Description;
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
                        //--------------------------------------
                        logger.Info("Screen source info: " + mainAdapter.Description.Description + " " + descr.DeviceName);
                        DesktopDuplicator deskDupl = new DesktopDuplicator();

                        deskDupl.Init(_output, device, SrcRect);
                        deskDupl.CaptureMouse = this.CaptureMouse;

                        deskDupls.Add(deskDupl);
                        //-------------------------------------------
                    }
                    else
                    {
                        logger.Debug("No common area: " + descr.DeviceName + " " + SrcRect.ToString());
                        continue;
                    }

                    _output.Dispose();
                }
                */

                var adaptersCount = dxgiFactory.GetAdapterCount1();
                //if (adaptersCount > 2)
                {// обычно 2 адаптера hardware(GPU) + software(Microsoft Basic Render Driver)
                    var adapters = dxgiFactory.Adapters1;
                    for (int adapterIndex= 0; adapterIndex < adapters.Length; adapterIndex++)
                    {
                        var _adapter = adapters[adapterIndex];
                        var adapterDescr = _adapter.Description1;
                        var flags = adapterDescr.Flags;
                        if (flags == AdapterFlags.None)
                        {
                            var outputCount = _adapter.GetOutputCount();
                            if (outputCount > 0)
                            {
                                var _outputs = _adapter.Outputs;
                                foreach (var _output in _outputs)
                                {
                                    var descr = _output.Description;
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
                                        logger.Info("Screen source info: " + _adapter.Description.Description + " " + descr.DeviceName);

                                        Device device = null;
                                        if (adapterToDeviceMap.ContainsKey(adapterIndex))
                                        {
                                            device = adapterToDeviceMap[adapterIndex];
                                        }
                                        else
                                        {
                                            device = new Device(_adapter, deviceCreationFlags, featureLevel);
                                            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                                            {
                                                multiThread.SetMultithreadProtected(true);
                                            }
                                        }

                                        Device destDevice = null;
                                        if (AdapterIndex != adapterIndex)
                                        {
                                            destDevice = mainDevice;
                                        }

                                        DesktopDuplicator deskDupl = new DesktopDuplicator();
                                        deskDupl.Init(_output, device, SrcRect, destDevice);
                                        deskDupl.CaptureMouse = this.CaptureMouse;

                                        deskDupls.Add(deskDupl);

                                    }
                                    else
                                    {
                                        logger.Debug("No common area: " + descr.DeviceName + " " + SrcRect.ToString());
                                        //continue;
                                    }
                                }

                                for (int i = 0; i < _outputs.Length; i++)
                                {
                                    var o = _outputs[i];
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
                if (mainAdapter != null)
                {
                    mainAdapter.Dispose();
                    mainAdapter = null;
                }

                if (output != null)
                {
                    output.Dispose();
                    output = null;
                }

                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }


            SharedTexture = new Texture2D(mainDevice,
                 new Texture2DDescription
                 {
                     CpuAccessFlags = CpuAccessFlags.None,
                     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                     Format = Format.B8G8R8A8_UNorm,
                     Width = DestSize.Width,
                     Height = DestSize.Height,

                     MipLevels = 1,
                     ArraySize = 1,
                     SampleDescription = { Count = 1, Quality = 0 },
                     Usage = ResourceUsage.Default,
                     //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                     OptionFlags = ResourceOptionFlags.Shared,

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

            renderTexture = new Texture2D(mainDevice,
                new Texture2DDescription
                {

                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,

                    Width = DestSize.Width,
                    Height = DestSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                    //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                    OptionFlags = ResourceOptionFlags.Shared,

                });

            if (DestSize.Width != SrcRect.Width || DestSize.Height != SrcRect.Height)
            {
                using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded))
                {
                    using (var surf = renderTexture.QueryInterface<Surface>())
                    {
                        //var pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore);

                        var pixelFormat = new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied);
                        var renderTargetProps = new Direct2D.RenderTargetProperties(pixelFormat);

                        renderTarget = new Direct2D.RenderTarget(factory2D1, surf, renderTargetProps);

                        //var d2dContext = new SharpDX.Direct2D1.DeviceContext(surface);
                    }

                }
            }

            deviceReady = true;

        }

        private bool deviceReady = false;
        public override ErrorCode UpdateBuffer(int timeout = 10)
        {
            ErrorCode Result = ErrorCode.Ok;

            try
            {
                if (!deviceReady)
                {
                    InitDx();
                }

                //DuplicationManager.UpdateBuffer();

                //ResourceRegion srcRegion = new ResourceRegion
                //{
                //    Left = normalizedSrcRect.Left,
                //    Top = normalizedSrcRect.Top,
                //    Right = normalizedSrcRect.Right,
                //    Bottom = normalizedSrcRect.Bottom,
                //    Back = 1,
                //};

                //var texture = DuplicationManager.compositionTexture;
                //device.ImmediateContext.CopySubresourceRegion(texture, 0, srcRegion, compositionTexture, 0, 0, 0);

                //------------------------------------------
                foreach (var dupl in deskDupls)
                {
                    var errorCode = dupl.TryGetScreenTexture(out Texture2D screenTexture);

                    if (errorCode != ErrorCode.Ok)
                    {
                        CloseDx();

                        Thread.Sleep(100);
                        return errorCode;

                    }
                    var duplRect = dupl.duplRect;

                    ResourceRegion srcRegion = new ResourceRegion
                    {
                        Left = duplRect.Left,
                        Top = duplRect.Top,
                        Right = duplRect.Right,
                        Bottom = duplRect.Bottom,
                        Back = 1,
                    };

                    var destRect = dupl.drawRect;

                    mainDevice.ImmediateContext.CopySubresourceRegion(screenTexture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);

                }
                //------------------------------------------------------


                Texture2D finalTexture = compositionTexture;
                if (renderTarget != null)
                {// масштабируем текстуру если нужно
                    renderTarget.BeginDraw();
                    renderTarget.Clear(Color.Black);//(Color.Red);

                    DrawScreen(renderTarget, compositionTexture);

                    renderTarget.EndDraw();
                    finalTexture = renderTexture;
                }

                Result = FinalyzeTexture(finalTexture);

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

        private ErrorCode FinalyzeTexture(Texture2D texture)
        {
            ErrorCode errorCode = ErrorCode.Unexpected;
            var deviceContext = mainDevice.ImmediateContext;

            if (UseHwContext)
            {

                deviceContext.CopyResource(texture, SharedTexture);
                deviceContext.Flush();

                errorCode = ErrorCode.Ok;
            }
            else
            {
                Texture2D stagingTexture = null;
                try
                {
                    // Create Staging texture CPU-accessible
                    stagingTexture = new Texture2D(mainDevice,
                        new Texture2DDescription
                        {
                            CpuAccessFlags = CpuAccessFlags.Read,
                            BindFlags = BindFlags.None,
                            Format = Format.B8G8R8A8_UNorm,
                            Width = DestSize.Width,
                            Height = DestSize.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDescription = { Count = 1, Quality = 0 },
                            Usage = ResourceUsage.Staging,
                            OptionFlags = ResourceOptionFlags.None,
                        });

                    deviceContext.CopyResource(texture, stagingTexture);
                    deviceContext.Flush();

                    errorCode = CopyToGdiBuffer(stagingTexture);

                }
                finally
                {
                    stagingTexture?.Dispose();
                }
            }

            return errorCode;
        }

        private void DrawScreen(SharpDX.Direct2D1.RenderTarget renderTarget, Texture2D texture)
        {
            using (var surf = texture.QueryInterface<Surface1>())
            {
                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                Direct2D.Bitmap screenBits = new Direct2D.Bitmap(renderTarget, surf, prop);
                try
                {
                    var srcDecr = surf.Description;
                    float srcWidth = srcDecr.Width;
                    float srcHeight = srcDecr.Height;

                    float destX = 0;
                    float destY = 0;
                    float destWidth = DestSize.Width;
                    float destHeight = DestSize.Height;

                    float scaleX = destWidth / srcWidth;
                    float scaleY = destHeight / srcHeight;

                    if (AspectRatio)
                    {
                        if (scaleY < scaleX)
                        {
                            scaleX = scaleY;
                            destX = ((destWidth - srcWidth * scaleX) / 2);
                        }
                        else
                        {
                            scaleY = scaleX;
                            destY = ((destHeight - srcHeight * scaleY) / 2);
                        }
                    }

                    destWidth = srcWidth * scaleX;
                    destHeight = srcHeight * scaleY;

                    var destRect = new RawRectangleF
                    {
                        Left = destX,
                        Right = destX + destWidth,
                        Top = destY,
                        Bottom = destY + destHeight,
                    };

                    renderTarget.DrawBitmap(screenBits, destRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);

                }
                finally
                {
                    screenBits?.Dispose();
                }
            }
        }



        private ErrorCode CopyToGdiBuffer(Texture2D texture)
        {

            ErrorCode Result = ErrorCode.Unexpected;

            var syncRoot = videoBuffer.syncRoot;
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(syncRoot, /*timeout*/1000, ref lockTaken);
                if (lockTaken)
                {
					var bmp = videoBuffer.bitmap;
                    MediaFoundation.DxTool.TextureToBitmap(texture, ref bmp);
					Result = ErrorCode.Ok;
                }
                else
                {
                    logger.Debug("lockTaken == false");
                }

            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(syncRoot);
                }
            }



            return Result;
        }


        public override void Close()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Close()");
            base.Close();

            CloseDx();
        }

        private void CloseDx()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::CloseDx()");

            deviceReady = false;

            if (deskDupls != null)
            {
                foreach (var dupl in deskDupls)
                {
                    dupl?.Close();
                }
                deskDupls = null;
            }

            if (compositionTexture != null && !compositionTexture.IsDisposed)
            {
                compositionTexture.Dispose();
                compositionTexture = null;
            }

            if (renderTexture != null && !renderTexture.IsDisposed)
            {
                renderTexture.Dispose();
                renderTexture = null;
            }

            if (renderTarget != null && !renderTarget.IsDisposed)
            {
                renderTarget.Dispose();
                renderTarget = null;
            }

            if (SharedTexture != null && !SharedTexture.IsDisposed)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (mainDevice != null && !mainDevice.IsDisposed)
            {
                mainDevice.Dispose();
                mainDevice = null;
            }

            for(int i= 0; i<adapterToDeviceMap.Count; i++)
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
