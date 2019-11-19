using MediaToolkit.MediaFoundation;
using MediaToolkit.NativeAPIs;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GDI = System.Drawing;

namespace WebCamTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("==============START=============");
            try
            {
                MediaManager.Startup();

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
                    Console.ReadKey();
                }

                foreach (var activate in activates)
                {
                    Console.WriteLine("---------------------------------------------");
                    var friendlyName = activate.Get(CaptureDeviceAttributeKeys.FriendlyName);
                    var isHwSource = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapHwSource);
                    //var maxBuffers = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapMaxBuffers);
                    var symbolicLink = activate.Get(CaptureDeviceAttributeKeys.SourceTypeVidcapSymbolicLink);

                    Console.WriteLine("FriendlyName " + friendlyName + "\r\n" +
                        "isHwSource " + isHwSource + "\r\n" +
                        //"maxBuffers " + maxBuffers + 
                        "symbolicLink " + symbolicLink);
                }


                var currentActivator = activates[0];

                var mediaSource = currentActivator.ActivateObject<MediaSource>();

                foreach (var a in activates)
                {
                    a.Dispose();
                }

                //mediaSource.CreatePresentationDescriptor(out PresentationDescriptor presentationDescriptor);

                //for(int i= 0; i < presentationDescriptor.Count; i++)
                //{
                //    var obj = presentationDescriptor.GetByIndex(i, out Guid guid);
                //    Console.WriteLine(guid + " " + obj.ToString());

                //}

                //for (int i = 0; i < presentationDescriptor.StreamDescriptorCount; i++)
                //{

                //    var streamDescriptor = presentationDescriptor.GetStreamDescriptorByIndex(i, out SharpDX.Mathematics.Interop.RawBool selected);
                //    Console.WriteLine(i + " " + streamDescriptor.ToString() + " " + selected);
                //    for (int j= 0;j< streamDescriptor.Count; j++)
                //    {
                //        var obj = streamDescriptor.GetByIndex(j, out Guid guid);
                //        Console.WriteLine(guid + " " + obj.ToString());
                //    }

                //    Console.WriteLine("------------------------------------");

                //}




                SourceReader sourceReader = null;
                using (var mediaAttributes = new MediaAttributes(IntPtr.Zero))
                {
                    MediaFactory.CreateAttributes(mediaAttributes, 1);
                    mediaAttributes.Set(SourceReaderAttributeKeys.EnableVideoProcessing, 1);

                    //MediaFactory.CreateSourceReaderFromMediaSource(mediaSource, mediaAttributes, sourceReader);

                    sourceReader = new SourceReader(mediaSource, mediaAttributes);
                }

                Device device = null;
                int adapterIndex = 0;
                using (var dxgiFactory = new SharpDX.DXGI.Factory1())
                {
                    var adapter = dxgiFactory.Adapters1[adapterIndex];

                    device = new Device(adapter,
                                        //DeviceCreationFlags.Debug |
                                        DeviceCreationFlags.VideoSupport |
                                        DeviceCreationFlags.BgraSupport);

                    using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
                    {
                        multiThread.SetMultithreadProtected(true);
                    }
                }

                var processor = new MfVideoProcessor(null);
                var inProcArgs = new MfVideoArgs
                {
                    Width = 320,
                    Height = 240,
                    Format = VideoFormatGuids.Rgb24,
                };


                var outProcArgs = new MfVideoArgs
                {
                    Width = 320,
                    Height = 240,
                    Format = VideoFormatGuids.Argb32,
                };

                processor.Setup(inProcArgs, outProcArgs);


                int streamIndex = 0;
                while (true)
                {
                    bool invalidStreamNumber = false;

                    int _streamIndex = -1;

                    for (int mediaIndex = 0; ; mediaIndex++)
                    {
                        try
                        {
                            var nativeMediaType = sourceReader.GetNativeMediaType(streamIndex, mediaIndex);

                            if (_streamIndex != streamIndex)
                            {
                                _streamIndex = streamIndex;
                                Console.WriteLine("====================== StreamIndex#" + streamIndex + "=====================");
                            }

                            Console.WriteLine(MfTool.LogMediaType(nativeMediaType));
                            nativeMediaType?.Dispose();

                        }
                        catch (SharpDX.SharpDXException ex)
                        {
                            if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.NoMoreTypes)
                            {
                                Console.WriteLine("");
                                break;
                            }
                            else if (ex.ResultCode == SharpDX.MediaFoundation.ResultCode.InvalidStreamNumber)
                            {
                                invalidStreamNumber = true;
                                break;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    if (invalidStreamNumber)
                    {
                        break;
                    }

                    streamIndex++;
                }

                //sourceReader.SetStreamSelection(SourceReaderIndex.AnyStream, true);

                //
                //var nativeMediaType = sourceReader.GetNativeMediaType(streamIndex, 0);

                Console.WriteLine("------------------CurrentMediaType-------------------");
                var currentMediaType = sourceReader.GetCurrentMediaType(SourceReaderIndex.FirstVideoStream);
                Console.WriteLine(MfTool.LogMediaType(currentMediaType));
                currentMediaType?.Dispose();

                Console.WriteLine("Any key to start...");
                Console.ReadKey();


                processor.Start();

                int sampleCount = 0;
                while (true)
                {
                    int actualIndex = 0;
                    SourceReaderFlags flags = SourceReaderFlags.None;
                    long timestamp = 0;
                    var sample = sourceReader.ReadSample(SourceReaderIndex.FirstVideoStream, SourceReaderControlFlags.None, out actualIndex, out flags, out timestamp);

                    try
                    {
                        Console.WriteLine("#" + sampleCount + " Timestamp " + timestamp + " Flags " + flags);

                        if (sample != null)
                        {
                            Console.WriteLine("SampleTime " + sample.SampleTime + " SampleDuration " + sample.SampleDuration + " SampleFlags " + sample.SampleFlags);
                            Sample outputSample = null;
                            try
                            {
                               //var res = false;
                                var res = processor.ProcessSample(sample, out outputSample);

                                if (res)
                                {
                                    Console.WriteLine("outputSample!=null" + (outputSample != null));

                                    var mediaBuffer = outputSample.ConvertToContiguousBuffer();
                                    var ptr = mediaBuffer.Lock(out int cbMaxLengthRef, out int cbCurrentLengthRef);
                                    var width = 320;
                                    var height = 240;

                                    GDI.Bitmap bmp = new GDI.Bitmap(width, height, GDI.Imaging.PixelFormat.Format32bppArgb);
                             
                                    var bmpData = bmp.LockBits(new GDI.Rectangle(0, 0, width, height), GDI.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                                    uint size = (uint)(bmpData.Stride * height);
                                    Kernel32.CopyMemory(bmpData.Scan0, ptr, size);
                                    bmp.UnlockBits(bmpData);

                                    mediaBuffer.Unlock();
                                    mediaBuffer?.Dispose();

                                    var fileName = @"d:\BMP\" + "#" + sampleCount + "_" + timestamp + ".bmp";

                                    bmp.Save(fileName, GDI.Imaging.ImageFormat.Bmp);

                                    bmp.Dispose();

                                }

                                outputSample?.Dispose();
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                            finally
                            {
                                if (outputSample != null)
                                {
                                    outputSample.Dispose();
                                }
                            }

                        }

                    }
                    finally
                    {
                        sample?.Dispose();
                    }

                    if (sampleCount > 5)
                    {
                        break;
                    }

                    sampleCount++;


                }

                mediaSource?.Shutdown();
                mediaSource?.Dispose();

                sourceReader?.Dispose();


                MediaManager.Shutdown();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("==============THE END=============");
            Console.WriteLine("Any key to quit...");
            Console.ReadKey();


        }

    }
}
