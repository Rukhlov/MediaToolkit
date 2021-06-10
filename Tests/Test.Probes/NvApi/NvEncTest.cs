using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MediaToolkit.Nvidia;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using static MediaToolkit.Nvidia.LibNvEnc;
using GDI = System.Drawing;

namespace Test.Encoder
{
    class NvEncTest
    {

        public static void Run()
        {
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


            var encoder = OpenEncoderForDirectX(device.NativePointer);

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

            encoder.InitializeEncoder(ref initparams);

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
            encoder.RegisterResource(ref reg);

            int count = 1;
            while (count-- > 0)
            {

                var pic = new NvEncPicParams
                {
                    Version = NvEncodeAPI.NV_ENC_PIC_PARAMS_VER,
                    PictureStruct = NvEncPicStruct.Frame,
                    InputBuffer = reg.AsInputPointer(),
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
