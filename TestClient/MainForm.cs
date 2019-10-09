
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
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using Message = System.ServiceModel.Channels.Message;
using MediaToolkit.NativeAPIs;

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

        private VideoForm testForm = null;
        private D3DImageProvider2 imageProvider = null;

        private void button4_Click(object sender, EventArgs e)
        {
            var address = addressTextBox.Text;
            if (string.IsNullOrEmpty(address))
            {
                return;
            }

            var port = (int)portNumeric.Value;

            try
            {
                remoteClient = new RemoteDesktopClient();

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

                remoteClient.Play(inputPars, outputPars, networkPars);

                string title = (@"rtp://" + address + ":" + port);

                ShowVideoForm(title);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }

        }

        private void ShowVideoForm(string title)
        {
            if (testForm == null || testForm.IsDisposed)
            {
                testForm = new VideoForm
                {
                    StartPosition = FormStartPosition.CenterParent,
                    Width = 1280,
                    Height = 720,

                    Text = title,
                };

                imageProvider = new D3DImageProvider2(Dispatcher.CurrentDispatcher);
                var reciver = remoteClient.VideoReceiver;

                imageProvider.Start(reciver.sharedTexture);

                var video = testForm.userControl11;
                video.DataContext = imageProvider;

                testForm.FormClosed += TestForm_FormClosed;
            }


            testForm.Visible = true;
        }

        private void CloseVideoForm()
        {
            if (testForm != null && !testForm.IsDisposed)
            {
                testForm.UnlinkInputManager();

                testForm.Close();
                testForm.FormClosed -= TestForm_FormClosed;
                testForm = null;
            }
        }

        public string ClientId { get; private set; }
        public string ServerId { get; private set; }

        private RemoteDesktopClient remoteClient = new RemoteDesktopClient();

        private ChannelFactory<IRemoteDesktopService> factory = null;

        private void Connect(string _addr)
        {
            try
            {
                remoteClient.Connect(_addr);

                string title = remoteClient.ServerName + " (" + _addr + ")"; //(@"rtp://" + address + ":" + 1234);

                ShowVideoForm(title);
                var inputMan = remoteClient.InputManager;
                testForm.LinkInputManager(inputMan);


            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }
        }

        private void Disconnect()
        {

            remoteClient?.Disconnect();

            CloseVideoForm();

        }


        private void connectButton_Click(object sender, EventArgs e)
        {
            var _addr = remoteDesktopTextBox.Text;

            Connect(_addr);
        }


        private void disconnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }
        }


        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            remoteClient?.Disconnect();

        }



        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                remoteClient?.Stop();

                CloseVideoForm();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }


        private void findServiceButton_Click(object sender, EventArgs e)
        {
            logger.Debug("\nFinding IRemoteDesktopService...");

            var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
            udpDiscoveryEndpoint.EndpointBehaviors.Add(new WcfDiscoveryAddressCustomEndpointBehavior());

            DiscoveryClient discoveryClient = new DiscoveryClient(udpDiscoveryEndpoint);

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


        private void CleanUp()
        {
            //...

        }

        private StatisticForm statisticForm = new StatisticForm();

        private void button8_Click(object sender, EventArgs e)
        {
            statisticForm.Visible = !statisticForm.Visible;
        }

        class ComboBoxItem
        {
            public string Name { get; set; }
            public object Tag { get; set; }
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
