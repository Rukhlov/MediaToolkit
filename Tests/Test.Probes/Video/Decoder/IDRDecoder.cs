using MediaToolkit.Codecs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Test.Probe;

//using UnityEngine;

//https://yumichan.net/video-processing/video-compression/introduction-to-h264-nal-unit/
//https://www.programmersought.com/article/24704507568/
public class IDRDecoder// : MonoBehaviour
{
    public static void Run()
    {

        string fileName = @"Files\IFrame_1920x1080_yuv420p.h264";
        //string fileName = @"Files\testsrc_1280x720_yuv420p_Iframe.h264";
       //  string fileName = @"Files\testsrc_320x240_yuv420p_1sec.h264";
        List<byte[]> nalUnits = new List<byte[]>();

        var total = File.ReadAllBytes(fileName);
        var nals = NalUnitReader.HandleH264AnnexbFrames(total);
        foreach (var nal in nals)
        {
            var array = nal.Array;
            var offset = nal.Offset;
            var nalType = array[offset] & 0x1F;
            Console.WriteLine("NalType: " + nalType + " " + nal.Count);
        }
        Console.WriteLine("--------------------------------");

        var _fileStream = File.Open(fileName, FileMode.Open);
        var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 0, 4, 4, 4, 4, 0, 0, 0, 5, 5, 5, 5, 0, 0, 0, 6, 6, 6, 6 };
        //var srcarray = new byte[] { 0, 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
        //var srcarray = new byte[] { 0, 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
        // var srcarray = new byte[] { 0, 1, 3, 3, 3, 3, 0, 0, 0, 1, 4, 4, 4, 4, 0, 0, 1, 5, 5, 5, 5, 0, 0, 1, 6, 6, 6, 6 };
        const int EndOfStream = -1;
        MemoryStream stream = new MemoryStream(srcarray);


        var nalUnitBytesList = new List<byte>(102400);
        var nalReader = new NalUnitReader(_fileStream);

        //parser._InitialiseInputBuffer();
        var dataAvailable = false;
        do
        {
            dataAvailable = nalReader.ReadNext(out var nal);
            if (nal != null && nal.Length > 0)
            {
                nalUnits.Add(nal);
            }

        } while (dataAvailable);


        foreach(var nal in nalUnits)
        {
            var nalType = nal[0] & 0x1F;
            Console.WriteLine("NalType: " + nalType  + " " + nal.Length);
        }

        _fileStream.Close();

        //IDRDecoder decoder = new IDRDecoder();

