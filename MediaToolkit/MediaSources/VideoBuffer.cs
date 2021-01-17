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
    public abstract class VideoBufferBase
    {
        protected VideoFrameBase[] frameBuffer = null;

        public VideoBufferBase() { }

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public PixFormat Format { get; protected set; }

        public abstract VideoDriverType DriverType { get; }

        public virtual VideoFrameBase GetFrame()
        {
            VideoFrameBase frame = null;

            if (frameBuffer != null && frameBuffer.Length > 0)
            {
                frame = frameBuffer[0];
            }
            return frame;
        }

		public event Action<VideoFrameBase> BufferUpdated;

		public void OnBufferUpdated(VideoFrameBase frame)
		{
			BufferUpdated?.Invoke(frame);
		}

		public abstract void Dispose();

	}

	public class D3D11VideoBuffer : VideoBufferBase
    {

		public D3D11VideoBuffer(SharpDX.Direct3D11.Device device, Size resolution, PixFormat format, int framesCount = 1)
		{
            this.Width = resolution.Width;
            this.Height = resolution.Height;
            this.Format = format;

			this.frameBuffer = new VideoFrameBase[framesCount];

			for (int i = 0; i < framesCount; i++)
			{
				frameBuffer[i] = new D3D11VideoFrame(device, resolution, Format);
			}
		}

        public override VideoDriverType DriverType => VideoDriverType.D3D11;
        public override void Dispose()
        {
            foreach (var frame in frameBuffer)
            {
				if (frame != null)
				{
					frame.Dispose();
				}
            }

			frameBuffer = null;
		}
    }


    public class MemoryVideoBuffer : VideoBufferBase
    { 

        public MemoryVideoBuffer(Size resolution, PixFormat format, int align, int framesCount = 1)
        {
            this.Width = resolution.Width;
            this.Height = resolution.Height;
            this.Format = format;

            frameBuffer = new VideoFrameBase[framesCount];
            for (int i= 0; i<framesCount; i++)
            {
                var dataSize = FFmpegLib.Utils.AllocImageData(resolution, format, align, out var frameData);

                frameBuffer[i] = new VideoFrame(frameData, dataSize, resolution.Width, resolution.Height, format, align);
            }
        }

        public override VideoDriverType DriverType => VideoDriverType.CPU;
        public override void Dispose()
        {
            foreach(var buf in frameBuffer)
            {
				buf.Dispose();		
			}

			frameBuffer = null;
		}

	}

    public abstract class VideoFrameBase : IVideoFrame
    {
        public IFrameBuffer[] Buffer { get; protected set; }

        public double Time { get; set; }
        public double Duration { get; set; }

        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public PixFormat Format { get; protected set; } = PixFormat.RGB32;
        public int Align { get; protected set; }

        public abstract VideoDriverType DriverType  { get; }

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
		public D3D11VideoFrame(SharpDX.Direct3D11.Device device, Size resolution, PixFormat format)
		{
			var width = resolution.Width;
			var	height = resolution.Height;

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

			_D3D11VideoFrame(frameData, dataSize, Width, Height, Format);
		
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

		private List<Texture2D> textures = new List<Texture2D>();

		public override VideoDriverType DriverType => VideoDriverType.D3D11;

        public IReadOnlyList<Texture2D> GetTextures()
        {
            List<Texture2D> textures = new List<Texture2D>();
            if (Buffer != null && Buffer.Length > 0)
            {
                foreach(var buf in Buffer)
                {
                    var pTexture = buf.Data;
                    var texture = new Texture2D(pTexture);
                    ((IUnknown)texture).AddReference();
                    textures.Add(texture);
                }
            }

            return textures;
        }

		public override void Dispose()
		{
			foreach (var t in textures)
			{
				if (!t.IsDisposed)
				{
					t.Dispose();
				}
			}
		}

	}

    public class VideoFrame : VideoFrameBase
    {
		public VideoFrame(int width, int height, PixFormat format, int align)
		{
			var size = FFmpegLib.Utils.AllocImageData(new Size(width, height),  format, align, out var buffer);
			Init(buffer, size, width, height, format, align);
		}

		public VideoFrame(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format, int align)
        {
			Init(buffer, size, width, height, format, align);
		}

		private void Init(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format, int align)
		{
			this.Width = width;
			this.Height = height;
			this.Format = format;
			this.Align = align;
			this.Buffer = buffer;
			this.DataLength = size;
		}

        public override VideoDriverType DriverType => VideoDriverType.CPU;
		public override void Dispose()
		{
			lock (syncRoot)
			{
				var b = Buffer;
				FFmpegLib.Utils.FreeImageData(ref b);
			}
		}

		public byte[] ConvertToContiguousBuffer()
		{
			return ConvertToContiguousBuffer(this);
		}

		public static byte[] ConvertToContiguousBuffer(IVideoFrame frame)
		{
			if(frame.DriverType != VideoDriverType.CPU)
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
			   || format == MediaToolkit.Core.PixFormat.RGB565)
			{
				int bytesPerPixel = 4;
				if (format == MediaToolkit.Core.PixFormat.RGB24)
				{
					bytesPerPixel = 3;
				}
				else if (format == MediaToolkit.Core.PixFormat.RGB565)
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

}
