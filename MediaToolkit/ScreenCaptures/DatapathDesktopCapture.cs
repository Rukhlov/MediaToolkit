using MediaToolkit.Core;

using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MediaToolkit.NativeAPIs;
using System.Diagnostics;
using MediaToolkit.Logging;
using MediaToolkit.SharedTypes;
using SharpDX.Direct3D11;
using MediaToolkit.DirectX;

namespace MediaToolkit.ScreenCaptures
{

    public class DatapathDesktopCapture : ScreenCapture
    {

        private static TraceSource _logger = TraceManager.GetTrace("MediaToolkit.ScreenCaptures");

        public static bool Initialized { get; private set; }
        public static bool Load()
        {
            _logger.Debug("Load()");
            if (!Initialized)
            {
                try
                {
                    var result = DCapt.DCaptLoad(out hLoad);

                    if (result == DCapt.CaptError.DESKCAPT_ERROR_API_ALREADY_LOADED)
                    {
                        //...

                    }

                    DCapt.ThrowIfError(result, "DCaptLoad");

                    Initialized = true;
                }
                catch (DllNotFoundException)
                {
                    _logger.Info("Datapath capture not found");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            return Initialized;
        }

        public static void Unload()
        {
            _logger.Debug("Unload()");

            try
            {
                if (hLoad != IntPtr.Zero)
                {
                    DCapt.DCaptFree(hLoad);
                    hLoad = IntPtr.Zero;
                    Initialized = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private static IntPtr hLoad = IntPtr.Zero;
        private IntPtr hCapt = IntPtr.Zero;

        // private BITMAPINFO bitmapInfo = default(BITMAPINFO);

        //private FrameBuffer srcBuffer = null;
        private VideoFrame dcaptFrame = null;
        private D3D11VideoFrame srcFrame = null;

        public override void Init(Rectangle captArea, Size destSize)
        {
            logger.Debug("Init(...) " + captArea.ToString() + " " + destSize.ToString());

            if (!Initialized)
            {
                if (!Load())
                {
                    throw new InvalidOperationException("DCapt not initialized");
                }
            }

            try
            {
                this.SrcRect = captArea;
                this.DestSize = destSize;

                InitCapture(captArea, DestSize);

                InitDx();

                if (DriverType == VideoDriverType.CPU)
                {
                    this.VideoBuffer = new MemoryVideoBuffer(DestSize, DestFormat, 16);
                }
                else if(DriverType == VideoDriverType.D3D11)
                {
                    this.VideoBuffer = new D3D11VideoBuffer(device, DestSize, DestFormat);
                }
                else
                {
                    throw new InvalidOperationException("Invalid driver type: " + DriverType);
                }


                pixConverter = new D3D11RgbToYuvConverter();
                pixConverter.KeepAspectRatio = AspectRatio;
                pixConverter.Init(device, SrcRect.Size, SrcFormat, DestSize, DestFormat, DownscaleFilter);


                srcFrame = new D3D11VideoFrame(dcaptFrame.Format, srcTexture);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                Close();
                throw;
            }

        }

        private int AdapterIndex = 0;
        private SharpDX.Direct3D11.Device device = null;
        private Texture2D srcTexture = null;
        private D3D11RgbToYuvConverter pixConverter = null;

        private void InitDx()
        {
            logger.Debug("DatapathDesktopCapture::InitDx()");

            SharpDX.DXGI.Factory1 dxgiFactory = null;

            try
            {
                dxgiFactory = new SharpDX.DXGI.Factory1();
                SharpDX.DXGI.Adapter1 adapter = null;
                try
                {
                    adapter = dxgiFactory.GetAdapter1(AdapterIndex);
                    //AdapterId = adapter.Description.Luid;
                    //logger.Info("Screen source info: " + adapter.Description.Description + " " + output.Description.DeviceName);

                    var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
#if DEBUG
                    //deviceCreationFlags |= DeviceCreationFlags.Debug;
#endif
                    device = new Device(adapter, deviceCreationFlags);
                    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
                }
                finally
                {
                    if (adapter != null)
                    {
                        adapter.Dispose();
                        adapter = null;
                    }
                }
            }
            finally
            {
                if (dxgiFactory != null)
                {
                    dxgiFactory.Dispose();
                    dxgiFactory = null;
                }
            }

            srcTexture = new Texture2D(device,
                new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Write,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = SharpDX.DXGI.Format.D16_UNorm,
                    Width = SrcRect.Width,
                    Height = SrcRect.Height,

                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Staging,
                    OptionFlags = ResourceOptionFlags.None,

                });

        }

        private void CloseDx()
        {
            if(srcTexture != null)
            {
                srcTexture.Dispose();
                srcTexture = null;
            }

            if (device != null)
            {
                device.Dispose();
                device = null;
            }     
        }

        private void InitCapture(Rectangle captArea, Size resolution)
        {

            var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
            DCapt.ThrowIfError(result, "DCaptCreateCapture");

            RECT srcRect = new RECT
            {
                Left = captArea.Left,
                Right = captArea.Right,
                Bottom = captArea.Bottom,
                Top = captArea.Top,
            };

            SIZE destSize = new SIZE
            {
                cx = resolution.Width,
                cy = resolution.Height,
            };

            // The bits per pixel of the saved data. Must be 2 
            int bitsPerPixel = 2;
            result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref destSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY,
               out var pBuffer, out var hBmi); // <--pBuffer и hBmi удаляются в DCaptFreeCapture()

            DCapt.ThrowIfError(result, "DCaptConfigureCapture");

            var bmiHeader = (BITMAPINFOHEADER)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFOHEADER));

            Console.WriteLine(bmiHeader.ToString());

           // this.bitmapInfo.bmiHeader = bmiHeader;

            var srcDataSize = bmiHeader.biSizeImage;
            var srcWidth = bmiHeader.biWidth;
            var srcHeight = bmiHeader.biHeight;
            if (srcHeight < 0)
            {
                srcHeight = -srcHeight;
            }

            var srcFormat = PixFormat.Unknown;
            var bitCount = bmiHeader.biBitCount;
            if (bitCount == 16)
            { //rgb 16 bit
                if (bmiHeader.biCompression == (uint)BI.BITFIELDS)
                {// может быть rgb555 или rgb565
                 // читаем маски цветов что бы определить формат
                    var hColor = IntPtr.Add(hBmi, (int)bmiHeader.biSize);
                    uint[] bmiColors = NativeAPIs.Utils.MarshalHelper.GetArrayData<uint>(hColor, 3);

                    var red = bmiColors[0];
                    var green = bmiColors[1];
                    var blue = bmiColors[2];
                    if (red == 0xF800 && green == 0x7E0 && blue == 0x1F)
                    {// rgb 565
                        srcFormat = PixFormat.RGB16;
                    }
                    else if (red == 0x7C00 && green == 0x3E0 && blue == 0x1F)
                    {// rgb 555
                        srcFormat = PixFormat.RGB15;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid color mask: " + string.Join(",", red, green, blue));
                    }
                }
                else if (bmiHeader.biCompression == (uint)BI.RGB)
                {// только rgb555
                    srcFormat = PixFormat.RGB15;
                }
                else
                {
                    throw new InvalidOperationException("Invalid compression field: " + bmiHeader.biCompression);
                }
            }
            else
            {// в других форматов быть не должно
                throw new NotSupportedException("Invalid bit count: " + bmiHeader.biBitCount);
            }

            //https://docs.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-bitmapinfoheader
            // Calculating Surface Stride
            var srcStride = ((((srcWidth * bitCount) + 31) & ~31) >> 3);
            var srcBuffer = new FrameBuffer(pBuffer, srcStride);
           
            dcaptFrame = new VideoFrame(new FrameBuffer[] { srcBuffer }, (int)srcDataSize, srcWidth, srcHeight, srcFormat, 16);
        }
      
        public override ErrorCode UpdateBuffer(int timeout = 10)
        {
            //logger.Verb("Update(...) " + timeout);

            ErrorCode errorCode = ErrorCode.Unexpected;

            if (!Initialized)
            {
                return ErrorCode.NotInitialized;
            }


            var dcaptBuffer = dcaptFrame.Buffer[0];
            var dcaptBufSize = dcaptFrame.DataLength;

            Kernel32.ZeroMemory(dcaptBuffer.Data, dcaptBufSize);

            var result = DCapt.DCaptUpdate(hCapt);
            DCapt.ThrowIfError(result, "DCaptCreateCapture");

            var deviceContext = device.ImmediateContext;
            var dataBox = deviceContext.MapSubresource(srcTexture, 0, MapMode.Write, MapFlags.None);
            try
            {
                var width = DestSize.Width;
                var height = DestSize.Height;

                var srcStride = dcaptBuffer.Stride;
                var srcPtr = dcaptBuffer.Data;

                var destPtr = dataBox.DataPointer;
                var destStride = dataBox.RowPitch;

                for (int i = 0; i < height; i++)
                {
                    Kernel32.CopyMemory(destPtr, srcPtr, (uint)destStride);
                    destPtr += destStride;
                    srcPtr += srcStride;
                }
            }
            finally
            {
                deviceContext.UnmapSubresource(srcTexture, 0);
            }


            var destFrame = VideoBuffer.GetFrame();
            bool lockTaken = false;
            try
            {
                lockTaken = destFrame.Lock(timeout);

                if (lockTaken)
                {
                    pixConverter.Process(srcFrame, destFrame);
                    errorCode = ErrorCode.Ok;
                }
                else
                {
                    logger.Warn("Drop bits...");
                }
            }
            finally
            {
                if (lockTaken)
                {
                    destFrame.Unlock();
                }
            }

            return errorCode;
            // Console.WriteLine("DCaptCreateCapture() " + result);
        }


        public override void Close()
        {

            logger.Debug("DatapathDesktopCapture::Close()");

            if (Initialized)
            {
                if (hCapt != IntPtr.Zero)
                {
                    var result = DCapt.DCaptFreeCapture(hCapt);
                    DCapt.ThrowIfError(result, "DCaptFreeCapture");
                    hCapt = IntPtr.Zero;
                }
            }

            if (pixConverter != null)
            {
                pixConverter.Close();
                pixConverter = null;
            }

            if (srcFrame != null)
            {
                srcFrame.Dispose();
                srcFrame = null;
            }

            if (dcaptFrame != null)
            {
                dcaptFrame.Dispose();
                dcaptFrame = null;
            }

            CloseDx();

            base.Close();

        }


        /// <summary>
        /// https://www.datapath.co.uk/supportdownloads/windows/imagedp4-sdk/Desktop-Capture-SDK.pdf
        /// https://www.datapath.co.uk/datapath-current-downloads/video-wall-downloads-1/sdks-10/74-deskcapt-v1-0-2
        /// </summary>
        class DCapt
        {
            public const string CaptDll = "capt.dll";

            private const uint DESKCAPT_ERROR_BASE = 0x011B0000;

            internal enum CaptError : uint
            {
                DESKCAPTERROR_NO_ERROR = 0,

                DESKCAPT_ERROR_UNKNOWN_ERROR = (DESKCAPT_ERROR_BASE + 0),
                DESKCAPT_ERROR_INVALID_HANDLE = (DESKCAPT_ERROR_BASE + 1),
                DESKCAPT_ERROR_BUFFER_TOO_SMALL = (DESKCAPT_ERROR_BASE + 2),
                DESKCAPT_ERROR_INVALID_RECT = (DESKCAPT_ERROR_BASE + 3),
                DESKCAPT_ERROR_INVALID_COLOUR = (DESKCAPT_ERROR_BASE + 4),
                DESKCAPT_ERROR_INVALID_FLAGS = (DESKCAPT_ERROR_BASE + 5),
                DESKCAPT_ERROR_API_ALREADY_LOADED = (DESKCAPT_ERROR_BASE + 6),
            }

            internal enum CaptFlags
            {
                CAPTURE_FLAG_OVERLAY = 1
            }

            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptLoad")]
            internal static extern CaptError DCaptLoad(out IntPtr hLoad);


            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptFree")]
            internal static extern CaptError DCaptFree(IntPtr hLoad);


            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptCreateCapture")]
            internal static extern CaptError DCaptCreateCapture(IntPtr hLoad, out IntPtr hCapt);

            /// <summary>
            /// This function configures a desktop capture rectangle for a copy into the required source buffer size. 
            /// *ppBuffer will not contain the captureuntil a successful call to DCaptUpdate has been made.
            /// </summary>
            /// <param name="hCapt">The Capture to configure</param>
            /// <param name="srcRect">The rectangle on the desktop to capture</param>
            /// <param name="dstSize">The size of the buffer to capture the desktop in</param>
            /// <param name="bitsPerPixel">The bits per pixel of the saved data. Must be 2</param>
            /// <param name="flags">Combination of CAPTURE_FLAG_*** flags</param>
            /// <param name="hBuf"> Address of pointer to to capture the desktop image</param>
            /// <param name="hInfo">Address of pointer to an RGBBITMAPINFO structure</param>
            /// <returns></returns>
            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptConfigureCapture")]
            internal static extern CaptError _DCaptConfigureCapture(IntPtr hCapt, ref RECT srcRect, ref Size dstSize, int bitsPerPixel, CaptFlags flags, ref IntPtr hBuf, ref IntPtr hInfo);

            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptConfigureCapture")]
            internal static extern CaptError DCaptConfigureCapture(IntPtr hCapt, ref RECT srcRect, ref SIZE dstSize, int bitsPerPixel, CaptFlags flags, out IntPtr hBuf, out IntPtr hInfo);

            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptUpdate")]
            internal static extern CaptError DCaptUpdate(IntPtr hCapt);

            [DllImport(CaptDll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, EntryPoint = "DCaptFreeCapture")]
            internal static extern CaptError DCaptFreeCapture(IntPtr hCapt);

            internal static void ThrowIfError(CaptError code, string message = "")
            {
                if (code != CaptError.DESKCAPTERROR_NO_ERROR)
                {
                    throw new Exception(message + " " + code);
                }
            }
        }


        //private void InitCapture(Rectangle captArea, Size resolution, PixelFormat pixelFormat)
        //{
        //	if (pixelFormat != PixelFormat.Format16bppRgb565)
        //	{
        //		throw new FormatException("Unsuppoted pix format " + pixelFormat);
        //	}

        //	try
        //	{
        //		var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
        //		DCapt.ThrowIfError(result, "DCaptCreateCapture");

        //		logger.Debug("DCaptCreateCapture() " + result);

        //		int biWidth = resolution.Width;
        //		int biHeight = resolution.Height;
        //		// 
        //		int biBitCount = Image.GetPixelFormatSize(pixelFormat);
        //		uint biSizeImage = (uint)(biWidth * biHeight * biBitCount / 8);

        //		const int BI_BITFIELDS = 3;

        //		var bmiHeader = new BITMAPINFOHEADER
        //		{
        //			biWidth = biWidth,
        //			biHeight = -biHeight,
        //			biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),

        //			biBitCount = (ushort)biBitCount,
        //			biPlanes = 1,

        //			biClrUsed = 0,
        //			biClrImportant = 0,
        //			biSizeImage = biSizeImage,
        //			biCompression = BI_BITFIELDS,

        //		};

        //		var bmiColors = GetColourMask(pixelFormat);

        //		//var bmiColors = new RGBQUAD[]
        //		//{
        //		//     new RGBQUAD
        //		//     {
        //		//         rgbRed = 0,
        //		//         rgbBlue = 248,
        //		//         rgbGreen = 0
        //		//     }
        //		//};

        //		BITMAPINFO bmi = new BITMAPINFO
        //		{
        //			bmiHeader = bmiHeader,
        //			// bmiColors = bmiColors,
        //		};


        //		IntPtr _hBmi = IntPtr.Zero;
        //		try
        //		{
        //			_hBmi = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BITMAPINFO)));
        //			Marshal.StructureToPtr(bmi, _hBmi, false);

        //			var dstSize = resolution;
        //			RECT srcRect = new RECT
        //			{
        //				Left = captArea.Left,
        //				Right = captArea.Right,
        //				Bottom = captArea.Bottom,
        //				Top = captArea.Top,
        //			};

        //			// The bits per pixel of the saved data. Must be 2 
        //			int bitsPerPixel = 2;

        //			IntPtr hBmi = _hBmi;
        //			result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref dstSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY, ref pBuffer, ref hBmi);
        //			DCapt.ThrowIfError(result, "DCaptConfigureCapture");

        //			this.bmi = (BITMAPINFO)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFO));
        //			var _bmiHeader = bmi.bmiHeader;

        //			logger.Debug("_bmiHeader " + _bmiHeader.biWidth + "x" + _bmiHeader.biHeight + " "
        //				+ _bmiHeader.biBitCount + " " + _bmiHeader.biCompression + " " + _bmiHeader.biSizeImage);

        //		}
        //		finally
        //		{
        //			if (_hBmi != IntPtr.Zero)
        //			{
        //				Marshal.FreeHGlobal(_hBmi);
        //			}
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		logger.Error(ex);

        //		Close();
        //		throw;
        //	}
        //}


        //private void Init(Rectangle captArea, Bitmap bmp)
        //{
        //    if (!(bmp.PixelFormat == PixelFormat.Format16bppRgb565 || bmp.PixelFormat == PixelFormat.Format16bppRgb565))
        //    {
        //        throw new FormatException("Unsuppoted pix format " + bmp.PixelFormat);
        //    }

        //    try
        //    {
        //        var result = DCapt.DCaptCreateCapture(hLoad, out hCapt);
        //        DCapt.ThrowIfError(result, "DCaptCreateCapture");

        //        logger.Debug("DCaptCreateCapture() " + result);

        //        int biWidth = bmp.Width;
        //        int biHeight = bmp.Height;
        //        // 
        //        int biBitCount = Image.GetPixelFormatSize(bmp.PixelFormat);
        //        uint biSizeImage = (uint)(biWidth * biHeight * biBitCount / 8);

        //        const int BI_BITFIELDS = 3;

        //        var bmiHeader = new BITMAPINFOHEADER
        //        {
        //            biWidth = biWidth,
        //            biHeight = -biHeight,
        //            biSize = (uint)Marshal.SizeOf(typeof(BITMAPINFOHEADER)),

        //            biBitCount = (ushort)biBitCount,
        //            biPlanes = 1,

        //            biClrUsed = 0,
        //            biClrImportant = 0,
        //            biSizeImage = biSizeImage,
        //            biCompression = BI_BITFIELDS,

        //        };

        //        var bmiColors = GetColourMask(bmp.PixelFormat);

        //        //var bmiColors = new RGBQUAD[]
        //        //{
        //        //     new RGBQUAD
        //        //     {
        //        //         rgbRed = 0,
        //        //         rgbBlue = 248,
        //        //         rgbGreen = 0
        //        //     }
        //        //};

        //        BITMAPINFO bmi = new BITMAPINFO
        //        {
        //            bmiHeader = bmiHeader,
        //           // bmiColors = bmiColors,
        //        };


        //        var dstSize = bmp.Size;

        //        RECT srcRect = new RECT
        //        {
        //            Left = captArea.Left,
        //            Right = captArea.Right,
        //            Bottom = captArea.Bottom,
        //            Top = captArea.Top,
        //        };

        //        IntPtr _hBmi = IntPtr.Zero;
        //        try
        //        {
        //            _hBmi = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BITMAPINFO)));
        //            Marshal.StructureToPtr(bmi, _hBmi, false);

        //            IntPtr hBmi = _hBmi;

        //            // The bits per pixel of the saved data. Must be 2 
        //            int bitsPerPixel = 2;//biBitCount / 8; 
        //            result = DCapt.DCaptConfigureCapture(hCapt, ref srcRect, ref dstSize, bitsPerPixel, DCapt.CaptFlags.CAPTURE_FLAG_OVERLAY, ref pBuffer, ref hBmi);
        //            DCapt.ThrowIfError(result, "DCaptConfigureCapture");

        //            this.bmi = (BITMAPINFO)Marshal.PtrToStructure(hBmi, typeof(BITMAPINFO));
        //            var _bmiHeader = bmi.bmiHeader;

        //            logger.Debug("_bmiHeader " + _bmiHeader.biWidth + "x" + _bmiHeader.biHeight + " "
        //                + _bmiHeader.biBitCount + " " + _bmiHeader.biCompression + " " + _bmiHeader.biSizeImage);

        //        }
        //        finally
        //        {
        //            if (_hBmi != IntPtr.Zero)
        //            {
        //                Marshal.FreeHGlobal(_hBmi);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex);

        //        Close();
        //        throw;
        //    }
        //}


    }
}
