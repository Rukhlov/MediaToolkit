using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Probe
{

    public class ___NalUnitBitstreamParser
    {

        private const int NalUnitStartSequenceMaxLength = 4;

        private Stream _reader;



        public ___NalUnitBitstreamParser(Stream reader)
        {
            _reader = reader;
        }



        public int BufferIndex { get; set; }
        public int CountOfBufferedBytes { get; set; } = 0;
        public byte[] InputBuffer { get; set; } = new byte[NalUnitStartSequenceMaxLength];
        public bool IsBufferFull => CountOfBufferedBytes >= NalUnitStartSequenceMaxLength;

        /// <summary>
        /// Determines if the start of a NAL Unit has been found.
        /// </summary>
        /// <remarks>
        /// Reference:
        /// B.2.1 Byte stream NAL unit syntax - and - B.3 Byte stream NAL unit decoding process
        /// while( next_bits( 24 ) != 0x000001 &amp;&amp; next_bits( 32 ) != 0x00000001 )
        /// </remarks>
        public bool IsNalUnitFound => (InputBuffer[0] == 0x0 && InputBuffer[1] == 0x0 && InputBuffer[2] == 0x1)
                   || (IsBufferFull && InputBuffer[0] == 0x0 && InputBuffer[1] == 0x0 && InputBuffer[2] == 0x0 && InputBuffer[3] == 0x1);


        /// <summary>
        /// Extract the bytes of the NAL Unit into the given collection
        /// </summary>
        /// <param name="nalUnitBytes"></param>
        /// <remarks>
        /// Reference: B.3 Byte stream NAL unit decoding process
        ///
        /// NumBytesInNalUnit is set equal to the number of bytes starting with the byte at the current position in the byte
        ///   stream up to and including the last byte that precedes the location of one or more of the following conditions:
        ///     – A subsequent byte-aligned three-byte sequence equal to 0x000000,
        ///     – A subsequent byte-aligned three-byte sequence equal to 0x000001,
        ///     – The end of the byte stream, as determined by unspecified means.
        /// </remarks>
        /// <returns>True if there is more data remaining in the input stream.</returns>
        public bool ExtractNalUnitBytesInto(IList<byte> nalUnitBytes)
        {
            int nextByte;
            byte one = 0;
            byte two = 0;
            byte three = 0;

            nextByte = _reader.ReadByte();
            if (nextByte == -1) return false;
            one = (byte)nextByte;
            nalUnitBytes.Add(one);

            nextByte = _reader.ReadByte();
            if (nextByte == -1) return false;
            two = (byte)nextByte;
            nalUnitBytes.Add(two);

            while ((nextByte = _reader.ReadByte()) != -1)
            {
                three = (byte)nextByte;

                if (one == 0x0 && two == 0x0 && three == 0x1) //(three == 0x0 || three == 0x1))
                {
                    // Remove the previous two bytes, as the are the end sequence and are not part of the Nal Unit payload
                    //nalUnitBytes.RemoveAt(nalUnitBytes.Count - 1);
                    //nalUnitBytes.RemoveAt(nalUnitBytes.Count - 1);

                    InputBuffer[0] = one;
                    InputBuffer[1] = two;
                    InputBuffer[2] = three;
                    InputBuffer[3] = 0x0;

                    BufferIndex = 3;
                    CountOfBufferedBytes = 3;

                    return true;    // we did not reach the end of the input stream
                }

                nalUnitBytes.Add(three);

                // Prepare for next iteration
                one = two;
                two = three;
            }

            // No more data - no final 3-byte sequence
            return false;
        }


        public byte[] ReadNext()
        {
            byte[] bytes = null;
            var nalUnitBytesList = new List<byte>(102400);

            while (true)//ReadByteIntoInputBuffer() != -1)
            {
               // if (IsNalUnitFound)
                {
                    // Extract the NAL Unit
                    //nalUnitBytesList.Clear();
                    bool hasMoreDataInInputStream = ExtractNalUnitBytesInto(nalUnitBytesList);

                    // Process the NAL Unit
                    //...
                    bytes = nalUnitBytesList.ToArray();
                    //nalUnits.Add(nalUnitBytesList.ToArray());

                    //if (!hasMoreDataInInputStream)
                    //{
                    //    break;
                    //}

                    break;
                }
            }

            return bytes; 
        }

        public void _InitialiseInputBuffer()
        {
            // Initialise the input buffer with the first 2 bytes
            CountOfBufferedBytes = _reader.Read(InputBuffer, 0, 2);
            BufferIndex = 2;
        }


        public int ReadByteIntoInputBuffer()
        {
            if (IsNalUnitFound)
            {
                return InputBuffer[3];
            }

            int result = _reader.ReadByte();
            if (result != -1)
            {
                InputBuffer[BufferIndex] = (byte)result;
                BufferIndex++;
                CountOfBufferedBytes++;

                if (BufferIndex == 2)
                {
                    BufferIndex = 3;
                }
            }

            return result;
        }

        public int _ReadByteIntoInputBuffer()
        {
            if (IsNalUnitFound)
            {
                return InputBuffer[3];
            }

            int result = _reader.ReadByte();
            if (result != -1)
            {
                InputBuffer[BufferIndex] = (byte)result;
                CountOfBufferedBytes++;

                if (BufferIndex == 2)
                {
                    BufferIndex = 3;
                }
            }

            return result;
        }

        //public void WriteInputBufferTo(Stream writer)
        //{
        //    if (CountOfBufferedBytes > 0)
        //    {
        //        writer.Write(InputBuffer, 0, CountOfBufferedBytes);
        //        CountOfBufferedBytes = 0;

        //        InputBuffer[0] = 0;
        //        InputBuffer[1] = 0;
        //        InputBuffer[2] = 0;
        //        InputBuffer[3] = 0;
        //    }
        //}

        //public void WriteOldestInputBufferByteTo(Stream writer)
        //{
        //    writer.WriteByte(InputBuffer[0]);
        //    CountOfBufferedBytes--;

        //    InputBuffer[0] = InputBuffer[1];
        //    InputBuffer[1] = InputBuffer[2];
        //    InputBuffer[2] = InputBuffer[3];
        //    InputBuffer[3] = 0;
        //}

    }



    public class _NalUnitBitstreamParser
    {

        private const int NalUnitStartSequenceMaxLength = 4;

        private Stream _reader;



        public _NalUnitBitstreamParser(Stream reader)
        {
            _reader = reader;
        }



        public int BufferIndex { get; set; }
        public int CountOfBufferedBytes { get; set; } = 0;
        public byte[] InputBuffer { get; set; } = new byte[NalUnitStartSequenceMaxLength];
        public bool IsBufferFull => CountOfBufferedBytes >= NalUnitStartSequenceMaxLength;

        /// <summary>
        /// Determines if the start of a NAL Unit has been found.
        /// </summary>
        /// <remarks>
        /// Reference:
        /// B.2.1 Byte stream NAL unit syntax - and - B.3 Byte stream NAL unit decoding process
        /// while( next_bits( 24 ) != 0x000001 &amp;&amp; next_bits( 32 ) != 0x00000001 )
        /// </remarks>
        public bool IsNalUnitFound => (InputBuffer[0] == 0x0 && InputBuffer[1] == 0x0 && InputBuffer[2] == 0x1)
                   || (IsBufferFull && InputBuffer[0] == 0x0 && InputBuffer[1] == 0x0 && InputBuffer[2] == 0x0 && InputBuffer[3] == 0x1);


        /// <summary>
        /// Extract the bytes of the NAL Unit into the given collection
        /// </summary>
        /// <param name="nalUnitBytes"></param>
        /// <remarks>
        /// Reference: B.3 Byte stream NAL unit decoding process
        ///
        /// NumBytesInNalUnit is set equal to the number of bytes starting with the byte at the current position in the byte
        ///   stream up to and including the last byte that precedes the location of one or more of the following conditions:
        ///     – A subsequent byte-aligned three-byte sequence equal to 0x000000,
        ///     – A subsequent byte-aligned three-byte sequence equal to 0x000001,
        ///     – The end of the byte stream, as determined by unspecified means.
        /// </remarks>
        /// <returns>True if there is more data remaining in the input stream.</returns>
        public bool ExtractNalUnitBytesInto(IList<byte> nalUnitBytes)
        {
            int nextByte;
            byte one = 0;
            byte two = 0;
            byte three = 0;

            nextByte = _reader.ReadByte();
            if (nextByte == -1) return false;
            one = (byte)nextByte;
            nalUnitBytes.Add(one);

            nextByte = _reader.ReadByte();
            if (nextByte == -1) return false;
            two = (byte)nextByte;
            nalUnitBytes.Add(two);

            while ((nextByte = _reader.ReadByte()) != -1)
            {
                three = (byte)nextByte;

                if (one == 0x0 && two == 0x0 && (three == 0x0 || three == 0x1))
                {
                    // Remove the previous two bytes, as the are the end sequence and are not part of the Nal Unit payload
                    nalUnitBytes.RemoveAt(nalUnitBytes.Count - 1);
                    nalUnitBytes.RemoveAt(nalUnitBytes.Count - 1);

                    InputBuffer[0] = one;
                    InputBuffer[1] = two;
                    InputBuffer[2] = three;
                    InputBuffer[3] = 0x0;

                    BufferIndex = 3;
                    CountOfBufferedBytes = 3;

                    return true;    // we did not reach the end of the input stream
                }

                nalUnitBytes.Add(three);

                // Prepare for next iteration
                one = two;
                two = three;
            }

            // No more data - no final 3-byte sequence
            return false;
        }

        public void _InitialiseInputBuffer()
        {
            // Initialise the input buffer with the first 2 bytes
            CountOfBufferedBytes = _reader.Read(InputBuffer, 0, 2);
            BufferIndex = 2;
        }


        public int ReadByteIntoInputBuffer()
        {
            if (IsNalUnitFound)
            {
                return InputBuffer[3];
            }

            int result = _reader.ReadByte();
            if (result != -1)
            {
                InputBuffer[BufferIndex] = (byte)result;
                BufferIndex++;
                CountOfBufferedBytes++;

                //if (BufferIndex == 2)
                //{
                //    BufferIndex = 3;
                //}
            }

            return result;
        }

        public int _ReadByteIntoInputBuffer()
        {
            if (IsNalUnitFound)
            {
                return InputBuffer[3];
            }

            int result = _reader.ReadByte();
            if (result != -1)
            {
                InputBuffer[BufferIndex] = (byte)result;
                CountOfBufferedBytes++;

                if (BufferIndex == 2)
                {
                    BufferIndex = 3;
                }
            }

            return result;
        }

        //public void WriteInputBufferTo(Stream writer)
        //{
        //    if (CountOfBufferedBytes > 0)
        //    {
        //        writer.Write(InputBuffer, 0, CountOfBufferedBytes);
        //        CountOfBufferedBytes = 0;

        //        InputBuffer[0] = 0;
        //        InputBuffer[1] = 0;
        //        InputBuffer[2] = 0;
        //        InputBuffer[3] = 0;
        //    }
        //}

        //public void WriteOldestInputBufferByteTo(Stream writer)
        //{
        //    writer.WriteByte(InputBuffer[0]);
        //    CountOfBufferedBytes--;

        //    InputBuffer[0] = InputBuffer[1];
        //    InputBuffer[1] = InputBuffer[2];
        //    InputBuffer[2] = InputBuffer[3];
        //    InputBuffer[3] = 0;
        //}

    }
}
