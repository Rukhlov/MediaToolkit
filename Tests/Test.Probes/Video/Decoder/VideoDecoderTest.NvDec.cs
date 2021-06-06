using MediaToolkit.DirectX;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Nvidia;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Probe
{
	public partial class VideoDecoderTest
	{


		private CuContext cuContext;
		private CuVideoDecoder cuVideoDecoder;
		private CuVideoContextLock videoContextLock;
		private CuVideoDecodeCreateInfo decodeInfo;


		private void NvDecDecoderTask(MfVideoArgs inputArgs)
		{

			LibCuda.Initialize();

			//var descriptions = CuDevice.GetDescriptions().ToArray();
			//foreach (var d in descriptions)
			//{
			//    var pciBusId = d.GetPciBusId();
			//    Console.WriteLine(string.Join(" ", d.Name, d.TotalMemory, pciBusId));
			//}
			//var device = descriptions[0].Device;

			CuDevice device;
			using (var dxgiDevice = device3D11.QueryInterface<SharpDX.DXGI.Device>())
			{
				using (var a = dxgiDevice.Adapter)
				{
					var result = LibCuda.D3D11GetDevice(out device, a.NativePointer);

					LibCuda.CheckResult(result, "D3D11GetDevice");
				}
			}

			//if (device.IsEmpty)
			//{
			//	throw new Exception("device.IsEmpty");
			//}

			//_context = device.CreateContext(CuContextFlags.Default);
			cuContext = device.CreateContext(CuContextFlags.SchedBlockingSync);
			videoContextLock = cuContext.CreateLock();
			

			var parserParams = new CuVideoParserParams
			{
				CodecType = CuVideoCodec.H264,
				MaxNumDecodeSurfaces = 1,
				MaxDisplayDelay = 0,
				ErrorThreshold = 100,
				UserData = IntPtr.Zero,
				SequenceCallback = SequenceCallback,
				DecodePicture = DecodePictureCallback,
				DisplayPicture = VideoDisplayCallback,
				
			};


			CuVideoParser parser = CuVideoParser.Create(ref parserParams);
			try
			{
				rgbProcessor = new RgbProcessor();
				var size = new System.Drawing.Size(inputArgs.Width, inputArgs.Height);
				rgbProcessor.Init(device3D11, size, MediaToolkit.Core.PixFormat.NV12, size, MediaToolkit.Core.PixFormat.RGB32);


				while (sourceReader.IsFull)
				//while (sourceReader.Count < 2)
				{
					Thread.Sleep(1);
					if (!running)
					{
						break;
					}
				}


				while (running)
				{

					while (sourceReader.PacketsAvailable)
					{
						bool packetTaken = sourceReader.TryGetPacket(out var packet, 10);
						if (!packetTaken)
						{
							Console.WriteLine("packet == false");
							continue;
						}

						var flags = CuVideoPacketFlags.Timestamp;
						long timestamp = (long)(packet.time * 10_000_000);
						parser.ParseVideoData(packet.data, flags, timestamp);
					}
				}
			}
			finally
			{
				parser.Dispose();

				if (rgbProcessor != null)
				{
					rgbProcessor.Close();
					rgbProcessor = null;
				}

				if (!lumaResource.IsEmpty)
				{
					lumaResource.Dispose();
				}

				if (lumaTexture != null)
				{
					lumaTexture.Dispose();
					lumaTexture = null;
				}

				if (!chromaResource.IsEmpty)
				{
					chromaResource.Dispose();
				}

				if (chromaTexture != null)
				{
					chromaTexture.Dispose();
					chromaTexture = null;
				}

			}

		}

		private CuCallbackResult SequenceCallback(IntPtr userData, ref CuVideoFormat format)
		{
			Console.WriteLine(">>>>>>>>>>>>>>>>>>>> SequenceCallback(...)");


			if (!format.IsSupportedByDecoder(out var error, out var caps))
			{
				Console.WriteLine(error);

				return CuCallbackResult.Failure;
			}

			if (!cuVideoDecoder.IsEmpty)
			{
				cuVideoDecoder.Reconfigure(ref format);

				return CuCallbackResult.Success;
			}

			decodeInfo = new CuVideoDecodeCreateInfo
			{
				CodecType = format.Codec,
				ChromaFormat = format.ChromaFormat,
				OutputFormat = format.GetSurfaceFormat(),
				BitDepthMinus8 = format.BitDepthLumaMinus8,
				DeinterlaceMode = format.ProgressiveSequence ? CuVideoDeinterlaceMode.Weave : CuVideoDeinterlaceMode.Adaptive,
				NumOutputSurfaces = 2,
				//CreationFlags = CuVideoCreateFlags.PreferCUDA,
				CreationFlags = CuVideoCreateFlags.PreferCUVID,
				NumDecodeSurfaces = format.MinNumDecodeSurfaces,
				VideoLock = videoContextLock,
				Width = format.CodedWidth,
				Height = format.CodedHeight,
				MaxWidth = format.CodedWidth,
				MaxHeight = format.CodedHeight,
				TargetWidth = format.CodedWidth,
				TargetHeight = format.CodedHeight,
				

			};

			cuVideoDecoder = CuVideoDecoder.Create(ref decodeInfo);

           return (CuCallbackResult)format.MinNumDecodeSurfaces;
           // return CuCallbackResult.Success;


        }


		private CuCallbackResult DecodePictureCallback(IntPtr userData, ref CuVideoPicParams param)
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>DecodePictureCallback(...)");
			
			cuVideoDecoder.DecodePicture(ref param);

			return CuCallbackResult.Success;
		}



		private CuGraphicsResource lumaResource;
		private Texture2D lumaTexture = null;
		private CuArray lumaArray;

		private CuGraphicsResource chromaResource;
		private Texture2D chromaTexture = null;
		private CuArray chromaArray;

        private unsafe CuCallbackResult VideoDisplayCallback(IntPtr userData, IntPtr infoPtr)
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>VideoDisplayCallback(...)");
			//var contextPush = cuContext.Push();

			CuVideoParseDisplayInfo displayInfo;
			if (infoPtr != IntPtr.Zero)
			{
				displayInfo = Marshal.PtrToStructure<CuVideoParseDisplayInfo>(infoPtr);
			}
			else
			{ //IsFinalFrame()
				return CuCallbackResult.Success;
			}

			var processingParam = new CuVideoProcParams
			{
				
				ProgressiveFrame = displayInfo.ProgressiveFrame,
				SecondField = displayInfo.RepeatFirstField + 1,
				TopFieldFirst = displayInfo.TopFieldFirst,
				UnpairedField = displayInfo.RepeatFirstField < 0 ? 1 : 0
			};

			var frame = cuVideoDecoder.MapVideoFrame(displayInfo.PictureIndex, ref processingParam, out var pitch);
			var status = cuVideoDecoder.GetDecodeStatus(displayInfo.PictureIndex);

			if (status != CuVideoDecodeStatus.Success)
			{
				Console.WriteLine("GetDecodeStatus(...) " + status);
				//...
			}

			var timestamp = displayInfo.Timestamp;

			//var chromaInfo = new CuVideoChromaFormatInformation(_info.ChromaFormat);
			//var chromaHeight = (int)(_info.Height * chromaInfo.HeightFactor);
			//var height = _info.Height + chromaHeight * chromaInfo.PlaneCount;
			//var bytesPerPixel = _info.BitDepthMinus8 > 0 ? 2 : 1;
			//var frameByteSize = pitch * height * bytesPerPixel;
			//var byteWidth = _info.Width * bytesPerPixel;

			var width = decodeInfo.Width;
			var height = decodeInfo.Height;

			lock (device3D11)
			{
				CuGraphicsResource[] resources = InitGraphicResources(width, height);

				fixed (CuGraphicsResource* resPtr = resources)
				{
					var stream = CuStream.Empty;

					LibCuda.GraphicsMapResources(resources.Length, resPtr, stream);

					if (lumaArray.IsEmpty)
					{
						lumaArray = lumaResource.GetMappedArray();
					}

					var memcopy = new CuMemcopy2D
					{
						SrcMemoryType = CuMemoryType.Device,
						SrcDevice = frame,
						SrcPitch = (IntPtr)pitch,
						DstMemoryType = CuMemoryType.Array,
						DstArray = lumaArray,//lumaResource.GetMappedArray(),
						DstPitch = (IntPtr)width,
						WidthInBytes = (IntPtr)width,
						Height = (IntPtr)height,
					};
					memcopy.Memcpy2D();

					if (chromaArray.IsEmpty)
					{
						chromaArray = chromaResource.GetMappedArray();
					}

					memcopy.SrcDevice = new CuDevicePtr(frame.Handle + pitch * height);
					memcopy.DstArray = chromaArray;//chromaResource.GetMappedArray(); 
					memcopy.DstPitch = (IntPtr)(width);
					memcopy.Height = (IntPtr)(height / 2);
					memcopy.Memcpy2D();


					LibCuda.GraphicsUnmapResources(resources.Length, resPtr, stream);
				}

			}



			frame.Dispose();


			//contextPush.Dispose();

			var destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = lumaTexture.Description.Width,
				Height = lumaTexture.Description.Height,
				Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,
			});

			{
				var size = new System.Drawing.Size(lumaTexture.Description.Width, lumaTexture.Description.Height);
				rgbProcessor.DrawTexture(new Texture2D[] { lumaTexture, chromaTexture }, destTexture, size);
				var sec = (double)timestamp / 10_000_000;

				OnSampleProcessed(destTexture, sec);
			}

			//lumaResource.Dispose();
			//lumaTexture.Dispose();

			//chromaResource.Dispose();
			//chromaTexture.Dispose();

			//var bytes = DxTool.DumpTexture(deviceD3D11, destTexture);
			//TestTools.WriteFile(bytes, "nvdec_test_dxinterop_rgba" + width + "x" + height + ".yuv");



			return CuCallbackResult.Success;
		}

		private unsafe CuGraphicsResource[] InitGraphicResources(int width, int height)
		{
			if (lumaTexture == null)
			{
				lumaTexture = new Texture2D(device3D11, new Texture2DDescription()
				{
					Width = width,
					Height = height,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,

					Format = Format.R8_UNorm,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,

				});
				lumaResource = CuGraphicsResource.Register(lumaTexture.NativePointer);
				//lumaArray = lumaResource.GetMappedArray();
			}

			if (chromaTexture == null)
			{
				chromaTexture = new Texture2D(device3D11, new Texture2DDescription()
				{
					Width = width / 2,
					Height = height / 2,
					MipLevels = 1,
					ArraySize = 1,
					SampleDescription = new SampleDescription(1, 0),
					Usage = ResourceUsage.Default,

					Format = Format.R8G8_UNorm,
					BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
					CpuAccessFlags = CpuAccessFlags.None,
					OptionFlags = ResourceOptionFlags.None,

				});
				chromaResource = CuGraphicsResource.Register(chromaTexture.NativePointer);
				//chromaArray = chromaResource.GetMappedArray();
			}

			var resources = new CuGraphicsResource[] { lumaResource, chromaResource };

			return resources;
		}

	}
}
