using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AudioSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Audio";

        public ObservableCollection<AudioDeviceViewModel> AudioSources { get; set; } = new ObservableCollection<AudioDeviceViewModel>();

        public AudioSettingsViewModel(PropertyAudioViewModel property, StreamerViewModelBase parent) : base(property,parent)
        {
            AudioSources.AddRange(AudioHelper.GetMultiMediaDeviceViewModels());
        }
    }
}
