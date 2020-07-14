using Prism.Commands;
using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System;
using System.Windows.Input;
using Unity;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public abstract class PropertyBaseViewModel : TrackableViewModel
    {
        protected readonly int MaxInfoLength = 60;

        public StreamViewModel Parent { get; set; }

        public virtual string Name { get; } = "N/A";

        public virtual string Info { get; set; } = string.Empty;

        public ICommand ShowSettingsCommand { get; set; }

        protected IDialogService DialogService { get; private set; }

        public PropertyBaseViewModel(StreamViewModel parent) : base(parent)
        {
            Parent = parent;
            DialogService = ServiceLocator.GetInstance<IDialogService>();
            DialogViewModel = BuildDialogViewModel();
            ShowSettingsCommand = new DelegateCommand<BaseWindowViewModel>(ShowSettings);
        }

        protected void ShowSettings(BaseWindowViewModel parent)
        {
            DialogService.Show(DialogViewModel);
        }

        protected IDialogViewModel DialogViewModel { get; }

        protected abstract IDialogViewModel BuildDialogViewModel();



        //public abstract object Clone();
    }
}
