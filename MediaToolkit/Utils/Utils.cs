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

    public class RegistryTool
    {
       
        public static void SetUserGpuPreferences(string fileName, int UserGpuPreferences)
        {
            //Windows 10 Build 1809 and higher
            //Starting with Windows 10 build 19564, Microsoft updated the Graphics settings page (Settings > System > Display > Graphics settings),
            //allowing for better control over designating which GPU your apps run on.

            //х.з где это документировано найдено на форуме
            //https://social.msdn.microsoft.com/Forums/office/en-US/faaa3a92-ed9a-4878-82b9-a43e175cc6e4/graphics-performance-preference
            /*
             * HKEY_CURRENT_USER\SOFTWARE\Microsoft\DirectX\UserGpuPreferences
                Power savings:
                [application full path with \\ as path separators] = "GpuPreference=1;"
                Maximum performance:
                [application full path with \\ as path separators] = "GpuPreference=2;"
            */
            var name = @"Software\Microsoft\DirectX\UserGpuPreferences";
            using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(name, true))
            {
                if (key != null)
                {// if supported, use Windows 10 graphics performance settings
                    var value = "GpuPreference=" + UserGpuPreferences + ";";
                    key.SetValue(fileName, value);

                }
            }
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
			if (!OperationContext.Current.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out messageProperty))
			{
				return null;
			}

            var remoteEndpointProperty = messageProperty as RemoteEndpointMessageProperty;
			if (remoteEndpointProperty == null)
			{
				return null;
			}

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
