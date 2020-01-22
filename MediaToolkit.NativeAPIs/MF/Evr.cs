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
