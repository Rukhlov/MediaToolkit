using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Interfaces;
using System.Reflection;
using GalaSoft.MvvmLight.Command;
using Polywall.Share.UI;
using ScreenStreamer.Wpf.Common.Managers;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class BaseWindowViewModel : StreamerViewModelBase, IWindowViewModel
    {
        private readonly StreamerViewModelBase parent;

        public BaseWindowViewModel(StreamerViewModelBase parent) :base(parent)
        {
            this.parent = parent;
            DiscardChangesCommand = new RelayCommand(this.ResetChanges);
            AcceptChangesCommand = new RelayCommand(this.AcceptChanges);
        }

        public RelayCommand DiscardChangesCommand { get; private set; }
        public RelayCommand AcceptChangesCommand { get; private set; }
        public virtual string Caption { get; set; } = "Polywall Streamer " + Assembly.GetExecutingAssembly().GetName().Version;

        public virtual double MinWidth => 370d;

        #region IsBottomVisible

        private bool _isBottomVisible = true;

        public virtual bool IsBottomVisible
        {
            get => _isBottomVisible;
            set
            {
                _isBottomVisible = value;
                RaisePropertyChanged(()=> IsBottomVisible);
            }
        }

        #endregion IsBottomVisible

        #region IsModalOpened
        private bool _isModalOpened = false;

        public bool IsModalOpened
        {
            get => _isModalOpened;
            set
            {
                _isModalOpened = value;
                RaisePropertyChanged(() => IsModalOpened);
            }
        }

        #endregion IsModalOpened

        #region IsVisible

        private bool _isVisible = true;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged(() => IsVisible);
            }
        }

        #endregion IsVisible

        public bool IsClosableOnLostFocus => this is PropertyWindowViewModel && !IsChanged;

        public void AcceptChanges()
        {
            parent.TrackNestedProperties(true);
            TrackNestedProperties(true);
            RaisePropertyChanged(() => IsChanged);
            ConfigurationManager.Save();
        }


    }
}
