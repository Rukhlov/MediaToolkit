using System;
using System.Runtime.InteropServices;
using static MediaToolkit.Nvidia.LibCuda;


namespace MediaToolkit.Nvidia
{

    public struct YuvInformation
    {
        public int BytesPerPixel { get; set; }
        public int FrameByteSize { get; set; }
        public int LumaHeight { get; set; }
        public int LumaPitch { get; set; }
        public int ChromaHeight { get; set; }
        public int ChromaOffset { get; set; }
        public int ChromaPitch { get; set; }
        public int ChromaPlaneCount { get; set; }
    }

    public struct CuVideoChromaFormatInformation
    {
        public float HeightFactor;
        public int PlaneCount;

        public CuVideoChromaFormatInformation(CuVideoChromaFormat format)
        {
            HeightFactor = 0.5f;
            PlaneCount = 1;

            switch (format)
            {
                case CuVideoChromaFormat.Monochrome:
                    HeightFactor = 0.0f;
                    PlaneCount = 0;
                    break;
                case CuVideoChromaFormat.YUV420:
                    break;
                case CuVideoChromaFormat.YUV422:
                    HeightFactor = 1.0f;
                    break;
                case CuVideoChromaFormat.YUV444:
                    HeightFactor = 1.0f;
                    PlaneCount = 2;
                    break;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CuRectangle
    {
        public static CuRectangle Null { get; } = new CuRectangle(0, 0, 0, 0);

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        /// <summary>left position of rect</summary>
        public int Left;
        /// <summary>top position of rect</summary>
        public int Top;
        /// <summary>right position of rect</summary>
        public int Right;
        /// <summary>bottom position of rect</summary>
        public int Bottom;

        public CuRectangle(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CuRectangleShort
    {
        public static CuRectangleShort Null { get; } = new CuRectangleShort(0, 0, 0, 0);

        public short Width => (short)(Right - Left);
        public short Height => (short)(Bottom - Top);

        /// <summary>left position of rect</summary>
        public short Left;
        /// <summary>top position of rect</summary>
        public short Top;
        /// <summary>right position of rect</summary>
        public short Right;
        /// <summary>bottom position of rect</summary>
        public short Bottom;

        public CuRectangleShort(short left, short top, short right, short bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }



    [StructLayout(LayoutKind.Explicit)]
    public struct CuVideoH264PicParamsExt
    {
        [FieldOffset(0)]
        public CuVideoH264MvcExt MvcExt;

        [FieldOffset(0)]
        public CuVideoH264SvcExt SvcExt;
    }


    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct CuVideoPicCodecParams
    {
        /// <summary>Also used for MPEG-1</summary>
        [FieldOffset(0)]
        public CuVideoMpeg2PicParams Mpeg2;
        [FieldOffset(0)]
        public CuVideoH264PicParams H264;
        [FieldOffset(0)]
        public CuVideoVC1PicParams VC1;
        [FieldOffset(0)]
        public CuVideoMpeg4PicParams Mpeg4;
        [FieldOffset(0)]
        public CuVideoJpegPicParams Jpeg;
        [FieldOffset(0)]
        public CuVideoHevcPicParams Hevc;
        [FieldOffset(0)]
        public CuVideoVP8PicParams VP8;
        [FieldOffset(0)]
        public CuVideoVP9PicParams VP9;
        [FieldOffset(0)]
        private fixed uint CodecReserved[1024];
    }

}
