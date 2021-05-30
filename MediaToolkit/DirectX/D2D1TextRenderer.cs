using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;

namespace MediaToolkit.DirectX
{
	public class D2D1TextRenderer
	{

		private SharpDX.DirectWrite.TextFormat textFormat = null;
		private Direct2D.SolidColorBrush foreBrush = null;
		private Direct2D.SolidColorBrush backBrush = null;
		private Direct2D.DeviceContext d2dContext = null;
		private SharpDX.DirectWrite.Factory dwriteFactory = null;
		private Direct2D.Bitmap1 targetBitmap = null;

		public GDI.Rectangle SrcRect { get; set; }

		private float fontSize = 16f;
		private string fontFamilyName = "Calibri";
		private bool initialized = false;

        private SharpDX.Direct3D11.Device device = null;

        public void Init(SharpDX.Direct3D11.Device device, Texture2D texture, GDI.Font gdiFont, GDI.Color gdiForeColor, GDI.Color gdiBackColor)
		{

			var srcDescr = texture.Description;
            this.device = device;
			using (Direct2D.Factory1 factory2D1 = new Direct2D.Factory1(Direct2D.FactoryType.MultiThreaded))
			{
				using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
				{
					using (var d2dDevice = new Direct2D.Device(factory2D1, dxgiDevice))
					{
						d2dContext = new Direct2D.DeviceContext(d2dDevice, Direct2D.DeviceContextOptions.None);

						var bitmapProperties = new Direct2D.BitmapProperties1(
						new SharpDX.Direct2D1.PixelFormat(srcDescr.Format, Direct2D.AlphaMode.Premultiplied),
						96, 96,
						Direct2D.BitmapOptions.Target | Direct2D.BitmapOptions.CannotDraw);

						using (var surf = texture.QueryInterface<Surface>())
						{
							targetBitmap = new Direct2D.Bitmap1(d2dContext, surf, bitmapProperties);

							d2dContext.Target = targetBitmap;
							d2dContext.TextAntialiasMode = Direct2D.TextAntialiasMode.Grayscale;
							d2dContext.DotsPerInch = new Size2F(96, 96);

						}
					}
				}
			}

			dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

			Color foreColor = new Color(gdiForeColor.R, gdiForeColor.G, gdiForeColor.B, gdiForeColor.A);
			foreBrush = new Direct2D.SolidColorBrush(d2dContext, foreColor);

			Color backColor = new Color(gdiBackColor.R, gdiBackColor.G, gdiBackColor.B, gdiBackColor.A);
			backBrush = new Direct2D.SolidColorBrush(d2dContext, backColor);

			// int fontSize = (int)(Math.Min(SrcRect.Width, SrcRect.Height) / 8);

			fontSize = gdiFont.Size;
			fontFamilyName = gdiFont.FontFamily.Name;

			//textFormat = new SharpDX.DirectWrite.TextFormat(dwriteFactory, fontFamilyName, fontSize)
			//{
			//   // TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
			//    TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
			//    ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near,
			//    //FlowDirection = SharpDX.DirectWrite.FlowDirection.TopToBottom,
			//    WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap,
			//};

			initialized = true;
		}

        public void DrawText(string text, GDI.Point pos, float fontSize)
        {
            lock (device)
            {
                DrawTextInternal(text, pos, fontSize);
            }
        }

        private void DrawTextInternal(string text, GDI.Point pos, float fontSize)
		{
			if (!initialized)
			{
				//invalid state...
				return;
			}

			d2dContext.BeginDraw();
			d2dContext.Transform = Matrix3x2.Identity;
			//d2dContext.Transform = Matrix3x2.Scaling(scaleX, scaleY);

			//SharpDX.Mathematics.Interop.RawRectangleF rect = new SharpDX.Mathematics.Interop.RawRectangleF
			//{
			//    Left = pos.X,
			//    Top = pos.Y,
			//    Right = 128,
			//    Bottom = 32,
			//};
			//d2dContext.FillRectangle(rect, backBrush);
			//d2dContext.DrawText(text, textFormat, rect, foreBrush);


			SharpDX.DirectWrite.TextFormat format = null;
			try
			{
				format = new SharpDX.DirectWrite.TextFormat(dwriteFactory, fontFamilyName, fontSize)
				{
					// TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
					TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
					ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Near,
					//FlowDirection = SharpDX.DirectWrite.FlowDirection.TopToBottom,
					WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap,
				};

				using (var layout = new SharpDX.DirectWrite.TextLayout(dwriteFactory, text, format, 0, 0))
				{
					var metrics = layout.Metrics;

					SharpDX.Mathematics.Interop.RawRectangleF rect = new SharpDX.Mathematics.Interop.RawRectangleF
					{
						Left = metrics.Left,
						Top = metrics.Top,
						Right = metrics.Width - metrics.Left,
						Bottom = metrics.Height - metrics.Top,
					};
					d2dContext.FillRectangle(rect, backBrush);

					var origin = new Vector2(pos.X, pos.Y);
					d2dContext.DrawTextLayout(origin, layout, foreBrush, Direct2D.DrawTextOptions.None);
					//d2dContext.Flush();
				}
			}
			finally
			{
				DxTool.SafeDispose(format);
			}

			d2dContext.EndDraw();
		}

		public void Close()
		{
			DxTool.SafeDispose(d2dContext);
			DxTool.SafeDispose(textFormat);
			DxTool.SafeDispose(foreBrush);
			DxTool.SafeDispose(backBrush);
			DxTool.SafeDispose(dwriteFactory);
			DxTool.SafeDispose(targetBitmap);

			initialized = false;
		}

	}


}
