using NLog;
using ScreenStreamer.Wpf.Interfaces;

using ScreenStreamer.Wpf.ViewModels.Dialogs;
using ScreenStreamer.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ScreenStreamer.Wpf.ViewModels;

namespace ScreenStreamer.Wpf
{
    internal class DialogService : IDialogService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDictionary<BorderViewModel, StreamBorderWindow> _streamBorders = new Dictionary<BorderViewModel, StreamBorderWindow>();
        private IDictionary<DesignBorderViewModel, DesignBorderWindow> _designBorders = new Dictionary<DesignBorderViewModel, DesignBorderWindow>();

        private IDictionary<IDialogViewModel, AppWindow> _windows = new Dictionary<IDialogViewModel, AppWindow>();
        private IDictionary<IDialogViewModel, AppWindow> _dialogs = new Dictionary<IDialogViewModel, AppWindow>();

        public void Handle(bool isVisible, IDialogViewModel viewModel)
        {
            logger.Debug("Handle(...) " + isVisible);

            if (isVisible)
            {
                Show(viewModel);
            }
            else
            {
                Hide(viewModel);
            }
        }

        public void Hide(IDialogViewModel viewModel)
        {
            logger.Debug("Hide(...)");

            if (viewModel is MainViewModel mainViewModel)
            {
                mainViewModel.IsVisible = false;
            }
            else
            {
                if (_windows.ContainsKey(viewModel))
                {
                    _windows[viewModel].Hide();
                }
                else if (viewModel is BorderViewModel streamBorderViewModel && _streamBorders.ContainsKey(streamBorderViewModel))
                {
                    _streamBorders[streamBorderViewModel].Hide();
                }
                else if (viewModel is DesignBorderViewModel designBorderViewModel && _designBorders.ContainsKey(designBorderViewModel))
                {
                    _designBorders[designBorderViewModel].Hide();
                }
                else if (_dialogs.ContainsKey(viewModel))
                {
                    _dialogs[viewModel].Close();
                }
            }
        }

        public void Register(IDialogViewModel viewModel, AppWindow window)
        {
            logger.Debug("Register(...)");
            if (_windows.ContainsKey(viewModel))
            {
                //...
            }

            _windows[viewModel] = window;
        }

        public void Show(IDialogViewModel viewModel)
        {
            logger.Debug("Show(...)");

            if (viewModel is BorderViewModel streamBorderViewModel)
            {
                if (!_streamBorders.ContainsKey(streamBorderViewModel))
                {
                    _streamBorders[streamBorderViewModel] = new StreamBorderWindow()
                    {
                        DataContext = streamBorderViewModel
                    };
                }
                _streamBorders[streamBorderViewModel].Show();
            }
            else if (viewModel is DesignBorderViewModel designBorderViewModel)
            {
                if (!_designBorders.ContainsKey(designBorderViewModel))
                {
                    _designBorders[designBorderViewModel] = new DesignBorderWindow()
                    {
                        DataContext = designBorderViewModel
                    };
                }

                var window = _designBorders[designBorderViewModel];

                // оставляем фокус на родительской форме
                // иначе при показе рамки, закрывается попап с настройками 
                window.ShowActivated = false;
                window.Show();

                //_designBorders[designBorderViewModel].Show();
            }
            else
            {
                var setInitialPosition = true;

                if (!_windows.ContainsKey(viewModel))
                {
                    _windows[viewModel] = new AppWindow(viewModel)
                    {
                        ShowInTaskbar = false,
                        
                    };

                    setInitialPosition = true;
                }
                if (viewModel is PropertyWindowViewModel propertyWindowViewModel &&
                    setInitialPosition)
                {
                    var propertyIndex = propertyWindowViewModel.Property.Parent.Properties.IndexOf(propertyWindowViewModel.Property);
                    if (propertyIndex < 0)
                    {
                        throw new InvalidOperationException("Property cannot be found");
                    }

                    var mainWindow = _windows.Single(w => w.Key is MainViewModel).Value;

                    _windows[viewModel].Top = mainWindow.Top + 116 + 30 * propertyIndex;
                    _windows[viewModel].Left = mainWindow.Width + mainWindow.Left;
                    _windows[viewModel].WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

                    foreach (var key in _windows.Keys.Where(model => model != viewModel && !(model is MainViewModel)))
                    {
                        Hide(key);
                    }
                }
                _windows[viewModel].Show();
            }
        }

