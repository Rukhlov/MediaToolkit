using Prism.Commands;

using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.ViewModels.Common;
using ScreenStreamer.Wpf.ViewModels.Dialogs;
using System;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.ViewModels.Properties
{
    public abstract class PropertyBaseViewModel : TrackableViewModel
    {
        protected readonly int MaxInfoLength = 60;

        public StreamViewModel Parent { get; }

        public virtual string Name { get; } = "N/A";

        public virtual string Info { get; set; } = string.Empty;

        public ICommand ShowSettingsCommand { get; set; }

        protected IDialogService DialogService { get; private set; }

        public PropertyBaseViewModel(StreamViewModel parent) : base(parent)
        {
            Parent = parent;
            DialogService = ServiceLocator.GetInstance<IDialogService>();
            DialogViewModel = BuildDialogViewModel();
            ShowSettingsCommand = new DelegateCommand<WindowViewModel>(ShowSettings);
        }

        protected void ShowSettings(WindowViewModel parent)
        {
            DialogService.Show(DialogViewModel);
        }

        protected IDialogViewModel DialogViewModel { get; }

        protected abstract IDialogViewModel BuildDialogViewModel();



        //public abstract object Clone();
    }
}
