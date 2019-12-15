using MediaToolkit.NativeAPIs;
using MediaToolkit.SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MediaToolkit.Utils
{

    public class MediaToolkitBootstrapper : IMediaToolkitBootstrapper
    {
        public void Startup()
        {
            var winVersion = Environment.OSVersion.Version;
            bool isCompatibleOSVersion = (winVersion.Major >= 6 && winVersion.Minor >= 2);

            if (!isCompatibleOSVersion)
            {
                throw new Exception("Windows versions earlier than 8 are not supported.");
            }

            // TODO:
            // Validate directx, medaiafoundations... 

            SharpDX.MediaFoundation.MediaManager.Startup();

            //SharpDX.Configuration.EnableReleaseOnFinalizer = true;

            SharpDX.Configuration.EnableObjectTracking = true;
            SharpDX.Diagnostics.ObjectTracker.StackTraceProvider = null;

            //SharpDX.Configuration.EnableTrackingReleaseOnFinalizer = false;

            MediaToolkit.NativeAPIs.WinMM.timeBeginPeriod(1);
        }


        public void Shutdown()
        {

            MediaToolkit.NativeAPIs.WinMM.timeEndPeriod(1);

            SharpDX.MediaFoundation.MediaManager.Shutdown();
        }
    }

    public abstract class StatCounter
    {
        public abstract string GetReport();
        public abstract void Reset();
    }

    public class Statistic
    {

        public readonly static List<StatCounter> Stats = new List<StatCounter>();

        public static object syncRoot = new object();
        public static void RegisterCounter(StatCounter counter)
        {
            if (counter == null)
            {
                return;
            }

            lock (syncRoot)
            {
                Stats.Add(counter);
            }
        }

        public static void UnregisterCounter(StatCounter counter)
        {
            if(counter == null)
            {
                return;
            }

            lock (syncRoot)
            {
                Stats.Remove(counter);
            }
        }

        public static string GetReport()
        {
            string report = "";
            lock (syncRoot)
            {
                StringBuilder sb = new StringBuilder();
                foreach(var stat in Stats)
                {
                    sb.AppendLine(stat.GetReport());
                }

                report = sb.ToString();

            }

            return report;
        }

        private static PerfCounter perfCounter = new PerfCounter();
        public static PerfCounter PerfCounter
        {
            get
            {
                if (perfCounter == null)
                {
                    perfCounter = new PerfCounter();
                }
                return perfCounter;
            }
        }
    }




    public class PerfCounter : StatCounter, IDisposable
    {
        public PerfCounter()
        {
            _PerfCounter();
        }

        private void _PerfCounter()
        {
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Disposed += Timer_Disposed;
            timer.Start();

        }

        public short CPU { get; private set; }

        private System.Timers.Timer timer = new System.Timers.Timer();
        private CPUCounter cpuCounter = new CPUCounter();

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.CPU = cpuCounter.GetUsage();
        }

        private void Timer_Disposed(object sender, EventArgs e)
        {
            cpuCounter?.Dispose();
        }

        public override string GetReport()
        {
            string cpuUsage = "";
            if (CPU >= 0 && CPU <= 100)
            {
                cpuUsage = "CPU=" + CPU + "%";
            }
            else
            {
                cpuUsage = "CPU=--%";
            }
            return cpuUsage;
        }

        public override void Reset()
        {
            //throw new NotImplementedException();
        }

        public void Dispose()
        {
            timer?.Stop();
            timer?.Dispose();
            timer = null;
        }

        class CPUCounter : IDisposable
        {

            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysKernel;
            private System.Runtime.InteropServices.ComTypes.FILETIME prevSysUser;

            private TimeSpan prevProcTotal;

            private short CPUUsage;
            //DateTime LastRun;

            private long lastTimestamp;

            private long runCount;

            private Process currentProcess;

            public CPUCounter()
            {
                CPUUsage = -1;
                lastTimestamp = 0;

                prevSysUser.dwHighDateTime = prevSysUser.dwLowDateTime = 0;
                prevSysKernel.dwHighDateTime = prevSysKernel.dwLowDateTime = 0;
                prevProcTotal = TimeSpan.MinValue;
                runCount = 0;

                currentProcess = Process.GetCurrentProcess();
            }

            public short GetUsage()
            {
                if (disposed)
                {
                    return 0;
                }

                short CPUCopy = CPUUsage;
                if (Interlocked.Increment(ref runCount) == 1)
                {
                    if (!EnoughTimePassed)
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    System.Runtime.InteropServices.ComTypes.FILETIME sysIdle, sysKernel, sysUser;
                    if (!Kernel32.GetSystemTimes(out sysIdle, out sysKernel, out sysUser))
                    {
                        Interlocked.Decrement(ref runCount);
                        return CPUCopy;
                    }

                    TimeSpan procTime = currentProcess.TotalProcessorTime;

                    if (prevProcTotal != TimeSpan.MinValue)
                    {
                        ulong sysKernelDiff = SubtractTimes(sysKernel, prevSysKernel);
                        ulong sysUserDiff = SubtractTimes(sysUser, prevSysUser);
                        ulong sysTotal = sysKernelDiff + sysUserDiff;

                        long procTotal = procTime.Ticks - prevProcTotal.Ticks;
                        // long procTotal = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                        if (sysTotal > 0)
                        {
                            CPUUsage = (short)((100.0 * procTotal) / sysTotal);
                        }
                    }
   
                    prevProcTotal = procTime;
                    prevSysKernel = sysKernel;
                    prevSysUser = sysUser;

                    lastTimestamp = Stopwatch.GetTimestamp();

                    CPUCopy = CPUUsage;
                }
                Interlocked.Decrement(ref runCount);

                return CPUCopy;

            }

            private ulong SubtractTimes(System.Runtime.InteropServices.ComTypes.FILETIME a, System.Runtime.InteropServices.ComTypes.FILETIME b)
            {
                ulong aInt = ((ulong)(a.dwHighDateTime << 32)) | (ulong)a.dwLowDateTime;
                ulong bInt = ((ulong)(b.dwHighDateTime << 32)) | (ulong)b.dwLowDateTime;

                return aInt - bInt;
            }

            private bool EnoughTimePassed
            {
                get
                {
                    const int minimumElapsedMS = 250;

                    long ticks = (long)((Stopwatch.GetTimestamp() - lastTimestamp) * 10000000.0 / (double)Stopwatch.Frequency);
                    TimeSpan sinceLast = new TimeSpan(ticks);

                    return sinceLast.TotalMilliseconds > minimumElapsedMS;
                }
            }

            private volatile bool disposed = false;
            public void Dispose()
            {
                disposed = true;

                if (currentProcess != null)
                {
                    currentProcess.Dispose();
                    currentProcess = null;
                }
            }
        }

    }

    public class RngProvider
    {
        private static System.Security.Cryptography.RNGCryptoServiceProvider provider =
            new System.Security.Cryptography.RNGCryptoServiceProvider();

        public static uint GetRandomNumber()
        {
            byte[] bytes = new byte[sizeof(UInt32)];
            provider.GetNonZeroBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }


    public class MediaTimer
    {
        public const long TicksPerMillisecond = 10000;
        public const long TicksPerSecond = TicksPerMillisecond * 1000;

        public static double GetRelativeTimeMilliseconds()
        {
            return (Ticks / (double)TicksPerMillisecond);
        }

        public static double GetRelativeTime()
        {
            return (Ticks / (double)TicksPerSecond);
        }

        public static long Ticks
        {
            get
            {
                return (long)(Stopwatch.GetTimestamp() * TicksPerSecond / (double)Stopwatch.Frequency);
                //return DateTime.Now.Ticks;
                //return NativeMethods.timeGetTime() * TicksPerMillisecond;
            }
        }

        public static DateTime GetDateTimeFromNtpTimestamp(ulong ntmTimestamp)
        {
            uint TimestampMSW = (uint)(ntmTimestamp >> 32);
            uint TimestampLSW = (uint)(ntmTimestamp & 0x00000000ffffffff);

            return GetDateTimeFromNtpTimestamp(TimestampMSW, TimestampLSW);
        }

        public static DateTime GetDateTimeFromNtpTimestamp(uint TimestampMSW, uint TimestampLSW)
        {
            /*
            Timestamp, MSW: 3670566484 (0xdac86654)
            Timestamp, LSW: 3876982392 (0xe7160e78)
            [MSW and LSW as NTP timestamp: Apr 25, 2016 09:48:04.902680000 UTC]
             * */

            DateTime ntpDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            uint ntpTimeMilliseconds = (uint)(Math.Round((double)TimestampLSW / (double)uint.MaxValue, 3) * 1000);
            return ntpDateTime
                .AddSeconds(TimestampMSW)
                .AddMilliseconds(ntpTimeMilliseconds);
        }

        private DateTime startDateTime;
        private long startTimestamp;

        private bool isRunning = false;

        public void Start(DateTime dateTime)
        {
            if (isRunning == false)
            {
                startDateTime = dateTime;
                startTimestamp = Stopwatch.GetTimestamp();

                isRunning = true;
            }
        }

        public DateTime Now
        {
            get
            {
                DateTime dateTime = DateTime.MinValue;
                if (isRunning)
                {
                    dateTime = startDateTime.AddTicks(ElapsedTicks);
                }

                return dateTime;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                TimeSpan timeSpan = TimeSpan.Zero;
                if (isRunning)
                {
                    timeSpan = new TimeSpan(ElapsedTicks);
                }
                return timeSpan;
            }
        }

        public long ElapsedTicks
        {
            get
            {
                long ticks = 0;
                if (isRunning)
                {
                    ticks = (long)((Stopwatch.GetTimestamp() - startTimestamp) * TicksPerSecond / (double)Stopwatch.Frequency);

                    if (ticks < 0)
                    {
                        Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ticks " + ticks);
                    }
                }
                return ticks;
            }
        }

        public void Stop()
        {

            if (isRunning)
            {
                isRunning = false;
            }

        }
    }

    public class StringHelper
    {
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0)
            {
                throw new ArgumentOutOfRangeException("decimalPlaces");
            }

            if (value > long.MinValue && value < long.MaxValue)
            {
                if (value < 0)
                {
                    return "-" + SizeSuffix(-value);
                }

                if (value == 0)
                {
                    return string.Format("{0:n" + decimalPlaces + "} bytes", 0);
                }

                // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
                int mag = (int)Math.Log(value, 1024);

                // 1L << (mag * 10) == 2 ^ (10 * mag) 
                // [i.e. the number of bytes in the unit corresponding to mag]
                decimal adjustedSize = (decimal)value / (1L << (mag * 10));

                // make adjustment when the value is large enough that
                // it would round up to 1000 or more
                if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
                {
                    mag += 1;
                    adjustedSize /= 1024;
                }

                return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            }

            return "";
        }
    }

    public static class TaskEx
    {
        public static Task<Task[]> WhenAllOrFirstException(params Task[] tasks)
        {
            var countdownEvent = new CountdownEvent(tasks.Length);
            var completer = new TaskCompletionSource<Task[]>();

            Action<Task> onCompletion = completed =>
            {
                if (completed.IsFaulted && completed.Exception != null)
                {
                    completer.TrySetException(completed.Exception.InnerExceptions);
                }

                if (countdownEvent.Signal() && !completer.Task.IsCompleted)
                {
                    completer.TrySetResult(tasks);
                }
            };

            foreach (var task in tasks)
            {
                task.ContinueWith(onCompletion);
            }

            return completer.Task;
        }
    }

    public class NetworkHelper
    {



        public static List<IPAddressInformation> GetActiveUnicastIpAddressInfos()
        {

            List<IPAddressInformation> ipAddrInfos = new List<IPAddressInformation>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                if (network.OperationalStatus == OperationalStatus.Up &&
                    network.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties prop = network.GetIPProperties();
                    foreach (IPAddressInformation addr in prop.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }
                        ipAddrInfos.Add(addr);
                    }
                }
            }
            return ipAddrInfos;
        }
    }


    public class UsbDeviceManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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
            logger.Trace("GetPresentedUsbHardwareIds()");

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

                            if (lastError == (long)WinErrors.ERROR_NO_MORE_ITEMS)
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

                                    if (lastError == (long)WinErrors.ERROR_INSUFFICIENT_BUFFER)
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

    public class WcfDiscoveryAddressCustomEndpointBehavior : IEndpointBehavior, IDispatchMessageInspector
    {
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            // Attach ourselves to the MessageInspectors of reply messages
            clientRuntime.CallbackDispatchRuntime.MessageInspectors.Add(this);
        }

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            object messageProperty;
            if (!OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out messageProperty)) return null;
            var remoteEndpointProperty = messageProperty as RemoteEndpointMessageProperty;
            if (remoteEndpointProperty == null) return null;

            // Extract message body
            string messageBody;
            using (var oldMessageStream = new MemoryStream())
            {
                using (var xw = XmlWriter.Create(oldMessageStream))
                {
                    request.WriteMessage(xw);
                    xw.Flush();
                    messageBody = Encoding.UTF8.GetString(oldMessageStream.ToArray());
                }
            }

            // Replace instances of 0.0.0.0 with actual remote endpoint address
            var remoteAddr = remoteEndpointProperty.Address;
            var remotePort = remoteEndpointProperty.Port;
            var remoteEndpoint = remoteAddr + ":" + remotePort;
            // messageBody = messageBody.Replace("0.0.0.0:0", remoteEndpoint);

            messageBody = messageBody.Replace("0.0.0.0", remoteEndpointProperty.Address);

            // NOTE: Do not close or dispose of this MemoryStream. It will be used by WCF down the line.
            var newMessageStream = new MemoryStream(Encoding.UTF8.GetBytes(messageBody));
            XmlDictionaryReader xdr = XmlDictionaryReader.CreateTextReader(newMessageStream, new XmlDictionaryReaderQuotas());

            // Create a new message with our modified endpoint address and
            // copy over existing properties and headers
            System.ServiceModel.Channels.Message newMessage = System.ServiceModel.Channels.Message.CreateMessage(xdr, int.MaxValue, request.Version);
            newMessage.Properties.CopyProperties(request.Properties);
            newMessage.Headers.CopyHeadersFrom(request.Headers);
            request = newMessage;


            return null;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }
    }

}
