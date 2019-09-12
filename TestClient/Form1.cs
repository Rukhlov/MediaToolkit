
using NLog;
using MediaToolkit;
using MediaToolkit.MediaFoundation;
using MediaToolkit.RTP;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using MediaToolkit.UI;

using Vlc.DotNet.Core;

namespace TestClient
{
    public partial class Form1 : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        public static readonly System.IO.DirectoryInfo VlcLibDirectory = new System.IO.DirectoryInfo(System.IO.Path.Combine(CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

        public Form1()
        {
            InitializeComponent();

            var args = new string[]
            {
             // "--extraintf=logger",
             //"--verbose=0",
               //"--network-caching=300"
             };

            logger.Debug("VlcLibDirectory: " + VlcLibDirectory);

            mediaPlayer = new VlcMediaPlayer(VlcLibDirectory, args);

            mediaPlayer.VideoHostControlHandle = this.panel1.Handle;


            SharpDX.MediaFoundation.MediaManager.Startup();

        }

        private VlcMediaPlayer mediaPlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
            

            //OpenFileDialog dlg = new OpenFileDialog
            //{
            //    InitialDirectory = CurrentDirectory,
            //};

            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    var file = dlg.FileName;              
            //    var opts = new string[]
            //    {
            //        // "--extraintf=logger",
            //        //"--verbose=0",
            //        "--network-caching=100",
            //    };

            //    mediaPlayer?.Play(new FileInfo(sdpFileName), opts);
            //}


            var file = Path.Combine(CurrentDirectory, "tmp.sdp");
            string[] sdp = 
            {
                "v=0",
                "s=SCREEN_STREAM",
                "c=IN IP4 239.0.0.1",
                "m=video 1234 RTP/AVP 96",
                "b=AS:2000",
                "a=rtpmap:96 H264/90000",
                "a=fmtp:96 packetization-mode=1",
                //"a=fmtp:96 packetization-mode=1 sprop-parameter-set=Z2QMH6yyAKALdCAAAAMAIAAAB5HjBkkAAAAB,aOvDyyLA",

                //"m=audio 1236 RTP/AVP 0",
                //"a=rtpmap:0 PCMU/8000",

            };


            File.WriteAllLines(file, sdp);

            var opts = new string[]
            {
                    // "--extraintf=logger",
                    //"--verbose=0",
                    //"--network-caching=100", //не работает
            };

            mediaPlayer?.Play(new FileInfo(file), opts);


        }
        //FileStream file = new FileStream("d:\\test_rtp.h264", FileMode.Create);

        private void button2_Click(object sender, EventArgs e)
        {
            mediaPlayer?.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(CurrentDirectory, "TestStreamer.exe"));
        }


        private Device device = null;

        private Texture2D sharedTexture = null;
        D3DImageProvider imageProvider = null;


        private MfH264Decoder decoder = null;
        private MfVideoProcessor processor = null;


        private H264Session h264Session = null;
        private RtpReceiver rtpReceiver = null;

        //private Form testForm = null;
        private Form2 testForm = null;

        private void button4_Click(object sender, EventArgs e)
        {
            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            var inputArgs = new MfVideoArgs
            {
                Width = 1920,
                Height = 1080,

                //Width = 2560,
                //Height = 1440,

                //Width = 640,//2560,
                //Height = 480,//1440,
                FrameRate = 30,
            };

            var outputArgs = new MfVideoArgs
            {
                //Width = 640,//2560,
                //Height = 480,//1440,
                //Width = 2560,
                //Height = 1440,

                Width = 1920,
                Height = 1080,

                FrameRate = 30,
            };


            int adapterIndex = 0;
            var dxgiFactory = new SharpDX.DXGI.Factory1();
            var adapter = dxgiFactory.Adapters1[adapterIndex];


            device = new Device(adapter,
                                //DeviceCreationFlags.Debug |
                                DeviceCreationFlags.VideoSupport |
                                DeviceCreationFlags.BgraSupport);

            using (var multiThread = device.QueryInterface<SharpDX.Direct3D11.Multithread>())
            {
                multiThread.SetMultithreadProtected(true);
            }

            sharedTexture = new Texture2D(device,
                new Texture2DDescription
                {

                    CpuAccessFlags = CpuAccessFlags.None,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                    Width = outputArgs.Width,//640,//texture.Description.Width,
                    Height = outputArgs.Height, //480,//texture.Description.Height,

                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = { Count = 1, Quality = 0 },
                    Usage = ResourceUsage.Default,
                                //OptionFlags = ResourceOptionFlags.GdiCompatible//ResourceOptionFlags.None,
                                OptionFlags = ResourceOptionFlags.Shared,

                });


            imageProvider = new D3DImageProvider(Dispatcher.CurrentDispatcher);
            imageProvider.Setup(sharedTexture);

 
            if (testForm == null || testForm.IsDisposed)
            {
                testForm = new Form2
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1280,
                    Height = 720,

                    Text = (@"rtp://" + address + ":" + port),
                };

                //System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost
                //{
                //    Dock = DockStyle.Fill,
                //};
                //testForm.Controls.Add(host);


                //UserControl1 video = new UserControl1();
                //host.Child = video;

                //video.DataContext = imageProvider;

                var video = testForm.userControl11;
                video.DataContext = imageProvider;

                testForm.FormClosed += TestForm_FormClosed;
            }

