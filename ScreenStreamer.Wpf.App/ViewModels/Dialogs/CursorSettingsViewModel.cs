using ScreenStreamer.Wpf.ViewModels.Common;
using ScreenStreamer.Wpf.ViewModels.Properties;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public class CursorSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Cursor";

        public CursorSettingsViewModel(PropertyCursorViewModel property, TrackableViewModel parent) : base(property, parent)
        {
        }
    }
}
