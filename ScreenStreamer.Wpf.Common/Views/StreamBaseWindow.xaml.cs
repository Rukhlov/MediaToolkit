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
using GalaSoft.MvvmLight.Messaging;
using Unity;
using NLog;

namespace ScreenStreamer.Wpf.Common.Views
{
    /// <summary>
    /// Interaction logic for FormBaseView.xaml
    /// </summary>
    public partial class StreamBaseWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public ICommand CloseCommand { get; set; }

        //For debug\
        public StreamBaseWindow()
        {
            var model = DependencyInjectionHelper.Container.Resolve<StreamMainModel>();
            var vm = new StreamMainViewModel(model);
            this.DataContext = vm;
            CloseCommand = new DelegateCommand(HandleClose);
            InitializeComponent();

            DependencyInjectionHelper.Container.Resolve<IDialogService>().Register(vm, this);
            var messenger = DependencyInjectionHelper.Container.Resolve<IMessenger>();
            messenger.Unregister<AcceptChangesMessage>(this);
            messenger.Register<AcceptChangesMessage>(this, VmOnPropertyChanged);

            ApplyInitialState();
        }

        private void VmOnPropertyChanged(AcceptChangesMessage obj)
        {
            //TODO Process changes
            logger.Debug("VmOnPropertyChanged(...) " + obj);


        }

        private void ApplyInitialState()
        {
            //TODO apply initial state

            logger.Debug("ApplyInitialState()");

        }

        public StreamBaseWindow(IDialogViewModel viewModel)
        {
            this.DataContext = viewModel;
            CloseCommand = new DelegateCommand(HandleClose);

            InitializeComponent();
        }

        private void HandleClose()
        {
            if (this.DataContext is IDialogViewModel dialogViewModel)
            {
                DependencyInjectionHelper.Container.Resolve<IDialogService>().Hide(dialogViewModel);
            }
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
            base.OnClosing(e);
        }
    }

    public class AcceptChangesMessage
    {
        public CustomBindableBase Model { get; set; }
        public string ChangedProperty { get; set; }
    }
}
