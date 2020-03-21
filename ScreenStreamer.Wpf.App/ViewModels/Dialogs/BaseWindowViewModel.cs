using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Interfaces;
using System.Reflection;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class BaseWindowViewModel : CustomBindableBase, IWindowViewModel
    {
        public virtual string Caption { get; set; } = "Polywall Streamer " + Assembly.GetExecutingAssembly().GetName().Version;

        public virtual double MinWidth => 370d;

        #region IsBottomVisible

        private bool _isBottomVisible = true;
        public virtual bool IsBottomVisible { get => _isBottomVisible; set { SetProperty(ref _isBottomVisible, value); } }

        #endregion IsBottomVisible

        #region IsModalOpened
        private bool _isModalOpened = false;
        public bool IsModalOpened { get => _isModalOpened; set { SetProperty(ref _isModalOpened, value); } }
        #endregion IsModalOpened

        #region IsVisible

        private bool _isVisible = true;
        public bool IsVisible { get => _isVisible; set { SetProperty(ref _isVisible, value); } }

        #endregion IsVisible

        public bool IsClosableOnLostFocus => this is PropertyWindowViewModel;
    }
}
