using MediaToolkit;
using MediaToolkit.NativeAPIs;
using NLog;
using ScreenStreamer.Wpf.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ScreenStreamer.Wpf
{
    public class Program
    {

        private static Logger logger = null;


        [STAThread]
        public static int Main(string[] args)
        {
            int exitCode = 0;

			InitLogger();

            logger.Info("========== START ============");

            bool createdNew = false;
            Mutex mutex = null;
            try
            {
                mutex = new Mutex(true, AppConsts.ApplicationId, out createdNew);
                if (!createdNew)
                {
                    logger.Info("Another instance is already running...");
					if (!Models.AppModel.AllowMutipleInstance)
					{
						var res = Services.WndProcService.ShowAnotherInstance();
						return 0;

						//return -1;
						//...
					}

				}

                if (args != null && args.Length > 0)
                {//...

                }

				try
				{
					MediaToolkitManager.Startup();

					Shcore.SetProcessPerMonitorDpiAwareness();

					var application = new App();
					application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
					application.InitializeComponent();

					logger.Info("============ RUN ===============");
					application.Run();
				}
				finally
				{
					MediaToolkitManager.Shutdown();
				}
            }
            catch(Exception ex)
            {
				ProcessError(ex);
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


                logger.Info("========== THE END ============");
            }

           
            return exitCode;
        }

        private static void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			logger.Debug("Application_DispatcherUnhandledException(...)");

			e.Handled = true;

			ProcessError(e.Exception);

		}

		private static void ProcessError(Exception ex)
		{
			//TODO: process error...
			//...


			var message = "An unexpected error has occurred. Application will be closed";

			try
			{
				var exceptionMessage = ex?.Message??message;

				logger.Fatal(exceptionMessage);

				var dialogService = new Services.DialogService();

				ViewModels.Dialogs.MessageBoxViewModel vm = new ViewModels.Dialogs.MessageBoxViewModel(exceptionMessage, "Error", MessageBoxButton.OK);
				var result = dialogService.ShowDialog(vm);

			}
			catch (Exception ex1)
			{
				logger.Error(ex1);
				MessageBox.Show(message, "Error", MessageBoxButton.OK);
			}
		}


		private static void InitLogger()
		{

			logger = LogManager.GetCurrentClassLogger();

			var logFactory = logger.Factory;
			if (logFactory == null)
			{
				return;
			}

			var logConfig = logFactory.Configuration;
			if (logConfig == null)
			{
				return;
			}

			var logRules = logConfig.LoggingRules;
			if (logRules == null)
			{
				return;
			}

			bool needConsole = false;
			foreach (var rule in logRules)
			{
				var targets = rule.Targets;
				if (targets == null)
				{
					continue;
				}

				foreach (var target in targets)
				{
					var targetName = target.Name;
					if (targetName == "console")
					{
						needConsole = true;
						break;
					}
				}

				if (needConsole)
				{
					break;
				}
			}

			if (needConsole)
			{
				var hWnd = Kernel32.GetConsoleWindow();
				if (hWnd == IntPtr.Zero)
				{
					Kernel32.AllocConsole();
				}

			}
		}
	}
}
