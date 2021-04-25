using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using System.IO;

using System.Threading;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
//using SharpDX.Direct2D1;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

using GDI = System.Drawing;
using Direct2D = SharpDX.Direct2D1;
//using MediaToolkit.Utils;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using MediaToolkit.Logging;

using System.Windows.Forms;
using SharpDX.Direct2D1;
using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using MediaToolkit.DirectX;
using MediaToolkit.Core;

namespace MediaToolkit.ScreenCaptures
{
	public class DummyRGBCapture : ScreenCapture
	{
		private GDI.Bitmap bitmap = null;
		private SharpDX.Direct3D11.Texture2D srcTexture = null;
		private SharpDX.Direct3D11.Texture2D sharedTexture = null;
		private SharpDX.Direct3D11.Device device = null;

		private Texture2D screenTexture = null;

		private SharpDX.DirectWrite.TextFormat textFormat = null;
		private Direct2D.SolidColorBrush sceneColorBrush = null;
		private SharpDX.Direct2D1.DeviceContext d2dContext = null;

		public override void Init(ScreenCaptureParameters captureParams)
		{
			base.Init(captureParams);

			this.device = captureParams.D3D11Device;

			if (device != null)
			{
				InitDx();
			}
			else
			{
				bitmap = new GDI.Bitmap(DestSize.Width, DestSize.Height, GDI.Imaging.PixelFormat.Format32bppArgb);
			}

		}

		private void InitDx()
		{
			using (GDI.Bitmap bmp = new GDI.Bitmap(SrcRect.Width, SrcRect.Height, GDI.Imaging.PixelFormat.Format32bppArgb))
			{
				using (var g = GDI.Graphics.FromImage(bmp))
				{
					g.FillRectangle(GDI.Brushes.Blue, 0, 0, bmp.Width, bmp.Height);
				}
				srcTexture = DxTool.GetTexture(bmp, device);
			}

			screenTexture = new Texture2D(device,
			  new Texture2DDescription
			  {
				  CpuAccessFlags = CpuAccessFlags.None,
				  BindFlags = BindFlags.RenderTarget,
				  Format = Format.B8G8R8A8_UNorm,
				  Width = SrcRect.Width,
				  Height = SrcRect.Height,
				  MipLevels = 1,
				  ArraySize = 1,
				  SampleDescription = { Count = 1, Quality = 0 },
				  Usage = ResourceUsage.Default,

				  OptionFlags = ResourceOptionFlags.None,

			  });

			using (SharpDX.Direct2D1.Factory1 factory2D1 = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded))
			{
				using (var dxgiDevice = device.QueryInterface<SharpDX.DXGI.Device>())
				{
					using (var d2dDevice = new SharpDX.Direct2D1.Device(factory2D1, dxgiDevice))
					{
						d2dContext = new SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.None);

						var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1(
						new SharpDX.Direct2D1.PixelFormat(screenTexture.Description.Format, SharpDX.Direct2D1.AlphaMode.Premultiplied),
						96, 96,
						SharpDX.Direct2D1.BitmapOptions.Target | SharpDX.Direct2D1.BitmapOptions.CannotDraw);

						using (var surf = screenTexture.QueryInterface<Surface>())
						{
							d2dContext.Target = new SharpDX.Direct2D1.Bitmap1(d2dContext, surf, bitmapProperties);
							d2dContext.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Grayscale;
							d2dContext.DotsPerInch = new SharpDX.Size2F(96, 96);

						}
					}
				}
			}

			dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
			
			sceneColorBrush = new Direct2D.SolidColorBrush(d2dContext, Color.Yellow);
            int fontSize = (int)(Math.Min(SrcRect.Width, SrcRect.Height) / 8);

			textFormat = new SharpDX.DirectWrite.TextFormat(dwriteFactory, "Calibri", fontSize)
			{
				TextAlignment = SharpDX.DirectWrite.TextAlignment.Center,
				ParagraphAlignment = SharpDX.DirectWrite.ParagraphAlignment.Center
            };


			sharedTexture = new SharpDX.Direct3D11.Texture2D(device,
				 new SharpDX.Direct3D11.Texture2DDescription
				 {
					 CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
					 BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | SharpDX.Direct3D11.BindFlags.ShaderResource,
					 Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
					 Width = SrcRect.Width,
					 Height = SrcRect.Height,
					 MipLevels = 1,
					 ArraySize = 1,
					 SampleDescription = { Count = 1, Quality = 0 },
					 Usage = SharpDX.Direct3D11.ResourceUsage.Default,
					 OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared,

				 });
		}
		private SharpDX.DirectWrite.Factory dwriteFactory = null;

        private Stopwatch stopwatch = new Stopwatch();
        private ulong drawCount = 0;

		public override ErrorCode TryGetFrame(out IVideoFrame frame, int timeout = 10)
		{
			frame = null;
			ErrorCode result = ErrorCode.Ok;

            DateTime time = DateTime.Now;
            drawCount++;
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            var fps = 1000.0 / elapsedMilliseconds;

			var text = SrcRect.Width + "x" + SrcRect.Height + "\r\n" +
                time.ToString("HH:mm:ss.fff") + "\r\n" + 
                "------------------------\r\n" +
                "FPS: " + fps.ToString("F1") + "\r\n" + 
                "Interval: " + elapsedMilliseconds +"\r\n" +
                "Count: " + drawCount;

            device.ImmediateContext.CopyResource(srcTexture, screenTexture);

            d2dContext.BeginDraw();
			d2dContext.Transform = Matrix3x2.Identity;

            using (var textLayout = new SharpDX.DirectWrite.TextLayout(dwriteFactory, text, textFormat, SrcRect.Width, SrcRect.Height))
            {
                d2dContext.DrawTextLayout(new Vector2(0, 0), textLayout, sceneColorBrush, DrawTextOptions.None);

            }

            d2dContext.EndDraw();


			device.ImmediateContext.CopyResource(screenTexture, sharedTexture);
			if (result == ErrorCode.Ok)
			{
				frame = new D3D11VideoFrame(sharedTexture);
			}

			return result;
		}


		public override void Close()
		{
			base.Close();

			CloseDx();

			if (bitmap != null)
			{
				bitmap.Dispose();
				bitmap = null;
			}
		}

		private void CloseDx()
		{
            DxTool.SafeDispose(screenTexture);
            DxTool.SafeDispose(srcTexture);
            DxTool.SafeDispose(sharedTexture);
            DxTool.SafeDispose(d2dContext);
            DxTool.SafeDispose(textFormat);
            DxTool.SafeDispose(sceneColorBrush);
            DxTool.SafeDispose(dwriteFactory);
		}
	}


}
