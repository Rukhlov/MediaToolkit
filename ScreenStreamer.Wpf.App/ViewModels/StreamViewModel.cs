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
using MediaToolkit.UI;

namespace ScreenStreamer.Wpf.Common.Models
{
    public class StreamViewModel : TrackableViewModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();



        public MediaStreamModel Model { get; }

        private IDialogService _dialogService;

        public MainViewModel MainViewModel { get; }

        [Track]
        public AdvancedSettingsViewModel AdvancedSettingsViewModel { get; set; }
        [Track]
        public PropertyVideoViewModel VideoViewModel { get; set; }
        [Track]
        public BorderViewModel BorderViewModel { get; set; }
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


       // [Track]
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

       // [Track]
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


        public bool IsAudioEnabled => (Properties.Single(p => p is PropertyAudioViewModel) as PropertyAudioViewModel).IsAudioEnabled;
        //public bool IsBorderVisible => (Properties.Single(p => p is PropertyBorderViewModel) as PropertyBorderViewModel).IsBorderVisible;

        public ObservableCollection<PropertyBaseViewModel> Properties { get; set; } = new ObservableCollection<PropertyBaseViewModel>();

        public StreamViewModel(MainViewModel mainViewModel, bool addInitialProperties, MediaStreamModel model)
        {
            Model = model;
            AdvancedSettingsViewModel = new AdvancedSettingsViewModel(Model.AdvancedSettingsModel, this);

            _dialogService = ServiceLocator.GetInstance<IDialogService>();

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
				Properties.Add(PropertyAudio = new PropertyAudioViewModel(this, Model.PropertyAudio));

				Properties.Add(PropertyNetwork = new PropertyNetworkViewModel(this, Model.PropertyNetwork));
               // Properties.Add(PropertyQuality = new PropertyQualityViewModel(this, Model.PropertyQuality));
              //  Properties.Add(PropertyCursor = new PropertyCursorViewModel(this, Model.PropertyCursor));
                
               // Properties.Add(PropertyBorder = new PropertyBorderViewModel(this, Model.PropertyBorder));
            }

            BorderViewModel = new BorderViewModel(this);
            DesignViewModel = new DesignBorderViewModel(this);

            dispatcher = Dispatcher.CurrentDispatcher;

            dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => OnStreamStateChanged(IsStarted))
        
                );

            Model.OnStreamStateChanged += Model_OnStreamStateChanged;
            Model.ErrorOccurred += Model_ErrorOccurred;
        }


        private Dispatcher dispatcher = null;

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
                OnStreamStateChanged(IsStarted);

            });
            
        }

        private void Model_ErrorOccurred(object obj)
        {
            logger.Debug("Model_ErrorOccurred(...)");
            var errorMessage = "Unknown error!\r\n\r\nSee log file for details...........";

            if (obj != null)
            {
                var ex = obj as Exception;
                if (ex != null)
                {
                    errorMessage = ex.Message + "\r\n\r\nSee log file for details..............";

                }
            }

            System.Windows.Forms.MessageBox.Show(errorMessage, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
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

        private void HideBorder()
        {
            
            if (Properties.Single(p => p is PropertyBorderViewModel) is PropertyBorderViewModel borderViewModel)
            {
                borderViewModel.IsBorderVisible = false;
            }
               
            _dialogService.Hide(BorderViewModel);
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
