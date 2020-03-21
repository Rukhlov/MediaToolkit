using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyAudioViewModel : PropertyBaseViewModel
    {
        private readonly PropertyAudioModel _model;
        public override string Name => "Audio";

        #region IsMicrophoneEnabled

        public bool IsMicrophoneEnabled { get => _model.IsMicrophoneEnabled; set { SetProperty(_model, () => _model.IsMicrophoneEnabled, value); Parent?.OnMicrophoneEnabledChanged(); } }

        #endregion IsMicrophoneEnabled

        #region IsComputerSoundEnabled

        public bool IsComputerSoundEnabled { get => _model.IsComputerSoundEnabled; set { SetProperty(_model,() => _model.IsComputerSoundEnabled, value); } }

        #endregion IsComputerSoundEnabled

        #region SelectedMicrophone

        private MultiMediaDeviceViewModel _selectedMicrophone;

        public MultiMediaDeviceViewModel SelectedMicrophone
        {
            get => _selectedMicrophone;
            set
            {
                SetProperty(ref _selectedMicrophone, value);
                _model.DeviceId = value.Device?.ID;
            }
        }

        #endregion SelectedMicrophone

        public PropertyAudioViewModel(StreamViewModel parent, PropertyAudioModel model) : base(parent)
        {
            _model = model;
            var devices = AudioHelper.GetMultiMediaDeviceViewModels();
            _selectedMicrophone = devices.FirstOrDefault(device => device.Device?.ID == model.DeviceId) ?? devices.FirstOrDefault();
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
            return new AudioSettingsViewModel(this);
        }
    }
}
