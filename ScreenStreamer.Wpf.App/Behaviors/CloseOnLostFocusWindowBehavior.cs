
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interactivity;

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
            Debug.WriteLine("AssociatedObject_Deactivated(...)");

            var windowViewModel = AssociatedObject.DataContext as IWindowViewModel;
            if (windowViewModel != null)
            {

                Debug.WriteLine(" windowViewModel.IsClosableOnLostFocus " + windowViewModel.IsClosableOnLostFocus);

                if (windowViewModel.IsClosableOnLostFocus)
                {
                    ServiceLocator.GetInstance<IDialogService>().Hide(windowViewModel);
                }
              
            }

            //if (AssociatedObject.DataContext is IWindowViewModel windowViewModel &&
            //        windowViewModel.IsClosableOnLostFocus)
            //{

            //    DependencyInjectionHelper.Container.Resolve<IDialogService>().Hide(windowViewModel);
            //}
        }
    }
}
