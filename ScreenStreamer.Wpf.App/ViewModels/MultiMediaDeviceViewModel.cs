using MediaToolkit.Core;
//using NAudio.CoreAudioApi;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class AudioDeviceViewModel
    {


        //public string DisplayName => Device.FriendlyName;
        //public MMDevice Device { get; set; }
        //public MMDevice Device { get; set; }

        public readonly AudioCaptureDevice Device = null;

        public string DeviceId => Device.DeviceId;
        public string DisplayName => Device.Name;

        public AudioDeviceViewModel(AudioCaptureDevice d)
        {
            this.Device = d;
        }


        public override bool Equals(object obj)
        {
            var mmDeviceViewModel = obj as AudioDeviceViewModel;
            if (mmDeviceViewModel == null)
            {
                return false;
            }

            return this.DeviceId == mmDeviceViewModel.DeviceId;

            //return this.Device.ID == mmDeviceViewModel.Device.ID;
        }

        public override int GetHashCode()
        {
            return this.DeviceId.GetHashCode();
        }
    }
}
