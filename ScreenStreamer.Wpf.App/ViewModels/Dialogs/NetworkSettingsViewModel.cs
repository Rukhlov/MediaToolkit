using System.Collections.ObjectModel;
using System.Net.NetworkInformation;

using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.ViewModels.Properties;
using ScreenStreamer.Wpf;
using ScreenStreamer.Wpf.Models;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class NetworkSettingsViewModel : PropertyWindowViewModel
    {
		public override string Caption => LocalizationManager.GetString("NetworkSettingsCaption");//"Network";

        public ObservableCollection<IPAddressInfoItem> Networks { get; set; } = new ObservableCollection<IPAddressInfoItem>();
        public ObservableCollection<ProtocolKind> UnicastProtocols { get; set; } = new ObservableCollection<ProtocolKind>();

        public NetworkSettingsViewModel(PropertyNetworkViewModel property, TrackableViewModel parent) : base(property, parent)
        {
            UnicastProtocols.AddRange(NetworkHelper.GetUnicastProtocolTypes());
            Networks.AddRange(NetworkHelper.GetNetworkInfos());
        }
    }


}