using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MediaToolkit.DirectX;
using MediaToolkit.Nvidia;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using GDI = System.Drawing;

namespace Test.Encoder
{
    class NvEncTest
    {

        public unsafe static void Run()
        {

            LibCuda.Initialize();

			LibNvEnc.Initialize();

            Factory1 factory = new Factory1();

            var adapter = factory.GetAdapter(0);
            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
             };

			var deviceCreationFlags = DeviceCreationFlags.None;
			var device3D11 = DxTool.CreateMultithreadDevice(adapter, deviceCreationFlags, featureLevel);
			factory.Dispose();
			adapter.Dispose();

			var bitmapFile = @"Files\rgba_1920x1080.raw";
			uint width = 1920;
			uint height = 1080;

			//var bitmapFile = @"Files\1920x1080.bmp";
			//var fileName = @"Files\2560x1440.bmp";
			//  var fileName = @"Files\rgba_352x288.bmp";

			var outputFile = "output.h264";


            var outputStream = File.Open(outputFile, FileMode.Create);

           var bitmapBytes = File.ReadAllBytes(bitmapFile);
			var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = (int)width,
				Height = (int)height,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,

				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

			};
			var texture = DxTool.TextureFromDump(device3D11, texDescr, bitmapBytes);
			

			Console.WriteLine($"Process: {(Environment.Is64BitProcess ? "64" : "32")} bits");
            Console.WriteLine($"Bipmap: {bitmapFile}");
            Console.WriteLine($"Output: {outputFile}");

            CuDevice cuDevice;
            using (var dxgiDevice = device3D11.QueryInterface<SharpDX.DXGI.Device>())
            {
                using (var a = dxgiDevice.Adapter)
                {
                    cuDevice = CuDevice.GetFromDxgiAdapter(a.NativePointer);
                }
            }
			device3D11.Dispose();

			var cuContext = cuDevice.CreateContext(CuContextFlags.SchedBlockingSync);

            var contextPush = cuContext.Push();

			var textureResource = CuGraphicsResource.Register(texture.NativePointer);

			var sessionParams = new NvEncOpenEncodeSessionExParams
            {
                Version = NvEncodeAPI.NV_ENC_OPEN_ENCODE_SESSION_EX_PARAMS_VER,
                ApiVersion = NvEncodeAPI.NVENCAPI_VERSION,
                Device = cuContext.ContextPtr.Handle,
                DeviceType = NvEncDeviceType.Cuda
            };

            var encoder = LibNvEnc.OpenEncoder(sessionParams);

            var initparams = new NvEncInitializeParams
            {
                Version = NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS_VER,
                EncodeGuid = NvEncCodecGuids.H264,
                EncodeHeight = height,
                EncodeWidth = width,
                MaxEncodeHeight = height,
                MaxEncodeWidth = width,
                DarHeight = height,
                DarWidth = width,
                FrameRateNum = 33,
                FrameRateDen = 1,
                ReportSliceOffsets = false,
                EnableSubFrameWrite = false,
                PresetGuid = NvEncPresetGuids.LowLatencyDefault,
                EnableEncodeAsync = 0,

            };

            encoder.InitializeEncoder(initparams);

            var bitstreamBuffer = encoder.CreateBitstreamBuffer();

			var maxWidth = encoder.GetEncodeCaps(NvEncCodecGuids.H264, NvEncCaps.WidthMax);
			var maxHeight = encoder.GetEncodeCaps(NvEncCodecGuids.H264, NvEncCaps.HeightMax);

			var minWidth = encoder.GetEncodeCaps(NvEncCodecGuids.H264, NvEncCaps.WidthMin);
			var minHeight = encoder.GetEncodeCaps(NvEncCodecGuids.H264, NvEncCaps.HeightMin);

            var _formats = encoder.GetInputFormats(NvEncCodecGuids.H264);
            foreach (var fmt in _formats)
            {
                Console.WriteLine(fmt);
            }

			Console.WriteLine("----------------------------------");

            //uint srcPitch = width * 4;
            uint widthInBytes = (uint)(width * 4);
            var deviceMemory = CuDeviceMemoryObj.AllocatePitch(out var pitch, widthInBytes, (uint)height, 16);

			var reg = new NvEncRegisterResource
			{
				Version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,
				BufferFormat = NvEncBufferFormat.Abgr,
				BufferUsage = NvEncBufferUsage.NvEncInputImage,
				ResourceToRegister = deviceMemory.DeviceHandle,
				Width = width,
				Height = height,
				Pitch = pitch,

				ResourceType = NvEncInputResourceType.Cudadeviceptr,
			};

			var regRes = encoder.RegisterResource(reg);


