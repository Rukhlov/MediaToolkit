using MediaToolkit;
using MediaToolkit.Common;
using MediaToolkit.UI;
using MediaToolkit.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    class RemoteDesktopClient : IRemoteDesktopClient
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ScreenReceiver VideoReceiver { get; private set; }
        public InputManager InputManager { get; private set; } 


        public string ClientId { get; private set; }
        public string ServerId { get; private set; }
        public string ServerName { get; private set; }
        public string ServerAddr { get; private set; }
        public ClientState State { get; private set; }

        private ChannelFactory<IRemoteDesktopService> factory = null;


        public void Connect(string _addr)
        {

            logger.Debug("RemoteDesktopClient::Connect(...) " + _addr);

            this.ServerAddr = _addr;
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
            var address = "net.tcp://" + ServerAddr + "/RemoteDesktop";
            try
            {


                var uri = new Uri(address);
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

                factory = new ChannelFactory<IRemoteDesktopService>(binding, new EndpointAddress(uri));
                var channel = factory.CreateChannel();

                try
                {
                    this.ClientId = RngProvider.GetRandomNumber().ToString();

                    var connectReq = new RemoteDesktopRequest
                    {
                        SenderId = ClientId,
                    };

                    var connectionResponse = channel.Connect(connectReq);
                    if (!connectionResponse.IsSuccess)
                    {
                        logger.Error("connectionResponse " + connectionResponse.FaultCode);
                        return;
                    }

                    this.ServerId = connectionResponse.ServerId;
                    this.ServerName = connectionResponse.HostName;

                    var screens = connectionResponse.Screens;
                    var primaryScreen = screens.FirstOrDefault(s => s.IsPrimary);

                    var startRequest = new StartSessionRequest
                    {
                        SenderId = this.ClientId,

                        SrcRect = primaryScreen.Bounds,
                        DestAddr = "", //"192.168.1.135",//localAddr.Address.ToString(), //localAddr.ToString(),
                        DestPort = 1234,
                        DstSize = new Size(1920, 1080),
                        EnableInputSimulator = true,

                    };


                    var startResponse = channel.Start(startRequest);
                    if (!startResponse.IsSuccess)
                    {
                        logger.Error("startResponse " + startResponse.FaultCode);
                        return;
                    }

                    var inputPars = new VideoEncodingParams
                    {
                        Width = startRequest.DstSize.Width,
                        Height = startRequest.DstSize.Height,

                        //Width = 640,//2560,
                        //Height = 480,//1440,
                        FrameRate = 30,
                    };

                    var outputPars = new VideoEncodingParams
                    {
                        //Width = 640,//2560,
                        //Height = 480,//1440,
                        Width = startRequest.DstSize.Width,
                        Height = startRequest.DstSize.Height,

                        FrameRate = 30,
                    };
                    var transport = TransportMode.Udp;

                    var networkPars = new NetworkStreamingParams
                    {
                        LocalAddr = ServerAddr,
                        LocalPort = 1234,
                        TransportMode = transport,
                    };

                    this.Play(inputPars, outputPars, networkPars);

                    InputManager = new InputManager();

                    InputManager.Start(ServerAddr, 8888);
                    running = true;

                    State = ClientState.Connected;

                    OnStateChanged(State);

                    while (running)
                    {

                        channel.PostMessage("Ping", null);


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
                    catch(Exception ex)
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

                Close();
            }
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



        internal void Play(VideoEncodingParams inputPars, VideoEncodingParams outputPars, NetworkStreamingParams networkPars)
        {
            logger.Debug("RemoteDesktopClient::Play(...)");
            VideoReceiver = new ScreenReceiver();

            VideoReceiver.Setup(inputPars, outputPars, networkPars);
            VideoReceiver.UpdateBuffer += VideoReceiver_UpdateBuffer;

            VideoReceiver.Play();
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
        public event Action UpdateBuffer;
        private void VideoReceiver_UpdateBuffer()
        {
            UpdateBuffer?.Invoke();
        }

        internal void Stop()
        {
            logger.Debug("RemoteDesktopClient::Stop()");

            if (VideoReceiver != null)
            {
                VideoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
                VideoReceiver.Stop();
            }
        }

        public void Disconnect()
        {
            logger.Debug("RemoteDesktopClient::Disconnect()");

            running = false;

            if (factory != null)
            {
                var channel = factory.CreateChannel();
                try
                {
                    var request = new RemoteDesktopRequest
                    {
                        SenderId = ClientId,
                    };

                    var resp = channel.Stop(request);
                    channel.Disconnect(request);

                    CloseReceiver();

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

        private void CloseReceiver()
        {
            if (InputManager != null)
            {
                InputManager.Stop();
            }

            Stop();
        }

        public void Close()
        {

            if (VideoReceiver != null)
            {
                VideoReceiver.UpdateBuffer -= VideoReceiver_UpdateBuffer;
                VideoReceiver.Stop();
                VideoReceiver = null;
            }

            if (InputManager != null)
            {
                InputManager.Stop();
                InputManager = null;

            }

            if (factory != null)
            {
                factory.Abort();
                factory = null;
            }
        }

        public object SendMessage(string id, object[] pars)
        {
            logger.Debug("IRemoteDesktopClient::SendMessage(...) " + id);

            return id + "OK!";
        }

        public void PostMessage(ServerRequest request)
        {
            var id = request.SenderId;
            var cmd = request.Command;
            var args = request.Args;

            logger.Debug("IRemoteDesktopClient::PostMessage(...) " + id + " " + cmd);

            var command = new InternalCommand
            {
                command = cmd,
                args = args,
            };

            EnqueueCommand(command);

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
    }



    public class InputManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private AutoResetEvent waitEvent = new AutoResetEvent(false);
        private object syncRoot = new object();
        private Socket socket = null;

        private string message = "";
        private bool running = false;

        public void Start(string address, int port)
        {
            logger.Debug("InputManager::Start(...) " + address + " " + port);
            Task.Run(() =>
            {

                logger.Debug("InputManagerTask BEGIN");

                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var ipaddr = IPAddress.Parse(address);
                    socket.Connect(ipaddr, port);
                    running = true;


                    while (running)
                    {

                        try
                        {
                            waitEvent.WaitOne();

                            lock (syncRoot)
                            {
                                if (running)
                                {
                                    byte[] data = Encoding.ASCII.GetBytes(message);
                                    socket.Send(data);
                                }

                                //client.Send(message);
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

                }
                finally
                {
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }

                    running = false;
                }

                logger.Debug("InputManagerTask END");
            });
        }

        public void ProcessMessage(string _message)
        {
            if (!running)
            {
                return;
            }

            lock (syncRoot)
            {
                this.message = _message;
            }

            waitEvent.Set();

        }
        public void Stop()
        {
            logger.Debug("InputManager::Stop()");

            running = false;
            waitEvent.Set();

        }
    }


    public enum ClientState
    {
        Created,
        Connected,
        Disconnected,
        Faulted,
    }
}
