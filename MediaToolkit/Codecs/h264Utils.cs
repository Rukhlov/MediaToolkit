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
            { //TODO: читать стрим не побайтно, в буфер 
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


		public static byte[] NalToRbsp(byte[] nal, int startIndex = 0)
		{// https://yumichan.net/video-processing/video-compression/introduction-to-h264-nal-unit/
		 //But there are chances that 0x000001 or 0x00000001 exists in the bitstream of a NAL unit.
	     //So a emulation prevention bytes, 0x03, is presented when there is 0x000000, 0x000001, 0x000002 and 0x000003 to make them become 0x00000300, 0x00000301, 0x00000302 and 0x00000303 respectively. 
			List<byte> buf = new List<byte>();
			uint zeroCount = 0;
			uint numBytes = 0;

			for (int i = startIndex; i < nal.Length; ++i)
			{
				byte val = nal[i];

				if (zeroCount >= 2 && val == 3)
				{
					// skip emulation_prevention_three_byte
					zeroCount = 0;
				}
				else
				{
					zeroCount = (val == 0) ? zeroCount + 1 : 0;
					buf.Add(val);

					++numBytes;
				}
			}

			return buf.ToArray();
		}
	}


	/// <summary>
	/// https://titanwolf.org/Network/Articles/Article?AID=3b12175c-8ae2-4a6d-9511-6788d27eab03#gsc.tab=0
	/// https://github.com/cplussharp/graph-studio-next/blob/master/src/H264StructReader.cpp#L32
	/// </summary>
	public class SequenceParameterSet
	{
		public static SequenceParameterSet Parse(byte[] rbsp)
		{
			H264BitStream bs = new H264BitStream(rbsp);

			SequenceParameterSet sps = new SequenceParameterSet();
			sps.Parse(bs);

			return sps;
		}
		public static bool TryParse(byte[] rbsp, out SequenceParameterSet sps)
		{
			bool success = false;
			sps = new SequenceParameterSet();
			try
			{
				H264BitStream bs = new H264BitStream(rbsp);
				sps.Parse(bs);
				success = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				success = false;
			}
			return success;
		}

		public int Profile => profile_idc;
		public int Level => level_idc;

		public int Width
		{
			get
			{
				return ((pic_width_in_mbs_minus1 + 1) * 16) - frame_crop_left_offset * 2 - frame_crop_right_offset * 2;
			}
		}

		public int Height
		{
			get
			{
				return ((2 - frame_mbs_only_flag) * (pic_height_in_map_units_minus1 + 1) * 16) - (frame_crop_top_offset * 2) - (frame_crop_bottom_offset * 2);
			}
		}
		public double MaxFps
		{
			get
			{ //The maximum possible frame rate is always equal to time_scale / (2 * num_units_in_tick),
				//regardless of the value of fixed_frame_rate_flag.
				double fps = 0;
				if (num_units_in_tick > 0 && time_scale > 0)
				{
					fps = time_scale / (2.0 * num_units_in_tick);
				}
				return fps;
			}
		}

		public bool FixedFrameRate => (fixed_frame_rate_flag != 0);
		public Tuple<int, int> TickRate => new Tuple<int, int>(num_units_in_tick, time_scale);

		public string ProfileName
		{
			get
			{
				var constraint_set1 = 0 != (constraint_sets & (1 << 1));
				var constraint_set3 = 0 != (constraint_sets & (1 << 3));
				var constraint_set4 = 0 != (constraint_sets & (1 << 4));
				var constraint_set5 = 0 != (constraint_sets & (1 << 5));

				switch (profile_idc)
				{
					case 44:
						return "CAVLC 4:4:4 Intra";
					case 66:
						return constraint_set1 ? "Constrained Baseline" : "Baseline";
					case 88:
						return "Extended";
					case 77:
						return "Main";
					case 244:
						return constraint_set3 ? "High 4:4:4 Intra" : "High 4:4:4 Predictive";
					case 122:
						return constraint_set3 ? "High 4:2:2 Intra" : "High 4:2:2";
					case 110:
						return constraint_set3 ? "High 10 Intra" : "High 10";
					case 100:
						if (constraint_set4 && constraint_set5)
						{
							return "Constrained High";
						}
						if (constraint_set4)
						{
							return "Progressive High";
						}
						return "High";
				}

				if (constraint_set1)
				{
					return "Constrained Baseline";
				}


				var sb = new StringBuilder();

				sb.AppendFormat("P{0}-CS[", profile_idc);

				for (var i = 0; i < 8; ++i)
				{
					if (0 != (constraint_sets & (1 << i)))
						sb.Append(i);
					else
						sb.Append('.');
				}

				sb.Append(']');

				return sb.ToString();
			}

		}


		public override string ToString()
		{
			return "H264: " + ProfileName + " profile, " + Level + " level, " + Width + "x" + Height + ", " + MaxFps + " fps";
		}

		internal int profile_idc;
		internal int constraint_sets;

		//internal int constraint_set0_flag;
		//internal int constraint_set1_flag;
		//internal int constraint_set2_flag;
		//internal int constraint_set3_flag;
		//internal int constraint_set4_flag;
		//internal int constraint_set5_flag;
		//internal int reserved_zero_2bits;

		internal int level_idc;

		internal int seq_parameter_set_id;
		internal int chroma_format_idc;

		internal int residual_colour_transform_flag;

		internal int bit_depth_luma_minus8;
		internal int bit_depth_chroma_minus8;
		internal int qpprime_y_zero_transform_bypass_flag;
		internal int seq_scaling_matrix_present_flag;

		internal int seq_scaling_list_present_flag;
		internal int log2_max_frame_num_minus4;
		internal int pic_order_cnt_type;
		internal int log2_max_pic_order_cnt_lsb_minus4;


		internal int delta_pic_order_always_zero_flag;
		internal int offset_for_non_ref_pic;
		internal int offset_for_top_to_bottom_field;
		internal int num_ref_frames_in_pic_order_cnt_cycle;

		internal int max_num_ref_frames;
		internal int gaps_in_frame_num_value_allowed_flag;
		internal int pic_width_in_mbs_minus1;
		internal int pic_height_in_map_units_minus1;
		internal int frame_mbs_only_flag;


		internal int mb_adaptive_frame_field_flag;

		internal int direct_8x8_inference_flag;
		internal int frame_cropping_flag;

		internal int frame_crop_left_offset;
		internal int frame_crop_right_offset;
		internal int frame_crop_top_offset;
		internal int frame_crop_bottom_offset;

		internal int vui_parameters_present_flag;

		internal int num_units_in_tick;
		internal int time_scale;
		internal int fixed_frame_rate_flag;
		internal int nal_hrd_parameters_present_flag;
		internal int vcl_hrd_parameters_present_flag;
		internal int pic_struct_present_flag;

		internal int cpb_removal_delay_length_minus1;
		internal int dpb_output_delay_length_minus1;
		internal int time_offset_length;

		internal SequenceParameterSet() { }
		internal SequenceParameterSet(H264BitStream bs)
		{
			this.Parse(bs);
		}

		internal void Parse(H264BitStream bs)
		{
			frame_crop_left_offset = 0;
			frame_crop_right_offset = 0;
			frame_crop_top_offset = 0;
			frame_crop_bottom_offset = 0;

			profile_idc = bs.ReadBits(8);

			constraint_sets = bs.ReadBits(8);

			//constraint_set0_flag = bs.ReadBit();
			//constraint_set1_flag = bs.ReadBit();
			//constraint_set2_flag = bs.ReadBit();
			//constraint_set3_flag = bs.ReadBit();
			//constraint_set4_flag = bs.ReadBit();
			//constraint_set5_flag = bs.ReadBit();
			//reserved_zero_2bits = bs.ReadBits(2);

			level_idc = bs.ReadBits(8);
			seq_parameter_set_id = bs.ReadUE();

			if (profile_idc == 100 || profile_idc == 110 ||
				profile_idc == 122 || profile_idc == 244 || profile_idc == 44 ||
				profile_idc == 83 || profile_idc == 86 || profile_idc == 118)
			{
				chroma_format_idc = bs.ReadUE();

				if (chroma_format_idc == 3)
				{
					residual_colour_transform_flag = bs.ReadBit();
				}
				bit_depth_luma_minus8 = bs.ReadUE();
				bit_depth_chroma_minus8 = bs.ReadUE();
				qpprime_y_zero_transform_bypass_flag = bs.ReadBit();
				seq_scaling_matrix_present_flag = bs.ReadBit();

				if (seq_scaling_matrix_present_flag != 0)
				{// по другому ->> https://sourceforge.net/projects/h264streamanalysis/ (скорее всего не правильно...)
				 // у всех реализовано по разному, х.з насколько этот вариант правильный
					for (var i = 0; i < (3 != chroma_format_idc ? 8 : 12); ++i)
					{
						seq_scaling_list_present_flag = bs.ReadBits(1);

						if (seq_scaling_list_present_flag != 0)
						{
							//TODO:...
							if (i < 6)
							{
								//ReadScalingList(bs, pps.ScalingList4x4[i], 16, &(pps.UseDefaultScalingMatrix4x4Flag[i]));                             
								ParseScalingList(bs, 16);
							}
							else
							{
								//ReadScalingList(bs, pps.ScalingList8x8[i - 6], 64, &(pps.UseDefaultScalingMatrix8x8Flag[i - 6]));
								ParseScalingList(bs, 64);
							}
						}

					}
				}

				//if (seq_scaling_matrix_present_flag != 0)
				//{
				//    int i = 0;
				//    for (i = 0; i < 8; i++)
				//    {
				//        seq_scaling_list_present_flag = bs.ReadBit();
				//        if (seq_scaling_list_present_flag != 0)
				//        {
				//            int sizeOfScalingList = (i < 6) ? 16 : 64;
				//            int lastScale = 8;
				//            int nextScale = 8;
				//            int j = 0;
				//            for (j = 0; j < sizeOfScalingList; j++)
				//            {
				//                if (nextScale != 0)
				//                {
				//                    int delta_scale = bs.ReadSE();
				//                    nextScale = (lastScale + delta_scale + 256) % 256;
				//                }
				//                lastScale = (nextScale == 0) ? lastScale : nextScale;
				//            }
				//        }
				//    }
				//}
			}

			log2_max_frame_num_minus4 = bs.ReadUE();
			pic_order_cnt_type = bs.ReadUE();
			if (pic_order_cnt_type == 0)
			{
				log2_max_pic_order_cnt_lsb_minus4 = bs.ReadUE();
			}
			else if (pic_order_cnt_type == 1)
			{
				delta_pic_order_always_zero_flag = bs.ReadBit();
				offset_for_non_ref_pic = bs.ReadSE();
				offset_for_top_to_bottom_field = bs.ReadSE();
				num_ref_frames_in_pic_order_cnt_cycle = bs.ReadUE();
				int i;
				for (i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; i++)
				{
					bs.ReadSE();
				}
			}
			max_num_ref_frames = bs.ReadUE();
			gaps_in_frame_num_value_allowed_flag = bs.ReadBit();
			pic_width_in_mbs_minus1 = bs.ReadUE();
			pic_height_in_map_units_minus1 = bs.ReadUE();
			frame_mbs_only_flag = bs.ReadBit();
			if (frame_mbs_only_flag == 0)
			{
				mb_adaptive_frame_field_flag = bs.ReadBit();
			}
			direct_8x8_inference_flag = bs.ReadBit();
			frame_cropping_flag = bs.ReadBit();
			if (frame_cropping_flag != 0)
			{
				frame_crop_left_offset = bs.ReadUE();
				frame_crop_right_offset = bs.ReadUE();
				frame_crop_top_offset = bs.ReadUE();
				frame_crop_bottom_offset = bs.ReadUE();
			}

			vui_parameters_present_flag = bs.ReadBit();
			if (vui_parameters_present_flag != 0)
			{
				VuiParameters(bs);
			}
		}

		private void ParseScalingList(H264BitStream bs, int sizeOfScalingList)
		{
			var lastScale = 8;
			var nextScale = 8;

			for (var j = 0; j < sizeOfScalingList; ++j)
			{
				if (0 != nextScale)
				{
					var delta_scale = bs.ReadSE();

					nextScale = (lastScale + delta_scale + 256) & 0xff;
					var useDefaultScalingMatrixFlag = (0 == j && 0 == nextScale);
				}

				var scalingList = 0 == nextScale ? lastScale : nextScale;
				lastScale = scalingList;
			}
		}

		private void VuiParameters(H264BitStream bs)
		{
			var aspect_ratio_info_present_flag = bs.ReadBit();

			if (aspect_ratio_info_present_flag != 0)
			{
				var aspect_ratio_idc = bs.ReadBits(8);
				if (aspect_ratio_idc == 255) //Extended_SAR
				{
					var sar_width = bs.ReadBits(16);
					var sar_height = bs.ReadBits(16);
				}
			}

			var overscan_info_present_flag = bs.ReadBit();
			if (overscan_info_present_flag != 0)
			{
				var overscan_appropriate_flag = bs.ReadBit();
			}

			var video_signal_type_present_flag = bs.ReadBit();
			if (video_signal_type_present_flag != 0)
			{
				var video_format = bs.ReadBits(3);
				var video_full_range_flag = bs.ReadBit();
				var colour_description_present_flag = bs.ReadBit();
				if (colour_description_present_flag != 0)
				{
					var colour_primaries = bs.ReadBits(8);
					var transfer_characteristics = bs.ReadBits(8);
					var matrix_coefficients = bs.ReadBits(8);
				}
			}

			var chroma_loc_info_present_flag = bs.ReadBit();
			if (chroma_loc_info_present_flag != 0)
			{
				var chroma_sample_loc_type_top_field = bs.ReadUE();
				var chroma_sample_loc_type_bottom_field = bs.ReadUE();
			}

			var timing_info_present_flag = bs.ReadBit();
			if (timing_info_present_flag != 0)
			{
				num_units_in_tick = bs.ReadBits(32);
				time_scale = bs.ReadBits(32);
				fixed_frame_rate_flag = bs.ReadBit();

			}

			nal_hrd_parameters_present_flag = bs.ReadBit();

			if (nal_hrd_parameters_present_flag != 0)
			{
				HrdParameters(bs);
			}


			vcl_hrd_parameters_present_flag = bs.ReadBits(1);

			if (vcl_hrd_parameters_present_flag != 0)
			{
				HrdParameters(bs);
			}


			if (nal_hrd_parameters_present_flag != 0 || vcl_hrd_parameters_present_flag != 0)
			{
				var low_delay_hrd_flag = bs.ReadBits(1);
			}

			pic_struct_present_flag = bs.ReadBits(1);

			var bitstream_restriction_flag = bs.ReadBits(1);
			if (bitstream_restriction_flag != 0)
			{
				var motion_vectors_over_pic_boundaries_flag = bs.ReadBits(1);
				var max_bytes_per_pic_denom = bs.ReadUE();
				var max_bits_per_mb_denom = bs.ReadUE();
				var log2_max_mv_length_horizontal = bs.ReadUE();
				var log2_max_mv_length_vertical = bs.ReadUE();
				var max_num_reorder_frames = bs.ReadUE();
				var max_dec_frame_buffering = bs.ReadUE();
			}
		}

		private void HrdParameters(H264BitStream bs)
		{
			var cpb_cnt_minus1 = bs.ReadUE();
			var bit_rate_scale = bs.ReadBits(4);
			var cpb_size_scale = bs.ReadBits(4);
			for (var SchedSelIdx = 0; SchedSelIdx <= cpb_cnt_minus1; SchedSelIdx++)
			{
				var bit_rate_value_minus1_SchedSelIdx = bs.ReadUE();
				var cpb_size_value_minus1_SchedSelIdx = bs.ReadUE();
				var cbr_flag_SchedSelIdx = bs.ReadBits(1);
			}
			var initial_cpb_removal_delay_length_minus1 = bs.ReadBits(5);
			cpb_removal_delay_length_minus1 = bs.ReadBits(5);
			dpb_output_delay_length_minus1 = bs.ReadBits(5);

			time_offset_length = bs.ReadBits(5);
		}
	}

	public class PictureParameterSet
	{
		//...
	}

	class H264BitStream
	{ // https://sourceforge.net/projects/h264streamanalysis/
		public H264BitStream(byte[] data, int startIndex = 0)
		{
			if (startIndex >= data.Length)
			{
				throw new ArgumentOutOfRangeException("startIndex == " + startIndex);
			}

			bytes = data;
			index = startIndex;
			bitsLeft = 8;
		}

		private readonly byte[] bytes;
		private int index = 0;
		private int bitsLeft = 0;

		public int ReadBit()
		{
			bitsLeft--;
			if (index >= bytes.Length)
			{// EOF
			 //return -1;
				throw new InvalidOperationException("EOF");
			}

			int val = (bytes[index] >> (bitsLeft)) & 0x01;

			if (bitsLeft == 0)
			{
				index++;
				bitsLeft = 8;
			}

			return val;
		}

		public int ReadBits(int n)
		{
			int val = 0;
			for (int i = 0; i < n; i++)
			{
				val |= (ReadBit() << (n - i - 1));
			}
			return val;
		}

		public int ReadUE()
		{
			int val = 0;
			int i = 0;

			while ((ReadBit() == 0) && (i < 32)) { i++; }

			val = ReadBits(i);
			val += (1 << i) - 1;
			return val;
		}

		public int ReadSE()
		{
			int val = ReadUE();
			if ((val & 0x01) != 0)
			{
				val = (val + 1) / 2;
			}
			else
			{
				val = -(val / 2);
			}
			return val;
		}
	}

}
