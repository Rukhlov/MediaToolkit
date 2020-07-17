using MediaToolkit;
using MediaToolkit.NativeAPIs;
using NLog;
using ScreenStreamer.Wpf.Common.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenStreamer.Wpf.UI
{
    public static class AppConsts
    {

        public const string ApplicationId = ApplicationName + "_" + "E7B28EAF-A330-4467-98F8-F3BCA7613268";

        public const string ApplicationName = "ScreenStreamerWPF";

    }

    public class Program
    {

        private static Logger logger = null;


        [STAThread]
        public static int Main(string[] args)
        {
            int exitCode = 0;

            logger = LogManager.GetCurrentClassLogger();

            logger.Info("========== START ============");

            bool createdNew = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, AppConsts.ApplicationId, out createdNew);
                if (!createdNew)
                {
                    logger.Info("Another instance is already running...");
					if (!AppModel.AllowMutipleInstance)
					{
						var res = Wpf.App.Services.WndProcService.ShowAnotherInstance();
						return 0;

						//return -1;
						//...
					}

				}

                if (args != null && args.Length > 0)
                {//...


                }

                MediaToolkitManager.Startup();

                Shcore.SetProcessPerMonitorDpiAwareness();
               
                var application = new App();
                application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
                application.InitializeComponent();

                logger.Info("============ RUN ===============");
                application.Run();

            }
            catch(Exception ex)
            {
                logger.Error(ex);
            }
            finally
            {
                if (mutex != null)
                {
                    if (createdNew)
                    {
                        mutex.ReleaseMutex();
                    }
                    mutex.Dispose();
                }

                MediaToolkitManager.Shutdown();

                //...
               //ConfigManager.Save();

                logger.Info("========== THE END ============");
            }

           
            return exitCode;
        }

        private static void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Exception ex = e.Exception;
			var message = "An unexpected error has occurred. Application will be closed";

            if (ex != null)
            {
				message = ex.Message;
			}

			logger.Fatal(message);

			try
			{
				var dialogService = new Common.Services.DialogService();

                Common.Models.Dialogs.MessageBoxViewModel vm = new Common.Models.Dialogs.MessageBoxViewModel(message, "Error", MessageBoxButton.OK);
				var result = dialogService.ShowDialog(vm);

			}
			catch(Exception ex1)
			{
				MessageBox.Show("An unexpected error has occurred. Application will be closed", "Error", MessageBoxButton.OK);
			}



			//var dialogResult = MessageBox.Show(ex.Message, "Unexpected Error..", MessageBoxButton.YesNo);
			//if(dialogResult == MessageBoxResult.Yes)
			//{
			//	Environment.Exit(100500);
			//}
        }

    }
}
