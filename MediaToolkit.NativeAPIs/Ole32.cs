using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace MediaToolkit.NativeAPIs.Ole
{
    [StructLayout(LayoutKind.Explicit)]
    public class ConstPropVariant : IDisposable
    {
 
        [DllImport("ole32.dll", ExactSpelling = true, PreserveSig = false), SuppressUnmanagedCodeSecurity]
        protected static extern void PropVariantCopy(
            [Out, MarshalAs(UnmanagedType.LPStruct)] PropVariant pvarDest,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarSource);

 
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

        [StructLayout(LayoutKind.Sequential)]
        protected struct Blob
        {
            public int cbSize;
            public IntPtr pBlobData;
        }

        [StructLayout(LayoutKind.Sequential)]
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

        [FieldOffset(8)]
        protected ushort uiVal;

        [FieldOffset(8)]
        protected byte bVal;

        [FieldOffset(8)]
        protected int intValue;

        [FieldOffset(8)]
        protected uint uintVal;

        [FieldOffset(8)]
        protected float fltVal;

        [FieldOffset(8)]
        protected long longValue;

        [FieldOffset(8)]
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


        public static explicit operator ushort(ConstPropVariant f)
        {
            return f.GetUShort();
        }

        public static explicit operator int(ConstPropVariant f)
        {
            return f.GetInt();
        }

       
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

            if ((p == null) || (p.type != type))
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

}
