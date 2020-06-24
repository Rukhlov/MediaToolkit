using Prism.Commands;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ScreenStreamer.Wpf.Common.Managers;
using Unity;
using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class MainViewModel : BaseWindowViewModel
    {
        private readonly MainModel _model;


        private bool _isEdit = false;
        public bool IsEdit
        {
            get => _isEdit;
            set
            {
                _isEdit = value;
                RaisePropertyChanged(() => IsEdit);
                _selectedStream.IsEditName = false;
            }
        }

        private StreamViewModel _selectedStream = null;
        public StreamViewModel SelectedStream
        {
            get
            {
                if (_selectedStream == null)
                {
                    _selectedStream = StreamList.FirstOrDefault();
                    if (_selectedStream != null)
                    {
                        _selectedStream.IsSelected = true;
                    }
                }
                return _selectedStream;
            }
            set
            {
                if (_selectedStream != value &&
                    _selectedStream != null)
                {
                    _selectedStream.IsSelected = false;
                    _selectedStream.IsEditName = false;
                }
                _selectedStream = value;
                RaisePropertyChanged(() => SelectedStream);

                if (_selectedStream != null)
                {
                    _selectedStream.IsSelected = true;
                }
            }
        }

        public bool HasMaxStreamsLimit => (StreamList.Count >= _model.MaxStreamCount);
        public bool HasNoStreams => StreamList.Count == 0;



        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ShowMainWindowCommand { get; set; }
        public ICommand HideMainWindowCommand { get; set; }
        public ICommand ActivateMainWindowCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand StopAllCommand { get; set; }
        public ICommand StartAllCommand { get; set; }
        public ICommand ShowStreamSettingsCommand { get; set; }

       

        public ObservableCollection<StreamViewModel> StreamList { get; set; } = new ObservableCollection<StreamViewModel>();

        public bool IsAllStarted => StreamList.All(s => s.IsStarted);

        public MainViewModel(MainModel model) : base(null)
        {
            _model = model;
            StreamList = new ObservableCollection<StreamViewModel>(_model.StreamList.Select(m => new StreamViewModel(this, true, m)));
            this.IsBottomVisible = false;
            this.DeleteCommand = new DelegateCommand(OnDeleteStream, () => StreamList.Count > 1);
            this.AddCommand = new DelegateCommand(OnAddStream,
                () => 
                    {
                        return true;
                       //return StreamList.Count < _model.MaxStreamCount;
                    }
                );

            this.ShowMainWindowCommand = new DelegateCommand(ShowMainWindow);
            this.HideMainWindowCommand = new DelegateCommand(HideMainWindow);
            this.ActivateMainWindowCommand = new DelegateCommand(Activate);
            this.ExitCommand = new DelegateCommand(Exit);
            this.StartAllCommand = new DelegateCommand(StartAll);
            this.StopAllCommand = new DelegateCommand(StopAll);
            this.ShowStreamSettingsCommand = new DelegateCommand<StreamViewModel>(ShowStreamSettings);

            wndProcService = new App.Services.WndProcService();

            wndProcService.ShowMainWindow += ShowMainWindow;
            wndProcService.Init();
        }


        private App.Services.WndProcService wndProcService = null;

        private void OnDeleteStream()
        {
            var dialogService = ServiceLocator.GetInstance<IDialogService>();
            var streamToDelete = this.SelectedStream;
            var deleteViewModel = new DeleteViewModel(streamToDelete);
            if (dialogService.ShowDialog(this, deleteViewModel) == true)
            {
                if (streamToDelete.IsStarted)
                {
                    streamToDelete.StartCommand.Execute(null);
                }

                dialogService.Close(streamToDelete.BorderViewModel);
                dialogService.Close(streamToDelete.DesignViewModel);
                dialogService.Close(streamToDelete.AdvancedSettingsViewModel);

                streamToDelete.Close();

                StreamList.Remove(streamToDelete);
                this._model.StreamList.Remove(streamToDelete.Model);
                IsEdit = false;
                SelectedStream = null;

                RaisePropertyChanged(nameof(HasMaxStreamsLimit));

                RaisePropertyChanged(nameof(HasNoStreams));
                RaisePropertyChanged(nameof(IsAllStarted));
                (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void OnAddStream()
        {
            var model = new MediaStreamModel
            {
                Name = $"{Environment.MachineName} (Stream {this.StreamList.Count + 1})"
            };

            var streamViewModel = new StreamViewModel(this, true, model);


            this.StreamList.Add(streamViewModel);
            this._model.StreamList.Add(model);
            SelectedStream = streamViewModel;

            RaisePropertyChanged(nameof(HasMaxStreamsLimit));
            RaisePropertyChanged(nameof(HasNoStreams));
            RaisePropertyChanged(nameof(IsAllStarted));
            (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }


        private void ShowMainWindow()
        {
            IsVisible = true;
        }

        private void HideMainWindow()
        {
            IsVisible = false;
        }


        public void RaiseIsAllStartedChanged()
        {
            this.RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void Activate()
        {
            ServiceLocator.GetInstance<IDialogService>().Activate();
        }

        private void Exit()
        {
            ServiceLocator.GetInstance<IDialogService>().CloseAll();
        }

        private void StartAll()
        {
            foreach (var s in StreamList)
            {
                //s.IsStarted = true;
                s.StartCommand.Execute(null);
            }
            RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void StopAll()
        {
            foreach (var s in StreamList)
            {
                // s.IsStarted = false;
                s.StartCommand.Execute(null);
            }

            RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void ShowStreamSettings(StreamViewModel streamViewModel)
        {
			//var mainWindow = Application.Current.MainWindow;
			//var dialogWindows = Application.Current
			//                            .Windows.Cast<Window>()
			//                            .Where(w => w is StreamBaseWindow && w != mainWindow);

			////Close child dialogs
			//foreach (var w in dialogWindows) w.Close();

			if (!IsVisible)
			{
				ShowMainWindow();
			}

            Activate();

            SelectedStream = streamViewModel;

            IsEdit = true;
        }
    }

}
