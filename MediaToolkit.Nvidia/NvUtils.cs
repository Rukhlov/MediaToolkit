using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.Nvidia
{
	public class LibNvApiException : Exception
	{
		public LibNvApiException(string callerName, string description, NvAPI.NvApiStatus status)
			: base($"{callerName} returned invalid status: {status}, {description}") { }
	}

	public class LibNvEncException : Exception
    {
        public LibNvEncException(string callerName, string description, NvEncStatus status)
            : base($"{callerName} returned invalid status: {status}, {description}") { }

        public LibNvEncException(string callerName, CuResult result, string errorName, string errorString)
            : base($"{callerName} returned invalid result: {result}. {errorName}: {errorString}") { }
    }

    public static class NvEncRegisterResourceEx
    {
        public static NvEncInputPtr AsInputPointer(this NvEncRegisterResource resource)
        {
            return new NvEncInputPtr
            {
                Handle = resource.RegisteredResource.Handle
            };
        }

        public static NvEncOutputPtr AsOutputPointer(this NvEncRegisterResource resource)
        {
            return new NvEncOutputPtr
            {
                Handle = resource.RegisteredResource.Handle
            };
        }
    }

    class MarshalHelper
    {
        public static void SetArrayData<T>(T[] items, out IntPtr targetPointer)
        {
            if (items != null && items.Length > 0)
            {
                var sizeOfItem = Marshal.SizeOf(typeof(T));
                targetPointer = Marshal.AllocHGlobal(sizeOfItem * items.Length);
                for (int i = 0; i < items.Length; i++)
                {
                    Marshal.StructureToPtr(items[i], targetPointer + (sizeOfItem * i), true);
                }
            }
            else
            {
                targetPointer = IntPtr.Zero;
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
    }


	internal static class Kernel32
	{
		[DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory")]
		public unsafe static extern bool ZeroMemory(byte* destination, int length);
	}
}
