using MediaToolkit.Core;
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
	public class _VideoBuffer
	{
		_VideoBuffer()
		{ }

		public void InitMemoryBuffer(Size size, PixFormat format, int align = 32)
		{
			if (initialized)
			{//...

			}

			this.FrameSize = size;
			this.Format = format;
			this.DriverType = VideoDriverType.CPU;
			
			this.DataLength = FFmpegLib.Utils.AllocImageData(size, format, align, out var frameData);
			this.FrameData = frameData;
			this.needCleanup = true;
			this.initialized = true;

		}

		private bool initialized = false;
		private bool needCleanup = false;
		public void InitDXBuffer(IntPtr pTexture)
		{
			if (initialized)
			{//...

			}
			using (SharpDX.Direct3D11.Texture2D texture = new SharpDX.Direct3D11.Texture2D(pTexture))
			{
				var descr = texture.Description;
				this.FrameSize = new Size(descr.Width, descr.Height);

				if (descr.Format == SharpDX.DXGI.Format.NV12)
				{
					this.Format = PixFormat.NV12;
				}
				else if (descr.Format == SharpDX.DXGI.Format.R8G8B8A8_UNorm)
				{
					this.Format = PixFormat.RGB32;
				}

				this.DriverType = VideoDriverType.DirectX;
				this.initialized = true;
			}
		}

		public double time = 0;

		public IFrameBuffer[] FrameData { get; private set; }

		public Size FrameSize { get; private set; } = Size.Empty;
		public PixFormat Format { get; private set; } = PixFormat.RGB32;

		public VideoDriverType DriverType { get; private set; }

		public long DataLength { get; private set; } = -1;

		private readonly object syncRoot = new object();
		public bool Lock(int timeout)
		{
			bool lockTaken = false;
			Monitor.TryEnter(syncRoot, timeout, ref lockTaken);
			return lockTaken;
		}

		public void Unlock()
		{
			Monitor.Exit(syncRoot);
		}

		public void Dispose()
		{
			lock (syncRoot)
			{
				if (needCleanup)
				{
					for (int i = 0; i < FrameData.Length; i++)
					{
						var data = FrameData[i];

						if (data.Data != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(data.Data);
						}
					}
				}
			}
		}
	}


}
