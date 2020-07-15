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
            var args = e.Args;

            var config = ConfigManager.LoadConfigurations();
            //TODO: validate...

            ServiceLocator.RegisterSingleton(config);

            ServiceLocator.RegisterSingleton(GalaSoft.MvvmLight.Messaging.Messenger.Default); //х.з зачем это...

            var dialogService = new Common.Services.DialogService();
            ServiceLocator.RegisterSingleton<Common.Interfaces.IDialogService>(dialogService);


            //var mainView = new Common.Models.Dialogs.StreamMainViewModel(config);
            //Common.Views.MainWindow mainWindow = new Common.Views.MainWindow
            //{
            //    DataContext = mainView,
            //};

            //mainWindow.InitializeComponent();

            //dialogService.Register(mainView, mainWindow);

            //dialogService.Show(mainView);


            base.OnStartup(e);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            //ConfigurationManager.Save();
            base.OnExit(e);
        }
    }
}
