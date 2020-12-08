using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.DirectX;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Test.Encoder
{
    class D3D11VideoProcessor
    {

        public void Test()
        {
            //var inputFileName = @"Files\1920x1080.bmp";
            //var inputFileName = @"Files\NV12_1920x1080.yuv";
            //var inputFileName = @"Files\!!!TEST_NV12_1920x1080.raw";
            //var inputFileName = @"Files\rgba_1920x1080.raw";
            //var inputFileName = @"Files\rgba_352x288.raw";
            // var inputFileName = @"Files\test_nv12_1366x768_1408.raw";
            var inputFileName = @"Files\NV12_352x288.yuv";

            //
            Console.WriteLine("InputFile: " + inputFileName);
            //int inputWidth = 1920;
            //int inputHeight = 1080;


            int inputWidth = 352;
            int inputHeight = 288;


            //int inputWidth = 1366;
            //int inputHeight = 768;

            int outputWidth = 1920;
            int outputHeight = 1080;
            //int outputWidth = 1280;
            //int outputHeight = 720;
            //int outputWidth = 640;
            //int outputHeight = 480;

            //int outputWidth = 128;
            //int outputHeight = 64;
            try
            {
                SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

                var index = 0;

                var adapter = factory1.GetAdapter(index);

                Console.WriteLine("Adapter" + index + ": " + adapter.Description.Description);

                //var _flags = DeviceCreationFlags.VideoSupport |
                //             DeviceCreationFlags.BgraSupport |
                //             DeviceCreationFlags.Debug;

                var _flags = DeviceCreationFlags.None;

                var device = new SharpDX.Direct3D11.Device(adapter, _flags);
                using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                {
                    multiThread.SetMultithreadProtected(true);
                }



                var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = inputWidth,
                    Height = inputHeight,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.None,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    //Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    Format = SharpDX.DXGI.Format.NV12,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                };

                var srcBytes = File.ReadAllBytes(inputFileName);
                var inputTexture = DxTool.TextureFromDump(device, texDescr, srcBytes);

                // var inputTexture = new Texture2D(device, texDescr);
                //var inputTexture = WicTool.CreateTexture2DFromBitmapFile(inputFileName, device, texDescr);

                var outputTexture = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = outputWidth,
                    Height = outputHeight,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                   // Format = SharpDX.DXGI.Format.NV12,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                });

                Console.WriteLine("QueryInterface<VideoDevice>()");
                var videoDevice = device.QueryInterface<VideoDevice>();

                var deviceContext = device.ImmediateContext;

                Console.WriteLine("QueryInterface<VideoContext>()");
                var videoContext = deviceContext.QueryInterface<VideoContext>();

                /*
                 *       D3D11_VIDEO_PROCESSOR_CONTENT_DESC contentDesc =
                        {
                            D3D11_VIDEO_FRAME_FORMAT_PROGRESSIVE,
                            { 1, 1 }, inDesc.Width, inDesc.Height,
                            { 1, 1 }, outDesc.Width, outDesc.Height,
                            D3D11_VIDEO_USAGE_PLAYBACK_NORMAL
                        };
                 */


                VideoProcessorContentDescription descr = new VideoProcessorContentDescription
                {
                    InputFrameFormat = VideoFrameFormat.Progressive,
                    InputFrameRate = new Rational(1, 1),
                    InputWidth = inputWidth,
                    InputHeight = inputHeight,

                    OutputFrameRate = new Rational(1, 1),
                    OutputWidth = outputWidth,
                    OutputHeight = outputHeight,
                    Usage = VideoUsage.PlaybackNormal,
                };

                Console.WriteLine("CreateVideoProcessorEnumerator(...)");

                videoDevice.CreateVideoProcessorEnumerator(ref descr, out var videoEnumerator);

                Console.WriteLine("CreateVideoProcessor(...)");
                videoDevice.CreateVideoProcessor(videoEnumerator, 0, out var videoProcessor);

                // D3D11_VIDEO_PROCESSOR_INPUT_VIEW_DESC inputVD = { 0, D3D11_VPIV_DIMENSION_TEXTURE2D,{ 0,0 } };
                VideoProcessorInputViewDescription inputViewDescr = new VideoProcessorInputViewDescription
                {
                    FourCC = 0,
                    Dimension = VpivDimension.Texture2D,
                    Texture2D = new Texture2DVpiv
                    {
                        MipSlice = 0,
                        ArraySlice = 0,
                    },
                };

                Console.WriteLine("CreateVideoProcessorInputView(...)");
                videoDevice.CreateVideoProcessorInputView(inputTexture, videoEnumerator, inputViewDescr, out var videoInputView);

                VideoProcessorOutputViewDescription outputViewDescr = new VideoProcessorOutputViewDescription
                {
                    Dimension = VpovDimension.Texture2D,
                };

                Console.WriteLine("CreateVideoProcessorOutputView(...)");
                videoDevice.CreateVideoProcessorOutputView(outputTexture, videoEnumerator, outputViewDescr, out var videoOutputView);

                //D3D11_VIDEO_PROCESSOR_STREAM stream = { TRUE, 0, 0, 0, 0, nullptr, pVPIn, nullptr };
                VideoProcessorStream videoProcessorStream = new VideoProcessorStream
                {
                    Enable = true,
                    OutputIndex = 0,
                    InputFrameOrField = 0,
                    PastFrames = 0,
                    FutureFrames = 0,
                    PInputSurface = videoInputView,
                };

                Console.WriteLine("VideoProcessorBlt(...)");
                videoContext.VideoProcessorBlt(videoProcessor, videoOutputView, 0, 1, new VideoProcessorStream[] { videoProcessorStream });

                var bytes = DxTool.DumpTexture(device, outputTexture);

                var outputDescr = outputTexture.Description;
                var _fileName = "!!!!TEST_" + outputDescr.Format + "_" + outputDescr.Width + "x" + outputDescr.Height + ".raw";
                File.WriteAllBytes(_fileName, bytes);

                Console.WriteLine("OutputFile: " + _fileName);

                outputTexture.Dispose();
                inputTexture.Dispose();


                videoContext.Dispose();
                videoProcessor.Dispose();
                videoDevice.Dispose();
                videoEnumerator.Dispose();
                videoInputView.Dispose();
                videoOutputView?.Dispose();

                device?.Dispose();
                adapter?.Dispose();
                factory1.Dispose();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }



        }

    }
}
