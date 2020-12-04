using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.Utils
{
	public interface IWndMessageProcessor
	{
		bool ProcessMessage(System.Windows.Forms.Message m);
	}

	/// <summary>
	/// Message only window
	/// </summary>
	public class NotifyWindow : NativeWindow
	{

		private readonly IWndMessageProcessor processor = null;
		public NotifyWindow(IWndMessageProcessor p)
		{
			this.processor = p;
		}

		public bool CreateWindow()
		{
			if (Handle == IntPtr.Zero)
			{
				CreateHandle(new CreateParams
				{//create message-only window
					Style = 0,
					ExStyle = 0,
					ClassStyle = 0,
					//Caption = "NotifyWindow",

					Parent = Defines.HWndMessage,
				});
			}
			return Handle != IntPtr.Zero;
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{

			base.WndProc(ref m);

			processor?.ProcessMessage(m);

		}

		public void DestroyWindow()
		{
			DestroyWindow(true, IntPtr.Zero);
		}

		private bool GetInvokeRequired(IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero) return false;
			int pid;
			var hwndThread = User32.GetWindowThreadProcessId(new HandleRef(this, hWnd), out pid);
			var currentThread = Kernel32.GetCurrentThreadId();

			return (hwndThread != currentThread);
		}

		private void DestroyWindow(bool destroyHwnd, IntPtr hWnd)
		{
			if (hWnd == IntPtr.Zero)
			{
				hWnd = Handle;
			}

			if (GetInvokeRequired(hWnd))
			{
				User32.PostMessage(hWnd, WM.CLOSE, 0, 0);
				return;
			}

			lock (this)
			{
				if (destroyHwnd)
				{
					base.DestroyHandle();
				}
			}
		}

		public override void DestroyHandle()
		{
			DestroyWindow(false, IntPtr.Zero);
			base.DestroyHandle();
		}


	}

}
