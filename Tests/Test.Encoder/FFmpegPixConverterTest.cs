using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

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

            //var inputFileName = @"Files\rgba_1920x1080.raw";

            //Size srcSize = new Size(1920, 1080);


            var inputFileName = @"Files\RGB32_1280x720.raw";
            Size srcSize = new Size(1280, 720);
            MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB32;

            //var inputFileName = @"Files\RGB24_1280x720.raw";
            //Size srcSize = new Size(1280, 720);
            //MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB24;


            //var inputFileName = @"Files\RGB565_1280x720.raw";
            //Size srcSize = new Size(1280, 720);
            //MediaToolkit.Core.PixFormat srcFormat = MediaToolkit.Core.PixFormat.RGB565;


            var srcBytes = File.ReadAllBytes(inputFileName);

            //Size destSize = new Size(1920, 1080);

            Size destSize = new Size(1280, 720);
            //Size destSize = new Size(640, 480);
            //Size destSize = new Size(352, 288);

            MediaToolkit.Core.PixFormat destFormat = MediaToolkit.Core.PixFormat.NV12;


            FFmpegLib.FFmpegPixelConverter converter = new FFmpegLib.FFmpegPixelConverter();

            converter.Init(srcSize, srcFormat, destSize, destFormat, MediaToolkit.Core.ScalingFilter.Bicubic);

            byte[] destBuffer = null;

            int count = 1;
            while (count-- > 0)
            {
                
                fixed (byte* ptr = srcBytes)
                {
                    converter.Convert((IntPtr)ptr, srcBytes.Length, out var destData, out var destLinesize);

                    destBuffer = ConvertToContiguousBuffer(destData, destLinesize, destSize, destFormat);

                    //converter.Convert2((IntPtr)ptr, 0, out destData);
                }

                Thread.Sleep(16);
            }


            if (destBuffer != null)
            {

                var outputFile = destFormat + "_" + destSize.Width + "x" + destSize.Height + ".raw";

                Console.WriteLine("Output File: " + outputFile);
                File.WriteAllBytes(outputFile, destBuffer);

            }
            else
            {
                Console.WriteLine("!!!!!!!!!destData == null");
            }


            converter.Close();


        }

        private static unsafe byte[] ConvertToContiguousBuffer( IntPtr[] pData, int[] pLinesize, Size size, MediaToolkit.Core.PixFormat format )
        {
            byte[] buffer = null;
            if (format == MediaToolkit.Core.PixFormat.NV12)
            {
                var bufferSize = pLinesize[0] * size.Height + pLinesize[1] * size.Height / 2;
                buffer = new byte[bufferSize];

                int offset = 0;
                var data = pData[0];
                var linesize = pLinesize[0];

                if (linesize > 0)
                {
                    var _size = linesize * size.Height;
                    Marshal.Copy(data, buffer, offset, _size);
                    offset += _size;
                }

                for (int i = 1; i < pData.Length; i++)
                {
                    data = pData[i];
                    linesize = pLinesize[i];

                    if (linesize > 0)
                    {
                        var _size = linesize * size.Height / 2;
                        Marshal.Copy(data, buffer, offset, _size);
                        offset += _size;
                    }

                }
            }
            else if (format == MediaToolkit.Core.PixFormat.RGB32
               || format == MediaToolkit.Core.PixFormat.RGB24
               || format == MediaToolkit.Core.PixFormat.RGB565)
            {
                var bufferSize = pLinesize[0] * size.Height;
                buffer = new byte[bufferSize];

                int offset = 0;
                var data = pData[0];
                var linesize = pLinesize[0];

                if (linesize > 0)
                {
                    var dataSize = linesize * size.Height;
                    Marshal.Copy(data, buffer, offset, dataSize);
                    offset += dataSize;
                }
            }
            else if (format == MediaToolkit.Core.PixFormat.I444 ||
                        format == MediaToolkit.Core.PixFormat.I422 ||
                        format == MediaToolkit.Core.PixFormat.I420)
            {// TODO: могут быть поразному выровнены...
                var lumaHeight = size.Height;
                var chromaHeight = size.Height;
                if (format == MediaToolkit.Core.PixFormat.I420)
                {
                    chromaHeight = size.Height / 2;
                }

                var bufferSize = pLinesize[0] * lumaHeight + pLinesize[1] * chromaHeight + pLinesize[2] * chromaHeight;
                buffer = new byte[bufferSize];

                int offset = 0;
                var data = pData[0];
                var linesize = pLinesize[0];

                if (linesize > 0)
                {
                    var dataSize = linesize * lumaHeight;
                    Marshal.Copy(data, buffer, offset, dataSize);
                    offset += dataSize;
                }

                for (int i = 1; i < pData.Length; i++)
                {
                    data = pData[i];
                    linesize = pLinesize[i];

                    if (linesize > 0)
                    {
                        var dataSize = linesize * chromaHeight;
                        Marshal.Copy(data, buffer, offset, dataSize);
                        offset += dataSize;
                    }

                }
            }


            return buffer;
        }
    }
}
