using MediaToolkit.Logging;
using MediaToolkit.NativeAPIs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaToolkit.Managers
{
    public class UsbDeviceManager
    {
        //private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Managers");

        public static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");


        private IntPtr notificationHandle = IntPtr.Zero;

        public bool RegisterNotification(IntPtr handle, Guid classGuid)
        {
            logger.Debug("RegisterNotification() " + handle + " " + classGuid);

            if (notificationHandle != IntPtr.Zero)
            {
                //TODO:
                logger.Warn("RegisterNotificationHandle = " + notificationHandle);
            }

            bool Success = false;
            try
            {
                DEV_BROADCAST_DEVICEINTERFACE broadcastInterface = new DEV_BROADCAST_DEVICEINTERFACE
                {
                    DeviceType = DBT.DEVTYP_DEVICEINTERFACE,
                    Reserved = 0,
                    ClassGuid = classGuid, //KSCATEGORY_VIDEO, // KSCATEGORY_CAPTURE,// GUID_CLASS_USB_DEVICE,
                };

                broadcastInterface.Size = Marshal.SizeOf(broadcastInterface);

                IntPtr notificationFilter = Marshal.AllocHGlobal(broadcastInterface.Size);
                Marshal.StructureToPtr(broadcastInterface, notificationFilter, true);

                notificationHandle = User32.RegisterDeviceNotification(handle, notificationFilter, 0);
                //Marshal.FreeHGlobal(notificationFilter);

                if (notificationHandle != IntPtr.Zero)
                {
                    Success = true;
                }
                else
                {
                    var lastError = Marshal.GetLastWin32Error();
                    logger.Error("RegisterDeviceNotification() ErrorCode: " + lastError);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return Success;
        }


        public void UnregisterNotification()
        {

            logger.Debug("UnregisterNotification()");

            try
            {
                if (!User32.UnregisterDeviceNotification(notificationHandle))
                {
                    var lastError = Marshal.GetLastWin32Error();
                    logger.Error("UnregisterDeviceNotification() " + lastError);
                }
                notificationHandle = IntPtr.Zero;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
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

        private string GetDeviceName(string dbcc_name)
        {
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
                    object result = key.GetValue("FriendlyName");
                    if (result != null)
                        return result.ToString();
                    result = key.GetValue("DeviceDesc");
                    if (result != null)
                        return result.ToString();
                }
            }
            return String.Empty;
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