        //decoder.Parse(fileName);


    }


    public void Parse(string fileName)
    {

        List<NALU> NALUs = GetNALU_ReadFile(fileName);

        NALU SPS_NALU = GetNALU(NALUs, 0x67);
        NALU PPS_NALU = GetNALU(NALUs, 0x68);
        NALU IDR_NALU = GetNALU(NALUs, 0x65);

        SPS sps = SPS_Decoder(SPS_NALU);
        PPS pps = PPS_Decoder(PPS_NALU);
        IDR idr = IDR_Decoder(sps, pps, IDR_NALU);

        Print_SPS(sps);
        Print_PPS(pps);
        Print_IDR(sps, pps, idr, IDR_NALU);
    }

    List<NALU> GetNALU_ReadFile(string H264_File_Path)
    {
        byte[] b = System.IO.File.ReadAllBytes(H264_File_Path);
        List<NALU> NALU_List = new List<NALU>();
        int i = 0;
        while (i < b.Length)
        {
            if (b[i] == 0x00 && b[i + 1] == 0x00 && b[i + 2] == 0x01)
            {
                i += 3;
                NALU nalu = new NALU();
                nalu.NAL_Header = b[i];
                nalu.forbidden_zero_bit = nalu.NAL_Header & 0x80;
                nalu.nal_ref_idc = nalu.NAL_Header & 0x60;
                nalu.nal_unit_type = nalu.NAL_Header & 0x1F;
                i += 1;
                nalu.RBSP = new List<byte>();
                do
                {
                    nalu.RBSP.Add(b[i]);
                    i += 1;
                    if (i == b.Length - 3)
                    {
                        nalu.RBSP.Add(b[i + 1]);
                        nalu.RBSP.Add(b[i + 2]);
                        break;
                    }
                } while ((b[i] == 0x00 && b[i + 1] == 0x00 && b[i + 2] == 0x01) == false);
                NALU_List.Add(nalu);
            }
            else
            {
                i += 1;
            }
        }
        return NALU_List;
    }

    void print(string str)
    {
        Console.WriteLine(str);
    }

    void Print_SPS(SPS sps)
    {
        print("-------------------------------------------------------------------------------");
        print("[ SPS ]");
        print("        profile_idc = " + sps.profile_idc);
        print("        level_idc = " + sps.level_idc);
        print("        seq_parameter_set_id = " + sps.seq_parameter_set_id);
        print("        chroma_format_idc = " + sps.chroma_format_idc);
        print("        bit_depth_luma_minus8 = " + sps.bit_depth_luma_minus8);
        print("        bit_depth_chroma_minus8 = " + sps.bit_depth_chroma_minus8);
        print("        qpprime_y_zero_transform_bypass_flag = " + sps.qpprime_y_zero_transform_bypass_flag);
        print("        seq_scaling_matrix_present_flag = " + sps.seq_scaling_matrix_present_flag);
        print("        log2_max_frame_num_minus4 = " + sps.log2_max_frame_num_minus4);
        print("        pic_order_cnt_type = " + sps.pic_order_cnt_type);
        print("        log2_max_pic_order_cnt_lsb_minus4 = " + sps.log2_max_pic_order_cnt_lsb_minus4);
        print("        max_num_ref_frames = " + sps.max_num_ref_frames);
        print("        gaps_in_frame_num_value_allowed_flag = " + sps.gaps_in_frame_num_value_allowed_flag);
        print("        pic_width_in_mbs_minus1 = " + sps.pic_width_in_mbs_minus1);
        print("        pic_height_in_map_units_minus1 = " + sps.pic_height_in_map_units_minus1);
        print("        frame_mbs_only_flag = " + sps.frame_mbs_only_flag);
        if (sps.frame_mbs_only_flag == 0)
        {
            print("        mb_adaptive_frame_field_flag = " + sps.mb_adaptive_frame_field_flag);
        }
        print("        direct_8x8_inference_flag = " + sps.direct_8x8_inference_flag);
        print("        frame_cropping_flag = " + sps.frame_cropping_flag);
        if (sps.frame_cropping_flag == 1)
        {
            print("        frame_crop_left_offset = " + sps.frame_crop_left_offset);
            print("        frame_crop_right_offset = " + sps.frame_crop_right_offset);
            print("        frame_crop_top_offset = " + sps.frame_crop_top_offset);
            print("        frame_crop_bottom_offset = " + sps.frame_crop_bottom_offset);
        }
        print("        vui_parameters_present_flag = " + sps.vui_parameters_present_flag);
    }

    void Print_PPS(PPS pps)
    {
        print("-------------------------------------------------------------------------------");
        print("[ PPS ]");
        print("        pic_parameter_set_id = " + pps.pic_parameter_set_id);
        print("        seq_parameter_set_id = " + pps.seq_parameter_set_id);
        print("        entropy_coding_mode_flag = " + pps.entropy_coding_mode_flag);
        print("        pic_order_present_flag = " + pps.pic_order_present_flag);
        print("        num_slice_groups_minus1 = " + pps.num_slice_groups_minus1);
        print("        num_ref_idx_l0_active_minus1 = " + pps.num_ref_idx_l0_active_minus1);
        print("        num_ref_idx_l1_active_minus1 = " + pps.num_ref_idx_l1_active_minus1);
        print("        weighted_pred_flag = " + pps.weighted_pred_flag);
        print("        weighted_bipred_idc = " + pps.weighted_bipred_idc);
        print("        pic_init_qp_minus26 = " + pps.pic_init_qp_minus26);
        print("        pic_init_qs_minus26 = " + pps.pic_init_qs_minus26);
        print("        chroma_qp_index_offset = " + pps.chroma_qp_index_offset);
        print("        deblocking_filter_control_present_flag = " + pps.deblocking_filter_control_present_flag);
        print("        constrained_intra_pred_flag = " + pps.constrained_intra_pred_flag);
        print("        redundant_pic_cnt_present_flag = " + pps.redundant_pic_cnt_present_flag);
    }

    void Print_IDR(SPS sps, PPS pps, IDR idr, NALU nalu)
    {
        // --------------------------------------------
        // Refer to chapter 7.4.3 of the H.264 official manual
        // determine which type of slice_type is I, P, B ...
        string slice_type_format = "-";
        switch (idr.slice_type)
        {
            case 0:
            case 5:
                slice_type_format = "P Slice";
                break;
            case 1:
            case 6:
                slice_type_format = "B Slice";
                break;
            case 2:
            case 7:
                slice_type_format = "I Slice";
                break;
            case 3:
            case 8:
                slice_type_format = "SP Slice";
                break;
            case 4:
            case 9:
                slice_type_format = "SI Slice";
                break;
        }

        // --------------------------------------------


        print("-------------------------------------------------------------------------------");
        print("[ IDR ]");
        print("        first_mb_in_slice : " + idr.first_mb_in_slice);
        print("        slice_type : " + idr.slice_type + " ( " + slice_type_format + " )");
        print("        pic_parameter_set_id : " + idr.pic_parameter_set_id);
        print("        frame_num : " + idr.frame_num);
        if (sps.frame_mbs_only_flag == 0)
        {
            print("        field_pic_flag : " + idr.field_pic_flag);
            if (idr.field_pic_flag == 1)
            {
                print("        bottom_field_flag : " + idr.bottom_field_flag);
            }
        }
        if (nalu.nal_unit_type == 5)
        {
            print("        idr_pic_id : " + idr.idr_pic_id);
        }
        if (sps.pic_order_cnt_type == 0)
        {
            print("        pic_order_cnt_lsb : " + idr.pic_order_cnt_lsb);
            if (pps.pic_order_present_flag == 1 && idr.field_pic_flag == 0)
            {
                print("        delta_pic_order_cnt_bottom : " + idr.delta_pic_order_cnt_bottom);
            }
        }

        print("        dec_ref_pic_marking() {");
        print("                no_output_of_prior_pics_flag : " + idr.dec_ref_pic_marking.no_output_of_prior_pics_flag);
        print("                long_term_reference_flag     : " + idr.dec_ref_pic_marking.long_term_reference_flag);
        print("        }");


        print("        slice_qp_delta : " + idr.slice_qp_delta);
    }


    unsafe SPS SPS_Decoder(NALU nalu)
    {
        SPS sps = new SPS();

        int bytePosition = 3, bitPosition = 0;

        byte[] strArray = nalu.RBSP.ToArray();

        sps.profile_idc = strArray[0];
        sps.level_idc = strArray[2];
        sps.seq_parameter_set_id = get_uev_code_num(strArray, &bytePosition, &bitPosition);

        if (sps.profile_idc == 100 || sps.profile_idc == 110 || sps.profile_idc == 122 || sps.profile_idc == 244
            || sps.profile_idc == 44 || sps.profile_idc == 83 || sps.profile_idc == 86 || sps.profile_idc == 118 || sps.profile_idc == 128)
        {
            sps.chroma_format_idc = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            if (sps.chroma_format_idc == 3)
            {
                sps.separate_colour_plane_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            }
            sps.bit_depth_luma_minus8 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            sps.bit_depth_chroma_minus8 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            sps.qpprime_y_zero_transform_bypass_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            sps.seq_scaling_matrix_present_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            if (sps.seq_scaling_matrix_present_flag == 1)
            {
                throw new Exception("Files in this format are not supported");
            }
        }
        sps.log2_max_frame_num_minus4 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        sps.pic_order_cnt_type = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        if (sps.pic_order_cnt_type == 0)
        {
            sps.log2_max_pic_order_cnt_lsb_minus4 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        }
        else if (sps.pic_order_cnt_type == 1)
        {
            throw new Exception("Files in this format are not supported");
        }
        sps.max_num_ref_frames = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        sps.gaps_in_frame_num_value_allowed_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        sps.pic_width_in_mbs_minus1 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        sps.pic_height_in_map_units_minus1 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        sps.frame_mbs_only_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        if (sps.frame_mbs_only_flag == 0)
        {
            sps.mb_adaptive_frame_field_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        }
        sps.direct_8x8_inference_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        sps.frame_cropping_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        if (sps.frame_cropping_flag == 1)
        {
            sps.frame_crop_left_offset = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            sps.frame_crop_right_offset = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            sps.frame_crop_top_offset = get_uev_code_num(strArray, &bytePosition, &bitPosition);
            sps.frame_crop_bottom_offset = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        }
        sps.vui_parameters_present_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        if (sps.vui_parameters_present_flag == 1)
        {
        }


        return sps;
    }

    unsafe PPS PPS_Decoder(NALU nalu)
    {
        PPS pps = new PPS();

        int bytePosition = 0, bitPosition = 0;

        byte[] strArray = nalu.RBSP.ToArray();

        pps.pic_parameter_set_id = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        pps.seq_parameter_set_id = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        pps.entropy_coding_mode_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.pic_order_present_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.num_slice_groups_minus1 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        if (pps.num_slice_groups_minus1 > 0)
        {
            throw new Exception("Files in this format are not supported");
        }
        pps.num_ref_idx_l0_active_minus1 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        pps.num_ref_idx_l1_active_minus1 = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        pps.weighted_pred_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.weighted_bipred_idc = get_bit_at_position(strArray, &bytePosition, &bitPosition) << 1 + get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.pic_init_qp_minus26 = get_sev_code_num(strArray, &bytePosition, &bitPosition);
        pps.pic_init_qs_minus26 = get_sev_code_num(strArray, &bytePosition, &bitPosition);
        pps.chroma_qp_index_offset = get_sev_code_num(strArray, &bytePosition, &bitPosition);
        pps.deblocking_filter_control_present_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.constrained_intra_pred_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
        pps.redundant_pic_cnt_present_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);

        return pps;
    }

    unsafe IDR IDR_Decoder(SPS sps, PPS pps, NALU nalu)
    {
        IDR idr = new IDR();

        int bytePosition = 0, bitPosition = 0;

        byte[] strArray = nalu.RBSP.ToArray();

        idr.first_mb_in_slice = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        idr.slice_type = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        idr.pic_parameter_set_id = get_uev_code_num(strArray, &bytePosition, &bitPosition);

        idr.frame_num = get_uint_code_num(strArray, &bytePosition, &bitPosition, sps.log2_max_frame_num_minus4 + 4);  // something wrong

        if (sps.frame_mbs_only_flag == 0)
        {
            idr.field_pic_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            if (idr.field_pic_flag == 1)
            {
                idr.bottom_field_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            }
        }

        if (nalu.nal_unit_type == 5)
        {
            idr.idr_pic_id = get_uev_code_num(strArray, &bytePosition, &bitPosition);
        }

        if (sps.pic_order_cnt_type == 0)
        {
            idr.pic_order_cnt_lsb = get_uint_code_num(strArray, &bytePosition, &bitPosition, sps.log2_max_pic_order_cnt_lsb_minus4 + 4); // something wrong
            if (pps.pic_order_present_flag == 1 && idr.field_pic_flag == 0)
            {
                idr.delta_pic_order_cnt_bottom = get_sev_code_num(strArray, &bytePosition, &bitPosition);
            }
        }

        if (nalu.nal_ref_idc != 0)
        {
            // Enter dec_ref_pic_marking() H264 function (simplified version)
            if (nalu.nal_unit_type == 5)
            {
                idr.dec_ref_pic_marking.no_output_of_prior_pics_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
                idr.dec_ref_pic_marking.long_term_reference_flag = get_bit_at_position(strArray, &bytePosition, &bitPosition);
            }
        }

        idr.slice_qp_delta = get_sev_code_num(strArray, &bytePosition, &bitPosition);

        return idr;
    }

    // -------------------------------------------------
    // H.264 Function

    unsafe int get_bit_at_position(byte[] buf, int* bytePotion, int* bitPosition)
    {
        int mask = 0, val = 0;

        mask = 1 << (7 - *bitPosition);

        val = ((buf[*bytePotion] & mask) != 0) ? 1 : 0;

        if (++*bitPosition > 7)
        {
            *bytePotion = *bytePotion + 1;
            *bitPosition = 0;
        }

        return val;
    }

    unsafe int get_uev_code_num(byte[] buf, int* bytePotion, int* bitPosition)
    {
        int val = 0, prefixZeroCount = 0;
        int prefix = 0, surfix = 0;

        while (true)
        {
            val = get_bit_at_position(buf, bytePotion, bitPosition);
            if (val == 0)
            {
                prefixZeroCount++;
            }
            else
            {
                break;
            }
        }

        prefix = (1 << prefixZeroCount) - 1;
        for (int i = 0; i < prefixZeroCount; i++)
        {
            val = get_bit_at_position(buf, bytePotion, bitPosition);
            surfix += val * (1 << (prefixZeroCount - i - 1));
        }

        prefix += surfix;

        return prefix;
    }


    unsafe int get_sev_code_num(byte[] buf, int* bytePotion, int* bitPosition)
    {
        int uev = get_uev_code_num(buf, bytePotion, bitPosition);
        int sign = uev % 2 == 0 ? -1 : 1;
        int sev = sign * ((uev + 1) >> 1);
        return sev;
    }

    unsafe int get_uint_code_num(byte[] buf, int* bytePotion, int* bitPosition, int length)
    {
        int uVal = 0;
        for (int idx = 0; idx < length; idx++)
        {
            uVal += get_bit_at_position(buf, bytePotion, bitPosition) << (length - idx - 1);
        }
        return uVal;
    }

    // -------------------------------------------------
    // Tool :

    // only find the nearest header
    NALU GetNALU(List<NALU> NALU_List, int header)
    {
        NALU _nalu = new NALU();
        bool isFind = false;
        foreach (NALU nalu in NALU_List)
        {
            if (nalu.NAL_Header == header)
            {
                _nalu = nalu;
                isFind = true;
                break;
            }
        }
        if (isFind == false)
        {
            throw new Exception("The specified NAL Header was not found");
        }
        return _nalu;
    }


    // -------------------------------------------------
    // Struct :

    struct NALU
    {
        // +---------------+
        // |  NAL_Header   |
        // +---------------+
        // |F|NRI| Type    |
        // +-+-+-+-+-+-+-+-+
        // |0|1|2|3|4|5|6|7|
        // +---------------+
        //
        // F = forbidden_zero_bit: (H264 specification variable)
        // NRI = nal_ref_idc: (H264 specification variable)
        // Type = nal_unit_type: (H264 specification variable)
        //
        // NAL_Header = F + NRI + Type

        public byte NAL_Header;

        public int forbidden_zero_bit;
        public int nal_ref_idc;
        public int nal_unit_type;

        public List<byte> RBSP;
    }

    struct SPS
    {
        public int profile_idc;
        public int level_idc;
        public int seq_parameter_set_id;
        public int chroma_format_idc;
        public int separate_colour_plane_flag;
        public int bit_depth_luma_minus8;
        public int bit_depth_chroma_minus8;
        public int qpprime_y_zero_transform_bypass_flag;
        public int seq_scaling_matrix_present_flag;
        public int log2_max_frame_num_minus4;
        public int pic_order_cnt_type;
        public int log2_max_pic_order_cnt_lsb_minus4;
        public int max_num_ref_frames;
        public int gaps_in_frame_num_value_allowed_flag;
        public int pic_width_in_mbs_minus1;
        public int pic_height_in_map_units_minus1;
        public int frame_mbs_only_flag;
        public int mb_adaptive_frame_field_flag;
        public int direct_8x8_inference_flag;
        public int frame_cropping_flag;
        public int frame_crop_left_offset;
        public int frame_crop_right_offset;
        public int frame_crop_top_offset;
        public int frame_crop_bottom_offset;
        public int vui_parameters_present_flag;
    }

    struct PPS
    {
        public int pic_parameter_set_id;
        public int seq_parameter_set_id;
        public int entropy_coding_mode_flag;
        public int pic_order_present_flag;
        public int num_slice_groups_minus1;
        public int num_ref_idx_l0_active_minus1;
        public int num_ref_idx_l1_active_minus1;
        public int weighted_pred_flag;
        public int weighted_bipred_idc;
        public int pic_init_qp_minus26;
        public int pic_init_qs_minus26;
        public int chroma_qp_index_offset;
        public int deblocking_filter_control_present_flag;
        public int constrained_intra_pred_flag;
        public int redundant_pic_cnt_present_flag;
    }

    struct IDR
    {
        public int first_mb_in_slice;
        public int slice_type;
        public int pic_parameter_set_id;
        public int frame_num;
        public int field_pic_flag;
        public int bottom_field_flag;
        public int idr_pic_id;
        public int pic_order_cnt_lsb;
        public int delta_pic_order_cnt_bottom;
        public int slice_qp_delta;

        public IDR_SubFunction__Dec_Ref_Pic_Marking dec_ref_pic_marking;
    }

    // IDR Sub
    struct IDR_SubFunction__Dec_Ref_Pic_Marking
    {
        public int no_output_of_prior_pics_flag;
        public int long_term_reference_flag;
    }



}


