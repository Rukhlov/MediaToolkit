using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Views;

namespace ScreenStreamer.Wpf.Common.Interfaces
{
    public interface IDialogService
    {
        void Hide(IDialogViewModel viewModel);
        void Show(IDialogViewModel viewModel);
        void Handle(bool isVisible, IDialogViewModel viewModel);

        bool? ShowDialog(IWindowViewModel parent, IDialogViewModel model);

        void Register(IDialogViewModel viewModel, MainWindow mainWindow);

        void CloseAll();

		void Activate();

		void Close(IDialogViewModel viewModel);

		void Close(BorderViewModel viewModel);

		void Close(DesignBorderViewModel viewModel);
	}
}
