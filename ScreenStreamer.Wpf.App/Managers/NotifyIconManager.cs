using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenStreamer.Wpf.Managers
{

    public class NotifyIcon
    {
        private Hardcodet.Wpf.TaskbarNotification.TaskbarIcon taskbarIcon = null;
        public NotifyIcon(ViewModels.Dialogs.MainViewModel mainViewModel)
        {
            ResourceDictionary resource = new ResourceDictionary
            {
                Source = new Uri(@"pack://application:,,,/ScreenStreamer.Wpf.App;component/Templates/NotifyIconTemplate.xaml"),
            };

            taskbarIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)resource["notifyIcon"];
            //taskbarIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)Application.Current.FindResource("notifyIcon");

            //taskbarIcon.TrayRightMouseDown += NotifyIcon_TrayRightMouseDown;
            taskbarIcon.TrayLeftMouseDown += NotifyIcon_TrayLeftMouseDown;
            taskbarIcon.DataContext = mainViewModel;
        }

        private void NotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            var dataContext = taskbarIcon.DataContext;
            if (dataContext != null)
            {
                var mainViewModel = dataContext as ViewModels.Dialogs.MainViewModel;
                if (mainViewModel != null)
                {
                    mainViewModel.ShowMainWindowCommand.Execute(null);
                }
            }

        }

        private void NotifyIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            var dataContext = taskbarIcon.DataContext;
            if (dataContext != null)
            {
                var mainViewModel = dataContext as ViewModels.Dialogs.MainViewModel;
                if (mainViewModel != null)
                {
                    mainViewModel.ActivateMainWindowCommand.Execute(null);
                }
            }

        }

        public void Dispose()
        {
            if (taskbarIcon != null)
            {
                //taskbarIcon.TrayRightMouseDown -= NotifyIcon_TrayRightMouseDown;
                taskbarIcon.TrayLeftMouseDown -= NotifyIcon_TrayLeftMouseDown;

                taskbarIcon.Dispose();
                taskbarIcon = null;
            }
        }
    }
}
