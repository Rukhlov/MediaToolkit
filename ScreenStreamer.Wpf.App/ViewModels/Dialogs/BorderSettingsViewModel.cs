using ScreenStreamer.Wpf.ViewModels.Common;
using ScreenStreamer.Wpf.ViewModels.Properties;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class BorderSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Border";

        
        public BorderSettingsViewModel(PropertyBorderViewModel property, TrackableViewModel parent) : base(property, parent)
        {
        }
    }
}
