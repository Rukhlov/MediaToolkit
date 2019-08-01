
using System;
using System.Collections.Generic;
using System.Drawing;
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
using ScreenStreamer.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace ScreenStreamer
{
    public class DXGIDesktopDuplicationCapture : ScreenCapture
    {
        public DXGIDesktopDuplicationCapture(object[] args) : base()
        { }

        private SharpDX.DXGI.Factory1 dxgiFactory = null;
        private Device device3d11 = null;
        private Adapter1 adapter = null;
        private Output output = null;

        private OutputDuplication deskDupl = null;
        private Texture2D acquiredDesktopImage = null;
        private Texture2D screenTexture = null;

        public override void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            logger.Debug("DXGIDesktopDuplicationCapture::Init()");

            base.Init(srcRect, destSize);

            InitDx(srcRect);

        }

        private void InitDx(GDI.Rectangle srcRect)
        {
            dxgiFactory = new SharpDX.DXGI.Factory1();
            adapter = dxgiFactory.Adapters1.FirstOrDefault();

            // Get DXGI.Output
            output = adapter.Outputs.FirstOrDefault();

            // Get DXGI.Output
            var hMonitor = User32.GetMonitorFromRect(srcRect);
            if (hMonitor != IntPtr.Zero)
            {
                output = adapter.Outputs.FirstOrDefault(o => o.Description.MonitorHandle == hMonitor);
            }

            if (output == null)
            {
                output = adapter.Outputs.FirstOrDefault();
            }

            device3d11 = new Device(adapter);
            using (var multiThread = device3d11.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            //using (var dxgi = device3d11.QueryInterface<SharpDX.DXGI.Device2>())
            //{
            //    SharpDX.Direct2D1.Device device2d = new SharpDX.Direct2D1.Device(dxgi);
            //    var d2dContext = new SharpDX.Direct2D1.DeviceContext(device2d, SharpDX.Direct2D1.DeviceContextOptions.None);

            //}

            using (var output1 = output.QueryInterface<Output1>())
            {
                // Duplicate the output
                deskDupl = output1.DuplicateOutput(device3d11);
            }

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = videoBuffer.bitmap.Width,
                Height = videoBuffer.bitmap.Height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };

            screenTexture = new Texture2D(device3d11, textureDesc);

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

                        device3d11.ImmediateContext.CopyResource(acquiredDesktopImage, screenTexture);
                        //using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                        //{
                        //    device3d11.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
                        //    //device3d11.ImmediateContext.CopySubresourceRegion(screenTexture2D, 0, null, screenTexture, 0);
                        //}
                        //device3d11.ImmediateContext.CopySubresourceRegion(screenTexture2D, 0, null, screenTexture, 0);

                        //var frameParams = GetFrameParams(frameInfo);

                        UpdateMouseInfo(frameInfo);

                        var syncRoot = videoBuffer.syncRoot;
                        bool lockTaken = false;

                        try
                        {
                            Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                            if (lockTaken)
                            {
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

                                    Result = TextureToBitmap(screenTexture, videoBuffer.bitmap, rect);
                                }
                                */
                                
                                //var _rect = new GDI.Rectangle(0, 0, videoBuffer.bitmap.Width, videoBuffer.bitmap.Height);

                                Result = TextureToBitmap(screenTexture, videoBuffer.bitmap);

                                if (this.CaptureMouse && cursorObj.Visible)
                                {
                                    var desktop = videoBuffer.bitmap;
                                    GDI.Bitmap bmpCursor = null;
                                    try
                                    {
                                        bmpCursor = cursorObj.GetBitmap(desktop);

                                        if (bmpCursor != null)
                                        {
                                            GDI.Graphics g = null;
                                            try
                                            {
                                                g = GDI.Graphics.FromImage(desktop);
                                                int x = cursorObj.Position.X;
                                                int y = cursorObj.Position.Y;

                                                g.DrawImage(bmpCursor, x, y);
                                            }
                                            finally
                                            {
                                                g?.Dispose();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        bmpCursor?.Dispose();
                                    }
       
                                }

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

                    //if (acquireResult.Success)
                    //{
                    //    deskDupl.ReleaseFrame();
                    //}
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

            internal Bitmap GetBitmap(Bitmap desktop)
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

                    bmpCursor = new Bitmap(width, height, GDI.Imaging.PixelFormat.Format32bppArgb);
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
                var srcData = device3d11.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);

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
                device3d11.ImmediateContext.UnmapSubresource(texture, 0);
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

            if (screenTexture != null && !screenTexture.IsDisposed)
            {
                screenTexture.Dispose();
                screenTexture = null;
            }

            if (output != null && !output.IsDisposed)
            {
                output.Dispose();
                output = null;
            }

            if (adapter != null && !adapter.IsDisposed)
            {
                adapter.Dispose();
                adapter = null;
            }

            if (device3d11 != null && !device3d11.IsDisposed)
            {
                device3d11.Dispose();
                device3d11 = null;
            }

            if (dxgiFactory != null && !dxgiFactory.IsDisposed)
            {
                dxgiFactory.Dispose();
                dxgiFactory = null;
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
