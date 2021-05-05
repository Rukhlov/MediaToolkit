using MediaToolkit.Codecs;
using MediaToolkit.MediaFoundation;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
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
using Device3D11 = SharpDX.Direct3D11.Device;
using Device3D9 = SharpDX.Direct3D9.DeviceEx;

namespace Test.Probe
{
	class VideoDecoderTest
	{
		private static MediaToolkit.Core.VideoDriverType driverType = MediaToolkit.Core.VideoDriverType.D3D9;

		static DeviceEx device9 = null;
		static Device3D11 device3D11 = null;
		static Texture2D SharedTexture = null;
		static Surface destSurf = null;

        private static MfVideoProcessor processor = null;

        public static void Run()
		{
			Console.WriteLine("RgbToNv12Converter::Run()");
			try
			{
                //string fileName = @"Files\testsrc_320x240_yuv420p_1sec.h264";
                ////string fileName = @"Files\testsrc_320x240_yuv420p_Iframe.h264";
                //var width = 320;
                //var height = 240;

                string fileName = @"Files\testsrc_1280x720_yuv420p_Iframe.h264";
                var width = 1280;
                var height = 720;

                //string fileName = @"Files\IFrame_1920x1080_yuv420p.h264";
                //var width = 1920;
                //var height = 1080;


                //string fileName = @"Files\testsrc_2560x1440_yuv420p_Iframe.h264";
                //var width = 2560;
                //var height = 1440;

                //string fileName = @"Files\testsrc_3840x2160_yuv420p_Iframe.h264";
                //var width = 3840;
                //var height = 2160;

                var inputArgs = new MfVideoArgs
				{
					Width = width,
					Height = height,

					//Width = 320,
					//Height = 240,
					FrameRate = MfTool.PackToLong(60, 1),
				};

               


                //var bytes = File.ReadAllBytes(fileName);



                int adapterIndex = 0;
				

				//IntPtr sharedD3D9Handle = IntPtr.Zero;
				
				using (var dxgiFactory = new SharpDX.DXGI.Factory1())
				{
					using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
					{
						device3D11 = new Device3D11(adapter,
						  // DeviceCreationFlags.Debug |
						  // DeviceCreationFlags.VideoSupport |
						   DeviceCreationFlags.BgraSupport);

						using (var multiThread = device3D11.QueryInterface<SharpDX.Direct3D11.Multithread>())
						{
							multiThread.SetMultithreadProtected(true);
						}
					}
				}

				SharedTexture = new Texture2D(device3D11,
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

				if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
				{

					inputArgs.D3DPointer = device3D11.NativePointer;
				}
				else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
				{
					var direct3D = new Direct3DEx();
					var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

					var presentParams = new PresentParameters
					{
						BackBufferWidth = width,
						BackBufferHeight = height,
						DeviceWindowHandle = hWnd,
						BackBufferCount = 1,
						Windowed = true,
						MultiSampleType = MultisampleType.None,
						SwapEffect = SwapEffect.Discard,
						PresentFlags = PresentFlags.Video,

					};

					var flags = CreateFlags.HardwareVertexProcessing |
								CreateFlags.Multithreaded;

					var deviceType = SharpDX.Direct3D9.DeviceType.Hardware;
					var nv12FourCC = new FourCC("NV12");
					var sourceFormat = (Format)((int)nv12FourCC);
					var targetFormat = Format.A8R8G8B8;

					bool result = direct3D.CheckDeviceFormatConversion(adapterIndex, deviceType, sourceFormat, targetFormat);
					Console.WriteLine("CheckDeviceFormatConversion(...): " + sourceFormat + " " + targetFormat + " " + result);

					device9 = new DeviceEx(direct3D, adapterIndex, deviceType, hWnd, flags, presentParams);
                    var caps = device9.Capabilities;
                    var canStretchRectFromTextures = caps.DeviceCaps2.HasFlag(DeviceCaps2.CanStretchRectFromTextures);

                    //using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, ref sharedD3D9Handle))
                    //{
                    //	destSurf = texture3d9.GetSurfaceLevel(0);

                    //};

                    //using (var resource = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
                    //{
                    //	var handle = resource.SharedHandle;
                    //	using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget,
                    //		Format.A8R8G8B8, Pool.Default, ref handle))
                    //	{
                    //		destSurf = texture3d9.GetSurfaceLevel(0);

                    //	};
                    //}

                    //   using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default))
                    //{
                    //	destSurf = texture3d9.GetSurfaceLevel(0);

                    //};

                    inputArgs.D3DPointer = device9.NativePointer;

				}


				inputArgs.DriverType = driverType;

                MfH264DecoderTest decoder = new MfH264DecoderTest();
                decoder.Setup(inputArgs);
                var outputType = decoder.OutputMediaType;

                processor = new MfVideoProcessor(device3D11);
                inputArgs.Format = outputType.Get(MediaTypeAttributeKeys.Subtype);
                var outProcArgs = new MfVideoArgs
                {
                    Width = width,
                    Height = height,
                    Format = outputType.Get(MediaTypeAttributeKeys.Subtype),//VideoFormatGuids.Argb32,
				};

                //processor.Setup(inputArgs, outProcArgs);
                //processor.Start();


                decoder.Start();







				//var bytes = File.ReadAllBytes(fileName);
				//{
				//    var pSample = MediaFactory.CreateSample();
				//    pSample.SampleTime = 0;

				//    using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
				//    {
				//        var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
				//        Marshal.Copy(bytes, 0, ptr, bytes.Length);
				//        mediaBuffer.CurrentLength = bytes.Length;
				//        mediaBuffer.Unlock();

				//        pSample.AddBuffer(mediaBuffer);
				//    }

				//    var res = decoder.ProcessSample(pSample, onSampleDecoded);

				//    pSample.Dispose();
				//}

				var stream = new FileStream(fileName, FileMode.Open);
				var nalReader = new NalUnitReader(stream);
				var dataAvailable = false;
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
							var pSample = MediaFactory.CreateSample();
							pSample.SampleTime = 0;

							using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
							{
								var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
								Marshal.Copy(bytes, 0, ptr, bytes.Length);
								mediaBuffer.CurrentLength = bytes.Length;
								mediaBuffer.Unlock();

								pSample.AddBuffer(mediaBuffer);
							}

							var res = decoder.ProcessSample(pSample, OnSampleDecoded);

							pSample.Dispose();


						}

					}


				} while (dataAvailable);

