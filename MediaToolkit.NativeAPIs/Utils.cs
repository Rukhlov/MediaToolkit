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

	public class ShortcutUtil
	{

		public static void CreateShortcut(string shortcutPath, string targetPath,
			string arguments = null, string workingDirectory = null, string description = null)
		{
			CShellLink cShellLink = new CShellLink();
			try
			{
				IShellLink iShellLink = (IShellLink)cShellLink;

				iShellLink.SetPath(targetPath);
				if (!string.IsNullOrEmpty(arguments))
				{
					iShellLink.SetArguments(arguments);
				}

				if (!string.IsNullOrEmpty(description))
				{
					iShellLink.SetDescription(description);
				}

				if (!string.IsNullOrEmpty(workingDirectory))
				{
					iShellLink.SetWorkingDirectory(workingDirectory);
				}

				iShellLink.SetShowCmd(SW.SHOWNORMAL);

				IPersistFile iPersistFile = (IPersistFile)iShellLink;
				iPersistFile.Save(shortcutPath, false);

			}
			finally
			{
				ComBase.SafeRelease(cShellLink);
			}
		}


		public static void DeleteShortcut(string shortcutFile, string targetFile = null)
		{
			if (!string.IsNullOrEmpty(targetFile))
			{
				CShellLink cShellLink = new CShellLink();
				try
				{
					IShellLink iShellLink = (IShellLink)cShellLink;
					IPersistFile iPersistFile = (IPersistFile)iShellLink;
					iPersistFile.Load(shortcutFile, 0);

					StringBuilder sb = new StringBuilder(260);
					WIN32_FIND_DATA data = new WIN32_FIND_DATA();
					iShellLink.GetPath(sb, 260, ref data, 0);

					var shortcutFileName = System.IO.Path.GetFullPath(sb.ToString());
					if (!string.Equals(shortcutFileName, targetFile, StringComparison.OrdinalIgnoreCase))
					{
						return;
					}
				}
				finally
				{
					ComBase.SafeRelease(cShellLink);
				}
			}

			if (System.IO.File.Exists(shortcutFile))
			{
				System.IO.File.Delete(shortcutFile);
			}

		}

	}

	public class WindowHook
	{
		private IntPtr hWnd;
		private IntPtr hWinEventHook;
		private Process process;

		private User32.WinEventDelegate WinEventDelegate;
		private GCHandle handle;


		public event Action<bool> VisibleChanged;
		public event Action<Rectangle> LocationChanged;
		public event Action WindowClosed;
		public event Action ProcessExited;


		public void Setup(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
			{
				return;
			}

			this.hWnd = hwnd;
			this.WinEventDelegate = new User32.WinEventDelegate(WinEventCallback);
			this.handle = GCHandle.Alloc(WinEventDelegate);

			var threadId = User32.GetWindowThreadProcessId(hwnd, out var processId);

			this.process = Process.GetProcessById((int)processId);
			this.process.EnableRaisingEvents = true;
			this.process.Exited += TargetProc_Exited;

			this.hWinEventHook = WinEventHookRange(SWEH_Events.EVENT_OBJECT_CREATE, SWEH_Events.EVENT_OBJECT_CONTENTSCROLLED, WinEventDelegate, processId, threadId);

		}


		private void WinEventCallback(IntPtr hWinEventHook, SWEH_Events eventType, IntPtr hWnd, SWEH_ObjectId idObject, long idChild, uint dwEventThread, uint dwmsEventTime)
		{
			//Debug.WriteLine(eventType + " " + hWnd);

			if (hWnd == this.hWnd)
			{
				if (idObject == (SWEH_ObjectId)SWEH_CHILDID_SELF)
				{
					Debug.WriteLine(eventType + " " + hWnd);

					if (eventType == SWEH_Events.EVENT_OBJECT_LOCATIONCHANGE)
					{
						var rectangle = GetWindowRect(hWnd);
						if (!rectangle.IsEmpty)
						{
							LocationChanged?.Invoke(rectangle);
						}

						//                  bool result = User32.GetWindowRect(hWnd, out var rect);
						//if (result)
						//{
						//	var rectangle = rect.AsRectangle;

						//	LocationChanged?.Invoke(rectangle);
						//}

					}
					else if (eventType == SWEH_Events.EVENT_OBJECT_SHOW)
					{
						VisibleChanged?.Invoke(true);
					}
					else if (eventType == SWEH_Events.EVENT_OBJECT_HIDE)
					{
						VisibleChanged?.Invoke(false);
					}
					else if (eventType == SWEH_Events.EVENT_OBJECT_DESTROY)
					{
						WindowClosed?.Invoke();
					}
				}

			}
		}

		private void TargetProc_Exited(object sender, EventArgs e)
		{
			ProcessExited?.Invoke();
		}

		public bool GetWindowVisibility()
		{
			bool isVisible = false;
			if (hWnd != IntPtr.Zero)
			{
				isVisible = User32.IsWindowVisible(hWnd);
			}

			return isVisible;
		}


		public Rectangle GetCurrentWindowRect()
		{
			return GetWindowRect(hWnd);

		}

		public bool Close()
		{
			bool result = false;
			if (hWnd != IntPtr.Zero)
			{
				result = User32.UnhookWinEvent(hWinEventHook);
			}

			if (handle.IsAllocated)
			{
				handle.Free();
			}

			hWnd = IntPtr.Zero;
			return result;
		}

		public static Rectangle GetWindowRect(IntPtr hwnd)
		{
			Rectangle rectangle = Rectangle.Empty;

			if (hwnd != IntPtr.Zero)
			{
				rectangle = MediaToolkit.NativeAPIs.DwmApi.GetExtendedFrameBounds(hwnd);
				if (rectangle.IsEmpty)
				{
					bool result = User32.GetWindowRect(hwnd, out var rect);
					if (result)
					{
						rectangle = rect.AsRectangle;
					}
				}
			}

			return rectangle;
		}

		public static long SWEH_CHILDID_SELF = 0;
		public static IntPtr WinEventHookRange(SWEH_Events eventFrom, SWEH_Events eventTo, User32.WinEventDelegate _delegate, uint idProcess, uint idThread)
		{
			new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.AllWindows).Demand();

			SWEH_dwFlags flags = SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
								 SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
								 SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

			return User32.SetWinEventHook(eventFrom, eventTo, IntPtr.Zero, _delegate, idProcess, idThread, flags);
		}

		public static IntPtr WinEventHookOne(SWEH_Events _event, User32.WinEventDelegate _delegate, uint idProcess, uint idThread)
		{
			new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.AllWindows).Demand();

			SWEH_dwFlags flags = SWEH_dwFlags.WINEVENT_OUTOFCONTEXT |
					 SWEH_dwFlags.WINEVENT_SKIPOWNPROCESS |
					 SWEH_dwFlags.WINEVENT_SKIPOWNTHREAD;

			return User32.SetWinEventHook(_event, _event, IntPtr.Zero, _delegate, idProcess, idThread, flags);
		}


	}


	public class ProcessTool
	{
		private static TraceSource tracer = new TraceSource("MediaToolkit.NativeAPIs");

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

	public class WindowDescription
	{
		public int processId = -1;
		public string processName = "";

		public IntPtr hWnd = IntPtr.Zero;
		public string windowTitle = "";
		public string windowClass = "";
		public System.Drawing.Rectangle clientRect = System.Drawing.Rectangle.Empty;

		public override string ToString()
		{
			return string.Join(" | ", windowTitle, windowClass, hWnd, processId, processName);
		}
	}

	public class DesktopManager
	{

		public static readonly string[] Win10ServiceProceses =
		{
			"startmenuexperiencehost.exe",
           // "applicationframehost.exe",
            "peopleexperiencehost.exe",
			"shellexperiencehost.exe",
			"microsoft.notes.exe",
			"systemsettings.exe",
			"textinputhost.exe",
			"searchapp.exe",
			"video.ui.exe",
			"searchui.exe",
			"lockapp.exe",
			"cortana.exe",
			"gamebar.exe",
			"tabtip.exe",
			"time.exe",

		};

		public static List<WindowDescription> GetWindows()
		{

			List<WindowDescription> windows = new List<WindowDescription>();

			User32.EnumDelegate filterCb = new User32.EnumDelegate((hWnd, pParam) =>
			{
				string windowTitle = "";
				int windowTextLength = User32.GetWindowTextLength(hWnd);
				if (windowTextLength > 0)
				{
					windowTextLength++;
					StringBuilder buf = new StringBuilder(windowTextLength);
					windowTextLength = User32.GetWindowText(hWnd, buf, buf.Capacity);
					windowTitle = buf.ToString();
				}

				if (string.IsNullOrEmpty(windowTitle))
				{//continue enumerating windows...
					return true;
				}

				string className = "";
				{
					StringBuilder buf = new StringBuilder(256);
					var len = User32.GetClassName(hWnd, buf, buf.Capacity);
					if (len > 0)
					{
						className = buf.ToString();
					}

				}

				//if(className == "ApplicationFrameWindow")
				//{ // UWP not supported...
				//    return true;
				//}

				if (!ValidateWindow(hWnd))
				{
					return true;
				}


				var clientRect = User32.GetClientRect(hWnd);

				if (clientRect.Width == 0 || clientRect.Height == 0)
				{
					return true;
				}


				var cloaked = DwmApi.GetCloaked(hWnd);

				var frameBounds = DwmApi.GetExtendedFrameBounds(hWnd);

				var windowRect = User32.GetWindowRect(hWnd);


				Console.WriteLine(windowRect + " " + frameBounds);

				//clientRect = frameBounds.AsRectangle;



				var threadId = User32.GetWindowThreadProcessId(new HandleRef(null, hWnd), out var procId);
				if (threadId > 0 && procId > 0)
				{
					try
					{
						using (Process p = Process.GetProcessById(procId))
						{
							var proccessExe = p.ProcessName.ToLower() + ".exe";

							if (proccessExe == "applicationframewindow.exe")
							{
								//TODO: uwp process...

							}

							if (!Win10ServiceProceses.Contains(proccessExe))
							{
								var windowDescription = new WindowDescription
								{
									processId = p.Id,
									processName = p.ProcessName,
									hWnd = hWnd,
									windowTitle = windowTitle,
									windowClass = className,
									clientRect = clientRect,
								};

								windows.Add(windowDescription);
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}

				}



				// Return true to indicate that we
				// should continue enumerating windows.
				return true;

			});

			var success = User32.EnumDesktopWindows(IntPtr.Zero, filterCb, IntPtr.Zero);
			if (!success)
			{
				var error = Marshal.GetLastWin32Error();
				throw new Win32Exception(error);
			}

			return windows;

		}

		private static bool ValidateWindow(IntPtr hWnd)
		{
			var isVisible = User32.IsWindowVisible(hWnd);

			if (!isVisible)
			{
				return false;
			}

			var isIconic = User32.IsIconic(hWnd);

			if (isIconic)
			{
				return false;
			}

			//var result = User32.GetClientRect(hWnd, out var rect);
			//if (!result)
			//{
			//	//var error = Marshal.GetLastWin32Error();
			//	//throw new Win32Exception(error);
			//	return false;
			//}

			//if (rect.Bottom == 0 || rect.Right == 0)
			//{
			//	return false;
			//}

			var styles = (long)User32.GetWindowLong(hWnd, GWL.STYLE);
			if ((styles & WS.Child) != 0)
			{
				return false;
			}

			var ex_styles = (long)User32.GetWindowLong(hWnd, GWL.EXSTYLE);
			if ((ex_styles & WS_EX.ToolWindow) != 0)
			{
				return false;
			}

			return true;
		}

		private static bool IsUwpWindow(IntPtr hWnd)
		{
			string className = "";

			StringBuilder buf = new StringBuilder(256);
			var len = User32.GetClassName(hWnd, buf, buf.Capacity);
			if (len > 0)
			{
				className = buf.ToString();
			}

			return className == "ApplicationFrameWindow";
		}


		public enum SessionType
		{
			Console,
			RDP
		}

		public class WindowsSession
		{
			public uint ID { get; internal set; }
			public string Name { get; internal set; }
			public SessionType Type { get; internal set; }
			public string Username { get; internal set; }
		}


		public static List<WindowsSession> GetActiveSessions()
		{
			var sessions = new List<WindowsSession>();
			var consoleSessionId = Kernel32.WTSGetActiveConsoleSessionId();

			sessions.Add(new WindowsSession()
			{
				ID = consoleSessionId,
				Type = SessionType.Console,
				Name = "Console",
				Username = GetUsernameFromSessionId(consoleSessionId)
			});

			IntPtr ppSessionInfo = IntPtr.Zero;
			var count = 0;
			var enumSessionResult = WtsApi32.WTSEnumerateSessions(WtsApi32.WTS_CURRENT_SERVER_HANDLE, 0, 1, ref ppSessionInfo, ref count);
			var dataSize = Marshal.SizeOf(typeof(WtsApi32.WTS_SESSION_INFO));
			var current = ppSessionInfo;

			if (enumSessionResult != 0)
			{
				for (int i = 0; i < count; i++)
				{
					WtsApi32.WTS_SESSION_INFO sessionInfo = (WtsApi32.WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)current, typeof(WtsApi32.WTS_SESSION_INFO));
					current += dataSize;
					if (sessionInfo.State == WtsApi32.WTS_CONNECTSTATE_CLASS.WTSActive && sessionInfo.SessionID != consoleSessionId)
					{

						sessions.Add(new WindowsSession()
						{
							ID = sessionInfo.SessionID,
							Name = sessionInfo.pWinStationName,
							Type = SessionType.RDP,
							Username = GetUsernameFromSessionId(sessionInfo.SessionID)
						});
					}
				}
			}

			return sessions;
		}

		public static bool GetCurrentDesktop(out string desktopName)
		{
			bool result = false;
			desktopName = "";

			IntPtr hDesktop = IntPtr.Zero;
			try
			{
				hDesktop = OpenInputDesktop();

				IntPtr pvInfo = IntPtr.Zero;
				try
				{
					uint nLenght = 256;
					pvInfo = Marshal.AllocHGlobal((int)nLenght);

					result = User32.GetUserObjectInformationW(hDesktop, AdvApi32.UOI_NAME, pvInfo, nLenght, out var lenNeeded);
					if (result)
					{
						Debug.Assert(lenNeeded > 0, "lenNeeded > 0");
						desktopName = Marshal.PtrToStringAuto(pvInfo);
					}

					//User32.GetUserObjectInformationW(hDesktop, AdvApi32.UOI_NAME, IntPtr.Zero, 0, out var lenNeeded);
					//if(lenNeeded > 0)
					//{
					//	uint nLenght = lenNeeded;
					//	pvInfo = Marshal.AllocHGlobal((int)nLenght);

					//	result = User32.GetUserObjectInformationW(hDesktop, AdvApi32.UOI_NAME, pvInfo, nLenght, out lenNeeded);
					//	if (result)
					//	{
					//		desktopName = Marshal.PtrToStringUni(pvInfo);//, (int)lenNeeded);
					//	}
					//}

				}
				finally
				{
					if (pvInfo != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(pvInfo);
					}
				}
			}
			finally
			{
				if (hDesktop != IntPtr.Zero)
				{
					User32.CloseDesktop(hDesktop);
				}
			}

			return result;
		}

		public static string GetUsernameFromSessionId(uint sessionId)
		{
			var username = string.Empty;
			var result = WtsApi32.WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsApi32.WTS_INFO_CLASS.WTSUserName, out var buffer, out var strLen);

			if (result)
			{
				if (strLen > 0)
				{
					try
					{
						username = Marshal.PtrToStringAnsi(buffer);
					}
					finally
					{
						WtsApi32.WTSFreeMemory(buffer);
					}
				}
				else
				{
					//...
				}
			}
			else
			{
				var code = Marshal.GetLastWin32Error();
				//...
			}

			return username;
		}

		public static IntPtr OpenInputDesktop()
		{
			return User32.OpenInputDesktop(0, true, ACCESS_MASK.GENERIC_ALL);
		}

		public static bool SwitchToInputDesktop()
		{
			bool result = false;
			var hDesktop = IntPtr.Zero;
			try
			{
				hDesktop = OpenInputDesktop();
				if (hDesktop != IntPtr.Zero)
				{
					result = User32.SetThreadDesktop(hDesktop);
					if (result)
					{
						result = User32.SwitchDesktop(hDesktop);
						if (!result)
						{
							var code = Marshal.GetLastWin32Error();
							Console.WriteLine("SwitchDesktop() code: " + code);
						}
					}
					else
					{
						var code = Marshal.GetLastWin32Error();
						Console.WriteLine("SetThreadDesktop() code: " + code);
					}
				}
				else
				{
					var code = Marshal.GetLastWin32Error();
					Console.WriteLine("OpenInputDesktop() code: " + code);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			finally
			{
				if (hDesktop != IntPtr.Zero)
				{
					User32.CloseDesktop(hDesktop);
				}
			}

			return result;

		}


		public static bool OpenInteractiveProcess(string applicationName, string desktopName, bool hiddenWindow, out AdvApi32.PROCESS_INFORMATION procInfo)
		{ //работает если только если запускать из сервиса !!!

			uint winlogonPid = 0;

			IntPtr hUserTokenDup = IntPtr.Zero;
			IntPtr hPToken = IntPtr.Zero;
			IntPtr hProcess = IntPtr.Zero;

			procInfo = new AdvApi32.PROCESS_INFORMATION();

			// Check for RDP session.  If active, use that session ID instead.
			var activeSessions = GetActiveSessions();
			var dwSessionId = activeSessions.Last().ID;

			// Obtain the process ID of the winlogon process that is running within the currently active session.
			Process[] processes = Process.GetProcessesByName("winlogon");
			foreach (Process p in processes)
			{
				if ((uint)p.SessionId == dwSessionId)
				{
					winlogonPid = (uint)p.Id;
				}
			}



			// Obtain a handle to the winlogon process.
			hProcess = Kernel32.OpenProcess(AdvApi32.MAXIMUM_ALLOWED, false, winlogonPid);

			// Obtain a handle to the access token of the winlogon process.
			if (!AdvApi32.OpenProcessToken(hProcess, AdvApi32.TokenAccess.TOKEN_DUPLICATE, out hPToken))
			{
				Kernel32.CloseHandle(hProcess);
				return false;
			}

			// Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser.
			AdvApi32.SECURITY_ATTRIBUTES sa = new AdvApi32.SECURITY_ATTRIBUTES();
			sa.Length = Marshal.SizeOf(sa);

			// Copy the access token of the winlogon process; the newly created token will be a primary token.
			if (!AdvApi32.DuplicateTokenEx(hPToken, /*AdvApi32.MAXIMUM_ALLOWED*/ AdvApi32.TokenAccess.TOKEN_ALL_ACCESS,
				ref sa,
				AdvApi32.SecurityImpersonationLevel.SecurityIdentification,
				AdvApi32.TokenType.TokenPrimary,
				out hUserTokenDup))
			{
				Kernel32.CloseHandle(hProcess);
				Kernel32.CloseHandle(hPToken);
				return false;
			}

			// By default, CreateProcessAsUser creates a process on a non-interactive window station, meaning
			// the window station has a desktop that is invisible and the process is incapable of receiving
			// user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
			// interaction with the new process.
			AdvApi32.STARTUPINFO si = new AdvApi32.STARTUPINFO();
			si.cb = Marshal.SizeOf(si);
			si.lpDesktop = @"winsta0\" + desktopName;

			// Flags that specify the priority and creation method of the process.
			uint dwCreationFlags;
			if (hiddenWindow)
			{
				dwCreationFlags = AdvApi32.NORMAL_PRIORITY_CLASS | AdvApi32.CREATE_UNICODE_ENVIRONMENT | AdvApi32.CREATE_NO_WINDOW;
				si.dwFlags = AdvApi32.STARTF_USESHOWWINDOW;
				si.wShowWindow = 0;
			}
			else
			{
				dwCreationFlags = AdvApi32.NORMAL_PRIORITY_CLASS | AdvApi32.CREATE_UNICODE_ENVIRONMENT | AdvApi32.CREATE_NEW_CONSOLE;
			}

			// Create a new process in the current user's logon session.
			//bool result = AdvApi32.CreateProcessAsUser(hUserTokenDup, null, applicationName, ref sa, ref sa, false, dwCreationFlags, IntPtr.Zero, null, ref si, out procInfo);
			bool result = AdvApi32.CreateProcessAsUser(hUserTokenDup, applicationName, "", ref sa, ref sa, false, dwCreationFlags, IntPtr.Zero, null, ref si, out procInfo);


			//bool result = AdvApi32.CreateProcessAsUser(IntPtr.Zero, applicationName, "", ref sa, ref sa, false, dwCreationFlags, IntPtr.Zero, null, ref si, out procInfo);


			// Invalidate the handles.
			Kernel32.CloseHandle(hProcess);
			Kernel32.CloseHandle(hPToken);
			Kernel32.CloseHandle(hUserTokenDup);

			return result;
		}


	}

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
				size = (uint)Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME)),
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
