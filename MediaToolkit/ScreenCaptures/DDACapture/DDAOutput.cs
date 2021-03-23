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
using MediaToolkit.Logging;

using System.Windows.Forms;
using SharpDX.Direct2D1;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using MediaToolkit.DirectX;

namespace MediaToolkit.ScreenCaptures
{
    public enum DDAOutputState
    {
        Initialized,
        InvalidArgs,
        AccessDenied,
        Unsupported,
        NotCurrentlyAvailable,
        SessionDisconnected,
        Closed,
    }

    public class DDAOutput
    {
        private static TraceSource logger = TraceManager.GetTrace("DesktopDuplicator");
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private Device device = null;
        private Output output = null;

        private OutputDuplication outuptDupl = null;

        public Texture2D SharedTexture { get; private set; } = null;

        private RgbProcessor textureProcessor = null;
        public IntPtr GetSharedHandle()
        {
            IntPtr handle = IntPtr.Zero;
            if (SharedTexture != null)
            {
                using (var sharedRes = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
                {
                    return sharedRes.SharedHandle;
                }
            }
            return IntPtr.Zero;
        }

        private DDAShapeBuffer shapeBuffer = null;

        public DDAOutputState OutputState { get; private set; } = DDAOutputState.Closed;

        public event Action FrameAcquired;
        public event Action Activated;
        public event Action Deactivated;

        public Tuple<int, int> FrameRate { get; set; } = new Tuple<int, int>(1, 60);

        private bool initialized = false;

        public int AdapterIndex { get; private set; } = 0;
        public int OutputIndex { get; private set; } = 0;
        public RawRectangle DesktopRect { get; private set; }

        public void Init(int adapterIndex, int outputIndex)
        {
            logger.Debug("DesktopDuplicator::Init(...) " + adapterIndex + " " + outputIndex);

            if (initialized || activations > 0)
            {
                logger.Error("Invalid state " + initialized + " " + activations);
                //...
            }

            this.AdapterIndex = adapterIndex;
            this.OutputIndex = outputIndex;

            var dxgiFactory = new SharpDX.DXGI.Factory1();
            try
            {
                var adapter = dxgiFactory.GetAdapter(AdapterIndex);
                try
				{
					this.output = adapter.GetOutput(OutputIndex);

					var _descr = output.Description;
					this.DesktopRect = _descr.DesktopBounds;

					//this.device = device;
					device = DxTool.CreateMultithreadDevice(adapter);
				}
				finally
                {
                    adapter.Dispose();
                }


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

				InitDuplicator();
                shapeBuffer = new DDAShapeBuffer();

                textureProcessor = new RgbProcessor();

                var srcSize = new GDI.Size(width, height);
                var outputDesrc = outuptDupl.Description;
                var destSize = new GDI.Size(width, height);
                if (outputDesrc.Rotation == DisplayModeRotation.Rotate270 
                    || outputDesrc.Rotation == DisplayModeRotation.Rotate90)
                {
                    destSize = new GDI.Size(height, width);
                }

                textureProcessor.Init(device, srcSize, Core.PixFormat.RGB32, srcSize, Core.PixFormat.RGB32, Core.ScalingFilter.FastLinear);

                initialized = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();
                throw;
            }
            finally
            {
                dxgiFactory.Dispose();
            }
        }



		internal void InitDuplicator()
        {

            logger.Debug("InitDuplicator()");

            try
            {
                using (var output1 = output.QueryInterface<Output1>())
                {
                    try
                    {// Duplicate the output
                     //https://docs.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutput1-duplicateoutput
                        outuptDupl = output1.DuplicateOutput(device);

                        OutputState = DDAOutputState.Initialized;

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
                            OutputState = DDAOutputState.InvalidArgs;
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
                            OutputState = DDAOutputState.Unsupported;

                        }
                        else if (ex.ResultCode == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable)
                        {
                            /*
                                * if DXGI reached the limit on the maximum number of concurrent duplication applications (default of four).
                                * Therefore, the calling application cannot create any desktop duplication interfaces until the other applications close.
                                */

                            OutputState = DDAOutputState.NotCurrentlyAvailable;
                        }
                        else if (ex.ResultCode == SharpDX.DXGI.ResultCode.SessionDisconnected)
                        {
                            //if DuplicateOutput failed because the session is currently disconnected.
                            OutputState = DDAOutputState.SessionDisconnected;
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


        public ErrorCode CaptureScreen(int timeout = 0)
        {
            ErrorCode Result = ErrorCode.Unexpected;
            if (!initialized)
            {
                return ErrorCode.NotInitialized;
            }

            if (OutputState != DDAOutputState.Initialized)
            {
                return ErrorCode.NotReady;
            }

            try
            {
                SharpDX.DXGI.Resource desktopResource = null;
                try
                {
                    //https://docs.microsoft.com/en-us/windows/win32/api/dxgi1_2/nf-dxgi1_2-idxgioutputduplication-acquirenextframe
                    var acquireResult = outuptDupl.TryAcquireNextFrame(timeout, out OutputDuplicateFrameInformation frameInfo, out desktopResource);

                    //logger.Debug("TryAcquireNextFrame() " + acquireResult);

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

                        var outputDescr = outuptDupl.Description;
                        var screenDescr = SharedTexture.Description;
                        if (outputDescr.Rotation == DisplayModeRotation.Identity 
                            || outputDescr.Rotation == DisplayModeRotation.Unspecified)
                        {
                            if(outputDescr.ModeDescription.Width == screenDescr.Width 
                                && outputDescr.ModeDescription.Height == screenDescr.Height)
                            {
                                device.ImmediateContext.CopyResource(duplTexture, SharedTexture);
                            }
                            else
                            {
                                textureProcessor.DrawTexture(duplTexture, SharedTexture, false);
                            }

                        }
                        else if(outputDescr.Rotation == DisplayModeRotation.Rotate90)
                        {
                            textureProcessor.DrawTexture(duplTexture, SharedTexture, false, DirectX.Transform.R90);
                        }
                        else if (outputDescr.Rotation == DisplayModeRotation.Rotate180)
                        {

                            textureProcessor.DrawTexture(duplTexture, SharedTexture, false, DirectX.Transform.R180);
                        }
                        else if (outputDescr.Rotation == DisplayModeRotation.Rotate270)
                        {
                            textureProcessor.DrawTexture(duplTexture, SharedTexture, false, DirectX.Transform.R270);
                        }

                    }

                    Result = 0;
                }
                finally
                {
                    if (desktopResource != null)
                    {
                        outuptDupl.ReleaseFrame();

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
                outuptDupl.GetFrameDirtyRects(bufferSizeBytes, dirtyRectsBuffer, out dirtyRectsBufferSizeRequired);
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
                    outuptDupl.GetFrameMoveRects(bufferSizeBytes, moveRectBuf, out moveRectsBufferSizeRequired);
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

                lock (shapeBuffer.SyncObj)
                {
                    shapeBuffer.Update(frameInfo);

                    if (frameInfo.PointerShapeBufferSize > 0)
                    {
                        try
                        {
                            var size = shapeBuffer.BufferSize;
                            var ptr = shapeBuffer.PtrShapeBuffer;

                            outuptDupl.GetFramePointerShape(size, ptr,
                                out int pointerShapeBufferSizeRequired,
                                out OutputDuplicatePointerShapeInformation pointerShapeInfo);

                            shapeBuffer.ShapeInfo = pointerShapeInfo;

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

        }

        #endregion


        public bool ResetDuplicator()
        {
            logger.Debug("ResetDuplicator()");
            try
            {
                CloseDuplicator();
                InitDuplicator();

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            return (OutputState == DDAOutputState.Initialized);
        }

        private void CloseDuplicator()
        {
            logger.Debug("CloseDuplicator()");
            if (outuptDupl != null && !outuptDupl.IsDisposed)
            {
                outuptDupl.Dispose();
                outuptDupl = null;
            }

            OutputState = DDAOutputState.Closed;
        }

		private volatile int activations = 0;
		private Task captureTask = null;

		public int Activate()
		{

			logger.Debug("Activate() ");
			lock (syncObj)
			{
				activations++;
			}

			if (activations > 1)
			{
				logger.Debug("Activate() == false" + activations);

				return activations;
			}

			captureTask = Task.Run(() =>
			{
				if (activations > 0)
				{
					Activated?.Invoke();
				}

				AutoResetEvent syncEvent = new AutoResetEvent(false);
				Stopwatch sw = new Stopwatch();
				int interval = (int)(FrameRate.Item1 * 1000.0 / FrameRate.Item2);

				while (activations > 0)
				{
					sw.Restart();

					if (OutputState != DDAOutputState.Initialized)
					{
						Thread.Sleep(100);
						ResetDuplicator();
						continue;
					}

					if (OutputState == DDAOutputState.Initialized)
					{
						int timeout = 0;
						var res = CaptureScreen(timeout);
						if (res == ErrorCode.Ok)
						{
							FrameAcquired?.Invoke();
						}
						else
						{
							OutputState = DDAOutputState.Closed;
							continue;
						}
					}

					var mSec = sw.ElapsedMilliseconds;
					var delay = (int)(interval - mSec);
					if (delay > 0)
					{
						syncEvent.WaitOne(delay);
						// logger.Debug(delay);
					}
				}

				if (syncEvent != null)
				{
					syncEvent.Dispose();
					syncEvent = null;

				}

				Deactivated?.Invoke();
			});

			return activations;
		}

		private object syncObj = new object();
		public int Deactivate()
		{
			logger.Debug("Deactivate()");
			lock (syncObj)
			{
				activations--;
			}

			return activations;

		}

		public void Close(bool force = false)
        {
            logger.Debug("Close()");

            activations = 0;

            if (captureTask != null)
            {
                if (captureTask.Status == TaskStatus.Running)
                {
                    bool waitResult = false;
                    do
                    {
                        waitResult = captureTask.Wait(1000);
                        if (!waitResult)
                        {
                            logger.Warn("DDAOutput::Close() " + waitResult);
                        }
                    } while (!waitResult);
                }

                captureTask.Dispose();
                captureTask = null;
            }


            CloseDuplicator();

            DxTool.SafeDispose(device);
            DxTool.SafeDispose(output);
            DxTool.SafeDispose(SharedTexture);

            if (shapeBuffer != null)
            {
                shapeBuffer.Dispose();
                shapeBuffer = null;
            }

            if (textureProcessor != null)
            {
                textureProcessor.Close();
                textureProcessor = null;
            }

            initialized = false;

        }


        public bool GetCursorFrame(out CursorFrame cursorFrame)
        {
            bool result = false;
            cursorFrame = null;

            if (shapeBuffer != null)
            {
                lock (shapeBuffer.SyncObj)
                {
                    if (shapeBuffer.Visible)
                    {
                        var shapeBuff = shapeBuffer.PtrShapeBuffer;
                        int dataSize = shapeBuffer.BufferSize;

                        IntPtr destPtr = Marshal.AllocHGlobal(dataSize);
                        Kernel32.CopyMemory(destPtr, shapeBuff, (uint)dataSize);

                        var shapeInfo = shapeBuffer.ShapeInfo;
                        var position = shapeBuffer.Position;
                        cursorFrame = new CursorFrame
                        {
                            Ptr = destPtr,
                            DataSize = dataSize,
                            Pitch = shapeInfo.Pitch,
                            Location = new GDI.Point(position.X, position.Y),
                            Size = new GDI.Size(shapeInfo.Width, shapeInfo.Height),
                            Visible = shapeBuffer.Visible,
                            Type = shapeInfo.Type,
                        };


                        result = true;
                    }
                }
            }

            return result;
        }
    }

    public class CursorFrame
    {
        public GDI.Point Location = GDI.Point.Empty;
        public GDI.Size Size = GDI.Size.Empty;

        public int Pitch = 0;
        public IntPtr Ptr = IntPtr.Zero;
        public int DataSize = 0;

        public int Type = 0;
        public bool Visible = false;

        public void Dispose()
        {
            if (Ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
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

    class DDAShapeBuffer
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

        public object SyncObj = new object();

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

        public void Alloc(int bufferSize)
        {
            FreeBuffer();

            this.BufferSize = bufferSize;
            PtrShapeBuffer = Marshal.AllocHGlobal(BufferSize);
        }

        public static unsafe byte[] GetMonochromeShape(IntPtr shapePtr, IntPtr desktopPtr, GDI.Size size)
        {
            int width = size.Width;
            int height = size.Height;

            var shapeBufferLenght = width * height * 4;
            var shapeBuffer = new byte[shapeBufferLenght];

            var maskBufferLenght = width * height / 8;
            var andMaskBuffer = new byte[maskBufferLenght];
            Marshal.Copy(shapePtr, andMaskBuffer, 0, andMaskBuffer.Length);

            var xorMaskBuffer = new byte[maskBufferLenght];
            Marshal.Copy((shapePtr + andMaskBuffer.Length), xorMaskBuffer, 0, xorMaskBuffer.Length);

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

                        shapeBuffer[pos] = (byte)((*(byte*)(desktopPtr + pos) & andMask) ^ xorMask);
                        pos++;
                    }
                    // Alpha

                    shapeBuffer[pos] = (byte)((*(byte*)(desktopPtr + pos) & 0xFF) ^ 0);

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

        public byte[] GetMonochromeShape(IntPtr desktopPtr, GDI.Size size)
        {
            return GetMonochromeShape(PtrShapeBuffer, desktopPtr, size);
        }


        public unsafe GDI.Bitmap GetGdiBitmap(GDI.Bitmap desktop)
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

                fixed (byte* desktopPtr = desktopBuffer)
                {
                    byte[] shapeBuffer = GetMonochromeShape((IntPtr)desktopPtr, new GDI.Size(width, height));

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


            }
            else if (ShapeInfo.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MASKED_COLOR)
            {
                //...
                // logger.Warn("Not supported");
            }

            return bmpCursor;
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



}
