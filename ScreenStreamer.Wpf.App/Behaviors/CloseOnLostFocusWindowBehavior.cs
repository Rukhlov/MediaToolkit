using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using System.Windows;
using System.Windows.Interactivity;
using Unity;

namespace ScreenStreamer.Wpf.Common.Behaviors
{
    public class CloseOnLostFocusWindowBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Deactivated += AssociatedObject_Deactivated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Deactivated -= AssociatedObject_Deactivated;
            base.OnDetaching();
        }

        private void AssociatedObject_Deactivated(object sender, System.EventArgs e)
        {
            if (AssociatedObject.DataContext is IWindowViewModel windowViewModel &&
                    windowViewModel.IsClosableOnLostFocus)
            {

                DependencyInjectionHelper.Container.Resolve<IDialogService>().Hide(windowViewModel);
            }
        }
    }
}
