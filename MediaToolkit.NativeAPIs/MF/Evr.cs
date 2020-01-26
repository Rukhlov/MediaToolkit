using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Ole;
using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs.MF.EVR
{
    public class Evr
    {
        [DllImport("Evr.dll", EntryPoint = "MFCreateVideoMixer", CallingConvention = CallingConvention.StdCall)]
        public static extern int CreateVideoMixer([In] IntPtr pOwner, 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice,
            [In, MarshalAs(UnmanagedType.LPStruct)]Guid riid,
            out IntPtr ppv);

        [DllImport("Evr.dll", EntryPoint = "MFCreateVideoPresenter", CallingConvention = CallingConvention.StdCall)]
        public static extern int MFCreateVideoPresenter([In] IntPtr pOwner, 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riidDevice, 
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid, 
            out IntPtr ppVideoPresenter);

    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("29AFF080-182A-4A5D-AF3B-448F3A6346CB")]
    public interface IMFVideoPresenter : IMFClockStateSink
    {
        #region IMFClockStateSink

        [PreserveSig]
        new HResult OnClockStart([In] long hnsSystemTime, [In] long llClockStartOffset );

        [PreserveSig]
        new HResult OnClockStop( [In] long hnsSystemTime );

        [PreserveSig]
        new HResult OnClockPause( [In] long hnsSystemTime);

        [PreserveSig]
        new HResult OnClockRestart([In] long hnsSystemTime);

        [PreserveSig]
        new HResult OnClockSetRate( [In] long hnsSystemTime, [In] float flRate );

        #endregion

        [PreserveSig]
        HResult ProcessMessage( MFVPMessageType eMessage, IntPtr ulParam);

        [PreserveSig]
        HResult GetCurrentMediaType( [MarshalAs(UnmanagedType.Interface)] out IMFVideoMediaType ppMediaType );
    }



    public enum MFVPMessageType
    {
        Flush = 0,
        InvalidateMediaType,
        ProcessInputNotify,
        BeginStreaming,
        EndStreaming,
        EndOfStream,
        Step,
        CancelStep
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("B99F381F-A8F9-47A2-A5AF-CA3A225A3890"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoMediaType : IMFMediaType
    {
        #region IMFAttributes methods

        [PreserveSig]
        new HResult GetItem(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFVideoMediaType.GetItem", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
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
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFVideoMediaType.GetItemByIndex", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pValue
            );

        [PreserveSig]
        new HResult CopyAllItems([In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest);

        #endregion

        #region IMFMediaType methods

        [PreserveSig]
        new HResult GetMajorType(
            out Guid pguidMajorType
            );

        [PreserveSig]
        new HResult IsCompressedFormat( [MarshalAs(UnmanagedType.Bool)] out bool pfCompressed );

        [PreserveSig]
        new HResult IsEqual(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pIMediaType,
            out MFMediaEqual pdwFlags
            );

        [PreserveSig]
        new HResult GetRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation
            );

        [PreserveSig]
        new HResult FreeRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            [In] IntPtr pvRepresentation
            );

        #endregion

        [PreserveSig, Obsolete("This method is deprecated by MS")]
        MFVideoFormat GetVideoFormat();

        [Obsolete("This method is deprecated by MS")]
        [PreserveSig]
        HResult GetVideoRepresentation(
            [In, MarshalAs(UnmanagedType.Struct)] Guid guidRepresentation,
            out IntPtr ppvRepresentation,
            [In] int lStride
            );
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public class MFVideoFormat
    {
        public int dwSize;
        public MFVideoInfo videoInfo;
        public Guid guidFormat;
        public MFVideoCompressedInfo compressedInfo;
        public MFVideoSurfaceInfo surfaceInfo;
    }



    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("A490B1E4-AB84-4D31-A1B2-181E03B1077A"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFVideoDisplayControl
    {
        [PreserveSig]
        HResult GetNativeVideoSize([Out] SIZE pszVideo, [Out] SIZE pszARVideo);

        [PreserveSig]
        HResult GetIdealVideoSize([Out] SIZE pszMin, [Out] SIZE pszMax);

        [PreserveSig]
        HResult SetVideoPosition([In] MFVideoNormalizedRect pnrcSource, [In] RECT prcDest);

        [PreserveSig]
        HResult GetVideoPosition([Out] MFVideoNormalizedRect pnrcSource, [Out] RECT prcDest);

        [PreserveSig]
        HResult SetAspectRatioMode([In] MFVideoAspectRatioMode dwAspectRatioMode);

        [PreserveSig]
        HResult GetAspectRatioMode(out MFVideoAspectRatioMode pdwAspectRatioMode);

        [PreserveSig]
        HResult SetVideoWindow([In] IntPtr hwndVideo);

        [PreserveSig]
        HResult GetVideoWindow(out IntPtr phwndVideo);

        [PreserveSig]
        HResult RepaintVideo();

        [PreserveSig]
        HResult GetCurrentImage([In, Out] IntPtr pBih, out IntPtr pDib, out int pcbDib, out long pTimeStamp);

        [PreserveSig]
        HResult SetBorderColor([In] int Clr);

        [PreserveSig]
        HResult GetBorderColor(out int pClr);

        [PreserveSig]
        HResult SetRenderingPrefs([In] MFVideoRenderPrefs dwRenderFlags);

        [PreserveSig]
        HResult GetRenderingPrefs(out MFVideoRenderPrefs pdwRenderFlags);

        [PreserveSig]
        HResult SetFullscreen([In, MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        [PreserveSig]
        HResult GetFullscreen([MarshalAs(UnmanagedType.Bool)] out bool pfFullscreen);
    }


    [Flags]
    public enum MFVideoRenderPrefs
    {
        None = 0,
        DoNotRenderBorder = 0x00000001,
        DoNotClipToDevice = 0x00000002,
        AllowOutputThrottling = 0x00000004,
        ForceOutputThrottling = 0x00000008,
        ForceBatching = 0x00000010,
        AllowBatching = 0x00000020,
        ForceScaling = 0x00000040,
        AllowScaling = 0x00000080,
        DoNotRepaintOnStop = 0x00000100,
        Mask = 0x000001ff,
    }


    [Flags]
    public enum MFVideoAspectRatioMode
    {
        None = 0x00000000,
        PreservePicture = 0x00000001,
        PreservePixel = 0x00000002,
        NonLinearStretch = 0x00000004,
        Mask = 0x00000007
    }



    [ComImport, SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("814C7B20-0FDB-4eec-AF8F-F957C8F69EDC")]
    public interface IMFVideoMixerBitmap
    {
        [PreserveSig]
        HResult SetAlphaBitmap([In, MarshalAs(UnmanagedType.LPStruct)] MFVideoAlphaBitmap pBmpParms);

        [PreserveSig]
        HResult ClearAlphaBitmap();

        [PreserveSig]
        HResult UpdateAlphaBitmapParameters([In] MFVideoAlphaBitmapParams pBmpParms);

        [PreserveSig]
        HResult GetAlphaBitmapParameters([Out] MFVideoAlphaBitmapParams pBmpParms);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MFVideoAlphaBitmap
    {
        public bool GetBitmapFromDC;
        public IntPtr Data;
        public MFVideoAlphaBitmapParams Params;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MFVideoAlphaBitmapParams
    {
        public MFVideoAlphaBitmapFlags dwFlags;
        public int clrSrcKey;
        public RECT rcSrc;
        public MFVideoNormalizedRect nrcDest;
        public float fAlpha;
        public int dwFilterMode;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public class MFVideoNormalizedRect
    {
        public float left;
        public float top;
        public float right;
        public float bottom;

        public MFVideoNormalizedRect()
        {
        }

        public MFVideoNormalizedRect(System.Drawing.RectangleF rect)
        {
            left = rect.Left;
            top = rect.Top;
            right = rect.Right;
            bottom = rect.Bottom;
        }

        public MFVideoNormalizedRect(float l, float t, float r, float b)
        {
            left = l;
            top = t;
            right = r;
            bottom = b;
        }

        public override string ToString()
        {
            return string.Format("left = {0}, top = {1}, right = {2}, bottom = {3}", left, top, right, bottom);
        }

        public override int GetHashCode()
        {
            return left.GetHashCode() |
                top.GetHashCode() |
                right.GetHashCode() |
                bottom.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is MFVideoNormalizedRect)
            {
                MFVideoNormalizedRect cmp = (MFVideoNormalizedRect)obj;

                return right == cmp.right && bottom == cmp.bottom && left == cmp.left && top == cmp.top;
            }

            return false;
        }

        public bool IsEmpty()
        {
            return (right <= left || bottom <= top);
        }

        public void CopyFrom(MFVideoNormalizedRect from)
        {
            left = from.left;
            top = from.top;
            right = from.right;
            bottom = from.bottom;
        }
    }

    [Flags]
    public enum MFVideoAlphaBitmapFlags
    {
        None = 0,
        EntireDDS = 0x00000001,
        SrcColorKey = 0x00000002,
        SrcRect = 0x00000004,
        DestRect = 0x00000008,
        FilterMode = 0x00000010,
        Alpha = 0x00000020,
        BitMask = 0x0000003f
    }


    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
    public interface IMFVideoProcessor
    {
        [PreserveSig]
        HResult GetAvailableVideoProcessorModes(out int lpdwNumProcessingModes,
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out Guid[] ppVideoProcessingModes);

        [PreserveSig]
        HResult GetVideoProcessorCaps([In, MarshalAs(UnmanagedType.LPStruct)] Guid lpVideoProcessorMode,
            out DXVA2VideoProcessorCaps lpVideoProcessorCaps);

        [PreserveSig]
        HResult GetVideoProcessorMode(out Guid lpMode);

        [PreserveSig]
        HResult SetVideoProcessorMode([In, MarshalAs(UnmanagedType.LPStruct)] Guid lpMode);

        [PreserveSig]
        HResult GetProcAmpRange(DXVA2ProcAmp dwProperty, out DXVA2ValueRange pPropRange);

        [PreserveSig]
        HResult GetProcAmpValues(DXVA2ProcAmp dwFlags, [Out, MarshalAs(UnmanagedType.LPStruct)] DXVA2ProcAmpValues Values);

        [PreserveSig]
        HResult SetProcAmpValues(DXVA2ProcAmp dwFlags, [In] DXVA2ProcAmpValues pValues);

        [PreserveSig]
        HResult GetFilteringRange(DXVA2Filters dwProperty, out DXVA2ValueRange pPropRange);

        [PreserveSig]
        HResult GetFilteringValue(DXVA2Filters dwProperty, out int pValue);

        [PreserveSig]
        HResult SetFilteringValue(DXVA2Filters dwProperty, [In] ref int pValue);

        [PreserveSig]
        HResult GetBackgroundColor(out int lpClrBkg);

        [PreserveSig]
        HResult SetBackgroundColor(int ClrBkg);
    }

    [Flags]
    public enum DXVA2ProcAmp
    {
        None = 0,
        Brightness = 0x0001,
        Contrast = 0x0002,
        Hue = 0x0004,
        Saturation = 0x0008
    }


    public enum DXVA2Filters
    {
        None = 0,
        NoiseFilterLumaLevel = 1,
        NoiseFilterLumaThreshold = 2,
        NoiseFilterLumaRadius = 3,
        NoiseFilterChromaLevel = 4,
        NoiseFilterChromaThreshold = 5,
        NoiseFilterChromaRadius = 6,
        DetailFilterLumaLevel = 7,
        DetailFilterLumaThreshold = 8,
        DetailFilterLumaRadius = 9,
        DetailFilterChromaLevel = 10,
        DetailFilterChromaThreshold = 11,
        DetailFilterChromaRadius = 12
    }


    [Flags]
    public enum MFVideoMixPrefs
    {
        None = 0,
        ForceHalfInterlace = 0x00000001,
        AllowDropToHalfInterlace = 0x00000002,
        AllowDropToBob = 0x00000004,
        ForceBob = 0x00000008,
        Mask = 0x0000000f
    }

    [Flags]
    public enum EVRFilterConfigPrefs
    {
        None = 0,
        EnableQoS = 0x1,
        Mask = 0x1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXVA2VideoProcessorCaps
    {
        public int DeviceCaps;
        public int InputPool;
        public int NumForwardRefSamples;
        public int NumBackwardRefSamples;
        public int Reserved;
        public int DeinterlaceTechnology;
        public int ProcAmpControlCaps;
        public int VideoProcessorOperations;
        public int NoiseFilterTechnology;
        public int DetailFilterTechnology;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DXVA2ValueRange
    {
        public int MinValue;
        public int MaxValue;
        public int DefaultValue;
        public int StepSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DXVA2ProcAmpValues
    {
        public int Brightness;
        public int Contrast;
        public int Hue;
        public int Saturation;
    }


}
