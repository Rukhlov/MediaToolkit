using FFmpegLib;
using MediaToolkit.Codecs;
using MediaToolkit.Core;
using MediaToolkit.DirectX;
using MediaToolkit.MediaFoundation;
using MediaToolkit.Utils;
using SharpDX.Direct3D11;

using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using SharpDX.Multimedia;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Device3D11 = SharpDX.Direct3D11;
using Device3D9 = SharpDX.Direct3D9;

namespace Test.Probe
{
    partial class VideoDecoderTest
    {
        public static void Run()
        {
            Console.WriteLine("VideoDecoderTest::Run()");
            try
            {
                MediaToolkit.Core.VideoDriverType driverType = MediaToolkit.Core.VideoDriverType.Cuda;


				//// string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_30fps_1sec_bf0.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_1sec.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_30fps_60sec.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_320x240_yuv420p_Iframe.h264";
				//var width = 320;
				//var height = 240;
				//var fps = 30;

				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_640x480_yuv420p_4frame.h264";
				//var width = 640;
				//var height = 480;\


				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv444p_30fps_30sec_bf0.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\smptebars_1280x720_nv12_30fps_30sec_bf0.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_yuv420p_30fps_30sec_bf0.h264";
				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_1280x720_nv12_30fps_30sec_bf0.h264";
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

				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_2560x1440_yuv420p_Iframe.h264";
				//var width = 2560;
				//var height = 1440;
				//var fps = 30;

				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_3840x2160_5sec.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_30fps_10sec_bf0.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_Iframe.h264";
				//var width = 3840;
				//var height = 2160;
				//var fps = 30;



				//string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\test_mov_annexb_4096x2304_5sec.h264";
				////string fileName = @"..\..\..\Resources\Utils\FFmpegBatch\output\testsrc_3840x2160_yuv420p_Iframe.h264";
				//var width = 4096;
				//var height = 2304;
				//var fps = 30;

				VideoDecoderTest test = new VideoDecoderTest();
                try
                {
                    test.Start(fileName, width, height, fps, driverType);
                }
                finally
                {

                    test.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private Device3D9.DeviceEx deviceEx = null;
        private Device3D11.Device device3D11 = null;
        private Texture2D sharedTexture = null;
        //private Device3D9.Surface destSurf = null;
        private SharpDX.Direct3D9.Texture sharedTex9 = null;
        private MfH264Decoder decoder = null;

        private D3D11Presenter presenter = null;

        private MfVideoProcessor xvp = null;
        //private RgbProcessor rgbProcessor = new RgbProcessor();

        private PresentationClock presentationClock = new PresentationClock();

       // private NalSourceReader sourceReader = null;
        private NalSourceReaderRealTime sourceReader = null;

        private int videoBuffeSize = 3;
        private BlockingCollection<VideoFrame> videoFrames = null;

        // private ConcurrentQueue<Frame> videoQueue = null;
        //private Queue<Frame> frames = new Queue<Frame>(4);

        private int VideoAdapterIndex = 0;
        private bool lowLatency = true;
        private bool xvpMode = false;

        private bool running = false;
        private bool dxgiNv12Supported = false;
        public void Start(string fileName, int width, int height, int fps, MediaToolkit.Core.VideoDriverType driverType)
        {
            Console.WriteLine("Start(...) " + string.Join(" ", fileName, width, height, fps, driverType));
            var inputArgs = new MfVideoArgs
            {
                Width = width,
                Height = height,

                //Width = 320,
                //Height = 240,
                FrameRate = MfTool.PackToLong(fps, 1),
                LowLatency = lowLatency,
            };

            string adapterInfo = "";

            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                using (var adapter = dxgiFactory.GetAdapter1(VideoAdapterIndex))
                {
                    adapterInfo = adapter.Description.Description;

                    device3D11 = new Device3D11.Device(adapter,
                       // DeviceCreationFlags.Debug |
                       // DeviceCreationFlags.VideoSupport |
                       DeviceCreationFlags.BgraSupport);

					using (var multiThread = device3D11.QueryInterface<SharpDX.Direct3D11.Multithread>())
					{
						multiThread.SetMultithreadProtected(true);
					}

					//using (var dxgiDevice = device3D11.QueryInterface<SharpDX.DXGI.Device1>())
					//{
					//    dxgiDevice.MaximumFrameLatency = 16;
					//    dxgiDevice.GPUThreadPriority = 1;
					//}

					var formatSupport = device3D11.CheckFormatSupport(SharpDX.DXGI.Format.NV12);
                    dxgiNv12Supported = formatSupport.HasFlag(FormatSupport.Texture2D);

                    var log = MfTool.LogEnumFlags(formatSupport);
                    Console.WriteLine("D3D11:: NV12 support: " + log);

                    //formatSupport = device3D11.CheckFormatSupport(SharpDX.DXGI.Format.B8G8R8A8_UNorm);
                    //log = MfTool.LogEnumFlags(formatSupport);
                    //Console.WriteLine("D3D11:: B8G8R8A8 support: " + log);
                }
            }


            if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
            {
                if (!dxgiNv12Supported)
                {
                    throw new NotSupportedException("NV12 format not supported");
                }

                sharedTexture = new Texture2D(device3D11,
                    new Texture2DDescription
                    {
                        CpuAccessFlags = CpuAccessFlags.None,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        //Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                        Width = width,
                        Height = height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Default,

                        OptionFlags = ResourceOptionFlags.Shared,

                    });

                if (xvpMode)
                {
                    InitXVP(width, height);
                }
                else
                {
                    InitVideoProcessor(width, height);
                }


                inputArgs.D3DPointer = device3D11.NativePointer;
            }
            else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
            {
                var direct3D = new Device3D9.Direct3DEx();
                var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

                var presentParams = new Device3D9.PresentParameters
                {
                    BackBufferWidth = width,
                    BackBufferHeight = height,
                    DeviceWindowHandle = hWnd,
                    BackBufferCount = 1,
                    Windowed = true,
                    MultiSampleType = Device3D9.MultisampleType.None,
                    SwapEffect = Device3D9.SwapEffect.Discard,
                    PresentFlags = Device3D9.PresentFlags.Video,
                };

                var flags = Device3D9.CreateFlags.HardwareVertexProcessing |
                            Device3D9.CreateFlags.Multithreaded;

                var deviceType = Device3D9.DeviceType.Hardware;
                var nv12FourCC = new FourCC("NV12");
                var sourceFormat = (Device3D9.Format)((int)nv12FourCC);
                var targetFormat = Device3D9.Format.A8R8G8B8;

                bool result = direct3D.CheckDeviceFormatConversion(VideoAdapterIndex, deviceType, sourceFormat, targetFormat);
                Console.WriteLine("D3D9::CheckDeviceFormatConversion(...): " + sourceFormat + " " + targetFormat + " " + result);
                if (!result)
                {
                    throw new NotSupportedException("CheckDeviceFormatConversion(...) " + sourceFormat + " " + targetFormat);
                }

                deviceEx = new Device3D9.DeviceEx(direct3D, VideoAdapterIndex, deviceType, hWnd, flags, presentParams);
                var caps = deviceEx.Capabilities;
                var canStretchRectFromTextures = caps.DeviceCaps2.HasFlag(Device3D9.DeviceCaps2.CanStretchRectFromTextures);

                if (!canStretchRectFromTextures)
                {
                    throw new NotSupportedException("canStretchRectFromTextures " + canStretchRectFromTextures);
                }

                inputArgs.D3DPointer = deviceEx.NativePointer;


                IntPtr sharedHandle = IntPtr.Zero;

                this.sharedTex9 = new SharpDX.Direct3D9.Texture(deviceEx, width, height, 1,
                    Device3D9.Usage.RenderTarget, Device3D9.Format.A8R8G8B8, Device3D9.Pool.Default,
                    ref sharedHandle);

                this.sharedTexture = device3D11.OpenSharedResource<Texture2D>(sharedHandle);


                direct3D.Dispose();
            }


            inputArgs.DriverType = driverType;


            presenter = new D3D11Presenter(device3D11);

            //var readerTask = new Task(() =>
            //{
            //    Console.WriteLine("SourceReaderTask BEGIN");
            //    SourceReaderTask(fileName, inputArgs);
            //    Console.WriteLine("SourceReaderTask END");
            //});

            sourceReader = new NalSourceReaderRealTime();
           // sourceReader = new NalSourceReader();


            //var readerTask = nalSource.Start(fileName, inputArgs);

            var decoderTask = new Task(() =>
            {
                Console.WriteLine("DecoderTask BEGIN");

				if(driverType == VideoDriverType.Cuda)
				{
					NvDecDecoderTask(inputArgs);
				}
				else
				{
					DecoderTask(inputArgs);
					//FFmpegDecoderTask(inputArgs);
				}

				Console.WriteLine("DecoderTask END");
            });

            var presenterTask = new Task(() =>
            {
                Console.WriteLine("PresenterTask BEGIN");

                PresenterTaskRealTime(fps);

                //PresenterTask(fps);

                Console.WriteLine("PresenterTask END");
            });

            var size = new System.Drawing.Size(width, height);
            Form f = new Form
            {
                ClientSize = size,
                Text = driverType + " " + adapterInfo + " " + fileName,
            };

            f.SizeChanged += (o, e) =>
            {
                if (presenter != null)
                {
                    presenter.Resize(f.ClientSize);
                    //presenter.RenderSize = f.ClientSize;
                }
            };
            f.KeyDown += (o, e) =>
            {
                if (e.KeyCode == Keys.D)
                {
                    presenter.ShowLabel = !presenter.ShowLabel;
                    Console.WriteLine("ShowLabel == " + presenter.ShowLabel);
                }
                else if (e.KeyCode == Keys.A)
                {
                    presenter.AspectRatio = !presenter.AspectRatio;
                    Console.WriteLine("AspectRatio == " + presenter.AspectRatio);
                }
                else if (e.KeyCode == Keys.V)
                {
                    presenter.VSync = !presenter.VSync;
                    Console.WriteLine("VSync == " + presenter.VSync);
                }
                else if (e.KeyCode == Keys.Add)
                {
                    //sourceReader.WaitDelay += 1;
                    //Console.WriteLine(sourceReader.WaitDelay);

                    //presentationClock.ClockRate += 0.05f;
                    //Console.WriteLine(presentationClock.ClockRate);
                }
                else if (e.KeyCode == Keys.Subtract)
                {
                    //sourceReader.WaitDelay -= 1;
                    //Console.WriteLine(sourceReader.WaitDelay);

                    //sourceReader.PacketInterval -= 0.001f;
                    //Console.WriteLine(sourceReader.PacketInterval);
                    //presentationClock.ClockRate -= 0.05f;
                    //Console.WriteLine(presentationClock.ClockRate);
                }
                else if (e.KeyCode == Keys.Space)
                {
                    presentationClock.Paused = !presentationClock.Paused;
                    Console.WriteLine("Paused == " + presentationClock.Paused);
                }
            };

            f.FormClosed += (o, e) =>
            {
                running = false;
                presenter?.Stop();
                sourceReader?.Stop();
            };

            f.Shown += (o, e) =>
            {
                running = true;

                var hwnd = f.Handle;
                presenter.Setup(size, size, hwnd, VideoAdapterIndex);
                presenter.AspectRatio = true;
                presenter.FramePerSec = 60;
                presenter.RenderSize = f.ClientSize;
                presenter.Start();

                //readerTask.Start();
                sourceReader.Start(fileName, inputArgs);
                decoderTask.Start();
                presenterTask.Start();

            };

            Application.Run(f);
            running = false;

            Task.WaitAll(decoderTask, presenterTask);


            presenter.Close();


        }

        private void PresenterTask(int presenterFps)
        {
            PerfCounter perfCounter = new PerfCounter();

            try
            {
                //presenterFps = 1;
                //videoQueue = new ConcurrentQueue<Frame>();
                videoFrames = new BlockingCollection<VideoFrame>(videoBuffeSize);

                double perFrameInterval = 1.0 / presenterFps;
                double perFrame_1_4th = perFrameInterval / 4;
                double perFrame_3_4th = 3 * perFrame_1_4th;

                //while(videoFrameBuffer.Count<7)
                while (videoFrames.IsAddingCompleted)
                //while (videoQueue.Count < videoBuffeSize)
                {
                    Thread.Sleep(1);

                    if (!running)
                    {
                        break;
                    }
                }

                Console.WriteLine("videoQueue.IsAddingCompleted");
                presentationClock.Reset();
                AutoResetEvent syncEvent = new AutoResetEvent(false);
                VideoFrame frame = null;
                try
                {
                    double frameTime = 0;
                    while (running)
                    {
                        bool presentNow = true;
                        int delay = 1;//(int)(presentInterval * 1000);


                        //while(videoFrameBuffer.Count>0)
                        while (videoFrames.Count > 0)
                        {
                            presentNow = true;
                            if (frame == null)
                            {
                                //frame = GetFrame();

                                // bool frameTaken = videoQueue.TryDequeue(out frame);
                                bool frameTaken = videoFrames.TryTake(out frame, 10);
                                if (!frameTaken)
                                {
                                    Console.WriteLine("frameTaken == false");
                                    continue;
                                }

                                //frame = videoQueue.Take();
                                //frame = frames.Dequeue();
                            }

                            if (frame.time < frameTime)
                            {
                                Console.WriteLine("Non monotonic time: " + frame.time + "<" + frameTime);
                                if (frame != null)
                                {
                                    frame.Dispose();
                                    frame = null;
                                }
                                continue;
                            }

                            frameTime = frame.time;
                            var delta = frame.time - presentationClock.GetTime();
                            if (delta < -perFrame_1_4th)
                            {// This sample is late.
                                presentNow = true;
                            }
                            else if (delta > perFrame_3_4th)
                            {// This sample is still too early. Go to sleep.
                                presentNow = false;
                                delay = (int)((delta - perFrame_3_4th) * 1000);
                            }

                            if (!presentNow && delay > 0 && running)
                            {
                                if (delay > 3000)
                                {
                                    Console.WriteLine(delay);
                                    delay = 3000;
                                }

                                //Debug.WriteLine(delay);
                                syncEvent.WaitOne(delay);
                                continue;
                            }

                            if (delta < -perFrameInterval * 3)
                            {
                                Console.WriteLine("Sample is too late: " + delta);
                                //if (frame != null)
                                //{
                                //	frame.Dispose();
                                //	frame = null;
                                //}
                                //continue;
                            }

                            try
                            {
                                var cpuReport = perfCounter.GetReport();
                                var timeNow = presentationClock.GetTime();
                                int timeDelta = (int)((frame.time - timeNow) * 1000);
                                var text = cpuReport + "\r\n"
                                    + timeNow.ToString("0.000") + "\r\n"
                                    + frame.time.ToString("0.000") + "\r\n"
                                    + timeDelta + "\r\n"
                                    + frame.seq;

                                presenter.Update(frame.tex, text);
                            }
                            finally
                            {
                                if (frame != null)
                                {
                                    frame.Dispose();
                                    frame = null;
                                }
                            }
                        }

                        syncEvent.WaitOne(10);
                    }

                }
                finally
                {
                    if (frame != null)
                    {
                        frame.Dispose();
                        frame = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                running = false;
            }
            finally
            {
                if (videoFrames != null && videoFrames.Count > 0)
                {
                    foreach (var f in videoFrames)
                    {
                        if (f != null)
                        {
                            f.Dispose();
                        }
                    }
                    videoFrames.Dispose();
                    videoFrames = null;
                }

                if (perfCounter != null)
                {
                    perfCounter.Dispose();
                    perfCounter = null;
                }
            }


        }


        RgbProcessor rgbProcessor = null;
        private void DecoderTask(MfVideoArgs inputArgs)
        {

            try
            {
                rgbProcessor = new RgbProcessor();
                var srcSize = new System.Drawing.Size(inputArgs.Width, inputArgs.Height);
                rgbProcessor.Init(device3D11, srcSize, MediaToolkit.Core.PixFormat.NV12, srcSize, MediaToolkit.Core.PixFormat.RGB32);

                decoder = new MfH264Decoder();
                decoder.Setup(inputArgs);

                decoder.Start();
                var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);

                Stopwatch sw = Stopwatch.StartNew();

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

                        //var packet = videoPackets.Dequeue();

                        using (var sample = MediaFactory.CreateSample())
                        {
                            try
                            {
                                sample.SampleTime = MfTool.SecToMfTicks(packet.time);
                                sample.SampleDuration = MfTool.SecToMfTicks(packet.duration);
                                var bytes = packet.data;
                                using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
                                {
                                    var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
                                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                                    mediaBuffer.CurrentLength = bytes.Length;
                                    mediaBuffer.Unlock();

                                    sample.AddBuffer(mediaBuffer);
                                }
                                // Console.WriteLine(">>>>>>>>>> ProcessSample(...) BEGIN " + (procCount++));
                                var res = decoder.ProcessSample(sample, OnSampleDecoded);
                                if (res == DecodeResult.Error)
                                {
                                    Console.WriteLine("decoder.ProcessSample == " + res);
                                }


                                //var _ts = sw.ElapsedMilliseconds;
                                //var delta = _ts - timestamp;
                                //timestamp = _ts;
                                //sw.Restart();
                                ////Console.WriteLine("<<<<<<<<<<<< ProcessSample(...) " + res);
                                ////Console.WriteLine("--------------------------------------" + delta);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }

                    syncEvent.WaitOne(10);
                }

                decoder.Drain();

                var decodeResult = DecodeResult.Ok;
                do
                {
                    //Console.WriteLine("ProcessSample(...) " + (procCount++));
                    decodeResult = decoder.ProcessSample(null, OnSampleDecoded);
                } while (decodeResult == DecodeResult.Ok);

                var totalMilliseconds = sw.ElapsedMilliseconds;
                Console.WriteLine("TotalMilliseconds=" + totalMilliseconds + " MSecPerFrame=" + (totalMilliseconds / (double)decodedCount));

                decoder.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                running = false;
            }
            finally
            {
                if (decoder != null)
                {
                    decoder.Close();
                    decoder = null;
                }

                if (rgbProcessor != null)
                {
                    rgbProcessor.Close();
                    rgbProcessor = null;
                }

            }
        }


        private void FFmpegDecoderTask(MfVideoArgs inputArgs)
        {

            RgbProcessor processor = null;
            H264Decoder decoder = null;
            try
            {
                decoder = new H264Decoder();
                var size = new System.Drawing.Size(inputArgs.Width, inputArgs.Height);
                VideoEncoderSettings settings = new VideoEncoderSettings
                {
                    EncoderFormat = VideoCodingFormat.H264,
                    Width = inputArgs.Width,
                    Height = inputArgs.Height,
                };

                decoder.Setup(settings);

                processor = new RgbProcessor();
                var srcSize = new System.Drawing.Size(inputArgs.Width, inputArgs.Height);

                // processor.Init(device3D11, srcSize, MediaToolkit.Core.PixFormat.RGB32, srcSize, MediaToolkit.Core.PixFormat.RGB32);
				processor.Init(device3D11, srcSize, MediaToolkit.Core.PixFormat.I420, srcSize, MediaToolkit.Core.PixFormat.RGB32);

				processor.BackColor = SharpDX.Color.Yellow;
				var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);
                Stopwatch sw = Stopwatch.StartNew();

                while (sourceReader.IsFull)
                //while (sourceReader.Count < 2)
                {
                    Thread.Sleep(1);
                    if (!running)
                    {
                        break;
                    }
                }

                Action<IVideoFrame> OnDataDecoded = new Action<IVideoFrame>((frame) =>
                {
                    //Console.WriteLine("OnDataDecoded() " + time);
                    var time = frame.Time;
                    var frameBuffer = frame.Buffer;

                    Texture2D[] srcTextures = null;
                    if (frame.Format == PixFormat.RGB32)
                    {
                        var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                        {
                            Width = size.Width,
                            Height = size.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                            BindFlags = BindFlags.ShaderResource,
                            Usage = ResourceUsage.Immutable,
                            CpuAccessFlags = CpuAccessFlags.None,
                            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,

                            OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                        };
                        var ptr = frameBuffer[0].Data;
                        var stride = frameBuffer[0].Stride;
                        SharpDX.DataBox[] data =
                        {
                            new SharpDX.DataBox(ptr, stride, 0),
                        };
                        var texture = new Texture2D(device3D11, texDescr, data);
                        srcTextures = new Texture2D[] { texture };
                    }
                    else
                    {

                        var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                        {
                            Width = size.Width,
                            Height = size.Height,
                            MipLevels = 1,
                            ArraySize = 1,
                            SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                            BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                            Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                            CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                            Format = SharpDX.DXGI.Format.R8_UNorm,

                            OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                        };
                        var lumaPtr = frameBuffer[0].Data;
                        var lumaStride = frameBuffer[0].Stride;

                        SharpDX.DataBox[] lumaData =
                        {
                            new SharpDX.DataBox(lumaPtr,  lumaStride, 0),
                        };
                        var lumaTexture = new Texture2D(device3D11, texDescr, lumaData);
                        var pixFormat = frame.Format;

                        if (pixFormat == PixFormat.I420)
                        {
                            texDescr.Width = size.Width / 2;
                            texDescr.Height = size.Height / 2;
                        }
                        else if (pixFormat == PixFormat.I422)
                        {
                            texDescr.Width = size.Width / 2;
                        }
                        else if (pixFormat == PixFormat.I444) { }
                        else
                        {
                            throw new NotSupportedException("Invalid format " + pixFormat);
                        }

                        var chromaCbPtr = frameBuffer[1].Data;
                        var chromaCbStride = frameBuffer[1].Stride;
                        SharpDX.DataBox[] cbData =
                        {
                            new SharpDX.DataBox(chromaCbPtr, chromaCbStride, 0),
                        };
                        var cbTexture = new Texture2D(device3D11, texDescr, cbData);

                        var chromaCrPtr = frameBuffer[2].Data;
                        var chromaCrStride = frameBuffer[2].Stride;
                        SharpDX.DataBox[] crData =
                        {
                            new SharpDX.DataBox(chromaCrPtr, chromaCrStride, 0),
                        };
                        var crTexture = new Texture2D(device3D11, texDescr, crData);

                        srcTextures = new Texture2D[] { lumaTexture, cbTexture, crTexture };
                    }

                    var destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        Width = size.Width,
                        Height = size.Height,
						// Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
						Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
						SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,

                    });

                    lock (device3D11)
                    {
                        processor.DrawTexture(srcTextures, destTexture, size);
                    }
					
					

					//device3D11.ImmediateContext.CopyResource(srcTextures[0], destTexture);
					OnSampleProcessed(destTexture, time);

                    foreach (var t in srcTextures)
                    {
                        DxTool.SafeDispose(t);
                    }
                });

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

                        //var packet = videoPackets.Dequeue();

                        try
                        {
                            var res = decoder.Decode(packet.data, packet.time, OnDataDecoded);

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }

                    syncEvent.WaitOne(10);
                }

                if (decoder.Drain())
                {
                    var decodeResult = 0;
                    do
                    {
                        //Console.WriteLine("ProcessSample(...) " + (procCount++));
                        decodeResult = decoder.Decode(null, 0, OnDataDecoded);
                    } while (decodeResult == 0);

                    var totalMilliseconds = sw.ElapsedMilliseconds;
                    Console.WriteLine("TotalMilliseconds=" + totalMilliseconds + " MSecPerFrame=" + (totalMilliseconds / (double)decodedCount));

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                running = false;
            }
            finally
            {
                if (processor != null)
                {
                    processor.Close();
                    processor = null;
                }

                if (decoder != null)
                {
                    decoder.Close();
                    decoder = null;
                }

            }
        }

        private Stopwatch stopwatch = new Stopwatch();
        long prevTimestamp = 0;
        private long decoderTime = 0;
        private ulong decodedCount = 0;
        private void OnSampleDecoded(Sample s)
        {
            try
            {
                if (!running)
                {
                    return;
                }

                var driverType = decoder.DriverType;
                var sampleTime = s.SampleTime;

                // в Win7 декодер возвращает не правильную длительность!
                // для видео с b-frame
                var sampleDur = s.SampleDuration;

                sampleDur = MfTool.SecToMfTicks(sourceReader.PacketInterval);


                //if (decoderTime > sampleTime)
                //{
                //    Console.WriteLine(decoderTime + ">" + sampleTime + " " + MfTool.MfTicksToSec(decoderTime-sampleTime));
                //}

                decoderTime += sampleDur;//sampleDur;

                //var timestamp = stopwatch.ElapsedMilliseconds;
                //var delta = timestamp - prevTimestamp;
                //prevTimestamp = timestamp;
                //Console.WriteLine("onSampleDecoded(...) " + decodedCount  + " " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(monotonicTime) + " " + timestamp + " " + delta);
                //stopwatch.Restart();


                //Console.WriteLine(MfTool.LogSample(s));
                //Console.WriteLine("-------------------");

                if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
                {
                    ProcessD3D11Sample(s, decoderTime);
                }
                else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
                {
                    ProcessD3D9Sample(s, decoderTime);
                }
                else
                {
                    ProcessCPUSample(s, decoderTime);
                }
            }
            finally
            {
                if (s != null)
                {
                    s.Dispose();
                    s = null;
                }
            }

            decodedCount++;

        }

        private void ProcessD3D9Sample(Sample s, long decoderTime)
        {

            using (var buffer = s.ConvertToContiguousBuffer())
            {
                MediaFactory.GetService(buffer, MediaServiceKeys.Buffer, IID.D3D9Surface, out var pSurf);

                using (var srcSurf = new SharpDX.Direct3D9.Surface(pSurf))
                {
                    var srcDescr = srcSurf.Description;
                    int width = srcDescr.Width;
                    int height = srcDescr.Height;

                    using (var sharedSurf9 = sharedTex9.GetSurfaceLevel(0))
                    {
                        deviceEx.StretchRectangle(srcSurf, sharedSurf9, Device3D9.TextureFilter.Linear);
                    }

                    deviceEx.Present();
                    //device3D11.ImmediateContext.Flush();

                    var texture = new Texture2D(device3D11,
                        new Texture2DDescription
                        {
                            Width = sharedTexture.Description.Width,
                            Height = sharedTexture.Description.Height,
                            Format = sharedTexture.Description.Format,
                            CpuAccessFlags = CpuAccessFlags.None,
                            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                            Usage = ResourceUsage.Default,
                            OptionFlags = ResourceOptionFlags.None,
                            SampleDescription = { Count = 1, Quality = 0 },
                            MipLevels = 1,
                            ArraySize = 1,

                        });

                    device3D11.ImmediateContext.CopySubresourceRegion(sharedTexture, 0, null, texture, 0);
                    device3D11.ImmediateContext.Flush();

                    var sampleTimeSec = MfTool.MfTicksToSec(s.SampleTime);
                    var decoderTimeSec = MfTool.MfTicksToSec(decoderTime);
                    OnSampleProcessed(texture, sampleTimeSec);


                }
            }
        }
        private void ProcessD3D9Sample2(Sample s, long decoderTime)
        {
            using (var buffer = s.ConvertToContiguousBuffer())
            {
                MediaFactory.GetService(buffer, MediaServiceKeys.Buffer, IID.D3D9Surface, out var pSurf);

                using (var srcSurf = new SharpDX.Direct3D9.Surface(pSurf))
                {
                    var srcDescr = srcSurf.Description;
                    int width = srcDescr.Width;
                    int height = srcDescr.Height;


                    IntPtr sharedHandle = IntPtr.Zero;
                    using (var sharedTexture9 = new SharpDX.Direct3D9.Texture(deviceEx, width, height, 1,
                        Device3D9.Usage.RenderTarget, Device3D9.Format.A8R8G8B8, Device3D9.Pool.Default,
                        ref sharedHandle))
                    {
                        using (var sharedSurf9 = sharedTexture9.GetSurfaceLevel(0))
                        {
                            deviceEx.StretchRectangle(srcSurf, sharedSurf9, Device3D9.TextureFilter.Linear);
                        }
                        deviceEx.Present();

                        var texture2d = device3D11.OpenSharedResource<Texture2D>(sharedHandle);

                        //using (var texture2d = device3D11.OpenSharedResource<Texture2D>(sharedHandle))
                        //{ //Dx9->Dx11 работает только на primary адаптере т.е AdatperIndex = 0;
                        //  // на вторичном адаптере текстура DX11 получается пустая, хотя исходная Dx9 с данными... 
                        //  // может быть это ограничение DX!??

                        //    device3D11.ImmediateContext.CopyResource(texture2d, SharedTexture);
                        //}

                        //Иначе может утекать память на Win7
                        //https://stackoverflow.com/questions/32428682/directx-9-to-11-opensharedresource-is-leaking-memory-like-crazy-am-i-doing-some
                        //device3D11.ImmediateContext.Flush();

                        var time = MfTool.MfTicksToSec(s.SampleTime);

                        //Thread.Sleep(10);
                        OnSampleProcessed(texture2d, time);

                    };
                }
            }
        }

        Stopwatch _sw = new Stopwatch();
        private void ProcessD3D11Sample(Sample s, long decoderTime)
        {
            if (xvpMode)
            {
                ProcessD3D11SampleXvp(s, decoderTime);
            }
            else
            {
                using (var buffer = s.ConvertToContiguousBuffer())
                {
                    using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                    {
                        dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                        var subresourceIndex = dxgiBuffer.SubresourceIndex;
                        using (Texture2D texture = new Texture2D(intPtr))
                        {
                            Texture2D destTexture = ConvertToRgb32(subresourceIndex, texture);

                            //Texture2D destTexture = VideoProcessorConvert(subresourceIndex, texture);

                            var sampleTimeSec = MfTool.MfTicksToSec(s.SampleTime);
                            var decoderTimeSec = MfTool.MfTicksToSec(decoderTime);
                            OnSampleProcessed(destTexture, decoderTimeSec);
                        }
                    }
                }
            }



            //Console.WriteLine(_sw.ElapsedMilliseconds);
        }

        private void ProcessD3D11SampleXvp(Sample s, long decoderTime)
        {
            var res = xvp.ProcessSample(s, out Sample rgbSample);
            if (res)
            {
                using (var buffer = rgbSample.ConvertToContiguousBuffer())
                {
                    using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                    {
                        dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                        using (Texture2D texture = new Texture2D(intPtr))
                        {
                            var subresourceIndex = dxgiBuffer.SubresourceIndex;

                            var destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                            {
                                Width = texture.Description.Width,
                                Height = texture.Description.Height,
                                Format = texture.Description.Format,
                                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                                MipLevels = 1,
                                ArraySize = 1,
                            });

                            device3D11.ImmediateContext.CopySubresourceRegion(texture, subresourceIndex, null, destTexture, 0);
                            //device3D11.ImmediateContext.Flush();

                            var time = MfTool.MfTicksToSec(s.SampleTime);
                            OnSampleProcessed(destTexture, time);
                        }
                    }
                }
            }

            if (rgbSample != null)
            {
                rgbSample.Dispose();
                rgbSample = null;
            }

        }

