using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using ScreenStreamer.Wpf;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyAudioViewModel : PropertyBaseViewModel
    {
        private readonly PropertyAudioModel _model;
        public override string Name => "Audio";


        //[Track]
        public bool IsAudioEnabled
        {
            get => _model.IsEnabled;
            set
            {
                SetProperty(_model, () => _model.IsEnabled, value);
                Parent?.OnAudioEnabledChanged();
            }
        }


        [Track]
        public bool IsComputerSoundEnabled
        {
            get => _model.IsComputerSoundEnabled;
            set
            {
                SetProperty(_model, () => _model.IsComputerSoundEnabled, value);
            }
        }


        private AudioSourceItem _selectedSource;

        public AudioSourceItem SelectedSource
        {
            get => _selectedSource;
            set
            {
                _selectedSource = value;
                RaisePropertyChanged(() => SelectedSource);
                _model.DeviceId = value?.DeviceId;
            }
        }


        public PropertyAudioViewModel(StreamViewModel parent, PropertyAudioModel model) : base(parent)
        {
            _model = model;

            var devices = AudioHelper.GetAudioSourceItems();
            _selectedSource = devices.FirstOrDefault(device => device.DeviceId == model.DeviceId) ?? devices.FirstOrDefault();
        }

        //public override object Clone()
        //{
        //    return new PropertyAudioViewModel(null, )
        //    {
        //        Info = this.Info,
        //        IsComputerSoundEnabled = this.IsComputerSoundEnabled,
        //        IsMicrophoneEnabled = this.IsMicrophoneEnabled,
        //        SelectedMicrophone = this.SelectedMicrophone
        //    };
        //}

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new AudioSettingsViewModel(this, Parent);
        }
    }
}
