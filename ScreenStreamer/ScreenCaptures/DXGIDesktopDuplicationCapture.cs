
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
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;

namespace ScreenStreamer
{
    public class DXGIDesktopDuplicationCapture : ScreenCapture
    {
        public DXGIDesktopDuplicationCapture(object[] args) : base()
        {

        }

        private Factory1 factory = new Factory1();
        private Device device = null;
        private Output output = null;
        private OutputDuplication duplicatedOutput = null;
        private Texture2D screenTexture = null;
        private Adapter1 adapter = null;

        public override void Init(GDI.Rectangle srcRect, GDI.Size destSize)
        {
            base.Init(srcRect, destSize);

            // # of graphics card adapter
            const int numAdapter = 0;

            // # of output device (i.e. monitor)
            const int numOutput = 0;

            Adapter1 adapter = factory.GetAdapter1(numAdapter);
            
            // Create device from Adapter
            device = new Device(adapter);

            // Get DXGI.Output
            output = adapter.GetOutput(numOutput);
            var output1 = output.QueryInterface<Output1>();
            

            // Width/Height of desktop to capture
            int width = ((SharpDX.Rectangle)output.Description.DesktopBounds).Width;
            int height = ((SharpDX.Rectangle)output.Description.DesktopBounds).Height;

            // Create Staging texture CPU-accessible
            var textureDesc = new Texture2DDescription
            {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Staging
            };
            
            screenTexture = new Texture2D(device, textureDesc);

            // Duplicate the output
            duplicatedOutput = output1.DuplicateOutput(device);

        }

        public override bool UpdateBuffer(int timeout = 10)
        {
            bool Result = false;

            try
            {
                SharpDX.DXGI.Resource screenResource;
                OutputDuplicateFrameInformation frameInfo;

                try
                {
                    // Try to get duplicated frame within given time
                    duplicatedOutput.AcquireNextFrame(500, out frameInfo, out screenResource);

                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    {
                        device.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
                    }
                    screenResource.Dispose();

                    var syncRoot = videoBuffer.syncRoot;
                    bool lockTaken = false;

                    try
                    {
                        Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                        if (lockTaken)
                        {
                            Result = TextureToBitmap(screenTexture, videoBuffer.bitmap);
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
                finally
                {
                    duplicatedOutput.ReleaseFrame();
                }

            }
            catch (SharpDXException ex)
            {
                logger.Error(ex);

                if (ex.ResultCode.Code != SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    throw ex;
                }
            }


            return Result;
        }

        private bool TextureToBitmap(Texture2D texture, GDI.Bitmap bmp)
        {
            bool success = false;
            try
            {
                var mapSource = device.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);

                int width = bmp.Width;
                int height = bmp.Height;

                var boundsRect = new System.Drawing.Rectangle(0, 0, width, height);

                // Copy pixels from screen capture Texture to GDI bitmap
                var mapDest = bmp.LockBits(boundsRect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                try
                {
                    var sourcePtr = mapSource.DataPointer;
                    var destPtr = mapDest.Scan0;
                    for (int y = 0; y < height; y++)
                    {
                        // Copy a single line 
                        Utilities.CopyMemory(destPtr, sourcePtr, width * 4);

                        // Advance pointers
                        sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                        destPtr = IntPtr.Add(destPtr, mapDest.Stride);
                    }

                    success = true;
                }
                finally
                {
                    bmp.UnlockBits(mapDest);
                }

            }
            finally
            {
                device.ImmediateContext.UnmapSubresource(texture, 0);
            }

            return success;
        }
        public override void Close()
        {
            base.Close();

            if (duplicatedOutput != null && !duplicatedOutput.IsDisposed)
            {
                duplicatedOutput.Dispose();
                duplicatedOutput = null;
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

            if (device != null && !device.IsDisposed)
            {
                device.Dispose();
                device = null;
            }

            if (factory != null && !factory.IsDisposed)
            {
                factory.Dispose();
                factory = null;
            }
        }
    }

}
