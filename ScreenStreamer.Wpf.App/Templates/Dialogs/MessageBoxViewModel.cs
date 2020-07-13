using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class MessageBoxViewModel : BaseWindowViewModel
    {

        public string Title { get; set; }
        public string DialogText { get; set; }

        //public MessageBoxResult DialogResult { get; private set; }

        public string OkButtonText { get; set; } = "OK";
        public string CancelButtonText { get; set; } = "Cancel";

        public bool IsCancelVisible => (messageBoxButton != MessageBoxButton.OK);

        public override string Caption => Title;

        public override double MinWidth => 300d;

        public override bool IsBottomVisible => false;

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }


        private TrackableViewModel parentViewModel = null;
        private MessageBoxButton messageBoxButton = MessageBoxButton.OK;

        public MessageBoxViewModel(string message, string title, MessageBoxButton button, MessageBoxImage image, TrackableViewModel parent) : base(parent)
        {
            this.parentViewModel = parent;
            this.messageBoxButton = button;

            if (iconDict.ContainsKey(image))
            {
                base.captionImage = iconDict[image];
            }
            else
            {
                base.captionImage = iconDict[0];
            }

            if (messageBoxButton == MessageBoxButton.OK)
            {
                OkButtonText = "OK";
            }
            else if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                OkButtonText = "OK";
                CancelButtonText = "Cancel";
            }
            else if (messageBoxButton == MessageBoxButton.YesNo)
            {
                OkButtonText = "Yes";
                CancelButtonText = "No";
            }

            this.OkCommand = new DelegateCommand<Window>(SetDialogResult);

            this.DialogText = message;
            this.Title = title;

        }

        public MessageBoxViewModel(TrackableViewModel parent) : this("", "", MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, TrackableViewModel parent) : this(message, "", MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, string title, MessageBoxButton button, TrackableViewModel parent) : this(message, title, button, MessageBoxImage.None, parent)
        { }

        public MessageBoxViewModel(string message, string title, MessageBoxImage image, TrackableViewModel parent) : this(message, title, MessageBoxButton.OK, image, parent)
        { }

        public MessageBoxViewModel(string message, string title, TrackableViewModel parent) : this(message, title, MessageBoxButton.OK, MessageBoxImage.None, parent)
        { }

        private void SetDialogResult(Window selfWindow)
        {

            selfWindow.DialogResult = true;
            selfWindow.Close();
        }


        private static Dictionary<MessageBoxImage, BitmapImage> iconDict = new Dictionary<MessageBoxImage, BitmapImage>
        {
            { MessageBoxImage.Information, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Info_32x32.png")) },
            { MessageBoxImage.Warning, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Warn_32x32.png")) },
            { MessageBoxImage.Error, new BitmapImage(new Uri("pack://application:,,,/ScreenStreamer.Wpf.App;Component/Icons/Error_32x32.png")) },
        };
    }
}
