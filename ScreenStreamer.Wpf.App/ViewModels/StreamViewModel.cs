using System;
using Prism.Commands;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using ScreenStreamer.Wpf.Common.Models.Properties;
using ScreenStreamer.Wpf.Common.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Unity;
using NLog;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamViewModel : CustomBindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamModel Model { get; }
        private IDialogService _dialogService;

        public StreamMainViewModel MainViewModel { get; }
        public AdvancedSettingsViewModel AdvancedSettingsViewModel { get; set; }
        public PropertyVideoViewModel VideoViewModel { get; set; }
        public StreamBorderViewModel BorderViewModel { get; set; }
        public DesignBorderViewModel DesignViewModel { get; set; }

        #region Name

        public string Name
        {
            get => Model.Name;
            set
            {
                SetProperty(Model,() => Model.Name, value);
                RaisePropertyChanged(nameof(StartContextMenuText));
            }
        }

        #endregion Name

        #region IsStarted

        public bool IsStarted
        {
            get => Model.IsStarted;
            //set
            //{
            //    SetProperty(Model, () => Model.IsStarted, value);
            //    OnIsStartedChanged(Model.IsStarted);
            //}
        }

        public bool IsEnabled
        {
            get => !Model.IsBusy;
        }

        #endregion IsStarted

        #region IsSelected

        private bool _isSelected = false;
        public bool IsSelected { get => _isSelected; set { SetProperty(ref _isSelected, value); } }

        #endregion IsSelected

        #region IsEditName

        private bool _isEditName = false;
        public bool IsEditName { get => _isEditName; set { SetProperty(ref _isEditName, value); if (!value) RaisePropertyChanged(nameof(Name)); } }

        #endregion IsEditName

        public string StartCommandText => IsStarted ? "Stop Stream" : "Start Stream";

        public string StartContextMenuText => IsStarted ? $"Stop {Name}" : $"Start {Name}";

        #region Commands

        public ICommand StartCommand { get; set; }
        public ICommand StopCommand { get; set; }

        public ICommand EditNameCommand { get; set; }
        public ICommand CopyUrlCommand { get; set; }
        public ICommand PreferencesCommand { get; set; }
        public ICommand HideBorderCommand { get; set; }

        #endregion Commands

        public bool IsMicrophoneEnabled => (Properties.Single(p => p is PropertyAudioViewModel) as PropertyAudioViewModel).IsMicrophoneEnabled;
        public bool IsBorderVisible => (Properties.Single(p => p is PropertyBorderViewModel) as PropertyBorderViewModel).IsBorderVisible;

        public ObservableCollection<PropertyBaseViewModel> Properties { get; set; } = new ObservableCollection<PropertyBaseViewModel>();

        public StreamViewModel(StreamMainViewModel mainViewModel, bool addInitialProperties, StreamModel model)
        {
            Model = model;
            AdvancedSettingsViewModel = new AdvancedSettingsViewModel(Model.AdvancedSettingsModel);
            _dialogService = DependencyInjectionHelper.Container.Resolve<IDialogService>();
            MainViewModel = mainViewModel;
            StartCommand = new DelegateCommand(SwitchStreamingState);
            //StopCommand = new DelegateCommand(InverseIsStarted);

            EditNameCommand = new DelegateCommand(EditName);
            CopyUrlCommand = new DelegateCommand(CopyUrl);
            PreferencesCommand = new DelegateCommand<BaseWindowViewModel>(Preferences);
            HideBorderCommand = new DelegateCommand(HideBorder);

            if (addInitialProperties)
            {
                Properties.Add(VideoViewModel = new PropertyVideoViewModel(this, Model.PropertyVideo));
                Properties.Add(new PropertyNetworkViewModel(this, Model.PropertyNetwork));
                Properties.Add(new PropertyQualityViewModel(this, Model.PropertyQuality));
                Properties.Add(new PropertyCursorViewModel(this, Model.PropertyCursor));
                Properties.Add(new PropertyAudioViewModel(this, Model.PropertyAudio));
                Properties.Add(new PropertyBorderViewModel(this, Model.PropertyBorder));
            }
            BorderViewModel = new StreamBorderViewModel(this);
            DesignViewModel = new DesignBorderViewModel(this);

            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => OnIsStartedChanged(IsStarted))
            );

            Model.OnStreamStateChanged += Model_OnStreamStateChanged;
        }



        private void CopyUrl()
        {
            //TODO CopyUrl
        }

        public void OnMicrophoneEnabledChanged()
        {
            RaisePropertyChanged(nameof(IsMicrophoneEnabled));
        }

        private void SwitchStreamingState()
        {
            logger.Debug("SwitchStreamingState()");

            //IsStarted = !IsStarted;

            Model.SwitchStreamingState();

            //OnIsStartedChanged(IsStarted);

            MainViewModel.RaiseIsAllStartedChanged();
        }

        private void EditName()
        {
            if (!_isEditName && !MainViewModel.IsEdit)
            {
                MainViewModel.IsEdit = true;
            }
            IsEditName = !_isEditName;
        }

        private void Preferences(BaseWindowViewModel parentWindow)
        {
            _dialogService.ShowDialog(parentWindow, AdvancedSettingsViewModel);
        }

        private void Model_OnStreamStateChanged()
        {
            
            OnIsStartedChanged(IsStarted);
        }

        private void OnIsStartedChanged(bool isStarted)
        {
            logger.Debug("OnIsStartedChanged(...) " + isStarted);

            RaisePropertyChanged(nameof(StartCommandText));
            RaisePropertyChanged(nameof(StartContextMenuText));
            RaisePropertyChanged(nameof(IsEnabled));
			RaisePropertyChanged(nameof(IsStarted));

			if (isStarted)
            {
                _dialogService.Hide(DesignViewModel);
                if (IsBorderVisible)
                {
                    _dialogService.Show(BorderViewModel);
                }
            }
            else if (!isStarted)
            {
                _dialogService.Hide(BorderViewModel);
                if (IsBorderVisible)
                {
                    _dialogService.Show(DesignViewModel);
                }
            }
        }

        private void HideBorder()
        {
            if (Properties.Single(p => p is PropertyBorderViewModel) is PropertyBorderViewModel borderViewModel) 
                borderViewModel.IsBorderVisible = false;
            _dialogService.Hide(BorderViewModel);
        }
    }
}
