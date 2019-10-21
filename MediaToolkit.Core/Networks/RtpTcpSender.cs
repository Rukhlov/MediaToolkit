using MediaToolkit.Common;
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


    public class RtpTcpSender

    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public RtpTcpSender(RtpSession session)
        {
            this.session = session;

        }

        private RtpSession session;
        private Socket socket;
        private IPEndPoint remoteEndpoint;


        public void Start(NetworkStreamingParams streamingParams)
        {
            try
            {

                Task.Run(() =>
                {
                    logger.Debug("RtpStreamer::Open(...)");

                    var localAddr = streamingParams.LocalAddr;
                    var localPort = streamingParams.LocalPort;

                    var localIp = IPAddress.Any;
                    if (!string.IsNullOrEmpty(localAddr))
                    {
                        if (IPAddress.TryParse(localAddr, out IPAddress _localIp))
                        {
                            localIp = _localIp;
                        }
                    }

                    var localEndpoint = new IPEndPoint(localIp, localPort);

                    var remoteIp = IPAddress.Parse(streamingParams.RemoteAddr);
                    remoteEndpoint = new IPEndPoint(remoteIp, streamingParams.RemotePort);

                    logger.Debug("RtpStreamer::Open(...) " + remoteEndpoint + " " + localEndpoint);


                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.Bind(localEndpoint);
                    socket.Listen(10);

                    running = true;

                    while (running)
                    {

                        try
                        {
                            logger.Info("Waiting for a connection " + localEndpoint.ToString());

                            var _socket = socket.Accept();

                            if (!running)
                            {
                                break;
                            }

                            var remotePoint = _socket.RemoteEndPoint;

                            logger.Info("Server started " + remotePoint.ToString());

                            while (running)
                            {
                                //if (!syncEvent.WaitOne(1000))
                                //{
                                //    continue;
                                //}

                                syncEvent.WaitOne(1000);

                                SendPackets(_socket);

                                if (!running)
                                {
                                    break;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            logger.Error(ex);
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

        private void SendPackets(Socket _socket)
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

                        // RFC 2326 10.12 Embedded (Interleaved) Binary Data

                        /*
                        *   S->C: $\000{2 byte length}{"length" bytes data, w/RTP header}
                            S->C: $\000{2 byte length}{"length" bytes data, w/RTP header}
                            S->C: $\001{2 byte length}{"length" bytes  RTCP packet}
                        */

                        const byte magicSymbol = (byte)'$';
                        byte channelId = 0x42;
                        var data = pkt.GetBytes();

                        ushort dataLength = (ushort)data.Length;

                        int frameSize = data.Length + 4; // data + header

                        byte[] frameBytes = new byte[frameSize];

                        int offset = 0;
                        frameBytes[offset] = magicSymbol;
                        offset++;

                        frameBytes[offset] = channelId;
                        offset++;

                        BigEndian.WriteUInt16(frameBytes, offset, dataLength);
                        offset += 2;

                        Array.Copy(data, 0, frameBytes, offset, data.Length);
                        offset += data.Length;

                        _socket?.Send(frameBytes, 0, frameBytes.Length, SocketFlags.None);

                        bytesSend += frameBytes.Length;

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
                        socket?.SendTo(data, 0, data.Length, SocketFlags.None, remoteEndpoint);
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

            logger.Debug("RtpStreamer::Close()");

            running = false;
            socket?.Close();

        }

    }

}