		public bool? ShowDialog(IDialogViewModel viewModel)
		{

            return ShowDialog(null, viewModel);
		}

		public bool? ShowDialog(IWindowViewModel parent, IDialogViewModel model)
        {
            logger.Debug("ShowDialog(...)");

            System.Windows.Window parentWindow = null;

            if (parent != null)
            {
                parent.IsModalOpened = true;
                if (_windows.ContainsKey(parent))
                {
                    parentWindow = _windows[parent];
                }

            }
            var dialogWindow = new AppWindow(model)
            {
                ShowInTaskbar = false,
            };

            // _dialogs[model] = new MainWindow(model);

            if (parentWindow != null)
            {
                var handle = new System.Windows.Interop.WindowInteropHelper(parentWindow).Handle;
                var startupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                if (handle!= IntPtr.Zero)
                {
                    dialogWindow.Owner = parentWindow;
                    startupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                }

                dialogWindow.WindowStartupLocation = startupLocation;

            }
            _dialogs[model] = dialogWindow;

            var result = _dialogs[model].ShowDialog();
            _dialogs.Remove(model);

            if (parent != null)
            {
                parent.IsModalOpened = false;
            }
            return result;
        }


        public void CloseAll()
        {
            logger.Debug("CloseAll()");

            //0. Close dialog windows
            foreach (var w in _dialogs)
            {
                w.Value.Close();
            }
              

            //1.Close design borders
            foreach (var b in _designBorders)
            {
                b.Value.Close();
            }
               

            //2.Close stream borders
            foreach (var b in _streamBorders)
            {
                b.Value.Close();
            }    
            

            //3. Close windows except main
            foreach (var w in _windows)
            {
                if (!(w.Key is MainViewModel))
                {
                    w.Value.Close();
                }
            }

            //4. Close main window
            foreach (var w in _windows) 
            {
                if (w.Key is MainViewModel)
                {
                    w.Value.Close();
                }
            }
        }

        public void Close(IDialogViewModel viewModel)
        {
            logger.Debug("Close() IDialogViewModel");

            if (_windows.ContainsKey(viewModel))
            {
                _windows[viewModel].Close();
                _windows.Remove(viewModel);
            }
            if (_dialogs.ContainsKey(viewModel))
            {
                _dialogs[viewModel].Close();
                _dialogs.Remove(viewModel);
            }
        }

        public void Close(BorderViewModel viewModel)
        {
            logger.Debug("Close() BorderViewModel");

            if (_streamBorders.ContainsKey(viewModel))
            {
                _streamBorders[viewModel].Close();
                _streamBorders.Remove(viewModel);
            }
        }

        public void Close(DesignBorderViewModel viewModel)
        {
            logger.Debug("Close() DesignBorderViewModel");

            if (_designBorders.ContainsKey(viewModel))
            {
                _designBorders[viewModel].Close();
                _designBorders.Remove(viewModel);
            }
        }

        public void Activate()
        {
            logger.Debug("Activate()");

            var mainViewModel = _windows.Single(w => w.Key is MainViewModel);
            if ((mainViewModel.Key as MainViewModel).IsVisible)
            {
                //0. Get visible Dialog
                var visibleDialog = _dialogs.FirstOrDefault(d => d.Value.IsVisible);
                var visibleWindow = _windows.FirstOrDefault(w => w.Value.IsVisible && w.Key != mainViewModel.Key);
                if (visibleDialog.Value != null)
                {
                    visibleDialog.Value.Activate();
                }
                //1. get visible window
                else if (visibleWindow.Value != null)
                {
                    visibleWindow.Value.Activate();
                }
                //2. activate main
                else
                {
                    mainViewModel.Value.Activate();
                }
            }
        }


    }
}
