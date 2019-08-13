
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

namespace ScreenStreamer
{
    public class DXGIDesktopDuplicationCapture : ScreenCapture
    {
        public DXGIDesktopDuplicationCapture(object[] args) : base()
        { }

        public Device Device3d11 { get; private set; }
        public Texture2D SharedTexture { get; private set; }

        //private SharpDX.Direct2D1.Factory1 factory2D1 = null;
        //private SharpDX.DXGI.Factory1 dxgiFactory = null;
        

        //private Adapter1 adapter = null;
        //private Output output = null;

        private OutputDuplication deskDupl = null;
        private Texture2D acquiredDesktopImage = null;
        private Texture2D desktopTexture = null;

        private Texture2D stagingTexture = null;

        SharpDX.Direct2D1.RenderTarget desktopTarget = null;

        public override void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Init()");

            base.Init(srcRect, destSize);

            InitDx(srcRect);

        }
        //private MfEncoderAsync encoder = null;
        //private MfWriter writer = null;

        private void InitDx(GDI.Rectangle srcRect)
        {

            SharpDX.DXGI.Factory1 dxgiFactory = null;
            Adapter1 adapter = null;
            Output output = null;
            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();
                //adapter = dxgiFactory.Adapters1.FirstOrDefault();
                adapter = dxgiFactory.Adapters1[0];

                //Device3d11 = new Device(adapter, DeviceCreationFlags.BgraSupport);

                var deviceCreationFlags = DeviceCreationFlags.Debug |
                    DeviceCreationFlags.VideoSupport |
                    DeviceCreationFlags.BgraSupport;

                Device3d11 = new Device(adapter, deviceCreationFlags);

                //using (var multiThread = Device3d11.QueryInterface<SharpDX.Direct3D11.Multithread>())
                //{
                //    multiThread.SetMultithreadProtected(true);
                //}


                // Get DXGI.Output
                output = adapter.Outputs.FirstOrDefault();

                var hMonitor = User32.GetMonitorFromRect(srcRect);
                if (hMonitor != IntPtr.Zero)
                {
                    output = adapter.Outputs.FirstOrDefault(o => o.Description.MonitorHandle == hMonitor);
                }

                if (output == null)
                {
                    output = adapter.Outputs.FirstOrDefault();
                }

                using (var output1 = output.QueryInterface<Output1>())
                {
                    // Duplicate the output
                    deskDupl = output1.DuplicateOutput(Device3d11);
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

            //// Create Staging texture CPU-accessible
            //stagingTexture = new Texture2D(Device3d11,
            //    new Texture2DDescription
            //    {
            //        CpuAccessFlags = CpuAccessFlags.Read,
            //        BindFlags = BindFlags.None,
            //        Format = Format.B8G8R8A8_UNorm,
            //        Width = videoBuffer.bitmap.Width,
            //        Height = videoBuffer.bitmap.Height,
            //        MipLevels = 1,
            //        ArraySize = 1,
            //        SampleDescription = { Count = 1, Quality = 0 },
            //        Usage = ResourceUsage.Staging,
            //        OptionFlags = ResourceOptionFlags.None,
            //    });

            SharedTexture = new Texture2D(Device3d11,
                 new Texture2DDescription
                 {

                     CpuAccessFlags = CpuAccessFlags.None,
                     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                     Format = Format.B8G8R8A8_UNorm,
                     Width = srcRect.Width,
                     Height = srcRect.Height,
                     MipLevels = 1,
                     ArraySize = 1,
                     SampleDescription = { Count = 1, Quality = 0 },
                     Usage = ResourceUsage.Default,
                     //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                     OptionFlags = ResourceOptionFlags.Shared,

                 });

            //var bmp = GDI.Bitmap.FromFile(@"d:\4.bmp");
            //var texture = GetTexture((GDI.Bitmap)bmp, Device3d11);
            //bmp.Dispose();
            //Device3d11.ImmediateContext.CopyResource(texture, SharedTexture);
            //Device3d11.ImmediateContext.Flush();

            desktopTexture = new Texture2D(Device3d11,
                new Texture2DDescription
                {
                    
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = srcRect.Width,
                    Height = srcRect.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                    //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                    OptionFlags = ResourceOptionFlags.Shared,
                    
                });



            using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded))
            {
                using (var desktopSurf = desktopTexture.QueryInterface<Surface>())
                {
                    //var pixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Ignore);

                    var pixelFormat = new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied);
                    var renderTargetProps = new Direct2D.RenderTargetProperties(pixelFormat);

                    desktopTarget = new Direct2D.RenderTarget(factory2D1, desktopSurf, renderTargetProps);

                    //var d2dContext = new SharpDX.Direct2D1.DeviceContext(surface);
                }
            }


            cursorObj = new CursorObj();

        
            deviceReady = true;

        }

        private bool deviceReady = false;

        public override bool UpdateBuffer(int timeout = 10)
        {
            bool Result = false;

            try
            {
                if (!deviceReady)
                {
                    InitDx(srcRect);
                }

                OutputDuplicateFrameInformation frameInfo;
                SharpDX.DXGI.Resource desktopResource = null;
                try
                {
                    var acquireResult = deskDupl.TryAcquireNextFrame(50, out frameInfo, out desktopResource);

                    if (acquireResult.Success)
                    {

                        if (acquiredDesktopImage != null)
                        {
                            acquiredDesktopImage.Dispose();
                            acquiredDesktopImage = null;
                        }

                        acquiredDesktopImage = desktopResource.QueryInterface<Texture2D>();

                        #region Update regions info

                        //var frameParams = GetFrameParams(frameInfo);
                        /*
                            var moveRects = frameParams.MoveRects;
                            foreach (OutputDuplicateMoveRectangle moveRect in moveRects)
                            {
                                //...
                                var srcPoint = moveRect.SourcePoint;
                                GDI.Point point = new GDI.Point(srcPoint.X, srcPoint.Y);

                                var destRect = moveRect.DestinationRect;
                                int x = destRect.Left;
                                int y = destRect.Top;
                                int width = destRect.Right - destRect.Left;
                                int height = destRect.Bottom - destRect.Top;
                                GDI.Rectangle rect = new GDI.Rectangle(x, y, width, height);

                                logger.Debug("srcPoint " + point.ToString() + " destRect " + rect.ToString());
                            }
                            var dirtyRects = frameParams.DirtyRects;
                            foreach (RawRectangle dirtyRect in dirtyRects)
                            {
                                int x = dirtyRect.Left;
                                int y = dirtyRect.Top;
                                int width = dirtyRect.Right - dirtyRect.Left;
                                int height = dirtyRect.Bottom - dirtyRect.Top;
                                GDI.Rectangle rect = new GDI.Rectangle(x, y, width, height);
                            }
                        */

                        //var _rect = new GDI.Rectangle(0, 0, videoBuffer.bitmap.Width, videoBuffer.bitmap.Height);

                        #endregion

                        UpdateMouseInfo(frameInfo);

                        using (var acquiredSurf = acquiredDesktopImage.QueryInterface<Surface1>())
                        {

                            var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                            Direct2D.Bitmap acquiredBits = new Direct2D.Bitmap(desktopTarget, acquiredSurf, prop);
                            try
                            {

                                desktopTarget.BeginDraw();

                                var scrDescr = acquiredDesktopImage.Description;
                                var scaleX = videoBuffer.bitmap.Width / (float)scrDescr.Width;
                                var scaleY = videoBuffer.bitmap.Height / (float)scrDescr.Height;
                                desktopTarget.Transform = Matrix3x2.Scaling(scaleX, scaleY);

                                var renderSize = desktopTarget.Size;
                                var srcRect = new RawRectangleF(0, 0, renderSize.Width, renderSize.Height);
                                desktopTarget.DrawBitmap(acquiredBits, srcRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);

                                if (this.CaptureMouse && cursorObj.Visible)
                                {
                                    DrawCursor();
                                }

                                desktopTarget.EndDraw();
                            }
                            finally
                            {
                                acquiredBits?.Dispose();
                            }
                        }

                        ResourceRegion scaledRegion = new ResourceRegion
                        {
                            Left = 0,
                            Top = 0,
                            Right = videoBuffer.bitmap.Width,
                            Bottom = videoBuffer.bitmap.Height,
                            Back = 1,
                        };

                        Device3d11.ImmediateContext.CopySubresourceRegion(desktopTexture, 0, scaledRegion, SharedTexture, 0);
                        Device3d11.ImmediateContext.Flush();


                        //device3d11.ImmediateContext.CopyResource(scaledTexture, stagingTexture);
                        //device3d11.ImmediateContext.CopyResource(desktopTexture, stagingTexture);


                        //var syncRoot = videoBuffer.syncRoot;
                        //bool lockTaken = false;
                        //try
                        //{
                        //    Monitor.TryEnter(syncRoot, /*timeout*/1000, ref lockTaken);
                        //    if (lockTaken)
                        //    {
                        //        Result = TextureToBitmap(StagingTexture, videoBuffer.bitmap);
                        //    }
                        //    else
                        //    {
                        //        logger.Debug("lockTaken == false");
                        //    }

                        //}
                        //finally
                        //{
                        //    if (lockTaken)
                        //    {
                        //        Monitor.Exit(syncRoot);
                        //    }
                        //}

                        Result = true;

                    }
                    else
                    {
                        if (acquireResult == SharpDX.DXGI.ResultCode.WaitTimeout)
                        {
                            return true;
                        }
                        else
                        {
                            logger.Debug("result.Code " + acquireResult.Code);
                            acquireResult.CheckError();
                        }
                    }
                }
                finally
                {
                    if (desktopResource != null)
                    {
                        deskDupl.ReleaseFrame();

                        desktopResource.Dispose();
                        desktopResource = null;
                    }
                }
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == SharpDX.DXGI.ResultCode.WaitTimeout)
                {
                    return true;
                }
                else
                {

                    const int E_ACCESSDENIED = -2147024891;// 0x80070005;

                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.AccessLost ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.AccessDenied ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved ||
                        ex.HResult == E_ACCESSDENIED)
                    {

                        
                        logger.Warn(ex.Descriptor.ToString());
                    }
                    else
                    {
                        logger.Error(ex);
                    }

                    CloseDx();

                    //throw;
                    Thread.Sleep(100);
                }

            }

            return Result;
        }
        private bool flag = false;
        private unsafe void DrawCursor()
        {

            var position = cursorObj.Position;

            var shapeBuff = cursorObj.PtrShapeBuffer;
            var shapeInfo = cursorObj.ShapeInfo;

            int width = shapeInfo.Width;
            int height = shapeInfo.Height;
            int pitch = shapeInfo.Pitch;

            int left = position.X;
            int top = position.Y;
            int right = position.X + width;
            int bottom = position.Y + height;


            if (cursorObj.ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {

                var data = new DataPointer(shapeBuff, height * pitch);
                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                var size = new Size2(width, height);
                var cursorBits = new Direct2D.Bitmap(desktopTarget, size, data, pitch, prop);
                try
                {

                    var rect = new RawRectangleF(left, top, right, bottom);

                    desktopTarget.DrawBitmap(cursorBits, rect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);

                }
                finally
                {
                    cursorBits?.Dispose();
                }
            }
            else if (cursorObj.ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME)
            {
                height = height / 2;

                left = position.X;
                top = position.Y;
                right = position.X + width;
                bottom = position.Y + height;
                pitch = width * 4;

                Texture2D desktopRegionTex = null;
                try
                {
                    desktopRegionTex = new Texture2D(Device3d11,
                    new Texture2DDescription
                    {
                        CpuAccessFlags = CpuAccessFlags.Read,
                        BindFlags = BindFlags.None,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Staging,
                        OptionFlags = ResourceOptionFlags.None,
                    });

                    var region = new ResourceRegion(left, top, 0, right, bottom, 1);
                    Device3d11.ImmediateContext.CopySubresourceRegion(acquiredDesktopImage, 0, region, desktopRegionTex, 0);

                    var dataBox = Device3d11.ImmediateContext.MapSubresource(desktopRegionTex, 0, MapMode.Read, MapFlags.None);
                    try
                    {
                        var desktopBuffer = new byte[width * height * 4];
                        Marshal.Copy(dataBox.DataPointer, desktopBuffer, 0, desktopBuffer.Length);

                        var shapeBufferLenght = width * height * 4;
                        var shapeBuffer = new byte[shapeBufferLenght];

                        var maskBufferLenght = width * height / 8;
                        var andMaskBuffer = new byte[maskBufferLenght];
                        Marshal.Copy(shapeBuff, andMaskBuffer, 0, andMaskBuffer.Length);

                        var xorMaskBuffer = new byte[maskBufferLenght];
                        Marshal.Copy(shapeBuff + andMaskBuffer.Length, xorMaskBuffer, 0, xorMaskBuffer.Length);

                        for (var row = 0; row < height; ++row)
                        {
                            byte mask = 0x80;

                            for (var col = 0; col < width; ++col)
                            {
                                var maskIndex = row * width / 8 + col / 8;

                                var andMask = ((andMaskBuffer[maskIndex] & mask) == mask) ? 0xFF : 0;
                                var xorMask = ((xorMaskBuffer[maskIndex] & mask) == mask) ? 0xFF : 0;

                                int pos = row * width * 4 + col * 4;
                                for (int i = 0; i < 3; i++)
                                {// RGB
                                    shapeBuffer[pos] = (byte)((desktopBuffer[pos] & andMask) ^ xorMask);
                                    pos++;
                                }
                                // Alpha
                                shapeBuffer[pos] = (byte)((desktopBuffer[pos] & 0xFF) ^ 0);

                                if (mask == 0x01)
                                {
                                    mask = 0x80;
                                }
                                else
                                {
                                    mask = (byte)(mask >> 1);
                                }
                            }
                        }


                        Direct2D.Bitmap cursorBits = null;
                        try
                        {
                            fixed (byte* ptr = shapeBuffer)
                            {
                                var data = new DataPointer(ptr, height * pitch);
                                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                                var size = new Size2(width, height);
                                cursorBits = new Direct2D.Bitmap(desktopTarget, size, data, pitch, prop);
                            };

                            var shapeRect = new RawRectangleF(left, top, right, bottom);

                            desktopTarget.DrawBitmap(cursorBits, shapeRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);
                        }
                        finally
                        {
                            cursorBits?.Dispose();
                        }

                    }
                    finally
                    {
                        Device3d11.ImmediateContext.UnmapSubresource(desktopRegionTex, 0);
                    }

                }
                finally
                {
                    desktopRegionTex?.Dispose();
                }
            }
            else if (cursorObj.ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {
                logger.Warn("Not supported cursor type " + ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR);
            }
        }

        class FramePars
        {
            public OutputDuplicateFrameInformation FrameInfo;
            public OutputDuplicateMoveRectangle[] MoveRects = new OutputDuplicateMoveRectangle[0];
            public RawRectangle[] DirtyRects = new RawRectangle[0];

        }

        private FramePars GetFrameParams(OutputDuplicateFrameInformation frameInfo)
        {
            FramePars frameParams = new FramePars();

            OutputDuplicateMoveRectangle[] moveRects = new OutputDuplicateMoveRectangle[0];
            RawRectangle[] dirtyRects = new RawRectangle[0];

            if (frameInfo.TotalMetadataBufferSize > 0) //if (frameInfo.AccumulatedFrames > 0)
            {
                var bufferSizeBytes = frameInfo.TotalMetadataBufferSize;

                int moveRectSizeBytes = GetMoveRects(bufferSizeBytes, out moveRects);
                bufferSizeBytes -= moveRectSizeBytes;


                int dirtyRectSizeBytes = GetDirtyRects(bufferSizeBytes, out dirtyRects);
                bufferSizeBytes -= dirtyRectSizeBytes;

                frameParams.DirtyRects = dirtyRects;
                frameParams.MoveRects = moveRects;
                frameParams.FrameInfo = frameInfo;

            }

            return frameParams;
        }

        private int GetDirtyRects(int bufferSizeBytes, out RawRectangle[] dirtyRects)
        {

            int dirtyRectsBufferSizeRequired = 0;
            dirtyRects = new RawRectangle[0];
            if (bufferSizeBytes <= 0)
            {
                return dirtyRectsBufferSizeRequired;

            }

            var rawRectItemSize = Marshal.SizeOf(typeof(RawRectangle));
            var maxRawRectItemCount = bufferSizeBytes / rawRectItemSize;

            var dirtyRectsBuffer = new RawRectangle[maxRawRectItemCount];

            try
            {
                deskDupl.GetFrameDirtyRects(bufferSizeBytes, dirtyRectsBuffer, out dirtyRectsBufferSizeRequired);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == SharpDX.DXGI.ResultCode.MoreData)
                {
                    logger.Error(ex);

                    // TODO:
                    //...
                }

                throw;
            }

            var dirtyCount = dirtyRectsBufferSizeRequired / rawRectItemSize;
            if (dirtyCount > 0)
            {
                if (dirtyCount > maxRawRectItemCount)
                {
                    dirtyRects = dirtyRectsBuffer.Take(dirtyCount).ToArray();
                }
                else
                {
                    dirtyRects = dirtyRectsBuffer;
                }
            }

            return dirtyRectsBufferSizeRequired;
        }

        private int GetMoveRects(int bufferSizeBytes, out OutputDuplicateMoveRectangle[] moveRects)
        {
            int moveRectsBufferSizeRequired = 0;

            moveRects = new OutputDuplicateMoveRectangle[0];
            if (bufferSizeBytes <= 0)
            {
                return moveRectsBufferSizeRequired;
            }

            var moveItemSize = Marshal.SizeOf(typeof(OutputDuplicateMoveRectangle));
            var maxMoveRectItemCount = bufferSizeBytes / moveItemSize;

            if (maxMoveRectItemCount > 0)
            {
                var moveRectBuf = new OutputDuplicateMoveRectangle[maxMoveRectItemCount];

                try
                {
                    deskDupl.GetFrameMoveRects(bufferSizeBytes, moveRectBuf, out moveRectsBufferSizeRequired);
                }
                catch (SharpDXException ex)
                {
                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.MoreData)
                    {
                        logger.Error(ex);

                        // TODO:
                        //maxMoveRectItemCount = moveRectsBufferSizeRequired / moveItemSize;
                        //moveRectBuf = new OutputDuplicateMoveRectangle[maxMoveRectItemCount];
                        // continue...
                    }

                    throw;
                }


                var moveCount = moveRectsBufferSizeRequired / moveItemSize;

                if (moveCount > 0)
                {
                    if (maxMoveRectItemCount > moveCount)
                    {
                        moveRects = moveRectBuf.Take(moveCount).ToArray();
                    }
                    else
                    {
                        moveRects = moveRectBuf;
                    }
                }
            }


            return moveRectsBufferSizeRequired;
        }

        private CursorObj cursorObj = new CursorObj();

        class CursorObj
        {
           // public byte[] ShapeBuffer = null;

            public IntPtr PtrShapeBuffer = IntPtr.Zero;
            public int BufferSize = 0;

            public OutputDuplicatePointerShapeInformation ShapeInfo;
            public RawPoint Position = new RawPoint();
            public bool Visible = false;

            public int WhoUpdatedPositionLast = 0;
            public long LastTimeStamp = 0;

            internal GDI.Bitmap GetBitmap(GDI.Bitmap desktop)
            {
                GDI.Bitmap bmpCursor = null;

                int x = Position.X;
                int y = Position.Y;

                int width = ShapeInfo.Width;
                int height = ShapeInfo.Height;
                int pitch = ShapeInfo.Pitch;

                int givenLeft = Position.X;
                int givenTop = Position.Y;


                int screenWidth = desktop.Width;
                int screenHeigth = desktop.Height;
                if (x + width > screenWidth)
                {
                    width = screenWidth - x;
                }

                if (y + height > screenHeigth)
                {
                    height = screenHeigth - y;
                }

                var rect = new GDI.Rectangle(x, y, width, height);

                if (ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
                {
                    bmpCursor = new GDI.Bitmap(width, height, pitch, GDI.Imaging.PixelFormat.Format32bppArgb, PtrShapeBuffer);

                }
                else if (ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME)
                {
                    height = height / 2;

                    var desktopBuffer = new byte[width * height * 4];

                    {
                        var data = desktop.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, desktop.PixelFormat);
                        var offset = 0;
                        for (var row = 0; row < height; row++)
                        {
                            Marshal.Copy(data.Scan0, desktopBuffer, offset, width * 4);
                            offset += width * 4;
                            data.Scan0 = IntPtr.Add(data.Scan0, data.Stride);
                        }

                        desktop.UnlockBits(data);
                    }

                    var shapeBufferLenght = width * height * 4;
                    var shapeBuffer = new byte[shapeBufferLenght];

                    var maskBufferLenght = width * height / 8;
                    var andMaskBuffer = new byte[maskBufferLenght];
                    Marshal.Copy(PtrShapeBuffer, andMaskBuffer, 0, andMaskBuffer.Length);

                    var xorMaskBuffer = new byte[maskBufferLenght];
                    Marshal.Copy(PtrShapeBuffer + andMaskBuffer.Length, xorMaskBuffer, 0, xorMaskBuffer.Length);

                    for (var row = 0; row < height; ++row)
                    {
                        byte mask = 0x80;

                        for (var col = 0; col < width; ++col)
                        {
                            var maskIndex = row * width / 8 + col / 8;

                            var andMask = ((andMaskBuffer[maskIndex] & mask) == mask) ? 0xFF : 0;
                            var xorMask = ((xorMaskBuffer[maskIndex] & mask) == mask) ? 0xFF : 0;

                            int pos = row * width * 4 + col * 4;
                            for ( int i = 0; i < 3; i++)
                            {// RGB
                                shapeBuffer[pos] = (byte)((desktopBuffer[pos] & andMask) ^ xorMask);
                                pos++;
                            }
                            // Alpha
                            shapeBuffer[pos] = (byte)((desktopBuffer[pos] & 0xFF) ^ 0); 

                            if (mask == 0x01)
                            {
                                mask = 0x80;
                            }
                            else
                            {
                                mask = (byte)(mask >> 1);
                            }
                        }
                    }

                    bmpCursor = new GDI.Bitmap(width, height, GDI.Imaging.PixelFormat.Format32bppArgb);
                    {
                        var data = bmpCursor.LockBits(new GDI.Rectangle(0, 0, width, height), GDI.Imaging.ImageLockMode.ReadWrite, bmpCursor.PixelFormat);
                        var offset = 0;
                        for (var row = 0; row < height; row++)
                        {
                            Marshal.Copy(shapeBuffer, offset, data.Scan0, width * 4);
                            offset += width * 4;
                            data.Scan0 = IntPtr.Add(data.Scan0, data.Stride);
                        }
                        bmpCursor.UnlockBits(data);
                    }

                }
                else if (ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR)
                {
                    //...
                    logger.Warn("Not supported");
                }

                return bmpCursor;


            }

            public void Dispose()
            {
                if (PtrShapeBuffer != null)
                {
                    Marshal.FreeHGlobal(PtrShapeBuffer);
                    PtrShapeBuffer = IntPtr.Zero;
                }
            }



        }

        private void UpdateMouseInfo(OutputDuplicateFrameInformation frameInfo)
        {
            if (frameInfo.LastMouseUpdateTime > 0)
            {// A non-zero mouse update timestamp indicates that there is a mouse position update and optionally a shape change

                var pointerPosition = frameInfo.PointerPosition;

                cursorObj.LastTimeStamp = frameInfo.LastMouseUpdateTime;
                cursorObj.Position = pointerPosition.Position;
                cursorObj.Visible = pointerPosition.Visible;

                // logger.Debug("mouseInfo " + mouseInfo.Position.X + " " +  mouseInfo.Position.Y);

                if (frameInfo.PointerShapeBufferSize > 0)
                {

                    var bufSize = frameInfo.PointerShapeBufferSize;

                    if (bufSize > cursorObj.BufferSize)
                    {
                        var ptr = cursorObj.PtrShapeBuffer;
                        if (ptr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                        cursorObj.BufferSize = bufSize;
                        cursorObj.PtrShapeBuffer = Marshal.AllocHGlobal(cursorObj.BufferSize);

                    }

                    int pointerShapeBufferSizeRequired;
                    try
                    {
                        OutputDuplicatePointerShapeInformation pointerShapeInfo;
                        deskDupl.GetFramePointerShape(cursorObj.BufferSize, cursorObj.PtrShapeBuffer, out pointerShapeBufferSizeRequired, out pointerShapeInfo);

                        cursorObj.ShapeInfo = pointerShapeInfo;
                        //logger.Debug("pointerShapeInfo " + pointerShapeInfo.Type);
                    }
                    catch (SharpDXException ex)
                    {
       
                        if (ex.ResultCode == SharpDX.DXGI.ResultCode.MoreData)
                        {
                            // TODO:
                            //...
                            logger.Error(ex);

                        }

                        throw;
                    }

                }
                else
                {// No new shape

                }
            }
        }


        private bool TextureToBitmap(Texture2D texture, GDI.Bitmap bmp)
        {

            bool success = false;
            var descr = texture.Description;
            if (bmp.Width != descr.Width || bmp.Height != descr.Height)
            {
                //...
                logger.Warn(bmp.Width != descr.Width || bmp.Height != descr.Height);
                return false;
            }

            try
            {
                var srcData = Device3d11.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);

                int width = bmp.Width;
                int height = bmp.Height;
                var rect = new GDI.Rectangle(0, 0, width, height);
                var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                try
                {
                    IntPtr srcPtr = srcData.DataPointer;
                    int srcOffset = rect.Top * srcData.RowPitch + rect.Left * 4;

                    srcPtr = IntPtr.Add(srcPtr, srcOffset);

                    var destPtr = destData.Scan0;
                    for (int row = rect.Top; row < rect.Bottom; row++)
                    {
                        Utilities.CopyMemory(destPtr, srcPtr, width * 4);
                        srcPtr = IntPtr.Add(srcPtr, srcData.RowPitch);
                        destPtr = IntPtr.Add(destPtr, destData.Stride);

                    }

                    success = true;
                }
                finally
                {
                    bmp.UnlockBits(destData);
                }

            }
            finally
            {
                Device3d11.ImmediateContext.UnmapSubresource(texture, 0);
            }

            return success;
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

            if (cursorObj != null)
            {
                cursorObj.Dispose();
                cursorObj = null;
            }

            
            if (desktopTexture != null && !desktopTexture.IsDisposed)
            {
                desktopTexture.Dispose();
                desktopTexture = null;
            }

            if (desktopTarget != null && !desktopTarget.IsDisposed)
            {
                desktopTarget.Dispose();
                desktopTarget = null;
            }

            if (acquiredDesktopImage != null && !acquiredDesktopImage.IsDisposed)
            {
                acquiredDesktopImage.Dispose();
                acquiredDesktopImage = null;
            }

            if (deskDupl != null && !deskDupl.IsDisposed)
            {
                deskDupl.Dispose();
                deskDupl = null;
            }

            if (SharedTexture != null && !SharedTexture.IsDisposed)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (Device3d11 != null && !Device3d11.IsDisposed)
            {
                Device3d11.Dispose();
                Device3d11 = null;
            }

            //if (output != null && !output.IsDisposed)
            //{
            //    output.Dispose();
            //    output = null;
            //}

            //if (adapter != null && !adapter.IsDisposed)
            //{
            //    adapter.Dispose();
            //    adapter = null;
            //}


            //if (factory2D1 != null && !factory2D1.IsDisposed)
            //{
            //    factory2D1.Dispose();
            //    factory2D1 = null;
            //}

            //if (dxgiFactory != null && !dxgiFactory.IsDisposed)
            //{
            //    dxgiFactory.Dispose();
            //    dxgiFactory = null;
            //}
        }


        public static SharpDX.Direct3D11.Texture2D GetTexture(GDI.Bitmap bitmap, Device device)
        {

            if (bitmap.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                bitmap = bitmap.Clone(new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height), GDI.Imaging.PixelFormat.Format32bppArgb);
            }
            var data = bitmap.LockBits(new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height), GDI.Imaging.ImageLockMode.ReadOnly, GDI.Imaging.PixelFormat.Format32bppArgb);
            var texture = new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                ArraySize = 1,
                //BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                //Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                //CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                //OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            }, new SharpDX.DataRectangle(data.Scan0, data.Stride));

            bitmap.UnlockBits(data);

            return texture;
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
