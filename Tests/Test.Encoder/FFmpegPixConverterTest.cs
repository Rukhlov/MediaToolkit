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
using MediaToolkit.Core;
using MediaToolkit;

namespace Test.Encoder
{
    class FFmpegPixConverterTest
    {
        public unsafe static void Run()
        {

            Console.WriteLine("FFmpegPixConverterTest::Run()");

			var srcLinesize = 0;
			//var inputFileName = @"Files\1920x1080.bmp";
			//Bitmap bmp = new Bitmap(inputFileName);

			//var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
			//	System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

			//var _size = new Size(bmpData.Width, bmpData.Height);
			//var src = bmpData.Scan0;
			//	var _totalSize = FFmpegLib.Utils.FillImageData(src, _size, MediaToolkit.Core.PixFormat.RGB24, 32,
			//	out var _destData, out var _destLinesize);

			//byte[] srcBytes = new byte[_totalSize];
			//Marshal.Copy(_destData[0], srcBytes, 0, _totalSize);
			//var srcLinesize = _destLinesize[0];

			//bmp.UnlockBits(bmpData);

			//var b = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			//var g = Graphics.FromImage(b);
			//g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);
			//g.Dispose();
			//bmp.Dispose();

			//bmp = b;
			//bmp.Save("TEST.bmp");

			//int _count = 10000;
			//while (_count-- > 0)
			//{
			//	var totalSize = FFmpegLib.Utils.AllocImageData(new Size(1920, 1080), MediaToolkit.Core.PixFormat.NV12, 16, out var frameBuffer);
			//	byte[] _test = { 0x00, 0x01, 0x01, 0x01 };
			//	Marshal.Copy(_test, 0, frameBuffer[0].Data, _test.Length);

			//	Marshal.Copy(_test, 0, frameBuffer[1].Data, _test.Length);

			//	FFmpegLib.Utils.FreeImageData(ref frameBuffer);

			//	Thread.Sleep(10);
			//}



			//var inputFileName = @"Files\NV12_352x288.yuv";
			//var inputFileName = @"Files\rgba_352x288.raw";
			//Size srcSize = new Size(352, 288);


			var inputFileName = @"Files\rgba_1920x1080.raw";

			//var inputFileName = @"Files\NV12_1920x1080.yuv";

			//var inputFileName = @"Files\rgba_1920x1080.raw";

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


            //int offset = 0;
            //var destPtr = _destData[0];
            //var destPitch = _destLinesize[0];//4 * 1920;
            //var destRowNumber = 1080;
            //var destBufferSize = destPitch * destRowNumber;

            ////var destPitch = frameBuffer[0].Stride;
            //int bufOffset = 0;
            //for (int i = 0; i < destRowNumber; i++)
            //{
            //     System.Runtime.InteropServices.Marshal.Copy(srcBytes, offset, destPtr, destPitch);

            //    bufOffset += destPitch;
            //    destPtr += destPitch;
            //}




           // Size destSize = new Size(1920, 1080);
			// Size destSize = new Size(3840, 2160);
			//Size destSize = new Size(2560, 1440);
			// Size destSize = new Size(1900, 1000);
			//Size destSize = new Size(1280, 720);
			//Size destSize = new Size(640, 480);
			Size destSize = new Size(352, 288);
			//Size destSize = new Size(393, 211);
			//Size destSize = new Size(555, 333);
			destSize = MediaToolkit.Utils.GraphicTools.DecreaseToEven(destSize);

            MediaToolkit.Core.PixFormat destFormat = MediaToolkit.Core.PixFormat.I444;


            FFmpegLib.FFmpegPixelConverter converter = new FFmpegLib.FFmpegPixelConverter();



			converter.Init(srcSize, srcFormat, destSize, destFormat, MediaToolkit.Core.ScalingFilter.Bicubic);

			IVideoFrame destFrame = new VideoFrame(destSize.Width, destSize.Height, destFormat, 16);


			byte[] destBuffer = null;
            Stopwatch sw = new Stopwatch();
            const int count = 1;
            int num = count;
            sw.Start();
            while (num-- > 0)
            {
				fixed (byte* ptr = srcBytes)
				{
					var destTotalSize = FFmpegLib.Utils.FillImageData((IntPtr)ptr, srcSize, srcFormat, 16, out var srcData, out var _srcLinesize);
					IFrameBuffer[] srcBuffer = new FrameBuffer[srcData.Length];
					for (int i = 0; i < srcBuffer.Length; i++)
					{
						srcBuffer[i] = new FrameBuffer(srcData[i], _srcLinesize[i]);
					}

					IVideoFrame srcFrame = new VideoFrame(srcBuffer, destTotalSize, srcSize.Width, srcSize.Height, srcFormat, 16);

					converter.Convert(srcFrame, destFrame);

					destBuffer = ConvertToContiguousBuffer(destFrame.Buffer, destSize, destFormat);
				}

				

				//fixed (byte* ptr = srcBytes)
				//{
				//    converter.Convert((IntPtr)ptr, srcLinesize, 32, out var destData);

				//    //converter.Convert((IntPtr)ptr, srcLinesize, out var destData, out var destLinesize);
				//    // converter.Convert((IntPtr)ptr, 1280, out var destData, out var destLinesize);

				//    destBuffer = ConvertToContiguousBuffer(destData, destSize, destFormat);

				//    //converter.Convert2((IntPtr)ptr, 0, out destData);
				//}
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

        private static unsafe byte[] ConvertToContiguousBuffer(IFrameBuffer[] frameBuffer, Size size, MediaToolkit.Core.PixFormat format)
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
				var pData = frameBuffer[0].Data;
				var dataStride = frameBuffer[0].Stride;
				for (int row = 0; row < lumaHeight; row++)
				{ //Y
					Marshal.Copy(pData, buffer, offset, lumaStride);
					offset += lumaStride;
					pData += dataStride;
				}

				pData = frameBuffer[1].Data;
				dataStride = frameBuffer[1].Stride;
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
                var pData = frameBuffer[0].Data;
                var dataStride = frameBuffer[0].Stride;

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
                var pData = frameBuffer[0].Data;
                var dataStride = frameBuffer[0].Stride;

                for (int row = 0; row < lumaHeight; row++)
                {//Y
                    Marshal.Copy(pData, buffer, offset, lumaStride);
                    offset += lumaStride;
                    pData += dataStride;
                }

                for (int i = 1; i < 3; i++)
                {// CbCr
                    pData = frameBuffer[i].Data;
                    dataStride = frameBuffer[i].Stride;
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
