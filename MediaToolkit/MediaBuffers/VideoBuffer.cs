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

		public event Action<IVideoFrame> BufferUpdated;

		public void OnBufferUpdated(IVideoFrame frame)
		{
			BufferUpdated?.Invoke(frame);
		}

		public abstract void Dispose();

	}

	public class D3D11VideoBuffer : VideoBufferBase
    {
		public readonly SharpDX.Direct3D11.Device D3D11Device = null;
		public D3D11VideoBuffer(SharpDX.Direct3D11.Device device, Size resolution, PixFormat format, int framesCount = 1)
		{
			this.D3D11Device = device;
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


}
