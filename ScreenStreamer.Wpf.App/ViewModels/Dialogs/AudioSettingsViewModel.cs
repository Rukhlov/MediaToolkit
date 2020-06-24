using MediaToolkit;
using MediaToolkit.Core;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Models.Properties;
using ScreenStreamer.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class AudioSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Audio";

        public ObservableCollection<AudioSourceItem> AudioSources { get; set; } = new ObservableCollection<AudioSourceItem>();

        public System.Windows.Input.ICommand UpdateAudioSourcesCommand { get; }

        public AudioSettingsViewModel(PropertyAudioViewModel property, TrackableViewModel parent) : base(property,parent)
        {
            AudioSources.AddRange(AudioSourceItem.GetMultiMediaDeviceViewModels());


            UpdateAudioSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);
        }


        public void UpdateSources()
        {
            AudioSources.Clear();

            AudioSources.AddRange(AudioSourceItem.GetMultiMediaDeviceViewModels());

            ((PropertyAudioViewModel)this.Property).SelectedSource = AudioSources.FirstOrDefault();
        }
    }


}
