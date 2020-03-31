using System;
using System.Drawing;
using ScreenStreamer.Wpf.Common.Enums;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
//using Polywall.Share.Exceptions;
using Prism.Commands;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyNetworkViewModel : PropertyBaseViewModel
    {
        private readonly PropertyNetworkModel _model;
        public override string Name => "Network";

        private IPAddressInfoViewModel _selectedNetwork;

        public IPAddressInfoViewModel SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                SetProperty(ref _selectedNetwork, value);
               // _model.Network = value?.IPAddressInfo?.ToString() ?? value.DisplayName;
                _model.Network = value?.IPAddressInfo?.Address?.ToString() ?? value.DisplayName;
                RaisePropertyChanged(nameof(Info));
            }
        }


        public int Port
        {
            get => _model.Port;
            set
            {
                SetProperty(_model, () => _model.Port, value);
                RaisePropertyChanged(nameof(Port));
            }
        }
 
        public bool IsUnicast
        {
            get => _model.IsUnicast;
            set
            {
                SetProperty(_model, () => _model.IsUnicast, value);
                RaisePropertyChanged(nameof(Info));
            }
        }

        public ProtocolKind UnicastProtocol
        {
            get => _model.UnicastProtocol;
            set
            {
                SetProperty(_model, () => _model.UnicastProtocol, value);
                RaisePropertyChanged(nameof(Info));
            }
        }

        public string MulticastIp
        {
            get => _model.MulticastIp;
            set
            {
                if (!IPAddress.TryParse(value, out _))
                {
                    throw new ArgumentException("Please provide valid IP address");
                }
                SetProperty(_model, () => _model.MulticastIp, value);
                RaisePropertyChanged(nameof(Info));
            }
        }



        public PropertyNetworkViewModel(StreamViewModel parent, PropertyNetworkModel model) : base(parent)
        {
            _model = model;
            var ips = NetworkHelper.GetIpAddressInfoViewModels();
            _selectedNetwork = ips.FirstOrDefault(ip =>
            {
                return $"{ip.IPAddressInfo?.Address}" == model.Network
                      || ip.DisplayName == model.Network;
            }) ?? ips.FirstOrDefault();
        }

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new NetworkSettingsViewModel(this);
        }

        public override string Info
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(this.IsUnicast ? this.UnicastProtocol.ToString() : this.MulticastIp);
                if (IsUnicast)
                {
                    builder.Append(": ");
                    builder.Append(this.SelectedNetwork.IPAddressInfo?.Address);
                }

                if (builder.Length > MaxInfoLength)
                {
                    return builder.ToString(0, MaxInfoLength - 3) + "...";
                }
                return builder.ToString();
            }
        }
    }
}
