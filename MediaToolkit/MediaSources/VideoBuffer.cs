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

	public class DxVideoBuffer :_VideoBuffer
	{
		DxVideoBuffer(IntPtr ptr)
		{
			this.pTexture = ptr;
			using (SharpDX.Direct3D11.Texture2D texture = new SharpDX.Direct3D11.Texture2D(ptr))
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
			}
		}

		private IntPtr pTexture = IntPtr.Zero;

		public override VideoDriverType DriverType => VideoDriverType.DirectX; 

		public override void Dispose()
		{

		}

	}

	public class MemoryVideoBuffer : _VideoBuffer
	{
		MemoryVideoBuffer(Size size, PixFormat format, int align = 32)
		{
			this.FrameSize = size;
			this.Format = format;

			this.DataLength = FFmpegLib.Utils.AllocImageData(size, format, align, out var frameData);
			this.FrameData = frameData;
		}

		public override VideoDriverType DriverType => VideoDriverType.CPU;
		public override void Dispose()
		{
			lock (syncRoot)
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


	public abstract class _VideoBuffer
	{

		public double time = 0;

		public IFrameBuffer[] FrameData { get; protected set; }

		public Size FrameSize { get; protected set; } = Size.Empty;
		public PixFormat Format { get; protected set; } = PixFormat.RGB32;

		public abstract VideoDriverType DriverType { get; }

		public long DataLength { get; protected set; } = -1;

		protected readonly object syncRoot = new object();
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
		public abstract void Dispose();

	}


}
