using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.RTP
{

    public class RtpConst
    {
        public const int Version = 2;
        public const uint NoTimestamp = uint.MaxValue;

        public const int MinRtpLength = 12;
        public const int MaxRtpLength = 8192;

        //...
        public const int RtcpValidMask = 0xc000 | 0x2000 | 0xfe;
        public const int MaxSdes = 255;
    }


    // 0                   1                   2                   3
    // 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|V=2|P|X|  CC   |M|     PT      |       sequence number         |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|                           timestamp                           |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|           synchronization source (SSRC) identifier            |
    //+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
    //|            contributing source (CSRC) identifiers             |
    //|                             ....                              |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //[
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //|      defined by profile       |           length              |
    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    //
    /// <summary>
    /// <see cref="http://www.ietf.org/rfc/rfc3550.txt"/> раздел 5.1
    /// </summary>
    public class RtpPacket
    {
        //First byte
        public int Version = 2;
        public bool Padding = false;
        public bool Extension = false;
        public int CSRCCount = 0;

        //Second byte
        public bool Marker = false;
        public int PayloadType = 0;

        //...
        public ushort Sequence = 0;
        public uint Timestamp;
        public uint SSRC = 0;
        public uint[] CSRC;

        public ushort HeaderExtensionProfile;

        public ArraySegment<byte> HeaderExtension { get; private set; }
        public ushort HeaderExtensionLength
        {
            get
            {
                return (ushort)HeaderExtension.Count;
            }
        }

        public ArraySegment<byte> Payload { get; private set; }
        public int PayloadLength
        {
            get
            {
                return Payload.Count;
            }
        }

        private byte[] packetBytes = new byte[RtpConst.MinRtpLength];

        public RtpPacket() { }

        public RtpPacket(byte[] payload, int position, int size = 0)
        {
            int payloadSize = payload.Length;
            if (size > 0)
            {
                payloadSize = size;
            }

            int headerSize = RtpConst.MinRtpLength;

            int packetSize = headerSize + payloadSize;

            this.packetBytes = new byte[packetSize];
            int offset = headerSize;

            System.Array.Copy(payload, position, packetBytes, offset, payloadSize);

            this.Payload = new ArraySegment<byte>(packetBytes, offset, payloadSize);

        }

        public void Alloc(int payloadSize, int extensionSize)
        {
            int headerSize = RtpConst.MinRtpLength;
            int size = payloadSize + extensionSize + headerSize;

            this.packetBytes = new byte[size];
            int offset = headerSize;

            this.HeaderExtension = new ArraySegment<byte>(packetBytes, offset, extensionSize);
            offset += extensionSize;

            this.Payload = new ArraySegment<byte>(packetBytes, offset, payloadSize);
        }

        public void Alloc(int payloadSize)
        {
            int headerSize = RtpConst.MinRtpLength;
            this.packetBytes = new byte[payloadSize + headerSize];
            this.Payload = new ArraySegment<byte>(packetBytes, headerSize, payloadSize);
        }


        public byte[] GetBytes()
        {
            // RTP Packet Header
            // 0 - Version, P, X, CC, M, PT and Sequence Number
            //32 - Timestamp. 
            //64 - SSRC
            //96 - CSRCs (optional)
            //nn - Extension ID and Length
            //nn - Extension header

            int padding = Padding ? 1 : 0;
            int extension = Extension ? 1 : 0;
            int marker = Marker ? 1 : 0;

            packetBytes[0] = (byte)((Version << 6) | (padding << 5) | (extension << 4) | CSRCCount);
            packetBytes[1] = (byte)(((marker & 0x01) << 7) | (PayloadType & 0x7F));

            BigEndian.WriteUInt16(packetBytes, 2, Sequence);

            BigEndian.WriteUInt32(packetBytes, 4, Timestamp);

            BigEndian.WriteUInt32(packetBytes, 8, SSRC);

            return packetBytes;
        }

        //public static RtpPacket Create(byte[] data)
        //{
        //    return Create(data, data.Length);
        //}

        public static RtpPacket Create(byte[] data, int len, RtpSession session)
        {
            if (len < RtpConst.MinRtpLength)
            {
                throw new InvalidOperationException("Invalid RTP packet. Packet size < " + RtpConst.MinRtpLength);
            }

            RtpPacket packet = new RtpPacket();
            packet.packetBytes = data;

            int dataLength = len;//data.Length;
            int offset = 0;
            byte firstByte = data[offset++];
            packet.Version = (byte)(firstByte >> 6);

            if (packet.Version != RtpConst.Version)
            {
                throw new InvalidOperationException("Invalid RTP packet. Unsupported version " + packet.Version);
            }

            /*
             * If the padding bit is set, the packet contains one or more
              additional padding octets at the end which are not part of the
              payload.  The last octet of the padding contains a count of how
              many padding octets should be ignored, including itself.  Padding
              may be needed by some encryption algorithms with fixed block sizes
              or for carrying several RTP packets in a lower-layer protocol data
              unit.
             */
             packet.Padding = ((firstByte & 0x20) == 0x20);

            if (packet.Padding)
            {// последний байт в пакете длинна области заполнения
                byte padding = data.Last();
                dataLength -= padding;
            }

            packet.Extension = (firstByte & 0x10) == 0x10;
            packet.CSRCCount = (byte)(firstByte & 0x0F);

            byte secondByte = data[offset++];
            packet.Marker = (secondByte & 0x80) == 0x80;

            packet.PayloadType = (secondByte & 0x7F);

            packet.Sequence = BigEndian.ReadUInt16(data, offset);
            offset += 2;

            packet.Timestamp = BigEndian.ReadUInt32(data, offset);
            offset += 4;

            packet.SSRC = BigEndian.ReadUInt32(data, offset);
            offset += 4;

            if(packet.SSRC != session.SSRC)
            {// Invalid session id
                return null;
            }

            offset += packet.CSRCCount * 4;

            if (packet.Extension)
            {   /*
                 *   An extension mechanism is provided to allow individual
                   implementations to experiment with new payload-format-independent
                   functions that require additional information to be carried in the
                   RTP data packet header.  This mechanism is designed so that the
                   header extension may be ignored by other interoperating
                   implementations that have not been extended.
                 */

                packet.HeaderExtensionProfile = BigEndian.ReadUInt16(data, offset);
                offset += 2;

                var headerExtensionLength = BigEndian.ReadUInt16(data, offset);
                offset += 2;

                packet.HeaderExtension = new ArraySegment<byte>(data, offset, headerExtensionLength);
                offset += headerExtensionLength;
            }
            else
            {
                packet.HeaderExtensionProfile = 0;
            }

            int payloadLength = (dataLength - offset);
            packet.Payload = new ArraySegment<byte>(data, offset, payloadLength);


            return packet;
        }

        public RtpPacket Clone()
        {
            RtpPacket packet = new RtpPacket();

            packet.Version = this.Version;
            packet.Padding = this.Padding;
            packet.Extension = this.Extension;
            packet.CSRCCount = this.CSRCCount;

            packet.Marker = this.Marker;
            packet.PayloadType = this.PayloadType;
            packet.Sequence = this.Sequence;

            packet.Timestamp = this.Timestamp;
            packet.SSRC = this.SSRC;

            packet.CSRC = this.CSRC;
            packet.HeaderExtensionProfile = this.HeaderExtensionProfile;

            packet.packetBytes = new byte[this.packetBytes.Length];

            this.packetBytes.CopyTo(packet.packetBytes, 0);

            return packet;
            
        }
    }

    public unsafe static class BigEndian
    {
        private static ushort Reverse(ushort n)
        {
            return (ushort)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        private static short Reverse(short n)
        {
            return (short)(((n & 0xff) << 8) | ((n >> 8) & 0xff));
        }

        private static uint Reverse(uint n)
        {
            return (uint)(((Reverse((ushort)n) & 0xffff) << 0x10) | (Reverse((ushort)(n >> 0x10)) & 0xffff));
        }

        private static int Reverse(int n)
        {
            return (int)(((Reverse((short)n) & 0xffff) << 0x10) | (Reverse((short)(n >> 0x10)) & 0xffff));
        }

        private static ulong Reverse(ulong n)
        {
            return (ulong)(((Reverse((uint)n) & 0xffffffffL) << 0x20) | (Reverse((uint)(n >> 0x20)) & 0xffffffffL));
        }

        private static long Reverse(long n)
        {
            return (long)(((Reverse((int)n) & 0xffffffffL) << 0x20) | (Reverse((int)(n >> 0x20)) & 0xffffffffL));
        }

        public static ushort ReadUInt16(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt16(data, offset));
        }

        public static ushort ReadUInt16(IntPtr data, int offset)
        {

            return Reverse((ushort)Marshal.ReadInt32(data, offset));//BitConverter.ToInt16(data, offset));
        }

        public static short ReadInt16(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToInt16(data, offset));
        }

        public static short ReadInt16(IntPtr data, int offset)
        {

            return Reverse(Marshal.ReadInt16(data, offset));//BitConverter.ToInt16(data, offset));
        }

        unsafe public static ushort ReadUInt16(void* data)
        {
            return Reverse(*(ushort*)data);
        }

        unsafe public static short ReadInt16(void* data)
        {
            return Reverse(*(short*)data);
        }

        public static uint ReadUInt24(byte[] data, int offset)
        {
            uint result = 0;
            result = data[offset];
            result <<= 8;
            result |= data[offset + 1];
            result <<= 8;
            result |= data[offset + 2];
            return result;
        }

        public static int ReadInt24(byte[] data, int offset)
        {
            int result = 0;
            result = data[offset];
            result <<= 8;
            result |= data[offset + 1];
            result <<= 8;
            result |= data[offset + 2];
            return result;
        }

        unsafe public static uint ReadUInt24(byte* data)
        {
            uint result = 0;
            result = data[0];
            result <<= 8;
            result |= data[1];
            result <<= 8;
            result |= data[2];
            return result;
        }

        unsafe public static int ReadInt24(byte* data)
        {
            int result = 0;
            result = data[0];
            result <<= 8;
            result |= data[1];
            result <<= 8;
            result |= data[2];
            return result;
        }

        public static uint ReadUInt32(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt32(data, offset));
        }
        public static uint ReadUInt32(IntPtr data, int offset)
        {
            return Reverse((uint)Marshal.ReadInt64(data, offset));//BitConverter.ToInt16(data, offset));
        }

        public static int ReadInt32(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToInt32(data, offset));
        }

        public static int ReadInt32(IntPtr data, int offset)
        {
            return Reverse(Marshal.ReadInt32(data, offset));//BitConverter.ToInt16(data, offset));
        }

        unsafe public static uint ReadUInt32(void* data)
        {
            return Reverse(*(uint*)data);
        }

        unsafe public static int ReadInt32(void* data)
        {
            return Reverse(*(int*)data);
        }

        public static ulong ReadUInt64(byte[] data, int offset)
        {
            return Reverse(BitConverter.ToUInt64(data, offset));
        }

        public static long ReadInt64(byte[] data, int offset)
        {

            //return System.Net.IPAddress.NetworkToHostOrder(BitConverter.ToInt64(data, offset));


            return Reverse(BitConverter.ToInt64(data, offset));
        }

        public static long ReadInt64(IntPtr data, int offset)
        {
            return Reverse(Marshal.ReadInt64(data, offset));//BitConverter.ToInt16(data, offset));
        }

        public static ulong ReadUInt64(IntPtr data, int offset)
        {
            return Reverse((ulong)Marshal.ReadInt64(data, offset));
        }


        unsafe public static ulong ReadUInt64(void* data)
        {
            return Reverse(*(ulong*)data);
        }

        unsafe public static long ReadInt64(void* data)
        {
            return Reverse(*(long*)data);
        }

        public static void WriteUInt16(byte[] data, int offset, ushort n)
        {
            data[offset] = (byte)(n >> 8);
            data[offset + 1] = (byte)(n & 0xff);
        }

        unsafe public static void WriteUInt16(void* data, ushort n)
        {
            ((byte*)data)[0] = (byte)(n >> 8);
            ((byte*)data)[1] = (byte)(n & 0xff);
        }

        public static void WriteUInt32(byte[] data, int offset, uint n)
        {
            data[offset] = (byte)(n >> 24);
            data[offset + 1] = (byte)((n >> 16) & 0xFF);
            data[offset + 2] = (byte)((n >> 8) & 0xFF);
            data[offset + 3] = (byte)(n & 0xFF);
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
