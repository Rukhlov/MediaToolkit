using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public abstract class PropertyWindowViewModel : BaseWindowViewModel
    {
        public PropertyBaseViewModel Property { get; }

        public PropertyWindowViewModel(PropertyBaseViewModel property)
        {
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
        }
    }
}
