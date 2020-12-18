using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace Test.Encoder
{
    class FFmpegPixConverterTest
    {
        public unsafe static void Run()
        {

            Console.WriteLine("FFmpegPixConverterTest::Run()");

            //var inputFileName = @"Files\1920x1080.bmp";
            //Bitmap bmp = new Bitmap(inputFileName);

            //var b = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //var g = Graphics.FromImage(b);
            //g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
            //g.Dispose();
            //bmp.Dispose();

            //bmp = b;
            //bmp.Save("TEST.bmp");

            //var inputFileName = @"Files\NV12_352x288.yuv";
            //var inputFileName = @"Files\rgba_352x288.raw";
            //Size srcSize = new Size(352, 288);


            //var inputFileName = @"Files\rgba_1920x1080.raw";

            //var inputFileName = @"Files\NV12_1920x1080.yuv";

            var inputFileName = @"Files\rgba_1920x1080.raw";

            Size srcSize = new Size(1920, 1080);


            //var inputFileName = @"Files\RGB32_1280x720.raw";
            //Size srcSize = new Size(1280, 720);
            //Size srcSize = new Size(352, 288);

            //Size srcSize = new Size(351, 280);
            MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB32;

			//var a = MediaToolkit.Utils.GraphicTools.Align(176, 32);
			//Console.WriteLine(a); 
			//var inputFileName = @"Files\RGB24_1280x720.raw";
			//Size srcSize = new Size(1280, 720);
			//MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB24;


			//var inputFileName = @"Files\RGB565_1280x720.raw";
			//Size srcSize = new Size(1280, 720);
			//MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB565;


			var srcBytes = File.ReadAllBytes(inputFileName);

			// Size destSize = new Size(3840, 2160);
			//Size destSize = new Size(2560, 1440);
			// Size destSize = new Size(1900, 1000);
			//Size destSize = new Size(1280, 720);
			// Size destSize = new Size(640, 480);
			//Size destSize = new Size(352, 288);
			//Size destSize = new Size(393, 211);
			Size destSize = new Size(555, 333);
			destSize = MediaToolkit.Utils.GraphicTools.DecreaseToEven(destSize);

            MediaToolkit.Core.PixFormat destFormat = MediaToolkit.Core.PixFormat.I420;


            FFmpegLib.FFmpegPixelConverter converter = new FFmpegLib.FFmpegPixelConverter();

            converter.Init(srcSize, srcFormat, destSize, destFormat, MediaToolkit.Core.ScalingFilter.Bicubic);

            byte[] destBuffer = null;
            Stopwatch sw = new Stopwatch();
            const int count = 1;
            int num = count;
            sw.Start();
            while (num-- > 0)
            {

                fixed (byte* ptr = srcBytes)
                {

                    converter.Convert((IntPtr)ptr, srcBytes.Length, out var destData, out var destLinesize);
                    // converter.Convert((IntPtr)ptr, 1280, out var destData, out var destLinesize);

                    destBuffer = ConvertToContiguousBuffer(destData, destLinesize, destSize, destFormat);

                    //converter.Convert2((IntPtr)ptr, 0, out destData);
                }
                //Thread.Sleep(33);

            }
            var time = sw.Elapsed;
            var frameInterval = time.TotalSeconds / count ;

            Console.WriteLine("TotalSec: " + time.TotalSeconds);
            Console.WriteLine("Interval: " + frameInterval + "\r\nFPS: " + 1.0/ frameInterval);

            if (destBuffer != null)
            {
				var outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
				if (!Directory.Exists(outputPath))
				{
					Directory.CreateDirectory(outputPath);

				}
                var fileName = destFormat + "_" + destSize.Width + "x" + destSize.Height + ".raw";
				var outputFile = Path.Combine(outputPath, fileName);
				
                Console.WriteLine("Output File: " + outputFile);
                File.WriteAllBytes(outputFile, destBuffer);

            }
            else
            {
                Console.WriteLine("!!!!!!!!!destData == null");
            }


            converter.Close();


        }

        private static unsafe byte[] ConvertToContiguousBuffer(IntPtr[] data, int[] linesize, Size size, MediaToolkit.Core.PixFormat format)
        {
            byte[] buffer = null;
            if (format == MediaToolkit.Core.PixFormat.NV12)
            {
				var lumaStride = size.Width;
				var lumaHeight = size.Height;

				var chromaStride = size.Width;
				var chromaHeight = size.Height / 2;

				//lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
				//chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);
				var bufferSize = lumaStride * lumaHeight + chromaStride * chromaHeight;
                buffer = new byte[bufferSize];

				int offset = 0;
				var pData = data[0];
				var dataStride = linesize[0];
				for (int row = 0; row < lumaHeight; row++)
				{ //Y
					Marshal.Copy(pData, buffer, offset, lumaStride);
					offset += lumaStride;
					pData += dataStride;
				}

				pData = data[1];
				dataStride = linesize[1];
				for (int row = 0; row < chromaHeight; row++)
				{// packed CbCr
					Marshal.Copy(pData, buffer, offset, chromaStride);
					offset += chromaStride;
					pData += dataStride;
				}
			}
            else if (format == MediaToolkit.Core.PixFormat.RGB32
               || format == MediaToolkit.Core.PixFormat.RGB24
               || format == MediaToolkit.Core.PixFormat.RGB565)
            {
				int bytesPerPixel = 4;		
				if(format == MediaToolkit.Core.PixFormat.RGB24)
				{
					bytesPerPixel = 3;
				}
				else if (format == MediaToolkit.Core.PixFormat.RGB565)
				{
					bytesPerPixel = 2;
				}

				var rgbStride = bytesPerPixel * size.Width;

				var bufferSize = rgbStride * size.Height;
                buffer = new byte[bufferSize];

                int offset = 0;
                var pData = data[0];
                var dataStride = linesize[0];

				for (int row = 0; row < size.Height; row++)
				{
					Marshal.Copy(pData, buffer, offset, rgbStride);
					offset += rgbStride;
					pData += dataStride;
				}
			}
            else if (format == MediaToolkit.Core.PixFormat.I444 ||
                format == MediaToolkit.Core.PixFormat.I422 ||
                format == MediaToolkit.Core.PixFormat.I420)
            {
                var lumaHeight = size.Height;
                var chromaHeight = size.Height;

                var lumaStride = size.Width;
                var chomaStride = size.Width;

                if (format == MediaToolkit.Core.PixFormat.I420)
                {
                    chromaHeight = size.Height / 2;
                    chomaStride = size.Width / 2;

				}
                else if (format == MediaToolkit.Core.PixFormat.I422)
                {
                    chomaStride = size.Width / 2;
                }

				//lumaStride = MediaToolkit.Utils.GraphicTools.Align(lumaStride, 16);
				//chomaStride = MediaToolkit.Utils.GraphicTools.Align(chomaStride, 16);

				var bufferSize = lumaStride * lumaHeight + 2 * chomaStride * chromaHeight;

                buffer = new byte[bufferSize];

                int offset = 0;
                var pData = data[0];
                var dataStride = linesize[0];

                for (int row = 0; row < lumaHeight; row++)
                {//Y
                    Marshal.Copy(pData, buffer, offset, lumaStride);
                    offset += lumaStride;
                    pData += dataStride;
                }

                for (int i = 1; i < 3; i++)
                {// CbCr
                    pData = data[i];
                    dataStride = linesize[i];
                    for (int row = 0; row < chromaHeight; row++)
                    {
                        Marshal.Copy(pData, buffer, offset, chomaStride);
                        offset += chomaStride;
                        pData += dataStride;
                    }
                    
                }
            }


            return buffer;
        }


    }
}
