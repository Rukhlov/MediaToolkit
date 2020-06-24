using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace ScreenStreamer.Wpf.Common.ContentTemplateSelectors
{
    public class FormContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AudioSettingsTemplate { get; set; }
        public DataTemplate VideoSettingsTemplate { get; set; }
        public DataTemplate NetworkSettingsTemplate { get; set; }
        public DataTemplate StreamMainTemplate { get; set; }
        public DataTemplate DeleteTemplate { get; set; }
        public DataTemplate AdvancedSettingsTemplate { get; set; }
        public DataTemplate CursorSettingsTemplate { get; set; }
        public DataTemplate QualitySettingsTemplate { get; set; }
        public DataTemplate BorderSettingsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;
            if (item is AudioSettingsViewModel) return AudioSettingsTemplate;
            if (item is MainViewModel) return StreamMainTemplate;
            if (item is DeleteViewModel) return DeleteTemplate;
            if (item is VideoSettingsViewModel) return VideoSettingsTemplate;
            if (item is NetworkSettingsViewModel) return NetworkSettingsTemplate;
            if (item is AdvancedSettingsViewModel) return AdvancedSettingsTemplate;
            if (item is CursorSettingsViewModel) return CursorSettingsTemplate;
            if (item is QualitySettingsViewModel) return QualitySettingsTemplate;
            if (item is BorderSettingsViewModel) return BorderSettingsTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
