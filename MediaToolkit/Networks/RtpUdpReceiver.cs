
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaToolkit.Core;
using MediaToolkit.SharedTypes;
using System.Diagnostics;
using MediaToolkit.Logging;

namespace MediaToolkit.Networks
{
    public class RtpUdpReceiver :IRtpReceiver
    {
        //private static Logger logger = LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Networks");

        private RtpSession session = null;

        private Socket socket = null;
        public IPEndPoint RemoteEndpoint { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        private volatile ErrorCode errorCode = ErrorCode.Ok;
        public ErrorCode ErrorCode => errorCode;

        private volatile ReceiverState state = ReceiverState.Closed;
        public ReceiverState State => state;

        private MulticastOption mcastOption = null;
        public RtpUdpReceiver(RtpSession session)
        {
            this.session = session;
        }

        public void Open(NetworkSettings settings) 
        {
            if (state!= ReceiverState.Closed)
            {
                throw new InvalidOperationException("Invalid receiver state " + state);
            }

            var address = settings.LocalAddr;
            var port = settings.LocalPort;
            var ttl = settings.MulticastTimeToLive;

            logger.Debug("RtpUdpReceiver::Open(...) " + address + " " + port + " " + ttl);

            try
            {
                IPAddress localAddress = IPAddress.Parse(address);

                bool isMulicast = Utils.NetTools.IsMulticastIpAddr(localAddress);


                RemoteEndpoint = new IPEndPoint(localAddress, port);
                LocalEndpoint = new IPEndPoint(IPAddress.Any, port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.ReceiveBufferSize = 100 * 1024;//int.MaxValue;//32 * 1024 * 1024;

                socket.Bind(LocalEndpoint);

                if (isMulicast)
                {

                    mcastOption = new MulticastOption(RemoteEndpoint.Address, LocalEndpoint.Address);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);

                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
                }

                state = ReceiverState.Initialized;

                logger.Info("Client started: " + LocalEndpoint.ToString() + " " + RemoteEndpoint.ToString());
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                Close();
                throw;
            }
        }

        public Task Start()
        {
            logger.Debug("RtpUdpReceiver::Start()");

            if (state != ReceiverState.Initialized)
            {
                throw new InvalidOperationException("Invalid state " + state);
            }

            return Task.Run(() =>
            {
                logger.Debug("UdpReceiver thread started...");
                state = ReceiverState.Running;
                try
                {
                    byte[] buf = new byte[16256];
                    while (state == ReceiverState.Running)
                    {
                        int bytesReceived = socket.Receive(buf, SocketFlags.None);

                        if (bytesReceived > 0)
                        {
                            //byte[] udp = new byte[bytesReceived];
                            //Array.Copy(buf, udp, bytesReceived);
                            //DataReceived(buf, bytesReceived);

                            RtpPacket rtpPacket = RtpPacket.Create(buf, bytesReceived, session);
                            if (rtpPacket != null)
                            {
                                OnRtpPacketReceived(rtpPacket);
                            }
                            
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    logger.Warn("UdpReceiver::ObjectDisposedException");
                }
                catch (Exception ex)
                {                   
                    logger.Error(ex);               
                }
                finally
                {
                    Close();

                    logger.Debug("UdpReceiver thread stopped...");
                }
            });

        }

        public event Action<RtpPacket> RtpPacketReceived;
        private void OnRtpPacketReceived(RtpPacket pkt)
        {
            RtpPacketReceived?.Invoke(pkt);
        }

        public event Action<byte[], int> DataReceived;
        private void OnDataReceived(byte[] buf, int len)
        {
            DataReceived?.Invoke(buf, len);
        }

        public void Stop()
        {
            logger.Debug("RtpReceiver::Stop()");
            if(state == ReceiverState.Running)
            {
                state = ReceiverState.Closing;
            }
        }

        public void Close()
        {
            if (socket != null)
            {
                if (mcastOption != null)
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, mcastOption);
                }

                socket.Close();
                socket = null;
            }

            state = ReceiverState.Closed;
        }

    }



}
