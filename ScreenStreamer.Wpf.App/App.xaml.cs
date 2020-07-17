using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;
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
        private static Logger logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Debug("OnStartup(...) " + string.Join(" ", e.Args));

            var args = e.Args;

            var config = ConfigManager.LoadConfigurations();
            //TODO: validate...

            if (!config.Validate())
            {
                //...
                // Reset config...

            }


            ServiceLocator.RegisterSingleton(config);

           // ServiceLocator.RegisterSingleton(GalaSoft.MvvmLight.Messaging.Messenger.Default); //х.з зачем это...

            var dialogService = new Common.Services.DialogService();
			ServiceLocator.RegisterSingleton<Common.Interfaces.IDialogService>(dialogService);


            var mainViewModel = new Common.Models.Dialogs.MainViewModel(config);

            Common.Views.MainWindow mainWindow = new Common.Views.MainWindow(mainViewModel);

            dialogService.Register(mainViewModel, mainWindow);

            dialogService.Show(mainViewModel);


            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {

            logger.Debug("OnExit(...) " + e.ApplicationExitCode);

			ConfigManager.Save();

			//ConfigurationManager.Save();
			base.OnExit(e);
        }
    }
}
