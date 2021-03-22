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
using SharpDX.Direct2D1;
using MediaToolkit.DirectX;

namespace MediaToolkit.ScreenCaptures
{

    class DDAOutputProvider
    {
        internal DDAOutputProvider(DDAOutput duplOut)
        {
            this.duplOutput = duplOut;
        }

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.ScreenCaptures");

        private Device device = null;

        internal Rectangle drawRect = Rectangle.Empty;
        internal Rectangle duplRect = Rectangle.Empty;

        public ResourceRegion SrcRegion { get; private set; } = default(ResourceRegion);

        private Device destDevice = null;
        private bool copyToAnotherGPU = false;

        private Texture2D stagingTexture0 = null;
        private Texture2D stagingTexture1 = null;

        private Texture2D screenTexture = null;
        private RenderTarget screenTarget = null;

        internal DDAOutput duplOutput = null;

        public bool CaptureMouse { get; set; }

        public int ActivateCapture()
        {
            logger.Debug("DDAOutputProvider::ActivateCapture(...)");
            int activations = -1;
            if (duplOutput != null)
            {
                activations = duplOutput.Activate();
            }

            return activations;
        }

        public int DeactivateCapture()
        {
            logger.Debug("DDAOutputProvider::DeactivateCapture(...)");
            int activations = -1;
            if (duplOutput != null)
            {
                activations = duplOutput.Deactivate();
            }

            return activations;
        }


        public void Init(Output output, Device device, GDI.Rectangle srcRect, Device destDevice = null)
        {
            logger.Debug("DDAOutputProvider::Init(...) " + srcRect.ToString());

            try
            {
                this.device = device;

                RawRectangle screenRect = output.Description.DesktopBounds;
                //int width = screenRect.Right - screenRect.Left;
                //int height = screenRect.Bottom - screenRect.Top;

                SetupRegions(screenRect, srcRect);

                int width = SrcRegion.Right - SrcRegion.Left;
                int height = SrcRegion.Bottom - SrcRegion.Top;

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


                if (destDevice != null)
                {
                    this.destDevice = destDevice;
                    copyToAnotherGPU = true;

                    stagingTexture0 = new Texture2D(device,
                      new SharpDX.Direct3D11.Texture2DDescription
                      {
                          Width = width,
                          Height = height,
                          MipLevels = 1,
                          ArraySize = 1,
                          SampleDescription = new SampleDescription(1, 0),
                          Usage = ResourceUsage.Staging,
                          Format = Format.B8G8R8A8_UNorm,
                          //BindFlags = BindFlags.ShaderResource,
                          CpuAccessFlags = CpuAccessFlags.Read,
                          OptionFlags = ResourceOptionFlags.None,

                      });

                    stagingTexture1 = new Texture2D(this.destDevice,
                        new SharpDX.Direct3D11.Texture2DDescription
                        {
                            Width = width,
                            Height = height,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDescription = new SampleDescription(1, 0),
                            Usage = ResourceUsage.Staging,
                            Format = Format.B8G8R8A8_UNorm,
                            CpuAccessFlags = CpuAccessFlags.Write | CpuAccessFlags.Read,
                            OptionFlags = ResourceOptionFlags.None,

                        });
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex);

                Close();

                throw;
            }

            deviceReady = true;

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
            
            SrcRegion = new ResourceRegion
            {
                Left = duplRect.Left,
                Top = duplRect.Top,
                Right = duplRect.Right,
                Bottom = duplRect.Bottom,
                Back = 1,
            };

            logger.Debug("duplRect=" + duplRect.ToString() + " drawRect=" + drawRect.ToString());

        }

