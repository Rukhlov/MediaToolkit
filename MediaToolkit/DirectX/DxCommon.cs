using MediaToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;

using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GDI = System.Drawing;

namespace MediaToolkit.DirectX
{
    public class WicTool
    {
        public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device)
        {
            using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
            {
                using (var bitmapSource = LoadBitmapSource(factory, fileName))
                {
                    var descr = new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                    };

                    return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
                }
            }
        }

        public static Texture2D CreateTexture2DFromBitmapFile(string fileName, SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr)
        {
            using (SharpDX.WIC.ImagingFactory2 factory = new SharpDX.WIC.ImagingFactory2())
            {
                using (var bitmapSource = LoadBitmapSource(factory, fileName))
                {
                    return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
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
            var descr = new SharpDX.Direct3D11.Texture2DDescription()
            {
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,

                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

            };

            return CreateTexture2DFromBitmapSource(device, descr, bitmapSource);
        }

        public static SharpDX.Direct3D11.Texture2D CreateTexture2DFromBitmapSource(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2DDescription descr, SharpDX.WIC.BitmapSource bitmapSource)
        {
            var bitmapSize = bitmapSource.Size;

            descr.Width = bitmapSize.Width;
            descr.Height = bitmapSize.Height;
            descr.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;

            int bitmapStride = bitmapSize.Width * 4;
            int bitmapLenght = bitmapSize.Height * bitmapStride;

            using (var buffer = new SharpDX.DataStream(bitmapLenght, true, true))
            {
                bitmapSource.CopyPixels(bitmapStride, buffer);

                var dataRect = new SharpDX.DataRectangle(buffer.DataPointer, bitmapStride);

                return new SharpDX.Direct3D11.Texture2D(device, descr, dataRect);
            }
        }

    }
    public class HlslCompiler
    {

        public static SharpDX.D3DCompiler.CompilationResult CompileShaderFromResources(string file, string entryPoint, string profile,
            SharpDX.Direct3D.ShaderMacro[] defines = null)
        {

            SharpDX.D3DCompiler.ShaderFlags flags = SharpDX.D3DCompiler.ShaderFlags.None;
#if DEBUG
            flags |= SharpDX.D3DCompiler.ShaderFlags.Debug | SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
#endif

            SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None;

            return CompileShaderFromResources(file, entryPoint, profile, flags, effectFlags);
        }

        private static SharpDX.D3DCompiler.CompilationResult CompileShaderFromResources(string file, string entryPoint, string profile,
            SharpDX.D3DCompiler.ShaderFlags shaderFlags = SharpDX.D3DCompiler.ShaderFlags.None,
            SharpDX.D3DCompiler.EffectFlags effectFlags = SharpDX.D3DCompiler.EffectFlags.None,
            SharpDX.Direct3D.ShaderMacro[] defines = null)
        {

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MediaToolkit.DirectX.Shaders." + file;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader ms = new StreamReader(stream))
                {
                    var shaderSource = ms.ReadToEnd();
                    var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderSource, entryPoint, profile, shaderFlags, effectFlags, defines, null, file);

                    return result;
                }
            }
        }
    }

    public class DxTool
    {

        public unsafe static Texture2D TextureFromDump(SharpDX.Direct3D11.Device device, Texture2DDescription descr, byte[] srcBuffer)
        {
            //descr.CpuAccessFlags = CpuAccessFlags.None;
            //descr.Usage = ResourceUsage.Default;
            //descr.BindFlags = SharpDX.Direct3D11.BindFlags.None;
            //descr.OptionFlags = ResourceOptionFlags.None;

            int width = descr.Width;
            int height = descr.Height;
            var format = descr.Format;

            int rowPitch = 0;
            int slicePitch = 0;
            if (format == Format.R8G8B8A8_UNorm)
            {
                rowPitch = 4 * width;
                slicePitch = rowPitch * height;

            }
            else if (format == Format.NV12)
            {// Width and height must be even.

                rowPitch = width;
                slicePitch = rowPitch * (height + height / 2);
            }
            else
            {// not supported...

            }

            fixed (byte* ptr = srcBuffer)
            {
                DataBox[] initData =
                {
                    new DataBox((IntPtr)ptr,  rowPitch, 0),
                };

                return new Texture2D(device, descr, initData);
            }

        }

        public static byte[] DumpTexture(SharpDX.Direct3D11.Device device, Texture2D texture)
        {
            byte[] destBuffer = null;

            var stagingDescr = texture.Description;
            stagingDescr.BindFlags = BindFlags.None;
            stagingDescr.CpuAccessFlags = CpuAccessFlags.Read;
            stagingDescr.Usage = ResourceUsage.Staging;
            stagingDescr.OptionFlags = ResourceOptionFlags.None;

            using (var stagingTexture = new Texture2D(device, stagingDescr))
            {
                device.ImmediateContext.CopyResource(texture, stagingTexture);

                var dataBox = device.ImmediateContext.MapSubresource(stagingTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                try
                {
                    int width = stagingDescr.Width;
                    int height = stagingDescr.Height;

                    var srcPitch = dataBox.RowPitch;
                    var srcDataSize = dataBox.SlicePitch;
                    var srcPtr = dataBox.DataPointer;

                    var destPitch = 4 * width;
                    var destRowNumber = height;
                    var destBufferSize = destPitch * destRowNumber;

                    if (stagingDescr.Format == Format.R8G8B8A8_SNorm || stagingDescr.Format == Format.B8G8R8A8_UNorm)
                    {

                    }
                    else if (stagingDescr.Format == Format.NV12)
                    {
                        destPitch = width;
                        destRowNumber = height + height / 2;
                        destBufferSize = destPitch * destRowNumber;
                    }
                    else if (stagingDescr.Format == Format.R8_UNorm)
                    {
                        destPitch = width;
                        destRowNumber = height;
                        destBufferSize = destPitch * destRowNumber;
                    }
                    else if (stagingDescr.Format == Format.R8G8_UNorm)
                    {
                        destPitch = 2 * width;
                        destRowNumber = height;
                        destBufferSize = destPitch * destRowNumber;
                    }

                    destBuffer = new byte[destBufferSize];
                    int bufOffset = 0;
                    for (int i = 0; i < destRowNumber; i++)
                    {
                        System.Runtime.InteropServices.Marshal.Copy(srcPtr, destBuffer, bufOffset, destPitch);
                        bufOffset += destPitch;
                        srcPtr += srcPitch;
                    }

                }
                finally
                {
                    device.ImmediateContext.UnmapSubresource(stagingTexture, 0);
                }

            }

            return destBuffer;
        }

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
                try
                {
                    var adaptDescr = _adapter.Description1;

                    var featureLevel = SharpDX.Direct3D11.Device.GetSupportedFeatureLevel(_adapter);
                    //var isSupported = SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(_adapter, SharpDX.Direct3D.FeatureLevel.Level_12_1);

                    bool success = GetSupportedFeatureLevel(_adapter, out var feature);

                    log.AppendLine("-------------------------------------");
                    log.AppendLine("#" + adapterIndex + " " + string.Join("| ", adaptDescr.Description, adaptDescr.DeviceId, adaptDescr.VendorId, featureLevel));

                    var outputs = _adapter.Outputs;

                    //foreach (var _output in _adapter.Outputs)
                    for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                    {
                        var _output = outputs[outputIndex];
                        try
                        {
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

                        }
                        finally
                        {
                            _output?.Dispose();
                        }

                    }
                }
                catch (SharpDXException ex)
                {
                    log.AppendLine(ex.Message);
                }
                finally
                {
                    _adapter?.Dispose();
                }

            }

            return log.ToString();
        }

        public static int GetDefaultAdapterFeatureLevel()
        {
            int featureLevel = 0;

            SharpDX.Direct3D.FeatureLevel[] features =
            {
                //SharpDX.Direct3D.FeatureLevel.Level_12_1,
                //SharpDX.Direct3D.FeatureLevel.Level_12_0,

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
				var flags = DeviceCreationFlags.None;
				//var flags = DeviceCreationFlags.BgraSupport;
				device = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, flags, features);
                featureLevel = (int)device.FeatureLevel;


            }
            catch (Exception ex) { }
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

    public class ColorSpaceHelper
    {
        static ColorSpaceHelper()
        {
            rgbToYuvDict = new Dictionary<ColorSpace, Dictionary<ColorRange, float[]>>
            {
                {
                    ColorSpace.BT601,
                    new Dictionary<ColorRange, float[]>
                    {
                        { ColorRange.Partial, RgbToYuv601 },
                        { ColorRange.Full, RgbToYuv601Full }
                    }
                },

                {
                    ColorSpace.BT709,
                    new Dictionary<ColorRange, float[]>
                    {
                        { ColorRange.Partial, RgbToYuv709 },
                        { ColorRange.Full, RgbToYuv709Full }
                    }
                }
            };

            yuvToRgbDict = new Dictionary<ColorSpace, Dictionary<ColorRange, float[]>>
            {
                {
                    ColorSpace.BT601,
                    new Dictionary<ColorRange, float[]>
                    {
                        { ColorRange.Partial, YuvToRgb601 },
                        { ColorRange.Full, YuvToRgb601Full }
                    }
                },

                {
                    ColorSpace.BT709,
                    new Dictionary<ColorRange, float[]>
                    {
                        { ColorRange.Partial, YuvToRgb709 },
                        { ColorRange.Full, YuvToRgb709Full }
                    }
                }
            };
        }

        private readonly static Dictionary<ColorSpace, Dictionary<ColorRange, float[]>> rgbToYuvDict = null;
        private readonly static Dictionary<ColorSpace, Dictionary<ColorRange, float[]>> yuvToRgbDict = null;

        public static Matrix GetRgbToYuvMatrix(ColorSpace colorSpace = ColorSpace.BT601, ColorRange colorRange = ColorRange.Partial)
        {
            var colorDict = rgbToYuvDict[colorSpace];
            return new Matrix(colorDict[colorRange]);
        }

        public static Matrix GetYuvToRgbMatrix(ColorSpace colorSpace = ColorSpace.BT601, ColorRange colorRange = ColorRange.Partial)
        {
            var colorDict = yuvToRgbDict[colorSpace];
            return new Matrix(colorDict[colorRange]);
        }


        public readonly static float[] RgbToYuv601 =
        {
             0.256788f,  0.504129f,  0.097906f,  0.062745f,
            -0.148223f, -0.290993f,  0.439216f,  0.501961f,
             0.439216f, -0.367788f, -0.071427f,  0.501961f,
             0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] RgbToYuv601Full =
        {
             0.299000f,  0.587000f,  0.114000f,  0.000000f,
            -0.168074f, -0.329965f,  0.498039f,  0.501961f,
             0.498039f, -0.417046f, -0.080994f,  0.501961f,
             0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] RgbToYuv709 =
        {
             0.182586f,  0.614231f,  0.062007f,  0.062745f,
            -0.100644f, -0.338572f,  0.439216f,  0.501961f,
             0.439216f, -0.398942f, -0.040274f,  0.501961f,
             0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] RgbToYuv709Full =
        {
             0.212600f,  0.715200f,  0.072200f,  0.000000f,
            -0.114123f, -0.383916f,  0.498039f,  0.501961f,
             0.498039f, -0.452372f, -0.045667f,  0.501961f,
             0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] YuvToRgb601 =
        {
            1.000000f,  0.000000f,  1.407520f, -0.706520f,
            1.000000f, -0.345491f, -0.716948f,  0.533303f,
            1.000000f,  1.778976f,  0.000000f, -0.892976f,
            0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] YuvToRgb601Full =
        {
            1.164384f,  0.000000f,  1.596027f, -0.874202f,
            1.164384f, -0.391762f, -0.812968f,  0.531668f,
            1.164384f,  2.017232f,  0.000000f, -1.085631f,
            0.000000f,  0.000000f,  0.000000f,  1.000000f
        };

        public readonly static float[] YuvToRgb709 =
        {
            1.164384f, 0.000000f, 1.792741f, -0.972945f,
            1.164384f, -0.213249f, -0.532909f, 0.301483f,
            1.164384f, 2.112402f, 0.000000f, -1.133402f,
            0.000000f, 0.000000f, 0.000000f, 1.000000f
        };

        public readonly static float[] YuvToRgb709Full =
        {
            1.000000f, 0.000000f, 1.581000f, -0.793600f,
            1.000000f, -0.188062f, -0.469967f, 0.330305f,
            1.000000f, 1.862906f, 0.000000f, -0.935106f,
            0.000000f, 0.000000f, 0.000000f, 1.000000f
        };

    }

}
