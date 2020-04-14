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
using System.Windows.Forms;
using System.Xml;

namespace MediaToolkit.Utils
{

	public interface IWndMessageProcessor
	{
		bool ProcessMessage(System.Windows.Forms.Message m);
	}

	public class NotifyWindow : NativeWindow
	{

		private readonly IWndMessageProcessor processor = null;
		internal NotifyWindow(IWndMessageProcessor p)
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

					Parent = Defines.HWndMessage,
				});
			}
			return Handle != IntPtr.Zero;
		}

		protected override void WndProc(ref System.Windows.Forms.Message m)
		{

			base.WndProc(ref m);

			processor.ProcessMessage(m);

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
                        //...
                        //Debug.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! ticks " + ticks);
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

    public class DisplayHelper
    {
        public static string GetFriendlyScreenName(System.Windows.Forms.Screen targetScreen)
        {
            string deviceFriendlyName = "";
            try
            {
                var displayInfos = NativeAPIs.Utils.DisplayTool.GetDisplayInfos();
                if (displayInfos.Count > 0)
                {
                    var displayInfo = displayInfos.FirstOrDefault(i => i.GdiDeviceName == targetScreen.DeviceName);

                    if (displayInfo != null)
                    {
                        deviceFriendlyName = displayInfo.Name;
                    }
                }
                
            }
            catch(Exception ex)
            {
                Trace.TraceError(ex.Message);
            }


            return deviceFriendlyName;
        }
    }

    public class NetTools
    {

        public static IEnumerable<int> GetFreePortRange(ProtocolType protocolType, int portsCount,
        int leftBound = 49152, int rightBound = 65535, IEnumerable<int> exceptPorts = null)
        {
            var totalRange = Enumerable.Range(leftBound, rightBound - leftBound + 1);

            IPGlobalProperties ipProps = IPGlobalProperties.GetIPGlobalProperties();

            IEnumerable<System.Net.IPEndPoint> activeListeners = null;
            if (protocolType == ProtocolType.Udp)
            {
                activeListeners = ipProps.GetActiveUdpListeners()
                    .Where(listener => listener.Port >= leftBound && listener.Port <= rightBound);
            }
            else if (protocolType == ProtocolType.Tcp)
            {
                activeListeners = ipProps.GetActiveTcpListeners()
                    .Where(listener => listener.Port >= leftBound && listener.Port <= rightBound);
            }

            //foreach (var listner in activeListeners) 
            //{
            //    Debug.WriteLine(listner);
            //}

            if (activeListeners == null) return null;

            //Список свободных портов  
            var freePorts = totalRange.Except(activeListeners.Select(listener => listener.Port));
            if (exceptPorts != null)
            {
                freePorts = freePorts.Except(exceptPorts);
            }

            return freePorts;
        }

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


        public static bool IsMulticastIpAddr(System.Net.IPAddress remoteIp)
        {
            var bytes = remoteIp.GetAddressBytes();

            return (bytes[0] >= 224 && bytes[0] <= 239);

        }
    }

    public class GraphicTools
    {
        public static Size DecreaseToEven(Size size)
        {
            int width = size.Width;
            if (width % 2 != 0)
            {
                width--;
            }

            int height = size.Height;
            if (height % 2 != 0)
            {
                height--;
            }

            return new Size(width, height);
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

    public class TestTools
    {

        public unsafe static void WriteFile(IntPtr pBuffer, long bufferLength, string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            UnmanagedMemoryStream ustream = new UnmanagedMemoryStream((byte*)pBuffer, bufferLength);
            ustream.CopyTo(file);
            ustream.Close();
            file.Close();

            //var f = File.Create(fileName);

            //byte[] data = new byte[bufferLength];
            //Marshal.Copy(pBuffer, data, 0, data.Length);
            //f.Write(data, 0, data.Length);
            //f.Close();
        }

        public static void WriteFile(byte[] buffer, string fileName)
        {
            using (var f = File.Create(fileName))
            {
                f.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
