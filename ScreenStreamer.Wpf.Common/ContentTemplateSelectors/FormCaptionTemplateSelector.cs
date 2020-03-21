﻿using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace ScreenStreamer.Wpf.Common.ContentTemplateSelectors
{
    public class FormCaptionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate StreamMainTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;
            if (item is StreamMainViewModel)
            {
                return StreamMainTemplate;
            }
            return DefaultTemplate;
        }
    }
}