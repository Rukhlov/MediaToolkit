using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Ole;
using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs.MF.ReadWrite
{

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    public interface IMFSourceReader
    {
        [PreserveSig]
        HResult GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        HResult SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fSelected
        );

        [PreserveSig]
        HResult GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        HResult GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        HResult SetCurrentMediaType(
            int dwStreamIndex,
            [In, Out] int pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        HResult SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant varPosition
        );

        [PreserveSig]
        HResult ReadSample(
            int dwStreamIndex,
            MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
            out int pdwActualStreamIndex,
            out MF_SOURCE_READER_FLAG pdwStreamFlags,
            out long pllTimestamp,
            out IMFSample ppSample
        );

        [PreserveSig]
        HResult Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        HResult GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        HResult GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSourceReader.GetPresentationAttribute", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    public interface IMFSourceReaderAsync
    {
        [PreserveSig]
        HResult GetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] out bool pfSelected
        );

        [PreserveSig]
        HResult SetStreamSelection(
            int dwStreamIndex,
            [MarshalAs(UnmanagedType.Bool)] bool fSelected
        );

        [PreserveSig]
        HResult GetNativeMediaType(
            int dwStreamIndex,
            int dwMediaTypeIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        HResult GetCurrentMediaType(
            int dwStreamIndex,
            out IMFMediaType ppMediaType
        );

        [PreserveSig]
        HResult SetCurrentMediaType(
            int dwStreamIndex,
            [In, Out] int pdwReserved,
            IMFMediaType pMediaType
        );

        [PreserveSig]
        HResult SetCurrentPosition(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidTimeFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant varPosition
        );

        [PreserveSig]
        HResult ReadSample(
            int dwStreamIndex,
            MF_SOURCE_READER_CONTROL_FLAG dwControlFlags,
            IntPtr pdwActualStreamIndex,
            IntPtr pdwStreamFlags,
            IntPtr pllTimestamp,
            IntPtr ppSample
        );

        [PreserveSig]
        HResult Flush(
            int dwStreamIndex
        );

        [PreserveSig]
        HResult GetServiceForStream(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidService,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [MarshalAs(UnmanagedType.IUnknown)] out object ppvObject
        );

        [PreserveSig]
        HResult GetPresentationAttribute(
            int dwStreamIndex,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidAttribute,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFSourceReaderAsync.GetPresentationAttribute", MarshalTypeRef = typeof(PVMarshaler))] PropVariant pvarAttribute
        );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("deec8d99-fa1d-4d82-84c2-2c8969944867")]
    public interface IMFSourceReaderCallback
    {
        [PreserveSig]
        HResult OnReadSample(
            HResult hrStatus,
            int dwStreamIndex,
            MF_SOURCE_READER_FLAG dwStreamFlags,
            long llTimestamp,
            IMFSample pSample
        );

        [PreserveSig]
        HResult OnFlush(
            int dwStreamIndex
        );

        [PreserveSig]
        HResult OnEvent(
            int dwStreamIndex,
            IMFMediaEvent pEvent
        );
    }

    public enum MF_SOURCE_READER_CONTROL_FLAG
    {
        None = 0,
        Drain = 0x00000001
    }

    public enum MF_SOURCE_READER_FLAG
    {
        None = 0,
        Error = 0x00000001,
        EndOfStream = 0x00000002,
        NewStream = 0x00000004,
        NativeMediaTypeChanged = 0x00000010,
        CurrentMediaTypeChanged = 0x00000020,
        AllEffectsRemoved = 0x00000200,
        StreamTick = 0x00000100
    }


}
