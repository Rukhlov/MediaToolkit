using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Test.Encoder
{
	class CopyImageTest
	{

		public static void Run()
		{

            string fileName = @"Files\rgba_3840x2160.raw";
            int width = 3840;
            int height = 2160;
            //string fileName = @"Files\rgba_1920x1080.raw";
            //int width = 1920;
            //int height = 1080;

            //string fileName = @"Files\RGB32_1280x720.raw";
            //int width = 1280;
            //int height = 720;


            var bytes = File.ReadAllBytes(fileName);
			var size = bytes.Length;
			int srcStride = width * 4;
			int widthInBytes = width * 4;
			int lines = height;

			IntPtr srcPtr = Marshal.AllocHGlobal(size);
			Marshal.Copy(bytes, 0, srcPtr, size);

			IntPtr destPtr = Marshal.AllocHGlobal(size);
			int destStride = srcStride;

			int count = 1000;
			int index = count;
			Stopwatch sw = Stopwatch.StartNew();
			while (index-- > 0)
			{
				var dest = destPtr;
				var src = srcPtr;
				SharpDX.MediaFoundation.MediaFactory.CopyImage(dest, destStride, src, srcStride, widthInBytes, lines);
			}
			var msec = sw.ElapsedMilliseconds;
			double timePerCopy = (double)msec/ count ;

			Console.WriteLine("MediaFactory.CopyImage(): " + msec + " " + timePerCopy);

			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				MediaToolkit.Utils.GraphicTools.CopyImage(destPtr, destStride, srcPtr, srcStride, widthInBytes, lines);
			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec/ count ;
			Console.WriteLine("GraphicTools.CopyImage(): " + msec + " " + timePerCopy);

			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				MediaToolkit.NativeAPIs.Kernel32.CopyMemory(destPtr, srcPtr, (uint)size);

			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("Kernel32.CopyMemory: " + msec + " " + timePerCopy);

			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				SharpDX.Utilities.CopyMemory(destPtr, srcPtr, size);

			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("SharpDX.Utilities.CopyMemory: " + msec + " " + timePerCopy);

            index = count;
            sw.Restart();
            while (index-- > 0)
            {
                var dest = destPtr;
                Marshal.Copy(bytes, 0, dest, bytes.Length);
            }
            msec = sw.ElapsedMilliseconds;
            timePerCopy = (double)msec / count;
            Console.WriteLine("Marshal.Copy: " + msec + " " + timePerCopy);

            index = count;
			sw.Restart();
			while (index-- > 0)
			{
				var dest = destPtr;
				var src = srcPtr;
				for (int i = 0; i < lines; i++)
				{
					SharpDX.Utilities.CopyMemory(dest, src, widthInBytes);
					src += srcStride;
					dest += destStride;
				}
			}

			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("SharpDX.Utilities.CopyMemory ByLine: " + msec + " " + timePerCopy);

			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				var dest = destPtr;
				var src = srcPtr;
				SharpDX.MediaFoundation.MediaFactory.CopyImage(dest, destStride, src, srcStride, widthInBytes, lines);
			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("MediaFactory.CopyImage()2: " + msec + " " + timePerCopy);

			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				MediaToolkit.Utils.GraphicTools.CopyImage(destPtr, destStride, srcPtr, srcStride, widthInBytes, lines);
			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("GraphicTools.CopyImage()2: " + msec + " " + timePerCopy);


			index = count;
			sw.Restart();
			while (index-- > 0)
			{
				var dest = destPtr;
				var src = srcPtr;
				SharpDX.MediaFoundation.MediaFactory.CopyImage(dest, destStride, src, srcStride, widthInBytes, lines);
			}
			msec = sw.ElapsedMilliseconds;
			timePerCopy = (double)msec / count;
			Console.WriteLine("MediaFactory.CopyImage()3: " + msec + " " + timePerCopy);





            MediaToolkit.Utils.TestTools.WriteFile(destPtr, size, "ImageCopyTest.raw");

			Marshal.FreeHGlobal(srcPtr);
			Marshal.FreeHGlobal(destPtr);
		}
	}
}
