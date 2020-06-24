using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Interfaces;
using System.Reflection;
using GalaSoft.MvvmLight.Command;
using Polywall.Share.UI;
using ScreenStreamer.Wpf.Common.Managers;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class BaseWindowViewModel : TrackableViewModel, IWindowViewModel
    {
        private readonly TrackableViewModel parent;

        public BaseWindowViewModel(TrackableViewModel parent) :base(parent)
        {
            this.parent = parent;
            DiscardChangesCommand = new RelayCommand(this.ResetChanges);
            AcceptChangesCommand = new RelayCommand(this.AcceptChanges);
        }

        public RelayCommand DiscardChangesCommand { get; private set; }
        public RelayCommand AcceptChangesCommand { get; private set; }
        public virtual string Caption { get; set; } = "Polywall Streamer " + Assembly.GetExecutingAssembly().GetName().Version;

        public virtual double MinWidth => 370d;


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



        public bool IsClosableOnLostFocus => this is PropertyWindowViewModel && !IsChanged;

       // public bool DontCloseOnLostFocus = false;

        //public bool IsClosableOnLostFocus
        //{
        //    get
        //    {
        //        bool isClosableOnLostFocus = false;
        //        if(this is PropertyWindowViewModel)
        //        {
        //            isClosableOnLostFocus = !IsChanged;
        //        }

        //        if (DontCloseOnLostFocus)
        //        {
        //            isClosableOnLostFocus = false;
        //        }

        //        return isClosableOnLostFocus;
        //    }
        //}

        public void AcceptChanges()
        {
            parent.TrackNestedProperties(true);
            TrackNestedProperties(true);
            RaisePropertyChanged(() => IsChanged);
            ConfigManager.Save();
        }


    }
}
