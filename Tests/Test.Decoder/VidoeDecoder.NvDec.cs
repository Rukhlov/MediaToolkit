﻿using MediaToolkit.DirectX;
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

namespace Test.Decoder
{
	public partial class VideoDecoderPresenter
	{

		private CuContext cuContext;
		private CuVideoDecoder cuVideoDecoder;
		private CuVideoContextLock videoContextLock;
		private CuVideoDecodeCreateInfo decodeInfo;

		private CuGraphicsResource lumaResource;
		private Texture2D lumaTexture = null;

		private CuGraphicsResource chromaResource;
		private Texture2D chromaTexture = null;

		private uint clockRate = 10_000_000;

		private void NvDecDecoderTask(MfVideoArgs inputArgs)
		{
			try
			{
                logger.Debug("Try to init CUDA environments...");

				LibCuda.Initialize();

				var driverVersion = LibCuda.DriverGetVersion();

                logger.Debug("CUDA driver ver: " + driverVersion);
                logger.Debug("CUDA API ver: " + LibCuda.ApiVerison);
				if (driverVersion == 0)
				{
					throw new InvalidOperationException("CUDA driver not installed");
				}
				else if (driverVersion < LibCuda.ApiVerison)
				{
                    logger.Warn("WARN: CUDA driver is incompatible with API (" + driverVersion + "<" + LibCuda.ApiVerison + ")");
				}


				AdapterDescription adapterDescr;
				CuDevice device;
				using (var dxgiDevice = device3D11.QueryInterface<SharpDX.DXGI.Device>())
				{
					using (var a = dxgiDevice.Adapter)
					{
						adapterDescr = a.Description;
						device = CuDevice.GetFromDxgiAdapter(a.NativePointer);
					}
				}


				var name = device.GetName();
				var pciId = device.GetPciBusId();
				var memory = device.GetTotalMemory();
				var id = device.GetId();

                logger.Debug("CUDA device info:\r\n" + string.Join("\r\n", id, name, pciId, memory));
                logger.Debug("--------------------------");
				cuContext = device.CreateContext(CuContextFlags.SchedBlockingSync);

				var codecType = CuVideoCodec.H264;
				var chromaFormat = CuVideoChromaFormat.YUV420;
				var bit = 8;
				var bitDepthMinus8 = (bit - 8);

				var decodeCaps = NvDecodeApi.GetDecoderCaps(codecType, chromaFormat, bitDepthMinus8);
				if (!decodeCaps.IsSupported)
				{
					throw new NotSupportedException("Not supported codec: " + string.Join(", ", codecType, chromaFormat, bitDepthMinus8));
				}
                logger.Debug("Codec caps: " + decodeCaps);

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

				CuVideoParser parser = NvDecodeApi.CreateParser(parserParams);

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

					clockRate = parserParams.ClockRate;
					if (clockRate == 0)
					{
						clockRate = 10_000_000;
					}

					AutoResetEvent syncEvent = new AutoResetEvent(false);
					while (running)
					{

						while (sourceReader.PacketsAvailable)
						{
							bool packetTaken = sourceReader.TryGetPacket(out var packet, 10);
							if (!packetTaken)
							{
                                logger.Debug("packet == false");
								continue;
							}
							//Console.WriteLine("packet.data.Length == " + packet.data.Length);
							var flags = CuVideoPacketFlags.Timestamp;
							long timestamp = (long)(packet.time * clockRate);
							parser.ParseVideoData(packet.data, flags, timestamp);
						}

						syncEvent.WaitOne(10);
					}

				}
				finally
				{
					if (parser != null)
					{
						parser.Dispose();
						parser = null;
					}

					if (rgbProcessor != null)
					{
						rgbProcessor.Close();
						rgbProcessor = null;
					}

					CloseGraphicsResources();

				}
			}
			catch (Exception ex)
			{
                logger.Error(ex.Message);

				running = false;
			}
			finally
			{
				CloseCuda();
			}


		}


		private int SequenceCallback(IntPtr userData, ref CuVideoFormat format)
		{
            logger.Debug(">>>>>>>>>>>>>>>>>>>> SequenceCallback(...)");
			/*
			 *  Parser triggers this callback for initial sequence header or when it encounters a 
			 *  video format change. Return value from sequence callback is interpreted by the driver as follows:
			 *	0: fail
			 *	1: succeeded, but driver should not override CUVIDPARSERPARAMS::ulMaxNumDecodeSurfaces
			 *	>1: succeeded, and driver should override CUVIDPARSERPARAMS::ulMaxNumDecodeSurfaces with this return value
			 */
			int result = 1;
			try
			{
				if (!NvDecodeApi.IsFormatSupportedByDecoder(format, out var error, out var caps))
				{
					throw new NotSupportedException(error);
				}

				if (cuVideoDecoder != null)
				{
					// TODO:
					//cuVideoDecoder.Reconfigure(ref format);
					//...
				}

				int numDecodeSurfaces = 4;
				if (format.MinNumDecodeSurfaces > 0)
				{
					numDecodeSurfaces = format.MinNumDecodeSurfaces;
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
					MaxNumDecodeSurfaces = numDecodeSurfaces,

					VideoLock = videoContextLock.NativePtr,
					Width = format.CodedWidth,
					Height = format.CodedHeight,
					MaxWidth = format.CodedWidth,
					MaxHeight = format.CodedHeight,
					TargetWidth = format.CodedWidth,
					TargetHeight = format.CodedHeight,

				};

				using (var contextPush = cuContext.Push())
				{
					cuVideoDecoder = NvDecodeApi.CreateDecoder(decodeInfo);
					result = cuVideoDecoder.DecodeInfo.MaxNumDecodeSurfaces;
				}

				InitGraphicResources(decodeInfo);

			}
			catch (Exception ex)
			{
				result = 0;

                logger.Error(ex.Message);
			}

			return result;

		}


