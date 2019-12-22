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

            syncContext = SynchronizationContext.Current;

            controlPanel.Visible = false;

            imageProvider = new D3DImageProvider2();

            imageProvider.RenderStarted += ImageProvider_RenderStarted;
            imageProvider.RenderStopped += ImageProvider_RenderStopped;

            UpdateControls();
        }

        private D3DImageProvider2 imageProvider = null;

        private volatile ClientState state = ClientState.Disconnected;
        public ClientState State => state;

        public event Action Connected;
        public event Action<object> Disconnected;

        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode Code => errorCode;

        private readonly SynchronizationContext syncContext = null;
        private AutoResetEvent syncEvent = new AutoResetEvent(false);
        private volatile bool cancelled = false;

        public VideoReceiver VideoReceiver { get; private set; }
        public AudioReceiver AudioReceiver { get; private set; }


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }
        public string ServerName { get; private set; }
        public string ServerAddr { get; private set; }
        public int ServerPort { get; private set; }

        private ChannelFactory<IScreenCastService> factory = null;

        private void connectButton_Click(object sender, EventArgs e)
        {
            logger.Debug("connectButton_Click()");

            try
            {
                if (state == ClientState.Disconnected)
                {
                    var addrStr = hostAddressTextBox.Text;

                    var uri = new Uri("net.tcp://" + addrStr);

                    logger.Info("Connect to: " + uri.ToString());

                    var host = uri.Host;
                    var port = uri.Port;

                    Connect(host, port);
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }

        private void Factory_Closed(object sender, EventArgs e)
        {
            logger.Debug("Factory_Closed()");
        }


        private void ImageProvider_RenderStarted()
        {
            logger.Debug("ImageProvider_RenderStarted()");
        }

        private void ImageProvider_RenderStopped(object obj)
        {
            logger.Debug("ImageProvider_RenderStopped(...) ");
        }

        private Task mainTask = null;
        public void Connect(string addr, int port)
        {
            logger.Debug("RemoteDesktopClient::Connecting(...) " + addr + " " + port);

            state = ClientState.Connecting;

            cancelled = false;
            errorCode = ErrorCode.Ok;

            this.ServerAddr = addr;
            this.ServerPort = port;

            this.ClientId = RngProvider.GetRandomNumber().ToString();

            var address = "net.tcp://" + ServerAddr + "/ScreenCaster";
            if (this.ServerPort > 0)
            {
                address = "net.tcp://" + ServerAddr + ":" + ServerPort + "/ScreenCaster";
            }

            UpdateControls();

            mainTask = Task.Run(() =>
            {
                int maxTryCount = 3;
                int tryCount = 0;

                while (tryCount <= maxTryCount && !cancelled)
                {
                    logger.Debug("Connecting count: " + tryCount);
                    //errorMessage = "";
                    errorCode = ErrorCode.Ok;

                    try
                    {
                        var uri = new Uri(address);

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
                        factory.Closed += Factory_Closed;

                        var channel = factory.CreateChannel();

                        try
                        {
                            var channelInfos = channel.GetChannelInfos();

                            state = ClientState.Connected;
                            UpdateControls();
                            Connected?.Invoke();

                            if (channelInfos == null)
                            {
                                errorCode = ErrorCode.NotСonfigured;
                                throw new Exception("Server not configured");
                            }

                            var videoChannelInfo = channelInfos.FirstOrDefault(c => c.MediaInfo is VideoChannelInfo);
                            if (videoChannelInfo != null)
                            {
                                if (videoChannelInfo.Transport == TransportMode.Tcp && videoChannelInfo.ClientsCount > 0)
                                {
                                    errorCode = ErrorCode.ServerIsBusy;
                                    throw new Exception("Server is busy");
                                }
                                SetupVideo(videoChannelInfo);
                            }

                            var audioChannelInfo = channelInfos.FirstOrDefault(c => c.MediaInfo is AudioChannelInfo);
                            if (audioChannelInfo != null)
                            {
                                if (audioChannelInfo.Transport == TransportMode.Tcp && videoChannelInfo.ClientsCount > 0)
                                {
                                    errorCode = ErrorCode.ServerIsBusy;
                                    throw new Exception("Server is busy");
                                }

                                SetupAudio(audioChannelInfo);
                            }

                            if (VideoReceiver != null)
                            {
                                VideoReceiver.Play();
                                imageProvider.Start();
                            }

                            if (AudioReceiver != null)
                            {
                                AudioReceiver.Play();
                            }

                            channel.PostMessage(new ServerRequest { Command = "Ping" });
                            tryCount = 0;
   
                            state = ClientState.Running;
                            UpdateControls();

                            while (state == ClientState.Running)
                            {
                                try
                                {
                                    channel.PostMessage(new ServerRequest { Command = "Ping" });
                                    if (imageProvider.ErrorCode != 0)
                                    {
                                        logger.Debug("imageProvider.ErrorCode: " + imageProvider.ErrorCode);
                                        //...
                                    }

                                    syncEvent.WaitOne(1000);
                                }
                                catch (Exception ex)
                                {
                                    state = ClientState.Interrupted;
                                    errorCode = ErrorCode.Interrupted;
                                }
                            }
                        }
                        finally
                        {
                            CloseChannel(channel);
                        }
                    }
                    catch (EndpointNotFoundException ex)
                    {
                        errorCode = ErrorCode.EndpointNotFound;
                        logger.Error(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);

                        if (errorCode == ErrorCode.Ok)
                        {
                            errorCode = ErrorCode.UnknownError;
                        }
                    }
                    finally
                    {
                        Close();
                    }

                    if (!cancelled)
                    {

                        if (errorCode != ErrorCode.Ok)
                        {
                            UpdateControls();

                            tryCount++;
                            SetStatus("Connection error try next " + tryCount + " of " + maxTryCount);
                        }

                    }
                }

                cancelled = false;

                state = ClientState.Disconnected;
                UpdateControls();

                Disconnected?.Invoke(null);
            });
        }


        public void Disconnect()
        {
            state = ClientState.Disconnecting;
            cancelled = true;

            UpdateControls();

        }


        private void SetupAudio(ScreencastChannelInfo audioChannelInfo)
        {
            logger.Debug("SetupAudio(...)");

            var audioInfo = audioChannelInfo.MediaInfo as AudioChannelInfo;
            if (audioInfo == null)
            {
                return;
            }

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


        private void SetupVideo(ScreencastChannelInfo videoChannelInfo)
        {
            logger.Debug("SetupVideo(...)");

            var videoInfo = videoChannelInfo.MediaInfo as VideoChannelInfo;
            if (videoInfo == null)
            {
                return;
            }


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
                //Resolution = new System.Drawing.Size(1920, 1080);

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
            VideoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;

            VideoReceiver.Setup(inputPars, outputPars, networkPars);

            syncContext.Send(_ =>
            {
                imageProvider.Setup(VideoReceiver.sharedTexture);

                //wpfRemoteControl.DataContext = imageProvider;

            }, null);
        }

        private void VideoReceiver_UpdateBuffer()
        {
            imageProvider?.Update();
        }


        public void Close()
        {
            if (imageProvider != null)
            {
                imageProvider.Close();

            }

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
                factory.Closed -= Factory_Closed;

                factory.Abort();
                factory = null;
            }

            //state = ClientState.Disconnected;
        }



        private DiscoveryClient discoveryClient = null;
        private bool finding = false;

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

        private void UpdateControls()
        {
            logger.Debug("UpdateControls(...) " + state);

            syncContext.Send(_ =>
            {
                _UpdateControls();

            }, null);

        }

        private void SetStatus(string text)
        {
            syncContext.Send(_ =>
            {
                labelStatus.Text = text;
            }, null);
        }

        private void _UpdateControls()
        {

            bool isDisconnected = (state == ClientState.Disconnected);
            bool isConnected = (state == ClientState.Connected);

            //connectButton.Enabled = !isConnected;
            if (isDisconnected)
            {
                connectButton.Text = "_Connect";

                wpfRemoteControl.DataContext = null;

                string _statusStr = "_Not Connected";

                if (errorCode!= ErrorCode.Ok)
                {
                    _statusStr = errorCode.ToString();

                    //Server Disconnected
                    //_Connection Error
                }

                labelStatus.Text = _statusStr;
            }
            else
            {
                if(state == ClientState.Running)
                {
                    connectButton.Text = "_Disconnect";
                    labelStatus.Text = "_Connected";

                    wpfRemoteControl.DataContext = imageProvider;
                }
                else if(state == ClientState.Connecting)
                {
                    labelStatus.Text = "_Connecting...";

                    //controlPanel.Enabled = false;
                    this.hostAddressTextBox.Text = ServerAddr + ":" + ServerPort;

                    connectButton.Text = "_Cancel";
                }
                else if (state == ClientState.Disconnecting)
                {
                    //labelStatus.Text = "_Disconnecting...";
                }
                else
                {
                    //connectButton.Text = "_Cancel";
                }

            }

            if (cancelled)
            {
                labelStatus.Text = "_Cancelling...";

            }

            //controlPanel.Enabled = true;

            findServiceButton.Enabled = isDisconnected;
            hostsComboBox.Enabled = isDisconnected;
            hostAddressTextBox.Enabled = isDisconnected;


            showDetailsButton.Text = controlPanel.Visible ? "<<" : ">>";

            //labelInfo.Text = errorMessage;
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



        private void CloseChannel(IScreenCastService channel)
        {
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

    class ComboBoxItem
    {
        public string Name { get; set; }
        public object Tag { get; set; }
    }



}
