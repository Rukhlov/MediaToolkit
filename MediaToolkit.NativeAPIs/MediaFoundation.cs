using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
    public class MediaFoundation
    {

        [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
         Guid("6AB0000C-FECE-4d1f-A2AC-A9573530656E")]
        public interface IMFVideoProcessor
        {
            [PreserveSig]
            HResult GetAvailableVideoProcessorModes(
                out int lpdwNumProcessingModes,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] out Guid[] ppVideoProcessingModes);

            [PreserveSig]
            HResult GetVideoProcessorCaps(
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid lpVideoProcessorMode,
                out DXVA2VideoProcessorCaps lpVideoProcessorCaps);

            [PreserveSig]
            HResult GetVideoProcessorMode(
                out Guid lpMode);

            [PreserveSig]
            HResult SetVideoProcessorMode(
                [In, MarshalAs(UnmanagedType.LPStruct)] Guid lpMode);

            [PreserveSig]
            HResult GetProcAmpRange(
                DXVA2ProcAmp dwProperty,
                out DXVA2ValueRange pPropRange);

            [PreserveSig]
            HResult GetProcAmpValues(
                DXVA2ProcAmp dwFlags,
                [Out, MarshalAs(UnmanagedType.LPStruct)] DXVA2ProcAmpValues Values);

            [PreserveSig]
            HResult SetProcAmpValues(
                DXVA2ProcAmp dwFlags,
                [In] DXVA2ProcAmpValues pValues);

            [PreserveSig]
            HResult GetFilteringRange(
                DXVA2Filters dwProperty,
                out DXVA2ValueRange pPropRange);

            [PreserveSig]
            HResult GetFilteringValue(
                DXVA2Filters dwProperty,
                out int pValue);

            [PreserveSig]
            HResult SetFilteringValue(
                DXVA2Filters dwProperty,
                [In] ref int pValue);

            [PreserveSig]
            HResult GetBackgroundColor(
                out int lpClrBkg);

            [PreserveSig]
            HResult SetBackgroundColor(
                int ClrBkg);
        }

        [Flags, UnmanagedName("DXVA2_ProcAmp_* defines")]
        public enum DXVA2ProcAmp
        {
            None = 0,
            Brightness = 0x0001,
            Contrast = 0x0002,
            Hue = 0x0004,
            Saturation = 0x0008
        }

        [UnmanagedName("Unnamed enum")]
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

        [Flags, UnmanagedName("MFVideoAlphaBitmapFlags")]
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

        [Flags, UnmanagedName("MFVideoMixPrefs")]
        public enum MFVideoMixPrefs
        {
            None = 0,
            ForceHalfInterlace = 0x00000001,
            AllowDropToHalfInterlace = 0x00000002,
            AllowDropToBob = 0x00000004,
            ForceBob = 0x00000008,
            Mask = 0x0000000f
        }

        [Flags, UnmanagedName("EVRFilterConfigPrefs")]
        public enum EVRFilterConfigPrefs
        {
            None = 0,
            EnableQoS = 0x1,
            Mask = 0x1
        }

        [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_VideoProcessorCaps")]
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

        [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_ValueRange")]
        public struct DXVA2ValueRange
        {
            public int MinValue;
            public int MaxValue;
            public int DefaultValue;
            public int StepSize;
        }

        [StructLayout(LayoutKind.Sequential), UnmanagedName("DXVA2_ProcAmpValues")]
        public class DXVA2ProcAmpValues
        {
            public int Brightness;
            public int Contrast;
            public int Hue;
            public int Saturation;
        }


        [UnmanagedName("MF_SOURCE_READER_CONTROL_FLAG")]
        public enum MF_SOURCE_READER_CONTROL_FLAG
        {
            None = 0,
            Drain = 0x00000001
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MFInt
        {
            protected int m_value;

            public MFInt()
                : this(0)
            {
            }

            public MFInt(int v)
            {
                m_value = v;
            }

            public int GetValue()
            {
                return m_value;
            }

            // While I *could* enable this code, it almost certainly won't do what you
            // think it will.  Generally you don't want to create a *new* instance of
            // MFInt and assign a value to it.  You want to assign a value to an
            // existing instance.  In order to do this automatically, .Net would have
            // to support overloading operator =.  But since it doesn't, use Assign()

            //public static implicit operator MFInt(int f)
            //{
            //    return new MFInt(f);
            //}

            public static implicit operator int(MFInt f)
            {
                return f.m_value;
            }

            public int ToInt32()
            {
                return m_value;
            }

            public void Assign(int f)
            {
                m_value = f;
            }

            public override string ToString()
            {
                return m_value.ToString();
            }

            public override int GetHashCode()
            {
                return m_value.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is MFInt)
                {
                    return ((MFInt)obj).m_value == m_value;
                }

                return Convert.ToInt32(obj) == m_value;
            }
        }

        [Flags, UnmanagedName("MF_SOURCE_READER_FLAG")]
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

        [UnmanagedName("MF_ATTRIBUTES_MATCH_TYPE")]
        public enum MFAttributesMatchType
        {
            OurItems,
            TheirItems,
            AllItems,
            InterSection,
            Smaller
        }

        [UnmanagedName("MF_ATTRIBUTE_TYPE")]
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

        [UnmanagedName("unnamed enum")]
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


        [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
        public class UnmanagedNameAttribute : System.Attribute
        {
            private string m_Name;

            public UnmanagedNameAttribute(string s)
            {
                m_Name = s;
            }

            public override string ToString()
            {
                return m_Name;
            }
        }

        internal class PVMarshaler : ICustomMarshaler
        {
            private class MyProps
            {
                public PropVariant m_obj;
                public IntPtr m_ptr;

                private int m_InProcsss;
                private bool m_IAllocated;
                private MyProps m_Parent = null;

                [ThreadStatic]
                private static MyProps[] m_CurrentProps;

                public int GetStage()
                {
                    return m_InProcsss;
                }

                public void StageComplete()
                {
                    m_InProcsss++;
                }

                public static MyProps AddLayer(int iIndex)
                {
                    MyProps p = new MyProps();
                    p.m_Parent = m_CurrentProps[iIndex];
                    m_CurrentProps[iIndex] = p;

                    return p;
                }

                public static void SplitLayer(int iIndex)
                {
                    MyProps t = AddLayer(iIndex);
                    MyProps p = t.m_Parent;

                    t.m_InProcsss = 1;
                    t.m_ptr = p.m_ptr;
                    t.m_obj = p.m_obj;

                    p.m_InProcsss = 1;
                }

                public static MyProps GetTop(int iIndex)
                {
                    // If the member hasn't been initialized, do it now.  And no, we can't
                    // do this in the PVMarshaler constructor, since the constructor may 
                    // have been called on a different thread.
                    if (m_CurrentProps == null)
                    {
                        m_CurrentProps = new MyProps[MaxArgs];
                        for (int x = 0; x < MaxArgs; x++)
                        {
                            m_CurrentProps[x] = new MyProps();
                        }
                    }
                    return m_CurrentProps[iIndex];
                }

                public void Clear(int iIndex)
                {
                    if (m_IAllocated)
                    {
                        Marshal.FreeCoTaskMem(m_ptr);
                        m_IAllocated = false;
                    }
                    if (m_Parent == null)
                    {
                        // Never delete the last entry.
                        m_InProcsss = 0;
                        m_obj = null;
                        m_ptr = IntPtr.Zero;
                    }
                    else
                    {
                        m_obj = null;
                        m_CurrentProps[iIndex] = m_Parent;
                    }
                }

                public IntPtr Alloc(int iSize)
                {
                    IntPtr ip = Marshal.AllocCoTaskMem(iSize);
                    m_IAllocated = true;
                    return ip;
                }
            }

            private readonly int m_Index;

            // Max number of arguments in a single method call that can use
            // PVMarshaler.
            private const int MaxArgs = 2;

            private PVMarshaler(string cookie)
            {
                int iLen = cookie.Length;

                // On methods that have more than 1 PVMarshaler on a
                // single method, the cookie is in the form:
                // InterfaceName.MethodName.0 & InterfaceName.MethodName.1.
                if (cookie[iLen - 2] != '.')
                {
                    m_Index = 0;
                }
                else
                {
                    m_Index = int.Parse(cookie.Substring(iLen - 1));
                    Debug.Assert(m_Index < MaxArgs);
                }
            }

            public IntPtr MarshalManagedToNative(object managedObj)
            {
                // Nulls don't invoke custom marshaling.
                Debug.Assert(managedObj != null);

                MyProps t = MyProps.GetTop(m_Index);

                switch (t.GetStage())
                {
                    case 0:
                        {
                            // We are just starting a "Managed calling unmanaged"
                            // call.

                            // Cast the object back to a PropVariant and save it
                            // for use in MarshalNativeToManaged.
                            t.m_obj = managedObj as PropVariant;

                            // This could happen if (somehow) managedObj isn't a
                            // PropVariant.  During normal marshaling, the custom
                            // marshaler doesn't get called if the parameter is
                            // null.
                            Debug.Assert(t.m_obj != null);

                            // Release any memory currently allocated in the
                            // PropVariant.  In theory, the (managed) caller
                            // should have done this before making the call that
                            // got us here, but .Net programmers don't generally
                            // think that way.  To avoid any leaks, do it for them.
                            t.m_obj.Clear();

                            // Create an appropriately sized buffer (varies from
                            // x86 to x64).
                            int iSize = GetNativeDataSize();
                            t.m_ptr = t.Alloc(iSize);

                            // Copy in the (empty) PropVariant.  In theory we could
                            // just zero out the first 2 bytes (the VariantType),
                            // but since PropVariantClear wipes the whole struct,
                            // that's what we do here to be safe.
                            Marshal.StructureToPtr(t.m_obj, t.m_ptr, false);

                            break;
                        }
                    case 1:
                        {
                            if (!System.Object.ReferenceEquals(t.m_obj, managedObj))
                            {
                                // If we get here, we have already received a call
                                // to MarshalNativeToManaged where we created a
                                // PropVariant and stored it into t.m_obj.  But
                                // the object we just got passed here isn't the
                                // same one.  Therefore instead of being the second
                                // half of an "Unmanaged calling managed" (as
                                // m_InProcsss led us to believe), this is really
                                // the first half of a nested "Managed calling
                                // unmanaged" (see Recursion in the comments at the
                                // top of this class).  Add another layer.
                                MyProps.AddLayer(m_Index);

                                // Try this call again now that we have fixed
                                // m_CurrentProps.
                                return MarshalManagedToNative(managedObj);
                            }

                            // This is (probably) the second half of "Unmanaged
                            // calling managed."  However, it could be the first
                            // half of a nested usage of PropVariants.  If it is a
                            // nested, we'll eventually figure that out in case 2.

                            // Copy the data from the managed object into the
                            // native pointer that we received in
                            // MarshalNativeToManaged.
                            Marshal.StructureToPtr(t.m_obj, t.m_ptr, false);

                            break;
                        }
                    case 2:
                        {
                            // Apparently this is 'part 3' of a 2 part call.  Which
                            // means we are doing a nested call.  Normally we would
                            // catch the fact that this is a nested call with the
                            // ReferenceEquals check above.  However, if the same
                            // PropVariant instance is being passed thru again, we
                            // end up here.
                            // So, add a layer.
                            MyProps.SplitLayer(m_Index);

                            // Try this call again now that we have fixed
                            // m_CurrentProps.
                            return MarshalManagedToNative(managedObj);
                        }
                    default:
                        {
                            Environment.FailFast("Something horrible has " +
                                                 "happened, probaby due to " +
                                                 "marshaling of nested " +
                                                 "PropVariant calls.");
                            break;
                        }
                }
                t.StageComplete();

                return t.m_ptr;
            }

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                // Nulls don't invoke custom marshaling.
                Debug.Assert(pNativeData != IntPtr.Zero);

                MyProps t = MyProps.GetTop(m_Index);

                switch (t.GetStage())
                {
                    case 0:
                        {
                            // We are just starting a "Unmanaged calling managed"
                            // call.

                            // Caller should have cleared variant before calling
                            // us.  Might be acceptable for types *other* than
                            // IUnknown, String, Blob and StringArray, but it is
                            // still bad design.  We're checking for it, but we
                            // work around it.

                            // Read the 16bit VariantType.
                            Debug.Assert(Marshal.ReadInt16(pNativeData) == 0);

                            // Create an empty managed PropVariant without using
                            // pNativeData.
                            t.m_obj = new PropVariant();

                            // Save the pointer for use in MarshalManagedToNative.
                            t.m_ptr = pNativeData;

                            break;
                        }
                    case 1:
                        {
                            if (t.m_ptr != pNativeData)
                            {
                                // If we get here, we have already received a call
                                // to MarshalManagedToNative where we created an
                                // IntPtr and stored it into t.m_ptr.  But the
                                // value we just got passed here isn't the same
                                // one.  Therefore instead of being the second half
                                // of a "Managed calling unmanaged" (as m_InProcsss
                                // led us to believe) this is really the first half
                                // of a nested "Unmanaged calling managed" (see
                                // Recursion in the comments at the top of this
                                // class).  Add another layer.
                                MyProps.AddLayer(m_Index);

                                // Try this call again now that we have fixed
                                // m_CurrentProps.
                                return MarshalNativeToManaged(pNativeData);
                            }

                            // This is (probably) the second half of "Managed
                            // calling unmanaged."  However, it could be the first
                            // half of a nested usage of PropVariants.  If it is a
                            // nested, we'll eventually figure that out in case 2.

                            // Copy the data from the native pointer into the
                            // managed object that we received in
                            // MarshalManagedToNative.
                            Marshal.PtrToStructure(pNativeData, t.m_obj);

                            break;
                        }
                    case 2:
                        {
                            // Apparently this is 'part 3' of a 2 part call.  Which
                            // means we are doing a nested call.  Normally we would
                            // catch the fact that this is a nested call with the
                            // (t.m_ptr != pNativeData) check above.  However, if
                            // the same PropVariant instance is being passed thru
                            // again, we end up here.  So, add a layer.
                            MyProps.SplitLayer(m_Index);

                            // Try this call again now that we have fixed
                            // m_CurrentProps.
                            return MarshalNativeToManaged(pNativeData);
                        }
                    default:
                        {
                            Environment.FailFast("Something horrible has " +
                                                 "happened, probaby due to " +
                                                 "marshaling of nested " +
                                                 "PropVariant calls.");
                            break;
                        }
                }
                t.StageComplete();

                return t.m_obj;
            }

            public void CleanUpManagedData(object ManagedObj)
            {
                // Note that if there are nested calls, one of the Cleanup*Data
                // methods will be called at the end of each pair:

                // MarshalNativeToManaged
                // MarshalManagedToNative
                // CleanUpManagedData
                //
                // or for recursion:
                //
                // MarshalManagedToNative 1
                // MarshalNativeToManaged 2
                // MarshalManagedToNative 2
                // CleanUpManagedData     2
                // MarshalNativeToManaged 1
                // CleanUpNativeData      1

                // Clear() either pops an entry, or clears
                // the values for the next call.
                MyProps t = MyProps.GetTop(m_Index);
                t.Clear(m_Index);
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                // Clear() either pops an entry, or clears
                // the values for the next call.
                MyProps t = MyProps.GetTop(m_Index);
                t.Clear(m_Index);
            }

            // The number of bytes to marshal.  Size varies between x86 and x64.
            public int GetNativeDataSize()
            {
                return Marshal.SizeOf(typeof(PropVariant));
            }

            // This method is called by interop to create the custom marshaler.
            // The (optional) cookie is the value specified in
            // MarshalCookie="asdf", or "" if none is specified.
            private static ICustomMarshaler GetInstance(string cookie)
            {
                return new PVMarshaler(cookie);
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public class ConstPropVariant : IDisposable
        {
            #region Declarations

            [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
            protected static extern void PropVariantCopy(
                [Out, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvarDest,
                [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarSource
                );

            #endregion

            public enum VariantType : short
            {
                None = 0,
                Short = 2,
                Int32 = 3,
                Float = 4,
                Double = 5,
                IUnknown = 13,
                UByte = 17,
                UShort = 18,
                UInt32 = 19,
                Int64 = 20,
                UInt64 = 21,
                String = 31,
                Guid = 72,
                Blob = 0x1000 + 17,
                StringArray = 0x1000 + 31
            }

            [StructLayout(LayoutKind.Sequential), UnmanagedName("BLOB")]
            protected struct Blob
            {
                public int cbSize;
                public IntPtr pBlobData;
            }

            [StructLayout(LayoutKind.Sequential), UnmanagedName("CALPWSTR")]
            protected struct CALPWstr
            {
                public int cElems;
                public IntPtr pElems;
            }

            #region Member variables

            [FieldOffset(0)]
            protected VariantType type;

            [FieldOffset(2)]
            protected short reserved1;

            [FieldOffset(4)]
            protected short reserved2;

            [FieldOffset(6)]
            protected short reserved3;

            [FieldOffset(8)]
            protected short iVal;

            [FieldOffset(8), CLSCompliant(false)]
            protected ushort uiVal;

            [FieldOffset(8), CLSCompliant(false)]
            protected byte bVal;

            [FieldOffset(8)]
            protected int intValue;

            [FieldOffset(8), CLSCompliant(false)]
            protected uint uintVal;

            [FieldOffset(8)]
            protected float fltVal;

            [FieldOffset(8)]
            protected long longValue;

            [FieldOffset(8), CLSCompliant(false)]
            protected ulong ulongValue;

            [FieldOffset(8)]
            protected double doubleValue;

            [FieldOffset(8)]
            protected Blob blobValue;

            [FieldOffset(8)]
            protected IntPtr ptr;

            [FieldOffset(8)]
            protected CALPWstr calpwstrVal;

            #endregion

            public ConstPropVariant()
                : this(VariantType.None)
            {
            }

            protected ConstPropVariant(VariantType v)
            {
                type = v;
            }

            public static explicit operator string(ConstPropVariant f)
            {
                return f.GetString();
            }

            public static explicit operator string[] (ConstPropVariant f)
            {
                return f.GetStringArray();
            }

            public static explicit operator byte(ConstPropVariant f)
            {
                return f.GetUByte();
            }

            public static explicit operator short(ConstPropVariant f)
            {
                return f.GetShort();
            }

            [CLSCompliant(false)]
            public static explicit operator ushort(ConstPropVariant f)
            {
                return f.GetUShort();
            }

            public static explicit operator int(ConstPropVariant f)
            {
                return f.GetInt();
            }

            [CLSCompliant(false)]
            public static explicit operator uint(ConstPropVariant f)
            {
                return f.GetUInt();
            }

            public static explicit operator float(ConstPropVariant f)
            {
                return f.GetFloat();
            }

            public static explicit operator double(ConstPropVariant f)
            {
                return f.GetDouble();
            }

            public static explicit operator long(ConstPropVariant f)
            {
                return f.GetLong();
            }

            [CLSCompliant(false)]
            public static explicit operator ulong(ConstPropVariant f)
            {
                return f.GetULong();
            }

            public static explicit operator Guid(ConstPropVariant f)
            {
                return f.GetGuid();
            }

            public static explicit operator byte[] (ConstPropVariant f)
            {
                return f.GetBlob();
            }

            // I decided not to do implicits since perf is likely to be
            // better recycling the PropVariant, and the only way I can
            // see to support Implicit is to create a new PropVariant.
            // Also, since I can't free the previous instance, IUnknowns
            // will linger until the GC cleans up.  Not what I think I
            // want.

            public MFAttributeType GetMFAttributeType()
            {
                switch (type)
                {
                    case VariantType.None:
                    case VariantType.UInt32:
                    case VariantType.UInt64:
                    case VariantType.Double:
                    case VariantType.Guid:
                    case VariantType.String:
                    case VariantType.Blob:
                    case VariantType.IUnknown:
                        {
                            return (MFAttributeType)type;
                        }
                    default:
                        {
                            throw new Exception("Type is not a MFAttributeType");
                        }
                }
            }

            public VariantType GetVariantType()
            {
                return type;
            }

            public string[] GetStringArray()
            {
                if (type == VariantType.StringArray)
                {
                    string[] sa;

                    int iCount = calpwstrVal.cElems;
                    sa = new string[iCount];

                    for (int x = 0; x < iCount; x++)
                    {
                        sa[x] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(calpwstrVal.pElems, x * IntPtr.Size));
                    }

                    return sa;
                }
                throw new ArgumentException("PropVariant contents not a string array");
            }

            public string GetString()
            {
                if (type == VariantType.String)
                {
                    return Marshal.PtrToStringUni(ptr);
                }
                throw new ArgumentException("PropVariant contents not a string");
            }

            public byte GetUByte()
            {
                if (type == VariantType.UByte)
                {
                    return bVal;
                }
                throw new ArgumentException("PropVariant contents not a byte");
            }

            public short GetShort()
            {
                if (type == VariantType.Short)
                {
                    return iVal;
                }
                throw new ArgumentException("PropVariant contents not a Short");
            }

            [CLSCompliant(false)]
            public ushort GetUShort()
            {
                if (type == VariantType.UShort)
                {
                    return uiVal;
                }
                throw new ArgumentException("PropVariant contents not a UShort");
            }

            public int GetInt()
            {
                if (type == VariantType.Int32)
                {
                    return intValue;
                }
                throw new ArgumentException("PropVariant contents not an int32");
            }

            [CLSCompliant(false)]
            public uint GetUInt()
            {
                if (type == VariantType.UInt32)
                {
                    return uintVal;
                }
                throw new ArgumentException("PropVariant contents not a uint32");
            }

            public long GetLong()
            {
                if (type == VariantType.Int64)
                {
                    return longValue;
                }
                throw new ArgumentException("PropVariant contents not an int64");
            }

            [CLSCompliant(false)]
            public ulong GetULong()
            {
                if (type == VariantType.UInt64)
                {
                    return ulongValue;
                }
                throw new ArgumentException("PropVariant contents not a uint64");
            }

            public float GetFloat()
            {
                if (type == VariantType.Float)
                {
                    return fltVal;
                }
                throw new ArgumentException("PropVariant contents not a Float");
            }

            public double GetDouble()
            {
                if (type == VariantType.Double)
                {
                    return doubleValue;
                }
                throw new ArgumentException("PropVariant contents not a double");
            }

            public Guid GetGuid()
            {
                if (type == VariantType.Guid)
                {
                    return (Guid)Marshal.PtrToStructure(ptr, typeof(Guid));
                }
                throw new ArgumentException("PropVariant contents not a Guid");
            }

            public byte[] GetBlob()
            {
                if (type == VariantType.Blob)
                {
                    byte[] b = new byte[blobValue.cbSize];

                    if (blobValue.cbSize > 0)
                    {
                        Marshal.Copy(blobValue.pBlobData, b, 0, blobValue.cbSize);
                    }

                    return b;
                }
                throw new ArgumentException("PropVariant contents not a Blob");
            }

            public object GetBlob(Type t, int offset)
            {
                if (type == VariantType.Blob)
                {
                    object o;

                    if (blobValue.cbSize > offset)
                    {
                        if (blobValue.cbSize >= Marshal.SizeOf(t) + offset)
                        {
                            o = Marshal.PtrToStructure(blobValue.pBlobData + offset, t);
                        }
                        else
                        {
                            throw new ArgumentException("Blob wrong size");
                        }
                    }
                    else
                    {
                        o = null;
                    }

                    return o;
                }
                throw new ArgumentException("PropVariant contents not a Blob");
            }

            public object GetBlob(Type t)
            {
                return GetBlob(t, 0);
            }

            public object GetIUnknown()
            {
                if (type == VariantType.IUnknown)
                {
                    if (ptr != IntPtr.Zero)
                    {
                        return Marshal.GetObjectForIUnknown(ptr);
                    }
                    else
                    {
                        return null;
                    }
                }
                throw new ArgumentException("PropVariant contents not an IUnknown");
            }

            public void Copy(PropVariant pdest)
            {
                if (pdest == null)
                {
                    throw new Exception("Null PropVariant sent to Copy");
                }

                // Copy doesn't clear the dest.
                pdest.Clear();

                PropVariantCopy(pdest, this);
            }

            public override string ToString()
            {
                // This method is primarily intended for debugging so that a readable string will show
                // up in the output window
                string sRet;

                switch (type)
                {
                    case VariantType.None:
                        {
                            sRet = "<Empty>";
                            break;
                        }

                    case VariantType.Blob:
                        {
                            const string FormatString = "x2"; // Hex 2 digit format
                            const int MaxEntries = 16;

                            byte[] blob = GetBlob();

                            // Number of bytes we're going to format
                            int n = Math.Min(MaxEntries, blob.Length);

                            if (n == 0)
                            {
                                sRet = "<Empty Array>";
                            }
                            else
                            {
                                // Only format the first MaxEntries bytes
                                sRet = blob[0].ToString(FormatString);
                                for (int i = 1; i < n; i++)
                                {
                                    sRet += ',' + blob[i].ToString(FormatString);
                                }

                                // If the string is longer, add an indicator
                                if (blob.Length > n)
                                {
                                    sRet += "...";
                                }
                            }
                            break;
                        }

                    case VariantType.Float:
                        {
                            sRet = GetFloat().ToString();
                            break;
                        }

                    case VariantType.Double:
                        {
                            sRet = GetDouble().ToString();
                            break;
                        }

                    case VariantType.Guid:
                        {
                            sRet = GetGuid().ToString();
                            break;
                        }

                    case VariantType.IUnknown:
                        {
                            object o = GetIUnknown();
                            if (o != null)
                            {
                                sRet = GetIUnknown().ToString();
                            }
                            else
                            {
                                sRet = "<null>";
                            }
                            break;
                        }

                    case VariantType.String:
                        {
                            sRet = GetString();
                            break;
                        }

                    case VariantType.Short:
                        {
                            sRet = GetShort().ToString();
                            break;
                        }

                    case VariantType.UByte:
                        {
                            sRet = GetUByte().ToString();
                            break;
                        }

                    case VariantType.UShort:
                        {
                            sRet = GetUShort().ToString();
                            break;
                        }

                    case VariantType.Int32:
                        {
                            sRet = GetInt().ToString();
                            break;
                        }

                    case VariantType.UInt32:
                        {
                            sRet = GetUInt().ToString();
                            break;
                        }

                    case VariantType.Int64:
                        {
                            sRet = GetLong().ToString();
                            break;
                        }

                    case VariantType.UInt64:
                        {
                            sRet = GetULong().ToString();
                            break;
                        }

                    case VariantType.StringArray:
                        {
                            sRet = "";
                            foreach (string entry in GetStringArray())
                            {
                                sRet += (sRet.Length == 0 ? "\"" : ",\"") + entry + '\"';
                            }
                            break;
                        }
                    default:
                        {
                            sRet = base.ToString();
                            break;
                        }
                }

                return sRet;
            }

            public override int GetHashCode()
            {
                // Give a (slightly) better hash value in case someone uses PropVariants
                // in a hash table.
                int iRet;

                switch (type)
                {
                    case VariantType.None:
                        {
                            iRet = base.GetHashCode();
                            break;
                        }

                    case VariantType.Blob:
                        {
                            iRet = GetBlob().GetHashCode();
                            break;
                        }

                    case VariantType.Float:
                        {
                            iRet = GetFloat().GetHashCode();
                            break;
                        }

                    case VariantType.Double:
                        {
                            iRet = GetDouble().GetHashCode();
                            break;
                        }

                    case VariantType.Guid:
                        {
                            iRet = GetGuid().GetHashCode();
                            break;
                        }

                    case VariantType.IUnknown:
                        {
                            iRet = GetIUnknown().GetHashCode();
                            break;
                        }

                    case VariantType.String:
                        {
                            iRet = GetString().GetHashCode();
                            break;
                        }

                    case VariantType.UByte:
                        {
                            iRet = GetUByte().GetHashCode();
                            break;
                        }

                    case VariantType.Short:
                        {
                            iRet = GetShort().GetHashCode();
                            break;
                        }

                    case VariantType.UShort:
                        {
                            iRet = GetUShort().GetHashCode();
                            break;
                        }

                    case VariantType.Int32:
                        {
                            iRet = GetInt().GetHashCode();
                            break;
                        }

                    case VariantType.UInt32:
                        {
                            iRet = GetUInt().GetHashCode();
                            break;
                        }

                    case VariantType.Int64:
                        {
                            iRet = GetLong().GetHashCode();
                            break;
                        }

                    case VariantType.UInt64:
                        {
                            iRet = GetULong().GetHashCode();
                            break;
                        }

                    case VariantType.StringArray:
                        {
                            iRet = GetStringArray().GetHashCode();
                            break;
                        }
                    default:
                        {
                            iRet = base.GetHashCode();
                            break;
                        }
                }

                return iRet;
            }

            public override bool Equals(object obj)
            {
                bool bRet;
                PropVariant p = obj as PropVariant;

                if ((((object)p) == null) || (p.type != type))
                {
                    bRet = false;
                }
                else
                {
                    switch (type)
                    {
                        case VariantType.None:
                            {
                                bRet = true;
                                break;
                            }

                        case VariantType.Blob:
                            {
                                byte[] b1;
                                byte[] b2;

                                b1 = GetBlob();
                                b2 = p.GetBlob();

                                if (b1.Length == b2.Length)
                                {
                                    bRet = true;
                                    for (int x = 0; x < b1.Length; x++)
                                    {
                                        if (b1[x] != b2[x])
                                        {
                                            bRet = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    bRet = false;
                                }
                                break;
                            }

                        case VariantType.Float:
                            {
                                bRet = GetFloat() == p.GetFloat();
                                break;
                            }

                        case VariantType.Double:
                            {
                                bRet = GetDouble() == p.GetDouble();
                                break;
                            }

                        case VariantType.Guid:
                            {
                                bRet = GetGuid() == p.GetGuid();
                                break;
                            }

                        case VariantType.IUnknown:
                            {
                                bRet = GetIUnknown() == p.GetIUnknown();
                                break;
                            }

                        case VariantType.String:
                            {
                                bRet = GetString() == p.GetString();
                                break;
                            }

                        case VariantType.UByte:
                            {
                                bRet = GetUByte() == p.GetUByte();
                                break;
                            }

                        case VariantType.Short:
                            {
                                bRet = GetShort() == p.GetShort();
                                break;
                            }

                        case VariantType.UShort:
                            {
                                bRet = GetUShort() == p.GetUShort();
                                break;
                            }

                        case VariantType.Int32:
                            {
                                bRet = GetInt() == p.GetInt();
                                break;
                            }

                        case VariantType.UInt32:
                            {
                                bRet = GetUInt() == p.GetUInt();
                                break;
                            }

                        case VariantType.Int64:
                            {
                                bRet = GetLong() == p.GetLong();
                                break;
                            }

                        case VariantType.UInt64:
                            {
                                bRet = GetULong() == p.GetULong();
                                break;
                            }

                        case VariantType.StringArray:
                            {
                                string[] sa1;
                                string[] sa2;

                                sa1 = GetStringArray();
                                sa2 = p.GetStringArray();

                                if (sa1.Length == sa2.Length)
                                {
                                    bRet = true;
                                    for (int x = 0; x < sa1.Length; x++)
                                    {
                                        if (sa1[x] != sa2[x])
                                        {
                                            bRet = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    bRet = false;
                                }
                                break;
                            }
                        default:
                            {
                                bRet = base.Equals(obj);
                                break;
                            }
                    }
                }

                return bRet;
            }

            public static bool operator ==(ConstPropVariant pv1, ConstPropVariant pv2)
            {
                // If both are null, or both are same instance, return true.
                if (System.Object.ReferenceEquals(pv1, pv2))
                {
                    return true;
                }

                // If one is null, but not both, return false.
                if (((object)pv1 == null) || ((object)pv2 == null))
                {
                    return false;
                }

                return pv1.Equals(pv2);
            }

            public static bool operator !=(ConstPropVariant pv1, ConstPropVariant pv2)
            {
                return !(pv1 == pv2);
            }

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="disposing">
            /// <c>true</c> to release both managed and unmanaged resources;
            /// <c>false</c> to release only unmanaged resources.
            /// </param>
            protected virtual void Dispose(bool disposing)
            {
                // If we are a ConstPropVariant, we must *not* call PropVariantClear.  That
                // would release the *caller's* copy of the data, which would probably make
                // him cranky.  If we are a PropVariant, the PropVariant.Dispose gets called
                // as well, which *does* do a PropVariantClear.
                type = VariantType.None;
#if DEBUG
                longValue = 0;
#endif
            }

            #endregion
        }

        [StructLayout(LayoutKind.Explicit)]
        public class PropVariant : ConstPropVariant
        {
            #region Declarations

            [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
            protected static extern void PropVariantClear(
                [In, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvar
                );

            #endregion

            public PropVariant()
                : base(VariantType.None)
            {
            }

            public PropVariant(string value)
                : base(VariantType.String)
            {
                ptr = Marshal.StringToCoTaskMemUni(value);
            }

            public PropVariant(string[] value)
                : base(VariantType.StringArray)
            {
                calpwstrVal.cElems = value.Length;
                calpwstrVal.pElems = Marshal.AllocCoTaskMem(IntPtr.Size * value.Length);

                for (int x = 0; x < value.Length; x++)
                {
                    Marshal.WriteIntPtr(calpwstrVal.pElems, x * IntPtr.Size, Marshal.StringToCoTaskMemUni(value[x]));
                }
            }

            public PropVariant(byte value)
                : base(VariantType.UByte)
            {
                bVal = value;
            }

            public PropVariant(short value)
                : base(VariantType.Short)
            {
                iVal = value;
            }

            [CLSCompliant(false)]
            public PropVariant(ushort value)
                : base(VariantType.UShort)
            {
                uiVal = value;
            }

            public PropVariant(int value)
                : base(VariantType.Int32)
            {
                intValue = value;
            }

            [CLSCompliant(false)]
            public PropVariant(uint value)
                : base(VariantType.UInt32)
            {
                uintVal = value;
            }

            public PropVariant(float value)
                : base(VariantType.Float)
            {
                fltVal = value;
            }

            public PropVariant(double value)
                : base(VariantType.Double)
            {
                doubleValue = value;
            }

            public PropVariant(long value)
                : base(VariantType.Int64)
            {
                longValue = value;
            }

            [CLSCompliant(false)]
            public PropVariant(ulong value)
                : base(VariantType.UInt64)
            {
                ulongValue = value;
            }

            public PropVariant(Guid value)
                : base(VariantType.Guid)
            {
                ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
                Marshal.StructureToPtr(value, ptr, false);
            }

            public PropVariant(byte[] value)
                : base(VariantType.Blob)
            {
                blobValue.cbSize = value.Length;
                blobValue.pBlobData = Marshal.AllocCoTaskMem(value.Length);
                Marshal.Copy(value, 0, blobValue.pBlobData, value.Length);
            }

            public PropVariant(object value)
                : base(VariantType.IUnknown)
            {
                if (value == null)
                {
                    ptr = IntPtr.Zero;
                }
                else if (Marshal.IsComObject(value))
                {
                    ptr = Marshal.GetIUnknownForObject(value);
                }
                else
                {
                    type = VariantType.Blob;

                    blobValue.cbSize = Marshal.SizeOf(value);
                    blobValue.pBlobData = Marshal.AllocCoTaskMem(blobValue.cbSize);

                    Marshal.StructureToPtr(value, blobValue.pBlobData, false);
                }
            }

            public PropVariant(IntPtr value)
                : base(VariantType.None)
            {
                Marshal.PtrToStructure(value, this);
            }

            public PropVariant(ConstPropVariant value)
            {
                if (value != null)
                {
                    PropVariantCopy(this, value);
                }
                else
                {
                    throw new NullReferenceException("null passed to PropVariant constructor");
                }
            }

            ~PropVariant()
            {
                Dispose(false);
            }

            public void Clear()
            {
                PropVariantClear(this);
            }

            #region IDisposable Members

            protected override void Dispose(bool disposing)
            {
                Clear();
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }

            #endregion
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

        [Flags, UnmanagedName("MF_MEDIATYPE_EQUAL_* defines")]
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
                [In, Out] MFInt pdwReserved,
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
                [In, Out] MFInt pdwReserved,
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

        public enum HResult
        {
            #region COM HRESULTs

            S_OK = 0,
            S_FALSE = 1,

            E_PENDING = unchecked((int)0x8000000A),

            /// <summary>Catastrophic failure</summary>
            E_UNEXPECTED = unchecked((int)0x8000FFFF),

            /// <summary>Not implemented</summary>
            E_NOTIMPL = unchecked((int)0x80004001),

            /// <summary>Ran out of memory</summary>
            E_OUTOFMEMORY = unchecked((int)0x8007000E),

            /// <summary>One or more arguments are invalid</summary>
            E_INVALIDARG = unchecked((int)0x80070057),

            /// <summary>No such interface supported</summary>
            E_NOINTERFACE = unchecked((int)0x80004002),

            /// <summary>Invalid pointer</summary>
            E_POINTER = unchecked((int)0x80004003),

            /// <summary>Invalid handle</summary>
            E_HANDLE = unchecked((int)0x80070006),

            /// <summary>Operation aborted</summary>
            E_ABORT = unchecked((int)0x80004004),

            /// <summary>Unspecified error</summary>
            E_FAIL = unchecked((int)0x80004005),

            /// <summary>General access denied error</summary>
            E_ACCESSDENIED = unchecked((int)0x80070005),

            #endregion

            #region Win32 HRESULTs

            /// <summary>The system cannot find the file specified.</summary>
            /// <unmanaged>HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND)</unmanaged>
            WIN32_ERROR_FILE_NOT_FOUND = unchecked((int)0x80070002),

            /// <summary>More data is available.</summary>
            /// <unmanaged>HRESULT_FROM_WIN32(ERROR_MORE_DATA)</unmanaged>
            WIN32_ERROR_MORE_DATA = unchecked((int)0x800700ea),

            /// <summary>No more data is available.</summary>
            /// <unmanaged>HRESULT_FROM_WIN32(ERROR_NO_MORE_ITEMS)</unmanaged>
            WIN32_ERROR_NO_MORE_ITEMS = unchecked((int)0x80070103),

            /// <summary>Element not found.</summary>
            /// <unmanaged>HRESULT_FROM_WIN32(ERROR_NOT_FOUND)</unmanaged>
            WIN32_ERROR_NOT_FOUND = unchecked((int)0x80070490),

            #endregion

            #region Structured Storage HRESULTs

            /// <summary>The underlying file was converted to compound file format.</summary>
            STG_S_CONVERTED = unchecked((int)0x00030200),

            /// <summary>Multiple opens prevent consolidated. (commit succeeded).</summary>
            STG_S_MULTIPLEOPENS = unchecked((int)0x00030204),

            /// <summary>Consolidation of the storage file failed. (commit succeeded).</summary>
            STG_S_CONSOLIDATIONFAILED = unchecked((int)0x00030205),

            /// <summary>Consolidation of the storage file is inappropriate. (commit succeeded).</summary>
            STG_S_CANNOTCONSOLIDATE = unchecked((int)0x00030206),

            /// <summary>Unable to perform requested operation.</summary>
            STG_E_INVALIDFUNCTION = unchecked((int)0x80030001),

            /// <summary>The file could not be found.</summary>
            STG_E_FILENOTFOUND = unchecked((int)0x80030002),

            /// <summary>There are insufficient resources to open another file.</summary>
            STG_E_TOOMANYOPENFILES = unchecked((int)0x80030004),

            /// <summary>Access Denied.</summary>
            STG_E_ACCESSDENIED = unchecked((int)0x80030005),

            /// <summary>There is insufficient memory available to complete operation.</summary>
            STG_E_INSUFFICIENTMEMORY = unchecked((int)0x80030008),

            /// <summary>Invalid pointer error.</summary>
            STG_E_INVALIDPOINTER = unchecked((int)0x80030009),

            /// <summary>A disk error occurred during a write operation.</summary>
            STG_E_WRITEFAULT = unchecked((int)0x8003001D),

            /// <summary>A lock violation has occurred.</summary>
            STG_E_LOCKVIOLATION = unchecked((int)0x80030021),

            /// <summary>File already exists.</summary>
            STG_E_FILEALREADYEXISTS = unchecked((int)0x80030050),

            /// <summary>Invalid parameter error.</summary>
            STG_E_INVALIDPARAMETER = unchecked((int)0x80030057),

            /// <summary>There is insufficient disk space to complete operation.</summary>
            STG_E_MEDIUMFULL = unchecked((int)0x80030070),

            /// <summary>The name is not valid.</summary>
            STG_E_INVALIDNAME = unchecked((int)0x800300FC),

            /// <summary>Invalid flag error.</summary>
            STG_E_INVALIDFLAG = unchecked((int)0x800300FF),

            /// <summary>The storage has been changed since the last commit.</summary>
            STG_E_NOTCURRENT = unchecked((int)0x80030101),

            /// <summary>Attempted to use an object that has ceased to exist.</summary>
            STG_E_REVERTED = unchecked((int)0x80030102),

            /// <summary>Can't save.</summary>
            STG_E_CANTSAVE = unchecked((int)0x80030103),

            #endregion

            #region Various HRESULTs

            /// <summary>The function failed because the specified GDI device did not have any monitors associated with it.</summary>
            ERROR_GRAPHICS_NO_MONITORS_CORRESPOND_TO_DISPLAY_DEVICE = unchecked((int)0xC02625E5),

            #endregion

            #region Media Foundation HRESULTs

            MF_E_PLATFORM_NOT_INITIALIZED = unchecked((int)0xC00D36B0),

            MF_E_CAPTURE_ENGINE_ALL_EFFECTS_REMOVED = unchecked((int)0xC00DABE5),
            MF_E_CAPTURE_NO_SAMPLES_IN_QUEUE = unchecked((int)0xC00DABEB),
            MF_E_CAPTURE_PROPERTY_SET_DURING_PHOTO = unchecked((int)0xC00DABEA),
            MF_E_CAPTURE_SOURCE_DEVICE_EXTENDEDPROP_OP_IN_PROGRESS = unchecked((int)0xC00DABE9),
            MF_E_CAPTURE_SOURCE_NO_AUDIO_STREAM_PRESENT = unchecked((int)0xC00DABE8),
            MF_E_CAPTURE_SOURCE_NO_INDEPENDENT_PHOTO_STREAM_PRESENT = unchecked((int)0xC00DABE6),
            MF_E_CAPTURE_SOURCE_NO_VIDEO_STREAM_PRESENT = unchecked((int)0xC00DABE7),
            MF_E_HARDWARE_DRM_UNSUPPORTED = unchecked((int)0xC00D3706),
            MF_E_HDCP_AUTHENTICATION_FAILURE = unchecked((int)0xC00D7188),
            MF_E_HDCP_LINK_FAILURE = unchecked((int)0xC00D7189),
            MF_E_HW_ACCELERATED_THUMBNAIL_NOT_SUPPORTED = unchecked((int)0xC00DABEC),
            MF_E_NET_COMPANION_DRIVER_DISCONNECT = unchecked((int)0xC00D4295),
            MF_E_OPERATION_IN_PROGRESS = unchecked((int)0xC00D3705),
            MF_E_SINK_HEADERS_NOT_FOUND = unchecked((int)0xC00D4A45),
            MF_INDEX_SIZE_ERR = unchecked((int)0x80700001),
            MF_INVALID_ACCESS_ERR = unchecked((int)0x8070000F),
            MF_INVALID_STATE_ERR = unchecked((int)0x8070000B),
            MF_NOT_FOUND_ERR = unchecked((int)0x80700008),
            MF_NOT_SUPPORTED_ERR = unchecked((int)0x80700009),
            MF_PARSE_ERR = unchecked((int)0x80700051),
            MF_QUOTA_EXCEEDED_ERR = unchecked((int)0x80700016),
            MF_SYNTAX_ERR = unchecked((int)0x8070000C),

            MF_E_BUFFERTOOSMALL = unchecked((int)0xC00D36B1),

            MF_E_INVALIDREQUEST = unchecked((int)0xC00D36B2),
            MF_E_INVALIDSTREAMNUMBER = unchecked((int)0xC00D36B3),
            MF_E_INVALIDMEDIATYPE = unchecked((int)0xC00D36B4),
            MF_E_NOTACCEPTING = unchecked((int)0xC00D36B5),
            MF_E_NOT_INITIALIZED = unchecked((int)0xC00D36B6),
            MF_E_UNSUPPORTED_REPRESENTATION = unchecked((int)0xC00D36B7),
            MF_E_NO_MORE_TYPES = unchecked((int)0xC00D36B9),
            MF_E_UNSUPPORTED_SERVICE = unchecked((int)0xC00D36BA),
            MF_E_UNEXPECTED = unchecked((int)0xC00D36BB),
            MF_E_INVALIDNAME = unchecked((int)0xC00D36BC),
            MF_E_INVALIDTYPE = unchecked((int)0xC00D36BD),
            MF_E_INVALID_FILE_FORMAT = unchecked((int)0xC00D36BE),
            MF_E_INVALIDINDEX = unchecked((int)0xC00D36BF),
            MF_E_INVALID_TIMESTAMP = unchecked((int)0xC00D36C0),
            MF_E_UNSUPPORTED_SCHEME = unchecked((int)0xC00D36C3),
            MF_E_UNSUPPORTED_BYTESTREAM_TYPE = unchecked((int)0xC00D36C4),
            MF_E_UNSUPPORTED_TIME_FORMAT = unchecked((int)0xC00D36C5),
            MF_E_NO_SAMPLE_TIMESTAMP = unchecked((int)0xC00D36C8),
            MF_E_NO_SAMPLE_DURATION = unchecked((int)0xC00D36C9),
            MF_E_INVALID_STREAM_DATA = unchecked((int)0xC00D36CB),
            MF_E_RT_UNAVAILABLE = unchecked((int)0xC00D36CF),
            MF_E_UNSUPPORTED_RATE = unchecked((int)0xC00D36D0),
            MF_E_THINNING_UNSUPPORTED = unchecked((int)0xC00D36D1),
            MF_E_REVERSE_UNSUPPORTED = unchecked((int)0xC00D36D2),
            MF_E_UNSUPPORTED_RATE_TRANSITION = unchecked((int)0xC00D36D3),
            MF_E_RATE_CHANGE_PREEMPTED = unchecked((int)0xC00D36D4),
            MF_E_NOT_FOUND = unchecked((int)0xC00D36D5),
            MF_E_NOT_AVAILABLE = unchecked((int)0xC00D36D6),
            MF_E_NO_CLOCK = unchecked((int)0xC00D36D7),
            MF_S_MULTIPLE_BEGIN = unchecked((int)0x000D36D8),
            MF_E_MULTIPLE_BEGIN = unchecked((int)0xC00D36D9),
            MF_E_MULTIPLE_SUBSCRIBERS = unchecked((int)0xC00D36DA),
            MF_E_TIMER_ORPHANED = unchecked((int)0xC00D36DB),
            MF_E_STATE_TRANSITION_PENDING = unchecked((int)0xC00D36DC),
            MF_E_UNSUPPORTED_STATE_TRANSITION = unchecked((int)0xC00D36DD),
            MF_E_UNRECOVERABLE_ERROR_OCCURRED = unchecked((int)0xC00D36DE),
            MF_E_SAMPLE_HAS_TOO_MANY_BUFFERS = unchecked((int)0xC00D36DF),
            MF_E_SAMPLE_NOT_WRITABLE = unchecked((int)0xC00D36E0),
            MF_E_INVALID_KEY = unchecked((int)0xC00D36E2),
            MF_E_BAD_STARTUP_VERSION = unchecked((int)0xC00D36E3),
            MF_E_UNSUPPORTED_CAPTION = unchecked((int)0xC00D36E4),
            MF_E_INVALID_POSITION = unchecked((int)0xC00D36E5),
            MF_E_ATTRIBUTENOTFOUND = unchecked((int)0xC00D36E6),
            MF_E_PROPERTY_TYPE_NOT_ALLOWED = unchecked((int)0xC00D36E7),
            MF_E_PROPERTY_TYPE_NOT_SUPPORTED = unchecked((int)0xC00D36E8),
            MF_E_PROPERTY_EMPTY = unchecked((int)0xC00D36E9),
            MF_E_PROPERTY_NOT_EMPTY = unchecked((int)0xC00D36EA),
            MF_E_PROPERTY_VECTOR_NOT_ALLOWED = unchecked((int)0xC00D36EB),
            MF_E_PROPERTY_VECTOR_REQUIRED = unchecked((int)0xC00D36EC),
            MF_E_OPERATION_CANCELLED = unchecked((int)0xC00D36ED),
            MF_E_BYTESTREAM_NOT_SEEKABLE = unchecked((int)0xC00D36EE),
            MF_E_DISABLED_IN_SAFEMODE = unchecked((int)0xC00D36EF),
            MF_E_CANNOT_PARSE_BYTESTREAM = unchecked((int)0xC00D36F0),
            MF_E_SOURCERESOLVER_MUTUALLY_EXCLUSIVE_FLAGS = unchecked((int)0xC00D36F1),
            MF_E_MEDIAPROC_WRONGSTATE = unchecked((int)0xC00D36F2),
            MF_E_RT_THROUGHPUT_NOT_AVAILABLE = unchecked((int)0xC00D36F3),
            MF_E_RT_TOO_MANY_CLASSES = unchecked((int)0xC00D36F4),
            MF_E_RT_WOULDBLOCK = unchecked((int)0xC00D36F5),
            MF_E_NO_BITPUMP = unchecked((int)0xC00D36F6),
            MF_E_RT_OUTOFMEMORY = unchecked((int)0xC00D36F7),
            MF_E_RT_WORKQUEUE_CLASS_NOT_SPECIFIED = unchecked((int)0xC00D36F8),
            MF_E_INSUFFICIENT_BUFFER = unchecked((int)0xC00D7170),
            MF_E_CANNOT_CREATE_SINK = unchecked((int)0xC00D36FA),
            MF_E_BYTESTREAM_UNKNOWN_LENGTH = unchecked((int)0xC00D36FB),
            MF_E_SESSION_PAUSEWHILESTOPPED = unchecked((int)0xC00D36FC),
            MF_S_ACTIVATE_REPLACED = unchecked((int)0x000D36FD),
            MF_E_FORMAT_CHANGE_NOT_SUPPORTED = unchecked((int)0xC00D36FE),
            MF_E_INVALID_WORKQUEUE = unchecked((int)0xC00D36FF),
            MF_E_DRM_UNSUPPORTED = unchecked((int)0xC00D3700),
            MF_E_UNAUTHORIZED = unchecked((int)0xC00D3701),
            MF_E_OUT_OF_RANGE = unchecked((int)0xC00D3702),
            MF_E_INVALID_CODEC_MERIT = unchecked((int)0xC00D3703),
            MF_E_HW_MFT_FAILED_START_STREAMING = unchecked((int)0xC00D3704),
            MF_S_ASF_PARSEINPROGRESS = unchecked((int)0x400D3A98),
            MF_E_ASF_PARSINGINCOMPLETE = unchecked((int)0xC00D3A98),
            MF_E_ASF_MISSINGDATA = unchecked((int)0xC00D3A99),
            MF_E_ASF_INVALIDDATA = unchecked((int)0xC00D3A9A),
            MF_E_ASF_OPAQUEPACKET = unchecked((int)0xC00D3A9B),
            MF_E_ASF_NOINDEX = unchecked((int)0xC00D3A9C),
            MF_E_ASF_OUTOFRANGE = unchecked((int)0xC00D3A9D),
            MF_E_ASF_INDEXNOTLOADED = unchecked((int)0xC00D3A9E),
            MF_E_ASF_TOO_MANY_PAYLOADS = unchecked((int)0xC00D3A9F),
            MF_E_ASF_UNSUPPORTED_STREAM_TYPE = unchecked((int)0xC00D3AA0),
            MF_E_ASF_DROPPED_PACKET = unchecked((int)0xC00D3AA1),
            MF_E_NO_EVENTS_AVAILABLE = unchecked((int)0xC00D3E80),
            MF_E_INVALID_STATE_TRANSITION = unchecked((int)0xC00D3E82),
            MF_E_END_OF_STREAM = unchecked((int)0xC00D3E84),
            MF_E_SHUTDOWN = unchecked((int)0xC00D3E85),
            MF_E_MP3_NOTFOUND = unchecked((int)0xC00D3E86),
            MF_E_MP3_OUTOFDATA = unchecked((int)0xC00D3E87),
            MF_E_MP3_NOTMP3 = unchecked((int)0xC00D3E88),
            MF_E_MP3_NOTSUPPORTED = unchecked((int)0xC00D3E89),
            MF_E_NO_DURATION = unchecked((int)0xC00D3E8A),
            MF_E_INVALID_FORMAT = unchecked((int)0xC00D3E8C),
            MF_E_PROPERTY_NOT_FOUND = unchecked((int)0xC00D3E8D),
            MF_E_PROPERTY_READ_ONLY = unchecked((int)0xC00D3E8E),
            MF_E_PROPERTY_NOT_ALLOWED = unchecked((int)0xC00D3E8F),
            MF_E_MEDIA_SOURCE_NOT_STARTED = unchecked((int)0xC00D3E91),
            MF_E_UNSUPPORTED_FORMAT = unchecked((int)0xC00D3E98),
            MF_E_MP3_BAD_CRC = unchecked((int)0xC00D3E99),
            MF_E_NOT_PROTECTED = unchecked((int)0xC00D3E9A),
            MF_E_MEDIA_SOURCE_WRONGSTATE = unchecked((int)0xC00D3E9B),
            MF_E_MEDIA_SOURCE_NO_STREAMS_SELECTED = unchecked((int)0xC00D3E9C),
            MF_E_CANNOT_FIND_KEYFRAME_SAMPLE = unchecked((int)0xC00D3E9D),

            MF_E_UNSUPPORTED_CHARACTERISTICS = unchecked((int)0xC00D3E9E),
            MF_E_NO_AUDIO_RECORDING_DEVICE = unchecked((int)0xC00D3E9F),
            MF_E_AUDIO_RECORDING_DEVICE_IN_USE = unchecked((int)0xC00D3EA0),
            MF_E_AUDIO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA1),
            MF_E_VIDEO_RECORDING_DEVICE_INVALIDATED = unchecked((int)0xC00D3EA2),
            MF_E_VIDEO_RECORDING_DEVICE_PREEMPTED = unchecked((int)0xC00D3EA3),

            MF_E_NETWORK_RESOURCE_FAILURE = unchecked((int)0xC00D4268),
            MF_E_NET_WRITE = unchecked((int)0xC00D4269),
            MF_E_NET_READ = unchecked((int)0xC00D426A),
            MF_E_NET_REQUIRE_NETWORK = unchecked((int)0xC00D426B),
            MF_E_NET_REQUIRE_ASYNC = unchecked((int)0xC00D426C),
            MF_E_NET_BWLEVEL_NOT_SUPPORTED = unchecked((int)0xC00D426D),
            MF_E_NET_STREAMGROUPS_NOT_SUPPORTED = unchecked((int)0xC00D426E),
            MF_E_NET_MANUALSS_NOT_SUPPORTED = unchecked((int)0xC00D426F),
            MF_E_NET_INVALID_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D4270),
            MF_E_NET_CACHESTREAM_NOT_FOUND = unchecked((int)0xC00D4271),
            MF_I_MANUAL_PROXY = unchecked((int)0x400D4272),
            MF_E_NET_REQUIRE_INPUT = unchecked((int)0xC00D4274),
            MF_E_NET_REDIRECT = unchecked((int)0xC00D4275),
            MF_E_NET_REDIRECT_TO_PROXY = unchecked((int)0xC00D4276),
            MF_E_NET_TOO_MANY_REDIRECTS = unchecked((int)0xC00D4277),
            MF_E_NET_TIMEOUT = unchecked((int)0xC00D4278),
            MF_E_NET_CLIENT_CLOSE = unchecked((int)0xC00D4279),
            MF_E_NET_BAD_CONTROL_DATA = unchecked((int)0xC00D427A),
            MF_E_NET_INCOMPATIBLE_SERVER = unchecked((int)0xC00D427B),
            MF_E_NET_UNSAFE_URL = unchecked((int)0xC00D427C),
            MF_E_NET_CACHE_NO_DATA = unchecked((int)0xC00D427D),
            MF_E_NET_EOL = unchecked((int)0xC00D427E),
            MF_E_NET_BAD_REQUEST = unchecked((int)0xC00D427F),
            MF_E_NET_INTERNAL_SERVER_ERROR = unchecked((int)0xC00D4280),
            MF_E_NET_SESSION_NOT_FOUND = unchecked((int)0xC00D4281),
            MF_E_NET_NOCONNECTION = unchecked((int)0xC00D4282),
            MF_E_NET_CONNECTION_FAILURE = unchecked((int)0xC00D4283),
            MF_E_NET_INCOMPATIBLE_PUSHSERVER = unchecked((int)0xC00D4284),
            MF_E_NET_SERVER_ACCESSDENIED = unchecked((int)0xC00D4285),
            MF_E_NET_PROXY_ACCESSDENIED = unchecked((int)0xC00D4286),
            MF_E_NET_CANNOTCONNECT = unchecked((int)0xC00D4287),
            MF_E_NET_INVALID_PUSH_TEMPLATE = unchecked((int)0xC00D4288),
            MF_E_NET_INVALID_PUSH_PUBLISHING_POINT = unchecked((int)0xC00D4289),
            MF_E_NET_BUSY = unchecked((int)0xC00D428A),
            MF_E_NET_RESOURCE_GONE = unchecked((int)0xC00D428B),
            MF_E_NET_ERROR_FROM_PROXY = unchecked((int)0xC00D428C),
            MF_E_NET_PROXY_TIMEOUT = unchecked((int)0xC00D428D),
            MF_E_NET_SERVER_UNAVAILABLE = unchecked((int)0xC00D428E),
            MF_E_NET_TOO_MUCH_DATA = unchecked((int)0xC00D428F),
            MF_E_NET_SESSION_INVALID = unchecked((int)0xC00D4290),
            MF_E_OFFLINE_MODE = unchecked((int)0xC00D4291),
            MF_E_NET_UDP_BLOCKED = unchecked((int)0xC00D4292),
            MF_E_NET_UNSUPPORTED_CONFIGURATION = unchecked((int)0xC00D4293),
            MF_E_NET_PROTOCOL_DISABLED = unchecked((int)0xC00D4294),
            MF_E_ALREADY_INITIALIZED = unchecked((int)0xC00D4650),
            MF_E_BANDWIDTH_OVERRUN = unchecked((int)0xC00D4651),
            MF_E_LATE_SAMPLE = unchecked((int)0xC00D4652),
            MF_E_FLUSH_NEEDED = unchecked((int)0xC00D4653),
            MF_E_INVALID_PROFILE = unchecked((int)0xC00D4654),
            MF_E_INDEX_NOT_COMMITTED = unchecked((int)0xC00D4655),
            MF_E_NO_INDEX = unchecked((int)0xC00D4656),
            MF_E_CANNOT_INDEX_IN_PLACE = unchecked((int)0xC00D4657),
            MF_E_MISSING_ASF_LEAKYBUCKET = unchecked((int)0xC00D4658),
            MF_E_INVALID_ASF_STREAMID = unchecked((int)0xC00D4659),
            MF_E_STREAMSINK_REMOVED = unchecked((int)0xC00D4A38),
            MF_E_STREAMSINKS_OUT_OF_SYNC = unchecked((int)0xC00D4A3A),
            MF_E_STREAMSINKS_FIXED = unchecked((int)0xC00D4A3B),
            MF_E_STREAMSINK_EXISTS = unchecked((int)0xC00D4A3C),
            MF_E_SAMPLEALLOCATOR_CANCELED = unchecked((int)0xC00D4A3D),
            MF_E_SAMPLEALLOCATOR_EMPTY = unchecked((int)0xC00D4A3E),
            MF_E_SINK_ALREADYSTOPPED = unchecked((int)0xC00D4A3F),
            MF_E_ASF_FILESINK_BITRATE_UNKNOWN = unchecked((int)0xC00D4A40),
            MF_E_SINK_NO_STREAMS = unchecked((int)0xC00D4A41),
            MF_S_SINK_NOT_FINALIZED = unchecked((int)0x000D4A42),
            MF_E_METADATA_TOO_LONG = unchecked((int)0xC00D4A43),
            MF_E_SINK_NO_SAMPLES_PROCESSED = unchecked((int)0xC00D4A44),
            MF_E_VIDEO_REN_NO_PROCAMP_HW = unchecked((int)0xC00D4E20),
            MF_E_VIDEO_REN_NO_DEINTERLACE_HW = unchecked((int)0xC00D4E21),
            MF_E_VIDEO_REN_COPYPROT_FAILED = unchecked((int)0xC00D4E22),
            MF_E_VIDEO_REN_SURFACE_NOT_SHARED = unchecked((int)0xC00D4E23),
            MF_E_VIDEO_DEVICE_LOCKED = unchecked((int)0xC00D4E24),
            MF_E_NEW_VIDEO_DEVICE = unchecked((int)0xC00D4E25),
            MF_E_NO_VIDEO_SAMPLE_AVAILABLE = unchecked((int)0xC00D4E26),
            MF_E_NO_AUDIO_PLAYBACK_DEVICE = unchecked((int)0xC00D4E84),
            MF_E_AUDIO_PLAYBACK_DEVICE_IN_USE = unchecked((int)0xC00D4E85),
            MF_E_AUDIO_PLAYBACK_DEVICE_INVALIDATED = unchecked((int)0xC00D4E86),
            MF_E_AUDIO_SERVICE_NOT_RUNNING = unchecked((int)0xC00D4E87),
            MF_E_TOPO_INVALID_OPTIONAL_NODE = unchecked((int)0xC00D520E),
            MF_E_TOPO_CANNOT_FIND_DECRYPTOR = unchecked((int)0xC00D5211),
            MF_E_TOPO_CODEC_NOT_FOUND = unchecked((int)0xC00D5212),
            MF_E_TOPO_CANNOT_CONNECT = unchecked((int)0xC00D5213),
            MF_E_TOPO_UNSUPPORTED = unchecked((int)0xC00D5214),
            MF_E_TOPO_INVALID_TIME_ATTRIBUTES = unchecked((int)0xC00D5215),
            MF_E_TOPO_LOOPS_IN_TOPOLOGY = unchecked((int)0xC00D5216),
            MF_E_TOPO_MISSING_PRESENTATION_DESCRIPTOR = unchecked((int)0xC00D5217),
            MF_E_TOPO_MISSING_STREAM_DESCRIPTOR = unchecked((int)0xC00D5218),
            MF_E_TOPO_STREAM_DESCRIPTOR_NOT_SELECTED = unchecked((int)0xC00D5219),
            MF_E_TOPO_MISSING_SOURCE = unchecked((int)0xC00D521A),
            MF_E_TOPO_SINK_ACTIVATES_UNSUPPORTED = unchecked((int)0xC00D521B),
            MF_E_SEQUENCER_UNKNOWN_SEGMENT_ID = unchecked((int)0xC00D61AC),
            MF_S_SEQUENCER_CONTEXT_CANCELED = unchecked((int)0x000D61AD),
            MF_E_NO_SOURCE_IN_CACHE = unchecked((int)0xC00D61AE),
            MF_S_SEQUENCER_SEGMENT_AT_END_OF_STREAM = unchecked((int)0x000D61AF),
            MF_E_TRANSFORM_TYPE_NOT_SET = unchecked((int)0xC00D6D60),
            MF_E_TRANSFORM_STREAM_CHANGE = unchecked((int)0xC00D6D61),
            MF_E_TRANSFORM_INPUT_REMAINING = unchecked((int)0xC00D6D62),
            MF_E_TRANSFORM_PROFILE_MISSING = unchecked((int)0xC00D6D63),
            MF_E_TRANSFORM_PROFILE_INVALID_OR_CORRUPT = unchecked((int)0xC00D6D64),
            MF_E_TRANSFORM_PROFILE_TRUNCATED = unchecked((int)0xC00D6D65),
            MF_E_TRANSFORM_PROPERTY_PID_NOT_RECOGNIZED = unchecked((int)0xC00D6D66),
            MF_E_TRANSFORM_PROPERTY_VARIANT_TYPE_WRONG = unchecked((int)0xC00D6D67),
            MF_E_TRANSFORM_PROPERTY_NOT_WRITEABLE = unchecked((int)0xC00D6D68),
            MF_E_TRANSFORM_PROPERTY_ARRAY_VALUE_WRONG_NUM_DIM = unchecked((int)0xC00D6D69),
            MF_E_TRANSFORM_PROPERTY_VALUE_SIZE_WRONG = unchecked((int)0xC00D6D6A),
            MF_E_TRANSFORM_PROPERTY_VALUE_OUT_OF_RANGE = unchecked((int)0xC00D6D6B),
            MF_E_TRANSFORM_PROPERTY_VALUE_INCOMPATIBLE = unchecked((int)0xC00D6D6C),
            MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_OUTPUT_MEDIATYPE = unchecked((int)0xC00D6D6D),
            MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_INPUT_MEDIATYPE = unchecked((int)0xC00D6D6E),
            MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_MEDIATYPE_COMBINATION = unchecked((int)0xC00D6D6F),
            MF_E_TRANSFORM_CONFLICTS_WITH_OTHER_CURRENTLY_ENABLED_FEATURES = unchecked((int)0xC00D6D70),
            MF_E_TRANSFORM_NEED_MORE_INPUT = unchecked((int)0xC00D6D72),
            MF_E_TRANSFORM_NOT_POSSIBLE_FOR_CURRENT_SPKR_CONFIG = unchecked((int)0xC00D6D73),
            MF_E_TRANSFORM_CANNOT_CHANGE_MEDIATYPE_WHILE_PROCESSING = unchecked((int)0xC00D6D74),
            MF_S_TRANSFORM_DO_NOT_PROPAGATE_EVENT = unchecked((int)0x000D6D75),
            MF_E_UNSUPPORTED_D3D_TYPE = unchecked((int)0xC00D6D76),
            MF_E_TRANSFORM_ASYNC_LOCKED = unchecked((int)0xC00D6D77),
            MF_E_TRANSFORM_CANNOT_INITIALIZE_ACM_DRIVER = unchecked((int)0xC00D6D78L),
            MF_E_LICENSE_INCORRECT_RIGHTS = unchecked((int)0xC00D7148),
            MF_E_LICENSE_OUTOFDATE = unchecked((int)0xC00D7149),
            MF_E_LICENSE_REQUIRED = unchecked((int)0xC00D714A),
            MF_E_DRM_HARDWARE_INCONSISTENT = unchecked((int)0xC00D714B),
            MF_E_NO_CONTENT_PROTECTION_MANAGER = unchecked((int)0xC00D714C),
            MF_E_LICENSE_RESTORE_NO_RIGHTS = unchecked((int)0xC00D714D),
            MF_E_BACKUP_RESTRICTED_LICENSE = unchecked((int)0xC00D714E),
            MF_E_LICENSE_RESTORE_NEEDS_INDIVIDUALIZATION = unchecked((int)0xC00D714F),
            MF_S_PROTECTION_NOT_REQUIRED = unchecked((int)0x000D7150),
            MF_E_COMPONENT_REVOKED = unchecked((int)0xC00D7151),
            MF_E_TRUST_DISABLED = unchecked((int)0xC00D7152),
            MF_E_WMDRMOTA_NO_ACTION = unchecked((int)0xC00D7153),
            MF_E_WMDRMOTA_ACTION_ALREADY_SET = unchecked((int)0xC00D7154),
            MF_E_WMDRMOTA_DRM_HEADER_NOT_AVAILABLE = unchecked((int)0xC00D7155),
            MF_E_WMDRMOTA_DRM_ENCRYPTION_SCHEME_NOT_SUPPORTED = unchecked((int)0xC00D7156),
            MF_E_WMDRMOTA_ACTION_MISMATCH = unchecked((int)0xC00D7157),
            MF_E_WMDRMOTA_INVALID_POLICY = unchecked((int)0xC00D7158),
            MF_E_POLICY_UNSUPPORTED = unchecked((int)0xC00D7159),
            MF_E_OPL_NOT_SUPPORTED = unchecked((int)0xC00D715A),
            MF_E_TOPOLOGY_VERIFICATION_FAILED = unchecked((int)0xC00D715B),
            MF_E_SIGNATURE_VERIFICATION_FAILED = unchecked((int)0xC00D715C),
            MF_E_DEBUGGING_NOT_ALLOWED = unchecked((int)0xC00D715D),
            MF_E_CODE_EXPIRED = unchecked((int)0xC00D715E),
            MF_E_GRL_VERSION_TOO_LOW = unchecked((int)0xC00D715F),
            MF_E_GRL_RENEWAL_NOT_FOUND = unchecked((int)0xC00D7160),
            MF_E_GRL_EXTENSIBLE_ENTRY_NOT_FOUND = unchecked((int)0xC00D7161),
            MF_E_KERNEL_UNTRUSTED = unchecked((int)0xC00D7162),
            MF_E_PEAUTH_UNTRUSTED = unchecked((int)0xC00D7163),
            MF_E_NON_PE_PROCESS = unchecked((int)0xC00D7165),
            MF_E_REBOOT_REQUIRED = unchecked((int)0xC00D7167),
            MF_S_WAIT_FOR_POLICY_SET = unchecked((int)0x000D7168),
            MF_S_VIDEO_DISABLED_WITH_UNKNOWN_SOFTWARE_OUTPUT = unchecked((int)0x000D7169),
            MF_E_GRL_INVALID_FORMAT = unchecked((int)0xC00D716A),
            MF_E_GRL_UNRECOGNIZED_FORMAT = unchecked((int)0xC00D716B),
            MF_E_ALL_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716C),
            MF_E_PROCESS_RESTART_REQUIRED = unchecked((int)0xC00D716D),
            MF_E_USERMODE_UNTRUSTED = unchecked((int)0xC00D716E),
            MF_E_PEAUTH_SESSION_NOT_STARTED = unchecked((int)0xC00D716F),
            MF_E_PEAUTH_PUBLICKEY_REVOKED = unchecked((int)0xC00D7171),
            MF_E_GRL_ABSENT = unchecked((int)0xC00D7172),
            MF_S_PE_TRUSTED = unchecked((int)0x000D7173),
            MF_E_PE_UNTRUSTED = unchecked((int)0xC00D7174),
            MF_E_PEAUTH_NOT_STARTED = unchecked((int)0xC00D7175),
            MF_E_INCOMPATIBLE_SAMPLE_PROTECTION = unchecked((int)0xC00D7176),
            MF_E_PE_SESSIONS_MAXED = unchecked((int)0xC00D7177),
            MF_E_HIGH_SECURITY_LEVEL_CONTENT_NOT_ALLOWED = unchecked((int)0xC00D7178),
            MF_E_TEST_SIGNED_COMPONENTS_NOT_ALLOWED = unchecked((int)0xC00D7179),
            MF_E_ITA_UNSUPPORTED_ACTION = unchecked((int)0xC00D717A),
            MF_E_ITA_ERROR_PARSING_SAP_PARAMETERS = unchecked((int)0xC00D717B),
            MF_E_POLICY_MGR_ACTION_OUTOFBOUNDS = unchecked((int)0xC00D717C),
            MF_E_BAD_OPL_STRUCTURE_FORMAT = unchecked((int)0xC00D717D),
            MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_PROTECTION_GUID = unchecked((int)0xC00D717E),
            MF_E_NO_PMP_HOST = unchecked((int)0xC00D717F),
            MF_E_ITA_OPL_DATA_NOT_INITIALIZED = unchecked((int)0xC00D7180),
            MF_E_ITA_UNRECOGNIZED_ANALOG_VIDEO_OUTPUT = unchecked((int)0xC00D7181),
            MF_E_ITA_UNRECOGNIZED_DIGITAL_VIDEO_OUTPUT = unchecked((int)0xC00D7182),

            MF_E_RESOLUTION_REQUIRES_PMP_CREATION_CALLBACK = unchecked((int)0xC00D7183),
            MF_E_INVALID_AKE_CHANNEL_PARAMETERS = unchecked((int)0xC00D7184),
            MF_E_CONTENT_PROTECTION_SYSTEM_NOT_ENABLED = unchecked((int)0xC00D7185),
            MF_E_UNSUPPORTED_CONTENT_PROTECTION_SYSTEM = unchecked((int)0xC00D7186),
            MF_E_DRM_MIGRATION_NOT_SUPPORTED = unchecked((int)0xC00D7187),

            MF_E_CLOCK_INVALID_CONTINUITY_KEY = unchecked((int)0xC00D9C40),
            MF_E_CLOCK_NO_TIME_SOURCE = unchecked((int)0xC00D9C41),
            MF_E_CLOCK_STATE_ALREADY_SET = unchecked((int)0xC00D9C42),
            MF_E_CLOCK_NOT_SIMPLE = unchecked((int)0xC00D9C43),
            MF_S_CLOCK_STOPPED = unchecked((int)0x000D9C44),
            MF_E_NO_MORE_DROP_MODES = unchecked((int)0xC00DA028),
            MF_E_NO_MORE_QUALITY_LEVELS = unchecked((int)0xC00DA029),
            MF_E_DROPTIME_NOT_SUPPORTED = unchecked((int)0xC00DA02A),
            MF_E_QUALITYKNOB_WAIT_LONGER = unchecked((int)0xC00DA02B),
            MF_E_QM_INVALIDSTATE = unchecked((int)0xC00DA02C),
            MF_E_TRANSCODE_NO_CONTAINERTYPE = unchecked((int)0xC00DA410),
            MF_E_TRANSCODE_PROFILE_NO_MATCHING_STREAMS = unchecked((int)0xC00DA411),
            MF_E_TRANSCODE_NO_MATCHING_ENCODER = unchecked((int)0xC00DA412),

            MF_E_TRANSCODE_INVALID_PROFILE = unchecked((int)0xC00DA413),

            MF_E_ALLOCATOR_NOT_INITIALIZED = unchecked((int)0xC00DA7F8),
            MF_E_ALLOCATOR_NOT_COMMITED = unchecked((int)0xC00DA7F9),
            MF_E_ALLOCATOR_ALREADY_COMMITED = unchecked((int)0xC00DA7FA),
            MF_E_STREAM_ERROR = unchecked((int)0xC00DA7FB),
            MF_E_INVALID_STREAM_STATE = unchecked((int)0xC00DA7FC),
            MF_E_HW_STREAM_NOT_CONNECTED = unchecked((int)0xC00DA7FD),

            MF_E_NO_CAPTURE_DEVICES_AVAILABLE = unchecked((int)0xC00DABE0),
            MF_E_CAPTURE_SINK_OUTPUT_NOT_SET = unchecked((int)0xC00DABE1),
            MF_E_CAPTURE_SINK_MIRROR_ERROR = unchecked((int)0xC00DABE2),
            MF_E_CAPTURE_SINK_ROTATE_ERROR = unchecked((int)0xC00DABE3),
            MF_E_CAPTURE_ENGINE_INVALID_OP = unchecked((int)0xC00DABE4),

            MF_E_DXGI_DEVICE_NOT_INITIALIZED = unchecked((int)0x80041000),
            MF_E_DXGI_NEW_VIDEO_DEVICE = unchecked((int)0x80041001),
            MF_E_DXGI_VIDEO_DEVICE_LOCKED = unchecked((int)0x80041002),

            #endregion
        }
    }
}
