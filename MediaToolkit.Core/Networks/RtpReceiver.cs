using NLog;
using MediaToolkit.RTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{
    public class RtpReceiver
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private RtpSession session = null;

        private Socket socket = null;
        public IPEndPoint RemoteEndpoint { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        private MulticastOption mcastOption = null;
        public RtpReceiver(RtpSession session)
        {
            this.session = session;
        }

        public void Open(string address, int port, int ttl = 10)
        {
            logger.Debug("RtpReceiver::Open(...) " + address + " " + port + " " + ttl);
            try
            {
                IPAddress addr = IPAddress.Parse(address);

                var bytes = addr.GetAddressBytes();
                bool isMulicast = (bytes[0] >= 224 && bytes[0] <= 239);

                RemoteEndpoint = new IPEndPoint(addr, port);
                LocalEndpoint = new IPEndPoint(IPAddress.Any, port);

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                socket.ReceiveBufferSize = 100 * 1024;//int.MaxValue;//32 * 1024 * 1024;

                socket.Bind(LocalEndpoint);

                if (isMulicast)
                {
                    mcastOption = new MulticastOption(RemoteEndpoint.Address, LocalEndpoint.Address);
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, mcastOption);
                }

                logger.Info("Client started: " + LocalEndpoint.ToString() + " " + RemoteEndpoint.ToString());
            }
            catch(Exception ex)
            {
                logger.Error(ex);

                CleanUp();
                throw;
            }
        }



        public Task Start()
        {
            if (running)
            {
                //...
            }

            return Task.Run(() => 
            {
                try
                {
                    running = true;

                    byte[] buf = new byte[16256];
                    while (running)
                    {
                        int bytesReceived = socket.Receive(buf, SocketFlags.None);

                        if (bytesReceived > 0)
                        {
                            //byte[] udp = new byte[bytesReceived];
                            //Array.Copy(buf, udp, bytesReceived);
                            //DataReceived(buf, bytesReceived);

                            RtpPacket rtpPacket = RtpPacket.Create(buf, bytesReceived);

                            OnRtpPacketReceived(rtpPacket);
                        }

                    }
                }
                catch (ObjectDisposedException)
                {
                    logger.Warn("RtpReceiver::ObjectDisposedException");
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    CleanUp();
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


        public void Close()
        {
            logger.Debug("RtpReceiver::Close()");


            if (socket != null)
            {
                if (mcastOption != null)
                {
                    socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, mcastOption);
                }

                CleanUp();
            }
        }

        private bool running = false;
        private void CleanUp()
        {
            running = false;
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

    }

}
