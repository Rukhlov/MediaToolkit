﻿using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Ole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs.Utils
{

    public class DisplayTool
    {
        public class DisplayInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string GdiDeviceName { get; set; }
            public uint DisplayId { get; set; }
        }

        public static DISPLAYCONFIG_SOURCE_DEVICE_NAME GetDisplayConfigSourceDeviceName(LUID adapterId, uint sourceId)
        {
            DISPLAYCONFIG_SOURCE_DEVICE_NAME deviceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME
            {
                size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_SOURCE_DEVICE_NAME)),
                adapterId = adapterId,
                id = sourceId,
                type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME

            };

            int result = User32.DisplayConfigGetDeviceInfo(ref deviceInfo);
            if (result != (int)HResult.S_OK)
            {
                throw new Win32Exception(result);
            }
                
            return deviceInfo;
        }


        public static DISPLAYCONFIG_TARGET_DEVICE_NAME GetDisplayConfigTargetDeviceName(LUID adapterId, uint targetId)
        {

            var deviceInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                size = (uint) Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                adapterId = adapterId,
                id = targetId,
                type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME

            };

            var result = User32.DisplayConfigGetDeviceInfo(ref deviceInfo);
            if (result != (int)HResult.S_OK)
            {
                throw new Win32Exception(result);
            }
            

            return deviceInfo;
        }

        public static List<DisplayInfo> GetDisplayInfos()
        {
            List<DisplayInfo> displayInfos = new List<DisplayInfo>();

            uint pathCount, modeCount;
            var result = User32.GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);

            if (result != (int)HResult.S_OK)
            {
                throw new Win32Exception(result);
            }
                

            var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = User32.QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
                ref pathCount, displayPaths, ref modeCount, displayModes, IntPtr.Zero);

            if (result != (int)HResult.S_OK)
            {
                throw new Win32Exception(result);
            }


            for (int i = 0; i < modeCount; i += 2)
            {
                if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                {
                    var displayMode = displayModes[i];

                    var monitorInfo = GetDisplayConfigTargetDeviceName(displayMode.adapterId, displayMode.id);

                    var gdiDeviceInfo = GetDisplayConfigSourceDeviceName(displayModes[i + 1].adapterId, displayModes[i + 1].id);

                    var di = new DisplayInfo
                    {
                        Name = monitorInfo.monitorFriendlyDeviceName,
                        Path = monitorInfo.monitorDevicePath,
                        DisplayId = displayModes[i].id,
                        GdiDeviceName = gdiDeviceInfo.viewGdiDeviceName,
  
                    };

                    displayInfos.Add(di);
                }
                    
            }

            return displayInfos;

        }
    }

    public class ComBase
    {
        public static void SafeRelease(object comObj)
        {
            if (comObj == null)
            {
                return;
            }

            if (Marshal.IsComObject(comObj))
            {
                int refCount = Marshal.ReleaseComObject(comObj);
                Debug.Assert(refCount == 0 , "refCount == 0");
                comObj = null;
            }
        }

    }

    class PVMarshaler : ICustomMarshaler
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

}
