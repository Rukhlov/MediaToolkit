using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AudioSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Audio";

        public ObservableCollection<MultiMediaDeviceViewModel> Microphones { get; set; } = new ObservableCollection<MultiMediaDeviceViewModel>();

        public AudioSettingsViewModel(PropertyAudioViewModel property) : base(property)
        {
            Microphones.AddRange(AudioHelper.GetMultiMediaDeviceViewModels());
        }
    }
}
