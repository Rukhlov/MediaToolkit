using MediaToolkit.Nvidia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Probe.NvApi
{
	class NvDecTest
	{
		public static void Run()
		{

			LibCuda.Initialize();
			var descriptions = CuDevice.GetDescriptions().ToArray();
			
			foreach(var d in descriptions)
			{
				var pciBusId = d.GetPciBusId();
				Console.WriteLine(string.Join(" ", d.Name, d.TotalMemory, pciBusId));
			}

			var parserParams = new CuVideoParserParams
			{
				CodecType = CuVideoCodec.H264,
				MaxNumDecodeSurfaces = 1,
				MaxDisplayDelay = 0,
				ErrorThreshold = 100,
				UserData = IntPtr.Zero,
				//DisplayPicture = VideoDisplayCallback,
				//DecodePicture = DecodePictureCallback,
				//SequenceCallback = SequenceCallback,

			};

			var parser = CuVideoParser.Create(ref parserParams);

			
			parser.Dispose();
		}
	}

}