				stream.Dispose();

				//            var nals = HandleH264AnnexbFrames(bytes);
				//foreach (var nal in nals)
				//{
				//	var buf = new byte[nal.Count];
				//	//buf.Concat(new byte[] { 0, 0, 1 });
				//	Array.Copy(nal.Array, nal.Offset, buf, 0, nal.Count);
				//	buf = new byte[] { 0, 0, 0, 1 }.Concat(buf).ToArray();

				//	var pSample = MediaFactory.CreateSample();
				//	pSample.SampleTime = 0;

				//	using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(buf.Length))
				//	{
				//		var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
				//		Marshal.Copy(bytes, 0, ptr, buf.Length);
				//		mediaBuffer.CurrentLength = buf.Length;
				//		mediaBuffer.Unlock();

				//		pSample.AddBuffer(mediaBuffer);
				//	}

				//	var res = decoder.ProcessSample(pSample, onSampleDecoded);

				//	pSample.Dispose();
				//}

				decoder.Drain();
				var _res = decoder.ProcessSample(null, OnSampleDecoded);


				decoder.Stop();

				//Stopwatch sw = new Stopwatch();
				//long time = 0;
				//long count = 0;
				//var res = false;

				//do
				//{
				//                var pSample = MediaFactory.CreateSample();
				//                pSample.SampleTime = 0;

				//                using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
				//                {
				//                    var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
				//                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
				//                    mediaBuffer.CurrentLength = bytes.Length;
				//                    mediaBuffer.Unlock();

				//                    pSample.AddBuffer(mediaBuffer);
				//                }


				//                var sec = (count * 16) /1000.0;
				//	//var sec = time / 1000.0;

				//	pSample.SampleTime = MfTool.SecToMfTicks(sec);
				//                //Console.WriteLine("pSample.SampleTime " + pSample.SampleTime + " "+ sec);
				//                res = decoder.ProcessSample(pSample, onSampleDecoded);
				//	//Thread.Sleep(16);
				//                time += sw.ElapsedMilliseconds;

