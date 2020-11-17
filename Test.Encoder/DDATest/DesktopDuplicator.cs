﻿using System;
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
using System.Windows.Forms;
using SharpDX.Direct2D1;


namespace Test.Encoder.DDATest
{
    public class DesktopFrame
    {
        public DesktopFrame(Texture2D texture, long timestamp)
        {
            this.Texture = texture;
            this.Timestamp = timestamp;
        }

        public readonly Texture2D Texture = null;
        public readonly long Timestamp = 0;
    }

    public class DesktopDuplicator
    {

        //private static TraceSource logger = TraceManager.GetTrace("DesktopDuplicator");
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public DesktopDuplicator(Device device = null)
        {
            this.device = device;
        }

        private Device device = null;
        private Output output = null;

        private OutputDuplication deskDupl = null;
        private Texture2D screenTexture = null;
        private RenderTarget screenTarget = null;

        public Texture2D SharedTexture { get; private set; } = null;
        public IntPtr GetSharedHandle()
        {
            IntPtr handle = IntPtr.Zero;
            if (SharedTexture != null)
            {
                using (var sharedRes = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
                {
                    handle = sharedRes.SharedHandle;
                }
            }

            return handle;
        }

        internal bool CaptureMouse = true;
        private CursorInfo cursorInfo = null;

        internal Rectangle drawRect = Rectangle.Empty;
        internal Rectangle duplRect = Rectangle.Empty;

        public bool Initialized { get; private set; } = false;

        private RawRectangle DesktopRect;

        private bool deviceReady = false;

        //public void Init(Output _output, GDI.Rectangle srcRect)

        public void Init(GDI.Rectangle screenRect, GDI.Rectangle srcRect)
        {
            logger.Debug("DesktopDuplicator::Init(...) " + screenRect);

            var hMonitor = Utils.GetMonitorFromRect(screenRect);


            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                var adapters = dxgiFactory.Adapters;
                foreach (var adapter in adapters)
                {
                    var outputs = adapter.Outputs;

                    foreach (var output in outputs)
                    {
                        var _descr = output.Description;
                        if (_descr.MonitorHandle == hMonitor)
                        {
                            this.output = output;
                            this.DesktopRect = _descr.DesktopBounds;

                            if (device == null)
                            {
                                InitDevice(adapter);
                            }

                            continue;
                        }

                        output.Dispose();
                    }

                    adapter.Dispose();
                }
            }



            InitTextures();

            InitDuplicator();

            SetupRegions(this.DesktopRect, srcRect);

            cursorInfo = new CursorInfo();

            deviceReady = true;
        }

        private void InitDevice(Adapter adapter)
        {
            var deviceCreationFlags =
                //DeviceCreationFlags.Debug |
                DeviceCreationFlags.VideoSupport |
                DeviceCreationFlags.BgraSupport;

            device = new Device(adapter, deviceCreationFlags);
            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }
        }


        private void InitTextures()
        {

            int width = DesktopRect.Right - DesktopRect.Left;
            int height = DesktopRect.Bottom - DesktopRect.Top;

            SharedTexture = new Texture2D(device,
                new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = Format.B8G8R8A8_UNorm,
                    Width = width,
                    Height = height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,

                    OptionFlags = ResourceOptionFlags.Shared,

                });

            screenTexture = new Texture2D(device,
              new Texture2DDescription
              {
                  CpuAccessFlags = CpuAccessFlags.None,
                  BindFlags = BindFlags.RenderTarget,
                  Format = Format.B8G8R8A8_UNorm,
                  Width = width,
                  Height = height,
                  MipLevels = 1,
                  ArraySize = 1,
                  SampleDescription = { Count = 1, Quality = 0 },
                  Usage = ResourceUsage.Default,

                  OptionFlags = ResourceOptionFlags.None,

              });

