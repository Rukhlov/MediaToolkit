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
using GalaSoft.MvvmLight.Command;
using Unity;
using NLog;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamViewModel : StreamerViewModelBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();



        public StreamModel Model { get; }
        private IDialogService _dialogService;

        public StreamMainViewModel MainViewModel { get; }
        [Track]
        public AdvancedSettingsViewModel AdvancedSettingsViewModel { get; set; }
        [Track]
        public PropertyVideoViewModel VideoViewModel { get; set; }
        [Track]
        public StreamBorderViewModel BorderViewModel { get; set; }
        [Track]
        public DesignBorderViewModel DesignViewModel { get; set; }
        [Track]
        public PropertyBorderViewModel PropertyBorder { get; set; }
        [Track]
        public PropertyAudioViewModel PropertyAudio { get; set; }
        [Track]
        public PropertyCursorViewModel PropertyCursor { get; set; }
        [Track]
        public PropertyQualityViewModel PropertyQuality { get; set; }
        [Track]
        public PropertyNetworkViewModel PropertyNetwork { get; set; }


        [Track]
        public string Name
        {
            get => Model.Name;
            set
            {
                SetProperty(Model, () => Model.Name, value);
                RaisePropertyChanged(nameof(StartContextMenuText));
            }
        }



        public bool IsEditable
        {
            get => !Model.IsStarted && !Model.IsBusy;
        }

        [Track]
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


        private bool _isSelected = false;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }


        private bool _isEditName = false;

        public bool IsEditName
        {
            get => _isEditName;
            set
            {
                _isEditName = value;
                RaisePropertyChanged(() => IsEditName);
                if (!value) RaisePropertyChanged(nameof(Name));
            }
        }



        public string StartCommandText => IsStarted ? "Stop Stream" : "Start Stream";

        public string StartContextMenuText => IsStarted ? $"Stop {Name}" : $"Start {Name}";


        public ICommand StartCommand { get; set; }
        public ICommand EditNameCommand { get; set; }
        public ICommand CopyUrlCommand { get; set; }
        public ICommand PreferencesCommand { get; set; }
        public ICommand HideBorderCommand { get; set; }
        public ICommand EditModeCommand { get; set; }


        public bool IsMicrophoneEnabled => (Properties.Single(p => p is PropertyAudioViewModel) as PropertyAudioViewModel).IsMicrophoneEnabled;
        public bool IsBorderVisible => (Properties.Single(p => p is PropertyBorderViewModel) as PropertyBorderViewModel).IsBorderVisible;

        public ObservableCollection<PropertyBaseViewModel> Properties { get; set; } = new ObservableCollection<PropertyBaseViewModel>();

        public StreamViewModel(StreamMainViewModel mainViewModel, bool addInitialProperties, StreamModel model)
        {
            Model = model;
            AdvancedSettingsViewModel = new AdvancedSettingsViewModel(Model.AdvancedSettingsModel, this);
            _dialogService = DependencyInjectionHelper.Container.Resolve<IDialogService>();
            MainViewModel = mainViewModel;
            StartCommand = new DelegateCommand(SwitchStreamingState);
            EditModeCommand = new RelayCommand(() => MainViewModel.IsEdit = true);
            EditNameCommand = new DelegateCommand(EditName);
            CopyUrlCommand = new DelegateCommand(CopyUrl);
            PreferencesCommand = new DelegateCommand<BaseWindowViewModel>(Preferences);
            HideBorderCommand = new DelegateCommand(HideBorder);

            if (addInitialProperties)
            {
                Properties.Add(VideoViewModel = new PropertyVideoViewModel(this, Model.PropertyVideo));
                Properties.Add(PropertyNetwork = new PropertyNetworkViewModel(this, Model.PropertyNetwork));
                Properties.Add(PropertyQuality = new PropertyQualityViewModel(this, Model.PropertyQuality));
                Properties.Add(PropertyCursor = new PropertyCursorViewModel(this, Model.PropertyCursor));
                Properties.Add(PropertyAudio = new PropertyAudioViewModel(this, Model.PropertyAudio));
                Properties.Add(PropertyBorder = new PropertyBorderViewModel(this, Model.PropertyBorder));
            }

            BorderViewModel = new StreamBorderViewModel(this);
            DesignViewModel = new DesignBorderViewModel(this);

            dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => OnIsStartedChanged(IsStarted))
        
                );

            Model.OnStreamStateChanged += Model_OnStreamStateChanged;
        }

        private Dispatcher dispatcher = null;

        private void CopyUrl()
        {
            //TODO CopyUrl
        }

        public void OnMicrophoneEnabledChanged()
        {
            RaisePropertyChanged(nameof(IsMicrophoneEnabled));
        }

        private void InverseIsStarted()
        {
            //IsStarted = !IsStarted;
            //MainViewModel.RaiseIsAllStartedChanged();
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
            //if (!_isEditName && !MainViewModel.IsEdit)
            //{
            //    MainViewModel.IsEdit = true;
            //}


            IsEditName = !_isEditName;
        }

        private void Preferences(BaseWindowViewModel parentWindow)
        {
            _dialogService.ShowDialog(parentWindow, AdvancedSettingsViewModel);
        }


        private void Model_OnStreamStateChanged()
        {

            dispatcher.Invoke(() =>
            {
                OnIsStartedChanged(IsStarted);
            });
            
        }

        private void OnIsStartedChanged(bool isStarted)
        {
            logger.Debug("OnIsStartedChanged(...) " + isStarted);

            RaisePropertyChanged(nameof(StartCommandText));
            RaisePropertyChanged(nameof(StartContextMenuText));
            RaisePropertyChanged(nameof(IsEnabled));
            RaisePropertyChanged(nameof(IsStarted));

            RaisePropertyChanged(nameof(IsEditable));
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
            {
                borderViewModel.IsBorderVisible = false;
            }
               
            _dialogService.Hide(BorderViewModel);
        }
    }
}
