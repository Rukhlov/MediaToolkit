﻿using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDI = System.Drawing;

namespace MediaToolkit.DirectX
{
    public class DxTool
    {

        public static Adapter1 FindAdapter1(long luid)
        {
            Adapter1 adapter1 = null;
            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {

                if (luid > 0)
                {
                    var adapters = dxgiFactory.Adapters1;
                    for (int i = 0; i < adapters.Length; i++)
                    {
                        var _adapter = adapters[i];
                        if (_adapter.Description1.Luid == luid)
                        {
                            adapter1 = _adapter;
                            continue;
                        }

                        _adapter.Dispose();
                    }
                }

                if (adapter1 == null)
                {
                    adapter1 = dxgiFactory.GetAdapter1(0);
                }
            }

            return adapter1;
        }

        public static string LogDxInfo()
        {
            StringBuilder log = new StringBuilder();

            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                var adapters = dxgiFactory.Adapters1;

                var adapterInfo = LogDxAdapters(adapters);

                log.AppendLine(adapterInfo);

            }

            return log.ToString();
        }


        public static string LogDxAdapters(Adapter1[] adapters)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine("");

            //foreach (var _adapter in adapters)
            for (int adapterIndex = 0; adapterIndex < adapters.Length; adapterIndex++)
            {
                var _adapter = adapters[adapterIndex];

                var featureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(_adapter);
                //var isSupported = SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(_adapter, SharpDX.Direct3D.FeatureLevel.Level_12_1);

                bool success = GetSupportedFeatureLevel(_adapter, out var feature);

                var adaptDescr = _adapter.Description1;
                log.AppendLine("-------------------------------------");
                log.AppendLine("#" + adapterIndex + " " + string.Join("| ", adaptDescr.Description, adaptDescr.DeviceId, adaptDescr.VendorId, featureLevel));

                var outputs = _adapter.Outputs;

                //foreach (var _output in _adapter.Outputs)
                for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                {
                    var _output = outputs[outputIndex];

                    var outputDescr = _output.Description;
                    var bound = outputDescr.DesktopBounds;
                    var rect = new GDI.Rectangle
                    {
                        X = bound.Left,
                        Y = bound.Top,
                        Width = (bound.Right - bound.Left),
                        Height = (bound.Bottom - bound.Top),
                    };

                    log.AppendLine("#" + outputIndex + " " + string.Join("| ", outputDescr.DeviceName, rect.ToString()));

                    _output.Dispose();
                }

                _adapter.Dispose();
            }

