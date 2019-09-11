using NLog;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.RTP
{
    public abstract class RtpSession
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        public const int MTU = 1300;

        public uint SSRC { get; protected set; } = 0;
        public int CSRC { get; protected set; } = 0;

        public ushort Sequence { get; protected set; } = 0;
        public int PayloadType { get; protected set; } = 0;
        public int ClockRate { get; protected set; } = 90000;

        // public abstract List<byte[]> Packetize(byte[] data, double sec);
        public abstract List<RtpPacket> Packetize(byte[] data, double sec);
        public abstract byte[] Depacketize(RtpPacket pkt);
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


        public override List<RtpPacket> Packetize(byte[] data, double sec)
        {
            List<RtpPacket> packets = new List<RtpPacket>();
            uint timestamp = (uint)(sec * ClockRate);
            if (data != null && data.Length > 0)
            {
                packets = CratePackets(data, timestamp);
            }

            return packets;
        }

        public override byte[] Depacketize(RtpPacket pkt)
        {
            throw new NotImplementedException();
        }

        public List<RtpPacket> CratePackets(byte[] data, uint timestamp)
        {
            //Debug.WriteLine("CratePackets(...) " + data.Length + " " + timestamp);

            List<RtpPacket> rtpPackets = new List<RtpPacket>();

            int MaxPacketSize = 160;

            int offset = 0;
            int dataSize = data.Length;
            while (dataSize > 0)
            {

                RtpPacket rtpPacket = new RtpPacket
                {
                    Timestamp = timestamp,
                    Sequence = this.Sequence,
                    SSRC = this.SSRC,
                    PayloadType = this.PayloadType,
                    Marker = false,
                    Extension = false,
                    Padding = false,
                };

                int payloadSize = dataSize;
                if (dataSize > MaxPacketSize)
                {
                    payloadSize = MaxPacketSize;
                }

                rtpPacket.Alloc(payloadSize);
                System.Array.Copy(data, offset, rtpPacket.Payload.Array, rtpPacket.Payload.Offset, payloadSize);

                offset += payloadSize;
                dataSize -= payloadSize;

                //Debug.WriteLine("offset " + offset + " " + timestamp);
                timestamp += (uint)payloadSize; //40 * 8;

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
            lastSequence = 0;

            logger.Debug("H264Session() " + SSRC);
        }

        public override List<RtpPacket> Packetize(byte[] data, double sec)
        {
            //file.Write(data, 0, data.Length);

            List<RtpPacket> packets = new List<RtpPacket>();
            uint timestamp = (uint)(sec * ClockRate);
            var nals = HandleH264AnnexbFrames(data);
            if (nals.Count > 0)
            {
                packets = CratePackets(nals, timestamp);
            }

            return packets;
        }

       // private Stream file = File.Create("test.h264");
        private List<ArraySegment<byte>> HandleH264AnnexbFrames(byte[] frame)
        {// получаем буфер который нужно порезать на NALUnit-ы

            List<ArraySegment<byte>> nalUnits = new List<ArraySegment<byte>>();

            int offset = 0;
            int pos1 = -1;
            int pos2 = -1;

            while (offset < frame.Length - 3)
            {
                if ((frame[offset] == 0 && frame[offset + 1] == 0 && frame[offset + 2] == 1))
                {
                    if (pos1 > 0)
                    {
                        pos2 = offset;
                        if (offset > 0)
                        {
                            if (frame[offset - 1] == 0)
                            {
                                pos2--;
                                //offset--;
                            }
                        }
                        int nalSize = pos2 - pos1;
                        nalUnits.Add(new ArraySegment<byte>(frame, pos1, nalSize));
                        pos2 = -1;
                    }

                    offset += 3;
                    pos1 = offset;
                    continue;
                }
                else
                {
                    //offset += 3;
                    offset++;
                }
            }

            if (pos1 > 0 && pos2 == -1)
            {
                pos2 = frame.Length;
                int nalSize = pos2 - pos1;

                nalUnits.Add(new ArraySegment<byte>(frame, pos1, nalSize));
            }
            //logger.Debug("nalUnits.Count " + nalUnits.Count);
            return nalUnits;
        }


        public List<RtpPacket> CratePackets(List<ArraySegment<byte>> nalSegments, uint timestamp)
        {
            List<RtpPacket> rtpPackets = new List<RtpPacket>();

            for (int x = 0; x < nalSegments.Count; x++)
            {
                var segment = nalSegments[x];

                byte[] nalBuffer = segment.Array;
                int nalPointer = segment.Offset;
                int nalSize = segment.Count;

                bool lastNal = (x == nalSegments.Count - 1);
                bool fragmenting = (nalSize > MTU);

                if (fragmenting)
                {// Если  NAL не влезает в udp пакет - то делим его на Fragmtent Unit-ы
                    int dataRemaining = nalSize;
                    //int nalPointer = 0;

                    int startBit = 1;
                    int endBit = 0;

                    // FU header
                    byte firstByte = nalBuffer[nalPointer];
                    nalPointer++;
                    dataRemaining--;

                    while (dataRemaining > 0)
                    {
                        RtpPacket rtpPacket = new RtpPacket
                        {
                            Marker = lastNal,
                            Timestamp = timestamp,
                            Sequence = this.Sequence,
                            SSRC = this.SSRC,
                            PayloadType = this.PayloadType,
                        };

                        int dataSize = Math.Min(MTU, dataRemaining);
                        if (dataRemaining - dataSize == 0)
                        {
                            endBit = 1;
                        }

                        int rtpPayloadSize = dataSize + 2;//2 bytes for FU - A header;

                        rtpPacket.Alloc(rtpPayloadSize);

                        var rtpPayload = rtpPacket.Payload.Array;
                        int offset = rtpPacket.Payload.Offset;


                        /* FU indicator
                         * +---------------+
                           |0|1|2|3|4|5|6|7|
                           +-+-+-+-+-+-+-+-+
                           |F|NRI|  Type   |
                           +---------------+
                         */
                        byte f_bit = 0;
                        byte nri = (byte)((firstByte >> 5) & 0x03); 
                        byte type = 28; // FU-A Fragmentation

                        rtpPayload[offset++] = (byte)((f_bit << 7) + (nri << 5) + type);

                        /* FU header
                          * +---------------+
                           |0|1|2|3|4|5|6|7|
                           +-+-+-+-+-+-+-+-+
                           |S|E|R|  Type   |
                           +---------------+

                         S:   1 bit
                              When set to one, the Start bit indicates the start of a
                              fragmented NAL unit.  When the following FU payload is not the
                              start of a fragmented NAL unit payload, the Start bit is set
                              to zero.

                         E:     1 bit
                                When set to one, the End bit indicates the end of a fragmented
                                NAL unit, i.e., the last byte of the payload is also the last
                                byte of the fragmented NAL unit.  When the following FU
                                payload is not the last fragment of a fragmented NAL unit, the
                                End bit is set to zero.

                         R:     1 bit
                                The Reserved bit MUST be equal to 0 and MUST be ignored by the
                                receiver.

                         Type:  5 bits
                                The NAL unit payload type as defined in Table 7-1

                          */
                        rtpPayload[offset++] = (byte)((startBit << 7) + (endBit << 6) + (0 << 5) + (firstByte & 0x1F));

                        // Copy NAL data
                        System.Array.Copy(nalBuffer, nalPointer, rtpPayload, offset, dataSize);

                        nalPointer += dataSize;
                        dataRemaining -= dataSize;

                        rtpPackets.Add(rtpPacket);

                        this.Sequence++;

                        startBit = 0;
                    }
                }
                else
                {
                    RtpPacket rtpPacket = new RtpPacket(nalBuffer, nalPointer, nalSize)
                    {
                        Marker = lastNal,
                        Timestamp = timestamp,
                        Sequence = this.Sequence,
                        SSRC = this.SSRC,
                        PayloadType = this.PayloadType,
                    };

                    rtpPackets.Add(rtpPacket);

                    this.Sequence++;
                }

            }

            return rtpPackets;

        }

        private ushort lastSequence = 0;
        public override byte[] Depacketize(RtpPacket pkt)
        {

            if (pkt.Sequence != (lastSequence + 1))
            {
                logger.Warn("Bad sequence " + lastSequence + " != " + pkt.Sequence);
                // TODO:
                // Добавляем в буфер и пытаемся упорядочить  
            }
            lastSequence = pkt.Sequence;


            byte[] data = null;

            byte[] payload = pkt.Payload.ToArray();
            if (payload != null && payload.Length > 0)
            {

                data = HandleH264NalUnit(payload, 0);
            }

            return data;
        }

        private List<byte[]> payloadBuffer = new List<byte[]>();
        private readonly byte[] startSequence = { 0, 0, 0, 1 };


        unsafe public byte[] HandleH264NalUnit(byte[] payload, double arrival)
        {
            if (payload == null)
            {
                return null;
            }

            int payloadLength = payload.Length;
            if (payloadLength == 0)
            {
                logger.Warn("payloadLength == 0");
                return null;
            }

            byte firstByte = payload[0];
            byte unitType = (byte)(firstByte & 0x1f);

            // RFC6184 Section 6 Packetization Mode:
            // 0 or not present: Single NAL mode (Only nals from 1-23 are allowed) - All receivers MUST support this mode.
            // 1: Non-interleaved Mode: 1-23, 24 (STAP-A), 28 (FU-A) are allowed. - This mode SHOULD be supported
            // 2: Interleaved Mode: 25 (STAP-B), 26 (MTAP16), 27 (MTAP24), 28 (FU-A), and 29 (FU-B) are allowed. - Some receivers MAY support this mode

            /*
             * 7.1.  Single NAL Unit and Non-Interleaved Mode

               The receiver includes a receiver buffer to compensate for
               transmission delay jitter.  The receiver stores incoming packets in
               reception order into the receiver buffer.  Packets are de-packetized
               in RTP sequence number order.  If a de-packetized packet is a single
               NAL unit packet, the NAL unit contained in the packet is passed
               directly to the decoder.  If a de-packetized packet is an STAP-A, the
               NAL units contained in the packet are passed to the decoder in the
               order in which they are encapsulated in the packet.  For all the FU-A
               packets containing fragments of a single NAL unit, the de-packetized
               fragments are concatenated in their sending order to recover the NAL
               unit, which is then passed to the decoder.
             */
            if (unitType >= 1 && unitType <= 23) // в одном пакете - один NAL unit ()
            {// Single NAL Unit Packet
             //  TODO: доделать 
             //logger.Debug("unitType = " + unitType);
             /*
              *   0                   1                   2                   3
                  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                 +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                 |F|NRI|  Type   |                                               |
                 +-+-+-+-+-+-+-+-+                                               |
                 |                                                               |
                 |               Bytes 2..n of a single NAL unit                 |
                 |                                                               |
                 |                               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                 |                               :...OPTIONAL RTP padding        |
              */

                int size = startSequence.Length + payloadLength;
                byte[] nalData = new byte[size];

                int offset = 0;

                Array.Copy(startSequence, 0, nalData, offset, startSequence.Length);
                offset += startSequence.Length;

                Array.Copy(payload, 0, nalData, offset, payloadLength);
                offset += payloadLength;

                //return nalData;

                if (unitType == 1)
                {// Coded slice of a non-IDR picture
                    return nalData;
                }

                //if(unitType != 9)
                //{ //Access unit delimiter
                //    return nalData;
                //}

                // !!!!!!!!!!!!! 
                payloadBuffer.Add(nalData);

                //System.Diagnostics.Debug.WriteLine("unitType {0} bufferOffset {1}", unitType, bufferOffset);
            }
            else if (unitType == 28) // фрагментированный пакет (FU-A)
            {   
                /* RFC6184 5.8.Fragmentation Units(FUs)
                 *   0                   1                   2                   3
                     0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    | FU indicator  |   FU header   |                               |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+                               |
                    |                                                               |
                    |                         FU payload                            |
                    |                                                               |
                    |                               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                               :...OPTIONAL RTP padding        |
                 */

                //logger.Debug("unitType = " + unitType);
                if (payloadLength > 1)
                {
                    // logger.Debug("payloadLength > 1");

                    byte fuHeader = payload[1];
                    byte startBit = (byte)(fuHeader >> 7);
                    byte endBit = (byte)((fuHeader & 0x40) >> 6);
                    //byte forbiddenBit = (byte)((fuHeader & 0x40) >> 7);

                    byte nalUnitType = (byte)(fuHeader & 0x1f); // идентификатор пакета

                    //logger.Debug("nalUnitType = " + nalUnitType);

                    int payloadDataOffset = 2;// смещение 2 байта

                    payloadLength -= payloadDataOffset;

                    int flags = (nalUnitType == 5) ? 1 : 0;

                    int fuSize = (startBit == 1) ? (payloadLength + startSequence.Length + 1) : payloadLength;
                    int offset = 0;

                    byte[] fuBytes = new byte[fuSize];

                    if (startBit == 1) //начало фрагмента
                    {
                        Array.Copy(startSequence, fuBytes, startSequence.Length);
                        offset += startSequence.Length;

                        fuBytes[offset] = (byte)((firstByte & 0xe0) | nalUnitType);
                        offset++;
                    }

                    Array.Copy(payload, payloadDataOffset, fuBytes, offset, payloadLength);
                    offset += payloadLength;

                    payloadBuffer.Add(fuBytes);

                    if (endBit == 1)
                    {
                        var totalLength = payloadBuffer.Sum(n => n.Length);
                        var nalUnit = new byte[totalLength];

                        int bufferOffset = 0;
                        foreach (var buf in payloadBuffer)
                        {
                            Array.Copy(buf, 0, nalUnit, bufferOffset, buf.Length);
                            bufferOffset += buf.Length;
                        }
                        payloadBuffer.Clear();

                        return nalUnit;

                    }
                }
            }
            else if (unitType == 24)
            { // STAP-A (one packet, multiple nals)
                /*
                 *  0                   1                   2                   3
                    0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                          RTP Header                           |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |STAP-A NAL HDR |         NALU 1 Size           | NALU 1 HDR    |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                         NALU 1 Data                           |
                    :                                                               :
                    +               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |               | NALU 2 Size                   | NALU 2 HDR    |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                         NALU 2 Data                           |
                    :                                                               :
                    |                               +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                    |                               :...OPTIONAL RTP padding        |
                    +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
                 */
                List<byte[]> nals = new List<byte[]>(16);

                int naluPointer = 1; //STAP-A NAL HDR
                while (naluPointer < payload.Length - 1)
                {                
                    int naluSize = BigEndian.ReadInt16(payload, naluPointer); //NALU 1 Size
                    naluPointer += 2;

                    if(naluSize > (payload.Length - naluPointer))
                    {
                        logger.Error("naluSize > (payload.Length - naluPointer) ");
                    }

                    byte[] nal = new byte[naluSize + startSequence.Length];
                    Array.Copy(startSequence, nal, startSequence.Length);

                    Array.Copy(payload, naluPointer, nal, startSequence.Length, naluSize); //NALU 1 HDR +  NALU 1 Data

                    naluPointer += naluSize;
                    nals.Add(nal);

                }

                var totalLength = nals.Sum(n => n.Length);
                var nalUnit = new byte[totalLength];
                {
                    int offset = 0;
                    foreach (var buf in nals)
                    {
                        Array.Copy(buf, 0, nalUnit, offset, buf.Length);
                        offset += buf.Length;
                    }
                }
                return nalUnit;

            }
            if (unitType == 25 || unitType == 26 || unitType == 27 || unitType == 29) // Interleaved Mode
            {// Не нужно

                throw new InvalidOperationException("Not supported unit type " + unitType);

            }

            if (unitType == 0 || unitType == 30 || unitType == 31) // reserved 
            {// эти пакеты приходить не должны!

                throw new InvalidOperationException("Invalid packet. Reserved unit type " + unitType);
            }

            return null;
        }


        /*
        private List<byte[]> _HandleH264AnnexbFrames(byte[] frame)
        {// получаем буфер который нужно порезать на NALUnit-ы
            List<byte[]> nals = new List<byte[]>();

            int offset = 0;
            int pos1 = -1;
            int pos2 = -1;

            try
            {
                while (offset < frame.Length - 4)
                {

                    if ((frame[offset] == 0 && frame[offset + 1] == 0 && frame[offset + 2] == 1))
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

                        offset += 3;
                        pos1 = offset;
                        continue;
                    }

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
                        //offset++;
                        offset += 4;
                        pos1 = offset;
                    }
                    else
                    {
                        // offset += 4;
                        offset++;
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

        public List<RtpPacket> _CratePackets(List<byte[]> nalArray, uint timestamp)
        {

            List<RtpPacket> rtpPackets = new List<RtpPacket>();

            for (int x = 0; x < nalArray.Count; x++)
            {

                byte[] rawNal = nalArray[x];
                bool lastNal = (x == nalArray.Count - 1);

                bool fragmenting = (rawNal.Length > MTU);

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
                        RtpPacket rtpPacket = new RtpPacket
                        {
                            Marker = lastNal,
                            Timestamp = timestamp,
                            Sequence = this.Sequence,
                            SSRC = this.SSRC,
                            PayloadType = this.PayloadType,
                        };

                        int dataSize = Math.Min(MTU, dataRemaining);

                        if (dataRemaining - dataSize == 0)
                        {
                            endBit = 1;
                        }

                        int rtpPayloadSize = dataSize + 2;//2 bytes for FU - A header;

                        rtpPacket.Alloc(rtpPayloadSize);

                        var rtpPayload = rtpPacket.Payload.Array;
                        int offset = rtpPacket.Payload.Offset;

                        // Now append the Fragmentation Header (with Start and End marker) and part of the raw_nal
                        byte f_bit = 0;
                        byte nri = (byte)((firstByte >> 5) & 0x03); // Part of the 1st byte of the Raw NAL (NAL Reference ID)
                        byte type = 28; // FU-A Fragmentation

                        rtpPayload[offset++] = (byte)((f_bit << 7) + (nri << 5) + type);
                        rtpPayload[offset++] = (byte)((startBit << 7) + (endBit << 6) + (0 << 5) + (firstByte & 0x1F));

                        System.Array.Copy(rawNal, nalPointer, rtpPayload, offset, dataSize);

                        nalPointer += dataSize;
                        dataRemaining -= dataSize;

                        rtpPackets.Add(rtpPacket);

                        this.Sequence++;

                        startBit = 0;
                    }
                }
                else
                {
                    RtpPacket rtpPacket = new RtpPacket(rawNal, 0)
                    {
                        Marker = lastNal,//(lastNal ? 1 : 0),
                        Timestamp = timestamp,
                        Sequence = this.Sequence,
                        SSRC = this.SSRC,
                        PayloadType = this.PayloadType,
                    };

                    rtpPackets.Add(rtpPacket);

                    this.Sequence++;
                }

            }

            return rtpPackets;

        }

    */

    }
}
