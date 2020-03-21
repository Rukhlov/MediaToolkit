using ScreenStreamer.Wpf.Common.Helpers;
using System;
using System.Net.NetworkInformation;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class IPAddressInfoViewModel
    { 
        public string DisplayName { get; set; }

        public IPAddressInformation IPAddressInfo { get; set; }

        public override bool Equals(object obj)
        {
            var ipAddressInfoViewModel = obj as IPAddressInfoViewModel;
            if (ipAddressInfoViewModel == null)
            {
                return false;
            }

            return this.DisplayName == ipAddressInfoViewModel.DisplayName &&
                   (this.IPAddressInfo == null ?
                        ipAddressInfoViewModel.IPAddressInfo == null :
                        this.IPAddressInfo.IsDeepEquals(ipAddressInfoViewModel.IPAddressInfo));
        }

        public override int GetHashCode()
        {
            return this.DisplayName.GetHashCode() +
                   (this.IPAddressInfo != null ? this.IPAddressInfo.GetDeepHashCode() : 0);
        }
    }
}