            testForm.Visible = true;


            decoder = new MfH264Decoder(device);

            decoder.Setup(inputArgs);
            decoder.Start();

            var decoderType = decoder.OutputMediaType;
            var decFormat = decoderType.Get(MediaTypeAttributeKeys.Subtype);
            var decFrameSize = MfTool.GetFrameSize(decoderType);


            processor = new MfVideoProcessor(device);
            var inProcArgs = new MfVideoArgs
            {
                Width = decFrameSize.Width,
                Height = decFrameSize.Height,
                Format = decFormat,
            };


           
            var outProcArgs = new MfVideoArgs
            {
                Width = inputArgs.Width,
                Height = inputArgs.Height,
                Format = VideoFormatGuids.Argb32,
            };

            processor.Setup(inProcArgs, outProcArgs);
            processor.Start();


            h264Session = new H264Session();

            rtpReceiver = new RtpReceiver(null);
            rtpReceiver.Open(address, port);
            rtpReceiver.RtpPacketReceived += RtpReceiver_RtpPacketReceived;
            rtpReceiver.Start();


        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseRtpStream();
        }

        private void RtpReceiver_RtpPacketReceived(RtpPacket packet)
        {
            byte[] rtpPayload = packet.Payload.ToArray();

            //totalPayloadReceivedSize += rtpPayload.Length;
            // logger.Debug("totalPayloadReceivedSize " + totalPayloadReceivedSize);

            var nal = h264Session.Depacketize(packet);
            if (nal != null)
            {
                Decode(nal);
            }
        }


        int sampleCount = 0;
        private static object syncRoot = new object();
        private void Decode(byte[] nal)
        {
            try
            {
                var encodedSample = MediaFactory.CreateSample();
                try
                {
                    using (MediaBuffer mb = MediaFactory.CreateMemoryBuffer(nal.Length))
                    {
                        var dest = mb.Lock(out int cbMaxLength, out int cbCurrentLength);
                        //logger.Debug(sampleCount + " Marshal.Copy(...) " + nal.Length);
                        Marshal.Copy(nal, 0, dest, nal.Length);

                        mb.CurrentLength = nal.Length;
                        mb.Unlock();

                        encodedSample.AddBuffer(mb);
                    }

                    var res = decoder.ProcessSample(encodedSample, out Sample decodedSample);

                    if (res)
                    {
                        if (decodedSample != null)
                        {
                            res = processor.ProcessSample(decodedSample, out Sample rgbSample);

                            if (res)
                            {
                                if (rgbSample != null)
                                {
                                    var buffer = rgbSample.ConvertToContiguousBuffer();
                                    using (var dxgiBuffer = buffer.QueryInterface<DXGIBuffer>())
                                    {
                                        var uuid = SharpDX.Utilities.GetGuidFromType(typeof(Texture2D));
                                        dxgiBuffer.GetResource(uuid, out IntPtr intPtr);
                                        using (Texture2D rgbTexture = new Texture2D(intPtr))
                                        {
                                            device.ImmediateContext.CopyResource(rgbTexture, sharedTexture);
                                            device.ImmediateContext.Flush();
                                        };

                                        imageProvider?.Update();
                                    }

                                    buffer?.Dispose();
                                }
                            }

                            rgbSample?.Dispose();

                        }
                    }

                    decodedSample?.Dispose();
                }
                finally
                {
                    encodedSample?.Dispose();
                }

                sampleCount++;
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            CloseRtpStream();

            if (testForm != null && !testForm.IsDisposed)
            {
                testForm.Close();
                testForm.FormClosed -= TestForm_FormClosed;
                testForm = null;
            }

            if (imageProvider != null)
            {
                imageProvider.Close();
                imageProvider = null;
            }

        }

        private void CloseRtpStream()
        {
            if (rtpReceiver != null)
            {
                rtpReceiver.RtpPacketReceived -= RtpReceiver_RtpPacketReceived;
                rtpReceiver.Close();
            }

            if (decoder != null)
            {
                decoder.Close();
                decoder = null;
            }
            if (processor != null)
            {
                processor.Close();
                processor = null;
            }

            if (sharedTexture != null)
            {
                sharedTexture.Dispose();
                sharedTexture = null;
            }
            if (device != null)
            {
                device.Dispose();
                device = null;
            }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (testForm != null && !testForm.IsDisposed)
            {


                var addr = textBox1.Text;
                int port = (int)numericUpDown1.Value;
                testForm.StartInputSimulator(addr, port);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (testForm != null && !testForm.IsDisposed)
            {
                testForm.StopInputSimulator();
            }
        }
    }
}
