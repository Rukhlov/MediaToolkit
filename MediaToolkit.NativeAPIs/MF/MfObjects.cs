using MediaToolkit.NativeAPIs.Ole;
using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs.MF.Objects
{

	[StructLayout(LayoutKind.Sequential)]
	public class MFTRegisterTypeInfo
	{
		public Guid guidMajorType;
		public Guid guidSubtype;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MFVideoInfo
    {
        public int dwWidth;
        public int dwHeight;
        public MFRatio PixelAspectRatio;
        public MFVideoChromaSubsampling SourceChromaSubsampling;
        public MFVideoInterlaceMode InterlaceMode;
        public MFVideoTransferFunction TransferFunction;
        public MFVideoPrimaries ColorPrimaries;
        public MFVideoTransferMatrix TransferMatrix;
        public MFVideoLighting SourceLighting;
        public MFRatio FramesPerSecond;
        public MFNominalRange NominalRange;
        public MFVideoArea GeometricAperture;
        public MFVideoArea MinimumDisplayAperture;
        public MFVideoArea PanScanAperture;
        public MFVideoFlags VideoFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MFVideoArea
    {
        public MFOffset OffsetX;
        public MFOffset OffsetY;
        public SIZE Area;

        public MFVideoArea()
        {
            OffsetX = new MFOffset();
            OffsetY = new MFOffset();
        }

        public MFVideoArea(float x, float y, int width, int height)
        {
            OffsetX = new MFOffset(x);
            OffsetY = new MFOffset(y);
            Area = new SIZE(width, height);
        }

        public void MakeArea(float x, float y, int width, int height)
        {
            OffsetX.MakeOffset(x);
            OffsetY.MakeOffset(y);
            Area.cx = width;
            Area.cy = height;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class MFOffset
    {
        public short fract;
        public short Value;

        public MFOffset()
        {
        }

        public MFOffset(float v)
        {
            Value = (short)v;
            fract = (short)(65536 * (v - Value));
        }

        public void MakeOffset(float v)
        {
            Value = (short)v;
            fract = (short)(65536 * (v - Value));
        }

        public float GetOffset()
        {
            return ((float)Value) + (((float)fract) / 65536.0f);
        }
    }

    [Flags]
    public enum MFVideoFlags : long
    {
        None = 0,
        PAD_TO_Mask = 0x0001 | 0x0002,
        PAD_TO_None = 0 * 0x0001,
        PAD_TO_4x3 = 1 * 0x0001,
        PAD_TO_16x9 = 2 * 0x0001,
        SrcContentHintMask = 0x0004 | 0x0008 | 0x0010,
        SrcContentHintNone = 0 * 0x0004,
        SrcContentHint16x9 = 1 * 0x0004,
        SrcContentHint235_1 = 2 * 0x0004,
        AnalogProtected = 0x0020,
        DigitallyProtected = 0x0040,
        ProgressiveContent = 0x0080,
        FieldRepeatCountMask = 0x0100 | 0x0200 | 0x0400,
        FieldRepeatCountShift = 8,
        ProgressiveSeqReset = 0x0800,
        PanScanEnabled = 0x20000,
        LowerFieldFirst = 0x40000,
        BottomUpLinearRep = 0x80000,
        DXVASurface = 0x100000,
        RenderTargetSurface = 0x400000,
        ForceQWORD = 0x7FFFFFFF
    }


    public enum MFVideoChromaSubsampling
    {
        Cosited = 7,
        DV_PAL = 6,
        ForceDWORD = 0x7fffffff,
        Horizontally_Cosited = 4,
        Last = 8,
        MPEG1 = 1,
        MPEG2 = 5,
        ProgressiveChroma = 8,
        Unknown = 0,
        Vertically_AlignedChromaPlanes = 1,
        Vertically_Cosited = 2
    }


    public enum MFVideoInterlaceMode
    {
        FieldInterleavedLowerFirst = 4,
        FieldInterleavedUpperFirst = 3,
        FieldSingleLower = 6,
        FieldSingleUpper = 5,
        ForceDWORD = 0x7fffffff,
        Last = 8,
        MixedInterlaceOrProgressive = 7,
        Progressive = 2,
        Unknown = 0
    }


    public enum MFVideoTransferFunction
    {
        Unknown = 0,
        Func10 = 1,
        Func18 = 2,
        Func20 = 3,
        Func22 = 4,
        Func240M = 6,
        Func28 = 8,
        Func709 = 5,
        Func2020Const = 12,
        Func2020 = 13,
        Func26 = 14,
        ForceDWORD = 0x7fffffff,
        Last = 9,
        sRGB = 7,
        Log_100 = 9,
        Log_316 = 10,
        x709_sym = 11 // symmetric 709
    }


    public enum MFVideoPrimaries
    {
        BT470_2_SysBG = 4,
        BT470_2_SysM = 3,
        BT709 = 2,
        EBU3213 = 7,
        ForceDWORD = 0x7fffffff,
        Last = 9,
        reserved = 1,
        SMPTE_C = 8,
        SMPTE170M = 5,
        SMPTE240M = 6,
        Unknown = 0,
        BT2020 = 9,
        XYZ = 10,
    }


    public enum MFVideoTransferMatrix
    {
        BT601 = 2,
        BT709 = 1,
        ForceDWORD = 0x7fffffff,
        SMPTE240M = 3,
        Unknown = 0,
        BT2020_10 = 4,
        BT2020_12 = 5,
        Last = 6,
    }


    public enum MFVideoLighting
    {
        Bright = 1,
        Dark = 4,
        Dim = 3,
        ForceDWORD = 0x7fffffff,
        Last = 5,
        Office = 2,
        Unknown = 0
    }


    public enum MFNominalRange
    {
        MFNominalRange_Unknown = 0,
        MFNominalRange_Normal = 1,
        MFNominalRange_Wide = 2,

        MFNominalRange_0_255 = 1,
        MFNominalRange_16_235 = 2,
        MFNominalRange_48_208 = 3,
        MFNominalRange_64_127 = 4,

        MFNominalRange_Last,
        MFNominalRange_ForceDWORD = 0x7fffffff,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MFRatio
    {
        public int Numerator;
        public int Denominator;

        public MFRatio(int n, int d)
        {
            Numerator = n;
            Denominator = d;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MFVideoCompressedInfo
    {
        public long AvgBitrate;
        public long AvgBitErrorRate;
        public int MaxKeyFrameSpacing;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct MFVideoSurfaceInfo
    {
        public int Format;
        public int PaletteEntries;
        public MFPaletteEntry[] Palette;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("AC6B7889-0740-4D51-8619-905994A55CC6")]
    public interface IMFAsyncResult
    {
        [PreserveSig]
        HResult GetState(
            [MarshalAs(UnmanagedType.IUnknown)] out object ppunkState
            );

        [PreserveSig]
        HResult GetStatus();

        [PreserveSig]
        HResult SetStatus(
            [In, MarshalAs(UnmanagedType.Error)] HResult hrStatus
            );

        [PreserveSig]
        HResult GetObject(
            [MarshalAs(UnmanagedType.Interface)] out object ppObject
            );

        [PreserveSig]
        IntPtr GetStateNoAddRef();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("2CD0BD52-BCD5-4B89-B62C-EADC0C031E7D"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEventGenerator
    {
        [PreserveSig]
        HResult GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        HResult BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        HResult EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        HResult QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] HResult hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("A27003CF-2354-4F2A-8D6A-AB7CFF15437E")]
    public interface IMFAsyncCallback
    {
        [PreserveSig]
        HResult GetParameters(
            out MFASync pdwFlags,
            out MFAsyncCallbackQueue pdwQueue
            );

        [PreserveSig]
        HResult Invoke(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncResult pAsyncResult
            );
    }


    [Flags]
    public enum MFEventFlag
    {
        None = 0,
        NoWait = 0x00000001
    }


    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("C40A00F2-B93A-4D80-AE8C-5A1C634F58E4"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFSample : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new HResult GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSample.GetItem", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new HResult CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new HResult GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new HResult GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new HResult GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new HResult GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new HResult GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new HResult GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new HResult GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new HResult SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new HResult DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new HResult DeleteAllItems();

        [PreserveSig]
        new HResult SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new HResult SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new HResult SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new HResult SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new HResult SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new HResult SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new HResult SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new HResult LockStore();

        [PreserveSig]
        new HResult UnlockStore();

        [PreserveSig]
        new HResult GetCount(
            out int pcItems
            );

        [PreserveSig]
        new HResult GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSample.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        HResult GetSampleFlags(
            out int pdwSampleFlags // Must be zero
            );

        [PreserveSig]
        HResult SetSampleFlags(
            [In] int dwSampleFlags // Must be zero
            );

        [PreserveSig]
        HResult GetSampleTime(
            out long phnsSampleTime
            );

        [PreserveSig]
        HResult SetSampleTime(
            [In] long hnsSampleTime
            );

        [PreserveSig]
        HResult GetSampleDuration(
            out long phnsSampleDuration
            );

        [PreserveSig]
        HResult SetSampleDuration(
            [In] long hnsSampleDuration
            );

        [PreserveSig]
        HResult GetBufferCount(
            out int pdwBufferCount
            );

        [PreserveSig]
        HResult GetBufferByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
            );

        [PreserveSig]
        HResult ConvertToContiguousBuffer(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaBuffer ppBuffer
            );

        [PreserveSig]
        HResult AddBuffer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
            );

        [PreserveSig]
        HResult RemoveBufferByIndex(
            [In] int dwIndex
            );

        [PreserveSig]
        HResult RemoveAllBuffers();

        [PreserveSig]
        HResult GetTotalLength(
            out int pcbTotalLength
            );

        [PreserveSig]
        HResult CopyToBuffer(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaBuffer pBuffer
            );
    }


    public enum MFAsyncCallbackQueue
    {
        Undefined = 0x00000000,
        Standard = 0x00000001,
        RT = 0x00000002,
        IO = 0x00000003,
        Timer = 0x00000004,
        MultiThreaded = 0x00000005,
        LongFunction = 0x00000007,
        PrivateMask = unchecked((int)0xFFFF0000),
        All = unchecked((int)0xFFFFFFFF)
    }

    [Flags]
    public enum MFASync
    {
        None = 0,
        FastIOProcessingCallback = 0x00000001,
        SignalCallback = 0x00000002,
        BlockingCallback = 0x00000004,
        ReplyCallback = 0x00000008,
        LocalizeRemoteCallback = 0x00000010,
    }


    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("045FA593-8799-42B8-BC8D-8968C6453507")]
    public interface IMFMediaBuffer
    {
        [PreserveSig]
        HResult Lock(
            out IntPtr ppbBuffer,
            out int pcbMaxLength,
            out int pcbCurrentLength
            );

        [PreserveSig]
        HResult Unlock();

        [PreserveSig]
        HResult GetCurrentLength(
            out int pcbCurrentLength
            );

        [PreserveSig]
        HResult SetCurrentLength(
            [In] int cbCurrentLength
            );

        [PreserveSig]
        HResult GetMaxLength(
            out int pcbMaxLength
            );
    }

    [Flags]
    public enum MFMediaEqual
    {
        None = 0,
        MajorTypes = 0x00000001,
        FormatTypes = 0x00000002,
        FormatData = 0x00000004,
        FormatUserData = 0x00000008
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("44AE0FA8-EA31-4109-8D2E-4CAE4997C555")]
    public interface IMFMediaType : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new HResult GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaType.GetItem", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new HResult CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new HResult GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new HResult GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new HResult GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new HResult GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new HResult GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new HResult GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new HResult GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new HResult SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new HResult DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new HResult DeleteAllItems();

        [PreserveSig]
        new HResult SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new HResult SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new HResult SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new HResult SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new HResult SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new HResult SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new HResult SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new HResult LockStore();

        [PreserveSig]
        new HResult UnlockStore();

        [PreserveSig]
        new HResult GetCount(
            out int pcItems
            );

        [PreserveSig]
        new HResult GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaType.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        HResult GetMajorType(
            out Guid pguidMajorType
            );

        [PreserveSig]
        HResult IsCompressedFormat(
            [MarshalAs(UnmanagedType.Bool)] out bool pfCompressed
            );

        [PreserveSig]
        HResult IsEqual(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
            out MFMediaEqual pdwFlags
            );

        [PreserveSig]
        HResult GetRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation
            );

        [PreserveSig]
        HResult FreeRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            [In] IntPtr pvRepresentation
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("2CD2D921-C447-44A7-A13C-4ADABFC247E3"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFAttributes
    {
        [PreserveSig]
        HResult GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAttributes.GetItem", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        HResult GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        HResult CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        HResult Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        HResult GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        HResult GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        HResult GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        HResult GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        HResult GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        HResult GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        HResult GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        HResult GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        HResult GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        HResult GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        HResult GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        HResult SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        HResult DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        HResult DeleteAllItems();

        [PreserveSig]
        HResult SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        HResult SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        HResult SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        HResult SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        HResult SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        HResult SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        HResult SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        HResult LockStore();

        [PreserveSig]
        HResult UnlockStore();

        [PreserveSig]
        HResult GetCount(
            out int pcItems
            );

        [PreserveSig]
        HResult GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFAttributes.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        HResult CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("DF598932-F10C-4E39-BBA2-C308F101DAA3"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaEvent : IMFAttributes
    {
        #region IMFAttributes methods

        [PreserveSig]
        new HResult GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetItem", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult GetItemType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out MFAttributeType pType
            );

        [PreserveSig]
        new HResult CompareItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult Compare(
            [MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
            MFAttributesMatchType MatchType,
            [MarshalAs(UnmanagedType.Bool)] out bool pbResult
            );

        [PreserveSig]
        new HResult GetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int punValue
            );

        [PreserveSig]
        new HResult GetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out long punValue
            );

        [PreserveSig]
        new HResult GetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out double pfValue
            );

        [PreserveSig]
        new HResult GetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out Guid pguidValue
            );

        [PreserveSig]
        new HResult GetStringLength(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
            int cchBufSize,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetAllocatedString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
            out int pcchLength
            );

        [PreserveSig]
        new HResult GetBlobSize(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out int pcbBlobSize
            );

        [PreserveSig]
        new HResult GetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
            int cbBufSize,
            out int pcbBlobSize
            );

        // Use GetBlob instead of this
        [PreserveSig]
        new HResult GetAllocatedBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
            out int pcbSize
            );

        [PreserveSig]
        new HResult GetUnknown(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppv
            );

        [PreserveSig]
        new HResult SetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
            );

        [PreserveSig]
        new HResult DeleteItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
            );

        [PreserveSig]
        new HResult DeleteAllItems();

        [PreserveSig]
        new HResult SetUINT32(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            int unValue
            );

        [PreserveSig]
        new HResult SetUINT64(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            long unValue
            );

        [PreserveSig]
        new HResult SetDouble(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            double fValue
            );

        [PreserveSig]
        new HResult SetGUID(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
            );

        [PreserveSig]
        new HResult SetString(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
            );

        [PreserveSig]
        new HResult SetBlob(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
            int cbBufSize
            );

        [PreserveSig]
        new HResult SetUnknown(
            [MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
            );

        [PreserveSig]
        new HResult LockStore();

        [PreserveSig]
        new HResult UnlockStore();

        [PreserveSig]
        new HResult GetCount(
            out int pcItems
            );

        [PreserveSig]
        new HResult GetItemByIndex(
            int unIndex,
            out Guid pguidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult CopyAllItems(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
            );

        #endregion

        [PreserveSig]
        HResult GetType(
            out MediaEventType pmet
            );

        [PreserveSig]
        HResult GetExtendedType(
            out Guid pguidExtendedType
            );

        [PreserveSig]
        HResult GetStatus(
            [MarshalAs(UnmanagedType.Error)] out HResult phrStatus
            );

        [PreserveSig]
        HResult GetValue(
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFMediaEvent.GetValue", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvValue
            );
    }


    public enum MFAttributesMatchType
    {
        OurItems,
        TheirItems,
        AllItems,
        InterSection,
        Smaller
    }

    public enum MFAttributeType
    {
        None = 0x0,
        Blob = 0x1011,
        Double = 0x5,
        Guid = 0x48,
        IUnknown = 13,
        String = 0x1f,
        Uint32 = 0x13,
        Uint64 = 0x15
    }

    public enum MediaEventType
    {
        MEUnknown = 0,
        MEError = 1,
        MEExtendedType = 2,
        MENonFatalError = 3,
        MEGenericV1Anchor = MENonFatalError,
        MESessionUnknown = 100,
        MESessionTopologySet = 101,
        MESessionTopologiesCleared = 102,
        MESessionStarted = 103,
        MESessionPaused = 104,
        MESessionStopped = 105,
        MESessionClosed = 106,
        MESessionEnded = 107,
        MESessionRateChanged = 108,
        MESessionScrubSampleComplete = 109,
        MESessionCapabilitiesChanged = 110,
        MESessionTopologyStatus = 111,
        MESessionNotifyPresentationTime = 112,
        MENewPresentation = 113,
        MELicenseAcquisitionStart = 114,
        MELicenseAcquisitionCompleted = 115,
        MEIndividualizationStart = 116,
        MEIndividualizationCompleted = 117,
        MEEnablerProgress = 118,
        MEEnablerCompleted = 119,
        MEPolicyError = 120,
        MEPolicyReport = 121,
        MEBufferingStarted = 122,
        MEBufferingStopped = 123,
        MEConnectStart = 124,
        MEConnectEnd = 125,
        MEReconnectStart = 126,
        MEReconnectEnd = 127,
        MERendererEvent = 128,
        MESessionStreamSinkFormatChanged = 129,
        MESessionV1Anchor = MESessionStreamSinkFormatChanged,
        MESourceUnknown = 200,
        MESourceStarted = 201,
        MEStreamStarted = 202,
        MESourceSeeked = 203,
        MEStreamSeeked = 204,
        MENewStream = 205,
        MEUpdatedStream = 206,
        MESourceStopped = 207,
        MEStreamStopped = 208,
        MESourcePaused = 209,
        MEStreamPaused = 210,
        MEEndOfPresentation = 211,
        MEEndOfStream = 212,
        MEMediaSample = 213,
        MEStreamTick = 214,
        MEStreamThinMode = 215,
        MEStreamFormatChanged = 216,
        MESourceRateChanged = 217,
        MEEndOfPresentationSegment = 218,
        MESourceCharacteristicsChanged = 219,
        MESourceRateChangeRequested = 220,
        MESourceMetadataChanged = 221,
        MESequencerSourceTopologyUpdated = 222,
        MESourceV1Anchor = MESequencerSourceTopologyUpdated,

        MESinkUnknown = 300,
        MEStreamSinkStarted = 301,
        MEStreamSinkStopped = 302,
        MEStreamSinkPaused = 303,
        MEStreamSinkRateChanged = 304,
        MEStreamSinkRequestSample = 305,
        MEStreamSinkMarker = 306,
        MEStreamSinkPrerolled = 307,
        MEStreamSinkScrubSampleComplete = 308,
        MEStreamSinkFormatChanged = 309,
        MEStreamSinkDeviceChanged = 310,
        MEQualityNotify = 311,
        MESinkInvalidated = 312,
        MEAudioSessionNameChanged = 313,
        MEAudioSessionVolumeChanged = 314,
        MEAudioSessionDeviceRemoved = 315,
        MEAudioSessionServerShutdown = 316,
        MEAudioSessionGroupingParamChanged = 317,
        MEAudioSessionIconChanged = 318,
        MEAudioSessionFormatChanged = 319,
        MEAudioSessionDisconnected = 320,
        MEAudioSessionExclusiveModeOverride = 321,
        MESinkV1Anchor = MEAudioSessionExclusiveModeOverride,

        MECaptureAudioSessionVolumeChanged = 322,
        MECaptureAudioSessionDeviceRemoved = 323,
        MECaptureAudioSessionFormatChanged = 324,
        MECaptureAudioSessionDisconnected = 325,
        MECaptureAudioSessionExclusiveModeOverride = 326,
        MECaptureAudioSessionServerShutdown = 327,
        MESinkV2Anchor = MECaptureAudioSessionServerShutdown,

        METrustUnknown = 400,
        MEPolicyChanged = 401,
        MEContentProtectionMessage = 402,
        MEPolicySet = 403,
        METrustV1Anchor = MEPolicySet,

        MEWMDRMLicenseBackupCompleted = 500,
        MEWMDRMLicenseBackupProgress = 501,
        MEWMDRMLicenseRestoreCompleted = 502,
        MEWMDRMLicenseRestoreProgress = 503,
        MEWMDRMLicenseAcquisitionCompleted = 506,
        MEWMDRMIndividualizationCompleted = 508,
        MEWMDRMIndividualizationProgress = 513,
        MEWMDRMProximityCompleted = 514,
        MEWMDRMLicenseStoreCleaned = 515,
        MEWMDRMRevocationDownloadCompleted = 516,
        MEWMDRMV1Anchor = MEWMDRMRevocationDownloadCompleted,

        METransformUnknown = 600,
        METransformNeedInput,
        METransformHaveOutput,
        METransformDrainComplete,
        METransformMarker,
        METransformInputStreamStateChanged,
        MEByteStreamCharacteristicsChanged = 700,
        MEVideoCaptureDeviceRemoved = 800,
        MEVideoCaptureDevicePreempted = 801,
        MEStreamSinkFormatInvalidated = 802,
        MEEncodingParameters = 803,
        MEContentProtectionMetadata = 900,
        MEDeviceThermalStateChanged = 950,
        MEReservedMax = 10000
    }

}
