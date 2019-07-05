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

        private uint ssrc = 0;
        private ushort sequence = 0;

        public void Open(string address, int port, int ttl = 10)
        {
            logger.Debug("Open(...)");

            sequence = 0;
            ssrc = RngProvider.GetRandomNumber();

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
                        // socket?.SendTo(rtp, 0, rtp.Length, SocketFlags.None, endpoint);
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

            while (offset < frame.Length)
            {
                if (frame[offset] == 0 &&
                    frame[offset + 1] == 0 &&
                    frame[offset + 2] == 0 &&
                    frame[offset + 3] == 1)
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

            return nals;
        }


        public List<byte[]> CratePackets(List<byte[]> nal_array, uint timestamp)
        {

            List<byte[]> rtp_packets = new List<byte[]>();

            for (int x = 0; x < nal_array.Count; x++)
            {

                byte[] raw_nal = nal_array[x];
                Boolean last_nal = false;
                if (x == nal_array.Count - 1)
                {
                    last_nal = true; // last NAL in our nal_array
                }

                bool fragmenting = false;

                if (raw_nal.Length > MTU)
                {
                    fragmenting = true;
                }

                if (fragmenting == false)
                {
                    // Put the whole NAL into one RTP packet.
                    // Note some receivers will have maximum buffers and be unable to handle large RTP packets.
                    // Also with RTP over RTSP there is a limit of 65535 bytes for the RTP packet.


                    byte[] rtp_packet = new byte[12 + raw_nal.Length]; // 12 is header size when there are no CSRCs or extensions
                                                                       // Create an single RTP fragment

                    // RTP Packet Header
                    // 0 - Version, P, X, CC, M, PT and Sequence Number
                    //32 - Timestamp. H264 uses a 90kHz clock
                    //64 - SSRC
                    //96 - CSRCs (optional)
                    //nn - Extension ID and Length
                    //nn - Extension header

                    int version = 2;
                    int padding = 0;
                    int extension = 0;

                    int marker = (last_nal == true ? 1 : 0); // set to 1 if the last NAL in the array


                    RTPPacketUtil.WriteHeader(rtp_packet, version, padding, extension, CSRC, marker, PayloadType);


                    RTPPacketUtil.WriteSequenceNumber(rtp_packet, Sequence);

                    RTPPacketUtil.WriteTS(rtp_packet, timestamp);

                    //UInt32 empty_ssrc = 0;
                    RTPPacketUtil.WriteSSRC(rtp_packet, SSRC);

                    // Now append the raw NAL
                    System.Array.Copy(raw_nal, 0, rtp_packet, 12, raw_nal.Length);

                    rtp_packets.Add(rtp_packet);

                    Sequence++;


                }
                else
                {
                    int data_remaining = raw_nal.Length;
                    int nal_pointer = 0;
                    int start_bit = 1;
                    int end_bit = 0;

                    // consume first byte of the raw_nal. It is used in the FU header
                    byte first_byte = raw_nal[0];
                    nal_pointer++;
                    data_remaining--;

                    while (data_remaining > 0)
                    {
                        int payload_size = Math.Min(MTU, data_remaining);
                        if (data_remaining - payload_size == 0) end_bit = 1;

                        byte[] rtp_packet = new byte[12 + 2 + payload_size]; // 12 is header size. 2 bytes for FU-A header. Then payload

                        // RTP Packet Header
                        // 0 - Version, P, X, CC, M, PT and Sequence Number
                        //32 - Timestamp. H264 uses a 90kHz clock
                        //64 - SSRC
                        //96 - CSRCs (optional)
                        //nn - Extension ID and Length
                        //nn - Extension header

                        int rtp_version = 2;
                        int rtp_padding = 0;
                        int rtp_extension = 0;
                        int rtp_csrc_count = 0;
                        int rtp_marker = (last_nal == true ? 1 : 0); // Marker set to 1 on last packet


                        RTPPacketUtil.WriteHeader(rtp_packet, rtp_version, rtp_padding, rtp_extension, rtp_csrc_count, rtp_marker, PayloadType);

                        RTPPacketUtil.WriteSequenceNumber(rtp_packet, Sequence);

                        RTPPacketUtil.WriteTS(rtp_packet, timestamp);

                        // UInt32 empty_ssrc = 0;
                        RTPPacketUtil.WriteSSRC(rtp_packet, SSRC);

                        // Now append the Fragmentation Header (with Start and End marker) and part of the raw_nal
                        byte f_bit = 0;
                        byte nri = (byte)((first_byte >> 5) & 0x03); // Part of the 1st byte of the Raw NAL (NAL Reference ID)
                        byte type = 28; // FU-A Fragmentation

                        rtp_packet[12] = (byte)((f_bit << 7) + (nri << 5) + type);
                        rtp_packet[13] = (byte)((start_bit << 7) + (end_bit << 6) + (0 << 5) + (first_byte & 0x1F));

                        System.Array.Copy(raw_nal, nal_pointer, rtp_packet, 14, payload_size);
                        nal_pointer = nal_pointer + payload_size;
                        data_remaining = data_remaining - payload_size;

                        rtp_packets.Add(rtp_packet);

                        Sequence++;

                        start_bit = 0;
                    }
                }
            }

            return rtp_packets;

        }
    }

    public static class RTPPacketUtil
    {

        public static void WriteHeader(byte[] rtp_packet, int rtp_version, int rtp_padding, int rtp_extension, int rtp_csrc_count, int rtp_marker, int rtp_payload_type)
        {
            rtp_packet[0] = (byte)((rtp_version << 6) | (rtp_padding << 5) | (rtp_extension << 4) | rtp_csrc_count);
            rtp_packet[1] = (byte)((rtp_marker << 7) | (rtp_payload_type & 0x7F));
        }

        public static void WriteSequenceNumber(byte[] rtp_packet, uint empty_sequence_id)
        {
            rtp_packet[2] = ((byte)((empty_sequence_id >> 8) & 0xFF));
            rtp_packet[3] = ((byte)((empty_sequence_id >> 0) & 0xFF));
        }

        public static void WriteTS(byte[] rtp_packet, uint ts)
        {
            rtp_packet[4] = ((byte)((ts >> 24) & 0xFF));
            rtp_packet[5] = ((byte)((ts >> 16) & 0xFF));
            rtp_packet[6] = ((byte)((ts >> 8) & 0xFF));
            rtp_packet[7] = ((byte)((ts >> 0) & 0xFF));
        }

        public static void WriteSSRC(byte[] rtp_packet, uint ssrc)
        {
            rtp_packet[8] = ((byte)((ssrc >> 24) & 0xFF));
            rtp_packet[9] = ((byte)((ssrc >> 16) & 0xFF));
            rtp_packet[10] = ((byte)((ssrc >> 8) & 0xFF));
            rtp_packet[11] = ((byte)((ssrc >> 0) & 0xFF));
        }
    }
}
