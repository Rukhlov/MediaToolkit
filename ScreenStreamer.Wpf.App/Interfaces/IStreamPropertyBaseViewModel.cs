using System;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Interfaces
{
    public interface IStreamPropertyBaseViewModel : ICloneable
    {
        ViewModels.StreamViewModel Parent { get; set; }

        string Name { get; set; }

        string Info { get; set; }

        ICommand ShowSettingsCommand { get; set; }
        
        void ShowSettings();
    }
}
