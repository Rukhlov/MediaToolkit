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
using System.Windows.Forms;
using System.Collections.Generic;
using ScreenStreamer.Common;

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
            //EditModeCommand = new DelegateCommand(() => MainViewModel.IsEdit = true);

            EditModeCommand = new DelegateCommand(SetEditMode);

            EditNameCommand = new DelegateCommand(SwitchEditNameState);
            CopyUrlCommand = new DelegateCommand(CopyUrl);
            PreferencesCommand = new DelegateCommand<WindowViewModel>(ShowPreferencesDialog);
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
                new Action(() => OnStreamStateChanged(IsStarted)));

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


		//public string StartCommandText => IsStarted ? "Stop Stream" : "Start Stream";

		public string StartCommandText => IsStarted ?
			LocalizationManager.GetString("StartCommandText_StopStream") :
			LocalizationManager.GetString("StartCommandText_StartStream");

		//public string StartContextMenuText => IsStarted ? $"Stop {Name}" : $"Start {Name}";
		public string StartContextMenuText
		{
			get
			{
				return Name;
				//var text = IsStarted ? 
				//	LocalizationManager.GetString("ContextMenu_Stop") :
				//	LocalizationManager.GetString("ContextMenu_Start");

				//return text + " " + Name;
			}
		}


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

        private void SwitchStreamingState()
        {
            logger.Debug("SwitchStreamingState()");

            var streamList = MainViewModel.StreamList;

            var currentVideoDevice = this.VideoViewModel.Display;
            if(currentVideoDevice == null)
            {
                var caption = "Streaming Error";
                var message = "Video device not found. Check device availability and try again";

                ShowError(caption, message);

                OnStreamStateChanged(IsStarted);
                return;
            }

            var currDeviceId = currentVideoDevice.DeviceId;

            if (VideoViewModel.IsScreenSource)
            {// Screen source...

                if (VideoViewModel.CaptureType.CaptType == MediaToolkit.Core.VideoCaptureType.DXGIDeskDupl)
                {// один захват экрана на монитор...

                    bool captureLocked = false;

                    var startedScreenStreams = streamList.Where(s => (s != this && s.IsStarted || !s.IsEnabled) && s.VideoViewModel.IsScreenSource).ToList();

                    foreach (var s in startedScreenStreams)
                    {
                        var videoProps = s.VideoViewModel;
                        if (videoProps.CaptureType.CaptType == MediaToolkit.Core.VideoCaptureType.DXGIDeskDupl)
                        {
                            var captRect = s.VideoViewModel.CaptureRect;

                            List<Screen> lockedScreens = new List<Screen>();

                            foreach (var screen in Screen.AllScreens)
                            {
                                var rect = System.Drawing.Rectangle.Intersect(captRect, screen.Bounds);
                                if (rect.Width > 0 && rect.Height > 0)
                                {
                                    lockedScreens.Add(screen);
                                }
                            }

                            if (lockedScreens.Count > 0)
                            {
                                var currCaptRect = this.VideoViewModel.CaptureRect;
                                foreach (var screen in lockedScreens)
                                {
                                    var rect = System.Drawing.Rectangle.Intersect(currCaptRect, screen.Bounds);
                                    if (rect.Width > 0 && rect.Height > 0)
                                    {
                                        captureLocked = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    //if (captureLocked)
                    //{
                    //    var caption = "Streaming Error";
                    //    var message = "The Desktop Duplication API capture is already in use on this screen area.\r\n" +
                    //                  "To start the stream, select another capture type.";

                    //    ShowError(caption, message);

                    //    OnStreamStateChanged(IsStarted);
                    //    return;
                    //}

                }

            }
            else
            {// UVC device
                // если камера используется в другом процессе, то ошибка вылезет при старте 
                var streams = streamList.Where(s => (s != this && (s.IsStarted || !s.IsEnabled)) && (s.VideoViewModel.Display.DeviceId == currDeviceId)).ToList();

                if (streams.Count > 0)
                {
                    var caption = "Streaming Error";
                    // var message = $"UVC device '{currentVideoDevice.Name}' is blocked by another application";
                    var message = $"UVC device '{currentVideoDevice.Name}' is already in use or blocked.\r\n";// + 
                     // "Multiple streaming from one UVC device is not currently supported.";

                    ShowError(caption, message);

                    OnStreamStateChanged(IsStarted);

                    return;
                }

            }


            MediaStreamer.SwitchStreamingState();


            //MainViewModel.RaiseIsAllStartedChanged();
        }

        private void SetEditMode()
        {
            MainViewModel.SelectedStream = this;
            MainViewModel.IsEdit = true;
        }

        private void SwitchEditNameState()
        {
            //if (!_isEditName && !MainViewModel.IsEdit)
            //{
            //    MainViewModel.IsEdit = true;
            //}


            IsEditName = !_isEditName;
        }

        private void ShowPreferencesDialog(WindowViewModel parentWindow)
        {
            dialogService.ShowDialog(parentWindow, AdvancedSettingsViewModel);
        }


        private void MediaStreamer_StateChanged()
        {
            logger.Debug("MediaStreamer_StateChanged(...)");

            dispatcher.Invoke(() =>
            {
                OnStreamStateChanged(IsStarted);

            });

        }

        private void MediaStreamer_ErrorOccurred(object obj)
        {
            logger.Debug("MediaStreamer_ErrorOccurred(...)");

            var caption = "Unknown Error";
            var message = "Unknown error!\r\n\r\nSee log file for details.";

            // TODO: process error...
            if (obj != null)
            {
                var ex = obj as Exception;

                if (ex != null)
                {
                    message = ex.Message + "\r\n\r\nSee log file for details.";

                    var streamerException = ex as StreamerException;
                    if (streamerException != null)
                    {
                        caption = streamerException.Caption;
                        message = streamerException.Message;
                    }
                }
            }

            dispatcher.Invoke(() =>
            {
                ShowError(caption, message);

            });

            //System.Windows.Forms.MessageBox.Show(errorMessage, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
        }


		private StatisticForm statisticForm = null;
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
                if (MainViewModel.IsVisible)
                {
                    if (IsBorderVisible)
                    {
                        dialogService.Show(DesignBorderViewModel);
                    }
                }

            }

            //if (isStarted)
            //{
            //    dialogService.Hide(DesignBorderViewModel);
            //    if (IsBorderVisible)
            //    {
            //        BorderViewModel.WpfLeft = DesignBorderViewModel.WpfLeft;
            //        BorderViewModel.WpfTop = DesignBorderViewModel.WpfTop;
            //        BorderViewModel.WpfWidth = DesignBorderViewModel.WpfWidth;
            //        BorderViewModel.WpfHeight = DesignBorderViewModel.WpfHeight;

            //        dialogService.Show(BorderViewModel);
            //    }

            //}
            //else             {
            //    dialogService.Hide(BorderViewModel);
            //    if (IsBorderVisible)
            //    {
            //        dialogService.Show(DesignBorderViewModel);
            //    }
            //}

            VideoViewModel.OnStreamStateChanged(isStarted);

            MainViewModel.OnAnyStreamStateChanged();

			if (isStarted)
			{
				if (VideoViewModel.ShowCaptureBorder && VideoViewModel.IsScreenSource)
				{
					borderForm = new RegionForm();
					borderForm.ShowBorder(VideoViewModel.CaptureRect);

				}

				if (VideoViewModel.ShowDebugInfo && VideoViewModel.IsDebugScreenSource)
				{
					if (statisticForm == null)
					{
						statisticForm = new StatisticForm();
					}
					statisticForm.Location = VideoViewModel.CaptureRect.Location;
					statisticForm.Start(MediaStreamer.Statistics);
				}

			}
            else
            {
                if (borderForm != null)
                {
                    borderForm.Close();
                    borderForm = null;
                }


				if (statisticForm != null)
				{
					statisticForm.Stop();
					statisticForm.Visible = false;
				}
			}

        }

        private void ShowError(string caption, string message)
        {
            var dialogService = ServiceLocator.GetInstance<IDialogService>();

            var view = new MessageBoxViewModel(message, caption, System.Windows.MessageBoxImage.Error);

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

            if (MediaStreamer != null)
            {
                MediaStreamer.Close();           
            }

            //if (borderForm != null)
            //{
            //    borderForm.Close();
            //}

        }
    }
}
