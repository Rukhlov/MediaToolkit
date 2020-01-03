using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.ServiceModel;
using MediaToolkit.Core;
using MediaToolkit;
using NLog;

using MediaToolkit.Utils;
using System.ServiceModel.Discovery;
using NAudio.Wave;
using MediaToolkit.UI;
using System.Windows.Threading;

namespace TestClient.Controls
{
    public partial class ScreenCastReceiverControl : UserControl
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenCastReceiverControl()
        {
            InitializeComponent();
            syncContext = SynchronizationContext.Current;

            //LoadMMDevicesCombo();
            this.StateChanged += ScreenCastControl_StateChanged;

            UpdateControls();
        }

        private SynchronizationContext syncContext = null;

        private void UpdateControls()
        {
            bool isConnected = (this.State == ClientState.Connected);

            connectButton.Enabled = !isConnected;
            disconnectButton.Enabled = isConnected;
            findServiceButton.Enabled = !isConnected;
            hostsComboBox.Enabled = !isConnected;
            remoteDesktopTextBox.Enabled = !isConnected;
        }

        private void ScreenCastControl_StateChanged(ClientState obj)
        {
            syncContext.Send(_ => 
            {
                if (obj == ClientState.Connected)
                {
                    ShowVideoForm("");
                    UpdateControls();
                }
                else
                {
                    CloseVideoForm();
                    UpdateControls();

                }
            }, null);

        }

        private void findServiceButton_Click(object sender, EventArgs e)
        {
            logger.Debug("\nFinding IRemoteDesktopService...");

            Find();

        }



