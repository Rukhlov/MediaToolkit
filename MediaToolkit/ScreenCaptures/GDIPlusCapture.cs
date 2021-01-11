using MediaToolkit.Core;
using MediaToolkit.SharedTypes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit.ScreenCaptures
{
	class GDIPlusCapture
	{
		public static bool GrabScreen(Rectangle captArea, ref Bitmap bmp)
		{
			bool result = false;

			Size srcSize = captArea.Size;
			Size destSize = new Size(bmp.Width, bmp.Height);

			if (srcSize == destSize)
			{
				Graphics g = Graphics.FromImage(bmp);
				try
				{
					g.CopyFromScreen(captArea.Left, captArea.Top, 0, 0, srcSize, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
					result = true;
				}
				finally
				{
					g.Dispose();
					g = null;
				}
			}
			else
			{// очень медленно лучше не использовать
				Bitmap buf = new Bitmap(srcSize.Width, srcSize.Height);
				try
				{
					Graphics g = Graphics.FromImage(buf);
					try
					{
						g.CopyFromScreen(captArea.Left, captArea.Top, 0, 0, srcSize,
							CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

						Graphics _g = Graphics.FromImage(bmp);
						try
						{
							//_g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
							//_g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
							//_g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

							_g.DrawImage(buf, 0, 0);
							result = true;
						}
						finally
						{
							_g.Dispose();
							_g = null;
						}
					}
					finally
					{
						g.Dispose();
						g = null;
					}
				}
				finally
				{
					if (buf != null)
					{
						buf.Dispose();
						buf = null;
					}
				}
			}

			return result;
		}


		public static Bitmap GetPrimaryScreen()
		{
			return GetScreen(System.Windows.Forms.Screen.PrimaryScreen.Bounds);
		}

		public static Bitmap GetScreen(Rectangle rect)
		{
			Size size = new Size(rect.Width, rect.Height);
			Bitmap bmp = new Bitmap(rect.Width, rect.Height);
			Graphics g = Graphics.FromImage(bmp);
			try
			{
				g.CopyFromScreen(rect.Left, rect.Top, 0, 0, size, CopyPixelOperation.SourceCopy);
			}
			finally
			{
				g.Dispose();
				g = null;
			}

			return bmp;
		}
	}

}
