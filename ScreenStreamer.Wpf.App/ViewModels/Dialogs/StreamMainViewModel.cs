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

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class StreamMainViewModel : BaseWindowViewModel
    {
        private readonly StreamMainModel _model;

        #region IsEdit

        private bool _isEdit = false;
        public bool IsEdit { get => _isEdit; set { SetProperty(ref _isEdit, value); _selectedStream.IsEditName = false; } }

        #endregion IsEdit

        #region SeletectedStream

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

                SetProperty(ref _selectedStream, value);

                if (_selectedStream != null)
                {
                    _selectedStream.IsSelected = true;
                }
            }
        }

        #endregion SeletectedStream

        public bool HasNoStreams => StreamList.Count == 0;

        #region Commands

        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ShowMainWindowCommand { get; set; }
        public ICommand HideMainWindowCommand { get; set; }
        public ICommand ActivateMainWindowCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        public ICommand StopAllCommand { get; set; }
        public ICommand StartAllCommand { get; set; }
        public ICommand ShowStreamSettingsCommand { get; set; }

        #endregion Commands

        public ObservableCollection<StreamViewModel> StreamList { get; set; } = new ObservableCollection<StreamViewModel>();

        public bool IsAllStarted => StreamList.All(s => s.IsStarted);

        public StreamMainViewModel(StreamMainModel model)
        {
            _model = model;
            StreamList = new ObservableCollection<StreamViewModel>(_model.StreamList.Select(m => new StreamViewModel(this, true, m)));
            this.IsBottomVisible = false;
            this.DeleteCommand = new DelegateCommand(Delete, () => StreamList.Count > 1);
            this.AddCommand = new DelegateCommand(Add);
            this.ShowMainWindowCommand = new DelegateCommand(ShowMainWindow);
            this.HideMainWindowCommand = new DelegateCommand(HideMainWindow);
            this.ActivateMainWindowCommand = new DelegateCommand(Activate);
            this.ExitCommand = new DelegateCommand(Exit);
            this.StartAllCommand = new DelegateCommand(StartAll);
            this.StopAllCommand = new DelegateCommand(StopAll);
            this.ShowStreamSettingsCommand = new DelegateCommand<StreamViewModel>(ShowStreamSettings);

        }

        private void Delete()
        {
            var dialogService = DependencyInjectionHelper.Container.Resolve<IDialogService>();
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

                StreamList.Remove(streamToDelete);
                this._model.StreamList.Remove(streamToDelete.Model);
                IsEdit = false;
                SelectedStream = null;

                RaisePropertyChanged(nameof(HasNoStreams));
                RaisePropertyChanged(nameof(IsAllStarted));
                (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void Add()
        {
            var model = new StreamModel { Name = $"Stream {this.StreamList.Count + 1}" };

            var streamViewModel = new StreamViewModel(this, true, model);

            this.StreamList.Add(streamViewModel);
            this._model.StreamList.Add(model);
            SelectedStream = streamViewModel;

            RaisePropertyChanged(nameof(HasNoStreams));
            RaisePropertyChanged(nameof(IsAllStarted));
            (DeleteCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        #region Show/Hide
        private void ShowMainWindow()
        {
            IsVisible = true;
        }

        private void HideMainWindow()
        {
            IsVisible = false;
        }
        #endregion Show/Hide

        public void RaiseIsAllStartedChanged()
        {
            this.RaisePropertyChanged(nameof(IsAllStarted));
        }

        private void Activate()
        {
            DependencyInjectionHelper.Container.Resolve<IDialogService>().Activate();
        }

        private void Exit()
        {
            DependencyInjectionHelper.Container.Resolve<IDialogService>().CloseAll();
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

            if (!IsVisible) ShowMainWindow();

            Activate();

            SelectedStream = streamViewModel;

            IsEdit = true;
        }
    }

}
