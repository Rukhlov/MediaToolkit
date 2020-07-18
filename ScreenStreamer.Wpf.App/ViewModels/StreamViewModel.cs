using System;
using Prism.Commands;
using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.ViewModels.Dialogs;
using ScreenStreamer.Wpf.ViewModels.Properties;

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using NLog;
using MediaToolkit.UI;
using ScreenStreamer.Wpf.Models;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels
{
    public class StreamViewModel : TrackableViewModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public StreamViewModel(MainViewModel mainViewModel, bool addInitialProperties, MediaStreamModel model)
        {
            MediaStreamer = model;

            dialogService = ServiceLocator.GetInstance<IDialogService>();

            MainViewModel = mainViewModel;
            StartCommand = new DelegateCommand(SwitchStreamingState);

            //EditModeCommand = new RelayCommand(() => MainViewModel.IsEdit = true);
            EditModeCommand = new DelegateCommand(() => MainViewModel.IsEdit = true);

            EditNameCommand = new DelegateCommand(EditName);
            CopyUrlCommand = new DelegateCommand(CopyUrl);
            PreferencesCommand = new DelegateCommand<BaseWindowViewModel>(Preferences);
            HideBorderCommand = new DelegateCommand(HideBorder);
            ShowSettingsCommand = new DelegateCommand(ShowSettings);


            if (addInitialProperties)
            {
                VideoViewModel = new PropertyVideoViewModel(this, MediaStreamer.PropertyVideo);
                PropertyAudio = new PropertyAudioViewModel(this, MediaStreamer.PropertyAudio);
                PropertyNetwork = new PropertyNetworkViewModel(this, MediaStreamer.PropertyNetwork);

                Properties.Add(VideoViewModel);
                Properties.Add(PropertyAudio);
                Properties.Add(PropertyNetwork);

                // Properties.Add(PropertyQuality = new PropertyQualityViewModel(this, Model.PropertyQuality));
                //  Properties.Add(PropertyCursor = new PropertyCursorViewModel(this, Model.PropertyCursor));

                //Properties.Add(PropertyBorder = new PropertyBorderViewModel(this, Model.PropertyBorder));
            }

            AdvancedSettingsViewModel = new AdvancedSettingsViewModel(MediaStreamer.AdvancedSettings, this);

            BorderViewModel = new BorderViewModel(this, MediaStreamer.PropertyBorder);

            DesignBorderViewModel = new DesignBorderViewModel(this, MediaStreamer.PropertyBorder);

            VideoViewModel.SetupDisplayRegion();

            dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => OnStreamStateChanged(IsStarted))

                );

            MediaStreamer.StateChanged += MediaStreamer_StateChanged;
            MediaStreamer.ErrorOccurred += MediaStreamer_ErrorOccurred;
        }

        private IDialogService dialogService;
        private Dispatcher dispatcher = null;

        public MediaStreamModel MediaStreamer { get; }

        public MainViewModel MainViewModel { get; }

        public ObservableCollection<PropertyBaseViewModel> Properties { get; set; } = new ObservableCollection<PropertyBaseViewModel>();


        [Track]
        public AdvancedSettingsViewModel AdvancedSettingsViewModel { get; set; }

        [Track]
        public PropertyVideoViewModel VideoViewModel { get; set; }

        [Track]
        public BorderViewModel BorderViewModel { get; set; }

        [Track]
        public DesignBorderViewModel DesignBorderViewModel { get; set; }

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


       // [Track]
        public string Name
        {
            get => MediaStreamer.Name;
            set
            {
				const int maxNameLength = 64;
				var name = value;
				if (name.Length > maxNameLength)
				{
					name = name.Substring(0, maxNameLength);
				}

                SetProperty(MediaStreamer, () => MediaStreamer.Name, name);
                RaisePropertyChanged(nameof(StartContextMenuText));
            }
        }



        public bool IsEditable
        {
            get => !MediaStreamer.IsStreaming && !MediaStreamer.IsBusy;
        }

       // [Track]
        public bool IsStarted
        {
            get => MediaStreamer.IsStreaming;
            //set
            //{
            //    SetProperty(Model, () => Model.IsStarted, value);
            //    OnIsStartedChanged(Model.IsStarted);
            //}
        }

        public bool IsEnabled
        {
            get => !MediaStreamer.IsBusy;
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

        public bool IsAudioEnabled => (Properties.Single(p => p is PropertyAudioViewModel) as PropertyAudioViewModel).IsAudioEnabled;
        //public bool IsBorderVisible => (Properties.Single(p => p is PropertyBorderViewModel) as PropertyBorderViewModel).IsBorderVisible;
        public bool IsBorderVisible { get; set; }


        public string StartCommandText => IsStarted ? "Stop Stream" : "Start Stream";

        public string StartContextMenuText => IsStarted ? $"Stop {Name}" : $"Start {Name}";


        public ICommand StartCommand { get; set; }
        public ICommand EditNameCommand { get; set; }
        public ICommand CopyUrlCommand { get; set; }
        public ICommand PreferencesCommand { get; set; }
        public ICommand HideBorderCommand { get; set; }
        public ICommand EditModeCommand { get; set; }
        public ICommand ShowSettingsCommand { get; } 



        private void CopyUrl()
        {
            //TODO CopyUrl
        }

        public void OnAudioEnabledChanged()
        {
            RaisePropertyChanged(nameof(IsAudioEnabled));
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

            MediaStreamer.SwitchStreamingState();

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
            dialogService.ShowDialog(parentWindow, AdvancedSettingsViewModel);
        }


        private void MediaStreamer_StateChanged()
        {

            dispatcher.Invoke(() =>
            {
                OnStreamStateChanged(IsStarted);

            });
            
        }

        private void MediaStreamer_ErrorOccurred(object obj)
        {
            logger.Debug("Model_ErrorOccurred(...)");
            // TODO: process error...

            dispatcher.Invoke(() =>
            {
                if (obj != null)
                {
                    var ex = obj as Exception;
                    OnStreamErrorOccurred(ex);
                }
            });

            //System.Windows.Forms.MessageBox.Show(errorMessage, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }



        private RegionForm borderForm = null;
        private void OnStreamStateChanged(bool isStarted)
        {
            logger.Debug("OnStreamStateChanged(...) " + isStarted);

            RaisePropertyChanged(nameof(StartCommandText));
            RaisePropertyChanged(nameof(StartContextMenuText));
            RaisePropertyChanged(nameof(IsEnabled));
            RaisePropertyChanged(nameof(IsStarted));

            RaisePropertyChanged(nameof(IsEditable));

            PropertyNetwork.UpdatePropInfo(IsStarted);


            if (isStarted)
            {
                dialogService.Hide(DesignBorderViewModel);

            }
            else 
            {
                if (IsBorderVisible)
                {
                    dialogService.Show(DesignBorderViewModel);
                }
            }

            //if (isStarted)
            //{
            //    _dialogService.Hide(DesignViewModel);
            //    if (IsBorderVisible)
            //    {
            //        _dialogService.Show(BorderViewModel);
            //    }

            //}
            //else if (!isStarted)
            //{
            //    _dialogService.Hide(BorderViewModel);
            //    if (IsBorderVisible)
            //    {
            //        _dialogService.Show(DesignViewModel);
            //    }
            //}

            VideoViewModel.OnStreamStateChanged(isStarted);

            MainViewModel.OnStreamStateChanged();

            if (isStarted)
            {
                if (VideoViewModel.ShowCaptureBorder && VideoViewModel.IsScreenSource)
                {
                    var captureRect = VideoViewModel.CaptureRect;
                    borderForm = new RegionForm(captureRect);
                    borderForm.Visible = true;
                }
            }
            else
            {
                if (borderForm != null)
                {
                    borderForm.Close();
                    borderForm = null;
                }

            }

        }

        private void OnStreamErrorOccurred(Exception ex = null)
        {

            var errorMessage = "Unknown error!\r\n\r\nSee log file for details...........";
            if (ex != null)
            {
                errorMessage = ex.Message + "\r\n\r\nSee log file for details..............";
            }

            var dialogService = ServiceLocator.GetInstance<IDialogService>();

            var caption = "Error";
            var view = new MessageBoxViewModel(errorMessage, caption, System.Windows.MessageBoxImage.Error);
            dialogService.ShowDialog(MainViewModel, view);
        }


        private void ShowSettings()
        {
            MainViewModel?.ShowStreamSettingsCommand?.Execute(this);
        }

        private void HideBorder()
        {
            
            if (Properties.Single(p => p is PropertyBorderViewModel) is PropertyBorderViewModel borderViewModel)
            {
                borderViewModel.IsBorderVisible = false;
            }
               
            dialogService.Hide(BorderViewModel);
        }

        public void Close()
        {
            //if (VideoViewModel != null)
            //{
            //    VideoViewModel.Close();
            //}

        }
    }
}
