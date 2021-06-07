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

                AutoResetEvent syncEvent = new AutoResetEvent(false);
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
						//Console.WriteLine("packet.data.Length == " + packet.data.Length);
						var flags = CuVideoPacketFlags.Timestamp;
						long timestamp = (long)(packet.time * 10_000_000);
						parser.ParseVideoData(packet.data, flags, timestamp);
					}

                    syncEvent.WaitOne(10);

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

				CloseGraphicsResources();

			}

		}



		private int SequenceCallback(IntPtr userData, ref CuVideoFormat format)
		{
			Console.WriteLine(">>>>>>>>>>>>>>>>>>>> SequenceCallback(...)");


			if (!format.IsSupportedByDecoder(out var error, out var caps))
			{
				Console.WriteLine(error);

				return 0;
			}

			if (!cuVideoDecoder.IsEmpty)
			{
				Console.WriteLine(">>>>>>>>>>>>>>>>>>>> SequenceCallback(...)::Reconfigure()");

				cuVideoDecoder.Reconfigure(ref format);

				return 1;
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

			using (var contextPush = cuContext.Push())
			{
				cuVideoDecoder = CuVideoDecoder.Create(ref decodeInfo);
			}

			InitGraphicResources(decodeInfo.Width, decodeInfo.Height, decodeInfo.OutputFormat);


			return decodeInfo.NumDecodeSurfaces;
			//return 1;


		}


		private int DecodePictureCallback(IntPtr userData, ref CuVideoPicParams param)
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>DecodePictureCallback(...)");
			using (var contextPush = cuContext.Push())
			{
				cuVideoDecoder.DecodePicture(ref param);
			}

			return 1;
		}



		private CuGraphicsResource lumaResource;
		private Texture2D lumaTexture = null;

		private CuGraphicsResource chromaResource;
		private Texture2D chromaTexture = null;

		private unsafe int VideoDisplayCallback(IntPtr userData, IntPtr infoPtr)
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>VideoDisplayCallback(...)");


			CuVideoParseDisplayInfo displayInfo;
			if (infoPtr != IntPtr.Zero)
			{
				displayInfo = Marshal.PtrToStructure<CuVideoParseDisplayInfo>(infoPtr);
			}
			else
			{ //IsFinalFrame()
				return 1;
			}

			using (var contextPush = cuContext.Push())
			{
				HandlePictureDisplay(displayInfo);
			}

			var timestamp = displayInfo.Timestamp;
			ProcessTextures(timestamp);

			return 1;
		}

		private unsafe void HandlePictureDisplay(CuVideoParseDisplayInfo displayInfo)
		{

			CuVideoFrame cuFrame = default(CuVideoFrame);
			try
			{
				/*
				 *  videoProcessingParameters.progressive_frame = pDispInfo->progressive_frame;
					videoProcessingParameters.second_field = pDispInfo->repeat_first_field + 1;
					videoProcessingParameters.top_field_first = pDispInfo->top_field_first;
					videoProcessingParameters.unpaired_field = pDispInfo->repeat_first_field < 0;
					videoProcessingParameters.output_stream = m_cuvidStream;
				 */
				var processingParam = new CuVideoProcParams
				{
					ProgressiveFrame = displayInfo.ProgressiveFrame,
					SecondField = displayInfo.RepeatFirstField + 1,
					TopFieldFirst = displayInfo.TopFieldFirst,
					UnpairedField = displayInfo.RepeatFirstField < 0 ? 1 : 0
				};


				cuFrame = cuVideoDecoder.MapVideoFrame(displayInfo.PictureIndex, ref processingParam, out var pitch);

				var status = cuVideoDecoder.GetDecodeStatus(displayInfo.PictureIndex);
				if (status != CuVideoDecodeStatus.Success)
				{
					Console.WriteLine("GetDecodeStatus(...) " + status);
					//...
				}

				var width = decodeInfo.Width;
				var height = decodeInfo.Height;

				lock (device3D11)
				{
					var resources = new CuGraphicsResource[] { lumaResource, chromaResource };

					fixed (CuGraphicsResource* resPtr = resources)
					{
						var stream = CuStream.Empty;

						var result = LibCuda.GraphicsMapResources(resources.Length, resPtr, stream);
						LibCuda.CheckResult(result);

						var memcopy = new CuMemcopy2D
						{
							SrcMemoryType = CuMemoryType.Device,
							SrcDevice = cuFrame,
							SrcPitch = (IntPtr)pitch,
							DstMemoryType = CuMemoryType.Array,
							DstArray = lumaResource.GetMappedArray(),
							DstPitch = (IntPtr)width,
							WidthInBytes = (IntPtr)width,
							Height = (IntPtr)height,
						};

						result = LibCuda.Memcpy2D(ref memcopy);
						LibCuda.CheckResult(result);

						memcopy.SrcDevice = new CuDevicePtr(cuFrame.Handle + pitch * height);
						memcopy.DstArray = chromaResource.GetMappedArray();
						memcopy.DstPitch = (IntPtr)(width);
						memcopy.Height = (IntPtr)(height / 2);

						result = LibCuda.Memcpy2D(ref memcopy);
						LibCuda.CheckResult(result);

						result = LibCuda.GraphicsUnmapResources(resources.Length, resPtr, stream);
						LibCuda.CheckResult(result);
					}
				}
			}
			finally
			{
				cuFrame.Dispose();
			}

		}

		private void ProcessTextures(long timestamp)
		{
			var size = new System.Drawing.Size(lumaTexture.Description.Width, lumaTexture.Description.Height);
			var destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = size.Width,
				Height = size.Height,
				Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
				MipLevels = 1,
				ArraySize = 1,
			});

	
			rgbProcessor.DrawTexture(new Texture2D[] { lumaTexture, chromaTexture }, destTexture, size);
			var sec = (double)timestamp / 10_000_000;

			OnSampleProcessed(destTexture, sec);


			//var bytes = DxTool.DumpTexture(deviceD3D11, destTexture);
			//TestTools.WriteFile(bytes, "nvdec_test_dxinterop_rgba" + width + "x" + height + ".yuv");
		}

		private void InitGraphicResources(int width, int height, CuVideoSurfaceFormat format)
		{
			if (format != 0)
			{
				throw new NotSupportedException("Invalid format: " + format);
			}

			var texDescr = new Texture2DDescription()
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

			};

			lumaTexture = new Texture2D(device3D11, texDescr);
			lumaResource = CuGraphicsResource.Register(lumaTexture.NativePointer, CuGraphicsRegisters.None);


			texDescr.Width = width / 2;
			texDescr.Height = height / 2;
			texDescr.Format = Format.R8G8_UNorm;

			chromaTexture = new Texture2D(device3D11, texDescr);
			chromaResource = CuGraphicsResource.Register(chromaTexture.NativePointer, CuGraphicsRegisters.None);

		}

		private void CloseGraphicsResources()
		{
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
}
