using Microsoft.VisualStudio.TestTools.UnitTesting;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MediaToolkit.NativeAPIs;
using System.ComponentModel;

namespace MediaToolkit.Utils.Tests
{
	[TestClass()]
	public class ProcessToolTests
	{
		[TestMethod()]
		public void CreateProcessTest()
		{
			var fileName = @"C:\Windows\System32\dfrgui.exe";
			var commandLine = "";

			//Assert.Fail();
			if ((Environment.Is64BitOperatingSystem) && (!(Environment.Is64BitProcess)))
			{
				int processId = 0;
				IntPtr ptr = IntPtr.Zero;
				try
				{
					var res = Kernel32.Wow64DisableWow64FsRedirection(ref ptr);
					if (!res)
					{
						var code = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
						var message = $"Wow64DisableWow64FsRedirection failed with error code: {code}";
						throw new Win32Exception(code, message);
					}

					processId = ProcessTool.CreateProcess(fileName, commandLine);
				}
				finally
				{
					Kernel32.Wow64RevertWow64FsRedirection(ptr);
				}

				if (processId > 0)
				{
					Process p = Process.GetProcessById(processId);
					//..
				}
				///
			
			}
			else
			{
				//..
			}

		}
	}
}