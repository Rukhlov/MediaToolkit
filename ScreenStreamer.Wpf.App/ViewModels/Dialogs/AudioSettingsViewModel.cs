using MediaToolkit;
using MediaToolkit.Core;
using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.ViewModels.Properties;
using ScreenStreamer.Wpf.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class AudioSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Audio";

        public ObservableCollection<AudioSourceItem> AudioSources { get; set; } = new ObservableCollection<AudioSourceItem>();

        public System.Windows.Input.ICommand UpdateAudioSourcesCommand { get; }

        public AudioSettingsViewModel(PropertyAudioViewModel property, TrackableViewModel parent) : base(property,parent)
        {
            AudioSources.AddRange(AudioHelper.GetAudioSources());


            UpdateAudioSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);
        }


        public void UpdateSources()
        {
            AudioSources.Clear();

            AudioSources.AddRange(AudioHelper.GetAudioSources());

            ((PropertyAudioViewModel)this.Property).SelectedSource = AudioSources.FirstOrDefault();
        }
    }


}
