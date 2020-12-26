using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Ole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs.Utils
{

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
                Debug.Assert(refCount == 0, "refCount == 0");
                comObj = null;
            }
        }

    }

    public class MarshalHelper
    {
        public static void PtrToArray<T>(IntPtr pArray, int length, out T[] outputArray)// where T : struct
        {
            outputArray = new T[length];
            var structSize = Marshal.SizeOf(typeof(T));

            IntPtr ptr = new IntPtr(pArray.ToInt64());
            for (int i = 0; i < length; i++)
            {
                outputArray[i] = (T)Marshal.PtrToStructure(ptr, typeof(T));
                ptr = IntPtr.Add(ptr, structSize);
            }
        }

		public static T[] GetArrayData<T>(IntPtr sourcePointer, int itemCount)
		{
			var lstResult = new List<T>();
			if (sourcePointer != IntPtr.Zero && itemCount > 0)
			{
				var sizeOfItem = Marshal.SizeOf(typeof(T));
				for (int i = 0; i < itemCount; i++)
				{
					lstResult.Add(GetArrayItemData<T>(sourcePointer + (sizeOfItem * i)));
				}
			}
			return lstResult.ToArray();
		}

		public static T GetArrayItemData<T>(IntPtr sourcePointer)
		{
			return (T)Marshal.PtrToStructure(sourcePointer, typeof(T));
		}

		public static IntPtr Align(IntPtr ptr, int align)
		{
			checked { align--; }
			if (IntPtr.Size == sizeof(Int32))
			{//x86
				return new IntPtr((ptr.ToInt32() + align) & ~align);
			}
			else if (IntPtr.Size == sizeof(Int64))
			{//amd64
				return new IntPtr((ptr.ToInt64() + align) & ~align);
			}
			else
			{
				throw new NotSupportedException("Platform is neither 32 bits nor 64 bits.");
			}
		}
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


    // Used (only) by MFExtern.MFTGetInfo.  In order to perform the marshaling,
    // we need to have the pointer to the array, and the number of elements. To
    // receive all this information in the marshaler, we are using the same
    // instance of this class for multiple parameters.  So ppInputTypes &
    // pcInputTypes share an instance, and ppOutputTypes & pcOutputTypes share
    // an instance.  To make life interesting, we also need to work correctly
    // if invoked on multiple threads at the same time.
    internal class RTIMarshaler : ICustomMarshaler
    {
        private struct MyProps
        {
            public System.Collections.ArrayList m_array;
            public MFInt m_int;
            public IntPtr m_MFIntPtr;
            public IntPtr m_ArrayPtr;
        }

        // When used with MFExtern.MFTGetInfo, there are 2 parameter pairs
        // (ppInputTypes + pcInputTypes, ppOutputTypes + pcOutputTypes).  Each
        // need their own instance, so s_Props is a 2 element array.
        [ThreadStatic]
        static MyProps[] s_Props;

        // Used to indicate the index of s_Props we should be using.  It is
        // derived from the MarshalCookie.
        private int m_Cookie;

        private RTIMarshaler(string cookie)
        {
            m_Cookie = int.Parse(cookie);
        }

        public IntPtr MarshalManagedToNative(object managedObj)
        {
            IntPtr p;

            // s_Props is threadstatic, so we don't need to worry about
            // locking.  And since the only method that RTIMarshaler supports
            // is MFExtern.MFTGetInfo, we know that MarshalManagedToNative gets
            // called first.
            if (s_Props == null)
                s_Props = new MyProps[2];

            // We get called twice: Once for the MFInt, and once for the array.
            // Figure out which call this is.
            if (managedObj is MFInt)
            {
                // Save off the object.  We'll need to use Assign() on this
                // later.
                s_Props[m_Cookie].m_int = managedObj as MFInt;

                // Allocate room for the int and set it to zero;
                p = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(MFInt)));
                Marshal.WriteInt32(p, 0);

                s_Props[m_Cookie].m_MFIntPtr = p;
            }
            else // Must be the array.  FYI: Nulls don't get marshaled.
            {
                // Save off the object.  We'll be calling methods on this in
                // MarshalNativeToManaged.
                s_Props[m_Cookie].m_array = managedObj as System.Collections.ArrayList;

                s_Props[m_Cookie].m_array.Clear();

                // All we need is room for the pointer
                p = Marshal.AllocCoTaskMem(IntPtr.Size);

                // Belt-and-suspenders.  Set this to null.
                Marshal.WriteIntPtr(p, IntPtr.Zero);

                s_Props[m_Cookie].m_ArrayPtr = p;
            }

            return p;
        }

        // We have the MFInt and the array pointer.  Populate the array.
        static void Parse(MyProps p)
        {
            // If we have an array to return things in (ie MFTGetInfo wasn't
            // passed nulls).  Note that the MFInt doesn't get set in that
            // case.
            if (p.m_array != null)
            {
                // Read the count
                int count = Marshal.ReadInt32(p.m_MFIntPtr);
                p.m_int.Assign(count);

                IntPtr ip2 = Marshal.ReadIntPtr(p.m_ArrayPtr);

                // I don't know why this might happen, but it seems worth the
                // check.
                if (ip2 != IntPtr.Zero)
                {
                    try
                    {
                        int iSize = Marshal.SizeOf(typeof(MFTRegisterTypeInfo));
                        IntPtr pos = ip2;

                        // Size the array
                        p.m_array.Capacity = count;

                        // Copy in the values
                        for (int x = 0; x < count; x++)
                        {
                            MFTRegisterTypeInfo rti = new MFTRegisterTypeInfo();
                            Marshal.PtrToStructure(pos, rti);
                            pos += iSize;
                            p.m_array.Add(rti);
                        }
                    }
                    finally
                    {
                        // Free the array we got back
                        Marshal.FreeCoTaskMem(ip2);
                    }
                }
            }
        }

        // Called just after invoking the COM method.  The IntPtr is the same
        // one that just got returned from MarshalManagedToNative.  The return
        // value is unused.
        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            Debug.Assert(s_Props != null);

            // Figure out which (if either) of the MFInts this is.
            for (int x = 0; x < 2; x++)
            {
                if (pNativeData == s_Props[x].m_MFIntPtr)
                {
                    Parse(s_Props[x]);
                    break;
                }
            }

            // This value isn't actually used
            return null;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
            // Never called.
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (s_Props[m_Cookie].m_MFIntPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(s_Props[m_Cookie].m_MFIntPtr);

                s_Props[m_Cookie].m_MFIntPtr = IntPtr.Zero;
                s_Props[m_Cookie].m_int = null;
            }
            if (s_Props[m_Cookie].m_ArrayPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(s_Props[m_Cookie].m_ArrayPtr);

                s_Props[m_Cookie].m_ArrayPtr = IntPtr.Zero;
                s_Props[m_Cookie].m_array = null;
            }
        }

        // The number of bytes to marshal out
        public int GetNativeDataSize()
        {
            return -1;
        }

        // This method is called by interop to create the custom marshaler.
        // The (optional) cookie is the value specified in MarshalCookie="xxx",
        // or "" if none is specified.
        private static ICustomMarshaler GetInstance(string cookie)
        {
            return new RTIMarshaler(cookie);
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


    abstract internal class DsMarshaler : ICustomMarshaler
    {
        #region Data Members
        // The cookie isn't currently being used.
        protected string m_cookie;

        // The managed object passed in to MarshalManagedToNative, and modified in MarshalNativeToManaged
        protected object m_obj;
        #endregion

        // The constructor.  This is called from GetInstance (below)
        public DsMarshaler(string cookie)
        {
            // If we get a cookie, save it.
            m_cookie = cookie;
        }

        // Called just before invoking the COM method.  The returned IntPtr is what goes on the stack
        // for the COM call.  The input arg is the parameter that was passed to the method.
        virtual public IntPtr MarshalManagedToNative(object managedObj)
        {
            // Save off the passed-in value.  Safe since we just checked the type.
            m_obj = managedObj;

            // Create an appropriately sized buffer, blank it, and send it to the marshaler to
            // make the COM call with.
            int iSize = GetNativeDataSize() + 3;
            IntPtr p = Marshal.AllocCoTaskMem(iSize);

            for (int x = 0; x < iSize / 4; x++)
            {
                Marshal.WriteInt32(p, x * 4, 0);
            }

            return p;
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        virtual public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            return m_obj;
        }

        // Release the (now unused) buffer
        virtual public void CleanUpNativeData(IntPtr pNativeData)
        {
            if (pNativeData != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pNativeData);
            }
        }

        // Release the (now unused) managed object
        virtual public void CleanUpManagedData(object managedObj)
        {
            m_obj = null;
        }

        // This routine is (apparently) never called by the marshaler.  However it can be useful.
        abstract public int GetNativeDataSize();

        // GetInstance is called by the marshaler in preparation to doing custom marshaling.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.

        // It is commented out in this abstract class, but MUST be implemented in derived classes
        //public static ICustomMarshaler GetInstance(string cookie)
    }

    internal class EMTMarshaler : DsMarshaler
    {
        public EMTMarshaler(string cookie) : base(cookie)
        {
        }

        // Called just after invoking the COM method.  The IntPtr is the same one that just got returned
        // from MarshalManagedToNative.  The return value is unused.
        override public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            DShow.AMMediaType[] emt = m_obj as DShow.AMMediaType[];

            for (int x = 0; x < emt.Length; x++)
            {
                // Copy in the value, and advance the pointer
                IntPtr p = Marshal.ReadIntPtr(pNativeData, x * IntPtr.Size);
                if (p != IntPtr.Zero)
                {
                    emt[x] = (DShow.AMMediaType)Marshal.PtrToStructure(p, typeof(DShow.AMMediaType));
                }
                else
                {
                    emt[x] = null;
                }
            }

            return null;
        }

        // The number of bytes to marshal out
        override public int GetNativeDataSize()
        {
            // Get the array size
            int i = ((Array)m_obj).Length;

            // Multiply that times the size of a pointer
            int j = i * IntPtr.Size;

            return j;
        }

        // This method is called by interop to create the custom marshaler.  The (optional)
        // cookie is the value specified in MarshalCookie="asdf", or "" is none is specified.
        public static ICustomMarshaler GetInstance(string cookie)
        {
            return new EMTMarshaler(cookie);
        }
    }


}
