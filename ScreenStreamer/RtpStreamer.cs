using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                foreach (byte[] rtp in packets)
                {
                    try
                    {
                        //socket?.SendTo(rtp, 0, rtp.Length, SocketFlags.None, endpoint);
                        socket?.BeginSendTo(rtp, 0, rtp.Length, SocketFlags.None, endpoint, null, null);
                        bytesSend += rtp.Length;

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

    public abstract class RtpSession
    {

        public const int MTU = 1400;

        public uint SSRC { get; protected set; } = 0;
        public int CSRC { get; protected set; } = 0;

        public ushort Sequence { get; protected set; } = 0;
        public int PayloadType { get; protected set; } = 0;
        public int ClockRate { get; protected set; } = 90000;

        public abstract List<byte[]> Packetize(byte[] data, double sec);
    }

    public class PCMUSession : RtpSession
    {
        public PCMUSession()
        {
            SSRC = RngProvider.GetRandomNumber();
            Sequence = 0;
            PayloadType = 0;
            ClockRate = 8000;
        }


        public override List<byte[]> Packetize(byte[] data, double sec)
        {
            List<byte[]> packets = new List<byte[]>();
            uint timestamp = (uint)(sec * ClockRate);
            if (data !=null && data.Length > 0)
            {
                packets = CratePackets(data, timestamp);
            }

            return packets;
        }

        public List<byte[]> CratePackets(byte[] data, uint timestamp)
        {
            //Debug.WriteLine("CratePackets(...) " + data.Length + " " + timestamp);

            List<byte[]> rtpPackets = new List<byte[]>();

            int MaxPacketSize = 160;

            int offset = 0;
            int dataSize = data.Length;
            while (dataSize > 0)
            {
                int packetSize = dataSize;
                if (dataSize > MaxPacketSize)
                {
                    packetSize = MaxPacketSize;
                }

                byte[] rtpPacket = new byte[12 + packetSize];

                int version = 2;
                int padding = 0;
                int extension = 0;

                int marker = 0;

                RTPPacketUtil.WriteHeader(rtpPacket, version, padding, extension, CSRC, marker, PayloadType);
                RTPPacketUtil.WriteSequenceNumber(rtpPacket, Sequence);
                RTPPacketUtil.WriteTS(rtpPacket, timestamp);
                RTPPacketUtil.WriteSSRC(rtpPacket, SSRC);



                System.Array.Copy(data, offset, rtpPacket, 12, packetSize);
                offset += packetSize;
                dataSize -= packetSize;

                //Debug.WriteLine("offset " + offset + " " + timestamp);
                timestamp += (uint)packetSize; //40 * 8;


                rtpPackets.Add(rtpPacket);

                Sequence++;

            }



            return rtpPackets;

        }


    }
    public class H264Session : RtpSession
    {
        public H264Session()
        {
            SSRC = RngProvider.GetRandomNumber();
            Sequence = 0;
            PayloadType = 96;
        }

        public override List<byte[]> Packetize(byte[] data, double sec)
        {
            List<byte[]> packets = new List<byte[]>();
            uint timestamp = (uint)(sec * ClockRate);
            var nals = HandleH264AnnexbFrames(data);
            if (nals.Count > 0)
            {
                packets = CratePackets(nals, timestamp);
            }

            return packets;
        }

        private List<byte[]> HandleH264AnnexbFrames(byte[] frame)
        {// получаем буфер который нужно порезать на NALUnit-ы
            List<byte[]> nals = new List<byte[]>();

            int offset = 0;
            int pos1 = -1;
            int pos2 = -1;

            try
            {
                while (offset < frame.Length - 4)
                {

                    //if ((frame[offset] == 0 && frame[offset + 1] == 0 && frame[offset + 2] == 1))
                    //{
                    //    if (pos1 > 0)
                    //    {
                    //        pos2 = offset;
                    //        int nalSize = pos2 - pos1;
                    //        var nal = new byte[nalSize];
                    //        Array.Copy(frame, pos1, nal, 0, nal.Length);

                    //        nals.Add(nal);
                    //        pos2 = -1;
                    //    }

                    //    offset += 3;
                    //    pos1 = offset;
                    //    continue;
                    //}

                    if ((frame[offset] == 0 && frame[offset + 1] == 0 && frame[offset + 2] == 0 && frame[offset + 3] == 1))
                    {

                        if (pos1 > 0)
                        {
                            pos2 = offset;
                            int nalSize = pos2 - pos1;
                            var nal = new byte[nalSize];
                            Array.Copy(frame, pos1, nal, 0, nal.Length);

                            nals.Add(nal);
                            pos2 = -1;
                        }

                        offset += 4;
                        pos1 = offset;
                    }
                    else
                    {
                        offset += 4;
                        //offset++;
                    }
                }

                if (pos1 > 0 && pos2 == -1)
                {
                    pos2 = frame.Length;
                    int nalSize = pos2 - pos1;

                    var nal = new byte[nalSize];
                    Array.Copy(frame, pos1, nal, 0, nal.Length);

                    nals.Add(nal);
                }

            }
            catch (Exception ex)
            {

            }

            return nals;
        }

        public List<byte[]> CratePackets(List<byte[]> nalArray, uint timestamp)
        {

            List<byte[]> rtpPackets = new List<byte[]>();

            for (int x = 0; x < nalArray.Count; x++)
            {

                byte[] rawNal = nalArray[x];
                bool lastNal = (x == nalArray.Count - 1);

                bool fragmenting = (rawNal.Length > MTU);

                RtpHeader rtpHeader = new RtpHeader
                {
                    Marker = (lastNal ? 1 : 0),
                    Timestamp = timestamp,
                    Sequence = this.Sequence,
                    SSRC = this.SSRC,
                    PayloadType = this.PayloadType,

                };

                byte[] headerBytes = rtpHeader.Serialize();

                if (fragmenting)
                {
                    int dataRemaining = rawNal.Length;
                    int nalPointer = 0;

                    int startBit = 1;
                    int endBit = 0;

                    // consume first byte of the raw_nal. It is used in the FU header
                    byte firstByte = rawNal[0];
                    nalPointer++;
                    dataRemaining--;

                    while (dataRemaining > 0)
                    {
                        int payloadSize = Math.Min(MTU, dataRemaining);

                        if (dataRemaining - payloadSize == 0)
                        {
                            endBit = 1;
                        }

                        int offset = 0;
                        int rtpLength = headerBytes.Length + payloadSize + 2; //2 bytes for FU - A header;

                        byte[] rtpPacket = new byte[rtpLength];

                        // Update header sequence!
                        RTPPacketUtil.WriteSequenceNumber(headerBytes, Sequence);

                        System.Array.Copy(headerBytes, 0, rtpPacket, offset, headerBytes.Length);
                        offset += headerBytes.Length;

                        // Now append the Fragmentation Header (with Start and End marker) and part of the raw_nal
                        byte f_bit = 0;
                        byte nri = (byte)((firstByte >> 5) & 0x03); // Part of the 1st byte of the Raw NAL (NAL Reference ID)
                        byte type = 28; // FU-A Fragmentation

                        rtpPacket[offset] = (byte)((f_bit << 7) + (nri << 5) + type);
                        offset++;

                        rtpPacket[offset] = (byte)((startBit << 7) + (endBit << 6) + (0 << 5) + (firstByte & 0x1F));
                        offset++;

                        System.Array.Copy(rawNal, nalPointer, rtpPacket, offset, payloadSize);

                        nalPointer +=  payloadSize;
                        dataRemaining -=  payloadSize;

                        rtpPackets.Add(rtpPacket);

                        this.Sequence++;


                        startBit = 0;
                    }
                }
                else
                {
                    int offset = 0;

                    int rtpLength = headerBytes.Length + rawNal.Length;
                    byte[] rtpPacket = new byte[rtpLength];
                    System.Array.Copy(headerBytes, 0, rtpPacket, offset, headerBytes.Length);

                    offset += headerBytes.Length;

                    // Put the whole NAL into one RTP packet.
                    // Note some receivers will have maximum buffers and be unable to handle large RTP packets.
                    // Also with RTP over RTSP there is a limit of 65535 bytes for the RTP packet.

                    // Now append the raw NAL
                    System.Array.Copy(rawNal, 0, rtpPacket, offset, rawNal.Length);

                    rtpPackets.Add(rtpPacket);

                    this.Sequence++;
                }

            }

            return rtpPackets;

        }

    }

    class RtpHeader
    {

        //First byte
        public int Version = 2;
        public int Padding = 0;
        public int Extension = 0;
        public int CSRCCount = 0;

        //Second byte
        public int Marker = 0;
        public int PayloadType = 0;

        //...
        public ushort Sequence = 0;
        public uint Timestamp;
        public uint SSRC = 0;
        //public uint[] CSRC;

        public byte[] Serialize()
        {
            // 12 is header size when there are no CSRCs or extensions
            byte[] rtpHeader = new byte[12];

            // RTP Packet Header
            // 0 - Version, P, X, CC, M, PT and Sequence Number
            //32 - Timestamp. 
            //64 - SSRC
            //96 - CSRCs (optional)
            //nn - Extension ID and Length
            //nn - Extension header

            RTPPacketUtil.WriteHeader(rtpHeader, Version, Padding, Extension, CSRCCount, Marker, PayloadType);
            RTPPacketUtil.WriteSequenceNumber(rtpHeader, Sequence);
            RTPPacketUtil.WriteTS(rtpHeader, Timestamp);
            RTPPacketUtil.WriteSSRC(rtpHeader, SSRC);

            return rtpHeader;
        }
    }

    public static class RTPPacketUtil
    {

        public static void WriteHeader(byte[] packet, int version, int padding, int extension, int csrc_count, int marker, int payload_type)
        {
            packet[0] = (byte)((version << 6) | (padding << 5) | (extension << 4) | csrc_count);
            packet[1] = (byte)((marker << 7) | (payload_type & 0x7F));
        }

        public static void WriteSequenceNumber(byte[] packet, uint sequence)
        {
            packet[2] = ((byte)((sequence >> 8) & 0xFF));
            packet[3] = ((byte)((sequence >> 0) & 0xFF));
        }

        public static void WriteTS(byte[] packet, uint ts)
        {
            packet[4] = ((byte)((ts >> 24) & 0xFF));
            packet[5] = ((byte)((ts >> 16) & 0xFF));
            packet[6] = ((byte)((ts >> 8) & 0xFF));
            packet[7] = ((byte)((ts >> 0) & 0xFF));
        }

        public static void WriteSSRC(byte[] packet, uint ssrc)
        {
            packet[8] = ((byte)((ssrc >> 24) & 0xFF));
            packet[9] = ((byte)((ssrc >> 16) & 0xFF));
            packet[10] = ((byte)((ssrc >> 8) & 0xFF));
            packet[11] = ((byte)((ssrc >> 0) & 0xFF));
        }
    }
}
