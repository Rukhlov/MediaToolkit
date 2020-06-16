using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AudioSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Audio";

        public ObservableCollection<AudioDeviceViewModel> AudioSources { get; set; } = new ObservableCollection<AudioDeviceViewModel>();

        public System.Windows.Input.ICommand UpdateAudioSourcesCommand { get; }

        public AudioSettingsViewModel(PropertyAudioViewModel property, StreamerViewModelBase parent) : base(property,parent)
        {
            AudioSources.AddRange(AudioHelper.GetMultiMediaDeviceViewModels());


            UpdateAudioSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);
        }


        public void UpdateSources()
        {
            AudioSources.Clear();

            AudioSources.AddRange(AudioHelper.GetMultiMediaDeviceViewModels());

            ((PropertyAudioViewModel)this.Property).SelectedSource = AudioSources.FirstOrDefault();
        }
    }
}
