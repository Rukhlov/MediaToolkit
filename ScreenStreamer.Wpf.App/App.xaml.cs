using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Managers;
using ScreenStreamer.Wpf.Common.Models;

namespace ScreenStreamer.Wpf.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            //...
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //ConfigurationManager.Save();
            base.OnExit(e);
        }
    }
}
