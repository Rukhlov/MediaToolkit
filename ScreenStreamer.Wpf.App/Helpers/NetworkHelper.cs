using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

//How to compare ip addresses:
//https://www.codeguru.com/csharp/csharp/cs_network/ip/article.php/c10651/IP-Address-Comparison-and-Conversion-in-C.htm

namespace ScreenStreamer.Wpf.Common.Helpers
{
    internal static class NetworkHelper
    {

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

        internal static List<IPAddressInfoViewModel> GetIpAddressInfoViewModels()
        {
            var result = new List<IPAddressInfoViewModel>()
            {
                new IPAddressInfoViewModel
                {
                    DisplayName = "All",
                    IPAddressInfo = null
                }
            };

            var interfaces = GetNetworkInterfaces();
            interfaces.ForEach(i => result.Add(new IPAddressInfoViewModel
            {
                DisplayName = i.IpAddressInfo.ToDisplayName(i.Network),
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
