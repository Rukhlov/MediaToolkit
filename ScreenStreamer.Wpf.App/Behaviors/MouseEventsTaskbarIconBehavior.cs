//using Hardcodet.Wpf.TaskbarNotification;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Interactivity;

//namespace ScreenStreamer.Wpf.Common.Behaviors
//{
//    public class MouseEventsTaskbarIconBehavior : Behavior<TaskbarIcon>
//    {
//        protected override void OnAttached()
//        {
//            base.OnAttached();
//            AssociatedObject.TrayLeftMouseDown += AssociatedObject_TrayRightMouseDown;
//        }

//        private void AssociatedObject_TrayRightMouseDown(object sender, System.Windows.RoutedEventArgs e)
//        {
//        }

//        private void AssociatedObject_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
//        {
            
//        }

//        protected override void OnDetaching()
//        {
//            AssociatedObject.TrayRightMouseDown -= AssociatedObject_TrayRightMouseDown;

//            //AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
//            base.OnDetaching();
//        }
//    }
//}
