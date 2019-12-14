using MediaToolkit.Core;
using MediaToolkit.RTP;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaToolkit
{


    public class RtpUdpSender : IRtpSender
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public RtpUdpSender(RtpSession session)
        {
            this.session = session;

        }

        private RtpSession session;
        private Socket socket;

        public int ClientsCount { get; private set; } = 0;

        public IPEndPoint RemoteEndpoint { get; private set; }
        public IPEndPoint LocalEndpoint { get; private set; }

        public void Setup(NetworkSettings streamingParams)
        {
            logger.Debug("RtpUdpSender::Setup(...)");


            var localAddr = streamingParams.LocalAddr;
            var localPort = 0;//streamingParams.LocalPort;

            var localIp = IPAddress.Any;
            if (string.IsNullOrEmpty(localAddr))
            {
                if (IPAddress.TryParse(localAddr, out IPAddress _localIp))
                {
                    localIp = _localIp;
                }
            }

            var localEndpoint = new IPEndPoint(localIp, localPort);

            var remoteIp = IPAddress.Parse(streamingParams.RemoteAddr);
            RemoteEndpoint = new IPEndPoint(remoteIp, streamingParams.RemotePort);

            logger.Debug("RtpStreamer::Open(...) " + RemoteEndpoint + " " + localEndpoint);

            bool isMulicast = NetTools.IsMulticastIpAddr(remoteIp);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (isMulicast)
            {
                var ttl = streamingParams.MulticastTimeToLive;
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
            }

            socket.Bind(localEndpoint);

        }

        public void Start()
        {
            logger.Debug("RtpUdpSender::Start()");
            try
            {

                running = true;

                logger.Info("Server started " + RemoteEndpoint.ToString());


                Task.Run(() =>
                {

                    while (running)
                    {
                        //if (!syncEvent.WaitOne(1000))
                        //{
                        //    continue;
                        //}

                        syncEvent.WaitOne(1000);

                        SendPackets();

                        if (!running)
                        {
                            break;
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Close();
            }
        }


        private object locker = new object();
        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private Queue<RtpPacket> packetQueue = new Queue<RtpPacket>();

        public void Push(byte[] bytes, double sec)
        {
            if (!running)
            {
                return;
            }

            if (packetQueue.Count > 1024)
            {
                packetQueue.Clear();
                logger.Warn("Buffer full drop frames...");
            }

            var packets = session.Packetize(bytes, sec);

            if (packets != null && packets.Count > 0)
            {
                foreach (var pkt in packets)
                {
                    lock (locker)
                    {
                        packetQueue.Enqueue(pkt.Clone());
                    }
                    //packetBuffer.Add(pkt);
                }
            }

            syncEvent.Set();
        }

        private void SendPackets()
        {
            if (!running)
            {
                return;
            }

            int bytesSend = 0;

            while (packetQueue.Count > 0)
            {
                if (!running)
                {
                    break;
                }

                RtpPacket pkt = null;
                lock (locker)
                {
                    pkt = packetQueue.Dequeue();
                }

                if (pkt != null)
                {
                    try
                    {
                        //var data = pkt;//.GetBytes();
                        var data = pkt.GetBytes();
                        //logger.Debug("pkt" + pkt.Sequence);
                        socket?.SendTo(data, 0, data.Length, SocketFlags.None, RemoteEndpoint);
                        //socket?.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, null, null);
                        bytesSend += data.Length;

                        // Statistic.RtpStats.Update(MediaTimer.GetRelativeTime(), rtp.Length);
                    }
                    catch (ObjectDisposedException) { }
                }
                
            }

        }

        public void Send(byte[] bytes, double sec)
        {
            if (!running)
            {
                return;
            }

            var packets = session.Packetize(bytes, sec);

            if (packets != null && packets.Count > 0)
            {
                int bytesSend = 0;
                foreach (var pkt in packets)
                {
                    if (!running)
                    {
                        break;
                    }

                    try
                    {
                        //var data = pkt;//.GetBytes();
                        var data = pkt.GetBytes();
                        //logger.Debug("pkt" + pkt.Sequence);
                        socket?.SendTo(data, 0, data.Length, SocketFlags.None, RemoteEndpoint);
                        //socket?.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, null, null);
                        bytesSend += data.Length;

                        // Statistic.RtpStats.Update(MediaTimer.GetRelativeTime(), rtp.Length);
                    }
                    catch (ObjectDisposedException) { }
                }
            }

        }

        private volatile bool running = false;
        public void Close()
        {

            logger.Debug("RtpUdpSender::Close()");

            running = false;
            socket?.Close();

        }

    }

}
