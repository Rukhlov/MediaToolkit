
using ScreenStreamer.Wpf.ViewModels.Dialogs;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Behaviors
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
            var target = (e.OriginalSource as FrameworkElement)?.DataContext as ViewModels.StreamViewModel;
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