            return log.ToString();
        }

        public static int GetDefaultAdapterFeatureLevel()
        {
            int featureLevel = 0;

            SharpDX.Direct3D.FeatureLevel[] features =
            {
                SharpDX.Direct3D.FeatureLevel.Level_12_1,
                SharpDX.Direct3D.FeatureLevel.Level_12_0,

                SharpDX.Direct3D.FeatureLevel.Level_11_1,
                SharpDX.Direct3D.FeatureLevel.Level_11_0,

                SharpDX.Direct3D.FeatureLevel.Level_10_1,
                SharpDX.Direct3D.FeatureLevel.Level_10_0,

                SharpDX.Direct3D.FeatureLevel.Level_9_3,
                SharpDX.Direct3D.FeatureLevel.Level_9_2,
                SharpDX.Direct3D.FeatureLevel.Level_9_1,
            };

            SharpDX.Direct3D11.Device device = null;
            try
            {
                var flags = DeviceCreationFlags.BgraSupport;
                device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags, features);
                featureLevel = (int)device.FeatureLevel;


            }
            catch (Exception) { }
            finally
            {
                if (device != null)
                {
                    device.Dispose();
                    device = null;
                }
            }

            return featureLevel;
        }

        public static bool GetSupportedFeatureLevel(Adapter adapter, out SharpDX.Direct3D.FeatureLevel outputLevel)
        {
            bool Result = false;
            outputLevel = SharpDX.Direct3D.FeatureLevel.Level_9_1;

            SharpDX.Direct3D.FeatureLevel[] features =
            {
                SharpDX.Direct3D.FeatureLevel.Level_12_1,
                SharpDX.Direct3D.FeatureLevel.Level_12_0,

                SharpDX.Direct3D.FeatureLevel.Level_11_1,
                SharpDX.Direct3D.FeatureLevel.Level_11_0,

                SharpDX.Direct3D.FeatureLevel.Level_10_1,
                SharpDX.Direct3D.FeatureLevel.Level_10_0,

                SharpDX.Direct3D.FeatureLevel.Level_9_3,
                SharpDX.Direct3D.FeatureLevel.Level_9_2,
                SharpDX.Direct3D.FeatureLevel.Level_9_1,
            };

            SharpDX.Direct3D11.Device device = null;
            try
            {
                device = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport, features);
                outputLevel = device.FeatureLevel;

                Result = true;

            }
            catch (Exception) { }
            finally
            {
                if (device != null)
                {
                    device.Dispose();
                    device = null;
                }
            }

            return Result;
        }

        //public static FeatureLevel GetSupportedFeatureLevel()
        //{
        //    SharpDX.Direct3D.FeatureLevel outputLevel;
        //    var device = new SharpDX.Direct3D11.Device(IntPtr.Zero);
        //    DeviceContext context;
        //    D3D11.CreateDevice(null, SharpDX.Direct3D.DriverType.Hardware, IntPtr.Zero, DeviceCreationFlags.None, null, 0, D3D11.SdkVersion, device, out outputLevel,
        //                       out context).CheckError();
        //    context.Dispose();
        //    device.Dispose();
        //    return outputLevel;
        //}

        public static void TextureToBitmap(Texture2D texture, ref GDI.Bitmap bmp)
        {

            var descr = texture.Description;
            if (descr.Format != Format.B8G8R8A8_UNorm)
            {
                throw new Exception("Invalid texture format " + descr.Format);
            }

            if (descr.Width <= 0 || descr.Height <= 0)
            {
                throw new Exception("Invalid texture size: " + descr.Width + " " + descr.Height);
            }

            if (bmp == null)
            {
                bmp = new GDI.Bitmap(descr.Width, descr.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
            }

            if (bmp.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                throw new Exception("Invalid bitmap format " + bmp.PixelFormat);

            }

            if (bmp.Width != descr.Width || bmp.Height != descr.Height)
            {
                throw new Exception("Invalid params");

            }

            using (Surface surface = texture.QueryInterface<Surface>())
            {
                try
                {
                    var srcData = surface.Map(SharpDX.DXGI.MapFlags.Read);

                    int width = bmp.Width;
                    int height = bmp.Height;
                    var rect = new GDI.Rectangle(0, 0, width, height);
                    var destData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                    try
                    {
                        int bytesPerPixel = GDI.Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                        IntPtr srcPtr = srcData.DataPointer;
                        int srcOffset = rect.Top * srcData.Pitch + rect.Left * bytesPerPixel;

                        srcPtr = IntPtr.Add(srcPtr, srcOffset);

                        var destPtr = destData.Scan0;
                        for (int row = rect.Top; row < rect.Bottom; row++)
                        {
                            Utilities.CopyMemory(destPtr, srcPtr, width * bytesPerPixel);
                            srcPtr = IntPtr.Add(srcPtr, srcData.Pitch);
                            destPtr = IntPtr.Add(destPtr, destData.Stride);

                        }
                    }
                    finally
                    {
                        bmp.UnlockBits(destData);
                    }
                }
                finally
                {
                    surface.Unmap();
                }
            }
        }

        public static byte[] GetTextureBytes(Texture2D texture)
        {
            byte[] textureBytes = null;
            using (Surface surface = texture.QueryInterface<Surface>())
            {
                try
                {
                    surface.Map(SharpDX.DXGI.MapFlags.Read, out DataStream dataStream);
                    textureBytes = dataStream.ReadRange<byte>((int)dataStream.Length);
                }
                finally
                {
                    surface.Unmap();
                }
            }

            return textureBytes;
        }


        public static Texture2D GetTexture(GDI.Bitmap bitmap, SharpDX.Direct3D11.Device device)
        {
            Texture2D texture = null;

            var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            if (bitmap.PixelFormat != GDI.Imaging.PixelFormat.Format32bppArgb)
            {
                var _bitmap = bitmap.Clone(rect, GDI.Imaging.PixelFormat.Format32bppArgb);

                bitmap.Dispose();
                bitmap = _bitmap;
            }

            var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            try
            {
                //Windows 7 
                var descr = new SharpDX.Direct3D11.Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    Format = Format.B8G8R8A8_UNorm,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,

                };

                /*
				var descr = new SharpDX.Direct3D11.Texture2DDescription
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
				};
				*/

                var dataRect = new SharpDX.DataRectangle(data.Scan0, data.Stride);

                texture = new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return texture;
        }
    }

}