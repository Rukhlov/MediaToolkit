using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit.Logging;

namespace MediaToolkit.Networks
{
    public abstract class RtpSession
    {
        //protected static Logger logger = LogManager.GetCurrentClassLogger();

        protected static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Networks");

        //public const int MTU = 1300;

        public int MTU { get; set; } = MediaToolkit.Core.Config.MaximumTransmissionUnit;

        public uint SSRC { get;  set; } = 0;
        public int CSRC { get; protected set; } = 0;

        public ushort Sequence { get; protected set; } = 0;
        public int PayloadType { get; protected set; } = 0;
        public int ClockRate { get; protected set; } = 90000;

        // public abstract List<byte[]> Packetize(byte[] data, double sec);
        public abstract List<RtpPacket> Packetize(byte[] data, double sec);
        public abstract MediaFrame Depacketize(RtpPacket pkt);

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

        public override MediaFrame Depacketize(RtpPacket pkt)
        {
            MediaFrame frame = null;
 
            if (pkt != null)
            {
                frame = new MediaFrame
                {
                    Data = pkt.Payload.ToArray()
                };
            }

            return frame;
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

            var nals = HandleH264AnnexbFrames(data);
            if (nals.Count > 0)
            {
                uint timestamp = (uint)(sec * this.ClockRate);
                packets = CreatePackets(nals, timestamp);
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


        public List<RtpPacket> CreatePackets(List<ArraySegment<byte>> nalSegments, uint timestamp)
        {
            List<RtpPacket> rtpPackets = new List<RtpPacket>();

            for (int nalIndex = 0; nalIndex < nalSegments.Count; nalIndex++)
            {
                var segment = nalSegments[nalIndex];

                byte[] nalBuffer = segment.Array;
                int nalPointer = segment.Offset;
                int nalSize = segment.Count;

                bool lastNal = (nalIndex == nalSegments.Count - 1);
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

        private long lastTimestamp = RtpConst.NoTimestamp;
        private long unwrappedTimestamp = 0;

        //private double monotonicTime = 0;
        private ushort lastSequence = 0;

        List<NALUnit> naluBuffer = new List<NALUnit>();

        public override MediaFrame Depacketize(RtpPacket pkt)
        {
            MediaFrame frame = null;

            if (pkt.Sequence != (lastSequence + 1))
            {
                logger.Warn("Bad sequence " + lastSequence + " != " + pkt.Sequence);
                // TODO:
                // Добавляем в буфер и пытаемся упорядочить  
            }

            lastSequence = pkt.Sequence;
            bool newTimestampSequence = false;
            var timestamp = pkt.Timestamp;
            if (timestamp != RtpConst.NoTimestamp && timestamp != lastTimestamp)
            {
                if (lastTimestamp != RtpConst.NoTimestamp)
                {
                    var diff = (timestamp - lastTimestamp);
                    if (diff < 0)
                    { // TODO:
                    }
       
                    unwrappedTimestamp += diff;
                }

                lastTimestamp = timestamp;
                newTimestampSequence = true;

               // logger.Warn(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> ");
            }

            if (newTimestampSequence)
            { // группируем по временным меткам 
                // т.е сервер должен передавать данные с правильным вр.метками иначе работать не будет...

                if (naluBuffer.Count > 0)
                {
                    var firstNalu = naluBuffer[0];

                    // все вр.метки должны быть одинаковые...
                    var naluTimestamp = firstNalu.timestamp;

                    var dataBuffer = new List<byte[]>();
                    int totalLength = 0;
                    foreach (var nal in naluBuffer)
                    {
                        if(naluTimestamp  != nal.timestamp)
                        {
                            logger.Warn("naluTimstamp  != nal.timestamp");
                        }

                        var data = nal.data;
                        dataBuffer.Add(data);
                        totalLength += data.Length;
                    }

                    //var dataBuffer = naluBuffer.Select(n => n.data);
                    //var totalLength = dataBuffer.Sum(n => n.Length);

                    var nalUnitData = new byte[totalLength];
                    int offset = 0;
                    foreach (var data in dataBuffer)
                    {
                        Array.Copy(data, 0, nalUnitData, offset, data.Length);
                        offset += data.Length;
                    }

                    var naluTime = (naluTimestamp / (double)ClockRate);

                    frame = new MediaFrame
                    {
                        Data = nalUnitData,
                        Time = naluTime,
                    };

                    naluBuffer.Clear();
                }

            }

            if(naluBuffer.Count> 256)
            {
                if(timestamp == RtpConst.NoTimestamp || lastTimestamp == RtpConst.NoTimestamp 
                    || timestamp == 0 || lastTimestamp == 0)
                {// если не удается получить правильные вр.метки - сбрасываем буфер
                    // т.к декодировать не сгруппированные данные не все декодеры могу...

                    logger.Error("No valid timestamp received; drop buffer");
                    naluBuffer.Clear();
                }
            }

            NALUnit nalUnit = null;

            byte[] payload = pkt.Payload.ToArray();
            if (payload != null && payload.Length > 0)
            {
                nalUnit = ParseH264Payload(payload, unwrappedTimestamp);
            }

            if (nalUnit != null)
            {
                naluBuffer.Add(nalUnit);
            }

            return frame;

            //if (data != null)
            //{
            //    frame = new MediaFrame
            //    {
            //        Data = data,
            //        Time = monotonicTime,
            //    };
            //}

            //return frame;
        }


        class NALUnit
        {
            public byte[] data = null;
            public long timestamp = 0;
            public int type = 0;
            public bool iFrame = false;
        }

        private List<byte[]> payloadBuffer = new List<byte[]>();
        private bool iFrameFlag = false;
        private readonly static byte[] startSequence = { 0, 0, 0, 1 };

        unsafe private NALUnit ParseH264Payload(byte[] payload, long timestamp)
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

            /*
                Type Name
                   0[unspecified]
                   1 Coded slice
                   2 Data Partition A
                   3 Data Partition B
                   4 Data Partition C
                   5 IDR(Instantaneous Decoding Refresh) Picture
                   6 SEI(Supplemental Enhancement Information)
                   7 SPS(Sequence Parameter Set)
                   8 PPS(Picture Parameter Set)
                   9 Access Unit Delimiter
                  10 EoS(End of Sequence)
                  11 EoS(End of Stream)
                  12 Filter Data
                13 - 23[extended]
                24 - 31[unspecified]
            */

            byte firstByte = payload[0];
            byte unitType = (byte)(firstByte & 0x1f);

            //logger.Verb("unitType " + unitType + " "+ timestamp);

            if (unitType >= 1 && unitType <= 23) // в одном пакете - один NAL unit ()
            {// Single NAL Unit Packet

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


                //logger.Debug("unitType = " + unitType);
                int size = startSequence.Length + payloadLength;
                byte[] nalData = new byte[size];

                int offset = 0;

                Array.Copy(startSequence, 0, nalData, offset, startSequence.Length);
                offset += startSequence.Length;

                Array.Copy(payload, 0, nalData, offset, payloadLength);
                offset += payloadLength;

                //if (unitType == 1)
                //{// Coded slice of a non-IDR picture
                //    //logger.Verb("--------------------- " + arrival);
                //    // return nalData;
                //}
                //else if (unitType == 5)
                //{// IDR(Instantaneous Decoding Refresh) Picture
                //    ////logger.Verb(" ========================== isIFrame =============================== " + arrival);
                //    //return nalUnit;
                //}
                //else if (unitType == 6)
                //{//Supplemental enhancement information (SEI)

                //}
                //else if (unitType == 7)
                //{// Sequence parameter set

                //}
                //else if (unitType == 8)
                //{// Picture parameter set

                //}
                //else if (unitType == 9)
                //{ //Access unit delimiter
                //    //return nalData;
                //}

                iFrameFlag = (unitType == 5);

                return new NALUnit
                {
                    data = nalData,
                    timestamp = timestamp,
                    type = unitType,
                    iFrame = iFrameFlag,
                };

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
                    byte fuHeader = payload[1];
                    byte startBit = (byte)(fuHeader >> 7);
                    byte endBit = (byte)((fuHeader & 0x40) >> 6);
                    //byte forbiddenBit = (byte)((fuHeader & 0x40) >> 7);

                    byte nalUnitType = (byte)(fuHeader & 0x1f); // идентификатор пакета

                    int payloadDataOffset = 2;// смещение 2 байта

                    payloadLength -= payloadDataOffset;

                    int flags = (nalUnitType == 5) ? 1 : 0;

                    int fuSize = (startBit == 1) ? (payloadLength + startSequence.Length + 1) : payloadLength;
                    int offset = 0;

                    byte[] fuBytes = new byte[fuSize];

                    if (startBit == 1) //начало фрагмента
                    {
                        iFrameFlag = (nalUnitType == 5 && ((fuHeader & 0x80) == 128));
                        //if (iFrameFlag)
                        //{
                        //    logger.Verb(" ========================== isIFrame =============================== ");
                        //}

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
                        var nalData = new byte[totalLength];

                        int bufferOffset = 0;
                        foreach (var buf in payloadBuffer)
                        {// TODO: убрать копирование в промежуточный буфер
                            Array.Copy(buf, 0, nalData, bufferOffset, buf.Length);
                            bufferOffset += buf.Length;
                        }
                        payloadBuffer.Clear();

                        //logger.Verb("--------------------- " + arrival);
                        return new NALUnit
                        {
                            data = nalData,
                            timestamp = timestamp,
                            type = nalUnitType,
                            iFrame = iFrameFlag
                        };

                    }
                }
            }
            else if (unitType == 24) // этот режим может понадобится для приема данных от сторонних RTSP серверов
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

                    if (naluSize > (payload.Length - naluPointer))
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
                var nalData = new byte[totalLength];
                {
                    int offset = 0;
                    foreach (var buf in nals)
                    {
                        Array.Copy(buf, 0, nalData, offset, buf.Length);
                        offset += buf.Length;
                    }
                }
                return new NALUnit
                {
                    data = nalData,
                    timestamp = timestamp,
                    type = unitType,
                };

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

    }

    public class MediaFrame
    {
        public double Time = double.NaN;
        public byte[] Data = null;
        public int Flags = 0;


    }
}
