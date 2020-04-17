using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MediaToolkit.NativeAPIs.DShow
{
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
}
