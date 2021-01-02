using MediaToolkit.Managers;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenStreamer.Wpf
{
	public enum UsbDeviceType
	{
		Unknown,
		Audio,
		Video,
	}

	public enum UsbDeviceChange
	{
		Arrival,
		Moved,
		//...
	}

	public class UsbChangeMessage
	{
		public UsbChangeMessage(UsbDeviceType type, UsbDeviceChange change, string name, string descr = "")
		{
			this.DeviceType = type;
			this.DeviceChange = change;
			this.DeviceName = name;
			this.DeviceDescription = descr;
		}

		public readonly UsbDeviceType DeviceType;
		public readonly UsbDeviceChange DeviceChange;
		public readonly string DeviceName;
		public readonly string DeviceDescription;

		public override string ToString()
		{
			return string.Join(" ", DeviceType, DeviceChange, DeviceName, DeviceDescription);
		}
	}

	public class UsbManager : IWndMessageProcessor
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private Dictionary<Guid, IntPtr> notifyHandles = null;

		private NotifyWindow notifyWindow = null;
		public event Action<UsbChangeMessage> DeviceChanged;

		public bool Initialized { get; private set; } = false;

		public bool Initialize()
		{
			logger.Debug("SystemManager::Initialize() " + Initialized);
			if (!Initialized)
			{
				try
				{
					notifyWindow = new NotifyWindow(this);

					notifyWindow.CreateWindow();

					var hWnd = notifyWindow.Handle;
					Debug.Assert(hWnd != IntPtr.Zero, "hWnd != IntPtr.Zero");

					RegisterNotification(hWnd, UsbCategory.Video);

					RegisterNotification(hWnd, UsbCategory.VideoCamera);

					RegisterNotification(hWnd, UsbCategory.Audio);

					Initialized = true;

				}
				catch (Exception ex)
				{
					logger.Error(ex.Message);
					Shutdown();
				}
			}

			return Initialized;
		}

		private void RegisterNotification(IntPtr hWnd, Guid classId)
		{
			if (notifyHandles == null)
			{
				notifyHandles = new Dictionary<Guid, IntPtr>();
			}

			var notifyHandle = UsbDeviceManager.RegisterNotification(hWnd, classId);
			if (notifyHandle != IntPtr.Zero)
			{
				notifyHandles.Add(classId, notifyHandle);
			}
			else
			{
				logger.Warn($"Fail to register {classId} notifications");
			}
		}

		bool IWndMessageProcessor.ProcessMessage(Message m)
		{
			bool result = false;

			switch ((uint)m.Msg)
			{
				case WM.DEVICECHANGE:
					{
						uint eventCode = (uint)m.WParam;

						if (eventCode == DBT.DEVICEARRIVAL || eventCode == DBT.DEVICEREMOVECOMPLETE)
						{
							if (UsbDeviceManager.TryPtrToDeviceInfo(m.LParam, out UsbDeviceInfo di))
							{   // получили информацию о подключенном устройстве в виде:
								// \\?\USB#VID_0A89&PID_000C#6&2c24ce2e&0&4#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
								
								var deviceType = UsbDeviceType.Unknown;
								if (di.ClassGuid == UsbCategory.Audio || di.ClassGuid == UsbCategory.AudioDevice)
								{ 
									deviceType = UsbDeviceType.Audio;
								}
								else if (di.ClassGuid == UsbCategory.VideoCamera || di.ClassGuid == UsbCategory.Video)
								{// update video sources...
									deviceType = UsbDeviceType.Video;
								}
								
								bool deviceArraval = (eventCode == DBT.DEVICEARRIVAL);
								var deviceChange = deviceArraval ? UsbDeviceChange.Arrival : UsbDeviceChange.Moved;
								var deviceName = di.Name;
								var deviceDescr = di.FriendlyName;

								DeviceChanged?.Invoke(new UsbChangeMessage(deviceType, deviceChange, deviceName, deviceDescr));

								result = true;
							}
							else
							{//TODO:
							 //...

							}
						}

						//logger.Debug("WM_DEVICECHANGE");
						break;
					}

			}

			return result;
		}

		public void Shutdown()
		{
			logger.Debug("SystemManager::Shutdown() " + Initialized);

			//if (!Initialized)
			//{
			//	return;
			//}

			if (notifyHandles != null && notifyHandles.Count > 0)
			{
				foreach (var handle in notifyHandles.Values)
				{
					var result = UsbDeviceManager.UnregisterNotification(handle);
				}
			}

			if (notifyWindow != null)
			{
				notifyWindow.DestroyWindow();

				notifyWindow = null;
			}

			Initialized = false;
		}
	}
}