        private void connectButton_Click(object sender, EventArgs e)
        {
            logger.Debug("connectButton_Click()");

            try
            {

                var addrStr = remoteDesktopTextBox.Text;
                //var uriBuilder = new UriBuilder(uri);
                var uri = new Uri("net.tcp://" + addrStr);

                logger.Info("Connect to: " + uri.ToString());

                var host = uri.Host;//uriBuilder.Host;
                var port = uri.Port;//uriBuilder.Port;

                Start(host, port);
            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            if (running)
            {
                running = false;
            }
            else
            {
                Close();
            }

            //Close();
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

                            remoteDesktopTextBox.Text = builder.Host + ":" + builder.Port;
                        }
                        catch (Exception ex)
                        {
                            logger.Debug(ex);
                        }

                    }

                }
            }
        }

        public VideoReceiver VideoReceiver { get; private set; }
        public AudioReceiver AudioReceiver { get; private set; }


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }
        public string ServerName { get; private set; }
        public string ServerAddr { get; private set; }
        public int ServerPort { get; private set; }
        public ClientState State { get; private set; }


        private ChannelFactory<IScreenCastService> factory = null;

        public void Start(string _addr, int port)
        {

            logger.Debug("RemoteDesktopClient::Connect(...) " + _addr);

            this.ServerAddr = _addr;
            this.ServerPort = port;

            Task.Run(() =>
            {
                ClientProc();
            });

        }

        private CommandQueue commandQueue = new CommandQueue();

        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private bool running = false;


        private void ClientProc()
        {
            var address = "net.tcp://" + ServerAddr + "/ScreenCaster";

            if (this.ServerPort > 0)
            {
                address = "net.tcp://" + ServerAddr + ":" + ServerPort + "/ScreenCaster";
            }
            
            try
            {

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
                    SendTimeout = TimeSpan.FromSeconds(10),
                    Security = security,
                };

                factory = new ChannelFactory<IScreenCastService>(binding, new EndpointAddress(uri));
                var channel = factory.CreateChannel();

                try
                {
                    //channel.PostMessage(new ServerRequest { Command = "Ping" });

                    var channelInfos = channel.GetChannelInfos();

                    if (channelInfos == null)
                    {
                        logger.Error("channelInfos == null");
                        return;
                    }


                    TransportMode transportMode = TransportMode.Udp;
                    var videoChannelInfo = channelInfos.FirstOrDefault(c => c.MediaInfo is VideoChannelInfo);

                    if (videoChannelInfo != null)
                    {
                        transportMode = videoChannelInfo.Transport;

                        if(transportMode == TransportMode.Tcp)
                        {
                            if (videoChannelInfo.ClientsCount > 0)
                            {
                                throw new Exception("Server is busy");
                            }
                        }

                        var videoAddr = videoChannelInfo.Address;

                        if(transportMode == TransportMode.Tcp)
                        {
                            videoAddr = ServerAddr;
                        }

                        var videoPort = videoChannelInfo.Port;

                        //if (string.IsNullOrEmpty(videoAddr))
                        //{
                        //    //channel.Play()
                        //}

                        //if (transportMode == TransportMode.Tcp)
                        //{
                        //   var res = channel.Play(channelInfos);
                        //}

                        var videoInfo = videoChannelInfo.MediaInfo as VideoChannelInfo;
                        if (videoInfo != null)
                        {
                            var inputPars = new VideoEncoderSettings
                            {
                                Resolution = videoInfo.Resolution,
                                //Width = videoInfo.Resolution.Width,
                                //Height = videoInfo.Resolution.Height,
                                FrameRate = videoInfo.Fps,
                            };

                            var outputPars = new VideoEncoderSettings
                            {
                                //Width = 640,//2560,
                                //Height = 480,//1440,
                                //Width = 1920,
                                //Height = 1080,

                                //FrameRate = 30,

                                //Width = videoInfo.Resolution.Width,
                                //Height = videoInfo.Resolution.Height,
                                Resolution = videoInfo.Resolution,
                                FrameRate = videoInfo.Fps,

                            };

                            //bool keepRatio = true;
                            //if (keepRatio)
                            //{
                            //    var srcSize = new Size(inputPars.Width, inputPars.Height);
                            //    var destSize = new Size(outputPars.Width, outputPars.Height);


                            //    var ratio = srcSize.Width / (double)srcSize.Height;
                            //    int destWidth = destSize.Width;
                            //    int destHeight = (int)(destWidth / ratio);
                            //    if (ratio < 1)
                            //    {
                            //        destHeight = destSize.Height;
                            //        destWidth = (int)(destHeight * ratio);
                            //    }
                            //    outputPars.Width = destWidth;
                            //    outputPars.Height = destHeight;
                            //}



                            var networkPars = new NetworkSettings
                            {
                                LocalAddr = videoAddr,
                                LocalPort = videoPort,
                                TransportMode = transportMode,

                                SSRC = videoChannelInfo.SSRC,
                            };

                            VideoReceiver = new VideoReceiver();


                            VideoReceiver.Setup(inputPars, outputPars, networkPars);
                            VideoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;
                        }

                    }


                    var audioChannelInfo =channelInfos.FirstOrDefault(c => c.MediaInfo is AudioChannelInfo);
                    if (audioChannelInfo != null)
                    {
                        var audioInfo = audioChannelInfo.MediaInfo as AudioChannelInfo;
                        if (audioInfo != null)
                        {

                            var audioAddr = audioChannelInfo.Address;
                            transportMode = audioChannelInfo.Transport;

                            if (transportMode == TransportMode.Tcp)
                            {
                                audioAddr = ServerAddr;
                            }

                            if (transportMode == TransportMode.Tcp)
                            {
                                if (audioChannelInfo.ClientsCount > 0)
                                {
                                    throw new Exception("Server is busy");
                                }
                            }

                            var audioPort = audioChannelInfo.Port;

                            AudioReceiver = new AudioReceiver();

                            var networkPars = new NetworkSettings
                            {
                                LocalAddr = audioAddr,
                                LocalPort = audioPort,
                                TransportMode = transportMode,

                                SSRC = audioChannelInfo.SSRC,

                            };

                            var audioDeviceId = "";
                            try
                            {
                                var devices = DirectSoundOut.Devices;
                                var device = devices.FirstOrDefault();
                                audioDeviceId = device?.Guid.ToString() ?? "";
                            }
                            catch(Exception ex)
                            {
                                logger.Error(ex);
                            }


                            var audioPars = new AudioEncoderSettings
                            {
                                SampleRate = audioInfo.SampleRate,
                                Channels = audioInfo.Channels,
                                Encoding = "ulaw",
                                DeviceId = audioDeviceId,//currentDirectSoundDeviceInfo?.Guid.ToString() ?? "",
                            };

                            AudioReceiver.Setup(audioPars, networkPars);
                        }

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

                    State = ClientState.Connected;

                    OnStateChanged(State);

                    while (running)
                    {

                        channel.PostMessage(new ServerRequest { Command = "Ping" });


                        syncEvent.WaitOne(1000);

                        //InternalCommand command = null;
                        //do
                        //{
                        //    command = DequeueCommand();
                        //    if (command != null)
                        //    {
                        //        ProcessCommand(command);
                        //    }

                        //} while (command != null);
                    }


                }
                finally
                {
                    running = false;

                    State = ClientState.Disconnected;
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


                State = ClientState.Faulted;
                OnStateChanged(State);

                //Close();
            }
            finally
            {
                Close();
            }
        }


        private D3DImageRenderer imageProvider = null;
        private VideoForm testForm = null;
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

                imageProvider?.Close();

               
                var texture = VideoReceiver?.sharedTexture;
                if (texture != null)
                {
                    imageProvider = new D3DImageRenderer(Dispatcher.CurrentDispatcher);

                    imageProvider.Setup(VideoReceiver.sharedTexture);
                    imageProvider.Start();

                    var video = testForm.userControl11;
                    video.DataContext = imageProvider;
                }

                testForm.FormClosed += TestForm_FormClosed;
            }


            testForm.Visible = true;
        }

        private void TestForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();

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

        private void Find()
        {
            var udpDiscoveryEndpoint = new UdpDiscoveryEndpoint();
            udpDiscoveryEndpoint.EndpointBehaviors.Add(new WcfDiscoveryAddressCustomEndpointBehavior());

            DiscoveryClient discoveryClient = new DiscoveryClient(udpDiscoveryEndpoint);

            var criteria = new FindCriteria(typeof(IScreenCastService));
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


        private void ProcessCommand(InternalCommand command)
        {
            // logger.Debug("ProcessInternalCommand(...)");

            if (!running)
            {
                return;
            }

            if (command == null)
            {
                return;
            }
        }


        public event Action<ClientState> StateChanged;
        private void OnStateChanged(ClientState state)
        {
            StateChanged?.Invoke(state);
        }

        public event Action Connected;
        private void OnConnected()
        {
            Connected?.Invoke();
        }
        public event Action Disconnected;
        private void OnDisconnected()
        {
            Disconnected?.Invoke();
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
        }



        private InternalCommand DequeueCommand()
        {
            if (!running)
            {
                return null;
            }

            return commandQueue.Dequeue();
        }

        private void EnqueueCommand(InternalCommand command)
        {
            if (!running)
            {
                return;
            }

            commandQueue.Enqueue(command);
            syncEvent.Set();
        }



        private void remoteDesktopTextBox_TextChanged(object sender, EventArgs e)
        {

        }


        //private void LoadMMDevicesCombo()
        //{
        //    IEnumerable<DirectSoundDeviceInfo> devices = null;
        //    try
        //    {
        //        devices = DirectSoundOut.Devices;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    audioRenderComboBox.DataSource = devices;
        //    audioRenderComboBox.DisplayMember = "Description";


        //}


    }
}