            using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded))
            {
                using (var surf = screenTexture.QueryInterface<Surface>())
                {
                    var pixelFormat = new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied);
                    var renderTargetProps = new Direct2D.RenderTargetProperties(pixelFormat);
                    screenTarget = new Direct2D.RenderTarget(factory2D1, surf, renderTargetProps);
                }
            }
        }

        private void SetupRegions(RawRectangle screenRect, GDI.Rectangle srcRect)
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
            drawRect = new Rectangle
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

            duplRect = new Rectangle
            {
                X = duplLeft,
                Y = duplTop,
                Width = duplRight - duplLeft,
                Height = duplBottom - duplTop,
            };


            logger.Debug("duplRect=" + duplRect.ToString() + " drawRect=" + drawRect.ToString());

        }

        internal void InitDuplicator()
        {
            try
            {
                using (var output1 = output.QueryInterface<Output1>())
                {
                    try
                    {// Duplicate the output
                     //https://docs.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutput1-duplicateoutput
                        deskDupl = output1.DuplicateOutput(device);

                        Initialized = true;

                    }
                    catch (SharpDXException ex)
                    {
                        if (ex.HResult == (int)HResult.E_INVALIDARG)
                        {

                            /*
                                * E_INVALIDARG for one of the following reasons:
                                * 
                                --The specified device (pDevice) is invalid, was not created on the correct adapter, 
                                or was not created from IDXGIFactory1 (or a later version of a DXGI factory interface that inherits from IDXGIFactory1).

                                --The calling application is already duplicating this desktop output.
                                */
                        }
                        else if (ex.HResult == (int)HResult.E_ACCESSDENIED)
                        {
                            //E_ACCESSDENIED if the application does not have access privilege to the current desktop image.
                            //For example, only an application that runs at LOCAL_SYSTEM can access the secure desktop.
                        }
                        else if (ex.ResultCode == SharpDX.DXGI.ResultCode.Unsupported)
                        {
                            /*
                                * DXGI_ERROR_UNSUPPORTED if the created IDXGIOutputDuplication interface does not support the current desktop mode or scenario.
                                * For example, 8bpp and non-DWM desktop modes are not supported.
                                * If DuplicateOutput fails with DXGI_ERROR_UNSUPPORTED,
                                * the application can wait for system notification of desktop switches and mode changes and then call DuplicateOutput again after such a notification occurs.
                                * For more information, refer to EVENT_SYSTEM_DESKTOPSWITCH and mode change notification (WM_DISPLAYCHANGE). 
                                */

                        }
                        else if (ex.ResultCode == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable)
                        {
                            /*
                                * if DXGI reached the limit on the maximum number of concurrent duplication applications (default of four).
                                * Therefore, the calling application cannot create any desktop duplication interfaces until the other applications close.
                                */
                        }
                        else if (ex.ResultCode == SharpDX.DXGI.ResultCode.SessionDisconnected)
                        {
                            //if DuplicateOutput failed because the session is currently disconnected.
                        }

                        throw;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                CloseDuplicator();

                throw;
            }

        }

        private bool running = false;
        public void StartCapture()
        {
            if (running)
            {
                return;
            }

            running = true;
            Task.Run(() =>
            {

                while (running)
                {
                    int timeout = 0;

                    if (Initialized)
                    {
                        var res = GrabScreen(timeout);
                        if (res == ErrorCode.Ok)
                        {
                            FrameAcquired?.Invoke();
                        }
                        else
                        {
                            Thread.Sleep(100);
                            ResetDuplicator();

                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                        ResetDuplicator();
                    }

                    Thread.Sleep(16);

                }
            });
        }


        public event Action FrameAcquired;

        public void StopCapture()
        {
            running = false;

        }

        public ErrorCode GrabScreen(int timeout = 0)
        {
            ErrorCode Result = ErrorCode.Unexpected;
            if (!deviceReady)
            {
                return ErrorCode.NotReady;
            }

            try
            {
                SharpDX.DXGI.Resource desktopResource = null;
                try
                {
                    //https://docs.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutputduplication-acquirenextframe
                    var acquireResult = deskDupl.TryAcquireNextFrame(timeout, out OutputDuplicateFrameInformation frameInfo, out desktopResource);
                    //SharpDX.Result acquireResult = SharpDX.Result.Ok;
                    if (acquireResult.Failure)
                    {

                        if (acquireResult == SharpDX.DXGI.ResultCode.WaitTimeout)
                        {
                            //DXGI_ERROR_WAIT_TIMEOUT if the time -out interval elapsed before the next desktop frame was available.

                            //logger.Debug("DXGI_ERROR_WAIT_TIMEOUT");

                            return ErrorCode.Ok;
                        }
                        else
                        {

                            /* 
                             * DXGI_ERROR_ACCESS_LOST if the desktop duplication interface is invalid. The desktop duplication interface typically becomes invalid when a different type of image is displayed on the desktop.
                             * Examples of this situation are:
                                -Desktop switch
                                -Mode change
                                -Switch from DWM on, DWM off, or other full-screen application
                                In this situation, the application must release the IDXGIOutputDuplication interface and create a new IDXGIOutputDuplication for the new content.


                                DXGI_ERROR_INVALID_CALL if the application called AcquireNextFrame without releasing the previous frame.

                                E_INVALIDARG if one of the parameters to AcquireNextFrame is incorrect; for example, if pFrameInfo is NULL.

                                Possibly other error codes that are described in the DXGI_ERROR topic.
                             */

                            logger.Debug("result.Code " + acquireResult.Code);
                            acquireResult.CheckError();
                        }
                    }

                    using (var duplTexture = desktopResource.QueryInterface<Texture2D>())
                    {
                        //UpdateRegions(frameInfo);

                        UpdateCursorInfo(frameInfo);

                        device.ImmediateContext.CopyResource(duplTexture, screenTexture);

                    }

                    if (CaptureMouse && cursorInfo.Visible)
                    {
                        DrawCursor(cursorInfo);
                    }

                    device.ImmediateContext.CopyResource(screenTexture, SharedTexture);
                    device.ImmediateContext.Flush();

                    Result = 0;
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
                    return 0;
                }
                else
                {
                    if (ex.ResultCode == SharpDX.DXGI.ResultCode.AccessLost ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.AccessDenied ||
                        ex.HResult == (int)HResult.E_ACCESSDENIED)
                    {

                        logger.Warn(ex.Descriptor.ToString());

                        Result = ErrorCode.AccessDenied;

                    }
                    else if (ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceReset ||
                        ex.ResultCode == SharpDX.DXGI.ResultCode.DeviceRemoved)
                    {
                        logger.Warn(ex.Descriptor.ToString());
                        Result = ErrorCode.NotInitialized;
                    }

                    else
                    {
                        logger.Error(ex);
                        Result = ErrorCode.Unexpected;
                    }

                }

                var hResult = device.DeviceRemovedReason;

            }

            return Result;
        }


        #region Process regions


        private void UpdateRegions(OutputDuplicateFrameInformation frameInfo)
        {
            var frameParams = GetFrameParams(frameInfo);

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

        #endregion

        #region Process cursor


        private void UpdateCursorInfo(OutputDuplicateFrameInformation frameInfo)
        {

            if (frameInfo.LastMouseUpdateTime > 0)
            {// A non-zero mouse update timestamp indicates that there is a mouse position update and optionally a shape change

                cursorInfo.Update(frameInfo);

                if (frameInfo.PointerShapeBufferSize > 0)
                {
                    try
                    {
                        var size = cursorInfo.BufferSize;
                        var ptr = cursorInfo.PtrShapeBuffer;

                        deskDupl.GetFramePointerShape(size, ptr,
                            out int pointerShapeBufferSizeRequired,
                            out OutputDuplicatePointerShapeInformation pointerShapeInfo);

                        cursorInfo.ShapeInfo = pointerShapeInfo;

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
                //{// No new shape

                //}

            }

        }
        private unsafe void DrawCursor(CursorInfo cursorInfo)
        {

            if (!cursorInfo.Visible)
            {
                //logger.Debug("No cursor");
                return;
            }

            var position = cursorInfo.Position;

            var shapeBuff = cursorInfo.PtrShapeBuffer;
            var shapeInfo = cursorInfo.ShapeInfo;

            int width = shapeInfo.Width;
            int height = shapeInfo.Height;
            int pitch = shapeInfo.Pitch;

            int left = position.X;
            int top = position.Y;
            int right = position.X + width;
            int bottom = position.Y + height;

            screenTarget.BeginDraw();

            //renderTarger.Clear(Color.Black);

            if (shapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {

                var data = new DataPointer(shapeBuff, height * pitch);
                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                var size = new Size2(width, height);
                var cursorBits = new Direct2D.Bitmap(screenTarget, size, data, pitch, prop);
                try
                {
                    var cursorRect = new RawRectangleF(left, top, right, bottom);

                    screenTarget.DrawBitmap(cursorBits, cursorRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);

                }
                finally
                {
                    cursorBits?.Dispose();
                }
            }
            else if (shapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME)
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
                    desktopRegionTex = new Texture2D(device,
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
                    var immediateContext = device.ImmediateContext;

                    immediateContext.CopySubresourceRegion(screenTexture, 0, region, desktopRegionTex, 0);

                    var dataBox = immediateContext.MapSubresource(desktopRegionTex, 0, MapMode.Read, MapFlags.None);
                    try
                    {

                        Direct2D.Bitmap cursorBits = null;
                        try
                        {
                            var desktopBuffer = new byte[width * height * 4];
                            Marshal.Copy(dataBox.DataPointer, desktopBuffer, 0, desktopBuffer.Length);

                            fixed (byte* ptr = cursorInfo.GetMonochromeShape(desktopBuffer, new GDI.Size(width, height)))
                            {
                                var data = new DataPointer(ptr, height * pitch);
                                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                                var size = new Size2(width, height);
                                cursorBits = new Direct2D.Bitmap(screenTarget, size, data, pitch, prop);
                                var shapeRect = new RawRectangleF(left, top, right, bottom);

                                screenTarget.DrawBitmap(cursorBits, shapeRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);
                            };

                        }
                        finally
                        {
                            cursorBits?.Dispose();
                        }

                    }
                    finally
                    {
                        immediateContext.UnmapSubresource(desktopRegionTex, 0);
                    }

                }
                finally
                {
                    desktopRegionTex?.Dispose();
                }
            }
            else if (shapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {
                logger.Warn("Not supported cursor type " + ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR);
            }


            screenTarget.EndDraw();

        }


        class CursorInfo
        {
            public IntPtr PtrShapeBuffer { get; private set; } = IntPtr.Zero;
            public int BufferSize { get; private set; } = 0;

            public OutputDuplicatePointerShapeInformation ShapeInfo { get; internal set; }
            public RawPoint Position { get; private set; } = new RawPoint();
            public bool Visible { get; private set; } = false;

            public int WhoUpdatedPositionLast { get; private set; } = 0;
            public long LastTimeStamp { get; private set; } = 0;

            public int DataSize => ShapeInfo.Height * ShapeInfo.Pitch;
            public GDI.Rectangle Rect => new GDI.Rectangle(Position.X, Position.Y, ShapeInfo.Width, ShapeInfo.Height);
            public GDI.Size Size => new GDI.Size(ShapeInfo.Width, ShapeInfo.Height);


            public void Update(OutputDuplicateFrameInformation frameInfo)
            {
                var pointerPosition = frameInfo.PointerPosition;

                this.LastTimeStamp = frameInfo.LastMouseUpdateTime;
                this.Position = pointerPosition.Position;
                this.Visible = pointerPosition.Visible;

                var bufSize = frameInfo.PointerShapeBufferSize;

                if (bufSize > this.BufferSize)
                {
                    Alloc(bufSize);
                }
            }

            internal GDI.Bitmap GetGdiBitmap(GDI.Bitmap desktop)
            {
                GDI.Bitmap bmpCursor = null;

                int x = Position.X;
                int y = Position.Y;

                int width = ShapeInfo.Width;
                int height = ShapeInfo.Height;
                int pitch = ShapeInfo.Pitch;

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

                    byte[] shapeBuffer = GetMonochromeShape(desktopBuffer, new GDI.Size(width, height));

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

            internal void Alloc(int bufferSize)
            {
                FreeBuffer();

                this.BufferSize = bufferSize;
                PtrShapeBuffer = Marshal.AllocHGlobal(BufferSize);
            }

            internal byte[] GetMonochromeShape(byte[] desktopBuffer, GDI.Size size)
            {

                int width = size.Width;
                int height = size.Height;

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

                return shapeBuffer;
            }


            private void FreeBuffer()
            {
                if (PtrShapeBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(PtrShapeBuffer);
                    PtrShapeBuffer = IntPtr.Zero;
                }
            }

            public void Dispose()
            {
                FreeBuffer();
            }


        }
        #endregion



        private void ResetDuplicator()
        {
            try
            {
                CloseDuplicator();
                InitDuplicator();

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private void CloseDuplicator()
        {

            if (deskDupl != null && !deskDupl.IsDisposed)
            {
                deskDupl.Dispose();
                deskDupl = null;
            }

            Initialized = false;
        }

        public void Close()
        {
            deviceReady = false;

            CloseDuplicator();

            if (device != null && !device.IsDisposed)
            {
                device.Dispose();
                device = null;
            }

            if (output != null && !output.IsDisposed)
            {
                output.Dispose();
                output = null;
            }

            if (SharedTexture != null && !SharedTexture.IsDisposed)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (screenTexture != null && !screenTexture.IsDisposed)
            {
                screenTexture.Dispose();
                screenTexture = null;
            }

            if (screenTarget != null && !screenTarget.IsDisposed)
            {
                screenTarget.Dispose();
                screenTarget = null;
            }

            if (cursorInfo != null)
            {
                cursorInfo.Dispose();
                cursorInfo = null;
            }


        }


    }
}
