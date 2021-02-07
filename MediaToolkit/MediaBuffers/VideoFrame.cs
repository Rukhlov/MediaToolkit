using MediaToolkit.Core;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{

    public abstract class VideoFrameBase : IVideoFrame
    {
        public IFrameBuffer[] Buffer { get; protected set; }

        public double Time { get; set; }
        public double Duration { get; set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public PixFormat Format { get; protected set; } = PixFormat.RGB32;
        public int Align { get; protected set; }

        public abstract VideoDriverType DriverType { get; }

        public ColorSpace ColorSpace { get; set; }
        public ColorRange ColorRange { get; set; }

        public int DataLength { get; protected set; }


        protected readonly object syncRoot = new object();
        public virtual bool Lock(int timeout = 10)
        {
            bool lockTaken = false;
            Monitor.TryEnter(syncRoot, timeout, ref lockTaken);
            return lockTaken;
        }

        public virtual void Unlock()
        {
            Monitor.Exit(syncRoot);
        }

        public virtual void Dispose()
        { }
    }

    public class D3D11VideoFrame : VideoFrameBase
    {
        public D3D11VideoFrame(Texture2D srcTexture)
        {
            int dataSize = 0;
            IFrameBuffer[] frameData = null;
            PixFormat format = PixFormat.Unknown;

            var descr = srcTexture.Description;
            var width = descr.Width;
            var height = descr.Height;

            if (descr.Format == SharpDX.DXGI.Format.B8G8R8A8_UNorm)
            {
                format = PixFormat.RGB32;
            }
            else if (descr.Format == SharpDX.DXGI.Format.NV12)
            {
                format = PixFormat.NV12;
                IsVideoFormat = true;
            }
            else
            {
                throw new InvalidOperationException("Unsupported texture format: " + descr.Format);
            }

            var tex = new Texture2D(srcTexture.NativePointer);
            ((IUnknown)tex).AddReference();
            textures.Add(tex);

			frameData = new FrameBuffer[]
			{
				new FrameBuffer(tex.NativePointer, 0)
			};

            _D3D11VideoFrame(frameData, dataSize, width, height, format);
        }

        public D3D11VideoFrame(PixFormat format, params Texture2D[] srcTextures)
        {
            int width = 0;
            int height = 0;
            int dataSize = 0;
            IFrameBuffer[] frameData = null;

            if (format == PixFormat.RGB32 || format == PixFormat.RGB24 || format == PixFormat.RGB16)
            {
                if (srcTextures.Length == 1)
                {
                    var rgbTexture = srcTextures[0];

                    var descr = rgbTexture.Description;
                    //if (descr.Format == SharpDX.DXGI.Format.R8G8B8A8_UNorm )
                    {
                        width = descr.Width;
                        height = descr.Height;
                        dataSize = 0;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid format");
                }

            }
            else if (format == PixFormat.NV12)
            {
                if (srcTextures.Length == 2)
                {
                    var lumaTexture = srcTextures[0];
                    var chromaTexture = srcTextures[1];

                    var descr = lumaTexture.Description;
                    if (descr.Format == SharpDX.DXGI.Format.R8_UNorm)
                    {
                        width = descr.Width;
                        height = descr.Height;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid luma texture format: " + descr.Format);
                    }
                }
                else if (srcTextures.Length == 1)
                {
                    var nv12Texture = srcTextures[0];
                    var descr = nv12Texture.Description;
                    if (descr.Format == SharpDX.DXGI.Format.NV12)
                    {
                        width = descr.Width;
                        height = descr.Height;
                        IsVideoFormat = true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid texture format: " + descr.Format);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid format");
                }
            }
            else if (format == PixFormat.I444 || format == PixFormat.I422 || format == PixFormat.I420)
            {
                if (srcTextures.Length == 3)
                {
                    var lumaTexture = srcTextures[0];
                    var CbTexture = srcTextures[1];
                    var CrTexture = srcTextures[2];

                    var descr = lumaTexture.Description;
                    if (descr.Format == SharpDX.DXGI.Format.R8_UNorm)
                    {
                        width = descr.Width;
                        height = descr.Height;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid luma texture format: " + descr.Format);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid format");
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid format: " + format);
            }

            frameData = new FrameBuffer[srcTextures.Length];
            for (int i = 0; i < srcTextures.Length; i++)
            {
                var srcTex = srcTextures[i];
                var tex = new Texture2D(srcTex.NativePointer);
                ((IUnknown)tex).AddReference();
                textures.Add(tex);
                frameData[i] = new FrameBuffer(tex.NativePointer, 0);
            }

            _D3D11VideoFrame(frameData, dataSize, width, height, format);

        }

        public D3D11VideoFrame(SharpDX.Direct3D11.Device device, Size resolution, PixFormat format)
        {
            var width = resolution.Width;
            var height = resolution.Height;


            SharpDX.DXGI.Format dxgiFormat = SharpDX.DXGI.Format.Unknown;
            if (format == PixFormat.RGB32)
            {
                dxgiFormat = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            }
            else if (format == PixFormat.NV12)
            {
                dxgiFormat = SharpDX.DXGI.Format.NV12;
            }
            else
            {
                throw new NotSupportedException();
            }

            var descr = new Texture2DDescription
            {
                Width = width,
                Height = height,
                Format = dxgiFormat,

                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = { Count = 1, Quality = 0 },
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            };


            var pitch = Width * SharpDX.DXGI.FormatHelper.SizeOfInBytes(dxgiFormat);
            var dataSize = pitch * Height;// 

            var formatSupport = device.CheckFormatSupport(dxgiFormat);
            bool videoFormatsSupporded = formatSupport.HasFlag(FormatSupport.Texture2D);

            IFrameBuffer[] frameData = null;
            if (videoFormatsSupporded)
            {
                var texture = new Texture2D(device, descr);

                textures.Add(texture);
                frameData = new FrameBuffer[]
                {
                    new FrameBuffer(texture.NativePointer, 0),
                };
                IsVideoFormat = true;
            }
            else
            {
                if (dxgiFormat == SharpDX.DXGI.Format.NV12)
                {
                    descr.Format = SharpDX.DXGI.Format.R8_UNorm;
                    var lumaTex = new Texture2D(device, descr);
                    textures.Add(lumaTex);

                    descr.Format = SharpDX.DXGI.Format.R8G8_UNorm;
                    descr.Width = width / 2;
                    descr.Height = height / 2;
                    var chromaTex = new Texture2D(device, descr);
                    textures.Add(chromaTex);

                    frameData = new FrameBuffer[]
                    {
                        new FrameBuffer(lumaTex.NativePointer, 0),
                        new FrameBuffer(chromaTex.NativePointer, 0),
                    };
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            _D3D11VideoFrame(frameData, dataSize, width, height, format);

        }

        public D3D11VideoFrame(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format)
        //: base(buffer, size, width, height, format, 0)
        {
            _D3D11VideoFrame(buffer, size, width, height, format);

        }

        private void _D3D11VideoFrame(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.Buffer = buffer;
            this.DataLength = size;
        }

        public bool IsVideoFormat = false;
        internal List<Texture2D> textures = new List<Texture2D>();

        public override VideoDriverType DriverType => VideoDriverType.D3D11;

        public IReadOnlyList<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            if (Buffer != null && Buffer.Length > 0)
            {
                foreach (var buf in Buffer)
                {
                    var pTexture = buf.Data;
                    var texture = new Texture2D(pTexture);
                    ((IUnknown)texture).AddReference();
                    textures.Add(texture);
                }
            }

            return textures;
        }


        private bool disposed = false;
        public override void Dispose()
        {
            foreach (var t in textures)
            {
                if (!t.IsDisposed)
                {
                    t.Dispose();
                    disposed = true;
                }
            }
        }

    }

    public class VideoFrame : VideoFrameBase
    {
        public VideoFrame(int width, int height, PixFormat format, int align)
        {
            var length = FFmpegLib.Utils.AllocImageData(new Size(width, height), format, align, out var buffer);
            Init(buffer, length, width, height, format, align, true);
        }

        public VideoFrame(IFrameBuffer[] buffer, int length, int width, int height, PixFormat format, int align)
        {
            Init(buffer, length, width, height, format, align);
        }

        private void Init(IFrameBuffer[] buffer, int length, int width, int height, PixFormat format, int align, bool ffmpegAllocated = false)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.Align = align;
            this.Buffer = buffer;
            this.DataLength = length;
            this.ffmpegAllocated = ffmpegAllocated;
        }

        public override VideoDriverType DriverType => VideoDriverType.CPU;

        private bool ffmpegAllocated = false;
        private bool disposed = false;
        public override void Dispose()
        {
            lock (syncRoot)
            {
                if (!disposed)
                {
                    if (ffmpegAllocated)
                    {
                        var b = Buffer;
                        FFmpegLib.Utils.FreeImageData(ref b);
                    }

                    disposed = true;
                }

            }
        }



        public byte[] ConvertToContiguousBuffer()
        {
            return ConvertToContiguousBuffer(this);
        }

        public static byte[] ConvertToContiguousBuffer(IVideoFrame frame)
        {
            if (frame.DriverType != VideoDriverType.CPU)
            {
                throw new InvalidOperationException("Invalid video frame driver type: " + frame.DriverType);
            }

            var buffer = frame.Buffer;
            var size = new Size(frame.Width, frame.Height);
            var format = frame.Format;

            return ConvertToContiguousBuffer(buffer, size, format);
        }

        private static byte[] ConvertToContiguousBuffer(IFrameBuffer[] frameBuffer, Size size, MediaToolkit.Core.PixFormat format)
        {

            byte[] buffer = null;
            if (format == MediaToolkit.Core.PixFormat.NV12)
            {
                var lumaStride = size.Width;
                var lumaHeight = size.Height;

                var chromaStride = size.Width;
                var chromaHeight = size.Height / 2;

                //lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
                //chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);
                var bufferSize = lumaStride * lumaHeight + chromaStride * chromaHeight;
                buffer = new byte[bufferSize];

                int offset = 0;
                var pData = frameBuffer[0].Data;
                var dataStride = frameBuffer[0].Stride;
                for (int row = 0; row < lumaHeight; row++)
                { //Y
                    Marshal.Copy(pData, buffer, offset, lumaStride);
                    offset += lumaStride;
                    pData += dataStride;
                }

                pData = frameBuffer[1].Data;
                dataStride = frameBuffer[1].Stride;
                for (int row = 0; row < chromaHeight; row++)
                {// packed CbCr
                    Marshal.Copy(pData, buffer, offset, chromaStride);
                    offset += chromaStride;
                    pData += dataStride;
                }
            }
            else if (format == MediaToolkit.Core.PixFormat.RGB32
               || format == MediaToolkit.Core.PixFormat.RGB24
               || format == MediaToolkit.Core.PixFormat.RGB16)
            {
                int bytesPerPixel = 4;
                if (format == MediaToolkit.Core.PixFormat.RGB24)
                {
                    bytesPerPixel = 3;
                }
                else if (format == MediaToolkit.Core.PixFormat.RGB16)
                {
                    bytesPerPixel = 2;
                }

                var rgbStride = bytesPerPixel * size.Width;

                var bufferSize = rgbStride * size.Height;
                buffer = new byte[bufferSize];

                int offset = 0;
                var pData = frameBuffer[0].Data;
                var dataStride = frameBuffer[0].Stride;

                for (int row = 0; row < size.Height; row++)
                {
                    Marshal.Copy(pData, buffer, offset, rgbStride);
                    offset += rgbStride;
                    pData += dataStride;
                }
            }
            else if (format == MediaToolkit.Core.PixFormat.I444 ||
                format == MediaToolkit.Core.PixFormat.I422 ||
                format == MediaToolkit.Core.PixFormat.I420)
            {
                var lumaHeight = size.Height;
                var chromaHeight = size.Height;

                var lumaStride = size.Width;
                var chomaStride = size.Width;

                if (format == MediaToolkit.Core.PixFormat.I420)
                {
                    chromaHeight = size.Height / 2;
                    chomaStride = size.Width / 2;

                }
                else if (format == MediaToolkit.Core.PixFormat.I422)
                {
                    chomaStride = size.Width / 2;
                }

                //lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
                //chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);

                var bufferSize = lumaStride * lumaHeight + 2 * chomaStride * chromaHeight;

                buffer = new byte[bufferSize];

                int offset = 0;
                var pData = frameBuffer[0].Data;
                var dataStride = frameBuffer[0].Stride;

                for (int row = 0; row < lumaHeight; row++)
                {//Y
                    Marshal.Copy(pData, buffer, offset, lumaStride);
                    offset += lumaStride;
                    pData += dataStride;
                }

                for (int i = 1; i < 3; i++)
                {// CbCr
                    pData = frameBuffer[i].Data;
                    dataStride = frameBuffer[i].Stride;
                    for (int row = 0; row < chromaHeight; row++)
                    {
                        Marshal.Copy(pData, buffer, offset, chomaStride);
                        offset += chomaStride;
                        pData += dataStride;
                    }

                }
            }


            return buffer;
        }
    }

	public class GDIFrame : VideoFrameBase
	{
		public GDIFrame(Bitmap bmp)
		{
			
			this.GdiBitmap = bmp;
			this.Width = bmp.Width;
			this.Height = bmp.Height;
			var format = GdiFormatToPixFormat(bmp.PixelFormat);
			if(format == PixFormat.Unknown)
			{
				throw new InvalidOperationException("Invalid frame format: " + bmp.PixelFormat);
			}

			this.Format = format;
		}

		public readonly Bitmap GdiBitmap = null;

		//private System.Drawing.Imaging.BitmapData bitmapData = null;
		//public IFrameBuffer LockBits()
		//{
		//	var rect = new System.Drawing.Rectangle(0, 0, Width, Height);
		//	bitmapData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
		//	return new FrameBuffer(bitmapData.Scan0, bitmapData.Stride);
		//}

		//public void UnlockBits()
		//{
		//	bitmap.UnlockBits(bitmapData);
		//	bitmapData = null;
		//}

		public override VideoDriverType DriverType => VideoDriverType.GDI;

		public static PixFormat GdiFormatToPixFormat(System.Drawing.Imaging.PixelFormat gdiFormat)
		{
			PixFormat pixFormat = PixFormat.Unknown;

			if (gdiFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
			{
				pixFormat = PixFormat.RGB32;
			}
			else if (gdiFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb)
			{
				pixFormat = PixFormat.RGB24;
			}
			else if (gdiFormat == System.Drawing.Imaging.PixelFormat.Format16bppRgb565)
			{
				pixFormat = PixFormat.RGB16;
			}
			else if (gdiFormat == System.Drawing.Imaging.PixelFormat.Format16bppRgb555)
			{
				pixFormat = PixFormat.RGB15;
			}

			return pixFormat;
		}
	}


}