			//        fixed(byte* srcPtr = bitmapBytes)
			//        {
			//            var memcopy = new CuMemcopy2D
			//            {
			//                SrcMemoryType = CuMemoryType.Host,
			//                SrcDevice = CuDevicePtr.DeviceCpu,
			//                SrcPitch = srcPitch,
			//                SrcHost = (IntPtr)srcPtr,

			//                DstMemoryType = CuMemoryType.Device,
			//                DstDevice = memObj.DevicePtr,

			//                DstPitch = destPitch,
			//                WidthInBytes = widthInBytes,
			//                Height = height,

			//            };

			//            var result = LibCuda.Memcpy2D(ref memcopy);
			//LibCuda.CheckResult(result);
			//        }



			int frameNum = 0;
            int count = 100;
            while (count-- > 0)
            {
				// Dx11 -> Cuda 
				var stream = CuStreamPtr.Empty;
				CuGraphicsResourcePtr[] graphicsResources = new CuGraphicsResourcePtr[] { textureResource.ResourcePtr };
				fixed (CuGraphicsResourcePtr* resPtr = graphicsResources)
				{
					var result = LibCuda.GraphicsMapResources(graphicsResources.Length, resPtr, stream);
					LibCuda.CheckResult(result);

					var _memcopy = new CuMemcopy2D
					{
						SrcMemoryType = CuMemoryType.Array,
						SrcArray = textureResource.GetMappedArray(),

						DstMemoryType = CuMemoryType.Device,
						DstDevice = deviceMemory.DevicePtr,

						DstPitch = pitch,
						WidthInBytes = widthInBytes,
						Height = height,
					};

					result = LibCuda.Memcpy2D(ref _memcopy);
					LibCuda.CheckResult(result);

					result = LibCuda.GraphicsUnmapResources(graphicsResources.Length, resPtr, stream);
					LibCuda.CheckResult(result);

				}


				var mapInputResource = new NvEncMapInputResource
                {
                   Version = NvEncodeAPI.NV_ENC_MAP_INPUT_RESOURCE_VER,
                   RegisteredResource = regRes.RegisteredResourcePointer,
                };

                encoder.MapInputResource(ref mapInputResource);

                var pic = new NvEncPicParams
                {
                    Version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                    PictureStruct = NvEncPicStruct.Frame,
                    InputBuffer = mapInputResource.MappedResource,


                    BufferFmt = NvEncBufferFormat.Abgr,
                    InputWidth = width,
                    InputHeight = height,
                    InputPitch = pitch,

                    
                    OutputBitstream = bitstreamBuffer.BitstreamBuffer,

                    InputTimeStamp = 0,
                    InputDuration = 33
                };


                encoder.EncodePicture(ref pic);

                using (var bitStream = encoder.LockBitstreamAndCreateStream(ref bitstreamBuffer))
                {
					bitStream.CopyTo(outputStream);               
                }

                encoder.UnmapInputResource(mapInputResource.MappedResource);

				//Thread.Sleep(_frameDuration);
				Console.WriteLine("FrameNum: " + frameNum);
				frameNum++;

			}

			textureResource.Dispose();

			texture.Dispose();
			outputStream.Dispose();

			regRes.Dispose();

			deviceMemory.Dispose();
			encoder.DestroyBitstreamBuffer(bitstreamBuffer.BitstreamBuffer);
			encoder.DestroyEncoder();