        private Texture2D VideoProcessorConvert(int subresourceIndex, Texture2D texture)
        {
            Texture2D outputTexture = null;
            Texture2D inputTexture = null;
            try
            {
                inputTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = texture.Description.Width,
                    Height = texture.Description.Height,
                    Format = texture.Description.Format,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.None,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                });

                device3D11.ImmediateContext.CopySubresourceRegion(texture, subresourceIndex, null, inputTexture, 0);
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

                VideoProcessorInputView inputView = null;
                try
                {
                    //Console.WriteLine("CreateVideoProcessorInputView(...)");
                    videoDevice.CreateVideoProcessorInputView(inputTexture, videoEnumerator, inputViewDescr, out inputView);

                    outputTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        Width = sharedTexture.Description.Width,
                        Height = sharedTexture.Description.Height,
                        Format = sharedTexture.Description.Format,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,
                    });

                    VideoProcessorOutputViewDescription outputViewDescr = new VideoProcessorOutputViewDescription
                    {
                        Dimension = VpovDimension.Texture2D,
                    };

                    VideoProcessorOutputView outputView = null;
                    try
                    {
                        videoDevice.CreateVideoProcessorOutputView(outputTexture, videoEnumerator, outputViewDescr, out outputView);

                        //D3D11_VIDEO_PROCESSOR_STREAM stream = { TRUE, 0, 0, 0, 0, nullptr, pVPIn, nullptr };
                        VideoProcessorStream videoProcessorStream = new VideoProcessorStream
                        {
                            Enable = true,
                            OutputIndex = 0,
                            InputFrameOrField = 0,
                            PastFrames = 0,
                            FutureFrames = 0,

                            PInputSurface = inputView,
                        };

                        //Console.WriteLine("VideoProcessorBlt(...)");
                        videoContext.VideoProcessorBlt(videoProcessor, outputView, 0, 1, new VideoProcessorStream[] { videoProcessorStream });

                    }
                    finally
                    {
                        DxTool.SafeDispose(outputView);
                    }

                }
                finally
                {
                    DxTool.SafeDispose(inputView);
                }

            }
            finally
            {
                DxTool.SafeDispose(inputTexture);
            }

            return outputTexture;
        }

        private Texture2D VideoProcessorConvert2(int subresourceIndex, Texture2D texture)
        {
            Texture2D destTexture = null;

            device3D11.ImmediateContext.CopySubresourceRegion(texture, subresourceIndex, null, inputTexture, 0);

            //D3D11_VIDEO_PROCESSOR_STREAM stream = { TRUE, 0, 0, 0, 0, nullptr, pVPIn, nullptr };
            VideoProcessorStream videoProcessorStream = new VideoProcessorStream
            {
                Enable = true,
                OutputIndex = 0,
                InputFrameOrField = 0,
                PastFrames = 0,
                FutureFrames = 0,

                PInputSurface = videoInputView,
                PpFutureSurfaces = null,
                PpPastSurfaces = null,
                PpFutureSurfacesRight = null,
                PpPastSurfacesRight = null,
                PInputSurfaceRight = null,
            };

            //Console.WriteLine("VideoProcessorBlt(...)");
            videoContext.VideoProcessorBlt(videoProcessor, videoOutputView, 0, 1, new VideoProcessorStream[] { videoProcessorStream });

            destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = sharedTexture.Description.Width,
                Height = sharedTexture.Description.Height,
                Format = sharedTexture.Description.Format,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
            });

            device3D11.ImmediateContext.CopyResource(sharedTexture, destTexture);
            return destTexture;
        }

        private Texture2D ConvertToRgb32(int subresourceIndex, Texture2D texture)
        {
            Texture2D destTexture = null;
            Texture2D srcTexture = null;
            try
            {
                srcTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = texture.Description.Width,
                    Height = texture.Description.Height,
                    Format = texture.Description.Format,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                });


                device3D11.ImmediateContext.CopySubresourceRegion(texture, subresourceIndex, null, srcTexture, 0);

                destTexture = new Texture2D(device3D11, new SharpDX.Direct3D11.Texture2DDescription()
                {
                    Width = texture.Description.Width,
                    Height = texture.Description.Height,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource | BindFlags.RenderTarget,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                });

                rgbProcessor.DrawTexture(srcTexture, destTexture);
            }
            finally
            {
                DxTool.SafeDispose(srcTexture);
            }

            return destTexture;
        }

        private void ProcessCPUSample(Sample s, long sampleTime)
        {
            // Console.WriteLine("onSampleDecoded(...) "  + sampleTime);

            var mediaType = decoder.OutputMediaType;
            var imageDims = MfTool.GetImageDimensions(mediaType);

            var size = new System.Drawing.Size(imageDims.Item1, imageDims.Item2);
            int stride = imageDims.Item3;

            using (var b = s.ConvertToContiguousBuffer())
            {

                Texture2D[] srcTextures = null;

                var _ptr = b.Lock(out var _maxLen, out var _currLen);

                if (dxgiNv12Supported)
                {
                    var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        Width = size.Width,
                        Height = size.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.NV12,

                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    };

                    SharpDX.DataBox[] initData =
                    {
                        new SharpDX.DataBox(_ptr,  stride, 0),
                    };

                    var nv12Texture = new Texture2D(device3D11, texDescr, initData);
                    srcTextures = new Texture2D[] { nv12Texture };
                }
                else
                {
                    var texDescr = new SharpDX.Direct3D11.Texture2DDescription()
                    {
                        Width = size.Width,
                        Height = size.Height,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                        BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                        Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                        CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                        Format = SharpDX.DXGI.Format.R8_UNorm,

                        OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                    };

                    SharpDX.DataBox[] lumaData =
                    {
                        new SharpDX.DataBox(_ptr,  stride, 0),
                    };

                    var lumaTexture = new Texture2D(device3D11, texDescr, lumaData);

                    texDescr.Format = SharpDX.DXGI.Format.R8G8_UNorm;
                    texDescr.Width = size.Width / 2;
                    texDescr.Height = size.Height / 2;

                    var chromaOffset = (stride * size.Height);
                    SharpDX.DataBox[] chromaData =
                    {
                        new SharpDX.DataBox((_ptr + chromaOffset), stride, 0),
                    };

                    var chromaTexture = new Texture2D(device3D11, texDescr, chromaData);

                    srcTextures = new Texture2D[] { lumaTexture, chromaTexture };
                }

                b.Unlock();

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


                rgbProcessor.DrawTexture(srcTextures, destTexture, size);

                foreach (var t in srcTextures)
                {
                    DxTool.SafeDispose(t);
                }

                //byte[] buf = new byte[_currLen];
                //var fileName = "CPU_" + size.Width + "x" + size.Height + "_" + format + "_" + sampleTime + ".raw";
                //var fullName = Path.Combine("decoded", fileName);
                //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, fullName);


                var sampleTimeSec = MfTool.MfTicksToSec(s.SampleTime);
                var decoderTimeSec = MfTool.MfTicksToSec(decoderTime);

                OnSampleProcessed(destTexture, decoderTimeSec);


            }
        }


        private void OnSampleProcessed(Texture2D tex, double time)
        {
            if (!running)
            {
                DxTool.SafeDispose(tex);
                return;
            }

            var arrivalTime = presentationClock.GetTime();
            // Console.WriteLine(arrivalTime + " " + time + " " + (arrivalTime - time));

            var frame = new VideoFrame
            {
                tex = tex,
                time = time,
                seq = decodedCount,
                arrival = arrivalTime,
            };

            //AddFrame(frame);

            videoFrames.Add(frame);

            //var texBytes = DxTool.DumpTexture(device3D11, SharedTexture);
            //var _descr = SharedTexture.Description;
            //var driverType = decoder.DriverType;

            //var fileName = driverType + "_" + _descr.Width + "x" + _descr.Height + "_" + _descr.Format + "_" + time + ".raw";
            //var fullName = Path.Combine("decoded", fileName);

            //File.WriteAllBytes(fullName, texBytes);
        }

        public void Close()
        {
            Console.WriteLine("Close()");

            DxTool.SafeDispose(sharedTexture);
            DxTool.SafeDispose(device3D11);

            DxTool.SafeDispose(sharedTex9);
            DxTool.SafeDispose(deviceEx);


            CloseVideoProcessor();

            //DxTool.SafeDispose(destSurf);

            //if (decoder != null)
            //{
            //	decoder.Stop();
            //	decoder.Close();
            //	decoder = null;
            //}

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


        class VideoFrame
        {
            public Texture2D tex = null;
            public double time = 0;
            public double duration = 0;
            public ulong seq = 0;
            public double arrival = 0;
            public void Dispose()
            {
                DxTool.SafeDispose(tex);
            }
        }


        class PresentationClock
        {
            private double clockRate = 1.0;
            public double ClockRate
            {
                get => clockRate;
                set
                {
                    if (value < 0)
                    {
                        clockRate = 0;
                    }
                    else if (value > 8)
                    {
                        clockRate = 8;
                    }
                    else
                    {
                        clockRate = value;
                    }
                }
            }
            public bool Paused { get; set; } = false;

            private double presentationTime = 0;
            private double lastTime = double.NaN;
            public double GetTime()
            {
                var currTime = MediaTimer.GetRelativeTime();
                if (!double.IsNaN(lastTime))
                {
                    if (!Paused)
                    {
                        presentationTime += (currTime - lastTime) * clockRate;
                    }
                }
                lastTime = currTime;


                return presentationTime;
            }

            public void Reset()
            {
                lastTime = double.NaN;
                presentationTime = 0;
            }
        }


        private VideoDevice videoDevice = null;
        private VideoContext videoContext = null;
        private VideoProcessorEnumerator videoEnumerator = null;
        private Device3D11.VideoProcessor videoProcessor = null;
        private VideoProcessorInputView videoInputView = null;
        private VideoProcessorOutputView videoOutputView = null;
        private Texture2D inputTexture = null;

        private void InitVideoProcessor(int width, int height)
        {
            Console.WriteLine("QueryInterface<VideoDevice>()");
            videoDevice = device3D11.QueryInterface<VideoDevice>();

            var deviceContext = device3D11.ImmediateContext;

            Console.WriteLine("QueryInterface<VideoContext>()");
            videoContext = deviceContext.QueryInterface<VideoContext>();


            VideoProcessorContentDescription descr = new VideoProcessorContentDescription
            {
                InputFrameFormat = VideoFrameFormat.Progressive,
                //InputFrameRate = new SharpDX.DXGI.Rational(1, 1),
                InputWidth = width,
                InputHeight = width,

                //OutputFrameRate = new SharpDX.DXGI.Rational(1, 1),
                OutputWidth = width,
                OutputHeight = width,
                Usage = VideoUsage.PlaybackNormal,


            };

            Console.WriteLine("CreateVideoProcessorEnumerator(...)");
            videoDevice.CreateVideoProcessorEnumerator(ref descr, out videoEnumerator);

            var videoProcCaps = videoEnumerator.VideoProcessorCaps;
            var rateConversionCapsCount = videoProcCaps.RateConversionCapsCount;
            for (int i = 0; i < rateConversionCapsCount; i++)
            {
                videoEnumerator.GetVideoProcessorRateConversionCaps(i, out var caps);

            }

            Console.WriteLine("CreateVideoProcessor(...)");
            videoDevice.CreateVideoProcessor(videoEnumerator, 0, out videoProcessor);

            //using (var videoContext1 = deviceContext.QueryInterface<VideoContext1>())
            //{
            //    videoContext1.VideoProcessorSetOutputColorSpace1(videoProcessor, SharpDX.DXGI.ColorSpaceType.RgbFullG10NoneP709);
            //}

            VideoProcessorOutputViewDescription outputViewDescr = new VideoProcessorOutputViewDescription
            {
                Dimension = VpovDimension.Texture2D,
            };

            Console.WriteLine("CreateVideoProcessorOutputView(...)");
            videoDevice.CreateVideoProcessorOutputView(sharedTexture, videoEnumerator, outputViewDescr, out var _videoOutputView);
            this.videoOutputView = _videoOutputView;


            inputTexture = new Texture2D(device3D11,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    //Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    Format = SharpDX.DXGI.Format.NV12,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    BindFlags = SharpDX.Direct3D11.BindFlags.None,
                    Usage = SharpDX.Direct3D11.ResourceUsage.Default,
                    CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                    OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,

                });

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
            videoDevice.CreateVideoProcessorInputView(inputTexture, videoEnumerator, inputViewDescr, out var _videoInputView);
            this.videoInputView = _videoInputView;
        }

        private void CloseVideoProcessor()
        {
            DxTool.SafeDispose(videoDevice);
            DxTool.SafeDispose(videoContext);
            DxTool.SafeDispose(videoEnumerator);
            DxTool.SafeDispose(videoProcessor);
            DxTool.SafeDispose(videoInputView);
            DxTool.SafeDispose(videoOutputView);
            DxTool.SafeDispose(inputTexture);
        }

        private void InitXVP(int width, int height)
        {
            xvp = new MfVideoProcessor(device3D11);

            var inProcArgs = new MfVideoArgs
            {
                Width = width,
                Height = height,
                Format = VideoFormatGuids.NV12,
            };

            var outProcArgs = new MfVideoArgs
            {
                Width = width,
                Height = height,
                Format = VideoFormatGuids.Argb32,
            };

            xvp.Setup(inProcArgs, outProcArgs);
            xvp.Start();

        }

        private void CloseXVP()
        {
            if (xvp != null)
            {
                xvp.Stop();
                xvp.Close();

            }
        }



        private double delayEstimate = 0;
        private double activeDelay = 0;

        bool isFirstTime = true;
        private double estimate1 = double.NaN;
        private double estimate2 = double.NaN;
        private double meanSkew = 0;
        private Tuple<double, double> prevPoint = null;
        private double AdjustmentDueToSkew(Tuple<double, double> currPoint, double offset = 0.0001, double smooth1 = 0.05, double smooth2 = 0.05, double maxdelay = 0.005)
        {
            double upBound = 1.0 + offset;
            double bottomBound = 1.0 - offset;

            double local = currPoint.Item1;
            double remote = currPoint.Item2;

            double adjustment = double.NaN;

            if (prevPoint != null)
            {
                double prevLocal = prevPoint.Item1;
                double prevRemote = prevPoint.Item2;

                double diffLocal = (local - prevLocal);
                double diffRemote = (remote - prevRemote);

                if (diffRemote != 0)
                {
                    var skew = diffLocal / diffRemote;

                    meanSkew = smooth1 * skew + (1 - smooth1) * meanSkew;

                    //Debug.WriteLine("meanSkew " + meanSkew + " x " + x + " prevX " + prevX + " y " + y + " prevY " + prevY + " diffX " + diffX + " diffY " + diffY);

                    if (double.IsInfinity(meanSkew))
                    {
                        throw new Exception("Inavalid meanSkew value (" + meanSkew + ")");
                    }
                }
                if (meanSkew > bottomBound && meanSkew < upBound)
                {
                    // mainLogger.Debug("meanSkew " + meanSkew);
                    double val = local - remote;
                    if (double.IsNaN(estimate1))
                    {
                        estimate1 = val;
                    }

                    estimate1 = (smooth1 * val + (1 - smooth1) * estimate1);

                    if (double.IsNaN(estimate2))
                    {
                        estimate2 = estimate1;
                    }

                    estimate2 = (smooth2 * estimate1 + (1 - smooth2) * estimate2);

                    if (isFirstTime)
                    {
                        isFirstTime = false;

                        activeDelay = estimate2;
                        delayEstimate = estimate2;
                    }
                    else
                    {
                        delayEstimate = estimate2;
                    }

                    double delay = (activeDelay - delayEstimate);

                    if (delay > maxdelay)
                    {
                        Console.WriteLine("meanSkew " + meanSkew + " activeDelay " + activeDelay + " delayEstimate " + delayEstimate + " delay " + delay);
                        activeDelay = delayEstimate;
                        adjustment = delay;


                    }
                    else if (delay < -maxdelay)
                    {
                        Console.WriteLine("meanSkew " + meanSkew + " activeDelay " + activeDelay + " delayEstimate " + delayEstimate + " delay " + delay);

                        activeDelay = delayEstimate;
                        adjustment = delay;

                    }
                }
                else if (meanSkew > 1.1 || meanSkew < 0.9) // (meanSkew > 1.5 || meanSkew < 0.5)
                {//large network delay

                    // mainLogger.Warn("meanSkew " + meanSkew);;
                }

            }

            prevPoint = currPoint;

            return adjustment;
        }



        //SortedList<long, VideoFrame> videoFrameBuffer = new SortedList<long, VideoFrame>(8);
        //AutoResetEvent waitEvent = new AutoResetEvent(false);
        //private void AddFrame(VideoFrame frame)
        //{
        //	while(videoFrameBuffer.Count > 8)
        //	{
        //		waitEvent.WaitOne(10);
        //	}

        //	{
        //		long timestamp = (long)(frame.time * 1000);
        //		videoFrameBuffer.Add(timestamp, frame);
        //	}

        //}

        //private VideoFrame GetFrame()
        //{
        //	VideoFrame frame = null;
        //	if (videoFrameBuffer.Count > 0)
        //	{
        //		var frameItem = videoFrameBuffer.FirstOrDefault();
        //		var timestamp = frameItem.Key;
        //		frame = frameItem.Value;
        //		videoFrameBuffer.Remove(timestamp);
        //	}

        //	if (videoFrameBuffer.Count < 8)
        //	{
        //		waitEvent.Set();
        //	}

        //    return frame;
        //}

    }

}
