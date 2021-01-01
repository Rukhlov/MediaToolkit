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
		public override string Caption => LocalizationManager.GetString("AudioSettingsCaption"); //"Audio";


		public ObservableCollection<AudioSourceItem> AudioSources { get; set; } = new ObservableCollection<AudioSourceItem>();

        public System.Windows.Input.ICommand UpdateAudioSourcesCommand { get; }

        public AudioSettingsViewModel(PropertyAudioViewModel property, TrackableViewModel parent) : base(property,parent)
        {

			var appModel = ServiceLocator.GetInstance<AppModel>();

            AudioSources.AddRange(appModel.AudioSources);


            UpdateAudioSourcesCommand = new Prism.Commands.DelegateCommand(UpdateSources);
        }


        public void UpdateSources()
        {
            AudioSources.Clear();

			var appModel = ServiceLocator.GetInstance<AppModel>();
			appModel.UpdateAudioSources();

			AudioSources.AddRange(appModel.AudioSources);

            ((PropertyAudioViewModel)this.Property).SelectedSource = AudioSources.FirstOrDefault();
        }
    }


}
