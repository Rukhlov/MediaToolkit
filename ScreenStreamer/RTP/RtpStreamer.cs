using ScreenStreamer.RTP;
using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer
{


    public class RtpStreamer
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public RtpStreamer(RtpSession session)
        {
            this.session = session;

        }

        private RtpSession session;
        private Socket socket;
        private IPEndPoint endpoint;


        public void Open(string address, int port, int ttl = 10)
        {
            logger.Debug("RtpStreamer::Open(...) " + address + " " + port + " " + ttl);

            IPAddress addr = IPAddress.Parse(address);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);

            // m_Socket.SendBufferSize = int.MaxValue;//32 * 1024 * 1024;
            //m_Socket.ReceiveBufferSize = int.MaxValue;//32 * 1024 * 1024;

            endpoint = new IPEndPoint(addr, port);

            logger.Info("Server started " + endpoint.ToString());
        }

        public void Send(byte[] bytes, double sec)
        { 

            var packets = session.Packetize(bytes, sec);

            if (packets != null && packets.Count > 0)
            {
                int bytesSend = 0;
                foreach (var pkt in packets)
                {
                    try
                    {
                        //var data = pkt;//.GetBytes();
                        var data = pkt.GetBytes();
                        //logger.Debug("pkt" + pkt.Sequence);
                        socket?.SendTo(data, 0, data.Length, SocketFlags.None, endpoint);
                        //socket?.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, null, null);
                        bytesSend += data.Length;

                        // Statistic.RtpStats.Update(MediaTimer.GetRelativeTime(), rtp.Length);
                    }
                    catch (ObjectDisposedException) { }
                }
            }

        }

        public void Close()
        {

            logger.Debug("Close()");

            socket?.Close();

        }

    }

}