        private bool deviceReady = false;
        public ErrorCode TryGetScreenTexture(out Rectangle desRect, out Texture2D texture)
        {
            texture = null;
            desRect = this.drawRect;

            ErrorCode Result = ErrorCode.Unexpected;
            if (!deviceReady)
            {
                return ErrorCode.NotReady;
            }

            try
            {
                var sharedHandle = duplOutput.GetSharedHandle();
                if (sharedHandle == IntPtr.Zero)
                {
                    return ErrorCode.NotReady;
                }

                using (var sharedTex = device.OpenSharedResource<Texture2D>(sharedHandle))
                {
                    device.ImmediateContext.CopySubresourceRegion(sharedTex, 0, SrcRegion, screenTexture, 0);
                }

                if (CaptureMouse)
                {
                    CursorFrame cursorFrame = null;
                    try
                    {
                        if (duplOutput.GetCursorFrame(out cursorFrame))
                        {
                            if (cursorFrame.Visible)
                            {
                                DrawCursor(cursorFrame);
                            }
                        }
                    }
                    finally
                    {
                        if (cursorFrame != null)
                        {
                            cursorFrame.Dispose();
                            cursorFrame = null;
                        }
                    }
                }


                if (!copyToAnotherGPU)
                {
                    texture = screenTexture;
                }
                else
                {// копируем через CPU :(

                    device.ImmediateContext.CopyResource(screenTexture, stagingTexture0);

                    DataBox srcBox = device.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                    try
                    {
                        DataBox destBox = destDevice.ImmediateContext.MapSubresource(stagingTexture1, 0, MapMode.Write, SharpDX.Direct3D11.MapFlags.None);
                        try
                        {
                            var descr0 = stagingTexture0.Description;
                            int width0 = descr0.Width;
                            int height0 = descr0.Height;

                            var descr1 = stagingTexture1.Description;
                            int width1 = descr1.Width;
                            int height1 = descr1.Height;

                            if(width0!= width1 || height0 != height1)
                            {
                                logger.Error("Size not same " + width0 + "!=" + width1 + " || " + height0 + "!=" + height1);
                                // TODO:...
                            }
  
                            var srcPitch = srcBox.RowPitch;
                            var destPitch = destBox.RowPitch;
                            var srcPtr = srcBox.DataPointer;
                            var destPtr = destBox.DataPointer;
 
                            if (srcPitch != destPitch)
                            {
                               // logger.Debug("Pitch not same " + srcPitch + "!=" + destPitch);

                                for(int i= 0; i < height1; i++)
                                {
                                    Utilities.CopyMemory(destPtr, srcPtr, destPitch);
                                    srcPtr = IntPtr.Add(srcPtr, srcPitch);
                                    destPtr = IntPtr.Add(destPtr, destPitch);
                                }
                            }
                            else
                            {
                                Kernel32.CopyMemory(destBox.DataPointer, srcBox.DataPointer, (uint)destBox.SlicePitch);
                            }
                           
                        }
                        finally
                        {
                            destDevice.ImmediateContext.UnmapSubresource(stagingTexture1, 0);
                        }
                    }
                    finally
                    {
                        device.ImmediateContext.UnmapSubresource(stagingTexture0, 0);
                    }

                    texture = stagingTexture1;
                }

                //texture = duplTexture;

                Result = 0;

            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);

            }

            return Result;
        }


        private unsafe void DrawCursor(CursorFrame cursorFrame)
        {
            if (!cursorFrame.Visible)
            {
                //logger.Debug("No cursor");
                return;
            }

            var position = cursorFrame.Location;

            var shapePtr = cursorFrame.Ptr;
            var shapeInfo = cursorFrame.Type;

            int width = cursorFrame.Size.Width;
            int height = cursorFrame.Size.Height;
            int pitch = cursorFrame.Pitch;

            int left = position.X;
            int top = position.Y;
            int right = position.X + width;
            int bottom = position.Y + height;


            if (cursorFrame.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {

                var data = new DataPointer(shapePtr, height * pitch);
                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                var size = new Size2(width, height);
                var cursorBits = new Direct2D.Bitmap(screenTarget, size, data, pitch, prop);
                try
                {
                    var cursorRect = new RawRectangleF(left, top, right, bottom);
                    screenTarget.BeginDraw();
                    screenTarget.DrawBitmap(cursorBits, cursorRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);
                    screenTarget.EndDraw();

                }
                finally
                {
                    cursorBits?.Dispose();
                }
            }
            else if (cursorFrame.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_MONOCHROME)
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
                            //var desktopBuffer = new byte[width * height * 4];
                            //Marshal.Copy(dataBox.DataPointer, desktopBuffer, 0, desktopBuffer.Length);

                            fixed (byte* ptr = DDAShapeBuffer.GetMonochromeShape(shapePtr, dataBox.DataPointer, new GDI.Size(width, height)))
                            {
                                var data = new DataPointer(ptr, height * pitch);
                                var prop = new Direct2D.BitmapProperties(new Direct2D.PixelFormat(Format.B8G8R8A8_UNorm, Direct2D.AlphaMode.Premultiplied));
                                var size = new Size2(width, height);
                                cursorBits = new Direct2D.Bitmap(screenTarget, size, data, pitch, prop);
                                var shapeRect = new RawRectangleF(left, top, right, bottom);

                                screenTarget.BeginDraw();
                                screenTarget.DrawBitmap(cursorBits, shapeRect, 1.0f, Direct2D.BitmapInterpolationMode.Linear);
                                screenTarget.EndDraw();
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
            else if (cursorFrame.Type == (int)ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR)
            {
                logger.Warn("Not supported cursor type " + ShapeType.DXGI_OUTDUPL_POINTER_SHAPE_TYPE_COLOR);
            }

        }


        public void Close()
        {
            logger.Debug("DDAOutputProvider::Close()");

            deviceReady = false;

            DxTool.SafeDispose(screenTarget);
            DxTool.SafeDispose(screenTexture);

            DxTool.SafeDispose(stagingTexture0);
            DxTool.SafeDispose(stagingTexture1);

        }

    }

}
