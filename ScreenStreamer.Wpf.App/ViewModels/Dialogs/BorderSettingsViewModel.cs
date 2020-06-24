using ScreenStreamer.Wpf.Common.Models.Properties;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class BorderSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Border";

        
        public BorderSettingsViewModel(PropertyBorderViewModel property, TrackableViewModel parent) : base(property, parent)
        {
        }
    }
}
