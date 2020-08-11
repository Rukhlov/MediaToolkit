using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using ScreenStreamer.Wpf.Helpers;
using ScreenStreamer.Wpf.Managers;


namespace ScreenStreamer.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static SystemManager SystemMan { get; } = new SystemManager();

        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Debug("OnStartup(...) " + string.Join(" ", e.Args));

            var args = e.Args;

            var appModel = ConfigManager.LoadConfigurations();
            //TODO: validate...


            if (!appModel.Init())
            {
                //...
                // Reset config...

            }

           // SystemMan.Initialize();

            ServiceLocator.RegisterInstance(appModel);

           // ServiceLocator.RegisterSingleton(GalaSoft.MvvmLight.Messaging.Messenger.Default); //х.з зачем это...

            var dialogService = new Services.DialogService();
			ServiceLocator.RegisterInstance<Interfaces.IDialogService>(dialogService);

            var mainViewModel = new ViewModels.Dialogs.MainViewModel(appModel);

            Views.AppWindow mainWindow = new Views.AppWindow(mainViewModel)
            {
                Title = "Polywall Streamer",
            };

            dialogService.Register(mainViewModel, mainWindow);

            dialogService.Show(mainViewModel);


            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            logger.Debug("OnExit(...) " + e.ApplicationExitCode);

           
            ConfigManager.Save();

            //SystemMan.Shutdown();

            base.OnExit(e);
        }
    }
}
