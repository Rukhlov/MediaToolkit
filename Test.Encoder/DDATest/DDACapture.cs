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
//using MediaToolkit.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using NLog;
using SharpDX.Direct3D;
//using MediaToolkit.Logging;
//using MediaToolkit.SharedTypes;

namespace Test.Encoder.DDATest
{
    public interface ITexture2DSource
    {
        Texture2D SharedTexture { get; }
        long AdapterId { get; }
        bool UseHwContext { get; set; }
    }

    public class DDACapture : ITexture2DSource
    {

        //private static TraceSource logger = TraceManager.GetTrace("DXGIDesktopDuplicationCapture");

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DDACapture(object[] args)
        { }

        private Device device = null;

        private Texture2D compositionTexture = null;

        private Texture2D renderTexture = null;
        Direct2D.RenderTarget renderTarget = null;

        public Texture2D SharedTexture { get; private set; }
        public long AdapterId { get; private set; }

        public bool UseHwContext { get; set; } = true;

        // private List<_DesktopDuplicator> deskDupls = new List<_DesktopDuplicator>();

        private List<DesktopDuplicator> deskDupls = new List<DesktopDuplicator>();

        public GDI.Rectangle SrcRect { get; private set; }
        public GDI.Size DestSize { get; private set; }
        public bool CaptureMouse { get; set; }
        public bool AspectRatio { get; set; }

        private GDI.Rectangle normalizedSrcRect = GDI.Rectangle.Empty;
        public void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Init() " + srcRect.ToString() + " " + destSize.ToString());

            //base.Init(srcRect, destSize);

            this.SrcRect = srcRect;

            if (destSize.IsEmpty)
            {
                destSize.Width = srcRect.Width;
                destSize.Height = srcRect.Height;
            }

            this.DestSize = new GDI.Size(destSize.Width, destSize.Height);

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
        public int adapterIndex = 0;
        private void InitDx()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::InitDx(...) " + SrcRect.ToString());


            SharpDX.DXGI.Factory1 dxgiFactory = null;
            Adapter1 adapter = null;
            Output output = null;
            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();

                logger.Info(Utils.LogDxAdapters(dxgiFactory.Adapters1));

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

                if (adapter == null)
                {// первым идет адаптер с которому подключен primary монитор
                    adapter = dxgiFactory.GetAdapter1(adapterIndex);
                }

                AdapterId = adapter.Description.Luid;

                //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);



                FeatureLevel[] featureLevel =
                {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0,
                    FeatureLevel.Level_10_1,
                };


                var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
                //DeviceCreationFlags.Debug |
                // DeviceCreationFlags.VideoSupport |
                //  DeviceCreationFlags.BgraSupport;

                device = new SharpDX.Direct3D11.Device(DriverType.Hardware, deviceCreationFlags, featureLevel);

                //device = new Device(adapter, deviceCreationFlags, featureLevel);

                using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }

                if (deskDupls != null)
                {
                    //...
                }

                //deskDupls = new List<_DesktopDuplicator>();
                deskDupls = new List<DesktopDuplicator>();
                foreach (var _output in adapter.Outputs)
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
                        logger.Info("Screen source info: " + adapter.Description.Description + " " + descr.DeviceName);

                        //_DesktopDuplicator deskDupl = new _DesktopDuplicator(device);

                        //deskDupl.Init(_output, SrcRect);
                        //deskDupl.CaptureMouse = this.CaptureMouse;

                        //deskDupls.Add(deskDupl);


                        DesktopDuplicator deskDupl = new DesktopDuplicator(device);