				//                sw.Restart();
				//	count++;

				//                pSample.Dispose();




				//            } while (true);


			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		static Stopwatch stopwatch = new Stopwatch();
		static long sampleDurTime = 0;
        private static void _OnSampleDecoded(Sample s)
        {
            var sampleTime = s.SampleTime;
            var sampleDur = s.SampleDuration;
            sampleDurTime += sampleDur;
            var msec = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart(); 

            Console.WriteLine("onSampleDecoded(...) " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(sampleDurTime) + " " + msec);
            //var _res = processor.ProcessSample(s, out Sample rgbSample);
            var _res = true;
            var rgbSample = s;
            try
            {
                if (_res)
                {
                    if (rgbSample != null)
                    {

                        var rgbBuffer = rgbSample.GetBufferByIndex(0);
                        //var rgbBuffer = rgbSample.ConvertToContiguousBuffer();
                        try
                        {
                            using (var dxgiBuffer = rgbBuffer.QueryInterface<DXGIBuffer>())
                            {
                                dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                                var index = dxgiBuffer.SubresourceIndex;

                                using (Texture2D rgbTexture = new Texture2D(intPtr))
                                {
                                    var d = rgbTexture.Device;
                                   // d.ImmediateContext.CopySubresourceRegion(rgbTexture, index, null, SharedTexture, 0);
                                    var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(d, rgbTexture);
                                    File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);

                                    //device3D11.ImmediateContext.CopyResource(rgbTexture, SharedTexture);
                                    //device3D11.ImmediateContext.Flush();
                                };
                                //var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);
                                //File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
                            }
                        }
                        finally
                        {
                            rgbBuffer?.Dispose();
                            rgbBuffer = null;
                        }

                    }
                }
            }
            finally
            {
                rgbSample?.Dispose();
                rgbSample = null;
            }
        }



