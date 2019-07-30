
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
using SharpDX.Direct2D1;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;
using ScreenStreamer.Utils;

namespace ScreenStreamer
{
    public class DXGIDesktopDuplicationCapture : ScreenCapture
    {
        public DXGIDesktopDuplicationCapture(object[] args) : base()
        {

        }

        private SharpDX.DXGI.Factory1 dxgiFactory = null;
        private Device device3d11 = null;
        private Adapter1 adapter = null;
        private Output output = null;

        private OutputDuplication duplicatedOutput = null;
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
                duplicatedOutput = output1.DuplicateOutput(device3d11);
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

            deviceLost = false;
            frameAcquired = false;
        }

        private bool deviceLost = false;
        private bool frameAcquired = false;
        public override bool UpdateBuffer(int timeout = 10)
        {
            bool Result = false;

            try
            {
                if (deviceLost)
                {
                    InitDx(srcRect);
                    deviceLost = false;
                }

                SharpDX.DXGI.Resource screenResource = null;
                OutputDuplicateFrameInformation frameInfo = default(OutputDuplicateFrameInformation);

                //if (frameAcquired)
                //{

                //    duplicatedOutput.ReleaseFrame();
                //}



                 // Try to get duplicated frame within given time
                var result = duplicatedOutput.TryAcquireNextFrame(50, out frameInfo, out screenResource);
                

                if (result.Success)
                {
                    frameAcquired = true;

                    //...
                    //Process FrameInfo
                    if (frameInfo.LastPresentTime == 0)
                    {
                        logger.Debug("frameInfo.LastPresentTime == 0");
                        //return false;
                    }

                    using (var screenTexture2D = screenResource.QueryInterface<Texture2D>())
                    {
                        device3d11.ImmediateContext.CopyResource(screenTexture2D, screenTexture);
                        //device3d11.ImmediateContext.CopySubresourceRegion(screenTexture2D, 0, null, screenTexture, 0);
                    }
    
                    
                    var syncRoot = videoBuffer.syncRoot;
                    bool lockTaken = false;

                    try
                    {
                        Monitor.TryEnter(syncRoot, timeout, ref lockTaken);

                        if (lockTaken)
                        {
                            Result = TextureToBitmap(screenTexture, videoBuffer.bitmap);
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
                    

                    duplicatedOutput.ReleaseFrame();
                    screenResource.Dispose();
                }
                else
                {


                    frameAcquired = false;

                    if (result != SharpDX.DXGI.ResultCode.WaitTimeout)
                    {
                        result.CheckError();
                    }
                }


            }
            catch (SharpDXException ex)
            {
                frameAcquired = false;

                if (ex.ResultCode == SharpDX.DXGI.ResultCode.WaitTimeout)
                {
                    return false;
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
                    deviceLost = true;

                    //throw;
                    Thread.Sleep(100);
                }

            }


            return Result;
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
                var rect = new System.Drawing.Rectangle(0, 0, width, height);
                var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                try
                {
                    var srcPtr = srcData.DataPointer;
                    var destPtr = destData.Scan0;
                    for (int y = 0; y < height; y++)
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

}
