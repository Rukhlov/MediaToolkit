using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MediaToolkit.UI;

using System.Windows.Threading;
using NLog;
using System.ServiceModel;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using MediaToolkit;
using MediaToolkit.Core;
using System.ServiceModel.Discovery;
using MediaToolkit.Utils;

using MediaToolkit.SharedTypes;

namespace MediaToolkit.UI
{
    public partial class ScreenCastControl : UserControl, IScreenCasterControl
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public ScreenCastControl()
        {
            InitializeComponent();

           // MediaToolkit.Utils.MediaLib.Startup();

            syncContext = SynchronizationContext.Current;

            controlPanel.Visible = false;


            showDetailsButton.Text = controlPanel.Visible ? "<<" : ">>";

            UpdateControls();

        }
        private readonly SynchronizationContext syncContext = null;

        public VideoReceiver VideoReceiver { get; private set; }
        public AudioReceiver AudioReceiver { get; private set; }


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }
        public string ServerName { get; private set; }
        public string ServerAddr { get; private set; }
        public int ServerPort { get; private set; }

        public ClientState State { get; private set; }

        private ChannelFactory<IScreenCastService> factory = null;

        private bool finding = false;

        private DiscoveryClient discoveryClient = null;

        private void findServiceButton_Click(object sender, EventArgs e)
        {
            logger.Debug("findServiceButton_Click(...)");

            if (!finding)
            {
                var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
                udpDiscoveryEndpoint.EndpointBehaviors.Add(new WcfDiscoveryAddressCustomEndpointBehavior());

                if (discoveryClient == null)
                {
                    discoveryClient = new DiscoveryClient(udpDiscoveryEndpoint);

                    discoveryClient.FindCompleted += DiscoveryClient_FindCompleted;
                    discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;
                }

                var criteria = new FindCriteria(typeof(IScreenCastService));
                criteria.Duration = TimeSpan.FromSeconds(5);


                finding = true;
                findServiceButton.Text = "_Cancel";
                labelStatus.Text = "_Finding...";

                connectButton.Enabled = false;
                hostsComboBox.Enabled = false;
                hostsComboBox.DataSource = null;

                discoveryClient.FindAsync(criteria, this);
            }
            else
            {
                if (discoveryClient != null)
                {
                    discoveryClient.CancelAsync(this);
                    discoveryClient.Close();
                }
            }
        }



        private void DiscoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            logger.Debug("FindProgressChanged(...) " + e.EndpointDiscoveryMetadata.Address.ToString());
        }

        private void DiscoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            logger.Debug("FindCompleted(...)");

            finding = false;

            List<ComboBoxItem> hostItems = new List<ComboBoxItem>();

            if (e.Cancelled)
            {
                logger.Debug("Cancelled");
            }
            if (e.Error != null)
            {
                logger.Debug(e.Error.ToString());
            }


            if (!e.Cancelled)
            {
                var result = e.Result;
                if (result != null)
                {


                    foreach (var ep in result.Endpoints)
                    {
                        string address = ep.Address.ToString();
                        string hostName = address;

                        var extensions = ep.Extensions;
                        if (extensions != null && extensions.Count > 0)
                        {
                            var hostElement = extensions.FirstOrDefault(el => el.Name == "HostName");
                            if (hostElement != null)
                            {
                                hostName = hostElement.Value;// + " {" + address + "}";
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

            discoveryClient.Close();
            discoveryClient = null;


            connectButton.Enabled = true;
            hostsComboBox.Enabled = true;

            findServiceButton.Text = "_Find";
            labelStatus.Text = "_Not Connected";
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (discoveryClient != null)
            {
                discoveryClient.CancelAsync(this);
                discoveryClient.Close();
            }

        }


        private void connectButton_Click(object sender, EventArgs e)
        {

            logger.Debug("connectButton_Click()");

            try
            {


                if (!running)
                {
                    var addrStr = hostAddressTextBox.Text;

                    var uri = new Uri("net.tcp://" + addrStr);

                    logger.Info("Connect to: " + uri.ToString());

                    var host = uri.Host;
                    var port = uri.Port;

                    Connecting(host, port);
                }
                else
                {
                    if (running)
                    {
                        running = false;
                        State = ClientState.Closing;
                    }
                    else
                    {
                        Close();
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }


        private void hostsComboBox_SelectedIndexChanged(object sender, EventArgs e)
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

                            hostAddressTextBox.Text = builder.Host + ":" + builder.Port;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex);
                        }

                    }

                }
            }
        }


        private D3DImageProvider2 imageProvider = null;
        private void ShowVideo()
        {
            if (VideoReceiver != null)
            {
                imageProvider?.Close();

                imageProvider = new D3DImageProvider2(Dispatcher.CurrentDispatcher);
                var reciver = this.VideoReceiver;

                imageProvider.Start(reciver.sharedTexture);


                this.wpfRemoteControl.DataContext = imageProvider;
            }

        }

        private void HideVideo()
        {
            imageProvider?.Close();

            wpfRemoteControl.DataContext = null;
        }

        public void Connect(string addr)
        {

        }

        public void Disconnect()
        {

        }

        public void Connecting(string addr, int port)
        {

            logger.Debug("RemoteDesktopClient::Connecting(...) " + addr);
            State = ClientState.Starting;
            errorMessage = "";

            labelStatus.Text = "_Connecting...";

            controlPanel.Enabled = false;

            this.ServerAddr = addr;
            this.ServerPort = port;
            this.hostAddressTextBox.Text = addr + ":" + port;

            Task.Run(() =>
            {
                ClientProc();
            });

        }

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private bool running = false;
        private string errorMessage = "";

        private void ClientProc()
        {

            try
            {
                var address = "net.tcp://" + ServerAddr + "/ScreenCaster";
                if (this.ServerPort > 0)
                {
                    address = "net.tcp://" + ServerAddr + ":" + ServerPort + "/ScreenCaster";
                }

                var uri = new Uri(address);
                this.ClientId = RngProvider.GetRandomNumber().ToString();

                //NetTcpSecurity security = new NetTcpSecurity
                //{
                //    Mode = SecurityMode.Transport,
                //    Transport = new TcpTransportSecurity
                //    {
                //        ClientCredentialType = TcpClientCredentialType.Windows,
                //        ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign,
                //    },
                //};

                NetTcpSecurity security = new NetTcpSecurity
                {
                    Mode = SecurityMode.None,
                };

                var binding = new NetTcpBinding
                {
                    ReceiveTimeout = TimeSpan.MaxValue,//TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(5),
                    OpenTimeout = TimeSpan.FromSeconds(5),

                    Security = security,
                };

                factory = new ChannelFactory<IScreenCastService>(binding, new EndpointAddress(uri));
                var channel = factory.CreateChannel();
                bool isBusy = false;
                try
                {

                    var channelInfos = channel.GetChannelInfos();

                    if (channelInfos == null)
                    {
                        logger.Error("channelInfos == null");
                        return;
                    }


                    //TransportMode transportMode = TransportMode.Udp;
                    var videoChannelInfo = channelInfos.FirstOrDefault(c => c.MediaInfo is VideoChannelInfo);

                    if (videoChannelInfo != null)
                    {
                        if(videoChannelInfo.Transport == TransportMode.Tcp && videoChannelInfo.ClientsCount > 0)
                        {
                            errorMessage = "_Server is busy, try later";
                            throw new Exception(errorMessage);

                        }

                        SetupVideo(videoChannelInfo);
                    }


                    var audioChannelInfo = channelInfos.FirstOrDefault(c => c.MediaInfo is AudioChannelInfo);
                    if (audioChannelInfo != null)
                    {
                        if (audioChannelInfo.Transport == TransportMode.Tcp && videoChannelInfo.ClientsCount > 0)
                        {
                            errorMessage = "_Server is busy, try later";
                            throw new Exception(errorMessage);
                        }

                        SetupAudio(audioChannelInfo);

                    }


                    if (VideoReceiver != null)
                    {
                        VideoReceiver.Play();
                    }

                    if (AudioReceiver != null)
                    {
                        AudioReceiver.Play();
                    }


                    running = true;

                    State = ClientState.Started;

                    OnStateChanged(State);

                    while (running)
                    {
                        try
                        {
                            channel.PostMessage(new ServerRequest { Command = "Ping" });

                            syncEvent.WaitOne(1000);
                        }
                        catch (Exception ex)
                        {

                            errorMessage = "_Server Disconnected";
                            

                            running = false;
                        }

                    }
                }
                finally
                {
                    running = false;

                    State = ClientState.Closed;
                    OnStateChanged(State);

                    try
                    {
                        var c = (IClientChannel)channel;
                        if (c.State != CommunicationState.Faulted)
                        {
                            c.Close();
                        }
                        else
                        {
                            c.Abort();
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);

                if(string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "_Connection Error";
                }



                State = ClientState.Faulted;
                OnStateChanged(State);

            }
            finally
            {
                Close();
                
            }
        }

        private void SetupAudio(ScreencastChannelInfo audioChannelInfo)
        {
            var audioInfo = audioChannelInfo.MediaInfo as AudioChannelInfo;
            if (audioInfo != null)
            {

                var audioAddr = audioChannelInfo.Address;

                if (audioChannelInfo.Transport == TransportMode.Tcp)
                {
                    audioAddr = ServerAddr;
                }

                var audioPort = audioChannelInfo.Port;

                AudioReceiver = new AudioReceiver();

                var networkPars = new NetworkSettings
                {
                    LocalAddr = audioAddr,
                    LocalPort = audioPort,
                    TransportMode = audioChannelInfo.Transport,
                    SSRC = audioChannelInfo.SSRC,

                };

                var audioDeviceId = "";
                try
                {
                    var devices = NAudio.Wave.DirectSoundOut.Devices;
                    var device = devices.FirstOrDefault();
                    audioDeviceId = device?.Guid.ToString() ?? "";
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }


                var audioPars = new AudioEncoderSettings
                {
                    SampleRate = audioInfo.SampleRate,
                    Channels = audioInfo.Channels,
                    Encoding = "ulaw",
                    DeviceId = audioDeviceId,
                };

                AudioReceiver.Setup(audioPars, networkPars);
            }
        }

        private void SetupVideo(ScreencastChannelInfo videoChannelInfo)
        {

            //if (transportMode == TransportMode.Tcp)
            //{
            //    var res = channel.Play(channelInfos);
            //}

            var videoInfo = videoChannelInfo.MediaInfo as VideoChannelInfo;
            if (videoInfo != null)
            {
                var videoAddr = videoChannelInfo.Address;

                if (videoChannelInfo.Transport == TransportMode.Tcp)
                {
                    videoAddr = ServerAddr;

                   
                }

                var videoPort = videoChannelInfo.Port;
                var inputPars = new VideoEncoderSettings
                {
                    Resolution = videoInfo.Resolution,
                    //Width = videoInfo.Resolution.Width,
                    //Height = videoInfo.Resolution.Height,
                    FrameRate = videoInfo.Fps,
                };

                var outputPars = new VideoEncoderSettings
                {
                    Resolution = videoInfo.Resolution,
                    //Resolution = new System.Drawing.Size(1920, 1080),
                    ////Width = 640,//2560,
                    ////Height = 480,//1440,
                    //Width = 1920,
                    //Height = 1080,

                    FrameRate = videoInfo.Fps,
                };

                var networkPars = new NetworkSettings
                {
                    LocalAddr = videoAddr,
                    LocalPort = videoPort,
                    TransportMode = videoChannelInfo.Transport,
                    SSRC = videoChannelInfo.SSRC,
                };

                VideoReceiver = new VideoReceiver();

                VideoReceiver.Setup(inputPars, outputPars, networkPars);
                VideoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;
            }
        }

        private void OnStateChanged(ClientState state)
        {
            syncContext.Send(_ => 
            {
                if (state == ClientState.Started)
                {
                    ShowVideo();
                }
                else
                {
                    HideVideo();
                }

                UpdateControls();

            }, null);
  

        }

        private void VideoReceiver_UpdateBuffer()
        {
            imageProvider?.Update();
        }



        public void Close()
        {

            if (VideoReceiver != null)
            {
                VideoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
                VideoReceiver.Stop();
                VideoReceiver = null;
            }

            if (AudioReceiver != null)
            {
                AudioReceiver.Stop();
                AudioReceiver = null;
            }


            if (factory != null)
            {
                factory.Abort();
                factory = null;
            }

            State = ClientState.Closed;
        }

        private void UpdateControls()
        {
            bool isConnected = (this.State == ClientState.Started);

            //connectButton.Enabled = !isConnected;
            connectButton.Text = isConnected ? "_Disconnect" : "_Connect";

            findServiceButton.Enabled = !isConnected;
            hostsComboBox.Enabled = !isConnected;
            hostAddressTextBox.Enabled = !isConnected;

            controlPanel.Enabled = true;

            if (errorMessage != "")
            {
                labelStatus.Text = errorMessage;
            }
            else
            {
                labelStatus.Text = isConnected ? "_Connected" : "_Not Connected";
            }

            //labelInfo.Text = errorMessage;
        }



        //private InputManager inputMan = null;

        //public void LinkInputManager(InputManager man)
        //{
        //    logger.Debug("LinkInputManager(...)");

        //    this.wpfRemoteControl.MouseMove += UserControl11_MouseMove;

        //    this.wpfRemoteControl.MouseLeftButtonDown += UserControl11_MouseLeftButtonDown;
        //    this.wpfRemoteControl.MouseLeftButtonUp += UserControl11_MouseLeftButtonUp;

        //    this.wpfRemoteControl.MouseRightButtonDown += UserControl11_MouseRightButtonDown;
        //    this.wpfRemoteControl.MouseRightButtonUp += UserControl11_MouseRightButtonUp;

        //    this.wpfRemoteControl.MouseDoubleClick += UserControl11_MouseDoubleClick;
        //    this.inputMan = man;



        //}

        //public void UnlinkInputManager()
        //{
        //    logger.Debug("UnlinkInputManager()");

        //    this.inputMan = null;

        //    this.wpfRemoteControl.MouseMove -= UserControl11_MouseMove;
        //    this.wpfRemoteControl.MouseLeftButtonDown -= UserControl11_MouseLeftButtonDown;
        //    this.wpfRemoteControl.MouseLeftButtonUp -= UserControl11_MouseLeftButtonUp;
        //    this.wpfRemoteControl.MouseRightButtonDown -= UserControl11_MouseRightButtonDown;
        //    this.wpfRemoteControl.MouseRightButtonUp -= UserControl11_MouseRightButtonUp;

        //    this.wpfRemoteControl.MouseDoubleClick -= UserControl11_MouseDoubleClick;
        //}

        private void UserControl11_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void UserControl11_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logger.Debug("MouseLeftButtonDown(...) ");

            //if (inputMan != null)
            //{
            //    var message = "MouseDown:" + (int)MouseButtons.Left + " " + 1;

            //    inputMan.ProcessMessage(message);
            //}


        }

        private void UserControl11_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logger.Debug("MouseLeftButtonUp(...) ");

            //if (inputMan != null)
            //{
            //    var message = "MouseUp:" + (int)MouseButtons.Left + " " + 1;

            //    inputMan.ProcessMessage(message);
            //}

        }

        private void UserControl11_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logger.Debug("MouseRightButtonUp(...) ");

            //if (inputMan != null)
            //{
            //    var message = "MouseUp:" + (int)MouseButtons.Right + " " + 1;

            //    inputMan.ProcessMessage(message);
            //}
        }

        private void UserControl11_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            logger.Debug("MouseRightButtonDown(...) ");

            //if (inputMan != null)
            //{
            //    var message = "MouseDown:" + (int)MouseButtons.Right + " " + 1;

            //    inputMan.ProcessMessage(message);
            //}
        }


        private void UserControl11_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //// Console.WriteLine("MouseMove(...)");

            //// Console.WriteLine("MouseMove(...)");

            //var pos = e.GetPosition(wpfRemoteControl);

            //double x = pos.X;
            //double y = pos.Y;

            //double w = wpfRemoteControl.RenderSize.Width;
            //double h = wpfRemoteControl.RenderSize.Height;

            //double _x = (x * 65536.0) / w;
            //double _y = (y * 65536.0) / h;

            //var time = DateTime.Now;

            //if (inputMan != null)
            //{
            //    var message = "MouseMove:" + _x.ToString(CultureInfo.InvariantCulture) + " " + _y.ToString(CultureInfo.InvariantCulture);

            //    inputMan.ProcessMessage(message);
            //}

        }


        private void detailsButton_Click(object sender, EventArgs e)
        {
            //controlPanel.Visible = !controlPanel.Visible;

            //detailsButton.Text = mainPanel.Visible ? "<<" : ">>";
        }

        private void showDetailsButton_Click(object sender, EventArgs e)
        {
            controlPanel.Visible = !controlPanel.Visible;

            showDetailsButton.Text = controlPanel.Visible ? "<<" : ">>";
        }

        private void labelStatus_Click(object sender, EventArgs e)
        {

        }
    }

    class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag { get; set; }
    }

    public enum ClientState
    {
        Created,
        Starting,
        Started,
        Closing,
        Closed,
        Faulted,
    }

}
