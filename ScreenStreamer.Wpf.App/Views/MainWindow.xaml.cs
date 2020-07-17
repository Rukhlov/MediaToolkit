//#define ENABLE_DEBUG_WINDOWS

using System.ComponentModel;
using Prism.Commands;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Input;
//using GalaSoft.MvvmLight.Messaging;

using System.Diagnostics;
using NLog;

namespace ScreenStreamer.Wpf.Common.Views
{
    /// <summary>
    /// Interaction logic for FormBaseView.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainWindow(IDialogViewModel viewModel)
        {
            InitializeComponent();

            this.DataContext = viewModel;
            CloseCommand = new DelegateCommand(HandleClose);
        }


        //For debug\
        public MainWindow()
        {
            var model = ServiceLocator.GetInstance<AppModel>();
            var vm = new MainViewModel(model);
            this.DataContext = vm;

            CloseCommand = new DelegateCommand(HandleClose);

            InitializeComponent();

            dialogService = ServiceLocator.GetInstance<IDialogService>();
            dialogService.Register(vm, this);


            //var messenger = ServiceLocator.GetInstance<IMessenger>();
            //messenger.Unregister<AcceptChangesMessage>(this);
            //messenger.Register<AcceptChangesMessage>(this, VmOnPropertyChanged);

            //ApplyInitialState();
        }

        private IDialogService dialogService = null;
        public ICommand CloseCommand { get; set; }


        private void HandleClose()
        {
            logger.Debug("HandleClose()");

            var dialogService = ServiceLocator.GetInstance<IDialogService>();
            if (dialogService != null)
            {
                if (DataContext != null)
                {
                    var viewModel = DataContext as IDialogViewModel;
                    if (viewModel != null)
                    {
                        dialogService.Hide(viewModel);
                    }
                }

            }

            //if (this.DataContext is IDialogViewModel dialogViewModel)
            //{
            //    ServiceLocator.GetInstance<IDialogService>().Hide(dialogViewModel);
            //}

            ////this.Hide();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                this.DragMove();
            }

#if ENABLE_DEBUG_WINDOWS
            if (e.ChangedButton == MouseButton.Right &&
                Keyboard.IsKeyDown(Key.C))
            {
                var contextMenu = new DebugContextMenu();
                contextMenu.DataContext = this.DataContext;
                contextMenu.Show();
            }

            if (e.ChangedButton == MouseButton.Right &&
                Keyboard.IsKeyDown(Key.B))
            {
                var streamViewModel = (this.DataContext as StreamMainViewModel)?.StreamList?.FirstOrDefault();
                if (streamViewModel != null)
                {
                    var border = new StreamBorderWindow()
                    {
                        DataContext = new StreamBorderViewModel(streamViewModel)
                    };
                    border.Show();
                }
            }

            if (e.ChangedButton == MouseButton.Right &&
                Keyboard.IsKeyDown(Key.D))
            {
                var streamViewModel = (this.DataContext as StreamMainViewModel)?.StreamList?.FirstOrDefault();
                if (streamViewModel != null)
                {
                    var border = new DesignBorderWindow()
                    {
                        DataContext = new DesignBorderViewModel(streamViewModel)
                    };
                    border.Show();
                }
            }
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            logger.Debug("OnClosing(...)");

            base.OnClosing(e);
        }


        //private void VmOnPropertyChanged(AcceptChangesMessage obj)
        //{
        //    //TODO Process changes
        //    Debug.WriteLine("VmOnPropertyChanged(...)");
        //}

        //private void ApplyInitialState()
        //{
        //    //TODO apply initial state

        //    Debug.WriteLine("ApplyInitialState()");
        //}

    }

    //public class AcceptChangesMessage
    //{
    //    public TrackableViewModel Model { get; set; }
    //    public string ChangedProperty { get; set; }
    //}
}
