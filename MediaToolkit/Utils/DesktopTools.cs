using MediaToolkit.NativeAPIs;
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


}
