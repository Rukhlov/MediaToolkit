using MediaToolkit.Common;
using FFmpegLib;
using NLog;
using MediaToolkit.MediaFoundation;
using MediaToolkit.RTP;
using MediaToolkit.Utils;

using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{

    public class VideoMulticastStreamer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ScreenSource screenSource = null;
        public VideoMulticastStreamer(ScreenSource source)
        {
            this.screenSource = source;
           
        }


        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private RtpSession h264Session = null;

        private RtpStreamer rtpStreamer = null;
        private StreamStats streamStats = null;
        //private FFmpegVideoEncoder encoder = null;
        private MfEncoderAsync mfEncoder = null;

        private MfVideoProcessor processor = null;
       
        public void Setup(VideoEncodingParams encodingParams, NetworkStreamingParams networkParams)
        {
            logger.Debug("ScreenStreamer::Setup()");

            try
            {
                h264Session = new H264Session();

                rtpStreamer = new RtpStreamer(h264Session);
                rtpStreamer.Open(networkParams.Address, networkParams.Port);
                var hwContext = screenSource.hwContext;

                //processor = new MfVideoProcessor(hwContext.device);
                //var inProcArgs = new MfVideoArgs
                //{
                //    Width = screenSource.Buffer.bitmap.Width,
                //    Height = screenSource.Buffer.bitmap.Height,
                //    Format = SharpDX.MediaFoundation.VideoFormatGuids.Argb32,
                //};


                //var outProcArgs = new MfVideoArgs
                //{
                //    Width = screenSource.Buffer.bitmap.Width,
                //    Height = screenSource.Buffer.bitmap.Height,
                //    Format = SharpDX.MediaFoundation.VideoFormatGuids.NV12,//.Argb32,
                //};

                //SharedTexture = new Texture2D(hwContext.device,
                //     new Texture2DDescription
                //     {

                //         CpuAccessFlags = CpuAccessFlags.None,
                //         BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                //         Format = SharpDX.DXGI.Format.NV12,
                //         //Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                //         Width = screenSource.Buffer.bitmap.Width,
                //         Height = screenSource.Buffer.bitmap.Height,

                //         MipLevels = 1,
                //         ArraySize = 1,
                //         SampleDescription = { Count = 1, Quality = 0 },
                //         Usage = ResourceUsage.Default,
                //         //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                //         OptionFlags = ResourceOptionFlags.Shared,

                //     });

                //processor.Setup(inProcArgs, outProcArgs);
                //processor.Start();



                var hwDevice = screenSource.hwContext.device;

                long adapterLuid = -1;
                using (var dxgiDevice = hwDevice.QueryInterface<SharpDX.DXGI.Device>())
                {
                    var adapter = dxgiDevice.Adapter;
                    adapterLuid = adapter.Description.Luid;

                }

                mfEncoder = new MfEncoderAsync();
                mfEncoder.Setup(new MfVideoArgs
                {
                    Width = screenSource.Buffer.bitmap.Width,
                    Height = screenSource.Buffer.bitmap.Height,
                    FrameRate = encodingParams.FrameRate,
                    AdapterId = adapterLuid,

                });
             

                mfEncoder.DataReady += MfEncoder_DataReady;

                

                //encoder = new FFmpegVideoEncoder();
                //encoder.Open(encodingParams);
                //encoder.DataEncoded += Encoder_DataEncoded;

                screenSource.BufferUpdated += ScreenSource_BufferUpdated;
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                CleanUp();

                throw;
            }

        }

        private Texture2D SharedTexture = null;

        private Stopwatch sw = new Stopwatch();
        public Task Start()
        {
            logger.Debug("ScreenStreamer::Start()");
            
            return Task.Run(() =>
            {
                logger.Info("Streaming thread started...");


                try
                {
                    streamStats = new StreamStats();

                    Statistic.RegisterCounter(streamStats);

                    var hwContext = screenSource.hwContext;
                    mfEncoder.Start();

                    while (!closing)
                    {
                        try
                        {
                            if (!syncEvent.WaitOne(1000))
                            {
                                continue;
                            }

                            if (closing)
                            {
                                break;
                            }

                            sw.Restart();

                            //Sample inputSample = null;
                            //try
                            //{
                            //    MediaBuffer mediaBuffer = null;
                            //    try
                            //    {
                            //        var texture = hwContext.SharedTexture;
                            //        MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID, texture, 0, false, out mediaBuffer);
                            //        inputSample = MediaFactory.CreateSample();
                            //        inputSample.AddBuffer(mediaBuffer);

                            //        inputSample.SampleTime = 0;
                            //        inputSample.SampleDuration = 0;
                            //    }
                            //    finally
                            //    {
                            //        mediaBuffer?.Dispose();
                            //    }

                            //    Sample nv12Sample = null;
                            //    try
                            //    {
                            //        bool result = processor.ProcessSample(inputSample, out nv12Sample);
                            //        if (result)
                            //        {
                            //            using (var buffer = nv12Sample.ConvertToContiguousBuffer())
                            //            {
                            //                using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                            //                {
                            //                    var uuid = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));
                            //                    dxgiBuffer.GetResource(uuid, out IntPtr intPtr);
                            //                    using (Texture2D nv12Texture = new Texture2D(intPtr))
                            //                    {
                                                    
                            //                        processor.device.ImmediateContext.CopyResource(nv12Texture, SharedTexture);
                            //                        processor.device.ImmediateContext.Flush();


                            //                        mfEncoder.WriteTexture(SharedTexture);
                            //                    };
                            //                }
                            //            }
    
                            //        }
                            //    }
                            //    finally
                            //    {
                            //        nv12Sample?.Dispose();
                            //    }
                            //}
                            //finally
                            //{
                            //    inputSample?.Dispose();
                            //}

                            mfEncoder.WriteTexture(hwContext.SharedTexture);

                            //var buffer = screenSource.Buffer;

                            //encoder.Encode(buffer);

                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex);
                            Thread.Sleep(1000);
                        }

                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);

                }
                finally
                {
                    CleanUp();

                    Statistic.UnregisterCounter(streamStats);
                }

                logger.Info("Streaming thread ended...");
            });

        }

       // FileStream file = new FileStream(@"d:\test_enc3.h264", FileMode.Create);
        private void MfEncoder_DataReady(byte[] buf)
        {
            //throw new NotImplementedException();
            var time = MediaTimer.GetRelativeTime();

           // var memo = new MemoryStream(buf);
           // memo.CopyTo(file);

            rtpStreamer.Send(buf, time);
            var processingTime = sw.ElapsedMilliseconds;
            //logger.Debug(processingTime);


            //var ts = hwContext.sw.ElapsedMilliseconds;
            //Console.WriteLine("ElapsedMilliseconds " + ts);
            streamStats.Update(time, buf.Length, processingTime);
        }

        //private void Encoder_DataEncoded(IntPtr ptr, int len, double time)
        //{
        //    if (closing)
        //    {
        //        return;
        //    }

        //    if (ptr != IntPtr.Zero && len > 0)
        //    {
        //        // получили данные от энкодера 
        //        byte[] frame = new byte[len];
        //        Marshal.Copy(ptr, frame, 0, len);

        //        rtpStreamer.Send(frame, time);

        //        streamStats.Update(time, frame.Length);
        //        // streamer.Send(frame, rtpTimestamp);
        //    }

        //}

        private void ScreenSource_BufferUpdated()
        {
            syncEvent.Set();
        }

        private bool closing = false;

        public void Close()
        {
            logger.Debug("VideoMulticastStreamer::Close()");
            closing = true;
            syncEvent.Set();
        }

        private void CleanUp()
        {
            logger.Debug("VideoMulticastStreamer::CleanUp()");
            if (mfEncoder != null)
            {
                mfEncoder.DataReady -= MfEncoder_DataReady;
                mfEncoder.Stop();
                //mfEncoder.Close();
            }

            if (processor != null)
            {
                processor.Close();
                processor = null;
            }

            //if (encoder != null)
            //{
            //    encoder.DataEncoded -= Encoder_DataEncoded;
            //    encoder.Close();
            //}

            screenSource.BufferUpdated -= ScreenSource_BufferUpdated;

            rtpStreamer?.Close();
        }

        class StreamStats : StatCounter
        {
            public uint buffersCount = 0;

            public long totalBytesSend = 0;
            public double sendBytesPerSec1 = 0;
            public double sendBytesPerSec2 = 0;
            public double sendBytesPerSec3 = 0;
            public double lastTimestamp = 0;

            public double totalTime = 0;
            public long avgEncodingTime = 0;

            public StreamStats() { }
            
            public void Update(double timestamp, int bytesSend, long encTime)
            {
                if (lastTimestamp > 0)
                {
                    var time = timestamp - lastTimestamp;
                    if (time > 0)
                    {
                        var bytesPerSec = bytesSend / time;

                        sendBytesPerSec1 = (bytesPerSec * 0.05 + sendBytesPerSec1 * (1 - 0.05));

                        sendBytesPerSec2 = (sendBytesPerSec1 * 0.05 + sendBytesPerSec2 * (1 - 0.05));

                        sendBytesPerSec3 = (sendBytesPerSec2 * 0.05 + sendBytesPerSec3 * (1 - 0.05));

                        totalTime += (timestamp - lastTimestamp);
                    }
                }

                avgEncodingTime = (long)(encTime * 0.1 + avgEncodingTime * (1 - 0.1));

                totalBytesSend += bytesSend;

                lastTimestamp = timestamp;
                buffersCount++;
            }

            public override string GetReport()
            {
                StringBuilder sb = new StringBuilder();

                //var mbytesPerSec = sendBytesPerSec / (1024.0 * 1024);
                //var mbytes = totalBytesSend / (1024.0 * 1024);


                TimeSpan time = TimeSpan.FromSeconds(totalTime);
                sb.AppendLine(time.ToString(@"hh\:mm\:ss\.fff"));

                sb.AppendLine(buffersCount + " Buffers");

                //sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec1) + "/s");
                //sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec2) + "/s");
                sb.AppendLine(StringHelper.SizeSuffix((long)sendBytesPerSec3) + "/s");
                sb.AppendLine(StringHelper.SizeSuffix(totalBytesSend));

                sb.AppendLine(avgEncodingTime + " ms");

                //sb.AppendLine(mbytes.ToString("0.0") + " MBytes");
                //sb.AppendLine(mbytesPerSec.ToString("0.000") + " MByte/s");

                return sb.ToString();
            }

            public override void Reset()
            {
                buffersCount = 0;

                totalBytesSend = 0;
                sendBytesPerSec1 = 0;

                lastTimestamp = 0;
            }
        }
    }

}
