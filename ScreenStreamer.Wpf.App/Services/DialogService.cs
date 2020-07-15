using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using ScreenStreamer.Wpf.Common.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ScreenStreamer.Wpf.Common.Services
{
    internal class DialogService : IDialogService
    {
        private IDictionary<BorderViewModel, StreamBorderWindow> _streamBorders = new Dictionary<BorderViewModel, StreamBorderWindow>();
        private IDictionary<DesignBorderViewModel, DesignBorderWindow> _designBorders = new Dictionary<DesignBorderViewModel, DesignBorderWindow>();

        private IDictionary<IDialogViewModel, MainWindow> _windows = new Dictionary<IDialogViewModel, MainWindow>();
        private IDictionary<IDialogViewModel, MainWindow> _dialogs = new Dictionary<IDialogViewModel, MainWindow>();

        public void Handle(bool isVisible, IDialogViewModel viewModel)
        {
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

        public void Register(IDialogViewModel viewModel, MainWindow mainWindow)
        {
            _windows[viewModel] = mainWindow;
        }

        public void Show(IDialogViewModel viewModel)
        {

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

                _designBorders[designBorderViewModel].Show();
            }
            else
            {
                var setInitialPosition = true;

                if (!_windows.ContainsKey(viewModel))
                {
                    _windows[viewModel] = new MainWindow(viewModel);
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

        public void ShowDialog(IDialogViewModel viewModel)
        {
            throw new NotImplementedException();
        }

        public bool? ShowDialog(IWindowViewModel parent, IDialogViewModel model)
        {
            System.Windows.Window parentWindow = null;

            if (parent != null)
            {
                parent.IsModalOpened = true;
                if (_windows.ContainsKey(parent))
                {
                    parentWindow = _windows[parent];
                }

            }
            var dialogWindow = new MainWindow(model);
            // _dialogs[model] = new MainWindow(model);

            if (parentWindow != null)
            {
                dialogWindow.Owner = parentWindow;
                dialogWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
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
            //0. Close dialog windows
            foreach (var w in _dialogs) w.Value.Close();

            //1.Close design borders
            foreach (var b in _designBorders) b.Value.Close();

            //2.Close stream borders
            foreach (var b in _streamBorders) b.Value.Close();

            //3. Close windows except main
            foreach (var w in _windows) if (!(w.Key is MainViewModel)) w.Value.Close();

            //4. Close main window
            foreach (var w in _windows) if (w.Key is MainViewModel) w.Value.Close();
        }

        public void Close(IDialogViewModel viewModel)
        {
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
            if (_streamBorders.ContainsKey(viewModel))
            {
                _streamBorders[viewModel].Close();
                _streamBorders.Remove(viewModel);
            }
        }

        public void Close(DesignBorderViewModel viewModel)
        {
            if (_designBorders.ContainsKey(viewModel))
            {
                _designBorders[viewModel].Close();
                _designBorders.Remove(viewModel);
            }
        }

        public void Activate()
        {
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
