using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.ViewModels.Dialogs;
using ScreenStreamer.Wpf;
using System.Linq;
using ScreenStreamer.Wpf.Models;
using ScreenStreamer.Wpf.ViewModels.Common;
using System.Text;

namespace ScreenStreamer.Wpf.ViewModels.Properties
{
    public class PropertyAudioViewModel : PropertyBaseViewModel
    {
        private readonly PropertyAudioModel _model;
		//public override string Name => "Audio";
		public override string Name => LocalizationManager.GetString("AudioPropertyName");

		//[Track]
		public bool IsAudioEnabled
        {
            get => _model.IsEnabled;
            set
            {
                SetProperty(_model, () => _model.IsEnabled, value);
                Parent?.OnAudioEnabledChanged();
				RaisePropertyChanged(nameof(Info));
			}
        }


        //[Track]
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

				RaisePropertyChanged(nameof(Info));
			}
        }


        public PropertyAudioViewModel(StreamViewModel parent, PropertyAudioModel model) : base(parent)
        {
            _model = model;

			var appModel = ServiceLocator.GetInstance<AppModel>();

			var devices = appModel.AudioSources;

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

		public override string Info
		{
			get
			{
				var builder = new StringBuilder();

				if (IsAudioEnabled)
				{
					var info = _selectedSource?.DisplayName ?? LocalizationManager.GetString("CommonStringsDeviceNotFound");
					builder.Append(info);
				}
				else
				{
					builder.Append(LocalizationManager.GetString("CommonStringsDisabled"));
				}

				if (builder.Length > MaxInfoLength)
				{
					return builder.ToString(0, MaxInfoLength - 3) + "...";
				}
				return builder.ToString();
			}

		}
	}
}