public static class StreamExtensions
{

    public static int ScanUntilFound(this Stream stream, byte[] searchBytes)
    {
        // For this class code comments, a common example is assumed:
        // searchBytes are {1,2,3,4} or 1234 for short
        // # means value that is outside of search byte sequence

        byte[] streamBuffer = new byte[searchBytes.Length];
        int nextRead = searchBytes.Length;
        int totalScannedBytes = 0;

        while (true)
        {
            FillBuffer(stream, streamBuffer, nextRead);
            totalScannedBytes += nextRead; //this is only used for final reporting of where it was found in the stream

            if (ArraysMatch(searchBytes, streamBuffer, 0))
                return totalScannedBytes; //found it

            nextRead = FindPartialMatch(searchBytes, streamBuffer);
        }
    }


    static int FindPartialMatch(byte[] searchBytes, byte[] streamBuffer)
    {
        // 1234 = 0 - found it. this special case is already catered directly in ScanUntilFound            
        // #123 = 1 - partially matched, only missing 1 value
        // ##12 = 2 - partially matched, only missing 2 values
        // ###1 = 3 - partially matched, only missing 3 values
        // #### = 4 - not matched at all

        for (int i = 1; i < searchBytes.Length; i++)
        {
            if (ArraysMatch(searchBytes, streamBuffer, i))
            {
                // EG. Searching for 1234, have #123 in the streamBuffer, and [i] is 1
                // Output: 123#, where # will be read using FillBuffer next. 
                Array.Copy(streamBuffer, i, streamBuffer, 0, searchBytes.Length - i);
                return i; //if an offset of [i], makes a match then only [i] bytes need to be read from the stream to check if there's a match
            }
        }

        return 4;
    }


