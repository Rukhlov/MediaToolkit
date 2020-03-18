using ScreenStreamer.Wpf.Common.Models;
using System;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Common.Interfaces
{
    public interface IStreamPropertyBaseViewModel : ICloneable
    {
        StreamViewModel Parent { get; set; }

        string Name { get; set; }

        string Info { get; set; }

        ICommand ShowSettingsCommand { get; set; }
        
        void ShowSettings();
    }
}
