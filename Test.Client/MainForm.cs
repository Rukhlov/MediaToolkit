
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

//using Vlc.DotNet.Core;
using System.ServiceModel.Discovery;

using System.ServiceModel;
using System.Net;
using System.ServiceModel.Channels;
using MediaToolkit.Utils;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;
using Message = System.ServiceModel.Channels.Message;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Core;
using NAudio.CoreAudioApi;
using NAudio.Wave;

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


            //mediaPlayer.VideoHostControlHandle = this.panel1.Handle;
            //remoteClient = new RemoteDesktopClient();

            //remoteClient.StateChanged += RemoteClient_StateChanged;

            //remoteClient.UpdateBuffer += RemoteClient_UpdateBuffer;



            UpdateControls();
        }



        private void button3_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(CurrentDirectory, "Test.Streamer.exe"));
        }

        private VideoForm testForm = null;
        private D3DImageProvider2 imageProvider = null;


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

                imageProvider = new D3DImageProvider2();
                var reciver = remoteClient.VideoReceiver;

                imageProvider.Setup(reciver.sharedTexture);
                imageProvider.Start();

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

        private RemoteDesktopClient remoteClient = null;


        private void Connect(string _addr)
        {
           
            try
            {
                if (remoteClient != null)
                {
                    remoteClient.StateChanged -= RemoteClient_StateChanged;
                    remoteClient.UpdateBuffer -= RemoteClient_UpdateBuffer;
                    remoteClient.Close();

                }

                remoteClient = new RemoteDesktopClient();

                remoteClient.StateChanged += RemoteClient_StateChanged;

                remoteClient.UpdateBuffer += RemoteClient_UpdateBuffer;

                remoteClient.Connect(_addr);
                this.Cursor = Cursors.WaitCursor;


                panel2.Enabled = false;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                CleanUp();
            }
        }

        private void RemoteClient_StateChanged(ClientState state)
        {
            logger.Debug("RemoteClient_StateChanged(...) " + state);

            this.Invoke((Action)(() =>
            {
                if (state == ClientState.Connected)
                {
                    string title = remoteClient.ServerName + " (" + remoteClient.ServerAddr + ")"; //(@"rtp://" + address + ":" + 1234);

                    ShowVideoForm(title);
                    var inputMan = remoteClient.InputManager;
                    testForm.LinkInputManager(inputMan);

                    UpdateControls();


                }
                else if (state == ClientState.Disconnected)
                {
                    UpdateControls();
                }
                else if (state == ClientState.Faulted)
                {
                    MessageBox.Show("(state == ClientState.Faulted)", "Achtung_Achtung");

                    remoteClient?.Close();

                    CloseVideoForm();
                }

                this.Cursor = Cursors.Default;
               // this.UseWaitCursor = false;
                panel2.Enabled = true;

            }));
        }

        Stopwatch sw = new Stopwatch();
        private void RemoteClient_UpdateBuffer()
        {
            

            imageProvider?.Update();
            //var ts = sw.ElapsedMilliseconds;
            //Console.WriteLine("ElapsedMilliseconds " + ts);

            //sw.Restart();
        }

        private void UpdateControls()
        {
            bool isConnected = (remoteClient != null && remoteClient.State == ClientState.Connected);

            connectButton.Enabled = !isConnected;
            disconnectButton.Enabled = isConnected;
        }


        private void Disconnect()
        {

  

            remoteClient.UpdateBuffer -= RemoteClient_UpdateBuffer;
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



        private void stopButton_Click(object sender, EventArgs e)
        {
            logger.Debug("stopButton_Click(...)");

            try
            {
                remoteClient.UpdateBuffer -= RemoteClient_UpdateBuffer;

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
            ProgressForm progress = new ProgressForm
            {
                StartPosition = FormStartPosition.CenterParent,
            };

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



        private void hostsComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            var obj = hostsComboBox.SelectedItem;

            if (obj != null)
            {
                var item = obj as ComboBoxItem;
                if (item != null)
                {
                    var tag = item.Tag;
                    if (tag != null)
                    {
                        var addr = tag.ToString();
                        try
                        {
                            var builder = new UriBuilder(addr);

                            remoteDesktopTextBox.Text = builder.Host;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex);
                        }

                    }

                }
            }
        }

        private void hostsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }


    class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag { get; set; }
    }

    //public class ProgressForm : Form
    //{

    //    private Button button = new Button();
    //    public ProgressBar progress = new ProgressBar();
    //    private TableLayoutPanel panel = new TableLayoutPanel();

    //    public ProgressForm()
    //    {
    //        this.MaximizeBox = false;

    //        this.Width = 450;
    //        this.Height = 60;

    //        button.Text = "_Cancel";
    //        button.AutoSize = true;

    //        this.DialogResult = System.Windows.Forms.DialogResult.OK;

    //        button.Dock = DockStyle.Fill;

    //        this.CancelButton = button;

    //        progress.Dock = DockStyle.Fill;
    //        progress.Style = ProgressBarStyle.Marquee;

    //        this.panel.Dock = DockStyle.Fill;

    //        this.panel.ColumnCount = 2;
    //        this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80));
    //        this.panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20));

    //        this.panel.Controls.Add(this.progress, 0, 0);
    //        this.panel.Controls.Add(this.button, 1, 0);

    //        this.Controls.Add(panel);
    //    }

    //}
}
