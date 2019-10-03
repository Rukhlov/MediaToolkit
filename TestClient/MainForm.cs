
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
using System.ServiceModel.Discovery;
using MediaToolkit.Common;
using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;
using MediaToolkit.Utils;

namespace TestClient
{
    public partial class MainForm : Form
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly string CurrentDirectory = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
        public static readonly System.IO.DirectoryInfo VlcLibDirectory = new System.IO.DirectoryInfo(System.IO.Path.Combine(CurrentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

        public MainForm()
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

            //mediaPlayer.VideoHostControlHandle = this.panel1.Handle;


            SharpDX.MediaFoundation.MediaManager.Startup();

        }

        private VideoForm vlcVideoForm = null;
        private VlcMediaPlayer mediaPlayer = null;
        private void button1_Click(object sender, EventArgs e)
        {
            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            if (vlcVideoForm == null || vlcVideoForm.IsDisposed)
            {
                vlcVideoForm = new VideoForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1280,
                    Height = 720,
                
                };

                mediaPlayer.VideoHostControlHandle = vlcVideoForm.elementHost1.Handle; //vlcVideoForm.Handle;

                //System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost
                //{
                //    Dock = DockStyle.Fill,
                //};
                //testForm.Controls.Add(host);


                //UserControl1 video = new UserControl1();
                //host.Child = video;

                //video.DataContext = imageProvider;


                vlcVideoForm.FormClosed += VlcVideoForm_FormClosed;
            }

            vlcVideoForm.Visible = true;


            var file = Path.Combine(CurrentDirectory, "tmp.sdp");
            string[] sdp = 
            {
                "v=0",
                "s=SCREEN_STREAM",
                "c=IN IP4 " + address, // "c=IN IP4 239.0.0.1",
                "m=video " + port  + " RTP/AVP 96", //"m=video 1234 RTP/AVP 96",
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

        private void VlcVideoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mediaPlayer?.Stop();
        }

        //FileStream file = new FileStream("d:\\test_rtp.h264", FileMode.Create);

        private void button2_Click(object sender, EventArgs e)
        {
            vlcVideoForm?.Close();
          

            //mediaPlayer?.Stop();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(CurrentDirectory, "TestStreamer.exe"));
        }


        private VideoReceiver videoReceiver = null;

        private D3DImageProvider imageProvider = null;


        //private Form testForm = null;
        private VideoForm testForm = null;

        private void button4_Click(object sender, EventArgs e)
        {
            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            videoReceiver = new VideoReceiver();

            var inputPars = new VideoEncodingParams
            {
                Width = (int)srcWidthNumeric.Value,            
                Height = (int)srcHeightNumeric.Value,

                //Width = 2560,
                //Height = 1440,

                //Width = 640,//2560,
                //Height = 480,//1440,
                FrameRate = 30,
            };

            var outputPars = new VideoEncodingParams
            {
                //Width = 640,//2560,
                //Height = 480,//1440,
                //Width = 2560,
                //Height = 1440,

                Width = (int)destWidthNumeric.Value,
                Height = (int)destHeightNumeric.Value,

                FrameRate = 30,
            };

            var networkPars = new NetworkStreamingParams
            {
                SrcAddr = address,
                SrcPort = port
            };

            videoReceiver.Setup(inputPars, outputPars, networkPars);
            videoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;


            if (testForm == null || testForm.IsDisposed)
            {
                testForm = new VideoForm
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
                imageProvider = new D3DImageProvider(Dispatcher.CurrentDispatcher);

                var video = testForm.userControl11;
                video.DataContext = imageProvider;

                testForm.FormClosed += TestForm_FormClosed;
            }

            testForm.Visible = true;

            imageProvider.Start(videoReceiver.sharedTexture);

            videoReceiver.Play();

        }

        private void VideoReceiver_UpdateBuffer()
        {
            imageProvider?.Update();
        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (videoReceiver != null)
            {
                videoReceiver.Stop();
                videoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
            }

            imageProvider?.Close();

        }



        private void button5_Click(object sender, EventArgs e)
        {

            if (videoReceiver != null)
            {
                videoReceiver.Stop();
                videoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
            }

            imageProvider?.Close();

            if (testForm != null && !testForm.IsDisposed)
            {
                testForm.Close();
                testForm.FormClosed -= TestForm_FormClosed;
                testForm = null;
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

        class ComboBoxItem
        {
            public string Name { get; set; }
            public object Tag{ get; set; }
        }

        private void findServiceButton_Click(object sender, EventArgs e)
        {
            logger.Debug("\nFinding IRemoteDesktopService...");

            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());

            var criteria = new FindCriteria(typeof(IRemoteDesktopService));
            criteria.Duration = TimeSpan.FromSeconds(5);
            ProgressForm progress = new ProgressForm();

            List<ComboBoxItem> hostItems = new List<ComboBoxItem>();

            discoveryClient.FindCompleted += (o, a) =>
            {
                logger.Debug("FindCompleted(...)");

                if (a.Cancelled)
                {
                    logger.Debug("Cancelled");
                }
                if (a.Error != null)
                {
                    logger.Debug(a.Error.ToString());
                }

                if (!a.Cancelled)
                {
                    var result = a.Result;
                    if (result != null)
                    {
                        foreach (var ep in result.Endpoints)
                        {
                            string address = ep.Address.ToString();
                            string hostName = address;

                            var extensions = ep.Extensions;
                            if(extensions!=null && extensions.Count > 0)
                            {
                                var hostElement = extensions.FirstOrDefault(el => el.Name == "HostName");
                                if (hostElement != null)
                                {
                                    hostName = hostElement.Value; // + " {" + address + "}";
                                }
                            }

                            logger.Debug(hostName);

                            hostItems.Add(new ComboBoxItem
                            {
                                Name = hostName,
                                Tag = address,
                            });
                        }
                    }



                }

                hostsComboBox.DataSource = hostItems;
                hostsComboBox.DisplayMember = "Name";

                progress.Close();
            };

            discoveryClient.FindProgressChanged += (o, a) =>
            {
   

                logger.Debug("FindProgressChanged(...) " + a.EndpointDiscoveryMetadata.Address.ToString());
            };


            progress.Shown += (o, a) =>
            {
                discoveryClient.FindAsync(criteria, this);

            };

            progress.FormClosed += (o, a) =>
            {
                logger.Debug("FormClosed(...)");

                if (discoveryClient != null)
                {
                    discoveryClient.CancelAsync(this);
                    discoveryClient.Close();
                }
            };

            progress.ShowDialog();

        }

        private ChannelFactory<IRemoteDesktopService> factory = null;
        private void connectButton_Click(object sender, EventArgs e)
        {
            var addr = remoteDesktopTextBox.Text;
            try
            {

                var uri = new Uri(addr);

                var binding = new NetTcpBinding
                {
                    ReceiveTimeout = TimeSpan.MaxValue,//TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(10),
                };
                factory = new ChannelFactory<IRemoteDesktopService>(binding, new EndpointAddress(uri));

                var channel = factory.CreateChannel();
                try
                {
                    channel.Connect("", null);

                    //var receiver = new RtpReceiver(null);
                    //receiver.Open("0.0.0.0", 1234);

                    //var hostName = Dns.GetHostName();
                    //var localIPs = Dns.GetHostAddresses(hostName);
                    //var localAddr = localIPs.FirstOrDefault(a=>a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    //var localAddr = receiver.LocalEndpoint;

                     var options = new RemoteDesktopOptions
                    {
                        DestAddr = "192.168.1.135",//localAddr.Address.ToString(), //localAddr.ToString(),
                        DestPort = 1234,
                        DstSize = new Size(2560, 1440),
                        
                    };

                    channel.Start(options);

                }
                finally
                {
                    if (channel != null)
                    {
                        ((IClientChannel)channel).Close();
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (factory != null)
                {
                    var channel = factory.CreateChannel();
                    try
                    {
                        channel.Stop();
                        channel.Disconnect();

                    }
                    finally
                    {
                        if (channel != null)
                        {
                            ((IClientChannel)channel).Close();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private StatisticForm statisticForm = new StatisticForm();

        private void button8_Click(object sender, EventArgs e)
        {
            statisticForm.Visible = !statisticForm.Visible;
        }


    }


    public class ProgressForm : Form
    {

        private Button button = new Button();
        public ProgressBar progress = new ProgressBar();
        private TableLayoutPanel panel = new TableLayoutPanel();

        public ProgressForm()
        {
            this.MaximizeBox = false;

            this.Width = 450;
            this.Height = 60;

            button.Text = "Cancel";
            button.AutoSize = true;

            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            button.Dock = DockStyle.Fill;

            this.CancelButton = button;

            progress.Dock = DockStyle.Fill;
            progress.Style = ProgressBarStyle.Marquee;

            this.panel.Dock = DockStyle.Fill;

            this.panel.ColumnCount = 2;
            this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80));
            this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20));

            this.panel.Controls.Add(this.progress, 0, 0);
            this.panel.Controls.Add(this.button, 1, 0);

            this.Controls.Add(panel);
        }

    }
}
