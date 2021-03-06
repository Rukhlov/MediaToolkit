﻿using MediaToolkit.Logging;
using MediaToolkit.NativeAPIs;
using MediaToolkit.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MediaToolkit.Managers
{
    public class UsbCategory
    {
	
        public static readonly Guid VideoCamera = KS.CATEGORY_VIDEO_CAMERA; //<--win10
		public static readonly Guid Video = KS.CATEGORY_VIDEO; // win7

		public static readonly Guid Audio = KS.CATEGORY_AUDIO;

        public static readonly Guid Capture = KS.CATEGORY_CAPTURE;
        public static readonly Guid AudioDevice = KS.CATEGORY_AUDIO_DEVICE;
    }

    public class UsbDeviceInfo
    {
        public string Name { get; set; }
        public Guid ClassGuid { get; set; }
        public string FriendlyName { get; set; }
    }

    //https://docs.microsoft.com/en-us/windows/win32/medfound/handling-video-device-loss
    public class UsbDeviceManager : IWndMessageProcessor
	{

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Managers");

		private NotifyWindow nativeWindow = null;

		public event Action<string> UsbDeviceArrival;
		public event Action<string> UsbDeviceMoveComplete;


		public bool Init(Guid classGuid)
		{
			logger.Debug("UsbDeviceManager::Init()");

			if (nativeWindow == null)
			{
				nativeWindow = new NotifyWindow(this);
				nativeWindow.CreateWindow();
			}

			var hWnd = nativeWindow.Handle;

            var handle = RegisterNotification(hWnd, classGuid);

			return (handle != IntPtr.Zero);
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

		public static IntPtr RegisterNotification(IntPtr handle, Guid classGuid)
		{
			logger.Debug("RegisterNotification() " + handle + " " + classGuid);

			var notificationHandle = IntPtr.Zero;

			try
			{
				DEV_BROADCAST_DEVICEINTERFACE broadcastInterface = new DEV_BROADCAST_DEVICEINTERFACE
				{
					DeviceType = DBT.DEVTYP_DEVICEINTERFACE,
					Reserved = 0,
					ClassGuid = classGuid,
				};

				broadcastInterface.Size = Marshal.SizeOf(broadcastInterface);

				IntPtr notificationFilter = Marshal.AllocHGlobal(broadcastInterface.Size);
				Marshal.StructureToPtr(broadcastInterface, notificationFilter, true);

				notificationHandle = User32.RegisterDeviceNotification(handle, notificationFilter, 0);
				//Marshal.FreeHGlobal(notificationFilter);

				if (notificationHandle == IntPtr.Zero)
				{
					var lastError = Marshal.GetLastWin32Error();
					logger.Error("RegisterDeviceNotification() ErrorCode: " + lastError);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return notificationHandle;
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
							if (TryPtrToDeviceName(m.LParam, out string deviceName))
							{   // получили информацию о подключенном устройстве в виде:
								// \\?\USB#VID_0A89&PID_000C#6&2c24ce2e&0&4#{a5dcbf10-6530-11d2-901f-00c04fb951ed}
								if (eventCode == DBT.DEVICEARRIVAL)
								{
									UsbDeviceArrival?.Invoke(deviceName);
								}
								else
								{
									UsbDeviceMoveComplete?.Invoke(deviceName);
								}
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

		public static bool UnregisterNotification(IntPtr handle)
		{

			logger.Debug("UnregisterNotification()");
			bool success = false;
			try
			{
				success = User32.UnregisterDeviceNotification(handle);
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

        public static bool TryPtrToDeviceInfo(IntPtr handle, out UsbDeviceInfo deviceInfo)
        {
            bool Result = false;
            deviceInfo = null;
            try
            {
                DEV_BROADCAST_HDR header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(handle, typeof(DEV_BROADCAST_HDR));

                if (header.DeviceType == DBT.DEVTYP_DEVICEINTERFACE)
                {
                    DEV_BROADCAST_DEVICEINTERFACE devInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(handle, typeof(DEV_BROADCAST_DEVICEINTERFACE));
					var broadcastName = devInterface.Name;
					var friendlyName = GetDeviceFriendlyName(broadcastName);

					deviceInfo = new UsbDeviceInfo
                    {
                        Name = broadcastName,
                        ClassGuid = devInterface.ClassGuid,
						FriendlyName = friendlyName,
                    };

                    Result = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return Result;
        }

        public static bool TryPtrToDeviceName(IntPtr lparam, out string deviceName)
		{
			bool Result = false;
			deviceName = "";
			try
			{
				DEV_BROADCAST_HDR header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lparam, typeof(DEV_BROADCAST_HDR));

				if (header.DeviceType == DBT.DEVTYP_DEVICEINTERFACE)
				{
					DEV_BROADCAST_DEVICEINTERFACE devInterface = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lparam, typeof(DEV_BROADCAST_DEVICEINTERFACE));
					deviceName = devInterface.Name;

					Result = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return Result;
		}


		public static bool TryPtrToDriveInfo(IntPtr lparam, out DriveInfo driveInfo)
		{
			bool Result = false;
			driveInfo = null;
			try
			{
				DEV_BROADCAST_HDR header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(lparam, typeof(DEV_BROADCAST_HDR));

				if (header.DeviceType == DBT.DEVTYP_VOLUME)
				{
					DEV_BROADCAST_VOLUME devInterface = (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lparam, typeof(DEV_BROADCAST_VOLUME));
					int mask = devInterface.UnitMask;

					int i;
					for (i = 0; i < 26; ++i)
					{
						if ((mask & 0x1) == 0x1)
						{
							break;
						}
						mask = mask >> 1;
					}

					string driveName = string.Concat((char)(i + 65), @":\");
					driveInfo = new DriveInfo(driveName);

					Result = true;
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);
			}

			return Result;
		}

		public static string GetDeviceFriendlyName(string dbcc_name)
		{
			var friendlyName = "";
			string[] Parts = dbcc_name.Split('#');
			if (Parts.Length >= 3)
			{
				string DevType = Parts[0].Substring(Parts[0].IndexOf(@"?\") + 2);
				string DeviceInstanceId = Parts[1];
				string DeviceUniqueID = Parts[2];
				string RegPath = @"SYSTEM\CurrentControlSet\Enum\" + DevType + "\\" + DeviceInstanceId + "\\" + DeviceUniqueID;
				Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(RegPath);
				if (key != null)
				{	
					//
					object result = key.GetValue("FriendlyName");
					if (result != null)
					{   // может быть в разных форматах:
						// @oem80.inf,%iPhone.DeviceDesc%;Apple Mobile Device USB Driver
						// Apple iPad
						friendlyName = result.ToString();
					}
					else
					{   // Lumia 720
						// @oem4.inf,%pid_08d7_dd%;Logitech QuickCam Communicate STX
						result = key.GetValue("DeviceDesc");
						if (result != null)
						{
							friendlyName = result.ToString();

						}
					}

					var _parts = friendlyName.Split(';');

					if (_parts.Length > 1)
					{
						friendlyName = _parts[1];
					}

				}
			}

			return friendlyName;
		}


		public static IEnumerable<string> GetPresentedUsbHardwareIds()
		{
			logger.Verb("GetPresentedUsbHardwareIds()");

			List<string> hardwareIds = new List<string>();

			IntPtr deviceInfoSet = IntPtr.Zero;
			long lastError = 0;
			const int INVALID_HANDLE_VALUE = -1;
			string devEnum = "USB";
			try
			{
				deviceInfoSet = SetupApi.SetupDiGetClassDevs(IntPtr.Zero, devEnum, IntPtr.Zero, (int)(DIGCF.DIGCF_PRESENT | DIGCF.DIGCF_ALLCLASSES));
				if ((deviceInfoSet != (IntPtr)INVALID_HANDLE_VALUE))
				{
					bool res = false;
					uint deviceIndex = 0;
					do
					{

						SP_DEVINFO_DATA devInfoData = new SP_DEVINFO_DATA();
						devInfoData.cbSize = (uint)Marshal.SizeOf(devInfoData);
						res = SetupApi.SetupDiEnumDeviceInfo(deviceInfoSet, deviceIndex, ref devInfoData);
						if (!res)
						{
							lastError = Marshal.GetLastWin32Error();

							if (lastError == (long)HResult.WIN32_ERROR_NO_MORE_ITEMS)
							{
								break;
							}

							logger.Error("SetupDiEnumDeviceInfo() " + lastError);
							break;
						}


						uint regType = 0;
						IntPtr propBuffer = IntPtr.Zero;
						uint bufSize = 1024;
						uint requiredSize = 0;

						try
						{

							propBuffer = Marshal.AllocHGlobal((int)bufSize);

							do
							{
								res = SetupApi.SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref devInfoData, (UInt32)SPDRP.SPDRP_HARDWAREID,
									ref regType, propBuffer, (uint)bufSize, ref requiredSize);

								if (!res)
								{
									lastError = Marshal.GetLastWin32Error();

									if (lastError == (long)HResult.WIN32_ERROR_INSUFFICIENT_BUFFER)
									{
										bufSize = requiredSize;

										if (propBuffer != IntPtr.Zero)
										{
											Marshal.FreeHGlobal(propBuffer);
										}

										propBuffer = Marshal.AllocHGlobal((int)bufSize);
										continue;
									}
									else
									{
										logger.Error("SetupDiGetDeviceRegistryProperty() " + lastError);
										break;
									}
								}

								string hardwareId = Marshal.PtrToStringAuto(propBuffer);
								logger.Debug(hardwareId);

								hardwareIds.Add(hardwareId);

							}
							while (false);
						}
						finally
						{
							if (propBuffer != IntPtr.Zero)
							{
								Marshal.FreeHGlobal(propBuffer);
							}
						}

						deviceIndex++;
					}
					while (true);

				}
				else
				{
					lastError = Marshal.GetLastWin32Error();
					logger.Error("SetupDiGetClassDevs() " + lastError);
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex);

			}
			finally
			{
				if (deviceInfoSet != IntPtr.Zero)
				{
					SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
				}
			}
			return hardwareIds;
		}


	}



}
