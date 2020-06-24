using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Common.Behaviors
{
    public class StreamSelectBehavior : Behavior<ItemsControl>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;

            base.OnDetaching();
        }

        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var list = (sender as ItemsControl)?.DataContext as MainViewModel;
            var target = (e.OriginalSource as FrameworkElement)?.DataContext as StreamViewModel;
            if (target != null &&
                list != null &&
                list.StreamList.Contains(target))
            {
                if (list.SelectedStream != target)
                {
                    list.SelectedStream = target;
                }
                list.IsEdit = true;
            }
        }
    }
}
