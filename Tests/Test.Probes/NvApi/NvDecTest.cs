using MediaToolkit.Codecs;
using MediaToolkit.DirectX;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Nvidia;
using MediaToolkit.Utils;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Probe.NvApi
{
    class NvDecTest
    {
        public static void Run()
        {

            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv444p_30fps_30sec_bf0.h264";
            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\smptebars_1280x720_nv12_30fps_30sec_bf0.h264";
            //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec.h264";
            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";
            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_nv12_30fps_30sec_bf0.h264";
            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_Iframe.h264";
            ////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_1fps_30sec_bf0.h264";
            //var width = 1280;
            //var height = 720;
            //var fps = 30;



            string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_1920x1080_5sec.h264";
            //string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1920x1080_yuv420p_30fps_30sec_bf0.h264";
            var width = 1920;
            var height = 1080;
            var fps = 30;


            var inputArgs = new MfVideoArgs
            {
                Width = width,
                Height = height,

                //Width = 320,
                //Height = 240,
                FrameRate = MfTool.PackToLong(fps, 1),
                LowLatency = true,
            };

            try
            {
                NvDecTest test = new NvDecTest();
                test.Start(fileName, inputArgs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        private CuContext _context;
        private CuVideoDecoder _decoder;
        private CuVideoContextLock _contextLock;
        private CuVideoDecodeCreateInfo decodeInfo;

        private NalSourceReader sourceReader = null;

        private SharpDX.Direct3D11.Device deviceD3D11 = null;
		private RgbProcessor rgbProcessor = null;


		private bool running = false;
		public void Start(string fileName, MfVideoArgs inputArgs)
		{
			running = true;


			using (Factory1 factory = new Factory1())
			{
				using (var adapter = factory.GetAdapter(0))
				{
					SharpDX.Direct3D.FeatureLevel[] featureLevel =
					{
						FeatureLevel.Level_11_1,
						FeatureLevel.Level_11_0,
						FeatureLevel.Level_10_1,
					};

					var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
					deviceD3D11 = DxTool.CreateMultithreadDevice(adapter, deviceCreationFlags, featureLevel);
				}
			}

			sourceReader = new NalSourceReader();
			sourceReader.Start(fileName, inputArgs);

			var decoderTask = Task.Run(() => { DecoderTask(inputArgs); });

			Task.WaitAll(decoderTask);


			_context.Dispose();
			_contextLock.Dispose();
			_decoder.Dispose();


			rgbProcessor?.Close();
			deviceD3D11.Dispose();
		}

		private void DecoderTask(MfVideoArgs inputArgs)
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
			using (var dxgiDevice = deviceD3D11.QueryInterface<SharpDX.DXGI.Device>())
			{
				using (var a = dxgiDevice.Adapter)
				{
                    device = CuDevice.GetFromDxgiAdapter(a.NativePointer);
				}
			}

			//if (device.IsEmpty)
			//{
			//	throw new Exception("device.IsEmpty");
			//}

			_context = device.CreateContext(CuContextFlags.SchedBlockingSync);
			_contextLock = _context.CreateLock();


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

			//CuVideoParser parser = CuVideoParser.Create(ref parserParams);
			try
			{
				rgbProcessor = new RgbProcessor();
				var size = new System.Drawing.Size(inputArgs.Width, inputArgs.Height);
				rgbProcessor.Init(deviceD3D11, size, MediaToolkit.Core.PixFormat.NV12, size, MediaToolkit.Core.PixFormat.RGB32);


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

						parser.ParseVideoData(packet.data, CuVideoPacketFlags.None, 0);
					}
				}
			}
			finally
			{
				parser.Dispose();

			}


			
			
		}

		private int SequenceCallback(IntPtr data, ref CuVideoFormat format)
        {
            Console.WriteLine(">>>>>>>>>>>>>>>>>>>> SequenceCallback(...)");


            if (!NvDecodeApi.IsFormatSupportedByDecoder(format, out var error, out var caps))
            {
                Console.Error.WriteLine(error);

                return 0;
            }

            if (_decoder!=null && !_decoder.IsEmpty)
            {
                //_decoder.Reconfigure(ref format);

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
                CreationFlags = CuVideoCreateFlags.PreferCUVID,
                MaxNumDecodeSurfaces = format.MinNumDecodeSurfaces,
                VideoLock = _contextLock.NativePtr,
                Width = format.CodedWidth,
                Height = format.CodedHeight,
                MaxWidth = format.CodedWidth,
                MaxHeight = format.CodedHeight,
                TargetWidth = format.CodedWidth,
                TargetHeight = format.CodedHeight,


            };

            _decoder = NvDecodeApi.CreateDecoder(decodeInfo);

            return 1;


        }


        private int DecodePictureCallback(IntPtr data, ref CuVideoPicParams param)
        {
            //Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>DecodePictureCallback(...)");

            _decoder.DecodePicture(ref param);

            return 1;
        }



		CuGraphicsResource lumaResource;
        Texture2D lumaTexture = null;
        CuArrayPtr lumaArray;

		CuGraphicsResource chromaResource;
        Texture2D chromaTexture = null;
        CuArrayPtr chromaArray;

        private unsafe int VideoDisplayCallback(IntPtr data, IntPtr infoPtr)
		{
			//Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>VideoDisplayCallback(...)");
			var contextPush = _context.Push();

			CuVideoParseDisplayInfo displayInfo;
			if (infoPtr != IntPtr.Zero)
			{
				displayInfo = Marshal.PtrToStructure<CuVideoParseDisplayInfo>(infoPtr);
			}
			else
			{ //IsFinalFrame()
				return 1;
			}

			var processingParam = new CuVideoProcParams
			{
				ProgressiveFrame = displayInfo.ProgressiveFrame,
				SecondField = displayInfo.RepeatFirstField + 1,
				TopFieldFirst = displayInfo.TopFieldFirst,
				UnpairedField = displayInfo.RepeatFirstField < 0 ? 1 : 0
			};

			var frame = _decoder.MapVideoFrame(displayInfo.PictureIndex, ref processingParam, out var pitch);
			var status = _decoder.GetDecodeStatus(displayInfo.PictureIndex);
			var timestamp = displayInfo.Timestamp;
		

			if (status != CuVideoDecodeStatus.Success)
			{
				Console.WriteLine("GetDecodeStatus(...) " + status);
				//...
			}


			//var chromaInfo = new CuVideoChromaFormatInformation(_info.ChromaFormat);
			//var chromaHeight = (int)(_info.Height * chromaInfo.HeightFactor);
			//var height = _info.Height + chromaHeight * chromaInfo.PlaneCount;
			//var bytesPerPixel = _info.BitDepthMinus8 > 0 ? 2 : 1;
			//var frameByteSize = pitch * height * bytesPerPixel;
			//var byteWidth = _info.Width * bytesPerPixel;

			var width = decodeInfo.Width;
			var height = decodeInfo.Height;

			CuGraphicsResourcePtr[] resources = InitGraphicResources(width, height).Select(r => r.ResourcePtr).ToArray();

			fixed (CuGraphicsResourcePtr* resPtr = resources)
			{
				var stream = CuStreamPtr.Empty;

				LibCuda.GraphicsMapResources(resources.Length, resPtr, stream);

				if (lumaArray.IsEmpty)
				{
					lumaArray = lumaResource.GetMappedArray();
				}

				var memcopy = new CuMemcopy2D
				{
					SrcMemoryType = CuMemoryType.Device,
					SrcDevice = frame.DevicePtr,
					SrcPitch = pitch,
					DstMemoryType = CuMemoryType.Array,
					DstArray = lumaArray,//lumaResource.GetMappedArray(),
					DstPitch = width,
					WidthInBytes = width,
					Height = height,
				};
                LibCuda.Memcpy2D(ref memcopy);

                if (chromaArray.IsEmpty)
				{
					chromaArray = chromaResource.GetMappedArray();
				}

				memcopy.SrcDevice = new CuDevicePtr(frame.Handle + pitch * height);
				memcopy.DstArray = chromaArray;//chromaResource.GetMappedArray(); 
				memcopy.DstPitch = width;
				memcopy.Height = (height / 2);
	
                LibCuda.Memcpy2D(ref memcopy);

                LibCuda.GraphicsUnmapResources(resources.Length, resPtr, stream);
			}


			var destTexture = new Texture2D(deviceD3D11, new SharpDX.Direct3D11.Texture2DDescription()
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

			var size = new System.Drawing.Size(lumaTexture.Description.Width, lumaTexture.Description.Height);
			rgbProcessor.DrawTexture(new Texture2D[] { lumaTexture, chromaTexture }, destTexture, size);


			//var bytes = DxTool.DumpTexture(deviceD3D11, destTexture);
			//TestTools.WriteFile(bytes, "nvdec_test_dxinterop_rgba" + width + "x" + height + ".yuv");

			var sec = (double)timestamp / 10_000_000;

			Console.WriteLine("Sec == " + sec);
			destTexture.Dispose();

			//lumaTexture.Dispose();
			//lumaResource.Dispose();

			//chromaTexture.Dispose();
			//chromaResource.Dispose();



			Thread.Sleep(30);
			frame.Dispose();

			contextPush.Dispose();

			return 1;
		}

		private unsafe CuGraphicsResource[] InitGraphicResources(int width, int height)
		{
			if (lumaTexture == null)
			{
				lumaTexture = new Texture2D(deviceD3D11, new Texture2DDescription()
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
				chromaTexture = new Texture2D(deviceD3D11, new Texture2DDescription()
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


		class VideoPacket
        {
            public byte[] data = null;
            public double time = 0;
            public double duration = 0;
        }


        class NalSourceReader
        {
            private BlockingCollection<VideoPacket> videoPackets = null;
            private volatile bool running = false;
            public bool PacketsAvailable
            {
                get
                {
                    bool available = false;
                    if (videoPackets != null)
                    {
                        available = videoPackets.Count > 0;
                    }
                    return available;
                }
            }

            public bool IsFull
            {
                get
                {
                    bool isAddingCompleted = false;
                    if (videoPackets != null)
                    {
                        isAddingCompleted = videoPackets.IsAddingCompleted;
                    }
                    return isAddingCompleted;
                }
            }
            public int Count
            {
                get
                {
                    int count = -1;
                    if (videoPackets != null)
                    {
                        count = videoPackets.Count;
                    }
                    return count;
                }
            }


            public bool TryGetPacket(out VideoPacket packet, int timeout)
            {
                packet = null;
                bool result = false;
                if (videoPackets != null)
                {
                    result = videoPackets.TryTake(out packet, timeout);
                }

                return result;
            }
            public double PacketInterval { get; private set; }
            public Task Start(string fileName, MfVideoArgs inputArgs)
            {
                if (running)
                {
                    throw new InvalidOperationException("Invalid state " + running);
                }

                running = true;
                return Task.Run(() =>
                {
                    //videoPackets = new Queue<VideoPacket>(4);
                    videoPackets = new BlockingCollection<VideoPacket>(8);
                    Stream stream = null;
                    try
                    {
                        var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
                        PacketInterval = (double)frameRate[1] / frameRate[0];
                        long packetCount = 0;
                        double packetTime = 0;

                        stream = new FileStream(fileName, FileMode.Open);
                        var nalReader = new NalUnitReader(stream);
                        var dataAvailable = false;

                        bool loopback = true;

                        Random rnd = new Random();
                        while (loopback)
                        {
                            List<byte[]> nalsBuffer = new List<byte[]>();
                            do
                            {
                                //int delay = (int)(sampleInterval * 1000);
                                //delay += rnd.Next(-5, 5);
                                //Thread.Sleep(delay);

                                dataAvailable = nalReader.ReadNext(out var nal);
                                if (nal != null && nal.Length > 0)
                                {
                                    var firstByte = nal[0];
                                    var nalUnitType = firstByte & 0x1F;
                                    nalsBuffer.Add(nal);

                                    if (nalUnitType == (int)NalUnitType.IDR || nalUnitType == (int)NalUnitType.Slice)
                                    {
                                        IEnumerable<byte> data = new List<byte>();
                                        var startCodes = new byte[] { 0, 0, 0, 1 };
                                        foreach (var n in nalsBuffer)
                                        {
                                            data = data.Concat(startCodes).Concat(n);
                                        }

                                        nalsBuffer.Clear();
                                        packetTime = PacketInterval * packetCount;
                                        var bytes = data.ToArray();
                                        var packet = new VideoPacket
                                        {
                                            data = bytes,
                                            time = packetTime,
                                            duration = PacketInterval,
                                        };

                                        //videoPackets.Enqueue(packet);
                                        videoPackets.Add(packet);
                                        packetCount++;
                                    }
                                }

                            } while (dataAvailable && running);

                            if (!running)
                            {
                                break;
                            }

                            stream.Position = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        running = false;
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                            stream = null;
                        }

                        if (videoPackets != null)
                        {
                            videoPackets.Dispose();
                            videoPackets = null;
                        }
                    }
                });
            }

            public void Stop()
            {
                running = false;
            }

        }

    }

}
