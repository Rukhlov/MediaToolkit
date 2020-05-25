using System.Collections.ObjectModel;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class NetworkSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Network";

        public ObservableCollection<IPAddressInfoViewModel> Networks { get; set; } = new ObservableCollection<IPAddressInfoViewModel>();
        public ObservableCollection<ProtocolKind> UnicastProtocols { get; set; } = new ObservableCollection<ProtocolKind>();

        public NetworkSettingsViewModel(PropertyNetworkViewModel property, StreamerViewModelBase parent) : base(property, parent)
        {
            UnicastProtocols.AddRange(NetworkHelper.GetUnicastProtocolTypes());
            Networks.AddRange(NetworkHelper.GetIpAddressInfoViewModels());
        }
    }
}