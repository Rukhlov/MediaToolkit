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
using System.Windows.Forms;
using SharpDX.Direct2D1;
using MediaToolkit.NativeAPIs;

namespace Test.Encoder.DDATest
{

    class DesktopDuplicator
    {
        internal DesktopDuplicator(DDAOutput duplOut)
        {
            this.duplOutput = duplOut;
        }

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.ScreenCaptures");

        private Device device = null;

        internal Rectangle drawRect = Rectangle.Empty;
        internal Rectangle duplRect = Rectangle.Empty;

        public ResourceRegion SrcRegion { get; private set; } = default(ResourceRegion);

        private Device destDevice = null;
        private bool copyToAnotherDevice = false;

        private Texture2D stagingTexture0 = null;
        private Texture2D stagingTexture1 = null;

        public Texture2D SharedTexture { get; private set; } = null;

        private DDAOutput duplOutput = null;

        public int ActivateCapture()
        {
            logger.Debug("DesktopDuplicator::ActivateCapture(...)");
            int activations = -1;
            if (duplOutput != null)
            {
                activations = duplOutput.Activate();
            }

            return activations;
        }

        public int DeactivateCapture()
        {
            logger.Debug("DesktopDuplicator::DeactivateCapture(...)");
            int activations = -1;
            if (duplOutput != null)
            {
                activations=  duplOutput.Deactivate();
            }

            return activations;
        }


        public void Init(Output output, Device device, GDI.Rectangle srcRect, Device destDevice = null)
        {
            logger.Debug("DesktopDuplicator::Init(...) " + srcRect.ToString());

            try
            {
                this.device = device;
                
                RawRectangle screenRect = output.Description.DesktopBounds;
                int width = screenRect.Right - screenRect.Left;
                int height = screenRect.Bottom - screenRect.Top;

                SetupRegions(screenRect, srcRect);

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



                if (destDevice != null)
                {
                    this.destDevice = destDevice;
                    copyToAnotherDevice = true;

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
        public ErrorCode TryGetScreenTexture(out ResourceRegion region, out Rectangle desRect, out Texture2D texture)
        {
            texture = null;
            region = this.SrcRegion;
            desRect = this.drawRect;

            ErrorCode Result = ErrorCode.Unexpected;
            if (!deviceReady)
            {
                return ErrorCode.NotReady;
            }

            try
            {

                var duplTexture = duplOutput.SharedTexture;
                if (duplTexture != null)
                {
                    using (var sharedRes = duplTexture.QueryInterface<SharpDX.DXGI.Resource>())
                    {
                        var handle = sharedRes.SharedHandle;
                        if (handle != IntPtr.Zero)
                        {
                            using (var sharedTex = device.OpenSharedResource<Texture2D>(handle))
                            {
                                device.ImmediateContext.CopyResource(sharedTex, SharedTexture);
                            }
                        }
                    }
                }
                else
                {// not ready ... 

                }

                if (!copyToAnotherDevice)
                {
                    texture = SharedTexture;
                }
                else
                {

                    device.ImmediateContext.CopyResource(SharedTexture, stagingTexture0);

                    DataBox srcBox = device.ImmediateContext.MapSubresource(stagingTexture0, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                    try
                    {
                        DataBox destBox = destDevice.ImmediateContext.MapSubresource(stagingTexture1, 0, MapMode.Write, SharpDX.Direct3D11.MapFlags.None);
                        try
                        {
                            var srcPitch = srcBox.SlicePitch;
                            var destPitch = destBox.SlicePitch;
                            if (srcPitch != destPitch)
                            {//...
                                logger.Warn("SlicePitch not same " + srcPitch + "!=" + destPitch);
                            }
                            Kernel32.CopyMemory(destBox.DataPointer, srcBox.DataPointer, (uint)destBox.SlicePitch);
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




        public void Close()
        {
            deviceReady = false;

            if (SharedTexture != null && !SharedTexture.IsDisposed)
            {
                SharedTexture.Dispose();
                SharedTexture = null;
            }

            if (stagingTexture0 != null && !stagingTexture0.IsDisposed)
            {
                stagingTexture0.Dispose();
                stagingTexture0 = null;
            }

            if (stagingTexture1 != null && !stagingTexture1.IsDisposed)
            {
                stagingTexture1.Dispose();
                stagingTexture1 = null;
            }
        }

    }



}
