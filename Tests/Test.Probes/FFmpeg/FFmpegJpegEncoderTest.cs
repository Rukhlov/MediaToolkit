using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class FFmpegJpegEncoderTest
	{

		public static void Run()
		{
			Console.WriteLine("FFmpegJpegEncoderTest::Run() BEGIN");

			//var fileName = @"Files\1920x1080.bmp";
			//var fileName = @"Files\rgba_640x480.bmp";
			var fileName = @"Files\2560x1440.bmp";
			
			Bitmap srcBitmap = new Bitmap(fileName);
			VideoBuffer video = new VideoBuffer(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

			var b = video.bitmap;

			var g = Graphics.FromImage(b);
			g.DrawImage(srcBitmap, 0, 0, b.Width, b.Height);
			g.Dispose();
			srcBitmap.Dispose();

			Console.WriteLine("InputFile: " + fileName);

			FFmpegLib.FFmpegVideoEncoder encoder = new FFmpegLib.FFmpegVideoEncoder();
			VideoEncoderSettings settings = new VideoEncoderSettings
			{
				EncoderId = "mjpeg",
				EncoderFormat = VideoCodingFormat.JPEG,
				Width = 1920,
				Height = 1080,
				FrameRate = new MediaRatio(1, 1),
			};

			encoder.Open(settings);

			encoder.DataEncoded += (ptr, size, time) =>
			{

				var path = AppDomain.CurrentDomain.BaseDirectory + "Jpeg";
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				var outputFile = Path.Combine(path, "full_FFjpeg.jpg");

				MediaToolkit.Utils.TestTools.WriteFile(ptr, size, outputFile);

				Console.WriteLine("OutputFile: " + outputFile);

			};
			encoder.Encode(video);

			encoder.Close();

			Console.WriteLine("FFmpegJpegEncoderTest::Run() END");

		}


	}
}