        private static void OnSampleDecoded(Sample s)
		{
            
            var sampleTime = s.SampleTime;
			var sampleDur = s.SampleDuration;
			sampleDurTime += sampleDur;
			var msec = stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();

            var log = MfTool.LogMediaAttributes(s);
            Console.WriteLine(log);

            Console.WriteLine("onSampleDecoded(...) " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(sampleDurTime) + " " + msec);
            //{
            //    var b = s.ConvertToContiguousBuffer();
            //    var _ptr = b.Lock(out var _maxLen, out var _currLen);
            //    byte[] buf = new byte[_currLen];
            //    //Marshal.Copy(_ptr, buf, 0, buf.Length);


            //    MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, @"decoded\decoded_nv12_" + sampleDurTime + ".yuv");

            //    b.Unlock();

            //    b.Dispose();
            //}


            if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
			{
				using (var buffer = s.ConvertToContiguousBuffer())
				{
                    using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                    {
                        dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                        using (Texture2D texture = new Texture2D(intPtr))
                        {
                            var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture);
                            File.WriteAllBytes(@"decoded\decoded_nv12_" + sampleTime + ".yuv", texBytes);
                        }
                    }
                }
			}
			else if (driverType == MediaToolkit.Core.VideoDriverType.D3D9)
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
                        using (var sharedTex9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, ref sharedHandle))
                        {
                            using (var sharedSurf9 = sharedTex9.GetSurfaceLevel(0))
                            {
                                device9.StretchRectangle(srcSurf, sharedSurf9, TextureFilter.Linear);
                            }

                            using (var texture2d = device3D11.OpenSharedResource<Texture2D>(sharedHandle))
                            {
                                //device3D11.ImmediateContext.Flush();
                                var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture2d);
                                var _descr = texture2d.Description;
                                var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

                                File.WriteAllBytes(@"decoded\" + fileName, texBytes);
                            }
                        };

                        //var rgbSurf = Surface.CreateRenderTarget(device9, width, height, Format.A8R8G8B8, MultisampleType.None, 0, true);
                        //device9.StretchRectangle(surface, rgbSurf, TextureFilter.None);

                        //var _dataRect = rgbSurf.LockRectangle(LockFlags.ReadOnly);
                        //var _ptr = _dataRect.DataPointer;
                        //var _size = height * _dataRect.Pitch;

                        //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _size, @"decoded\decoded_dx9_rgba.yuv");
                        //rgbSurf.UnlockRectangle();

                        //                IntPtr handle = IntPtr.Zero;
                        //                using (var sharedSurf = Surface.CreateOffscreenPlainEx(device9, width, height, Format.A8R8G8B8, Pool.Default, Usage.None, ref handle))
                        //                {

                        //                    //var rect = sharedSurf.LockRectangle(LockFlags.None);
                        //                    //var _ptr = rect.DataPointer;
                        //                    //var _size = height * rect.Pitch;

                        //                    //MediaToolkit.Utils.TestTools.WriteFile(_ptr, _size, @"decoded\decoded_dx9_rgba.yuv");

                        //                    //sharedSurf.UnlockRectangle();

                        //                    //using (var surface2d = device3D11.OpenSharedResource<SharpDX.DXGI.Surface>(handle))
                        //                    //{
                        //                    //    var tex = surface2d.QueryInterface<Texture2D>();

                        //                    //    device9.StretchRectangle(rgbSurf, sharedSurf, TextureFilter.None);

                        //                    //    var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, tex);
                        //                    //    var _descr = tex.Description;
                        //                    //    var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

                        //                    //    File.WriteAllBytes(@"decoded\" + fileName, texBytes);
                        //                    //}

                        //                    using (var texture2d = device3D11.OpenSharedResource<Texture2D>(handle))
                        //                    {
                        //                        device3D11.ImmediateContext.Flush();
                        //using (var destSurf = texture3d9.GetSurfaceLevel(0))
                        //{
                        //	device9.StretchRectangle(surface, destSurf, TextureFilter.Linear);
                        //}


                        //var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, texture2d);
                        //                        var _descr = texture2d.Description;
                        //                        var fileName = _descr.Format + "_" + _descr.Width + "x" + _descr.Height + "_" + sampleTime + ".yuv";

                        //                        File.WriteAllBytes(@"decoded\" + fileName, texBytes);
                        //                    }
                        //                }

                        //using (var resource = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
                        //{
                        //	var handle = resource.SharedHandle;



                        //	using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget,
                        //		Format.A8R8G8B8, Pool.Default, ref handle))
                        //	{
                        //		//var dataRect = surface.LockRectangle(LockFlags.ReadOnly);
                        //		//var ptr = dataRect.DataPointer;
                        //		//var size = (height + height / 2) * dataRect.Pitch;

                        //		//MediaToolkit.Utils.TestTools.WriteFile(ptr, size, @"decoded\decoded_dx9_nv12.yuv");
                        //		//surface.UnlockRectangle();

                        //		using (var sharedSurf = texture3d9.GetSurfaceLevel(0))
                        //		{

                        //			device9.StretchRectangle(rgbSurf, sharedSurf, TextureFilter.None);
                        //		}

                        //		var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);

                        //		File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
                        //	};

                        //}



                        ////..
                        ////using (var destSurface = new SharpDX.Direct3D9.Surface(surfPtr))
                        //{
                        //	device9.StretchRectangle(surface, destSurf, TextureFilter.Linear);

                        //	//using (var sharedTex = device.OpenSharedResource<Texture2D>(sharedD3D9Handle))
                        //	//{
                        //	//	device.ImmediateContext.Flush();
                        //	//	var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, sharedTex);

                        //	//	File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
                        //	//}

                        //	var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device3D11, SharedTexture);

                        //	File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
                        //}


                    }


				}
			}
			else
			{
				// Console.WriteLine("onSampleDecoded(...) "  + sampleTime);

				var b = s.ConvertToContiguousBuffer();
				var _ptr = b.Lock(out var _maxLen, out var _currLen);
				byte[] buf = new byte[_currLen];
				//Marshal.Copy(_ptr, buf, 0, buf.Length);


				MediaToolkit.Utils.TestTools.WriteFile(_ptr, _currLen, @"decoded\decoded_nv12_" + sampleDurTime + ".yuv");

				b.Unlock();

				b.Dispose();
			}

			s.Dispose();


		}


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
