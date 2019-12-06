using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using MediaToolkit.UI;

using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using GDI = System.Drawing;

namespace WebCamTest
{
    class Program
    {

        public static void EnumerateCaptureSources()
        {

            Activate[] activates = null;
            using (var attributes = new MediaAttributes())
            {
                MediaFactory.CreateAttributes(attributes, 1);
                attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);
                activates = MediaFactory.EnumDeviceSources(attributes);
            }

            if (activates == null || activates.Length == 0)
            {
                Console.WriteLine("SourceTypeVideoCapture not found");
                return;
            }


            foreach (var _activate in activates)
            {
                Console.WriteLine("---------------------------------------------");
                var friendlyName = _activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                var isHwSource = _activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapHwSource);
                //var maxBuffers = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapMaxBuffers);
                var symbolicLink = _activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);


                Console.WriteLine("FriendlyName " + friendlyName + "\r\n" +
                "isHwSource " + isHwSource + "\r\n" +
                //"maxBuffers " + maxBuffers + 
                "symbolicLink " + symbolicLink);



                var mediaSource = _activate.ActivateObject<MediaSource>();

                var log = MfTool.LogMediaSource(mediaSource);

                Console.WriteLine(log);
  
                mediaSource?.Dispose();

                _activate?.Dispose();
            }


        }


        [STAThread]
        static void Main(string[] args)
        {

            Console.WriteLine("==============START=============");
            try
            {
                MediaManager.Startup();

                //EnumerateCaptureSources();
                //Console.ReadKey();
                //return;

                var device = GetVideoCaptureDevices();
                var d = device[1];
                VideoCaptureSource videoCaptureSource = new VideoCaptureSource();

                MfVideoCaptureParams captureParams = new MfVideoCaptureParams
                {
                    DestSize = new GDI.Size(1920, 1080),
                    DeviceId = d.SymLink,

                };

                videoCaptureSource.Setup(captureParams);


                PreviewForm previewForm = null;
                D3DImageProvider2 provider = null;
                var uiThread = new Thread(() =>
                {

                    provider = new D3DImageProvider2(Dispatcher.CurrentDispatcher);
                    previewForm = new PreviewForm();

                    previewForm.d3DImageControl1.DataContext = provider;

                    previewForm.Show();

                    provider.Start(videoCaptureSource.SharedTexture);

                    Application.Run();
                });

                uiThread.IsBackground = true;
                uiThread.SetApartmentState(ApartmentState.STA);


                Console.WriteLine("Any key to start...");
                Console.ReadKey();

                uiThread.Start();

                videoCaptureSource.BufferUpdated += () =>
                {
                    provider?.Update();
                };

                videoCaptureSource.Start();

                Console.ReadKey();
                Console.WriteLine("Any key to stop...");
                videoCaptureSource.Stop();
                videoCaptureSource.Close();

                Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());

                MediaManager.Shutdown();

                //previewForm?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("==============THE END=============");
            Console.WriteLine("Any key to quit...");
            Console.ReadKey();


        }

        public class VideoCaptureDevice
        {
            public string Name = "";
            public string SymLink = "";
        }

        public static List<VideoCaptureDevice> GetVideoCaptureDevices()
        {
            List<VideoCaptureDevice> devices = new List<VideoCaptureDevice>();

            Activate[] activates = null;
            try
            {
                using (var attributes = new MediaAttributes())
                {
                    MediaFactory.CreateAttributes(attributes, 1);
                    attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);

                    activates = MediaFactory.EnumDeviceSources(attributes);

                    foreach (var activate in activates)
                    {
                        var friendlyName = activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                        var isHwSource = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapHwSource);
                        //var maxBuffers = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapMaxBuffers);
                        var symbolicLink = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);

                        //var mediaTypes = activate.Get(TransformAttributeKeys.MftOutputTypesAttributes);

                        devices.Add(new VideoCaptureDevice { Name = friendlyName, SymLink = symbolicLink });

                        Console.WriteLine("FriendlyName " + friendlyName + "\r\n" +
                            "isHwSource " + isHwSource + "\r\n" +
                            //"maxBuffers " + maxBuffers + 
                            "symbolicLink " + symbolicLink);

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (activates != null)
                {
                    foreach (var act in activates)
                    {
                        act.Dispose();
                    }
                }
            }

            return devices;

        }


        //[STAThread]
        //static void __Main(string[] args)
        //{

        //    Console.WriteLine("==============START=============");
        //    try
        //    {
        //        MediaManager.Startup();

        //        Activate[] activates = null;
        //        using (var attributes = new MediaAttributes())
        //        {
        //            MediaFactory.CreateAttributes(attributes, 1);
        //            attributes.Set(CaptureDeviceAttributeKeys.SourceType, CaptureDeviceAttributeKeys.SourceTypeVideoCapture.Guid);

        //            activates = MediaFactory.EnumDeviceSources(attributes);

        //        }

        //        if (activates == null || activates.Length == 0)
        //        {
        //            Console.WriteLine("SourceTypeVideoCapture not found");
        //            Console.ReadKey();
        //        }

        //        foreach (var activate in activates)
        //        {
        //            Console.WriteLine("---------------------------------------------");
        //            var friendlyName = activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
        //            var isHwSource = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapHwSource);
        //            //var maxBuffers = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapMaxBuffers);
        //            var symbolicLink = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);

        //            Console.WriteLine("FriendlyName " + friendlyName + "\r\n" +
        //                "isHwSource " + isHwSource + "\r\n" +
        //                //"maxBuffers " + maxBuffers + 
        //                "symbolicLink " + symbolicLink);
        //        }


        //        var currentActivator = activates[0];

        //        var mediaSource = currentActivator.ActivateObject<MediaSource>();

        //        foreach (var a in activates)
        //        {
        //            a.Dispose();
        //        }

        //        //mediaSource.CreatePresentationDescriptor(out PresentationDescriptor presentationDescriptor);

        //        //for(int i= 0; i < presentationDescriptor.Count; i++)
        //        //{
        //        //    var obj = presentationDescriptor.GetByIndex(i, out Guid guid);
        //        //    Console.WriteLine(guid + " " + obj.ToString());

        //        //}

        //        //for (int i = 0; i < presentationDescriptor.StreamDescriptorCount; i++)
        //        //{

        //        //    var streamDescriptor = presentationDescriptor.GetStreamDescriptorByIndex(i, out SharpDX.Mathematics.Interop.RawBool selected);
        //        //    Console.WriteLine(i + " " + streamDescriptor.ToString() + " " + selected);
        //        //    for (int j= 0;j< streamDescriptor.Count; j++)
        //        //    {
        //        //        var obj = streamDescriptor.GetByIndex(j, out Guid guid);
        //        //        Console.WriteLine(guid + " " + obj.ToString());
        //        //    }

        //        //    Console.WriteLine("------------------------------------");

        //        //}




        //        SourceReader sourceReader = null;
        //        using (var mediaAttributes = new MediaAttributes(IntPtr.Zero))
        //        {
        //            MediaFactory.CreateAttributes(mediaAttributes, 2);
        //            mediaAttributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, 1);


        //            //var devMan = new DXGIDeviceManager();
        //            //devMan.ResetDevice(device);

        //            //mediaAttributes.Set(SourceReaderAttributeKeys.D3DManager, devMan);


        //            //MediaFactory.CreateSourceReaderFromMediaSource(mediaSource, mediaAttributes, sourceReader);

        //            sourceReader = new SourceReader(mediaSource, mediaAttributes);
        //        }

        //        LogMediaTypes(sourceReader);

        //        Console.WriteLine("------------------CurrentMediaType-------------------");
        //        var mediaType = sourceReader.GetCurrentMediaType(SourceReaderIndex.FirstVideoStream);
        //        Console.WriteLine(MfTool.LogMediaType(mediaType));

        //        var frameSize = MfTool.GetFrameSize(mediaType);
        //        var subtype = mediaType.Get(MediaTypeAttributeKeys.Subtype);


        //        Device device = null;
        //        int adapterIndex = 0;
        //        using (var dxgiFactory = new SharpDX.DXGI.Factory1())
        //        {
        //            var adapter = dxgiFactory.Adapters1[adapterIndex];

        //            device = new Device(adapter,
        //                                //DeviceCreationFlags.Debug |
        //                                DeviceCreationFlags.VideoSupport |
        //                                DeviceCreationFlags.BgraSupport);

        //            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
        //            {
        //                multiThread.SetMultithreadProtected(true);
        //            }
        //        }


        //        var processor = new MfVideoProcessor(null);
        //        var inProcArgs = new MfVideoArgs
        //        {
        //            Width = frameSize.Width,
        //            Height = frameSize.Height,
        //            // Format = VideoFormatGuids.Rgb24,
        //            Format = subtype,//VideoFormatGuids.NV12,
        //        };


        //        var outProcArgs = new MfVideoArgs
        //        {
        //            Width = frameSize.Width,
        //            Height = frameSize.Height,
        //            Format = VideoFormatGuids.Argb32,
        //            //Format = VideoFormatGuids.Rgb32,//VideoFormatGuids.Argb32,
        //        };

        //        processor.Setup(inProcArgs, outProcArgs);


        //        //processor.SetMirror(VideoProcessorMirror.MirrorHorizontal);
        //        processor.SetMirror(VideoProcessorMirror.MirrorVertical);



        //        var texture = new Texture2D(device,
        //            new Texture2DDescription
        //            {
        //                CpuAccessFlags = CpuAccessFlags.Read,
        //                BindFlags = BindFlags.None,
        //                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        //                Width = frameSize.Width,
        //                Height = frameSize.Height,
        //                MipLevels = 1,
        //                ArraySize = 1,
        //                SampleDescription = { Count = 1, Quality = 0 },
        //                Usage = ResourceUsage.Staging,
        //                OptionFlags = ResourceOptionFlags.None,
        //            });

        //        Texture2D SharedTexture = new Texture2D(device,
        //         new Texture2DDescription
        //         {

        //             CpuAccessFlags = CpuAccessFlags.None,
        //             BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        //             Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        //             Width = frameSize.Width,
        //             Height = frameSize.Height,

        //             MipLevels = 1,
        //             ArraySize = 1,
        //             SampleDescription = { Count = 1, Quality = 0 },
        //             Usage = ResourceUsage.Default,
        //             //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
        //             OptionFlags = ResourceOptionFlags.Shared,

        //         });

        //        mediaType?.Dispose();



        //        D3DImageProvider2 provider = null;
        //        var uiThread = new Thread(() =>
        //        {

        //            provider = new D3DImageProvider2(Dispatcher.CurrentDispatcher);
        //            PreviewForm previewForm = new PreviewForm();

        //            previewForm.d3DImageControl1.DataContext = provider;

        //            previewForm.Show();

        //            provider.Start(SharedTexture);

        //            Application.Run();
        //        });

        //        uiThread.SetApartmentState(ApartmentState.STA);


        //        Console.WriteLine("Any key to start...");
        //        Console.ReadKey();

        //        uiThread.Start();

        //        processor.Start();

        //        int sampleCount = 0;
        //        while (true)
        //        {
        //            int actualIndex = 0;
        //            SourceReaderFlags flags = SourceReaderFlags.None;
        //            long timestamp = 0;
        //            var sample = sourceReader.ReadSample(SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None, out actualIndex, out flags, out timestamp);

        //            try
        //            {
        //                //Console.WriteLine("#" + sampleCount + " Timestamp " + timestamp + " Flags " + flags);

        //                if (sample != null)
        //                {
        //                    //Console.WriteLine("SampleTime " + sample.SampleTime + " SampleDuration " + sample.SampleDuration + " SampleFlags " + sample.SampleFlags);
        //                    Sample outputSample = null;
        //                    try
        //                    {
        //                        var res = processor.ProcessSample(sample, out outputSample);

        //                        if (res)
        //                        {
        //                            //Console.WriteLine("outputSample!=null" + (outputSample != null));

        //                            var mediaBuffer = outputSample.ConvertToContiguousBuffer();
        //                            var ptr = mediaBuffer.Lock(out int cbMaxLengthRef, out int cbCurrentLengthRef);

        //                            var width = outProcArgs.Width;
        //                            var height = outProcArgs.Height;

        //                            var dataBox = device.ImmediateContext.MapSubresource(texture, 0, MapMode.Read, MapFlags.None);

        //                            Kernel32.CopyMemory(dataBox.DataPointer, ptr, (uint)cbCurrentLengthRef);

        //                            device.ImmediateContext.UnmapSubresource(texture, 0);


        //                            device.ImmediateContext.CopyResource(texture, SharedTexture);
        //                            device.ImmediateContext.Flush();
        //                            provider?.Update();

        //                            //GDI.Bitmap bmp = new GDI.Bitmap(width, height, GDI.Imaging.PixelFormat.Format32bppArgb);

        //                            //DxTool.TextureToBitmap(texture, bmp);

        //                            ////var bmpData = bmp.LockBits(new GDI.Rectangle(0, 0, width, height), GDI.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
        //                            ////uint size = (uint)(bmpData.Stride * height);
        //                            ////Kernel32.CopyMemory(bmpData.Scan0, ptr, size);
        //                            ////bmp.UnlockBits(bmpData);

        //                            ////var fileName = @"d:\BMP\" + "#" + sampleCount + "_" + timestamp + ".bmp";
        //                            ////bmp.Save(fileName, GDI.Imaging.ImageFormat.Bmp);

        //                            //bmp.Dispose();

        //                            mediaBuffer.Unlock();
        //                            mediaBuffer?.Dispose();

        //                        }

        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Console.WriteLine(ex);
        //                    }
        //                    finally
        //                    {
        //                        if (outputSample != null)
        //                        {
        //                            outputSample.Dispose();
        //                        }
        //                    }

        //                }

        //            }
        //            finally
        //            {
        //                sample?.Dispose();
        //            }

        //            if (sampleCount > 50000)
        //            {
        //                break;
        //            }

        //            sampleCount++;


        //        }

        //        mediaSource?.Shutdown();
        //        mediaSource?.Dispose();

        //        sourceReader?.Dispose();


        //        MediaManager.Shutdown();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    Console.WriteLine("==============THE END=============");
        //    Console.WriteLine("Any key to quit...");
        //    Console.ReadKey();


        //}

        //private static void LogMediaTypes(SourceReader sourceReader)
        //{
        //    int streamIndex = 0;
        //    while (true)
        //    {
        //        bool invalidStreamNumber = false;

        //        int _streamIndex = -1;

        //        for (int mediaIndex = 0; ; mediaIndex++)
        //        {
        //            try
        //            {
        //                var nativeMediaType = sourceReader.GetNativeMediaType(streamIndex, mediaIndex);

        //                if (_streamIndex != streamIndex)
        //                {
        //                    _streamIndex = streamIndex;
        //                    Console.WriteLine("====================== StreamIndex#" + streamIndex + "=====================");
        //                }

        //                Console.WriteLine(MfTool.LogMediaType(nativeMediaType));
        //                nativeMediaType?.Dispose();

        //            }
        //            catch (SharpDX.SharpDXException ex)
        //            {
        //                if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
        //                {
        //                    Console.WriteLine("");
        //                    break;
        //                }
        //                else if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidStreamNumber)
        //                {
        //                    invalidStreamNumber = true;
        //                    break;
        //                }
        //                else
        //                {
        //                    throw;
        //                }
        //            }
        //        }

        //        if (invalidStreamNumber)
        //        {
        //            break;
        //        }

        //        streamIndex++;
        //    }
        //}
    }
}