                        deskDupl.Init(desktopRect, SrcRect);
                        //deskDupl.StartCapture();

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

            }
            finally
            {
                if (adapter != null)
                {
                    adapter.Dispose();
                    adapter = null;
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


            SharedTexture = new Texture2D(device,
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

            compositionTexture = new Texture2D(device,
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

            renderTexture = new Texture2D(device,
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
                    OptionFlags = ResourceOptionFlags.None,

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


        public void Start()
        {

            foreach (var d in deskDupls)
            {
                d.StartCapture();
            }

            // return;

            Task.Run(() =>
            {
                while (true)
                {
                    var res = UpdateBuffer(10);

                    if (res != ErrorCode.Ok)
                    {
                        logger.Warn("UpdateBuffer(...) " + res);
                    }

                    //var tex = videoSource.GetTexture();
                    //videoRenderer.UpdataBuffer(tex);


                    Thread.Sleep(10);

                }
            });
        }

        private bool deviceReady = false;


        public ErrorCode UpdateBuffer(int timeout = 10)
        {
            ErrorCode Result = ErrorCode.Ok;

            try
            {
                if (!deviceReady)
                {
                    InitDx();
                }

                foreach (var dupl in deskDupls)
                {
                    try
                    {
                        //if (!dupl.Initialized)
                        //{
                        //    dupl.InitDuplicator();

                        //}

                        //ErrorCode errorCode = dupl.TryGetScreenTexture(out Texture2D screenTexture, 0);
                        //if (errorCode != ErrorCode.Ok)
                        //{
                        //    logger.Warn("Device lost, close DX " + errorCode);

                        //    dupl.CloseDuplicator();

                        //    //CloseDx();

                        //    Thread.Sleep(100);
                        //    return errorCode;

                        //}


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
                        var sharedTexture = dupl.SharedTexture;

                        device.ImmediateContext.CopySubresourceRegion(sharedTexture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);

                        //using (var sharedTexture = device.OpenSharedResource<Texture2D>(dupl.GetSharedHandle()))
                        //{
                        //    device.ImmediateContext.CopySubresourceRegion(sharedTexture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);
                        //    //device.ImmediateContext.Flush();
                        //}

                        //using (var sharedRes = screenTexture.QueryInterface<SharpDX.DXGI.Resource>())
                        //{
                        //   var handle = sharedRes.SharedHandle;
                        //    using (var sharedTexture = device.OpenSharedResource<Texture2D>(handle))
                        //    {
                        //        device.ImmediateContext.CopySubresourceRegion(sharedTexture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);
                        //        device.ImmediateContext.Flush();
                        //    }
                        //}



                        //device.ImmediateContext.CopySubresourceRegion(screenTexture, 0, srcRegion, compositionTexture, 0, destRect.Left, destRect.Top);

                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }


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

                //SharedTexture = finalTexture;


                Result = FinalyzeTexture(finalTexture);

                Result = ErrorCode.Ok;

            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);
                // Process error...

                //CloseDx();

                //throw;
                Thread.Sleep(100);

            }

            return Result;
        }

        private ErrorCode FinalyzeTexture(Texture2D texture)
        {
            ErrorCode errorCode = ErrorCode.Unexpected;
            var deviceContext = device.ImmediateContext;

            if (UseHwContext)
            {

                deviceContext.CopyResource(texture, SharedTexture);
                // deviceContext.Flush();

                errorCode = ErrorCode.Ok;
            }
            else
            {
                Texture2D stagingTexture = null;
                try
                {
                    // Create Staging texture CPU-accessible
                    stagingTexture = new Texture2D(device,
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

                    //errorCode = CopyToGdiBuffer(stagingTexture);

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

        //private ErrorCode CopyToGdiBuffer(Texture2D texture)
        //{

        //    ErrorCode Result = ErrorCode.Unexpected;

        //    var syncRoot = videoBuffer.syncRoot;
        //    bool lockTaken = false;
        //    try
        //    {
        //        Monitor.TryEnter(syncRoot, /*timeout*/1000, ref lockTaken);
        //        if (lockTaken)
        //        {
        //            var bmp = videoBuffer.bitmap;
        //            MediaFoundation.DxTool.TextureToBitmap(texture, ref bmp);
        //            Result = ErrorCode.Ok;
        //        }
        //        else
        //        {
        //            logger.Debug("lockTaken == false");
        //        }

        //    }
        //    finally
        //    {
        //        if (lockTaken)
        //        {
        //            Monitor.Exit(syncRoot);
        //        }
        //    }



        //    return Result;
        //}


        public void Close()
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Close()");
            //base.Close();

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

            if (device != null && !device.IsDisposed)
            {
                device.Dispose();
                device = null;
            }

        }


    }


    enum ShapeType
    {
        /// <summary>
        /// The pointer type is a monochrome mouse pointer, which is a monochrome bitmap. 
        /// The bitmap's size is specified by width and height in a 1 bits per pixel (bpp) device independent bitmap (DIB) 
        /// format AND mask that is followed by another 1 bpp DIB format XOR mask of the same size.
        /// </summary>
        DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME = 0x1,

        /// <summary>
        /// The pointer type is a color mouse pointer, which is a color bitmap. 
        /// The bitmap's size is specified by width and height in a 32 bpp ARGB DIB format.
        /// </summary>
        DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR = 0x2,

        /// <summary>
        /// The pointer type is a masked color mouse pointer.
        /// A masked color mouse pointer is a 32 bpp ARGB format bitmap with the mask value in the alpha bits. 
        /// The only allowed mask values are 0 and 0xFF. When the mask value is 0, the RGB value should replace the screen pixel. 
        /// When the mask value is 0xFF, an XOR operation is performed on the RGB value and the screen pixel; the result replaces the screen pixel.
        /// </summary>
        DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR = 0x4

    }


}
