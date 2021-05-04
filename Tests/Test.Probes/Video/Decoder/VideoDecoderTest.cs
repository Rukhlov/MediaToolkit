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
		public static void Run()
		{
			Console.WriteLine("RgbToNv12Converter::Run()");
			try
			{
                string fileName = @"Files\testsrc_320x240_yuv420p_1sec.h264";
                //string fileName = @"Files\testsrc_320x240_yuv420p_Iframe.h264";
                var width = 320;
                var height = 240;

                //string fileName = @"Files\testsrc_1280x720_yuv420p_Iframe.h264";
                //var width = 1280;
                //var height = 720;

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

				Surface destSurf = null;
				DeviceEx device9 = null;

				int adapterIndex = 0;
                var driverType = MediaToolkit.Core.VideoDriverType.CPU;

				IntPtr sharedD3D9Handle = IntPtr.Zero;
				Device3D11 device = null;
				using (var dxgiFactory = new SharpDX.DXGI.Factory1())
				{
					using (var adapter = dxgiFactory.GetAdapter1(adapterIndex))
					{
						device = new Device3D11(adapter,
						   //DeviceCreationFlags.Debug |
						   //DeviceCreationFlags.VideoSupport |
						   DeviceCreationFlags.BgraSupport);

						using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
						{
							multiThread.SetMultithreadProtected(true);
						}
					}
				}

				var SharedTexture = new Texture2D(device,
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

                    inputArgs.D3DPointer = device.NativePointer;
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
					

					//using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default, ref sharedD3D9Handle))
					//{
					//	destSurf = texture3d9.GetSurfaceLevel(0);

					//};

					using (var resource = SharedTexture.QueryInterface<SharpDX.DXGI.Resource>())
					{

						var handle = resource.SharedHandle;
						using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget,
							Format.A8R8G8B8, Pool.Default, ref handle))
						{
							destSurf = texture3d9.GetSurfaceLevel(0);

						};

					}

					//   using (var texture3d9 = new SharpDX.Direct3D9.Texture(device9, width, height, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default))
					//{
					//	destSurf = texture3d9.GetSurfaceLevel(0);

					//};

					inputArgs.D3DPointer = device9.NativePointer;

                }
				

				inputArgs.DriverType = driverType;



                MfH264DecoderTest decoder = new MfH264DecoderTest();

				decoder.Setup(inputArgs);

				decoder.Start();



				Stopwatch stopwatch = new Stopwatch();
                long sampleDurTime = 0;
				Action<Sample> onSampleDecoded = new Action<Sample>((s) =>
				{
                    var sampleTime = s.SampleTime;
                    var sampleDur = s.SampleDuration;
                    sampleDurTime += sampleDur;
					var msec = stopwatch.ElapsedMilliseconds;
					stopwatch.Restart();
					Console.WriteLine("onSampleDecoded(...) " + MfTool.MfTicksToSec(sampleTime) + " " + MfTool.MfTicksToSec(sampleDurTime) + " " + msec);

                    if (driverType == MediaToolkit.Core.VideoDriverType.D3D11)
                    {
                        using (var buffer = s.ConvertToContiguousBuffer())
                        {
                            using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                            {
                                dxgiBuffer.GetResource(IID.D3D11Texture2D, out IntPtr intPtr);
                                using (Texture2D texture = new Texture2D(intPtr))
                                {

                                   var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, texture);
                                    File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
                                }
                            }
                        }
                    }
                    else if(driverType == MediaToolkit.Core.VideoDriverType.D3D9)
                    {
                        using (var buffer = s.ConvertToContiguousBuffer())
                        {
                            MediaFactory.GetService(buffer, MediaServiceKeys.Buffer, IID.D3D9Surface, out var pSurf);

                            using (var surface = new SharpDX.Direct3D9.Surface(pSurf))
                            {
								//..
								//using (var destSurface = new SharpDX.Direct3D9.Surface(surfPtr))
								{
									device9.StretchRectangle(surface, destSurf, TextureFilter.Linear);

									//using (var sharedTex = device.OpenSharedResource<Texture2D>(sharedD3D9Handle))
									//{
									//	device.ImmediateContext.Flush();
									//	var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, sharedTex);

									//	File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
									//}

									var texBytes = MediaToolkit.DirectX.DxTool.DumpTexture(device, SharedTexture);

									File.WriteAllBytes(@"decoded\decoded_rgba_" + sampleTime + ".yuv", texBytes);
								}


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

                });

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
                    if(nal!=null && nal.Length > 0)
                    {
                        var firstByte = nal[0];
                        var nalUnitType = firstByte & 0x1F;
                        nalsBuffer.Add(nal);

                        if (nalUnitType == (int)NalPacketType.Idr || nalUnitType == (int)NalPacketType.Slice)
                        {
                            IEnumerable<byte> data = new List<byte>();
                            var startCodes = new byte[] { 0, 0, 0, 1 };
                            foreach(var n in nalsBuffer)
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

                            var res = decoder.ProcessSample(pSample, onSampleDecoded);

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
				var _res = decoder.ProcessSample(null, onSampleDecoded);


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

		public static void Run2()
		{
			Console.WriteLine("RgbToNv12Converter::Run()");
			try
			{
				string fileName = @"Files\testsrc_320x240_yuv420p_Iframe.h264";

				var inputArgs = new MfVideoArgs
				{
					Width = 320,
					Height = 240,
					FrameRate = MfTool.PackToLong(30, 1),
				};

				var bytes = File.ReadAllBytes(fileName);

				MfH264Decoder decoder = new MfH264Decoder(null);

				decoder.Setup(inputArgs);

				decoder.Start();

				var pSample = MediaFactory.CreateSample();
				pSample.SampleTime = 0;
				pSample.SampleDuration = 0;
			
				using (var mediaBuffer = MediaFactory.CreateMemoryBuffer(bytes.Length))
				{
					var ptr = mediaBuffer.Lock(out var maxLen, out var currLen);
					Marshal.Copy(bytes, 0, ptr, bytes.Length);
					mediaBuffer.Unlock();
					pSample.AddBuffer(mediaBuffer);
				}
				Action<Sample> onSampleDecoded = new Action<Sample>((s) =>
				   {

				   });

				var res = decoder.ProcessSample(pSample, onSampleDecoded);

				var direct3D = new Direct3DEx();

				var hWnd = MediaToolkit.NativeAPIs.User32.GetDesktopWindow();

				var presentParams = new PresentParameters
				{
					//Windowed = true,
					//SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
					//DeviceWindowHandle = IntPtr.Zero,
					//PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
					//BackBufferCount = 1,

					Windowed = true,
					MultiSampleType = MultisampleType.None,
					SwapEffect = SwapEffect.Discard,
					PresentFlags = PresentFlags.None,
				};

				var flags = CreateFlags.HardwareVertexProcessing |
							CreateFlags.Multithreaded |
							CreateFlags.FpuPreserve;

				int adapterIndex = 0;

				var device = new DeviceEx(direct3D, adapterIndex, SharpDX.Direct3D9.DeviceType.Hardware, hWnd, flags, presentParams);




				using (var resource = texture11.QueryInterface<SharpDX.DXGI.Resource>())
				{
					var handle = resource.SharedHandle;
					//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
					//var format = (Format)((int)fourCC);
					//var fourCC = new SharpDX.Multimedia.FourCC("NV12");
					var format = Format.A8R8G8B8;
					using (var texture3d9 = new SharpDX.Direct3D9.Texture(device,
						1920, 1080, 1, Usage.RenderTarget, format, Pool.Default,
						ref handle))
					{
						var surface = texture3d9.GetSurfaceLevel(0);
					};

					//var surface = Surface.CreateOffscreenPlain(device, 1920, 1080, format, Pool.Default, ref handle);
				}


					//Direct3DDeviceManager manager = new Direct3DDeviceManager();
					//manager.ResetDevice(device, manager.CreationToken);

					//MfH264Dxva2Decoder decoder = new MfH264Dxva2Decoder(manager);
					//var inputArgs = new MfVideoArgs
					//{
					//	Width = 1920,
					//	Height = 1080,
					//	FrameRate = MfTool.PackToLong(30, 1),
					//};

					//decoder.Setup(inputArgs);
					//decoder.Start();
				}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		private static Texture2D texture11 = null;
		private SharpDX.Direct3D11.Device device = null;
		public void Start(string fileName)
		{
			SharpDX.DXGI.Factory1 factory1 = new SharpDX.DXGI.Factory1();

			var index = 0;

			var adapter = factory1.GetAdapter(index);

			Console.WriteLine("Adapter" + index + ": " + adapter.Description.Description);

			var _flags = DeviceCreationFlags.None;

			device = new SharpDX.Direct3D11.Device(adapter, _flags);
			using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
			{
				multiThread.SetMultithreadProtected(true);
			}

			var inputArgs = new MfVideoArgs
			{
				Width = 1920,
				Height = 1080,
				FrameRate = MfTool.PackToLong(30, 1),
			};


			texture11 = new Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
			{
				Width = inputArgs.Width,
				Height = inputArgs.Height,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
				BindFlags = SharpDX.Direct3D11.BindFlags.RenderTarget | BindFlags.ShaderResource,
				Usage = SharpDX.Direct3D11.ResourceUsage.Default,
				CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
				Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
				//Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
				//Format = SharpDX.DXGI.Format.NV12,
				OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.Shared,

			});

			MfH264Decoder decoder = new MfH264Decoder(null);

			decoder.Setup(inputArgs);
		}

        private static List<ArraySegment<byte>> HandleH264AnnexbFrames(byte[] frame)
        {// получаем буфер который нужно порезать на NALUnit-ы

            List<ArraySegment<byte>> nalUnits = new List<ArraySegment<byte>>();

            int offset = 0;
            int pos1 = -1;
            int pos2 = -1;

            while (offset < frame.Length - 3)
            {
                if ((frame[offset] == 0 && frame[offset + 1] == 0 && frame[offset + 2] == 1))
                {
                    if (pos1 > 0)
                    {
                        pos2 = offset;
                        if (offset > 0)
                        {
                            if (frame[offset - 1] == 0)
                            {
                                pos2--;
                                //offset--;
                            }
                        }
                        int nalSize = pos2 - pos1;
                        nalUnits.Add(new ArraySegment<byte>(frame, pos1, nalSize));
                        pos2 = -1;
                    }

                    offset += 3;
                    pos1 = offset;
                    continue;
                }
                else
                {
                    //offset += 3;
                    offset++;
                }
            }

            if (pos1 > 0 && pos2 == -1)
            {
                pos2 = frame.Length;
                int nalSize = pos2 - pos1;

                nalUnits.Add(new ArraySegment<byte>(frame, pos1, nalSize));
            }

            //logger.Debug("nalUnits.Count " + nalUnits.Count);
            return nalUnits;
        }


    }



}
