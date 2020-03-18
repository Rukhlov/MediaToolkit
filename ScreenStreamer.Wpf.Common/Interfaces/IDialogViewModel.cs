namespace ScreenStreamer.Wpf.Common.Interfaces
{
    public interface IDialogViewModel
    {
    }

    public interface IWindowViewModel : IDialogViewModel
    {
        bool IsModalOpened { get; set; }

        bool IsVisible { get; set; }

        bool IsClosableOnLostFocus { get; }
    }
}
