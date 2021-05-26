using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MediaToolkit.NativeAPIs.DShow
{

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
		Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In, MarshalAs(UnmanagedType.LPStruct)] Guid pType, [Out] out IEnumMoniker ppEnumMoniker, [In] CDef dwFlags);
    }

    [Flags]
    public enum CDef
    {
        None = 0,
        ClassDefault = 0x0001,
        BypassClassManager = 0x0002,
        ClassLegacy = 0x0004,
        MeritAboveDoNotUse = 0x0008,
        DevmonCMGRDevice = 0x0010,
        DevmonDMO = 0x0020,
        DevmonPNPDevice = 0x0040,
        DevmonFilter = 0x0080,
        DevmonSelectiveMask = 0x00f0
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaFilter : IPersist
    {
        #region IPersist Methods

        [PreserveSig]
        new int GetClassID([Out] out Guid pClassID);

        #endregion

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Run([In] long tStart);

        [PreserveSig]
        int GetState([In] int dwMilliSecsTimeout,[Out] out FilterState filtState);

        [PreserveSig]
        int SetSyncSource([In] IReferenceClock pClock);

        [PreserveSig]
        int GetSyncSource([Out] out IReferenceClock pClock);
    }


    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBaseFilter : IMediaFilter
    {
        #region IPersist Methods

        [PreserveSig]
        new int GetClassID(
            [Out] out Guid pClassID);

        #endregion

        #region IMediaFilter Methods

        [PreserveSig]
        new int Stop();

        [PreserveSig]
        new int Pause();

        [PreserveSig]
        new int Run(long tStart);

        [PreserveSig]
        new int GetState([In] int dwMilliSecsTimeout, [Out] out FilterState filtState);

        [PreserveSig]
        new int SetSyncSource([In] IReferenceClock pClock);

        [PreserveSig]
        new int GetSyncSource([Out] out IReferenceClock pClock);

        #endregion

        [PreserveSig]
        int EnumPins([Out] out IEnumPins ppEnum);

        [PreserveSig]
        int FindPin(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Id,
            [Out] out IPin ppPin
            );

        [PreserveSig]
        int QueryFilterInfo([Out] out FilterInfo pInfo);

        [PreserveSig]
        int JoinFilterGraph([In] IFilterGraph pGraph, [In, MarshalAs(UnmanagedType.LPWStr)] string pName );

        [PreserveSig]
        int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
    }


    public enum FilterState
    {
        Stopped,
        Paused,
        Running
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IReferenceClock
    {
        [PreserveSig]
        int GetTime([Out] out long pTime);

        [PreserveSig]
        int AdviseTime(
            [In] long baseTime,
            [In] long streamTime,
            [In] IntPtr hEvent, // System.Threading.WaitHandle?
            [Out] out int pdwAdviseCookie
            );

        [PreserveSig]
        int AdvisePeriodic(
            [In] long startTime,
            [In] long periodTime,
            [In] IntPtr hSemaphore, // System.Threading.WaitHandle?
            [Out] out int pdwAdviseCookie
            );

        [PreserveSig]
        int Unadvise([In] int dwAdviseCookie);
    }



    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumPins
    {
        [PreserveSig]
        int Next(
            [In] int cPins,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] ppPins,
            [In] IntPtr pcFetched
            );

        [PreserveSig]
        int Skip([In] int cPins);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumPins ppEnum);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterGraph
    {
        [PreserveSig]
        int AddFilter(
            [In] IBaseFilter pFilter,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName
            );

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        [PreserveSig]
        int FindFilterByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter
            );

        [PreserveSig]
        int ConnectDirect(
            [In] IPin ppinOut,
            [In] IPin ppinIn,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        [PreserveSig]
        [Obsolete("This method is obsolete; use the IFilterGraph2.ReconnectEx method instead.")]
        int Reconnect([In] IPin ppin);

        [PreserveSig]
        int Disconnect([In] IPin ppin);

        [PreserveSig]
        int SetDefaultSyncSource();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumFilters
    {
        [PreserveSig]
        int Next(
            [In] int cFilters,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] ppFilter,
            [In] IntPtr pcFetched
            );

        [PreserveSig]
        int Skip([In] int cFilters);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumFilters ppEnum);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct FilterInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string achName;
        [MarshalAs(UnmanagedType.Interface)] public IFilterGraph pGraph;
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPin
    {
        [PreserveSig]
        int Connect(
            [In] IPin pReceivePin,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        [PreserveSig]
        int ReceiveConnection(
            [In] IPin pReceivePin,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        [PreserveSig]
        int Disconnect();

        [PreserveSig]
        int ConnectedTo(
            [Out] out IPin ppPin);

        /// <summary>
        /// Release returned parameter with DsUtils.FreeAMMediaType
        /// </summary>
        [PreserveSig]
        int ConnectionMediaType(
            [Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        /// <summary>
        /// Release returned parameter with DsUtils.FreePinInfo
        /// </summary>
        [PreserveSig]
        int QueryPinInfo([Out] out PinInfo pInfo);

        [PreserveSig]
        int QueryDirection(out PinDirection pPinDir);

        [PreserveSig]
        int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string Id);

        [PreserveSig]
        int QueryAccept([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        [PreserveSig]
        int EnumMediaTypes([Out] out IEnumMediaTypes ppEnum);

        [PreserveSig]
        int QueryInternalConnections(
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IPin[] ppPins,
            [In, Out] ref int nPin
            );

        [PreserveSig]
        int EndOfStream();

        [PreserveSig]
        int BeginFlush();

        [PreserveSig]
        int EndFlush();

        [PreserveSig]
        int NewSegment(
            [In] long tStart,
            [In] long tStop,
            [In] double dRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumMediaTypes
    {
        [PreserveSig]
        int Next(
            [In] int cMediaTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utils.EMTMarshaler), SizeParamIndex = 0)] AMMediaType[] ppMediaTypes,
            [In] IntPtr pcFetched
            );

        [PreserveSig]
        int Skip([In] int cMediaTypes);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumMediaTypes ppEnum);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct PinInfo
    {
        [MarshalAs(UnmanagedType.Interface)] public IBaseFilter filter;
        public PinDirection dir;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string name;
    }

    public enum PinDirection
    {
        Input,
        Output
    }

    [StructLayout(LayoutKind.Sequential)]
    public class AMMediaType
    {
        public Guid majorType;
        public Guid subType;
        [MarshalAs(UnmanagedType.Bool)] public bool fixedSizeSamples;
        [MarshalAs(UnmanagedType.Bool)] public bool temporalCompression;
        public int sampleSize;
        public Guid formatType;
        public IntPtr unkPtr; // IUnknown Pointer
        public int formatSize;
        public IntPtr formatPtr; // Pointer to a buff determined by formatType
    }


    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("D8D715A3-6E5E-11D0-B3F0-00AA003761C5"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMVfwCompressDialogs
    {
        [PreserveSig]
        int ShowDialog(
            [In] VfwCompressDialogs iDialog,
            [In] IntPtr hwnd
            );

        [PreserveSig]
        int GetState(
            [In] IntPtr pState,
            [In, Out] ref int pcbState
            );

        [PreserveSig]
        int SetState(
            [In] IntPtr pState,
            [In] int pcbState
            );

        [PreserveSig]
        int SendDriverMessage(
            [In] int uMsg,
            [In] int dw1,
            [In] int dw2
            );
    }

    [ComImport, Guid("62BE5D10-60EB-11d0-BD3B-00A0C911CE86")]
    public class CreateDevEnum
    {
    }



    [Flags]
    public enum VfwCompressDialogs
    {
        None = 0,
        Config = 0x01,
        About = 0x02,
        QueryConfig = 0x04,
        QueryAbout = 0x08
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("901db4c7-31ce-41a2-85dc-8fa0bf41b8da"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICodecAPI
    {
        [PreserveSig]
        int IsSupported([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

        [PreserveSig]
        int IsModifiable([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

        [PreserveSig]
        int GetParameterRange([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api, [Out] out object ValueMin, [Out] out object ValueMax, [Out] out object SteppingDelta);

        [PreserveSig]
        int GetParameterValues([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,out IntPtr ip, [Out] out int ValuesCount);

        [PreserveSig]
        int GetDefaultValue([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,[Out, MarshalAs(UnmanagedType.Struct)] out object Value);

        [PreserveSig]
        int GetValue([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,[Out, MarshalAs(UnmanagedType.Struct)] out object Value );

        [PreserveSig]
        int SetValue( [In, MarshalAs(UnmanagedType.LPStruct)] Guid Api,[In] ref object Value);

        [PreserveSig]
        int RegisterForEvent([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api, [In] IntPtr userData);

        [PreserveSig]
        int UnregisterForEvent([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api);

        [PreserveSig]
        int SetAllDefaults();

        [PreserveSig]
        int SetValueWithNotify([In, MarshalAs(UnmanagedType.LPStruct)] Guid Api, [In] object Value, [Out] out Guid[] ChangedParam,[Out] out int ChangedParamCount );

        [PreserveSig]
        int SetAllDefaultsWithNotify([Out] out Guid[] ChangedParam,[Out] out int ChangedParamCount );

        [PreserveSig]
        int GetAllSettings([In] IStream pStream);

        [PreserveSig]
        int SetAllSettings([In] IStream pStream);

        [PreserveSig]
        int SetAllSettingsWithNotify([In] IStream pStream,[Out] out Guid[] ChangedParam, [Out] out int ChangedParamCount);
    }

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity, 
		Guid("a8809222-07bb-48ea-951c-33158100625b"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGetCapabilitiesKey
	{
		[PreserveSig]
		int GetCapabilitiesKey([Out] out IntPtr pHKey);
	}

}
