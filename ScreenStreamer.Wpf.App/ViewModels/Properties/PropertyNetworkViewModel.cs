using System;
using System.Drawing;

using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
//using Polywall.Share.Exceptions;
using Prism.Commands;
using ScreenStreamer.Wpf;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyNetworkViewModel : PropertyBaseViewModel
    {
        private readonly PropertyNetworkModel _model;
        public override string Name => "Network";


  
        private IPAddressInfoItem _selectedNetwork;
        [Track]
        public IPAddressInfoItem SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                _selectedNetwork = value;
                RaisePropertyChanged(() => SelectedNetwork);
                _model.Network = value?.IPAddressInfo?.Address?.ToString() ?? value.DisplayName;
                RaisePropertyChanged(nameof(Info));
            }
        }

        public int CommunicationPort => _model.CommunicationPort;

        [Track]
        public int Port
        {
            get => _model.Port;
            set
            {
                SetProperty(_model, () => _model.Port, value);

                RaisePropertyChanged(nameof(Info));
            }
        }



        [Track]
        public bool IsUnicast
        {
            get => _model.IsUnicast;
            set
            {
                SetProperty(_model, () => _model.IsUnicast, value);
                RaisePropertyChanged(nameof(Info));
            }
        }



        [Track]
        public ProtocolKind UnicastProtocol
        {
            get => _model.UnicastProtocol;
            set
            {
                SetProperty(_model, () => _model.UnicastProtocol, value);
                RaisePropertyChanged(nameof(Info));
            }
        }



        [Track]
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

        [Track]
        public int MulticastVideoPort
        {
            get => _model.MulticasVideoPort;
            set
            {
                SetProperty(_model, () => _model.MulticasVideoPort, value);

            }
        }

        [Track]
        public int MulticastAudioPort
        {
            get => _model.MulticasAudioPort;
            set
            {
                SetProperty(_model, () => _model.MulticasAudioPort, value);

            }
        }



        public PropertyNetworkViewModel(StreamViewModel parent, PropertyNetworkModel model) : base(parent)
        {
            _model = model;
            var ips = NetworkHelper.GetNetworkInfos();
            _selectedNetwork = ips.FirstOrDefault(ip =>
            {
                return $"{ip.IPAddressInfo?.Address}" == model.Network
                      || ip.DisplayName == model.Network;
            }) ?? ips.FirstOrDefault();
        }

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new NetworkSettingsViewModel(this, Parent);
        }

        private bool isStarted = false;
        public void UpdatePropInfo(bool isStarted)
        {
            this.isStarted = isStarted;
            RaisePropertyChanged(nameof(Info));
        }

        public override string Info
        {
            get
            {
                var address = "0.0.0.0";
                var _port = isStarted? this.CommunicationPort : this.Port;


                string port = (_port > 0 ? _port.ToString() : "Auto");

                var ipAddrInfo= SelectedNetwork.IPAddressInfo;
                if(ipAddrInfo != null)
                {
                    address = ipAddrInfo.Address.ToString();
                }

                var info = SelectedNetwork.InterfaceName + " (" + address + ":"+ port + ")";
                if (info.Length > MaxInfoLength)
                {
                    info = info.Substring(0, MaxInfoLength - 3) + "...";
                }

                return info;

                //var builder = new StringBuilder();

                //builder.Append(this.IsUnicast ? this.UnicastProtocol.ToString() : this.MulticastIp);
                //if (IsUnicast)
                //{
                //    builder.Append(": ");
                //    builder.Append(this.SelectedNetwork.IPAddressInfo?.Address);
                //}

                //if (builder.Length > MaxInfoLength)
                //{
                //    return builder.ToString(0, MaxInfoLength - 3) + "...";
                //}
                //return builder.ToString();


            }
        }
    }
}
