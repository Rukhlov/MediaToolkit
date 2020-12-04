using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Utils
{
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


}
