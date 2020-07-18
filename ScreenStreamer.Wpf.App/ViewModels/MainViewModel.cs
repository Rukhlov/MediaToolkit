using Prism.Commands;
using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ScreenStreamer.Wpf.Managers;

using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using ScreenStreamer.Wpf.Models;
using ScreenStreamer.Wpf.ViewModels.Common;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class MainViewModel : BaseWindowViewModel
    {
        private readonly AppModel appModel;

        public MainViewModel(AppModel model) : base(null)
        {
            appModel = model;

            StreamList = new ObservableCollection<StreamViewModel>(appModel.StreamList.Select(streamModel => new StreamViewModel(this, true, streamModel)));
            this.IsBottomVisible = false;

            this.AddCommand = new DelegateCommand(OnAddStream);
            this.DeleteCommand = new DelegateCommand(OnDeleteStream, () => StreamList.Count > 1);
            
            this.ShowMainWindowCommand = new DelegateCommand(OnShowMainWindow);
            this.HideMainWindowCommand = new DelegateCommand(OnHideMainWindow);
            this.ActivateMainWindowCommand = new DelegateCommand(OnActivateMainWindow);
            this.ExitCommand = new DelegateCommand(OnExit);
            this.StartAllCommand = new DelegateCommand(OnStartAll);
            this.StopAllCommand = new DelegateCommand(OnStopAll);
            this.ShowStreamSettingsCommand = new DelegateCommand<StreamViewModel>(OnShowStreamSettings);

            wndProcService = new Services.WndProcService();

            wndProcService.ShowMainWindow += OnShowMainWindow;
            wndProcService.Init();
        }

        public override string Caption
        {
            get => "Polywall Streamer " + AppModel.AppVersion;
        }

        private Services.WndProcService wndProcService = null;

        private bool isEdit = false;
        public bool IsEdit
        {
            get => isEdit;
            set
            {
                isEdit = value;
                RaisePropertyChanged(() => IsEdit);
                selectedStream.IsEditName = false;
            }
        }

        private StreamViewModel selectedStream = null;
        public StreamViewModel SelectedStream
        {
            get
            {
                if (selectedStream == null)
                {
                    selectedStream = StreamList.FirstOrDefault();
                    if (selectedStream != null)
                    {
                        selectedStream.IsSelected = true;
                    }
                }
                return selectedStream;
            }
            set
            {
                if (selectedStream != value &&
                    selectedStream != null)
                {
                    selectedStream.IsSelected = false;
                    selectedStream.IsEditName = false;
                }
                selectedStream = value;
                RaisePropertyChanged(() => SelectedStream);

                if (selectedStream != null)
                {
                    selectedStream.IsSelected = true;
                }
            }
        }

        public ObservableCollection<StreamViewModel> StreamList { get; set; } = new ObservableCollection<StreamViewModel>();

        public bool HasMaxStreamsLimit => (StreamList.Count >= appModel.MaxStreamCount);
        public bool HasNoStreams => StreamList.Count == 0;

        public bool IsAllStarted => StreamList.All(s => s.IsStarted);
        public bool IsAnyStarted => StreamList.Any(s => s.IsStarted);

        public BitmapImage ActiveIcon
        {
            get
            {
                return IsAnyStarted ? iconDict["StreamOn"] : iconDict["TrayLogo"];
            }
        }


        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ShowMainWindowCommand { get; set; }
        public ICommand HideMainWindowCommand { get; set; }
        public ICommand ActivateMainWindowCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand StopAllCommand { get; set; }
        public ICommand StartAllCommand { get; set; }
        public ICommand ShowStreamSettingsCommand { get; set; }



        private void OnAddStream()
        {
            var model = new MediaStreamModel
            {
                Name = $"{Environment.MachineName} (Stream {this.StreamList.Count + 1})"
            };

            var streamViewModel = new StreamViewModel(this, true, model);


            this.StreamList.Add(streamViewModel);
            this.appModel.StreamList.Add(model);
            SelectedStream = streamViewModel;

            RaisePropertyChanged(nameof(HasMaxStreamsLimit));
            RaisePropertyChanged(nameof(HasNoStreams));
            RaisePropertyChanged(nameof(IsAllStarted));
            (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private void OnDeleteStream()
        {
            var dialogService = ServiceLocator.GetInstance<IDialogService>();

            var message = $"Are you sure want to delete '{SelectedStream.Name}'?";
            var caption = "Delete";
            var messageBoxView = new MessageBoxViewModel(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            //var deleteViewModel = new DeleteViewModel(SelectedStream);
            //if (dialogService.ShowDialog(this, deleteViewModel) == true)
           
            if (dialogService.ShowDialog(this, messageBoxView) == true)
            {
                if (SelectedStream.IsStarted)
                {
                    SelectedStream.StartCommand.Execute(null);
                }

                dialogService.Close(SelectedStream.BorderViewModel);
                dialogService.Close(SelectedStream.DesignBorderViewModel);
                dialogService.Close(SelectedStream.AdvancedSettingsViewModel);

                SelectedStream.Close();

                StreamList.Remove(SelectedStream);
                this.appModel.StreamList.Remove(SelectedStream.MediaStreamer);

                IsEdit = false;
                SelectedStream = null;

                RaisePropertyChanged(nameof(HasMaxStreamsLimit));

                RaisePropertyChanged(nameof(HasNoStreams));
                RaisePropertyChanged(nameof(IsAllStarted));


                (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }


        private void OnShowMainWindow()
        {
            IsVisible = true;
            this.RaisePropertyChanged(nameof(ActiveIcon));
        }

        private void OnHideMainWindow()
        {
            IsVisible = false;

            this.RaisePropertyChanged(nameof(ActiveIcon));
        }


        public void RaiseIsAllStartedChanged()
        {
            this.RaisePropertyChanged(nameof(IsAllStarted));


        }

        public void OnStreamStateChanged()
        {
            RaisePropertyChanged(nameof(IsAnyStarted));

            RaisePropertyChanged(nameof(ActiveIcon));
        }

        private void OnActivateMainWindow()
        {
            ServiceLocator.GetInstance<IDialogService>().Activate();
        }


        private void OnStartAll()
        {
            foreach (var s in StreamList)
            {
                //s.IsStarted = true;
                s.StartCommand.Execute(null);
            }
            RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void OnStopAll()
        {
            foreach (var s in StreamList)
            {
                // s.IsStarted = false;
                s.StartCommand.Execute(null);
            }

            RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void OnShowStreamSettings(StreamViewModel streamViewModel)
        {
			//var mainWindow = Application.Current.MainWindow;
			//var dialogWindows = Application.Current
			//                            .Windows.Cast<Window>()
			//                            .Where(w => w is StreamBaseWindow && w != mainWindow);

			////Close child dialogs
			//foreach (var w in dialogWindows) w.Close();

			if (!IsVisible)
			{
				OnShowMainWindow();
			}

            OnActivateMainWindow();

            SelectedStream = streamViewModel;

            IsEdit = true;
        }

        private void OnExit()
        {
            ServiceLocator.GetInstance<IDialogService>().CloseAll();
        }


        private static Dictionary<string, BitmapImage> iconDict = new Dictionary<string, BitmapImage>
        {
            { "TrayLogo", new BitmapImage( new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/tray_logo.ico")) },
            { "StreamOff", new BitmapImage( new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/streamoff.ico")) },
            { "StreamOn", new BitmapImage( new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/streamon.ico")) },

        };
    }

}
