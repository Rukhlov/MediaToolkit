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
using System.Threading.Tasks;

namespace MediaToolkit
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
        private IPEndPoint remoteEndpoint;


        public void Open(NetworkStreamingParams streamingParams)
        {
            
            var srcAddr = streamingParams.SrcAddr;
            var srcPort = streamingParams.SrcPort;

            var localIp = IPAddress.Any;
            if (string.IsNullOrEmpty(srcAddr))
            {
                if(IPAddress.TryParse(srcAddr, out IPAddress _localIp))
                {
                    localIp = _localIp;
                }
            }
            
            var localEndpoint = new IPEndPoint(localIp, srcPort);

            var remoteIp = IPAddress.Parse(streamingParams.DestAddr);
            remoteEndpoint = new IPEndPoint(remoteIp, streamingParams.DestPort);

            logger.Debug("RtpStreamer::Open(...) " + remoteEndpoint + " " + localEndpoint);


            var bytes = remoteIp.GetAddressBytes();
            bool isMulicast = (bytes[0] >= 224 && bytes[0] <= 239);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
           
            if (isMulicast)
            {
                var ttl = streamingParams.MulticastTimeToLive;
                socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, ttl);
            }

            socket.Bind(localEndpoint);
            logger.Info("Server started " + remoteEndpoint.ToString());
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
                        socket?.SendTo(data, 0, data.Length, SocketFlags.None, remoteEndpoint);
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

            logger.Debug("RtpStreamer::Close()");

            socket?.Close();

        }

    }

}