		private int DecodePictureCallback(IntPtr userData, ref CuVideoPicParams param)
		{
            //logger.Debug(">>>>>>>>>>>>>>>>>>>>>DecodePictureCallback(...)");

            /*
			 *  Parser triggers this callback when bitstream data for one frame is 
			 *  ready. In case of field pictures, there may be two decode calls per one display call since two 
			 *  fields make up one frame. Return value from this callback is interpreted as:
			 *	0: fail
			 *	≥1: succeeded
			 */

            int result = 1;
			try
			{
				using (var contextPush = cuContext.Push())
				{
					cuVideoDecoder.DecodePicture(ref param);
				}
			}
			catch (Exception ex)
			{
                logger.Error(ex.Message);
				result = 0;
			}

			return result;
		}


		private int VideoDisplayCallback(IntPtr userData, IntPtr infoPtr)
		{
            //logger.Debug(">>>>>>>>>>>>>>>>>>>>>>>VideoDisplayCallback(...)");

            /*
			 *	Parser triggers this callback when a frame in display order is ready. 
			 *	Return value from this callback is interpreted as:
			 *	0: fail
			 *	≥1: succeeded
			 */

            if (infoPtr == IntPtr.Zero)
			{
                //IsFinalFrame()
                logger.Debug("LastFrame");
				return 1;
			}

			int result = 1;
			try
			{
				CuVideoParseDisplayInfo displayInfo = Marshal.PtrToStructure<CuVideoParseDisplayInfo>(infoPtr);

				using (var contextPush = cuContext.Push())
				{
					HandlePictureDisplay(displayInfo);
				}

				var timestamp = displayInfo.Timestamp;
				ProcessTextures(timestamp);
			}
			catch (Exception ex)
			{
                logger.Error(ex.Message);
				result = 0;
			}

			return result;
		}

		private unsafe void HandlePictureDisplay(CuVideoParseDisplayInfo displayInfo)
		{

			CuVideoFrame cuFrame = null;
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
                    logger.Warn("GetDecodeStatus(...) " + status);
					//...
				}

				var width = decodeInfo.Width;
				var height = decodeInfo.Height;

				lock (device3D11)
				{
					var resources = new CuGraphicsResourcePtr[] { lumaResource.ResourcePtr, chromaResource.ResourcePtr };

					fixed (CuGraphicsResourcePtr* resPtr = resources)
					{
						var stream = CuStreamPtr.Empty;

						var result = LibCuda.GraphicsMapResources(resources.Length, resPtr, stream);
						LibCuda.CheckResult(result);

						var memcopy = new CuMemcopy2D
						{
							SrcMemoryType = CuMemoryType.Device,
							SrcDevice = cuFrame.DevicePtr,
							SrcPitch = pitch,
							DstMemoryType = CuMemoryType.Array,
							DstArray = lumaResource.GetMappedArray(),
							DstPitch = width,
							WidthInBytes = width,
							Height = height,
						};

						result = LibCuda.Memcpy2D(ref memcopy);
						LibCuda.CheckResult(result);

						memcopy.SrcDevice = new CuDevicePtr(cuFrame.Handle + pitch * height);
						memcopy.DstArray = chromaResource.GetMappedArray();
						memcopy.DstPitch = (width);
						memcopy.Height = (height / 2);

						result = LibCuda.Memcpy2D(ref memcopy);
						LibCuda.CheckResult(result);

						result = LibCuda.GraphicsUnmapResources(resources.Length, resPtr, stream);
						LibCuda.CheckResult(result);
					}
				}
			}
			finally
			{
				if (cuFrame != null)
				{
					cuFrame.Dispose();
					cuFrame = null;
				}

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
			var sec = (double)timestamp / clockRate;

			OnSampleProcessed(destTexture, sec);


			//var bytes = DxTool.DumpTexture(deviceD3D11, destTexture);
			//TestTools.WriteFile(bytes, "nvdec_test_dxinterop_rgba" + width + "x" + height + ".yuv");
		}

		private void InitGraphicResources(CuVideoDecodeCreateInfo decodeInfo)
		{
            logger.Debug("InitGraphicResources()");

			int width = decodeInfo.Width;
			int height = decodeInfo.Height;
			var format = decodeInfo.OutputFormat;

			if (format != CuVideoSurfaceFormat.Default)
			{// only NV12 currently supported
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
            logger.Debug("CloseGraphicsResources()");

			if (lumaResource != null)
			{
				lumaResource.Dispose();
				lumaResource = null;
			}

			if (lumaTexture != null)
			{
				lumaTexture.Dispose();
				lumaTexture = null;
			}

			if (chromaResource != null)
			{
				chromaResource.Dispose();
				chromaResource = null;
			}

			if (chromaTexture != null)
			{
				chromaTexture.Dispose();
				chromaTexture = null;
			}
		}


		private void CloseCuda()
		{
            logger.Debug("CloseCuda()");

			if (cuVideoDecoder != null)
			{
				cuVideoDecoder.Dispose();
				cuVideoDecoder = null;
			}

			if (videoContextLock != null)
			{
				videoContextLock.Dispose();
				videoContextLock = null;
			}

			if (cuContext != null)
			{
				cuContext.Dispose();
				cuContext = null;
			}

			decodeInfo = default(CuVideoDecodeCreateInfo);
		}


	}
}
