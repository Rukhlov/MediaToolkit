using MediaToolkit.Codecs;
using MediaToolkit.DirectX;
using MediaToolkit.MediaFoundation;
using SharpDX.Direct3D11;

using SharpDX.MediaFoundation;
using SharpDX.MediaFoundation.DirectX;
using SharpDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Device3D11 = SharpDX.Direct3D11;
using Device3D9 = SharpDX.Direct3D9;

namespace Test.Probe
{
	class VideoDecoderTest
	{
        public static void Run()
		{
			Console.WriteLine("RgbToNv12Converter::Run()");
			try
			{
                MediaToolkit.Core.VideoDriverType driverType = MediaToolkit.Core.VideoDriverType.CPU;


                //// string fileName = @"Files\testsrc_320x240_yuv420p_30fps_1sec_bf0.h264";
                ////string fileName = @"Files\testsrc_320x240_yuv420p_1sec.h264";
                //string fileName = @"Files\testsrc_320x240_yuv420p_30fps_60sec.h264";
                ////string fileName = @"Files\testsrc_320x240_yuv420p_Iframe.h264";
                //var width = 320;
                //var height = 240;
                //var fps = 30;

                //string fileName = @"Files\testsrc_640x480_yuv420p_4frame.h264";
                //var width = 640;
                //var height = 480;


                //string fileName = @"Files\testsrc_1280x720_yuv420p_Iframe.h264";
                //var width = 1280;
                //var height = 720;


                string fileName = @"Files\testsrc_1920x1080_yuv420p_30fps_30sec_bf0.h264";
                //string fileName = @"Files\IFrame_1920x1080_yuv420p.h264";
                var width = 1920;
                var height = 1080;
                var fps = 30;

                //string fileName = @"Files\testsrc_2560x1440_yuv420p_Iframe.h264";
                //var width = 2560;
                //var height = 1440;

                //string fileName = @"Files\testsrc_3840x2160_yuv420p_Iframe.h264";
                //var width = 3840;
                //var height = 2160;

                VideoDecoderTest test = new VideoDecoderTest();
                test.Start(fileName, width, height, fps, driverType);

                test.Close();
            }
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

        private Device3D9.DeviceEx deviceEx = null;
        private Device3D11.Device device3D11 = null;
       // private Texture2D SharedTexture = null;
        //private Device3D9.Surface destSurf = null;
        private MfH264DecoderTest decoder = new MfH264DecoderTest();

        public void Start(string fileName, int width, int height, int fps, MediaToolkit.Core.VideoDriverType driverType)
        {
            var inputArgs = new MfVideoArgs
            {
                Width = width,
                Height = height,

                //Width = 320,
                //Height = 240,
                FrameRate = MfTool.PackToLong(fps, 1),
            };


            int adapterIndex = 0;
            using (var dxgiFactory = new SharpDX.DXGI.Factory1())
            {
                using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
                {
                    device3D11 = new Device3D11.Device(adapter,
                       // DeviceCreationFlags.Debug |
                       // DeviceCreationFlags.VideoSupport |
                       DeviceCreationFlags.BgraSupport);

                    using (var multiThread = device3D11.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
                }
            }

            //SharedTexture = new Texture2D(device3D11,
            //        new Texture2DDescription
            //        {
            //            CpuAccessFlags = CpuAccessFlags.None,
            //            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
            //            Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
            //                //Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
            //                Width = width,
            //            Height = height,
            //            MipLevels = 1,
            //            ArraySize = 1,
            //            SampleDescription = { Count = 1, Quality = 0 },
            //            Usage = ResourceUsage.Default,

            //            OptionFlags = ResourceOptionFlags.Shared,

            //        });

            if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
            {

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

                bool result = direct3D.CheckDeviceFormatConversion(adapterIndex, deviceType, sourceFormat, targetFormat);
                Console.WriteLine("CheckDeviceFormatConversion(...): " + sourceFormat + " " + targetFormat + " " + result);
                if (!result)
                {
                    throw new NotSupportedException("CheckDeviceFormatConversion(...) " + sourceFormat + " " + targetFormat);
                }

                deviceEx = new Device3D9.DeviceEx(direct3D, adapterIndex, deviceType, hWnd, flags, presentParams);
                var caps = deviceEx.Capabilities;
                var canStretchRectFromTextures = caps.DeviceCaps2.HasFlag(Device3D9.DeviceCaps2.CanStretchRectFromTextures);

                if (!canStretchRectFromTextures)
                {
                    throw new NotSupportedException("canStretchRectFromTextures " + canStretchRectFromTextures);
                }

                inputArgs.D3DPointer = deviceEx.NativePointer;

                direct3D.Dispose();
            }


            inputArgs.DriverType = driverType;

            decoder.Setup(inputArgs);

            decoder.Start();
            var frameRate = MfTool.UnPackLongToInts(inputArgs.FrameRate);

            int procCount = 0;
            double sampleInterval = (double)frameRate[1] / frameRate[0];
            long samplesCount = 0;
            double sampleTime = 0;
            var stream = new FileStream(fileName, FileMode.Open);
            var nalReader = new NalUnitReader(stream);
            var dataAvailable = false;
            Stopwatch sw = Stopwatch.StartNew();
            List<byte[]> nalsBuffer = new List<byte[]>();
            do
            {
                dataAvailable = nalReader.ReadNext(out var nal);
                if (nal != null && nal.Length > 0)
                {
                    var firstByte = nal[0];
                    var nalUnitType = firstByte & 0x1F;
                    nalsBuffer.Add(nal);

                    if (nalUnitType == (int)NalPacketType.Idr || nalUnitType == (int)NalPacketType.Slice)
                    {
                        IEnumerable<byte> data = new List<byte>();
                        var startCodes = new byte[] { 0, 0, 0, 1 };
                        foreach (var n in nalsBuffer)
                        {
                            data = data.Concat(startCodes).Concat(n);
                        }

                        nalsBuffer.Clear();
                        var bytes = data.ToArray();

                        using (var pSample = MediaFactory.CreateSample())
                        {
                            try
                            {
                                sampleTime = sampleInterval * samplesCount;
                                pSample.SampleTime = MfTool.SecToMfTicks(sampleTime);
                                pSample.SampleDuration = MfTool.SecToMfTicks(sampleInterval);

                                using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
                                {
                                    var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
                                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                                    mediaBuffer.CurrentLength = bytes.Length;
                                    mediaBuffer.Unlock();

                                    pSample.AddBuffer(mediaBuffer);
                                }
                                //Console.WriteLine("ProcessSample(...) " + (procCount++));
                                var res = decoder.ProcessSample(pSample, OnSampleDecoded);
                                if (res == DecodeResult.Error)
                                {
                                    Console.WriteLine("decoder.ProcessSample == false");
                                }

                                samplesCount++;

                                Thread.Sleep(30);

                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    }
                }

            } while (dataAvailable);

            stream.Dispose();

            decoder.Drain();

            
            var decodeResult =  DecodeResult.Ok;
            do
            {
                //Console.WriteLine("ProcessSample(...) " + (procCount++));
                decodeResult = decoder.ProcessSample(null, OnSampleDecoded);
            } while (decodeResult == DecodeResult.Ok);

            var totalMilliseconds = sw.ElapsedMilliseconds;
            Console.WriteLine("TotalMilliseconds=" + totalMilliseconds + " MSecPerFrame=" + (totalMilliseconds/ (double)decodedCount));


            decoder.Stop();
        }

        private Stopwatch stopwatch = new Stopwatch();
        private long monotonicTime = 0;
        private int decodedCount = 0;
        private void OnSampleDecoded(Sample s)
		{
            try
            {
                var driverType = decoder.DriverType;
                var sampleTime = s.SampleTime;
                var sampleDur = s.SampleDuration;
                monotonicTime += sampleDur;

                var msec = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();

                //Console.WriteLine("onSampleDecoded(...) " + decodedCount  + " " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(monotonicTime) + " " + msec);

                //Console.WriteLine(MfTool.LogSample(s));
                //Console.WriteLine("-------------------");

                if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
                {
                    ProcessD3D11Sample(s, monotonicTime);
                }
                else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
                {
                    ProcessD3D9Sample(s, monotonicTime);
                }
                else
                {
                    ProcessCPUSample(s, monotonicTime);
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

        private void ProcessD3D9Sample(Sample s, long sampleTime)
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
                    using (var sharedTex9 = new SharpDX.Direct3D9.Texture(deviceEx, width, height, 1,
                        Device3D9.Usage.RenderTarget, Device3D9.Format.A8R8G8B8, Device3D9.Pool.Default,
                        ref sharedHandle))
                    {  
                        using (var sharedSurf9 = sharedTex9.GetSurfaceLevel(0))
                        {
                            deviceEx.StretchRectangle(srcSurf, sharedSurf9, Device3D9.TextureFilter.Linear);
                        }

                        using (var texture2d = device3D11.OpenSharedResource<Texture2D>(sharedHandle))
                        { //Dx9->Dx11 работает только на primary адаптере т.е AdatperIndex = 0;
                            // на вторичном адаптере текстура DX11 получается пустая, хотя исходная Dx9 с данными... 
                            // может быть это ограничение DX!??


                            //device3D11.ImmediateContext.Flush();
                            //var texBytes =DxTool.DumpTexture(device3D11, texture2d);
                            //var _descr = texture2d.Description;

                            //var fileName = "D3D9_" +  _descr.Width + "x" + _descr.Height + "_" + _descr.Format + "_" + sampleTime + ".raw";
                            //var fullName = Path.Combine("decoded", fileName);

                            //File.WriteAllBytes(fullName, texBytes);
                        }
                    };
                }
            }
        }

        private void ProcessD3D11Sample(Sample s, long sampleTime)
        {
            using (var buffer = s.ConvertToContiguousBuffer())
            {
                using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                {
                    dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                    using (Texture2D texture = new Texture2D(intPtr))
                    {
                        //var _descr = texture.Description;
                        //var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture);

                        //var fileName = "D3D11_" + _descr.Width + "x" + _descr.Height + "_" + _descr.Format + "_" + sampleTime + ".raw";
                        //var fullName = Path.Combine("decoded", fileName);

                        //File.WriteAllBytes(fullName, texBytes);
                    }
                }
            }
        }

        private void ProcessCPUSample(Sample s, long sampleTime)
        {
            // Console.WriteLine("onSampleDecoded(...) "  + sampleTime);
            using (var b = s.ConvertToContiguousBuffer())
            {
                var _ptr = b.Lock(out var _maxLen, out var _currLen);
                byte[] buf = new byte[_currLen];

                //var mediaType = decoder.OutputMediaType;
                //var size = MfTool.GetFrameSize(mediaType);
                //var format = MfTool.GetMediaSubtypeName(mediaType);

                //var fileName = "CPU_" + size.Width + "x" + size.Height + "_" + format + "_" + sampleTime + ".raw";
                //var fullName = Path.Combine("decoded", fileName);

                //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, fullName);

                b.Unlock();
            }
        }

        public void Close()
        {
           
            DxTool.SafeDispose(deviceEx);
            DxTool.SafeDispose(device3D11);
            //DxTool.SafeDispose(SharedTexture);
            //DxTool.SafeDispose(destSurf);

            if (decoder != null)
            {
                decoder.Stop();
                decoder.Close();
                decoder = null;
            }

        }





        //private void OnSampleDecoded(Sample s)
        //{
        //    var driverType = decoder.DriverType;

        //    var sampleTime = s.SampleTime;
        //    var sampleDur = s.SampleDuration;
        //    sampleDurTime += sampleDur;
        //    var msec = stopwatch.ElapsedMilliseconds;
        //    stopwatch.Restart();

        //    var log = MfTool.LogMediaAttributes(s);
        //    Console.WriteLine(log);

        //    Console.WriteLine("onSampleDecoded(...) " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(sampleDurTime) + " " + msec);
        //    //{
        //    //    var b = s.ConvertToContiguousBuffer();
        //    //    var _ptr = b.Lock(out var _maxLen, out var _currLen);
        //    //    byte[] buf = new byte[_currLen];
        //    //    //Marshal.Copy(_ptr, buf, 0, buf.Length);


        //    //    MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, @"decoded\decoded_nv12_" + sampleDurTime + ".yuv");

        //    //    b.Unlock();

        //    //    b.Dispose();
        //    //}


        //    if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
        //    {
        //        using (var buffer = s.ConvertToContiguousBuffer())
        //        {
        //            using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
        //            {
        //                dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
        //                using (Texture2D texture = new Texture2D(intPtr))
        //                {
        //                    var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture);
        //                    File.WriteAllBytes(@"decoded\decoded_nv12_" + sampleTime + ".yuv", texBytes);
        //                }
        //            }
        //        }
        //    }
        //    else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
        //    {
        //        using (var buffer = s.ConvertToContiguousBuffer())
        //        {
        //            MediaFactory.GetService(buffer, MediaServiceKeys.Buffer, IID.D3D9Surface, out var pSurf);

        //            using (var srcSurf = new SharpDX.Direct3D9.Surface(pSurf))
        //            {
        //                var srcDescr = srcSurf.Description;
        //                int width = srcDescr.Width;
        //                int height = srcDescr.Height;

        //                IntPtr sharedHandle = IntPtr.Zero;
        //                using (var sharedTex9 = new SharpDX.Direct3D9.Texture(deviceEx, width, height, 1,
        //                    Device3D9.Usage.RenderTarget, Device3D9.Format.A8R8G8B8, Device3D9.Pool.Default,
        //                    ref sharedHandle))
        //                {
        //                    using (var sharedSurf9 = sharedTex9.GetSurfaceLevel(0))
        //                    {
        //                        deviceEx.StretchRectangle(srcSurf, sharedSurf9, Device3D9.TextureFilter.Linear);
        //                    }

        //                    using (var texture2d = device3D11.OpenSharedResource<Texture2D>(sharedHandle))
        //                    {
        //                        //device3D11.ImmediateContext.Flush();
        //                        var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture2d);
        //                        var _descr = texture2d.Description;
        //                        var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

        //                        File.WriteAllBytes(@"decoded\" + fileName, texBytes);
        //                    }
        //                };

        //                //var rgbSurf = Surface.CreateRenderTarget(device9, width, height, Format.A8R8G8B8, MultisampleType.None, 0, true);
        //                //device9.StretchRectangle(surface, rgbSurf, TextureFilter.None);

        //                //var _dataRect = rgbSurf.LockRectangle(LockFlags.ReadOnly);
        //                //var _ptr = _dataRect.DataPointer;
        //                //var _size = height * _dataRect.Pitch;

        //                //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _size, @"decoded\decoded_dx9_rgba.yuv");
        //                //rgbSurf.UnlockRectangle();

        //                //                IntPtr handle = IntPtr.Zero;
        //                //                using (var sharedSurf = Surface.CreateOffscreenPlainEx(device9, width, height, Format.A8R8G8B8, Pool.Default, Usage.None, ref handle))
        //                //                {

        //                //                    //var rect = sharedSurf.LockRectangle(LockFlags.None);
        //                //                    //var _ptr = rect.DataPointer;
        //                //                    //var _size = height * rect.Pitch;

        //                //                    //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _size, @"decoded\decoded_dx9_rgba.yuv");

        //                //                    //sharedSurf.UnlockRectangle();

        //                //                    //using (var surface2d = device3D11.OpenSharedResource<SharpDX.DXGI.Surface>(handle))
        //                //                    //{
        //                //                    //    var tex = surface2d.QueryInterface<Texture2D>();

        //                //                    //    device9.StretchRectangle(rgbSurf, sharedSurf, TextureFilter.None);

        //                //                    //    var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, tex);
        //                //                    //    var _descr = tex.Description;
        //                //                    //    var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

        //                //                    //    File.WriteAllBytes(@"decoded\" + fileName, texBytes);
        //                //                    //}

        //                //                    using (var texture2d = device3D11.OpenSharedResource<Texture2D>(handle))
        //                //                    {
        //                //                        device3D11.ImmediateContext.Flush();
        //                //using (var destSurf = texture3d9.GetSurfaceLevel(0))
        //                //{
        //                //	device9.StretchRectangle(surface, destSurf, TextureFilter.Linear);
        //                //}


        //                //var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture2d);
        //                //                        var _descr = texture2d.Description;
        //                //                        var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

        //                //                        File.WriteAllBytes(@"decoded\" + fileName, texBytes);
        //                //                    }
        //                //                }

        //                //using (var resource = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
        //                //{
        //                //	var handle = resource.SharedHandle;



        //                //	using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget,
        //                //		Format.A8R8G8B8, Pool.Default, ref handle))
        //                //	{
        //                //		//var dataRect = surface.LockRectangle(LockFlags.ReadOnly);
        //                //		//var ptr = dataRect.DataPointer;
        //                //		//var size = (height + height / 2) * dataRect.Pitch;

        //                //		//MediaToolkit.Utils.TestTools.WriteFile(ptr, size, @"decoded\decoded_dx9_nv12.yuv");
        //                //		//surface.UnlockRectangle();

        //                //		using (var sharedSurf = texture3d9.GetSurfaceLevel(0))
        //                //		{

        //                //			device9.StretchRectangle(rgbSurf, sharedSurf, TextureFilter.None);
        //                //		}

        //                //		var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);

        //                //		File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
        //                //	};

        //                //}



        //                ////..
        //                ////using (var destSurface = new SharpDX.Direct3D9.Surface(surfPtr))
        //                //{
        //                //	device9.StretchRectangle(surface, destSurf, TextureFilter.Linear);

        //                //	//using (var sharedTex = device.OpenSharedResource<Texture2D>(sharedD3D9Handle))
        //                //	//{
        //                //	//	device.ImmediateContext.Flush();
        //                //	//	var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, sharedTex);

        //                //	//	File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
        //                //	//}

        //                //	var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);

        //                //	File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
        //                //}


        //            }


        //        }
        //    }
        //    else
        //    {
        //        // Console.WriteLine("onSampleDecoded(...) "  + sampleTime);

        //        var b = s.ConvertToContiguousBuffer();
        //        var _ptr = b.Lock(out var _maxLen, out var _currLen);
        //        byte[] buf = new byte[_currLen];
        //        //Marshal.Copy(_ptr, buf, 0, buf.Length);


        //        MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, @"decoded\decoded_nv12_" + sampleDurTime + ".yuv");

        //        b.Unlock();

        //        b.Dispose();
        //    }

        //    s.Dispose();


        //}


        //private void _OnSampleDecoded(Sample s)
        //{
        //    var sampleTime = s.SampleTime;
        //    var sampleDur = s.SampleDuration;
        //    sampleDurTime += sampleDur;
        //    var msec = stopwatch.ElapsedMilliseconds;
        //    stopwatch.Restart();

        //    Console.WriteLine("onSampleDecoded(...) " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(sampleDurTime) + " " + msec);
        //    //var _res = processor.ProcessSample(s, out Sample rgbSample);
        //    var _res = true;
        //    var rgbSample = s;
        //    try
        //    {
        //        if (_res)
        //        {
        //            if (rgbSample != null)
        //            {

        //                var rgbBuffer = rgbSample.GetBufferByIndex(0);
        //                //var rgbBuffer = rgbSample.ConvertToContiguousBuffer();
        //                try
        //                {
        //                    using (var dxgiBuffer = rgbBuffer.QueryInterface<DXGIBuffer>())
        //                    {
        //                        dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
        //                        var index = dxgiBuffer.SubresourceIndex;

        //                        using (Texture2D rgbTexture = new Texture2D(intPtr))
        //                        {
        //                            var d = rgbTexture.Device;
        //                            // d.ImmediateContext.CopySubresourceRegion(rgbTexture, index, null, SharedTexture, 0);
        //                            var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(d, rgbTexture);
        //                            File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);

        //                            //device3D11.ImmediateContext.CopyResource(rgbTexture, SharedTexture);
        //                            //device3D11.ImmediateContext.Flush();
        //                        };
        //                        //var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);
        //                        //File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
        //                    }
        //                }
        //                finally
        //                {
        //                    rgbBuffer?.Dispose();
        //                    rgbBuffer = null;
        //                }

        //            }
        //        }
        //    }
        //    finally
        //    {
        //        rgbSample?.Dispose();
        //        rgbSample = null;
        //    }
        //}


        //public static void Run2()
        //{
        //	Console.WriteLine("RgbToNv12Converter::Run()");
        //	try
        //	{
        //		string fileName = @"Files\testsrc_320x240_yuv420p_Iframe.h264";

        //		var inputArgs = new MfVideoArgs
        //		{
        //			Width = 320,
        //			Height = 240,
        //			FrameRate = MfTool.PackToLong(30, 1),
        //		};

        //		var bytes = File.ReadAllBytes(fileName);

        //		MfH264Decoder decoder = new MfH264Decoder(null);

        //		decoder.Setup(inputArgs);

        //		decoder.Start();

        //		var pSample = MediaFactory.CreateSample();
        //		pSample.SampleTime = 0;
        //		pSample.SampleDuration = 0;

        //		using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
        //		{
        //			var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
        //			Marshal.Copy(bytes, 0, ptr, bytes.Length);
        //			mediaBuffer.Unlock();
        //			pSample.AddBuffer(mediaBuffer);
        //		}
        //		Action<Sample> onSampleDecoded = new Action<Sample>((s) =>
        //		   {

        //		   });

        //		var res = decoder.ProcessSample(pSample, onSampleDecoded);

        //		var direct3D = new Direct3DEx();

        //		var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

        //		var presentParams = new PresentParameters
        //		{
        //			//Windowed = true,
        //			//SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
        //			//DeviceWindowHandle = IntPtr.Zero,
        //			//PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
        //			//BackBufferCount = 1,

        //			Windowed = true,
        //			MultiSampleType = MultisampleType.None,
        //			SwapEffect = SwapEffect.Discard,
        //			PresentFlags = PresentFlags.None,
        //		};

        //		var flags = CreateFlags.HardwareVertexProcessing |
        //					CreateFlags.Multithreaded |
        //					CreateFlags.FpuPreserve;

        //		int adapterIndex = 0;

        //		var device = new DeviceEx(direct3D, adapterIndex, SharpDX.Direct3D9.DeviceType.Hardware, hWnd, flags, presentParams);




        //		using (var resource = texture11.QueryInterface<SharpDX.DXGI.Resource>())
        //		{
        //			var handle = resource.SharedHandle;
        //			//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
        //			//var format = (Format)((int)fourCC);
        //			//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
        //			var format = Format.A8R8G8B8;
        //			using (var texture3d9 = new SharpDX.Direct3D9.Texture(device,
        //				1920, 1080, 1, Usage.RenderTarget, format, Pool.Default,
        //				ref handle))
        //			{
        //				var surface = texture3d9.GetSurfaceLevel(0);
        //			};

        //			//var surface = Surface.CreateOffscreenPlain(device, 1920, 1080, format, Pool.Default, ref handle);
        //		}


        //		//Direct3DDeviceManager manager = new Direct3DDeviceManager();
        //		//manager.ResetDevice(device, manager.CreationToken);

        //		//MfH264Dxva2Decoder decoder = new MfH264Dxva2Decoder(manager);
        //		//var inputArgs = new MfVideoArgs
        //		//{
        //		//	Width = 1920,
        //		//	Height = 1080,
        //		//	FrameRate = MfTool.PackToLong(30, 1),
        //		//};

        //		//decoder.Setup(inputArgs);
        //		//decoder.Start();
        //	}
        //	catch (Exception ex)
        //	{
        //		Console.WriteLine(ex);
        //	}
        //}


        //private static Texture2D texture11 = null;
        //private SharpDX.Direct3D11.Device device = null;
        //public void Start(string fileName)
        //{
        //	SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

        //	var index = 0;

        //	var adapter = factory1.GetAdapter(index);

        //	Console.WriteLine("Adapter" + index + ": " + adapter.Description.Description);

        //	var _flags = DeviceCreationFlags.None;

        //	device = new SharpDX.Direct3D11.Device(adapter, _flags);
        //	using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
        //	{
        //		multiThread.SetMultithreadProtected(true);
        //	}

        //	var inputArgs = new MfVideoArgs
        //	{
        //		Width = 1920,
        //		Height = 1080,
        //		FrameRate = MfTool.PackToLong(30, 1),
        //	};


        //	texture11 = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
        //	{
        //		Width = inputArgs.Width,
        //		Height = inputArgs.Height,
        //		MipLevels = 1,
        //		ArraySize = 1,
        //		SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
        //		BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | BindFlags.ShaderResource,
        //		Usage = SharpDX.Direct3D11.ResourceUsage.Default,
        //		CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
        //		Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        //		//Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
        //		//Format = SharpDX.DXGI.Format.NV12,
        //		OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared,

        //	});

        //	MfH264Decoder decoder = new MfH264Decoder(null);

        //	decoder.Setup(inputArgs);
        //}



    }



}
