
using ScreenStreamer.Wpf.Interfaces;
using System.Reflection;

using ScreenStreamer.Wpf.Managers;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using Prism.Commands;
using ScreenStreamer.Wpf.ViewModels.Dialogs;

namespace ScreenStreamer.Wpf.ViewModels.Common
{
    public class WindowViewModel : TrackableViewModel, IWindowViewModel
    {
        private readonly TrackableViewModel parent;

        public WindowViewModel(TrackableViewModel parent) :base(parent)
        {
            this.parent = parent;
            DiscardChangesCommand = new DelegateCommand(this.ResetChanges);
            AcceptChangesCommand = new DelegateCommand(this.AcceptChanges);
        }


        public virtual string Caption { get; set; } = ""; //"Polywall Streamer " + Assembly.GetExecutingAssembly().GetName().Version;
        public ICommand DiscardChangesCommand { get; private set; }
        public ICommand AcceptChangesCommand { get; private set; }

        protected BitmapImage captionImage = null;
        public virtual BitmapImage CaptionImage => captionImage;

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



        public virtual bool IsClosableOnLostFocus => this is PropertyWindowViewModel && !IsChanged;

        public void AcceptChanges()
        {
            parent.TrackNestedProperties(true);
            TrackNestedProperties(true);
            RaisePropertyChanged(() => IsChanged);


            ConfigManager.Save();
        }


    }
}
