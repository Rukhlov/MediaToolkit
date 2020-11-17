using MediaToolkit.NativeAPIs;
using SharpDX.Direct3D11;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDI = System.Drawing;


namespace Test.Encoder
{
    public class TextureLoader
    {

        public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device)
        {
            using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
            {
                using (var bitmapSource = LoadBitmapSource(factory, fileName))
                {
                    return CreateTexture2DFromBitmapSource(device, bitmapSource);
                }
            }
        }

        public static SharpDX.WIC.BitmapSource LoadBitmapSource(SharpDX.WIC.ImagingFactory2 factory, string filename)
        {
            using (var bitmapDecoder = new SharpDX.WIC.BitmapDecoder(factory, filename, SharpDX.WIC.DecodeOptions.CacheOnDemand))
            {
                var formatConverter = new SharpDX.WIC.FormatConverter(factory);

                using (var frameDecode = bitmapDecoder.GetFrame(0))
                {
                    formatConverter.Initialize(frameDecode,
                        SharpDX.WIC.PixelFormat.Format32bppPRGBA,
                        SharpDX.WIC.BitmapDitherType.None,
                        null,
                        0.0,
                        SharpDX.WIC.BitmapPaletteType.Custom);
                }

                return formatConverter;
            }
        }

        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.WIC.BitmapSource bitmapSource)
        {
            var bitmapSize = bitmapSource.Size;

            int bitmapStride = bitmapSize.Width * 4;
            int bitmapLenght = bitmapSize.Height * bitmapStride;

            using (var buffer = new SharpDX.DataStream(bitmapLenght, true, true))
            {
                bitmapSource.CopyPixels(bitmapStride, buffer);

                var dataRect = new SharpDX.DataRectangle(buffer.DataPointer, bitmapStride);

                //GDI.Bitmap bitmap = new GDI.Bitmap(size.Width, size.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
                //var rect = new GDI.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                //var data = bitmap.LockBits(rect, GDI.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                //Kernel32.CopyMemory(data.Scan0, buffer.DataPointer, (uint)dataLen);
                //bitmap.Save(@"d:\test23424.jpg");
                //bitmap.UnlockBits(data);

                var descr = new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = bitmapSize.Width,
                    Height = bitmapSize.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                };

                return new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }


        }
    }
}