			contextPush.Dispose();
			cuContext.Dispose();
		}

        public static void RunDX11()
        {

			LibNvEnc.Initialize();

			Factory1 factory = new Factory1();

            var adapter = factory.GetAdapter(0);
            SharpDX.Direct3D.FeatureLevel[] featureLevel =
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0,
                FeatureLevel.Level_10_1,
             };


            var deviceCreationFlags = DeviceCreationFlags.BgraSupport;
            //DeviceCreationFlags.Debug |
            //DeviceCreationFlags.VideoSupport |
            //;

            var device = new SharpDX.Direct3D11.Device(adapter, deviceCreationFlags, featureLevel);

            var bitmapFile = @"Files\1920x1080.bmp";
            //var fileName = @"Files\2560x1440.bmp";
            //  var fileName = @"Files\rgba_352x288.bmp";

            var outputFile = "output.h264";

            var output = File.OpenWrite(outputFile);
         
            Console.WriteLine($"Process: {(Environment.Is64BitProcess ? "64" : "32")} bits");
            Console.WriteLine($"Bipmap: {bitmapFile}");
            Console.WriteLine($"Output: {outputFile}");

            var texture = WicTool.CreateTexture2DFromBitmapFile(bitmapFile, device);

            var desc = texture.Description;


            var encoder = LibNvEnc.OpenEncoderForDirectX(device.NativePointer);

            var initparams = new NvEncInitializeParams
            {
                Version = NvEncodeAPI.NV_ENC_INITIALIZE_PARAMS_VER,
                EncodeGuid = NvEncCodecGuids.H264,
                EncodeHeight = (uint)desc.Height,
                EncodeWidth = (uint)desc.Width,
                MaxEncodeHeight = (uint)desc.Height,
                MaxEncodeWidth = (uint)desc.Width,
                DarHeight = (uint)desc.Height,
                DarWidth = (uint)desc.Width,
                FrameRateNum = 33,
                FrameRateDen = 1,
                ReportSliceOffsets = false,
                EnableSubFrameWrite = false,
                PresetGuid = NvEncPresetGuids.LowLatencyDefault,
                EnableEncodeAsync = 0,

            };

            encoder.InitializeEncoder(initparams);

           var _bitstreamBuffer = encoder.CreateBitstreamBuffer();


            var maxWidth = 0;
            var maxHeight = 0;
            var minWidth = 0;
            var minHeight = 0;

            NvEncCapsParam capsParam = new NvEncCapsParam
            {
                CapsToQuery = NvEncCaps.WidthMax,
                Version = NvEncodeAPI.NV_ENC_CAPS_PARAM_VER,
            };
            encoder.GetEncodeCaps(NvEncCodecGuids.H264, ref capsParam, ref maxWidth);

            capsParam = new NvEncCapsParam
            {
                CapsToQuery = NvEncCaps.HeightMax,
                Version = NvEncodeAPI.NV_ENC_CAPS_PARAM_VER,
            };
            encoder.GetEncodeCaps(NvEncCodecGuids.H264, ref capsParam, ref maxHeight);

            capsParam = new NvEncCapsParam
            {
                CapsToQuery = NvEncCaps.WidthMin,
                Version = NvEncodeAPI.NV_ENC_CAPS_PARAM_VER,
            };
            encoder.GetEncodeCaps(NvEncCodecGuids.H264, ref capsParam, ref minWidth);

            capsParam = new NvEncCapsParam
            {
                CapsToQuery = NvEncCaps.HeightMin,
                Version = NvEncodeAPI.NV_ENC_CAPS_PARAM_VER,
            };
            encoder.GetEncodeCaps(NvEncCodecGuids.H264, ref capsParam, ref minHeight);

            uint formatsCount = encoder.GetInputFormatCount(NvEncCodecGuids.H264);
            // NvEncBufferFormat[] formats = new NvEncBufferFormat[formatsCount];
            //Span<NvEncBufferFormat> span = new Span<NvEncBufferFormat>(_formats);

            var _formats = encoder.GetInputFormats(NvEncCodecGuids.H264);

            //formatsCount = 4;
            //_encoder.GetInputFormats(NvEncCodecGuids.H264, formats, ref formatsCount);
            //var formats = span.ToArray();
            foreach (var fmt in _formats)
            {
                Console.WriteLine(fmt);
            }


            var reg = new NvEncRegisterResource
            {
                Version = NvEncodeAPI.NV_ENC_REGISTER_RESOURCE_VER,

                BufferFormat = NvEncBufferFormat.Abgr,
                BufferUsage = NvEncBufferUsage.NvEncInputImage,
                ResourceToRegister = texture.NativePointer,
                Width = (uint)desc.Width,
                Height = (uint)desc.Height,
				Pitch = 0,

				//Pitch = (uint)desc.Width
				//ResourceType = NvEncInputResourceType.Directx,

			};

            /*
             * For DirectX 11 interface, this buffer can be created using DirectX 11
                CreateBuffer() API, by specifying usage = D3D11_USAGE_DEFAULT; BindFlags
                = (D3D11_BIND_VIDEO_ENCODER | D3D11_BIND_SHADER_RESOURCE); and
                CPUAccessFlags = 0;
             */

            // Registers the hardware texture surface as a resource for
            // NvEnc to use.
            var regRes = encoder.RegisterResource(reg);

            int count = 1;
            while (count-- > 0)
            {

				var pic = new NvEncPicParams
				{
					Version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
					PictureStruct = NvEncPicStruct.Frame,
					InputBuffer = regRes.GetInputPointer(),
                    // BufferFmt = NvEncBufferFormat.Abgr,
                    InputWidth = (uint)desc.Width,
                    InputHeight = (uint)desc.Height,
                    OutputBitstream = _bitstreamBuffer.BitstreamBuffer,
                    InputTimeStamp = (ulong)0,
                    InputDuration = 33
                };

                // Do the actual encoding. With this configuration this is done
                // sync (blocking).
                encoder.EncodePicture(ref pic);

                object _writeMutex = new object();

                // The output is written to the bitstream, which is now copied
                // to the output file.
                using (var sm = encoder.LockBitstreamAndCreateStream(
                    ref _bitstreamBuffer))
                {
                    lock (_writeMutex)
                    {
                        sm.CopyTo(output);
                    }
                }

                texture.Dispose();

                //Thread.Sleep(_frameDuration);
            }


        }
    }
}
