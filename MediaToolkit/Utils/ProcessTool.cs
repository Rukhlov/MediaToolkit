using MediaToolkit.NativeAPIs;
using MediaToolkit.NativeAPIs.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{
	public class ProcessTool
	{
		private static TraceSource tracer = new TraceSource("MediaToolkit");

		public static int StartProcessWithSystemToken(string applicationName, string commandLine)
		{
			tracer.TraceEvent(TraceEventType.Verbose, 0, "StartProcessWithSystemToken(...) " + applicationName + " " + commandLine);

			var systemProcessName = "lsass";
			//var systemProcessName = "winlogon";
			using (Process currentProcess = Process.GetCurrentProcess())
			{
				IntPtr hCurrentProcToken = IntPtr.Zero;
				try
				{
					var hCurrentProcess = currentProcess.Handle;

					bool res = AdvApi32.OpenProcessToken(hCurrentProcess, AdvApi32.TokenAccess.TOKEN_ALL_ACCESS, out hCurrentProcToken);
					if (!res)
					{
						var code = Marshal.GetLastWin32Error();
						var message = $"Failed to retrieve handle to processes access token. OpenProcessToken failed with error: {code}";
						throw new Win32Exception(code, message);
					}

					var accessTokenPrivileges = ATPrivilege.GetPrivilegesFromToken(hCurrentProcToken);

					//https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createprocesswithtokenw
					//The process that calls CreateProcessWithTokenW must have the SE_IMPERSONATE_NAME privilege. 
					//If this function fails with ERROR_PRIVILEGE_NOT_HELD(1314), use the CreateProcessAsUser or CreateProcessWithLogonW function instead.			
					var impersonatePrivilege = accessTokenPrivileges.FirstOrDefault(t => t.Name == PrivilegeConstants.SE_IMPERSONATE_NAME);
					if (impersonatePrivilege == null)
					{
						//TODO:
						throw new NotSupportedException();
					}

					if (!((impersonatePrivilege.Attributes & PrivilegeConstants.SE_PRIVILEGE_ENABLED) == PrivilegeConstants.SE_PRIVILEGE_ENABLED))
					{
						//TODO:...
						//AdjustTokenPrivileges(...)

						throw new NotImplementedException(PrivilegeConstants.SE_IMPERSONATE_NAME + " disabled");
					}

				}
				finally
				{
					if (hCurrentProcToken != IntPtr.Zero)
					{
						Kernel32.CloseHandle(hCurrentProcToken);
					}
				}
			}


			IntPtr hOriginalSystemToken = IntPtr.Zero;

			try
			{
				hOriginalSystemToken = GetTokenFromProcess(systemProcessName);

				IntPtr hDuplicatedSystemToken = IntPtr.Zero;
				try
				{
					AdvApi32.SECURITY_ATTRIBUTES secAttr = new AdvApi32.SECURITY_ATTRIBUTES();
					secAttr.bInheritHandle = 0;


					bool result = AdvApi32.DuplicateTokenEx(hOriginalSystemToken,
									AdvApi32.TokenAccess.TOKEN_ALL_ACCESS, ref secAttr,
									AdvApi32.SecurityImpersonationLevel.SecurityImpersonation,
									AdvApi32.TokenType.TokenPrimary,
									out hDuplicatedSystemToken);

					if (!result)
					{
						var code = Marshal.GetLastWin32Error();
						var message = $"Failed to duplicate new primary token.";

						throw new Win32Exception(code, message);
					}



					AdvApi32.PROCESS_INFORMATION lpProcessInformation = default(AdvApi32.PROCESS_INFORMATION);
					try
					{
						var dwLogonFlags = AdvApi32.LogonFlags.NetCredentialsOnly;//???
						var dwCreationFlags = AdvApi32.CreationFlags.NewConsole;// ??
						var lpEnvironment = IntPtr.Zero;
						string lpCurrentDirectory = null;//@"C:\";

						//TODO: setup info...
						var lpStartupInfo = new AdvApi32.STARTUPINFO();

						var args = string.Join(" ", applicationName, commandLine);
						result = AdvApi32.CreateProcessWithTokenW(hDuplicatedSystemToken,
							dwLogonFlags,
							null, args,
							dwCreationFlags, lpEnvironment, lpCurrentDirectory,
							ref lpStartupInfo, out lpProcessInformation);

						if (!result)
						{
							var code = Marshal.GetLastWin32Error();
							var message = $"Failed to create shell. CreateProcessWithTokenW failed with error code: {code}";
							throw new Win32Exception(code, message);
						}

						return lpProcessInformation.dwProcessId;

					}
					finally
					{
						if (lpProcessInformation.hProcess != IntPtr.Zero)
						{
							Kernel32.CloseHandle(lpProcessInformation.hProcess);
						}
						if (lpProcessInformation.hThread != IntPtr.Zero)
						{
							Kernel32.CloseHandle(lpProcessInformation.hThread);
						}
					}
				}
				finally
				{
					if (hDuplicatedSystemToken != IntPtr.Zero)
					{
						Kernel32.CloseHandle(hDuplicatedSystemToken);
					}
				}
			}
			finally
			{
				if (hOriginalSystemToken != IntPtr.Zero)
				{
					Kernel32.CloseHandle(hOriginalSystemToken);
				}
			}

		}

		private static IntPtr GetTokenFromProcess(string processName)
		{
			IntPtr hToken = IntPtr.Zero;

			var process = Process.GetProcessesByName(processName).FirstOrDefault();
			if (process == null)
			{
				throw new Exception($"Failed to find {processName} process.");
			}

			IntPtr hProcess = IntPtr.Zero;
			try
			{
				int pid = process.Id;

				bool inheritHandle = true;
				var processAccess = AdvApi32.ProcessAccessFlags.All;
				hProcess = Kernel32.OpenProcess((uint)processAccess, inheritHandle, (uint)pid);
				if (hProcess == IntPtr.Zero)
				{
					var code = Marshal.GetLastWin32Error();
					string message = $"Failed to open handle to process '{pid}' with the access flag '{processAccess.ToString()}'";
					throw new Win32Exception(code, message);
				}

				var desiredAccess = (AdvApi32.TokenAccess.TOKEN_DUPLICATE | AdvApi32.TokenAccess.TOKEN_QUERY);
				bool result = AdvApi32.OpenProcessToken(hProcess, desiredAccess, out hToken);
				if (!result)
				{
					var code = Marshal.GetLastWin32Error();
					var message = $"Failed to retrieve handle to processes access token. OpenProcessToken failed with error: {code}";

					throw new Win32Exception(code, message);
				}
			}
			finally
			{
				if (hProcess != IntPtr.Zero)
				{
					Kernel32.CloseHandle(hProcess);
				}
			}

			return hToken;
		}

		public static System.Security.Principal.WindowsIdentity GetProcIdentity(Process process)
		{
			System.Security.Principal.WindowsIdentity wi = null;

			IntPtr hToken = IntPtr.Zero;
			try
			{
				var hProcess = process.Handle;
				bool res = AdvApi32.OpenProcessToken(hProcess, AdvApi32.TokenAccess.TOKEN_ALL_ACCESS, out hToken);
				if (!res)
				{
					var code = Marshal.GetLastWin32Error();
					var message = $"Failed to retrieve handle to processes access token. OpenProcessToken failed with error: {code}";
					throw new Win32Exception(code, message);
				}
				wi = new System.Security.Principal.WindowsIdentity(hToken);
			}
			finally
			{
				if (hToken != IntPtr.Zero)
				{
					Kernel32.CloseHandle(hToken);
				}
			}

			return wi;
		}

		public static string GetUserInfo(IntPtr hToken)
		{// можно получить с помощью System.Security.Principal.WindowsIdentity
			string userInfo = string.Empty;
			uint tokenInfoLength = 0;

			bool success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokenInfoLength, out tokenInfoLength);
			if (tokenInfoLength > 0)
			{
				IntPtr tokenInfo = IntPtr.Zero;
				try
				{
					tokenInfo = Marshal.AllocHGlobal((int)tokenInfoLength);
					success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfoLength, out tokenInfoLength);
					if (success)
					{

						AdvApi32.TOKEN_USER tokenUser = (AdvApi32.TOKEN_USER)Marshal.PtrToStructure(tokenInfo, typeof(AdvApi32.TOKEN_USER));
						int sidAndAttrSize = Marshal.SizeOf(new AdvApi32.SID_AND_ATTRIBUTES());

						IntPtr ptr = IntPtr.Zero;
						try
						{
							var sid = tokenUser.User.Sid;
							AdvApi32.ConvertSidToStringSid(sid, out ptr);
							userInfo = Marshal.PtrToStringAuto(ptr);
						}
						finally
						{
							if (ptr != IntPtr.Zero)
							{
								Kernel32.LocalFree(ptr);
							}
						}
					}
				}
				finally
				{
					if (tokenInfo != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(tokenInfo);
					}
				}

			}

			return userInfo;
		}

		public static string GetGroupLogonId(IntPtr hToken)
		{
			string userInfo = string.Empty;
			uint tokenInfoLength = 0;

			bool success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenGroups, IntPtr.Zero, tokenInfoLength, out tokenInfoLength);
			if (tokenInfoLength > 0)
			{
				IntPtr tokenInfo = IntPtr.Zero;
				try
				{
					tokenInfo = Marshal.AllocHGlobal((int)tokenInfoLength);
					success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenGroups, tokenInfo, tokenInfoLength, out tokenInfoLength);
					if (success)
					{

						AdvApi32.TOKEN_GROUPS groups = (AdvApi32.TOKEN_GROUPS)Marshal.PtrToStructure(tokenInfo, typeof(AdvApi32.TOKEN_GROUPS));
						int sidAndAttrSize = Marshal.SizeOf(typeof(AdvApi32.SID_AND_ATTRIBUTES));

						var infoPtr = new IntPtr(tokenInfo.ToInt64() + IntPtr.Size);

						for (int i = 0; i < groups.GroupCount; i++)
						{
							//var infoPtr = new IntPtr(tokenInfo.ToInt64() + i * sidAndAttrSize + IntPtr.Size);

							AdvApi32.SID_AND_ATTRIBUTES sidAndAttributes = (AdvApi32.SID_AND_ATTRIBUTES)Marshal.PtrToStructure(infoPtr, typeof(AdvApi32.SID_AND_ATTRIBUTES));
							if ((sidAndAttributes.Attributes & AdvApi32.SE_GROUP_LOGON_ID) == AdvApi32.SE_GROUP_LOGON_ID)
							{
								IntPtr ptr = IntPtr.Zero;
								try
								{
									AdvApi32.ConvertSidToStringSid(sidAndAttributes.Sid, out ptr);
									userInfo = Marshal.PtrToStringAuto(ptr);
								}
								finally
								{
									if (ptr != IntPtr.Zero)
									{
										Kernel32.LocalFree(ptr);
									}
								}

								break;
							}

							infoPtr += sidAndAttrSize;

						}
					}
				}
				finally
				{
					if (tokenInfo != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(tokenInfo);
					}
				}

			}

			return userInfo;
		}

		class ATPrivilege
		{
			public string Name { get; set; }
			public uint Attributes { get; set; }

			public static List<ATPrivilege> GetPrivilegesFromToken(IntPtr hToken)
			{
				List<ATPrivilege> privs = new List<ATPrivilege>();

				uint tokenInfLength = 0;

				bool success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenPrivileges, IntPtr.Zero, tokenInfLength, out tokenInfLength);

				if (tokenInfLength > 0)
				{
					IntPtr tokenInfo = IntPtr.Zero;
					try
					{
						tokenInfo = Marshal.AllocHGlobal((int)tokenInfLength);
						success = AdvApi32.GetTokenInformation(hToken, SECUR32.TOKEN_INFORMATION_CLASS.TokenPrivileges, tokenInfo, tokenInfLength, out tokenInfLength);

						if (success)
						{
							AdvApi32.TOKEN_PRIVILEGES tokenPrivileges = (AdvApi32.TOKEN_PRIVILEGES)Marshal.PtrToStructure(tokenInfo, typeof(AdvApi32.TOKEN_PRIVILEGES));

							IntPtr privArrayPtr = new IntPtr(tokenInfo.ToInt64() + 4); //x86 ?? 

							var privilegeCount = tokenPrivileges.PrivilegeCount;

							MarshalHelper.PtrToArray(privArrayPtr, privilegeCount, out AdvApi32.LUID_AND_ATTRIBUTES[] attrs);

							foreach (var attr in attrs)
							{
								var privilegeName = "";

								IntPtr ptrLuid = IntPtr.Zero;
								try
								{
									ptrLuid = Marshal.AllocHGlobal(Marshal.SizeOf(attr.Luid));
									Marshal.StructureToPtr(attr.Luid, ptrLuid, true);

									int luidNameLen = 0;
									AdvApi32.LookupPrivilegeName(null, ptrLuid, null, ref luidNameLen);

									//Debug.Assert(luidNameLen > 0, "luidNameLen > 0");

									if (luidNameLen > 0)
									{
										var buf = new StringBuilder(luidNameLen);
										if (AdvApi32.LookupPrivilegeName(null, ptrLuid, buf, ref luidNameLen))
										{
											privilegeName = buf.ToString();
										}
									}
								}
								finally
								{
									if (ptrLuid != IntPtr.Zero)
									{
										Marshal.FreeHGlobal(ptrLuid);
									}
								}

								if (string.IsNullOrEmpty(privilegeName))
								{
									var code = Marshal.GetLastWin32Error();
									var message = $"Failed to retreive privelege name: {code}";

									throw new Win32Exception(code, message);
								}

								Console.WriteLine("PrivilegeName: " + privilegeName + " " + attr.Attributes);
								//tracer.TraceEvent(TraceEventType.Verbose, 0, "PrivilegeName: " + privilegeName);

								privs.Add(new ATPrivilege
								{
									Name = privilegeName,
									Attributes = attr.Attributes
								});
							}

						}
						else
						{
							var code = Marshal.GetLastWin32Error();
							var message = $"GetTokenInformation failed with error: {code}";

							throw new Win32Exception(code, message);
						}
					}
					finally
					{
						if (tokenInfo != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(tokenInfo);
						}
					}

				}
				else
				{
					var code = Marshal.GetLastWin32Error();
					var message = $"GetTokenInformation failed with error: {code}";

					throw new Win32Exception(code, message);
				}

				return privs;

			}
		}

		class ATGroup
		{

			public string SIDString { get; }
			public IntPtr SIDPtr { get; }
			public int Attributes { get; }
			public string Name { get; }
			public string Domain { get; }
			public AdvApi32.SidNameUse Type { get; }

			public ATGroup(string sidName, IntPtr sidPtr, int attributes, string name, string domain, AdvApi32.SidNameUse tpe)
			{
				this.SIDPtr = sidPtr;
				this.SIDString = sidName;
				this.Attributes = attributes;
				this.Name = name;
				this.Domain = domain;
				this.Type = tpe;
			}
		}

	}
}
