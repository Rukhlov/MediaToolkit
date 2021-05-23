using MediaToolkit.Codecs;
using MediaToolkit.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class FFmpegDecoderTest
	{

		public static void Run()
		{
			Console.WriteLine("FFmpegDecoderTest::Run() BEGIN");
           
            string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_640x480_yuv420p_4IPframe.h264";
            //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_640x480_yuv420p_4frame.h264";
            //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_640x480_nv12_1frame.h264";
            //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_Iframe.h264";
            var width = 640;
            var height = 480;

            FFmpegLib.H264Decoder decoder = new FFmpegLib.H264Decoder();
			VideoEncoderSettings settings = new VideoEncoderSettings
			{
				EncoderFormat = VideoCodingFormat.H264,
				Width = width,
				Height = height,
			};
            decoder.DataDecoded += (frame, time) =>
              {
                  var lumaSize = frame[0].Stride * height;
                  var CbSize = frame[1].Stride * height /2;
                  var CrSize = frame[2].Stride * height /2;

                  var totalSize = lumaSize + CbSize + CrSize;
                  byte[] _bytes = new byte[totalSize];
                  int offset = 0;
                  Marshal.Copy(frame[0].Data, _bytes, offset, lumaSize);
                  offset += lumaSize;

                  Marshal.Copy(frame[1].Data, _bytes, offset, CbSize);
                  offset += CbSize;

                  Marshal.Copy(frame[2].Data, _bytes, offset, CrSize);
                  offset += CrSize;
                  var decodedName = width + "x" + height + "_yuv420p_" + time + ".yuv";
                 
                  File.WriteAllBytes(decodedName, _bytes);
                  Console.WriteLine(decodedName);

                  //foreach (var b in buff)
                  //{
                  //    Console.WriteLine(b.Stride);
                  //}

              };

            decoder.Setup(settings);
            var bytes = File.ReadAllBytes(fileName);
           var nals = NalUnitReader.HandleH264AnnexbFrames(bytes);
            IEnumerable<byte> data = new List<byte>();
            var startCodes = new byte[] { 0, 0, 0, 1 };
            double sec = 0;
            foreach (var nal in nals)
            {
                var n = DumpSegemnt(nal);
                var firstByte = n[0];

                data = data.Concat(startCodes).Concat(n);

                var nalUnitType = firstByte & 0x1F;

                if (nalUnitType == (int)NalUnitType.IDR || nalUnitType == (int)NalUnitType.Slice)
                {
                    decoder.Decode(data.ToArray(), sec);
                    sec += 0.033;

                    data = new List<byte>();
                }
            }

           
            decoder.Close();

			////var fileName = @"Files\1920x1080.bmp";
			////var fileName = @"Files\rgba_640x480.bmp";
			//var fileName = @"Files\2560x1440.bmp";

			//Bitmap srcBitmap = new Bitmap(fileName);
			//VideoBuffer video = new VideoBuffer(1920, 1080, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

			//var b = video.bitmap;

			//var g = Graphics.FromImage(b);
			//g.DrawImage(srcBitmap, 0, 0, b.Width, b.Height);
			//g.Dispose();
			//srcBitmap.Dispose();

			//Console.WriteLine("InputFile: " + fileName);

			//FFmpegLib.FFmpegVideoEncoder encoder = new FFmpegLib.FFmpegVideoEncoder();
			//VideoEncoderSettings settings = new VideoEncoderSettings
			//{
			//	EncoderId = "mjpeg",
			//	EncoderFormat = VideoCodingFormat.JPEG,
			//	Width = 1920,
			//	Height = 1080,
			//	FrameRate = new MediaRatio(1, 1),
			//};

			//encoder.Open(settings);

			//encoder.DataEncoded += (ptr, size, time) =>
			//{

			//	var path = AppDomain.CurrentDomain.BaseDirectory + "Jpeg";
			//	if (!Directory.Exists(path))
			//	{
			//		Directory.CreateDirectory(path);
			//	}

			//	var outputFile = Path.Combine(path, "full_FFjpeg.jpg");

			//	MediaToolkit.Utils.TestTools.WriteFile(ptr, size, outputFile);

			//	Console.WriteLine("OutputFile: " + outputFile);

			//};
			//encoder.Encode(video);

			//encoder.Close();

			Console.WriteLine("FFmpegDecoderTest::Run() END");

		}

        private static byte[] DumpSegemnt(ArraySegment<byte> s)
        {
            var bytes = new byte[s.Count];
            Array.Copy(s.Array, s.Offset, bytes, 0, bytes.Length);

            return bytes;

        }

    }
}
