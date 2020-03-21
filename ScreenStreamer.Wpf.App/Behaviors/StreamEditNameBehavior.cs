using ScreenStreamer.Wpf.Common.Models;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Common.Behaviors
{
    public class StreamEditNameBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;

            DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsReadOnlyProperty, typeof(TextBox))
                                        .AddValueChanged(AssociatedObject, OnReadOnlyChanged);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            DependencyPropertyDescriptor.FromProperty(TextBoxBase.IsReadOnlyProperty, typeof(TextBox))
                                        .RemoveValueChanged(AssociatedObject, OnReadOnlyChanged);
            base.OnDetaching();
        }

        private void OnReadOnlyChanged(object sender, EventArgs e)
        {
            if ((sender as TextBox)?.IsReadOnly == false)
            {
                ((TextBox)sender).Tag = this;
                ((TextBox)sender).Focus();
                ((TextBox)sender).SelectAll();
            }
            else if ((sender as TextBox)?.IsReadOnly == true)
            {
                ((TextBox)sender).SelectionLength = 0;
            }
        }

        private void AssociatedObject_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                if (textBox.Tag == null)
                {
                    var streamViewModel = textBox.DataContext as StreamViewModel;
                    streamViewModel.IsEditName = false;
                }
                textBox.Tag = null;
            }
        }
    }
}
