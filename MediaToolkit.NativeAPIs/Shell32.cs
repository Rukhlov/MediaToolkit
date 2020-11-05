using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
	public static class Shell32
	{
		[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
		public static extern void SHGetFolderPathW(IntPtr hwndOwner, int nFolder, IntPtr hToken, uint dwFlags, IntPtr pszPath);


	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WIN32_FIND_DATA
	{

		uint dwFileAttributes;
		System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
		System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
		System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
		uint nFileSizeHight;
		uint nFileSizeLow;
		uint dwOID;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = Defines.MaxPath)]
		string cFileName;
	}

	[ComImport]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IShellLink
	{
		[PreserveSig]
		void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, ref WIN32_FIND_DATA pfd, uint fFlags);

		[PreserveSig]
		void GetIDList(out IntPtr ppidl);

		[PreserveSig]
		void SetIDList(IntPtr ppidl);

		[PreserveSig]
		void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);

		[PreserveSig]
		void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);

		[PreserveSig]
		void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,int cch);

		[PreserveSig]
		void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);

		[PreserveSig]
		void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);

		[PreserveSig]
		void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);

		[PreserveSig]
		void GetHotkey(out ushort pwHotkey);

		[PreserveSig]
		void SetHotkey(ushort wHotkey);

		[PreserveSig]
		void GetShowCmd(out int piShowCmd);

		[PreserveSig]
		void SetShowCmd(int iShowCmd);

		[PreserveSig]
		void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch,out int piIcon);

		[PreserveSig]
		void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);

		[PreserveSig]
		void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,uint dwReserved);

		[PreserveSig]
		void Resolve(IntPtr hwnd, uint fFlags);

		[PreserveSig]
		void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
	}

	[Guid("00021401-0000-0000-C000-000000000046")]
	[ClassInterface(ClassInterfaceType.None)]
	[ComImport()]
	public class CShellLink
	{
	}

	[ComImport, Guid("0000010c-0000-0000-c000-000000000046"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersist
	{
		[PreserveSig]
		void GetClassID(out Guid pClassID);
	}


	[ComImport, Guid("0000010b-0000-0000-C000-000000000046"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersistFile : IPersist
	{
		new void GetClassID(out Guid pClassID);
		[PreserveSig]
		int IsDirty();

		[PreserveSig]
		void Load([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);

		[PreserveSig]
		void Save([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [In, MarshalAs(UnmanagedType.Bool)] bool fRemember);

		[PreserveSig]
		void SaveCompleted([In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

		[PreserveSig]
		void GetCurFile([In, MarshalAs(UnmanagedType.LPWStr)] string ppszFileName);
	}
}
