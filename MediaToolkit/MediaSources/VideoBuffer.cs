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

            SharpDX.DXGI.Format dxFormat = SharpDX.DXGI.Format.Unknown;
            if (format == PixFormat.RGB32)
            {
                dxFormat = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
            }
            else if (format == PixFormat.NV12)
            {
                dxFormat = SharpDX.DXGI.Format.NV12;
            }
            else
            {
                throw new NotSupportedException();
            }

            frameBuffer = new VideoFrameBase[framesCount];
			var width = resolution.Width;
			var height = resolution.Height;
            var pitch = width * SharpDX.DXGI.FormatHelper.SizeOfInBytes(dxFormat);
            var dataSize = pitch * height;// 

            var descr = new Texture2DDescription
			{
				Width = width,
				Height = height,
				Format = dxFormat,

				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = { Count = 1, Quality = 0 },
				Usage = ResourceUsage.Default,
				OptionFlags = ResourceOptionFlags.Shared,
				CpuAccessFlags = CpuAccessFlags.None,
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
			};

            

            for (int i = 0; i < framesCount; i++)
			{
				var texture = new Texture2D(device, descr);
				
				textures.Add(texture);
				IFrameBuffer[] frameData =
				{
					new FrameBuffer(texture.NativePointer, 0),
				};

				frameBuffer[i] = new D3D11VideoFrame(frameData, dataSize, width, height, format);

			}
		}

		private List<Texture2D> textures = new List<Texture2D>();
        public override VideoDriverType DriverType => VideoDriverType.D3D11;

        public override void Dispose()
        {
            foreach (var t in textures)
            {
                if (!t.IsDisposed)
                {
                    t.Dispose();
                }
            }

			frameBuffer = null;
		}
    }


    public class MemoryVideoBuffer : VideoBufferBase
    { 

        public MemoryVideoBuffer(Size resolution, PixFormat format, int align, int framesCount = 1)
        {
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
            foreach(var f in frameBuffer)
            {
				var buffer = f.Buffer;
				for (int i = 0; i < buffer.Length; i++)
				{
					var data = buffer[i];
					if (data.Data != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(data.Data);
					}
				}			
			}
		}
    }

    public abstract class VideoFrameBase : IVideoFrame
    {

        //public VideoFrameBase(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format, int align)
        //{
        //    this.Width = width;
        //    this.Height = height;

        //    this.Format = format;
        //    this.Align = align;

        //    this.Buffer = buffer;
        //}

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
        {

        }
    }

    public class D3D11VideoFrame : VideoFrameBase
    {
        public D3D11VideoFrame(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format)
            //: base(buffer, size, width, height, format, 0)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.Buffer = buffer;
            this.DataLength = size;
        }

        public override VideoDriverType DriverType => VideoDriverType.D3D11;
    }

    public class VideoFrame : VideoFrameBase
    {
        public VideoFrame(IFrameBuffer[] buffer, int size, int width, int height, PixFormat format, int align)
            //: base(buffer, size, width, height, format, align)
        {
            this.Width = width;
            this.Height = height;
            this.Format = format;
            this.Align = align;
            this.Buffer = buffer;
            this.DataLength = size;
        }

        public override VideoDriverType DriverType => VideoDriverType.CPU;
    }

}
