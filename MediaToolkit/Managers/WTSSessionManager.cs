using MediaToolkit.Logging;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.Managers
{

	public class WTSSessionManager : IWndMessageProcessor
	{
		private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Managers");

		private NotifyWindow nativeWindow = null;


		public event Action SessionLock;
		public event Action SessionUnlock;


		public bool Init()
		{
			logger.Debug("UsbDeviceManager::Init()");

			if (nativeWindow == null)
			{
				nativeWindow = new NotifyWindow(this);
				nativeWindow.CreateWindow();
			}

			var hWnd = nativeWindow.Handle;

			return RegisterNotification(hWnd);
		}

		public void Close()
		{
			logger.Debug("UsbDeviceManager::Close()");

			if (nativeWindow != null)
			{
				var hWnd = nativeWindow.Handle;
				var result = UnregisterNotification(hWnd);

				nativeWindow.DestroyWindow();

				nativeWindow = null;
			}
		}

		public static bool RegisterNotification(IntPtr hWnd)
		{
			logger.Debug("RegisterNotification() " + hWnd);

			var result = false;

			try
			{
				result = WtsApi32.WTSRegisterSessionNotification(hWnd, WtsApi32.NOTIFY_FOR_THIS_SESSION);
				if (!result)
				{
					var code = Marshal.GetLastWin32Error();
					logger.Error("WTSRegisterSessionNotification() " + code);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return result;
		}

		bool IWndMessageProcessor.ProcessMessage(Message m)
		{
			bool result = true;

			if (m.Msg == WM.WTSSESSION_CHANGE)
			{
				if (m.WParam.ToInt32() == WtsApi32.WTS.SESSION_LOCK)
				{
					SessionLock?.Invoke();
				}
				else if (m.WParam.ToInt32() == WtsApi32.WTS.SESSION_UNLOCK)
				{
					SessionUnlock?.Invoke();
				}
			}

			return result;
		}

		public static bool UnregisterNotification(IntPtr hWnd)
		{

			logger.Debug("UnregisterNotification()");
			bool success = false;
			try
			{
				success = WtsApi32.WTSUnRegisterSessionNotification(hWnd);
				if (!success)
				{
					var lastError = Marshal.GetLastWin32Error();
					logger.Error("UnregisterDeviceNotification() " + lastError);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return success;

		}

	}


}
