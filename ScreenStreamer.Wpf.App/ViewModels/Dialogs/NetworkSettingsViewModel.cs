using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using ScreenStreamer.Wpf;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class NetworkSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Network";

        public ObservableCollection<IPAddressInfoItem> Networks { get; set; } = new ObservableCollection<IPAddressInfoItem>();
        public ObservableCollection<ProtocolKind> UnicastProtocols { get; set; } = new ObservableCollection<ProtocolKind>();

        public NetworkSettingsViewModel(PropertyNetworkViewModel property, TrackableViewModel parent) : base(property, parent)
        {
            UnicastProtocols.AddRange(NetworkHelper.GetUnicastProtocolTypes());
            Networks.AddRange(NetworkHelper.GetIpAddressInfoViewModels());
        }
    }


}