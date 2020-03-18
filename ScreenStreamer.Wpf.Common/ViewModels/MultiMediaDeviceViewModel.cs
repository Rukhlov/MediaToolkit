using NAudio.CoreAudioApi;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class MultiMediaDeviceViewModel
    {
        public string DisplayName => Device.FriendlyName;
        public MMDevice Device { get; set; }

        public override bool Equals(object obj)
        {
            var mmDeviceViewModel = obj as MultiMediaDeviceViewModel;
            if (mmDeviceViewModel == null)
            {
                return false;
            }

            return this.Device.ID == mmDeviceViewModel.Device.ID;
        }

        public override int GetHashCode()
        {
            return this.Device.ID.GetHashCode();
        }
    }
}
