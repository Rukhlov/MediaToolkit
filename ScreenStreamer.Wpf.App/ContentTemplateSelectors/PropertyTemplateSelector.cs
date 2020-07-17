using ScreenStreamer.Wpf.ViewModels.Properties;
using System.Windows;
using System.Windows.Controls;

namespace ScreenStreamer.Wpf.ContentTemplateSelectors
{
    public class PropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PropertyAudioTemplate { get; set; }
        public DataTemplate PropertyQualityTemplate { get; set; }
        public DataTemplate PropertyVideoTemplate { get; set; }
        public DataTemplate PropertyNetworkTemplate { get; set; }
        public DataTemplate PropertyCursorTemplate { get; set; }
        public DataTemplate PropertyBorderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;
            if (item is PropertyAudioViewModel)return PropertyAudioTemplate;
            if (item is PropertyQualityViewModel)return PropertyQualityTemplate;
            if (item is PropertyVideoViewModel)return PropertyVideoTemplate;
            if (item is PropertyNetworkViewModel) return PropertyNetworkTemplate;
            if (item is PropertyCursorViewModel)return PropertyCursorTemplate;
            if (item is PropertyBorderViewModel)return PropertyBorderTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
