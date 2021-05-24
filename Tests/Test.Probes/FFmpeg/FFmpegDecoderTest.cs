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


			Action<IVideoFrame> OnDataDecoded = new Action<IVideoFrame>((frame) =>
              {
				  var frameBuffer = frame.Buffer;
				  var time = frame.Time;

                  var lumaSize = frameBuffer[0].Stride * height;
                  var CbSize = frameBuffer[1].Stride * height /2;
                  var CrSize = frameBuffer[2].Stride * height /2;

                  var totalSize = lumaSize + CbSize + CrSize;
                  byte[] _bytes = new byte[totalSize];
                  int offset = 0;
                  Marshal.Copy(frameBuffer[0].Data, _bytes, offset, lumaSize);
                  offset += lumaSize;

                  Marshal.Copy(frameBuffer[1].Data, _bytes, offset, CbSize);
                  offset += CbSize;

                  Marshal.Copy(frameBuffer[2].Data, _bytes, offset, CrSize);
                  offset += CrSize;
                  var decodedName = width + "x" + height + "_yuv420p_" + time + ".yuv";
                 
                  File.WriteAllBytes(decodedName, _bytes);
                  Console.WriteLine(decodedName);

                  //foreach (var b in buff)
                  //{
                  //    Console.WriteLine(b.Stride);
                  //}

              });

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
                    decoder.Decode(data.ToArray(), sec, OnDataDecoded);
                    sec += 0.033;

                    data = new List<byte>();
                }
            }
			if (decoder.Drain())
			{
				int decodeResult = 0;
				while (decodeResult == 0)
				{
					decodeResult = decoder.Decode(null, 0, OnDataDecoded);
				}
			}

			decoder.Close();

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
