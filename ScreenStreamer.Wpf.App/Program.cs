using MediaToolkit;
using MediaToolkit.NativeAPIs;
using Microsoft.Win32;
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

        public static StartupParameters StartupParams { get; private set; }

        [STAThread]
        public static int Main(string[] args)
        {
            int exitCode = 0;

			InitLogger();

            logger.Info("============================ START ============================");

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

                StartupParams = StartupParameters.Create(args);

				logger.Info(StartupParams.GetSysInfo());

                try
				{
					MediaToolkitManager.Startup();

					Shcore.SetProcessPerMonitorDpiAwareness();

					var application = new App();
					application.DispatcherUnhandledException += Application_DispatcherUnhandledException;
					application.InitializeComponent();

					logger.Info("============================ RUN ============================");
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

                logger.Info("============================ THE END ============================");
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

				var vm = new ViewModels.Dialogs.MessageBoxViewModel(exceptionMessage, "Error", MessageBoxButton.OK);
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

            var config = LogManager.Configuration;
            if (config != null)
            {
                var vars = config.Variables;
                if (vars != null)
                {
                    if (!vars.ContainsKey("AppConfigPath"))
                    {
                        vars.Add("AppConfigPath", ConfigManager.ConfigPath);
                    }

                }
            }

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



        public class StartupParameters
        {
            public string UserName { get; private set; } = "";

            public bool IsSystem { get; private set; } = false;
            public bool IsElevated { get; private set; } = false;
            public bool NoRestart { get; private set; } = false;
            public bool RunAsSystem { get; private set; } = false;

            public bool IsRemoteSession { get; private set; } = false;
            public bool IsRemotelyControlled { get; private set; } = false;
            public bool IsCompositionEnabled { get; private set; } = false;

            public static StartupParameters Create(string[] args)
            {
                logger.Debug("CommandLine: " + string.Join(" ", args));

                StartupParameters startupParams = new StartupParameters();

                foreach (var arg in args)
                {
                    var _arg = arg?.ToLower();

                    if (_arg == "-norestart")
                    {
                        startupParams.NoRestart = true;
                    }
                    else if (_arg == "-system")
                    {
                        startupParams.RunAsSystem = true;
                    }
                    else
                    {
                        //...
                    }
                }

                using (var wi = System.Security.Principal.WindowsIdentity.GetCurrent())
                {

                    startupParams.UserName = wi.Name;
                    startupParams.IsElevated = (wi.Owner != wi.User);
                    startupParams.IsSystem = wi.IsSystem;

                }

                startupParams.IsRemoteSession = SystemParameters.IsRemoteSession;
                startupParams.IsRemotelyControlled = SystemParameters.IsRemotelyControlled;
                startupParams.IsCompositionEnabled = DwmApi.IsCompositionEnabled();



                return startupParams;
            }

			public string GetSysInfo()
			{
                var sysInfo = Environment.OSVersion + " " + (Environment.Is64BitOperatingSystem ? "x64" : "x86");

               // System.Runtime.InteropServices.RuntimeInformation.OSDescription


                if (StartupParams.IsSystem)
				{// run as system...
					sysInfo += "IsSystem\r\n";
				}
				else if (StartupParams.IsElevated)
				{// запущен с повышеными правами
					sysInfo += "IsElevated\r\n";
				}
				else if (StartupParams.IsRemotelyControlled || StartupParams.IsRemoteSession)
				{// rdp 
					sysInfo += "IsRDP: " + StartupParams.IsRemotelyControlled + ";" + StartupParams.IsRemoteSession + "\r\n";
				}
				else if (!StartupParams.IsCompositionEnabled)
				{// выключена композитная отрисовка
					sysInfo += "DWM Disabled\r\n";
				}


				//...
				return sysInfo;
			}

            public override string ToString()
            {
                return string.Join(";", UserName, IsElevated, IsSystem, NoRestart, IsRemoteSession, IsRemotelyControlled, IsCompositionEnabled);
            }



        }
    }
 
}
