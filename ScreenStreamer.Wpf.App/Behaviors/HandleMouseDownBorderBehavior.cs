using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Behaviors
{
    public class HandleMouseDownBorderBehavior : Behavior<Border>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            base.OnDetaching();
        }
    }
}
