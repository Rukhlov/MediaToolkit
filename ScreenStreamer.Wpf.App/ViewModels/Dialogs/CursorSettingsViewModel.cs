using ScreenStreamer.Wpf.Common.Models.Properties;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class CursorSettingsViewModel : PropertyWindowViewModel
    {
        public override string Caption => "Cursor";

        public CursorSettingsViewModel(PropertyCursorViewModel property, StreamerViewModelBase parent) : base(property, parent)
        {
        }
    }
}
