using ScreenStreamer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.RTP
{
    public abstract class RtpSession
    {

        public const int MTU = 1400;

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

                RtpPacket _rtpPacket = new RtpPacket
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

                _rtpPacket.Alloc(payloadSize);
                System.Array.Copy(data, offset, _rtpPacket.Payload.Array, _rtpPacket.Payload.Offset, payloadSize);

                offset += payloadSize;
                dataSize -= payloadSize;

                //Debug.WriteLine("offset " + offset + " " + timestamp);
                timestamp += (uint)payloadSize; //40 * 8;

                rtpPackets.Add(_rtpPacket);

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

        public override List<RtpPacket> Packetize(byte[] data, double sec)
        {
            List<RtpPacket> packets = new List<RtpPacket>();
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

        public List<RtpPacket> CratePackets(List<byte[]> nalArray, uint timestamp)
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
                            Marker = lastNal,//(lastNal ? 1 : 0),
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
                    RtpPacket rtpPacket = new RtpPacket(rawNal)
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


        public override byte[] Depacketize(RtpPacket packet)
        {
            byte[] data = null;

            byte[] payload = packet.Payload.ToArray();
            if (payload != null && payload.Length > 0)
            {
                data = HandleH264NalUnit(payload, 0);
            }

            return data;
        }

        private MemoryStream nalBuffer = new MemoryStream();

        private ushort lastSequence = 0;
        unsafe public byte[] HandleH264NalUnit(byte[] payload, double arrival)
        {
            if (payload == null) return null;
            int payloadLength = payload.Length;

            if (payloadLength == 0) return null;

            //if (++lastSequence != rtp.Sequence)
            //{
            //    logger.Warn("Bad sequence " + lastSequence + " != " + rtp.Sequence);
            //}
            //lastSequence = rtp.Sequence;


            byte[] startSequence = { 0, 0, 0, 1 };

            byte firstByte = payload[0];
            byte unitType = (byte)(firstByte & 0x1f);

            // RFC6184 Section 6 Packetization Mode:
            // 0 or not present: Single NAL mode (Only nals from 1-23 are allowed) - All receivers MUST support this mode.
            // 1: Non-interleaved Mode: 1-23, 24 (STAP-A), 28 (FU-A) are allowed. - This mode SHOULD be supported
            // 2: Interleaved Mode: 25 (STAP-B), 26 (MTAP16), 27 (MTAP24), 28 (FU-A), and 29 (FU-B) are allowed. - Some receivers MAY support this mode

            if (unitType >= 1 && unitType <= 23) // в одном пакете - один NAL unit ()
            {
                //logger.Debug("unitType = " + unitType);

                int size = startSequence.Length + payloadLength;
                int flags = 0;
                byte[] nalData = new byte[size];

                // MediaPacket packet = new MediaPacket(size, rtp.Timestamp, flags, arrival);
                int offset = 0;

                //NativeMethods.CopyMemory(packet.data + offset, (IntPtr)startSequence, startSequenceLength);
                Array.Copy(startSequence, 0, nalData, offset, startSequence.Length);

                offset += startSequence.Length;

                Array.Copy(payload, 0, nalData, offset, payloadLength);
                //NativeMethods.CopyMemory(packet.data + offset, rtp.Payload, rtp.PayloadLength);
                offset += payloadLength;

                if (unitType == 1)
                {
                    return nalData;
                }

                nalBuffer.Write(nalData, 0, nalData.Length);
                //packetBuffer.Add(packet);

                //System.Diagnostics.Debug.WriteLine("unitType {0} bufferOffset {1}", unitType, bufferOffset);
            }
            else if (unitType == 24)  // STAP-A (one packet, multiple nals)
            { // Камеры передающие такие пакеты на попадались
                // TODO: реализовать т.к этот режим должен поддерживаться

                throw new Exception("Not supported NAL type " + unitType);

                //logger.Error(error);
            }
            else if (unitType == 28) // фрагментированный пакет (FU-A)
            {
                //logger.Debug("unitType = " + unitType);
                if (payloadLength > 1)
                {
                    // logger.Debug("payloadLength > 1");

                    byte fuHeader = payload[1];//*(byte*)(rtp.Payload + 1);
                    byte startBit = (byte)(fuHeader >> 7);
                    byte endBit = (byte)((fuHeader & 0x40) >> 6);
                    //byte forbiddenBit = (byte)((fuHeader & 0x40) >> 7);

                    byte nalUnitType = (byte)(fuHeader & 0x1f); // идентификатор пакета

                    //logger.Debug("nalUnitType = " + nalUnitType);

                    int payloadDataOffset = 2;// смещение 2 байта
                    //rtp.Payload += payloadDataOffset;
                    payloadLength -= payloadDataOffset;

                    int flags = (nalUnitType == 5) ? 1 : 0;

                    int size = (startBit == 1) ? (payloadLength + startSequence.Length + 1) : payloadLength;
                    int offset = 0;

                    byte[] nalBytes = new byte[size];

                    //MediaPacket payload = new MediaPacket(size, rtp.Timestamp, flags, arrival);

                    if (startBit == 1) //начало фрагмента
                    {
                        //nalBuffer = new MemoryStream();

                        Array.Copy(startSequence, nalBytes, startSequence.Length);
                        //Marshal.Copy((IntPtr)startSequence, nalBytes, offset, startSequenceLength);
                        // NativeMethods.CopyMemory(payload.data + offset, (IntPtr)startSequence, startSequenceLength);
                        offset += startSequence.Length;

                        //*((byte*)(payload.data + offset)) = (byte)((firstByte & 0xe0) | nalUnitType);
                        nalBytes[offset] = (byte)((firstByte & 0xe0) | nalUnitType);
                        offset++;
                    }

                    Array.Copy(payload, payloadDataOffset, nalBytes, offset, payloadLength);

                    //NativeMethods.CopyMemory(payload.data + offset, rtp.Payload, payloadLength);
                    offset += payloadLength;

                    nalBuffer.Write(nalBytes, 0, nalBytes.Length);
                    //packetBuffer.Add(payload);

                    if (endBit == 1)
                    {
                        var nal = nalBuffer.ToArray();

                        nalBuffer = new MemoryStream();

                        return nal;
                        //return FinalizeBuffer();
                    }
                }
            }

            if (unitType == 25 || unitType == 26 || unitType == 27 || unitType == 29) // Interleaved Mode
            {//

                throw new InvalidOperationException("Not supported unit type " + unitType);

            }

            if (unitType == 0 || unitType == 30 || unitType == 31) // reserved 
            {

                throw new InvalidOperationException("Invalid packet. Reserved unit type " + unitType);
            }

            return null;
        }
    }
}
