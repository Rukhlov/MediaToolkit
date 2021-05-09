using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Codecs
{
    /// <summary>
    /// https://yumichan.net/video-processing/video-compression/introduction-to-h264-nal-unit/
    /// </summary>
    public enum NalUnitType
    {
        Unspecified = 0,
        Slice = 1,
        DataPartitionA = 2,
        DataPartitionB = 3,
        DataPartitionC = 4,
        IDR = 5,
        SupplentalEnhancementInfo = 6,
        SequenceParameterSet = 7,
        PictureParameterSet = 8,
        AccessUnitDelimiter = 9,
        EndOfSequence = 10,
        EndOfStream = 11,
        FillerData = 12
    }


    public class NalUnitReader
    {

        private readonly Stream stream;
        public NalUnitReader(Stream stream)
        {
            this.stream = stream;
        }

        private bool startCodeReceived = false;
        public bool ReadNext(out byte[] nals)
        {
            nals = null;

            int nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                return false;
            }

            byte one = 0;
            byte two = 0;
            byte three = 0;
            // MemoryStream buffer = new MemoryStream(1024 * 1024);
            List<byte> buffer = new List<byte>(1024);
            one = (byte)nextByte;
            if (startCodeReceived)
            {
                buffer.Add(one);
                // buffer.WriteByte(one);
            }

            nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                return false;
            }

            two = (byte)nextByte;
            if (startCodeReceived)
            {
                buffer.Add(two);
                //buffer.WriteByte(two);
            }

            while ((nextByte = stream.ReadByte()) != -1)
            {
                three = (byte)nextByte;

                if (one == 0x0 && two == 0x0 && three == 0x1)
                {
                    if (startCodeReceived)
                    {
                        buffer.Add(three);
                        //buffer.WriteByte(three);

                        // var array = new List<byte>(buffer.ToArray());

                        if (buffer.Count > 3)
                        {// Trim end...
                            var lastIndex = buffer.Count - 1;
                            if (buffer[lastIndex] == 0x1 && buffer[lastIndex - 1] == 0x0 && buffer[lastIndex - 2] == 0x0 && buffer[lastIndex - 3] == 0x0)
                            {
                                nals = new byte[buffer.Count - 4];
                                buffer.CopyTo(0, nals, 0, nals.Length);

                                //buffer.RemoveRange(lastIndex - 3, 4);
                            }
                            else if (buffer[lastIndex] == 0x1 && buffer[lastIndex - 1] == 0x0 && buffer[lastIndex - 2] == 0x0)
                            {
                                nals = new byte[buffer.Count - 3];
                                buffer.CopyTo(0, nals, 0, nals.Length);

                                //buffer.RemoveRange(lastIndex - 2, 3);
                            }
                        }

                        if (nals == null)
                        {
                            nals = buffer.ToArray();
                        }

                        return true;
                    }

                    // first start code...
                    startCodeReceived = true;
                    continue;
                }

                if (startCodeReceived)
                {
                    buffer.Add(three);
                    //buffer.WriteByte(three);
                }

                one = two;
                two = three;
            }

            nals = buffer.ToArray();
            return false;
        }



        public static List<ArraySegment<byte>> HandleH264AnnexbFrames(byte[] frame)
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

    }

}
