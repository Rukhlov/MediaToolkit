

using ScreenStreamer.Wpf;
using ScreenStreamer.Wpf.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System;

//How to compare ip addresses:
//https://www.codeguru.com/csharp/csharp/cs_network/ip/article.php/c10651/IP-Address-Comparison-and-Conversion-in-C.htm

namespace ScreenStreamer.Wpf.Helpers
{
    internal static class NetworkHelper
    {

        internal static int FindAvailableTcpPort()
        {
            var port = -1;

            var appModel =  ServiceLocator.GetInstance<AppModel>();
            IEnumerable<int> appPorts = null;

            if (appModel != null)
            {
                var streamList = appModel.StreamList;
                if(streamList.Count > 0)
                {
                    appPorts = streamList.Select(s => s.PropertyNetwork.Port);
                }
            }

            var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(ProtocolType.Tcp, 1, 808, exceptPorts: appPorts);
            if (freeTcpPorts != null)
            {// может в любой момент изменится и свободный порт будет занят !!!
                var newPort = freeTcpPorts.FirstOrDefault();

                port = newPort;
            }

            return port;
        }


        internal static int GetRandomTcpPort()
        {
            var port = 0;
            var freeTcpPorts = MediaToolkit.Utils.NetTools.GetFreePortRange(System.Net.Sockets.ProtocolType.Tcp, 1).ToList();
            if (freeTcpPorts != null && freeTcpPorts.Count > 0)
            {
                Random rnd = new Random();
                var index = rnd.Next(0, freeTcpPorts.Count);

                port = freeTcpPorts[index]; //freeTcpPorts.FirstOrDefault();

            }

            return port;
        }

        private static List<(NetworkInterface Network, IPAddressInformation IpAddressInfo)> GetNetworkInterfaces()
        {

            var result = new List<(NetworkInterface Network, IPAddressInformation IpAddressInfo)>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var network in networkInterfaces)
            {

                if (network.OperationalStatus == OperationalStatus.Up &&
                    network.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var adapterProperties = network.GetIPProperties();

                    if (adapterProperties.GatewayAddresses.FirstOrDefault() != null)
                    {
                        foreach (var ip in adapterProperties.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                result.Add((network, ip));
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal static string ToDisplayName(this IPAddressInformation ipAddressInfo, NetworkInterface networkInterface)
        {
            return $"{networkInterface.Name} ({ipAddressInfo.Address})";
        }

        internal static List<IPAddressInfoItem> GetNetworkInfos()
        {
            var result = new List<IPAddressInfoItem>()
            {
                new IPAddressInfoItem
                {
                    DisplayName = "All",
                    InterfaceName = "All",
                    IPAddressInfo = null
                }
            };

            var interfaces = GetNetworkInterfaces();
            interfaces.ForEach(i => result.Add(new IPAddressInfoItem
            {
                DisplayName = i.IpAddressInfo.ToDisplayName(i.Network),
                InterfaceName = i.Network.Name,
                IPAddressInfo = i.IpAddressInfo

            }));
            return result;
        }

        internal static List<ProtocolKind> GetUnicastProtocolTypes() => new List<ProtocolKind>() { ProtocolKind.TCP, ProtocolKind.UDP };

        internal static bool IsDeepEquals(this IPAddressInformation source, IPAddressInformation target)
        {
            return source.Address.Equals(target.Address) &&
                   source.IsDnsEligible == target.IsDnsEligible &&
                   source.IsTransient == target.IsTransient;
        }

        internal static int GetDeepHashCode(this IPAddressInformation source)
        {
            return source.Address.GetHashCode() +
                   (source.IsDnsEligible ? 1 : -1) +
                   (source.IsTransient ? 10 : -10);
        }
    }
}