    static void FillBuffer(Stream stream, byte[] streamBuffer, int bytesNeeded)
    {
        // EG1. [123#] - bytesNeeded is 1, when the streamBuffer contains first three matching values, but now we need to read in the next value at the end 
        // EG2. [####] - bytesNeeded is 4

        var bytesAlreadyRead = streamBuffer.Length - bytesNeeded; //invert
        while (bytesAlreadyRead < streamBuffer.Length)
        {
            bytesAlreadyRead += stream.Read(streamBuffer, bytesAlreadyRead, streamBuffer.Length - bytesAlreadyRead);
        }
    }


    static bool ArraysMatch(byte[] searchBytes, byte[] streamBuffer, int startAt)
    {
        for (int i = 0; i < searchBytes.Length - startAt; i++)
        {
            if (searchBytes[i] != streamBuffer[i + startAt])
                return false;
        }
        return true;
    }
}

public static class StreamHelper
{

    public static long IndexOf(this Stream stream, byte[] pattern)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentOutOfRangeException("stream");
        }
        if (pattern == null || pattern.Length == 0)
        {
            throw new ArgumentNullException("pattern");
        }

        long position = stream.Position;
        List<int> list = new List<int>();
        int num;
        while ((num = stream.ReadByte()) != -1)
        {
            if (num == (int)pattern[0])
            {
                list.Add(0);
            }
            for (int i = 0; i < list.Count; i++)
            {
                if (num == (int)pattern[list[i]])
                {
                    List<int> list2;
                    int index;
                    (list2 = list)[index = i] = list2[index] + 1;
                    if (list[i] == pattern.Length)
                    {
                        long result = stream.Position - (long)pattern.Length + 1L;
                        stream.Seek(position, SeekOrigin.Begin);
                        return result;
                    }
                }
                else
                {
                    list[i] = -1;
                }
            }
            list.RemoveAll((int x) => x == -1);
        }
        stream.Seek(position, SeekOrigin.Begin);
        return -1L;
    }
}


public static class __StreamExtensions
{
    public static IEnumerable<long> ScanAOB(this Stream stream, params byte[] aob)
    {
        long position;
        byte[] buffer = new byte[aob.Length - 1];

        while ((position = stream.Position) < stream.Length)
        {
            if (stream.ReadByte() != aob[0]) continue;
            if (stream.Read(buffer, 0, aob.Length - 1) == 0) continue;

            if (buffer.SequenceEqual(aob.Skip(1)))
            {
                yield return position;
            }
        }
    }

    public static IEnumerable<long> ScanAOB(this Stream stream, params byte?[] aob)
    {
        long position;
        byte[] buffer = new byte[aob.Length - 1];

        while ((position = stream.Position) < stream.Length)
        {
            if (stream.ReadByte() != aob[0]) continue;
            if (stream.Read(buffer, 0, aob.Length - 1) == 0) continue;

            if (buffer.Cast<byte?>().SequenceEqual(aob.Skip(1), new AobComparer()))
            {
                yield return position;
            }
        }
    }

    private class AobComparer : IEqualityComparer<byte?>
    {
        public bool Equals(byte? x, byte? y) => x == null || y == null || x == y;
        public int GetHashCode(byte? obj) => obj?.GetHashCode() ?? 0;
    }
}

